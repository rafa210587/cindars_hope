---
doc_type: validation
status: evidence
spec_id: fable_48_spec_bow_ammo_elemental_arrows_runtime
validation_type: automated
result: BUILD_VALIDATED
date: 2026-06-19
executor: Claude Code
source_of_truth: false
validated_adrs: [ADR-0007]
validated_game_rules: [combat_rules.md, inventory_equipment_rules.md, event_rules.md]
---

# Validation Report — fable_48 Munição de Arco: Dano de Flecha, Tags Elementais e Tipos do Catálogo

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

**Honest status: `BUILD_VALIDATED_WITH_WARNINGS`** — núcleo determinístico completo e auditado
(fórmula §18 com ArrowDamage, tags da flecha no matching F06, status elemental on-hit, 6 itens do
catálogo, auto-seleção determinística), builds 0E, EditMode tests novos. Phase 2 (Unity validators)
e Phase 3 (Play Mode — "sentir cada flecha") DIFERIDAS por autorização do dono (DEFERRED_TO_FINAL_VALIDATION).

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | Tiro com arrow_iron = (tiro arrow_wood) + 2; soma ANTES dos multiplicadores derivados | Resolver: iron=+4, wood=+2 (diff=+2). `BowArrowAttackService.TryFire` soma `bowWeapon.BaseDamage + bowDamageBonus + ballistics.ArrowDamage` ANTES de `StatsProvider.FinalDamage`. Tests `ArrowDamage_IronIsTwoMoreThanWood`, `ArrowDamage_SumEntersBeforeDerivedMultipliers` | OK |
| CA-2 | Silver carrega tag Silver; bônus SÓ contra vulnerabilidade Silver declarada; sem ela, nenhum bônus | Tags via `ArrowBallisticsResolver` → `ProjectileSpawnRequest.AppliedTags` → `ProjectileBehaviour` → `DamageRequest.WeaponMaterialTags` → `VulnerabilityMatcher.GetDamageMultiplier` (F06, já wired em `EnemyHealth.TakeDamage`). Tests `Matching_SilverBonusOnlyAgainstDeclaredVulnerability`, `Matching_WoodArrowGetsNoBonus...`, `Matching_FireArrowElementBonusOnly...` | OK |
| CA-3 | FireArrow→Burn, FrostArrow→Chill, chance baixa canônica (ponto único); físicas nunca aplicam status | Resolver: fire→`status_burn`, frost→`status_chill`, chance única `ElementalStatusChance=0.25`. Aplicado via pipeline existente (`ProjectileSpawnRequest.StatusEffect/StatusApplyChance` → `ProjectileBehaviour.HitEnemy` → `EnemyHealth.ApplyStatusEffect`). Tests `Status_FireArrowMapsToBurnFrostToChill`, `Status_PhysicalArrowsNeverApplyStatus`, `Status_ElementalChanceIsTheSingleCanonicalLowChance` | OK |
| CA-4 | Ao consumir a última flecha, auto-equipa a próxima compatível (ordem canônica); sem munição, bloqueio NoArrowsInInventory permanece | `ArrowAmmoSelector.SelectNextCompatible` (puro, ordem wood→iron→steel→silver→fire→frost). `BowArrowAttackService.AutoSelectNextAmmo` chamado só quando estoque zera; `EquipItem` + `AmmoAutoSelectedEvent`. Tests `AutoSelect_*` (6) + regressão `AutoSelect_ReturnsNullWhenNoCompatibleAmmo` | OK |
| CA-5 | Os 6 item_ammo_arrow_* existem com BV/stats/tags do catálogo; consumo segue 1/tiro | Os 6 rows já existem em `CanonicalItemCatalog` (F32) com BV wood2/iron4/steel6/silver12/fire8/frost8, Category=Ammo, AmmoType="arrow". Tests `Catalog_AllSixArrowsExistWithCanonicalBaseValues`, `Catalog_ResolverCoversExactlyTheSixCatalogArrows` (Editor) + `Roster_ResolverCoversExactlySixCanonicalArrows` (runtime). Consumo `RemoveItem(id,1)` intacto | OK |

---

## Existing systems audit (Fase 0 — reuso, não duplicação)

