using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.Services;

/// <summary>
/// A collection of validation rules for schema validation.
/// </summary>
public class ConstraintValidator
{
    private readonly List<IValidationRule> _rules;

    public ConstraintValidator(IEnumerable<IValidationRule> rules)
    {
        _rules = rules.ToList();
    }

    /// <summary>
    /// Executes all validation rules for the given field.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="fieldValue">The value of the field being validated.</param>
    /// <param name="schemaField">The schema definition for the field.</param>
    /// <returns>A validation result.</returns>
    public ValidationResult Validate(string fieldName, string fieldValue, SchemaField schemaField)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Validate(fieldName, fieldValue, schemaField);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.Success(fieldName);
    }
}