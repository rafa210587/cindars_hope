# SPEC — Player: Fadiga, Sono e Colapso 02:00 (Wiring + Cama na Fazenda)

> **Spec ID:** `fable_16_spec_player_fatigue_sleep_collapse_wiring`
> **Status:** BUILD_VALIDATED (executada — E02; evidência: docs/validation/fable_16_spec_player_fatigue_sleep_collapse_wiring_execution_report.md)
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **Priority:** P0 (corretiva)
> **Type:** Integration / Runtime
> **Domain:** Player / Time
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_corretivas
> **Can run with:** fable_04, fable_09, fable_19
> **Must not run with:** fable_15 (DayStarted/FarmScene locks), fable_17 (FarmScene generator)
> **Repo lock scope:** `Player/Conditions/**`, `GameTimeManager` consumers, `CreateMvpFarmScene.cs`
> **Depends on:** WAVE 05 (FatigueSystem/SleepRecoveryCalculator órfãos), WAVE 02 (hora do dia)
> **Blocks:** N/A
> **Scope:** ligar fadiga/sono/colapso ao loop com cama interagível e dormir = day transition.
> **Out of scope:** sonhos/eventos noturnos, cama na cidade (estalagem — F19 hook), penalidades por clima.

required_adrs: []
required_game_rules: [player_rules.md, time_rules.md]

---

# /speckit.specify

## Contexto

Auditoria 00B: `Player/Conditions/` (FatigueSystem, FatigueState, FatigueThreshold,
FatigueGainContext, SleepRecoveryCalculator, PlayerConditionSnapshot — WAVE 05) está completo
e órfão: nada o instancia, nenhuma cena tem cama, e o canônico "02:00 é limite padrão de
colapso/sono forçado" (SEASONS direction, resolvido em SPEC_SOURCE_MAP) não acontece. Dormir
hoje só existe via `DayAdvanceInput` (tecla debug).

## Problema

Sem fadiga/sono o dia não tem custo: o jogador vara madrugadas minerando sem consequência,
quebrando o balanceamento central de farm sim (orçamento de tempo/energia por dia) e
deixando `SleepRecoveryCalculator` e os thresholds pagos sem uso.

## Objetivo

Ao final desta spec, `FatigueSystem` deve rodar hospedado em um `PlayerConditionService`
(bootstrap), acumulando fadiga por hora acordado e por ação (hooks de stamina gasta),
aplicando thresholds (lentidão leve/feedback) e forçando colapso às 02:00 (acordar na cama
com penalidade); uma cama interagível na FarmScene deve permitir dormir voluntariamente
(day transition + recuperação via `SleepRecoveryCalculator`), persistindo fadiga no save.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/a_implementar/fable/fable_00B_adherence_audit_queue_triage.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe, testado e NÃO recriar (apenas LIGAR):
- Player/Conditions/* (sistema completo de fadiga/sono)
- GameTimeManager (hora), TimeManager (day transition), DayAdvanceInput (debug — manter)
- StaminaManager (hook de gasto), ModalManager, SaveManager padrão WI-18
- CreateMvpFarmScene (Zone_HouseEntrance em (-5,-7) — local da cama)
Não existe:
- host do FatigueSystem; cama; colapso; persistência de fadiga
Auditar Fase 0:
- API exata do FatigueSystem/thresholds; SaveData.cs (campo de fadiga já previsto? grep achou "Fatigue" em Save/SaveData.cs — confirmar wiring)
```

## Escopo

```text
Inclui:
- PlayerConditionService (NOVO host bootstrap): tick por hora in-game (GameTimeManager),
  ganho por hora acordado + por stamina gasta (evento/callback do StaminaManager — auditar);
- thresholds aplicados: Tired (feedback HUD), Exhausted (move speed x0.85 via PlayerController
  hook), Critical (stamina máxima temporariamente reduzida);
- colapso 02:00: fade-out simples (tela preta 1s via GameplayHudCanvas), day transition,
  acordar na cama com fadiga residual (SleepRecoveryCalculator com penalidade de colapso);
- BedInteractable (NOVO IInteractable) na casa da FarmScene (gerador): confirmar dormir →
  day transition + recuperação plena;
- persistência: campo de fadiga no save (auditar SaveData.cs — wiring do campo existente ou
  aditivo padrão WI-18);
- EditMode tests: acúmulo por hora/ação, thresholds, recuperação dormir vs colapso, round-trip.
```

## Fora de escopo

```text
Não inclui: cama da estalagem (F19 hook), modificadores por clima/estação, sonhos,
buffs de comida sobre fadiga.
```

## Regras de não duplicação

```text
Proibido reescrever Player/Conditions — somente hospedar.
DayAdvanceInput (debug) permanece; dormir usa o MESMO caminho de day transition.
```

## Critérios de aceite

### CA-1 Fadiga viva
- Fadiga sobe por hora acordado e por ações; thresholds aplicam efeitos observáveis.
### CA-2 Colapso canônico
- Às 02:00 colapso força novo dia com penalidade; dormir voluntário recupera mais.
### CA-3 Persistência
- Fadiga sobrevive a save/load; save antigo carrega com fadiga 0.
- Evidência: EditMode tests + cenário humano (varar a noite).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Player/Conditions/PlayerConditionService.cs (NOVO host)
Assets/_Game/Scripts/World/BedInteractable.cs                    (NOVO)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs  (cama na casa)
Assets/_Game/Scripts/Save/SaveManager.cs                         (wiring campo fadiga)
Assets/_Game/Tests/EditMode/Player/FatigueWiringTests.cs
```

## Paralelização

- Parallelizable: CONDITIONAL — locks: FarmScene generator, day transition consumers.

## Impacto em save/load

```text
Campo aditivo (float fatigue) — default 0; sem migration; sem refs Unity.
```

## Impacto em eventos

```text
Adds: PlayerFatigueChangedEvent, PlayerCollapsedEvent | Unsubscribe: YES
```

## Impacto em UI/Unity

```text
Scenes via gerador; Play Mode final: YES; Human timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos

```text
Risco: colapso durante modal/dialogo. Mitigação: adiar colapso até fechar modal (mesmo guard
de input existente); teste do gate.
Risco: lentidão por threshold conflitar com Chill (F01). Mitigação: multiplicadores compostos
com floor 0.5x documentado.
```

## Rollback

```text
Remover host/cama/wiring — módulos voltam a órfãos; save com campo extra continua válido.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar API de Conditions/, SaveData.cs e hooks do StaminaManager.
- [ ] T002 — PlayerConditionService + acúmulo + thresholds + testes.
- [ ] T003 — Colapso 02:00 (fade + transition + penalidade) + testes.
- [ ] T004 — BedInteractable + gerador FarmScene.
- [ ] T005 — Save wiring + round-trip/legado.
- [ ] T006 — Eventos/HUD debug; csproj; run_strict_validation; report.
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
- Requires regression test: YES (DayAdvanceInput/day transition intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano dormir/colapsar

## Definition of Done

```text
Fadiga/sono/colapso vivos com cama na fazenda e persistência; builds 0E; report.
```

## Anti-regressão

```text
Day transition único (sem segundo caminho). Modal guard respeitado. Hunger/stamina intactos.
```
