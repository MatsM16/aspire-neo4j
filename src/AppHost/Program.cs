using AppHost.Neo4j;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddNeo4j("neo4j");

builder.AddProject<Projects.ApiService>("api")
    .WithReference(db);

builder.Build().Run();
