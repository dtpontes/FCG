# Data Model: Consulta de Clientes via API

**Feature**: [spec.md](spec.md) | **Research**: [research.md](research.md)

Esta feature é somente-leitura: nenhuma entidade, tabela ou coluna é criada, alterada ou
removida. As entidades abaixo já existem em `FCG.Domain`/`FCG.Infrastructure` e são apenas
consultadas.

## Entidades existentes (inalteradas)

### Client (`src/FCG.Domain/Entities/Client.cs`)

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `long` (herdado de `Base`) | Identificador único do cliente |
| `Name` | `string` | Nome do cliente |
| `UserId` | `string` | FK para `AspNetUsers.Id` (Identity) |
| `User` | `IdentityUser` | Navegação para o usuário associado (ASP.NET Core Identity) |

**Relacionamento**: `Client 1 — 1 IdentityUser`, configurado em
`DbContext.cs`: `HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)`
(`DeleteBehavior.Cascade`).

**Regra de acesso a dados (nova nesta feature)**: leituras via os novos endpoints DEVEM carregar
`User` (ver [research.md § 1](research.md)) para que `Email` esteja disponível — sem introduzir
nenhum novo campo na entidade.

### IdentityUser (ASP.NET Core Identity — `AspNetUsers`)

Entidade padrão do framework, sem tipo customizado no projeto. Campo relevante para esta
feature: `Email` (já populado no cadastro existente de cliente/usuário).

## Forma de resposta (DTO já existente, reaproveitado)

### RegisterClientResponseDto (`src/FCG.Service/DTO/Response/RegisterClientResponseDto.cs`)

| Campo | Tipo | Origem (mapeamento AutoMapper existente) |
|---|---|---|
| `Id` | `long` | `Client.Id` |
| `Name` | `string` | `Client.Name` |
| `Email` | `string` | `Client.User.Email` |

Usado tanto para o item de listagem (`GET /api/Client`) quanto para a consulta individual
(`GET /api/Client/{id}`) — ver [research.md § 5](research.md) para a justificativa de reuso.

## Interfaces novas (contratos internos, não banco de dados)

### `IClientRepository` (novo — `FCG.Domain.Repositories`)

```csharp
public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByIdWithUserAsync(long id);
    Task<IEnumerable<Client>> GetAllWithUserAsync();
}
```

Estende o repositório genérico já existente; não introduz nenhuma dependência de EF Core no
Domain (a implementação com `Include` fica em `FCG.Infrastructure`).

## Estados / transições

Não aplicável — feature não cria, atualiza nem remove `Client`; apenas leitura dos estados já
existentes (cliente existe / cliente não existe / nenhum cliente cadastrado), já cobertos nos
cenários de aceite do spec.
