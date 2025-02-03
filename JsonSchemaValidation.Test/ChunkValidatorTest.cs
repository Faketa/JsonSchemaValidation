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
        private Mock<ILogger> _mockLogger;
        private ChunkValidator _chunkValidator;
        private ConstraintValidator _constraintValidator;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _constraintValidator = new ConstraintValidator(new IValidationRule[]
            {
                new LengthValidationRule(),
                new MandatoryValidationRule()
            });
            _chunkValidator = new ChunkValidator(_constraintValidator, _mockLogger.Object);
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
