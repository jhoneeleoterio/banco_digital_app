using AwesomeAssertions;
using BancoDigital.Application.UseCases.Contas;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;
using NSubstitute;

namespace BancoDigital.UnitTests.Application;

public sealed class ObterExtratoUseCaseTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly ITransferenciaRepository _transferenciaRepository = Substitute.For<ITransferenciaRepository>();
    private readonly ObterExtratoUseCase _sut;

    public ObterExtratoUseCaseTests()
    {
        _sut = new ObterExtratoUseCase(_contaRepository, _transferenciaRepository);
    }

    [Fact]
    public async Task ExecutarAsync_ContaNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        _contaRepository.ObterPorIdAsync(contaId, Arg.Any<CancellationToken>()).Returns((Conta?)null);

        // Act
        var acao = async () => await _sut.ExecutarAsync(contaId);

        // Assert
        await acao.Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_ContaComTransferencias_MovimentacoesOrdenadaDecrescente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var conta = Conta.Criar(clienteId, "0001-1", 1000m);
        var outraContaId = Guid.NewGuid();

        var mais_antiga = CriarTransferencia(conta.Id, outraContaId, 100m, DateTimeOffset.UtcNow.AddHours(-2));
        var mais_recente = CriarTransferencia(conta.Id, outraContaId, 200m, DateTimeOffset.UtcNow.AddHours(-1));
        var mais_nova = CriarTransferencia(outraContaId, conta.Id, 50m, DateTimeOffset.UtcNow);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _transferenciaRepository.ObterPorContaIdAsync(conta.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Transferencia> { mais_antiga, mais_recente, mais_nova });

        // Act
        var resultado = await _sut.ExecutarAsync(conta.Id);

        // Assert — movimentações devem estar em ordem decrescente de data
        resultado.Movimentacoes.Should().HaveCount(3);
        resultado.Movimentacoes[0].RealizadaEm.Should().BeAfter(resultado.Movimentacoes[1].RealizadaEm);
        resultado.Movimentacoes[1].RealizadaEm.Should().BeAfter(resultado.Movimentacoes[2].RealizadaEm);
    }

    [Fact]
    public async Task ExecutarAsync_TransferenciaEnviada_MovimentacaoDebito()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var conta = Conta.Criar(clienteId, "0001-1", 1000m);
        var destino = Guid.NewGuid();
        var transferencia = CriarTransferencia(conta.Id, destino, 300m, DateTimeOffset.UtcNow);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _transferenciaRepository.ObterPorContaIdAsync(conta.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Transferencia> { transferencia });

        // Act
        var resultado = await _sut.ExecutarAsync(conta.Id);

        // Assert
        resultado.Movimentacoes[0].Tipo.Should().Be("Debito");
        resultado.Movimentacoes[0].Valor.Should().Be(300m);
    }

    [Fact]
    public async Task ExecutarAsync_TransferenciaRecebida_MovimentacaoCredito()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var conta = Conta.Criar(clienteId, "0001-1", 1000m);
        var origem = Guid.NewGuid();
        var transferencia = CriarTransferencia(origem, conta.Id, 150m, DateTimeOffset.UtcNow);

        _contaRepository.ObterPorIdAsync(conta.Id, Arg.Any<CancellationToken>()).Returns(conta);
        _transferenciaRepository.ObterPorContaIdAsync(conta.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Transferencia> { transferencia });

        // Act
        var resultado = await _sut.ExecutarAsync(conta.Id);

        // Assert
        resultado.Movimentacoes[0].Tipo.Should().Be("Credito");
        resultado.Movimentacoes[0].Valor.Should().Be(150m);
    }

    private static Transferencia CriarTransferencia(
        Guid origemId, Guid destinoId, decimal valor, DateTimeOffset realizadaEm)
    {
        var t = Transferencia.Criar(origemId, destinoId, valor);
        // Usa reflection para ajustar a data nos testes (necessário porque RealizadaEm é private set)
        typeof(Transferencia)
            .GetProperty(nameof(Transferencia.RealizadaEm))!
            .SetValue(t, realizadaEm);
        return t;
    }
}
