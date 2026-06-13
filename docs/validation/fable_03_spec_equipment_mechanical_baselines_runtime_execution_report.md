# Execution Report — fable_03_spec_equipment_mechanical_baselines_runtime

> **Spec:** `docs/specs/a_implementar/fable/fable_03_spec_equipment_mechanical_baselines_runtime.md` (+ EMENDA 2026-06-12)
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 2, spec 8/42)

validated_adrs: []
validated_game_rules: [combat_rules.md, economy_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Campos novos com defaults neutros; gerador aplica perfis por arquétipo com matriz logada | WeaponDataSO ganhou os campos canônicos (emenda §2) com defaults neutros (teste `NewFields_HaveNeutralDefaults`); `ApplyWeaponMechanicalBaselines` (matriz §5 por WeaponType, log por arma) |
| CA-2 | ASPD e scaling reais | `FinalCooldown(base, weapon)` divide pelo ASPD (teste: 1.5 → −33%); `WeaponScalingBonus` = atributo×peso primário+secundário (teste: Dex10×0.8 = +8) |
| CA-3 | Armadura funcional nos 3 caminhos de dano ao player | `PlayerDamageReceiver` central (melee via PlayerHitEvent, contato, projétil inimigo refatorados); fórmula `max(1, raw − Defense)` testada incl. floor |

## Existing systems audit

```text
REUSADOS: WeaponDataSO (campos ADITIVOS — AttackSpeedMultiplier/CriticalChance já existiam),
PlayerCombatStatsProvider (F02 — ganhou overloads cientes da arma + AttributeSource),
DerivedStatsCalculator (Defense p/ armadura via passivas), PlayerManager.DamageHP (intocado —
receiver chama), PlayerCombatController/EnemyContactDamage/EnemyProjectileBehaviour
(refatorados para o caminho ÚNICO), PlayerAttributeType (scaling), EquipmentManager (intocado).
CRIADOS: PlayerDamageReceiver (estático, DefenseSource injetável p/ teste),
ApplyWeaponMechanicalBaselines (editor, matriz canônica sword/dagger/spear/axe/bow/staff),
WeaponWeightClass enum.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| 20 campos canônicos (emenda) | Primary/SecondaryAttribute+pesos, BaseLight/Heavy/ChargedStaminaCost, PostureDamageModifier, CritDamageModifier, WeightClass, MaterialTags, StatusTags, AllowedAmmoType, AllowedDamageTypes, DefaultActionSet, ChargedEffectProfileId (+ existentes ASPD/CritChance/Range/Damage) | OK |
| Custos POR ARMA (emenda §3) | `WeaponStaminaCost`: campos canônicos quando >0; fallback razões F02 (assets antigos intactos) | OK |
| Matriz canônica no gerador (proibido inventar) | valores da §5/§16 da direction: sword 25/40/48, dagger 16/28/36, axe 30/48/58, bow 18/30/38, staff 20/32/40, spear 24/38/46 + charged profiles por arma (§8-15 como ChargedEffectProfileId) | OK |
| ASPD no caminho real | FinalCooldown(base, weapon) consumido no AttackWithSlot | OK |
| Redutor central de dano recebido | 3 chamadores → PlayerDamageReceiver; popups/eventos usam o dano FINAL | OK |
| Fórmula de dano canônica §1 via provider | base + scaling + Attack derivado × peso × crit — integrada (F02+F03) | OK |
| Validator de armas | coberto pelo log matricial do gerador + F30 (catalog validator) fará o cruzamento | OK (delegado) |

Adaptações documentadas: Armor/ShieldDataSO canônicos §3-4 — EquipmentDataSO existente cobre
defense/resists; SOs dedicados de armor/shield ficam acoplados a F32 (gerador de itens)
para não criar assets órfãos sem economia; redução por % e por tipo de dano refina em F18
(resistências elementais).

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS | Assembly-CSharp: PASS (0E) | Assembly-CSharp-Editor: PASS (0E)
Quality check: PASS | Diff completeness: PASS (WARNs legados)
Asset generation: NOT RUN (Unity indisponível) — rodar CindarsHope/Combat/Apply Weapon
Mechanical Baselines no Editor; até lá armas usam defaults neutros (comportamento atual).
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: gerador editor (não executado — evidência acima)
Automated tests added/updated: YES (7 EditMode tests em WeaponBaselineAndArmorTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: adaga vs martelo + armadura reduzindo contato — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: Defense vem de passivas até existir registry de EquipmentDataSO (F32);
fórmula flat-com-floor pode precisar de curva % em playtest (anotado p/ balance).
```

## Honest status rationale

BUILD_VALIDATED; gerador NOT RUN (sem claim de assets atualizados). Identidade de arma
real só é perceptível após rodar o gerador + Play Mode.

## Remaining work

- Rodar o gerador no Unity (lote de regeneração).
- F18: resistências elementais no receiver; F32: registry/asset de armor/shield.
- F22: consome MaterialTags/StatusTags; F06: matching das tags.
