FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY BancoDigital.slnx ./
COPY src/BancoDigital.Domain/BancoDigital.Domain.csproj src/BancoDigital.Domain/
COPY src/BancoDigital.Application/BancoDigital.Application.csproj src/BancoDigital.Application/
COPY src/BancoDigital.Infrastructure/BancoDigital.Infrastructure.csproj src/BancoDigital.Infrastructure/
COPY src/BancoDigital.Api/BancoDigital.Api.csproj src/BancoDigital.Api/
COPY Directory.Build.props ./
COPY global.json ./

RUN dotnet restore src/BancoDigital.Api/BancoDigital.Api.csproj

COPY src/ src/

RUN dotnet publish src/BancoDigital.Api/BancoDigital.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BancoDigital.Api.dll"]
