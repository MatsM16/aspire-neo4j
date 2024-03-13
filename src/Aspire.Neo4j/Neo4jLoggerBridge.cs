using Microsoft.Extensions.Logging;

namespace Aspire.Neo4j;

#pragma warning disable CA2254 // Template should require an explicit type

internal class Neo4jLoggerBridge(ILogger logger) : global::Neo4j.Driver.ILogger
{
    public void Debug(string message, params object[] args) => logger.LogDebug(message, args);

    public void Error(Exception cause, string message, params object[] args) => logger.LogError(cause, message, args);

    public void Info(string message, params object[] args) => logger.LogInformation(message, args);

    public bool IsDebugEnabled() => logger.IsEnabled(LogLevel.Debug);

    public bool IsTraceEnabled() => logger.IsEnabled(LogLevel.Trace);

    public void Trace(string message, params object[] args) => logger.LogTrace(message, args);

    public void Warn(Exception cause, string message, params object[] args) => logger.LogWarning(cause, message, args);
}

#pragma warning restore CA2254 // Template should require an explicit type