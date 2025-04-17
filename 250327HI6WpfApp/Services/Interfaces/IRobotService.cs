namespace _250327HI6WpfApp.Services
{
    public interface IRobotService
    {
        string SendGetRequest(string path);
        string SendPostRequest(string path, string jsonBody);
        string FormatJson(string json);
        void UpdateConnection(string ipAddress, string port);
    }
}