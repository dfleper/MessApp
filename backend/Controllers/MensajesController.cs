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
        if (!IsAdminAuthorized(out var unauthorizedResult))
        {
            return unauthorizedResult;
        }

        var safeLimit = Math.Clamp(limit ?? DefaultAdminLimit, 1, MaxAdminLimit);
        var mensajes = await _repository.GetLatestAsync(safeLimit, cancellationToken);

        return Ok(mensajes);
    }

    [HttpPatch("admin/{id:long}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] long id, CancellationToken cancellationToken)
    {
        if (!IsAdminAuthorized(out var unauthorizedResult))
        {
            return unauthorizedResult;
        }

        var updated = await _repository.MarkAsReadAsync(id, cancellationToken);
        return updated ? NoContent() : NotFound(new { error = "Mensaje no encontrado" });
    }

    [HttpDelete("admin/{id:long}")]
    public async Task<IActionResult> SoftDelete([FromRoute] long id, CancellationToken cancellationToken)
    {
        if (!IsAdminAuthorized(out var unauthorizedResult))
        {
            return unauthorizedResult;
        }

        var deleted = await _repository.SoftDeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(new { error = "Mensaje no encontrado" });
    }

    [HttpDelete("admin/purge")]
    public async Task<IActionResult> PurgeAll(CancellationToken cancellationToken)
    {
        if (!IsAdminAuthorized(out var unauthorizedResult))
        {
            return unauthorizedResult;
        }

        var removedRows = await _repository.PurgeAllAsync(cancellationToken);
        return Ok(new { deleted = removedRows });
    }

    private bool IsAdminAuthorized(out IActionResult? unauthorizedResult)
    {
        if (string.IsNullOrWhiteSpace(_adminApiKey))
        {
            unauthorizedResult = StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Admin access not configured" });
            return false;
        }

        var providedKey = Request.Headers["X-Admin-Key"].ToString();
        if (!IsValidAdminKey(providedKey, _adminApiKey))
        {
            unauthorizedResult = Unauthorized(new { error = "Invalid admin credentials" });
            return false;
        }

        unauthorizedResult = null;
        return true;
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