<!--
Sync Impact Report
==================
Version change: [TEMPLATE] → 1.0.0
Rationale: Initial ratification — first concrete constitution for this project (MINOR/MAJOR
not applicable to a template→real transition; treated as the baseline 1.0.0).

Modified principles: N/A (template placeholders replaced with concrete principles)
Added sections:
  - I. Arquitetura em Camadas Respeitada
  - II. Spec Antes de Código (SDD)
  - III. Testes de Regras de Negócio
  - IV. Consistência de API
  - V. Simplicidade (YAGNI)
  - Stack Técnica Fixa (Section 2)
  - Fluxo de Desenvolvimento (Section 3)
  - Governance
Removed sections: none

Templates requiring updates:
  - .specify/templates/plan-template.md ✅ no changes needed (Constitution Check gate already
    generic and reads from this file at plan time)
  - .specify/templates/spec-template.md ✅ no changes needed (spec structure is stack-agnostic)
  - .specify/templates/tasks-template.md ✅ no changes needed (task categorization already
    supports test tasks; project-specific paths are filled in per-feature by /speckit-tasks)
  - .claude/skills/speckit-*/SKILL.md ✅ reviewed, no CLAUDE-only or agent-specific references
    found requiring changes

Follow-up TODOs: none
-->

# FCG Constitution

## Core Principles

### I. Arquitetura em Camadas Respeitada

A solução é organizada nos projetos `FCG.Domain`, `FCG.Infrastructure`, `FCG.Service` e
`FCG.Presentation`. As dependências DEVEM apontar sempre para dentro: `FCG.Presentation` pode
depender de `FCG.Service`; `FCG.Service` pode depender de `FCG.Domain` e `FCG.Infrastructure`
(via abstrações); `FCG.Infrastructure` pode depender de `FCG.Domain`. `FCG.Domain` NÃO PODE
depender de `FCG.Infrastructure`, `FCG.Service` ou `FCG.Presentation` sob nenhuma circunstância.
Regras de negócio, entidades e invariantes DEVEM residir no `FCG.Domain`; `FCG.Infrastructure`
contém apenas detalhes técnicos (persistência, integrações externas, EF Core); `FCG.Service`
orquestra casos de uso; `FCG.Presentation` expõe endpoints (REST/GraphQL) e não contém lógica
de negócio.

**Rationale**: Isolar o Domain de detalhes de infraestrutura garante que as regras de negócio
permaneçam testáveis e estáveis mesmo quando banco de dados, frameworks ou APIs externas mudam.

### II. Spec Antes de Código (SDD)

Nenhuma feature é implementada sem uma spec aprovada (`spec.md` gerado via `/speckit-specify`
e revisado). Ambiguidades de requisito DEVEM ser marcadas explicitamente como
`[NEEDS CLARIFICATION]` e resolvidas (via `/speckit-clarify` ou decisão explícita do usuário)
antes do planejamento técnico avançar — NUNCA adivinhadas ou preenchidas com suposições
silenciosas.

**Rationale**: Especificar antes de codar evita retrabalho, alinha expectativas de negócio e
técnicas antes do custo de implementação, e cria rastreabilidade entre requisito e código.

### III. Testes de Regras de Negócio

Toda regra de negócio DEVE ter cobertura de teste em `FCG.Service.Tests`. Os nomes de teste
DEVEM seguir o padrão descritivo `Metodo_Cenario_Resultado` (ex.:
`CriarCliente_DocumentoInvalido_DeveLancarExcecao`). Testes cobrem tanto o caminho feliz quanto
cenários de erro/borda relevantes ao requisito da spec.

**Rationale**: Um padrão de nomenclatura consistente torna a suíte de testes autoexplicativa e
facilita localizar rapidamente qual regra de negócio quebrou e em que cenário.

### IV. Consistência de API

Endpoints REST e GraphQL DEVEM seguir convenções consistentes de nomenclatura, verbos HTTP e
formato de resposta/erro em toda a solução. Toda operação relevante DEVE emitir logging
estruturado via Serilog (nível apropriado, contexto suficiente para diagnóstico). Todo endpoint
REST DEVE estar documentado no Swagger (sumário, parâmetros, respostas possíveis).

**Rationale**: Consistência entre endpoints reduz a curva de aprendizado de consumidores da
API; logging estruturado e documentação viva são essenciais para operar e depurar o sistema em
produção.

### V. Simplicidade (YAGNI)

Antes de criar uma nova abstração, camada ou padrão, o padrão existente equivalente no código
DEVE ser reutilizado. Complexidade adicional (interfaces genéricas, camadas extras, configuração
especulativa) só é justificada por uma necessidade concreta e atual da tarefa — não por
necessidades hipotéticas futuras. Quando uma violação de simplicidade for necessária, ela DEVE
ser registrada e justificada na seção "Complexity Tracking" do plano da feature.

**Rationale**: Abstrações prematuras aumentam o custo de manutenção sem benefício comprovado;
manter o código simples acelera revisão, testes e onboarding.

## Stack Técnica Fixa

- **Linguagem/Runtime**: .NET 9 / C# 13.
- **Persistência**: SQL Server via EF Core, com migrations versionadas em controle de código —
  toda alteração de schema passa por uma migration, nunca por alteração manual do banco.
- **APIs**: REST (documentado via Swagger) e GraphQL.
- **Logging**: Serilog, estruturado, configurado em `FCG.Presentation`.
- **Containerização**: Docker/Docker Compose para execução local e deploy.

Mudança de qualquer item desta stack (ex.: troca de ORM, banco de dados ou framework de log)
é uma decisão arquitetural que exige atualização desta constituição, não apenas do código.

## Fluxo de Desenvolvimento

Toda feature segue o fluxo Spec Kit: `/speckit-specify` → `/speckit-clarify` (se houver
ambiguidade) → `/speckit-plan` → `/speckit-tasks` → `/speckit-implement`. O `Constitution Check`
do plano DEVE validar aderência aos cinco princípios acima antes da Fase 0 e novamente após o
design da Fase 1. Pull requests que violem a arquitetura em camadas (Princípio I) ou que
introduzam regra de negócio sem teste correspondente (Princípio III) NÃO DEVEM ser aprovados.

## Governance

Esta constituição prevalece sobre qualquer prática, convenção informal ou preferência
individual em conflito. Emendas exigem: (1) documentação da mudança e motivação, (2) atualização
do número de versão conforme versionamento semântico (MAJOR: remoção ou redefinição incompatível
de princípio; MINOR: novo princípio ou seção adicionada; PATCH: esclarecimento/redação), e (3)
verificação de que os templates dependentes (`plan-template.md`, `spec-template.md`,
`tasks-template.md`) permanecem consistentes com o texto atualizado. Toda revisão de código e
todo plano de feature DEVEM verificar conformidade com os princípios aqui descritos;
complexidade que os viole deve ser justificada explicitamente ou removida.

**Version**: 1.0.0 | **Ratified**: 2026-07-15 | **Last Amended**: 2026-07-15
