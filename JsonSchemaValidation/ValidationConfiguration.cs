namespace JsonSchemaValidation;

// Configuration class
/// <summary>
/// Provides configuration for chunk size and validation rules.
/// </summary>
public class ValidationConfiguration
{
    /// <summary>
    /// Gets or sets the chunk size for processing input data.
    /// </summary>
    public int ChunkSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the collection of validation rules to apply.
    /// </summary>
    public IEnumerable<IValidationRule> ValidationRules { get; set; } = Array.Empty<IValidationRule>();
}