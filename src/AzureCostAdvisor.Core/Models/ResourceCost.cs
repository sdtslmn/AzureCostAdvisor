using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Core.Models
{
    public record ResourceCost(
    string ResourceId,
    string ResourceType,
    string Location,
    decimal Cost,
    string Currency);
}
