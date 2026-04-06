using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.Seed;
using ServicePulseMonitor.Features.Services;
using ServicePulseMonitor.Features.HealthChecks;
using ServicePulseMonitor.Hubs;
using ServicePulseMonitor.Options;
using ServicePulseMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Database Context
builder.Services.AddDbContext<ServicePulseDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardCors", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddHttpClient();
builder.Services.Configure<HealthCollectorOptions>(
    builder.Configuration.GetSection("HealthCollector"));
builder.Services.AddHostedService<HealthCollectorService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ServicePulseDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await context.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("DashboardCors");

app.UseAuthorization();

app.MapHub<HealthHub>("/hubs/health");
app.MapControllers();

app.Run();

/// <summary>Entry point — exposes <see cref="Program"/> as a partial class so test projects can reference it via <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program { }