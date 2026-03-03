using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Haven.Infrastructure
{
    public class MongoGeofenceRepository : IGeofenceRepository
    {
        private readonly IMongoCollection<GeofenceData> _collection;

        public MongoGeofenceRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("haven_location_db");
            _collection = database.GetCollection<GeofenceData>("geofences");
        }

        public async Task SetGeofenceAsync(Guid groupId, double radiusMeters)
        {
            var filter = Builders<GeofenceData>.Filter.Eq(g => g.GroupId, groupId);
            var update = Builders<GeofenceData>.Update.Set(g => g.RadiusMeters, radiusMeters);
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }

        public async Task<GeofenceData?> GetGeofenceAsync(Guid groupId)
        {
            var filter = Builders<GeofenceData>.Filter.Eq(g => g.GroupId, groupId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }

    public class GeofenceData
    {
        public Guid GroupId { get; set; }
        public double RadiusMeters { get; set; }
    }

    public interface IGeofenceRepository
    {
        Task SetGeofenceAsync(Guid groupId, double radiusMeters);
        Task<GeofenceData?> GetGeofenceAsync(Guid groupId);
    }
}
