namespace AppHost.Neo4j;

public sealed class Neo4jServerResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "bolt";
    internal const string DashboardEndpointName = "http";

    public Neo4jServerResource(string name) : base(name)
    {
        PrimaryEndpoint = new(this, PrimaryEndpointName);
        DashboardEndpoint = new(this, DashboardEndpointName);
    }

    public EndpointReference PrimaryEndpoint { get; }

    public EndpointReference DashboardEndpoint { get; }

    public string Username => "neo4j";
    public string Password => "supersecretpassword";

    public string? GetConnectionString()
    {
        var uri = new Uri(PrimaryEndpoint.Value);
        return $"Host={uri.Scheme}://{uri.Host}:{uri.Port};Username={Username};Password={Password}";
    }

    public string? ConnectionStringExpression => $"wip";
}
