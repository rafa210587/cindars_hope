# SPEC — Caverna: Save Completo da Run (Snapshot Multi-Nível + Política de Save em Boss)

> **Spec ID:** `fable_44_spec_cave_save_completion_policy`
> **Status:** A implementar
> **Wave:** FABLE Batch 9
> **Priority:** P2
> **Type:** Save / Runtime
> **Domain:** Cave / Save
> **Parallelizable:** NO
> **Parallel group:** N/A (save schema lock)
> **Can run with:** specs que não tocam save nem caverna
> **Must not run with:** F07, F12, F21, F26, F42 (seções de save — um por vez), F43 (cave/boss runtime)
> **Repo lock scope:** `Cave/Runtime/**` (save data/mapper/snapshot), SaveManager (fluxo de save manual), CaveLevelRuntimeController
> **Depends on:**
> - `fable_13_spec_save_debt_closure_runtime` (EXECUTADA — BUILD_VALIDATED; base desta spec)
> - `fable_09_spec_cave_biome_layout_variety_runtime` (tamanhos 42×42–65×65 afetam orçamento)
> **Blocks:** N/A
> **Scope:** persistir snapshots de TODOS os níveis visitados da run (com orçamento de tamanho) e bloquear save manual durante boss fight ativo.
> **Out of scope:** mudanças no formato de snapshot além de compressão/cap, autosave novo, save na nuvem, replay validator novo.

required_adrs: [ADR-0005]
required_game_rules: [save_rules.md, cave_rules.md]

---

# /speckit.specify

## Contexto

A F13 (executada, report `docs/validation/fable_13_spec_save_debt_closure_runtime_execution_report.md`)
fechou o débito de save da run da caverna mas com um limite DOCUMENTADO: o
`CaveRunSaveData.CurrentLevelSnapshot` persiste APENAS o snapshot do nível corrente
("snapshot multi-nível: 16_spec futura" — comentário no próprio DTO). O report também
registrou em Remaining work: "snapshot multi-nível no save + política de save em boss".
Esta spec é essa pendência. Com a F09 os níveis oscilam até 65×65, e o snapshot serializa
`WalkableTilesList`/`WallTilesList` por tile — o orçamento de tamanho precisa ser MEDIDO
antes de decidir entre compressão de listas de tiles e cap de N níveis mais recentes.

## Problema

Hoje, ao salvar no nível 8 e voltar ao nível 5, o nível 5 é rematerializado por replay de
seed — correto para layout (ADR-0005), mas TODO estado mutável dos níveis não-correntes
(HP de inimigos, inimigo morto, nós depletados fora da lista global) se perde entre
sessões, violando a expectativa do stable-run percebido pelo jogador. Além disso, salvar
no meio de uma boss fight permite snapshot de estado de boss a meio caminho (fases F05,
arena) que o restore não reconstrói — save corrompível por design. Sem esta spec, a
promessa "a run sobrevive a fechar o jogo" da F13 só vale para um nível.

## Objetivo

Ao final desta spec, `CaveRunSaveData` deve persistir uma lista de `VisitedLevelSnapshot`
(todos os níveis visitados, dentro de um orçamento de tamanho decidido com medição na
Fase 0), o save manual deve ser bloqueado durante boss fight ativo com feedback claro
("não é possível salvar agora"), e o load legado (apenas `CurrentLevelSnapshot`) deve
continuar funcionando — sem alterar `LayoutHash`, replay ou o contrato stable-run.

## Fontes obrigatórias lidas

