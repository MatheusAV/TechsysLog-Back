using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Documents
{
    public sealed class AddressDocument
    {
        [BsonElement("cep")]
        public string Cep { get; set; } = default!;

        [BsonElement("street")]
        public string Street { get; set; } = default!;

        [BsonElement("number")]
        public string Number { get; set; } = default!;

        [BsonElement("district")]
        public string District { get; set; } = default!;

        [BsonElement("city")]
        public string City { get; set; } = default!;

        [BsonElement("state")]
        public string State { get; set; } = default!;
    }
}
