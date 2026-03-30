using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpeechAnalytics.Application.Interfaces;
using SpeechAnalytics.Application.Services;
using SpeechAnalytics.Infrastructure.LlmAnalysis;
using SpeechAnalytics.Infrastructure.Persistence;

namespace SpeechAnalytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var azureSettings = new AzureOpenAiSettings();
        configuration.GetSection("AzureOpenAi").Bind(azureSettings);
        services.AddSingleton(azureSettings);

        services.AddHttpClient<ILlmAnalysisService, AzureOpenAiAnalysisService>();
        services.AddSingleton<ICallSessionRepository, InMemoryCallSessionRepository>();
        services.AddScoped<LiveCallOrchestrator>();

        return services;
    }
}
