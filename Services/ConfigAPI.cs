namespace SargazoAI_Backend.Services;

public static class PythonApiConfig
{

    private const string BaseUrl = "sargazoai-predicter-production.up.railway.app";

    public static class Endpoints
    {

        public static string Health => $"{BaseUrl}/health";

        public static string Predict => $"{BaseUrl}/predict-coordinate";

        public static string PredictBiomass => $"{BaseUrl}/predict-biomass";
    }

    public static string GetEndpoint(string endpoint) => endpoint;

    public static string GetBaseUrl() => BaseUrl;
}
