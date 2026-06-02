using System.Text.Json.Serialization;
using BancoDigital.Api.Endpoints;
using BancoDigital.Api.Middleware;
using BancoDigital.Application.Extensions;
using BancoDigital.Infrastructure.Extensions;
using BancoDigital.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(connectionString);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference("/docs");

app.MapContas();
app.MapTransferencias();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
