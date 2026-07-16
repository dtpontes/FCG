---

description: "Task list for Consulta de Clientes via API"
---

# Tasks: Consulta de Clientes via API

**Input**: Design documents from `/specs/001-consulta-clientes-api/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md),
[research.md](research.md), [contracts/client-query-api.md](contracts/client-query-api.md),
[quickstart.md](quickstart.md)

**Tests**: Incluídos — regras de negócio novas (listagem e consulta por id) DEVEM ser cobertas em
`FCG.Service.Tests`, conforme Princípio III da constituição e pedido explícito do usuário.

**Organization**: Tasks agrupadas por user story (US1 = listagem, P1 · US2 = consulta por id, P2),
precedidas por uma fase Fundacional obrigatória e comum a ambas.

## Decisões aplicadas nesta geração (input do usuário)

- **`IClientRepository` herda de `IRepository<Client>`** — preserva os métodos genéricos
  (`AddAsync`, `Update`, `Delete`, `SaveChangesAsync`, `GetByIdAsync`, `GetAllAsync`) usados hoje
  por `CreateClient`, evitando qualquer quebra nesse fluxo.
- **DTO de resposta**: reaproveitar `RegisterClientResponseDto` (Id, Name, Email) — nenhum
  `ClientResponseDto` novo é criado (decisão já registrada em [research.md § 5](research.md), YAGNI).
- **Teste existente `ClientServiceTests.CreateClient_ReturnsClient_WhenValid`** é atualizado
  (T006) para usar `Mock<IClientRepository>` no lugar de `Mock<IRepository<Client>>`, garantindo
  compilação e zero regressões após a mudança de tipo em `IUnitOfWork.Clients`.
- **Novos testes**: GetAll (lista vazia e lista com itens) e GetById (id existente → dados; id
  inexistente → `null`, que o controller traduz em 404 via `BaseController.Response`).

## Path Conventions

Solução .NET em camadas (sem `src/`/`tests/` genéricos de template — caminhos reais abaixo):

- `src/FCG.Domain/Repositories/`
- `src/FCG.Infrastructure/Repositories/`, `src/FCG.Infrastructure/UnitOfWork.cs`
- `src/FCG.Service/Interfaces/`, `src/FCG.Service/ClientService.cs`
- `src/FCG.Presentation/Controllers/ClientController.cs`
- `tests/FCG.Service.Tests/ClientServiceTests.cs`

---

## Phase 1: Setup

**Purpose**: Confirmar baseline antes de qualquer alteração (não há projeto novo para inicializar
— toda a infraestrutura já existe).

- [X] T001 Rodar `dotnet build FCG.sln` e `dotnet test tests/FCG.Service.Tests/FCG.Service.Tests.csproj`
      a partir da raiz do repositório para confirmar que a build e a suíte de testes atual estão
      verdes antes de iniciar as mudanças (nenhum arquivo alterado nesta task)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Introduzir a especialização de repositório com eager-load de `Client.User`, da qual
tanto US1 quanto US2 dependem, e atualizar o teste existente que quebraria com a mudança de tipo.

**⚠️ CRITICAL**: Nenhuma user story pode começar antes desta fase estar completa — a mudança de
tipo em `IUnitOfWork.Clients` afeta a compilação de todo o `FCG.Service` e de
`ClientServiceTests.cs`.

- [X] T002 Criar a interface `IClientRepository : IRepository<Client>` em
      `src/FCG.Domain/Repositories/IClientRepository.cs`, com os métodos adicionais
      `Task<Client?> GetByIdWithUserAsync(long id)` e
      `Task<IEnumerable<Client>> GetAllWithUserAsync()` (sem nenhuma referência a EF Core neste
      arquivo — mantém `FCG.Domain` livre de dependência de `FCG.Infrastructure`)
- [X] T003 Criar `ClientRepository` em `src/FCG.Infrastructure/Repositories/ClientRepository.cs`,
      herdando de `Repository<Client>` (para manter `AddAsync`/`Update`/`Delete`/
      `SaveChangesAsync`/`GetByIdAsync`/`GetAllAsync` genéricos) e implementando
      `IClientRepository`. Como `Repository<T>._context`/`_dbSet` são `private` (não acessíveis
      à subclasse), `ClientRepository` recebe seu próprio `AppDbContext context` no construtor
      (repassado também para `base(context)`) e usa `context.Set<Client>()` internamente para os
      dois novos métodos: `GetByIdWithUserAsync` via
      `context.Set<Client>().Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id)` e
      `GetAllWithUserAsync` via `context.Set<Client>().Include(c => c.User).ToListAsync()`
      (depende de T002)
- [X] T004 Alterar o tipo da propriedade `Clients` de `IRepository<Client>` para
      `IClientRepository` em `src/FCG.Domain/Repositories/IUnitOfWork.cs` (depende de T002)
- [X] T005 Atualizar `src/FCG.Infrastructure/UnitOfWork.cs` para que o campo/propriedade `Clients`
      construa e retorne um `ClientRepository` (em vez de `Repository<Client>`), mantendo o
      padrão lazy (`_clients ??= new ClientRepository(_context)`) (depende de T003, T004)
- [X] T006 Atualizar o teste existente `CreateClient_ReturnsClient_WhenValid` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: trocar
      `var repoMock = new Mock<IRepository<Client>>();` por
      `var repoMock = new Mock<IClientRepository>();` (a linha
      `unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);` permanece igual, pois
      `IClientRepository` estende `IRepository<Client>`) — confirmar que o teste continua
      compilando e passando sem nenhuma outra alteração (depende de T004)

**Checkpoint**: `dotnet build FCG.sln` e `dotnet test` voltam a ficar verdes; `IClientRepository`
disponível via `IUnitOfWork.Clients` para as duas user stories.

---

## Phase 3: User Story 1 - Listar todos os clientes cadastrados (Priority: P1) 🎯 MVP

**Goal**: Expor `GET /api/Client`, retornando todos os clientes cadastrados (id, nome, e-mail do
usuário associado), incluindo o caso de base vazia (lista vazia, não erro).

**Independent Test**: Chamar `GET /api/Client` autenticado como Admin — com clientes cadastrados,
recebe a lista completa; sem nenhum cliente cadastrado, recebe `200 OK` com lista vazia.

### Tests for User Story 1

> **NOTE**: Escrever estes testes primeiro; confirmar que falham (método ainda não existe) antes
> de implementar T009–T010.

- [X] T007 [US1] Adicionar teste
      `GetAllClientsAsync_SemClientesCadastrados_RetornaListaVazia` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de
      `IClientRepository.GetAllWithUserAsync()` retornando `Enumerable.Empty<Client>()`; mock de
      `IMapper.Map<IEnumerable<RegisterClientResponseDto>>` retornando lista vazia; assert que o
      resultado não é nulo e está vazio (`Assert.Empty`)
- [X] T008 [US1] Adicionar teste
      `GetAllClientsAsync_ComClientesCadastrados_RetornaListaDeClientes` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de `GetAllWithUserAsync()` retornando
      2 `Client` (cada um com `User` populado); mock do `IMapper` retornando a lista de
      `RegisterClientResponseDto` correspondente; assert de contagem e dos campos `Id`/`Name`/`Email`
      dos itens retornados (depende de T007 — mesmo arquivo, editar em sequência)

### Implementation for User Story 1

- [X] T009 [US1] Adicionar `Task<IEnumerable<RegisterClientResponseDto>> GetAllClientsAsync();`
      à interface `IClientService` em `src/FCG.Service/Interfaces/IClientService.cs` (depende de
      T007–T008 estarem escritos e falhando)
- [X] T010 [US1] Implementar `GetAllClientsAsync` em `src/FCG.Service/ClientService.cs`: chamar
      `await _unitOfWork.Clients.GetAllWithUserAsync()` e mapear com
      `_mapper.Map<IEnumerable<RegisterClientResponseDto>>(clients)` (depende de T009)
- [X] T011 [US1] Adicionar a ação `GET` (sem sufixo de rota) em
      `src/FCG.Presentation/Controllers/ClientController.cs`: `[HttpGet]`,
      `[Authorize(Roles = "Admin")]`,
      `[ProducesResponseType(typeof(IEnumerable<RegisterClientResponseDto>), 200)]`, com XML doc
      comment (`<summary>`/`<returns>`) seguindo o padrão das ações existentes; chama
      `await _clientService.GetAllClientsAsync()` e retorna `Response(result)` (depende de T010)

**Checkpoint**: US1 completa e testável de ponta a ponta de forma independente — listagem
funcional, incluindo o caso de base vazia (FR-006), restrita ao papel `Admin` (FR-008).

---

## Phase 4: User Story 2 - Consultar um cliente específico por id (Priority: P2)

**Goal**: Expor `GET /api/Client/{id}`, retornando os dados do cliente quando o id existe, e 404
quando não existe.

**Independent Test**: Chamar `GET /api/Client/{id}` autenticado como Admin — com um id existente,
recebe os dados do cliente; com um id inexistente, recebe `404 Not Found`.

### Tests for User Story 2

> **NOTE**: Escrever estes testes primeiro; confirmar que falham antes de implementar T014–T015.

- [X] T012 [US2] Adicionar teste
      `GetClientByIdAsync_QuandoClienteExiste_RetornaDadosDoCliente` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de
      `IClientRepository.GetByIdWithUserAsync(id)` retornando um `Client` com `User` populado;
      mock do `IMapper` retornando o `RegisterClientResponseDto` correspondente; assert dos
      campos `Id`/`Name`/`Email` do resultado
- [X] T013 [US2] Adicionar teste
      `GetClientByIdAsync_QuandoClienteNaoExiste_RetornaNulo` em
      `tests/FCG.Service.Tests/ClientServiceTests.cs`: mock de `GetByIdWithUserAsync(id)`
      retornando `null`; assert que o resultado do service é `null` (o controller traduz isso em
      404 via `BaseController.Response`) (depende de T012 — mesmo arquivo, editar em sequência)

### Implementation for User Story 2

- [X] T014 [US2] Adicionar `Task<RegisterClientResponseDto?> GetClientByIdAsync(long id);` à
      interface `IClientService` em `src/FCG.Service/Interfaces/IClientService.cs` (depende de
      T009 já ter sido aplicado nesse mesmo arquivo, e de T012–T013 estarem escritos e falhando)
- [X] T015 [US2] Implementar `GetClientByIdAsync` em `src/FCG.Service/ClientService.cs`: chamar
      `await _unitOfWork.Clients.GetByIdWithUserAsync(id)`; se não encontrado, logar
      `Information` estruturado (`"Cliente {ClientId} não encontrado", id`) via
      `ILogger<ClientService>` injetado no construtor, e retornar `null`; caso contrário,
      `_mapper.Map<RegisterClientResponseDto>(client)` (depende de T014)
- [X] T016 [US2] Adicionar a ação `GET("{id}")` em
      `src/FCG.Presentation/Controllers/ClientController.cs`: `[HttpGet("{id}")]`,
      `[Authorize(Roles = "Admin")]`,
      `[ProducesResponseType(typeof(RegisterClientResponseDto), 200)]`,
      `[ProducesResponseType(404)]`, com XML doc comment seguindo o padrão das ações existentes;
      chama `await _clientService.GetClientByIdAsync(id)` e retorna `Response(result)` — o 404
      automático já é dado pelo `BaseController.Response` quando `result` é `null` (depende de
      T015)

**Checkpoint**: US1 e US2 funcionam de forma independente e simultânea — consulta por id
funcional, incluindo o caso de id inexistente (FR-005), restrita ao papel `Admin` (FR-008).

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validação final de ponta a ponta e conferência de zero regressões.

- [X] T017 [P] Revisar os XML doc comments (`<summary>`, `<returns>`) das duas novas ações em
      `src/FCG.Presentation/Controllers/ClientController.cs` e confirmar no Swagger UI
      (`http://localhost:8080/swagger`) que aparecem corretamente documentadas
