# SPEC — Forja de Gear Tier Alto: Craft Mithril+, Receitas First-Kill e Upgrades +1/+2/+3

> **Spec ID:** `fable_49_spec_high_tier_gear_crafting_upgrades`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P1
> **Type:** Runtime / Data / Integration
> **Domain:** Equipment / Crafting / Economy
> **Parallelizable:** NO (lock em crafting + equipment save)
> **Parallel group:** N/A
> **Can run with:** specs de quest/festival que não tocam itens (F51/F52/F53)
> **Must not run with:** F22 (equipment save aditivo), F32 (geradores de item), F48 (ItemDataSO), F03/F06 (stats/loot)
> **Repo lock scope:** CraftingService/CraftingManager/recipes, equipment save DTO (ItemInstance), geradores de item, loot first-kill de boss
> **Depends on:**
> - `fable_32_spec_item_catalog_data_expansion` (E18 — materiais mithril/bromecian/star_iron e armas nominais)
> - `fable_03_spec_equipment_mechanical_baselines_runtime` (EXECUTADA — matriz de stats por tipo+material)
> - `fable_33_spec_bestiary_data_expansion_60_creatures` (E21 — bosses de gate que dropam receita first-kill)
> - `fable_22_spec_essence_tempering_forge` (E22 — precedente de campo aditivo no equipment save)
> **Blocks:** balance de gear 56+ jogável; únicas de drop montadas de peças (futuro)
> **Scope:** receitas de craft das armas/armaduras de tier alto no crafting existente, drops de receita first-kill em boss de gate, upgrade level (+1/+2/+3) no ItemInstance com custo de materiais (save aditivo, padrão F22).
> **Out of scope:** Têmpera de Essência (F22), únicas de drop por peças (vask_hammer/master_blade — spec futura), loja vendendo tier alto (catálogo proíbe), UI nova de forja.

required_adrs: [ADR-0006-save-data-contracts-simple-dtos.md]
required_game_rules: [inventory_equipment_rules.md, save_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O ITEM_CATALOG (§12) define a regra dura da progressão de gear: "Tiers Mithril/Bromeciana/
Pedra Negra/Meteórica: SOMENTE craft/têmpera (não loja)". O BALANCE_CURVES (§8) ancora as
bandas: 56-70 = Mithril (WeaponDmg 15-18), 71-85 = Liga bromeciana (17-20), 86-101 = Pedra
Negra/Meteórica (19-23) — sem esses crafts o jogador chega à banda 56+ com Aço e a tabela
de validação Q9.3 quebra. O CAVE_BESTIARY_CATALOG amarra o gating: o boss do gate 60
(Gravedelver Artificer Lord) dropa **first-kill a receita "Mithril Work"** — receitas de
tier alto entram pela caverna, não por loja. O EQUIPMENT_WEAPONS_ARMOR_MATERIALS fecha o
trio: §34 (crafting depende de material+tier+forja+componentes de caverna+ouro+receita;
"crafting não deve ignorar a caverna"), §35 (upgrades melhoram UM foco por vez — dano OU
durabilidade OU stamina... "upgrade não deve melhorar tudo ao mesmo tempo") e §45 (o save
de equipamento persiste `upgrade level` por item instance e recalcula derivados no load).

O repo tem o crafting funcional do WI-14 (`CraftingService`/`CraftingManager`/
`CraftingStation`/`CraftingRecipeInitializer` + modal de UI) e o equipment save com
ItemInstance/durabilidade (F13/WI), com o precedente F22 de campos aditivos
(infusionElement/infusionTier). O que NÃO existe: receitas de gear tier alto, conceito de
receita APRENDIDA por drop first-kill de boss, e upgrade level no ItemInstance.

## Problema

Sem esta spec, a progressão de equipamento TERMINA no aço de loja: as bandas 56-101 do
balance não têm arma/armadura alcançável, os materiais raros do catálogo (mithril_ore,
bromecian_alloy, star_iron) não têm consumidor, o first-kill de boss de gate não recompensa
nada de gear, e a decisão "tier alto SOMENTE craft" fica sem mecanismo. Upgrades (§35)
ficariam para sempre no papel — nenhum sink de materiais/ouro no late game.

## Objetivo

Ao final desta spec, o jogador que derrota o boss do gate 60 pela primeira vez deve
aprender "Mithril Work", craftar arma/armadura de Mithril na forja com materiais da caverna
+ ouro, e aplicar upgrades +1/+2/+3 focados em itens equipáveis — com receitas tier alto
NUNCA em loja, upgrade persistido como campo aditivo simples no ItemInstance (padrão F22),
derivados recalculados no load (§45) e tudo dentro do crafting existente (zero sistema
paralelo).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§9, §12)
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md (§34-36, §45)
docs/design/gameplay/combat/BALANCE_CURVES_DIRECTION_v1.0.md (§8 bandas 56+)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (drops first-kill dos bosses de gate)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/economy-balance-tuning/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CraftingService/CraftingRequest/CraftingManager/CraftingStation/CraftingJob (WI-14) +
  CraftingRecipeInitializer (Editor) + CraftingModal/ViewModels (UI);
