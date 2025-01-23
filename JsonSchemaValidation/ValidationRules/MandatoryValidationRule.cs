using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.ValidationRules;

/// <summary>
/// Validation rule to ensure a field is not null or empty when mandatory.
/// </summary>
public class MandatoryValidationRule : IValidationRule
{
    /// <inheritdoc />
    public ValidationResult Validate(string fieldName, string fieldValue, SchemaField schemaField)
    {
        if (schemaField.Mandatory && string.IsNullOrWhiteSpace(fieldValue))
        {
            return ValidationResult.Failure(fieldName,
                schemaField.MandatoryErrorMessage ?? "Field is mandatory but missing or empty.");
        }

        return ValidationResult.Success(fieldName);
    }
}