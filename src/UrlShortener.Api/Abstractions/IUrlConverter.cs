namespace UrlShortener.Api.Abstractions;

internal interface IUrlConverter
{
    uint Decode(string shortUrl);
    string Encode(uint id);
}
