# Research: Edição de Cliente

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Nenhum `[NEEDS CLARIFICATION]` restou no Technical Context do plano — todas as decisões abaixo
partem de padrões já existentes no código (`FCG.Service`, `FCG.Presentation`) e da infraestrutura
de dados já criada na feature 001 (`IClientRepository`), levantados por inspeção direta do
repositório.

## 1. Reaproveitamento total de `FCG.Domain`/`FCG.Infrastructure`

**Decision**: Nenhuma mudança em `FCG.Domain` ou `FCG.Infrastructure`. A atualização de nome usa
`await _unitOfWork.Clients.GetByIdWithUserAsync(id)` (já existente, feature 001) para carregar o
cliente rastreado pelo `AppDbContext`, atribui o novo `Name`, chama
`_unitOfWork.Clients.Update(client)` (genérico, de `IRepository<Client>`) e
`await _unitOfWork.SaveChangesAsync()`.

**Rationale**: `GetByIdWithUserAsync` já retorna a entidade `Client` rastreada (tracked) pelo
`DbContext` — nenhuma consulta adicional ou repositório novo é necessário para persistir a
mudança. Reutilizar `GetByIdWithUserAsync` (em vez do `GetByIdAsync` genérico, sem `Include`)
também garante que a resposta de sucesso já tenha `client.User` populado para o mapeamento de
`Email` em `RegisterClientResponseDto`, sem uma segunda consulta.

**Alternatives considered**:
- *Criar um método `UpdateNameAsync` dedicado em `IClientRepository`* — rejeitado: o padrão
  repositório/UnitOfWork já expõe `Update`/`SaveChangesAsync` genéricos suficientes; adicionar um
  método específico só para atribuir um campo violaria o Princípio V (YAGNI) sem benefício.

## 2. Distinção 404 vs. 400 sem lógica nova no controller

**Decision**: `ClientService.UpdateClientNameAsync` retorna `RegisterClientResponseDto?`, usando
exatamente a mesma convenção de `null` já estabelecida em `CreateClient`:
- Nome inválido → `IsValidTransaction(dto)` retorna `false` (e já chama
  `NotifyValidationError` internamente) → método retorna `null`.
- Id inexistente → método retorna `null` **sem** notificar erro de validação.
- Sucesso → retorna o DTO mapeado (não `null`).

O controller apenas chama `return Response(client);` — o `BaseController.Response(object entity)`
já existente distingue os três casos: `entity != null` → 200; `entity == null` e
`IsValidTransaction()` (checada internamente) falsa → 400 (`ValidationErrors()`); `entity == null`
e sem erro de validação → 404.

**Rationale**: Esse mecanismo já funciona e é testado pelo `CreateClient` existente (validação →
400 via `ValidationErrors()`, sucesso → 200). Reaproveitá-lo integralmente evita introduzir uma
segunda convenção de erro na mesma API (Princípio IV: consistência) e elimina qualquer código
condicional novo no controller (Princípio V: YAGNI) — a ação de atualização fica tão simples
quanto `GetClientById`.

**Alternatives considered**:
- *Retornar um tipo de resultado explícito (`enum`/tupla `NotFound`/`Invalid`/`Success`)* —
  rejeitado: mais expressivo, mas introduz um padrão novo não usado em nenhum outro lugar da API;
  o mecanismo `null` + notificação de validação já resolve o caso com zero código adicional no
  controller.

## 3. DTO de requisição e validação

**Decision**: Novo `UpdateClientDto : BaseDto` com um único campo `Name` (string, obrigatório).
Validador `UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>`:
- `NotEmpty()` — nome vazio ou só espaços é inválido.
- `MinimumLength(2)` — nome "muito curto" (limite assumido no spec, documentado como suposição).
- `MaximumLength(100)` — mesmo limite já configurado para `Client.Name` em
  `DbContext.cs` (`HasMaxLength(100)`), evitando estourar o schema como erro de banco em vez de
  validação (400).

