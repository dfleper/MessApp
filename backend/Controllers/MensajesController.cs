using System.Text.RegularExpressions;
using MessApp.Api.Models;
using MessApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessApp.Api.Controllers;

[ApiController]
[Route("api/mensajes")]
public sealed class MensajesController : ControllerBase
{
    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", RegexOptions.Compiled);
    private readonly MensajesRepository _repository;

    public MensajesController(MensajesRepository repository)
    {
        _repository = repository;
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
}