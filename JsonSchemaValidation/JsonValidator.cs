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
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">Validation configuration</param>
    /// <param name="logger">Microsoft Logger</param>
    public JsonValidator(ValidationConfiguration configuration, ILogger logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Validation configuration cannot be null.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

        _configuration = configuration;
        _validationRules = new ConstraintValidator(configuration.ValidationRules);
        _schemaReader = new SchemaReader(logger);
        _inputProcessor = new InputProcessor(logger);
        _resultWriter = new ResultWriter(logger);
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
    public async Task ValidateAndProcessAsync(string schemaPath, string inputDataPath, string outputPath, CancellationToken cancellationToken)
    {
        schemaPath = schemaPath ?? throw new ArgumentNullException(nameof(schemaPath));
        inputDataPath = inputDataPath ?? throw new ArgumentNullException(nameof(inputDataPath));

        await using var schemaStream = File.OpenRead(schemaPath);
        await using var inputStream = File.OpenRead(inputDataPath);

        await ValidateAndProcessAsync(schemaStream, inputStream, outputPath, cancellationToken);
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
    public async Task ValidateAndProcessAsync(Stream schemaStream, Stream inputStream, string outputPath, CancellationToken cancellationToken)
    {
        schemaStream = schemaStream ?? throw new ArgumentNullException(nameof(schemaStream));
        inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));

        var schema = _schemaReader.ReadSchemaAsync(schemaStream, cancellationToken);

        await schema;
        if (schema.Result == null)
        {
            _logger.LogError("Failed to load schema.");
            return;
        }

        var results = new ConcurrentBag<ValidationResult>();
        var anyChunk = false;

        await foreach (var chunk in _inputProcessor.ChunkInputAsync(inputStream, _configuration.ChunkSize, cancellationToken))
        {
            anyChunk = true;
            ValidateChunk(chunk, schema.Result, results);
        }

        if (!anyChunk)
        {
            _logger.LogError("Input JSON is empty.");
            return;
        }

        await _resultWriter.WriteResultsAsync(outputPath, results, cancellationToken);
    }

    /// <summary>
    /// Parallel validation a single chunk of input records.
    /// </summary>
    /// <param name="chunk">The chunk of input records to validate.</param>
    /// <param name="schema">The schema to validate against.</param>
    /// <param name="results">The concurrent bag to collect validation results.</param>
    private void ValidateChunk(List<Dictionary<string, string>> chunk, Dictionary<string, SchemaField> schema,  ConcurrentBag<ValidationResult> results)
    {
        Parallel.ForEach(chunk, record =>
        {
            foreach (var field in schema)
            {
                var (fieldName, schemaField) = field;
                record.TryGetValue(fieldName, out var fieldValue);

                var result = _validationRules.Validate(fieldName, fieldValue, schemaField);
                if (!result.IsValid)
                {
                    _logger.LogWarning(
                        $"Validation failed for field '{result.Field}': {result.ErrorMessage}. Record: {JsonConvert.SerializeObject(record)}");
                }

                _logger.LogInformation(
                    $"Validation success for field '{result.Field}'. Record: {JsonConvert.SerializeObject(record)}");
                results.Add(result);
            }
        });
    }
}