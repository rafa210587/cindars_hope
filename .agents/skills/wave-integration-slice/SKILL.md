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

## Phase 2 — Doc de decisão

```
docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_DECISION.md
```

```markdown
## Strategy
REUSE_EXISTING_<SYSTEM> | NEW_SYSTEM | EXTEND_EXISTING

## Rationale
<por quê>

## Sistemas reutilizados
- <sistema>: <o que é reutilizado>

## Deferidos
- <debt tag>: <motivo>
```

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

## Phase 4 — Docs (obrigatórios)

| Doc | Path |
|-----|------|
| Report | `docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_REPORT.md` |
| Human checklist | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_PLAYMODE_CHECKLIST.md` |
| Wiring instructions (se Unity action necessária) | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_UNITY_<SLUG>_WIRING_INSTRUCTIONS.md` |

**Report mínimo:**
```markdown
## Status
BUILD_VALIDATED_WITH_UI_DEBT | BUILD_VALIDATED | BLOCKED

## Strategy applied
REUSE_EXISTING_X

## Assembly-CSharp: PASS | FAIL
## Assembly-CSharp-Editor: PASS | FAIL
## Docs validation: PASS | EXPECTED_FAIL_LEGACY_ONLY

## Human Play Mode needed: YES | NO
## Can continue to next WAVE_INTEGRATION: YES | NO
```

**Human checklist mínimo:**
```markdown
| Step | Expected | Pass/Fail |
|------|----------|-----------|
| Open scene, Press Play | No red errors | |
| <ação específica da spec> | <resultado esperado> | |
| Stop Play | Scene not corrupted | |
```

---

## Phase 5 — Build Validation

```powershell
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { Write-Host "VALIDATION FAILED"; exit 1 }
```

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
