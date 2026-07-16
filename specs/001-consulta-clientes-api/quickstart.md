# Quickstart: Consulta de Clientes via API

**Feature**: [spec.md](spec.md) | **Contract**: [contracts/client-query-api.md](contracts/client-query-api.md)

Guia para validar manualmente, de ponta a ponta, que a listagem e a consulta por id de clientes
funcionam conforme os critérios de aceite do spec.

## Pré-requisitos

- Docker e Docker Compose instalados.
- Solução subida localmente:

  ```sh
  docker-compose up --build
  ```

- Swagger UI disponível em `http://localhost:8080/swagger`.
- Um usuário com papel `Admin` já existente (ou criado via o fluxo de cadastro de usuário já
  existente na API, atribuindo o papel `Admin`) e um token JWT obtido para esse usuário.

## Cenário 1 — Listagem com base vazia (FR-006)

1. Em um ambiente/banco sem nenhum cliente cadastrado, chamar:

   ```http
   GET /api/Client
   Authorization: Bearer <token-admin>
   ```

2. **Esperado**: `200 OK` com `data: []` (lista vazia, não erro).

## Cenário 2 — Cadastro seguido de listagem (US1)

1. Cadastrar um cliente via endpoint já existente:

   ```http
   POST /api/Client/register
   ```

2. Chamar `GET /api/Client` com o mesmo token admin.
3. **Esperado**: `200 OK` com `data` contendo o cliente recém-criado, incluindo `id`, `name` e
   `email` (igual ao e-mail do usuário cadastrado no passo 1).

## Cenário 3 — Consulta por id existente (US2)

1. Usando o `id` retornado no Cenário 2, chamar:

   ```http
   GET /api/Client/{id}
   Authorization: Bearer <token-admin>
   ```

2. **Esperado**: `200 OK` com `data` contendo `id`, `name` e `email` do cliente consultado.

## Cenário 4 — Consulta por id inexistente (FR-005)

1. Chamar `GET /api/Client/{id}` com um `id` que certamente não existe (ex.: `999999`).
2. **Esperado**: `404 Not Found`, sem detalhes de erro interno expostos.

## Cenário 5 — Controle de acesso (FR-008)

1. Repetir os Cenários 1–4 sem token, ou com um token de usuário sem papel `Admin`.
2. **Esperado**: `401 Unauthorized` (sem token) ou `403 Forbidden` (token válido, sem papel
   `Admin`) — a listagem e a consulta nunca retornam dados nesses casos.

## Validação automatizada equivalente

Os cenários acima têm cobertura equivalente em testes de unidade (`ClientServiceTests.cs`,
`FCG.Service.Tests`) para a lógica de negócio (lista vazia, cliente existente, cliente
inexistente) — ver tarefas geradas por `/speckit-tasks`. O controle de acesso (Cenário 5) é
validado manualmente via Swagger/HTTP, pois é uma responsabilidade de middleware/framework, não
de regra de negócio no Service.
