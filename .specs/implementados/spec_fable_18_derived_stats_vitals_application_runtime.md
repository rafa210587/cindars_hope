# SPEC — Atributos Derivados: Aplicação em Vitals (MaxHP/Stamina/Mana, Regen, Resistências)

> **Spec ID:** `fable_18_spec_derived_stats_vitals_application_runtime`
> **Status:** BUILD_VALIDATED (executada — E09; evidência: docs/validation/fable_18_spec_derived_stats_vitals_application_runtime_execution_report.md)
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **Priority:** P1 (corretiva — complementa F02)
> **Type:** Integration / Runtime
> **Domain:** Player
> **Parallelizable:** NO
> **Parallel group:** fable_corretivas
> **Can run with:** N/A
> **Must not run with:** fable_02 (deve executar DEPOIS — usa PlayerCombatStatsProvider), fable_16
> **Repo lock scope:** PlayerManager/StaminaManager/ManaManager/HungerManager, `PlayerCombatStatsProvider`
> **Depends on:** `fable_02` (provider/invalidação)
> **Blocks:** N/A
> **Scope:** aplicar TODAS as saídas do DerivedStatsCalculator que F02 não cobre aos managers de vitals.
> **Out of scope:** novos atributos, UI de stats (F14 equipment screen exibe), rebalance.

required_adrs: []
required_game_rules: [player_rules.md]

---

# /speckit.specify

## Contexto

Auditoria 00B: `DerivedStatsCalculator` calcula MaxHP, MaxStamina, StaminaRegen, MaxMana,
ManaRegen, MoveSpeed, Toxic/Cold/HeatResistance, BowRange/BowDamageBonus,
CraftTimeReduction, RepairEfficiencyBonus, HungerDrainReduction — e ninguém chama. F02
corrige Attack/AttackSpeed via `PlayerCombatStatsProvider`. Esta spec aplica o RESTO:
equipar armadura com +20 HP deve subir o HP máximo; passiva de regen deve regenerar;
resistências devem alimentar o `PlayerDamageReceiver` (F03) e os status (F01).

## Problema

Metade do PLAYER_DERIVED_ATTRIBUTES continua decorativa mesmo após F02/F03: builds de
sobrevivência (HP/regen/resistência/fome) não existem, e itens da loja com esses bônus são
propaganda enganosa.

## Objetivo

Ao final desta spec, o `PlayerCombatStatsProvider` (F02) deve expor o DerivedStats completo
e um `PlayerVitalsApplier` deve sincronizar Max/Regen nos managers (preservando proporção
corrente ao mudar máximos), resistências no receiver/status e HungerDrainReduction no
HungerManager — com recálculo nos mesmos eventos de invalidação de F02.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/a_implementar/fable/fable_00B_adherence_audit_queue_triage.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- DerivedStatsCalculator (completo, órfão); PlayerCombatStatsProvider (F02)
- PlayerManager (HP), StaminaManager (Initialize(max,start), AddStamina), ManaManager,
  HungerManager; PlayerDamageReceiver (F03); StatusEffectManager (F01)
Auditar Fase 0:
- managers expõem setters de Max/Regen? (StaminaManager.Initialize existe; HP/Mana auditar)
- regen atual: onde stamina/mana regeneram hoje (loop de quem?)
```

## Escopo

```text
Inclui:
- PlayerVitalsApplier (NOVO, hospedado no provider F02): aplica MaxHP/MaxStamina/MaxMana
  (proporção preservada: 50% de 100 → 50% de 120), StaminaRegen/ManaRegen (APIs de regen —
  adicionar setters mínimos aos managers SE ausentes, sem reescrevê-los);
- resistências Toxic/Cold/Heat → PlayerDamageReceiver (redução por DamageType correspondente)
  e duração de status (F01: Poison/Chill encurtados por resistência — fórmula documentada);
- HungerDrainReduction → HungerManager (multiplicador de drain);
- MoveSpeed derivado → PlayerController (multiplicador, compondo com Chill/fadiga, floor 0.5x);
- BowRange/BowDamageBonus → BowArrowAttackService (consume provider);
- CraftTimeReduction/RepairEfficiencyBonus → CraftingRuntime/EquipmentManager (hooks;
  se integração for invasiva, registrar como follow-up explícito no report — não inflar escopo);
- EditMode tests: proporção em mudança de máximo, composição de multiplicadores, resistências.
```

## Fora de escopo

```text
Não inclui: UI; rebalance de valores; atributos novos; respec.
```

## Regras de não duplicação

```text
Uma única fonte de stats: provider F02. Proibido segundo applier ou cálculo inline em managers.
```

## Critérios de aceite

### CA-1 Vitals derivados
- Equipar item +MaxHP sobe máximo preservando proporção; desequipar reverte sem matar o player (floor 1 HP).
### CA-2 Resistências e fome
- ToxicResistance reduz dano Toxic e duração de Poison; HungerDrainReduction mensurável.
### CA-3 Recálculo por evento
- Equip/unequip/compra de passiva recalcula tudo (mesma invalidação F02).
- Evidência: EditMode tests com stats sintéticos.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs (NOVO)
Assets/_Game/Scripts/Player/{Stamina,Mana,Hunger}Manager.cs (setters mínimos se ausentes)
Assets/_Game/Scripts/Combat/{PlayerCombatStatsProvider, PlayerDamageReceiver, BowArrowAttackService}.cs (integração)
Assets/_Game/Tests/EditMode/Player/VitalsApplicationTests.cs
```

## Paralelização

- Parallelizable: NO — managers centrais.

## Impacto em save/load

```text
Schema change: NO (máximos derivam de equipment/skills já persistidos; HP corrente já salvo).
```

## Impacto em eventos / UI

```text
Adds events: NO | UI: NO | Play Mode final: YES | Human timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos

```text
Risco: load aplicar máximos antes do restore de equipment → clamp errado.
Mitigação: applier roda pós-restore (ordem documentada) + teste de sequência.
```

## Rollback

```text
Remover applier; managers com setters extras inertes.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar setters/regen atuais dos managers.
- [ ] T002 — Provider expõe DerivedStats completo; applier com proporção.
- [ ] T003 — Resistências no receiver + duração de status.
- [ ] T004 — Hunger/MoveSpeed/Bow integrações.
- [ ] T005 — Craft/Repair hooks ou follow-up documentado.
- [ ] T006 — Testes; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES | EditMode tests: YES | PlayMode/human: YES (lote)
- Regression: YES (vitals sem equipment = atuais) | Human timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum evidence for ACCEPTED: testes + cenário humano equipando item de +HP

## Definition of Done

```text
DerivedStats 100% aplicado (ou follow-ups explícitos); zero regressão sem equipment; builds 0E; report.
```

## Anti-regressão

```text
Player sem equipment/passivas = valores atuais exatos. Restore order respeitada. Floor 1 HP.
```
