# SargazoAI Backend

Backend API desarrollado en ASP.NET Core para predicción de trayectorias y biomasa de sargazo en el Caribe Mexicano.

## 🌊 Descripción

Este proyecto integra múltiples modelos de Machine Learning para:

1. **Predecir trayectorias** de sargazo usando coordenadas históricas
2. **Obtener datos oceánicos** en tiempo real (temperatura, corrientes)
3. **Predecir biomasa** de sargazo basado en condiciones oceánicas

## 🚀 Tecnologías

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Web API
- **C#** - Lenguaje de programación
- **Swagger/OpenAPI** - Documentación de API
- **HttpClient** - Consumo de microservicios externos

## 📋 Requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- Microservicios Python corriendo en:
  - `http://127.0.0.1:8000/predict-coordinate` (predicción de coordenadas)
  - `http://127.0.0.1:8000/predict-biomass` (predicción de biomasa)
- Conexión a Internet (para API de Open-Meteo)

## 🔧 Instalación

1. **Clonar el repositorio:**

```bash
git clone https://github.com/RaulBecerraB/SargazoAI-Backend.git
cd SargazoAI-Backend
```

2. **Restaurar dependencias:**

```bash
dotnet restore
```

3. **Compilar el proyecto:**

```bash
dotnet build
```

4. **Ejecutar la aplicación:**

```bash
dotnet run
```

La API estará disponible en:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

## 📡 Endpoints

### GET /api/coordinate/predict

Predice trayectorias futuras y biomasa de sargazo.

**Query Parameters:**

- `iterations` (int, opcional): Número de predicciones futuras (default: 1)

**Ejemplo de request:**

```http
GET http://localhost:5000/api/coordinate/predict?iterations=5
```

**Ejemplo de response:**

```json
{
  "predictedCoordinates": [
    {
      "latitude": 21.290123,
      "longitude": -89.652456,
      "sargassumBiomass": 27.89
    },
    {
      "latitude": 21.289987,
      "longitude": -89.653234,
      "sargassumBiomass": 28.12
    }
  ],
  "iterationsCount": 5
}
```

**Campos de respuesta:**

- `latitude`: Latitud predicha (grados decimales)
- `longitude`: Longitud predicha (grados decimales)
- `sargassumBiomass`: Biomasa de sargazo predicha (kg/m²)
- `iterationsCount`: Número total de iteraciones realizadas

## 🏗️ Arquitectura

```
┌─────────────────┐
│   ASP.NET Core  │
│   Web API       │
└────────┬────────┘
         │
         ├─────────────────────────────┐
         │                             │
         ▼                             ▼
┌─────────────────┐         ┌──────────────────┐
│  Microservicio  │         │   Open-Meteo     │
│  Python ML      │         │   Marine API     │
│  (Coordenadas)  │         │  (Datos Océano)  │
└─────────────────┘         └──────────────────┘
         │
         ▼
┌─────────────────┐
│  Microservicio  │
│  Python ML      │
│  (Biomasa)      │
└─────────────────┘
```

## 🔄 Flujo de Predicción

1. **Entrada inicial**: 4 coordenadas semilla (hardcoded en el endpoint)
2. **Iteración por cada predicción**:
   - Envía ventana de 5 coordenadas al microservicio Python
   - Recibe siguiente coordenada predicha
   - Consulta datos oceánicos (Open-Meteo API)
   - Predice biomasa usando ML
   - Actualiza ventana deslizante
3. **Salida**: Lista de coordenadas con biomasa predicha

## 📁 Estructura del Proyecto

```
SargazoAI-Backend/
│
├── Controllers/
│   └── CoordinateController.cs      # Endpoint principal
│
├── Services/
│   ├── CoordinateService.cs         # Lógica de negocio
│   └── ConfigAPI.cs                 # Configuración de APIs
│
├── DTOs/
│   ├── CoordinateDTO.cs             # Coordenada + biomasa
│   ├── PredictionRequestDTO.cs      # Request para ML Python
│   ├── PredictionResponseDTO.cs     # Response de coordenadas
│   ├── PredictionResultDTO.cs       # Resultado final
│   ├── BiomassDTO.cs                # Request/Response biomasa
│   └── OceanDataDTO.cs              # Datos oceánicos
│
├── Program.cs                       # Configuración de la app
├── appsettings.json                 # Configuración
└── README.md                        # Este archivo
```

## 🔗 APIs Externas Utilizadas

### 1. Microservicio Python - Predicción de Coordenadas

- **URL**: `http://127.0.0.1:8000/predict-coordinate`
- **Método**: POST
- **Body**:

```json
{
  "sequence": [
    [21.29, -89.65],
    [21.29, -89.66],
    ...
  ]
}
```

### 2. Open-Meteo Marine API

- **URL**: `https://marine-api.open-meteo.com/v1/marine`
- **Método**: GET
- **Parámetros**: latitude, longitude, hourly data (últimas 24h)
- **Datos**: Temperatura, velocidad y dirección de corrientes

### 3. Microservicio Python - Predicción de Biomasa

- **URL**: `http://127.0.0.1:8000/predict-biomass`
- **Método**: POST
- **Body**:

```json
{
  "lat": 21.29,
  "lon": -89.65,
  "avg_sea_surface_temperature": 28.5,
  "avg_ocean_current_velocity": 0.35,
  "avg_ocean_current_direction": 1.57
}
```

## ⚙️ Configuración

### CORS

El proyecto está configurado para aceptar requests de cualquier origen (\*). Para cambiar esto, edita `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
```

### URLs de Microservicios

Para cambiar las URLs de los microservicios Python, edita `Services/ConfigAPI.cs`:

```csharp
private const string BaseUrl = "http://127.0.0.1:8000";
```

## 🧪 Testing

### Usando cURL:

```bash
curl -X GET "http://localhost:5000/api/coordinate/predict?iterations=3"
```

### Usando PowerShell:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/api/coordinate/predict?iterations=3' -Method Get
```

### Usando Swagger UI:

1. Navega a `http://localhost:5000/swagger`
2. Expande el endpoint `/api/coordinate/predict`
3. Click en "Try it out"
4. Ingresa el número de iteraciones
5. Click en "Execute"

## 📊 Logs

La aplicación genera logs detallados en la consola:

```
info: Iniciando predicción de coordenadas con 3 iteraciones
info: Iteración 1/3
info: Predicción 1: Lat=21.290123, Long=-89.652456, Biomasa=27.89kg/m²
info: Iteración 2/3
info: Predicción 2: Lat=21.289987, Long=-89.653234, Biomasa=28.12kg/m²
...
```

## 🐛 Troubleshooting

### Error: "Microservicio Python no disponible"

- Verifica que los servicios Python estén corriendo en `http://127.0.0.1:8000`
- Revisa los logs para ver intentos de conexión

### Error: "La secuencia debe tener 5 filas"

- El modelo espera exactamente 5 coordenadas
- El servicio automáticamente hace padding si hay menos de 5

### Sin datos oceánicos

- Verifica conexión a Internet
- La API de Open-Meteo es gratuita pero tiene límites de rate

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto está bajo la Licencia MIT.

## 👥 Autores

- **RaulBecerraB** - [GitHub](https://github.com/RaulBecerraB)

## 🙏 Agradecimientos

- Open-Meteo por su API gratuita de datos oceánicos
- Equipo de desarrollo del proyecto SargazoAI
- Comunidad de .NET

---

**Nota**: Este proyecto es parte de un sistema más amplio de monitoreo y predicción de sargazo. Para información sobre los microservicios Python, consulta los repositorios correspondientes.
