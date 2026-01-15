using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Documents
{
    public sealed class DeliveryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        [BsonElement("orderNumber")]
        public string OrderNumber { get; set; } = default!;

        [BsonElement("deliveredAt")]
        public DateTime DeliveredAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
