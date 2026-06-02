# CONSTITUTION.md — Constituição do Projeto

> Este documento define as regras inegociáveis do projeto. Todo código, PR e decisão técnica deve estar em conformidade com os princípios aqui descritos. Em caso de conflito com outras documentações, este arquivo prevalece.

---

## 1. Guard Rails

### 1.1 Regras de Negócio Invioláveis

- **Saldo nunca pode ser negativo.** Qualquer operação que resulte em saldo negativo deve ser rejeitada no domínio antes de chegar ao banco de dados.
- **Transferência é atômica.** Débito e crédito ocorrem na mesma transação de banco de dados. Não existe estado intermediário persistido.
- **Valor de transferência deve ser positivo.** Valores zero ou negativos são rejeitados na camada de validação de entrada.
- **Conta não pode transferir para si mesma.** `contaOrigemId == contaDestinoId` é rejeitado como erro de domínio.
- **Notificação nunca bloqueia a transferência.** Falha no serviço de notificação não causa rollback. O erro deve ser logado, nunca propagado.

### 1.2 Segurança

- **Nunca logar dados sensíveis.** Saldos, valores de transferência e dados pessoais de clientes não devem aparecer em logs de nível `Information` ou superior em produção. Use `Debug` ou `Trace` para fins de diagnóstico local.
- **Nunca expor stack traces em respostas de API.** O middleware de exceções deve sempre retornar `ProblemDetails` sem o campo `exception` em produção (`app.Environment.IsProduction()`).
- **Connection strings via variáveis de ambiente.** Nenhuma credencial de banco de dados, chave de API ou secret pode ser commitada no repositório — nem mesmo em arquivos de exemplo não comentados.
- **IDs internos são GUIDs.** Nunca expor IDs auto-incrementais ou sequenciais na API pública para evitar enumeração de recursos.

### 1.3 Integridade dos Dados

- **Migrations são imutáveis após merge em `main`.** Nunca edite um arquivo de migration já aplicado em produção. Crie uma nova migration para corrigir ou desfazer.
- **Seed é idempotente.** O seed de dados deve verificar existência antes de inserir. Rodar o seed múltiplas vezes não deve duplicar registros.
- **Precisão monetária é `decimal(18,2)`.** Nenhuma operação financeira deve usar `float` ou `double`. Arredondamentos devem usar `MidpointRounding.AwayFromZero`.

### 1.4 Qualidade de Código

- **Cobertura mínima de 80%** nos projetos `Domain` e `Application`. PRs que reduzam a cobertura abaixo desse limite não devem ser mergeados.
- **Zero warnings no build.** O projeto deve compilar com `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` em CI.
- **Sem dependências circulares entre projetos.** A direção das dependências é: `Api → Application → Domain`. `Infrastructure` depende de `Application`. Nenhuma camada interna conhece a camada acima dela.

---

## 2. Acessibilidade

> Embora este projeto seja uma API REST (sem interface visual), acessibilidade se aplica à experiência do desenvolvedor (DX) e à documentação.

### 2.1 Documentação da API (Scalar / OpenAPI)

- **Todo endpoint deve ter `WithSummary` e `WithDescription`** em português claro, sem jargões técnicos desnecessários.
- **Exemplos de request e response são obrigatórios** para todos os endpoints via `WithOpenApi()` e anotações de schema.
- **Mensagens de erro devem ser humanas.** O campo `detail` do `ProblemDetails` deve explicar o problema e, quando possível, sugerir a correção:
  ```
  ✗ "VALIDATION_ERROR_004"
  ✓ "O valor da transferência deve ser maior que zero."
  ```
- **Códigos HTTP devem ser semânticos e documentados.** Cada endpoint deve declarar explicitamente todos os status codes possíveis (`200`, `201`, `400`, `404`, `422`, `500`) com `Produces<T>`.

### 2.2 Experiência do Desenvolvedor

