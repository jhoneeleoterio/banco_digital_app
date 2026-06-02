using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BancoDigital.Infrastructure.Persistence.Repositories;

internal sealed class ContaRepository(AppDbContext context) : IContaRepository
{
    public async Task<Conta?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Contas
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Conta>> ObterPorIdsComLockAsync(Guid id1, Guid id2, CancellationToken ct = default)
    {
        // FOR UPDATE garante lock pessimista para evitar race condition em transferências concorrentes.
        var idParam1 = id1.ToString();
        var idParam2 = id2.ToString();

        var contas = await context.Contas
            .FromSqlRaw(
                "SELECT * FROM contas WHERE id IN ({0}::uuid, {1}::uuid) ORDER BY id FOR UPDATE",
                idParam1,
                idParam2)
            .ToListAsync(ct);

        return contas.AsReadOnly();
    }

    public async Task AdicionarAsync(Cliente cliente, Conta conta, CancellationToken ct = default)
    {
        context.Clientes.Add(cliente);
        context.Contas.Add(conta);
        await context.SaveChangesAsync(ct);
    }

    public async Task<string> GerarProximoNumeroAsync(CancellationToken ct = default)
    {
        var count = await context.Contas.CountAsync(ct);
        var sequencial = (count + 1).ToString("D4");
        var digito = (count + 1) % 10;
        return $"{sequencial}-{digito}";
    }
}
