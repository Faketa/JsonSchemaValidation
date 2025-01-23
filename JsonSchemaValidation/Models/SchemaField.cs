namespace JsonSchemaValidation.Models;

/// <summary>
/// Represents a schema field with its validation properties.
/// </summary>
public record SchemaField(
    int Length,
    bool Mandatory,
    string? LengthErrorMessage = null,
    string? MandatoryErrorMessage = null);