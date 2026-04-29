using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using MessApp.Api.Models;
using MessApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessApp.Api.Controllers;

[ApiController]
[Route("api/mensajes")]
public sealed class MensajesController : ControllerBase
{
    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", RegexOptions.Compiled);
    private const int DefaultAdminLimit = 20;
    private const int MaxAdminLimit = 100;
    private readonly MensajesRepository _repository;
    private readonly string _adminApiKey;

    public MensajesController(MensajesRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _adminApiKey = configuration.GetSection(AdminAccessOptions.SectionName)
            .Get<AdminAccessOptions>()?.ApiKey?.Trim() ?? string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMensajeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!NombreRegex.IsMatch(request.Nombre.Trim()))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Nombre)] = ["El nombre solo puede contener letras y espacios"]
            }));
        }

        var id = await _repository.CreateAsync(request, HttpContext.Connection.RemoteIpAddress, cancellationToken);

        return Created($"/api/mensajes/{id}", new { id });
    }

    [HttpGet("/health")]
    public IActionResult Health() => Ok(new { status = "ok" });

    [HttpGet("admin")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetForAdmin([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_adminApiKey))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Admin access not configured" });
        }

        var providedKey = Request.Headers["X-Admin-Key"].ToString();
        if (!IsValidAdminKey(providedKey, _adminApiKey))
        {
            return Unauthorized(new { error = "Invalid admin credentials" });
        }

        var safeLimit = Math.Clamp(limit ?? DefaultAdminLimit, 1, MaxAdminLimit);
        var mensajes = await _repository.GetLatestAsync(safeLimit, cancellationToken);

        return Ok(mensajes);
    }

    private static bool IsValidAdminKey(string providedKey, string configuredKey)
    {
        if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(configuredKey))
        {
            return false;
        }

        var providedBytes = Encoding.UTF8.GetBytes(providedKey);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);

        return providedBytes.Length == configuredBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
    }
}