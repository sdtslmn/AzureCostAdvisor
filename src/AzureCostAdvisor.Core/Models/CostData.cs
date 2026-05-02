using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Core.Models
{
    public record CostData(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalCost,
    string Currency,
    IReadOnlyList<ResourceCost> Resources); 
}
