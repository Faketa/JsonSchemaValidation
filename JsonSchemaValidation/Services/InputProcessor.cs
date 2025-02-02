using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonSchemaValidation.Services;

/// <summary>
/// Service for processing input JSON data in chunks for validation.
/// This class reads JSON streams efficiently, handling both array and single-object structures,
/// while supporting cancellation tokens to enable safe termination of long-running operations.
/// </summary>
public class InputProcessor
{
    private readonly ILogger _logger;

    public InputProcessor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Splits input JSON data into chunks for processing.
    /// Supports processing large JSON files by yielding records in chunks
    /// to reduce memory footprint and improve performance.
    /// </summary>
    /// <param name="inputDataStream">The stream containing input JSON data.</param>
    /// <param name="chunkSize">The maximum number of records per chunk.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An asynchronous enumerable that yields lists of deserialized records,
    /// ensuring minimal memory usage while processing large JSON inputs.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the inputDataStream is null.</exception>
    public async IAsyncEnumerable<List<Dictionary<string, string>>> ChunkInputAsync(
        Stream inputDataStream, int chunkSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (inputDataStream == null)
            throw new ArgumentNullException(nameof(inputDataStream), "Input data stream cannot be null.");

        cancellationToken.ThrowIfCancellationRequested();

        using var inputReader = new StreamReader(inputDataStream);
        using var jsonReader = new JsonTextReader(inputReader);
        var serializer = new JsonSerializer();
        var currentChunk = new List<Dictionary<string, string>>();

        if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
        {
            while (await jsonReader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    try
                    {
                        var record = serializer.Deserialize<Dictionary<string, string>>(jsonReader);
                        if (record != null)
                        {
                            currentChunk.Add(record);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning($"Malformed record skipped: {ex.Message}");
                    }

                    if (currentChunk.Count >= chunkSize)
                    {
                        yield return new List<Dictionary<string, string>>(currentChunk);
                        currentChunk.Clear();
                    }
                }
                else if (jsonReader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
            }

            if (currentChunk.Any())
            {
                yield return new List<Dictionary<string, string>>(currentChunk);
            }
        }
        else if (jsonReader.TokenType == JsonToken.StartObject)
        {
            // Handle single object JSON
            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<string, string> record = new Dictionary<string, string>();

            try
            {
                record = serializer.Deserialize<Dictionary<string, string>>(jsonReader);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"Malformed record skipped: {ex.Message}");
            }

            if (record.Count > 0)
            {
                yield return new List<Dictionary<string, string>> { record };
            }
        }
        else
        {
            _logger.LogError("Input JSON is neither an array nor a valid object.");
        }
    }
}
