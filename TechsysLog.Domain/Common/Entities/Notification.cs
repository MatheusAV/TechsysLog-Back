namespace TechsysLog.Domain.Common.Entities
{
    public sealed class Notification
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; private set; } = default!;
        public string Message { get; private set; } = default!;
        public bool IsRead { get; private set; }       
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
