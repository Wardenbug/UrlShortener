// Migrations/202508041214_CreateUrlTable.cs
using FluentMigrator;

namespace UrlShortener.Api.Migrations;

[Migration(202508041214)]
internal sealed class CreateUrlTable : Migration
{
    public override void Up()
    {
        Create.Table("urls")
            .WithColumn("id")
                .AsInt32()
                .PrimaryKey()
                .Identity()
            .WithColumn("original_url")
                .AsString()
                .NotNullable()
            .WithColumn("short_code")
                .AsString()
                .NotNullable();
    }

    public override void Down()
    {
        Delete.Table("urls");
    }
}
