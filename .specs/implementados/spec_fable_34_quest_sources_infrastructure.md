# SPEC — Quests: Infraestrutura das 5 Fontes (board, NPC, mural, segredos, main)

> **Spec ID:** `fable_34_spec_quest_sources_infrastructure`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P1
> **Type:** Runtime / Integration
> **Domain:** Quests
> **Parallelizable:** NO (QuestRegistry lock)
> **Parallel group:** N/A (cadeia quest — ordem F34 → F35 → F10 → F36)
> **Can run with:** N/A
> **Must not run with:** F10, F35, F36 (mesma cadeia quest — ordem F34 → F35 → F10 → F36)
> **Repo lock scope:** QuestRegistry/QuestManager, board interactables, geradores de quest
> **Depends on:**
> - F26 (amizade p/ gates)
> - F32 (itens de recompensa)
> - CreateMvpTownScene (board físico)
> **Blocks:**
> - F35 (cadeias)
> - F36 (atos)
> - F37 (festivais anunciam no mural)
> **Scope:** as 5 fontes de quest do catálogo funcionando como canais distintos + XP escalado.
> **Out of scope:** conteúdo das cadeias (F35/F36), UI rica (aba F14 lista), voice.

required_adrs: []
required_game_rules: [quest_rules.md]

---

# /speckit.specify

## Contexto

O `QUEST_CATALOG` v1.0 define que a vila fala com o jogador por 5 fontes, cada uma com
mecânica própria: (1) Quadro de Contratos com templates rotativos procedurais (gerido
pelo Hund — moldes bd_cull/bd_gather/bd_delivery/bd_escort_supply/bd_harvest/bd_repair,
aceita-se e entrega-se NO quadro), (2) NPCs diretos com cadeias pessoais ("!" prateado),
(3) Mural da prefeitura com anúncios read-only (festivais próximos, marcos da main),
(4) segredos da caverna — quests SEM marcador entregues por criaturas não-agressivas
(Old Scrounger King, Goblin Warchief, Silence Warden) e pelos mercadores errantes — e
(5) main quest em 4 atos ("!" dourado, nunca expira).

Decisões humanas vinculantes (Q6.1/Q6.2): XP e ouro ESCALAM com o nível da quest (nada
estático) e a main quest dá +1 skill point por ATO concluído. As regras de fila do
catálogo: sem limite de quests ativas, 1 quest "tracked" no HUD, abas no log
(Main/Side/Contratos/Secretas — a última só lista as já descobertas).

O repo tem `QuestRegistry`/`QuestManager` (aceitar/progresso/recompensa/save),
`QuestFlagService` e condições/objetivos das WAVEs 09/26 — mas só com quests FIXAS:
não há canais, templates rotativos, instância dinâmica, XP escalado nem mural. Esta spec
constrói a INFRAESTRUTURA dos canais; o conteúdo autoral das cadeias e atos fica em
F35/F36, e os festivais anunciam no mural via F37.

## Problema

Sem canais distintos, todo o conteúdo do catálogo (~86 quests) teria que entrar como
quest fixa de registry — sem rotação diária do quadro, sem ofertas secretas na caverna,
sem mural e sem recompensa escalada. F35/F36 ficariam bloqueadas ou criariam mecanismos
próprios (duplicação na cadeia mais sensível do jogo). E a decisão "XP escala" ficaria
sem dono: recompensas estáticas envelhecem mal com o cap 100.

## Objetivo

