using JsonSchemaValidation.Models;

namespace JsonSchemaValidation;

/// <summary>
/// Interface for defining validation rules for schema fields.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Validates a schema field based on the rule's logic.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="fieldValue">The value of the field being validated.</param>
    /// <param name="schemaField">The schema definition of the field.</param>
    /// <returns>A ValidationResult indicating success or failure.</returns>
    ValidationResult Validate(string fieldName, string fieldValue, SchemaField schemaField);
}