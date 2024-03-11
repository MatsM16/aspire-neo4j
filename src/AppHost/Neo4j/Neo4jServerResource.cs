namespace Aspire.Neo4j.AppHost.Neo4j;

public class Neo4jServerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "tcp";
    internal const string PrimarySchemeName = "bolt";
}
