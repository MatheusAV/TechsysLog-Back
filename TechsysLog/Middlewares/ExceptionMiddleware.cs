using System.Text.Json;
using TechsysLog.Application.Exceptions;

namespace TechsysLog.Api.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;
     
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.HttpStatusHint ?? StatusCodes.Status400BadRequest;

            var payload = new
            {
                error = new
                {
                    code = ex.Code,
                    message = ex.Message
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var payload = new
            {
                error = new
                {
                    code = "UNEXPECTED",
                    message = "Erro inesperado."
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
