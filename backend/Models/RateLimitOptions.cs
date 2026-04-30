namespace MessApp.Api.Models;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";
    public int SecondsBetweenMessages { get; set; } = 900; // Change in MensajesController.cs, appsettings.json and RateLimitOptions.cs
}