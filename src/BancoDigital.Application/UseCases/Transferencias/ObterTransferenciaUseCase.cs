using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;

namespace BancoDigital.Application.UseCases.Transferencias;

public sealed class ObterTransferenciaUseCase(ITransferenciaRepository transferenciaRepository)
{
    public async Task<TransferenciaResponse> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var transferencia = await transferenciaRepository.ObterPorIdAsync(id, ct);

        if (transferencia is null)
        {
            throw new NotFoundException($"Transferência com id '{id}' não encontrada.");
        }

        return new TransferenciaResponse(
            transferencia.Id,
            transferencia.Valor,
            transferencia.RealizadaEm,
            transferencia.Status);
    }
}
