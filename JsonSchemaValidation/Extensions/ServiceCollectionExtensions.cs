using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JsonSchemaValidation.Services;
using JsonSchemaValidation.ValidationRules;

namespace JsonSchemaValidation.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonSchemaValidation(this IServiceCollection services, IConfiguration configuration)
    {
        //Configuration
        services.Configure<ValidationConfiguration>(configuration.GetSection("ValidationSettings"));

        //Services
        services.AddSingleton<ConstraintValidator>();
        services.AddSingleton<SchemaReader>();
        services.AddSingleton<InputProcessor>();
        services.AddSingleton<ResultWriter>();
        services.AddSingleton<ChunkValidator>();
        services.AddSingleton<PostgreSQLDataProvider>();
        services.AddSingleton<JsonValidator>();

        //Logging
        services.AddLogging(builder => builder.AddConsole());

        return services;
    }
}
