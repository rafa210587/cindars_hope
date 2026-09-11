# Skills — fase mecânica 5: ações de sobrevivência

> **Spec ID:** spec_skills_15_survival_actions_v1  
> **Status:** PLAYMODE_VALIDATED — implementação e validação automatizada concluídas em 2026-09-10; aceite humano agrupado no fechamento da wave  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Data+Save / Skills+Survival+Cave / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_14_magic_shapes_v1  
> **Blocks:** spec_skills_16_crafting_actions_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode+Cave stable-run / execute-spec-strict

# /speckit.specify

## Spec

- **AC01:** Último Fôlego arma por 5 s ao cruzar HP<25%, cura `ceil(MaxHP × 25/30/35/40/45%)`, tem cooldown de 90 s e só funciona uma vez por `encounterId`. Marcar o encontro consumido precede a cura. Load não rearma nem interpreta o HP restaurado como novo cruzamento. O encontro nasce no primeiro aggro do grupo, reforços aderem ao mesmo ID, perda temporária de aggro/reaggro/load não resolve e somente todos mortos, leash coletivo concluído, transição canônica, KO ou fim de run encerra o encontro.
- **AC02:** Sinal custa 15 STA, cooldown 18 s e dura 5/6/7 s. Reduz em 30% os custos de corrida/dodge e concede +12% de velocidade somente quando movimento/dodge se afasta da ameaça primária (`dot >= 0,5`). A ameaça é o hostil vivo mais próximo do encontro, desempate por `enemyInstanceId`. Qualquer ação ofensiva termina o Sinal no commit, antes de seus bônus incidirem.
- **AC03:** Isca tem cooldown 12 s, consome `item_consumable_improvised_lure` somente no commit e é colocada até 6 tiles em chão caminhável com linha de efeito. Num raio de 5 tiles, seleciona no máximo cinco alvos por distância e `enemyInstanceId`: simples são atraídos por 4/6/8 s; elite desvia por 1/1,5/2 s; boss ignora. Dano do player no alvo encerra sua atração.
- **AC04:** Kit tem cooldown 45 s e canaliza 0,8 s fora de combate, mantendo velocidade `<= 0,05 tile/s` por uma tolerância máxima de 0,2 s. Movimento ou dano cancela sem consumo; dano no frame final vence. Ao fim, revalida HP e item, então consome `item_consumable_field_dressing` e cura `ceil(MaxHP × 18/21/24/27/30%)`.
- **AC05:** Instinto tem cooldown 30 s e revela em raio inclusivo de 7 tiles, por 4/5/6/7/8 s, apenas registros já materializados: recursos não exauridos, traps/hazards ativos e `IInteractable` habilitado e não secreto. Não altera fog/minimap/loot, não revela conteúdo ainda não materializado e não restaura STA.
- **AC06:** Campo tem cooldown 60 s, exige uso fora de combate, consome `item_consumable_camp_supply` no commit e pode ser usado uma vez por `runId` canônico. Cria âncora fixa no chão com raio 2,5 tiles por 8/10/12/14/16 s. Os benefícios valem apenas dentro da zona: -50% fome/fadiga e +50% somente nos canais periódicos naturais já existentes de STA/MP; HP só participa se existir regen-base positiva. Dano dissolve o Campo sem reembolso e nunca modifica restore, lifesteal ou consumíveis. `CaveRunSeed` serve apenas ao determinismo e não identifica a run.
- **AC07:** `runId` é opaco, persistido e independente da seed. Nasce na entrada canônica quando não há run ativa; checkpoint/load/revisita/transição entre níveis preservam; saída à superfície, KO e fim explícito encerram; nova entrada cria outro. `encounterId = runId + level + ordinal` e somente um encontro do player fica ativo. IDs, encontros consumidos, uso de Campo e tempos restantes usam tipos simples; integram save/snapshot sem regenerar `CaveLevel`. Canais pré-commit são cancelados no load. Último Fôlego/Campo ativos só restauram tempo restante na mesma run/nível, com gatilhos suprimidos durante restore.

Fora: capstone Telisandra, conteúdo procedural novo, loot oculto e UI/arte.

# /speckit.plan

Antes de executar, ler os documentos FASE9F disponíveis exigidos por AGENTS. Criar `SurvivalSkillState`,
`SurvivalSkillSaveData`, executores coesos, `SkillItemTransaction` puro e provider próprio; registrar pelo composition root.
Criar autoridade mínima de encontro sobre os eventos de aggro/pack/morte/leash/transição, pois `CombatStateTracker`
é apenas uma janela de dano e não representa encounter. Criar `runId` persistido na autoridade de cave sem derivá-lo
de `CaveRunSeed`. O restore de Survival deve ocorrer antes do HP do player e suprimir gatilhos. Não inserir estado
no `SkillTreeSaveData` nem alterar geração procedural/snapshots visitados.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/SelfRestoreSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/SurvivalSkillEffectExecutor.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SurvivalSkillState.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SurvivalSkillSaveData.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SkillItemTransaction.cs` (CREATE)
- `Assets/_Game/Scripts/Save/Providers/SurvivalSkillSectionProvider.cs` (CREATE)
- `Assets/_Game/Scripts/Save/SaveData.cs` e `Assets/_Game/Scripts/Save/SaveManager.cs` (somente registrar seção/versionamento necessário)
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` (somente encounter resolvido quando aplicável)
- `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` e `GenerateCanonicalItemCatalog.cs` (três consumíveis)
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- contratos Foundation para modificador de mobilidade/custos, ação ofensiva, encounter e reveal temporário
- testes `SurvivalSkillActionTests.cs`, `SurvivalSkillSaveTests.cs`, `SkillSurvivalPlayModeTests.cs` (CREATE)

Transação: validate context/item/channel→commit item/flag→aplicar→cooldown. Edge cases: save durante janela,
reaggro, transição, dano no último frame, inventário muda e revisita do mesmo nível/run.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Autoridade encounter/run, eventos tipados e `runId` independente da seed | AC01, AC06, AC07 |
| [x] | T02 | DTO/provider/save, ordem de restore e timers/flags idempotentes | AC01, AC06, AC07 |
| [x] | T03 | Itens canônicos, transações atômicas, modificadores comuns e registros de reveal | AC02–AC06 |
| [x] | T04 | Último Fôlego, Sinal e Kit, incluindo cancelamentos e commit | AC01, AC02, AC04 |
| [x] | T05 | Isca, Instinto e Campo, incluindo seleção determinística e zona | AC03, AC05, AC06 |
| [x] | T06 | EditMode/save roundtrip + PlayMode cave curta/longa/revisita | AC01–AC07 |
| [x] | T07 | Validar stable-run, `failed=0`, revisão independente e closeout | AC01–AC07 |

## Veredito de equilíbrio pré-implementação

**SOUND_WITH_AMENDMENTS.** Os valores acima ficam dentro do envelope esperado porque cada recuperação
relevante tem custo material, cooldown ou limite por encounter/run; controle não afeta boss e é reduzido em elite;
e Sinal exige direção de fuga e sacrifica pressão ofensiva. A aprovação numérica final permanece na fase de
balance acceptance, usando builds completas, inimigos reais e runs curtas/longas. Nenhuma habilidade desta fase
pode ser promovida apenas por feedback visual ou executor placeholder.
