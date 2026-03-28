namespace ServicePulseMonitor.Data.Models;

/// <summary>Represents a user account in the monitoring system.</summary>
public class User
{
    /// <summary>Unique identifier (GUID) of the user.</summary>
    public Guid UserGuid { get; set; }

    /// <summary>Optional display name shown in the dashboard.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Unique login username.</summary>
    public string Username { get; set; } = null!;

    /// <summary>BCrypt hash of the user's password.</summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>Access level or role of the user, or <c>null</c> if not assigned.</summary>
    public string? AccessLevel { get; set; }

    /// <summary>UTC timestamp when the user account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Alerts associated with this user.</summary>
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
