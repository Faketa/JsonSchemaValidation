# JsonSchemaValidation Library

## Overview
The JsonSchemaValidation library is a tool for validating JSON data with customizable schemas. It's built for performance and scalability, using modern C# practices like async programming and data streaming.

## Features
- **Schema-based Validation:** Define validation rules for fields, including length constraints, mandatory fields, and more.
- **Chunked Processing:** Processes large datasets efficiently by splitting input into smaller chunks.
- **Customizable Rules:** Easily extendable with custom validation rules via interfaces.
- **Streaming Support:** Utilizes streaming techniques to avoid loading entire datasets into memory.
- **Detailed Logging:** Provides logging for tracking validation progress and debugging issues.
- **Cancellation Support:** Includes support for cancellation tokens to handle long-running operations gracefully.
- **Functional Programming Principles:** Implements immutability and higher-order functions where applicable.

## Installation
Clone or download the repository and add the project to your solution. Alternatively, you can include the library as a NuGet package if published.

```bash
# Clone the repository
git clone https://github.com/Faketa/JsonSchemaValidation.git
```

## Getting Started

### Prerequisites
- .NET 8.0 or later.
- **Dependencies**:
  - `Newtonsoft.Json`: For JSON serialization and deserialization.
  - `Microsoft.Extensions.Logging`: For structured logging.
  - `Microsoft.Extensions.Logging.Console`: For adding the Console.

### Example Usage

#### Basic Setup
```csharp
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
```

#### Schema Example
```json
{
  "name": { "length": 50, "mandatory": true },
  "email": { "length": 100, "mandatory": true },
  "age": { "length": 3, "mandatory": false }
}
```

#### Input Example
```json
[
  { "name": "John Doe", "email": "john.doe@example.com", "age": "30" },
  { "name": "Jane Smith", "email": "jane.smith@example.com", "age": "24" }
]
```

## Unit Testing
The `JsonSchemaValidation` library includes comprehensive unit tests to ensure reliability and correctness. Unit tests are implemented using the NUnit 3 framework and cover various edge cases for core components.

### Testing Framework
- **NUnit 3** is used for unit testing.
- **Moq** is utilized for mocking dependencies such as `ILogger`.

### Key Tests
#### JsonValidator
- **Validates and processes input:** Ensures that JSON input is validated against the schema and results are written to an output file.
- **Handles invalid schema:** Verifies that errors are logged when the schema is malformed.
- **Handles empty input:** Ensures that a log error is generated for empty JSON input.
- **Respects cancellation tokens:** Validates that operations can be canceled midway.

#### SchemaReader
- **Reads and deserializes schema:** Ensures that valid schemas are correctly parsed into objects.
- **Throws on malformed or empty schema:** Tests behavior for invalid JSON or empty schema files.
- **Handles cancellation tokens:** Validates cancellation behavior during schema reading.

#### InputProcessor
- **Processes input in chunks:** Ensures large datasets are split into manageable chunks.
- **Handles single and array JSON inputs:** Validates both single object and array structures.
- **Logs malformed records:** Ensures that invalid records are skipped with appropriate warnings.
- **Respects cancellation tokens:** Ensures that processing stops upon cancellation.

#### ResultWriter
- **Writes results to a file:** Validates that results are serialized and written correctly.
- **Handles cancellation tokens:** Ensures partial writes are canceled and incomplete files are cleaned up.
- **Throws on null output paths:** Ensures proper exceptions are thrown for invalid input.

### Running Tests
Run tests using the .NET CLI:
```bash
cd Tests

dotnet test
```

The test results will display in the console, indicating the success or failure of each test case.

## Architecture

### Core Components
1. **ValidationConfiguration**
   - Configures the chunk size and validation rules for the library.

2. **SchemaReader**
   - Reads and deserializes the schema from a JSON file.

3. **InputProcessor**
   - Splits the input JSON data into chunks for processing.

4. **ResultWriter**
   - Writes validation results to an output file in JSON format.

5. **JsonValidator**
   - The main class that orchestrates schema validation, input processing, and result writing.

### Custom Validation Rules
The library supports creating custom rules by implementing the `IValidationRule` interface. Two default rules are included:
- **LengthValidationRule**: Validates that a field does not exceed a specified length.
- **MandatoryValidationRule**: Ensures that a field is not null or empty if marked as mandatory.

### Flow Diagram
1. Read the schema using `SchemaReader`.
2. Process input data in chunks using `InputProcessor`.
3. Validate each record in a chunk against the schema.
4. Write validation results to an output file using `ResultWriter`.

### Folder Structure
```plaintext
src/
├── JsonValidator.cs
├── SchemaReader.cs
├── InputProcessor.cs
├── ResultWriter.cs
├── ValidationRules/
│   ├── IValidationRule.cs
│   ├── LengthValidationRule.cs
│   ├── MandatoryValidationRule.cs
├── Models/
│   ├── SchemaField.cs
│   ├── ValidationResult.cs
└── Configuration/
    └── ValidationConfiguration.cs
```

## Customization
To add a new validation rule, implement the `IValidationRule` interface:
```csharp
public class CustomRule : IValidationRule
{
    public ValidationResult Validate(string fieldName, string fieldValue, SchemaField schemaField)
    {
        // Custom validation logic
        return ValidationResult.Success(fieldName);
    }
}
```
Then include it in the `ValidationConfiguration.ValidationRules` collection.

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request with clear documentation of changes.

## Contact
For questions or feedback, please contact [info@cezarytomczak.pl].
