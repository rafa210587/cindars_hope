---
doc_type: validation_report
spec_id: fable_38_spec_minimap_v1_runtime
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-21
validated_adrs: [ADR-0005]
validated_game_rules: [ui_rules.md, cave_rules.md]
---

# Execution Report — fable_38 — Minimapa v1 (runtime)

> Status: **BUILD_VALIDATED_WITH_WARNINGS** — lógica determinística pronta + EditMode tests;
> binding visual (RawImage/Texture2D em scene/canvas) e Play Mode DEFERIDOS para a validação
> final do lote (DEFERRED_UI_VISUAL / DEFERRED_TO_FINAL_VALIDATION), como o widget F20.

## Acceptance criteria extracted

| CA | Critério (resumo) | Implementação | Evidência |
|----|-------------------|---------------|-----------|
| CA-1 | Caverna nasce escura; andar revela raio 6; revisita na MESMA run mantém revelado (estado de nível do stable-run) | `MinimapRenderer.CollectRevealedCells` (raio circular) + `VisitedLevelSnapshot.RevealCells/IsCellRevealed` (campo aditivo `RevealedCells`, fora do LayoutHash) + `MinimapCaveSourceBuilder.RevealAroundPlayer` | EditMode: `CollectRevealedCells_Radius6_*`, `RevealCells_IsIdempotent_RevisitKeepsRevealed`; cenário humano passos 5-6 |
| CA-2 | Nova run (KO/new game → troca CaveRunSeed) reseta o fog | Fog vive no snapshot keyed por run; nova run = `VisitedLevelSnapshots` limpo/recriado (sem save global) | EditMode: `NewRunSeed_ProducesFreshSnapshot_FogReset`, `LegacySnapshot_WithoutRevealedField_StartsDark`; cenário humano passo 7 |
| CA-3 | Town/Farm estático sem fog (pontos coloridos); M alterna widget; aba Mapa 2× com legenda | `MinimapGridSource(useFog:false)` + paleta de ícones; `MinimapWidget.HandleToggleInput` (M + modal guard); `GameplayScreenTab.Map` (já existe) renderiza buffer 2× via `RenderNow` | EditMode: `CellColor_*`, `IconColor_*`, `Render_TownFarm_NoFog_*`, `Render_PlayerIconPaintsWhite_*`; cenário humano passos 1-4, 9 |
| CA-4 | Update do minimapa no máximo 4×/s (sem custo por frame) | `MinimapWidget` acumulador `1/4 s` com `Time.unscaledDeltaTime`; buffer/Texture2D reutilizados | EditMode: `Throttle_FourUpdatesPerSecond_*`; cenário humano passo 10 |

## Existing systems audit

| Sistema | Encontrado | Decisão | Nota |
|---------|-----------|---------|------|
| Estado de nível do stable-run | `VisitedLevelSnapshot` (Cave/Runtime) | **ESTENDIDO** (campo aditivo `RevealedCells` + `RevealCells/IsCellRevealed`) | Mesmo padrão de `OpenedChestIds`/`TrapStates`: estado mutável FORA do LayoutHash. NÃO criou save global (ADR-0005). |
| Grid do nível atual | `VisitedLevelSnapshot.Walkable/WallTilesList`, `Width/Height`, `Entrance/ExitPosition` | **REUSADO** (lido por `MinimapCaveSourceBuilder`) | Nunca reconstrói layout nem reroll — só lê. |
| HUD widget pattern (F20) | `ClockCalendarHudWidgetModel`/`View` | **SEGUIDO** (projection pura + adapter fino, binding visual DEFERIDO) | `MinimapRenderer` puro / `MinimapWidget` casca fina. |
| Aba Mapa (F14) | `GameplayScreenTab.Map` (enum já existe), `GameplayScreensPanelModel` | **REUSADO** | Não criou nova aba/enum; renderer produz buffer 2× para a aba. |
| Input/modal guard | `GameBootstrap.ModalManager.HasActiveModal` (usado pelo `GameplayInputRouter`) | **REUSADO** | M alterna só o overlay não-modal; com modal aberto, M não alterna. |
| Lantern (F31) | `MagicItemCatalog.LanternOfTrueSight` | **HOOK** (`ResolveRevealRadius(hasLantern)` 6→9) | Sem tocar o catálogo de itens. |
| Segunda câmera / RenderTexture | — | **NÃO CRIADO** | Render procedural via `Color[]`→`Texture2D` (regra de não-duplicação). |

## Spec Compliance Matrix

