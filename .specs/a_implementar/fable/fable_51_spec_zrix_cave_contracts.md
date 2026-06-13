# SPEC — Contratos de Caverna do Zrix: Quadro na Entrada, Marcos de Profundidade e Desafios Semanais (8 cc_*)

> **Spec ID:** `fable_51_spec_zrix_cave_contracts`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P1
> **Type:** Runtime / Content / Integration
> **Domain:** Quests / Cave
> **Parallelizable:** NO (cadeia quest — QuestRegistry lock)
> **Parallel group:** N/A (ordem na cadeia quest: após F34; nunca junto de F35/F36/F52/F53)
> **Can run with:** F48/F49 (combate/itens — superfícies disjuntas)
> **Must not run with:** F10, F34, F35, F36, F43, F52, F53 (cadeia quest), F09/F44 (caverna runtime)
> **Repo lock scope:** QuestRegistry/QuestManager, QuestBoardService/instâncias dinâmicas (F34), gerador de cena da fazenda (entrada da caverna), eventos de profundidade/boss
> **Depends on:**
> - `fable_34_spec_quest_sources_infrastructure` (E38 — canais/instância dinâmica/recompensa escalada; EMENDA OBRIGATÓRIA: incluir `QuestSource.CaveContract` no enum — esta spec cita e consome a emenda)
> - `fable_09_spec_cave_biome_layout_variety_runtime` (E27 — níveis/bandas estáveis p/ marcos de profundidade)
> - `fable_05_spec_cave_boss_phase_ai_runtime` (E14 — bosses de gate p/ rematch)
> **Blocks:** conteúdo de contrato adicional pós-v1; títulos/charms de desafio futuros
> **Scope:** quadro/NPC do Zrix na entrada da caverna (interactable via gerador), os 8 contratos cc_* do catálogo (6 marcos de profundidade 1× + 2 desafios semanais dinâmicos via padrão board da F34), recompensas do catálogo.
> **Out of scope:** secretas da caverna (F52), quadro de avisos da praça (F34), loja do Zrix, mapa visual rico do "mapa do trecho" (entrega como item/flag; visual fica com F38 minimapa).

