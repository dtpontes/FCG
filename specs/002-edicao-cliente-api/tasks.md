---

description: "Task list for Edição de Cliente"
---

# Tasks: Edição de Cliente

**Input**: Design documents from `/specs/002-edicao-cliente-api/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md),
[research.md](research.md), [contracts/client-update-api.md](contracts/client-update-api.md),
[quickstart.md](quickstart.md)

**Tests**: Incluídos — regra de negócio nova (atualização de nome) DEVE ser coberta em
`FCG.Service.Tests`, conforme Princípio III da constituição, seguindo o mesmo padrão já usado
para `CreateClient`/`GetAllClientsAsync`/`GetClientByIdAsync` (feature 001).

**Organization**: Uma única user story (US1 = atualizar nome, P1). Não há fase Foundational com
tasks próprias nesta feature — toda a infraestrutura de acesso a dados necessária
(`IClientRepository.GetByIdWithUserAsync`, `IRepository<Client>.Update`/`SaveChangesAsync`) já
existe da feature 001 e não é modificada (ver [research.md § 1](research.md)).

## Decisões aplicadas nesta geração

- **Nenhuma mudança em `FCG.Domain`/`FCG.Infrastructure`** — reaproveita integralmente
  `IClientRepository`/`IUnitOfWork` já existentes.
- **Distinção 404 vs. 400 sem lógica nova no controller**: `UpdateClientNameAsync` retorna
  `null` sem notificar erro de validação para "não encontrado" (→ `BaseController.Response`
  produz 404), e `null` com notificação de validação para "nome inválido" (→ produz 400) — o
  controller apenas chama `Response(result)`.
- **DTO de resposta reaproveitado**: `RegisterClientResponseDto` (sem DTO novo de resposta).
- **Verbo HTTP**: `PATCH` (atualização parcial de um único campo), não `PUT`.
- **Logging**: reaproveita `ILogger<ClientService>` já injetado na feature 001 para o caso "não
  encontrado".

## Path Conventions

- `src/FCG.Service/DTO/Request/`, `src/FCG.Service/DTO/Validator/`
- `src/FCG.Service/Interfaces/IClientService.cs`, `src/FCG.Service/ClientService.cs`
- `src/FCG.Presentation/Controllers/ClientController.cs`
- `tests/FCG.Service.Tests/ClientServiceTests.cs`

---

## Phase 1: Setup

**Purpose**: Confirmar baseline antes de qualquer alteração.

- [ ] T001 Rodar `dotnet build FCG.sln` e `dotnet test tests/FCG.Service.Tests/FCG.Service.Tests.csproj`
      a partir da raiz do repositório para confirmar que a build e a suíte de testes atual
      (18 testes da feature 001) estão verdes antes de iniciar as mudanças (nenhum arquivo
      alterado nesta task)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: N/A para esta feature — não há pré-requisito bloqueante além do Setup. A única
"fundação" necessária (`IClientRepository.GetByIdWithUserAsync`, `Update`/`SaveChangesAsync`
genéricos) já foi entregue e testada na feature 001 e não é modificada aqui (ver
[research.md § 1](research.md)). Todo o trabalho desta feature é local à User Story 1.

---

## Phase 3: User Story 1 - Corrigir o nome de um cliente existente (Priority: P1) 🎯 MVP

**Goal**: Expor `PATCH /api/Client/{id}`, permitindo corrigir apenas o nome de um cliente já
cadastrado, com as três respostas exigidas: sucesso (200 + dados atualizados), id inexistente
(404), nome inválido (400).

**Independent Test**: Cadastrar um cliente, chamar `PATCH /api/Client/{id}` com um nome válido e
confirmar via `GET /api/Client/{id}` (feature 001) que o nome foi atualizado; chamar com um id
inexistente e confirmar 404; chamar com um nome inválido e confirmar 400 e que o nome original
permanece inalterado.

### Modelos (DTO + Validação) para User Story 1

- [X] T002 [US1] Criar `UpdateClientDto : BaseDto` em
      `src/FCG.Service/DTO/Request/UpdateClientDto.cs`, com um único campo `required string Name`
      (com XML doc comment `<summary>`/`<example>` seguindo o padrão de `RegisterClientDto`) e
      `IsValid()` delegando para `UpdateClientDtoValidator` (mesmo padrão de
      `RegisterClientDto.IsValid()`)
- [X] T003 [US1] Criar `UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>` em
      `src/FCG.Service/DTO/Validator/UpdateClientDtoValidator.cs`:
      `RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("O nome é obrigatório.")`
      + `.Must(name => name.Trim().Length >= 2).WithMessage("O nome deve ter no mínimo 2 caracteres.")`
      + `.MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.")` (depende de
      T002; segue o estilo de `GameRequestDtoValidator`). **IMPORTANTE**: `Cascade(CascadeMode.Stop)`
      é obrigatório aqui — sem ele, o FluentValidation por padrão avalia todas as regras mesmo após
      `NotEmpty()` falhar, e `.Must(name => name.Trim()...)` lançaria `NullReferenceException` se
      `Name` chegar como `null` via JSON (o modificador `required` do C# não é reforçado pela
      desserialização), resultando em 500 em vez do 400 exigido por FR-004

