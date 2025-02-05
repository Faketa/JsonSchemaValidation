using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonSchemaValidation.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSchemaValidation.Test.Extensions;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class InputProcessorTests
    {
        private InputProcessor _inputProcessor;
        private Mock<ILogger> _loggerMock;
        private string _testFilesPath;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _inputProcessor = new InputProcessor(_loggerMock.Object);
            _testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");
        }

        [Test]
        public void ChunkInputAsync_NullStream_ThrowsArgumentNullException()
        {
            var cancellationToken = CancellationToken.None;
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var _ in _inputProcessor.ChunkInputAsync(null, 2, cancellationToken)) { }
            });
        }
        
        [Test]
        public async Task ChunkInputAsync_ValidArrayInput_ShouldReturnChunks()
        {
            // Arrange
            var inputJson = $"{_testFilesPath}input-collection.json";
            await using var inputDataStream = File.OpenRead(inputJson);

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 2, CancellationToken.None).ToListAsync();

            // Assert
            Assert.AreEqual(2, chunks.Count());
            Assert.AreEqual(2, chunks.First().Count);
            Assert.AreEqual(1, chunks.Last().Count);
        }

        
        [Test]
        public async Task ChunkInputAsync_SingleObjectInput_ShouldReturnSingleChunk()
        {
            // Arrange
            var inputJson = $"{_testFilesPath}input.json";
            await using var inputDataStream = File.OpenRead(inputJson);

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 2, CancellationToken.None).ToListAsync();

            // Assert
            Assert.AreEqual(1, chunks.Count());
            Assert.AreEqual(1, chunks.First().Count);
        }

        [Test]
        public async Task ChunkInputAsync_EmptyArray_ShouldReturnNoChunks()
        {
            // Arrange
            var inputJson = "[]";
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 2, CancellationToken.None).ToListAsync();

            // Assert
            Assert.IsEmpty(chunks);
        }

        [Test]
        public async Task ChunkInputAsync_InvalidJson_ShouldLogError()
        {
            // Arrange
            var inputJson = $"{_testFilesPath}input-invalid.json";
            await using var inputDataStream = File.OpenRead(inputJson);

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 2, CancellationToken.None).ToListAsync();

            // Assert
            Assert.IsEmpty(chunks);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Malformed record skipped")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.AtLeastOnce);
        }

        [Test]
        public async Task ChunkInputAsync_EmptyJson_ShouldLogError()
        {
            // Arrange
            var inputJson = $"{_testFilesPath}input-empty.json";
            await using var inputDataStream = File.OpenRead(inputJson);

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 2, CancellationToken.None).ToListAsync();

            // Assert
            Assert.IsEmpty(chunks);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Input JSON is empty.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.AtLeastOnce);
        }

        [Test]
        public void ChunkInputAsync_NullStream_ShouldThrowException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _inputProcessor.ChunkInputAsync(null, 2, CancellationToken.None).ToListAsync());
        }

        [Test]
        public async Task ChunkInputAsync_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var inputJson = $"{_testFilesPath}input-collection.json";
            await using var inputDataStream = File.OpenRead(inputJson);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(0); // Immediately cancel

            // Act
            try
            {
                await _inputProcessor.ChunkInputAsync(inputDataStream, 2, cts.Token).ToListAsync();
                Assert.Fail("Expected TaskCanceledException was not thrown.");
            }
            catch (OperationCanceledException)
            {
                Assert.Pass("Task was successfully canceled.");
            }
        }
    }
}
