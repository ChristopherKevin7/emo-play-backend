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
        _aiServiceUrl = configuration["AIService:Url"] ?? "http://localhost:8000";
    }

    public async Task<AIBatchAnalysisResult> AnalyzeBatchAsync(List<string> base64Images, string targetEmotion)
    {
        var targetUrl = $"{_aiServiceUrl}/api/v1/analyze";

        _logger.LogInformation("[AIService] Iniciando chamada ao módulo de IA");
        _logger.LogInformation("[AIService] URL alvo: {Url}", targetUrl);
        _logger.LogInformation("[AIService] Emoção alvo (local): '{TargetEmotion}'", targetEmotion);
        _logger.LogInformation("[AIService] Quantidade de imagens: {Count}", base64Images.Count);

        try
        {
            var requestBody = new
            {
                images = base64Images
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("[AIService] Body enviado: {{ images: [{Count} imagens] }}",
                base64Images.Count);

            var jsonContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("[AIService] Enviando POST para: {Url}", targetUrl);
            var response = await _httpClient.PostAsync(targetUrl, jsonContent);

            _logger.LogInformation("[AIService] Status code recebido: {StatusCode} ({StatusCodeInt})",
                response.StatusCode, (int)response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[AIService] Resposta bruta do módulo de IA: {RawResponse}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[AIService] ERRO: Módulo de IA retornou status {StatusCode}. Body: {Body}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"AI Service returned {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<AIBatchAnalysisResult>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                _logger.LogError("[AIService] ERRO: Não foi possível deserializar a resposta: {RawResponse}", responseContent);
                throw new InvalidOperationException("Failed to deserialize AI service response");
            }

            _logger.LogInformation("[AIService] Resultado desserializado - {Count} predictions:", result.Predictions.Count);
            foreach (var pred in result.Predictions)
            {
                _logger.LogInformation("[AIService]   → {Emotion}: {Score:F4}", pred.Emotion, pred.Score);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("[AIService] ERRO de HTTP ao chamar módulo de IA: {Message}", ex.Message);
            _logger.LogError("[AIService] URL que falhou: {Url}", targetUrl);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("[AIService] ERRO inesperado: {Message}", ex.Message);
            _logger.LogError("[AIService] StackTrace: {StackTrace}", ex.StackTrace);
            throw;
        }
    }
}
