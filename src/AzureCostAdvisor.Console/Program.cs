using AzureCostAdvisor.src.AzureCostAdvisor.Azure.Services;
using AzureCostAdvisor.src.AzureCostAdvisor.Console;
using AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces;
using AzureCostAdvisor.src.AzureCostAdvisor.Llm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)   // for local dev secrets
    .AddEnvironmentVariables();

builder.Services.AddSingleton<IAzureCostService, AzureCostService>();

builder.Services.AddSingleton<IAdvisorService>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new OpenAiAdvisorService(
        cfg["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("AzureOpenAI:Endpoint missing"),
        cfg["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("AzureOpenAI:ApiKey missing"),
        cfg["AzureOpenAI:Deployment"] ?? throw new InvalidOperationException("AzureOpenAI:Deployment missing"));
});

builder.Services.AddSingleton<CostAdvisorApp>();

using var host = builder.Build();
await host.Services.GetRequiredService<CostAdvisorApp>().RunAsync();