```text
docs/validation/fable_13_spec_save_debt_closure_runtime_execution_report.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/rules/cave-stable-run.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/cave-stable-run-guard/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CaveRunSaveData + CaveRunSaveMapper (F13): espelho 1:1 do CaveRuntimeState; mapper inclui
  só state.CurrentLevel (teste Mapper_IncludesOnlyCurrentLevelSnapshot fixa o limite atual);
- VisitedLevelSnapshot ([Serializable], WalkableTilesList/WallTilesList por Vector2Int,
  EnemySpawns, ResourceNodeStates, EnemyHpRecords F13 — fora do LayoutHash);
- CaveRuntimeState.VisitedLevelSnapshots (Dictionary<int, VisitedLevelSnapshot> em memória —
  multi-nível JÁ existe em runtime; só o SAVE corta para 1);
- CaveSnapshotService/CaveSnapshotCacheManager, CaveReplayValidator (hash de replay);
- SaveManager (capture/restore padrão WI-18 via cache do bootstrap);
- CaveLevelRuntimeController + CaveBossSpawner (spawn de boss por nível — ponto do flag);
- RefreshCurrentSnapshotEnemyHp/RefreshSnapshotEnemyHpBeforeTransition (F13).
Não existe:
- persistência multi-nível, política de orçamento/compressão/cap, flag de boss fight ativo,
  bloqueio de save manual com feedback.
Auditar Fase 0: tamanho REAL serializado (JsonUtility) de um snapshot 55×55 e 65×65;
como o save manual é disparado (menu/atalho) para inserir o gate num ponto único.
```

## Engineering stories

```text
Como jogador, quero voltar 3 níveis depois de recarregar o jogo e encontrar os inimigos
que matei ainda mortos e os nós que coletei ainda vazios.
Como SaveManager, quero um orçamento explícito para a seção da caverna, não uma lista sem teto.
Como jogador, quero uma mensagem clara quando tentar salvar durante um boss, não um save
silenciosamente corrompido.
Como CaveReplayValidator, quero que LayoutHash e replay continuem idênticos (ADR-0005).
```

## Escopo

```text
Inclui:
- Fase 0 com MEDIÇÃO: serializar snapshots reais (42×42 / 55×55 / 65×65) e registrar KB no
  report; decidir e DOCUMENTAR a política: (a) compressão das listas de tiles (ex.: RLE por
  linhas ou bounds+bitmask em List<int>) se ganho ≥~60%, e/ou (b) cap de N níveis mais
  recentes (LRU por ordem de visita; níveis fora do cap voltam a replay puro de seed) — a
  decisão vira constante nomeada + nota no report;
- CaveRunSaveData.VisitedLevelSnapshots (List<VisitedLevelSnapshot> ADITIVA, ordenada por
  CaveLevel; CurrentLevelSnapshot MANTIDO para compat de load legado e escrito em paralelo);
- CaveRunSaveMapper: ToSaveData inclui todos os snapshots válidos (respeitando o cap),
  determinístico (ordenação estável); FromSaveData restaura o dicionário completo; load
  legado (lista vazia/null) cai no comportamento F13 sem erro;
- política de save em boss: flag IsBossFightActive no CaveLevelRuntimeController (set no
  spawn/engage do boss do nível; clear em derrota do boss, morte do player ou saída do
  nível); gate no ponto único de save MANUAL do SaveManager → recusa + feedback UI
  "não é possível salvar agora" (toast/mensagem existente; sem UI nova); saves automáticos
  de fluxo (dormir etc.) não ocorrem dentro da caverna — confirmar na Fase 0;
- teste de tamanho de save: teste EditMode que serializa run sintética com K níveis no cap
  e assert de orçamento (bytes < limite documentado);
- EditMode tests: round-trip multi-nível (3 níveis com HP/depleted distintos), cap LRU
  determinístico, load legado só com CurrentLevelSnapshot, compressão reversível
  (se adotada — propriedade encode/decode), flag de boss (set/clear) e gate de save.
```

## Fora de escopo

```text
Não inclui: mudar geração/replay/LayoutHash; novo sistema de autosave; persistir estado de
FASE de boss (bloquear save em boss elimina a necessidade); compressão do resto do save;
UI nova de save.
```

## Regras de não duplicação

