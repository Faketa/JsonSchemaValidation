using JsonSchemaValidation;
using JsonSchemaValidation.ValidationRules;
using JsonSchemaValidation.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddJsonSchemaValidation(configuration);

//Validation rules
services.AddSingleton<IValidationRule, MandatoryValidationRule>();
services.AddSingleton<IValidationRule, LengthValidationRule>();

var serviceProvider = services.BuildServiceProvider();
var validator = serviceProvider.GetRequiredService<JsonValidator>();

string testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");

//Validate JSON from PostgreSQL DB
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    await validator.ValidateAsync($"{testFilesPath}schema.json", "Custom", "data", $"{testFilesPath}output.json", cancellationToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred in the application: {ex.Message}");
}

//Validate JSON one object
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    await validator.ValidateAsync($"{testFilesPath}schema.json", $"{testFilesPath}input.json", $"{testFilesPath}output.json", cancellationToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred in the application: {ex.Message}");
}

//Validate JSON collection
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    await validator.ValidateAsync($"{testFilesPath}schema.json", $"{testFilesPath}input-collection.json", $"{testFilesPath}output-collection.json", cancellationToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred in the application: {ex.Message}");
}

//Validate JSON one object with custom error message in schema
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    await validator.ValidateAsync($"{testFilesPath}schema-custom-error.json", $"{testFilesPath}input-collection.json", $"{testFilesPath}output-collection-custom-error.json", cancellationToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred in the application: {ex.Message}");
}

//Validate JSON large collection
try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    await validator.ValidateAsync($"{testFilesPath}schema.json", $"{testFilesPath}input-large.json", $"{testFilesPath}output-large.json", cancellationToken);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred in the application: {ex.Message}");
}