- equipment save com ItemInstance (id/baseItemId/durability) + precedente F22 de campos
  aditivos (infusionElement/infusionTier);
- EquipmentManager/EquippedItemResolver + recálculo de stats no load (F02/F03);
- materiais do catálogo (F32): mithril_ore, bromecian_alloy, star_iron, cores/scales;
- loot de boss (F06) e fichas de boss com first-kill (F33).
Não existe:
- receitas de craft de gear tier alto (Mithril/Bromeciana/Pedra Negra/Meteórica);
- conceito de receita aprendida/desbloqueada por drop first-kill;
- UpgradeRecipeSO / upgrade level no ItemInstance / serviço de upgrade.
Auditar Fase 0:
- shape do RecipeSO/CraftingRecipe atual (campo de "requer receita aprendida" cabe aditivo?);
- onde o equipment save guarda ItemInstance (owner da seção; campo upgradeLevel aditivo);
- como F06 modela drops first-kill (flag por boss? reaproveitar);
- quais armas/armaduras tier alto o gerador F32 já cria (itens existem; faltam receitas).
```

## Engineering stories

```text
Como jogador, quero que derrotar o boss do gate 60 me ensine a forjar Mithril — a caverna
paga a forja.
Como jogador, quero craftar a arma da minha banda com minério que EU minerei, nunca comprar.
Como jogador, quero dar +1/+2/+3 focado na arma que amo em vez de descartá-la.
Como save, quero upgrade level como int aditivo no ItemInstance, com derivados recalculados.
Como economia, quero sinks de material/ouro no late game (custos de craft/upgrade).
```

## Escopo

```text
Inclui:
- receitas de craft tier alto no crafting existente (CraftingRecipeInitializer aditivo):
  armas por banda da matriz canônica (Mithril 15-18 / Bromeciana 17-20 / Pedra Negra-
  Meteórica 19-23 — BALANCE §8) + armaduras/escudos correspondentes dos itens F32;
  custo = materiais do catálogo (mithril_ore, bromecian_alloy, star_iron, componentes de
  drop §36) + ouro; estação = forja (CraftingStation existente);
- gating por receita aprendida: campo aditivo na receita (requiredRecipeUnlockId);
  RecipeUnlockService (flags persistidas simples) — receitas tier alto começam bloqueadas;
- drops first-kill: boss de gate concede recipe unlock 1× (ex.: gate 60 Gravedelver
  Artificer Lord → "Mithril Work" conforme bestiário; gates 70+ → unlocks dos tiers
  seguintes conforme fichas F33); idempotente (re-kill não duplica);
- upgrades: UpgradeRecipeSO (NOVO — alvo por família/tier, custo materiais+ouro, FOCO
  único por nível: dano OU durabilidade OU peso OU stamina OU block — §35), níveis +1/+2/+3;
- EquipmentUpgradeService: aplica upgrade no ItemInstance (campo aditivo upgradeLevel:int,
  upgradeFocus:string), valida custo via inventário, publica evento; stats recalculados
  pelo fluxo existente (§45 — nunca persistir derivado);
- save: campos aditivos no ItemInstance (upgradeLevel/upgradeFocus, defaults 0/vazio) +
  flags de recipe unlock (seção de flags existente ou campo aditivo — auditar Fase 0);
