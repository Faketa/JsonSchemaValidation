using JsonSchemaValidation.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaValidation.Test;

[TestFixture]
public class PostgreSQLDataProviderTest
{
    private Mock<ILogger<PostgreSQLDataProvider>> _mockLogger;
    private Mock<IOptions<ValidationConfiguration>> _mockConfig;
    private PostgreSQLDataProvider _dataProvider;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<PostgreSQLDataProvider>>();
        _mockConfig = new Mock<IOptions<ValidationConfiguration>>();
        _mockConfig.Setup(c => c.Value).Returns(new ValidationConfiguration { ConnectionString = "Host=host;Username=user;Password=pass;Database=postgres;Port=5432" });
        _dataProvider = new PostgreSQLDataProvider(_mockConfig.Object, _mockLogger.Object);
    }

    [Test]
    public async Task ReadJsonDataAsync_ValidTableAndColumn_ShouldReturnStream()
    {
        var tableName = "Custom";
        var columnName = "data";

        using var memoryStream = await _dataProvider.ReadJsonDataAsync(tableName, columnName);
        Assert.That(memoryStream, Is.Not.Null);

        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        var result = await reader.ReadToEndAsync();
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task ReadJsonDataAsync_DatabaseDoesNotExist_ShouldThrowException()
    {
        var failingConfig = new Mock<IOptions<ValidationConfiguration>>();
        failingConfig.Setup(c => c.Value).Returns(new ValidationConfiguration { ConnectionString = "Host=host;Username=user;Password=pass;Database=wrong_database;Port=5432" });
        var failingProvider = new PostgreSQLDataProvider(failingConfig.Object, _mockLogger.Object);

        var tableName = "Custom";
        var columnName = "data";

        Assert.ThrowsAsync<Npgsql.PostgresException>(async () => await failingProvider.ReadJsonDataAsync(tableName, columnName));
    }

    [Test]
    public async Task ReadJsonDataAsync_ConnectionWrongHost_ShouldThrowException()
    {
        var failingConfig = new Mock<IOptions<ValidationConfiguration>>();
        failingConfig.Setup(c => c.Value).Returns(new ValidationConfiguration { ConnectionString = "Host=wrong_host;Username=user;Password=pass;Database=wrong_database;Port=5432" });
        var failingProvider = new PostgreSQLDataProvider(failingConfig.Object, _mockLogger.Object);

        var tableName = "Custom";
        var columnName = "data";

        Assert.ThrowsAsync<System.Net.Sockets.SocketException>(async () => await failingProvider.ReadJsonDataAsync(tableName, columnName));
    }
}