```text
Não criar segundo snapshot/DTO — estender VisitedLevelSnapshot/CaveRunSaveData existentes
com campos/lista aditivos.
Não criar segundo mapper — evoluir CaveRunSaveMapper (testes F13 atualizados, não deletados).
Não criar segundo canal cena↔save — manter cache do bootstrap (padrão F13).
Não criar segundo ponto de decisão de save — gate no fluxo manual único do SaveManager.
```

## Critérios de aceite

### CA-1 Multi-nível persistido
- Run com níveis 1-3 visitados (inimigo morto no 1, nó depletado no 2) → save/load →
  os 3 snapshots restaurados; revisitar mantém mortos/depletados.
- Evidência: teste round-trip multi-nível + cenário humano.

### CA-2 Orçamento de tamanho
- Medição da Fase 0 registrada no report (KB por tamanho de nível); política
  (compressão e/ou cap N) implementada com constante nomeada; teste de orçamento passa.
- Evidência: report §medição + SaveSizeBudgetTest.

### CA-3 Save bloqueado em boss
- Save manual durante boss fight ativo é recusado com mensagem; após derrotar o boss
  (ou sair do nível), save volta a funcionar.
- Evidência: testes do flag/gate + cenário humano.

### CA-4 Compatibilidade e stable-run
- Save F13 (só CurrentLevelSnapshot) carrega sem erro; LayoutHash/replay inalterados
  (testes F13 de hash continuam passando).
