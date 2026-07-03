# Execution Report — spec_codex_07_boot_smoke_editmode

> **Spec:** `.specs/a_implementar/spec_codex_07_boot_smoke_editmode.md`
> **Status:** BUILD_VALIDATED
> **Type:** Validation / Testing (EditMode smoke, no Play Mode)
> **Date:** 2026-07-03

---

## Acceptance criteria extracted

Da seção "14. Critérios de aceite" da spec:

| # | Critério | Evidência |
|---|---|---|
| 14.1 | Consistência campo↔propriedade: para todo `[SerializeField]` de tipo manager/database em `GameBootstrap`, existe uma propriedade pública do mesmo tipo; evidência = teste EditMode passando, citando quantos campos foram verificados | `GameBootstrapWiringSmokeTests.EveryPrivateManagerField_HasCorrespondingPublicProperty` — verifica via reflection todos os `[SerializeField]` privados de `GameBootstrap` (exceto `_playerData`, documentado como exclusão intencional — ver seção "Existing systems audit"); loga a contagem verificada via `TestContext.WriteLine` |
| 14.2 | Checklist da skill `boot-integration-smoke` preenchido no execution report, SIM/NÃO APLICÁVEL justificado linha a linha | Ver seção "Checklist da skill boot-integration-smoke" abaixo |
| 14.3 | Sem falso claim de Play Mode — nenhum teste ou doc afirma "Play Mode validado" | Confirmado: nenhuma menção a "Play Mode validado"/`PLAYMODE_VALIDATED` neste report; teste e comentários rotulam explicitamente o smoke como sinal estático, não prova de gameplay |

---

## Existing systems audit (Phase 0)

Leitura completa de `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` (479 linhas) nesta sessão, confirmando/expandindo a auditoria já embutida na seção 9 da spec:

- `GameBootstrap` declara **24 campos `[SerializeField]` privados** (L29-53): `_playerManager`, `_inventoryManager`, `_timeManager`, `_gameTimeManager`, `_modalManager`, `_saveManager`, `_hungerManager`, `_staminaManager`, `_craftingManager`, `_economyManager`, `_shopManager`, `_equipmentManager`, `_progressionManager`, `_statusEffectManager`, `_playerData`, `_itemDatabase`, `_weaponDatabase`, `_spellDatabase`, `_statusEffectDatabase`, `_skillActionDatabase`, `_manaManager`, `_caveRunManager`, `_anyaFountain`, `_skillTreeManager`, `_bestiaryManager`.
  - Correção contada: são 25 campos, não 24 (a auditoria da spec citava "24" mas parou de listar antes do fim — `_bestiaryManager` é o 25º). O teste conta dinamicamente via reflection, então esse número exato não é hardcoded no código, só documentado aqui para registro.
- Todos os 25 campos, **exceto `_playerData`**, têm propriedade pública `=>` correspondente (L60-84): `PlayerManager`, `InventoryManager`, `TimeManager`, `GameTimeManager`, `ModalManager`, `SaveManager`, `HungerManager`, `StaminaManager`, `ManaManager`, `CraftingManager`, `EconomyManager`, `ShopManager`, `EquipmentManager`, `PlayerProgressionManager` (não `ProgressionManager` — mismatch de nome deliberado, ver abaixo), `StatusEffectManager`, `CaveRunManager`, `AnyaFountain`, `SkillTreeManager`, `BestiaryManager`, `ItemDatabase`, `WeaponDatabase`, `SpellDatabase`, `StatusEffectDatabase`, `SkillActionDatabase`.
- `_playerData` (`PlayerDataSO`) **não tem propriedade pública equivalente** — é consumido internamente em `InitializeManagers()`/`EquipStarterCombatLoadout()` (ex.: L139, L156, L204, L226) mas nunca exposto para outros sistemas lerem via `GameBootstrap.Instance.PlayerData`. Confirmado por leitura completa do arquivo (nenhuma ocorrência de `public ... PlayerData`). Isto é uma exclusão real do padrão 1:1, documentada explicitamente no teste (constante `IsExcludedFromWiringContract`) em vez de silenciosamente ignorada.
- `_progressionManager` → `PlayerProgressionManager` (L41, L73) é o **known mismatch** já previsto na seção "26. Riscos técnicos" da spec (campo `_fooManager` mapeado para propriedade que não é `Foo` puro). Coberto por um teste dedicado (`KnownMismatches_AreExplicitlyDocumented_AndStillResolve`) que confirma explicitamente esse caso, em vez de deixá-lo passar silenciosamente pela heurística de nome genérica.
- `CorpseRecoveryManager` (L56, L76) **não é `[SerializeField]`** — é um campo privado comum, resolvido em código (`InitializeDeathSystem()`, L395-406), exatamente como a spec já previu ("provavelmente resolvido em código, auditar" — confirmado). Corretamente fora do escopo do teste de reflection (que só itera `[SerializeField]`).
- Não existe, no `GameBootstrap.cs` real, nenhuma regra condicional de "qual manager depende de qual" extraível para método puro — o wiring em `InitializeManagers()` é sequencial e direto (checagens `if (_x != null)` seguidas de chamadas), sem lógica de resolução computável isolável além do que já é 1:1 field↔property. Confirma a hipótese da spec (seção 11, "Inclui"): nenhuma lógica nova foi inventada; a Fase 1 (reflection) já é o entregável central.
- Confirmado (Grep): zero arquivos `Assets/_Game/Tests/EditMode/**` com "Boot" no nome antes desta spec.
- Nenhuma das 10 classes deletadas por `spec_codex_06_dead_code_removal_batch1` (`CaveDebugVisualizer`, `EnemyPatrolController`, `TargetVulnerabilityState`, `EnvironmentalExposureManager`, `PlayerWeaponController`, `EquipmentHUD`, `ManaHUD`, `PlayerNeedsHUD`, `PlayerStatusHUD`, `PlayerDodgeController` de `Player/`, `DeathScreenController`) é referenciada por `GameBootstrap.cs` nem pelo teste novo — confirmado por leitura completa do arquivo de produção e do teste escrito.

