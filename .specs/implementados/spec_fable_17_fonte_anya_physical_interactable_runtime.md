# SPEC — Fonte de Anya Física: Interactable na Fazenda, Funções por Fragmento e Respawn

> **Spec ID:** `fable_17_spec_fonte_anya_physical_interactable_runtime`
> **Status:** BUILD_VALIDATED (executada — E03; evidência: docs/validation/fable_17_spec_fonte_anya_physical_interactable_runtime_execution_report.md)
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **Priority:** P0 (corretiva — hub central do jogo sem corpo)
> **Type:** Integration / Runtime / UI
> **Domain:** Farm / MainProgression
> **Parallelizable:** NO
> **Parallel group:** fable_corretivas
> **Can run with:** N/A
> **Must not run with:** fable_15, fable_16 (FarmScene generator), fable_10 (MainProgression lock)
> **Repo lock scope:** `Fonte/**`, `CreateMvpFarmScene.cs`, respawn path do player
> **Depends on:** WAVE 10 (FonteState/FonteFunctionUnlockService), fable_10 (Fragmento da Água via quest)
> **Blocks:** finais/purificação (WAVE 16/19 futuras)
> **Scope:** dar corpo físico e menu funcional à Fonte consumindo o estado WAVE 10 existente.
> **Out of scope:** decisão final Proteger/Selar/Usar, cinemática, arte, Água Viva como crop.

required_adrs: []
required_game_rules: [fonte_rules.md]

---

# /speckit.specify

## Contexto

A Fonte de Anya é o coração do design (respawn → Água Viva → respec → purificação → decisão
final, evoluindo por fragmentos). A WAVE 10 entregou TODO o estado (`FonteState`,
`FonteFunctionUnlockService`, `FragmentStateRecord`, validators) e a WAVE 04 um
`FonteMenuViewModel` CONTRACT_ONLY. Auditoria 00B: **zero menções a "Fonte" nos geradores de
cena** — não há objeto, interação, respawn nem menu. O jogador morre e respawna por outro
caminho (auditar PlayerRespawnedEvent atual).

## Problema

O hub narrativo-mecânico do jogo não existe fisicamente: F10 (Ato 1) concederia o Fragmento
da Água para desbloquear... nada visitável. A regra canônica "Fonte Menu não mostra função
não desbloqueada" não tem onde valer.

## Objetivo

Ao final desta spec, a FarmScene deve ter a Fonte (composto visual: bacia+água+pedestal,
perto da casa) com `FonteInteractable` abrindo menu modal (IMGUI no padrão dos painéis
atuais OU Canvas se F14 já executada — decidir na Fase 0) que mostra SOMENTE funções
desbloercadas via `FonteFunctionUnlockService`; respawn por morte deve ocorrer na Fonte; a
função Água Viva (desbloqueada pelo Fragmento da Água) deve conceder 1 frasco/dia
(item consumível que restaura e reduz fadiga).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/a_implementar/fable/fable_00B_adherence_audit_queue_triage.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- Fonte/{FonteState, FonteFunctionUnlockService, FonteAnyaValidator} (WAVE 10)
- UI/Fonte/FonteMenuViewModel (WAVE 04 CONTRACT_ONLY — consumir)
- MainProgression/FragmentStateRecord; PlayerRespawnedEvent; ModalManager
- SkillTreeManager (respec — função futura da Fonte; hook apenas)
- CreateMvpFarmScene v2
Não existe:
- objeto/interactable/menu/respawn-na-Fonte/Água Viva item
Auditar Fase 0:
- caminho atual de morte/respawn do player (quem consome PlayerRespawnedEvent? onde reposiciona?)
- API exata do FonteFunctionUnlockService (quais FonteFunction enum values)
```

## Escopo

```text
Inclui:
- visual composto da Fonte no CreateMvpFarmScene (perto da casa, (-3.5,-7)): bacia circular,
  água com pulso procedural (reuso do padrão ProjectileVisualAnimator), pedestal com colisão;
- FonteInteractable (IInteractable) → FonteMenuController (modal, ModalType novo Fonte ou
  reuso — auditar enum): lista funções com lock/unlock honesto (Respawn sempre; Água Viva
  por fragmento; Respec/Purificação visíveis como "???" se não desbloqueadas — regra de spoiler);
- respawn na Fonte: ao morrer, reposicionar player na Fonte (integrar no caminho auditado),
  publicar evento existente;
- Água Viva: item item_consumable_agua_viva (ItemDataInitializer) concedido 1x/dia via menu
  quando desbloqueada (flag diária no FonteState — auditar persistência WAVE 10; se não
  cobrir, campo aditivo padrão WI-18);
- respec: botão que chama SkillTreeManager respec API se desbloqueado (auditar API; se
  inexistente, exibir como futura — documentar);
- EditMode tests: visibilidade por unlock, concessão diária idempotente, respawn position.
```

## Fora de escopo

```text
Não inclui: purificação/decisão final (futuras), Canvas bonito (F14 reusa), Água Viva
plantável (WAVE 20 futuras), cutscene de fragmento.
```

## Regras de não duplicação

```text
Consumir FonteState/UnlockService/ViewModel existentes — proibido estado paralelo.
Menu segue padrão modal existente (push/pop ModalManager).
```

## Critérios de aceite

### CA-1 Fonte física e honesta
- Fonte na cena; menu mostra Respawn (sempre) e Água Viva apenas com fragmento; funções
  futuras ocultas/seladas (canon).
### CA-2 Respawn real
- Morte → player acorda na Fonte com HP parcial.
### CA-3 Água Viva diária idempotente
- 1 frasco/dia; reload não duplica; consumir restaura HP+reduz fadiga (integra F16 se presente).
- Evidência: EditMode tests + cenário humano.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Fonte/{FonteInteractable, FonteMenuController, FonteDailyGrantState}.cs (NOVOS)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (visual+interactable)
Assets/_Game/Tests/EditMode/Fonte/FontePhysicalTests.cs
```

## Paralelização

- Parallelizable: NO — FarmScene generator + MainProgression locks.

## Impacto em save/load

```text
Campo aditivo de concessão diária (lastGrantDay int) se WAVE 10 não cobrir — sem migration.
```

## Impacto em eventos

```text
Adds: FonteFunctionUsedEvent | Unsubscribe: YES
```

## Impacto em UI/Unity

```text
Changes UI: YES (menu modal) | Scenes via gerador | Play Mode final: YES
Human timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos

```text
Risco: caminho de respawn atual desconhecido. Mitigação: Fase 0 mapeia; integrar sem
quebrar KO da caverna (CavePlayerDefeatedEvent → retorno é fluxo separado e intocado).
```

## Rollback

```text
Remover interactable/menu do gerador; estado WAVE 10 intacto.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar respawn atual, FonteFunction enum, persistência WAVE 10.
- [ ] T002 — Visual+interactable no gerador.
- [ ] T003 — FonteMenuController honesto (lock/unlock) + testes.
- [ ] T004 — Respawn na Fonte + teste de posição.
- [ ] T005 — Água Viva diária + item + idempotência.
- [ ] T006 — csproj; run_strict_validation; report.
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
- Requires regression test: YES (KO da caverna intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano morrer→Fonte e fragmento→Água Viva

## Definition of Done

```text
Fonte física com menu honesto, respawn e Água Viva diária; builds 0E; report.
```

## Anti-regressão

```text
Fonte não revela função selada (canon). Cave KO flow intacto. Modal guard. Sem refs Unity em save.
```
