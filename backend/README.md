# Backend (MessApp.Api)

Guía actualizada para ejecutar el backend con la configuración vigente del repositorio.

## Stack y versiones (sin cambios)

- .NET SDK image: `mcr.microsoft.com/dotnet/sdk:8.0`
- ASP.NET Core Web API
- PostgreSQL image: `postgres:16-alpine`
- Provider DB: `Npgsql`

## Estructura funcional

- `Program.cs`: configuración de servicios, CORS, swagger y pipeline.
- `Controllers/MensajesController.cs`: endpoints públicos + endpoints admin protegidos.
- `Services/MensajesRepository.cs`: acceso a PostgreSQL con SQL parametrizado.
- `db/init.sql`: inicialización de la tabla `mensajes`.

## Configuración clave

Variables usadas en `docker-compose.yml`:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://0.0.0.0:5000`
- `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=mensajes;Username=postgres;Password=postgres`
- `AdminAccess__ApiKey=${ADMIN_API_KEY}`

Significado de `${ADMIN_API_KEY}`:
- Debes definir `ADMIN_API_KEY` en tu entorno antes de levantar el backend.
- Si no se define, los endpoints admin quedarán deshabilitados (`503`) por seguridad.

## Desarrollo con Docker (recomendado)

Desde `backend/`:

```bash
docker compose up --build
```

Servicios:

- API: `http://localhost:5000`
- Health: `http://localhost:5000/health`
- PostgreSQL: `localhost:5432` (`postgres/postgres`, DB `mensajes`)

### Cómo está montado el código

El servicio **sí monta el código local**:

- `.:/app` para desarrollo en vivo.
- `nuget_cache:/root/.nuget/packages` para cachear paquetes.

La base de datos usa:

- `postgres_data:/var/lib/postgresql/data`
- `./db/init.sql:/docker-entrypoint-initdb.d/init.sql:ro`

## Endpoints

### Público

- `POST /api/mensajes`
- `GET /health`

`POST /api/mensajes` recibe:

```json
{
  "nombre": "string",
  "email": "string",
  "asunto": "string",
  "mensaje": "string"
}
```

Validaciones:

- `nombre`: 2-60 y regex de letras/espacios.
- `email`: 5-100 y formato email.
- `asunto`: 2-80.
- `mensaje`: 10-250.

### Admin (requiere `X-Admin-Key`)

- `GET /api/mensajes/admin?limit={n}` (limit clamp 1..100, default 20)
- `PATCH /api/mensajes/admin/{id}/read`
- `DELETE /api/mensajes/admin/{id}` (soft delete)
- `DELETE /api/mensajes/admin/purge` (hard delete total)

Si la clave no es válida: `401`.
Si no hay clave configurada: `503`.

## Seguridad y comportamiento

- Comparación de API key en tiempo constante (`FixedTimeEquals`).
- Consultas SQL parametrizadas en todas las operaciones.
- CORS restringido a `http://localhost:5173`.
- Endpoints admin con `ResponseCache` deshabilitado (`NoStore`).

## Comandos útiles Docker

```bash
docker compose up --build -d
docker compose logs -f backend
docker compose logs -f db
docker compose down
docker compose down -v
```

## Prueba rápida manual

Con backend y db arriba:

```bash
curl -i http://localhost:5000/health
```

Esperado: `200` y `{"status":"ok"}`.