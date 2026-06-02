using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;

namespace BancoDigital.Application.UseCases.Contas;

public sealed class ObterContaUseCase(IContaRepository contaRepository)
{
    public async Task<ContaResponse> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var conta = await contaRepository.ObterPorIdAsync(id, ct);

        if (conta is null)
        {
            throw new NotFoundException($"Conta com id '{id}' não encontrada.");
        }

        return new ContaResponse(conta.Id, conta.Numero, conta.Saldo, conta.ClienteId, conta.Cliente.Nome);
    }
}
