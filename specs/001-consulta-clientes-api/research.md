# Research: Consulta de Clientes via API

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Nenhum `[NEEDS CLARIFICATION]` restou no Technical Context do plano — todas as decisões abaixo
partem de padrões já existentes no código (`FCG.Domain`, `FCG.Infrastructure`, `FCG.Service`,
`FCG.Presentation`), levantados por inspeção direta do repositório.

## 1. Eager-load de `Client.User` para o campo `Email`

**Decision**: Introduzir `IClientRepository : IRepository<Client>` em `FCG.Domain.Repositories`
com dois métodos adicionais — `GetAllWithUserAsync()` e `GetByIdWithUserAsync(long id)` — e
implementá-los em `FCG.Infrastructure.Repositories.ClientRepository` usando
`.Include(c => c.User)`. `IUnitOfWork.Clients` passa a ser tipado como `IClientRepository` em
vez de `IRepository<Client>`.

**Rationale**: `AutoMapperProfile.cs` já mapeia `RegisterClientResponseDto.Email` a partir de
`src.User.Email` (`CreateMap<Client, RegisterClientResponseDto>().ForMember(dest => dest.Email,
opt => opt.MapFrom(src => src.User.Email))`). O `Repository<T>` genérico usa
`_dbSet.FindAsync(id)` / `_dbSet.ToListAsync()`, que **não** carregam a navegação `User` — sem
o eager-load, `Email` lançaria `NullReferenceException` (ou mapearia default) para clientes lidos
via os novos endpoints. A interface especializada mantém `FCG.Domain` livre de qualquer
referência a EF Core (o `Include` fica inteiramente em `FCG.Infrastructure`), respeitando o
Princípio I.

**Alternatives considered**:
- *Adicionar um parâmetro genérico de `Include` a `IRepository<T>`* — rejeitado: exigiria expor
  tipos do EF Core (`IIncludableQueryable`) na assinatura do Domain, violando o isolamento de
  Infrastructure exigido pela constituição.
- *Fazer uma segunda consulta ao `UserManager`/Identity para preencher o e-mail após o
  `GetByIdAsync` genérico* — rejeitado: mais uma chamada ao banco por item da listagem (N+1),
  mais complexo que um único `Include`, sem benefício.
- *Criar um novo DTO/projeção via `Select` direto no `DbContext`, sem carregar a entidade
  completa* — descartado por ora: adicionaria uma abstração de query fora do padrão
  repositório/UnitOfWork já estabelecido, contrariando o Princípio V (YAGNI) para uma feature de
  2 endpoints simples; pode ser revisitado se performance se tornar um problema real.

## 2. Controle de acesso (Admin) nos novos endpoints

**Decision**: Aplicar `[Authorize(Roles = "Admin")]` nas duas novas ações do `ClientController`.

**Rationale**: A clarificação de 2026-07-15 (spec.md, FR-008) decidiu restringir ambos os
endpoints ao papel `Admin`. A infraestrutura de auth já existe (`AuthenticationConfiguration.cs`,
JWT Bearer, papéis `"Admin"`/`"User"` semeados em `IdentityConfiguration.cs`), mas **nenhum
`[Authorize]` é usado hoje em nenhum controller** — esta feature introduz o primeiro uso real do
atributo, usando o mesmo literal de papel (`"Admin"`) já semeado, sem criar política nova.

**Alternatives considered**:
- *Criar uma policy nomeada (`AddAuthorization(options => options.AddPolicy("AdminOnly", ...))`)*
  — rejeitado por ora: adicionaria uma camada de configuração extra sem necessidade concreta
  para apenas 2 endpoints; `[Authorize(Roles = "Admin")]` direto é suficiente e mais simples
  (Princípio V). Pode evoluir para policy se mais endpoints admin-only surgirem.

## 3. REST vs GraphQL para os novos endpoints

**Decision**: Implementar somente como endpoints REST em `ClientController`, sem adicionar
schema GraphQL para `Client`.

**Rationale**: O `ClientController` existente (`register`) já é REST-only; não existe nenhum
tipo/query GraphQL para `Client` (ou qualquer outra entidade) no código hoje, apesar do pacote
`GraphQL.AspNet` estar registrado globalmente em `Program.cs`. Adicionar esquema GraphQL para
`Client` nesta feature seria introduzir um padrão novo não solicitado pela spec (que fala apenas
em "consumidor da API", sem exigir GraphQL) — mantém-se consistente com o padrão REST já
existente para este mesmo controller (Princípio IV: consistência com o que já existe).

