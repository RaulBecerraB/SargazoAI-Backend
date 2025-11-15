using SargazoAI_Backend.DTOs;
using System.Text.Json;

namespace SargazoAI_Backend.Services;

public interface ICoordinateService
{
    Task<IEnumerable<CoordinateDTO>> GetCoordinatesAsync();
    Task<PredictionResultDTO> PredictCoordinatesAsync(List<CoordinateDTO> initialSequence, int iterations);
}

public class CoordinateService : ICoordinateService
{
    private readonly ILogger<CoordinateService> _logger;
    private readonly HttpClient _httpClient;

    public CoordinateService(ILogger<CoordinateService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public Task<IEnumerable<CoordinateDTO>> GetCoordinatesAsync()
    {
        _logger.LogInformation("Obteniendo coordenadas...");

        // Dummy data - Algunas ubicaciones interesantes
        var coordinates = new List<CoordinateDTO>
        {
            new CoordinateDTO { Latitude = 40.7128, Longitude = -74.0060 },      // New York
            new CoordinateDTO { Latitude = 51.5074, Longitude = -0.1278 },       // London
            new CoordinateDTO { Latitude = 48.8566, Longitude = 2.3522 },        // Paris
            new CoordinateDTO { Latitude = 35.6762, Longitude = 139.6503 },      // Tokyo
            new CoordinateDTO { Latitude = -33.8688, Longitude = 151.2093 },     // Sydney
            new CoordinateDTO { Latitude = 19.4326, Longitude = -99.1332 },      // Mexico City
            new CoordinateDTO { Latitude = 37.7749, Longitude = -122.4194 },     // San Francisco
            new CoordinateDTO { Latitude = 40.4168, Longitude = -3.7038 },       // Madrid
        };

        _logger.LogInformation($"Se retornaron {coordinates.Count} coordenadas");

        return Task.FromResult<IEnumerable<CoordinateDTO>>(coordinates);
    }

    /// <summary>
    /// Predice coordenadas futuras usando el microservicio Python
    /// Itera N veces, usando siempre las últimas 5 coordenadas (4 iniciales + 1 predicha)
    /// </summary>
    public async Task<PredictionResultDTO> PredictCoordinatesAsync(List<CoordinateDTO> initialSequence, int iterations)
    {
        _logger.LogInformation($"Iniciando predicción de coordenadas con {iterations} iteraciones");

        if (initialSequence == null || initialSequence.Count < 4)
        {
            _logger.LogError("La secuencia inicial debe tener al menos 4 coordenadas");
            throw new ArgumentException("La secuencia inicial debe tener al menos 4 coordenadas");
        }

        if (iterations <= 0)
        {
            _logger.LogError("El número de iteraciones debe ser mayor a 0");
            throw new ArgumentException("El número de iteraciones debe ser mayor a 0");
        }

        var predictedCoordinates = new List<CoordinateDTO>();

        // Inicializar la ventana deslizante con las últimas 4 coordenadas
        var slidingWindow = initialSequence.TakeLast(4).ToList();

        try
        {
            for (int i = 0; i < iterations; i++)
            {
                _logger.LogInformation($"Iteración {i + 1}/{iterations}");

                // Crear la solicitud para el microservicio Python
                // The Python service expects a sequence with 5 rows (time steps).
                // Ensure we send exactly 5 by padding with the last known coordinate if needed.
                var seq = slidingWindow.Select(c => new List<double> { c.Latitude, c.Longitude }).ToList();
                while (seq.Count < 5)
                {
                    // duplicate the last coordinate to reach required length
                    seq.Add(new List<double>(seq.Last()));
                }

                var predictionRequest = new PredictionRequestDTO
                {
                    Sequence = seq
                };

                // Llamar al microservicio Python
                var response = await CallPythonMicroserviceAsync(predictionRequest);

                // Crear el DTO con las coordenadas predichas
                var predictedCoordinate = new CoordinateDTO
                {
                    Latitude = response.Latitud_siguiente,
                    Longitude = response.Longitud_siguiente
                };

                predictedCoordinates.Add(predictedCoordinate);

                // Actualizar la ventana deslizante: remover la primera, agregar la nueva predicción
                slidingWindow.RemoveAt(0);
                slidingWindow.Add(predictedCoordinate);

                _logger.LogInformation($"Predicción {i + 1}: Lat={response.Latitud_siguiente:F6}, Long={response.Longitud_siguiente:F6}");
            }

            _logger.LogInformation($"Predicciones completadas. Total: {predictedCoordinates.Count}");

            return new PredictionResultDTO
            {
                PredictedCoordinates = predictedCoordinates,
                IterationsCount = iterations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la predicción de coordenadas");
            throw;
        }
    }

    private async Task<PredictionResponseDTO> CallPythonMicroserviceAsync(PredictionRequestDTO request)
    {
        const int maxAttempts = 3;
        int attempt = 0;

        // Serialize using camelCase to match the Python service OpenAPI contract (expects "sequence")
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        while (true)
        {
            attempt++;
            try
            {
                _logger.LogDebug("Calling Python microservice (attempt {Attempt})", attempt);
                var response = await _httpClient.PostAsync(PythonApiConfig.Endpoints.Predict, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Python microservice returned non-success status {Status}. Response: {Response}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Python microservice returned status {response.StatusCode}");
                }

                var predictionResponse = JsonSerializer.Deserialize<PredictionResponseDTO>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (predictionResponse == null)
                {
                    _logger.LogError("Failed to deserialize prediction response. Raw content: {Content}", responseContent);
                    throw new InvalidOperationException("No se pudo deserializar la respuesta del microservicio");
                }

                return predictionResponse;
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed connecting to Python microservice. Retrying...", attempt);
                // Exponential backoff
                var delayMs = 500 * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delayMs);
                // recreate content because StringContent can be disposed/consumed
                content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error definitivo al conectar con el microservicio Python después de {Attempt} intentos", attempt);
                throw new HttpRequestException("Error al conectar con el microservicio Python", ex);
            }
        }
    }
}

