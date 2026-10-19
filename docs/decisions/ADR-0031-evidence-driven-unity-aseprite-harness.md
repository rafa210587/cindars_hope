---
doc_type: adr
status: accepted
adr_id: ADR-0031
title: Harness Unity e Aseprite orientado por evidência
date: 2026-09-10
source_documents:
  - CLAUDE.md
  - docs/project/CURRENT_STATE.md
  - docs/validation/UNITY_ASEPRITE_SKILLS_RESEARCH_20260910.md
supersedes: []
superseded_by: []
applies_to:
  - agent-execution
  - editor-tooling
  - art-pipeline
---

# ADR-0031 — Harness Unity e Aseprite orientado por evidência

## Status

**accepted** para a refatoração do harness autorizada pelo humano em 2026-09-10.
A escolha e instalação de fornecedor MCP permanecem pendentes de piloto; este ADR não
declara compatibilidade operacional, integração instalada nem aceitação de gameplay.

## Context

O projeto já possui planejamento por spec, revisão independente por risco, runners Unity,
autoria Aseprite e revisão visual. A pesquisa externa encontrou alternativas úteis, mas também
workflows que pressupõem iOS, URP, serviços online, edição direta de YAML ou ferramentas ausentes.
As necessidades atuais incluem wiring reproduzível, capturas de cenas, preservação de animação,
diagnóstico de layout e evidência real de performance. Adicionar catálogos inteiros duplicaria decisões.

## Decision

1. Manter `.claude/` como fonte canônica, com catálogo sob demanda e cópias Codex geradas.
   Avaliar recursos externos por lacuna concreta, origem/licença, compatibilidade e evidência;
   não importar instruções em massa nem substituir o ciclo de specs por outro framework.
2. Separar instrução, agente e capacidade: skill descreve o procedimento; agente define ownership;
   CLI/MCP fornece operações. Nenhum texto instalado comprova que uma ferramenta está disponível.
3. Preparar operação Unity MCP independente de fornecedor. Comparar Coplay, IvanMurzak e Unity
   oficial usando o mesmo piloto. Preferir conexão local explícita, versão fixada e operações
   restritas ao escopo; conexão local não significa execução offline do modelo.
4. Um owner controla cada Editor/projeto. Preservar cenas dirty e instância do usuário; confirmar
   identidade e prontidão após reload. Timeout de uma mutação exige leitura do estado antes de retry.
   Usar generators/validators existentes para alterações persistentes reproduzíveis.
5. Manter Aseprite CLI/Lua como base. MCP é adaptador opcional, sem mudar o contrato de origem,
   layers/cels, palette, tags, durations, slices e export. Inspecionar efeitos auxiliares do adaptador.
6. Fortalecer agentes existentes, sem criar outro agente genérico de Unity ou pixel art.
   Auditor continua sem implementar correções. Profiling diferencia hipótese estática de medição;
   UI exige ação e resultado observados, além da hierarquia visível.
7. Preservar Point/None/no-mipmaps, localization local, Canvas/projection, event bus e save IDs.
   Sugestões externas de compressão, serviços, packages e arquitetura não alteram esses contratos.

## Consequences

### Positive

- Mais evidência de Editor, layout e assets com ownership explícito.
- Menor dependência de fornecedor e menos instruções concorrentes.
- Diagnóstico proporcional, sem promessas de qualidade baseadas em quantidade de tools ou agentes.

### Negative

- Adaptadores e compatibilidade precisam de manutenção quando as versões mudarem.
- As capacidades MCP ainda precisam de teste no ambiente; documentação não mede benefício real.

### Operational

- Skills novas: `unity-mcp-operations` e `unity-performance-profiling`.
- Refatorar agentes de wiring/performance e encaminhamentos de validação, HUD e Aseprite.
- Executar geração, paridade, idempotência, links e cenários do harness; registrar baseline de docs.
- Piloto futuro: identidade/console/cena; validator existente; captura apropriada; reload;
  mutação isolada e sua persistência; timeout sem duplicação; preservação de cena dirty.
  Registrar versões, comandos, duração observada, arquivos afetados e limitações por candidato.

## Applies To

Skills, agentes, seleção de integrações e evidência de tooling/art do projeto.

## NOT Applicable To

Mudanças de gameplay, save, packages, render pipeline ou instalação de servidores nesta entrega.
ADRs de arquitetura/gameplay existentes continuam válidos; nenhum é substituído.

## Source Documents

- [Pesquisa e decisões por recurso](../validation/UNITY_ASEPRITE_SKILLS_RESEARCH_20260910.md).
- [Contexto atual](../project/CURRENT_STATE.md).
- [Padrão do harness](../../.claude/HARNESS_AUTHORING_STANDARD.md).
- [ADR-0008: edição de assets](ADR-0008-unity-yaml-editing-policy.md).

## Migration Notes

Instruções próprias derivadas de análise das fontes; nenhum pacote externo foi copiado ou instalado.
Proveniência/licença da adaptação anterior de `pixel-art-animator` permanece intacta.
Não há alteração de game rules: são decisões operacionais do harness, sem mudança de gameplay.
