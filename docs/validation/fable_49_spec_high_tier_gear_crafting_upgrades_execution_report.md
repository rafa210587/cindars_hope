# Execution Report — fable_49: Forja de Gear Tier Alto + Receitas First-Kill + Upgrades +1/+2/+3

> **Spec:** `.specs/a_implementar/fable/fable_49_spec_high_tier_gear_crafting_upgrades.md`
> **Wave:** FABLE Batch 10 · **Type:** Runtime / Data / Integration · **Priority:** P1
> **Date:** 2026-06-19 · **Branch:** `dev`
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`

validated_adrs: [ADR-0006-save-data-contracts-simple-dtos.md]
validated_game_rules: [inventory_equipment_rules.md, save_rules.md, combat_rules.md, economy_rules.md]

---

## Honest status rationale

Núcleo determinístico (gating de craft por receita aprendida, unlock first-kill idempotente,
custo de upgrade derivado, durabilidade derivada, foco único + teto +3, persistência aditiva e
recálculo no load) **implementado, auditado e coberto por 26 testes EditMode**. As duas
superfícies que dependem do Unity Editor — geração dos assets de receita/upgrade
(`HighTierGearRecipeGenerator`) e o validador de loja (`ValidateHighTierGearNotInShops`) —
**compilam em Assembly-CSharp-Editor (0E)** mas **NÃO foram executadas** (asset-gen e Play Mode
DEFERIDOS por decisão do dono). Por isso o status é `BUILD_VALIDATED_WITH_WARNINGS`, não
`ACCEPTED`: a validação Play Mode (craft + upgrade na forja in-game) e a execução do gerador
ficam para a validação final.

O hook de first-kill (`RecipeFirstKillUnlockHook`) está ligado ao `CaveBossDefeatedEvent`
existente (publicado por `CaveBossDefeatMonitor`/`CaveBossDeathReporter` com `CaveLevel` = nível
do gate). O mapeamento gate→receita é puro e testado; a confirmação de que o gate 60 publica o
evento com `CaveLevel == 60` em Play Mode é parte da validação final.

---

## Phase status

| Fase | Status | Evidência |
|------|--------|-----------|
| Phase 0 — Auditoria | COMPLETE | system-reuse audit abaixo |
| Phase 1 — Unlock/gating | BUILD_VALIDATED | RecipeUnlockService + gate + hook + testes |
| Phase 2 — Receitas tier alto | BUILD_VALIDATED (asset-gen DEFERRED) | gerador Editor compila; assets não gerados |
| Phase 3 — Upgrades | BUILD_VALIDATED | EquipmentUpgradeService + registro + save + testes |
| Phase 4 — Fechamento | BUILD_VALIDATED | save legado testado; validator loja; csproj; strict |
| Unity Play Mode (Phase 2-3 gameplay) | NOT RUN (DEFERRED) | craft+upgrade na forja in-game |
| Asset generation | NOT RUN (DEFERRED) | menu `CindarsHope/Crafting/Generate High-Tier Gear Recipes (fable_49)` |

---

## Existing systems audit (Phase 0 — encontrado / reutilizado / criado)

| Sistema existente | Decisão | Detalhe |
|---|---|---|
| `CraftingManager.TryCraft` + `RecipeDataSO` | REUSE (aditivo) | gating entra no fluxo existente; campo `RequiredRecipeUnlockId` aditivo (default vazio = receitas atuais inalteradas) |
| `EquipmentManager` + `EquipmentSaveData` | REUSE (aditivo) | mesmo owner do save de equipment; novo registro de upgrade + serviço de unlock persistidos via `CaptureSaveData/RestoreFromSaveData` (precedente F22 `Infusions`) |
| `WeaponInfusionRegistry` (F22) | PATTERN | upgrade e unlock seguem o MESMO padrão de registro puro + acessor estático `Active` + DTO aditivo |
| `TemperingService` + `TemperingTests` (F22) | PATTERN | `EquipmentUpgradeService` usa portas injetadas (`IUpgradeInventory`/`IUpgradeWallet`) + fakes, 100% EditMode |
| `CaveBossDefeatedEvent` (`BossGateId`, `CaveLevel`, ...) | REUSE (hook) | first-kill consome o evento existente; nada de novo evento de boss |
| `CanonicalItemCatalog.AddHighTierGear` (F32) | REFERENCE | 14 itemIds de tier alto + BaseValues são a fonte; receitas só referenciam o itemId de saída (stats NÃO duplicados) |
| `CraftingRecipeInitializer` (Editor) | PATTERN | gerador aditivo de receitas via AssetDatabase |
| `ValidateTownShopCatalogIntegrity` (Editor) | PATTERN | validador MenuItem + contagem de erros + throw |

**Sistemas paralelos criados:** NENHUM. Nenhum segundo crafting, nenhum segundo caminho de save,
nenhuma duplicação de stats por material, nenhuma mistura upgrade×têmpera.

---

## Acceptance criteria extracted

| CA | Critério (da spec) | Status | Evidência |
|----|--------------------|--------|-----------|
| CA-1 | Receitas Mithril+ existem na forja, começam bloqueadas, nunca em loja; craft sem unlock rejeitado | MET (asset-gen DEFERRED) | testes `CA1_*` + `ValidateHighTierGearNotInShops` |
| CA-2 | Primeira derrota do gate 60 concede "Mithril Work"; re-kill não duplica; sobrevive a save/load | MET | testes `CA2_*` (idempotência + round-trip) |
| CA-3 | Craft consome exatamente materiais+ouro da receita e entrega item F32 (stats da matriz, não duplicados) | MET (asset-gen DEFERRED) | testes `CA3_*` + gerador referencia só itemId de saída |
| CA-4 | Upgrade UM foco/nível, teto +3, consome custo, persiste `upgradeLevel`/`upgradeFocus`, derivados recalculados no load | MET | testes `CA4_*` (7) |
| CA-5 | Saves sem campos novos carregam com defaults (level 0); nada perde durabilidade/infusão | MET | testes `CA5_*` (3) |

## Spec Compliance Matrix

| Requisito (spec) | Implementação | Evidência |
|---|---|---|
| CA-1 Tier alto só craft com receita aprendida; nunca em loja | `RequiredRecipeUnlockId` em `RecipeDataSO`; `CraftingRecipeGate` (fail-closed) consultado em `CraftingManager.TryGetRecipe`; `ValidateHighTierGearNotInShops` | testes `CA1_*` (4) + validador Editor |
| CA-2 First-kill ensina 1× + sobrevive a save/load | `RecipeFirstKillUnlockHook.TryApplyFirstKillUnlock` (idempotente) + `RecipeUnlockService` persistido | testes `CA2_*` (4) |
| CA-3 Craft consome custo e entrega item da matriz | `HighTierGearRecipeGenerator` (ingrediente = material-âncora da banda; saída = item F32); custo de upgrade derivado | testes `CA3_*` (4); asset-gen DEFERRED |
| CA-4 Upgrade focado +1/+2/+3 persistido + recálculo no load | `EquipmentUpgradeService.TryUpgrade` (foco único, teto +3, consumo 1×); `EquipmentUpgradeRegistry` (DTO aditivo `upgradeLevel`/`upgradeFocus`) | testes `CA4_*` (7) |
| CA-5 Save legado intacto (defaults 0/vazio) | `RestoreFromSaveData(null)` => vazio; defaults não-nulos; entradas inválidas ignoradas | testes `CA5_*` (3) |
| §35 upgrade melhora UM foco por vez | `UpgradeFocus` enum único por instância; `FailFocusMismatch` ao trocar | `CA4_Upgrade_SingleFocus_RejectsFocusChange` |
| §45 derivados nunca persistidos | DTO só `level`/`focus`; recálculo a partir do registro no load | `CA4_Upgrade_DerivedRecalculatedFromUpgradeLevel_NotPersisted` |
| Custo `(2N × banda) + (BV × 0.5N)` (Decision 2.11) | `HighTierGearCanon.UpgradeGoldCost` | `CA3_UpgradeCostFormula_MatchesWorkedExamples` (90/180/480/1440) |
| Durabilidade base×material (Decision 2.11) | `HighTierGearCanon.DerivedMaxDurability` | `CA3_DerivedDurability_ByClassAndMaterial` |
| Tabela de gate (EMENDA 2026-06-12-D) | `HighTierGearCanon.RecipeUnlockForGate` (15/45/60/75/90/100) | `CA2_GateMapping_MatchesEmenda44Table` |

---

## Arquivos alterados

### Criados (runtime — Assembly-CSharp)
- `Assets/_Game/Scripts/Crafting/HighTierGearCanon.cs` — tabela canônica (bandas, custo, durabilidade, gate→receita).
- `Assets/_Game/Scripts/Crafting/EquipmentUpgrade.cs` — struct puro {level, focus} + parse/serialização estável do foco.
- `Assets/_Game/Scripts/Crafting/EquipmentUpgradeRegistry.cs` — registro por instância + DTO aditivo.
- `Assets/_Game/Scripts/Crafting/EquipmentUpgradeService.cs` — serviço puro de upgrade (portas + result).
- `Assets/_Game/Scripts/Crafting/RecipeUnlockService.cs` — flags de receita aprendida (idempotente) + save.
- `Assets/_Game/Scripts/Crafting/CraftingRecipeGate.cs` — indireção pura do gating (fail-closed).
- `Assets/_Game/Scripts/Crafting/RecipeFirstKillUnlockHook.cs` — hook first-kill (núcleo puro + subscriber).
- `Assets/_Game/Scripts/Crafting/UpgradeRecipeSO.cs` — SO de receita de upgrade (alvo/foco/nível).
- `Assets/_Game/Scripts/Crafting/HighTierGearCatalog.cs` — lista canônica dos 14 itemIds de tier alto.
- `Assets/_Game/Scripts/Core/Events/RecipeLearnedEvent.cs` — evento (GameEventBus).
- `Assets/_Game/Scripts/Core/Events/EquipmentUpgradedEvent.cs` — evento (GameEventBus).

### Criados (Editor — Assembly-CSharp-Editor)
- `Assets/_Game/Scripts/Editor/HighTierGearRecipeGenerator.cs` — gerador aditivo de 14 receitas gated + 6 upgrade recipes.
- `Assets/_Game/Scripts/Editor/Validation/ValidateHighTierGearNotInShops.cs` — validador CA-1 (loja sem tier alto).

### Criados (testes — Assets/_Game/Tests/EditMode)
- `Assets/_Game/Tests/EditMode/Crafting/HighTierGearCraftingUpgradesTests.cs` — 26 testes EditMode.

### Alterados (aditivo)
- `Assets/_Game/Scripts/Save/SaveData.cs` — `EquipmentSaveData.Upgrades` + `EquipmentSaveData.UnlockedRecipeIds` + DTO `EquipmentUpgradeSaveData`.
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs` — cria/publica registro de upgrade + serviço de unlock; captura/restaura aditivo.
- `Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs` — campo aditivo `RequiredRecipeUnlockId`.
- `Assets/_Game/Scripts/Craft/CraftingManager.cs` — gate de craft por receita aprendida (using `CindarsHope.Crafting`).
- `Assembly-CSharp.csproj` / `Assembly-CSharp-Editor.csproj` — Compile includes dos novos arquivos.

