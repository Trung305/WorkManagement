namespace WorkManagement.Web.Middleware
{
    namespace WorkManagement.Web.Middleware
    {
        public class GlobalExceptionMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<GlobalExceptionMiddleware> _logger;

            public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                catch (Exception ex)
                {
                    var requestId = context.TraceIdentifier;
                    _logger.LogError(ex,
                        "Unhandled exception | RequestId: {RequestId} | Path: {Path} | Method: {Method}",
                        requestId,
                        context.Request.Path,
                        context.Request.Method);

                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 500;
                        if (context.Request.Headers["Accept"].ToString().Contains("application/json"))
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(
                                $"{{\"error\":\"Lỗi hệ thống\",\"requestId\":\"{requestId}\"}}");
                        }
                        else
                        {
                            context.Response.Redirect($"/Home/Error?requestId={requestId}");
                        }
                    }
                }
            }
        }
    }
}
