namespace TechsysLog.Domain.Common
{
    public abstract class EntityBase
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    }
}
