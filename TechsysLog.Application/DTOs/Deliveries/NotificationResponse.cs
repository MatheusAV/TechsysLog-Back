namespace TechsysLog.Application.DTOs.Deliveries
{
    public sealed record NotificationResponse(
      string Id,
      string Message,
      bool IsRead,
      DateTime CreatedAt
  );
}
