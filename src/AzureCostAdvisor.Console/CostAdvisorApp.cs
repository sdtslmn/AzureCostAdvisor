using AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Console
{
    public class CostAdvisorApp
    {
        private readonly IAzureCostService _cost;
        private readonly IAdvisorService _advisor;
        private readonly IConfiguration _cfg;

        public CostAdvisorApp(IAzureCostService cost, IAdvisorService advisor, IConfiguration cfg)
        {
            _cost = cost;
            _advisor = advisor;
            _cfg = cfg;
        }

        public async Task RunAsync()
        {
            var subId = _cfg["Azure:SubscriptionId"];
            if (string.IsNullOrWhiteSpace(subId))
            {
                System.Console.Write("Subscription ID: ");
                subId = System.Console.ReadLine();
            }

            var to = DateTime.UtcNow.Date;
            var from = to.AddDays(-30);

            System.Console.WriteLine($"\nFetching costs {from:yyyy-MM-dd} → {to:yyyy-MM-dd}...");
            var costs = await _cost.GetCostsAsync(subId!, from, to);

            System.Console.WriteLine($"Total: {costs.TotalCost:F2} {costs.Currency}  ({costs.Resources.Count} resources)\n");
            System.Console.WriteLine("Top 5 resources:");
            foreach (var r in costs.Resources.Take(5))
                System.Console.WriteLine($"  {r.Cost,10:F2} {r.Currency}  {r.ResourceType,-40} {r.ResourceId}");

            System.Console.WriteLine("\nAsking the advisor...\n");
            var recs = await _advisor.GetRecommendationsAsync(costs);

            foreach (var r in recs)
            {
                var savings = r.EstimatedSavings is null ? "" : $" (~{r.EstimatedSavings:F0} {costs.Currency})";
                System.Console.WriteLine($"[{r.Severity}] {r.Title}{savings}");
                System.Console.WriteLine($"  {r.Description}\n");
            }
        }
    }
}
