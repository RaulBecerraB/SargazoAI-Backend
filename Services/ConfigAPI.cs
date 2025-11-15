namespace SargazoAI_Backend.Services;

public static class PythonApiConfig
{

    private const string BaseUrl = "http://127.0.0.1:8000";

    public static class Endpoints
    {

        public static string Health => $"{BaseUrl}/health";

        public static string Predict => $"{BaseUrl}/predict";
    }

    public static string GetEndpoint(string endpoint) => endpoint;

    public static string GetBaseUrl() => BaseUrl;
}
