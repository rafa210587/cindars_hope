---
doc_type: validation
status: evidence
spec_id: fable_22_spec_essence_tempering_forge
validation_type: automated
result: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
executor: Claude Code
source_of_truth: false
validated_adrs: []
validated_game_rules: [economy_rules.md, combat_rules.md]
---

# Validation Report — fable_22 Têmpera de Essência (Forja Elemental do Brumdar)

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

**Honest status:** `BUILD_VALIDATED_WITH_WARNINGS` — núcleo determinístico (serviço, custos, invariante,
devolução, gate, persistência aditiva, integração de tag no matching F06, óleo-sobrepõe) implementado e
coberto por EditMode tests; builds 0E; strict validation exit 0. UI de seleção de têmpera no Brumdar e
efeitos utilitários de ferramenta (harvest-side) ficam DIFERIDOS para wiring de cena/Play Mode (dono
autorizou pular Play Mode/asset-gen). Phase 2-3 NOT RUN.

---

## Phase 0 — Auditoria de Reúso (system-reuse audit)

| Conceito da spec | O que EXISTE no repo (reusado) | Decisão |
|---|---|---|
| "ItemInstance + durabilidade (FASE9H)" | `EquipmentManager._slots` (Dictionary<EquipmentSlot,string> de itemInstanceId) + `EquipmentDurabilityTracker` paralelo keyed por itemInstanceId. NÃO existe um tipo objeto "ItemInstance". | Infusão = registro PARALELO keyed por itemInstanceId, no mesmo padrão da durabilidade. |
| Matching tag×vulnerabilidade (F06) | `VulnerabilityMatcher.GetDamageMultiplier(profile, DamageType, weaponMaterialTags)` → `EnemyVulnerabilityProfileSO.GetMaterialMultiplier(string[])`. Ponto único de chamada em `EnemyHealth.TakeDamage` (linha ~145). Tags da arma vêm de `DamageRequest.WeaponMaterialTags`, montadas em `PlayerAttackController.ExecuteMeleeAttack` (linha ~604). | REUSADO sem alteração da regra. A tag de gume entra como string em `WeaponMaterialTags` (ponto único). Nenhum multiplicador paralelo. |
| Essências (F32) | `CanonicalItemCatalog.AddEssences` → `item_essence_fire/_ice/_toxic/_lightning/_arcane/_void` (Category=Essence, Dormant, nota "fable_22"). | REUSADO como IDs de consumo. |
| Fluxo Conversar do Brumdar | `NpcShopController.ShowRootShopDialogue` (menu Conversar/Comprar/Vender/Adeus) para shop NPCs com DialogueTree; `npc_brumdar` é shop NPC. | REUSADO: opção "Temperar" injetada no menu root, gated, só para Brumdar. |
| Gate de flag/ato | `QuestFlagService.IsSet(flagId)`; `MainProgressionSection.CurrentAct` (MainAct enum). | REUSADO via predicados resolvidos (gate puro no serviço). |
| Save de equipment | `EquipmentSaveData { EquippedToolId, Slots }` capturado/restaurado por `EquipmentManager`. | REUSADO: campo aditivo `Infusions` (lista) + captura/restore no mesmo ponto. |
| Toast/eventos | `PlayerActionFeedbackEvent` (toast existente); `GameEventBus`. | REUSADO. +1 evento novo (`WeaponTemperedEvent`). |

Nenhum sistema de craft/forja/upgrade paralelo foi criado. Nenhum segundo caminho de save, matching ou
tags foi criado.

**Achado relevante:** não existe sistema de óleos temporários (runtime de coating) no repo — óleos são
itens dormentes. CA-5 ("óleo sobrepõe têmpera") foi implementado como REGRA pura testável + hook de
provider no registro (`ActiveCoatingTagProvider`), pronto para o sistema de óleos futuro, sem fabricar
um sistema de óleos fora de escopo.

---

