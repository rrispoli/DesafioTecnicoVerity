using Entries.Application;
using Entries.Infrastructure;
using Entries.Infrastructure.Persistence;
using Entries.Worker;
using Serilog;
using ServiceDefaults;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Serilog
builder.Services.AddSerilog();

// Aspire-managed SQL Server
builder.AddSqlServerDbContext<ApplicationDbContext>("entries-database");

// Aspire-managed RabbitMQ
builder.AddMassTransitRabbitMq("rabbitmq");

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();
host.Run();
