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

        public Task ResolveMusterPointAsync(string incidentId)
        {
            if (_store.TryGetValue(incidentId, out var mp))
            {
                var resolved = new MusterPointRequest
                {
                    Id = mp.Id,
                    IncidentId = mp.IncidentId,
                    Name = mp.Name,
                    Latitude = mp.Latitude,
                    Longitude = mp.Longitude,
                    RadiusMetres = mp.RadiusMetres,
                    CreatedBy = mp.CreatedBy,
                    CreatedAt = mp.CreatedAt,
                    ResolvedAt = DateTime.UtcNow,
                    TotalMembers = mp.TotalMembers,
                    ArrivedCount = mp.ArrivedCount,
                };
                _store[incidentId] = resolved;
            }
            return Task.CompletedTask;
        }
    }
}
