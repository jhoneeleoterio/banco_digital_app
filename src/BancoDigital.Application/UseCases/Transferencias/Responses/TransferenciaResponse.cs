using BancoDigital.Domain.Entities;

namespace BancoDigital.Application.UseCases.Transferencias.Responses;

public sealed record TransferenciaResponse(
    Guid Id,
    decimal Valor,
    DateTimeOffset RealizadaEm,
    StatusTransferencia Status);
