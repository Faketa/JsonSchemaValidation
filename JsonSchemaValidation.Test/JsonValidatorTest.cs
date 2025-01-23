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
        }

        [Test]
        public async Task ValidateAndProcessAsync_ValidInput_ShouldWriteResults()
        {
            // Arrange
            var schemaJson = "{" +
                             "  \"name\": { \"Length\": 10, \"Mandatory\": true }," +
                             "  \"email\": { \"Length\": 50, \"Mandatory\": true }" +
                             "}";
            var inputJson = "[{ \"name\": \"John\", \"email\": \"john@example.com\" }]";

            using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schemaJson));
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));
            var outputPath = "output.json";

            // Act
            await _validator.ValidateAndProcessAsync(schemaStream, inputDataStream, outputPath, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));

            var outputContent = await File.ReadAllTextAsync(outputPath);
            //Assert.That(outputContent, Does.Contain("IsValid": true));

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
