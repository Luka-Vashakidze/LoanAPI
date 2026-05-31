using System.Net;
using System.Text.Json;
using Project.Exceptions;

namespace Project.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not Found: {Message}", ex.Message);
                await WriteResponseAsync(context, (int)HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Bad Request: {Message}", ex.Message);
                await WriteResponseAsync(context, (int)HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
                await WriteResponseAsync(context, (int)HttpStatusCode.Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await WriteResponseAsync(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    "An internal server error occurred.");
            }
        }

        private static Task WriteResponseAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = JsonSerializer.Serialize(new
            {
                StatusCode = statusCode,
                Message = message
            });

            return context.Response.WriteAsync(payload);
        }
    }
}
