# SPEC — Quests de Festival: as 8 fq_* (Uma por Festival, Repetíveis por Ano)

> **Spec ID:** `fable_53_spec_festival_quests`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Runtime / Content / Integration
> **Domain:** Quests / World / NPC
> **Parallelizable:** NO (cadeia quest — QuestRegistry lock)
> **Parallel group:** N/A (ordem na cadeia quest: após F34/F37; nunca junto de F35/F51/F52)
> **Can run with:** F48/F49 (combate/itens — superfícies disjuntas)
> **Must not run with:** F10, F34, F35, F36, F43, F51, F52 (cadeia quest), F37 (festivais — dependência direta), F26 (amizade)
> **Repo lock scope:** QuestRegistry/geradores de quest, consumo das flags festival_active(id) e barracas (F37), diálogos de NPC organizador
> **Depends on:**
> - `fable_37_spec_festivals_lunar_events_runtime` (E32 — flags festival_active(id), barracas, calendário de festivais)
> - `fable_34_spec_quest_sources_infrastructure` (E38 — fluxo de quest/instâncias/recompensa escalada/mural anuncia)
> - `fable_26_spec_friendship_state_contract` (E35 — presentes/amizade do fq_anonovo)
> - `fable_32_spec_item_catalog_data_expansion` (E18 — itens de recompensa; festival_cake)
> **Blocks:** itens de festival adicionais (pendência do catálogo); minigames ricos de festival (futuro)
> **Scope:** autorar as 8 quests fq_* do catálogo (Parte F), ofertadas apenas durante o festival correspondente (flag festival_active), consumindo barracas/NPCs da F37, com recompensas do catálogo e repetição anual.
> **Out of scope:** infraestrutura de festival (F37 — calendário/flags/barracas), minigames dedicados (concursos resolvem por dados existentes: qualidade de crop, duelos da arena), decoração/arte, aniversários de NPC.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [event_rules.md, save_rules.md, farm_rules.md]

---

# /speckit.specify

## Contexto

O QUEST_CATALOG (Parte F) fecha o conteúdo de festival do v1 em 8 quests, uma por
festival do calendário:

```text
fq_plantio (Primavera d7) — concurso de plantio de Thandra.
fq_caravana (P d21) — escoltar a carga de Finan/Merithus pela estrada (combate leve).
fq_luas (Verão d14) — coletar 3 ecos, um sob cada lua, na mesma noite.
fq_torneio (V d28) — 3 duelos na arena do portão (Alaric arbitra; Kaand aprova).
fq_colheita (Outono d7) — o maior crop do vale (qualidade conta!).
fq_veus (O d21) — no mercado noturno expandido, achar o item "que não existe".
fq_vigilia (Inverno d14) — a vigília de Anya na Fonte (sem combate; lore pesada).
fq_anonovo (I d28) — entregar presentes a 5 NPCs antes da meia-noite.
```

