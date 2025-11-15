namespace SargazoAI_Backend.DTOs;

public class CoordinateDTO
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Biomasa de sargazo predicha (kg/m²)
    /// </summary>
    public double? SargassumBiomass { get; set; }
}
