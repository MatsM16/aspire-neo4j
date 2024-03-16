using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Neo4j.Driver;

namespace Aspire.Neo4j;

/// <summary>
/// Extension methods for connecting to a Neo4j graph database.
/// </summary>
public static class AspireNeo4jExtensions
{
    private const string DefaultConfigSectionName = "Aspire:Neo4j:Driver";
    private const string ActivitySourceName = "Aspire.Neo4j.Driver";

    /// <summary>
    /// Registers <see cref="IDriver"/> as a singleton in the services provided by the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to read config from and add services to.</param>
    /// <param name="name">A name used to retrieve the connection string from the <c>ConnectionStrings</c> configuration section.</param>
    /// <param name="configureSettings">An optional method that can be used for customizing the <see cref="Neo4jDriverSettings"/>. It is invoked after the settings are read from the configuration.</param>
    /// <param name="configureDriver">An optional method that can be used for customizing the drivers configuration through <see cref="ConfigBuilder"/>. It is invoked after the settings are read from the configuration.</param>
    public static void AddNeo4jDriver(
        this IHostApplicationBuilder builder, 
        string name,
        Action<Neo4jDriverSettings>? configureSettings = null,
        Action<ConfigBuilder>? configureDriver = null)
    {
        AddNeo4jDriver(builder, DefaultConfigSectionName, configureSettings, configureDriver, name, serviceKey:null);
    }

    /// <summary>
    /// Registers <see cref="IDriver"/> as a keyed singleton in the services provided by the <paramref name="builder"/>. The service key is <paramref name="name"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to read config from and add services to.</param>
    /// <param name="name">A name used to retrieve the connection string from the <c>ConnectionStrings</c> configuration section.</param>
    /// <param name="configureSettings">An optional method that can be used for customizing the <see cref="Neo4jDriverSettings"/>. It is invoked after the settings are read from the configuration.</param>
    /// <param name="configureDriver">An optional method that can be used for customizing the drivers configuration through <see cref="ConfigBuilder"/>. It is invoked after the settings are read from the configuration.</param>
    public static void AddKeyedNeo4jDriver(
        this IHostApplicationBuilder builder,
        string name,
        Action<Neo4jDriverSettings>? configureSettings = null,
        Action<ConfigBuilder>? configureDriver = null)
    {
        AddNeo4jDriver(builder, $"{DefaultConfigSectionName}:{name}", configureSettings, configureDriver, name, serviceKey:name);
    }

    private static void AddNeo4jDriver(
        IHostApplicationBuilder builder,
        string configurationSectionName,
        Action<Neo4jDriverSettings>? configureSettings,
        Action<ConfigBuilder>? configureDriver,
        string connectionName,
        object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var configSection = builder.Configuration.GetSection(configurationSectionName);

        var settings = new Neo4jDriverSettings();
        configSection.Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string configConnectionString)
        {
            settings.ConnectionString = configConnectionString;
        }

        configureSettings?.Invoke(settings);

        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString, nameof(settings) + '.' + nameof(settings.ConnectionString));

        var connectionString = new DbConnectionStringBuilder
        {
            ConnectionString = settings.ConnectionString
        };

        var host = GetHost(connectionString);
        var authToken = GetAuthToken(connectionString);

        IDriver CreateDriver(IServiceProvider services)
        {
            var logger = serviceKey is null
                ? services.GetRequiredService<Neo4jLoggerBridge>()
                : services.GetRequiredKeyedService<Neo4jLoggerBridge>(serviceKey);

            var driver = GraphDatabase.Driver(host, authToken, config =>
            {
                config.WithLogger(logger);
                configureDriver?.Invoke(config);
            });

            return driver;
        }

        if (serviceKey is null)
        {
            builder.Services.AddSingleton(CreateDriver);
            builder.Services.AddSingleton(sp => new Neo4jLoggerBridge(sp.GetRequiredService<ILoggerFactory>().CreateLogger("Neo4j.Driver")));
        }
        else
        {
            builder.Services.AddKeyedSingleton(serviceKey, CreateDriver);
            builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => new Neo4jLoggerBridge(sp.GetRequiredService<ILoggerFactory>().CreateLogger("Neo4j.Driver_" + serviceKey)));
        }

        if (settings.HealthChecks)
        {
            builder.Services.AddHealthChecks().Add(new HealthCheckRegistration(
                serviceKey is null ? "neo4j" : $"neo4j_{serviceKey}",
                sp => new Neo4jHealthCheck(serviceKey is null
                    ? sp.GetRequiredService<IDriver>()
                    : sp.GetRequiredKeyedService<IDriver>(serviceKey)),
                failureStatus: default,
                tags: default,
                timeout: default));
        }

        if (settings.Tracing)
        {
            // Add OpenTelemetry tracing
            builder.Services
                .AddOpenTelemetry()
                .WithTracing(tracerBuilder =>
                {
                    tracerBuilder.AddSource(ActivitySourceName);
                });
        }

        if (settings.Metrics)
        {
            // Add OpenTelemetry tracing
            builder.Services
                .AddOpenTelemetry()
                .WithTracing(tracerBuilder =>
                {
                    tracerBuilder.AddSource(ActivitySourceName);
                });
        }
    }

    private static string GetHost(DbConnectionStringBuilder connectionString)
    {
        if (connectionString["host"] is string host)
        {
            return host;
        }

        throw new FormatException("The connection string must contain a 'host' property.");
    }

    private static IAuthToken GetAuthToken(DbConnectionStringBuilder connectionString)
    {
        if (connectionString["username"] is string username && connectionString["password"] is string password)
        {
            return AuthTokens.Basic(username, password);
        }

        if (connectionString["bearer"] is string bearerToken)
        {
            return AuthTokens.Bearer(bearerToken);
        }

        if (connectionString["kerberos"] is string kerberosToken)
        {
            return AuthTokens.Kerberos(kerberosToken);
        }

        return AuthTokens.None;
    }
}