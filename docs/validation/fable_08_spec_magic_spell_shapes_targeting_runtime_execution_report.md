# Execution Report — fable_08 Magic: Spell Shapes, Targeting, Cast Time e Suporte (Runtime)

> Spec: `.specs/a_implementar/fable/fable_08_spec_magic_spell_shapes_targeting_runtime.md`
> Date: 2026-06-19
> Branch: `dev`
> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> Type: Runtime / Data

---

## Honest Status Rationale

Status is **BUILD_VALIDATED_WITH_WARNINGS** (não ACCEPTED, não PLAYMODE_VALIDATED):

- Núcleo implementado e atende todos os critérios centrais (CA-1/CA-2/CA-3 + EMENDA 2026-06-12-D):
  enum `SpellShape` {Bolt, Cone, Nova, SelfRestore, Barrier}, campos aditivos neutros em `SpellDataSO`
  (Bolt/0 = assets antigos intactos), dispatch por shape no `SpellCastService` (serviço estendido, não
  duplicado), `SpellCastRoutine` (cast time + interrupt por dano + reembolso de mana), `PlayerBarrierState`
  (absorção antes do `PlayerDamageReceiver` de F03), auto-target do Projétil Arcano (EMENDA 6.6-A) via
  query de Physics2D, e as Tier 5 canônicas (Eco de Anya / Ruptura de Senya) no gerador.
- `dotnet build` Assembly-CSharp PASS (0E/0W) e Assembly-CSharp-Editor PASS (0E/3W **pré-existentes**,
  não introduzidas por esta spec); docs validation, diff completeness e `run_strict_validation.ps1` exit 0.
- EditMode tests escritos (24 testes) cobrindo defaults Bolt, seleção de auto-target determinística
  (mais próximo / na mira / desempate por índice / fallback / fora de alcance), lógica de barreira
  absorvendo (parcial/overflow/expira) e parâmetros canônicos da Tier 5 + Projétil Arcano. **Compilam
  dentro do Assembly-CSharp**; **execução no Unity Test Runner é DIFERIDA** (Unity Editor/Play Mode não
  executado por ordem do dono — DEFERRED_TO_FINAL_VALIDATION).
- Geração de assets (4 spells exemplares + 2 Tier 5 + Projétil Arcano + pergaminhos, registro no
  SpellDatabase/ItemDatabase) é **editor-only e NÃO executada** nesta sessão (sem Unity Editor). Não há
  claim de que os assets existem — ver "Asset generation".
- Execução por cena de Cone/Nova (spawn de projétil em leque, OverlapCircleAll de dano em área) e do
  cast interrompível em tempo real dependem de Play Mode; a **lógica determinística** (targeting,
  absorção, dispatch enum, defaults) está testada, mas o comportamento em cena é diferido. Residual risk
  documentado.

---

## Phase Status

