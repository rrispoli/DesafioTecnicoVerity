using DailyBalance.Application;
using DailyBalance.Infrastructure;
using DailyBalance.Infrastructure.Persistence;
using DailyBalance.Worker.Consumers;
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
builder.AddSqlServerDbContext<ApplicationDbContext>("dailybalance-database");

// Aspire-managed RabbitMQ
builder.AddMassTransitRabbitMq("rabbitmq",
    massTransitConfiguration: registration =>
        registration.AddConsumer<EntryCreatedEventConsumer>());

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var host = builder.Build();
host.Run();
