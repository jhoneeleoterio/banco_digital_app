using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;

namespace BancoDigital.Application.UseCases.Contas;

public sealed class ObterExtratoUseCase(
    IContaRepository contaRepository,
    ITransferenciaRepository transferenciaRepository)
{
    public async Task<ExtratoResponse> ExecutarAsync(Guid contaId, CancellationToken ct = default)
    {
        var conta = await contaRepository.ObterPorIdAsync(contaId, ct);

        if (conta is not null)
        {
            var transferencias = await transferenciaRepository.ObterPorContaIdAsync(contaId, ct);

            var movimentacoes = transferencias
                .OrderByDescending(t => t.RealizadaEm)
                .Select(t => MapearMovimentacao(t, contaId))
                .ToList();

            return new ExtratoResponse(conta.Id, conta.Numero, conta.Saldo, movimentacoes);
        }

        throw new NotFoundException($"Conta com id '{contaId}' não encontrada.");
    }

    private static MovimentacaoResponse MapearMovimentacao(Transferencia transferencia, Guid contaId)
    {
        if (transferencia.ContaOrigemId == contaId)
        {
            return new MovimentacaoResponse(
                "Debito",
                transferencia.Valor,
                "Transferência enviada",
                transferencia.RealizadaEm);
        }

        return new MovimentacaoResponse(
            "Credito",
            transferencia.Valor,
            "Transferência recebida",
            transferencia.RealizadaEm);
    }
}
