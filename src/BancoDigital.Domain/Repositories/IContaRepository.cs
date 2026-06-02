using BancoDigital.Domain.Entities;

namespace BancoDigital.Domain.Repositories;

public interface IContaRepository
{
    Task<Conta?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Conta>> ObterPorIdsComLockAsync(Guid id1, Guid id2, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, Conta conta, CancellationToken ct = default);
    Task<string> GerarProximoNumeroAsync(CancellationToken ct = default);
}
