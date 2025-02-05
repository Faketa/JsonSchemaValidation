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

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class JsonValidatorTest
    {
        private JsonValidator _validator;
        private Mock<ILogger> _loggerMock;
        private ValidationConfiguration _configuration;
        private string _testFilesPath;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _configuration = new ValidationConfiguration
            {
                ChunkSize = 2,
                ValidationRules = new IValidationRule[]
                {
                    new LengthValidationRule(),
                    new MandatoryValidationRule()
                }
            };
            _validator = new JsonValidator(_configuration, _loggerMock.Object);
            _testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");
        }

        [Test]
        public async Task ValidateAsync_ValidInput_ShouldWriteResults()
        {
            // Arrange
            var schemaJson = $"{_testFilesPath}schema.json";
            var inputJson = $"{_testFilesPath}input.json";
            var outputPath = $"{_testFilesPath}output.json";

            // Act
            await _validator.ValidateAsync(schemaJson, inputJson, outputPath, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));

            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Does.Contain("\"IsValid\": true"));

            // Cleanup
            File.Delete(outputPath);
        }

        [Test]
        public void Constructor_NullLogger_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JsonValidator(_configuration, null));
        }

        [Test]
        public void Constructor_NullConfiguration_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JsonValidator(null, _loggerMock.Object));
        }
    }
}
