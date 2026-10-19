# Execution Report — spec_codex_12_dead_code_removal_batch2

Status: BUILD_VALIDATED

## Acceptance criteria extracted

- 14.1 — 15 arquivos removidos com zero referencias (Grep antes/depois documentado).
- 14.2 — 4 eventos mortos removidos (2 arquivos inteiros + 2 pontuais) com zero publish/subscribe residual.
- 14.3 — ValidateSpec11Damage.cs ajustado (condicao L62 nao depende mais de StatusTickedEvent; logica remanescente intacta).
- 14.4 — Build limpo: dotnet build PASS (0 erros / 0 warnings novos) para Assembly-CSharp.csproj e Assembly-CSharp-Editor.csproj.

## Existing systems audit

Fase 0 (re-Grep obrigatorio, replicando o protocolo do lote 1 / spec_codex_06) foi executada imediatamente antes de cada delecao:

- Grep de nome de classe para os 15 arquivos em todo `Assets/**` (codigo + testes): cada classe aparece apenas no proprio arquivo, exceto `QuestConditionService.cs`, que tambem contem `QuestContext` (auxiliar do mesmo arquivo, sem uso externo).
- Grep de `RuntimeInitializeOnLoadMethod` em cada um dos 15 arquivos: zero ocorrencias (nenhum auto-bootstrap invisivel).
- Extracao de GUID de cada `.meta` e busca desse GUID em todos os `.unity`/`.prefab`/`.asset` do projeto (1291 arquivos escaneados): zero ocorrencias para os 15 GUIDs.
- Grep dos 4 nomes de evento em todo `Assets/**`: cada um aparece apenas na propria definicao (nenhum publish, nenhum subscribe).
- Leitura completa de `ValidateSpec11Damage.cs` antes de editar: a condicao em L62 checa 3 substrings simultaneas via leitura de texto do source (nao reflexao); ajuste = remover apenas `&& script.text.Contains("StatusTickedEvent")`, preservando a checagem de `DamageAppliedEvent` e `StatusAppliedEvent`.

**Desvio nao previsto pela spec (achado em Fase 4, corrigido antes do closeout):** `Assets/_Game/Scripts/Loot/RewardTableDefinition.cs` continha, alem da classe `RewardTableDefinition` (de fato morta), a classe auxiliar `RewardGrantResult` no MESMO ARQUIVO. `RewardGrantResult` E referenciada por um teste EditMode ativo: `Assets/_Game/Tests/EditMode/Economy/LootTableContractTests.cs:154` (`RewardGrantResult.Fail("blocked")`). O Grep de Fase 0 (nome de classe `RewardTableDefinition`) nao capturou essa dependencia porque o nome da classe referenciada e diferente do nome do arquivo/classe auditada pela spec. Ao rodar `dotnet build` apos a delecao, o erro `CS0103: O nome "RewardGrantResult" nao existe no contexto atual` expos a referencia real. Acao tomada: `RewardTableDefinition.cs` + `.meta` foram RESTAURADOS via `git checkout -- <arquivo>` e a entrada correspondente foi re-adicionada ao `Assembly-CSharp.csproj`. Este arquivo NAO foi deletado nesta execucao — ver secao de desvios abaixo.

## Files changed

**14 arquivos deletados (.cs + .meta cada, total 28 arquivos):**

```
Assets/_Game/Scripts/Farm/Lots/FarmLotSignInteractable.cs (+ .meta)
Assets/_Game/Scripts/Quests/QuestConditionService.cs (+ .meta)
Assets/_Game/Scripts/UI/Cave/CaveCheckpointSideMenuController.cs (+ .meta)
Assets/_Game/Scripts/UI/Notification/NotificationToastController.cs (+ .meta)
Assets/_Game/Scripts/UI/Runtime/ClockCalendarHudWidgetView.cs (+ .meta)
Assets/_Game/Scripts/World/Altars/GodMarkAltarInteractable.cs (+ .meta)
Assets/_Game/Scripts/World/WorldTimeProvider.cs (+ .meta)
Assets/_Game/Scripts/Cave/Loot/CaveTreasureProfile.cs (+ .meta)
Assets/_Game/Scripts/Cave/Runtime/CaveConfinementValidator.cs (+ .meta)
Assets/_Game/Scripts/Cave/Validation/CaveBossGateValidator.cs (+ .meta)
Assets/_Game/Scripts/City/Layout/CityZoneDefinition.cs (+ .meta)
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs (+ .meta)
Assets/_Game/Scripts/Farm/Harvest/HarvestCommand.cs (+ .meta)
Assets/_Game/Scripts/Save/SaveSectionOwnershipRegistry.cs (+ .meta)
```

