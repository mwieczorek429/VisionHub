using System.Diagnostics;

namespace VisionHub.Api.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Incoming request: {method} {url}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context); 

                stopwatch.Stop();


                _logger.LogInformation("Response: {statusCode} for {method} {url} in {elapsed}ms",
                    context.Response.StatusCode,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "Unhandled exception for {method} {url} after {elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);

                throw; 
            }
        }
    }

}
