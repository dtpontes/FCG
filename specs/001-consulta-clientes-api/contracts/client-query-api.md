# API Contract: Consulta de Clientes

**Feature**: [spec.md](../spec.md) | **Data Model**: [data-model.md](../data-model.md)

Base route: `api/Client` (mesmo controller REST já existente, `ClientController`).
Autenticação: JWT Bearer (existente). Autorização: papel `Admin` obrigatório em ambos os
endpoints (FR-008).

## GET /api/Client

Lista todos os clientes cadastrados.

**Autorização**: `Authorize(Roles = "Admin")`

**Request**: sem parâmetros, sem corpo.

**Responses**:

| Status | Condição | Corpo |
|---|---|---|
| 200 OK | Sempre que a requisição é autorizada (mesmo com 0 clientes) | `{ "success": true, "data": [ { "id": number, "name": string, "email": string } ] }` — `data` é `[]` quando não há clientes cadastrados (FR-006) |
| 401 Unauthorized | Sem token válido | padrão do middleware de auth |
| 403 Forbidden | Token válido, mas sem papel `Admin` | padrão do middleware de auth |

## GET /api/Client/{id}

Consulta um cliente específico por id.

**Autorização**: `Authorize(Roles = "Admin")`

**Request**:

| Parâmetro | Local | Tipo | Obrigatório |
|---|---|---|---|
| `id` | rota | `long` | sim |

**Responses**:

| Status | Condição | Corpo |
|---|---|---|
| 200 OK | Cliente com o `id` informado existe | `{ "success": true, "data": { "id": number, "name": string, "email": string } }` |
| 404 Not Found | Nenhum cliente com o `id` informado (FR-005) | `{ "success": false, "errors": ["Not Found"] }` (formato padrão de `BaseController.Response`) |
| 401 Unauthorized | Sem token válido | padrão do middleware de auth |
| 403 Forbidden | Token válido, mas sem papel `Admin` | padrão do middleware de auth |

## Fora do escopo deste contrato

Criação, edição, remoção, filtros, busca por nome e paginação — conforme delimitado no spec.
