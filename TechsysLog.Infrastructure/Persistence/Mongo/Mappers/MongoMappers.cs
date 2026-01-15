using MongoDB.Bson;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Common.Enums;
using TechsysLog.Domain.Common.ValueObjects;
using TechsysLog.Infrastructure.Persistence.Mongo.Documents;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Mappers
{

    public static class MongoMappers
    {
        public static UserDocument ToDoc(this User user) => new()
        {
            Id = user.Id, 
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt
        };

        public static User ToDomain(this UserDocument doc)
            => new(doc.Name, doc.Email, doc.PasswordHash);

        public static OrderDocument ToDoc(this Order order) => new()
        {
           
            OrderNumber = order.OrderNumber,
            Description = order.Description,
            Value = order.Value,
            Status = order.Status.ToString(),            
            Address = new AddressDocument
            {
                Cep = order.DeliveryAddress.Cep,
                Street = order.DeliveryAddress.Street,
                Number = order.DeliveryAddress.Number,
                District = order.DeliveryAddress.District,
                City = order.DeliveryAddress.City,
                State = order.DeliveryAddress.State
            }
        };

        public static Order ToDomain(this OrderDocument doc)
        {
            var address = new Address(doc.Address.Cep, doc.Address.Street, doc.Address.Number,
                                      doc.Address.District, doc.Address.City, doc.Address.State);

            var order = new Order(doc.OrderNumber, doc.Description, doc.Value, address);

            // Ajuste do status conforme persistência
            if (Enum.TryParse<OrderStatus>(doc.Status, out var status) && status == OrderStatus.Delivered)
                order.MarkAsDelivered();

            return order;
        }

        public static DeliveryDocument ToDoc(this Delivery delivery) => new()
        {
            Id = string.IsNullOrWhiteSpace(delivery.Id) ? ObjectId.GenerateNewId().ToString() : delivery.Id,
            OrderNumber = delivery.OrderNumber,
            DeliveredAt = delivery.DeliveredAt,
            CreatedAt = delivery.CreatedAt
        };

        public static Delivery ToDomain(this DeliveryDocument doc)
            => new(doc.OrderNumber, doc.DeliveredAt);

        public static NotificationDocument ToDoc(this Notification n) => new()
        {
            Id = string.IsNullOrWhiteSpace(n.Id) ? ObjectId.GenerateNewId().ToString() : n.Id,
            UserId = n.UserId,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };

        public static Notification ToDomain(this NotificationDocument doc)
        {
            var n = new Notification(doc.UserId, doc.Message);
            if (doc.IsRead) n.MarkAsRead();
            return n;
        }
    }
}