required_adrs: [ADR-0005-cave-stable-run-and-replay.md, ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [cave_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

O QUEST_CATALOG define cinco fontes de quest (decisão Q6.1); a 4ª é o canal próprio da
caverna: "CONTRATOS DE CAVERNA — quadro do Zrix na entrada da caverna: marcos de
profundidade e desafios de run". O §11 fecha o conteúdo v1 em 8 contratos `cc_*`:

```text
cc_depth_5 / _15 / _30 / _50 / _70 / _90 — "First to depth N" (marcos 1×; XP alto + mapa do trecho).
cc_boss_rematch (semanal) — re-derrotar um boss de gate: essência garantida.
cc_no_hit_floor (semanal) — limpar 1 nível sem tomar dano: título + charm.
```

A F34 constrói a infraestrutura dos canais (QuestSource, QuestInstance dinâmica,
recompensa escalada com XP por nível, board com rotação determinística) — mas o enum
inicial dela ({Board, Npc, Mural, CaveSecret, Main}) não contemplava contratos de caverna:
**a F34 será emendada para incluir `QuestSource.CaveContract`**, e esta spec é a
consumidora dessa emenda (citar a emenda na execução; se a emenda ainda não estiver
aplicada quando esta spec rodar, aplicá-la como primeiro passo é dependência same-wave
resolvível, não bloqueio). O Zrix já existe como NPC do roster da cidade (draconato
civilizado — FABLE_DECISOES §1); a entrada da caverna fica na FarmScene
(CreateMvpFarmScene), onde o quadro físico deve nascer via gerador.

## Problema

Sem esta spec, a caverna — o coração do jogo — não tem canal próprio de objetivos: nenhuma
recompensa por avançar profundidade pela primeira vez, nenhuma razão sistêmica para
re-enfrentar bosses de gate (essências da Têmpera F22 ficam só no drop aleatório), nenhum
desafio de maestria (no-hit). A 4ª fonte do catálogo fica morta e o total v1 de ~86 quests
perde 8.

## Objetivo

Ao final desta spec, o jogador deve poder interagir com o quadro do Zrix na entrada da
caverna e: aceitar os marcos de profundidade disponíveis (1× cada, concluídos
automaticamente ao alcançar o nível N pela primeira vez com a quest ativa); aceitar o
desafio semanal de boss rematch (alvo determinístico por semana entre os gates JÁ
derrotados) e o de no-hit floor (limpar 1 nível elegível sem tomar dano) — tudo entrando
no fluxo único de quest da F34 (source CaveContract, instâncias dinâmicas persistidas,
recompensa escalada), com rotação semanal determinística e zero segundo sistema de quest.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md (§1 fontes, §2 recompensas, §11 quadro do Zrix)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§1 Zrix draconato; §6 recompensas escaladas)
.specs/a_implementar/fable/fable_34_spec_quest_sources_infrastructure.md (+ emenda CaveContract)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (pós-F34):
- QuestRegistry/QuestManager (aceite/progresso/recompensa/save) + QuestFlagService;
- QuestSource + QuestInstance dinâmica + recompensa escalada (ponto único) + save aditivo
  de instâncias (F34); padrão board com rotação determinística (QuestBoardService);
- CaveLevelRuntimeController / portais (profundidade corrente), bosses de gate (F05),
  registro de gates derrotados (auditar shape);
- Zrix no roster de NPCs (TownNpcDialogueLibrary/NpcTownRosterRegistry);
- CreateMvpFarmScene (gerador — entrada da caverna na fazenda);
- DayStartedEvent/calendário (semana derivável do dia).
Não existe:
- QuestSource.CaveContract (emenda F34 — citar/aplicar);
- quadro do Zrix como interactable na entrada da caverna;
- os 8 cc_* (definições/templates) e suas mecânicas de progresso (profundidade alcançada,
  rematch de boss, no-hit por nível);
- rastreio de "dano tomado neste nível" para no-hit.
Auditar Fase 0:
- como a profundidade máxima alcançada é registrada (evento? save?) — reusar se existir;
- shape do registro de bosses de gate derrotados (F36/cave save) p/ elegibilidade de rematch;
- evento de dano ao player (existente no combate) p/ flag no-hit por nível;
- definição de "semana" no calendário (dia % 7? estação?) — usar regra canônica existente.
```

## Engineering stories

```text
Como jogador, quero recompensa clara na primeira vez que alcanço a profundidade 15/30/50...
Como jogador, quero um motivo semanal para voltar a um boss antigo (essência garantida).
Como jogador, quero provar maestria limpando um nível sem tomar dano — e ganhar o título.
Como QuestManager, quero contratos de caverna no MESMO fluxo de aceite/progresso/recompensa.
Como Zrix, quero meu quadro na boca da caverna — quem desce, assina; quem volta, recebe.
```

## Escopo

```text
Inclui:
- emenda F34 consumida: QuestSource.CaveContract no enum (se ausente, aplicar como passo 0
  — campo aditivo, sem renumerar valores existentes);
- interactable Board_Zrix na entrada da caverna (FarmScene via gerador CreateMvpFarmScene;
  prompt/diálogo curto do Zrix; abre a lista de contratos — projection F34, sem UI nova);
- 6 marcos 1×: cc_depth_5/_15/_30/_50/_70/_90 — quest fixa por marco (IDs canônicos);
  objetivo = alcançar CaveLevel N com a quest ativa; conclusão automática no evento de
  mudança de nível; recompensa: XP alto (fórmula escalada F34, QuestLevel = N) + item/flag
  "mapa do trecho" (map_segment_<faixa> — flag consumível pelo F38 futuramente);
- cc_boss_rematch (semanal, dinâmica): alvo = boss de gate JÁ derrotado, escolhido
  deterministicamente por StableHash(worldSeed|semana) entre os elegíveis; objetivo =
  derrotá-lo de novo na semana; recompensa: essência garantida da banda do boss (item F32);
  sem boss derrotado ainda = contrato indisponível com texto claro;
- cc_no_hit_floor (semanal, dinâmica): alvo = limpar 1 nível elegível (banda atual do
  jogador, determinístico por semana) sem tomar dano; rastreio: flag por nível zerada na
  entrada, suja em qualquer dano recebido, avaliada ao limpar; recompensa: título (flag
  estável title_no_hit_<n>) + charm (acessório F32/F23);
- rotação semanal determinística: instâncias novas a cada semana via padrão board F34
  (DayStartedEvent → se semana mudou, regenerar os 2 semanais; aceitos em andamento não
  são revogados — regra de board F34);
- save: instâncias dinâmicas e flags pelo MESMO mecanismo aditivo F34 (parâmetros simples);
  marcos 1× = quests fixas normais persistidas pelo fluxo existente;
- EditMode tests: rotação semanal determinística (mesma semana = mesmo alvo), elegibilidade
  de rematch (só gates derrotados), conclusão de marco por evento de profundidade,
  máquina no-hit (dano suja; limpar sem dano completa), idempotência dos marcos 1×,
  round-trip de save das instâncias.
```

## Fora de escopo

```text
Não inclui:
- secretas da caverna scq_* (F52) e quadro da praça/mural (F34);
- loja/serviços do Zrix (F25 serviços únicos);
- minimapa/visual do "mapa do trecho" (F38 — aqui é item/flag);
- balanço fino de recompensas além da fórmula escalada (playtest);
- UI nova (lista de contratos usa a projection/painel do board F34).
```

## Regras de não duplicação

```text
Não criar segundo board — reusar o padrão de rotação/instância do QuestBoardService (F34);
o quadro do Zrix é outro PONTO DE ACESSO com outra fonte (CaveContract), não outro sistema.
Não criar segundo rastreador de profundidade — reusar o registro existente (Fase 0).
Não duplicar fórmula de recompensa — ponto único F34.
Não recriar registro de boss derrotado — consumir o existente (F36/cave save).
IDs canônicos cc_* do catálogo — nunca renomear.
```

## Critérios de aceite

### CA-1 Quadro do Zrix funcional

- Interactable na entrada da caverna lista contratos disponíveis (source CaveContract) e
  permite aceite/entrega pelo fluxo F34.
- Evidência: gerador atualizado + cenário humano; EditMode test do filtro por source.

### CA-2 Marcos de profundidade 1×

- cc_depth_N completa automaticamente ao alcançar o nível N com a quest ativa; premia 1×
  (re-alcançar/reload não duplica); recompensa = XP escalado + map_segment flag/item.
- Evidência: EditMode tests (conclusão por evento sintético + idempotência com save/load).

### CA-3 Rematch semanal determinístico

- Mesmo worldSeed + mesma semana = mesmo boss alvo, sempre entre gates já derrotados;
  semana seguinte rotaciona; sem gates derrotados = contrato indisponível.
- Evidência: EditMode tests da seleção por seed/semana e da elegibilidade.

### CA-4 No-hit honesto

- Qualquer dano recebido no nível invalida o no-hit daquele nível; limpar sem dano com o
  contrato ativo completa e premia título + charm.
- Evidência: EditMode tests da máquina de flag (entrada zera; dano suja; clear avalia).

### CA-5 Persistência e stable run

- Instâncias semanais e progresso sobrevivem a save/load (parâmetros simples); nada nesta
  spec altera seeds/snapshot da caverna (ADR-0005).
- Evidência: round-trip test + diff de escopo sem arquivos de geração procedural.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/CaveContracts/
  CaveContractCatalog.cs       (NOVO — definições dos 8 cc_* / templates dos semanais)
  CaveContractService.cs       (NOVO — rotação semanal, elegibilidade, progresso de marcos)
  NoHitFloorTracker.cs         (NOVO — flag por nível: entrada/dano/clear)
QuestSource (F34)              (emenda: valor CaveContract — aditivo)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (interactable Board_Zrix)
QuestRegistry/QuestManager     (consumo apenas — fluxo F34 inalterado)
Assets/_Game/Tests/EditMode/Quests/CaveContractsTests.cs (NOVO)
docs/validation/fable_51_spec_zrix_cave_contracts_execution_report.md
```

## Contratos

### Data contracts

- Marcos: 6 QuestDefinitions fixas (IDs cc_depth_5..90, source CaveContract, QuestLevel=N).
- Semanais: templates instanciados como QuestInstance F34 {templateId, alvo (bossId/levelN),
  semana:int, recompensa calculada} — tipos simples.
- Flags estáveis: map_segment_<faixa>, title_no_hit_<n> (QuestFlagService).

### Runtime contracts

- `CaveContractService`: em DayStartedEvent, se a semana mudou → regenerar os 2 semanais
  (StableHash(worldSeed|semana); rematch sorteia entre gates derrotados; no-hit fixa nível
  elegível da banda do jogador); progresso de marcos assina o evento de mudança de nível
  da caverna; entrega no quadro (fluxo board F34).
- `NoHitFloorTracker`: Reset(level) na entrada; MarkDamaged() em dano do player; Evaluate()
  no clear do nível — puro/testável; integração via eventos existentes de dano/clear.

### Event contracts

- `CaveContractsRefreshedEvent(semana)` (NOVO — board/UI escutam).
- Consome eventos existentes: mudança de nível da caverna, dano no player, boss derrotado,
  DayStartedEvent — tudo via GameEventBus; unsubscribe obrigatório.

### Save contracts

- Instâncias semanais/progresso pelo save aditivo de QuestInstance (F34 — sem seção nova);
  flags por QuestFlagService (persistido). Marcos = quests fixas no fluxo existente.
  Sem refs Unity.

### UI contracts

- Lista do quadro = projection do Quest Log F34 filtrada por source CaveContract (aba
  Contratos já agrupa por fonte). Sem tela nova.

## Sistemas afetados

```text
Quests (registry/manager/board/instâncias/flags — consumo F34 + enum aditivo)
Cave runtime (leitura de eventos de nível/boss/dano — cirúrgico)
Cena da fazenda (interactable Board_Zrix via gerador)
Event bus (1 evento novo + consumo)
Save (instâncias/flags pelos mecanismos existentes)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/CaveContracts/** (novos)
Assets/_Game/Scripts/Quests/QuestSource.cs (APENAS valor CaveContract aditivo — emenda F34)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (Board_Zrix)
pontos de assinatura de eventos cave/dano (cirúrgico — arquivos auditados na Fase 0)
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/** ; ProjectSettings/**
Geradores procedurais/seeds/snapshot da caverna (ADR-0005 — leitura de eventos apenas)
QuestManager/QuestRegistry core (consumo; nenhuma reescrita de fluxo)
Conteúdo de F35/F36/F52/F53 (cadeias, atos, secretas, festivais)
SaveManager core
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Emenda CaveContract aplicada? Registro de profundidade/bosses derrotados; evento de dano;
regra canônica de semana; ponto de clear de nível.

### Fase 1 — Enum e marcos
QuestSource.CaveContract (aditivo) + 6 cc_depth_N fixas + conclusão por evento de nível +
flags map_segment + testes de idempotência.

### Fase 2 — Semanais
CaveContractService (rotação semanal StableHash; rematch entre gates derrotados; no-hit
nível elegível) + NoHitFloorTracker + CaveContractsRefreshedEvent + testes.

### Fase 3 — Quadro físico
Board_Zrix no CreateMvpFarmScene (entrada da caverna) + filtro por source na projection.

### Fase 4 — Fechamento
Round-trip de save; regressões (board da praça intacto); csproj; run_strict_validation;
execution report com cláusula Dependency Chain (emenda F34).
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F48/F49 (combate/itens).
- Must not run with: F10/F34/F35/F36/F43/F52/F53 (cadeia quest — uma por vez), F09/F44
  (caverna runtime).
- Shared files/systems that require lock: QuestRegistry/QuestSource, QuestBoardService/
  instâncias, CreateMvpFarmScene, eventos cave.
- Reason: escreve no enum/fluxo de quest da F34 e no gerador da fazenda — superfícies da
  cadeia quest e da cadeia de cena.

## Impacto em save/load

```text
Does this change save schema? NO além do já aditivo da F34 (instâncias dinâmicas reusadas;
flags via QuestFlagService existente)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO (IDs/ints/flags)
```

## Impacto em eventos

```text
Adds events: YES — CaveContractsRefreshedEvent
Changes existing events: NO (apenas consumo: nível/boss/dano/DayStarted)
Requires unsubscribe pattern: YES (CaveContractService/NoHitFloorTracker assinam o bus)
```

## Impacto em UI/Unity

```text
Changes UI: NO (projection F34 filtrada por source; aba Contratos existente)
Changes scenes: via gerador (Board_Zrix na FarmScene) — sem YAML manual
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (definições em código/gerador de quest F34)
Requires Play Mode final validation: YES (aceitar marco, descer, completar)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: emenda F34 (CaveContract) não aplicada quando esta spec rodar.
Mitigação: tratar como dependência same-wave (BLOCKED_BY_DEPENDENCY_PENDING → aplicar
emenda aditiva → retornar) conforme spec_dependency_resolution; nunca criar enum paralelo.

Risco: no-hit dar falso positivo (dano não rastreado — DoT/hazard).
Mitigação: MarkDamaged ligado ao ponto ÚNICO de aplicação de dano ao player (auditado);
teste cobre dano direto e DoT sintético.

Risco: rematch sortear boss não derrotado ou rotação driftar entre máquinas.
Mitigação: elegibilidade filtrada pelo registro persistido + StableHash(worldSeed|semana)
sem Random; testes dedicados.

Risco: marco premiar 2× em reload na transição de nível.
Mitigação: idempotência pelo fluxo de recompensa F34 (quest concluída não repete) + teste
com round-trip.

Risco: quadro físico colidir com layout da FarmScene (F41 lotes).
Mitigação: ponto de ancoragem na entrada da caverna via gerador; coordenação de lock com
specs de cena.
```

## Rollback

```text
Remover Board_Zrix do gerador e desligar o CaveContractService (sem regeneração semanal)
desativa o canal; marcos aceitos viram quests inertes (não ofertadas). Valor de enum
aditivo permanece inofensivo. Nenhum save real apagado; board da praça (F34) intacto.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar emenda CaveContract, registro de profundidade/bosses, evento de dano, semana canônica.
- [ ] T002 — QuestSource.CaveContract (aditivo) + 6 cc_depth_N + conclusão por evento + flags + testes.
- [ ] T003 — CaveContractService (rotação semanal, rematch elegível, no-hit) + NoHitFloorTracker + evento + testes.
- [ ] T004 — Board_Zrix no CreateMvpFarmScene + filtro por source na projection.
- [ ] T005 — Round-trip de save + regressões; csproj; run_strict_validation; execution report (Dependency Chain).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (rotação semanal, elegibilidade, máquina no-hit,
  idempotência de marcos)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — aceitar marco e
  completar descendo; aceitar rematch)
- Requires regression test: YES (board da praça F34 intacto; quests fixas existentes
  intactas; nenhum arquivo procedural de caverna alterado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com 1 marco e
  1 semanal completados

## Definition of Done

```text
8 cc_* funcionais (6 marcos 1× + 2 semanais dinâmicos determinísticos) no fluxo único F34
com source CaveContract; quadro do Zrix interagível na entrada da caverna via gerador;
recompensas do catálogo (XP escalado, mapa do trecho, essência garantida, título + charm);
instâncias persistidas; stable run intocado; builds 0E; execution report criado.
```

## Anti-regressão

```text
IDs canônicos cc_* preservados; nenhum segundo board/registry.
Board da praça (3 contratos/dia) e mural F34 intactos.
CaveRunSeed/snapshot jamais alterados por esta spec (ADR-0005).
Semanais aceitos não são revogados pela rotação (regra board F34).
Save de quest: tipos simples; saves legados carregam com defaults.
Eventos só via GameEventBus; unsubscribe nos serviços; nenhum GameObject.Find.
```
