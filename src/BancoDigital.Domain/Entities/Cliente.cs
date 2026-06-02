namespace BancoDigital.Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }

    private Cliente() { Nome = string.Empty; }

    public static Cliente Criar(string nome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome, nameof(nome));
        return new Cliente { Id = Guid.NewGuid(), Nome = nome };
    }
}