### Tests for User Story 1

> **NOTE**: Escrever estes testes primeiro; confirmar que falham (método ainda não existe) antes
> de implementar T007–T008.

- [X] T004 [US1] Adicionar teste
      `UpdateClientNameAsync_QuandoClienteExisteENomeValido_RetornaClienteAtualizado` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de
      `IClientRepository.GetByIdWithUserAsync(id)` retornando um `Client` existente com `User`
      populado; mock do `IMapper` retornando o `RegisterClientResponseDto` com o nome já
      atualizado; assert de que o resultado não é nulo, `Name` reflete o novo valor e `Email`
      permanece o mesmo; `repoMock.Verify(r => r.Update(It.IsAny<Client>()), Times.Once)` e
      `unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once)`
- [X] T005 [US1] Adicionar teste `UpdateClientNameAsync_QuandoClienteNaoExiste_RetornaNulo` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de `GetByIdWithUserAsync(id)`
      retornando `null`; assert que o resultado do service é `null`;
      `repoMock.Verify(r => r.Update(It.IsAny<Client>()), Times.Never)` e
      `unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never)`
      (confirma que nada é persistido — FR-003) (depende de T004 — mesmo arquivo, editar em
      sequência)
- [X] T006 [US1] Adicionar teste `UpdateClientNameAsync_QuandoNomeInvalido_RetornaNulo` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`, cobrindo em um `[Theory]`/`[InlineData]`
      (ou 3 `[Fact]` separados) os três casos: `Name = ""` (vazio), `Name = "  "` (só espaços) e
      `Name = null!` (nulo — regressão para o risco de `NullReferenceException` coberto pelo
      `Cascade(CascadeMode.Stop)` de T003); real (sem mock de validação — usa o
      `UpdateClientDtoValidator` real via `IsValidTransaction`); assert que o resultado é `null`
      em todos os casos, sem lançar exceção;
      `repoMock.Verify(r => r.GetByIdWithUserAsync(It.IsAny<long>()), Times.Never)` (a validação
      deve falhar antes de qualquer acesso a dados) e
      `unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never)`
      (confirma que nada é persistido — FR-004) (depende de T005 — mesmo arquivo, editar em
      sequência)

### Implementation for User Story 1

- [X] T007 [US1] Adicionar
      `Task<RegisterClientResponseDto?> UpdateClientNameAsync(long id, UpdateClientDto updateClientDto);`
      à interface `IClientService` em `src/FCG.Service/Interfaces/IClientService.cs` (depende de
      T002, T004–T006 estarem escritos e falhando)
- [X] T008 [US1] Implementar `UpdateClientNameAsync` em `src/FCG.Service/ClientService.cs`:
      `if (!IsValidTransaction(updateClientDto)) return null;` → em seguida
      `var client = await _unitOfWork.Clients.GetByIdWithUserAsync(id);` → se `null`, logar
      `Information` (`"Cliente {ClientId} não encontrado", id`, mesmo padrão de
      `GetClientByIdAsync`) e retornar `null` → caso contrário, `client.Name =
      updateClientDto.Name;`, `_unitOfWork.Clients.Update(client);`,
      `await _unitOfWork.SaveChangesAsync();`, retornar
      `_mapper.Map<RegisterClientResponseDto>(client)` (depende de T007)
- [X] T009 [US1] Adicionar a ação `PATCH("{id}")` em
      `src/FCG.Presentation/Controllers/ClientController.cs`: `[HttpPatch("{id}")]`,
      `[Authorize(Roles = "Admin")]`,
      `[ProducesResponseType(typeof(RegisterClientResponseDto), 200)]`,
      `[ProducesResponseType(400)]`, `[ProducesResponseType(404)]`, com XML doc comment
      seguindo o padrão das ações existentes; chama
      `await _clientService.UpdateClientNameAsync(id, updateClientDto)` e retorna
      `Response(result)` — os três status (200/400/404) já são produzidos automaticamente por
      `BaseController.Response(object)`, sem lógica condicional adicional no controller (depende
      de T008)

**Checkpoint**: US1 completa e testável de ponta a ponta de forma independente — atualização de
nome funcional, incluindo id inexistente (FR-003) e nome inválido (FR-004), restrita ao papel
`Admin` (FR-008), sem afetar e-mail/usuário associado (FR-005/FR-006).

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Validação final de ponta a ponta e conferência de zero regressões.

- [X] T010 [P] Revisar os XML doc comments (`<summary>`, `<param>`, `<returns>`) da nova ação em
      `src/FCG.Presentation/Controllers/ClientController.cs` e confirmar no Swagger UI
      (`http://localhost:8080/swagger`) que `PATCH /api/Client/{id}` aparece corretamente
      documentada, com os três response schemas (200/400/404)
