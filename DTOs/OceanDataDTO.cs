namespace SargazoAI_Backend.DTOs;

/// <summary>
/// DTO para respuesta de la API de Open-Meteo
/// </summary>
public class OceanDataResponseDTO
{
    public OceanDataHourlyDTO? Hourly { get; set; }
}

/// <summary>
/// Datos horarios de la API de Open-Meteo
/// </summary>
public class OceanDataHourlyDTO
{
    public List<string>? Time { get; set; }
    public List<double?>? Sea_Surface_Temperature { get; set; }
    public List<double?>? Ocean_Current_Velocity { get; set; }
    public List<double?>? Ocean_Current_Direction { get; set; }
}

/// <summary>
/// DTO para datos oceánicos procesados
/// </summary>
public class OceanDataDTO
{
    public double? AvgSeaSurfaceTemperature { get; set; }
    public double? AvgOceanCurrentVelocity { get; set; }
    public double? AvgOceanCurrentDirection { get; set; }
    public string? LastUpdateTime { get; set; }
}
