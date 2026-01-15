using TechsysLog.Domain.Common.Enums;
using TechsysLog.Domain.Common.ValueObjects;

namespace TechsysLog.Domain.Common.Entities
{
    public sealed class Order : EntityBase
    {
        public string OrderNumber { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public decimal Value { get; private set; }
        public Address DeliveryAddress { get; private set; } = default!;
        public OrderStatus Status { get; private set; } = OrderStatus.Created;

        private Order() { }

        public Order(string orderNumber, string description, decimal value, Address deliveryAddress)
        {
            SetOrderNumber(orderNumber);
            SetDescription(description);
            SetValue(value);
            DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
            Status = OrderStatus.Created;
        }

        public void MarkAsDelivered()
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Pedido já está entregue.");

            Status = OrderStatus.Delivered;
        }

        private void SetOrderNumber(string orderNumber)
        {
            OrderNumber = string.IsNullOrWhiteSpace(orderNumber) ? throw new ArgumentException("Número do pedido é obrigatório.") : orderNumber.Trim();
        }

        private void SetDescription(string description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Descrição é obrigatória.") : description.Trim();
        }

        private void SetValue(decimal value)
        {
            if (value <= 0) throw new ArgumentException("Valor deve ser maior que zero.");
            Value = value;
        }
    }
}
