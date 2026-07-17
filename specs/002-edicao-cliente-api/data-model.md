# Data Model: Edição de Cliente

**Feature**: [spec.md](spec.md) | **Research**: [research.md](research.md)

Esta feature não altera nenhuma entidade, tabela ou coluna. A única mudança de schema é o
`UPDATE` do valor de uma coluna já existente (`Clients.Name`).

## Entidade existente (schema inalterado)

### Client (`src/FCG.Domain/Entities/Client.cs`)

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `long` | Identificador único do cliente (imutável por esta operação) |
| `Name` | `string` | Nome do cliente — **único campo alterado por esta feature** |
| `UserId` | `string` | FK para `AspNetUsers.Id` (imutável por esta operação — FR-006) |
| `User` | `IdentityUser` | Usuário associado (imutável por esta operação — FR-005) |

Nenhuma mudança na entidade, no mapeamento EF Core (`DbContext.cs`) ou nas migrations.

## Novo DTO de requisição

### UpdateClientDto (`src/FCG.Service/DTO/Request/UpdateClientDto.cs`, novo)

| Campo | Tipo | Regras de validação |
|---|---|---|
| `Name` | `string` (obrigatório) | Não vazio/não só espaços; mínimo 2 caracteres (após trim); máximo 100 caracteres (mesmo limite do schema) |

Deliberadamente não inclui `Email`, `Password` ou qualquer referência ao usuário associado —
essa ausência estrutural é o que garante FR-005/FR-006 (impossível enviar esses campos nesta
operação).

## DTO de resposta (reaproveitado, sem alteração)

### RegisterClientResponseDto (`src/FCG.Service/DTO/Response/RegisterClientResponseDto.cs`)

Mesma forma já usada por `POST /api/Client/register`, `GET /api/Client` e
`GET /api/Client/{id}` (feature 001): `Id`, `Name`, `Email`. Retornado com os valores já
atualizados em caso de sucesso desta operação.

## Estados / transições

- **Cliente existe, nome válido** → `Name` atualizado, entidade permanece no mesmo estado
  "cadastrado" (não há máquina de estados formal para `Client`).
- **Cliente existe, nome idêntico ao atual** → operação idempotente, tratada como sucesso (sem
  transição real de estado, mas sem erro).
- **Cliente não existe** → nenhuma transição; nenhuma linha é afetada.
- **Nome inválido** → nenhuma transição; nenhuma linha é afetada.