**Rationale**: Segue exatamente o estilo já usado em `GameRequestDtoValidator`
(`NotEmpty().WithMessage(...)`, `MaximumLength(100).WithMessage(...)`). Nenhum DTO existente
(`RegisterClientDto`) é reaproveitado para a requisição, pois ele exige `Email`/`Password`
obrigatórios que não fazem sentido para esta operação (FR-005/FR-006 proíbem alterar e-mail);
um DTO dedicado e mínimo é a opção mais simples que atende ao escopo (Princípio V).

**Alternatives considered**:
- *Reaproveitar `RegisterClientDto` marcando `Email`/`Password` como opcionais* — rejeitado:
  abriria a possibilidade de o cliente da API enviar esses campos por engano, exigindo lógica
  extra para ignorá-los explicitamente; um DTO com apenas `Name` já torna a proibição de
  FR-005/FR-006 estrutural (não é possível nem enviar esses campos).

## 4. Verbo HTTP

**Decision**: `[HttpPatch("{id}")]` em vez de `PUT`.

**Rationale**: A requisição contém apenas o campo `Name`, não uma representação completa do
recurso `Client` (que também inclui `Id` e o usuário associado, imutáveis por esta operação) —
semanticamente isso é uma atualização parcial (PATCH), não uma substituição completa (PUT). Não
há precedente de `PUT`/`PATCH` em nenhum outro controller do projeto; esta é a primeira operação
de atualização exposta via REST na API, então a escolha segue a convenção REST padrão para
atualização parcial de um subconjunto de campos.

**Alternatives considered**:
- *`PUT /api/Client/{id}` com corpo completo (nome + e-mail, mesmo que e-mail seja ignorado)* —
  rejeitado: obrigaria o consumidor a reenviar dados que não podem ser alterados (e-mail),
  confundindo a semântica de "isto é somente leitura nesta operação" (FR-005/FR-006).

## 5. DTO de resposta

**Decision**: Reaproveitar `RegisterClientResponseDto` (Id, Name, Email) como retorno de sucesso
— nenhum DTO novo é criado.

**Rationale**: Mesma decisão já tomada na feature 001 para os endpoints de leitura; os campos
retornados no sucesso da atualização são idênticos aos já expostos por
`GET /api/Client/{id}`, e o mapeamento AutoMapper `Client → RegisterClientResponseDto` já existe
e já lê `User.Email` corretamente (a entidade retornada por `GetByIdWithUserAsync` já tem `User`
carregado).

**Alternatives considered**:
- *Retornar apenas um indicador de sucesso sem os dados atualizados* — rejeitado: o spec
  (FR-002, Assumptions) já define que a resposta de sucesso deve incluir os dados atualizados do
  cliente, para consistência com o restante da API e para permitir que o consumidor confirme o
  novo valor sem uma chamada adicional a `GET /api/Client/{id}`.

## 6. Logging estruturado

**Decision**: `UpdateClientNameAsync` reaproveita o `ILogger<ClientService>` já injetado na
feature 001 e registra um log `Information` (`"Cliente {ClientId} não encontrado"`, mesmo texto e
nível já usado em `GetClientByIdAsync`) quando o id não existe.

**Rationale**: Mantém consistência com o precedente já estabelecido (research.md § 7 da feature
001) para o mesmo tipo de evento de negócio (cliente não encontrado), sem introduzir uma nova
convenção de logging.

**Alternatives considered**: nenhuma — decisão já resolvida pela feature 001; aqui apenas se
aplica o mesmo padrão ao novo método.

## 7. Migração de banco de dados

**Decision**: Nenhuma migration EF Core é necessária.

**Rationale**: A coluna `Name` da tabela `Clients` já existe (`NOT NULL`, `MaxLength(100)`,
`Migrations/20250528135851_initial.cs`); esta feature apenas executa um `UPDATE` sobre uma coluna
já existente, sem alteração de schema.
