using System.Net;
using System.Text.Json;
using Application.Core;

namespace API.MiddleWare
{
    public class ExceptionMiddleWware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleWware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleWware(RequestDelegate next, ILogger<ExceptionMiddleWware> logger,
         IHostEnvironment env)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new AppException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new AppException(context.Response.StatusCode, "Server Error");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);

            }
        }
    }
}