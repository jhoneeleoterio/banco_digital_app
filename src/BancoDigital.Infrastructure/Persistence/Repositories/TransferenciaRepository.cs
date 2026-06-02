using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BancoDigital.Infrastructure.Persistence.Repositories;

internal sealed class TransferenciaRepository(AppDbContext context) : ITransferenciaRepository
{
    public async Task<Transferencia?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Transferencias.FirstOrDefaultAsync(t => t.Id == id, ct);
    

    public async Task<IReadOnlyList<Transferencia>> ObterPorContaIdAsync(Guid contaId, CancellationToken ct = default)
        => await context.Transferencias
                        .Where(t => t.ContaOrigemId == contaId || t.ContaDestinoId == contaId)
                        .OrderByDescending(t => t.RealizadaEm)
                        .ToListAsync(ct);

    public async Task AdicionarAsync(Transferencia transferencia, CancellationToken ct = default)
    {
        context.Transferencias.Add(transferencia);
        await context.SaveChangesAsync(ct);
    }
}
