# JsonSchemaValidation Library

## Overview
The `JsonSchemaValidation` library provides a modular, scalable, and efficient framework for validating JSON data against customizable schema rules. It is designed for performance, maintainability, and extensibility, leveraging modern C# practices, including asynchronous programming, dependency injection, and separation of concerns.

## Features
- **Schema-based Validation:** Define validation rules for fields, including length constraints, mandatory fields, and more.
- **Chunked Processing:** Processes large datasets efficiently by splitting input into smaller chunks.
- **Customizable Rules:** Easily extendable with custom validation rules via interfaces.
- **Streaming Support:** Utilizes streaming techniques to avoid loading entire datasets into memory.
- **Detailed Logging:** Provides granular logging for tracking validation progress and debugging issues.
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

### Example Usage

#### Basic Setup
```csharp
using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation;

class Program
{
    static async Task Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("JsonValidatorLogger");

        var configuration = new ValidationConfiguration
        {
            ChunkSize = 100,
            ConstraintValidator = new IValidationRule[]
            {
                new LengthValidationRule(),
                new MandatoryValidationRule()
            }
        };

        var validator = new JsonValidator(configuration, logger);

        await using var schemaStream = File.OpenRead("schema.json");
        await using var inputStream = File.OpenRead("input.json");

        var outputPath = "output.json";

        await validator.ValidateAndProcessAsync(schemaStream, inputStream, outputPath, CancellationToken.None);
        Console.WriteLine("Validation completed.");
    }
}
```

#### Schema Example
```json
{
  "name": { "Length": 50, "Mandatory": true },
  "email": { "Length": 100, "Mandatory": true },
  "age": { "Length": 3, "Mandatory": false }
}
```

#### Input Example
```json
[
  { "name": "John Doe", "email": "john.doe@example.com", "age": "30" },
  { "name": "Jane Smith", "email": "jane.smith@example.com" }
]
```

### Unit Testing
Unit tests for all major components are included using NUnit 3. To run the tests:

1. Navigate to the `Tests` directory.
2. Execute the tests using the .NET CLI:
   ```bash
   dotnet test
   ```

## Architecture

### Core Components
- **JsonValidator:** Orchestrates the validation process.
- **SchemaReader:** Reads and deserializes schema definitions.
- **InputProcessor:** Processes input data in manageable chunks.
- **ResultWriter:** Writes validation results to output files.

### Validation Rules
- **LengthValidationRule:** Ensures field values do not exceed the defined maximum length.
- **MandatoryValidationRule:** Ensures mandatory fields are present and non-empty.

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

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request with clear documentation of changes.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Contact
For questions or feedback, please contact [info@cezarytomczak.pl].
