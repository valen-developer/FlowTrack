using FlowTrack.Shared.Domain.Bus;
using FlowTrack.Shared.Domain.Exception;
using FlowTrack.Shared.Infrastructure.HttpErrorResponses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FlowTrack.Shared.Infrastructure;

public record ApplicationOptions(
    string ApplicationName,
    bool WithLogger = false,
    bool WithAuthentication = false
);

public class Application
{
    private readonly WebApplication _app;
    private readonly ApplicationOptions _options;

    public Application(WebApplication app, ApplicationOptions options)
    {
        _app = app;
        _options = options;
        ConfigApplication();
    }

    private void ConfigApplication()
    {
        if (_options.WithLogger)
        {
            _app.UseMiddleware<RequestLoggingMiddleware>();
        }

        if (_options.WithAuthentication)
        {
            _app.UseAuthentication();
            _app.UseAuthorization();
        }

        _app.UseExceptionHandler(handler =>
        {
            handler.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<IDomainLogger>();

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;
                if (exception == null)
                    return;

                var httpStatusCode = 500;
                var httpErrorResponse = new HttpErrorResponse(
                    "Internal Server Error",
                    "exception.internal.server"
                );

                if (exception is ActionHandlerExecutionException actionHandlerExecutionException)
                {
                    var cause = actionHandlerExecutionException.GetCause();
                    if (cause is DomainException ex)
                    {
                        var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(ex);
                        httpStatusCode = statusCode;
                        httpErrorResponse = httpErrorException;
                    }
                }

                if (exception is DomainException domainException)
                {
                    var (statusCode, httpErrorException) = DomainToHttpExceptionMapper.Map(
                        domainException
                    );
                    httpStatusCode = statusCode;
                    httpErrorResponse = httpErrorException;
                }

                logger.Error(
                    new LogMessage(
                        Action: "ExceptionHandler",
                        Message: "An exception occurred",
                        Attributes: httpErrorResponse
                    ),
                    exception
                );

                context.Response.StatusCode = httpStatusCode;
                await context.Response.WriteAsJsonAsync(httpErrorResponse);
            });
        });

        _app.MapControllers();

        if (_app.Environment.IsDevelopment())
        {
            _app.MapOpenApi();
        }

        _app.UseHttpsRedirection();
    }

    public void UseMiddleware<T>()
        where T : class
    {
        _app.UseMiddleware<T>();
    }

    public async Task Run()
    {
        try
        {
            await _app.RunAsync();
        }
        catch
        {
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
