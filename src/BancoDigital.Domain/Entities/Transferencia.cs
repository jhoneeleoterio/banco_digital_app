namespace BancoDigital.Domain.Entities;

public sealed class Transferencia
{
    public Guid Id { get; private set; }
    public Guid ContaOrigemId { get; private set; }
    public Guid ContaDestinoId { get; private set; }
    public decimal Valor { get; private set; }
    public DateTimeOffset RealizadaEm { get; private set; }
    public StatusTransferencia Status { get; private set; }

    public Conta ContaOrigem { get; private set; } = null!;
    public Conta ContaDestino { get; private set; } = null!;

    private Transferencia() { }

    public static Transferencia Criar(Guid contaOrigemId, Guid contaDestinoId, decimal valor)
    {
        return new Transferencia
        {
            Id = Guid.NewGuid(),
            ContaOrigemId = contaOrigemId,
            ContaDestinoId = contaDestinoId,
            Valor = valor,
            RealizadaEm = DateTimeOffset.UtcNow,
            Status = StatusTransferencia.Concluida
        };
    }

    public static Transferencia CriarComFalha(Guid contaOrigemId, Guid contaDestinoId, decimal valor)
    {
        return new Transferencia
        {
            Id = Guid.NewGuid(),
            ContaOrigemId = contaOrigemId,
            ContaDestinoId = contaDestinoId,
            Valor = valor,
            RealizadaEm = DateTimeOffset.UtcNow,
            Status = StatusTransferencia.Falha
        };
    }
}
