using System.ComponentModel.DataAnnotations;

namespace MessApp.Api.Models;

public sealed class CreateMensajeRequest
{
    public const int MinNameLength = 2;
    public const int MaxNameLength = 60;
    public const int MinEmailLength = 5;
    public const int MaxEmailLength = 100;
    public const int MinSubjectLength = 2;
    public const int MaxSubjectLength = 80;
    public const int MinMessageLength = 10;
    public const int MaxMessageLength = 250;

    [Required]
    [StringLength(MaxNameLength, MinimumLength = MinNameLength)]
    public string Nombre { get; init; } = string.Empty;

    [Required]
    [StringLength(MaxEmailLength, MinimumLength = MinEmailLength)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(MaxSubjectLength, MinimumLength = MinSubjectLength)]
    public string Asunto { get; init; } = string.Empty;

    [Required]
    [StringLength(MaxMessageLength, MinimumLength = MinMessageLength)]
    public string Mensaje { get; init; } = string.Empty;
}