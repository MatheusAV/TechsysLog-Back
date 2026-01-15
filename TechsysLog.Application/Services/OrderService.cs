using TechsysLog.Application.Abstractions.Cep;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Common.ValueObjects;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Application.Services
{
    public sealed class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly ICepService _cep;
        private readonly INotificationService _notifications;
        private readonly INotificationPublisher _publisher;

        public OrderService(
            IOrderRepository orders,
            ICepService cep,
            INotificationService notifications,
            INotificationPublisher publisher)
        {
            _orders = orders;
            _cep = cep;
            _notifications = notifications;
            _publisher = publisher;
        }

        public async Task CreateAsync(CreateOrderRequest request, string actorUserId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.OrderNumber)) throw AppErrors.Validation("Número do pedido é obrigatório.");
            if (request.Value <= 0) throw AppErrors.Validation("Valor deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(request.Cep)) throw AppErrors.Validation("CEP é obrigatório.");
            if (string.IsNullOrWhiteSpace(request.Number)) throw AppErrors.Validation("Número do endereço é obrigatório.");

            var existing = await _orders.GetByOrderNumberAsync(request.OrderNumber.Trim(), ct);
            if (existing is not null) throw AppErrors.Conflict("Já existe pedido com este número.");

            // Busca endereço via API externa a partir do CEP (regra do case)
            var cepResult = await _cep.GetAddressByCepAsync(request.Cep, ct);

            var address = new Address(
                cep: cepResult.Cep,
                street: cepResult.Street,
                number: request.Number,
                district: cepResult.District,
                city: cepResult.City,
                state: cepResult.State
            );

            var order = new Order(request.OrderNumber, request.Description, request.Value, address);
            await _orders.CreateAsync(order, ct);

            // Notificação para o usuário que realizou a ação (pode ser ajustado para outros perfis)
            var message = $"Pedido {order.OrderNumber} cadastrado.";
            var notif = await _notifications.CreateAsync(actorUserId, message, ct);

            // Atualização em tempo real
            await _publisher.PublishToAllAsync(new
            {
                type = "ORDER_CREATED",
                orderNumber = order.OrderNumber,
                status = order.Status.ToString(),
                createdAt = order.CreatedAt
            }, ct);

            await _publisher.PublishToUserAsync(actorUserId, new
            {
                type = "NOTIFICATION",
                notificationId = notif.Id,
                message = notif.Message,
                createdAt = notif.CreatedAt
            }, ct);
        }

        public async Task<List<OrderResponse>> ListAsync(CancellationToken ct)
        {
            var list = await _orders.ListAsync(ct);

            return list.Select(o => new OrderResponse(
                OrderNumber: o.OrderNumber,
                Description: o.Description,
                Value: o.Value,
                Cep: o.DeliveryAddress.Cep,
                Street: o.DeliveryAddress.Street,
                Number: o.DeliveryAddress.Number,
                District: o.DeliveryAddress.District,
                City: o.DeliveryAddress.City,
                State: o.DeliveryAddress.State,
                Status: o.Status.ToString(),
                CreatedAt: o.CreatedAt
            )).ToList();
        }
    }
}
