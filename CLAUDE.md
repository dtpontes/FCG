# FCG — Guia do repositório para o Claude Code

> **O que é:** API .NET 9 / C# 13 para gestão de **clientes** e **usuários**, com
> arquitetura em camadas, EF Core + SQL Server, autenticação JWT e SDD (Spec Kit).
> **Última verificação no código:** 2026-07-15.
>
> Este arquivo é o guia **curto** do repositório (mapa + regras). O processo de
> desenvolvimento é **Spec-Driven Development** — a constituição do projeto vive em
> [`.specify/memory/constitution.md`](.specify/memory/constitution.md) e **prevalece**
> sobre este guia em caso de conflito de princípios.

---

## 0. Bootstrap — toda sessão

1. Leia este arquivo inteiro (é curto).
2. Leia a **constituição** em `.specify/memory/constitution.md` — são os princípios que regem tudo.
3. **Nenhuma feature sem spec aprovada** (§6). Se a tarefa é uma feature, comece pelo fluxo SDD.
4. Antes de mudar comportamento de backend, leia `src/FCG.Presentation/Program.cs` e o
   `appsettings.{Ambiente}.json` do serviço.
5. **Respeite o padrão já existente no repo** (§4) — não introduza estilos novos sem decisão explícita.

---

## 1. O que este repositório É e o que NÃO é

**É** uma API de **clientes (`Client`)** e **usuários (`User`/Identity)**: cadastro/consulta de
clientes, autenticação (login, registro, reset de senha) e autorização por papel.

> ⚠️ **Games NÃO faz parte deste projeto.** Existe uma entidade/serviço `Game` residual no
> código, mas a **manutenção de jogos pertence a OUTRO projeto**. Não construa features de
> games aqui; não trate `Game` como domínio ativo deste repositório.

### O que NÃO existe (evite supor)
- ❌ Mensageria (RabbitMQ/Kafka) · ❌ gRPC · ❌ microserviços — é um **único serviço em camadas**.
- ❌ OAuth externo/SSO — auth é **JWT próprio + ASP.NET Identity**.
- ❌ CQRS/MediatR-como-pipeline — MediatR aqui é só **notificações de domínio** (não commands/queries).

---

## 2. Stack

| Camada | Tecnologia |
|---|---|
| Runtime | **.NET 9 / C# 13** (sem `global.json`) |
| API | ASP.NET Core **MVC Controllers** (`api/[controller]`) + GraphQL + Swagger |
| ORM / Banco | **EF Core** + **SQL Server** (`AppDbContext`, migrations em `FCG.Infrastructure`) |
| Auth | **JWT Bearer** + ASP.NET Identity + BCrypt; papéis **`Admin`/`User`** |
| Mapeamento | **AutoMapper** (profiles em `FCG.Service/AutoMapper`) |
| Validação | **FluentValidation** (`FCG.Service/DTO/Validator`) |
| Mediação | **MediatR** — apenas notificações de domínio (`DomainNotification`) |
| Logging | **Serilog** (request logging global) + New Relic |
| Testes | **xUnit + Moq** (**sem FluentAssertions**) |
| Deploy | Docker/Docker Compose · GitHub Actions (`.github/workflows/main.yml`) · Azure Web App |

---

## 3. Arquitetura em camadas (regra de dependência)

```
FCG.Presentation ──> FCG.Service ──> FCG.Domain
        │                 │
        └─────────────────┴──> FCG.Infrastructure (implementa interfaces do Domain)
```

- **`FCG.Domain`** — entidades (`Client`, `Base`), interfaces (`IRepository<T>`, `IUnitOfWork`,
  `IClientRepository`), notificações. **NÃO pode depender de EF Core / Infrastructure.**
- **`FCG.Infrastructure`** — `AppDbContext`, migrations, repositórios (`Repository<T>`,
  `ClientRepository`), `UnitOfWork`.
- **`FCG.Service`** — serviços de aplicação (`ClientService`, `UserService`), DTOs (Request/Response/
  Validator), AutoMapper. Depende só de interfaces do Domain.
- **`FCG.Presentation`** — Controllers (finos), `BaseController`, middlewares, `Program.cs`,
  configurações (`Configuration/*.cs`).
- **`tests/FCG.Service.Tests`** — testes de serviço.

---

## 4. Padrões do repositório (siga estes)

- **Controllers finos** herdam de `BaseController` e delegam ao serviço. Retorno via envelope:
  - `Response(entity)` → `200 Ok` com `ResponseObject.Succeed(entity)`; se `entity` for **null**
    (e sem notificações) → **`404 NotFound`**.
  - `Response()` → `200` se transação válida; se houver notificações → **`400`** com os erros.
  - Ou seja: para "não encontrado", o serviço retorna **null** (sem notificação) e o controller
    faz `Response(result)`.
- **Acesso a dados via `IUnitOfWork`** (`_unitOfWork.Clients`, `.SaveChangesAsync()`) + repositório.
  Repositório genérico `Repository<T>` cobre CRUD simples.
- **AutoMapper** para mapear entidade → DTO (ex.: `Client.User.Email` → `Email` no profile).
- **FluentValidation** para validar DTOs de entrada.
- **Regras de negócio no Domain/Service**; controllers não contêm regra.
- **Autorização por papel** com `[Authorize(Roles = "Admin")]` (auth já configurada no pipeline).

> **Consulta com dados relacionados (eager loading):** o `Repository<T>` genérico **não** faz
> `Include`. Para carregar navegações (ex.: `Client.User` para expor o e-mail), crie um
> **repositório especializado** (`IClientRepository : IRepository<Client>`) cuja implementação
> **injeta o `AppDbContext`** e usa `.Include(...)`. **Não** altere o `Repository<T>` genérico
> (seus campos `_context`/`_dbSet` são `private` de propósito).

