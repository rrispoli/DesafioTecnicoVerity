using DailyBalance.Application;
using DailyBalance.Infrastructure;
using DailyBalance.Infrastructure.Persistence;
using DailyBalance.WebApi.Endpoints;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using ServiceDefaults;
using Shared.WebApi.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Aspire service defaults (OpenTelemetry, health checks, service discovery)
    builder.AddServiceDefaults();

    // Serilog
    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    // Aspire-managed SQL Server
    builder.AddSqlServerDbContext<ApplicationDbContext>("dailybalance-database");

    // Application & Infrastructure
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();
    builder.Services.AddHttpContextAccessor();

    // Authentication and Authorization
    builder.Services
        .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, _ => { });
    builder.Services.AddAuthorization();

    // Global exception handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // OpenAPI with API key security scheme
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var info = document.Info ?? new OpenApiInfo();
            info.Title = "Desafio Técnico Verity - Daily Balance API";
            info.Description = "Serviço responsável pelo saldo diário consolidado";
            info.Contact = new OpenApiContact
            {
                Name = "Rafael Ríspoli",
                Email = "rispoli.rafael@gmail.com"
            };
            document.Info = info;

            var components = document.Components ?? new OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Description = "Enter your API key"
            };

            document.Components = components;

            var schemeReference = new OpenApiSecuritySchemeReference("ApiKey");
            var securityRequirement = new OpenApiSecurityRequirement
            {
                [schemeReference] = []
            };

            document.Security ??= [];
            document.Security.Add(securityRequirement);
            return Task.CompletedTask;
        });
    });

    // ProblemDetails
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Global exception handler
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Desafio Técnico Verity - Daily Balance API");
            options.WithTheme(ScalarTheme.BluePlanet);
            options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    // Map endpoints
    app.MapBalancesEndpoints();

    // Aspire default endpoints (health, alive)
    app.MapDefaultEndpoints();

    // Apply database migrations in development
    if (app.Environment.IsDevelopment())
        await ApplicationDbContextInitializer.ApplyMigrationsAsync(app.Services);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
