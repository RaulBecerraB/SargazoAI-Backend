namespace SargazoAI_Backend.DTOs;

/// <summary>
/// DTO para la solicitud de predicción al microservicio Python
/// </summary>
public class PredictionRequestDTO
{
    /// <summary>
    /// Secuencia de coordenadas [latitud, longitud]
    /// </summary>
    public List<List<double>> Sequence { get; set; } = new();
}
