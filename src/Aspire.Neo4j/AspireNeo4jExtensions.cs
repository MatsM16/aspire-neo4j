using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ConnectionString = connectionString;
        }

        configureSettings?.Invoke(settings);

        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString, nameof(settings) + '.' + nameof(settings.ConnectionString));

        IDriver CreateDriver(IServiceProvider services)
        {
            var connectionDetails = new Uri(settings.ConnectionString);
            var host = $"{connectionDetails.Scheme}://{connectionDetails.Host}:{connectionDetails.Port}";
            var token = connectionDetails.UserInfo?.Split(':') is [ { Length: > 0} username, { Length: > 0 } password]
                ? AuthTokens.Basic(username, password)
                : AuthTokens.None;

            var driver = GraphDatabase.Driver(host, token, config =>
            {
                config.WithLogger(services.GetRequiredService<Neo4jLoggerBridge>());
                configureDriver?.Invoke(config);
            });

            return driver;
        }

        if (serviceKey is null)
        {
            builder.Services.AddSingleton(CreateDriver);
        }
        else
        {
            builder.Services.AddKeyedSingleton(serviceKey, CreateDriver);
        }

        // We only need one of these per application.
        builder.Services.TryAddSingleton<Neo4jLoggerBridge>();
    }
}