using Serilog.Context;

namespace WorkManagement.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate RequestId nếu chưa có
            var requestId = context.TraceIdentifier;

            // Push vào Serilog context để tất cả log trong request đều có RequestId
            using (LogContext.PushProperty("RequestId", requestId))
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path} started | RequestId: {RequestId} | IP: {IP} | User: {User}",
                        context.Request.Method,
                        context.Request.Path,
                        requestId,
                        context.Connection.RemoteIpAddress,
                        context.User?.Identity?.Name ?? "anonymous");

                    await _next(context);

                    sw.Stop();
                    _logger.LogInformation(
                        "HTTP {Method} {Path} {StatusCode} completed in {Elapsed}ms | RequestId: {RequestId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        sw.ElapsedMilliseconds,
                        requestId);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex,
                        "HTTP {Method} {Path} FAILED in {Elapsed}ms | RequestId: {RequestId}",
                        context.Request.Method,
                        context.Request.Path,
                        sw.ElapsedMilliseconds,
                        requestId);
                    throw;
                }
            }
        }
    }
}