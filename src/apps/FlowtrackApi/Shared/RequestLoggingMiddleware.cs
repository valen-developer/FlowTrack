using System.Diagnostics;
using Serilog.Context;

namespace FlowtrackApi.Shared;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<RequestLoggingMiddleware> logger)
    {
        // 1. Tomar o generar CorrelationId
        var correlationId = CorrelationContext.Get()
            ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? CorrelationContext.Generate();

        CorrelationContext.Set(correlationId);

        // 2. Scope de Serilog para enriquecer todos los logs de esta request
        using var _ = LogContext.PushProperty("CorrelationId", correlationId);

        // 3. Scope de ILogger para los logs que usen ILogger<T> directamente
        using var scope = logger.BeginScope(new { CorrelationId = correlationId });

        var sw = Stopwatch.StartNew();

        logger.LogInformation(
            "HTTP {Method} {Path} started",
            context.Request.Method,
            context.Request.Path
        );

        try
        {
            await _next(context);

            sw.Stop();
            logger.LogInformation(
                "HTTP {Method} {Path} completed {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "HTTP {Method} {Path} failed with exception in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds
            );
            throw; // El ExceptionHandler de ASP.NET Core se encarga del resto
        }
    }
}
