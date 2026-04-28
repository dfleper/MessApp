# Frontend (Vite)

Guía de arranque del frontend con la configuración actual del repositorio.

## Estado de configuración revisado

- `.gitignore`: ignora `node_modules`, `dist`, cachés y archivos locales de entorno.
- `.dockerignore`: excluye `node_modules`, `dist`, `.git` y logs del contexto de build.
- `Dockerfile`: usa `node:lts-alpine3.23`, instala dependencias con `npm install` y arranca con Vite.
- `docker-compose.yml`: publica el frontend en `5173` y define `VITE_API_URL=http://localhost:5000`.

## Requisitos

- Docker
- Docker Compose

> No necesitas instalar Node.js localmente si ejecutas el frontend dentro de Docker.

## Arranque rápido

Desde la carpeta `frontend/`:

```bash
docker compose up --build
```

Aplicación disponible en:

- `http://localhost:5173`

## Comandos útiles

Levantar en segundo plano:

```bash
docker compose up --build -d
```

Ver logs del frontend:

```bash
docker compose logs -f frontend
```

Parar servicios:

```bash
docker compose down
```

Parar y borrar volumen de dependencias:

```bash
docker compose down -v
```


## ¿El código está dentro del contenedor?

Sí. El código del frontend se copia dentro de la imagen con `COPY . .` en el `Dockerfile` y se ejecuta desde `/app` dentro del contenedor.

El servicio ya no monta `.:/app`, por lo que depende del código empaquetado en la imagen.

## Integración con backend

Este frontend está configurado para consumir API en:

```text
VITE_API_URL=http://localhost:5000
```

Si cambias el host/puerto del backend, actualiza esa variable en `frontend/docker-compose.yml`.