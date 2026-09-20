using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace StoreManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (KeyNotFoundException ex) { await WriteError(context, HttpStatusCode.NotFound, ex.Message); }
        catch (ArgumentException ex) { await WriteError(context, HttpStatusCode.BadRequest, ex.Message); }
        catch (UnauthorizedAccessException ex) { await WriteError(context, HttpStatusCode.Forbidden, ex.Message); }
        catch (DbUpdateConcurrencyException) { await WriteError(context, HttpStatusCode.Conflict, "The record changed while you were editing it. Refresh and retry."); }
        catch (InvalidOperationException ex) { await WriteError(context, HttpStatusCode.Conflict, ex.Message); }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error.");
            await WriteError(context, HttpStatusCode.Conflict, "The operation could not be saved. Check for duplicate or conflicting data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled store management API error.");
            await WriteError(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteError(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message, traceId = context.TraceIdentifier }));
    }
}