- [X] T018 [P] Executar manualmente os 5 cenários de [quickstart.md](quickstart.md) via
      `docker-compose up --build` + Swagger/HTTP client, cobrindo listagem vazia, listagem com
      itens, consulta por id existente, consulta por id inexistente (404) e acesso sem papel
      `Admin` (401/403)
- [X] T019 Rodar `dotnet test tests/FCG.Service.Tests/FCG.Service.Tests.csproj` completo e
      confirmar que todos os testes de `ClientServiceTests.cs` (o existente T006 + os 4 novos
      T007/T008/T012/T013) e de `GameServiceTests.cs` passam, sem nenhuma regressão; confirmar
      também (FR-007) que `ClientController.cs` ganhou apenas as 2 novas ações `[HttpGet]` (T011,
      T016) e nenhum novo `[HttpPost]`/`[HttpPut]`/`[HttpDelete]` foi introduzido

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: depende do Setup — BLOQUEIA as duas user stories
- **User Story 1 (Phase 3)** e **User Story 2 (Phase 4)**: ambas dependem apenas da Foundational
  completa; podem ser feitas em paralelo por pessoas diferentes, mas como compartilham os mesmos
  3 arquivos (`IClientService.cs`, `ClientService.cs`, `ClientController.cs`,
  `ClientServiceTests.cs`), a ordem sequencial US1 → US2 descrita acima evita conflitos de edição
  simultânea no mesmo arquivo
