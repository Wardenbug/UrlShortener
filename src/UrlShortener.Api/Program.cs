using FluentMigrator.Runner;
using UrlShortener.Api.Migrations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Database"))
        .ScanIn(typeof(CreateUrlTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);
