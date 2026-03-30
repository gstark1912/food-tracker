# Arquitectura del proyecto

## Stack tecnológico

### Backend
| Componente | Tecnología |
|------------|------------|
| Runtime | .NET 8 |
| Framework | ASP.NET Core Minimal API |
| ORM | Entity Framework Core 8 |
| Base de datos | PostgreSQL 16 |
| Driver PostgreSQL | Npgsql.EntityFrameworkCore.PostgreSQL 8.0 |
| Documentación API | Swashbuckle (Swagger) |
| Tests | xUnit + FsCheck (property-based) |

### Frontend
| Componente | Tecnología |
|------------|------------|
| Framework | Vue 3 (Composition API) |
| Build tool | Vite 5 |
| Estado global | Pinia 2 |
| Router | Vue Router 4 |
| Gráficos | Chart.js 4 + vue-chartjs 5 |
| Tests | Vitest + @vue/test-utils |
| Servidor estático | nginx (en producción) |

---

## Estructura de carpetas

```
food-tracker/
├── api/                              # Backend .NET 8
│   ├── src/
│   │   ├── App.Api/
│   │   │   ├── Data/
│   │   │   │   ├── AppDbContext.cs   # DbContext con las 3 tablas
│   │   │   │   └── Migrations/       # Migraciones EF Core
│   │   │   ├── Endpoints/
│   │   │   │   ├── TrackerEndpoints.cs    # /api/tracker/*
│   │   │   │   ├── WeeklyEndpoints.cs     # /api/weekly/*
│   │   │   │   └── StatisticsEndpoints.cs # /api/statistics
│   │   │   ├── Models/
│   │   │   │   ├── DayEntry.cs
│   │   │   │   ├── MomentEntry.cs
│   │   │   │   ├── WeeklySummary.cs
│   │   │   │   ├── Fibonacci.cs      # Lógica de secuencia Fibonacci
│   │   │   │   └── DTOs.cs           # Records de request/response
│   │   │   ├── Program.cs
│   │   │   └── App.Api.csproj
│   │   └── App.Api.Tests/
│   │       └── ...                   # Tests xUnit + FsCheck
│   ├── Dockerfile
│   ├── railway.toml
│   └── FoodTracker.sln
│
├── web/                              # Frontend Vue 3
│   ├── src/
│   │   ├── views/
│   │   │   ├── TrackerView.vue       # Vista principal de carga diaria
│   │   │   └── StatisticsView.vue    # Vista de estadísticas y gráfico
│   │   ├── components/
│   │   │   ├── DayCard.vue           # Contenedor del día con botones de acción
│   │   │   ├── MomentRow.vue         # Fila de un momento del día
│   │   │   ├── FibonacciControl.vue  # Botones +/- con lógica Fibonacci
│   │   │   ├── WeekSummaryCard.vue   # Tarjeta de resumen semanal
│   │   │   ├── TrendBadge.vue        # Badge de tendencia (verde/rojo)
│   │   │   └── ScoreChart.vue        # Gráfico score vs semana
│   │   ├── stores/
│   │   │   ├── trackerStore.js       # Estado del día actual + llamadas API
│   │   │   └── statsStore.js         # Estado de estadísticas y WeeklySummaries
│   │   ├── router/
│   │   │   └── index.js              # / → TrackerView, /stats → StatisticsView
│   │   ├── App.vue
│   │   └── main.js
│   ├── Dockerfile
│   ├── nginx.conf.template          # Template nginx (envsubst reemplaza $PORT y $VITE_API_BASE_URL)
│   ├── index.html
│   └── package.json
│
├── docs/                             # Documentación de referencia
│   ├── schema.md                     # Diagrama ER y descripción de tablas
│   ├── architecture.md               # Este archivo
│   └── api.md                        # Referencia de endpoints REST
│
└── docker-compose.yml                # Entorno de desarrollo local
```

---

## Patrones de diseño

### Minimal API endpoints por dominio
El backend organiza los endpoints en clases estáticas por dominio (`TrackerEndpoints`, `WeeklyEndpoints`, `StatisticsEndpoints`). Cada clase expone un método de extensión `Map*Endpoints(this WebApplication app)` que se llama desde `Program.cs`. Esto evita controllers y mantiene el código cohesivo por feature.

