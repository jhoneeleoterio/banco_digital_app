# Banco Digital API

API REST para um banco digital simplificado, construída com **.NET 10** e **ASP.NET Core Minimal API**. Permite transferências entre contas, consulta de extrato e notificações pós-transação, com foco em consistência transacional e suporte a alta concorrência.

---

## Sumário

- [Pré-requisitos](#pré-requisitos)
- [Rodando com Docker (recomendado)](#rodando-com-docker-recomendado)
- [Rodando localmente (sem Docker)](#rodando-localmente-sem-docker)
- [Rodando os testes](#rodando-os-testes)
- [Endpoints disponíveis](#endpoints-disponíveis)
- [Exemplos de uso (cURL)](#exemplos-de-uso-curl)
- [Decisões de arquitetura](#decisões-de-arquitetura)
- [Estrutura do projeto](#estrutura-do-projeto)

---

## Pré-requisitos

### Para rodar com Docker
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) 24+

### Para rodar localmente
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/) rodando localmente

---

## Rodando com Docker (recomendado)

Este é o jeito mais rápido de subir tudo sem instalar dependências manualmente.

### 1. Clone o repositório

```bash
git clone https://github.com/<org>/banco-digital-api.git
cd banco-digital-api
```

### 2. Suba os containers

```bash
docker compose up --build
```

Aguarde até ver a mensagem:

```
api  | Application started. Press Ctrl+C to shut down.
```

A API estará disponível em **http://localhost:8080**.

### 3. Acesse a documentação interativa

Abra no navegador: **http://localhost:8080/docs**

### Parando os containers

```bash
docker compose down
```

Para remover também os volumes (apaga os dados do banco):

```bash
docker compose down -v
```

---

## Rodando localmente (sem Docker)

### 1. Clone o repositório

```bash
git clone https://github.com/<org>/banco-digital-api.git
cd banco-digital-api
```

### 2. Configure o banco de dados

Certifique-se de ter um PostgreSQL rodando e crie um banco chamado `banco_digital`:

```sql
CREATE DATABASE banco_digital;
```

### 3. Configure a connection string

Crie o arquivo `src/BancoDigital.Api/appsettings.Development.json` (já está no `.gitignore`):

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=banco_digital;Username=postgres;Password=sua_senha"
  }
}
```

Ou exporte como variável de ambiente:

```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=banco_digital;Username=postgres;Password=sua_senha"
```

### 4. Aplique as migrations e o seed

```bash
dotnet ef database update \
  --project src/BancoDigital.Infrastructure \
  --startup-project src/BancoDigital.Api
```

Isso criará as tabelas e populará o banco com 5 contas de exemplo.

### 5. Rode a API

```bash
dotnet run --project src/BancoDigital.Api
```

A API estará disponível em **https://localhost:5001** (HTTPS) ou **http://localhost:5000** (HTTP).

Documentação interativa: **https://localhost:5001/docs**

---

## Rodando os testes

```bash
dotnet test
```

Para ver o output detalhado:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Para gerar relatório de cobertura:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

O relatório será gerado em `tests/BancoDigital.UnitTests/TestResults/`.

---

## Endpoints disponíveis

### Contas

| Método | Rota                      | Descrição                        |
|--------|---------------------------|----------------------------------|
| `POST` | `/contas`                 | Cria uma nova conta              |
| `GET`  | `/contas/{id}`            | Retorna dados e saldo da conta   |
| `GET`  | `/contas/{id}/extrato`    | Lista movimentações da conta     |

### Transferências

| Método | Rota                      | Descrição                           |
|--------|---------------------------|-------------------------------------|
| `POST` | `/transferencias`         | Realiza transferência entre contas  |
| `GET`  | `/transferencias/{id}`    | Consulta uma transferência          |

### Sistema

| Método | Rota       | Descrição                         |
|--------|------------|-----------------------------------|
| `GET`  | `/health`  | Status de saúde da API e do banco |
| `GET`  | `/docs`    | Documentação interativa (Scalar)  |

---

## Exemplos de uso (cURL)

### Criar uma conta

```bash
curl -X POST http://localhost:5000/contas \
  -H "Content-Type: application/json" \
  -d '{"nomeCliente": "Maria Silva", "saldoInicial": 1500.00}'
```

Resposta (`201 Created`):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "numero": "0006-1",
  "saldo": 1500.00,
  "nomeCliente": "Maria Silva",
  "clienteId": "1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed"
}
```

---

### Consultar uma conta

```bash
curl http://localhost:5000/contas/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

### Realizar uma transferência

```bash
curl -X POST http://localhost:5000/transferencias \
  -H "Content-Type: application/json" \
  -d '{
    "contaOrigemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "contaDestinoId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "valor": 250.00
  }'
```

Resposta (`201 Created`):
```json
{
  "id": "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
  "valor": 250.00,
  "realizadaEm": "2025-06-01T14:30:00Z",
  "status": "Concluida",
  "contaOrigemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "contaDestinoId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

---

### Consultar extrato

```bash
curl http://localhost:5000/contas/3fa85f64-5717-4562-b3fc-2c963f66afa6/extrato
```

Resposta (`200 OK`):
```json
{
  "contaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "numero": "0006-1",
  "saldoAtual": 1250.00,
  "movimentacoes": [
    {
      "tipo": "Debito",
      "valor": 250.00,
      "descricao": "Transferência enviada",
      "realizadaEm": "2025-06-01T14:30:00Z"
    }
  ]
}
```

---

### Exemplo de erro — saldo insuficiente

```bash
curl -X POST http://localhost:5000/transferencias \
  -H "Content-Type: application/json" \
  -d '{"contaOrigemId": "...", "contaDestinoId": "...", "valor": 999999.00}'
```

Resposta (`422 Unprocessable Entity`):
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Saldo insuficiente",
  "status": 422,
  "detail": "A conta de origem não possui saldo suficiente para esta transferência."
}
```

---

## Decisões de arquitetura

### Clean Architecture em camadas

O projeto é dividido em quatro camadas com dependências unidirecionais:

```
Api → Application → Domain
Infrastructure → Application
```

- **Domain**: entidades e regras de negócio puras, sem dependência de frameworks.
- **Application**: use cases, interfaces de repositório e serviços. Orquestra o domínio.
- **Infrastructure**: implementações concretas (EF Core, PostgreSQL, notificações).
- **Api**: endpoints Minimal API, DI, middlewares. Ponto de entrada HTTP.

Essa separação garante que as regras de negócio sejam testáveis de forma isolada e que a troca de banco de dados ou framework não impacte o domínio.

### Domain Model Rico

As regras de negócio vivem nas entidades de domínio, não nos use cases. A entidade `Conta` é responsável por validar se possui saldo suficiente antes de debitar. Isso evita que a mesma regra precise ser duplicada em múltiplos places.

### Lock Pessimista para alta concorrência

Transferências concorrentes para a mesma conta são tratadas com bloqueio pessimista a nível de linha no PostgreSQL (`SELECT FOR UPDATE`). Isso garante que duas transações simultâneas não leiam o mesmo saldo desatualizado e causem inconsistência, sem recorrer a mecanismos de fila externos.

### Transação explícita com commit manual

O `RealizarTransferenciaUseCase` abre uma transação de banco de dados explicitamente, executa débito + crédito + persistência da transferência, faz o commit e **só então** chama o serviço de notificação. Isso garante que uma falha na notificação jamais cause rollback de uma transferência já liquidada.

### Notificação desacoplada via interface

`INotificacaoService` é uma abstração na camada `Application`. A implementação concreta em `Infrastructure` simula o envio com log estruturado. Isso permite trocar a implementação por um serviço real (e-mail, SMS, fila de mensagens) sem alterar nenhuma regra de negócio.

### Minimal API com extensões por recurso

Os endpoints não ficam todos no `Program.cs`. Cada grupo de rotas é mapeado em uma classe de extensão estática dedicada (`ContasEndpoints`, `TransferenciasEndpoints`), mantendo o `Program.cs` enxuto e cada módulo organizado.

### ProblemDetails (RFC 7807) para erros

Todos os erros da API retornam o formato `ProblemDetails` padronizado, com campos `type`, `title`, `status` e `detail`. O middleware `ExceptionHandlingMiddleware` intercepta exceções de domínio e as converte para os status HTTP corretos (400, 404, 422, 500), sem expor stack traces em produção.

---

## Estrutura do projeto

```
banco-digital-api/
├── src/
│   ├── BancoDigital.Api/              # Minimal API, endpoints, Program.cs
│   │   ├── Endpoints/
│   │   │   ├── ContasEndpoints.cs
│   │   │   └── TransferenciasEndpoints.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── BancoDigital.Application/      # Use cases, DTOs, interfaces
│   │   ├── UseCases/
│   │   │   ├── CriarContaUseCase.cs
│   │   │   ├── ObterContaUseCase.cs
│   │   │   ├── ObterExtratoUseCase.cs
│   │   │   └── RealizarTransferenciaUseCase.cs
│   │   ├── DTOs/
│   │   └── Interfaces/
│   │       └── INotificacaoService.cs
│   │
│   ├── BancoDigital.Domain/           # Entidades, exceções de domínio
│   │   ├── Entities/
│   │   │   ├── Cliente.cs
│   │   │   ├── Conta.cs
│   │   │   └── Transferencia.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   └── NotFoundException.cs
│   │   └── Repositories/
│   │       ├── IContaRepository.cs
│   │       └── ITransferenciaRepository.cs
│   │
│   └── BancoDigital.Infrastructure/   # EF Core, repositórios, notificação
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Migrations/
│       │   └── Repositories/
│       └── Services/
│           └── NotificacaoService.cs
│
├── tests/
│   └── BancoDigital.UnitTests/
│       ├── Domain/
│       │   └── ContaTests.cs
│       └── Application/
│           ├── RealizarTransferenciaUseCaseTests.cs
│           └── CriarContaUseCaseTests.cs
│
├── docker-compose.yml
├── Dockerfile
├── CONSTITUTION.md
├── SPEC.md
├── PLAN.md
├── TASK.md
└── README.md
```

---

## Variáveis de ambiente

| Variável                        | Descrição                        | Padrão (desenvolvimento)                            |
|---------------------------------|----------------------------------|-----------------------------------------------------|
| `ConnectionStrings__Default`    | Connection string do PostgreSQL  | `Host=db;Database=banco_digital;Username=postgres;Password=postgres` |
| `ASPNETCORE_ENVIRONMENT`        | Ambiente da aplicação            | `Development`                                       |
| `ASPNETCORE_HTTP_PORTS`         | Porta HTTP da API                | `5000`                                              |
