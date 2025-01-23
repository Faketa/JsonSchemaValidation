using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.Services;

public class ConstraintValidator
{
    private readonly List<IValidationRule> _rules;

    public ConstraintValidator(IEnumerable<IValidationRule> rules)
    {
        _rules = rules.ToList();
    }

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