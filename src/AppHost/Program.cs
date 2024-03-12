using Aspire.Neo4j.AppHost.Neo4j;

var builder = DistributedApplication.CreateBuilder(args);

//var neo4j = builder.AddNeo4j("neo4j")
//    .WithEnvironment("NEO4J_AUTH", "none");

builder.AddProject<Projects.ApiService>("apiservice");
    //.WithReference(neo4j);

builder.Build().Run();
