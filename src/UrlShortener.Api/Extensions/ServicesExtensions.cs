using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using FluentMigrator.Runner;
using StackExchange.Redis;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Data;
using UrlShortener.Api.Data.Migrations;

namespace UrlShortener.Api.Extensions;

internal static class ServicesExtensions
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("Database") ??
           throw new ArgumentNullException(nameof(configuration));
        string redisConnectionString = configuration.GetConnectionString("Cache") ??
           throw new ArgumentNullException(nameof(configuration));

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(databaseConnectionString)
                .ScanIn(typeof(CreateUrlTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
                   new SqlConnectionFactory(databaseConnectionString));

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnectionString = configuration.GetConnectionString("Cache") ??
        throw new ArgumentNullException(nameof(configuration));

        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);

        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisOptions));

        services.AddRedisRateLimiting();

        return services;
    }
}