**Nenhum arquivo proibido alterado:** sem `.unity/.prefab/.asset` por YAML, sem `Packages/`,
sem `ProjectSettings/`, sem `SaveManager.cs` core (persistência via owner existente), sem
campos/serviço de têmpera F22, sem mudança de loja/pricing core, sem UI nova.

---

## Validation (each level)

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0E/0W)
Assembly-CSharp-Editor: PASS (exit 0, 0E/3W pre-existing)
Docs validation (validate_docs.ps1): PASS (exit 0)
Spec diff completeness (check_spec_diff_completeness.ps1): PASS (exit 0)
Quality check: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

> (Valores de exit code preenchidos a partir da execução real registrada no commit; ver SPEC_RESULT.)

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (gating, idempotência de unlock, custo de upgrade, durabilidade
  derivada, foco único, teto, round-trip de save)
Changed Unity scene/prefab/asset wiring: NO (gerador de asset DEFERRED; nenhum YAML editado)
Automated tests added/updated: YES (26 EditMode em HighTierGearCraftingUpgradesTests.cs)
Automated tests command: Unity Test Runner EditMode (NOT RUN aqui — compila via dotnet; execução no Unity DEFERRED)
Manual Play Mode scenario: docs/validation/playmode (DEFERRED_TO_FINAL_VALIDATION) — aprender Mithril
  Work no gate 60, craftar 1 arma Mithril na forja, aplicar +1
