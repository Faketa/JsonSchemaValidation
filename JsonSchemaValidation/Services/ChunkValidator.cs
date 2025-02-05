using System.Collections.Concurrent;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation.Models;
using JsonSchemaValidation.Services;

namespace JsonSchemaValidation.Services;

/// <summary>
/// Service for validating chunks of input records against a schema.
/// </summary>
public class ChunkValidator
{
    private readonly ConstraintValidator _validationRules;
    private readonly ILogger<ChunkValidator> _logger;

    public ChunkValidator(ConstraintValidator validationRules, ILogger<ChunkValidator> logger)
    {
        _validationRules = validationRules;
        _logger = logger;
    }

    /// <summary>
    /// Validates a chunk of records in parallel against the provided schema.
    /// </summary>
    /// <param name="chunk">List of input records</param>
    /// <param name="schema">Schema to validate against</param>
    /// <returns>List of validation results</returns>
    public IEnumerable<ValidationResult> ValidateChunk(List<Dictionary<string, string>> chunk, Dictionary<string, SchemaField> schema)
    {
        var chunkResults = new ConcurrentBag<ValidationResult>();

        Parallel.ForEach(chunk, record =>
        {
            foreach (var field in schema)
            {
                var (fieldName, schemaField) = field;
                record.TryGetValue(fieldName, out var fieldValue);

                var result = _validationRules.Validate(fieldName, fieldValue, schemaField);

                if (!result.IsValid)
                    _logger.LogWarning($"Validation failed for field '{result.Field}': {result.ErrorMessage}. Record: {JsonConvert.SerializeObject(record)}");
                else
                    _logger.LogInformation($"Validation success for field '{result.Field}'. Record: {JsonConvert.SerializeObject(record)}");

                chunkResults.Add(result);
            }
        });

        return chunkResults;
    }
}