- **README é a porta de entrada.** Qualquer desenvolvedor deve conseguir rodar o projeto do zero seguindo apenas o README, sem conhecimento prévio do repositório.
- **Mensagens de validação são sempre em português.** O público-alvo da API é brasileiro; mensagens em inglês criam barreira desnecessária para consumidores da API.
- **Health check público.** O endpoint `/health` deve estar acessível sem autenticação para facilitar integração com ferramentas de monitoramento e onboarding.
- **Nomes de rotas e campos em camelCase no JSON.** Seguir a convenção padrão de JSON APIs para reduzir a curva de aprendizado de consumidores da API.

### 2.3 Logs e Observabilidade

- **Logs devem ser estruturados** (`ILogger<T>` com message templates, nunca interpolação de strings):
  ```csharp
  // ✗ Evitar
  _logger.LogInformation($"Transferência {id} concluída");

  // ✓ Preferir
  _logger.LogInformation("Transferência {TransferenciaId} concluída", id);
  ```
- **Níveis de log devem ser respeitados:**
  | Nível       | Uso                                               |
  |-------------|---------------------------------------------------|
  | `Trace`     | Diagnóstico muito detalhado (somente dev local)   |
  | `Debug`     | Valores internos úteis para debugging             |
  | `Information` | Eventos de negócio relevantes (transferência iniciada, concluída) |
  | `Warning`   | Situações inesperadas mas recuperáveis            |
  | `Error`     | Falhas que impactam o usuário                     |
  | `Critical`  | Falhas que derrubam o sistema                     |

---

## 3. Convenções de Código

### 3.1 Nomenclatura

| Elemento                    | Convenção         | Exemplo                              |
|-----------------------------|-------------------|--------------------------------------|
| Classes, Records, Enums     | `PascalCase`      | `RealizarTransferenciaUseCase`       |
| Métodos e propriedades      | `PascalCase`      | `ExecutarAsync`, `SaldoAtual`        |
| Campos privados             | `_camelCase`      | `_contaRepository`                   |
| Parâmetros e variáveis      | `camelCase`       | `contaOrigemId`, `valorTransferido`  |
| Interfaces                  | `IPascalCase`     | `IContaRepository`                   |
| Constantes                  | `PascalCase`      | `SaldoMinimo`                        |
| Arquivos                    | `PascalCase`      | `ContaRepository.cs`                 |
| Entidades de domínio        | Português         | `Conta`, `Transferencia`, `Cliente`  |
| Infraestrutura técnica      | Inglês            | `AppDbContext`, `BaseRepository`     |

### 3.2 Estrutura de Arquivos

Cada arquivo deve conter **uma única responsabilidade pública**. Tipos auxiliares pequenos (ex.: enums usados apenas por uma classe) podem coexistir no mesmo arquivo, mas devem ser a exceção.

```
// ✗ Evitar — múltiplas classes públicas no mesmo arquivo
public class Conta { }
public class ContaValidator { }
public class ContaMapper { }

// ✓ Preferir — um arquivo por responsabilidade
Conta.cs
ContaValidator.cs
ContaMapper.cs
```

### 3.3 Use Cases

- Todo use case é uma classe com um único método público: `ExecutarAsync(TRequest request, CancellationToken ct = default)`.
- Use cases não referenciam `HttpContext`, `IActionResult` ou qualquer primitiva HTTP.
- Use cases recebem e retornam Records (DTOs imutáveis), nunca entidades de domínio.

```csharp
// ✓ Estrutura padrão de use case
public sealed class RealizarTransferenciaUseCase(
    IContaRepository contaRepository,
    ITransferenciaRepository transferenciaRepository,
    INotificacaoService notificacaoService)
{
    public async Task<TransferenciaResponse> ExecutarAsync(
        RealizarTransferenciaRequest request,
        CancellationToken ct = default)
    {
        // ...
    }
}
```

### 3.4 Endpoints (Minimal API)

- Endpoints são mapeados em classes estáticas de extensão (`static class ContasEndpoints`) com um método `MapContas(this IEndpointRouteBuilder app)`.
- A lógica de negócio **nunca** fica inline no endpoint — o delegate chama o use case e faz o mapeamento HTTP.
- Retornos devem usar os helpers tipados do `TypedResults`:

