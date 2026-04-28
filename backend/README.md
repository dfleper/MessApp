# Backend (MessApp.Api)

Guía de arranque del backend con el entorno actual del repositorio, sin cambiar versiones ni stack.

## Estado de configuración revisado

- `.gitignore`: ignora artefactos de build (`bin/`, `obj/`), logs y `.env` locales.
- `.dockerignore`: evita enviar `bin/`, `obj/`, `.git`, logs, `.env` y metadatos al contexto de build.
- `Dockerfile`: usa `mcr.microsoft.com/dotnet/sdk:8.0`, expone `5000` y ejecuta `dotnet watch`.
- `docker-compose.yml`: levanta API + PostgreSQL (imagen `postgres:16-alpine`) y define conexión por variable de entorno.

## Requisitos

- Docker
- Docker Compose

> No necesitas instalar .NET ni PostgreSQL en tu máquina para correr este backend en modo desarrollo con Docker.

## Arranque rápido

Desde la carpeta `backend/`:

```bash
docker compose up --build
```

Servicios esperados:

- API: `http://localhost:5000`
- Healthcheck API: `http://localhost:5000/health`
- PostgreSQL: `localhost:5432` (usuario `postgres`, password `postgres`, DB `mensajes`)

## Comandos útiles

Levantar en segundo plano:

```bash
docker compose up --build -d
```

Ver logs del backend:

```bash
docker compose logs -f backend
```

Ver logs de base de datos:

```bash
docker compose logs -f db
```

Parar servicios:

```bash
docker compose down
```

Parar y borrar volúmenes:

```bash
docker compose down -v
```


## ¿El código está dentro del contenedor?

Sí. El código del backend se copia dentro de la imagen mediante `COPY . .` en el `Dockerfile` y se ejecuta desde `/app` dentro del contenedor.

El servicio ya no monta `.:/app`, así que el contenedor no depende del código del host para arrancar.

## Notas de desarrollo

- El backend se ejecuta con el código empaquetado en la imagen del contenedor.
- Si modificas código, reconstruye imagen con `docker compose up --build` para aplicar cambios.