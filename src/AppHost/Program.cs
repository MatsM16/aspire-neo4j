using Aspire.Neo4j.AppHost.Neo4j;

var builder = DistributedApplication.CreateBuilder(args);

var neo4jServer = new Neo4jServerResource("neo4j");
builder.AddResource(neo4jServer)

builder.Build().Run();
