# Frontend (Vite)

Guía actualizada para ejecutar y usar el frontend de **MessApp** con la configuración real del repositorio.

## Stack y versiones (sin cambios)

- Vite `8.0.10`
- Tailwind CSS `4.2.4`
- Plugin `@tailwindcss/vite` `4.2.4`
- Node base image: `node:lts-alpine3.23`

## Estructura funcional

- `index.html` + `src/main.js`: formulario público para crear mensajes.
- `admin.html` + `src/admin.js`: panel admin para listar, marcar leído, soft delete y purge.
- `src/style.css`: estilos del frontend.

## Variables de entorno

- `VITE_API_URL` (default en código: `http://localhost:5000`)

En Docker Compose ya viene definida como:

```yaml
VITE_API_URL=http://localhost:5000
```

## Desarrollo con Docker (recomendado)

Desde `frontend/`:

```bash
docker compose up --build
```

Disponible en:

- `http://localhost:5173`
- Admin UI: `http://localhost:5173/admin.html`

### Cómo está montado el código

El servicio **sí monta el código local**:

- `.:/app` para hot reload en desarrollo.
- `frontend_node_modules:/app/node_modules` para persistir dependencias del contenedor.

## Desarrollo local (sin Docker)

Requisitos:

- Node.js con npm

Comandos:

```bash
npm install
npm run dev -- --host 0.0.0.0
```

## Scripts disponibles

```bash
npm run dev
npm run build
npm run preview -- --host 0.0.0.0
```

## Flujo del formulario público

`src/main.js` valida antes de enviar:

- Nombre: 2-60, solo letras y espacios.
- Email: 5-100, formato email.
- Asunto: 2-80.
- Mensaje: 10-250.

Luego hace `POST` a:

- `POST {VITE_API_URL}/api/mensajes`

## Flujo del panel admin

`src/admin.js` exige API key y la manda en header:

- `X-Admin-Key: <api-key>`

Acciones:

- Cargar mensajes: `GET /api/mensajes/admin?limit=100`
- Marcar leído: `PATCH /api/mensajes/admin/{id}/read`
- Soft delete: `DELETE /api/mensajes/admin/{id}`
- Purge total: `DELETE /api/mensajes/admin/purge`

## Comandos útiles Docker

```bash
docker compose up --build -d
docker compose logs -f frontend
docker compose down
docker compose down -v
```

## Integración con backend

Este frontend espera backend en `http://localhost:5000` (salud en `/health`).

Si cambias host/puerto del backend, actualiza `VITE_API_URL` en `frontend/docker-compose.yml`.