namespace SargazoAI_Backend.DTOs;

/// <summary>
/// DTO para las predicciones iteradas
/// </summary>
public class PredictionResultDTO
{
    /// <summary>
    /// Arreglo de coordenadas predichas
    /// </summary>
    public List<CoordinateDTO> PredictedCoordinates { get; set; } = new();

    /// <summary>
    /// Número de iteraciones realizadas
    /// </summary>
    public int IterationsCount { get; set; }
}