Ao final desta spec, o projeto deve ter as 5 fontes funcionando como canais distintos —
board com 3 contratos/dia determinísticos e instanciação dinâmica, mural read-only,
API de secret quests consumida por mercador errante/monstro pacífico, side/main pelo
fluxo existente — com XP/ouro escalados por fórmula, +1 skill point por ato (idempotente),
instâncias dinâmicas persistidas com parâmetros simples e agrupamento por fonte na
projection do Quest Log (F14), sem segundo registry e sem tocar no conteúdo autoral.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (§fontes, §templates, §XP)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§5 quests)
docs/design/gameplay/balance/BALANCE_CURVES_DIRECTION_v1.0.md (XP por nível de quest)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- QuestRegistry / QuestManager (aceitar/progresso/recompensa/save) — fluxo único de quest;
- QuestFlagService (flags persistidas);
- condições/objetivos de quest (WAVE 09/26);
- DayStartedEvent (gatilho da rotação diária);
- SkillTreeManager (recebe o hook de +1 ponto por ato);
- CreateMvpTownScene (gerador da cena da cidade — board físico? auditar).
Não existe:
- fontes como canais (QuestSource);
- templates rotativos / quest dinâmica (QuestInstance);
- XP/ouro escalados; +1 skill point por ato;
- mural read-only; API de secret quests;
- agrupamento por fonte na projection do log.
Auditar Fase 0:
- shape de QuestDefinition (aceita instância dinâmica? campos aditivos?);
- seção de save de quest (onde persistir parâmetros de instância);
- existe board físico na TownScene? (gerador de cena — criar interactable se não).
```

## Engineering stories

```text
Como jogador, quero 3 contratos novos por dia no quadro da praça, com recompensa à altura do meu nível.
Como jogador, quero descobrir quests secretas conversando com criaturas da caverna e mercadores errantes.
Como QuestManager, quero que quests dinâmicas entrem no MESMO fluxo de aceite/progresso/recompensa/save.
Como F35/F36, quero canais prontos (Npc/Main) para só autorar conteúdo em cima.
```

## Escopo

```text
Inclui:
- QuestSource enum {Board, Npc, Mural, CaveSecret, Main} em QuestDefinition (aditivo);
- BOARD: QuestBoardService — 3 contratos/dia dos templates do catálogo (caça N da banda /
  coleta N do item / entrega), rotação determinística (StableHash por dia), instanciação
  dinâmica (QuestInstance com parâmetros: alvo, quantidade, recompensa calculada);
  interactable Board_Contratos na praça (gerador de cena — auditar se existe);
  regras do catálogo: contratos nunca pedem item de quest nem apontam criaturas
  não-agressivas;
- MURAL: anúncios read-only (festival próximo F37, marcos de main) — interactable
  separado do board (mural ≠ board);
- CAVE SECRET: API OfferSecretQuest(questId) — consumida pelo mercador errante (chance 15%
  de ofertar em vez de loja, determinística) e por interactable de monstro pacífico
  (F33 flag peaceful); secretas só aparecem no log após descobertas;
- XP escalado: XP = base × (1 + 0.08 × questLevel) (curva do catálogo); recompensa de
  gold idem; QuestLevel: daily = nível do player no aceite; main = fixo por ato;
- main quest: +1 skill point por ato concluído (hook no SkillTreeManager, idempotente);
- aba Quest Log (F14) agrupa por fonte (campo novo na projection);
- save: quests dinâmicas persistem como instância (parâmetros simples: IDs/ints) —
  estender a seção existente de quest (campos aditivos);
- EditMode tests: rotação determinística, instanciação de template, XP escalado,
  skill point por ato (idempotente), save/load de instância dinâmica.
```

## Fora de escopo

```text
Não inclui:
- conteúdo autoral das cadeias de NPC (F35) e dos atos da main (F36);
- UI rica de quest (a aba F14 lista; tracker/marcadores visuais ficam com F14/F20);
- voice/diálogo gravado;
- festivais em si (F37 — o mural só os anuncia);
- recompensas únicas item-a-item das cadeias (autoria F35 com itens F32).
```

## Regras de não duplicação

```text
Não criar segundo registry/manager — quests dinâmicas entram no fluxo existente de
aceite/progresso/recompensa/save.
Conteúdo autoral das cadeias/atos = F35/F36 (aqui só infraestrutura de canal).
Mural ≠ board: mural é read-only, sem aceite — não duplicar o board nele.
Não duplicar fórmula de recompensa — ponto único de cálculo escalado.
```

## Critérios de aceite

### CA-1 Board rotativo determinístico

- O quadro oferece 3 contratos/dia derivados dos templates, determinísticos por dia
  (StableHash); aceitar transforma o contrato em quest ativa normal no fluxo existente.
- Evidência: EditMode tests de rotação (mesmo dia = mesmos contratos; dia seguinte =
  rotação) + instanciação.

### CA-2 Contrato funcional com XP escalado

- Contrato de caça conta kills da banda certa e premia XP/ouro pela fórmula
  base × (1 + 0.08 × questLevel).
- Evidência: EditMode test da fórmula + progresso de objetivo com kills sintéticos.

### CA-3 Secret quests determinísticas

- O mercador errante oferta secret quest com chance determinística (15%, por seed);
  monstro pacífico oferta via API; secretas só listam após descoberta.
- Evidência: EditMode test da chance por seed + API com flag peaceful sintética.

### CA-4 Skill point por ato idempotente

- Ato concluído concede +1 skill point exatamente 1× — repetir o turn-in/reload não
  duplica.
- Evidência: EditMode test de idempotência (incluindo após save/load).

### CA-5 Persistência de instâncias

- Instâncias dinâmicas (alvo/quantidade/recompensa/fonte) sobrevivem a save/load com
  parâmetros simples.
- Evidência: EditMode test de round-trip da seção de quest estendida.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/
  QuestSource.cs           (NOVO — enum {Board, Npc, Mural, CaveSecret, Main})
  QuestBoardService.cs     (NOVO — templates, rotação diária, instanciação)
  QuestInstance.cs         (NOVO — parâmetros simples: alvo, quantidade, recompensa)
  SecretQuestOffer.cs      (NOVO — API OfferSecretQuest(questId))
QuestRegistry/QuestManager (campos/fluxo aditivos: source, instância dinâmica,
                            recompensa escalada, hook de ato → skill point)
seção de save de quest    (campos aditivos p/ instâncias)
interactables Board_Contratos e Mural (gerador de cena — CreateMvpTownScene)
CaveWanderingMerchant     (oferta secreta 15% determinística)
projection do Quest Log   (campo de fonte p/ agrupamento na aba F14)
Assets/_Game/Tests/EditMode/Quests/QuestSourcesTests.cs (NOVO)
docs/validation/fable_34_spec_quest_sources_infrastructure_execution_report.md
```

