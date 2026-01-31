namespace ServicePulseMonitor.Data.Models;

public class User
{
    public Guid UserGuid { get; set; }
    public string? DisplayName { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? AccessLevel { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
