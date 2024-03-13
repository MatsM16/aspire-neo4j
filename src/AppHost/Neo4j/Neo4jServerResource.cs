namespace Aspire.Neo4j.AppHost.Neo4j;
/*
public class Neo4jServerResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "bolt";
    internal const string DashboardEndpointName = "http";

    public Neo4jServerResource(string name) : base(name)
    {
        PrimaryEndpoint = new (this, PrimaryEndpointName);
        DashboardEndpoint = new (this, DashboardEndpointName);

        PasswordInput = new (this, "password");
    }

    public EndpointReference PrimaryEndpoint { get; }

    public EndpointReference DashboardEndpoint { get; }

    internal InputReference PasswordInput { get; }

    private ReferenceExpression ConnectionString =>
        ReferenceExpression.Create(
            $"bolt://neo4j:{PasswordInput}@{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");

    public string Username => "neo4j";
    public string Password => "neo4j";
    //public string Password => PasswordInput.Input.Value ?? throw new InvalidOperationException("Password cannot be null.");

    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default) => ConnectionString.GetValueAsync(cancellationToken);

    public string? ConnectionStringExpression => ConnectionString.ValueExpression;
}
/**/