using BancoDigital.Application.Services;
using BancoDigital.Domain.Repositories;
using BancoDigital.Infrastructure.Persistence;
using BancoDigital.Infrastructure.Persistence.Repositories;
using BancoDigital.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BancoDigital.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IContaRepository, ContaRepository>();
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
