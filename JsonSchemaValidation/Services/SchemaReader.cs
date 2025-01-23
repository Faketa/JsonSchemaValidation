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
        public async Task<Dictionary<string, SchemaField>?> ReadSchemaAsync(Stream schemaStream,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var schemaReader = new StreamReader(schemaStream);
                var schemaContent = await schemaReader.ReadToEndAsync(cancellationToken);
                return JsonConvert.DeserializeObject<Dictionary<string, SchemaField>>(schemaContent)
                       ?? throw new InvalidOperationException("Schema is null or malformed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading schema: {ex.Message}");
                return null;
            }
        }
    }
}