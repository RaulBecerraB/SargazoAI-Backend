namespace SargazoAI_Backend.DTOs;

/// <summary>
/// DTO para la solicitud de predicción de biomasa
/// </summary>
public class BiomassRequestDTO
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Avg_Sea_Surface_Temperature { get; set; }
    public double Avg_Ocean_Current_Velocity { get; set; }
    public double Avg_Ocean_Current_Direction { get; set; }
}

/// <summary>
/// DTO para la respuesta de predicción de biomasa
/// </summary>
public class BiomassResponseDTO
{
    public double Sargassum_Biomass { get; set; }
}