Nenhum sistema paralelo criado. Nenhuma edição em `GameBootstrap.cs` (arquivo proibido pela spec, seção 19) — apenas leitura via reflection.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|---|---|---|
| Teste de reflection campo↔propriedade (11. Escopo) | `GameBootstrapWiringSmokeTests.EveryPrivateManagerField_HasCorrespondingPublicProperty` | OK |
| Teste `Instance == null` fora de Play Mode (11. Escopo) | `GameBootstrapWiringSmokeTests.Instance_IsNull_OutsidePlayMode` | OK |
| Allowlist de exceções à convenção 1:1 (26. Riscos técnicos) | `KnownMismatchFieldNames` (`_progressionManager`) + exclusão documentada de `_playerData` (`IsExcludedFromWiringContract`); teste dedicado `KnownMismatches_AreExplicitlyDocumented_AndStillResolve` | OK |
| Nenhuma lógica de wiring nova inventada (11. Escopo, "se não houver tal lógica... documentar") | Documentado nesta seção: `InitializeManagers()` não tem regra condicional extraível | OK |
| Checklist da skill preenchido (14.2) | Ver seção dedicada abaixo | OK |
| Sem edição de `GameBootstrap.cs` / `*RuntimeBootstrap.cs` (19. Arquivos proibidos) | Confirmado — apenas leitura | PRESERVED |
| Sem `FindObjectOfType` no teste (32. Anti-regressão) | Confirmado por leitura do arquivo novo — só `System.Reflection` e `GameBootstrap.Instance` | PRESERVED |
| Sem claim de `PLAYMODE_VALIDATED` (32. Anti-regressão) | Confirmado — status deste report é `BUILD_VALIDATED`; nenhuma menção de Play Mode validado em código/report | PRESERVED |
| Regra de não duplicação (13.) — seguir exatamente o procedimento da skill, não recriar `GameBootstrap` | Confirmado — teste usa reflection sobre a classe real, nenhum bootstrap paralelo | PRESERVED |

---

## Files changed

- `Assets/_Game/Tests/EditMode/Boot/GameBootstrapWiringSmokeTests.cs` (novo) — 3 testes EditMode: `EveryPrivateManagerField_HasCorrespondingPublicProperty` (reflection sobre todos os `[SerializeField]` de `GameBootstrap`, com exclusão documentada de `_playerData`), `KnownMismatches_AreExplicitlyDocumented_AndStillResolve` (confirma o mismatch `_progressionManager`→`PlayerProgressionManager`), `Instance_IsNull_OutsidePlayMode` (documenta a limitação de `Instance` fora de Play Mode). Namespace `CindarsHope.Tests.EditMode.Boot`, seguindo a convenção NUnit já usada no projeto.
- `Assembly-CSharp.csproj` (modificado) — uma linha `<Compile Include>` adicionada para o novo arquivo de teste, seguindo a convenção já usada para os demais arquivos de `Tests/EditMode/**` (inserida ao lado de `VitalsApplicationTests.cs`).
- `docs/validation/spec_codex_07_boot_smoke_editmode_execution_report.md` (este arquivo, novo).

