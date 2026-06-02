using AwesomeAssertions;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Exceptions;

namespace BancoDigital.UnitTests.Domain;

public sealed class ContaTests
{
    private static Conta CriarContaComSaldo(decimal saldo) =>
        Conta.Criar(Guid.NewGuid(), "0001-1", saldo);

    // ── Debitar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Debitar_SaldoSuficiente_ReducSaldo()
    {
        // Arrange
        var conta = CriarContaComSaldo(1000m);

        // Act
        conta.Debitar(300m);

        // Assert
        conta.Saldo.Should().Be(700m);
    }

    [Fact]
    public void Debitar_SaldoExato_ZeraSaldo()
    {
        // Arrange
        var conta = CriarContaComSaldo(500m);

        // Act
        conta.Debitar(500m);

        // Assert
        conta.Saldo.Should().Be(0m);
    }

    [Fact]
    public void Debitar_SaldoInsuficiente_LancaDomainException()
    {
        // Arrange
        var conta = CriarContaComSaldo(100m);

        // Act
        var acao = () => conta.Debitar(200m);

        // Assert
        acao.Should().ThrowExactly<DomainException>()
            .WithMessage("*Saldo insuficiente*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Debitar_ValorNaoPositivo_LancaDomainException(decimal valor)
    {
        // Arrange
        var conta = CriarContaComSaldo(1000m);

        // Act
        var acao = () => conta.Debitar(valor);

        // Assert
        acao.Should().ThrowExactly<DomainException>();
    }

    // ── Creditar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Creditar_ValorPositivo_AumentaSaldo()
    {
        // Arrange
        var conta = CriarContaComSaldo(500m);

        // Act
        conta.Creditar(250m);

        // Assert
        conta.Saldo.Should().Be(750m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Creditar_ValorNaoPositivo_LancaDomainException(decimal valor)
    {
        // Arrange
        var conta = CriarContaComSaldo(500m);

        // Act
        var acao = () => conta.Creditar(valor);

        // Assert
        acao.Should().ThrowExactly<DomainException>();
    }

    [Fact]
    public void Debitar_Creditar_ArredondamentoDecimal_Correto()
    {
        // Arrange
        var conta = CriarContaComSaldo(1000.005m);

        // Act
        conta.Debitar(0.005m);

        // Assert
        conta.Saldo.Should().Be(1000.00m);
    }
}
