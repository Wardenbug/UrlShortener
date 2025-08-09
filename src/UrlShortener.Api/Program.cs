using AspNetCoreRateLimit;
using FluentMigrator.Runner;
using Serilog;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Endpoints;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddMemoryCache();

builder.Services.AddRateLimiting(builder.Configuration);

builder.Services.AddSingleton<IUrlConverter, Base62Converter>();
builder.Services.AddSingleton<ICacheService, CacheService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue<bool>("RunMigrations"))
{
    using IServiceScope scope = app.Services.CreateScope();
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.UseIpRateLimiting();
app.UseSerilogRequestLogging();
app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();


#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program;
#pragma warning restore S1118 // Utility classes should not have public constructors
#pragma warning restore CA1515 // Consider making public types internal
