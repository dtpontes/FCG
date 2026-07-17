# API Contract: Edição de Cliente

**Feature**: [spec.md](../spec.md) | **Data Model**: [data-model.md](../data-model.md)

Base route: `api/Client` (mesmo controller REST já existente, `ClientController`).
Autenticação: JWT Bearer (existente). Autorização: papel `Admin` obrigatório (FR-008), mesma
restrição já aplicada em `GET /api/Client` e `GET /api/Client/{id}` (feature 001).

## PATCH /api/Client/{id}

Atualiza o nome de um cliente existente. Nenhum outro campo é aceito ou alterado por esta
operação (FR-005, FR-006).

**Autorização**: `Authorize(Roles = "Admin")`

**Request**:

| Parâmetro | Local | Tipo | Obrigatório |
|---|---|---|---|
| `id` | rota | `long` | sim |

Corpo (`application/json`):

```json
{
  "name": "Novo Nome do Cliente"
}
```

| Campo | Tipo | Obrigatório | Regras |
|---|---|---|---|
| `name` | string | sim | não vazio/não só espaços; 2–100 caracteres |

**Responses**:

| Status | Condição | Corpo |
|---|---|---|
| 200 OK | Cliente com o `id` informado existe e `name` é válido | `{ "success": true, "data": { "id": number, "name": string, "email": string } }` — `data` reflete o nome já atualizado (FR-002) |
| 400 Bad Request | `name` vazio, só espaços, ou fora do intervalo 2–100 caracteres (FR-004) | `{ "success": false, "errors": ["<mensagens de validação>"] }` |
| 404 Not Found | Nenhum cliente com o `id` informado (FR-003) | `{ "success": false, "errors": ["Not Found"] }` (formato padrão de `BaseController.Response`) |
| 401 Unauthorized | Sem token válido | padrão do middleware de auth |
| 403 Forbidden | Token válido, mas sem papel `Admin` | padrão do middleware de auth |

## Fora do escopo deste contrato

Alterar e-mail ou usuário associado (FR-005), reassociar o cliente a outro usuário (FR-006),
remoção de clientes (FR-007) — conforme delimitado no spec.
