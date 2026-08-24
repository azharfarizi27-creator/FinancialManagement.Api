using System.Net;
using System.Text.Json;
using FinancialManagement.Api.Exceptions;
using FluentValidation;

namespace FinancialManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId = context.TraceIdentifier;
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        object? errors = null;

        switch (exception)
        {
            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                _logger.LogWarning("AppException [{StatusCode}] on {Path}: {Message} (TraceId: {TraceId})",
                    appEx.StatusCode, context.Request.Path, appEx.Message, traceId);
                break;

            case ValidationException valEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Validasi data gagal.";
                errors = valEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                });
                _logger.LogWarning("ValidationException on {Path}: {Errors} (TraceId: {TraceId})",
                    context.Request.Path, JsonSerializer.Serialize(errors), traceId);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Terjadi kesalahan internal pada server.";
                _logger.LogError(exception, "Unhandled error on {Path} (TraceId: {TraceId}): {Message}",
                    context.Request.Path, traceId, exception.Message);
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message,
            errors,
            traceId
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}