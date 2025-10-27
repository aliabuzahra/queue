using Microsoft.EntityFrameworkCore;
using Serilog;
using VirtualQueue.Application;
using VirtualQueue.Infrastructure;
using VirtualQueue.Infrastructure.Data;
using VirtualQueue.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add the worker service
builder.Services.AddHostedService<Worker>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VirtualQueueDbContext>();
    await context.Database.EnsureCreatedAsync();
}

try
{
    Log.Information("Starting Virtual Queue Worker");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
