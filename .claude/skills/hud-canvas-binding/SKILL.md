---
name: hud-canvas-binding
description: A metade de cena/Canvas do padrão de UI — montar a hierarquia de Canvas e plugar a View MonoBehaviour no GameplayHudCanvas / scene creators, fechando o "DEFERRED: binding do canvas" que toda spec de UI deixou. Use depois da ui-projection-pattern, ao materializar um widget de HUD, tela ou overlay numa cena.
---

# Skill: Binding de Canvas/HUD

A `(skill: ui-projection-pattern)` entrega a metade testável em C# puro (ViewModel/projection) — mas **toda** spec de UI da run (`fable_14`, `fable_20`, `fable_38`, `fable_45`, `fable_56`, `fable_62`, `fable_64`, `fable_65`) terminou com *"DEFERRED: binding do canvas"*. Esta skill é a outra metade: como construir a hierarquia de `Canvas` e plugar a `View` em runtime, reusando o bootstrap de HUD existente em vez de editar `.unity` à mão.

## Quando usar

- Depois de criar um ViewModel/projection (`(skill: ui-projection-pattern)`), para torná-lo visível.
- Ao adicionar um **widget de HUD** sempre-on (relógio/calendário, minimapa, metas diárias).
- Ao adicionar uma **tela/panel** (bestiário, sistema, morte) que entra no painel único.
- Ao fechar o débito "DEFERRED_UI_VISUAL" de uma spec já BUILD_VALIDATED.

## Por que existe

A lógica de UI compila e é testada em EditMode, mas não aparece até a `View` existir numa cena com `Canvas`, `RectTransform` e binding de `Text`/`Image`. Editar `.unity` YAML à mão é proibido (rule `unity-assets`); o caminho canônico é **construir a hierarquia via código** (no bootstrap de HUD ou num scene creator), igual ao resto do projeto.

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `GameplayHudBootstrap` (`CindarsHope.UI.HUD`) | Self-bootstrap (`RuntimeInitializeOnLoadMethod`) que monta o HUD em runtime; ponto de entrada para registrar um widget novo |
| `GameplayHudCanvasController` | Controla o `Canvas` do HUD em runtime; hospeda os widgets sempre-on |
| `CreateMvpFarmScene` / `CreateMvpTownScene` / `CreateMvpCaveScene` (`Editor/SceneCreation`) | Scene creators — onde objetos persistentes de cena são montados via Unity API |
| `ModalManager` + `(skill: ui-modal-stack)` | Telas modais entram pelo stack, não direto no HUD |
| `UiFocusController` | Roteamento de input focus (sem mover player com modal aberto) |

## Procedimento

### Widget de HUD sempre-on (relógio, minimapa, metas)

1. Tenha o ViewModel/projection pronto e testado (`(skill: ui-projection-pattern)`).
2. Crie a `View` MonoBehaviour fina em `Scripts/UI/<Area>/`: campos `[SerializeField]` para `Text`/`Image`, um método `Rebind(viewModel)`, subscribe a eventos do `GameEventBus` em `OnEnable`, unsubscribe em `OnDisable`.
3. Monte a hierarquia **em código** dentro de `GameplayHudBootstrap`/`GameplayHudCanvasController` (criar `GameObject` filho do Canvas, `AddComponent<RectTransform>`, ancoragem, `AddComponent<TextMeshProUGUI>`/`Image`, `AddComponent<MinhaView>`, ligar os `[SerializeField]` via código). **Não** edite `.unity` YAML.
4. O widget faz rebuild a partir de evento, nunca poll.

### Tela/panel (entra no painel único / modal)

1. Para tela modal: registre no `ModalManager` (`(skill: ui-modal-stack)`); o painel único de abas (F14) já tem o slot — plugue a `View` na aba certa.
2. Roteie input focus pelo `UiFocusController`; garanta que o player não move com modal aberto.
3. Estados empty/error/confirmation já vêm do ViewModel (precedente F14).

### Quando o binding tem que viver na cena (objeto físico)

Se o elemento é um objeto de mundo (não HUD/Canvas) — ex.: marcador, NPC, carta — use `(skill: scene-interactable-wiring)` e estenda o `CreateMvp*Scene` correspondente; rode depois o orquestrador `CindarsHope/Prepare Everything (One Click)` para regenerar a cena.

## Regras

- **Nunca** editar `.unity`/`.prefab` YAML à mão (rule `unity-assets`). Hierarquia de Canvas é montada por código (bootstrap ou scene creator via Unity API).
- A `View` é MonoBehaviour **fino**: só binding + subscribe/unsubscribe; zero regra de domínio (essa vive no ViewModel).
- Rebuild por evento (`GameEventBus`), nunca `Update` polling.
- Modal passa pelo `ModalManager` + `UiFocusController`; HUD sempre-on entra pelo `GameplayHudBootstrap`.
- Reusar o canvas/bootstrap existente — **não** criar um segundo `Canvas` global, segundo `EventSystem` ou segundo HUD root.
- Closeout: a parte visual exige cenário humano de Play Mode (`(skill: gameplay-test-scenario)`); declare honestamente o que foi montado vs. o que ainda precisa de conferência visual.

## Saída esperada (checklist)

```text
ViewModel/projection pronto e testado (EditMode): SIM
View MonoBehaviour fina (so binding + subscribe/unsubscribe): SIM
Hierarquia de Canvas montada por codigo (sem editar .unity): SIM
Widget/painel reusa GameplayHudBootstrap/ModalManager (sem 2o canvas): SIM
Rebuild por evento (sem poll): SIM
Cenario humano de Play Mode criado: SIM/NAO APLICAVEL
```

## Relacionados

- `(skill: ui-projection-pattern)` — a metade em C# puro; esta skill é a metade de cena
- `(skill: ui-modal-stack)` — telas modais entram pelo stack
- `(skill: scene-interactable-wiring)` — objetos de mundo (não-Canvas) na cena
- `(skill: bootstrap-wiring)` — wiring de managers que o HUD consome
- `(skill: gameplay-test-scenario)` — cenário humano para a validação visual
- `(rule: unity-assets)` — sem edição manual de YAML
