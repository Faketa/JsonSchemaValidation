using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using JsonSchemaValidation.Models;
using JsonSchemaValidation.Services;
using JsonSchemaValidation.ValidationRules;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class ChunkValidatorTest
    {
        private Mock<ILogger<ChunkValidator>> _mockLogger;
        private Mock<ConstraintValidator> _mockConstraintValidator;
        private ChunkValidator _chunkValidator;

        [SetUp]
        public void Setup()
        {
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
            _mockLogger = new Mock<ILogger<ChunkValidator>>();

            _chunkValidator = new ChunkValidator(_mockConstraintValidator.Object, _mockLogger.Object);
        }

        [Test]
        public void ValidateChunk_ValidData_ShouldReturnValidResults()
        {
            var schema = new Dictionary<string, SchemaField>
            {
                { "name", new SchemaField(10, true, null, null) }
            };

            var chunk = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "name", "John Doe" } }
            };

            var results = _chunkValidator.ValidateChunk(chunk, schema);
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.First().IsValid, Is.True);
        }

        [Test]
        public void ValidateChunk_InvalidData_ShouldReturnErrors()
        {
            var schema = new Dictionary<string, SchemaField>
            {
                { "age", new SchemaField(3, true, null, null) }
            };

            var chunk = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "age", "3000" } }
            };

            var results = _chunkValidator.ValidateChunk(chunk, schema);
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.First().IsValid, Is.False);
        }
        
        [Test]
        public void ValidateChunk_EmptyChunk_ShouldReturnEmptyResults()
        {
            var schema = new Dictionary<string, SchemaField>
            {
                { "name", new SchemaField(10, true, null, null) }
            };

            var chunk = new List<Dictionary<string, string>>();

            var results = _chunkValidator.ValidateChunk(chunk, schema);
            Assert.That(results, Is.Empty);
        }
    }
}
