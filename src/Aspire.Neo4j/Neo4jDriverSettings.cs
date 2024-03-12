namespace Aspire.Neo4j;

/// <summary>
/// Provides the client configuration settings for connecting to a Neo4j graph database.
/// </summary>
public sealed class Neo4jDriverSettings
{
    /// <summary>
    /// Gets or sets the connection string of the Neo4j server to connect to.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the Neo4j health check is enabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="true"/>.
    /// </value>
    public bool HealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the OpenTelemetry tracing is enabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="true"/>.
    /// </value>
    public bool Tracing { get; set; } = true;
}
