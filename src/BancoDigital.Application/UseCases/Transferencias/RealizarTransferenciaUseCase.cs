using BancoDigital.Application.Services;
using BancoDigital.Domain.Entities;
using BancoDigital.Domain.Exceptions;
using BancoDigital.Domain.Repositories;
using Microsoft.Extensions.Logging;
using ValidationException = BancoDigital.Domain.Exceptions.ValidationException;

namespace BancoDigital.Application.UseCases.Transferencias;

public sealed class RealizarTransferenciaUseCase(
    IContaRepository contaRepository,
    ITransferenciaRepository transferenciaRepository,
    INotificacaoService notificacaoService,
    ILogger<RealizarTransferenciaUseCase> logger)
{
    public async Task<TransferenciaResponse> ExecutarAsync(
        RealizarTransferenciaRequest request,
        CancellationToken ct = default)
    {
        if (request.Valor <= 0)
        {
            throw new ValidationException("O valor da transferência deve ser maior que zero.");
        }

        if (request.ContaOrigemId == request.ContaDestinoId)
        {
            throw new DomainException("A conta de origem e destino não podem ser iguais.");
        }

        var contas = await contaRepository.ObterPorIdsComLockAsync(
            request.ContaOrigemId, request.ContaDestinoId, ct);

        var origem = contas.FirstOrDefault(c => c.Id == request.ContaOrigemId);
        var destino = contas.FirstOrDefault(c => c.Id == request.ContaDestinoId);

        if (origem is null)
        {
            throw new NotFoundException($"Conta de origem '{request.ContaOrigemId}' não encontrada.");
        }

        if (destino is null)
        {
            throw new NotFoundException($"Conta de destino '{request.ContaDestinoId}' não encontrada.");
        }

        var transferencia = Transferencia.Criar(origem.Id, destino.Id, request.Valor);

        try
        {
            origem.Debitar(request.Valor);
            destino.Creditar(request.Valor);

            await transferenciaRepository.AdicionarAsync(transferencia, ct);

            logger.LogInformation(
                "Transferência {TransferenciaId} concluída: conta {Origem} → conta {Destino}",
                transferencia.Id, origem.Id, destino.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var falha = Transferencia.CriarComFalha(request.ContaOrigemId, request.ContaDestinoId, request.Valor);
            await transferenciaRepository.AdicionarAsync(falha, ct);

            logger.LogError(ex,
                "Falha na transferência de conta {Origem} para conta {Destino}",
                request.ContaOrigemId, request.ContaDestinoId);

            throw;
        }

        // Notificação fora do bloco try — falha aqui não reverte a transferência (CONSTITUTION §1.1)
        try
        {
            await notificacaoService.NotificarAsync(origem.Id, destino.Id, request.Valor, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Falha ao notificar transferência {TransferenciaId} — transferência já concluída",
                transferencia.Id);
        }

        return new TransferenciaResponse(transferencia.Id, transferencia.Valor, transferencia.RealizadaEm, transferencia.Status);
    }
}
