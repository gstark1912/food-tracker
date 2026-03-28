# Referencia de API REST

Base URL: `http://localhost:8080` (local) o la URL pública del servicio en Railway.

Todos los bodies son JSON. Todos los timestamps son UTC.

---

## Health

### GET /health

Verifica que el servicio está corriendo.

**Response 200**
```json
{ "status": "healthy" }
```

---

## Tracker

### GET /api/tracker/next-pending

Retorna el próximo día pendiente de completar. Si hay días pasados no finalizados, retorna el más antiguo. Si no, retorna el día actual.

Como efecto secundario, calcula y persiste el `WeeklySummary` de la semana ISO actual si todavía no existe (cálculo lazy).

**Response 200**
```json
{
  "date": "2025-03-24",
  "isCurrentDay": true,
  "isFinalized": false,
  "moments": [
    { "moment": "Mañana",   "food": 0, "exercise": 0 },
    { "moment": "Mediodía", "food": 3, "exercise": 1 },
    { "moment": "Tarde",    "food": 0, "exercise": 0 },
    { "moment": "Noche",    "food": 0, "exercise": 0 }
  ]
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `date` | `string` | Fecha en formato `yyyy-MM-dd` |
| `isCurrentDay` | `boolean` | `true` si la fecha es hoy |
| `isFinalized` | `boolean` | `true` si el día ya fue finalizado |
| `moments` | `array` | Exactamente 4 momentos del día |

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | OK |

---

### GET /api/tracker/day/{date}

Retorna un día específico por fecha.

**Path params**
| Param | Formato | Ejemplo |
|-------|---------|---------|
| `date` | `yyyy-MM-dd` | `2025-03-20` |

**Response 200**
```json
{
  "date": "2025-03-20",
  "isCurrentDay": false,
  "isFinalized": true,
  "moments": [
    { "moment": "Mañana",   "food": 5, "exercise": 2 },
    { "moment": "Mediodía", "food": 8, "exercise": 3 },
    { "moment": "Tarde",    "food": 3, "exercise": 1 },
    { "moment": "Noche",    "food": 2, "exercise": 0 }
  ]
}
```

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | OK |
| 400 | Formato de fecha inválido |
| 404 | No existe registro para esa fecha |

**Response 400**
```json
{ "error": "Formato de fecha inválido: 2025-3-5. Use yyyy-MM-dd" }
```

**Response 404**
```json
{ "error": "No existe registro para la fecha 2025-03-20" }
```

---

### PUT /api/tracker/day/{date}

Crea o actualiza el `DayEntry` del día actual. Solo se puede editar el día de hoy — intentar editar otra fecha retorna 400.

**Path params**
| Param | Formato | Ejemplo |
|-------|---------|---------|
| `date` | `yyyy-MM-dd` | `2025-03-24` |

**Request body**
```json
{
  "moments": [
    { "moment": "Mañana",   "food": 5, "exercise": 2 },
    { "moment": "Mediodía", "food": 8, "exercise": 3 },
    { "moment": "Tarde",    "food": 3, "exercise": 1 },
    { "moment": "Noche",    "food": 2, "exercise": 0 }
  ]
}
```

Se requieren exactamente 4 momentos. Los valores de `food` y `exercise` deben pertenecer a la secuencia Fibonacci: `0, 1, 2, 3, 5, 8, 13`.

**Response 200**

Body vacío (status 200 OK).

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | Guardado correctamente |
| 400 | Formato de fecha inválido |
| 400 | La fecha no es el día actual |
| 400 | No se enviaron exactamente 4 momentos |
| 400 | Valor de `food` o `exercise` fuera de la secuencia Fibonacci |
| 400 | El día ya fue finalizado |

**Response 400 (valor inválido)**
```json
{ "error": "El valor 4 no es válido para Comida en Mañana. Valores permitidos: 0, 1, 2, 3, 5, 8, 13" }
```

**Response 400 (día finalizado)**
```json
{ "error": "El día 2025-03-24 ya fue finalizado y no puede modificarse" }
```

---

### POST /api/tracker/day/{date}/finalize

Finaliza un día. Una vez finalizado, el día es inmutable. Retorna la fecha del siguiente día pendiente.

**Path params**
| Param | Formato | Ejemplo |
|-------|---------|---------|
| `date` | `yyyy-MM-dd` | `2025-03-24` |

**Request body**

Sin body.

**Response 200**
```json
{ "nextPendingDate": "2025-03-25" }
```

`nextPendingDate` es el día pendiente más antiguo, o el día actual si no hay pendientes.

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | Finalizado correctamente |
| 400 | Formato de fecha inválido |
| 400 | El día ya estaba finalizado |
| 404 | No existe registro para esa fecha |

**Response 400 (ya finalizado)**
```json
{ "error": "El día 2025-03-24 ya fue finalizado" }
```

---

## Weekly

### GET /api/weekly

Lista todos los `WeeklySummary` ordenados por semana descendente (más reciente primero).

**Response 200**
```json
[
  {
    "year": 2025,
    "weekNumber": 12,
    "weeklyScore": 47,
    "weightKg": 82.5,
    "weekStart": "2025-03-17",
    "weekEnd": "2025-03-23"
  },
  {
    "year": 2025,
    "weekNumber": 11,
    "weeklyScore": 53,
    "weightKg": null,
    "weekStart": "2025-03-10",
    "weekEnd": "2025-03-16"
  }
]
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `year` | `int` | Año ISO 8601 |
| `weekNumber` | `int` | Número de semana ISO (1–53) |
| `weeklyScore` | `int` | `sum(food) - sum(exercise)` de la semana |
| `weightKg` | `decimal \| null` | Peso corporal registrado, o `null` si no se registró |
| `weekStart` | `string` | Lunes de la semana (`yyyy-MM-dd`) |
| `weekEnd` | `string` | Domingo de la semana (`yyyy-MM-dd`) |

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | OK (puede ser array vacío) |

