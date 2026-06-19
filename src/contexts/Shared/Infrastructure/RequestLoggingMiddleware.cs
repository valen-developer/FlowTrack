using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace FlowTrack.Shared.Infrastructure;

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

        logger.Info(new LogMessage(Action: "Http Request", Message: $"{method} {path} started"));

        try
        {
            var originalBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body = originalBody;
            responseBody.Seek(0, SeekOrigin.Begin);
            var bodyText = new StreamReader(responseBody).ReadToEnd();
            await responseBody.CopyToAsync(originalBody);

            sw.Stop();
            logger.Info(
                new LogMessage(
                    Action: "Http Response",
                    Message: $"{context.Response.StatusCode} {method} {path} completed in {sw.ElapsedMilliseconds}ms",
                    Attributes: new
                    {
                        HttpResponse = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Method = method,
                            Path = path,
                            ElapsedMs = sw.ElapsedMilliseconds,
                            Body = bodyText,
                        },
                    }
                )
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.Error(
                new LogMessage(
                    Action: "Http Request",
                    Message: $"{method} {path} failed in {sw.ElapsedMilliseconds}ms",
                    Attributes: new
                    {
                        Method = method,
                        Path = path,
                        ElapsedMs = sw.ElapsedMilliseconds,
                    }
                ),
                ex
            );
            throw;
        }
    }
}
