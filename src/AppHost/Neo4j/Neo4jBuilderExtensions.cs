namespace Aspire.Neo4j.AppHost.Neo4j;
/*
public static class Neo4jBuilderExtensions
{
    public static IResourceBuilder<Neo4jServerResource> AddNeo4j(this IDistributedApplicationBuilder builder, string name, int? port = null)
    {
        var neo4j = new Neo4jServerResource(name);
        return builder.AddResource(neo4j)
                      .WithEndpoint(containerPort: 7474, hostPort: 7474, name: "tcp")
                      .WithEndpoint(containerPort: 7687, hostPort: port, scheme:"bolt", name: "bolt")
                      .WithImage("neo4j")
                      .WithEnvironment("NEO4J_AUTH", () => $"{neo4j.Username}/{neo4j.Password}")
                      .PublishAsContainer();
    }

    public static IResourceBuilder<Neo4jServerResource> PublishAsContainer(this IResourceBuilder<Neo4jServerResource> builder)
    {
        return builder.WithManifestPublishingCallback(context => context.WriteContainerAsync(builder.Resource));
    }

    public static IResourceBuilder<Neo4jServerResource> PublishAsConnectionString(this IResourceBuilder<Neo4jServerResource> builder)
    {
        return builder.WithManifestPublishingCallback(context => context.WriteConnectionString(builder.Resource));
    }
}
/**/