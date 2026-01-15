using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Documents
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public sealed class UserDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = default!;

        [BsonElement("email")]
        public string Email { get; set; } = default!;

        [BsonElement("name")]
        public string Name { get; set; } = default!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = default!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

}
