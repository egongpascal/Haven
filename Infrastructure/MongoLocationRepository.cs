using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class MongoLocationRepository : ILocationRepository
    {
        private readonly IMongoCollection<LocationData> _collection;

        public MongoLocationRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("haven_location_db");
            _collection = database.GetCollection<LocationData>("locations");
        }

        public async Task SaveLocationAsync(LocationData location)
        {
            await _collection.InsertOneAsync(location);
        }

        public async Task<IEnumerable<LocationData>> GetLocationsByGroupAsync(string groupId)
        {
            var filter = Builders<LocationData>.Filter.Eq(l => l.GroupId, groupId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<LocationData?> GetLatestLocationAsync(Guid userId)
        {
            var filter = Builders<LocationData>.Filter.Eq(l => l.UserId, userId);
            // Sort by Timestamp descending and take the first (latest)
            return await _collection
                .Find(filter)
                .SortByDescending(l => l.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}
