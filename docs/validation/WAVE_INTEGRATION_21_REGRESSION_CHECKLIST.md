# WAVE_INTEGRATION_21 — Regression Checklist

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Contexto

WAVE21 resultou em NO_OP_NO_P0_P1_FOUND — nenhum código foi alterado.

Este checklist serve como smoke test de regressão para confirmar que builds e comportamentos core continuam funcionando após WAVE20+21.

---

## Build Smoke (automatizado — já executado)

| Area | Smoke Test | Expected | Result | Notes |
|---|---|---|---|---|
| Build runtime | `dotnet restore + dotnet build Assembly-CSharp.csproj` | PASS (0E/0W) | PASS ✓ | Temp/ foi limpado; restore necessário |
| Build editor | `dotnet restore + dotnet build Assembly-CSharp-Editor.csproj` | PASS (0E/3W pre) | PASS ✓ | 3 warnings são pre-existing |

---

## Play Mode Smoke (humano — sub-slice sem wiring manual)

| Area | Smoke Test | Expected | Pass/Fail | Notes |
|---|---|---|---|---|
| Boot | Abrir FarmScene, Press Play | Player spawna sem erro | | |
| Farm movement | WASD/Arrows | Player anda, camera segue | | |
| DebugHud | Verificar visibilidade | Gold/HP/Stamina/Hotbar visible | | |
| Inventory | Pressionar I | Inventory abre com itens | | |
| Inventory modal | Tentar Dash com inventory aberto | Dash NÃO executa | | |
| Inventory close | Escape | Inventory fecha, Dash funciona | | |
| Skill tree | Pressionar U | Skill tree abre | | |
| Skill use | Dash (Space), Dodge (DoubleTap), Block (Shift) | Executa corretamente | | |
| Quest | (se Town wired) J = QuestLog abre | QuestLog modal funciona | | Requer WAVE13 |
| Save | Pressionar F5 | "Jogo salvo." no HUD | | |
| Load | Pressionar F9 | "Jogo carregado." no HUD | | |
| State persistence | Load após save | Gold/inventory/quest mantidos | | |
| Modal guards | Inventory+Skill+QuestLog durante modal | Dash/Dodge/Block bloqueados | | |

---

## O Que NÃO Mudou em WAVE21

```
Nenhum arquivo C# foi alterado.
Nenhuma cena foi alterada.
Nenhum asset foi alterado.
Apenas documentação e validator criados.
```

Portanto não há risco de regressão introduzida por WAVE21.

---

## Resultado Esperado

```
Build: PASS (idêntico ao WAVE20)
Play Mode sub-slice: IDÊNTICO ao WAVE20 (sem mudança de código)
Regressão introduzida por WAVE21: NENHUMA
```
