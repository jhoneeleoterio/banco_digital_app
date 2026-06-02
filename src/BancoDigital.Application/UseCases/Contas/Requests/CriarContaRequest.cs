namespace BancoDigital.Application.UseCases.Contas.Requests;

public sealed record CriarContaRequest(string NomeCliente, decimal SaldoInicial);