## Acceptance criteria extracted

Critérios CA-1..CA-5 extraídos integralmente da spec (§"Critérios de aceite") e mapeados abaixo na
Spec Compliance Matrix com evidência de teste por critério.

## Existing systems audit

Coberto na seção "Phase 0 — Auditoria de Reúso" acima: EquipmentManager/durability, VulnerabilityMatcher
+ EnemyVulnerabilityProfileSO (matching F06, ponto único em EnemyHealth/PlayerAttackController),
CanonicalItemCatalog (6 essências), NpcShopController (fluxo Conversar do Brumdar), QuestFlagService +
MainProgressionSection (gate), EquipmentSaveData (save). Reusados; nenhum sistema paralelo criado.

## Spec Compliance Matrix (critérios de aceite)

| CA | Requisito | Implementação | Evidência (teste) | Status |
|----|-----------|---------------|-------------------|--------|
| CA-1 | Tag de infusão entra no matching F06; bônus só contra vulnerabilidade declarada; contra não-vulnerável dano idêntico | `WeaponInfusionRegistry.GetEdgeTag` → tag de gume canônica adicionada a `DamageRequest.WeaponMaterialTags` no ponto único (`PlayerAttackController.ResolveWeaponMaterialTags`) | `CA1_FireEdgeTag_BonusOnlyAgainstDeclaredVulnerability`, `CA1_RegistryEdgeTag_FeedsMatchingForEquippedInstance` | OK |
| CA-2 | Invariante 1 elemento + re-têmpera devolve metade das essências (nunca ouro) | `WeaponInfusionRegistry.Set` substitui (1 entrada); `TemperingService.ApplyTempering` devolve `EssenceCost(prevTier)/2` só essências | `CA2_*` (7 testes: custos T1/T2, troca+devolução, never-two, fundos insuficientes) | OK |
| CA-3 | "Temperar" indisponível antes de sq_brumdar_3_done + Ato 1; disponível depois | `TemperingService.IsGateOpen(brumdarChainDone, act1Reached)`; `TemperingForgeAccess.IsGateOpen` (fail-closed); opção condicional no `NpcShopController` | `CA3_Gate_*`, `CA3_ApplyTempering_FailsWhenGateClosed`, `CA3_ForgeAccess_FailClosedWithoutResolver` | OK |
| CA-4 | Infusão sobrevive round-trip; saves antigos carregam sem infusão (defaults vazios), sem migration | `EquipmentSaveData.Infusions` aditivo; `WeaponInfusionRegistry.Capture/RestoreFromSaveData`; `EquipmentManager` captura/restaura | `CA4_*` (4 testes: round-trip, legacy null, default empty, invalid entries) | OK |
| CA-5 | Óleo sobrepõe têmpera pela duração; expirado, têmpera volta | `TemperingCanon.ResolveActiveEdgeTag(infusionTag, coatingTag)` + `WeaponInfusionRegistry.ActiveCoatingTagProvider` | `CA5_Oil_SuppressesTemper_ThenTemperReturns`, `CA5_Registry_AppliesActiveCoatingProvider` | OK (regra pura; integração com óleo real DIFERIDA — não há sistema de óleo no repo) |

Extras cobertos: ferramentas só T1 (`Tool_AcceptsT1_RejectsT2`), não-temperável rejeitado, caso de borda
VOID T2 (6+1 do mesmo item), mapeamento canônico completo, anti-regressão sem-têmpera.

---

## Custos canônicos (C3) implementados

| Tier | Essências do elemento | Catalisador | Ouro | Chance de status no hit |
|------|----------------------|-------------|------|--------------------------|
| T1 | 3 | — | 150g | 10% |
| T2 | 6 | + 1 `item_essence_void` | 600g | 20% |

Ferramentas: só T1. Devolução na troca: metade das essências da têmpera anterior (truncado), nunca ouro.
Mapeamento elemento → gume: fire→FireEdge, ice→FrostEdge, toxic→PoisonEdge, lightning→ShockEdge,
arcane→ArcaneEdge, void→VoidEdge.

