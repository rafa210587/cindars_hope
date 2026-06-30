# SPEC — Combate: Light/Heavy/Charged, Stagger e Integração de Atributos Derivados

> **Spec ID:** `fable_02_spec_combat_weapon_actions_derived_stats_runtime`
> **Status:** BUILD_VALIDATED (executada — E07; evidência: docs/validation/fable_02_spec_combat_weapon_actions_derived_stats_runtime_execution_report.md)
> **Wave:** FABLE — Gap Closure Bloco A (combate)
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Combat / Player
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_A
> **Can run with:** N/A (altera o caminho central de ataque)
> **Must not run with:** fable_01, fable_03, fable_06, fable_08, fable_14
> **Repo lock scope:** `PlayerAttackController.cs`, `DamageCalculator.cs`, `CooldownHelper.cs`, serviços de ataque
> **Depends on:**
> - `fable_01_spec_status_effects_canonical_set_runtime`
> - WAVE 05 (DerivedStatsCalculator), slice 2026-06-12 (serviços bow/spell)
> **Blocks:**
> - `fable_03`, `fable_05`
> **Scope:** ataques leve/pesado/carregado com stagger e dano/ASPD derivados de atributos.
> **Out of scope:** novas armas/assets, rebalance de inimigos, animações de arte.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

`COMBAT_CORE_DIRECTION.md` define ataques leve (tap), pesado (hold curto) e carregado
(hold longo) com custo de stamina crescente, posture/stagger em inimigos e guardbreak.
`PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md` define que dano e velocidade de ataque derivam de
atributos. O repo tem `DerivedStatsCalculator` (Attack, AttackSpeed, resistências — calculado
com equipment + passivas de skill) **que o combate ignora**: `PlayerAttackController.ExecuteMeleeAttack`
usa `weapon.BaseDamage` cru e `CooldownHelper.CalculateWeaponCooldown(weapon)` sem ASPD.
Posture/stagger não existem (apenas knockback).

## Problema

Atributos, equipamentos com bônus e passivas compradas na skill tree não mudam o dano real —
progressão de build é cosmética. Sem stagger, armas pesadas não têm identidade e o guardbreak
da direction (Investida Quebra-Guarda já existe como skill) não tem alvo mecânico.

## Objetivo

Ao final desta spec, Q/E devem suportar tap=light, hold=heavy, hold longo=charged (com
telegraph de carga), o dano final deve passar por `DamageCalculator` com `DerivedStats.Attack`
e o cooldown por `AttackSpeed`; inimigos devem ter `PostureState` com stagger ao quebrar,
sem alterar inputs existentes de interação (E continua interagindo quando há candidato).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- PlayerAttackController (Q/E, modal guard, unarmed fallback, dispatch bow/spell/melee)
- BowArrowAttackService / SpellCastService (slice 2026-06-12)
- DamageCalculator.Calculate(DamageRequest, defense)
- DerivedStatsCalculator (Attack/AttackSpeed/MaxHP/resistências com equipment+passivas)
- KnockbackController, EnemyHealth, HitFlashController, EnemyVulnerabilityState
- CooldownHelper
Não existe:
- input hold/charge (só GetKeyDown)
- PostureState/stagger em inimigos
- consumo de DerivedStats no caminho de dano/cooldown
Auditar na Fase 0:
- onde DerivedStatsCalculator é chamado hoje (PlayerManager? EquipmentManager?) e cache/invalidação
```

## Engineering stories

```text
Como jogador, quero que segurar Q/E carregue um golpe mais forte com custo maior de stamina.
Como build de força, quero que Attack derivado aumente o dano real de qualquer arma.
Como inimigo tanque, quero posture que só quebra com golpes pesados/carregados.
Como skill Investida Quebra-Guarda, quero causar dano de posture bônus.
```

## Escopo

```text
Inclui:
- ChargeTracker no PlayerAttackController: tap (<0.25s)=light, hold (0.25-0.9s)=heavy,
  hold (>=0.9s)=charged; liberar no KeyUp; telegraph visual simples (flash no sprite do player);