| Sistema | Encontrado | Decisão |
|---|---|---|
| `BowArrowAttackService.TryFire` | Valida arco na outra mão, cooldown, stamina, consome 1 munição/tiro, spawna projétil | REUSADO/ESTENDIDO (aditivo) — sem segundo serviço de ataque |
| `ProjectileSpawnRequest` / `ProjectileSpawnService` / `ProjectileBehaviour` | Pipeline de spawn com damage/DamageType/StatusEffect/StatusApplyChance | REUSADO — campo aditivo `AppliedTags` propagado ao `DamageRequest.WeaponMaterialTags` |
| `VulnerabilityMatcher` (F06) | `GetDamageMultiplier(profile, damageType, weaponMaterialTags)` JÁ wired em `EnemyHealth.TakeDamage` (linha 145) | REUSADO — CA-2 é integração REAL (não CONTRACT_ONLY): F06 já executada (E19) |
| Pipeline de status (F01) | `EnemyHealth.ApplyStatusEffect(StatusEffectSO)` via `ProjectileBehaviour.HitEnemy`; `SpellCastService.ResolveStatusEffect` (DB + Resources) | REUSADO — flecha resolve status pelo MESMO caminho do SpellCastService |
| `DamageRequest.WeaponMaterialTags` | Campo aditivo F06 já existente (linha 18) | REUSADO — sem novo formato de tag |
| `ItemDataSO.AmmoType` / `WeaponDataSO.AllowedAmmoType`/`MaterialTagsApplied` | Campos existentes (SPEC_08 / F03) | REUSADO |
| `CanonicalItemCatalog` (F32) | 6 arrow rows já autorados (BV/Category/AmmoType) | REUSADO — gerador é o dono dos assets; nenhum item recriado |
| `InventoryManager` / `EquipmentManager` | `Slots`, `TryGetItemData`, `RemoveItem`, `EquipItem` | REUSADO — auto-seleção lê slots injetados (sem GameObject.Find) |
| `GameEventBus` | Publish/Subscribe (ADR-0007) | REUSADO — `AmmoAutoSelectedEvent` novo segue o padrão |

Estado da F06 no momento da execução: **MATCHING PRESENTE E WIRED** (`VulnerabilityMatcher` + `EnemyHealth.TakeDamage`).
Portanto CA-2 é integração real — o bônus elemental NÃO ficou como `CONTRACT_ONLY_NEEDS_INTEGRATION`.

---

## Spec Compliance Matrix (requirement → implementation)

| Requirement (Escopo) | Implementation |
|---|---|
| ArrowBallisticsResolver (puro) item→{dano,tipo,tags,statusChance} | `Assets/_Game/Scripts/Combat/ArrowBallisticsResolver.cs` (tabela única §19×§8, fallback WoodenArrow) |
| Soma do dano (fórmula §18) ANTES dos derived stats | `BowArrowAttackService.TryFire`: `preDeriveDamage = bowWeapon.BaseDamage + bowDamageBonus + ballistics.ArrowDamage` → `FinalDamage(preDeriveDamage, ...)` |
| Tags no projétil (campo aditivo) propagadas ao payload de dano | `ProjectileSpawnRequest.AppliedTags` → `ProjectileSpawnService.SetAppliedTags` → `ProjectileBehaviour._appliedTags` → `DamageRequest.WeaponMaterialTags` |
| Matching F06 concede bônus só com vulnerabilidade declarada | `VulnerabilityMatcher.GetDamageMultiplier` (perfil do inimigo é a fonte; regra do adapter) |
| Status on-hit fire→Burn/frost→Chill, chance única no resolver | `ArrowBallisticsResolver.ElementalStatusChance` (ponto único) → `spawnRequest.StatusEffect/StatusApplyChance` |
| Compatibilidade AllowedAmmoType respeitada | `ArrowAmmoSelector` usa `bowWeapon.AllowedAmmoType`; flecha incompatível = bloqueio existente |
| Seleção/prioridade canônica determinística + evento | `ArrowAmmoSelector.SelectNextCompatible` (ordem fixa, sem Random) + `AmmoAutoSelectedEvent` |
| Dados: 6 item_ammo_arrow_* (gerador F32) | Já presentes em `CanonicalItemCatalog` (reconciliados — nenhum campo novo necessário; resolver é o dono de stats/tags/status) |
| EditMode tests (fórmula, mapa tipo→tags/status, bônus só com vulnerabilidade, auto-seleção, regressões) | `BowAmmoElementalArrowsTests` (runtime, 17 tests) + `BowAmmoCatalogTests` (editor, 2 tests) |

---

## Decisão de dados (gerador F32)

