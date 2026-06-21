---
name: observability-and-logging
description: Convenções de logging diagnóstico e observabilidade em runtime — one-shot guards para logs repetidos, formato canônico de wiring-error (scene/object/component/campo/id), prefixos por sistema e um debug overlay opcional. Use ao adicionar log de erro/aviso em runtime, ao investigar log spam, ou em qualquer sistema *RuntimeBootstrap que se auto-conecta sem visibilidade.
---

# Skill: Observabilidade e Logging

O projeto tem ~14 `*RuntimeBootstrap` singletons que se auto-conectam via `RuntimeInitializeOnLoadMethod` e ~70 sistemas falando por `GameEventBus` — mas **nenhuma convenção de logging** além da rule `error-handling-resilience` ("logs precisam carregar contexto"). O resultado é log spam (ex.: `AudioClip.Create` falhando por frame antes do guard `s_clipCreateFailureLogged` em `ProceduralSfxFactory`) e wiring que falha em silêncio. Esta skill é o padrão de *como logar* e *como enxergar* o runtime.

## Quando usar

- Ao adicionar qualquer `Debug.LogWarning`/`LogError` em runtime code (`Assets/_Game/Scripts/**`, exceto `Editor/`).
- Ao investigar **log spam** (a mesma linha repetindo por frame ou por evento).
- Ao logar um **wiring-error** (dependência obrigatória ausente, manager não conectado).
- Ao adicionar um sistema `*RuntimeBootstrap` que se auto-conecta e precisa provar que conectou.
- Quando o humano pergunta "o que está rodando / por que isso não acontece" e não há como ver.

## Por que existe

Log sem contexto não permite localizar a falha sem debugger; log repetido por frame afoga o Console e esconde o resto; wiring silencioso (categoria 4 da rule `error-handling-resilience`) vira NRE três frames depois. A invariante: **um log por causa, com contexto suficiente para agir, e no máximo uma vez quando a causa é estável.**

## Sistemas existentes (reusar, não duplicar)

| Referência | Papel |
|---|---|
| `ProceduralSfxFactory` (`s_clipCreateFailureLogged`) | Precedente canônico de **one-shot estático** — log de falha de backend 1× no processo |
| `AudioManager` (`_missingClipLogged`) | Precedente de **one-shot por instância** (singleton DontDestroyOnLoad) |
| `GameBootstrap` | Ponto onde managers são injetados — wiring-error nasce aqui quando uma ref obrigatória é null |
| rule `error-handling-resilience` | Taxonomy de 4 categorias de falha — define quando logar vs. throw vs. fallback |
| rule `unity-architecture` | Wiring-error nunca pode ser mascarado por scene search silencioso |

## Procedimento

### 1. Escolher o guard de repetição certo

| Situação | Guard | Exemplo |
|---|---|---|
| Causa estável no processo inteiro (backend indisponível, asset global ausente) | `private static bool s_xLogged;` | `ProceduralSfxFactory.s_clipCreateFailureLogged` |
| Causa por instância de singleton | `private bool _xLogged;` (nunca volta a false) | `AudioManager._missingClipLogged` |
| Causa por id/chave distinta (quer 1× por id, nunca repetir) | `HashSet<string> _loggedIds` | "item X sem icon" 1× por item |
| Erro genuíno por frame que NÃO deve ser engolido | **não** silencie — é bug (categoria 4) → throw/assert em dev |

Regra: o guard reduz **ruído de causa estável**, não esconde bug novo. Nunca reseta um guard one-shot só para "ver de novo".

### 2. Formato canônico de wiring-error

Uma linha precisa permitir localizar a falha sem debugger. Inclua: **sistema, scene, GameObject, component, campo ausente, id afetado, fallback aplicado**.

```csharp
Debug.LogError(
  $"[Wiring] {nameof(MeuManager)}: dependencia obrigatoria '{nameof(_database)}' nula " +
  $"(scene='{gameObject.scene.name}', go='{name}'). Sistema desabilitado ate o wiring. " +
  $"Corrija no GameBootstrap ou no scene creator.");
```

Proibido: `Debug.LogError("Error loading data")` (sem sistema, sem contexto, sem ação).

### 3. Prefixo por sistema

Todo log de runtime começa com `[<Sistema>]` (`[Audio]`, `[Wiring]`, `[Quest]`, `[Save]`, `[Cave]`). Permite filtrar o Console por sistema e medir o ruído de cada um.

### 4. Debug overlay opcional (enxergar em runtime)

Para sistemas críticos sem visibilidade, exponha um overlay dev-only (tecla F) que lê estado já público (sem novo acoplamento):

```csharp
// MonoBehaviour dev-only, RuntimeInitializeOnLoadMethod, gateado por #if DEVELOPMENT_BUILD || UNITY_EDITOR
// OnGUI mostra: managers conectados (Instance != null), contadores de evento, estado de FSM atual.
```

Leia apenas `Instance`/propriedades públicas; **nunca** use `FindObjectOfType` nem crie acoplamento de gameplay para o overlay (rule `unity-architecture`).

## Regras

- Todo log de runtime carrega contexto (sistema + onde + o quê + fallback). Sem linhas peladas.
- Log de causa estável é **one-shot** (static ou por-instância). Antes de adicionar um `LogWarning` num caminho chamado por frame/por evento, pergunte: "isso pode repetir?" Se sim, guarde.
- Wiring-error obrigatório = `LogError` com formato canônico **+** desabilitar o sistema de forma segura (não continuar com null).
- Bug/invariante quebrado (categoria 4) = **fail fast** (throw/assert) em dev, não silencie com guard.
- Overlay/tracer são **dev-only** e read-only; nunca entram em build shipped nem viram dependência de gameplay.
- Não criar um `Logger`/`LogManager` paralelo que duplique `UnityEngine.Debug` — use `Debug.Log*` com as convenções acima (rode skill `system-reuse-audit` antes de qualquer wrapper de log).

## Saída esperada (checklist)

```text
Logs novos com prefixo [Sistema]: SIM
Log em caminho por-frame/por-evento tem guard de repeticao: SIM/NAO APLICAVEL
Wiring-error usa formato canonico (scene/go/component/campo/id): SIM/NAO APLICAVEL
Bug real (categoria 4) faz fail fast em vez de log silencioso: SIM
Overlay/tracer (se criado) e dev-only e read-only: SIM/NAO APLICAVEL
```

## Quando NÃO usar

- Texto **voltado ao player** → não é log; vai por `(skill: localization-authoring)` + HUD event.
- Falha de gameplay esperada (cooldown, sem stamina) → `bool` + `FailureReason`, não log (rule `csharp-style`).

## Relacionados

- `(rule: error-handling-resilience)` — taxonomy de 4 categorias; esta skill é o "como logar" dela
- `(rule: unity-architecture)` — wiring-error nunca mascarado por scene search
- `(skill: event-catalog-and-tracing)` — observabilidade da camada de eventos
- `(skill: runtime-bootstrap-pattern)` — onde nascem os wiring-errors de self-wiring
- `(agent: bugfix-investigator)` — consome logs com contexto para localizar a falha
