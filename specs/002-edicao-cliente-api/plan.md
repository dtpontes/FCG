# Implementation Plan: Edição de Cliente

**Branch**: `002-edicao-cliente-api` | **Date**: 2026-07-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-edicao-cliente-api/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Adicionar uma operação de atualização parcial à API de Clientes já existente:
`PATCH /api/Client/{id}`, permitindo corrigir apenas o nome de um cliente já cadastrado. Sucesso
retorna os dados atualizados (id, nome, e-mail); id inexistente retorna 404; nome inválido
(vazio, muito curto ou excedendo o tamanho máximo) retorna 400. Restrito ao papel `Admin`, na
mesma linha da feature de consulta (001). A abordagem técnica reaproveita integralmente a
infraestrutura de acesso a dados já criada na feature 001 (`IClientRepository.GetByIdWithUserAsync`,
`IRepository<Client>.Update`/`SaveChangesAsync` genéricos) — nenhuma mudança em `FCG.Domain` ou
`FCG.Infrastructure` é necessária. O único mecanismo novo é um DTO de requisição dedicado
(`UpdateClientDto`, apenas `Name`) com validação FluentValidation, e a distinção 400 vs. 404 é
obtida reaproveitando o comportamento já existente de `BaseController.Response(object)`: o
Service retorna `null` sem notificar erro de validação para "não encontrado" (→ 404), e `null`
com notificação de validação para "nome inválido" (→ 400) — o mesmo mecanismo que já funciona em
`CreateClient`.

## Technical Context

**Language/Version**: .NET 9 / C# 13

**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core (SQL Server provider),
AutoMapper, FluentValidation, MediatR (notificações de domínio), ASP.NET Core Identity + JWT
Bearer (autenticação e papéis), Swashbuckle/Swagger, Serilog

**Storage**: SQL Server via EF Core, `AppDbContext` existente; nenhuma alteração de schema —
apenas `UPDATE` do campo `Name` já existente na tabela `Clients` (coluna já `NOT NULL`,
`MaxLength(100)`, configurada em `DbContext.cs`)

**Testing**: xUnit + Moq, projeto `tests/FCG.Service.Tests` (mesmo padrão usado em
`ClientServiceTests.cs`, incluindo o `Mock<ILogger<ClientService>>` já introduzido na feature 001)

**Target Platform**: ASP.NET Core Web API hospedada em container Docker (Linux), conforme
`docker-compose.yml`/deploy Azure já existentes

**Project Type**: web-service, solução em camadas (.NET, single repo multi-projeto):
`FCG.Domain` / `FCG.Infrastructure` / `FCG.Service` / `FCG.Presentation` + `FCG.Service.Tests`
— esta feature toca apenas `FCG.Service` e `FCG.Presentation`

**Performance Goals**: Sem meta nova definida pela spec; operação de escrita pontual (um único
`UPDATE` por chamada), dentro do comportamento padrão já observado nas demais operações da API

**Constraints**: `FCG.Domain` NÃO PODE depender de `FCG.Infrastructure`/EF Core (Princípio I,
já satisfeito — nenhuma mudança nessas camadas); a operação DEVE exigir papel `Admin` (FR-008);
NÃO DEVE alterar e-mail/usuário associado (FR-005/FR-006); NÃO DEVE remover clientes (FR-007)

**Scale/Scope**: Mesma escala da feature 001 (uso interno/admin); 1 endpoint de escrita
(atualização de nome), sem criação/remoção e sem alterar outros atributos

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Avaliação | Status |
|---|---|---|
| I. Arquitetura em Camadas Respeitada | Nenhuma mudança em `FCG.Domain`/`FCG.Infrastructure` — reaproveita `IClientRepository`/`IRepository<Client>` já existentes. Regra de negócio (validação de nome, distinção 404/400) fica inteiramente em `FCG.Service`. | PASS |
| II. Spec Antes de Código (SDD) | Spec aprovada (`spec.md`) antes deste plano; nenhuma ambiguidade de alto impacto pendente (ver Assumptions do spec). | PASS |
| III. Testes de Regras de Negócio | Novo método `UpdateClientNameAsync` em `ClientService` será coberto em `FCG.Service.Tests` (sucesso, id inexistente, nome inválido) usando `Metodo_Cenario_Resultado`. | PASS |
| IV. Consistência de API | Novo endpoint segue convenção REST já usada em `ClientController` (`api/[controller]`), com XML doc comments + `[ProducesResponseType]` para Swagger; reaproveita `RegisterClientResponseDto` para a resposta de sucesso (mesma decisão da feature 001); `_logger` (`ILogger<ClientService>`, já introduzido na feature 001) registra o caso "cliente não encontrado", mantendo o mesmo padrão de diagnóstico. | PASS |
| V. Simplicidade (YAGNI) | Reaproveita integralmente `IClientRepository`, `IUnitOfWork`, `RegisterClientResponseDto`, `BaseController.Response(object)` (a mesma chamada já distingue 200/400/404 sem nenhuma lógica nova no controller) e o padrão `IsValidTransaction`/`NotifyValidationError` já usado em `CreateClient`. Único código novo: `UpdateClientDto` + validator + 1 método de Service + 1 ação de Controller. | PASS |

Nenhuma violação identificada — seção "Complexity Tracking" permanece vazia.

**Re-check pós-Fase 1** (após `research.md`, `data-model.md`, `contracts/`, `quickstart.md`):
nenhuma decisão de design alterou `FCG.Domain`/`FCG.Infrastructure`; nenhum DTO de resposta novo
foi criado; o contrato de API permanece consistente com a feature 001 (mesmo padrão de erro,
mesmo controle de acesso). Todos os 5 princípios continuam PASS.

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
├── FCG.Domain/                              # SEM ALTERAÇÃO nesta feature
│
├── FCG.Infrastructure/                      # SEM ALTERAÇÃO nesta feature
│
├── FCG.Service/
│   ├── DTO/
│   │   ├── Request/
│   │   │   └── UpdateClientDto.cs           # NOVO: apenas Name (BaseDto)
│   │   └── Validator/
│   │       └── UpdateClientDtoValidator.cs  # NOVO: NotEmpty + MinimumLength(2) +
│   │                                        #   MaximumLength(100)
│   ├── Interfaces/
│   │   └── IClientService.cs                # MODIFICADO: + UpdateClientNameAsync
│   └── ClientService.cs                     # MODIFICADO: implementa UpdateClientNameAsync
│                                             #   (reaproveita _logger já existente)
│
└── FCG.Presentation/
    └── Controllers/
        └── ClientController.cs              # MODIFICADO: + PATCH api/Client/{id}
                                             #   com [Authorize(Roles = "Admin")]

tests/
└── FCG.Service.Tests/
    └── ClientServiceTests.cs                # MODIFICADO: + testes para UpdateClientNameAsync
```

**Structure Decision**: Mesma solução em camadas já estabelecida nas features anteriores, sem
novos projetos. Esta feature é a mais enxuta até agora: `FCG.Domain` e `FCG.Infrastructure`
permanecem intocados, pois toda a infraestrutura de acesso a dados necessária
(`IClientRepository.GetByIdWithUserAsync`, `IRepository<Client>.Update`/`SaveChangesAsync`) já
foi criada na feature 001. Apenas `FCG.Service` (novo DTO + validator + método) e
`FCG.Presentation` (nova ação) são tocados.

## Complexity Tracking

Nenhuma violação da Constitution Check foi identificada — esta seção não se aplica a este plano.
