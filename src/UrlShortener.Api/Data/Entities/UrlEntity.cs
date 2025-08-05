
namespace UrlShortener.Api.Data.Entities;

internal sealed record UrlEntity(int Id, string OriginalUrl, string ShortCode);
