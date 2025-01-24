using JsonSchemaValidation.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonSchemaValidation.Services;

/// <summary>
/// Service for writing validation results to an output file.
/// </summary>
public class ResultWriter
{
    private readonly ILogger _logger;

    public ResultWriter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Writes validation results to the specified output file.
    /// </summary>
    /// <param name="outputPath">The path to the output file.</param>
    /// <param name="results">The validation results to write.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    /// <exception cref="Exception">Logs errors related to file writing issues.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the outputPath is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task WriteResultsAsync(string outputPath, IEnumerable<ValidationResult> results, CancellationToken cancellationToken)
    {
        if (outputPath == null)
        {
            throw new ArgumentNullException(nameof(outputPath), "Output path cannot be null.");
        }

        try
        {
            await using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await using var writer = new StreamWriter(outputStream);

            var serializedResults = JsonConvert.SerializeObject(results, Formatting.Indented);

            foreach (char chunk in serializedResults)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await writer.WriteAsync(chunk.ToString());
            }

            _logger.LogInformation($"Results successfully written to {outputPath}.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Write operation was canceled.");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath); // Remove incomplete file if operation was canceled.
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing results to file: {ex.Message}");
        }
    }
}