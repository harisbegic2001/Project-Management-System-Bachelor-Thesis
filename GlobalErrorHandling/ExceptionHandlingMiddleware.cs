using System.Net;

namespace JWT_Implementation.GlobalErrorHandling;

public class ExceptionHandlingMiddleware
{
       private readonly RequestDelegate _next;


    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the next request delegate on the Http context and catches all unhandled exceptions in the controllers.
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext), "Catastrophic failure! The HttpContext is null.");
        }

        try
        {
            await _next(httpContext);
        }
        catch (ArgumentNullException ex)
        {
            await HandleBadRequestExceptionAsync(httpContext, ex);
        }

        catch (Exception ex)
        {
            await HandleInternalServerErrorAsync(httpContext, ex);
        }
    }

    /// <summary>
    /// Handles unhandled exceptions and returns status code 500.
    /// </summary>
    private static async Task HandleInternalServerErrorAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await httpContext.Response.WriteAsync(new ExceptionDetails
        {
            StatusCode = httpContext.Response.StatusCode,
            Message = "Internal Server Error.",
        }.ToString());
    }

    /// <summary>
    /// Handles argument null and validation exceptions (both are due to bad request) and returns status code 400.
    /// </summary>

    private static async Task HandleBadRequestExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        await httpContext.Response.WriteAsync(new ExceptionDetails
        {
            StatusCode = httpContext.Response.StatusCode,
            Message = exception.Message,
        }.ToString());
    }
    
    
}