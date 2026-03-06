using Haven.Domain.DTO;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class InMemoryMusterPointRepository : IMusterPointRepository
    {
        private static readonly ConcurrentDictionary<string, MusterPointRequest> _store = new();

        public Task SaveMusterPointAsync(MusterPointRequest request)
        {
            _store.AddOrUpdate(request.IncidentId, request, (_, __) => request);
            return Task.CompletedTask;
        }

        public Task<MusterPointRequest> GetByIncidentIdAsync(string incidentId)
        {
            _store.TryGetValue(incidentId, out var mp);
            return Task.FromResult(mp);
        }
    }
}
