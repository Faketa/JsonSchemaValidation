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

string testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");

//Sample check for JSON one object
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    logger.LogInformation("Validation and processing of JSON one object started.");

    await validator.ValidateAndProcessAsync($"{testFilesPath}schema.json", $"{testFilesPath}input.json", $"{testFilesPath}output.json", cancellationToken);

    logger.LogInformation("Validation and processing completed.");
}
catch (Exception ex)
{
    logger.LogError($"An error occurred in the application: {ex.Message}");
}

//Sample check for JSON collection
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    logger.LogInformation("Validation and processing of JSON collection started.");

    await validator.ValidateAndProcessAsync($"{testFilesPath}schema.json", $"{testFilesPath}input-collection.json", $"{testFilesPath}output-collection.json", cancellationToken);

    logger.LogInformation("Validation and processing completed.");
}
catch (Exception ex)
{
    logger.LogError($"An error occurred in the application: {ex.Message}");
}


//Sample check for JSON large collection
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    logger.LogInformation("Validation and processing of JSON large collection started.");

    await validator.ValidateAndProcessAsync($"{testFilesPath}schema.json", $"{testFilesPath}input-large.json", $"{testFilesPath}output-large.json", cancellationToken);

    logger.LogInformation("Validation and processing completed.");
}
catch (Exception ex)
{
    logger.LogError($"An error occurred in the application: {ex.Message}");
}

//Sample check for JSON one object with custom error message in schema
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    logger.LogInformation("Validation and processing of JSON one object with custom error message in schema started.");

    await validator.ValidateAndProcessAsync($"{testFilesPath}schema-custom-error.json", $"{testFilesPath}input-collection.json", $"{testFilesPath}output-collection-custom-error.json", cancellationToken);

    logger.LogInformation("Validation and processing completed.");
}
catch (Exception ex)
{
    logger.LogError($"An error occurred in the application: {ex.Message}");
}



