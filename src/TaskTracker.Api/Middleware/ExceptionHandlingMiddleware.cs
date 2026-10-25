using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Common;

namespace TaskTracker.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            var (status, title, errors) = ex switch
            {
                ValidationException validation => (StatusCodes.Status400BadRequest, "Validation failed", validation.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray())),
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message, null),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message, null),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message, null),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
            };
            if (status == 500) logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Extensions = { ["errors"] = errors ?? new Dictionary<string, string[]>() } });
        }
    }
}
