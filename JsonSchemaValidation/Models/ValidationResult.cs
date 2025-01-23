namespace JsonSchemaValidation.Models;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public record ValidationResult(string Field, bool IsValid, string? ErrorMessage)
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <returns>A ValidationResult indicating success.</returns>
    public static ValidationResult Success(string field) => new(field, true, null);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A ValidationResult indicating failure.</returns>
    public static ValidationResult Failure(string field, string errorMessage) => new(field, false, errorMessage);
}