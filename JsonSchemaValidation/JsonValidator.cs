using System.Collections.Concurrent;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation.Models;
using JsonSchemaValidation.Services;

namespace JsonSchemaValidation;

/// <summary>
/// Main service for schema validation, input processing and result writer.
/// </summary>
public class JsonValidator
{
    private readonly ValidationConfiguration _configuration;
    private readonly ConstraintValidator _validationRules;
    private readonly SchemaReader _schemaReader;
    private readonly InputProcessor _inputProcessor;
    private readonly ResultWriter _resultWriter;
    private readonly ChunkValidator _chunkValidator;
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">Validation configuration</param>
    /// <param name="logger">Microsoft Logger</param>
    public JsonValidator(ValidationConfiguration configuration, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _configuration = configuration;
        _validationRules = new ConstraintValidator(configuration.ValidationRules);
        _schemaReader = new SchemaReader(logger);
        _inputProcessor = new InputProcessor(logger);
        _resultWriter = new ResultWriter(logger);
        _chunkValidator = new ChunkValidator(_validationRules, logger);
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

        var schema = await _schemaReader.ReadSchemaAsync(schemaStream, cancellationToken);

        var results = new List<ValidationResult>();

        await foreach (var chunk in _inputProcessor.ChunkInputAsync(inputStream, _configuration.ChunkSize, cancellationToken))
        {
            var validatedChunk = _chunkValidator.ValidateChunk(chunk, schema);
            results.AddRange(validatedChunk);
        }

        await _resultWriter.WriteResultsAsync(outputPath, results, cancellationToken);
    }
}