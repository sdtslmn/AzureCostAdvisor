using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces;
using AzureCostAdvisor.src.AzureCostAdvisor.Core.Models;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Azure.Services
{
    public class AzureCostService : IAzureCostService
    {
        private readonly ArmClient _armClient;

        public AzureCostService()
        {
            // DefaultAzureCredential picks up: az login, env vars, managed identity, etc.
            _armClient = new ArmClient(new DefaultAzureCredential());
        }

        public async Task<CostData> GetCostsAsync(
            string subscriptionId,
            DateTime from,
            DateTime to,
            CancellationToken ct = default)
        {
            var scope = new ResourceIdentifier($"/subscriptions/{subscriptionId}");

            var dataset = new QueryDataset
            {
                Granularity = GranularityType.Daily
            };
            dataset.Aggregation.Add("totalCost", new QueryAggregation("Cost", FunctionType.Sum));
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ResourceId"));
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ResourceType"));
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, "ResourceLocation"));

            var query = new QueryDefinition(ExportType.ActualCost, TimeframeType.Custom, dataset)
            {
                TimePeriod = new QueryTimePeriod(from, to)
            };

            var response = await _armClient.UsageQueryAsync(scope, query, ct);
            var result = response.Value;

            // Resolve column indices defensively — column order is not guaranteed.
            var cols = result.Columns.Select((c, i) => (c.Name, i))
                                     .ToDictionary(x => x.Name, x => x.i, StringComparer.OrdinalIgnoreCase);

            var resources = new List<ResourceCost>();
            decimal total = 0;
            string currency = "USD";

            foreach (var row in result.Rows)
            {
                var cost = Convert.ToDecimal(row[cols["totalCost"]]);
                var resId = row[cols["ResourceId"]]?.ToString() ?? "";
                var resType = row[cols["ResourceType"]]?.ToString() ?? "";
                var location = row[cols["ResourceLocation"]]?.ToString() ?? "";
                currency = row[cols["Currency"]]?.ToString() ?? currency;

                resources.Add(new ResourceCost(resId, resType, location, cost, currency));
                total += cost;
            }

            // Aggregate by ResourceId since you'll get one row per day per resource.
            var grouped = resources
                .GroupBy(r => r.ResourceId)
                .Select(g => new ResourceCost(
                    g.Key,
                    g.First().ResourceType,
                    g.First().Location,
                    g.Sum(x => x.Cost),
                    g.First().Currency))
                .OrderByDescending(r => r.Cost)
                .ToList();

            return new CostData(from, to, total, currency, grouped);
        }
    }
}
