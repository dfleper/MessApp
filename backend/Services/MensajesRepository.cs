
using System.Net;
using MessApp.Api.Models;
using Npgsql;

namespace MessApp.Api.Services;

public sealed class MensajesRepository
{
    private readonly string _connectionString;

    public MensajesRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");
    }

    public async Task<long> CreateAsync(CreateMensajeRequest request, IPAddress? remoteIp, CancellationToken cancellationToken)
    {
        var ip = remoteIp?.ToString() ?? "0.0.0.0";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO mensajes (nombre, email, asunto, texto, ip_usuario)
            VALUES (@nombre, @email, @asunto, @texto, CAST(@ip AS inet))
            RETURNING id;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("nombre", request.Nombre.Trim());
        command.Parameters.AddWithValue("email", request.Email.Trim());
        command.Parameters.AddWithValue("asunto", request.Asunto.Trim());
        command.Parameters.AddWithValue("texto", request.Mensaje.Trim());
        command.Parameters.AddWithValue("ip", ip);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is long id
            ? id
            : throw new InvalidOperationException("Failed to persist mensaje");
    }
}