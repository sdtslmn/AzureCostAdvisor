using AzureCostAdvisor.src.AzureCostAdvisor.Core.Models;

namespace AzureCostAdvisor.src.AzureCostAdvisor.Core.Interfaces
{
    public interface IAdvisorService
    {
        Task<IReadOnlyList<CostRecommendation>> GetRecommendationsAsync(
            CostData costData,
            CancellationToken ct = default);
    }
}