- EditMode tests: unlock idempotente por first-kill, craft bloqueado sem receita, custo
  consumido exatamente 1×, upgrade aplica foco único e respeita teto +3, round-trip de
  save (upgradeLevel/legado sem campo), recálculo de stats com upgrade.
```

## Fora de escopo

```text
Não inclui:
- Têmpera de Essência (F22 — sistema irmão; esta spec não toca infusion*);
- únicas montadas de peças (vask_hammer/master_blade/elder_scale set — spec futura);
- venda de tier alto em loja (PROIBIDO pelo catálogo §12);
- receitas de comida/poção (donas: catálogo §5/§7 — fluxos já existentes);
- UI nova (CraftingModal existente lista as receitas; upgrade entra como receita na forja).
```

## Regras de não duplicação

```text
Não criar segundo sistema de crafting — receitas tier alto entram no CraftingService/WI-14.
Não criar segundo caminho de save — campos aditivos no ItemInstance (padrão F22).
Não duplicar stats por material — matriz canônica F03 é a fonte; receita só referencia item.
Não duplicar first-kill — se F06 já modela flag de primeiro kill por boss, reusar.
Upgrade ≠ Têmpera: upgrade é numérico focado (§35); têmpera é elemental (F22). Não misturar.
```

## Critérios de aceite

### CA-1 Tier alto somente craft com receita aprendida

- Receitas Mithril+ existem na forja, começam bloqueadas e NUNCA aparecem em loja;
  craft sem unlock é rejeitado com motivo claro.
- Evidência: EditMode tests (bloqueado/desbloqueado) + validator de que nenhum shop vende
  item de tier Mithril+.

### CA-2 First-kill ensina a receita 1×

- Primeira derrota do boss do gate 60 concede "Mithril Work"; re-derrotar não duplica;
  o unlock sobrevive a save/load.
- Evidência: EditMode test de idempotência com round-trip de save.

### CA-3 Craft consome custo e entrega item da matriz

- Craft de arma Mithril consome exatamente os materiais+ouro da receita e entrega o item
  F32 cujos stats vêm da matriz canônica (BALANCE §8 — sem stats duplicados na receita).
- Evidência: EditMode test de consumo/entrega + conferência de stats via resolver F03.

### CA-4 Upgrade focado +1/+2/+3 persistido

- Upgrade aplica UM foco por nível, respeita teto +3, consome custo, persiste como
  upgradeLevel/upgradeFocus no ItemInstance e os derivados são recalculados no load.
- Evidência: EditMode tests (foco único, teto, custo, round-trip + recálculo).

### CA-5 Save legado intacto

- Saves sem os campos novos carregam com defaults (upgradeLevel=0) e nenhum item perde
  durabilidade/infusão existente.
- Evidência: EditMode test de load legado.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Crafting/
  RecipeUnlockService.cs        (NOVO — flags de receita aprendida, idempotente)
  UpgradeRecipeSO.cs            (NOVO — alvo, custo, foco, nível máx)
  EquipmentUpgradeService.cs    (NOVO — valida custo, aplica upgradeLevel/Focus, evento)
CraftingService/recipes         (ADITIVO — requiredRecipeUnlockId nas receitas)
Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs (ADITIVO — receitas tier alto)
Assets/_Game/Scripts/Editor/ (gerador de UpgradeRecipeSO + unlock por boss — AssetDatabase)
equipment save / ItemInstance   (campos aditivos upgradeLevel:int, upgradeFocus:string)
loot first-kill de boss (F06/F33) → hook de RecipeUnlock (consumo, não rewrite)
Assets/_Game/Scripts/Core/Events/{RecipeLearnedEvent,EquipmentUpgradedEvent}.cs (NOVOS)
Assets/_Game/Tests/EditMode/Crafting/HighTierGearCraftingUpgradesTests.cs (NOVO)
docs/validation/fable_49_spec_high_tier_gear_crafting_upgrades_execution_report.md
```

## Contratos

### Data contracts

- Receita tier alto: receita existente + `requiredRecipeUnlockId:string` (aditivo, default
  vazio = sempre disponível — receitas atuais inalteradas).
