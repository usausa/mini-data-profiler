var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Example_Api>("api")
    .WithExternalHttpEndpoints();

builder.Build().Run();
