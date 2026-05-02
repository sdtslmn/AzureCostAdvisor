using AzureCostAdvisor.src.AzureCostAdvisor.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces
{
    public interface IAzureCostService
    {
        Task<CostData> GetCostsAsync(
            string subscriptionId,
            DateTime from,
            DateTime to,
            CancellationToken ct = default);
    }
}
