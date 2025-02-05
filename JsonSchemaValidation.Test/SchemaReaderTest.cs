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
        private string _testFilesPath;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _schemaReader = new SchemaReader(_loggerMock.Object);
            _testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");
        }

        [Test]
        public async Task ReadSchemaAsync_ValidSchema_ShouldReturnDictionary()
        {
            // Arrange
            var schemaJson = $"{_testFilesPath}schema.json";
            await using var schemaStream = File.OpenRead(schemaJson);

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
            var schemaJson = $"{_testFilesPath}schema-empty.json";
            await using var schemaStream = File.OpenRead(schemaJson);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _schemaReader.ReadSchemaAsync(schemaStream, CancellationToken.None));

            Assert.AreEqual("Schema is null, empty, or malformed.", ex.Message);
        }

        [Test]
        public async Task ReadSchemaAsync_SchemaWithExtraFields_ShouldIgnoreExtras()
        {
            // Arrange
            var schemaJson = $"{_testFilesPath}schema-with-extra-field.json";
            await using var schemaStream = File.OpenRead(schemaJson);

            // Act
            var result = await _schemaReader.ReadSchemaAsync(schemaStream, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("name"));
            Assert.IsTrue(result.ContainsKey("age"));
            Assert.IsTrue(result.ContainsKey("email"));
        }

        [Test]
        public async Task ReadSchemaAsync_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var schemaJson = $"{_testFilesPath}schema.json";
            await using var schemaStream = File.OpenRead(schemaJson);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await _schemaReader.ReadSchemaAsync(schemaStream, cts.Token));
        }
    }
}
