using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Documents
{
    public sealed class NotificationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        [BsonElement("userId")]
        public string UserId { get; set; } = default!;

        [BsonElement("message")]
        public string Message { get; set; } = default!;

        [BsonElement("isRead")]
        public bool IsRead { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