| Requisito da spec | Implementação | Status |
|-------------------|---------------|--------|
| MinimapWidget (RawImage+Texture2D, 4×/s, toggle M) | `MinimapWidget.cs` | OK (binding visual DEFERIDO) |
| Renderer puro (célula→cor, centragem/rolagem, fog, ícones) | `MinimapRenderer.cs` + `MinimapGridSource.cs` | OK |
| Fog raio 6 no estado de nível existente (não save global) | `VisitedLevelSnapshot.RevealedCells` + builder | OK |
| Town/Farm estático completo + pontos coloridos | `MinimapGridSource(useFog:false)` + ícones | OK (montagem das posições de cena = wiring DEFERIDO ao Editor) |
| Cave player centrado, mapa rola; escadas quando reveladas | `ComputeViewOrigin` + `MinimapCaveSourceBuilder` ícones StairsUp/Down | OK |
| Toggle M; aba Mapa 2× com legenda | `HandleToggleInput`; `GameplayScreenTab.Map` + `RenderNow` 2× | OK (legenda visual DEFERIDA) |
| lantern_of_true_sight 6→9 (hook) | `ResolveRevealRadius` | OK |
| EditMode tests (reveal/persistência/célula→cor/centragem) | `MinimapTests.cs` (18 tests) | OK |
| Sem segunda câmera / save global novo / 2º canvas-builder | — | OK |

## Validation

```text
Validation method: dotnet build (--no-restore) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp: PASS (exit 0, 0 erros, 1 aviso pré-existente)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros, 3 avisos pré-existentes)
Docs validation: PASS (exit 0)
Diff completeness (scoped): PASS (exit 0)
run_strict_validation: ver nota ambiental (3 cenas .unity já modificadas no working tree, NÃO desta spec)
```

EditMode test command: `Unity Test Runner (EditMode)` — NOT RUN nesta sessão (sem Unity Editor;
DEFERRED_TO_FINAL_VALIDATION). Os 18 tests compilam dentro de `Assembly-CSharp.csproj` (build 0E).

## Honest status rationale

Núcleo determinístico (fog reveal, persistência intra-run, reset por seed, célula→cor,
centragem/rolagem, throttle) está implementado e coberto por 18 EditMode tests que **compilam**
(build 0E). O binding visual final (RawImage/Texture2D no GameplayHudCanvas, posições de cena de
Town/Farm, legenda da aba Mapa) e a execução do Play Mode/Test Runner exigem o Unity Editor e
ficam DEFERIDOS para a validação final do lote — exatamente como o widget de relógio F20 foi
fechado (DEFERRED_UI_VISUAL). Por isso **BUILD_VALIDATED_WITH_WARNINGS**, não ACCEPTED.

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (fog reveal por raio, centragem/rolagem, célula→cor, throttle, persistência intra-run)
Changed Unity scene/prefab/asset wiring: NO (nenhum .unity/.prefab/.asset editado; binding visual DEFERIDO ao Editor)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/UI/MinimapTests.cs, 18 tests)
Automated tests command: Unity Test Runner (EditMode) — NOT RUN nesta sessão (sem Unity Editor); compila no build (0E)
Manual Play Mode scenario: docs/validation/playmode/fable_38_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: binding visual da Texture2D, anchoring %, montagem das posições de cena (Town/Farm) e a execução do Play Mode são verificados apenas na validação final do lote.
```

## validated_game_rules

- `ui_rules.md` — Rule 1/7 (overlay não-modal, nunca bloqueia movimento; visibilidade reage a
  modal), Rule 3 (minimapa sup-dir sob o relógio, nunca mostra inimigos — F38), Rule 8 (anchor
  em % — mother rule). Cumprido: M só alterna overlay; sem inimigos no mapa; sem pixels absolutos.
- `cave_rules.md` / ADR-0005 — fog vive EXCLUSIVAMENTE no `VisitedLevelSnapshot` (estado de nível
  do stable-run), aditivo e FORA do LayoutHash; revisita mantém; nova run reseta; nenhum reroll de
  layout/seed por causa do minimapa; nenhum save global novo.

## Remaining work (DEFERRED)

- Binding visual no Editor (RawImage + Texture2D no GameplayHudCanvas, sob o relógio F20).
- Montagem das posições de cena de Town/Farm (player/NPC/board/entrada) no builder do HUD.
- Legenda visual da aba Mapa (2×).
- Execução do Unity Test Runner (EditMode) + Play Mode (cenário humano).
