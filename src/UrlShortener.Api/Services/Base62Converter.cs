using UrlShortener.Api.Abstractions;

namespace UrlShortener.Api.Services;

internal sealed class Base62Converter : IUrlConverter
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Encode(uint id)
    {
        if (id == 0)
        {
            return Alphabet[0].ToString();
        }

        var result = new Span<char>(new char[11]);
        int index = result.Length;

        while (id > 0)
        {
            result[--index] = Alphabet[(int)(id % 62)];
            id /= 62;
        }

        return new string(result.Slice(index));
    }

    public uint Decode(string shortUrl)
    {
        if (shortUrl.Length > 11)
        {
            return 0;
        }

        int id = 0;
        foreach (char ch in shortUrl)
        {
            id *= 62;
            if (ch is >= 'a' and <= 'z')
            {
                id += ch - 'a';
            }
            else if (ch is >= 'A' and <= 'Z')
            {
                id += ch - 'A' + 26;
            }
            else if (ch is >= '0' and <= '9')
            {
                id += ch - '0' + 52;
            }
        }
        return (uint)id;
    }

}
