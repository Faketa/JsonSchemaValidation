using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonSchemaValidation.Services;

/// <summary>
/// Service for processing input data in chunks.
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
    /// </summary>
    /// <param name="inputDataStream">The stream containing input JSON data.</param>
    /// <param name="chunkSize">The maximum number of records per chunk.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An enumerable of chunks containing deserialized records.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task<IEnumerable<List<Dictionary<string, string>>>> ChunkInputAsync(Stream inputDataStream, int chunkSize, CancellationToken cancellationToken)
    {
        if (inputDataStream == null)
            throw new ArgumentNullException(nameof(inputDataStream), "Input data stream cannot be null.");
        
        var chunks = new List<List<Dictionary<string, string>>>();
        try
        {
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
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Malformed record skipped: {ex.Message}");
                        }

                        if (currentChunk.Count >= chunkSize)
                        {
                            chunks.Add(new List<Dictionary<string, string>>(currentChunk));
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
                    chunks.Add(currentChunk);
                }
            }
            else if (jsonReader.TokenType == JsonToken.StartObject)
            {
                // Handle single object JSON
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var record = serializer.Deserialize<Dictionary<string, string>>(jsonReader);
                    if (record != null)
                    {
                        chunks.Add(new List<Dictionary<string, string>> { record });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Malformed record skipped: {ex.Message}");
                }
            }
            else
            {
                _logger.LogError("Input JSON is neither an array nor a valid object.");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing input: {ex.Message}");
        }

        return chunks;
    }
}