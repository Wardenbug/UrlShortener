using System.Data;

namespace UrlShortener.Api.Abstractions;
internal interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
