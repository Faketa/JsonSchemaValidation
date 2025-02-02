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

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class InputProcessorTests
    {
        private InputProcessor _inputProcessor;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _inputProcessor = new InputProcessor(_loggerMock.Object);
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
            var inputJson = "[" +
                            "{\"name\":\"John\",\"email\":\"John.Doe@example.com\",\"age\":\"30\"}," +
                            "{\"name\":\"Jane\",\"email\":\"John.Doe@example.com\",\"age\":\"25\"}," +
                            "{\"name\":\"Doe\",\"email\":\"John.Doe@example.com\",\"age\":\"40\"}" +
                            "]";
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));

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
            var inputJson = "{\"name\":\"John\",\"email\":\"John.doe@example.com\",\"age\":\"30\"}";
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));

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
            var inputJson = "[{\"name\":\"John"; // Malformed JSON
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));

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
            var inputJson = "[" +
                            "{\"name\":\"John\",\"email\":\"john.doe@example.com\",\"age\":\"30\"}," +
                            "{\"name\":\"Jane\",\"email\":\"Jane.doe@example.com\",\"age\":\"25\"}" +
                            "]";
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));
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

        [Test]
        public async Task ChunkInputAsync_LargeInput_ShouldProcessEfficiently()
        {
            // Arrange
            var inputJson = new StringBuilder("[");
            for (int i = 0; i < 10000; i++)
            {
                inputJson.Append($"{{\"name\":\"User{i}\",\"email\":\"User{i}@example.com\",\"age\":\"{20 + i % 50}\"}},");
            }

            inputJson.Remove(inputJson.Length - 1, 1).Append("]");
            using var inputDataStream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson.ToString()));

            // Act
            var chunks = await _inputProcessor.ChunkInputAsync(inputDataStream, 100, CancellationToken.None).ToListAsync();

            // Assert
            Assert.AreEqual(100, chunks.Count());
            Assert.AreEqual(100, chunks.First().Count);
        }
    }

    /// <summary>
    /// Helper extension method to convert IAsyncEnumerable to List.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }
            return list;
        }
    }
}
