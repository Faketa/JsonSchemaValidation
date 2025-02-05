using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.Services
{
    /// <summary>
    /// Service for reading and deserializing schema files.
    /// </summary>
    public class SchemaReader
    {
        private readonly ILogger<SchemaReader> _logger;

        public SchemaReader(ILogger<SchemaReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reads a schema from the provided stream and deserializes it into a dictionary.
        /// </summary>
        /// <param name="schemaStream">The stream containing the schema JSON.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A dictionary representing the schema, or null if deserialization fails.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the schema is null or malformed.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the schemaStream is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        /// <exception cref="JsonException">Thrown when the Schema is malformed JSON</exception>
        public async Task<Dictionary<string, SchemaField>?> ReadSchemaAsync(Stream schemaStream, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(schemaStream);

            try
            {
                using var schemaReader = new StreamReader(schemaStream);
                var schemaContent = await schemaReader.ReadToEndAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(schemaContent))
                {
                    _logger.LogError("Schema content is empty.");
                    throw new InvalidOperationException("Schema is null, empty, or malformed.");
                }

                var schema = JsonConvert.DeserializeObject<Dictionary<string, SchemaField>>(schemaContent);

                if (schema == null || schema.Count == 0)
                {
                    _logger.LogError("Schema deserialization resulted in null or empty schema.");
                    throw new InvalidOperationException("Schema is null, empty, or malformed.");
                }

                return schema;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Schema reading operation was canceled.");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                throw new InvalidOperationException("Schema is malformed JSON.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                throw;
            }
        }
    }
}