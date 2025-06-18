using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BaseWebApp.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next piece of middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // If an exception occurs, log it and handle it
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Create a simpler response for production, and a more detailed one for development
        var response = _env.IsDevelopment()
            ? new { message = exception.Message, details = exception.StackTrace?.ToString() }
            : new { message = "An internal server error has occurred.", details = "Error details are hidden in non-development environments." };
        
        var jsonResponse = JsonSerializer.Serialize(response);
        
        await context.Response.WriteAsync(jsonResponse);
    }
}