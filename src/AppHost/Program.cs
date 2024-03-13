var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ApiService>("api");

builder.Build().Run();