- multiplicadores: light 1.0x dmg/1.0x stamina; heavy 1.6x/1.8x; charged 2.4x/2.6x + posture 3x;
- PlayerCombatStatsProvider (NOVO): resolve DerivedStats correntes (cache + invalidação em
  EquipmentSlotChangedEvent/SkillNodePurchased) e expõe FinalDamage(baseDamage) e FinalCooldown(baseCooldown);
- EnemyPostureState (NOVO componente): MaxPosture por EnemyDataSO.baseDifficulty; dano de posture
  por golpe (peso do golpe); ao quebrar → estado Stunned no EnemyBrain por 1.2s + vulnerability window;
- integração: ExecuteMeleeAttack/serviços usam PlayerCombatStatsProvider; melee aplica posture damage;
- skill melee.investida_quebra_guarda passa a aplicar posture damage 3x (substituir knockback bônus);
- EditMode tests: thresholds de carga, multiplicadores, quebra de posture, cache de stats.
```

## Fora de escopo

```text
Não inclui:
- posture no player (block já mitiga via ação de movimento);
- novas armas/assets/ASPD por arma (F03);
- rebalance dos inimigos do roster;
- animações; UI de barra de posture (HUD futuro — publicar evento apenas).
```

## Regras de não duplicação

```text
Não criar segundo DamageCalculator nem segundo caminho de dano.
Não recriar DerivedStatsCalculator — consumir.
Não duplicar lógica de stamina (TrySpendStamina existente).
Posture é componente novo, não fork de EnemyHealth.
```

## Critérios de aceite

### CA-1 Três pesos de ataque
- Tap/hold/hold-longo geram light/heavy/charged com multiplicadores especificados.
- E continua interagindo quando há InteractionCandidate (sem regressão).
- Evidência: EditMode tests do ChargeTracker (puro) + cenário humano.

### CA-2 Atributos no dano real
- Com um equipamento de +Attack equipado, o dano logado no CombatLog aumenta de acordo.
- Com passiva de AttackSpeed, o cooldown efetivo diminui.
- Evidência: testes do PlayerCombatStatsProvider com stats sintéticos.

### CA-3 Posture/stagger
- Inimigo com posture cheia não stagger com light; quebra com 2 charged; ao quebrar entra
  Stunned e abre vulnerability window.
- Evidência: testes do EnemyPostureState + log `CombatLog: EnemyPostureBroken`.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/
  AttackChargeTracker.cs        (NOVO — puro, testável)
  PlayerCombatStatsProvider.cs  (NOVO)
  EnemyPostureState.cs          (NOVO)
  PlayerAttackController.cs     (integração)
Assets/_Game/Scripts/Core/Events/CombatPostureEvents.cs (NOVO: EnemyPostureBrokenEvent, PlayerChargedAttackEvent)
Assets/_Game/Tests/EditMode/Core/
  AttackChargeTrackerTests.cs / EnemyPostureStateTests.cs / PlayerCombatStatsProviderTests.cs
```

## Contratos

### Data contracts
N/A novo em SO; usa `EnemyDataSO.baseDifficulty` para MaxPosture (tabela interna documentada).

### Runtime contracts
`AttackChargeTracker`: `Begin(time)`, `Release(time) → AttackWeight {Light, Heavy, Charged}`.
`PlayerCombatStatsProvider`: `int FinalDamage(int)`, `float FinalCooldown(float)`, invalidação por eventos.
`EnemyPostureState`: `ApplyPostureDamage(float)`, `IsBroken`, auto-recover.

### Event contracts
`EnemyPostureBrokenEvent(enemyId)`, `PlayerChargedAttackEvent(weight)` — novos.

### Save contracts
N/A (posture/charge transientes).

### UI contracts
N/A (eventos publicados para HUD futuro).

## Sistemas afetados

