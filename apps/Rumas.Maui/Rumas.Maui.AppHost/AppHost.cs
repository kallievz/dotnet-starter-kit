var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Rumas_Maui>("rumas-maui");

builder.AddProject<Projects.Rumas_Maui_Web>("rumas-maui-web");

builder.Build().Run();
