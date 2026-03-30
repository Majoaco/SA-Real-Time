using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Domain.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace SpeechAnalytics.Infrastructure.LlmAnalysis;

public sealed class AzureOpenAiAnalysisService : ILlmAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAiSettings _settings;

    public AzureOpenAiAnalysisService(HttpClient httpClient, AzureOpenAiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<RealTimeSuggestionResult> GetRealTimeSuggestionsAsync(
        string transcriptSoFar,
        CallType callType,
        IReadOnlyCollection<string> completedSteps,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildSuggestionSystemPrompt(callType);
        var userPrompt = BuildSuggestionUserPrompt(transcriptSoFar, completedSteps);

        var responseText = await CallAzureOpenAiAsync(systemPrompt, userPrompt, cancellationToken);
        return ParseSuggestionResponse(responseText);
    }

    public async Task<TemperatureAnalysisResult> AnalyzeTemperatureAsync(
        string transcriptSoFar,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
            Sos un analizador de sentimiento en tiempo real para llamadas de call center.
            Analizá la conversación y devolvé un JSON con:
            {"emotional": 0-100, "sales": 0-100, "conflict": 0-100}
            - emotional: sentimiento general (0=muy negativo, 100=muy positivo)
            - sales: interés del cliente en comprar (0=rechazo total, 100=quiere cerrar)
            - conflict: nivel de tensión (0=sin conflicto, 100=conflicto severo)
            Respondé SOLO con el JSON, sin texto adicional.
            """;

        var responseText = await CallAzureOpenAiAsync(systemPrompt, transcriptSoFar, cancellationToken);
        return ParseTemperatureResponse(responseText);
    }

    private string BuildSuggestionSystemPrompt(CallType callType)
    {
        var callTypeDesc = callType == CallType.Outbound ? "saliente (el asesor llama)" : "entrante (el cliente llama)";
        return $$"""
            Sos un asistente en tiempo real para un agente de call center BBVA.
            Tipo de llamada: {{callTypeDesc}}.

            Protocolo BBVA - Pasos a verificar:
            1. APERTURA: Presentación con nombre/apellido, origen, preguntar por persona, motivo
            2. DISCURSO: Mensaje comercial claro y respetuoso
            3. CORDIALIDAD: Trato agradable, empático y positivo
            4. CIERRES PARCIALES: Obtener compromisos parciales
            5. CONDUCCIÓN: Guiar la llamada con seguridad
            6. PROCEDIMIENTO/BENEFICIOS: Seguir speech, info correcta
            7. OBJECIONES: Rebatir con seguridad y argumentos
            8. SOLICITUD DATOS: Pedir datos para venta, ofrecer ECU y alertas SMS
            9. DESPEDIDA: Cierre formal, rellamado, sucursal, condiciones generales
            10. REGISTRO: Codificación correcta

            Respondé en JSON:
            {
              "suggestions": ["sugerencia corta 1", "sugerencia corta 2"],
              "completed_steps": ["apertura", "discurso"],
              "current_phase": "cordialidad"
            }
            Máximo 2 sugerencias, directas y accionables. SOLO JSON.
            """;
    }

    private static string BuildSuggestionUserPrompt(string transcript, IReadOnlyCollection<string> completedSteps)
    {
        var stepsStr = completedSteps.Any() ? string.Join(", ", completedSteps) : "ninguno";
        return $$"""
            Pasos ya completados: {{stepsStr}}

            Transcripción hasta ahora:
            {{transcript}}
            """;
    }

    private async Task<string> CallAzureOpenAiAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var url = $"{_settings.Endpoint}/openai/deployments/{_settings.DeploymentName}/chat/completions?api-version=2024-02-01";

        var request = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            max_tokens = 500,
            temperature = 0.3
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = JsonContent.Create(request);
        httpRequest.Headers.Add("api-key", _settings.ApiKey);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private static RealTimeSuggestionResult ParseSuggestionResponse(string responseText)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(responseText);
            var suggestions = json.GetProperty("suggestions")
                .EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var completedSteps = json.GetProperty("completed_steps")
                .EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var currentPhase = json.TryGetProperty("current_phase", out var phase) ? phase.GetString() : null;

            return new RealTimeSuggestionResult(suggestions, completedSteps, currentPhase);
        }
        catch
        {
            return new RealTimeSuggestionResult(new List<string>(), new List<string>(), null);
        }
    }

    private static TemperatureAnalysisResult ParseTemperatureResponse(string responseText)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(responseText);
            return new TemperatureAnalysisResult(
                json.GetProperty("emotional").GetInt32(),
                json.GetProperty("sales").GetInt32(),
                json.GetProperty("conflict").GetInt32());
        }
        catch
        {
            return new TemperatureAnalysisResult(50, 50, 0);
        }
    }
}

public sealed class AzureOpenAiSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4o";
}
