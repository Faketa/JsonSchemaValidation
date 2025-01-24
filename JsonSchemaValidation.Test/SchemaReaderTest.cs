using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonSchemaValidation.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class SchemaReaderTest
    {
        private SchemaReader _schemaReader;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _schemaReader = new SchemaReader(_loggerMock.Object);
        }

        [Test]
        public async Task ReadSchemaAsync_ValidSchema_ShouldReturnDictionary()
        {
            // Arrange
            var schemaJson = "{" +
                             "  \"name\": { \"Length\": 10, \"Mandatory\": true }," +
                             "  \"email\": { \"Length\": 50, \"Mandatory\": true }," +
                             "  \"age\": { \"Length\": 3, \"Mandatory\": false }" +
                             "}";
            using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schemaJson));

            // Act
            var result = await _schemaReader.ReadSchemaAsync(schemaStream, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.ContainsKey("name"));
            Assert.IsTrue(result.ContainsKey("email"));
            Assert.IsTrue(result.ContainsKey("age"));
        }

        [Test]
        public async Task ReadSchemaAsync_EmptySchema_ShouldThrowException()
        {
            // Arrange
            var emptySchemaJson = "{}";
            using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(emptySchemaJson));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _schemaReader.ReadSchemaAsync(schemaStream, CancellationToken.None));

            Assert.AreEqual("Schema is null, empty, or malformed.", ex.Message);
        }

        [Test]
        public async Task ReadSchemaAsync_SchemaWithExtraFields_ShouldIgnoreExtras()
        {
            // Arrange
            var schemaJson = "{" +
                             "  \"name\": { \"Length\": 10, \"Mandatory\": true, \"ExtraField\": \"Ignored\" }" +
                             "}";
            using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schemaJson));

            // Act
            var result = await _schemaReader.ReadSchemaAsync(schemaStream, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("name"));
        }

        [Test]
        public async Task ReadSchemaAsync_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var schemaJson = "{" +
                             "  \"name\": { \"Length\": 10, \"Mandatory\": true }" +
                             "}";
            using var schemaStream = new MemoryStream(Encoding.UTF8.GetBytes(schemaJson));
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await _schemaReader.ReadSchemaAsync(schemaStream, cts.Token));
        }
    }
}
