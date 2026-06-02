using BancoDigital.Application.Services;
using Microsoft.Extensions.Logging;

namespace BancoDigital.Infrastructure.Services;

internal sealed class NotificacaoService(ILogger<NotificacaoService> logger) : INotificacaoService
{
    public async Task NotificarAsync(Guid contaOrigemId, Guid contaDestinoId, decimal valor, CancellationToken ct = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(50), ct);

        logger.LogInformation(
            "Notificação enviada: transferência de {Valor:C} da conta {Origem} para conta {Destino}",
            valor, contaOrigemId, contaDestinoId);
    }
}
