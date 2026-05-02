using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Core.Models
{
    public record CostRecommendation(
    string Title,
    string Description,
    string Severity,             // Low | Medium | High
    decimal? EstimatedSavings);
}
