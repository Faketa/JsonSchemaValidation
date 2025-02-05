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
        private Mock<ILogger<ResultWriter>> _mockLogger;
        private string _testFilesPath;

        [SetUp]
        public void SetUp()
        {
            _testFilesPath = Path.GetFullPath(@"..\..\..\..\JsonSchemaValidation.Test\Testfiles\");
            _mockLogger = new Mock<ILogger<ResultWriter>>();
            _resultWriter = new ResultWriter(_mockLogger.Object);
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
            var outputPath = $"{_testFilesPath}output-test.json";

            // Act
            await _resultWriter.WriteResultsAsync(outputPath, results, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Does.Contain("Field1"));
            Assert.That(outputContent, Does.Contain("Field2"));
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
            var outputPath = $"{_testFilesPath}output-empty-results-test.json";

            // Act
            await _resultWriter.WriteResultsAsync(outputPath, results, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(outputPath));
            var outputContent = await File.ReadAllTextAsync(outputPath);
            Assert.That(outputContent, Is.EqualTo("[]"));
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
            var outputPath = $"{_testFilesPath}output-cancel-test.json";

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
            _mockLogger.Verify(
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
