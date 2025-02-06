using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaValidation.Services;

public class PostgreSQLDataProvider
{
    private readonly ILogger<PostgreSQLDataProvider> _logger;
    private readonly ValidationConfiguration _configuration;

    public PostgreSQLDataProvider(IOptions<ValidationConfiguration> configuration, ILogger<PostgreSQLDataProvider> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    /// <summary>
    /// Reads JSON data from the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to read from.</param>
    /// <param name="columnName">The name of the column to read from.</param>
    /// <returns>JSON as string.</returns>
    public async Task<Stream> ReadJsonDataAsync(string tableName, string columnName)
    {
        _logger.LogInformation("Connection to PostgreSQL DB started.");

        var query = $"SELECT {columnName} FROM {tableName}";

        await using var connection = new NpgsqlConnection(_configuration.ConnectionString);
        await connection.OpenAsync();

        _logger.LogInformation("Connection established.");

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

        _logger.LogInformation($"Retreiving data from {tableName}.");

        var stringResult = await reader.ReadAsync() ? reader.GetString(0) : string.Empty;

        // convert string to stream
        byte[] byteArray = Encoding.UTF8.GetBytes(stringResult);
        return new MemoryStream(byteArray);
    }
}

