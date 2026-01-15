namespace TechsysLog.Application.DTOs.Users
{
    public sealed record AuthResponse(
    string UserId,
    string Name,
    string Email,
    string AccessToken);

}
