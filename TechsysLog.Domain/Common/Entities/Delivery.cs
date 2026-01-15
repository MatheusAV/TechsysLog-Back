namespace TechsysLog.Domain.Common.Entities
{
    public sealed class Delivery : EntityBase
    {
        public string OrderNumber { get; private set; } = default!;
        public DateTime DeliveredAt { get; private set; }

        private Delivery() { }

        public Delivery(string orderNumber, DateTime deliveredAt)
        {
            OrderNumber = string.IsNullOrWhiteSpace(orderNumber) ? throw new ArgumentException("Número do pedido é obrigatório.") : orderNumber.Trim();
            DeliveredAt = deliveredAt == default ? DateTime.UtcNow : deliveredAt;
        }
    }
}