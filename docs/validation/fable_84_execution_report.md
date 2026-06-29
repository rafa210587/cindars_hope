# Execution Report — fable_84_spec_player_attack_anim_archetype

> **Spec ID:** `fable_84_spec_player_attack_anim_archetype`
> **Status:** BUILD_VALIDATED
> **Date:** 2026-06-29
> **Executor:** Claude Sonnet 4.6 (spec-implementer)

---

## Fase 0 — Auditoria (pré-edição)

### Grep de IsSword e PlayerMeleeSwingEvent

Consumidores/producers encontrados ANTES da implementação:

| Arquivo | Linha | Contexto |
|---|---|---|
| `Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs` | 12, 23, 25, 29 | Definição do campo `IsSword` e construtor |
| `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs` | 115, 117 | Publisher: `weapon.Type == WeaponType.Sword` |
| `Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs` | 11, 105, 111, 128, 130, 132 | Subscribe/Unsubscribe + `OnMeleeSwing` + `if (!evt.IsSword) return` |
| `Assets/_Game/Scripts/Core/Events/PlayerBowShootEvent.cs` | 9 | Apenas comentário doc mencionando PlayerMeleeSwingEvent (não usa IsSword) |

**Total de arquivos com IsSword:** 3 (PlayerMeleeSwingEvent.cs, PlayerAttackController.Attacks.cs, PlayerWalkAnimator.cs). Nenhum consumer adicional fora do esperado pela spec.

### Estado das pastas de sprites

| Pasta | Estado |
|---|---|
| `Resources/PlayerSprites/attack/` | EXISTE — 8 subpastas de direção (right, upright, up, upleft, left, downleft, down, downright) |
| `Resources/PlayerSprites/attack_sword/` | NÃO EXISTIA (correto) |
| `Resources/PlayerSprites/attack_heavy/` | JÁ EXISTIA com 40 arquivos (arte preexistente — nenhuma ação necessária) |
| `Resources/PlayerSprites/attack_thrust/` | NÃO EXISTE |
| `Resources/PlayerSprites/attack_dagger/` | NÃO EXISTE |
| `Resources/PlayerSprites/attack_cast/` | NÃO EXISTE |
| `Resources/PlayerSprites/bow/` | EXISTE (não alterado) |

### Enum PlayerAttackAnimArchetype

Resultado: `No matches found` — confirmado que não existia antes da implementação.

---

## Arquivos criados

| Arquivo | Ação |
|---|---|
| `Assets/_Game/Scripts/Core/Events/PlayerAttackAnimArchetype.cs` | CRIADO — enum `PlayerAttackAnimArchetype { Sword=0, Bow=1, Heavy=2, Thrust=3, Dagger=4, Cast=5 }`, namespace `CindarsHope.Core.Events`, sem `using CindarsHope.Combat` |
| `Assets/_Game/Scripts/Combat/WeaponAttackArchetypeMapper.cs` | CRIADO — mapper `WeaponType → PlayerAttackAnimArchetype`, mapa completo de 10 valores, default fallback Sword + LogWarning |
| `Assets/_Game/Scripts/Editor/RenameAttackSpriteFolder.cs` | CRIADO — script Editor one-shot `AssetDatabase.MoveAsset` (attack/ → attack_sword/), idempotente; EXECUÇÃO DEFERIDA ao Unity |
| `Assets/_Game/Tests/EditMode/Combat/WeaponAttackArchetypeMapperTests.cs` | CRIADO — 10 testes cobrindo todos os WeaponType values |

## Arquivos alterados

| Arquivo | Mudança |
|---|---|
| `Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs` | Removido `bool IsSword`; adicionado `PlayerAttackAnimArchetype Archetype`; construtor atualizado; doc atualizado |
| `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs` | Linha ~117: `weapon.Type == WeaponType.Sword` → `WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type)` |
| `Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs` | `_attackFrames[][]` → `_attackFramesByArchetype[][][]`; `LoadAllFrames` carrega 6 arquétipos × 8 direções; `OnMeleeSwing` despacha por arquétipo com fallback Sword; adicionado `ArchetypeFolders[]` estático; adicionado `IsSetEmpty()` |
| `Assembly-CSharp.csproj` | Adicionado include: `PlayerAttackAnimArchetype.cs`, `WeaponAttackArchetypeMapper.cs` |
| `Assembly-CSharp-Editor.csproj` | Adicionado include: `RenameAttackSpriteFolder.cs`, `WeaponAttackArchetypeMapperTests.cs` |

---

## Spec Compliance Matrix

