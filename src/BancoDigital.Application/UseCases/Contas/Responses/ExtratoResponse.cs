namespace BancoDigital.Application.UseCases.Contas.Response;

public sealed record ExtratoResponse(
    Guid ContaId,
    string Numero,
    decimal SaldoAtual,
    IReadOnlyList<MovimentacaoResponse> Movimentacoes);
