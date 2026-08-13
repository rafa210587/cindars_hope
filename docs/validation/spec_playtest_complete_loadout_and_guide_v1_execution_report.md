---
doc_type: validation
status: evidence
spec_id: spec_playtest_complete_loadout_and_guide_v1
validation_type: automated_and_manual_pending
result: PARTIAL
date: 2026-08-12
executor: Codex
source_of_truth: false
validated_adrs: []
validated_game_rules: []
---

# Execution Report — spec_playtest_complete_loadout_and_guide_v1

> **Status honesto:** `PARTIAL`. A implementação e os builds modulares concluíram com exit code 0,
> mas a validação de docs retornou exit 1, o Unity Editor aberto impediu o batchmode e o `.csproj`
> gerado ainda não incluiu o novo arquivo de teste. Play Mode humano permanece `NOT RUN` por design.

## Scope lock

Arquivos permitidos efetivamente usados:

```text
Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs
Assets/_Game/Tests/EditMode/Editor/DebugLoadoutDefinitionTests.cs
docs/validation/playmode/PLAYTEST_SIMPLES.md
docs/validation/spec_playtest_complete_loadout_and_guide_v1_execution_report.md
```

Arquivos protegidos não editados: `Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`,
`Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Inventory/**`,
`Assets/_Game/Scripts/Farm/**`, `Packages/**`, `ProjectSettings/**` e `docs_old/**`.

A spec alvo foi completada com os marcadores de governança exigidos após a primeira validação;
`.codex/config.toml.bak-20260812-1934` permaneceu preservado e fora do escopo.

## Phase 0 — Existing systems audit

| Sistema | Evidência | Decisão |
|---|---|---|
| Provisionamento QA | `DebugLoadoutProvisioner` já era o único provisionador | REUSE/EXTEND |
| Starter de jogo novo | `RepairPlayerStartingItems` já garante hoe/regador, mas não repara save existente | Não alterar |
| Equipamento | `EquipmentManager.EquipTool` e `HasTool` já cobrem ferramentas | Reutilizar |
| Hotbar | `HotbarState.SetSlot` já cobre bindings 0..5 | Reutilizar |
| Farm input | `FarmTillingInputController` exige Hoe/WateringCan e usa `F` | Documentar |

Busca estática confirmou exatamente uma definição de asset para cada um dos 15 IDs do kit, inclusive
`item_shop_tool_hoe_basic`, `item_shop_tool_watering_can_basic`, `item_tool_pickaxe_iron` e
`item_tool_fishing_rod_basic`.

## Implementação

- `DebugLoadoutEntry` e `SmokeLoadout` tornam a definição do kit pura e inspecionável.
- `TryValidateSmokeLoadout` rejeita ID vazio/duplicado, amount inválido, slot fora de 0..5 ou
  duplicado e ausência de hoe, watering can, pickaxe, fishing rod, seed, weapon, ammo, consumable e
  pelo menos dois materiais.
- Slots visíveis determinísticos: `1` hoe, `2` watering can, `3` iron pickaxe, `4` fishing rod,
  `5` bow, `6` carrot seed.
- O provisionamento aceita item pré-existente, reaplica a hotbar, equipa somente a hoe canônica e
  acumula falhas de item/hotbar/equipment. `READY FOR PLAYTEST` só é emitido sem falhas; caso contrário
  emite `REQUIRED FAILURES (N)` com detalhes.
- O guia foi reescrito com setup, controles e 11 etapas. A inspeção contou `11/11` ocorrências de cada
  label contratual: `Pré-condição`, `Item/slot`, `Ação exata`, `Esperado`, `Se falhar` e `Resultado`.

## Acceptance criteria e Spec Compliance Matrix

| Critério | Implementação/evidência | Status |
|---|---|---|
| 14.1 ferramentas reais e slots 0/1/2/3 | IDs canônicos na definição; aliases legados ausentes; teste criado | IMPLEMENTADO; NUnit NOT RUN |
| 14.2 categorias completas | seed, 4 tools, bow, ammo, potion/food, 4 materiais, crop e fish | IMPLEMENTADO; NUnit NOT RUN |
| 14.3 prontidão real | literais `READY FOR PLAYTEST` e `REQUIRED FAILURES`; Editor build exit 0 | OK |
| 14.4 guia operacional | seis labels em todas as 11 etapas; contagem 11/11 | OK |
| 14.5 compilação modular | Editor e Tests.EditMode builds exit 0 | OK, com ressalva do teste novo abaixo |

## Validation

