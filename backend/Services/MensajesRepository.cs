using System.Net;
using MessApp.Api.Models;
using Npgsql;

namespace MessApp.Api.Services;

public sealed class MensajesRepository
{
    private readonly string _connectionString;

    public MensajesRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseUrl = configuration["DATABASE_URL"];

        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : ConvertDatabaseUrl(databaseUrl)
                ?? throw new InvalidOperationException("Missing connection string: DefaultConnection or DATABASE_URL");
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            CREATE TABLE IF NOT EXISTS mensajes (
              id BIGSERIAL PRIMARY KEY,
              nombre VARCHAR(120) NOT NULL,
              email VARCHAR(255) NOT NULL,
              asunto VARCHAR(200) NOT NULL,
              texto TEXT NOT NULL,
              ip_usuario INET NOT NULL,
              created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
              read_at TIMESTAMPTZ NULL,
              updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
              deleted_at TIMESTAMPTZ NULL
            );

            CREATE INDEX IF NOT EXISTS idx_mensajes_ip_created_at
              ON mensajes (ip_usuario, created_at DESC)
              WHERE deleted_at IS NULL;";

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
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

    public async Task<IReadOnlyList<MensajeAdminItem>> GetLatestAsync(int limit, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var hasReadAtColumn = await HasReadAtColumnAsync(connection, cancellationToken);
        var sql = hasReadAtColumn
            ? @"
            SELECT id, nombre, email, asunto, texto, ip_usuario::text, created_at, read_at
            FROM mensajes
            WHERE deleted_at IS NULL
            ORDER BY created_at DESC
            LIMIT @limit;"
            : @"
            SELECT id, nombre, email, asunto, texto, ip_usuario::text, created_at, NULL::timestamptz AS read_at
            FROM mensajes
            WHERE deleted_at IS NULL
            ORDER BY created_at DESC
            LIMIT @limit;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<MensajeAdminItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new MensajeAdminItem(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetDateTime(6),
                reader.IsDBNull(7) ? null : reader.GetDateTime(7)));
        }

        return result;
    }

    public async Task<bool> MarkAsReadAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (!await HasReadAtColumnAsync(connection, cancellationToken))
        {
            await EnsureReadAtColumnIfPossibleAsync(connection, cancellationToken);

            if (!await HasReadAtColumnAsync(connection, cancellationToken))
            {
                return false;
            }
        }

        const string sql = @"
            UPDATE mensajes
            SET read_at = COALESCE(read_at, NOW()),
                updated_at = NOW()
            WHERE id = @id AND deleted_at IS NULL;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE mensajes
            SET deleted_at = NOW(),
                updated_at = NOW()
            WHERE id = @id AND deleted_at IS NULL;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<int> PurgeAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "DELETE FROM mensajes;";
        await using var command = new NpgsqlCommand(sql, connection);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> HasReadAtColumnAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'mensajes'
                  AND column_name = 'read_at'
            );";

        await using var command = new NpgsqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool exists && exists;
    }

    private static async Task EnsureReadAtColumnIfPossibleAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE mensajes
            ADD COLUMN IF NOT EXISTS read_at TIMESTAMPTZ NULL;";

        try
        {
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "42501")
        {
            // Insufficient privilege to alter schema; caller will fallback gracefully.
        }
    }

    private static string? ConvertDatabaseUrl(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
        {
            return databaseUrl;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
            Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty)
        };

        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = Uri.UnescapeDataString(parts.ElementAtOrDefault(1) ?? string.Empty);

            if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode))
            {
                builder.SslMode = sslMode;
            }
        }

        return builder.ConnectionString;
    }

}

public sealed record MensajeAdminItem(
    long Id,
    string Nombre,
    string Email,
    string Asunto,
    string Mensaje,
    string IpUsuario,
    DateTime CreatedAt,
    DateTime? ReadAt);