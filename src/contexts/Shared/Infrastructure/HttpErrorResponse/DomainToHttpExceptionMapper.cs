using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Infrastructure;

public sealed class DomainToHttpExceptionMapper
{
    public static (int statusCode, HttpErrorResponse) Map(DomainException exception)
    {
        return exception switch
        {
            UnAuthenticatedException => (
                401,
                new HttpErrorResponse(exception.Message, exception.Code)
            ),
            NotFoundException => (404, new HttpErrorResponse(exception.Message, exception.Code)),
            InvalidException => (400, new HttpErrorResponse(exception.Message, exception.Code)),
            InternalException => (500, new HttpErrorResponse(exception.Message, exception.Code)),
            _ => (500, new HttpErrorResponse("Internal Server Error", "exception.internal.server")),
        };
    }
}