```csharp
// Program.cs
app.MapTrackerEndpoints();
app.MapWeeklyEndpoints();
app.MapStatisticsEndpoints();
```

### Pinia stores
El frontend usa un store por dominio funcional. `trackerStore` maneja el estado del día actual y las llamadas a `/api/tracker/*`. `statsStore` maneja las estadísticas y los WeeklySummaries. Los stores son la única fuente de verdad — los componentes no llaman a la API directamente.

### Cálculo semanal lazy
El `WeeklySummary` de la semana ISO actual se calcula la primera vez que el usuario accede a la app (en `GET /api/tracker/next-pending`). No hay jobs en background ni cron. Si el resumen ya existe, no se recalcula. Esto simplifica el deploy y evita dependencias de infraestructura adicionales.

### Fibonacci con tope en 13
Los valores de comida y ejercicio siguen la secuencia `[0, 1, 2, 3, 5, 8, 13]`. El botón "+" en 13 no hace nada; el botón "-" en 0 no hace nada. La lógica vive en `Fibonacci.cs` (backend, para validación) y en `trackerStore.js` (frontend, para la UI). El backend valida que los valores recibidos pertenezcan a la secuencia antes de persistir.

---

## Correr localmente con docker-compose

El `docker-compose.yml` levanta PostgreSQL y la API. El frontend se corre por separado con Vite.

```bash
# 1. Levantar PostgreSQL + API
docker-compose up -d

# 2. Levantar el frontend (en otra terminal)
cd web
npm install
npm run dev
```

La API queda disponible en `http://localhost:8080` y el frontend en `http://localhost:5173`.

Las migraciones de EF Core se aplican automáticamente al iniciar la API (`db.Database.Migrate()` en `Program.cs`).

Variables de entorno configuradas en `docker-compose.yml`:
- `ConnectionStrings__DefaultConnection` — cadena de conexión a PostgreSQL
- `Cors__AllowedOrigins` — orígenes permitidos para CORS (por defecto `http://localhost:5173`)
- `ASPNETCORE_ENVIRONMENT` — `Development` (habilita Swagger en `/swagger`)

---

## Deploy en Railway

El proyecto se despliega como dos servicios separados en Railway: uno para la API y otro para el frontend.

### Servicio 1: API (.NET)

1. Crear un nuevo servicio en Railway apuntando al directorio `api/`.
2. Railway detecta el `Dockerfile` automáticamente.
3. Agregar un servicio de PostgreSQL en el mismo proyecto de Railway.
4. Configurar las variables de entorno del servicio API:
   - `ConnectionStrings__DefaultConnection` — usar la variable `${{Postgres.DATABASE_URL}}` de Railway o construirla manualmente: `Host=...;Database=...;Username=...;Password=...`
   - `Cors__AllowedOrigins` — URL pública del frontend (ej: `https://food-tracker-web.up.railway.app`)
5. El `railway.toml` configura el health check en `/health`.

```toml
# api/railway.toml
[build]
dockerfilePath = "Dockerfile"

[deploy]
healthcheckPath = "/health"
```

### Servicio 2: Frontend (Vue + nginx)

1. Crear un segundo servicio apuntando al directorio `web/`.
2. Railway detecta el `Dockerfile` automáticamente.
3. Configurar la variable de entorno:
   - `VITE_API_BASE_URL` — URL pública de la API (ej: `https://food-tracker-api.up.railway.app`). Se usa en dos momentos:
     - **Build time** (como `ARG`): Vite la embebe en el bundle del frontend.
     - **Runtime** (como `ENV`): nginx la usa para hacer proxy reverso de `/api/*` hacia la API.
4. Railway asigna automáticamente la variable `PORT` en runtime. El `Dockerfile` usa `envsubst` para inyectar `$PORT` y `$VITE_API_BASE_URL` en el template de nginx antes de arrancar.
5. El archivo `nginx.conf.template` contiene placeholders `${PORT}` y `${VITE_API_BASE_URL}` que se resuelven en runtime. Las variables propias de nginx (`$uri`, `$host`, etc.) no se tocan porque `envsubst` recibe explícitamente solo las variables a reemplazar.

### Orden de deploy recomendado

1. Crear el servicio de PostgreSQL.
2. Deployar la API (aplica migraciones al arrancar).
3. Deployar el frontend con la URL de la API ya conocida.
