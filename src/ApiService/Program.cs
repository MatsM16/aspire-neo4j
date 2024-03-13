using Aspire.Neo4j;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4j.Driver.Preview.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddNeo4jDriver("neo4j", settings =>
{
    settings.ConnectionString = "bolt://localhost:7687";
});

var app = builder.Build();
/*
await using var session = app.Services.GetRequiredKeyedService<IAsyncSession>(DB_NAME);
await session.ExecuteWriteAsync(async query =>
{
    await query.RunAsync(
        """
        CREATE (a:Person {name: 'Alice'})
        CREATE (b:Person {name: 'Bob'})
        CREATE (c:Person {name: 'Chad'})
        CREATE (d:Person {name: 'David'})
        CREATE (e:Person {name: 'Eve'})

        MERGE (a)-[:KNOWS]->(b)
        MERGE (b)-[:KNOWS]->(a)
        
        MERGE (a)-[:KNOWS]->(c)
        MERGE (c)-[:KNOWS]->(a)

        MERGE (b)-[:KNOWS]->(c)
        MERGE (c)-[:KNOWS]->(b)
        
        MERGE (d)-[:KNOWS]->(e)
        MERGE (e)-[:KNOWS]->(d)
        
        MERGE (c)-[:KNOWS]->(d)
        MERGE (d)-[:KNOWS]->(c)
        """);
});
/**/
app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/greet", () => Results.Ok("Hello World!"));


app.MapGet("/person", async (IDriver driver, CancellationToken cancellationToken) =>
{
    var result = await driver
        .ExecutableQuery("MATCH (n:Person) RETURN n.name as Name")
        .ExecuteAsync(cancellationToken)
        .AsObjectsAsync<Person>()
        .ConfigureAwait(false);

    return Results.Ok(result);
})
.WithName("GetPeople")
.WithOpenApi()
.Produces<List<Person>>(200);

app.MapGet("/person/{name}", async (IDriver driver, string name, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrEmpty(name))
        return Results.Problem("Name is required", statusCode: 400);

    var result = await driver
        .ExecutableQuery("MATCH (n:Person {name: $name}) RETURN n.name as Name").WithParameters(new { name })
        .ExecuteAsync(cancellationToken)
        .AsObjectsAsync<Person>()
        .ConfigureAwait(false);

    return Results.Ok(result.FirstOrDefault());

})
.WithName("GetPerson")
.WithOpenApi()
.Produces<Person>(200)
.ProducesProblem(400);

app.MapGet("/person/{name}/friends", async (IDriver driver, string name, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrEmpty(name))
        return Results.Problem("Name is required", statusCode: 400);

    var result = await driver
        .ExecutableQuery("MATCH (a:Person {name: $name}) -[:KNOWS]->(b:Person) RETURN b.name as Name").WithParameters(new { name })
        .ExecuteAsync(cancellationToken)
        .AsObjectsAsync<Person>()
        .ConfigureAwait(false);

    return Results.Ok(result);
})
.WithName("GetFriends")
.WithOpenApi()
.Produces<List<Person>>(200)
.ProducesProblem(400);

app.MapPost("/person", async (IDriver driver, [FromBody] Person person, CancellationToken cancellationToken) =>
{
    if (person is null)
        return Results.Problem("Person is required", statusCode: 400);

    if (string.IsNullOrEmpty(person.Name))
        return Results.Problem("Person.Name is required", statusCode: 400);

    await driver
        .ExecutableQuery("CREATE (n:Person {name: $name})").WithParameters(new { name = person.Name })
        .ExecuteAsync(cancellationToken);

    return Results.Ok(person);
})
.WithName("CreatePerson")
.WithOpenApi()
.Produces<Person>(200)
.ProducesProblem(400);

app.MapPost("/person/{name}/friends", async (IDriver driver, string name, [FromBody] Person person, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrEmpty(name))
        return Results.Problem("Name is required", statusCode: 400);

    if (person is null)
        return Results.Problem("Person is required", statusCode: 400);

    if (string.IsNullOrEmpty(person.Name))
        return Results.Problem("Person.Name is required", statusCode: 400);

    await driver
        .ExecutableQuery("""
            MATCH (a:Person {name: $name_a})
            MATCH (b:Person {name: $name_b})
            MERGE (a)-[:KNOWS]->(b)
            MERGE (b)-[:KNOWS]->(a)
            """)
        .WithParameters(new { name_a = person.Name, name_b = name })
        .ExecuteAsync(cancellationToken)
        .ConfigureAwait(false);

    return Results.Ok();

})
.WithName("MakeFriend")
.WithOpenApi()
.Produces(200)
.ProducesProblem(400);

app.Run();

record Person()
{
    public string? Name { get; init; }
}