**1 arquivo de evento inteiro deletado (.cs + .meta):**

```
Assets/_Game/Scripts/Core/Events/EquipmentUpgradedEvent.cs (+ .meta)
```

**3 arquivos de evento editados (removida so a definicao morta, arquivo compartilhado preservado):**

```
Assets/_Game/Scripts/Core/Events/UIEvents.cs — removido "public struct PauseClosedEvent { }"
Assets/_Game/Scripts/Core/Events/PlayerCombatEvents.cs — removida classe SpellCastFailedEvent
Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs — removida classe StatusTickedEvent
```

**1 validator editado:**

```
Assets/_Game/Scripts/Editor/Validation/ValidateSpec11Damage.cs — condicao L62 nao depende mais de "StatusTickedEvent"; comentario explicativo adicionado; checagem de DamageAppliedEvent/StatusAppliedEvent preservada
```

**1 csproj editado localmente para permitir o dotnet build fallback nesta sessao (NAO faz parte do diff commitavel):**

```
Assembly-CSharp.csproj — removidas 14 entradas <Compile Include> dos arquivos deletados + EquipmentUpgradedEvent.cs; re-adicionada a entrada de RewardTableDefinition.cs (restaurado)
```

Nota: `Assembly-CSharp.csproj` esta listado em `.gitignore` (confirmado via `git check-ignore -v`) — e auto-gerado pelo Unity Editor a cada abertura do projeto e nao e rastreado pelo git. A edicao foi necessaria apenas para o `dotnet build` desta sessao (sem Unity Editor aberto) compilar corretamente contra o estado pos-delecao; o Unity Editor regenerara este arquivo automaticamente na proxima abertura, refletindo os arquivos deletados sem qualquer acao adicional.

**Arquivo restaurado (NAO deletado nesta execucao):**

```
Assets/_Game/Scripts/Loot/RewardTableDefinition.cs (+ .meta) — restaurado via git checkout apos build expor uso real de RewardGrantResult (classe auxiliar do mesmo arquivo) em LootTableContractTests.cs:154
```

## Spec Compliance Matrix

| Criterio (secao 14) | Status | Evidencia |
|---|---|---|
| 14.1 15 arquivos removidos, zero referencias | PARCIAL — 14/15 removidos | RewardTableDefinition.cs NAO foi deletado (ver desvio); os outros 14 confirmados sem referencia via Grep + GUID scan antes/depois |
| 14.2 4 eventos removidos, zero publish/subscribe | PASS | PauseClosedEvent, EquipmentUpgradedEvent, SpellCastFailedEvent, StatusTickedEvent removidos; Grep pos-delecao mostra zero ocorrencias fora do comentario explicativo em ValidateSpec11Damage.cs |
| 14.3 ValidateSpec11Damage.cs ajustado | PASS | L62 nao depende mais de "StatusTickedEvent"; logica de DamageAppliedEvent/StatusAppliedEvent preservada; build do Editor PASS |
| 14.4 Build limpo | PASS | Assembly-CSharp.csproj exit 0 (5 warnings pre-existentes, CS0649 em CombatTelemetrySession/EnemySkinCatalog); Assembly-CSharp-Editor.csproj exit 0 (7 warnings pre-existentes) |

## Validation

