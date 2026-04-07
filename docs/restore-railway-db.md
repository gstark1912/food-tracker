# Restaurar DB de Railway en Docker local

## Prerequisitos

- Docker corriendo
- Container `food-tracker-postgres` levantado (`docker compose up -d postgres`)

## Pasos

### 1. Dump desde Railway

Usamos un container temporal de Postgres 18 (misma versión que Railway) porque el local es Postgres 16:

```bash
docker run --rm postgres:18-alpine pg_dump "postgresql://postgres:FFMaqTzkEhjfgPioyiofOyQvRDYquHnX@hopper.proxy.rlwy.net:37971/railway" --no-owner --no-acl -F p > backup.sql
```

> Si las credenciales cambiaron, sacá la connection string actualizada del dashboard de Railway → Postgres service → Connect → Public URL.

### 2. Limpiar tablas locales

```bash
docker exec food-tracker-postgres psql -U postgres -d foodtracker -c "TRUNCATE moment_entries, day_entries, weekly_summaries, \"__EFMigrationsHistory\" CASCADE;"
```

### 3. Restaurar en local

```bash
Get-Content backup.sql | docker exec -i food-tracker-postgres psql -U postgres -d foodtracker
```

> Los errores de "relation already exists" y "multiple primary keys" son normales — las tablas ya existen, solo se importan los datos.

### 4. Verificar

```bash
docker exec food-tracker-postgres psql -U postgres -d foodtracker -c "SELECT 'day_entries' as tabla, count(*) FROM day_entries UNION ALL SELECT 'moment_entries', count(*) FROM moment_entries UNION ALL SELECT 'weekly_summaries', count(*) FROM weekly_summaries;"
```

### 5. Limpiar

```bash
del backup.sql
```

## Notas

- Los comandos del paso 3 son para PowerShell. En bash usarías `cat backup.sql | docker exec ...` o redirección con `<`.
- Si Railway actualiza la versión de Postgres, actualizá el tag de la imagen en el paso 1 (ej: `postgres:19-alpine`).
