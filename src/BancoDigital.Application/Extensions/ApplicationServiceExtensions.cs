using BancoDigital.Application.UseCases.Contas;
using BancoDigital.Application.UseCases.Transferencias;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDigital.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CriarContaUseCase>();
        services.AddScoped<ObterContaUseCase>();
        services.AddScoped<ObterExtratoUseCase>();
        services.AddScoped<RealizarTransferenciaUseCase>();
        services.AddScoped<ObterTransferenciaUseCase>();

        return services;
    }
}
