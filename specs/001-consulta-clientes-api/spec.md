# Feature Specification: Consulta de Clientes via API

**Feature Branch**: `001-consulta-clientes-api`

**Created**: 2026-07-15

**Status**: Draft

**Input**: User description: "Feature: Consulta de Clientes via API. Contexto: hoje a API permite cadastrar clientes, mas não há como consultá-los. Áreas internas/admin precisam visualizar os clientes já cadastrados para suporte e conferência. User stories: (1) Como consumidor da API (admin), quero listar todos os clientes cadastrados para visualizar a base de clientes. (2) Como consumidor da API, quero consultar um cliente específico por id para ver seus dados (nome e usuário/e-mail associado). Critérios de aceite: a listagem retorna todos os clientes com dados básicos (id, nome, e-mail do usuário associado); a consulta por id existente retorna os dados do cliente; a consulta por id inexistente retorna 404; quando não houver clientes, a listagem retorna lista vazia. Fora de escopo: cadastro (já existe), edição e remoção de clientes, filtros, busca por nome e paginação, e qualquer alteração no domínio de usuários."

## Clarifications

### Session 2026-07-15

- Q: Qual controle de acesso deve ser aplicado aos dois novos endpoints de consulta de clientes, considerando que expõem e-mail (dado pessoal) e que a US1 menciona "admin" enquanto a US2 diz apenas "consumidor da API"? → A: Restrito a papel admin (ambos) — listagem e consulta por id exigem autenticação + papel/permissão de admin (ou equivalente interno), reutilizando o mecanismo de auth já existente na API.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Listar todos os clientes cadastrados (Priority: P1)

Como consumidor da API (área interna/admin), quero listar todos os clientes cadastrados para
visualizar a base de clientes existente, com fins de suporte e conferência.

**Why this priority**: Sem uma listagem, não há nenhuma forma de saber quais clientes existem
no sistema — esta é a capacidade mínima que resolve a dor relatada (impossibilidade de
consultar o que já foi cadastrado). Sozinha já entrega valor completo de visibilidade da base.

**Independent Test**: Pode ser testada isoladamente chamando a operação de listagem e
verificando que os dados básicos de todos os clientes cadastrados são retornados, incluindo o
caso em que não há nenhum cliente cadastrado.

**Acceptance Scenarios**:

1. **Given** existem clientes cadastrados no sistema, **When** a listagem de clientes é
   solicitada, **Then** todos os clientes cadastrados são retornados, cada um com id, nome e
   e-mail do usuário associado.
2. **Given** não existe nenhum cliente cadastrado no sistema, **When** a listagem de clientes é
   solicitada, **Then** uma lista vazia é retornada (não um erro).

---

### User Story 2 - Consultar um cliente específico por id (Priority: P2)

Como consumidor da API (área interna/admin), quero consultar um cliente específico por id para
ver seus dados (nome e usuário/e-mail associado), sem precisar percorrer a listagem completa.

**Why this priority**: Complementa a listagem permitindo conferência pontual e direcionada de
um cliente já identificado (por exemplo, a partir de um id obtido em outro fluxo de suporte).
Depende conceitualmente da existência de clientes, mas é independentemente testável e utilizável
mesmo sem a User Story 1 implementada.

**Independent Test**: Pode ser testada isoladamente chamando a operação de consulta com um id
de cliente existente e verificando o retorno dos dados corretos, e com um id inexistente
verificando o retorno de "não encontrado".

**Acceptance Scenarios**:

1. **Given** existe um cliente cadastrado com um determinado id, **When** esse id é consultado,
   **Then** os dados do cliente (id, nome e e-mail do usuário associado) são retornados.
2. **Given** não existe cliente cadastrado com um determinado id, **When** esse id é consultado,
   **Then** o sistema informa que o cliente não foi encontrado (404), sem expor erro genérico
   interno.

---

### Edge Cases

- Quando não houver clientes cadastrados, a listagem retorna lista vazia, não erro.
- Quando o id consultado não corresponder a nenhum cliente existente, o sistema retorna 404
  (não encontrado).
- Quando o cliente existente não possuir um usuário/e-mail associado íntegro (dado
  inconsistente pré-existente), a consulta retorna os dados disponíveis sem falhar — o
  tratamento desse dado herdado está fora do escopo desta feature.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir listar todos os clientes cadastrados.
- **FR-002**: Cada item da listagem DEVE incluir, no mínimo: id do cliente, nome do cliente e
  e-mail do usuário associado.
- **FR-003**: O sistema DEVE permitir consultar um cliente específico a partir do seu id.
- **FR-004**: Uma consulta por um id de cliente existente DEVE retornar os dados desse cliente
  (id, nome e e-mail do usuário associado).
- **FR-005**: Uma consulta por um id de cliente inexistente DEVE indicar claramente que o
  cliente não foi encontrado (HTTP 404), sem expor detalhes internos do erro.
- **FR-006**: Quando não houver nenhum cliente cadastrado, a listagem DEVE retornar uma lista
  vazia, e não um erro.
- **FR-007**: A listagem e a consulta por id NÃO DEVEM permitir criar, editar ou remover
  clientes, nem alterar dados do domínio de usuários — essas operações permanecem fora do
  escopo desta feature.
- **FR-008**: Ambos os endpoints (listagem e consulta por id) DEVEM exigir autenticação e
  restringir o acesso a usuários com papel/permissão de admin (ou equivalente interno),
  reutilizando o mecanismo de autenticação/autorização já existente na API. Chamadas sem essa
  permissão DEVEM ser rejeitadas.

### Key Entities

- **Cliente**: Representa um cliente já cadastrado no sistema. Atributos relevantes para esta
  feature: identificador único (id), nome e referência ao usuário associado (do qual se expõe o
  e-mail). O cadastro do cliente já existe fora do escopo desta feature; aqui ele é apenas
  consultado.
- **Usuário associado**: Entidade de usuário já existente, referenciada pelo cliente unicamente
  para expor o e-mail nos resultados de listagem e consulta. Nenhuma alteração no domínio de
  usuários faz parte desta feature.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Uma área interna/admin consegue visualizar a lista completa de clientes
  cadastrados em uma única consulta, sem necessidade de passos adicionais.
- **SC-002**: 100% das consultas por um id de cliente existente retornam os dados corretos
  (id, nome, e-mail do usuário associado) desse cliente.
- **SC-003**: 100% das consultas por um id de cliente inexistente resultam em uma resposta de
  "não encontrado", claramente distinguível de uma falha do sistema.
- **SC-004**: Quando a base de clientes está vazia, a área consumidora recebe uma lista vazia e
  consegue distinguir esse estado (base vazia) de um erro do sistema, sem tratamento especial.

## Assumptions

- A listagem retorna os clientes em uma ordem padrão do sistema (por exemplo, ordem de
  cadastro); ordenação customizada, filtros, busca por nome e paginação estão fora do escopo,
  conforme informado pelo usuário.
- O "usuário/e-mail associado" referenciado nos critérios de aceite corresponde ao usuário já
  vinculado ao cliente no cadastro existente; nenhuma alteração no domínio de usuários é
  necessária ou está incluída nesta feature.
- Cadastro, edição, remoção de clientes, filtros, busca por nome e paginação estão
  explicitamente fora do escopo desta feature, conforme informado pelo usuário.
