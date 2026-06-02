namespace BancoDigital.Application.UseCases.Contas.Response;

public sealed record MovimentacaoResponse(
    string Tipo,
    decimal Valor,
    string Descricao,
    DateTimeOffset RealizadaEm);
