using System.Text.Json;
using EMOPlay.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMOPlay.Infrastructure.ExternalServices;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _aiServiceUrl;

    public AIService(HttpClient httpClient, ILogger<AIService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _aiServiceUrl = configuration["AIService:Url"] ?? "http://localhost:5000";
    }

    public async Task<AIEmotionAnalysisResult> AnalyzeEmotionAsync(string base64Image, string targetEmotion)
    {
        try
        {
            var request = new
            {
                image = base64Image,
                targetEmotion = targetEmotion
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_aiServiceUrl}/api/emotion/analyze", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"AI Service error: {response.StatusCode}");
                throw new HttpRequestException($"AI Service returned {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AIEmotionAnalysisResult>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? throw new InvalidOperationException("Failed to deserialize AI service response");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error analyzing emotion: {ex.Message}");
            throw;
        }
    }
}
