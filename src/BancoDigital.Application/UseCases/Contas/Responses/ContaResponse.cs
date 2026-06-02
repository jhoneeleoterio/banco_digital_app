namespace BancoDigital.Application.UseCases.Contas.Response;

public sealed record ContaResponse(Guid Id, string Numero, decimal Saldo, Guid ClienteId, string NomeCliente);
