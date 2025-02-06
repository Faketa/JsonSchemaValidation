using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using JsonSchemaValidation.Services;
using JsonSchemaValidation.ValidationRules;
using Microsoft.Extensions.Options;
using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class JsonValidatorTest
    {
        private Mock<ConstraintValidator> _mockConstraintValidator;
        private Mock<SchemaReader> _mockSchemaReader;
        private Mock<InputProcessor> _mockInputProcessor;
        private Mock<ResultWriter> _mockResultWriter;
        private Mock<ChunkValidator> _mockChunkValidator;
        private Mock<PostgreSQLDataProvider> _mockPostgreSQLDataProvider;
        private Mock<ILogger<JsonValidator>> _mockLogger;
        private Mock<IOptions<ValidationConfiguration>> _mockConfig;
        private JsonValidator _jsonValidator;

        private string _testFilesPath;

        [SetUp]
        public void SetUp()
        {
            _testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");

            var mockMandatoryRule = new Mock<IValidationRule>();
            mockMandatoryRule
                .Setup(r => r.Validate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SchemaField>()))
                .Returns((string field, string value, SchemaField schemaField) =>
                    string.IsNullOrEmpty(value)
                        ? ValidationResult.Failure(field, "Field is required.")
                        : ValidationResult.Success(field));

            var mockLengthRule = new Mock<IValidationRule>();
            mockLengthRule
                .Setup(r => r.Validate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SchemaField>()))
                .Returns((string field, string value, SchemaField schemaField) =>
                    value.Length > schemaField.Length
                        ? ValidationResult.Failure(field, $"Field exceeds max length of {schemaField.Length}.")
                        : ValidationResult.Success(field));

            var validationRules = new List<IValidationRule> { mockMandatoryRule.Object, mockLengthRule.Object };

            _mockConstraintValidator = new Mock<ConstraintValidator>(validationRules);
            _mockSchemaReader = new Mock<SchemaReader>(Mock.Of<ILogger<SchemaReader>>());
            _mockInputProcessor = new Mock<InputProcessor>(Mock.Of<IOptions<ValidationConfiguration>>(), Mock.Of<ILogger<InputProcessor>>());
            _mockResultWriter = new Mock<ResultWriter>(Mock.Of<ILogger<ResultWriter>>());
            _mockChunkValidator = new Mock<ChunkValidator>(_mockConstraintValidator.Object, Mock.Of<ILogger<ChunkValidator>>());
            _mockPostgreSQLDataProvider = new Mock<PostgreSQLDataProvider>(Mock.Of<IOptions<ValidationConfiguration>>(), Mock.Of<ILogger<PostgreSQLDataProvider>>());
            _mockLogger = new Mock<ILogger<JsonValidator>>();
            _mockConfig = new Mock<IOptions<ValidationConfiguration>>();
            _mockConfig.Setup(c => c.Value).Returns(new ValidationConfiguration { ChunkSize = 2 });

            _jsonValidator = new JsonValidator(
                _mockConfig.Object,
                _mockConstraintValidator.Object,
                _mockSchemaReader.Object,
                _mockInputProcessor.Object,
                _mockResultWriter.Object,
                _mockChunkValidator.Object,
                _mockPostgreSQLDataProvider.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task ValidateAsync_ValidInput_ShouldWriteResults()
        {
            // Arrange
            var schemaJson = $"{_testFilesPath}schema.json";
            var inputJson = $"{_testFilesPath}input.json";
            var outputPath = $"{_testFilesPath}output.json";

            // Act
            await _jsonValidator.ValidateAsync(schemaJson, inputJson, outputPath, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));

            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Does.Contain("\"IsValid\": true"));
        }
    }
}
