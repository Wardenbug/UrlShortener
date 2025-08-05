using FluentMigrator.Runner;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Data;
using UrlShortener.Api.Data.Migrations;

namespace UrlShortener.Api.Extensions;

internal static class ServicesExtensions
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ??
           throw new ArgumentNullException(nameof(configuration));

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(configuration.GetConnectionString(connectionString))
                .ScanIn(typeof(CreateUrlTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
                   new SqlConnectionFactory(connectionString));
    }
}
