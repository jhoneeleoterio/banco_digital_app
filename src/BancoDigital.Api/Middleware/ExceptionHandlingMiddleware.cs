using BancoDigital.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = BancoDigital.Domain.Exceptions.ValidationException;

namespace BancoDigital.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await EscreverProblemDetails(context, StatusCodes.Status404NotFound, "Recurso não encontrado", ex.Message);
        }
        catch (ValidationException ex)
        {
            await EscreverProblemDetails(context, StatusCodes.Status400BadRequest, "Dados inválidos", ex.Message);
        }
        catch (DomainException ex)
        {
            await EscreverProblemDetails(context, StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao processar requisição {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await EscreverProblemDetails(context, StatusCodes.Status500InternalServerError,
                "Erro interno", "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static async Task EscreverProblemDetails(HttpContext context, int status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
