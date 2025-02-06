using System.Collections.Concurrent;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation.Models;
using JsonSchemaValidation.Services;
using Microsoft.Extensions.Options;
using System.Text;

namespace JsonSchemaValidation;

/// <summary>
/// Main service for schema validation, input processing and result writer.
/// </summary>
public class JsonValidator
{
    private readonly ValidationConfiguration _configuration;
    private readonly ConstraintValidator _constraintValidator;
    private readonly SchemaReader _schemaReader;
    private readonly InputProcessor _inputProcessor;
    private readonly ResultWriter _resultWriter;
    private readonly ChunkValidator _chunkValidator;
    private readonly PostgreSQLDataProvider _postgreSQLDataProvider;
    private readonly ILogger<JsonValidator> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">Validation configuration</param>
    /// <param name="logger">Microsoft Logger</param>
    public JsonValidator(
            IOptions<ValidationConfiguration> configuration,
            ConstraintValidator constraintValidator,
            SchemaReader schemaReader,
            InputProcessor inputProcessor,
            ResultWriter resultWriter,
            ChunkValidator chunkValidator,
            PostgreSQLDataProvider postgreSQLDataProvider,
            ILogger<JsonValidator> logger)
    {
        _configuration = configuration.Value;
        _constraintValidator = constraintValidator;
        _schemaReader = schemaReader;
        _inputProcessor = inputProcessor;
        _resultWriter = resultWriter;
        _chunkValidator = chunkValidator;
        _postgreSQLDataProvider = postgreSQLDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// Validates input data against a schema and writes results to an output file.
    /// </summary>
    /// <param name="schemaPath">The path containing the schema JSON.</param>
    /// <param name="inputDataPath">The path containing the input JSON data.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <param name="cancellationToken">The Cancellation Token</param>
    /// <returns>A task representing the asynchronous validation and writing process.</returns>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">Part of the filename or directory cannot be found.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be found.</exception>
    public async Task ValidateAsync(string schemaPath, string inputDataPath, string outputPath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schemaPath);
        ArgumentNullException.ThrowIfNull(inputDataPath);

        await using var schemaStream = File.OpenRead(schemaPath);
        await using var inputStream = File.OpenRead(inputDataPath);

        await ValidateAsync(schemaStream, inputStream, outputPath, cancellationToken);
    }

    /// <summary>
    /// Validates input data from PostgreSQL DB against a schema and writes results to an output file.
    /// </summary>
    /// <param name="schemaPath">The path containing the schema JSON.</param>
    /// <param name="tableName">The table name of PostgreSQL DB</param>
    /// <param name="columnName">The column name where the JSONB data is located</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <param name="cancellationToken">The Cancellation Token</param>
    /// <returns>A task representing the asynchronous validation and writing process.</returns>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">Part of the filename or directory cannot be found.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be found.</exception>
    public async Task ValidateAsync(string schemaPath, string tableName, string columnName, string outputPath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schemaPath);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        await using var schemaStream = File.OpenRead(schemaPath);
        var inputStream = await _postgreSQLDataProvider.ReadJsonDataAsync(tableName, columnName);

        await ValidateAsync(schemaStream, inputStream, outputPath, cancellationToken);
    }

    /// <summary>
    /// Validates input data against a schema and writes results to an output file.
    /// </summary>
    /// <param name="schemaPath">The stream containing the schema JSON.</param>
    /// <param name="inputDataPath">The stream containing the input JSON data.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <param name="cancellationToken">The Cancellation Token</param>
    /// <returns>A task representing the asynchronous validation and writing process.</returns>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">Part of the filename or directory cannot be found.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be found.</exception>
    public async Task ValidateAsync(Stream schemaStream, Stream inputStream, string outputPath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(schemaStream);
        ArgumentNullException.ThrowIfNull(inputStream);

        _logger.LogInformation("Validation and processing of JSON started.");

        var schema = await _schemaReader.ReadSchemaAsync(schemaStream, cancellationToken);

        var results = new List<ValidationResult>();

        await foreach (var chunk in _inputProcessor.ChunkInputAsync(inputStream, cancellationToken))
        {
            var validatedChunk = _chunkValidator.ValidateChunk(chunk, schema);
            results.AddRange(validatedChunk);
        }

        await _resultWriter.WriteResultsAsync(outputPath, results, cancellationToken);

        _logger.LogInformation("Validation and processing completed.");
    }
}