```text
Combat melee/ranged/spell (dano final), Skills (quebra-guarda), Enemy AI (Stunned), Equipment/SkillTree (consumo de stats), Event bus
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/** (arquivos listados na arquitetura)
Assets/_Game/Scripts/Skills/Runtime/Effects/MeleeStrikeSkillEffectExecutor.cs (posture bônus)
Assets/_Game/Scripts/Core/Events/CombatPostureEvents.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (AddComponent EnemyPostureState)
Assets/_Game/Tests/EditMode/Core/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
Assets/**/*.unity, *.prefab, *.asset
Packages/**, ProjectSettings/**
SaveManager/GameSaveData; EnemyHealth.cs (sem fork de dano)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria: chamadas atuais de DerivedStatsCalculator; inputs Q/E; baseline de logs.
### Fase 1 — AttackChargeTracker + PlayerCombatStatsProvider (puros + testes).
### Fase 2 — Integração no PlayerAttackController e serviços (dano/cooldown/stamina por peso).
### Fase 3 — EnemyPostureState + materializer + EnemyBrain.Stunned + eventos.
### Fase 4 — Skill quebra-guarda; testes; run_strict_validation; report.
```

## Paralelização

- Parallelizable: NO
- Reason: trava o caminho central de ataque consumido por F03/F05/F06/F08.

## Impacto em save/load

```text
Does this change save schema? NO — posture e charge são transientes.
```

## Impacto em eventos

```text
Adds events: YES (2) | Changes existing: NO | Requires unsubscribe: YES (provider)
```

## Impacto em UI/Unity

```text
Changes UI: NO | Changes scenes: NO | Changes prefabs: NO | Changes assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: hold de E conflitar com interação. Mitigação: charge só inicia se não houver
InteractionCandidate no KeyDown; teste de regressão do gate de interação.
Risco: cache de stats stale após respec. Mitigação: invalidar também em SkillTreeResetEvent (auditar nome real).
```

## Rollback

```text
Remover componentes novos; PlayerAttackController volta a GetKeyDown + BaseDamage; eventos novos sem consumidores podem permanecer.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar consumo atual de DerivedStatsCalculator e eventos de invalidação.
- [ ] T002 — AttackChargeTracker puro + testes de threshold.
- [ ] T003 — PlayerCombatStatsProvider + testes (equipment/passiva sintéticos).
- [ ] T004 — Integrar peso de ataque em melee (dano/stamina/cooldown).
- [ ] T005 — Integrar FinalDamage/FinalCooldown em bow/spell services.
- [ ] T006 — EnemyPostureState + AddComponent no materializer + Stunned no brain.
- [ ] T007 — Posture damage por peso + quebra-guarda 3x + eventos.
- [ ] T008 — Testes EditMode; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (feel de combate)
- Requires regression test: YES (gate de interação do E; dano unarmed fallback)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com 3 pesos visíveis e stagger

## Definition of Done

```text
3 pesos funcionais; stats derivados no dano/cooldown; posture com stagger; zero regressão
em interação/modal guard; builds 0E; report criado.
```

## Anti-regressão

```text
Modal aberto continua bloqueando ataque.
E com InteractionCandidate continua interagindo.
Unarmed fallback continua funcionando.
Não serializar referência Unity; não usar GameObject.Find.
```

## Notas para execução posterior

```text
F03 adiciona ASPD/scaling por arma em cima do provider criado aqui.
HUD de posture/charge fica para F14/Canvas.
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. CRÍTICO: existe chance normal de crítico (canon) — skills/armas somam CritChance;
   CriticalWindow GARANTE crítico; CoreExposed garante + bônus. Substituir a regra
   "crit só em janela" desta spec pela canônica. Multiplicador base 1.5×.
2. MULTIPLICADORES por peso (canônicos, EQUIPMENT_MECHANICAL_BASELINES §6):
   light ×1.00/posture ×1.00 · heavy ×1.45/×1.60 · charged curto ×1.65/×1.80 ·
   charged longo ×1.90/×2.20 (substituem 1.6/2.4 propostos).
3. CUSTOS de stamina POR ARMA da matriz canônica (ex.: sword light 25/heavy 40/charged 48;
   dagger 16/28/36; hammer 36/58/70) — o ChargeTracker lê da arma, não de globais.
4. Thresholds de hold: tap <0.25s = light · 0.25-0.9s = heavy · >=0.9s = charged (mantidos).
```