A F37 entrega a infraestrutura de festival: dias canônicos, flag `festival_active(id)`,
3 barracas na praça (interactables temporários), anúncio no mural (F34) e tudo derivável
de calendário + worldSeed (sem save novo). A F34 entrega o fluxo de quest com recompensa
escalada. O total do catálogo (§13: "20 main + 36 side + 6 moldes daily + 8 contratos +
8 secretas + 8 festivais ≈ 86") conta com estas 8.

Os festivais são anuais por natureza (datas fixas de estação/dia): as fq_* devem reabrir
a cada ano do calendário — concluir no ano 1 não pode bloquear o ano 2 (recompensa de
item única vs. repetível segue o catálogo: XP/ouro escalado sempre; itens únicos como
título/troféu apenas na primeira conclusão).

## Problema

Sem esta spec, os festivais da F37 são cenário sem verbo: barracas que vendem e falas que
mudam, mas nenhum objetivo. As 8 quests do catálogo ficam mortas, os ganchos sociais
(presentes do ano-novo com amizade F26, vigília de lore da Anya, torneio de arena) não
existem, e o jogador não tem razão mecânica para estar presente no dia do festival —
contradizendo a direção "dias de festival visivelmente diferentes".

## Objetivo

Ao final desta spec, em cada dia de festival a quest correspondente deve ser ofertada
(pelo NPC organizador/barraca, apenas com `festival_active(id)` verdadeira), completável
dentro do festival, com recompensa escalada (ponto único F34) e itens/efeitos do
catálogo; expirar sem punição ao fim do festival (reaparece no próximo ano); e cada uma
das 8 deve usar APENAS sistemas existentes como mecânica (qualidade de crop F15/F32,
escolta leve, coleta noturna por lua, duelos de arena, compra no mercado noturno,
interação com a Fonte F17, presentes/amizade F26) — sem nenhum minigame novo.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md (Parte F, §2 recompensas, §13 conexões)
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md (festivais/luas/calendário)
docs/specs/a_implementar/fable/fable_37_spec_festivals_lunar_events_runtime.md (contratos consumidos)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/npc-dialogue-authoring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (pós-F34/F37/F26):
- flag festival_active(id) + barracas + calendário de festivais (F37);
- QuestRegistry/QuestManager/instâncias dinâmicas/recompensa escalada (F34);
- amizade/presentes (F26), diálogo condicional (F28), qualidade de crop (F15/F32),
  Fonte interactable (F17), arena/duelos (encontros existentes), mercado noturno (F19/F40
  — auditar shape), calendário/luas (WAVE 02 + F37).
Não existe:
- as 8 definições fq_* (condições/recompensas/ofertantes);
- mecanismo de quest com janela de validade = duração do festival (expira sem punição,
  reabre no ano seguinte);
- rastreio "3 ecos, um sob cada lua, na mesma noite" e "5 presentes antes da meia-noite".
Auditar Fase 0:
- como F37 expõe "festival ativo" (flag consultável + evento de início/fim?);
- shape do ano no calendário (ano:int disponível? senão derivar de dia absoluto);
- arena do portão e mercado noturno existem como pontos interagíveis? (fallback: barraca
  F37 como ofertante/ponto de interação);
- entrega de presente (F26) publica evento consumível para contagem do fq_anonovo.
```

## Engineering stories

```text
Como jogador, quero que cada festival tenha UMA quest com a cara dele — plantar, escoltar,
caçar ecos, duelar, colher, garimpar o mercado, velar, presentear.
Como jogador, quero que perder o festival deste ano só signifique esperar o próximo.
Como NPC organizador, quero ofertar minha quest apenas durante meu festival.
Como QuestManager, quero quests de festival no MESMO fluxo de aceite/progresso/recompensa.
```

## Escopo

```text
Inclui:
- 8 definições fq_* (IDs canônicos) ofertadas pelo NPC organizador/barraca da F37 somente
  com festival_active(id); aceite/progresso/recompensa pelo fluxo F34;
- janela de validade: quest expira (estado Expired, sem punição) quando o festival termina;
  reofertada no mesmo festival do ano seguinte (chave de repetição = ano do calendário);
- recompensas: XP/ouro escalados (ponto único F34; QuestLevel = nível do player no aceite,
  regra de daily do catálogo §2); itens temáticos do catálogo F32 onde definidos
  (ex.: festival_cake; troféu/título de torneio/colheita = flag única na 1ª conclusão;
  repetições anuais premiam XP/ouro/consumíveis);
- mecânicas por quest (REUSO estrito):
  fq_plantio — plantar N sementes de Thandra no dia (eventos de plantio existentes);
  fq_caravana — escolta leve: acompanhar a carga por waypoints com 1-2 encontros (spawn
    existente; sem AI nova de comboio — checkpoint walk);
  fq_luas — coletar 3 ecos interactables spawnados à noite, 1 por lua (luas F37/WAVE02);
  fq_torneio — vencer 3 duelos na arena (encontros existentes; Alaric arbitra via diálogo);
  fq_colheita — entregar 1 crop de qualidade Ouro (CropQualityResolver F15);
  fq_veus — comprar/achar o "item que não existe" no mercado noturno (item especial 1×);
  fq_vigilia — interagir com a Fonte (F17) à noite e permanecer na vigília (diálogo/lore
    em sequência; sem combate);
  fq_anonovo — entregar presentes a 5 NPCs distintos antes da meia-noite (entrega de
    presente F26 contada por evento);
- rastreadores puros: NightEchoTracker (3 ecos/luas/mesma noite), GiftCountTracker
  (5 NPCs distintos/antes da meia-noite) — testáveis;
- diálogos curtos dos organizadores (F28 pools aditivos);
- save: progresso pelo mecanismo de quest existente; chave anual = flags
  fq_<id>_done_year_<n> (QuestFlagService); troféus = flags únicas;
- EditMode tests: oferta gated por festival_active, expiração sem punição no fim do
  festival, reoferta no ano seguinte, trackers (ecos por lua/mesma noite; 5 presentes
  distintos/meia-noite), troféu único vs repetição anual, round-trip de save.
```

## Fora de escopo

```text
Não inclui:
- infraestrutura de festival (F37) e mural (F34);
- minigames dedicados (concursos resolvem por dados/encontros existentes);
- itens de festival ADICIONAIS (pendência do catálogo — 1 por festival fica para spec futura);
- decoração/arte/voice; aniversários de NPC;
- romance (F46) — presentes do ano-novo usam amizade F26 apenas.
```

## Regras de não duplicação

```text
Não criar calendário/flag paralelos — festival_active(id) da F37 é a única fonte.
Não criar fluxo novo de quest — F34 (aceite/progresso/recompensa/instância).
Não duplicar fórmula de recompensa — ponto único F34.
Não criar minigame — cada fq_* mapeia para sistemas existentes (lista do escopo).
Não recriar entrega de presente — evento da F26.
IDs canônicos fq_* — nunca renomear.
```

## Critérios de aceite

### CA-1 Oferta gated e expiração limpa

- fq_* só é ofertada com festival_active(id); ao fim do festival a quest não concluída
  expira sem punição e some do log ativo.
- Evidência: EditMode tests (oferta com/sem flag; expiração por evento de fim).

### CA-2 Repetição anual com troféu único

- No mesmo festival do ano seguinte a quest é reofertada; XP/ouro premiam de novo; o
  troféu/título é concedido apenas na primeira conclusão (idempotente entre anos).
- Evidência: EditMode tests com avanço de ano sintético + flags.

### CA-3 Trackers honestos

- fq_luas exige os 3 ecos na MESMA noite (amanhecer reseta); fq_anonovo exige 5 NPCs
  DISTINTOS antes da meia-noite (o 6º presente ou NPC repetido não conta).
- Evidência: EditMode tests dos dois trackers (casos felizes e de reset).

### CA-4 As 8 completáveis por sistemas existentes

- Cada fq_* completa via mecânica reusada (plantio, escolta-checkpoint, duelos, qualidade
  Ouro, compra, Fonte, presentes) — nenhuma classe de minigame nova no diff.
- Evidência: Spec Compliance Matrix + diff audit (non-regression review).

### CA-5 Persistência

- Progresso ativo, flags anuais e troféus sobrevivem a save/load.
- Evidência: round-trip tests.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/FestivalQuests/
  FestivalQuestCatalog.cs      (NOVO — 8 definições + janela de validade + chave anual)
  FestivalQuestService.cs      (NOVO — oferta gated, expiração, reoferta anual)
  NightEchoTracker.cs          (NOVO — 3 ecos/luas/mesma noite, puro)
  GiftCountTracker.cs          (NOVO — 5 NPCs distintos/meia-noite, puro)
diálogos organizadores (F28 pools — aditivo)
ecos noturnos / item "que não existe" (interactable/entrada de shop via mecanismos F37/F19)
Assets/_Game/Tests/EditMode/Quests/FestivalQuestsTests.cs (NOVO)
docs/validation/fable_53_spec_festival_quests_execution_report.md
```

## Contratos

### Data contracts

- 8 QuestDefinitions (IDs canônicos fq_*, source Npc com tag de festival na instância —
  auditar na Fase 0 se F34 pede valor de enum próprio; preferir tag aditiva a enum novo).
- Flags: fq_<id>_trophy (única), fq_<id>_done_year_<n> (anual) — QuestFlagService.

### Runtime contracts

- `FestivalQuestService`: assina eventos de início/fim de festival (F37); no início,
  oferta a fq_* do id; no fim, expira não concluídas (estado limpo, sem punição);
  reoferta no ano seguinte (ano derivado do calendário).
- Trackers puros com janelas temporais (noite/meia-noite) alimentados por eventos
  existentes (coleta, presente F26, relógio/DayStarted).

### Event contracts

- `FestivalQuestOfferedEvent(questId)` / `FestivalQuestExpiredEvent(questId)` (NOVOS —
  UI/toast escutam). Consome: eventos F37 (festival início/fim), F26 (presente entregue),
  plantio/colheita/compra/duelo existentes — via GameEventBus, unsubscribe obrigatório.

### Save contracts

- Progresso pelo save de quest existente (F34); flags anuais/troféus pelo
  QuestFlagService. Sem seção nova; sem refs Unity; defaults seguros para saves legados.

### UI contracts

- Log/abas F34 (source Side/Npc); diálogos F28; sem tela nova. Mural F34 já anuncia o
  festival (F37) — esta spec não toca o mural.

## Sistemas afetados

```text
Quests (definições/serviço/flags — consumo F34)
Festivais (consumo de flags/eventos/barracas F37 — leitura)
NPC/diálogo (pools F28 organizadores)
Amizade (consumo do evento de presente F26)
Farm/World (eventos de plantio/colheita/coleta — leitura)
Save / Event bus
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/FestivalQuests/** (novos)
pools de diálogo F28 (aditivo — organizadores)
ponto de spawn de ecos noturnos / entrada do item especial no mercado noturno
  (mecanismos F37/F19 auditados na Fase 0 — aditivo cirúrgico)
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/** ; ProjectSettings/**
WorldEventService/calendário/flags de festival (F37 — leitura apenas)
QuestManager/QuestRegistry core (consumo); mural (F34)
Conteúdo F35/F36/F51/F52; sistema de amizade core (F26 — leitura de evento)
SaveManager core
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Eventos de início/fim de festival (F37), ano no calendário, arena/mercado noturno como
pontos de interação, evento de presente (F26), tag de festival na instância F34.

### Fase 1 — Serviço e ciclo de vida
FestivalQuestService (oferta gated, expiração limpa, reoferta anual) + flags anuais/troféu
+ testes do ciclo (oferta/expira/reabre/idempotência de troféu).

### Fase 2 — As 8 quests (mecânicas reusadas)
Definições + condições por sistemas existentes (plantio, escolta-checkpoint, duelos,
qualidade Ouro, compra especial, Fonte, presentes) + NightEchoTracker/GiftCountTracker +
testes dos trackers.

### Fase 3 — Diálogo e pontas
Pools F28 dos organizadores + ecos noturnos/item especial via mecanismos F37/F19 +
eventos novos de oferta/expiração.

### Fase 4 — Fechamento
Round-trips; regressões (festivais F37 funcionam sem as quests; quests existentes
intactas); csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F48/F49 (combate/itens).
- Must not run with: F10/F34/F35/F36/F43/F51/F52 (cadeia quest), F37 (dependência direta —
  deve estar concluída), F26/F28 (social, se tocarem pools simultaneamente).
- Shared files/systems that require lock: QuestRegistry, pools F28, consumo de eventos F37.
- Reason: conteúdo na cadeia quest consumindo superfícies sociais/festival recém-criadas.

## Impacto em save/load

```text
Does this change save schema? NO além dos mecanismos existentes (quest save F34 + flags)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES — FestivalQuestOfferedEvent, FestivalQuestExpiredEvent
Changes existing events: NO (consumo: festival início/fim, presente, plantio, compra, duelo)
Requires unsubscribe pattern: YES (FestivalQuestService e trackers assinam o bus)
```

## Impacto em UI/Unity

```text
Changes UI: NO (log/abas F34; diálogos F28; toasts)
Changes scenes: NO (ecos/itens via mecanismos de spawn existentes — sem YAML manual)
Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador apenas (definições/diálogos, se asset-based)
Requires Play Mode final validation: YES (1 festival completo com quest)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: quest pendurada após o fim do festival (estado órfão no log).
Mitigação: expiração ligada ao evento de fim (F37) com varredura de segurança no
DayStarted seguinte + teste.

Risco: troféu duplicado entre anos ou perdido em reload.
Mitigação: flag única idempotente + round-trip test.

Risco: fq_luas impossível se a noite não tiver as 3 luas visíveis (regra lunar F37).
Mitigação: Fase 0 valida a regra lunar do catálogo (Verão d14 = noite das luas); o
gerador da quest usa a janela canônica; teste do tracker com janela.

Risco: fq_caravana exigir AI de comboio inexistente.
Mitigação: escolta-checkpoint (waypoints + encontros spawnados) — decisão de escopo
explícita; sem AI nova; documentado no report.

Risco: dependências sociais (F26/F28/F37) incompletas no momento da execução.
Mitigação: cadeia same-wave (BLOCKED_BY_DEPENDENCY_PENDING → resolver → retornar)
conforme spec_dependency_resolution; nunca duplicar os sistemas.
```

## Rollback

```text
Desligar o FestivalQuestService (nenhuma oferta) devolve os festivais F37 puros; flags
anuais/troféus ficam inertes. Remover definições/trackers desfaz a spec; quests fixas e
festivais intactos; nenhum save real apagado.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar eventos F37 (início/fim), ano do calendário, arena/mercado noturno, evento de presente F26.
- [ ] T002 — FestivalQuestService (oferta gated, expiração, reoferta anual, troféu único) + flags + testes.
- [ ] T003 — 8 definições fq_* com mecânicas reusadas + NightEchoTracker/GiftCountTracker + testes.
- [ ] T004 — Diálogos organizadores (F28) + ecos noturnos/item especial + eventos de oferta/expiração.
- [ ] T005 — Round-trips/regressões; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gating por flag, expiração/reoferta anual, trackers
  temporais, idempotência de troféu)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — completar 1 quest
  de festival no dia certo)
- Requires regression test: YES (festivais F37 funcionam sem quest; quests fixas/diárias
  existentes intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano completando
  fq_colheita ou fq_anonovo em festival real

## Definition of Done

```text
8 fq_* ofertadas somente durante o festival correspondente, completáveis por sistemas
existentes, expirando sem punição e reabrindo a cada ano; XP/ouro escalados (ponto único
F34); troféus únicos idempotentes; trackers de noite/presentes testados; flags persistidas;
builds 0E; execution report com Spec Compliance Matrix.
```

## Anti-regressão

```text
IDs canônicos fq_* preservados; festival_active(id) é a única fonte de gating (F37).
Nenhum minigame/classe de mecânica nova; nenhum calendário paralelo.
Festivais F37 e mural F34 intocados; quests existentes intactas.
Troféu jamais duplica entre anos (idempotência testada com reload).
Flags só via QuestFlagService; tipos simples; saves legados com defaults.
Eventos só via GameEventBus; unsubscribe no serviço/trackers; nenhum GameObject.Find.
```
