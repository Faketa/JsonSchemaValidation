using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.ValidationRules;

/// <summary>
/// Validation rule to ensure a field does not exceed the maximum length.
/// </summary>
public class LengthValidationRule : IValidationRule
{
    /// <inheritdoc />
    public ValidationResult Validate(string fieldName, string fieldValue, SchemaField schemaField)
    {
        if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Length > schemaField.Length)
        {
            return ValidationResult.Failure(fieldName,
                schemaField.LengthErrorMessage ?? $"Field exceeds maximum length of {schemaField.Length}.");
        }

        return ValidationResult.Success(fieldName);
    }
}