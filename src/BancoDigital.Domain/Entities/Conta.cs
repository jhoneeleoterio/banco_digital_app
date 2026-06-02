using BancoDigital.Domain.Exceptions;

namespace BancoDigital.Domain.Entities;

public sealed class Conta
{
    public Guid Id { get; private set; }
    public string Numero { get; private set; }
    public decimal Saldo { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;

    private Conta() { Numero = string.Empty; }

    public static Conta Criar(Guid clienteId, string numero, decimal saldoInicial)
    {
        if (saldoInicial < 0)
        {
            throw new DomainException("O saldo inicial não pode ser negativo.");
        }

        return new Conta
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            Saldo = saldoInicial,
            ClienteId = clienteId
        };
    }

    public void Debitar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new DomainException("O valor do débito deve ser maior que zero.");
        }

        if (Saldo < valor)
        {
            throw new DomainException("Saldo insuficiente para realizar a operação.");
        }

        Saldo = Math.Round(Saldo - valor, 2, MidpointRounding.AwayFromZero);
    }

    public void Creditar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new DomainException("O valor do crédito deve ser maior que zero.");
        }

        Saldo = Math.Round(Saldo + valor, 2, MidpointRounding.AwayFromZero);
    }
}
