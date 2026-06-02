namespace BancoDigital.Application.UseCases.Transferencias.Requests;

public sealed record RealizarTransferenciaRequest(Guid ContaOrigemId, Guid ContaDestinoId, decimal Valor);
