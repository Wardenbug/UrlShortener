using FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Data;
using UrlShortener.Api.Data.Migrations;

namespace UrlShortener.IntegrationTests.Abstraction;
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("urlshortener")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer =
        new RedisBuilder()
            .WithImage("redis:latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ISqlConnectionFactory>();
            services.RemoveAll<IMigrationRunner>();
            services.RemoveAll<IMigrationRunnerBuilder>();
            services.RemoveAll<IMigrationProcessor>();
            services.RemoveAll<IMigrationGenerator>();
            services.RemoveAll<IConnectionMultiplexer>();

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                  new SqlConnectionFactory(_database.GetConnectionString()));

            services.Configure<RedisCacheOptions>(redisCacheOptions =>
               redisCacheOptions.Configuration = _redisContainer.GetConnectionString());

            services.AddSingleton<IConnectionMultiplexer>(
                provider => ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

            services.RemoveAll<IMigrationRunner>();

            services.AddFluentMigratorCore()
               .ConfigureRunner(rb => rb
                   .AddPostgres()
                   .WithGlobalConnectionString(_database.GetConnectionString())
                   .ScanIn(typeof(CreateUrlTable).Assembly).For.Migrations())
               .AddLogging(lb => lb.AddFluentMigratorConsole());
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }
}