- `UpgradeRecipeSO`: {upgradeId, targetFamily/tier, level (1-3), focus (enum/string única),
  materialCosts[] (itemId+qty), goldCost} — tipos simples e IDs.
- Unlock: flag estável `recipe_unlock_<slug>` (ex.: recipe_unlock_mithril_work).

### Runtime contracts

- `RecipeUnlockService`: `IsUnlocked(id)` / `Unlock(id)` idempotente; consultado pelo
  CraftingService antes de aceitar CraftingRequest de receita gated.
- `EquipmentUpgradeService`: `TryUpgrade(itemInstanceId, upgradeRecipe)` — valida teto +3,
  custo via InventoryManager, grava campos no ItemInstance, dispara recálculo existente.
- Hook first-kill: ao registrar primeiro kill do boss de gate (mecanismo F06/F33 auditado),
  chamar Unlock(receita da ficha do boss).

### Event contracts

- `RecipeLearnedEvent(recipeUnlockId)` e `EquipmentUpgradedEvent(itemInstanceId, level,
  focus)` — GameEventBus; UI/toast existentes escutam.

### Save contracts

- ItemInstance: campos aditivos `upgradeLevel` (default 0) e `upgradeFocus` (default vazio)
  — mesmo DTO/seção do precedente F22; flags de unlock na seção de flags persistida
  existente (auditar owner na Fase 0). Sem refs Unity; derivados NUNCA persistidos (§45).

### UI contracts

- CraftingModal existente lista receitas desbloqueadas (gated ficam ocultas ou marcadas —
  seguir padrão atual do modal); nenhum painel novo.

## Sistemas afetados

```text
Crafting (receitas, gating, initializer)
Equipment/ItemInstance (upgrade aditivo + recálculo)
Save/load (campos aditivos + flags)
Loot/boss first-kill (hook de unlock)
Economia (sinks de material/ouro tier alto)
Event bus (2 eventos novos)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Crafting/** (novos serviços/SO + CraftingService aditivo)
Assets/_Game/Scripts/Craft/** (apenas integração aditiva com estação/job, se necessário)
Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs + gerador de upgrades/unlocks
equipment save DTO / ItemInstance (campos aditivos — owner auditado na Fase 0)
hook de first-kill no fluxo de loot/boss F06 (cirúrgico)
Assets/_Game/Scripts/Core/Events/{RecipeLearnedEvent,EquipmentUpgradedEvent}.cs
Assets/_Game/Tests/EditMode/Crafting/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets SÓ via gerador/AssetDatabase)
Packages/** ; ProjectSettings/**
F22 (infusion*) — não tocar nos campos/serviço de têmpera
Shop/pricing core (tier alto NUNCA entra em loja — nenhuma mudança de loja é necessária)
SaveManager core (apenas campos aditivos nos DTOs existentes)
UI nova de forja (CraftingModal existente apenas)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
RecipeSO/CraftingRecipe (campo gated aditivo?), owner do equipment save/ItemInstance,
modelo de first-kill (F06), itens tier alto já gerados (F32).

### Fase 1 — Unlock e gating
RecipeUnlockService + requiredRecipeUnlockId + hook first-kill (gate 60 → Mithril Work;
gates seguintes conforme fichas F33) + RecipeLearnedEvent + testes de idempotência.

### Fase 2 — Receitas tier alto
CraftingRecipeInitializer aditivo: receitas de armas/armaduras Mithril/Bromeciana/
Pedra Negra-Meteórica (custo materiais+ouro do catálogo §9/§36; saída = itens F32) + testes
de consumo/entrega.

### Fase 3 — Upgrades
UpgradeRecipeSO + EquipmentUpgradeService (+1/+2/+3, foco único §35) + campos aditivos no
ItemInstance + recálculo §45 + EquipmentUpgradedEvent + testes (teto/foco/custo/round-trip).

### Fase 4 — Fechamento
Save legado testado; validator loja-sem-tier-alto; csproj; run_strict_validation; report.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F51/F52/F53 (quest/festival).
- Must not run with: F22 (mesmo DTO de equipment save), F32/F48 (itens/ItemDataSO),
  F03/F06 (stats/loot).
- Shared files/systems that require lock: CraftingService/recipes, equipment save DTO,
  geradores de item, loot de boss.
- Reason: escreve no DTO de equipment e no initializer de receitas — superfícies
  compartilhadas com F22/F32/F48.

## Impacto em save/load

```text
Does this change save schema? YES (aditivo) — upgradeLevel/upgradeFocus no ItemInstance +
flags recipe_unlock_* na seção de flags existente.
Does this add a save section? NO
Does this require migration? NO (defaults: 0/vazio; saves legados carregam intactos)
Does this persist Unity references? NO (ints/strings/IDs apenas)
Owner/restore order: inalterados — mesmas seções, mesmos owners.
```

## Impacto em eventos

```text
Adds events: YES — RecipeLearnedEvent, EquipmentUpgradedEvent
Changes existing events: NO
Requires unsubscribe pattern: NO (serviços puros; consumidores UI já seguem padrão)
```

## Impacto em UI/Unity

```text
Changes UI: NO (CraftingModal existente lista; toasts via eventos)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador/AssetDatabase apenas (receitas/upgrades)
Requires Play Mode final validation: YES (craft + upgrade na forja in-game)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: campo aditivo no ItemInstance quebrar deserialização de saves antigos.
Mitigação: defaults seguros + teste de load legado (padrão F22 já provado).

