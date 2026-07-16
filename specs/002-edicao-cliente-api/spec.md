# Feature Specification: Edição de Cliente

**Feature Branch**: `002-edicao-cliente-api`

**Created**: 2026-07-16

**Status**: Draft

**Input**: User description: "Feature: Edição de Cliente. Contexto: já é possível cadastrar e consultar clientes, mas não há como corrigir os dados de um cliente já cadastrado. Áreas internas/admin precisam corrigir o nome de um cliente. User story: Como consumidor da API (admin), quero atualizar o nome de um cliente existente para corrigir dados cadastrais. Critérios de aceite: dado um cliente existente e um nome válido, quando a atualização é solicitada, então o novo nome é persistido e a resposta indica sucesso; dado um id inexistente, quando a atualização é solicitada, então retorna 404; dado um nome inválido (vazio/curto), então retorna erro de validação (400). Fora de escopo: alterar e-mail ou o usuário associado (domínio de usuários), reassociar o cliente a outro usuário, e remoção de clientes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Corrigir o nome de um cliente existente (Priority: P1)

Como consumidor da API (área interna/admin), quero atualizar o nome de um cliente já
cadastrado para corrigir dados cadastrais incorretos ou desatualizados.

**Why this priority**: É a única capacidade solicitada nesta feature e resolve diretamente a dor
relatada (impossibilidade de corrigir um cliente já cadastrado). Sem ela, um erro de cadastro no
nome permanece incorrigível pela API.

**Independent Test**: Pode ser testada isoladamente cadastrando um cliente, solicitando a
atualização do nome com um valor válido e verificando que uma consulta subsequente ao mesmo
cliente reflete o novo nome; e também solicitando atualização para um id inexistente e para um
nome inválido, verificando as respostas de erro correspondentes.

**Acceptance Scenarios**:

1. **Given** um cliente existente e um nome válido, **When** a atualização é solicitada,
   **Then** o novo nome é persistido e a resposta indica sucesso, retornando os dados
   atualizados do cliente (id, nome, e-mail do usuário associado).
2. **Given** um id de cliente inexistente, **When** a atualização é solicitada, **Then** o
   sistema retorna 404 (não encontrado) e nenhuma alteração é persistida.
3. **Given** um nome inválido (vazio, composto apenas por espaços, ou muito curto), **When** a
   atualização é solicitada, **Then** o sistema retorna um erro de validação (400) e nenhuma
   alteração é persistida.

---

### Edge Cases

- Quando o nome enviado for idêntico ao nome atual do cliente, a atualização é aceita e tratada
  como bem-sucedida (operação idempotente), não como erro.
- Quando o nome enviado contiver apenas espaços em branco, é tratado como inválido (equivalente
  a vazio) e retorna erro de validação (400).
- Quando o nome enviado exceder o tamanho máximo já suportado para o campo, retorna erro de
  validação (400), em vez de uma falha genérica de persistência.
- Uma tentativa de atualização que também tente alterar e-mail ou usuário associado não tem
  efeito sobre esses dados — apenas o nome é considerado por esta operação.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir atualizar o nome de um cliente existente, identificado
  por id.
- **FR-002**: Dado um id existente e um nome válido, o sistema DEVE persistir o novo nome e
  retornar uma resposta de sucesso com os dados atualizados do cliente (id, nome, e-mail do
  usuário associado).
- **FR-003**: Dado um id de cliente inexistente, o sistema DEVE retornar 404 (não encontrado) e
  NÃO DEVE persistir nenhuma alteração.
- **FR-004**: Dado um nome inválido (vazio, somente espaços, ou fora dos limites de tamanho
  aceitos), o sistema DEVE retornar um erro de validação (400) e NÃO DEVE persistir nenhuma
  alteração.
- **FR-005**: Esta operação NÃO DEVE permitir alterar o e-mail do cliente ou qualquer dado do
  usuário associado (domínio de usuários).
- **FR-006**: Esta operação NÃO DEVE permitir reassociar o cliente a um usuário diferente.
- **FR-007**: Esta operação NÃO DEVE remover clientes — remoção permanece fora do escopo.
- **FR-008**: A operação de atualização DEVE exigir autenticação e restringir o acesso a
  usuários com papel/permissão de admin (ou equivalente interno), na mesma linha do controle de
  acesso já aplicado às operações de consulta de clientes.

### Key Entities

- **Cliente**: Entidade já existente (cadastro e consulta implementados em feature anterior).
  Esta feature só permite corrigir o atributo nome; id e usuário associado (e-mail) permanecem
  imutáveis por esta operação.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Uma área interna/admin consegue corrigir o nome de um cliente existente em uma
  única operação, e a mudança é refletida imediatamente em consultas subsequentes a esse
  cliente.
- **SC-002**: 100% das tentativas de atualização com um id de cliente inexistente resultam em
  uma resposta de "não encontrado", sem nenhuma alteração persistida.
- **SC-003**: 100% das tentativas de atualização com um nome inválido resultam em um erro de
  validação claro, sem nenhuma alteração persistida.
- **SC-004**: 100% das tentativas de atualização não têm nenhum efeito sobre o e-mail ou o
  usuário associado do cliente — esses dados permanecem inalterados após a operação.

## Assumptions

- O controle de acesso (autenticação/autorização) desta operação reutiliza a mesma restrição a
  papel admin já definida para as operações de consulta de clientes (feature anterior); esta
  feature não introduz uma nova regra de permissão.
- Um nome é considerado "muito curto" quando tiver menos de 2 caracteres (após remover espaços
  em branco nas extremidades); este limite mínimo é um valor razoável assumido na ausência de
  um critério explícito do usuário, e pode ser ajustado no planejamento técnico se necessário.
- O limite máximo de tamanho do nome é o mesmo já suportado pelo cadastro de cliente existente;
  um valor que exceda esse limite é tratado como erro de validação (400), não como falha de
  persistência.
- A resposta de sucesso inclui os dados atualizados do cliente (id, nome, e-mail do usuário
  associado), no mesmo formato já usado pelas demais operações de cliente, para consistência da
  API.
- Alterar e-mail, usuário associado, remoção de clientes, filtros e paginação estão
  explicitamente fora do escopo desta feature, conforme informado pelo usuário.