**Alternatives considered**:
- *Adicionar query GraphQL equivalente* — rejeitado: fora do escopo definido na spec e sem
  precedente no código para nenhuma entidade; ficaria inconsistente ser o único endpoint
  GraphQL do sistema.

## 4. Nomenclatura de testes: convenção da constituição vs. testes legados

**Decision**: Os novos testes em `ClientServiceTests.cs` seguem `Metodo_Cenario_Resultado`
(Princípio III da constituição), ex.: `GetAllClientesAsync_QuandoNaoHaClientes_RetornaListaVazia`.

**Rationale**: Os testes já existentes no repositório (`CreateClient_ReturnsClient_WhenValid`,
`GetGameById_ReturnsGame_WhenExists`) seguem a ordem inversa (`Metodo_Resultado_Cenario`, em
inglês). A constituição do projeto (ratificada em 2026-07-15) prevalece sobre convenções
informais anteriores (seção Governance) e define explicitamente `Metodo_Cenario_Resultado` como
padrão obrigatório para novas regras de negócio testadas. Os testes legados não são renomeados
nesta feature (fora de escopo), mas todo teste novo criado aqui segue o padrão da constituição.

**Alternatives considered**:
- *Seguir a convenção legada do repositório por consistência local* — rejeitado: contradiria
  diretamente o Princípio III, que é não-negociável por definição da constituição.

## 5. DTO de resposta

**Decision**: Reaproveitar `RegisterClientResponseDto` (Id, Name, Email) como retorno tanto da
listagem quanto da consulta por id — nenhum DTO novo é criado.

**Rationale**: Os campos exigidos pelos critérios de aceite (id, nome, e-mail do usuário
associado) já são exatamente os campos existentes em `RegisterClientResponseDto`, e o mapeamento
AutoMapper `Client → RegisterClientResponseDto` já existe. Mesmo padrão já usado para `Game`
(`GameResponseDto` reaproveitado em create/get/list/update). Criar um DTO novo só para os
endpoints de leitura duplicaria uma estrutura idêntica sem ganho (Princípio V).

**Alternatives considered**:
- *Criar `ClientResponseDto` dedicado para leitura* — rejeitado: nome mais "correto"
  semanticamente, mas duplica 3 campos idênticos sem nenhuma diferença de forma; reavaliar
  apenas se os campos de leitura e escrita divergirem no futuro.

## 6. Migração de banco de dados

**Decision**: Nenhuma migration EF Core é necessária.

**Rationale**: Feature somente-leitura; tabela `Clients` e o FK para `AspNetUsers` já existem
(`Migrations/20250528135851_initial.cs`). O `Include(c => c.User)` introduzido na Seção 1 é uma
mudança de query em tempo de execução, não de schema.

## 7. Logging estruturado para o caso "cliente não encontrado"

**Decision**: `ClientService.GetClientByIdAsync` recebe `ILogger<ClientService>` no construtor e
emite um log `Information` estruturado (`"Cliente {ClientId} não encontrado"`) quando o id
consultado não existe (retorno `null`).

**Rationale**: O `SerilogRequestLoggingMiddleware` global já cobre logging estruturado de
requisição/resposta (método, rota, status, duração) para toda a API, mas não carrega contexto de
negócio (qual id foi consultado, por que a operação retornou 404). O Princípio IV da constituição
exige "contexto suficiente para diagnóstico" por operação relevante — este é o primeiro ponto de
injeção de `ILogger<T>` em um Service do projeto (identificado durante `/speckit-analyze` como uma
lacuna interpretativa a fechar), mantendo o restante do padrão atual inalterado (nenhum
`ILogger` em `ClientController` ou nos demais métodos).

**Alternatives considered**:
- *Confiar apenas no middleware global* — rejeitado após `/speckit-analyze`: satisfaz o
  "logging estruturado" em nível de HTTP, mas não o "contexto suficiente para diagnóstico" por
  operação de negócio exigido pela constituição para o caso de not-found.