- **Polish (Phase 5)**: depende de US1 e US2 completas

### Within Each User Story

- Testes (T007–T008 / T012–T013) escritos e falhando antes da implementação
- Interface do Service antes da implementação do Service
- Service antes do Controller

### Parallel Opportunities

- Dentro da Foundational, T002 não depende de nenhuma outra task e pode começar primeiro; T003 e
  T004 dependem de T002 e podem ser feitas em paralelo entre si (arquivos diferentes) antes de
  T005/T006
- T017 e T018 (Polish) podem ser feitas em paralelo entre si
- Como US1 e US2 tocam os mesmos 3 arquivos de produção, **não** há paralelismo real de edição
  entre as duas fases — o paralelismo aqui é limitado à Foundational e ao Polish

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar Phase 1: Setup
2. Completar Phase 2: Foundational (bloqueia tudo)
3. Completar Phase 3: User Story 1
4. **PARAR e VALIDAR**: testar a listagem isoladamente (T018, cenários 1–2)
5. Já entrega valor: visibilidade completa da base de clientes

### Incremental Delivery

1. Setup + Foundational → base pronta (IClientRepository disponível)
2. US1 (listagem) → testar independentemente → MVP entregável
3. US2 (consulta por id) → testar independentemente → complementa o MVP
4. Polish → validação de ponta a ponta e confirmação de zero regressões

## Notes

- [P] = arquivos diferentes, sem dependência entre si
- [US1]/[US2] mapeiam a task à user story correspondente do spec
- Commitar após cada task ou grupo lógico (ex.: após T006, após T011, após T016)
- Nenhuma migration de EF Core é necessária (feature somente-leitura — ver research.md § 6)
