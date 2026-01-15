namespace TechsysLog.Application.Exceptions
{
    public static class AppErrors
    {
        public static AppException NotFound(string message) => new("NOT_FOUND", message, 404);
        public static AppException Conflict(string message) => new("CONFLICT", message, 409);
        public static AppException Validation(string message) => new("VALIDATION", message, 400);
        public static AppException Unauthorized(string message) => new("UNAUTHORIZED", message, 401);
    }
}