```csharp
// ✗ Evitar
app.MapPost("/contas", async (CriarContaRequest req, CriarContaUseCase uc) => {
    var result = await uc.ExecutarAsync(req);
    return Results.Created($"/contas/{result.Id}", result);
});

// ✓ Preferir — extraído em extensão
public static class ContasEndpoints
{
    public static IEndpointRouteBuilder MapContas(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/contas").WithTags("Contas");

        group.MapPost("/", CriarConta)
             .WithSummary("Cria uma nova conta")
             .Produces<ContaResponse>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> CriarConta(
        CriarContaRequest request,
        CriarContaUseCase useCase,
        CancellationToken ct)
    {
        var conta = await useCase.ExecutarAsync(request, ct);
        return TypedResults.Created($"/contas/{conta.Id}", conta);
    }
}
```

### 3.5 Tratamento de Exceções

- **Nunca use `try/catch` para controle de fluxo de negócio.** Exceções de domínio são para situações verdadeiramente excepcionais, não para validação de entrada rotineira.
- **Nunca engula exceções silenciosamente** (`catch (Exception) { }`). Se não for tratar, não capture.
- O `ExceptionHandlingMiddleware` é o único ponto que converte exceções em respostas HTTP.

```csharp
// ✗ Evitar — controle de fluxo via exceção
try { var conta = await _repo.ObterPorIdAsync(id); }
catch (NotFoundException) { return null; }

// ✓ Preferir — verificação explícita
var conta = await _repo.ObterPorIdAsync(id);
if (conta is null) throw new NotFoundException($"Conta {id} não encontrada.");
```

### 3.6 Async/Await

- Todo método de I/O (banco, rede, arquivo) deve ser `async Task<T>`.
- Sempre passar e respeitar `CancellationToken` em métodos públicos de repositórios e use cases.
- Nunca usar `.Result` ou `.Wait()` em código assíncrono — causa deadlocks.
- Preferir `await using` para recursos descartáveis assíncronos.

### 3.7 Injeção de Dependência

- Usar **Primary Constructors** do C# 12+ para injeção de dependência em classes simples.
- Registrar dependências em métodos de extensão separados por camada (`AddApplicationServices`, `AddInfrastructureServices`), nunca diretamente no `Program.cs`.
- Tempo de vida padrão:
  | Tipo                  | Lifetime      |
  |-----------------------|---------------|
  | Use Cases             | `Scoped`      |
  | Repositórios          | `Scoped`      |
  | DbContext             | `Scoped`      |
  | Serviços de notificação | `Scoped`    |

### 3.8 Testes

- Nomenclatura: `Método_Cenário_ResultadoEsperado`
- Estrutura obrigatória: comentários `// Arrange`, `// Act`, `// Assert`
- Um único `Assert` lógico por teste (múltiplos `.Should()` encadeados sobre o mesmo objeto são permitidos)
- Mocks via `NSubstitute` — nunca classes fake manuais, a menos que `NSubstitute` não suporte o caso
- Sem acesso a banco de dados real em testes unitários — usar repositórios mockados

---

## 4. O que Nunca Fazer

Esta seção lista práticas proibidas no projeto, independentemente de justificativa:

| ❌ Proibido                                                    | ✅ Alternativa                                      |
|---------------------------------------------------------------|-----------------------------------------------------|
| `float` ou `double` para valores monetários                   | `decimal` com precisão explícita                    |
| Credenciais hardcoded em qualquer arquivo                     | Variáveis de ambiente ou secrets manager            |
| `Thread.Sleep` em código de produção                          | `await Task.Delay` com `CancellationToken`          |
| Retornar entidades de domínio diretamente nos endpoints       | DTOs/Records mapeados                               |
| Acesso a `HttpContext` fora da camada `Api`                   | Parâmetros explícitos passados pelo endpoint        |
| Migrations editadas após merge em `main`                      | Nova migration para corrigir                        |
| `Console.WriteLine` para logging                              | `ILogger<T>` estruturado                            |
| Stack trace em response body em produção                      | `ProblemDetails` sem campo `exception`              |
| Lógica de negócio em repositórios                             | Use cases e entidades de domínio                    |
| Testes que dependem de ordem de execução                      | Testes completamente isolados                       |
