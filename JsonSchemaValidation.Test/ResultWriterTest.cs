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
using JsonSchemaValidation.Models;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class ResultWriterTest
    {
        private ResultWriter _resultWriter;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _resultWriter = new ResultWriter(_loggerMock.Object);
        }

        [Test]
        public async Task WriteResultsAsync_ValidResults_ShouldWriteToFile()
        {
            // Arrange
            var results = new List<ValidationResult>
            {
                ValidationResult.Success("Field1"),
                ValidationResult.Failure("Field2", "Error message")
            };
            var outputPath = "test_output.json";

            // Act
            await _resultWriter.WriteResultsAsync(outputPath, results, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Does.Contain("Field1"));
            Assert.That(outputContent, Does.Contain("Field2"));

            // Cleanup
            File.Delete(outputPath);
        }

        [Test]
        public void WriteResultsAsync_NullPath_ShouldThrowException()
        {
            // Arrange
            var results = new List<ValidationResult>
            {
                ValidationResult.Success("Field1")
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _resultWriter.WriteResultsAsync(null, results, CancellationToken.None));
        }

        [Test]
        public async Task WriteResultsAsync_EmptyResults_ShouldWriteEmptyArray()
        {
            // Arrange
            var results = new List<ValidationResult>();
            var outputPath = "test_empty_output.json";

            // Act
            await _resultWriter.WriteResultsAsync(outputPath, results, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Is.EqualTo("[]"));

            // Cleanup
            File.Delete(outputPath);
        }

        [Test]
        public async Task WriteResultsAsync_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var results = new List<ValidationResult>
                {
                    ValidationResult.Success("Field1"),
                    ValidationResult.Failure("Field2", "Error message")
                };
            var outputPath = "test_cancel_output.json";
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Immediately cancel.

            // Act & Assert
            try
            {
                await _resultWriter.WriteResultsAsync(outputPath, results, cts.Token);
                Assert.Fail("Expected TaskCanceledException was not thrown.");
            }
            catch (OperationCanceledException)
            {
                Assert.Pass("TaskCanceledException was successfully thrown.");
            }

            // Verify that the file does not exist.
            Assert.IsFalse(File.Exists(outputPath), "Output file should not exist after cancellation.");
        }

        [Test]
        public async Task WriteResultsAsync_InvalidPath_ShouldLogError()
        {
            // Arrange
            var results = new List<ValidationResult>
            {
                ValidationResult.Success("Field1")
            };
            var invalidPath = "/invalid_path/output.json";

            // Act
            await _resultWriter.WriteResultsAsync(invalidPath, results, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error writing results to file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }
    }
}
