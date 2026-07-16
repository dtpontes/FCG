# Implementation Plan: Consulta de Clientes via API

**Branch**: `001-consulta-clientes-api` | **Date**: 2026-07-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-consulta-clientes-api/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Adicionar duas operações de leitura à API de Clientes já existente: listar todos os clientes
cadastrados (`GET /api/Client`) e consultar um cliente específico por id (`GET /api/Client/{id}`),
retornando id, nome e e-mail do usuário associado. Ambos os endpoints são restritos ao papel
`Admin` (decisão da clarificação de 2026-07-15). A abordagem técnica reaproveita o padrão já
estabelecido para `Game` (métodos `GetAllXAsync`/`GetXByIdAsync` no Service, DTO de resposta
única, `Response(entity)` do `BaseController` para o 404 automático em `null`), com um único
ajuste necessário: os métodos genéricos de `IRepository<T>` não fazem eager-load da navegação
`Client.User`, então será adicionada uma especialização `IClientRepository` (Domain) com
`Include(c => c.User)` na implementação (Infrastructure) para que o mapeamento existente de
`Email` a partir de `User.Email` continue funcionando nos novos endpoints de leitura.

## Technical Context

**Language/Version**: .NET 9 / C# 13

**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core (SQL Server provider),
AutoMapper, MediatR (notificações de domínio), ASP.NET Core Identity + JWT Bearer (autenticação
e papéis), Swashbuckle/Swagger, Serilog

**Storage**: SQL Server via EF Core, `AppDbContext` existente (`src/FCG.Infrastructure/DbContext.cs`);
nenhuma alteração de schema — apenas leitura sobre a tabela `Clients` e `AspNetUsers` já
existentes

**Testing**: xUnit + Moq, projeto `tests/FCG.Service.Tests` (padrão já usado em
`ClientServiceTests.cs` e `GameServiceTests.cs`); sem FluentAssertions (usar `Assert.*` do xUnit)

**Target Platform**: ASP.NET Core Web API hospedada em container Docker (Linux), conforme
`docker-compose.yml`/deploy Azure já existentes

**Project Type**: web-service, solução em camadas (.NET, single repo multi-projeto):
`FCG.Domain` / `FCG.Infrastructure` / `FCG.Service` / `FCG.Presentation` + `FCG.Service.Tests`

**Performance Goals**: Sem meta nova definida pela spec; a feature deve responder dentro do
comportamento padrão já observado nas demais operações síncronas da API (sem otimização
específica exigida, dado o volume atual pequeno e a ausência de paginação — decisão explícita
do usuário)

**Constraints**: `FCG.Domain` NÃO PODE depender de `FCG.Infrastructure`/EF Core (Princípio I);
os dois endpoints DEVEM exigir papel `Admin` (FR-008); nenhuma alteração no domínio de usuários
(`IdentityUser`) é permitida

**Scale/Scope**: Mesma escala atual da base de clientes (uso interno/admin, sem paginação por
decisão explícita do usuário); apenas 2 endpoints de leitura, sem criação/edição/remoção

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Avaliação | Status |
|---|---|---|
| I. Arquitetura em Camadas Respeitada | Nova interface `IClientRepository` fica em `FCG.Domain` (sem referenciar EF Core); implementação com `Include` fica em `FCG.Infrastructure`. `FCG.Service` continua dependendo apenas de `IUnitOfWork`/interfaces do Domain. Nenhuma regra de negócio nova vai para Infrastructure ou Presentation. | PASS |
| II. Spec Antes de Código (SDD) | Spec aprovada e clarificada (`spec.md`, sessão 2026-07-15) antes deste plano; nenhuma ambiguidade de alto impacto pendente. | PASS |
| III. Testes de Regras de Negócio | Novos métodos `GetAllClientsAsync`/`GetClientByIdAsync` em `ClientService` serão cobertos em `FCG.Service.Tests` (lista vazia, id existente, id inexistente) usando a nomenclatura `Metodo_Cenario_Resultado` definida na constituição — ver desvio documentado em `research.md` em relação à convenção legada do repositório. | PASS (com nota) |
| IV. Consistência de API | Novos endpoints seguem convenção REST já usada em `ClientController` (`api/[controller]`), com XML doc comments + `[ProducesResponseType]` para Swagger; nenhum endpoint GraphQL de Cliente existe hoje, então não é criado um novo (ver `research.md`). Logging estruturado de requisição/resposta já é coberto pelo `SerilogRequestLoggingMiddleware` global (método, rota, status, duração); adicionalmente, `ClientService.GetClientByIdAsync` emite um log `Information` estruturado (`"Cliente {ClientId} não encontrado"`) quando retorna `null`, reforçando o diagnóstico do caso de negócio (FR-005) — único ponto de logging por ação introduzido nesta feature. | PASS |
| V. Simplicidade (YAGNI) | Reaproveita `IRepository<T>`, `IUnitOfWork`, `RegisterClientResponseDto`, `BaseController.Response(entity)` (404 automático em `null`) e o padrão de service já usado em `GameService`. Único acréscimo é a especialização mínima de repositório necessária para o eager-load de `User`. | PASS |

