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

    public async Task InvokeAsync(HttpContext context, IDomainLogger logger)
    {
        // 1. Tomar o generar CorrelationId
        var correlationId =
            CorrelationContext.Get()
            ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? CorrelationContext.Generate();

        CorrelationContext.Set(correlationId);

        // 2. Scope de Serilog para enriquecer todos los logs de esta request
        using var _ = LogContext.PushProperty("CorrelationId", correlationId);

        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        logger.Info(new LogMessage(
            Action: "Http Request",
            Message: $"{method} {path} started"
        ));

        try
        {
            await _next(context);

            sw.Stop();
            logger.Info(new LogMessage(
                Action: "Http Request",
                Message: $"{context.Response.StatusCode} {method} {path} completed in {sw.ElapsedMilliseconds}ms",
                Attributes: new
                {
                    Method = method,
                    Path = path,
                    StatusCode = context.Response.StatusCode,
                    ElapsedMs = sw.ElapsedMilliseconds
                }
            ));
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.Error(new LogMessage(
                Action: "Http Request",
                Message: $"{method} {path} failed in {sw.ElapsedMilliseconds}ms",
                Attributes: new
                {
                    Method = method,
                    Path = path,
                    ElapsedMs = sw.ElapsedMilliseconds
                }
            ), ex);
            throw;
        }
    }
}
