namespace BancoDigital.Application.Services;

public interface INotificacaoService
{
    Task NotificarAsync(Guid contaOrigemId, Guid contaDestinoId, decimal valor, CancellationToken ct = default);
}
