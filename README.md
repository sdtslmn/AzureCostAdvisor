AzureCostAdvisor
An LLM-powered Azure cost analysis and optimization tool. AzureCostAdvisor pulls
your Azure consumption data via the Cost Management API, then uses Azure OpenAI
to generate prioritized, human-readable recommendations for reducing your cloud
spend.
> **Status:** 🚧 Early development.
---
Why
Azure's native cost tools are powerful but raw — they tell you what you spent,
not what to do about it. AzureCostAdvisor closes that gap by combining:
Azure Cost Management API — authoritative cost data per resource
Azure Advisor (planned) — Microsoft's own rule-based recommendations
LLM reasoning — synthesizes findings into a prioritized action list with
estimated savings
The result is a single command that answers: "Where is my money going, and
what should I do this week to spend less?"
---
Features
📊 Pulls 30-day cost breakdown by resource, type, and location
🤖 LLM-generated optimization recommendations with severity and savings estimates
🔌 Pluggable LLM provider (Azure OpenAI today; OpenAI / GitHub Models swappable)
🧱 Clean layered architecture — domain logic has zero SDK dependencies
🔐 Uses `DefaultAzureCredential` — no secrets needed for Azure auth in dev
Roadmap
[ ] Azure Advisor API integration
[ ] Function-calling tools (idle resource detection, RI candidates, right-sizing)
[ ] Azure Resource Graph queries for utilization data
[ ] HTML / Markdown report export
[ ] Multi-subscription support
[ ] Local caching of cost queries
---
Architecture
```
┌─────────────────────────┐
│  AzureCostAdvisor.Console│  ← entry point, DI, config
└────────────┬────────────┘
             │
   ┌─────────┴──────────┐
   ▼                    ▼
┌──────────────┐  ┌──────────────┐
│   .Azure     │  │    .Llm      │
│ Cost Mgmt    │  │ Azure OpenAI │
└──────┬───────┘  └──────┬───────┘
       │                 │
       └────────┬────────┘
                ▼
       ┌────────────────┐
       │     .Core      │  ← models + interfaces
       └────────────────┘
```
The `Core` project has no external dependencies — swapping the LLM provider or
the cost data source is a one-project change.
Project layout
```
AzureCostAdvisor/
├── src/
│   ├── AzureCostAdvisor.Console/   # entry point, DI wiring, CLI
│   ├── AzureCostAdvisor.Core/      # models + interfaces (no SDK refs)
│   ├── AzureCostAdvisor.Azure/     # Azure Cost Management integration
│   └── AzureCostAdvisor.Llm/       # LLM advisor implementation
├── tests/
│   └── AzureCostAdvisor.Tests/
├── AzureCostAdvisor.sln
└── README.md
```
---
Prerequisites
.NET 8 SDK or later
Azure CLI — `az login` for local development
Azure subscription with Cost Management Reader role
Azure OpenAI resource with a deployed chat model (e.g. `gpt-4o-mini`)
---
Getting started
1. Clone and build
```bash
git clone <your-repo-url> AzureCostAdvisor
cd AzureCostAdvisor
dotnet build
```
2. Authenticate to Azure
```bash
az login
```
`DefaultAzureCredential` will pick this up automatically. In production it will
fall back to managed identity, environment variables, etc.
3. Configure secrets
Use .NET user secrets so nothing sensitive ends up in source control:
```bash
cd src/AzureCostAdvisor.Console
dotnet user-secrets init
dotnet user-secrets set "Azure:SubscriptionId"   "<your-subscription-id>"
dotnet user-secrets set "AzureOpenAI:Endpoint"   "https://<your-resource>.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey"     "<your-key>"
dotnet user-secrets set "AzureOpenAI:Deployment" "gpt-4o-mini"
```
4. Run
```bash
dotnet run --project src/AzureCostAdvisor.Console
```
---
Configuration reference
Key	Description	Required
`Azure:SubscriptionId`	Subscription to analyze	Yes
`AzureOpenAI:Endpoint`	Azure OpenAI resource endpoint	Yes
`AzureOpenAI:ApiKey`	API key for the resource	Yes
`AzureOpenAI:Deployment`	Deployment name of the chat model	Yes
Configuration sources are layered in this order (later wins):
`appsettings.json`
User secrets (local dev only)
Environment variables
---
Example output
```
Fetching costs 2026-04-02 → 2026-05-02...
Total: 1,847.32 USD  (43 resources)

Top 5 resources:
    412.18 USD  Microsoft.Compute/virtualMachines        /subscriptions/.../vm-prod-01
    287.40 USD  Microsoft.Sql/servers/databases          /subscriptions/.../sqldb-main
    198.05 USD  Microsoft.Storage/storageAccounts        /subscriptions/.../stprodlogs
    ...

Asking the advisor...

[High] Idle production VM detected (~285 USD)
  vm-prod-01 has averaged <3% CPU over the period. Consider downsizing from
  Standard_D8s_v3 to Standard_D2s_v3, or shutting down outside business hours.

[Medium] Storage account using hot tier for archival data (~95 USD)
  stprodlogs holds 800GB in hot tier with no recent reads. Move to cool or
  archive tier via lifecycle management policy.

[Low] Untagged resources hindering attribution
  17 resources lack a 'CostCenter' tag, making chargeback impossible.
```
---
Security notes
Never commit `appsettings.json` with real keys — use user secrets or env vars
The LLM only receives aggregated cost data (resource IDs, types, amounts) —
no credentials, connection strings, or resource contents are sent
For production, prefer managed identity over API keys for both Azure
Resource Manager and Azure OpenAI
---
Contributing
This is an early-stage personal project. Issues and PRs welcome once the
function-calling rewrite lands.
---
License
TBD