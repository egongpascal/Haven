using Haven.Domain.DTO;

namespace Haven.Infrastructure
{
    public interface IMusterPointRepository
    {
        Task SaveMusterPointAsync(MusterPointRequest request);
        Task<MusterPointRequest> GetByIncidentIdAsync(string incidentId);
        Task ResolveMusterPointAsync(string incidentId);
    }
}
