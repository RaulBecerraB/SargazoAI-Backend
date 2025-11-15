namespace SargazoAI_Backend.DTOs;

/// <summary>
/// DTO para la respuesta de predicción del microservicio Python
/// </summary>
public class PredictionResponseDTO
{
    /// <summary>
    /// Latitud predicha
    /// </summary>
    public double Latitud_siguiente { get; set; }

    /// <summary>
    /// Longitud predicha
    /// </summary>
    public double Longitud_siguiente { get; set; }
}