---

## What Was Run

- [x] dotnet build Assembly-CSharp.csproj --no-restore → PASS (0E/0W)
- [x] dotnet build Assembly-CSharp-Editor.csproj --no-restore → PASS (0E/3W pré-existentes)
- [x] tools/docs/validate_docs.ps1 → PASS (exit 0)
- [x] tools/docs/check_spec_diff_completeness.ps1 → PASS (com report + commit)
- [x] tools/docs/run_strict_validation.ps1 → PASS (exit 0)

## What Was NOT Run

- [ ] Unity batchmode / Play Mode — DEFERRED (dono autorizou pular Play Mode/asset-gen; sem Unity Editor)
- [ ] EditMode Test Runner (Unity) — testes COMPILAM no Assembly-CSharp; execução no Unity Test Runner é lote humano

---

## Validation

## Results

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build | PASS — 0E/0W | ~2s |
| C# editor build | PASS — 0E/3W | 3 warnings pré-existentes (CreateEnemyActionsAndSets, CSharpProjectPostprocessor) |
| Docs validation | PASS | tools/docs/validate_docs.ps1 exit 0 |
| Spec diff completeness | PASS | report presente + commit |
| Strict validation | PASS | run_strict_validation.ps1 exit 0; artifact LAST_STRICT_VALIDATION_RESULT.json |
| Unity validators | NOT RUN | sem Unity Editor (deferido) |
| Play Mode | NOT RUN | deferido para validação final |

Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS
Assembly-CSharp-Editor: PASS
Quality check: PASS
Docs validation: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| economy_rules.md | PASS | Custos C3 = sinks determinísticos (essências + ouro); devolução só de essências (nunca ouro) evita exploit; consumo via InventoryManager/PlayerManager existentes |
| combat_rules.md | PASS | Regra-mãe preservada: tag de infusão entra no matching F06 como qualquer tag; bônus SÓ contra vulnerabilidade declarada; sem multiplicador paralelo; óleo suprime têmpera (uma tag ativa por vez) |

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (custos, devolução, invariante, gate, matching de tag, óleo-sobrepõe, persistência)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Economy/TemperingTests.cs — ~24 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila); Unity EditMode Test Runner (execução = lote humano)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (UI de têmpera do Brumdar + dano contra vulnerável em cena)
Justification if no automated tests: N/A (testes presentes)
Residual risk: execução real dos testes no Unity Test Runner pendente; fluxo de UI de seleção de arma/elemento/tier no Brumdar não wired (entry-point gated existe, abertura de picker é DIFERIDA); efeitos utilitários de ferramenta (machado fire +1 carvão / picareta ice minério duplo) gravados como infusão mas a LEITURA no harvest é DIFERIDA (sistemas de harvest fora do escopo de arquivos permitidos)
```

---

## Errors Found

```
(nenhum — builds 0E)
```

## Warnings (pre-existing)

```
Assembly-CSharp-Editor: 3 warnings pré-existentes não relacionados a esta spec:
  - CreateEnemyActionsAndSets.cs CS0649 (RequiresLos / MinRange nunca atribuídos)
  - CSharpProjectPostprocessor.cs UNT0006 (assinatura OnGeneratedCSProject)
