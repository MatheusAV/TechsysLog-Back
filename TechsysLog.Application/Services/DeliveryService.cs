using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Application.Services
{
    public sealed class DeliveryService : IDeliveryService
    {
        private readonly IOrderRepository _orders;
        private readonly IDeliveryRepository _deliveries;
        private readonly INotificationService _notifications;
        private readonly INotificationPublisher _publisher;

        public DeliveryService(
            IOrderRepository orders,
            IDeliveryRepository deliveries,
            INotificationService notifications,
            INotificationPublisher publisher)
        {
            _orders = orders;
            _deliveries = deliveries;
            _notifications = notifications;
            _publisher = publisher;
        }

        public async Task RegisterAsync(RegisterDeliveryRequest request, string actorUserId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.OrderNumber))
                throw AppErrors.Validation("Número do pedido é obrigatório.");

            var order = await _orders.GetByOrderNumberAsync(request.OrderNumber.Trim(), ct);
            if (order is null) throw AppErrors.NotFound("Pedido não encontrado.");

            var alreadyDelivered = await _deliveries.GetByOrderNumberAsync(order.OrderNumber, ct);
            if (alreadyDelivered is not null) throw AppErrors.Conflict("Entrega já registrada para este pedido.");

            // Regra: ao registrar entrega, altera status do pedido
            order.MarkAsDelivered();
            await _orders.UpdateAsync(order, ct);

            var delivery = new Delivery(order.OrderNumber, request.DeliveredAt ?? DateTime.UtcNow);
            await _deliveries.CreateAsync(delivery, ct);

            var message = $"Entrega registrada para o pedido {order.OrderNumber}.";
            var notif = await _notifications.CreateAsync(actorUserId, message, ct);

            // Atualizações em tempo real
            await _publisher.PublishToAllAsync(new
            {
                type = "DELIVERY_REGISTERED",
                orderNumber = order.OrderNumber,
                status = order.Status.ToString(),
                deliveredAt = delivery.DeliveredAt
            }, ct);

            await _publisher.PublishToUserAsync(actorUserId, new
            {
                type = "NOTIFICATION",
                notificationId = notif.Id,
                message = notif.Message,
                createdAt = notif.CreatedAt
            }, ct);
        }
    }
}