| Critério (spec §14) | Implementado | Evidência |
|---|---|---|
| 14.1 Enum no Core, sem using Combat | SIM | `PlayerAttackAnimArchetype.cs` namespace `CindarsHope.Core.Events`, sem using Combat |
| 14.2 PlayerMeleeSwingEvent sem IsSword, Archetype presente | SIM | grep IsSword retorna zero |
| 14.3 WeaponAttackArchetypeMapper cobre 10 WeaponType values | SIM | switch completo + default fallback |
| 14.4 Publisher usa mapper | SIM | `WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type)` na linha ~117 |
| 14.5 PlayerWalkAnimator despacha por arquétipo com fallback | SIM | `OnMeleeSwing` usa `_attackFramesByArchetype`, fallback Sword via `IsSetEmpty` |
| 14.6 Pasta renomeada | DEFERIDO — ver abaixo | Script Editor criado; rename exige Unity aberto |
| 14.7 EditMode tests 10 casos | SIM | `WeaponAttackArchetypeMapperTests.cs` — 10 testes |
| 14.8 Build exit 0 | SIM | Assembly-CSharp: 0E/1W preexistente; Editor: 0E/3W preexistentes |

---

## Validação

```text
Validation method: dotnet build --no-restore (após dotnet restore)
Assembly-CSharp exit code: 0 (0 erros, 1 warning preexistente em CombatTelemetrySession)
Assembly-CSharp-Editor exit code: 0 (0 erros, 3 warnings preexistentes)
run_strict_validation.ps1: CORRUPTION_DETECTED — falha no corruption guard por 20 arquivos .meta
  de PlayerSprites/idle/down e idle/up que estavam DELETADOS ANTES desta spec
  (visíveis no git status inicial como "D" — não causados por fable_84)
grep IsSword em Assets/_Game/Scripts: 0 resultados (PASS)
```

---

## Item DEFERIDO — Rename de pasta `attack/` → `attack_sword/`

**Motivo:** Renomear via `AssetDatabase.MoveAsset` exige o Unity Editor aberto. Fazer via File System direto quebraria GUIDs dos `.meta` dos sprites de espada, quebrando a animação de espada em Play Mode.

**Artefato criado:** `Assets/_Game/Scripts/Editor/RenameAttackSpriteFolder.cs` — método `RenameAttackSpriteFolder.Execute()`, idempotente (não faz nada se `attack_sword/` já existir ou `attack/` não existir).

**Ação humana necessária:** Com o Unity Editor aberto, chamar `RenameAttackSpriteFolder.Execute()` via console do Unity OU mover manualmente a pasta `attack/` → `attack_sword/` via Project window do Unity.

**Impacto enquanto deferido:** O `PlayerWalkAnimator` vai tentar carregar `PlayerSprites/attack_sword/{dir}` e não vai encontrar sprites (pasta ainda chama `attack/`). O fallback Sword vai ser acionado mas também não vai encontrar frames (porque a pasta original é `attack/`). Resultado: animação de espada não toca até o rename ser executado. Arco e walk/idle não são afetados.

**Residual risk:** Animação de espada em Play Mode fica silenciosa (sem crash) até o rename ser feito no Unity.

---

## Testing Quality Gate

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (mapeamento WeaponType → arquétipo)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — WeaponAttackArchetypeMapperTests.cs (10 casos)
Automated tests command:        dotnet build Assembly-CSharp-Editor.csproj --no-restore (exit 0)
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (ver §25 da spec)
Justification if no tests:      N/A — testes criados
Residual risk:
  1. Pasta attack/ ainda não renomeada — animação de espada silenciosa até ação humana no Unity
  2. run_strict_validation.ps1 falha no corruption guard por arquivos .meta preexistentes deletados
     (idle/down, idle/up) — NÃO causado por esta spec
  3. Play Mode human validation deferida para batch final de onda
```

---

## Status

```text
BUILD_VALIDATED
Phase 1 (código C#): PASS — Assembly-CSharp exit 0, Assembly-CSharp-Editor exit 0
Phase 2 (Unity/Play Mode): DEFERRED_TO_FINAL_VALIDATION
Rename de pasta: DEFERRED — requer Unity Editor aberto (script Editor criado)
Spec pode ser promovida para implementados/: NÃO — aguarda Play Mode human validation
```

---

## Remaining Work

1. Humano executa rename no Unity Editor (`RenameAttackSpriteFolder.Execute()` ou via Project window)
2. Play Mode: equipar espada → confirmar animação de espada (após rename)
3. Play Mode: equipar Axe → confirmar fallback Sword sem crash
4. Play Mode: equipar Staff → confirmar fallback Sword sem crash
5. Play Mode: equipar arco → atirar → confirmar animação de arco intacta
6. Play Mode: walk/idle em 4 direções → confirmar inalterados
