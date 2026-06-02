using BancoDigital.Application.UseCases.Transferencias;
using Microsoft.AspNetCore.Mvc;

namespace BancoDigital.Api.Endpoints;

public static class TransferenciasEndpoints
{
    public static IEndpointRouteBuilder MapTransferencias(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transferencias").WithTags("Transferências");

        group.MapPost("/", RealizarTransferencia)
             .WithSummary("Realiza uma transferência")
             .WithDescription("Transfere um valor entre duas contas de forma atômica.")
             .Produces<TransferenciaResponse>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", ObterTransferencia)
             .WithSummary("Consulta uma transferência")
             .WithDescription("Retorna os detalhes de uma transferência pelo seu identificador.")
             .Produces<TransferenciaResponse>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> RealizarTransferencia(
        RealizarTransferenciaRequest request,
        RealizarTransferenciaUseCase useCase,
        CancellationToken ct)
    {
        var transferencia = await useCase.ExecutarAsync(request, ct);
        return TypedResults.Created($"/transferencias/{transferencia.Id}", transferencia);
    }

    private static async Task<IResult> ObterTransferencia(
        Guid id,
        ObterTransferenciaUseCase useCase,
        CancellationToken ct)
    {
        var transferencia = await useCase.ExecutarAsync(id, ct);
        return TypedResults.Ok(transferencia);
    }
}
