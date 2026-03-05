using Haven.Domain.DTO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public interface IEmergencyRepository
    {
        Task SaveEmergencyAsync(EmergencyRequest request);
        Task UpdateEmergencyStatusAsync(string id, string status);
        Task<List<EmergencyRequest>> GetByGroupAsync(string groupId);
        Task<EmergencyRequest> GetByIdAsync(string id);
    }

    public class EmergencyRepository : IEmergencyRepository
    {
        private readonly IMongoCollection<EmergencyRequest> _collection;

        public EmergencyRepository(string connectionString)
        {
            var client = new MongoDB.Driver.MongoClient(connectionString);
            var database = client.GetDatabase("haven_location_db");
            _collection = database.GetCollection<EmergencyRequest>("emergencies");
        }

        public async Task SaveEmergencyAsync(EmergencyRequest request)
        {
            request.Timestamp = DateTime.UtcNow;
            request.Status = "Active";
            await _collection.InsertOneAsync(request);
        }

        public async Task UpdateEmergencyStatusAsync(string id, string status)
        {
            var filter = Builders<EmergencyRequest>.Filter.Eq(e => e.Id, id);
            var update = Builders<EmergencyRequest>.Update.Set(e => e.Status, status);
            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<List<EmergencyRequest>> GetByGroupAsync(string groupId)
        {
            var filter = Builders<EmergencyRequest>.Filter.Eq(e => e.GroupId, groupId);
            var sort = Builders<EmergencyRequest>.Sort.Descending(e => e.Timestamp);
            return await _collection.Find(filter).Sort(sort).Limit(50).ToListAsync();
        }

        public async Task<EmergencyRequest> GetByIdAsync(string id)
        {
            var filter = Builders<EmergencyRequest>.Filter.Eq(e => e.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
