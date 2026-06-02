using AwesomeAssertions;
using BancoDigital.Application.UseCases.Contas;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Repositories;
using NSubstitute;

namespace BancoDigital.UnitTests.Application;

public sealed class CriarContaUseCaseTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly CriarContaUseCase _sut;

    public CriarContaUseCaseTests()
    {
        _sut = new CriarContaUseCase(_contaRepository);
        _contaRepository.GerarProximoNumeroAsync(Arg.Any<CancellationToken>()).Returns("0001-1");
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_RetornaContaComSaldoInicial()
    {
        // Arrange
        var request = new CriarContaRequest("Maria Souza", 2500m);

        // Act
        var resultado = await _sut.ExecutarAsync(request);

        // Assert
        resultado.Saldo.Should().Be(2500m);
        resultado.NomeCliente.Should().Be("Maria Souza");
        resultado.Numero.Should().Be("0001-1");
        resultado.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_PersistirClienteEConta()
    {
        // Arrange
        var request = new CriarContaRequest("Pedro Alves", 1000m);

        // Act
        await _sut.ExecutarAsync(request);

        // Assert
        await _contaRepository.Received(1)
            .AdicionarAsync(
                Arg.Is<Cliente>(c => c.Nome == "Pedro Alves"),
                Arg.Is<Conta>(c => c.Saldo == 1000m),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_SaldoZero_CriaContaComSaldoZero()
    {
        // Arrange
        var request = new CriarContaRequest("Ana Lima", 0m);

        // Act
        var resultado = await _sut.ExecutarAsync(request);

        // Assert
        resultado.Saldo.Should().Be(0m);
    }
}