- [X] T011 [P] Executar manualmente os 5 cenários de [quickstart.md](quickstart.md) via
      `docker-compose up --build` (ou execução equivalente) + Swagger/HTTP client, cobrindo
      atualização com nome válido, id inexistente, nome inválido (vazio/curto/excedendo limite),
      idempotência (mesmo nome) e acesso sem papel `Admin` (401/403)
- [X] T012 Rodar `dotnet test tests/FCG.Service.Tests/FCG.Service.Tests.csproj` completo e
      confirmar que todos os testes de `ClientServiceTests.cs` (os 18 já existentes da feature
      001 + os novos de T004/T005/T006) e de `GameServiceTests.cs` passam, sem nenhuma regressão;
      confirmar também (FR-006/FR-007) que `ClientController.cs` ganhou apenas a nova ação
      `[HttpPatch]` (T009) — nenhum novo `[HttpDelete]`/`[HttpPut]` foi introduzido — e que
      `UpdateClientDto` (T002) não possui nenhuma propriedade `Email`/`UserId`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: N/A — nenhuma task própria nesta feature
- **User Story 1 (Phase 3)**: depende apenas do Setup
- **Polish (Phase 4)**: depende de US1 completa

### Within User Story 1

- Modelos (T002–T003) antes dos testes que os utilizam (T004–T006)
- Testes (T004–T006) escritos e falhando antes da implementação (T007–T009)
- Interface do Service (T007) antes da implementação do Service (T008)
- Service (T008) antes do Controller (T009)

### Parallel Opportunities

- T002 e T003 tocam arquivos diferentes, mas T003 depende do tipo `UpdateClientDto` definido em
  T002 — sequenciais, sem paralelismo real
- T004–T006 tocam o mesmo arquivo de teste — sequenciais
- T010 e T011 (Polish) podem ser feitas em paralelo entre si

---

## Implementation Strategy

### MVP First (única user story)

1. Completar Phase 1: Setup
2. Completar Phase 3: User Story 1 (única história desta feature)
3. **PARAR e VALIDAR**: testar a atualização de nome isoladamente (T011, cenários 1–3)
4. Já entrega valor completo: correção de dados cadastrais de clientes

### Incremental Delivery

1. Setup → baseline confirmado
2. User Story 1 (atualização de nome) → testar independentemente → MVP entregável (única
   entrega desta feature)
3. Polish → validação de ponta a ponta e confirmação de zero regressões

## Notes

- [P] = arquivos diferentes, sem dependência entre si
- [US1] mapeia a task à única user story do spec
- Commitar após cada task ou grupo lógico (ex.: após T003, após T006, após T009)
- Nenhuma migration de EF Core é necessária (apenas `UPDATE` de coluna já existente — ver
  research.md § 7)