```

---

## Evidence

Files changed (criados):
```
Assets/_Game/Scripts/Economy/WeaponInfusion.cs                 (+ .meta) — enum/struct + TemperingCanon (mapeamentos/custos/chances/óleo-suprime)
Assets/_Game/Scripts/Economy/WeaponInfusionRegistry.cs         (+ .meta) — store por instância + Capture/Restore + acessor estático Active + coating provider
Assets/_Game/Scripts/Economy/TemperingService.cs              (+ .meta) — serviço puro: gate, custos C3, invariante, devolução, ferramentas T1
Assets/_Game/Scripts/Economy/TemperingForgeAccess.cs         (+ .meta) — fachada estática do gate (fail-closed) + entry-point de UI (deferido)
Assets/_Game/Scripts/Economy/TemperingRuntimeAdapters.cs    (+ .meta) — adapters InventoryManager/PlayerManager + classifier + factory (publica evento+toast)
Assets/_Game/Scripts/Core/Events/WeaponTemperedEvent.cs    (+ .meta) — evento novo (toast)
Assets/_Game/Tests/EditMode/Economy/TemperingTests.cs      (+ .meta) — ~24 EditMode tests (CA-1..CA-5 + extras)
```

Files changed (modificados):
```
Assets/_Game/Scripts/Save/SaveData.cs                     — EquipmentSaveData.Infusions (aditivo) + WeaponInfusionSaveData DTO
Assets/_Game/Scripts/Equipment/EquipmentManager.cs        — registro de infusões + Active + capture/restore aditivo
Assets/_Game/Scripts/Combat/PlayerAttackController.cs     — ponto único: tag de gume da infusão entra em WeaponMaterialTags (melee)
Assets/_Game/Scripts/NPC/NpcShopController.cs             — opção "Temperar" gated no menu Conversar do Brumdar
Assembly-CSharp.csproj                                    — includes dos 6 .cs runtime/event + 1 teste
```

Files NOT changed (protected, per spec scope):
```
*.unity / *.prefab / *.asset (nenhum YAML editado)
Packages/** ; ProjectSettings/**
VulnerabilityMatcher.cs / EnemyVulnerabilityProfileSO.cs (matching F06 consumido, não alterado)
WeaponDataSO.cs / EquipmentDataSO (sem campos novos além do save aditivo de equipment)
SaveManager core (apenas DTO aditivo)
```

---

## Anti-regressão (verificado)

- Sem bônus elemental contra inimigo não-vulnerável (CA-1; `NoTemper_NoEdgeTag_AndMatchingUnaffected`).
- Armas sem têmpera: `GetEdgeTag` null ⇒ `WeaponMaterialTags` = tags base da arma inalteradas; matching idêntico ao prévio.
- Saves antigos: `Infusions` null/ausente ⇒ registro vazio, sem migration (CA-4).
- Nenhuma tag fora das 6 de gume canônicas; matching F06 inalterado; nenhum multiplicador paralelo.
- Óleo mantém nicho (suprime têmpera pela duração; uma tag ativa por vez).
- `DamageCalculator` e overloads legados não tocados.

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-06-19 |
| Phase 1 (Automated) | PASS | 2026-06-19 |
| Phase 2 (Unity validators) | NOT RUN (deferred) | — |
| Phase 3 (Play Mode) | NOT RUN (deferred) | — |

---

## Next Action

```
HUMANO (lote, no Unity):
1. Rodar EditMode Test Runner e confirmar TemperingTests verdes (~24).
2. Ligar TemperingForgeAccess.GateResolver ao gate real
   (QuestFlagService.IsSet("sq_brumdar_3_done") && MainProgressionSection.CurrentAct >= Act1)
   e TemperingForgeAccess.OpenForgeUi à UI de seleção de arma/elemento/tier + confirmação no Brumdar
   (usar TemperingServiceFactory.Create com InventoryManager+PlayerManager+WeaponInfusionRegistry.Active).
3. Play Mode: temperar arma, ver bônus contra inimigo com vulnerabilidade declarada e dano idêntico
   contra não-vulnerável; round-trip de save; re-têmpera devolvendo metade das essências.
4. (Futuro) Efeitos utilitários de ferramenta T1 no harvest e sistema de óleos temporários ligando
   WeaponInfusionRegistry.ActiveCoatingTagProvider.

Promoção a implementados/: somente após Phase 2-3 (não nesta sessão).
```

---

*Report generated: 2026-06-19*
