using Microsoft.AspNetCore.Mvc;
using SargazoAI_Backend.DTOs;
using SargazoAI_Backend.Services;

namespace SargazoAI_Backend.Controllers;

/// <summary>
/// Controlador para manejar operaciones de coordenadas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CoordinateController : ControllerBase
{
    private readonly ICoordinateService _coordinateService;
    private readonly ILogger<CoordinateController> _logger;

    public CoordinateController(ICoordinateService coordinateService, ILogger<CoordinateController> logger)
    {
        _coordinateService = coordinateService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un arreglo de coordenadas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoordinateDTO>>> GetCoordinates()
    {
        try
        {
            _logger.LogInformation("Solicitud GET para obtener coordenadas");
            var coordinates = await _coordinateService.GetCoordinatesAsync();
            return Ok(coordinates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener coordenadas");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
        }
    }

    /// <summary>
    /// Predice coordenadas futuras usando el microservicio Python
    /// </summary>
    /// <param name="iterations">Número de iteraciones de predicción (query parameter)</param>
    [HttpGet("predict")]
    public async Task<ActionResult<PredictionResultDTO>> PredictCoordinates(
        [FromQuery] int iterations = 1)
    {
        try
        {
            if (iterations <= 0)
            {
                return BadRequest("El número de iteraciones debe ser mayor a 0");
            }

            // Use the provided initial coordinates (4 points) as the seed sequence
            var initialSequence = new List<CoordinateDTO>
            {
                new CoordinateDTO { Latitude = 21.295448, Longitude = -89.639144 },
                new CoordinateDTO { Latitude = 21.292690, Longitude = -89.642461 },
                new CoordinateDTO { Latitude = 21.293261, Longitude = -89.642678 },
                new CoordinateDTO { Latitude = 21.292013, Longitude = -89.643941 }
            };

            _logger.LogInformation($"Solicitud GET predict: seed {initialSequence.Count} coords, {iterations} iterations");

            var result = await _coordinateService.PredictCoordinatesAsync(initialSequence, iterations);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación en predicción");
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al conectar con el microservicio Python");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Microservicio Python no disponible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en predicción");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la solicitud");
        }
    }
}

