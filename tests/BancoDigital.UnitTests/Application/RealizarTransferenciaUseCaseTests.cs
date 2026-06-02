using AwesomeAssertions;
using BancoDigital.Application.Services;
using BancoDigital.Application.UseCases.Transferencias;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ValidationException = BancoDigital.Domain.Exceptions.ValidationException;

namespace BancoDigital.UnitTests.Application;

public sealed class RealizarTransferenciaUseCaseTests
{
    private readonly IContaRepository _contaRepository = Substitute.For<IContaRepository>();
    private readonly ITransferenciaRepository _transferenciaRepository = Substitute.For<ITransferenciaRepository>();
    private readonly INotificacaoService _notificacaoService = Substitute.For<INotificacaoService>();
    private readonly RealizarTransferenciaUseCase _sut;

    public RealizarTransferenciaUseCaseTests()
    {
        _sut = new RealizarTransferenciaUseCase(
            _contaRepository,
            _transferenciaRepository,
            _notificacaoService,
            NullLogger<RealizarTransferenciaUseCase>.Instance);
    }

    private static Conta CriarConta(decimal saldo) =>
        Conta.Criar(Guid.NewGuid(), $"{Guid.NewGuid():N}"[..4] + "-1", saldo);

    // ── Fluxo feliz ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecutarAsync_TransferenciaValida_DebitaECreditaContas()
    {
        // Arrange
        var origem = CriarConta(1000m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 300m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        // Act
        var resultado = await _sut.ExecutarAsync(request);

        // Assert
        origem.Saldo.Should().Be(700m);
        destino.Saldo.Should().Be(500m);
        resultado.Status.Should().Be(StatusTransferencia.Concluida);
    }

    [Fact]
    public async Task ExecutarAsync_TransferenciaValida_PersistirTransferencia()
    {
        // Arrange
        var origem = CriarConta(1000m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 100m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        // Act
        await _sut.ExecutarAsync(request);

        // Assert
        await _transferenciaRepository.Received(1)
            .AdicionarAsync(Arg.Is<Transferencia>(t =>
                t.ContaOrigemId == origem.Id &&
                t.ContaDestinoId == destino.Id &&
                t.Valor == 100m &&
                t.Status == StatusTransferencia.Concluida),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_TransferenciaValida_NotificacaoDisparadaAposCommit()
    {
        // Arrange
        var origem = CriarConta(1000m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 50m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        // Act
        await _sut.ExecutarAsync(request);

        // Assert
        await _notificacaoService.Received(1)
            .NotificarAsync(origem.Id, destino.Id, 50m, Arg.Any<CancellationToken>());
    }

    // ── Validações de entrada ────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecutarAsync_ValorNaoPositivo_LancaValidationException(decimal valor)
    {
        // Arrange
        var request = new RealizarTransferenciaRequest(Guid.NewGuid(), Guid.NewGuid(), valor);

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert
        await acao.Should().ThrowExactlyAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecutarAsync_MesmaConta_LancaDomainException()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var request = new RealizarTransferenciaRequest(contaId, contaId, 100m);

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert
        await acao.Should().ThrowExactlyAsync<DomainException>()
            .WithMessage("*não podem ser iguais*");
    }

    // ── Conta não encontrada ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecutarAsync_ContaOrigemNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var destino = CriarConta(500m);
        var request = new RealizarTransferenciaRequest(Guid.NewGuid(), destino.Id, 100m);

        _contaRepository.ObterPorIdsComLockAsync(Arg.Any<Guid>(), destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { destino });

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert
        await acao.Should().ThrowExactlyAsync<NotFoundException>()
            .WithMessage("*origem*");
    }

    [Fact]
    public async Task ExecutarAsync_ContaDestinoNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var origem = CriarConta(500m);
        var request = new RealizarTransferenciaRequest(origem.Id, Guid.NewGuid(), 100m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem });

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert
        await acao.Should().ThrowExactlyAsync<NotFoundException>()
            .WithMessage("*destino*");
    }

    // ── Regra de domínio ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecutarAsync_SaldoInsuficiente_LancaDomainException()
    {
        // Arrange
        var origem = CriarConta(50m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 500m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert
        await acao.Should().ThrowExactlyAsync<DomainException>()
            .WithMessage("*Saldo insuficiente*");
    }

    [Fact]
    public async Task ExecutarAsync_SaldoInsuficiente_PersisteFalha()
    {
        // Arrange
        var origem = CriarConta(50m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 500m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        // Act
        try { await _sut.ExecutarAsync(request); } catch { /* esperado */ }

        // Assert
        await _transferenciaRepository.Received(1)
            .AdicionarAsync(Arg.Is<Transferencia>(t => t.Status == StatusTransferencia.Falha),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_FalhaNotificacao_NaoPropagaErro()
    {
        // Arrange
        var origem = CriarConta(1000m);
        var destino = CriarConta(200m);
        var request = new RealizarTransferenciaRequest(origem.Id, destino.Id, 100m);

        _contaRepository.ObterPorIdsComLockAsync(origem.Id, destino.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Conta> { origem, destino });

        _notificacaoService.NotificarAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Serviço de notificação indisponível"));

        // Act
        var acao = async () => await _sut.ExecutarAsync(request);

        // Assert — falha na notificação não propaga (CONSTITUTION §1.1)
        await acao.Should().NotThrowAsync();
    }
}
