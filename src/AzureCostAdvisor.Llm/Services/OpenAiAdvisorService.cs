using Azure;
using Azure.AI.OpenAI;
using AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces;
using AzureCostAdvisor.src.AzureCostAdvisor.Core.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Llm.Services
{
    public class OpenAiAdvisorService : IAdvisorService
    {
        private readonly ChatClient _chat;

        public OpenAiAdvisorService(string endpoint, string apiKey, string deployment)
        {
            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _chat = client.GetChatClient(deployment);
        }

        public async Task<IReadOnlyList<CostRecommendation>> GetRecommendationsAsync(
            CostData costData,
            CancellationToken ct = default)
        {
            // Send only top N to keep the prompt cheap.
            var topResources = costData.Resources.Take(25).ToList();

            var payload = JsonSerializer.Serialize(new
            {
                costData.PeriodStart,
                costData.PeriodEnd,
                costData.TotalCost,
                costData.Currency,
                Resources = topResources
            }, new JsonSerializerOptions { WriteIndented = false });

            var system = """
            You are an Azure cost-optimization expert.
            Return ONLY a JSON array of recommendations. Schema:
            [{ "title": string, "description": string, "severity": "Low"|"Medium"|"High", "estimatedSavings": number|null }]
            No prose, no markdown fences.
            """;

            var user = $"Analyze this Azure cost data and recommend optimizations.\n\n{payload}";

            var response = await _chat.CompleteChatAsync(
                new ChatMessage[]
                {
                new SystemChatMessage(system),
                new UserChatMessage(user)
                },
                new ChatCompletionOptions { Temperature = 0.2f },
                ct);

            var text = response.Value.Content[0].Text.Trim();

            // Strip accidental code fences.
            if (text.StartsWith("```"))
                text = text.Trim('`').Replace("json\n", "", StringComparison.OrdinalIgnoreCase);

            return JsonSerializer.Deserialize<List<CostRecommendation>>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CostRecommendation>();
        }
    }
}
