using Serilog;

namespace FCG.Presentation.Middlewares
{
    public class SerilogRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogRequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Log.Information("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);

            await _next(context);

            Log.Information("Finished handling request. Response status: {StatusCode}", context.Response.StatusCode);
        }
    }
}
