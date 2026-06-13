# SPEC — Magia: Spell Shapes, Targeting, Cast Time e Suporte (Heal/Barrier)

> **Spec ID:** `fable_08_spec_magic_spell_shapes_targeting_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco C (magia)
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** Combat
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_C
> **Can run with:** N/A
> **Must not run with:** fable_01, fable_02, fable_07 (dependências diretas)
> **Repo lock scope:** `SpellDataSO`, `SpellCastService`, `RuntimeProjectileFactory`
> **Depends on:**
> - `fable_01` (status), `fable_07` (spellbook)
> **Blocks:** N/A
> **Scope:** formas de spell (bolt/cone/nova/self/barrier/heal) com cast time e interrupt.
> **Out of scope:** novas escolas completas, VFX de arte, spell HUD Canvas (F14), capstones.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

`MAGIC_SPELLS_ACTIONS_DIRECTION.md` define que spells têm SpellShape (projétil, cone, nova,
self, barreira, cura), cast time interrompível, MP cost e scaling. Hoje toda spell é um
projétil único linear instantâneo (`SpellCastService` → `ProjectileSpawnService`); cura/
barreira/nova não têm caminho de execução; cast é instantâneo (sem janela de interrupt).

## Problema

Sem shapes, o catálogo de magia colapsa em "flecha colorida": impossível autorar a cura da
direction, a nova de gelo defensiva ou a barreira — e o ManaManager existente fica
subutilizado. F07 entrega o aprendizado; sem esta spec, aprender não diversifica nada.

## Objetivo

Ao final desta spec, `SpellDataSO` deve ter `SpellShape` + `CastTimeSeconds` e o
`SpellCastService` deve executar 6 shapes (Bolt atual, Cone, Nova, SelfBuff/Heal, Barrier,
HealProjectile-ally-future-stub) com cast time cancelável por dano — preservando todas as
spells existentes como Bolt default.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- SpellDataSO (BaseDamage, ManaCost, Range, ProjectileSpeed, CooldownSeconds, StatusEffectId/Chance)
- SpellCastService (cooldown, mana, projétil) — F07 adiciona CanCast
- RuntimeProjectileFactory/ProjectileSpawnService (suporta fan/pierce via request)
- PlayerManager.RestoreHP; StatusEffectManager (F01: status no player)
- PlayerDamagedEvent (gatilho de interrupt)
Não existe:
- SpellShape, cast time, cone/nova/self/barrier
```

## Engineering stories

```text
Como mago, quero uma nova de gelo (360°) que aplica Chill nos inimigos próximos.
Como sobrevivente, quero cura com 1.2s de cast que cancela se eu tomar dano.
Como tanque arcano, quero barreira temporária que absorve N de dano.
Como autor de conteúdo, quero shapes data-driven para criar spells sem código novo.
```

## Escopo

```text
Inclui:
- SpellShape enum {Bolt, Cone, Nova, SelfRestore, Barrier} + CastTimeSeconds + ConeAngle/
  NovaRadius/RestoreHp/BarrierAbsorb/BarrierSeconds em SpellDataSO (defaults: Bolt, 0s — assets atuais intactos);
- SpellCastRoutine (NOVO MonoBehaviour fino no player, criado pelo PlayerAttackController):
  gerencia cast time com telegraph (flash) e cancela em PlayerDamagedEvent;
- executores por shape no SpellCastService:
  Bolt = atual; Cone = N projéteis em leque (reusa fan do request) OU OverlapCircle+filtro
  angular para curto alcance; Nova = OverlapCircleAll no raio com dano+status; SelfRestore =
  RestoreHP/AddStamina/RestoreMana; Barrier = PlayerBarrierState (NOVO: absorve antes de
  PlayerDamageReceiver de F03 — integração com hook documentado);
- gerador editor: 4 spells exemplares (spell_ice_nova, spell_minor_heal, spell_arcane_barrier,
  spell_flame_cone) no SpellDatabase + itens LearnableScroll correspondentes (via gerador F07);
- EditMode tests: shape dispatch, cancelamento de cast, barreira absorvendo, defaults Bolt.
```

## Fora de escopo

```text
Não inclui: targeting de mouse/área livre; spell HUD; ally targeting (companions futuros);
scaling por atributo além do FinalDamage de F02; interrupt por movimento (só por dano).
```

## Regras de não duplicação

```text
Não criar segundo serviço de cast — estender SpellCastService.
Não criar segundo caminho de dano em área — reutilizar OverlapCircleAll padrão do melee.
Barrier integra com PlayerDamageReceiver (F03), não cria receptor paralelo.
```