Nenhuma edição em `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` nem em qualquer `*RuntimeBootstrap.cs` (arquivos proibidos, só lidos).

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1 (falhas pré-existentes, nenhuma relacionada a esta spec — ver abaixo)
Assembly-CSharp: PASS (exit 0, 0 erros, 0 avisos)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros novos; 7 avisos pré-existentes não relacionados — CS0649/UNT0006 em ValidateEnemySkinBindings.cs, CreateEnemyActionsAndSets.cs, CSharpProjectPostprocessor.cs, arquivos não tocados por esta spec)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY — falhas pré-existentes: spec_npc_physics_cat_companion.md sem markers/headers (não tocado); placeholders em tools/codex/Generate-CodexHarness.ps1 (não tocado); reports antigos sem seções obrigatórias (05_spec_farm_*, 01_spec_*, 02_spec_*, etc. — todos anteriores a esta sessão, não tocados por esta spec)
Quality check: FAIL — "Forbidden files altered": Assets/_Game/Data/Enemies/Roster/*.asset (36 arquivos) + Assets/_Game/Scenes/{CaveScene,FarmScene,TownScene}.unity — TODOS modificados ANTES desta sessão (confirmados pré-existentes no git status de entrada da sessão; nenhum destes arquivos foi tocado por esta spec, que só cria arquivos novos sob Assets/_Game/Tests/EditMode/Boot/** e docs/validation/**)
```

Confirmado (busca por "Boot", "GameBootstrap", "spec_codex_07" no output completo do `run_strict_validation.ps1`): **nenhuma** falha do script menciona `GameBootstrapWiringSmokeTests.cs`, `GameBootstrap.cs`, ou este execution report. Todas as falhas remanescentes (docs legacy + quality check "forbidden files") são pré-existentes e fora do escopo/arquivos tocados por esta spec, exatamente como sinalizado pelo orquestrador antes da execução (assets `Enemies/Roster` + 3 cenas `.unity` modificados pré-sessão).

EditMode Test Runner (Unity): **NOT RUN** — sem Unity Editor interativo disponível nesta sessão (ambiente CLI/batchmode). Os 3 testes novos **compilam** (build PASS confirma compilação via `dotnet build`, não execução real via NUnit/Test Runner). Escritos seguindo a convenção do projeto (`[Test]` NUnit, namespace `CindarsHope.Tests.EditMode.Boot`), consistente com o padrão de todos os demais arquivos em `Assets/_Game/Tests/EditMode/**`, cuja execução real também é `NOT RUN` nesta sessão pelo mesmo motivo ambiental.

---

## Checklist da skill boot-integration-smoke

Da seção "Saída esperada" de `.claude/skills/boot-integration-smoke/SKILL.md`:

```text
Contrato de evento testado em EditMode (publish -> subscriber reage): NAO APLICAVEL
  — esta spec explicitamente não cobre contrato de evento (seção 11 "Fora": "Testar contrato de
  evento (GameEventBus publish→subscribe) — nenhum evento específico foi pedido nesta spec").

Logica de wiring extraida e testada (pura): NAO APLICAVEL
  — auditado nesta sessão (leitura completa de GameBootstrap.cs): InitializeManagers() não contém
  regra condicional de "qual manager depende de qual" isolável em método puro; é wiring sequencial
  direto (checagens if (_x != null) + chamada), já coberto pelo teste de reflection campo↔propriedade.
  Documentado em vez de inventar lógica nova, conforme a spec autoriza (seção 11).

Ids do bootstrap resolvem (sem null) — testado: NAO APLICAVEL
  — GameBootstrap não expõe um catálogo/registry de IDs de domínio resolvíveis (ex.: item_/npc_/quest_);
  os campos verificados são referências de manager/database (tipos), não lookups por ID string.
  Validação de IDs de catálogo já é coberta por specs/testes dedicados (ex.: StableIdsValidationTests.cs),
  fora do escopo desta spec.

Validador de cena para serialized refs do GameBootstrap: NAO APLICAVEL
  — explicitamente fora de escopo (seção 11 "Fora": "Validador de cena (Editor/batchmode)... mencionado
  como follow-up natural, não implementado aqui"). Listado como próximo passo natural na seção 33 da spec.

Residual real (Play Mode) listado no cenario humano + ledger: SIM
  — ver seção "What was NOT done" abaixo: injeção real de serialized refs nas 3 cenas, self-wiring de
  RuntimeInitializeOnLoadMethod, e qualquer comportamento de lifecycle seguem sem prova automatizada;
  documentado como residual explícito, não como PASS.

Smoke NAO declarado como PLAYMODE_VALIDATED: SIM
  — status deste report é BUILD_VALIDATED; nenhuma menção de "Play Mode validado" em código ou neste
  documento.
```

---

## Testing Quality Gate

- Changed runtime code: NO (nenhuma alteração em `Assets/_Game/Scripts/**`; apenas leitura de `GameBootstrap.cs`)
- Changed deterministic logic: NO (teste novo, sem lógica de produção nova — conforme a spec já antecipava na seção 30)
- Requires EditMode tests: YES — é o próprio entregável desta spec (`GameBootstrapWiringSmokeTests.cs`)
- Requires PlayMode automated or final human scenario: NO — a spec classifica isto explicitamente como smoke, não substituto de Play Mode (seção 30: "Requires PlayMode automated or final human scenario: NO")
- Requires regression test: NO
- Automated tests command: `dotnet build .\Assembly-CSharp.csproj --no-restore` (compila; execução real via Unity Test Runner NOT RUN — ver seção Validation acima)
- Manual Play Mode scenario: NOT REQUIRED (conforme seção 25 da spec: "Human validation timing: NOT REQUIRED")
- Justification if no tests: N/A — testes foram criados
- Residual risk: baixo. O smoke prova a **estrutura** do contrato campo↔propriedade (o caso real de regressão: alguém adiciona um manager novo e esquece a propriedade pública, ou vice-versa), mas não prova que os `[SerializeField]` estão de fato **preenchidos** nas cenas reais (`TownScene`/`FarmScene`/`CaveScene`) — isso é explicitamente fora de escopo (validador de cena, follow-up natural) e permanece dependente de Play Mode/inspeção manual no Editor.

---

## Honest status rationale

- **BUILD_VALIDATED** (não `ACCEPTED`/`UNITY_VALIDATED`/`PLAYMODE_VALIDATED`): `dotnet build` passou para ambos os assemblies (0 erros, 0 avisos novos) e os 3 testes novos compilam corretamente, mas:
  - O Unity Test Runner (EditMode) não foi executado nesta sessão (sem Unity Editor interativo disponível) — os testes compilam mas não foram confirmados PASS em runtime NUnit real. Reportado como `NOT RUN`, não como PASS, em conformidade com a rule `validation-truth`.
  - `run_strict_validation.ps1` retornou exit code 1, mas **nenhuma** das falhas (docs legacy + quality check "forbidden files") cita qualquer arquivo tocado por esta spec — todas são pré-existentes, confirmadas no `git status` de entrada da sessão (assets `Enemies/Roster/*.asset` + 3 cenas `.unity` já modificados antes desta spec começar). Isto é consistente com a orientação explícita do orquestrador para esta tarefa.
  - Nenhum arquivo `.unity`/`.prefab`/`.asset` foi tocado por esta spec.
  - A spec explicitamente rotula este trabalho como "smoke" — sinal barato, não prova de gameplay — e este report nunca eleva o resultado a `PLAYMODE_VALIDATED`, conforme exigido pela seção 32 (Anti-regressão) e pela rule `validation-truth`.

---

## What was NOT done (explicit)

- **Validador de cena (Editor/batchmode)** que confirme que os `[SerializeField]` de `GameBootstrap` estão de fato preenchidos nas cenas reais (`TownScene`/`FarmScene`/`CaveScene`) — explicitamente fora de escopo (seção 12/78 da spec), listado como follow-up natural.
- **Teste de `*RuntimeBootstrap` (`RuntimeInitializeOnLoadMethod`)** — só roda em Play Mode; fora de escopo, documentado como residual (seção 11 "Fora").
- **Teste de contrato de evento (`GameEventBus` publish→subscriber)** — nenhum evento específico foi pedido nesta spec; escopo novo se uma spec futura quiser isso (seção 11 "Fora").
- **Execução real do Unity Test Runner (EditMode)** — `NOT RUN`, motivo: sem Unity Editor interativo disponível nesta sessão. Documentado como residual risk.
- **Qualquer alteração em `GameBootstrap.cs`** — proibido pela spec; nenhuma feita.
- **Correção das falhas pré-existentes** (assets `Enemies/Roster`, 3 cenas `.unity`, docs legacy) — fora do scope desta spec (arquivos não listados nos "Arquivos permitidos" da seção 18); não corrigidos, conforme instrução do orquestrador de listar apenas falhas que citem arquivos tocados por esta sessão (nenhuma existe).

---

## Rollback

```text
Remover Assets/_Game/Tests/EditMode/Boot/GameBootstrapWiringSmokeTests.cs (e a pasta Boot/ se vazia).
Reverter a linha adicionada em Assembly-CSharp.csproj.
Remover este execution report.
```
