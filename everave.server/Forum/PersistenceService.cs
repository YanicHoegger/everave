using MongoDB.Driver;

namespace everave.server.Forum
{
    public class PersistenceService
    {
        private readonly IMongoCollection<Entry> _collection;

        public PersistenceService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("everave");
            _collection = database.GetCollection<Entry>("editor_contents");
        }

        public async Task SaveAsync(Entry doc)
        {
            await _collection.InsertOneAsync(doc);
        }

        public async Task<List<Entry>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
    }
}
