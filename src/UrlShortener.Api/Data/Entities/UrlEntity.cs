
namespace UrlShortener.Api.Data.Entities;

internal sealed class UrlEntity
{
    public int Id { get; }
    public required string OriginalUrl { get; init; }
    public required string ShortCode { get; init; }

    public UrlEntity()
    {

    }

    public static UrlEntity Create(string originalUrl, string shortCode)
    {
        return new UrlEntity() { OriginalUrl = originalUrl, ShortCode = shortCode };
    }
};
