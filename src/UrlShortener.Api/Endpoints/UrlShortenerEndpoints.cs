using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Data.Entities;
using UrlShortener.Api.Endpoints.DTOs;

namespace UrlShortener.Api.Endpoints;

internal static class UrlShortenerEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/shorten", CreateLinkAsync);
        builder.MapGet("/{code}", RedirectToOriginalAsync);

        return builder;
    }

    public static async Task<Results<Ok<ShortenResponse>, BadRequest<string>>> CreateLinkAsync(
        CreateShortLinkRequest request,
        IUrlConverter urlConverter,
        ISqlConnectionFactory sqlConnectionFactory,
        IConfiguration configuration)
    {
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
        {
            return TypedResults.BadRequest("Invalid URL format.");
        }

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        int newId = await connection.ExecuteScalarAsync<int>("SELECT nextval('urls_id_seq')");

        string shortCode = urlConverter.Encode(newId);

        const string insertSql = @"
                INSERT INTO public.urls (id, original_url, short_code)
                VALUES (@Id, @OriginalUrl, @ShortCode)
            ";

        await connection.ExecuteAsync(insertSql,
            new
            {
                Id = newId,
                request.OriginalUrl,
                ShortCode = shortCode
            });

        return TypedResults.Ok(
            new ShortenResponse($"{configuration["Domain"]}{shortCode}")
            );
    }

    public static async Task<IResult> RedirectToOriginalAsync(
        string code,
        IUrlConverter urlConverter,
        ICacheService cacheService,
        ISqlConnectionFactory sqlConnectionFactory,
        CancellationToken cancellationToken)
    {
        int id;
        try
        {
            id = urlConverter.Decode(code);
        }
        catch (FormatException)
        {
            return Results.BadRequest("Invalid code format.");
        }
        catch (OverflowException)
        {
            return Results.BadRequest("Code value is out of range.");
        }

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        string? cachedUrl = await cacheService.GetAsync<string>(code, cancellationToken);

        const string sql = """
                SELECT
                    id AS Id,
                    original_url AS OriginalUrl
                FROM public.urls
                WHERE id = @Id  
                """;

        const string incrementRedirectSql = """
            UPDATE public.urls SET redirect_count = redirect_count + 1 WHERE id = @Id;
            """;

        if (cachedUrl is not null)
        {

            await connection.ExecuteAsync(incrementRedirectSql, new { Id = id });

            return Results.Redirect(cachedUrl);
        }

        UrlEntity? url = await connection.QueryFirstOrDefaultAsync<UrlEntity>(
            sql,
            new
            {
                Id = id
            });

        if (url is null)
        {
            return Results.NotFound("URL not found.");
        }

        await cacheService.SetAsync(code, url.OriginalUrl, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(incrementRedirectSql, new { Id = id });

        return Results.Redirect(url.OriginalUrl);
    }
}
