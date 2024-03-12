using Aspire.Neo4j;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddNeo4jDriver("neo4j");

builder.Services.AddKeyedTransient("friendDb", (sp, key) =>
{
    var driver = sp.GetRequiredService<IDriver>();
    return driver.AsyncSession(x => x.WithDatabase(key.ToString()));
});

var app = builder.Build();

await using var session = app.Services.GetRequiredKeyedService<IAsyncSession>("friendDb");
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

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/person", ([FromKeyedServices("friendDb")] IAsyncSession session) =>
{
    var result = session.ExecuteReadAsync(async query =>
    {
        var cursor = await query.RunAsync("MATCH (n:Person) RETURN n");
        var peopleRecords = await cursor.ToListAsync();
        return peopleRecords.Select(record => new Person(record["name"].As<string>()));
    });

})
.WithName("GetPeople")
.WithOpenApi();

app.MapGet("/person/{name}", ([FromKeyedServices("friendDb")] IAsyncSession session, string name) =>
{
    var result = session.ExecuteReadAsync(async query =>
    {
        var cursor = await query.RunAsync("MATCH (n:Person {name: $name}) RETURN n", new { name });
        var peopleRecords = await cursor.ToListAsync();
        return peopleRecords.Select(record => new Person(record["name"].As<string>())).FirstOrDefault();
    });

})
.WithName("GetPerson")
.WithOpenApi();

app.MapGet("/person/{name}/friends", ([FromKeyedServices("friendDb")] IAsyncSession session, string name) =>
{
    var result = session.ExecuteReadAsync(async query =>
    {
        var cursor = await query.RunAsync("MATCH (a:Person {name: $name}) -[:KNOWS]->(b:Person) RETURN b", new { name });
        var peopleRecords = await cursor.ToListAsync();
        return peopleRecords.Select(record => new Person(record["name"].As<string>())).FirstOrDefault();
    });

})
.WithName("GetFriends")
.WithOpenApi();

app.MapPost("/person", ([FromKeyedServices("friendDb")] IAsyncSession session, [FromBody] Person person) =>
{
    var result = session.ExecuteWriteAsync(async query =>
    {
        await query.RunAsync("CREATE (n:Person {name: $name})", new { name = person.Name });
    });

})
.WithName("CreatePerson")
.WithOpenApi();

app.MapPost("/person/{name}/friends", ([FromKeyedServices("friendDb")] IAsyncSession session, string name, [FromBody] Person person) =>
{
    var result = session.ExecuteWriteAsync(async query =>
    {
        await query.RunAsync(
            """
            MATCH (a:Person {name: $name_a})
            MATCH (b:Person {name: $name_b})
            MERGE (a)-[:KNOWS]->(b)
            MERGE (b)-[:KNOWS]->(a)
            """, new { name_a = person.Name, name_b = name });
    });

})
.WithName("MakeFriend")
.WithOpenApi();

app.Run();


record Person(string Name);