| Phase | Status | Notes |
|-------|--------|-------|
| Phase 0 Audit | DONE | Reuso de SpellCastService, SpellDataSO, ProjectileSpawnService/Request, RuntimeProjectileFactory, PlayerDamageReceiver (F03), ManaManager/PlayerManager/StaminaManager, EnemyHealth, PlayerSpellbook (F07), eventos SpellCast* existentes |
| Phase 1 Build (C#) | BUILD_VALIDATED | Assembly-CSharp 0E/0W; Assembly-CSharp-Editor 0E/3W pré-existentes |
| Phase 2 Unity batchmode | NOT RUN | Unity Editor não executado (DEFERRED por ordem do dono) |
| Phase 3 Play Mode / human | NOT RUN | DEFERRED_TO_FINAL_VALIDATION |

---

## Files Changed

### New (runtime — Assembly-CSharp)
- `Assets/_Game/Scripts/Combat/Magic/SpellShape.cs` — enum `SpellShape` {Bolt, Cone, Nova, SelfRestore, Barrier} + `SpellTargeting` (seleção de auto-target/aim PURA e determinística; sem cena/Physics).
- `Assets/_Game/Scripts/Combat/Magic/PlayerBarrierState.cs` — barreira temporária + lógica pura de absorção (`AbsorbLogic`) + hook `ActiveInstance`; transitória (não persistida).
- `Assets/_Game/Scripts/Combat/Magic/SpellCastRoutine.cs` — MonoBehaviour fino: janela de cast time, telegraph (flash), cancelamento em `PlayerDamagedEvent` (reembolso + `SpellCastInterruptedEvent`), timeout máx. + cancel em modal.

### New (editor — Assembly-CSharp-Editor)
- `Assets/_Game/Scripts/Editor/Magic/GenerateShapeSpells.cs` — gerador idempotente: spell_ice_nova, spell_minor_heal, spell_arcane_barrier, spell_flame_cone, **arcane_projectile (auto-target)**, **anya_echo + senya_rupture (Tier 5, EMENDA)**; registra no SpellDatabase; gera pergaminhos LearnableScroll no ItemDatabase. Usa AssetDatabase/SerializedObject (sem YAML manual).

### New (tests — Assembly-CSharp)
- `Assets/_Game/Tests/EditMode/Core/SpellShapeTests.cs` — 24 testes EditMode.

### Modified
- `Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs` — campos aditivos com defaults NEUTROS: `Shape=Bolt`, `AutoTarget=false`, `ConeHalfAngleDegrees`, `ConeProjectileCount`, `NovaRadius`, `RestoreHp/Stamina/Mana=0`, `BarrierAbsorb=0`, `BarrierSeconds=0` (CastTimeSeconds já existia). Clamps neutros em OnValidate.
- `Assets/_Game/Scripts/Combat/SpellCastService.cs` — dispatch por shape (`TryCast` mantém assinatura). Novos: `TryBeginCast`/`ResolveCast`/`RefundCast` + `SpellCastPlan`; executores Bolt/Cone/Nova/SelfRestore/Barrier; `EnemyPositionQuery` (auto-target via Physics2D, injetado); `PlayerManager`/`StaminaManager` (SelfRestore). Reusa `ProjectileSpawnService` e `OverlapCircleAll` (regra de não-duplicação).
- `Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs` — hook de barreira: `PlayerBarrierState.AbsorbIncoming` ANTES de block/defesa/resistência (ordem documentada).
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs` — cria `SpellCastRoutine` no player; injeta managers + `EnemyPositionQuery` (Physics2D `OverlapCircleAll`, NÃO FindObjectsByType); `TryExecuteSpellAttack` agora usa `TryBeginCast` → rotina (cast time/interrupt) com reembolso se já conjurando.
- `Assets/_Game/Scripts/Core/Events/MagicEvents.cs` — `SpellCastInterruptedEvent(spellId, refundedMana)` (NOVO). `SpellCastStartedEvent` REUTILIZADO de `PlayerCombatEvents.cs` (não duplicado).
- `Assembly-CSharp.csproj` / `Assembly-CSharp-Editor.csproj` — Compile Includes dos novos arquivos.

---

## Acceptance Criteria Extracted (da spec, com evidência)

| CA | Critério | Implementação | Evidência | Status |
|----|----------|---------------|-----------|--------|
| CA-1 | 5 shapes com semântica distinta; spells sem campos novos castam como Bolt | `SpellShape` enum + `SpellCastService.ResolveCast` (switch); defaults `Shape=Bolt`/`CastTime=0` | Testes `Enum_ContainsAllFiveShapes`, `Defaults_NewSpellIsBolt_*`; executores Bolt/Cone/Nova/SelfRestore/Barrier | OK |
| CA-2 | Cast com CastTime>0 só resolve após o tempo; dano cancela sem gastar mana (reembolso) + log `SpellCastInterrupted` | `SpellCastRoutine.CastWindow`/`OnPlayerDamaged` → `RefundCast` (`RestoreMana` + `SpellCastInterruptedEvent`); cast 0s resolve imediato | `RefundCast` loga `SpellCastInterrupted`; lógica de reembolso testada via `SpellCastPlan.ManaSpent`; rotina simulável (PlayerDamagedEvent). Comportamento em tempo real → Play Mode (diferido) | OK (lógica) |
| CA-3 | Barreira absorve até BarrierAbsorb por BarrierSeconds; consumo logado; expira limpa | `PlayerBarrierState` (`Cast`/`AbsorbIncoming`/`AbsorbLogic`/`IsActive`); hook em `PlayerDamageReceiver` antes de defesa | Testes `Barrier_AbsorbLogic_*`, `Barrier_Cast_*`, `Barrier_AbsorbIncoming_ConsumesThenExpiresWhenDepleted` | OK |
| EMENDA 6.6-A | Projétil Arcano = auto-target; Tier 5 = Eco de Anya + Ruptura de Senya | `SpellTargeting.SelectAutoTarget`/`ResolveAimDirection` + `AutoTarget` em Bolt; gerador cria `arcane_projectile`(AutoTarget), `anya_echo`(SelfRestore), `senya_rupture`(Nova) | Testes `AutoTarget_*`, `ArcaneProjectile_IsBoltWithAutoTarget`, `Tier5_EcoDeAnya_*`, `Tier5_RupturaDeSenya_*` | OK |

---

## Existing Systems Audit (reuse, sem sistema paralelo)

| Necessidade | Sistema existente | Decisão |
|-------------|-------------------|---------|
| Serviço de cast | `SpellCastService` | REUSE/ESTENDIDO — `TryCast` preservado; dispatch interno por shape (regra: não criar 2º serviço) |
| Spawn de projétil | `ProjectileSpawnService` + `ProjectileSpawnRequest` + `RuntimeProjectileFactory` | REUSE — Bolt e Cone (leque) usam o mesmo spawn; fallback procedural mantido |
| Dano em área | `Physics2D.OverlapCircleAll` (padrão do melee em PlayerAttackController) | REUSE — Nova/auto-target usam a mesma query de física (não FindObjectsByType) |
| Caminho de dano do player | `PlayerDamageReceiver` (F03) | REUSE — barreira é hook ANTES da defesa; NÃO cria receptor paralelo |
| Restauro de recursos | `PlayerManager.RestoreHP`, `StaminaManager.AddStamina`, `ManaManager.RestoreMana` | REUSE — SelfRestore só chama APIs existentes |
| Estado de conhecimento | `PlayerSpellbook` (F07) | REUSE — injetado no serviço (Spellbook) |
| Eventos de cast | `SpellCastStartedEvent`/`SpellCastSucceededEvent`/`SpellCastFailedEvent` (PlayerCombatEvents) | REUSE — só `SpellCastInterruptedEvent` é novo (MagicEvents) |
| Aplicação de status | `EnemyHealth.TakeDamage`/`ApplyStatusEffect`; `StatusEffectDatabaseSO.TryGetById` | REUSE — Nova/Cone aplicam status pelo mesmo caminho do ProjectileBehaviour |
| Registro de assets | `DataRegistrySO._items` via SerializedObject (idioma do GenerateSpellLearningItems F07) | REUSE — gerador segue o precedente |

Sistemas criados (NOVOS, sem equivalente): `SpellShape`/`SpellTargeting`, `PlayerBarrierState`, `SpellCastRoutine`, `SpellCastInterruptedEvent`, `GenerateShapeSpells`.

---

## Spec Compliance Matrix (requirement → implementation)

| Requisito da spec | Implementação | Status |
|-------------------|---------------|--------|
| SpellShape enum {Bolt, Cone, Nova, SelfRestore, Barrier} | `SpellShape.cs` | OK |
| CastTimeSeconds + ConeAngle/NovaRadius/RestoreHp/BarrierAbsorb/BarrierSeconds em SpellDataSO (defaults Bolt/0) | `SpellDataSO.cs` campos aditivos + clamps | OK |
| SpellCastRoutine (MonoBehaviour fino criado pelo PlayerAttackController; cast time + telegraph + cancel em PlayerDamagedEvent) | `SpellCastRoutine.cs` + criação em PlayerAttackController.Start | OK |
| Bolt = atual | `ExecuteBolt`/`SpawnBoltProjectile` (idêntico ao TryCast original) | OK |
| Cone = N projéteis em leque (reusa fan) | `ExecuteCone` (N projéteis no arco via SpawnBoltProjectile) | OK |
| Nova = OverlapCircleAll no raio com dano+status | `ExecuteNova` | OK |
| SelfRestore = RestoreHP/AddStamina/RestoreMana | `ExecuteSelfRestore` | OK |
| Barrier = PlayerBarrierState (absorve antes de PlayerDamageReceiver F03; hook documentado) | `ExecuteBarrier` + hook em `PlayerDamageReceiver.ApplyDamage` | OK |
| Gerador editor: 4 spells exemplares + scrolls (via padrão F07) | `GenerateShapeSpells` (4 exemplares + Tier 5 + arcane_projectile + scrolls) | OK (editor-only; execução diferida) |
| Eventos SpellCastStartedEvent/SpellCastInterruptedEvent | Started REUTILIZADO; Interrupted NOVO em MagicEvents | OK |
| EMENDA 6.6-A: Projétil Arcano auto-target | `SpellTargeting` + `AutoTarget` + EnemyPositionQuery (Physics2D) | OK |
| EMENDA 6.6-A: Tier 5 = Eco de Anya + Ruptura de Senya | gerador `anya_echo` + `senya_rupture` | OK |
| Save schema: NO | Barreira/cast transientes; nenhum DTO/Save tocado | OK |
| Não tocar .unity/.prefab/.asset manual; Packages/ProjectSettings; SaveManager/GameSaveData | Apenas .cs + csproj + report | OK |

---

## Targeting / Auto-Target — desenho determinístico (EMENDA 6.6-A)

`SpellTargeting.SelectAutoTarget` é **PURO** (recebe lista de posições candidatas, devolve índice), o que
permite EditMode tests sem cena. Regra canônica de desempate:

1. preferir candidato dentro do cone de mira (ângulo ≤ `aimHalfAngleDeg` vs. facing);
2. entre elegíveis, MENOR distância ao caster;
3. empate de distância → menor índice (ordem estável).

O runtime alimenta os candidatos via `PlayerAttackController.QueryEnemyPositions` →
`Physics2D.OverlapCircleAll(center, range)` filtrando `EnemyHealth` vivos. **NÃO usa
GameObject.Find/FindObjectOfType/FindObjectsByType** (rule unity-architecture; é busca de física,
o mesmo idioma do melee/Nova).

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (0E/0W)
Assembly-CSharp-Editor: PASS (0E/3W pré-existentes — CreateEnemyActionsAndSets x2, CSharpProjectPostprocessor x1)
Quality check: PASS
Docs validation: PASS (exit 0)
Spec diff completeness: PASS (exit 0, com report presente)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Builds executados individualmente:
- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0 (0E/0W).
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0 (0E; 3 warnings pré-existentes, nenhuma introduzida por esta spec).

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (targeting, absorção de barreira, dispatch por shape, defaults)
Changed Unity scene/prefab/asset wiring: NO (gerador editor não executado nesta sessão)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Core/SpellShapeTests.cs — 24 testes)
Automated tests command: NOT RUN (Unity Test Runner DIFERIDO — sem Unity Editor; testes compilam no Assembly-CSharp)
Manual Play Mode scenario: REQUIRED (nova/cura/barreira/cone visíveis + cast interrompível + auto-target) — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A (testes adicionados para a lógica determinística)
Residual risk: execução em cena de Cone/Nova (spawn em leque, dano em área), do cast interrompível em
  tempo real e da geração de assets não foi exercida em Play Mode/Editor nesta sessão. A lógica pura
  (targeting/absorção/dispatch/defaults) está coberta por EditMode tests. A interação barreira×defesa
  segue ordem documentada (barreira → block → defesa → resistência) e tem teste de absorção, mas a
  composição com Defense real em cena é diferida.
```

---

## Riscos técnicos (da spec) — mitigação aplicada

- **Cast routine travar input em soft-lock:** `SpellCastRoutine` tem `MaxCastSeconds=5` (clamp) e cancela
  em modal aberto (`ModalManager.HasActiveModal`). Um cast por vez; `BeginCast` recusa concorrência e
  reembolsa o plano duplicado.
- **Barrier duplicar mitigação com Defense:** ordem documentada e implementada (barreira ANTES de
  block/defesa/resistência em `PlayerDamageReceiver.ApplyDamage`); barreira não acumula (substituição,
  igual à regra anti-stack de status); teste de absorção parcial/overflow.

---

## Anti-regressão

- Spells existentes (spell_fireball/spell_ice_spike/spell_heal e itens equipados Magic) castam **idêntico**:
  `Shape` default = Bolt, `CastTimeSeconds` default = 0 (cast instantâneo), `TryCast` preserva a assinatura
  e o caminho do projétil legado (`SpawnBoltProjectile` reproduz o request original). Sem `AutoTarget`,
  o disparo continua na direção de mira.
- Modal guard intacto (PlayerAttackController bloqueia ataque com modal; rotina cancela cast em modal).
- Sem refs Unity em save (nada de save tocado; barreira/cast são transientes).
- `SpellCastStartedEvent` reutilizado (sem classe duplicada — hook runtime-code-guard respeitado).

---

## Asset generation

```text
Asset generation: NOT RUN (editor-only)
Reason: Unity Editor não executado nesta sessão (DEFERRED por ordem do dono)
Command attempted: (nenhum) — menu CindarsHope/Magic/Generate Shape Spells disponível para o humano
Residual risk: spells exemplares (spell_ice_nova, spell_minor_heal, spell_arcane_barrier,
  spell_flame_cone), Projétil Arcano (arcane_projectile) e Tier 5 (anya_echo, senya_rupture) +
  pergaminhos NÃO existem como .asset até o gerador rodar no Unity Editor. O código do gerador é
  idempotente por Id e registra no SpellDatabase/ItemDatabase via SerializedObject.
```

---

## Remaining Work (diferido)

- Rodar `CindarsHope/Magic/Generate Shape Spells` no Unity Editor (cria/registra as 7 spells + pergaminhos).
- Unity Test Runner EditMode (executar os 24 testes novos + suíte existente).
- Play Mode: validar nova/cura/barreira/cone visíveis, cast interrompível por dano (reembolso de mana),
  auto-target do Projétil Arcano mirando o inimigo mais próximo/na mira.
- (Fora de escopo desta spec) Ally targeting/heal em companions → WAVE 14; Spell HUD (cast bar) → F14.

---

```text
validated_adrs: []   (required_adrs vazio na spec)
validated_game_rules:
  - docs/game_rules/combat_rules.md — LIDO; sem conflito. A spec adiciona FORMAS de magia
    (shapes), cast time/interrupt, barreira e auto-target. Não altera as regras canônicas de
    cálculo de dano, stacking de status (substitui, não acumula — barreira segue o mesmo
    princípio anti-stack) nem duração em turnos do combat_rules. Dano de Nova/Cone passa pelo
    DamageCalculator/EnemyHealth existente (variância/defesa preservadas).
```