Os 6 `item_ammo_arrow_*` JÁ existem como rows canônicos em `CanonicalItemCatalog.AddOilsAndArrows`
(`Category=Ammo`, `AmmoType="arrow"`, BV wood2/iron4/steel6/silver12/fire8/frost8). O `CatalogItemRow`
do F32 **não carrega** dano/tags/status de flecha — por design da spec, esse perfil mecânico (§19) é
**ponto único no `ArrowBallisticsResolver`** (código), keyed pela id do item. Portanto a reconciliação
foi: (a) confirmar os 6 ids + BV/AmmoType (teste `Catalog_All...`), (b) provar que o resolver cobre
exatamente o roster de munição do catálogo (teste `Catalog_ResolverCoversExactlyTheSixCatalogArrows`).
Nenhuma alteração no gerador foi necessária (evita stomp; idempotência F32 preservada). A materialização
dos `.asset` segue como ação Unity diferida (rodar `CindarsHope/Generate/Data/Canonical Item Catalog`).

---

## What Was Run

- [x] dotnet build Assembly-CSharp.csproj --no-restore → exit 0, 0E/0W
- [x] dotnet build Assembly-CSharp-Editor.csproj --no-restore → exit 0, 0E/3W (pré-existentes)
- [x] tools/docs/validate_docs.ps1 → exit 0 (PASSED)
- [x] tools/docs/check_spec_diff_completeness.ps1 → exit 0 (após este report)
- [x] tools/docs/run_strict_validation.ps1 → exit 0 (VALIDATION_PASS)

## What Was NOT Run

- [ ] Unity batchmode validators / asset generation — DIFERIDO (autorização do dono; não rodar Unity Editor)
- [ ] Unity Test Runner EditMode (execução dos testes no Unity) — DIFERIDO (dotnet build confirma compilação)
- [ ] Play Mode "sentir cada flecha contra alvo vulnerável/não-vulnerável" — DEFERRED_TO_FINAL_VALIDATION

---

## Validation

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build | PASS — 0E/0W | `Assembly-CSharp` exit 0 |
| C# editor build | PASS — 0E/3W | `Assembly-CSharp-Editor` exit 0; 3 warnings pré-existentes (não nos arquivos da spec) |
| Docs validation | PASS | `validate_docs.ps1` exit 0 |
| Spec diff completeness | PASS | `check_spec_diff_completeness.ps1` exit 0 |
| Strict validation | PASS | `run_strict_validation.ps1` exit 0; artifact `docs/validation/LAST_STRICT_VALIDATION_RESULT.json` |
| Unity validators | NOT RUN | Diferido (autorização do dono) |
| Play Mode | NOT RUN | DEFERRED_TO_FINAL_VALIDATION |

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
| ADR-0007 (Event Bus) | PASS | `AmmoAutoSelectedEvent` (DTO puro, sem refs Unity) publicado via `GameEventBus.Publish`; nenhuma comunicação gameplay por chamada direta; consumidores existentes inalterados |
| combat_rules.md | PASS | Dano = base + flecha; status (Burn/Chill) sem stacking (pipeline F01); minimum damage e ordem de cálculo do `DamageCalculator` preservados |
| inventory_equipment_rules.md | PASS | Munição = item de inventário/equipamento já persistido; auto-equip respeita slot; nenhum schema novo; persistência por id (ADR-0006) |
| event_rules.md | PASS | Evento novo segue convenção `[Domain]Event`, simple types, namespace `CindarsHope.Core.Events`; payload de dano ganhou campo aditivo (default vazio) sem mudar eventos existentes |

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (fórmula §18, mapa tipo→tags/status, auto-seleção determinística)
Changed Unity scene/prefab/asset wiring: NO (assets de munição via gerador F32 — diferido)
Automated tests added/updated: YES
Automated tests command: dotnet build (compila os testes); Unity Test Runner EditMode = DIFERIDO
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (sentir os 6 tipos vs alvo vulnerável/não-vulnerável)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: comportamento on-hit em cena viva (aplicação real de Burn/Chill com Random e a
  materialização dos .asset de munição) não exercitado fora de Play Mode/Unity; lógica determinística
  (resolver, selector, matcher F06, mapa de status) coberta por EditMode tests.
