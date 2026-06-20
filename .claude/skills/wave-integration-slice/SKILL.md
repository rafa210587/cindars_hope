---
name: wave-integration-slice
description: Protocolo para specs de WAVE_INTEGRATION — auditar sistemas existentes, decidir reuse vs. new, implementar, documentar e produzir checklist humano. Use em qualquer spec WAVE_INTEGRATION (scene integration, runtime binding, NPC wiring, HUD binding, economy loop, skill effects, movement actions).
---

# Skill: Slice de Wave Integration

## Quando usar

A tarefa é uma spec `WAVE_INTEGRATION_*` cobrindo:
- Scene runtime binding (HUD, inventory, equipment)
- Wiring de loop de economy/shipping
- NPC dialogue/shop runtime
- Skill effects / active slot binding
- Player movement/ability runtime
- Wiring de resource interactable
- Qualquer slice de "fazer o código realmente rodar numa scene"

## Quando NÃO usar

- A spec é uma WAVE pura (farm, cave, quest, calendar) sem scene integration
- A spec é docs-only ou asset-only

---

## Phase 0 — Preflight

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
git status --short | Select-Object -First 20
git branch --show-current   # must be dev
```

Confirme:
- Branch = `dev`
- Working tree limpa (ou só arquivos esperados)

---

## Phase 1 — Auditar sistemas existentes

Antes de escrever uma única linha de código, audite o que já existe.

Leia:
1. A spec
2. `docs/project/CURRENT_STATE.md` (seção WAVE_INTEGRATION)
3. O relatório de WAVE_INTEGRATION imediatamente anterior (se listado como dependência)

Crie o doc de auditoria:
```
docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_RUNTIME_AUDIT.md (opcional se simples)
```

Responda estas perguntas na auditoria:
```
System X already exists?        YES / NO / PARTIAL
Controllers already attached?   YES / NO
RuntimeInitializeOnLoad used?   YES / NO
Modal guard exists?             YES / NO
Stamina/economy wired?          YES / NO
Bootstrap accessible?           YES / NO
Scene creator updated?          YES / NO
Debt from prior wave?           list it
```

**Regra reuse-first**: Se o sistema já existe e funciona, estenda-o. NÃO crie um sistema paralelo.

---

## Phase 2 — Doc de decisão

Crie:
```
docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_DECISION.md
```

Estrutura:
```markdown
# WAVE_INTEGRATION_<N> — <Title> Decision

## Strategy
REUSE_EXISTING_<SYSTEM> / NEW_SYSTEM / EXTEND_EXISTING

## Rationale
<why this strategy>

## Reused systems
- <system>: <what is reused>

## Deferred
- <debt tag>: <reason deferred>

## Files in scope
- <list>
```

---

## Phase 3 — Implementação

### Padrão de Runtime Bootstrap (ao anexar ao player/scene em runtime)

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void EnsureInstance()
{
    if (_instance != null) return;
    var go = new GameObject("XRuntimeBootstrap");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<XRuntimeBootstrap>();
}
```

Refaça o bind no scene load:
```csharp
private void OnEnable() => SceneManager.sceneLoaded += HandleSceneLoaded;
private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;
```

### Modal Guard (sempre exigido para runtime dirigido por input)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

### Dependências opcionais graciosas

```csharp
// Stamina — optional
var bootstrap = GameBootstrap.Instance;
if (bootstrap != null && _staminaManager == null)
    _staminaManager = bootstrap.StaminaManager;

// Use only if present
if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost))
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente."));
    return;
}
```

### Debt Tags (use em comentários de código e relatórios)

```
TODO_INTEGRATION_NOT_FINAL
ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_NN
HUD_FEEDBACK_CONSUMER_DEBT
BALANCE_FINAL_PENDING
BLOCK_DAMAGE_REDUCTION_DEFERRED_TO_COMBAT_RUNTIME
```

---

## Phase 4 — Docs

### Docs obrigatórios (sempre)

| Doc | Path |
|-----|------|
| Report | `docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_REPORT.md` |
| Human checklist | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_PLAYMODE_CHECKLIST.md` |

### Docs opcionais (quando wiring no Unity é necessário)

| Doc | Path |
|-----|------|
| Wiring instructions | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_UNITY_<SLUG>_WIRING_INSTRUCTIONS.md` |

### Estrutura do report

```markdown
# WAVE_INTEGRATION_<N> — <Title> Report

## Status
BUILD_VALIDATED_WITH_UI_DEBT / BUILD_VALIDATED / BLOCKED

## Root cause / gap addressed
<what was missing>

## Strategy applied
REUSE_EXISTING_X

## Files created
- path

## Files changed
- path

## Debts
- DEBT_TAG: explanation

## Assembly-CSharp: PASS / FAIL
## Assembly-CSharp-Editor: PASS / FAIL
## Docs validation: PASS / EXPECTED_FAIL_LEGACY_ONLY

## Human Play Mode needed: YES
## Can continue to next WAVE_INTEGRATION: YES / NO
```

### Estrutura do human checklist

```markdown
| Step | Expected result | Pass/Fail | Notes |
|------|----------------|-----------|-------|
| Open Scene | No red errors | | |
| Press Play | System initializes | | |
| ... | ... | | |
| Stop Play | Scene not corrupted | | |
```

---

## Phase 5 — Build Validation

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }

dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

Esperado: 0 erros. Warnings de editor preexistentes são aceitáveis.

---

## Phase 6 — Atualizar CURRENT_STATE

Atualize a seção WAVE_INTEGRATION em `docs/project/CURRENT_STATE.md`:

```
- WAVE_INTEGRATION_<N>: <STATUS> (<date>) — <one-line summary>; human must execute checklist before ACCEPTED/WAVE_INTEGRATION_<N+1> continuation
```

---

## Status Taxonomy para trabalho de integração

| Status | Significado |
|--------|-------------|
| `BUILD_VALIDATED` | Código compila, lógica completa, sem debt deferido |
| `BUILD_VALIDATED_WITH_UI_DEBT` | Núcleo conectado, UI/visual/PlayMode deferido |
| `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE` | Código completo, humano ainda não testou |
| `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED` | Precisa de ação de scene no Unity Editor |
| `BLOCKED` | Não pode continuar sem scene/Packages/escopo proibido |

---

## Regressões comuns

- Criar um novo sistema quando um existente deveria ser estendido
- Não checar o modal guard para ações dirigidas por input
- Chamar `MovePosition` de coroutine sem a flag `IsBeingDisplaced` (ver `player-ability-runtime`)
- Commitar arquivos de scene que o Unity auto-modificou
- Faltar wiring instructions quando ação humana no Unity é necessária

## Quando parar e reportar

- Spec exige edição de scene YAML → documente como `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`, pare
- Spec exige Packages/ ou ProjectSettings/ → `BLOCKED`
- Auditoria reuse-first encontra sistema paralelo já sendo construído → pare e reporte o conflito