## Critérios de aceite

### CA-1 Shapes funcionais
- 5 shapes executam com semântica distinta; spells existentes (sem campos novos) castam como Bolt.
- Evidência: testes de dispatch por shape + caracterização do Bolt.

### CA-2 Cast time interrompível
- Spell com CastTimeSeconds>0 só resolve após o tempo; dano durante o cast cancela sem gastar
  mana (reembolso) e loga `CombatLog: SpellCastInterrupted`.
- Evidência: teste do SpellCastRoutine (simulação de PlayerDamagedEvent).

### CA-3 Barreira real
- Barrier absorve até BarrierAbsorb de dano por BarrierSeconds; consumo logado; expira limpa.
- Evidência: testes do PlayerBarrierState.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs        (campos aditivos)
Assets/_Game/Scripts/Combat/Magic/SpellShape.cs          (NOVO)
Assets/_Game/Scripts/Combat/Magic/SpellCastRoutine.cs    (NOVO)
Assets/_Game/Scripts/Combat/Magic/PlayerBarrierState.cs  (NOVO)
Assets/_Game/Scripts/Combat/SpellCastService.cs          (dispatch por shape)
Assets/_Game/Scripts/Editor/Magic/GenerateShapeSpells.cs (NOVO)
Assets/_Game/Tests/EditMode/Core/SpellShapeTests.cs
```

## Contratos

### Data contracts — campos aditivos com defaults neutros (Bolt/0).
### Runtime contracts — `SpellCastService.TryCast` mantém assinatura; dispatch interno.
### Event contracts — `SpellCastStartedEvent/SpellCastInterruptedEvent` (novos).
### Save contracts — N/A (barreira/cast transientes).
### UI contracts — N/A.

## Sistemas afetados

```text
Magic cast, Player damage path (barrier), Status application (nova/cone), Editor tooling
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/MagicEvents.cs
Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs (hook de barreira)
Assets/_Game/Tests/EditMode/Core/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab; *.asset manual; Packages/ProjectSettings; SaveManager/GameSaveData
```

## Estratégia de implementação

```md
### Fase 0 — Caracterizar Bolt atual (testes) e auditar PlayerDamageReceiver (F03).
### Fase 1 — Campos/enum + dispatch (Bolt intacto).
### Fase 2 — Nova/Cone/SelfRestore + status (F01).
### Fase 3 — SpellCastRoutine (cast/interrupt/reembolso) + Barrier.
### Fase 4 — Gerador + testes + validação estrita + report.
```

## Paralelização

- Parallelizable: NO
- Reason: depende e trava SpellCastService/receiver compartilhados com F03/F07.

## Impacto em save/load

```text
Does this change save schema? NO
```

## Impacto em eventos

```text
Adds events: YES (2) | Changes existing: NO | Unsubscribe: YES (routine)
```

## Impacto em UI/Unity

```text
Changes assets: YES (gerador) | Scenes/prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: cast routine travar input em soft-lock. Mitigação: timeout máximo + cancel em modal.
Risco: barrier duplicar mitigação com Defense. Mitigação: ordem documentada (barrier antes de defense) + teste.
```

## Rollback

```text
Remover arquivos novos; campos aditivos neutros mantêm spells atuais como Bolt instantâneo.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Caracterização do Bolt + auditoria do receiver.
- [ ] T002 — SpellShape/campos + dispatch.
- [ ] T003 — Nova/Cone/SelfRestore com status F01.
- [ ] T004 — SpellCastRoutine (cast time, interrupt, reembolso).
- [ ] T005 — PlayerBarrierState + hook no receiver.
- [ ] T006 — GenerateShapeSpells + scrolls F07.
- [ ] T007 — Testes; csproj; run_strict_validation; report.
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
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (Bolt caracterizado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com nova/cura/barreira visíveis

## Definition of Done

```text
5 shapes + cast interrompível + barreira; spells antigas intactas; builds 0E; report.
```

## Anti-regressão

```text
Spells existentes castam idêntico (Bolt default). Modal guard intacto. Sem refs Unity em save.
```

## Notas para execução posterior

```text
Ally targeting/heal em companions: promoção WAVE 14.
Spell HUD (cast bar): F14.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. TIER 5 do v1 (6.6-A): Eco de Anya + Ruptura de Senya (Véu de Nyx/Martelo de Thoren pós-v1).
2. Projétil Arcano: AUTO-TARGET (6.6-A).
```
