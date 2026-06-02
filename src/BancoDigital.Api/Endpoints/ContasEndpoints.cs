using BancoDigital.Application.UseCases.Contas;
using Microsoft.AspNetCore.Mvc;

namespace BancoDigital.Api.Endpoints;

public static class ContasEndpoints
{
    public static IEndpointRouteBuilder MapContas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/contas").WithTags("Contas");

        group.MapPost("/", CriarConta)
             .WithSummary("Cria uma nova conta")
             .WithDescription("Cria um novo cliente e uma conta bancária com saldo inicial.")
             .Produces<ContaResponse>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", ObterConta)
             .WithSummary("Consulta uma conta")
             .WithDescription("Retorna os dados e o saldo atual de uma conta.")
             .Produces<ContaResponse>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}/extrato", ObterExtrato)
             .WithSummary("Consulta o extrato")
             .WithDescription("Retorna o saldo atual e todas as movimentações da conta, ordenadas da mais recente para a mais antiga.")
             .Produces<ExtratoResponse>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> CriarConta(
        CriarContaRequest request,
        CriarContaUseCase useCase,
        CancellationToken ct)
    {
        var conta = await useCase.ExecutarAsync(request, ct);
        return TypedResults.Created($"/contas/{conta.Id}", conta);
    }

    private static async Task<IResult> ObterConta(
        Guid id,
        ObterContaUseCase useCase,
        CancellationToken ct)
    {
        var conta = await useCase.ExecutarAsync(id, ct);
        return TypedResults.Ok(conta);
    }

    private static async Task<IResult> ObterExtrato(
        Guid id,
        ObterExtratoUseCase useCase,
        CancellationToken ct)
    {
        var extrato = await useCase.ExecutarAsync(id, ct);
        return TypedResults.Ok(extrato);
    }
}