| Comando/check | Exit code | Resultado honesto |
|---|---:|---|
| `dotnet build CindarsHope.Editor.csproj` | 0 | PASS verificado pelo orquestrador — 0 erros, 0 warnings |
| `dotnet build CindarsHope.Tests.EditMode.csproj` | 0 | PASS do projeto gerado — 0 erros, 0 warnings |
| Roslyn `csc` isolado sobre `DebugLoadoutDefinitionTests.cs` | 0 | PASS de compilação do arquivo novo contra Editor + NUnit |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\validate_docs.ps1` | 1 | FAIL — somente dívidas preexistentes fora desta spec; a spec alvo não aparece mais nos erros |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | FAIL — corruption guard PASS, generated-project builds PASS, docs non-zero |
| `tools/unity/RunUnityEditModeTests.ps1` | NOT RUN | Unity Editor aberto; batchmode proibido pelo lock |
| Play Mode pelo novo guia | NOT RUN | Aguardando execução e reporte humano `OK/FALHOU` |

O primeiro disparo direto de `validate_docs.ps1` foi bloqueado pela Execution Policy. O retry correto
com `-ExecutionPolicy Bypass` executou e retornou exit 1. A primeira rodada apontou também metadados
ausentes na spec alvo; eles foram corrigidos. A repetição continuou exit 1 apenas por documentos e
scanners preexistentes fora do escopo, sem citar `spec_playtest_complete_loadout_and_guide_v1`.

### Limitação do teste novo

`CindarsHope.Tests.EditMode.csproj` usa lista explícita gerada pelo Unity e, enquanto o Editor estava
aberto, não adicionou `DebugLoadoutDefinitionTests.cs`. Portanto o exit 0 desse projeto não prova que o
novo arquivo foi incluído no assembly Unity. Para reduzir o risco, o arquivo foi compilado isoladamente
com Roslyn `csc`, referências `netstandard2.1`, `CindarsHope.Editor.dll` e `nunit.framework.dll` (exit 0).
Os quatro testes ainda não foram executados pelo NUnit/Unity. Não foi editado o `.csproj`, pois ele está
fora do scope permitido.

## Testing Quality Gate

```text
Changed runtime code:           NO (tooling Editor only)
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — 4 EditMode tests
Automated tests command:        NUnit NOT RUN — Unity Editor aberto; Roslyn compile isolado exit 0
Manual Play Mode scenario:      docs/validation/playmode/PLAYTEST_SIMPLES.md
Justification if no tests:      N/A — testes foram escritos; execução está bloqueada pelo Editor aberto
Residual risk:                  Unity/NUnit ainda não provou reflection access nem comportamento em Play Mode
```

## Non-Regression Review

```text
File & Directory:  PASS — entregáveis dentro da §18; nenhum asset/scene/prefab tocado
Git Safety:        PASS — sem push, reset, clean, stash ou commit
Runtime APIs:      PASS — nenhuma API de busca global adicionada; tooling Editor apenas
Save DTOs:         N/A
Balance Values:    PASS — quantidades são dados fixos do kit QA, não balance runtime
Event Bus:         N/A
Namespaces:        PASS — nenhum CindarsHope.Debug
Status claims:     PASS — sem ACCEPTED/PLAYMODE_VALIDATED/PASS de NUnit
Testing QG:        WARNING — teste escrito, porém Unity/NUnit não executado
Status geral:      WARNING
```

Risco residual: o menu e os bindings precisam ser exercitados no Play Mode; o teste EditMode precisa
ser importado/regenerado pelo Unity e executado após fechar o Editor atual.

## Dependency Chain

```text
Original target: spec_playtest_complete_loadout_and_guide_v1
Dependency chain: handoff/current state/static audit — disponíveis e lidos
Forbidden dependencies: none
Resolved depth: 0
Plan file / Batch state: not required (no pending same-wave spec)
Can continue original target: YES
```

## Phase status e trabalho restante

| Fase | Status |
|---|---|
| Phase 0 — audit | COMPLETE |
| Phase 1 — implementation | CODE_COMPLETE |
| Phase 2 — modular builds | PASS (exit 0) |
| Phase 2 — docs/strict | FAIL (exit 1) / pending spec-governance correction |
| Phase 2 — Unity/EditMode | NOT RUN — Editor aberto |
| Phase 3 — human Play Mode | NOT RUN — aguardando humano |

Próximas ações:

1. Fechar o Unity Editor, reabrir/importar para regenerar o `.csproj` e confirmar que ele inclui
   `DebugLoadoutDefinitionTests.cs`.
2. Rodar `RunUnityEditModeTests.ps1` com filtro
   `CindarsHope.Tests.EditMode.Editor.DebugLoadoutDefinitionTests`; exigir exit 0 e 4/4 passed.
3. Executar `PLAYTEST_SIMPLES.md` e reportar os 11 resultados `OK/FALHOU`.

Nenhum commit e nenhum push foram realizados.
