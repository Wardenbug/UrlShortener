using System.Data;
using Npgsql;
using UrlShortener.Api.Abstractions;

namespace UrlShortener.Api.Data;

internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        return connection;
    }
}
