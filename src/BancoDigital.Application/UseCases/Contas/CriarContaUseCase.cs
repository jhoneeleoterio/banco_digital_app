using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Repositories;

namespace BancoDigital.Application.UseCases.Contas;

public sealed class CriarContaUseCase(IContaRepository contaRepository)
{
    public async Task<ContaResponse> ExecutarAsync(CriarContaRequest request, CancellationToken ct = default)
    {
        var numero = await contaRepository.GerarProximoNumeroAsync(ct);
        var cliente = Cliente.Criar(request.NomeCliente);
        var conta = Conta.Criar(cliente.Id, numero, request.SaldoInicial);

        await contaRepository.AdicionarAsync(cliente, conta, ct);

        return new ContaResponse(conta.Id, conta.Numero, conta.Saldo, cliente.Id, cliente.Nome);
    }
}
