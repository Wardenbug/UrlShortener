namespace UrlShortener.Api.Abstractions;

internal interface IUrlConverter
{
    int Decode(string shortUrl);
    string Encode(int id);
}