## Contratos

### Data contracts

- `QuestSource` enum {Board, Npc, Mural, CaveSecret, Main} — campo aditivo em
  QuestDefinition.
- `QuestInstance`: questTemplateId, alvo (ID), quantidade (int), questLevel (int),
  recompensa calculada (ints), source — apenas tipos simples.

### Runtime contracts

- `QuestBoardService`: em DayStartedEvent, gera 3 contratos do dia
  (StableHash por dia → templates + parâmetros por nível do player); aceite converte em
  quest ativa no QuestManager existente; entrega NO quadro.
- `SecretQuestOffer.OfferSecretQuest(questId)`: ponto único de oferta — consumido pelo
  mercador errante (15% determinístico) e por interactables pacíficos (F33).
- Recompensa escalada: ponto único — XP/gold = base × (1 + 0.08 × questLevel).
- Hook de ato: conclusão de ato da main → +1 skill point no SkillTreeManager
  (idempotente por flag de ato).

### Event contracts

- `QuestBoardRefreshedEvent` (NOVO — publicado na rotação diária; UI/board escutam).
- Demais comunicações pelos eventos de quest existentes (GameEventBus).

### Save contracts

- Seção de quest existente, campos aditivos: instâncias dinâmicas (parâmetros simples —
  IDs/ints), flags de ato premiado. Sem seção nova; sem referências Unity.

### UI contracts

- Projection do Quest Log ganha campo de fonte; a aba F14 agrupa por ele
  (Main/Side/Contratos/Secretas — secretas só descobertas). Sem tela nova nesta spec.

## Sistemas afetados