---

### GET /api/weekly/{year}/{week}

Retorna un `WeeklySummary` específico.

**Path params**
| Param | Tipo | Ejemplo |
|-------|------|---------|
| `year` | `int` | `2025` |
| `week` | `int` | `12` |

**Response 200**
```json
{
  "year": 2025,
  "weekNumber": 12,
  "weeklyScore": 47,
  "weightKg": 82.5,
  "weekStart": "2025-03-17",
  "weekEnd": "2025-03-23"
}
```

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | OK |
| 404 | No existe resumen para esa semana |

**Response 404**
```json
{ "error": "No existe resumen para la semana 2025-W12" }
```

---

### POST /api/weekly/{year}/{week}/weight

Registra el peso corporal en un `WeeklySummary`. El peso es inmutable: una vez registrado no puede modificarse.

**Path params**
| Param | Tipo | Ejemplo |
|-------|------|---------|
| `year` | `int` | `2025` |
| `week` | `int` | `12` |

**Request body**
```json
{ "weightKg": 82.5 }
```

**Response 200**

Retorna el `WeeklySummary` actualizado con el mismo formato que `GET /api/weekly/{year}/{week}`.

```json
{
  "year": 2025,
  "weekNumber": 12,
  "weeklyScore": 47,
  "weightKg": 82.5,
  "weekStart": "2025-03-17",
  "weekEnd": "2025-03-23"
}
```

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | Peso registrado correctamente |
| 400 | El peso ya fue registrado para esa semana |
| 404 | No existe resumen para esa semana |

**Response 400**
```json
{ "error": "El peso ya fue registrado para la semana 2025-W12" }
```

---

## Statistics

### GET /api/statistics

Retorna los últimos N `WeeklySummary` con tendencia calculada respecto a la semana anterior. Ordenados de más reciente a más antiguo.

**Query params**
| Param | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `n` | `int` | `10` | Cantidad de semanas a retornar |

**Ejemplo:** `GET /api/statistics?n=5`

**Response 200**
```json
{
  "summaries": [
    {
      "summary": {
        "year": 2025,
        "weekNumber": 12,
        "weeklyScore": 47,
        "weightKg": 82.5,
        "weekStart": "2025-03-17",
        "weekEnd": "2025-03-23"
      },
      "trend": "down"
    },
    {
      "summary": {
        "year": 2025,
        "weekNumber": 11,
        "weeklyScore": 53,
        "weightKg": null,
        "weekStart": "2025-03-10",
        "weekEnd": "2025-03-16"
      },
      "trend": "up"
    },
    {
      "summary": {
        "year": 2025,
        "weekNumber": 10,
        "weeklyScore": 41,
        "weightKg": 83.0,
        "weekStart": "2025-03-03",
        "weekEnd": "2025-03-09"
      },
      "trend": null
    }
  ]
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `summaries` | `array` | Lista de semanas con tendencia |
| `summary` | `object` | Datos del `WeeklySummary` |
| `trend` | `"up" \| "down" \| null` | `"down"` si el score bajó respecto a la semana anterior (mejor), `"up"` si subió (peor), `null` para la semana más antigua del conjunto |

**Nota sobre la tendencia:** un score más bajo es mejor (menos comida, más ejercicio). `"down"` se muestra en verde en la UI.

**Códigos HTTP**
| Código | Situación |
|--------|-----------|
| 200 | OK (puede retornar array vacío si no hay datos) |

---

## Errores comunes

| HTTP | Situación | Ejemplo de mensaje |
|------|-----------|--------------------|
| 400 | Valor Fibonacci inválido | `"El valor 4 no es válido para Comida en Mañana. Valores permitidos: 0, 1, 2, 3, 5, 8, 13"` |
| 400 | Intento de modificar día finalizado | `"El día 2025-03-24 ya fue finalizado y no puede modificarse"` |
| 400 | Intento de registrar peso ya existente | `"El peso ya fue registrado para la semana 2025-W12"` |
| 400 | Fecha con formato incorrecto | `"Formato de fecha inválido: 2025-3-5. Use yyyy-MM-dd"` |
| 404 | DayEntry no encontrado | `"No existe registro para la fecha 2025-03-20"` |
| 404 | WeeklySummary no encontrado | `"No existe resumen para la semana 2025-W12"` |
| 500 | Error interno | `"Error interno al guardar los datos"` |

Todos los errores tienen el formato:
```json
{ "error": "Descripción del error" }
```
