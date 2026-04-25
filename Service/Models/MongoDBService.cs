using MongoDB.Driver;

namespace Service.Models
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Money> _moneyCollection;

        public MongoDBService()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");

            var mongoDatabase = mongoClient.GetDatabase("MoneyDB");

            _moneyCollection = mongoDatabase.GetCollection<Money>("Money");
        }

        public async Task<List<Money>> GetAsync() =>
            await _moneyCollection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(Money newMoney) =>
            await _moneyCollection.InsertOneAsync(newMoney);
    }
}