Risco: receita duplicar stats e divergir da matriz canônica F03.
Mitigação: receita referencia APENAS itemId de saída; stats vêm do resolver — teste CA-3.

Risco: unlock first-kill duplicar em re-kill/reload.
Mitigação: flag idempotente + teste com round-trip.

Risco: upgrade "melhorar tudo" violando §35.
Mitigação: foco único por UpgradeRecipeSO (enum) validado no serviço + teste.

Risco: F06/F33 ainda sem modelo de first-kill no momento da execução.
Mitigação: Fase 0 audita; se ausente, RecipeUnlockService expõe Unlock(id) e o hook fica
documentado como integração pendente (status honesto, nunca unlock automático).
```

## Rollback

```text
Remover os serviços novos e o campo gated (default vazio) devolve o crafting atual;
receitas tier alto somem da forja. Campos aditivos no ItemInstance ficam inertes (0/vazio)
em saves já gravados — sem migração reversa, sem apagar save real.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar RecipeSO, equipment save/ItemInstance, modelo first-kill (F06), itens F32.
- [ ] T002 — RecipeUnlockService + gating aditivo + hook first-kill (Mithril Work no gate 60) + evento + testes.
- [ ] T003 — Receitas tier alto no CraftingRecipeInitializer (Mithril/Bromeciana/Pedra Negra-Meteórica) + testes.
- [ ] T004 — UpgradeRecipeSO + EquipmentUpgradeService + campos aditivos ItemInstance + recálculo + evento + testes.
- [ ] T005 — Save legado + validator loja-sem-tier-alto; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gating, idempotência de unlock, custo, teto de upgrade,
  round-trip de save)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — craft e upgrade na
  forja in-game)
- Requires regression test: YES (receitas existentes sem unlock continuam craftáveis;
  durabilidade/infusão F22 intactas; save legado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano aprendendo
  Mithril Work, craftando 1 arma Mithril e aplicando +1

## Definition of Done

```text
Receitas tier alto gated por unlock first-kill (idempotente, persistido); craft consome
custo e entrega item da matriz canônica; upgrades +1/+2/+3 de foco único persistidos como
campos aditivos com recálculo no load; loja jamais vende Mithril+; saves legados intactos;
builds 0E; execution report com Spec Compliance Matrix.
```

## Anti-regressão

```text
Receitas existentes (comida/itens atuais) craftam exatamente como antes (gated default vazio).
Campos F22 (infusion*) intocados; durabilidade preservada em upgrade.
Nenhum derivado persistido no save (§45); DTOs só tipos simples + IDs.
Tier Mithril+ jamais em ShopInventory (validator).
Nenhum segundo sistema de crafting/upgrade; eventos só via GameEventBus.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. Tabela de receitas por gate APROVADA (4.4): 15 Cobre Temperado · 45 Aço Profundo ·
   60 Mithril Work · 75 Bromeciana · 90 Pedra Negra · 100 Meteórica (30 = planta de baú,
   não-receita-de-arma; fica com o sistema de corpse).
```