```text
Validation method: dotnet build (2x) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + check_spec_quality.ps1 rodados individualmente
Assembly-CSharp: PASS (exit 0; 5 warnings pre-existentes CS0649 em CombatTelemetrySession.cs e EnemySkinCatalog.cs, nao relacionados a esta spec)
Assembly-CSharp-Editor: PASS (exit 0; 7 warnings pre-existentes CS0649/UNT0006 em ValidateEnemySkinBindings.cs, CreateEnemyActionsAndSets.cs, CSharpProjectPostprocessor.cs, nao relacionados a esta spec)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (exit 1; erros == spec_npc_physics_cat_companion.md faltando marcadores speckit + tools/codex/Generate-CodexHarness.ps1 placeholders — ambos legados conhecidos, pre-existentes, nao relacionados a esta spec)
check_spec_diff_completeness.ps1: FAIL/WARN (exit 0; WARN esperado "runtime code changed but no test files" — justificado pelo Testing Quality Gate da propria spec, secao 30: remocao de codigo morto sem comportamento observavel, sem teste exigido)
check_spec_quality.ps1: FAIL (exit 1; forbidden-files check aponta Enemies/Roster (35 assets) + 3 cenas (.unity) + NPCs assets alterados PRE-SESSAO por outro trabalho em andamento no repo, fora do escopo desta spec — nao tocados por esta execucao; WARN de secoes ausentes em reports antigos, tambem pre-existente)
run_strict_validation.ps1 (script agregador): CORRUPTION_DETECTED no Step 0 — FALSO POSITIVO ESPERADO. O corruption guard (tools/validate_no_corruption.ps1) verifica arquivos RASTREADOS pelo git que sumiram do disco, sem distinguir delecao intencional de corrupcao real. Os 15 arquivos (30 entradas contando .meta) aparecem como "sumido/ilegivel" porque foram deletados via Remove-Item e ainda NAO foram commitados (o guard compara contra git ls-files, que reflete o ultimo commit). Confirmado via "git status --short": todas as 30 entradas aparecem como " D" (deleted, working tree), nao como corrupcao de filesystem. Os steps subsequentes do script agregador (build, docs, diff completeness, quality) foram rodados manualmente e documentados acima porque o Step 0 aborta o script antes de chegar neles.
Result artifact: nao gerado (LAST_STRICT_VALIDATION_RESULT.json nao foi escrito porque o script agregador abortou no Step 0; evidencia de cada passo documentada manualmente nesta secao)
```

## Honest status rationale

BUILD_VALIDATED, nao ACCEPTED/BUILD_VALIDATED_FULL: os dois builds C# (Assembly-CSharp e Assembly-CSharp-Editor) passam com exit code 0 e zero erros/warnings novos, e o Grep antes/depois confirma zero referencias residuais para os 14 arquivos + 4 eventos efetivamente removidos. O criterio 14.1 nao foi cumprido a 100% (14 de 15 arquivos, nao 15) porque a Fase 0 desta spec (Grep do nome de classe `RewardTableDefinition`) nao detectou que o mesmo arquivo tambem definia `RewardGrantResult`, uma classe auxiliar com nome diferente e com uso real em um teste EditMode ja existente (`LootTableContractTests.cs`). Isso e exatamente o tipo de desvio que o protocolo de re-verificacao desta spec pede para reportar em vez de forcar a delecao: o arquivo foi restaurado via `git checkout`, a entrada correspondente foi devolvida ao `.csproj`, e o build voltou a passar limpo. Nenhum outro arquivo ou evento teve desvio — os 14 arquivos + 4 eventos restantes foram confirmados mortos por Grep de classe, ausencia de RuntimeInitializeOnLoadMethod, e busca de GUID em 1291 arquivos .unity/.prefab/.asset, sem nenhuma referencia encontrada antes ou depois da delecao.

O `run_strict_validation.ps1` agregador nao pode ser citado como "VALIDATION_PASS" porque seu Step 0 (corruption guard) aborta o script inteiro ao ver os arquivos deletados-mas-nao-commitados como "sumido/ilegivel" — comportamento esperado do guard para qualquer delecao de arquivo rastreado que ainda nao foi commitada, nao uma falha real desta spec. Os passos individuais (build x2, docs, diff completeness, quality) foram rodados manualmente e nenhum deles reporta erro novo atribuivel aos arquivos desta spec; os erros/warnings existentes sao os legados ja conhecidos e listados no prompt de execucao (spec_npc_physics_cat_companion, tools/codex/ placeholders, Enemies/Roster + 3 cenas alteradas por outra sessao em paralelo). Apos o commit desta mudanca, o corruption guard nao devera mais reportar esses arquivos como ausentes (eles deixarao de estar "rastreados-mas-ausentes" e passarao a nao existir no proprio git).

Testing Quality Gate (secao 30 da spec): nenhum teste automatizado adicionado, conforme justificado na propria spec — remocao de codigo morto confirmado sem comportamento observavel; nenhum EditMode test cobria os 14 arquivos/4 eventos removidos (confirmado por ausencia de referencia nos arquivos de teste, exceto o caso de RewardGrantResult, que permanece com cobertura de teste intacta pois o arquivo foi restaurado).
