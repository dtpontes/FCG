# Quickstart: Edição de Cliente

**Feature**: [spec.md](spec.md) | **Contract**: [contracts/client-update-api.md](contracts/client-update-api.md)

Guia para validar manualmente, de ponta a ponta, que a atualização do nome de um cliente
funciona conforme os critérios de aceite do spec.

## Pré-requisitos

- Ambiente local rodando (`docker-compose up --build` ou execução equivalente), Swagger UI em
  `http://localhost:8080/swagger`.
- Um token JWT de um usuário com papel `Admin` (mesmo fluxo usado na feature 001: `POST
  /api/Users/register` + `POST /api/Users/login`).
- Um cliente já cadastrado (`POST /api/Client/register`), com o `id` retornado anotado.

## Cenário 1 — Atualização com nome válido (US1, cenário 1)

1. Chamar:

   ```http
   PATCH /api/Client/{id}
   Authorization: Bearer <token-admin>
   Content-Type: application/json

   { "name": "Nome Corrigido" }
   ```

2. **Esperado**: `200 OK` com `data.name == "Nome Corrigido"` e `data.email` inalterado.
3. Chamar `GET /api/Client/{id}` (feature 001) e confirmar que o nome atualizado é o mesmo
   retornado no passo anterior (SC-001).

## Cenário 2 — Id inexistente (US1, cenário 2 / FR-003)

1. Chamar `PATCH /api/Client/999999` com um corpo válido (`{ "name": "Qualquer Nome" }`).
2. **Esperado**: `404 Not Found`, sem nenhuma alteração em qualquer cliente existente.

## Cenário 3 — Nome inválido (US1, cenário 3 / FR-004)

1. Chamar `PATCH /api/Client/{id}` (id existente) com cada um dos corpos abaixo, um de cada vez:
   - `{ "name": "" }` (vazio)
   - `{ "name": "  " }` (só espaços)
   - `{ "name": "A" }` (abaixo do mínimo de 2 caracteres)
   - `{ "name": "<string com mais de 100 caracteres>" }` (acima do máximo)
2. **Esperado**: `400 Bad Request` em todos os casos, com mensagens de validação; `GET
   /api/Client/{id}` confirma que o nome original permanece inalterado.

## Cenário 4 — Idempotência (edge case)

1. Chamar `PATCH /api/Client/{id}` com o mesmo nome já atual do cliente.
2. **Esperado**: `200 OK`, tratado como sucesso (não como erro).

## Cenário 5 — Controle de acesso (FR-008)

1. Repetir o Cenário 1 sem token, e novamente com um token de usuário sem papel `Admin`.
2. **Esperado**: `401 Unauthorized` (sem token) ou `403 Forbidden` (token válido, sem papel
   `Admin`) — o nome nunca é alterado nesses casos.

## Validação automatizada equivalente

Os Cenários 1–3 têm cobertura equivalente em testes de unidade (`ClientServiceTests.cs`,
`FCG.Service.Tests`) para a lógica de negócio (`UpdateClientNameAsync`: sucesso, id inexistente,
nome inválido) — ver tarefas geradas por `/speckit-tasks`. O Cenário 5 (controle de acesso) é
validado manualmente via Swagger/HTTP, pois é responsabilidade de middleware/framework, não de
regra de negócio no Service.
