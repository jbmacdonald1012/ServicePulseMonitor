using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Models;
using System.Text.Json;

namespace ServicePulseMonitor.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(ServicePulseDbContext context)
    {
        if (await context.Services.AnyAsync())
        {
            return;
        }

        var users = CreateUsers();
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var services = CreateServices();
        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();

        var healthChecks = CreateHealthChecks(services);
        await context.HealthChecks.AddRangeAsync(healthChecks);

        var dependencies = CreateDependencies(services);
        await context.ServiceDependencies.AddRangeAsync(dependencies);

        var alertRules = CreateAlertRules(services);
        await context.AlertRules.AddRangeAsync(alertRules);

        await context.SaveChangesAsync();
    }

    private static List<User> CreateUsers()
    {
        return new List<User>
        {
            new User
            {
                UserGuid = Guid.NewGuid(),
                DisplayName = "Admin User",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                AccessLevel = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserGuid = Guid.NewGuid(),
                DisplayName = "Test User",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                AccessLevel = "User",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Service> CreateServices()
    {
        return new List<Service>
        {
            new Service
            {
                ServiceName = "User Service",
                BaseUrl = "http://localhost:5001",
                Description = "Handles user authentication and management",
                RegisteredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Order Service",
                BaseUrl = "http://localhost:5002",
                Description = "Processes and manages orders",
                RegisteredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Notification Service",
                BaseUrl = "http://localhost:5003",
                Description = "Sends notifications via email and webhooks",
                RegisteredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow.AddMinutes(-30)
            }
        };
    }

    private static List<HealthCheck> CreateHealthChecks(List<Service> services)
    {
        var healthChecks = new List<HealthCheck>();
        var random = new Random();

        foreach (var service in services)
        {
            for (int i = 0; i < 10; i++)
            {
                healthChecks.Add(new HealthCheck
                {
                    ServiceId = service.ServiceId,
                    Status = i < 8 ? "Healthy" : (i == 8 ? "Degraded" : "Unhealthy"),
                    ResponseTimeMs = random.Next(50, 500),
                    CheckedAt = DateTime.UtcNow.AddMinutes(-i * 5),
                    Details = JsonDocument.Parse(
                        $"{{\"cpu\":{random.Next(10, 80)},\"memory\":{random.Next(40, 90)}}}")
                });
            }
        }

        return healthChecks;
    }

    private static List<ServiceDependency> CreateDependencies(List<Service> services)
    {
        if (services.Count < 3) return new List<ServiceDependency>();

        return new List<ServiceDependency>
        {
            new ServiceDependency
            {
                ServiceId = services[1].ServiceId,
                DependsOnServiceId = services[0].ServiceId,
                DiscoveredAt = DateTime.UtcNow
            },
            new ServiceDependency
            {
                ServiceId = services[2].ServiceId,
                DependsOnServiceId = services[1].ServiceId,
                DiscoveredAt = DateTime.UtcNow
            }
        };
    }

    private static List<AlertRule> CreateAlertRules(List<Service> services)
    {
        return new List<AlertRule>
        {
            new AlertRule
            {
                ServiceId = services[0].ServiceId,
                RuleType = "Alert",
                Threshold = 500,
                LogType = "error",
                NotificationChannel = "email"
            }
        };
    }
}
