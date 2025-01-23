using JsonSchemaValidation;
using JsonSchemaValidation.ValidationRules;
using Microsoft.Extensions.Logging;

var configuration = new ValidationConfiguration
{
 ChunkSize = 100,
 ValidationRules = new IValidationRule[]
 {
  new LengthValidationRule(),
  new MandatoryValidationRule()
 }
};

using var loggerFactory = LoggerFactory.Create(builder =>
{
 builder.AddConsole();
});

var logger = loggerFactory.CreateLogger("JsonSchemaValidationLogger");

var validator = new JsonValidator(configuration, logger);

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

await validator.ValidateAndProcessAsync("../../../schema.json", "../../../input-object.json", "../../../output.json", cancellationToken);
