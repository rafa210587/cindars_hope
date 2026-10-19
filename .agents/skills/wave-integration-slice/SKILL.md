---
name: wave-integration-slice
description: Protocolo para specs de WAVE_INTEGRATION — auditar sistemas existentes, decidir reuse vs. new, implementar, documentar e produzir checklist humano. Usar em specs WAVE_INTEGRATION de scene binding, runtime wiring, NPC, HUD, economy loop, skill effects ou movement actions.
---

# Skill: Slice de Wave Integration

## Quando usar

A spec é `WAVE_INTEGRATION_*` cobrindo:
- Scene runtime binding (HUD, inventory, equipment)
- Economy/shipping loop
- NPC dialogue/shop runtime
- Skill effects / active slot binding
- Player movement/ability runtime
- Resource interactable wiring

## Quando NÃO usar

- Spec WAVE pura (farm, cave, quest, calendar) sem scene integration.
- Spec docs-only ou asset-only.

---

## Phase 0 — Preflight

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
git status --short | Select-Object -First 20
git branch --show-current  # deve ser dev
```

---

## Phase 1 — Auditar sistemas existentes

Antes de escrever código, audite o que existe:

```
Sistema X já existe?              YES / NO / PARTIAL
Controllers já attached?          YES / NO
RuntimeInitializeOnLoad usado?    YES / NO
Modal guard existe?               YES / NO
Stamina/economy wired?            YES / NO
Bootstrap acessível?              YES / NO
Debt de wave anterior?            listar
```

**Regra reuse-first:** se o sistema existe e funciona, estenda-o. Não crie paralelo.

---

## Phase 2 — Decision

Record reuse/extend/new and the reason in the integration paragraph of the spec report.
Do not create a separate decision document; use the existing systems found in Phase 1.

---

## Phase 3 — Implementação

**Modal guard** (obrigatório para qualquer input de gameplay):
```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

**Dependências opcionais graciosas:**
```csharp
var bs = GameBootstrap.Instance;
if (bs != null && _staminaManager == null)
    _staminaManager = bs.StaminaManager;
if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost))
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent(LocalizationService.Get("action.x.no_stamina")));
    return;
}
```

Para bootstrap pattern de runtime binding → ver skill `bootstrap-wiring`.  
Para ability com movimento → ver skill `player-ability-runtime`.

---

## Phase 4 — Evidence

Use applicable tests plus one paragraph in `docs/validation/<spec_id>/REPORT.md` (or the
existing spec report), at most 60 lines total. Include strategy, wiring, action/expected
result, command/inputs/XML/exit and PASS/FAIL/NOT RUN. Reuse a wave checklist when relevant.
One folder per spec; overwrite reruns. Only approved-keyart scenes allow one comparative
capture. No asset snapshots/backups (cite commit hash), per-run folders or default index.html.
Docs/harness use the diff; pure logic/bugfix use EditMode + exit code in the commit.

---

## Phase 5 — Build Validation

Select tests/checks from `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`; reuse evidence for
unchanged inputs. Keep actual exit codes and FAIL/NOT RUN; never label a legacy failure PASS.
Preserve save/migration, IDs, secrets and data-integrity checks.

---

## Phase 6 — Atualizar CURRENT_STATE

```
WAVE_INTEGRATION_<N>: <STATUS> (<date>) — <resumo>; human must execute checklist
```

---

## Status Taxonomy

| Status | Significado |
|--------|-------------|
| `BUILD_VALIDATED` | Código compila, lógica completa |
| `BUILD_VALIDATED_WITH_UI_DEBT` | Núcleo conectado, UI/visual deferido |
| `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED` | Precisa de ação no Unity Editor |
| `BLOCKED` | Requer scene/Packages/escopo proibido |

---

## Quando parar e reportar

- Spec exige edição de scene YAML → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`, parar.
- Spec exige Packages/ ou ProjectSettings/ → `BLOCKED`.
- Auditoria reuse-first encontra sistema paralelo sendo construído → parar, reportar conflito.
- Mover o player exige reescrever `PlayerController` → parar, reportar.

## Regressões comuns

- Criar novo sistema quando o existente deveria ser estendido.
- Omitir o modal guard para ações dirigidas por input.
- Chamar `MovePosition` de coroutine sem `IsBeingDisplaced` (→ ver `player-ability-runtime`).
- Commitar arquivos de scene que o Unity auto-modificou.

## Relacionados

- `(skill: bootstrap-wiring)` — runtime bootstrap pattern
- `(skill: player-ability-runtime)` — abilities com movimento/displacement
- `(skill: input-gamepad-routing)` — modal guard e roteamento de input
- `(skill: non-regression-review)` — auditoria de regressão antes do closeout