```text
Quest (registry/manager/save/projection)
Skill tree (hook +1 ponto por ato)
Cena da cidade (interactables Board/Mural via gerador)
Mercador errante da caverna (oferta secreta)
Event bus (QuestBoardRefreshedEvent)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/** (QuestSource/BoardService/Instance/SecretOffer + manager/registry aditivos)
seção de save de quest (campos aditivos — owner auditado na Fase 0)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (interactables Board/Mural)
CaveWanderingMerchant (ponto de oferta — arquivo auditado na Fase 0)
projection do Quest Log (campo de fonte)
SkillTreeManager (hook idempotente de skill point por ato)
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/**
ProjectSettings/**
Conteúdo autoral de cadeias/atos (F35/F36)
UI de telas (F14 — apenas o campo na projection)
SaveManager core (apenas campos aditivos na seção de quest)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar QuestDefinition (instância dinâmica? aditivos?), seção de save de quest e
existência do board físico na TownScene (gerador).

### Fase 1 — Canais e instâncias
QuestSource + QuestInstance dinâmica no fluxo existente + save aditivo + testes de
round-trip.

### Fase 2 — Board e mural
QuestBoardService (templates do catálogo, rotação StableHash por dia, 3/dia,
regras: sem item de quest, sem alvo não-agressivo) + interactables Board_Contratos e
Mural (read-only) no gerador de cena + QuestBoardRefreshedEvent.

### Fase 3 — Recompensas
XP/ouro escalados (ponto único, fórmula 1 + 0.08 × questLevel) + skill point por ato
(idempotente) + testes.

### Fase 4 — Secretas e fechamento
SecretQuestOffer (mercador 15% determinístico / interactable pacífico) + agrupamento
por fonte na projection; csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Must not run with: F10, F35, F36 — mesma cadeia quest; ordem obrigatória
  F34 → F35 → F10 → F36.
- Shared files/systems that require lock: QuestRegistry/QuestManager, board
  interactables, geradores de quest.
- Reason: altera o fluxo central de quest e a seção de save correspondente — superfícies
  das quais toda a cadeia F35/F10/F36 depende.

## Impacto em save/load

```text
Does this change save schema? YES (campos aditivos na seção de quest existente)
Does this add a save section? NO
Does this require migration? NO (campos aditivos com defaults seguros; saves antigos
carregam sem instâncias dinâmicas)
Does this persist Unity references? NO (instâncias = IDs/ints simples)
Owner/restore order: inalterados — mesma seção, mesmo owner.
```

## Impacto em eventos

```text
Adds events: YES — QuestBoardRefreshedEvent (rotação diária)
Changes existing events: NO
Requires unsubscribe pattern: YES (BoardService assina DayStartedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: campo de fonte na projection apenas (aba F14 agrupa)
Changes scenes: via gerador (interactables Board/Mural) — sem YAML manual
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (templates em código/dados do gerador de quest)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: instância dinâmica × save (objetos complexos no DTO).
Mitigação: parâmetros simples (IDs/ints) apenas — regra save-dto-simple-types-only;
round-trip testado.

Risco: skill point por ato duplicar em reload/re-turn-in.
Mitigação: flag de ato premiado persistida + teste de idempotência com save/load.

Risco: rotação do board driftar entre máquinas/loads.
Mitigação: StableHash por dia (sem Random), coberto por teste.

Risco: contrato de caça apontar criatura não-agressiva ou pedir item de quest.
Mitigação: regras do catálogo aplicadas na geração do template + teste.

Risco: board físico inexistente na TownScene.
Mitigação: Fase 0 audita; interactable criado via gerador de cena (nunca YAML manual).
```

## Rollback

```text
Board/mural não geram ofertas (flag) e a API de secretas não é chamada — quests fixas
existentes ficam intactas. Campos aditivos de save são inofensivos vazios. Reverter os
arquivos novos + hooks desfaz a spec; não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar QuestDefinition/save/board físico (TownScene).
- [ ] T002 — QuestSource + QuestInstance dinâmica + save aditivo + testes de round-trip.
- [ ] T003 — QuestBoardService (templates/rotação StableHash/3 por dia) + interactables Board/Mural + evento.
- [ ] T004 — XP/ouro escalados (1 + 0.08 × questLevel) + skill point por ato idempotente + testes.
- [ ] T005 — Secret offers (mercador 15% determinístico / monstro pacífico) + projection por fonte; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (rotação, fórmula, chance secreta, idempotência)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (quests fixas existentes intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano aceitando contrato
  do board e concluindo

## Definition of Done

```text
5 canais funcionais como infraestrutura (board rotativo determinístico 3/dia, mural
read-only, API de secretas com 2 consumidores, side/main no fluxo existente); XP/ouro
escalados por ponto único; +1 skill point/ato idempotente; instâncias dinâmicas salvas
com tipos simples; projection agrupa por fonte; builds 0E; execution report criado.
```

## Anti-regressão

```text
Quests fixas existentes intactas (aceite/progresso/recompensa/save inalterados p/ elas).
Nenhum segundo registry/manager de quest.
Contratos jamais pedem item de quest nem apontam criaturas não-agressivas (regra canônica).
Save de quest: somente tipos simples; saves antigos carregam com defaults seguros.
Skill point por ato nunca duplica (idempotência testada com reload).
Eventos só via GameEventBus; nenhum GameObject.Find em runtime.
```


---

## EMENDA 2026-06-12-B (auditoria de completude — VINCULANTE)

```text
1. O enum QuestSource ganha o 6º valor: CaveContract (contratos do Zrix — QUEST_CATALOG
   §11 define o Zrix como a 4ª das 5 fontes; o canal tinha ficado fora do enum).
   A fable_51 (Zrix) implementa o canal; esta spec apenas RESERVA o valor no enum e
   garante que o Quest Log (F14) agrupa por ele.
2. A API OfferSecretQuest passa a ser consumida também pela fable_52 (conteúdo scq_*).
```


---

## EMENDA 2026-06-12-C (débito WI-26 — VINCULANTE)

```text
1. PREREQUISITE_UI_DEBT (WAVE_INTEGRATION_26): o QuestGiverInteractable NÃO valida
   PrerequisiteQuestIds antes de ofertar. Esta spec DEVE fechar o débito: oferta só
   aparece com pré-requisitos completos (teste EditMode do gate).
```
