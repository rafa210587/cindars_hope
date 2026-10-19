# Dados de execução, ranks e estado consultável das habilidades

> **Spec ID:** spec_skills_03_dados_rank_readiness_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P1
> **Type:** Data
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [00](spec_skills_00_decisoes_catalogo_v1.md), [01](spec_skills_01_avatar_execucao_v1.md), [02](spec_skills_02_pontos_respec_save_v1.md)
> **Blocks:** [04](spec_skills_04_passivas_capstones_v1.md), [05](spec_skills_05_mecanicas_ativas_v1.md), [06](spec_skills_06_ui_canvas_skills_v1.md), [07](spec_skills_07_pixelart_feedback_v1.md), [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

O contexto de efeito não transporta rank/variante. O catálogo de executores hardcoda tuning e inclui feedback-only; flags dos nós não descrevem sempre o executor real. UI e execução não compartilham a mesma decisão de disponibilidade.

**Escopo:** Estabelecer uma fonte de tuning por rank e consultas de disponibilidade comuns à UI/execução, com cooldown conforme D01.

**Fora do escopo:** Sem números novos arbitrários, catálogo de skills paralelo ou conversão automática de dormentes em skills funcionais.

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

- **AC01:** Preview e execução usam o mesmo rank/variante e valores; R1 e R2 com dados distintos produzem efeitos distintos.
- **AC02:** EvaluateNode/EvaluateSlot podem ser chamados repetidamente sem mudar o jogo; toda recusa tem chave localizada.
- **AC03:** As 31 ações do catálogo têm estado explícito: executável com consumidor validado ou dormente com motivo; nenhum feedback-only é classificado operacional.
- **AC04:** Cooldown obedece D01 em duplicata, troca de slot, modal, pausa, cena e save/load, com casos esperados escritos antes de implementar.

# /speckit.plan

## Plan — contratos e solução

Propostos:
`SkillNodeReadiness SkillTreeManager.EvaluateNode(string nodeId)`;
`SkillActionReadiness ActiveSkillExecutionController.EvaluateSlot(int slotIndex)`.
DTOs de consulta em Skills, sem Unity refs:
SkillNodeReadiness: bool CanPurchase, CanRankUp, RequiresVariant; string FailureKey; int CurrentRank, RankCap.
SkillActionReadiness: bool CanExecute; string FailureKey; float RemainingCooldown, TotalCooldown.
São consultas sem custo, evento, cooldown ou mutação. Readiness de uso NÃO é gate de equipar.

Estender SkillEffectContext com `string NodeId`, `int Rank`, `string VariantId`.
Estender SkillActionSO existente com `List<SkillActionRankData> RankData`, cujo DTO de autoria reúne Rank, BaseDamage, ManaCost, StaminaCost, CooldownSeconds, Range, DurationSeconds, StatusChance, ProjectileCount.
Não duplicar a propriedade Icon já existente. Campos de formas adicionais só entram com consumidor especificado em 05.
Resolver dados uma vez ao preparar a ação; executor não calcula rank a partir de slot nem tem segunda tabela numérica.
Fonte de conteúdo escolhida em 00; generator atual sincroniza assets e valida IDs, resolução de efeito e rank coverage.

D01 deve definir chave e relógio de cooldown. Proposta: actionId compartilhado evita duplicatas/troca; relógio acompanha tempo de gameplay, modal não interrompe contagem se o mundo continua. Pausa real congela ambos. Save/migração de cooldown requer contrato separado explícito se adotado, não é consequência implícita da mudança de chave.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/SkillActionSO.cs`
- `Assets/_Game/Scripts/Skills/SkillActionDatabaseSO.cs`
- `Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeManager.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SkillEffectContext.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs`
- `Assets/_Game/Scripts/Gameplay/SkillActionEffectCatalog.cs`

Padrões aplicáveis: **data-catalog-authoring, skill-tree-authoring, registry-catalog-pattern, action-feedback-pipeline**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Dados:** Estender SkillActionSO e SkillEffectContext; ajustar GenerateCanonicalSkillCatalog para cobrir ranks do contrato 00 sem trocar IDs.
2. **Consulta de compra:** Implementar EvaluateNode em SkillTreeManager reutilizando a validação pura da spec 02, incluindo cap/variante e sem mutação.
3. **Consulta de uso:** Implementar EvaluateSlot no controller a partir do mesmo resolvedor usado em TryUseSlot; preservar APIs GetSlotCooldownRemaining/Total.
4. **Executor e recarga:** Fazer ActiveSkillExecutorCatalog receber tuning resolvido, consumir Rank/VariantId e implementar chave/relógio exatos aprovados em D01.
5. **Consistência:** Validar catálogo↔action↔executor↔ranks, registrar dormentes honestamente e comparar consultas com resultado real de execução.

### Falhas, migração e limites

D01 (cooldown), D02 (ranks) e D03 (dormentes) precisam estar resolvidos antes de fechar dados e comportamento. Payload detalhado de formas de magia depende de 05.

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
| [ ] | T01 | Dependências da spec | Dados — Estender SkillActionSO e SkillEffectContext; ajustar GenerateCanonicalSkillCatalog para cobrir ranks do contrato 00 sem trocar IDs. | AC01 |
| [ ] | T02 | T01 | Consulta de compra — Implementar EvaluateNode em SkillTreeManager reutilizando a validação pura da spec 02, incluindo cap/variante e sem mutação. | AC02 |
| [ ] | T03 | T02 | Consulta de uso — Implementar EvaluateSlot no controller a partir do mesmo resolvedor usado em TryUseSlot; preservar APIs GetSlotCooldownRemaining/Total. | AC02, AC03 |
| [ ] | T04 | T03 | Executor e recarga — Fazer ActiveSkillExecutorCatalog receber tuning resolvido, consumir Rank/VariantId e implementar chave/relógio exatos aprovados em D01. | AC01, AC04 |
| [ ] | T05 | T04 | Consistência — Validar catálogo↔action↔executor↔ranks, registrar dormentes honestamente e comparar consultas com resultado real de execução. | AC01, AC02, AC03, AC04 |
| [ ] | T06 | T05 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **Readiness_IsPure:** 100 consultas deixam snapshot e contador de eventos iguais.
- **RankData_R2ChangesEffect:** fixture R1=10/R2=15 resulta em 10/15 antes de mitigação.
- **Catalog_AllActiveActionsHaveExplicitReadiness:** 31 ações classificadas; fixture feedback-only nunca CanExecute=true.
- **Cooldown_ApprovedPolicy_AllTransitions:** tabela D01 cobre duplicata, troca, modal, pausa e load sem expectativa implícita.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- D01 (cooldown), D02 (ranks) e D03 (dormentes) precisam estar resolvidos antes de fechar dados e comportamento. Payload detalhado de formas de magia depende de 05.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
O contrato habilidade por habilidade está em [refinamento funcional v3](../../../docs/refinements/a_implementar/ref_skill_catalog_functional_contract_v3.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.
