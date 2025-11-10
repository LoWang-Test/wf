using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace PackageGenerator;

public class Login
{
    private const string TokenUrl = "https://www.nuget.org/api/v2/token";
    private const string Audience = "https://www.nuget.org";
    private readonly HttpClient _httpClient = new();

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Requesting OIDC token...");
        var requestToken = Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_TOKEN");
        ArgumentException.ThrowIfNullOrEmpty(requestToken);
        var requestUrl = Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_URL");
        ArgumentException.ThrowIfNullOrEmpty(requestUrl);

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{requestUrl}&audience={HttpUtility.UrlEncode(Audience)}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestToken);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        Console.WriteLine("OIDC token acquired.");

        var keyRequest = new ApiKeyRequest
        {
            Username = "Lo_Wang",
            TokenType = "ApiKey"
        };
        var message = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
        message.Content = JsonContent.Create(keyRequest);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Value);
        message.Headers.UserAgent.Clear();
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("nuget", "login-action"));

        response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("API key acquired.");

        return (await response.Content.ReadFromJsonAsync<ApiKeyResponse>()).ApiKey;
    }

    class TokenResponse
    {
        public string? Value { get; set; }
    }

    class ApiKeyRequest
    {
        public required string Username { get; set; }
        public required string TokenType { get; set; }
    }

    class ApiKeyResponse
    {
        public string ApiKey { get; set; }
    }
}