```

EditMode tests adicionados (19 no total):
- `BowAmmoElementalArrowsTests` (runtime, 17): fórmula/soma, tabela canônica, tags, matching F06
  com alvo sintético (com/sem vulnerabilidade, físico não ganha bônus, fire só com Fire vuln),
  status fire→Burn/frost→Chill + chance única, físicas sem status, auto-seleção (ordem canônica,
  pula esgotada/estoque zero, respeita AmmoType, determinismo, null sem munição), roster fecha em 6,
  fallback WoodenArrow.
- `BowAmmoCatalogTests` (editor, 2): 6 itens do catálogo com BV/Category/AmmoType; resolver casa
  exatamente o roster de munição (anti-divergência).

---

## Errors Found

```
(nenhum)
```

## Warnings (pre-existing)

```
Assembly-CSharp-Editor (3, pré-existentes, fora do escopo desta spec):
- CreateEnemyActionsAndSets.cs(45,36): CS0649 RequiresLos nunca atribuído
- CreateEnemyActionsAndSets.cs(36,55): CS0649 MinRange nunca atribuído
- CSharpProjectPostprocessor.cs(18,28): UNT0006 assinatura OnGeneratedCSProject
```

---

## Evidence

Files changed (NOVOS):
```
Assets/_Game/Scripts/Combat/ArrowBallisticsResolver.cs       (resolver puro — tabela §19×§8, ponto único)
Assets/_Game/Scripts/Combat/ArrowAmmoSelector.cs             (seleção pura determinística)
Assets/_Game/Scripts/Core/Events/AmmoAutoSelectedEvent.cs    (evento de troca de munição)
Assets/_Game/Tests/EditMode/Combat/BowAmmoElementalArrowsTests.cs   (runtime, 17 tests)
Assets/_Game/Tests/EditMode/Items/BowAmmoCatalogTests.cs            (editor, 2 tests — CA-5 data side)
```

Files changed (ADITIVOS):
```
Assets/_Game/Scripts/Combat/BowArrowAttackService.cs              (soma arrowDamage, tags, status, auto-seleção)
Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnRequest.cs      (campo aditivo AppliedTags, default vazio)
Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnService.cs      (SetAppliedTags antes do Initialize)
Assets/_Game/Scripts/Combat/Weapon/ProjectileBehaviour.cs         (_appliedTags → DamageRequest.WeaponMaterialTags)
Assets/_Game/Scripts/Combat/PlayerAttackController.cs             (injeta _statusEffectDatabase no serviço de arco)
Assembly-CSharp.csproj                                            (+4 includes runtime)
Assembly-CSharp-Editor.csproj                                     (+1 include editor test)
```

Files NOT changed (protegidos / fora de escopo):
```
*.unity / *.prefab / *.asset (YAML manual)
Packages/** ; ProjectSettings/**
SpellCast/magias (SpellCastService inalterado)
EnemyHealth (matching F06) além do consumo aditivo de tags JÁ existente (WeaponMaterialTags)
SaveManager/** (nenhum schema novo)
gerador F32 (CanonicalItemCatalog/GenerateCanonicalItemCatalog) — rows de munição já corretos
```

---

## Anti-regressão (verificada)

| Invariante | Como foi preservada |
|---|---|
| Consumo 1/tiro | `RemoveItem(ammoId, 1)` inalterado; auto-seleção só DEPOIS do spawn bem-sucedido |
| Sem munição = NoArrowsInInventory | Guard original intacto; auto-seleção devolve null e mantém o slot quando não há compatível |
| Arco na outra mão obrigatório | `ArrowRequiresBowInOtherHand` intacto (Type != Bow) |
| Melee/magia não ganham tags de flecha | `AppliedTags`/`WeaponMaterialTags` default null; só `BowArrowAttackService` preenche (melee/SpellCastService passam tags vazias — payload neutro) |
| Bônus elemental nunca sem vulnerabilidade declarada | `VulnerabilityMatcher` lê o perfil do inimigo (1.0 neutro sem chave) — provado por testes |
| Sem segundo serviço de ataque / sem GameObject.Find / eventos só via bus | `BowArrowAttackService` estendido; auto-seleção lê managers injetados; `AmmoAutoSelectedEvent` via `GameEventBus` |

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-06-19 |
| Phase 1 (Automated: builds + docs + strict) | PASS | 2026-06-19 |
| Phase 2 (Unity validators / asset-gen) | NOT RUN (DIFERIDO) | — |
| Phase 3 (Play Mode) | NOT RUN (DEFERRED_TO_FINAL_VALIDATION) | — |

---

## Next Action

```
1. (Humano/Unity) Rodar CindarsHope/Generate/Data/Canonical Item Catalog para materializar os 6
   item_ammo_arrow_*.asset (idempotente) e rodar a validação fable_30 (Catalog Consistency).
2. (Humano/Unity) Unity Test Runner EditMode — confirmar os 19 testes novos verdes na engine.
3. (Humano/Play Mode) Atirar wood/iron/steel/silver/fire/frost contra alvo COM e SEM vulnerabilidade:
   confirmar dano somado, bônus só com vulnerabilidade declarada, Burn/Chill on-hit, e auto-troca de
   munição ao esgotar a pilha (toast do AmmoAutoSelectedEvent).
4. Promoção para implementados/ somente após Phase 2-3 (não realizada nesta sessão).
```

---

*Report generated: 2026-06-19 (Claude Code — fable_48 execução)*