---

## 5. Convenções de API

- Rotas: `api/[controller]` (ex.: `api/Client`, `api/Users`). Versão simples, sem versionamento de rota.
- Envelope de resposta: `ResponseObject<T>` (`Succeed`/`Fail`) — ver `FCG.Presentation/Models`.
- Status: `200` sucesso · `400` validação/notificação · `404` não encontrado · `401/403` auth.
- Auth por superfície: JWT Bearer; operações sensíveis exigem papel **`Admin`**.
- Documente endpoints com XML doc + `[ProducesResponseType]` (aparecem no Swagger).

---

## 6. Spec-Driven Development (SDD) — o processo

**Nenhuma feature sem spec aprovada.** O Spec Kit está instalado (`.specify/`, comandos
`/speckit-*` em `.claude/skills`).

**Ciclo:** `/speckit-constitution` → `/speckit-specify` → `/speckit-clarify` → `/speckit-plan`
→ `/speckit-checklist` (opc.) → `/speckit-tasks` → `/speckit-analyze` → `/speckit-implement`
→ `/speckit-converge`.

- As specs vivem em **`specs/NNN-nome-da-feature/`** (`spec.md`, `plan.md`, `tasks.md`, `contracts/`).
- `spec.md` = **o QUÊ e o PORQUÊ** (sem solução técnica). `plan.md` = **o COMO**.
- **Sempre rode `/speckit-analyze` antes do `implement`** — ele pega inconsistências e
  bloqueadores de compilação cross-artefato antes de codar.
- Ambiguidade → `/speckit-clarify`. **Nunca suponha.**
- Ao terminar, **verifique de verdade**: `dotnet build`, `dotnet test`, e exercite os endpoints
  no Swagger (com token **Admin** quando a rota exigir).

---

## 7. Convenções de Git

- Branch default: **`master`**. **Nunca commite direto na `master`.**
- Trabalho de spec sai da `master` numa branch por feature: `NNN-nome-da-feature`
  (ex.: `001-consulta-clientes-api`).
- Commits: `feat(spec-NNN): descrição` · `fix(spec-NNN): ...` · `chore: ...`.
- Ao concluir: abrir **PR** para a `master` (o CI roda build + testes).
- Versione **`specs/`** e **`.specify/`** (registro da intenção + motor do SDD).
- **Nunca** fechar issue, deletar branch ou dar merge sem confirmação do desenvolvedor.

---

## 8. Testes

- **xUnit + Moq**, projeto `tests/FCG.Service.Tests`. **Não** use FluentAssertions — use `Assert.*`.
- Nomeie: **`Metodo_Cenario_Resultado`** (ex.: `GetClientByIdAsync_QuandoClienteNaoExiste_RetornaNulo`).
- Cubra regras de negócio no nível de serviço: caminho feliz, vazio e erro/não encontrado.
- Ao mudar a assinatura de dependências (ex.: tipo em `IUnitOfWork`), **atualize os mocks dos
  testes existentes** — garanta zero regressões.

---

## 9. O que o Claude NUNCA deve fazer

- ❌ Construir features de **games** aqui (é de outro projeto).
- ❌ Implementar sem spec aprovada · supor regra ambígua (pergunte / `/speckit-clarify`).
- ❌ Trocar o estilo do repo: **MVC → Minimal APIs**, **SQL Server → outro banco**, ou transformar
  o MediatR em pipeline CQRS — sem decisão explícita.
- ❌ Fazer o `Domain` depender de `Infrastructure`/EF Core.
- ❌ Colocar regra de negócio em controller.
- ❌ Alterar o `Repository<T>` genérico para resolver um caso pontual — prefira repositório especializado.
- ❌ Commitar direto na `master` ou commitar segredos.

---

## 10. Dívida técnica / pegadinhas conhecidas

- **Papel em minúsculo:** `ClientService.CreateClient` cria o usuário com o papel `"user"`
  (minúsculo), enquanto os papéis semeados são `"Admin"`/`"User"` (`IdentityConfiguration`).
  Atenção a checagens de papel sensíveis a caixa. (Não corrigir sem spec.)
- **DTO reaproveitado:** `RegisterClientResponseDto` (nome sugere cadastro) é usado também na
  **consulta** de clientes. Funciona (tem `Id`/`Name`/`Email`), mas é acoplamento — se um dia o
  cadastro ganhar campos, a consulta vaza junto. Considerar `ClientResponseDto` dedicado no futuro.
- **Docker build cache:** já houve caso de `docker compose up --build` reutilizar layer velho e
  "esconder" mudanças de código. Se o comportamento parecer defasado, rebuild com `--no-cache`.
- **Endpoints sem `[Authorize]`:** por padrão os controllers herdados não exigem auth; a proteção
  é por atributo explícito. Ao expor dado sensível (ex.: base de clientes/PII), **exija papel `Admin`**.

---

## 11. Decisions Log

### [2026-07-15] Adoção de SDD (Spec Kit) + primeira feature
**Decisão:** projeto passa a usar Spec-Driven Development (Spec Kit instalado). Primeira feature
entregue pelo fluxo: **Consulta de Clientes via API** (`specs/001-consulta-clientes-api/`),
listar + buscar por id, restrita a `Admin`.
**Impacto:** toda feature nova passa pelo gate de spec; specs versionadas em `specs/`.

<!-- Mantenha este arquivo curto. Princípios profundos → .specify/memory/constitution.md -->
