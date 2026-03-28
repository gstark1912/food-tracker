# 🥗 Food Tracker

Herramienta personal para trackear hábitos alimenticios y de ejercicio día a día. Cada día se divide en 4 momentos (Mañana, Mediodía, Tarde, Noche) y se registran valores de comida y ejercicio usando la secuencia de Fibonacci. Al final de cada semana se calcula un score automáticamente.

## Stack

- **Backend**: .NET 8 Minimal API + Entity Framework Core + PostgreSQL
- **Frontend**: Vue 3 + Vite + Pinia
- **Deploy**: Railway (dos servicios separados)

## Correr localmente

### Requisitos
- Docker Desktop
- Node.js 18+
- .NET 8 SDK

### 1. Levantar PostgreSQL

```bash
docker-compose up -d postgres
```

### 2. Levantar la API

```bash
cd api
dotnet run --project src/App.Api/App.Api.csproj
```

La API queda en `http://localhost:5000`. Las migraciones se aplican automáticamente al iniciar.

### 3. Levantar el frontend

```bash
cd web
npm install
npm run dev
```

El frontend queda en `http://localhost:5173`.

## Deploy en Railway

Ver [docs/architecture.md](docs/architecture.md) para instrucciones detalladas.

En resumen: crear dos servicios en Railway apuntando a `api/` y `web/`, agregar un servicio PostgreSQL, y configurar las variables de entorno:

| Servicio | Variable | Valor |
|----------|----------|-------|
| API | `ConnectionStrings__DefaultConnection` | URL de PostgreSQL |
| API | `Cors__AllowedOrigins` | URL pública del frontend |
| API | `App__TimeZoneId` | `America/Argentina/Buenos_Aires` |
| Web | `VITE_API_BASE_URL` | URL pública de la API |

## Documentación

- [docs/architecture.md](docs/architecture.md) — stack, estructura y patrones
- [docs/api.md](docs/api.md) — referencia de endpoints REST
- [docs/schema.md](docs/schema.md) — schema de base de datos
