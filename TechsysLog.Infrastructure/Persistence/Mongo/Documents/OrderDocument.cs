using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TechsysLog.Domain.Common;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Documents
{
    public sealed class OrderDocument : EntityBase
    {     

        [BsonElement("orderNumber")]
        public string OrderNumber { get; set; } = default!;

        [BsonElement("description")]
        public string Description { get; set; } = default!;

        [BsonElement("value")]
        public decimal Value { get; set; }

        [BsonElement("address")]
        public AddressDocument Address { get; set; } = default!;

        [BsonElement("status")]
        public string Status { get; set; } = default!; // "Created" | "Delivered"
      
    }
}
