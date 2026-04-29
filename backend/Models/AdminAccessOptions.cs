namespace MessApp.Api.Models;

public sealed class AdminAccessOptions
{
    public const string SectionName = "AdminAccess";

    public string ApiKey { get; init; } = string.Empty;
}