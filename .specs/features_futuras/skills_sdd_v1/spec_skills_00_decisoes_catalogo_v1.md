# Reconciliação de contratos e decisões das habilidades

> **Spec ID:** spec_skills_00_decisoes_catalogo_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P0
> **Type:** Governance
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** Nenhuma dependência de mudança de design
> **Blocks:** [03](spec_skills_03_dados_rank_readiness_v1.md), [04](spec_skills_04_passivas_capstones_v1.md), [05](spec_skills_05_mecanicas_ativas_v1.md), [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Documental; sem execução Unity
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

O catálogo ativo tem 66 nós (31 ativos/35 passivos), mas assets e documentação descrevem conjuntos diferentes. Há divergências sobre ranks de capstone, cooldown, habilidades dormentes e números do addendum.

**Escopo:** Produzir uma matriz de 66 nós e decisões explícitas para os conflitos, preservando IDs e regras humanas aceitas.

**Fora do escopo:** Não remover nós, alterar ranks, abolir dormentes ou aplicar novos números apenas com base no refinamento.

### Contexto que deve ser preservado

RPG de ação classless em Vaalara/Dornécia, cinco árvores e quatro slots; Farm/Town/Cave se complementam.
D&D inspira papéis e ferramentas, sem copiar classes, d20 ou regras de descanso.
Anya/Fonte/Fruto Mana não viram substitutos genéricos de recursos comuns; mistérios de lore permanecem.
IDs e save existentes são preservados. Estado canônico atual prevalece sobre contagens históricas.

### Phase 0 observada

Auditoria de código desta sessão: [relatório](../../../docs/validation/SKILLS_CAPABILITIES_AUDIT_2026_09_09.md).
Baseline analítica: [relatório de equilíbrio](../../../docs/validation/skills_balance_v1/BALANCE_REVIEW.md).
O fato que motiva esta fatia é o problema descrito acima; não é evidência de correção.
Na execução, reconfirmar os arquivos abaixo com `rg -n` para as classes/métodos citados.
Se houver drift, atualizar spec/plan/tasks antes de alterar comportamento. Não reexecutar backlog antigo para reverter CURRENT_STATE.

### Critérios de aceitação

- **AC01:** A matriz contém todos os 66 IDs ativos da baseline, sem duplicatas; qualquer mudança de contagem é explicada com comparação do catálogo atual.
- **AC02:** Cada conflito D01–D07 tem fonte, regra vigente, recomendação, consequência e estado de decisão; zero contradições ocultas.
- **AC03:** Toda proposta de habilidade explica cenário próprio, custo, contrajogo e vínculo com Vaalara; nada altera lore ou Fonte/Fruto Mana implicitamente.

# /speckit.plan

## Plan — contratos e solução

Entregável funcional produzido para revisão: `docs/refinements/a_implementar/ref_skill_catalog_functional_contract_v3.md`.
Cada linha contém nodeId, actionId, árvore, tier, ranks, variante, pré-requisitos, descrição prometida, executor/consumidor real, fórmula por rank, custo, cooldown, targeting, feedback, estado (operacional/parcial/dormente), fonte e decisão.
Não classificar registro no catálogo como efeito funcional.

Decisões a resolver:
- D01 APROVADA pelo usuário: cooldown por actionId, incluindo duplicatas e troca; segue tempo escalado, não congela com modal se o mundo continua rodando. Sem nova persistência entre processos. Implementação/testes na [fatia D01](../../a_implementar/spec_skills_03_shared_cooldowns_v1.md).
- D02: reconciliar rank máximo das capstones (direção de três versus limite dinâmico atual).
- D03: preservar decisões humanas 1.5/1.6 de dormentes e hooks pendentes; determinar como a UI explica os limites.
- D04: reconciliar Fluxo Lento percentual versus regeneração flat e stacking de crafting/comércio.
- D05: manter cura + revelação de Instinto e Kit de 30 HP/45 s sem item, fora de combate, conforme addendum, salvo nova decisão.
- D06: variantes e gatilhos das capstones; janela deslizante de magia versus reinício por inatividade.
- D07: Contra-ataque e Purify Taint já previstos são candidatos de restauração; Contenção de Circuito permanece hipótese condicional, sem novo nó.
Uma proposta recomendada não é decisão humana já aceita.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillTierRules.cs`
- `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
- `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md`
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`

Padrões aplicáveis: **refinement-authoring, skill-tree-authoring, decision-rule-extraction**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Matriz:** Ler os sete arquivos citados e o inventário da auditoria; confrontar cada ID com runtime e preencher a matriz.
2. **Decisões:** Registrar D01–D07 no contrato proposto, com recomendações do refinamento v2 e impacto nas specs consumidoras.
3. **Revisão de design:** Revisar a matriz por Farm/Town/Cave, builds legais e exclusividade das variantes; registrar objeções e alternativas.
4. **Fechamento:** Atualizar apenas os planos afetados pelas decisões aceitas; encaminhar decisão material pendente ao usuário com resultado concreto.

### Falhas, migração e limites

D01–D07 impedem apenas a promoção das fatias que dependem deles; corrigir vínculo de avatar e integridade de pontos não depende de escolher novos números.

Ausência de dependência obrigatória deve ser diagnosticada; não esconder falha por fallback de busca global.
Não publicar sucesso antes do efeito/commit correspondente. Não salvar referências Unity.
Sem edição manual de YAML. Wiring/assets exigem ferramenta Editor e evidência de geração.
Cave visited-level snapshots permanecem intactos; esta spec não autoriza regeneração.
Leitura de cena/testes antigos não prova Play Mode vigente.

# /speckit.tasks

## Tasks — revisão 1

Todos os itens estão **não executados**. Tarefas de pesquisa/decisão fecham o Plan; não autorizam preencher lacunas enquanto se implementa.

| Done | ID | Dependência | Arquivo/método e edição — ownership | Cobertura |
|---|---|---|---|---|
| [ ] | T01 | Dependências da spec | Matriz — Ler os sete arquivos citados e o inventário da auditoria; confrontar cada ID com runtime e preencher a matriz. | AC01 |
| [ ] | T02 | T01 | Decisões — Registrar D01–D07 no contrato proposto, com recomendações do refinamento v2 e impacto nas specs consumidoras. | AC02 |
| [ ] | T03 | T02 | Revisão de design — Revisar a matriz por Farm/Town/Cave, builds legais e exclusividade das variantes; registrar objeções e alternativas. | AC03 |
| [ ] | T04 | T03 | Fechamento — Atualizar apenas os planos afetados pelas decisões aceitas; encaminhar decisão material pendente ao usuário com resultado concreto. | AC01, AC02, AC03 |
| [ ] | T05 | T04 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03 |

### Testes e evidência esperada

- **MatrixMatchesLiveCatalog:** comparar conjunto exato de IDs; diferença vazia ou delta explicitamente aprovado.
- **DecisionCoverage:** todas as referências D01–D07 resolvem para uma entrada; nenhuma decisão pendente aparece como aprovada.

Verificação documental: conjuntos/IDs, fontes, decisões e links. Não há código para compilar nesta fatia.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- D01–D07 impedem apenas a promoção das fatias que dependem deles; corrigir vínculo de avatar e integridade de pontos não depende de escolher novos números.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.


