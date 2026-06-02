using BancoDigital.Domain.Entities;

namespace BancoDigital.Domain.Repositories;

public interface ITransferenciaRepository
{
    Task<Transferencia?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Transferencia>> ObterPorContaIdAsync(Guid contaId, CancellationToken ct = default);
    Task AdicionarAsync(Transferencia transferencia, CancellationToken ct = default);
}
