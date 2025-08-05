using FluentMigrator.Runner;
using UrlShortener.Api.Abstractions;
using UrlShortener.Api.Endpoints;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddSingleton<IUrlConverter, UrlConverter>();

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

app.MapEndpoints();

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);
