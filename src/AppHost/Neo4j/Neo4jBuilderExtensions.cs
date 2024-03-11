namespace Aspire.Neo4j.AppHost.Neo4j;

public static class Neo4jBuilderExtensions
{
    public static IResourceBuilder<Neo4jServerResource> AddNeo4j(this IDistributedApplicationBuilder builder, string name, int? port = null)
    {
        var nats = new Neo4jServerResource(name);
        return builder.AddResource(nats)
                      .WithEndpoint(containerPort: 4222, hostPort: port, name: Neo4jServerResource.PrimaryEndpointName)
                      .WithAnnotation(new ContainerImageAnnotation { Image = "nats", Tag = "latest" })
                      .PublishAsContainer();
    }


    public static IResourceBuilder<Neo4jServerResource> PublishAsContainer(this IResourceBuilder<Neo4jServerResource> builder)
    {
        return builder.WithManifestPublishingCallback(context => context.WriteContainer(builder.Resource));
    }
}