- Evidência: teste de load legado + suite de replay existente verde.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Runtime/
  CaveRunSaveData.cs        (lista aditiva VisitedLevelSnapshots + constantes de orçamento)
  CaveRunSaveMapper.cs      (multi-nível + cap LRU determinístico)
  CaveSnapshotTileCodec.cs  (NOVO — apenas se compressão for adotada na Fase 0; puro)
Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs (flag IsBossFightActive)
Assets/_Game/Scripts/Save/SaveManager.cs (gate de save manual — cirúrgico)
Assets/_Game/Tests/EditMode/Cave/CaveMultiLevelSaveTests.cs (+ atualização SaveDebtClosureTests)
docs/validation/fable_44_spec_cave_save_completion_policy_execution_report.md
```

## Contratos

### Data contracts
`CaveRunSaveData.VisitedLevelSnapshots: List<VisitedLevelSnapshot>` (aditivo, ordenado por
CaveLevel); constantes `MaxPersistedLevelSnapshots` e `SaveSizeBudgetBytes` documentadas.
### Runtime contracts
`CaveLevelRuntimeController.IsBossFightActive` (bool, leitura pelo SaveManager via canal
existente do bootstrap — sem GameObject.Find); codec puro encode/decode se adotado.
### Event contracts
N/A novos (feedback de recusa usa canal de toast/mensagem existente; se inexistente,
`SaveBlockedEvent(string reason)` mínimo — decidir Fase 0).
### Save contracts
Campos aditivos; sem bump de SchemaVersion; load legado com lista vazia = comportamento F13;
sem refs Unity; Vector2Int já serializado hoje (mantido ou comprimido em List<int> — codec).
### UI contracts
Mensagem "Não é possível salvar agora." no canal de feedback existente. Sem tela nova.

## Sistemas afetados

```text
Save/load (seção cave), Cave runtime (controller/materializer leitura), Boss flow,
feedback de UI mínimo, testes EditMode existentes da F13.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Runtime/{CaveRunSaveData,CaveRunSaveMapper,CaveSnapshotTileCodec}.cs
Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs
Assets/_Game/Scripts/Save/SaveManager.cs (gate apenas)
Assets/_Game/Tests/EditMode/Cave/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manuais ; Packages/** ; ProjectSettings/**
CaveSnapshotService/CaveReplayValidator (hash/replay intocados — só leitura)
Geradores procedurais (layout/seed) ; VisitedLevelSnapshot.LayoutHash (qualquer mudança)
```

## Estratégia de implementação

```md
### Fase 0 — Medição de tamanho (42/55/65) + auditoria do fluxo de save manual e autosaves; DECISÃO compressão×cap registrada.
### Fase 1 — Lista aditiva no DTO + mapper multi-nível + cap LRU + (codec se decidido) + testes.
### Fase 2 — Flag IsBossFightActive + gate no save manual + feedback.
### Fase 3 — Teste de orçamento + atualização dos testes F13 + load legado.
### Fase 4 — run_strict_validation + execution report (com a medição) + cenário humano.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs sem save/cave
- Must not run with: F07, F12, F21, F26, F42, F43
- Shared files/systems that require lock: GameSaveData/SaveManager, Cave/Runtime
- Reason: altera save schema (aditivo) e runtime controller da caverna.

## Impacto em save/load

```text
Does this change save schema? YES (lista aditiva na seção cave existente)
Does this add a save section? NO (estende CaveRunSaveData)
Does this require migration? NO (aditivo; legado = lista vazia → comportamento F13)
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: CONDITIONAL (SaveBlockedEvent mínimo se não houver canal de feedback — Fase 0)
Changes existing events: NO | Requires unsubscribe pattern: NO
```

## Impacto em UI/Unity

```text
Changes UI: mínimo (mensagem de recusa em canal existente) | Changes scenes/prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (save em boss + reload multi-nível)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: save inchar (65×65 ≈ 4225 tiles × N níveis). Mitigação: medição obrigatória Fase 0 +
cap LRU + teste de orçamento com limite explícito.
Risco: compressão quebrar replay/restore. Mitigação: codec puro com teste de propriedade
encode(decode(x))==x; LayoutHash calculado ANTES do codec (inalterado).
Risco: flag de boss órfão (nunca limpo) travar save para sempre. Mitigação: clear em TODOS
os caminhos (derrota, morte, saída de nível, saída da caverna) + teste.
Risco: regressão nos testes F13. Mitigação: atualizar Mapper_IncludesOnlyCurrentLevelSnapshot
para o novo contrato documentando a mudança (não deletar cobertura).
```

## Rollback

```text
Lista aditiva ignorada por builds antigos; remover gate devolve o save manual anterior;
CurrentLevelSnapshot continua escrito (compat F13). Nenhum save real do usuário apagado.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: medir snapshot serializado (42/55/65), auditar fluxo de save manual; decidir compressão×cap (registrar).
- [ ] T002 — Lista VisitedLevelSnapshots aditiva + mapper multi-nível + cap LRU + testes round-trip/legado.
- [ ] T003 — CaveSnapshotTileCodec (se decidido) + teste de propriedade encode/decode.
- [ ] T004 — IsBossFightActive (set/clear em todos os caminhos) + gate de save manual + feedback.
- [ ] T005 — SaveSizeBudgetTest + atualização SaveDebtClosureTests; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (mapper, cap, codec, gate)
- Requires EditMode tests: YES (round-trip multi-nível, cap, codec, orçamento, flag/gate, legado)
- Requires PlayMode automated or final human scenario: YES (reload multi-nível + tentativa de save em boss)
- Requires regression test: YES (suite F13 + replay hash intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano salvando no
  nível 3, recarregando e revisitando níveis 1-2 com estado preservado

## Definition of Done

```text
Snapshots multi-nível persistidos dentro do orçamento medido e documentado; save manual
bloqueado em boss com feedback; load legado F13 OK; LayoutHash/replay inalterados;
builds 0E; report com medição de tamanho; sem claim ACCEPTED.
```

## Anti-regressão

```text
LayoutHash e replay determinístico intocados (ADR-0005); CaveRunSeed nunca muda por save/load;
EnemyHpRecords continuam fora do hash; load de save F13 sem erro; save fora de boss nunca
bloqueado; nenhum Unity ref em DTO.
```

## Notas para execução posterior

```text
Se a medição da Fase 0 mostrar que compressão é desnecessária com cap N=8 (ex.: <200KB),
adotar só o cap e registrar a decisão — não implementar codec especulativo.
```
