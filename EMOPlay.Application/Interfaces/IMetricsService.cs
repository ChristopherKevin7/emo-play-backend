using EMOPlay.Application.DTOs.Metrics;

namespace EMOPlay.Application.Interfaces;

public interface IMetricsService
{
    Task<ChildMetricsResponse> GetChildMetricsAsync(Guid childId, Guid? sessionId = null, DateTime? startDate = null, DateTime? endDate = null);
}
