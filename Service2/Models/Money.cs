
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace Service.Models
{
    public class Money
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public string Id { get; set; }
        public string Name { get; set; }
        public int Year_of_creation { get; set; }
        public string Country { get; set; }
      


    }
}
