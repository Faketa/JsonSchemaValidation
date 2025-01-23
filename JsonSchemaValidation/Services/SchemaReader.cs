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
        private readonly ILogger _logger;

        public SchemaReader(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reads a schema from the provided stream and deserializes it into a dictionary.
        /// </summary>
        /// <param name="schemaStream">The stream containing the schema JSON.</param>
        /// <returns>A dictionary representing the schema, or null if deserialization fails.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the schema is null or malformed.</exception>
        public async Task<Dictionary<string, SchemaField>?> ReadSchemaAsync(Stream schemaStream, CancellationToken cancellationToken)
        {
            if (schemaStream == null)
            {
                throw new ArgumentNullException(nameof(schemaStream), "Schema stream cannot be null.");
            }

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