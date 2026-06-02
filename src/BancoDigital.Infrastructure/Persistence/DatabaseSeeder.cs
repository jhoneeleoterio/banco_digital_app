using BancoDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BancoDigital.Infrastructure.Persistence;

public sealed class DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
{
    private static readonly string[] NumerosContas = ["0001-1", "0002-3", "0003-5", "0004-7", "0005-9"];
    private static readonly string[] Nomes = ["Ana Lima", "Bruno Costa", "Carlos Mendes", "Diana Rocha", "Eduardo Faria"];
    private static readonly decimal[] Saldos = [5000.00m, 12500.50m, 800.00m, 30000.00m, 1250.75m];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await context.Clientes.AnyAsync(ct))
        {
            return;
        }

        logger.LogInformation("Iniciando seed de dados...");

        for (int i = 0; i < Nomes.Length; i++)
        {
            var cliente = Cliente.Criar(Nomes[i]);
            var conta = Conta.Criar(cliente.Id, NumerosContas[i], Saldos[i]);

            context.Clientes.Add(cliente);
            context.Contas.Add(conta);
        }

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Seed concluído: {Quantidade} contas criadas.", Nomes.Length);
    }
}
