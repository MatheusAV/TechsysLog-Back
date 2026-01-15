using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Domain.Common.Entities
{
    public sealed class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        private Notification() { }

        public Notification(string userId, string message)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? throw new ArgumentException("UserId é obrigatório.") : userId.Trim();
            Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException("Mensagem é obrigatória.") : message.Trim();
            IsRead = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
