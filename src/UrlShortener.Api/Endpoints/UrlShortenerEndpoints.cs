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
        IConfiguration configuration,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
        {
            return TypedResults.BadRequest("Invalid URL format.");
        }

        logger.LogInformation("Generating short link for {OrinalLink}",
           request.OriginalUrl);

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        uint newId = await connection.ExecuteScalarAsync<uint>("SELECT nextval('urls_id_seq')", cancellationToken);

        string shortCode = urlConverter.Encode(newId);

        const string insertSql = @"
                INSERT INTO public.urls (id, original_url, short_code)
                VALUES (@Id, @OriginalUrl, @ShortCode)
            ";

        await connection.ExecuteAsync(insertSql,
            new
            {
                Id = (int)newId,
                request.OriginalUrl,
                ShortCode = shortCode
            });

        string shortLink = $"{configuration["Domain"]}{shortCode}";

        logger.LogInformation("Generated short link {ShortLink} for {OriginalLink}",
            shortLink,
            request.OriginalUrl);

        return TypedResults.Ok(
            new ShortenResponse(shortLink));
    }

    public static async Task<IResult> RedirectToOriginalAsync(
        string code,
        IUrlConverter urlConverter,
        ICacheService cacheService,
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<Program> logger,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        uint id;
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

        string shortLink = $"{configuration["Domain"]}{code}";


        if (cachedUrl is not null)
        {
            logger.LogInformation("Redirected to {Url} from {ShortUrl}", cachedUrl, shortLink);
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
            logger.LogWarning("Short URL {ShortUrl} not found", shortLink);

            return Results.NotFound("URL not found.");
        }

        await cacheService.SetAsync(code, url.OriginalUrl, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(incrementRedirectSql, new { Id = id });

        logger.LogInformation("Redirected to {Url} from {ShortUrl}", url.OriginalUrl, shortLink);

        return Results.Redirect(url.OriginalUrl);
    }
}
