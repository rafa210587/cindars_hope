# Identidade mecânica das ações ativas e restaurações justificadas

> **Spec ID:** spec_skills_05_mecanicas_ativas_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [00](spec_skills_00_decisoes_catalogo_v1.md), [01](spec_skills_01_avatar_execucao_v1.md), [03](spec_skills_03_dados_rank_readiness_v1.md)
> **Blocks:** [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

A baseline reduz muitos conceitos a projéteis/golpes semelhantes, mantém seis execuções apenas de feedback e não mapeia crafting_quick_repair. Ranks e formas prometidas precisam ter consequência real.

**Escopo:** Especificar e implementar por família apenas as mecânicas consolidadas no contrato 00, incluindo falha, targeting, rank e efeitos verificáveis.

**Fora do escopo:** Sem novo conjunto de classes D&D, teleporte livre, magia que substitui Fonte ou inclusão automática da hipótese Contenção de Circuito.

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

- **AC01:** Cada ação habilitada produz diferença mensurável no alvo/mundo; um toast sozinho nunca satisfaz sucesso.
- **AC02:** Cada forma de ataque tem testes de alvo dentro/fora, máximo de alvos, obstáculo e cancelamento; custos só seguem a política de cast aprovada.
- **AC03:** Kit/Instinto preservam D05; quick repair e efeitos de ofício recusam alvo inelegível sem consumo.
- **AC04:** Restaurações de D07 entram apenas com gates/prérequisitos e orçamento de build aprovados; a hipótese de Circuito permanece fora.

# /speckit.plan

## Plan — contratos e solução

Manter `ISkillEffectExecutor` e `SkillEffectRegistry`; adicionar executores pequenos apenas para uma semântica que não caiba nos atuais.
Cada ação precisa de payload definido: target, geometria, custo, windup/cancel, efeito, duração, rank, cooldown e feedback. Não implementar os seis feedback-only como buffs genéricos.

Famílias para divisão obrigatória antes de executar:
- Físicas: melee com alcance real; fan diferencia cobertura de dano concentrado; pierce explicita cap de alvos e eventual decaimento.
- Magia: cone/raio/área/marca/barreira/cura devem respeitar direção própria. Área não se torna projétil só para reusar executor.
- Sobrevivência: Instinto preserva cura/revelação; Kit mantém 30 HP/45 s, sem item, fora de combate até decisão contrária. Field patch/retirada/isca precisam definir alvo e benefício.
- Ofício/fazenda: marca de eficiência e quick repair precisam de consumidor de trabalho/reparo, custo, objeto elegível e recusa sem consumo.

Candidatos de restauração D07: Contra-ataque (janela 1,5 s no próximo hit, +20/35/50 pontos percentuais de crit, sujeito ao cap vigente) e Purify Taint (24 MP, 22 s, cast 0,6 s, T3, requisitos de água/cura) vêm das direções, não são acréscimos arbitrários.
Contenção de Circuito: apenas estudo condicionado a emissores recorrentes; modo do selo existente, 36 MP/28 s/cast 0,75 s/duração 3–5 s como hipótese, sem novo nó, sem boss disable e sem regenerar snapshot. Fora da implementação deste pacote.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ProjectileSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SlowFieldSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SelfRestoreSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Gameplay/SkillActionEffectCatalog.cs`
- `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
- `docs/design/gameplay/combat/SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md`

Padrões aplicáveis: **ability-effect-composition, player-ability-runtime, game-feel-checklist, action-feedback-pipeline**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Contratos por família:** Transcrever os payloads aprovados em 00 para tabelas por actionId e dividir esta spec em filhas com arquivos/assinaturas exclusivos antes da promoção.
2. **Física:** Adaptar MeleeStrikeSkillEffectExecutor/ProjectileSkillEffectExecutor para geometria, hits, fan/pierce e rank; usar deslocamento resolvido em 01.
3. **Magia:** Reusar SlowFieldSkillEffectExecutor e pipeline de spells onde semanticamente adequado; especificar executores novos somente após auditoria de reuso.
4. **Sobrevivência/ofício:** Fechar D05 e apontar consumidores de SelfRestore/FarmCrop e reparo; substituir apenas feedback-only cobertos por contrato.
5. **Restaurações:** Criar filhas de Contra-ataque/Purify somente se D07 aprovado, com pré-requisitos e cap de crit explicitados.
6. **Cenários:** Executar suite por família e atualizar matriz de funcionalidade; manter dormentes os efeitos ainda não implementados.

### Falhas, migração e limites

Esta é uma spec guarda-chuva de decomposição, não uma unidade autorizada de código. D05/D07, payloads de 00 e assinatura dos consumidores ainda precisam fechar nas filhas.

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
| [ ] | T01 | Dependências da spec | Contratos por família — Transcrever os payloads aprovados em 00 para tabelas por actionId e dividir esta spec em filhas com arquivos/assinaturas exclusivos antes da promoção. | AC01, AC02 |
| [ ] | T02 | T01 | Física — Adaptar MeleeStrikeSkillEffectExecutor/ProjectileSkillEffectExecutor para geometria, hits, fan/pierce e rank; usar deslocamento resolvido em 01. | AC01, AC02 |
| [ ] | T03 | T02 | Magia — Reusar SlowFieldSkillEffectExecutor e pipeline de spells onde semanticamente adequado; especificar executores novos somente após auditoria de reuso. | AC01, AC02 |
| [ ] | T04 | T03 | Sobrevivência/ofício — Fechar D05 e apontar consumidores de SelfRestore/FarmCrop e reparo; substituir apenas feedback-only cobertos por contrato. | AC01, AC03 |
| [ ] | T05 | T04 | Restaurações — Criar filhas de Contra-ataque/Purify somente se D07 aprovado, com pré-requisitos e cap de crit explicitados. | AC04 |
| [ ] | T06 | T05 | Cenários — Executar suite por família e atualizar matriz de funcionalidade; manter dormentes os efeitos ainda não implementados. | AC01, AC02, AC03, AC04 |
| [ ] | T07 | T06 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **ActiveEffect_ChangesGameplay_NotOnlyFeedback:** snapshot de alvo/mundo muda pelo efeito contratado.
- **Shape_InsideOutsideAndObstacle:** só receptores elegíveis recebem efeito; cobertura conforme geometria.
- **Repair_InvalidTarget_NoSpend:** alvo não reparável mantém recursos e cooldown.
- **Kit_CombatGate_PreservesApprovedValues:** fora combate recupera 30 HP até máximo; em combate recusa conforme D05.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Esta é uma spec guarda-chuva de decomposição, não uma unidade autorizada de código. D05/D07, payloads de 00 e assinatura dos consumidores ainda precisam fechar nas filhas.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
Os payloads de custo, efeito, reação, animação e requisito estão no [refinamento funcional v3](../../../docs/refinements/a_implementar/ref_skill_catalog_functional_contract_v3.md); esta guarda-chuva deve gerar filhas a partir desse contrato.
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.