Nenhuma violação identificada — seção "Complexity Tracking" permanece vazia.

**Re-check pós-Fase 1** (após `research.md`, `data-model.md`, `contracts/`, `quickstart.md`):
nenhuma decisão de design introduziu dependência nova de `FCG.Domain` para `FCG.Infrastructure`,
nenhum DTO/entidade nova foi criada além do mínimo (`IClientRepository`), e o contrato de API
permanece REST-only e admin-only conforme decidido. Todos os 5 princípios continuam PASS.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── FCG.Domain/
│   ├── Entities/
│   │   └── Client.cs                       # inalterado (Id, Name, UserId, User: IdentityUser)
│   └── Repositories/
│       ├── IRepository.cs                  # inalterado (genérico: GetAllAsync/GetByIdAsync/...)
│       ├── IUnitOfWork.cs                  # MODIFICADO: Clients passa a ser IClientRepository
│       └── IClientRepository.cs            # NOVO: GetAllWithUserAsync/GetByIdWithUserAsync
│
├── FCG.Infrastructure/
│   ├── DbContext.cs                        # inalterado (FK Client→User já configurada)
│   ├── UnitOfWork.cs                       # MODIFICADO: expõe ClientRepository
│   └── Repositories/
│       ├── Repository.cs                   # inalterado (genérico)
│       └── ClientRepository.cs             # NOVO: Include(c => c.User) nas consultas
│
├── FCG.Service/
│   ├── Interfaces/
│   │   └── IClientService.cs               # MODIFICADO: + GetAllClientsAsync/GetClientByIdAsync
│   ├── ClientService.cs                    # MODIFICADO: implementa os 2 novos métodos;
│   │                                        #   + ILogger<ClientService> injetado (log de
│   │                                        #   "não encontrado" em GetClientByIdAsync)
│   └── DTO/Response/
│       └── RegisterClientResponseDto.cs    # inalterado — reaproveitado (Id, Name, Email)
│
└── FCG.Presentation/
    └── Controllers/
        └── ClientController.cs             # MODIFICADO: + GET api/Client, GET api/Client/{id}
                                             #   ambos com [Authorize(Roles = "Admin")]

tests/
└── FCG.Service.Tests/
    └── ClientServiceTests.cs               # MODIFICADO: + testes para os 2 novos métodos
```

**Structure Decision**: Solução .NET em camadas já estabelecida (`FCG.Domain` →
`FCG.Infrastructure`/`FCG.Service` → `FCG.Presentation`), sem novos projetos. A feature é
implementada inteiramente dentro dos projetos existentes, seguindo o mesmo padrão já usado para
`Game` (Service com `GetAllXAsync`/`GetXByIdAsync`, Controller fino delegando ao Service). O
único arquivo novo por camada é a especialização mínima de repositório (`IClientRepository` /
`ClientRepository`) necessária para carregar `Client.User` junto com o cliente.

## Complexity Tracking

Nenhuma violação da Constitution Check foi identificada — esta seção não se aplica a este plano.