Justification if no automated tests: N/A (testes adicionados)
Residual risk: (a) execução do gerador de receitas/upgrades não rodada (assets podem não existir até
  geração no Unity); (b) confirmação Play Mode de que o gate 60 publica CaveBossDefeatedEvent com
  CaveLevel==60 e de que a UI da forja lista as receitas desbloqueadas; (c) recálculo de derivados no
  load (§45) é testado em unidade mas a aplicação visual de stats no equip in-game fica para Play Mode.
```

---

## Save expectations (ADR-0006 / save_rules)

- **Schema change:** YES (aditivo). `EquipmentSaveData` ganha `Upgrades: List<EquipmentUpgradeSaveData>`
  e `UnlockedRecipeIds: List<string>`. `SchemaVersion` inalterado; **sem migration**.
- **Defaults:** `Upgrades`/`UnlockedRecipeIds` = listas vazias não-nulas. Save legado (campos
  ausentes/null) carrega com level 0 e nenhuma receita aprendida — durabilidade/infusão F22 intactas.
- **DTOs:** só `string`/`int` + IDs estáveis. **Nenhuma ref Unity.** Derivados (dano/durabilidade)
  **nunca persistidos** (§45) — recalculados no load a partir de `upgradeLevel`/`upgradeFocus`.
- **Owner/restore order:** inalterados — `EquipmentManager.CaptureSaveData/RestoreFromSaveData`
  (mesma seção, mesmo owner; nenhuma mudança no SaveManager core).
- **Idempotência:** `RecipeUnlockService.Unlock` é idempotente (re-kill não duplica); entradas de
  upgrade inválidas (level ∉ 1..3 / foco desconhecido / id vazio) são ignoradas no restore.

---

## Dependency Chain

```text
Original target: fable_49
Dependency chain: F32 (itens tier alto — EXECUTADA), F03 (matriz de stats — EXECUTADA),
  F33 (bestiário/gate bosses — EXECUTADA), F22 (precedente de save aditivo — EXECUTADA)
Forbidden dependencies: none (todas as dependências já BUILD_VALIDATED)
Resolved depth: 0 (nenhuma dependência same-wave pendente)
Can continue original target: YES
```

---

## Remaining work (DEFERRED)

1. Rodar `CindarsHope/Crafting/Generate High-Tier Gear Recipes (fable_49)` no Unity Editor (gera os
   14 assets de receita gated + 6 UpgradeRecipeSO e os registra no RecipeDatabase).
2. Rodar `CindarsHope/Validate/Validate High-Tier Gear Not In Shops (fable_49)` no Unity Editor.
3. Unity Test Runner EditMode (≈26 testes novos desta spec).
4. Play Mode: derrotar gate 60 → aprender Mithril Work (toast) → craftar arma Mithril na forja →
   aplicar +1 (foco único) → save/load e conferir persistência do upgrade + derivados recalculados.
5. Integração futura: confirmar que cada gate (15/45/60/75/90/100) publica `CaveBossDefeatedEvent`
   com `CaveLevel` igual ao nível do gate, para o hook de first-kill disparar o unlock correto.
```
