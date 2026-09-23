var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

var entriesDatabase = sqlServer.AddDatabase("entries-database");

builder.AddProject<Projects.Entries_WebApi>("entries-webapi")
    .WithReference(entriesDatabase)
    .WaitFor(entriesDatabase)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Entries_Worker>("entries-worker")
    .WithReference(entriesDatabase)
    .WaitFor(entriesDatabase)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

var dailyBalanceDatabase = sqlServer.AddDatabase("dailybalance-database");

builder.AddProject<Projects.DailyBalance_WebApi>("dailybalance-webapi")
    .WithReference(dailyBalanceDatabase)
    .WaitFor(dailyBalanceDatabase)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.DailyBalance_Worker>("dailybalance-worker")
    .WithReference(dailyBalanceDatabase)
    .WaitFor(dailyBalanceDatabase)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

builder.Build().Run();
