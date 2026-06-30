# SPEC — Cidade Viva: Aniversários de NPC + Cama da Estalagem + ADR de Reputação

> **Spec ID:** `fable_57_spec_living_city_birthdays_inn_reputation_adr`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P3
> **Type:** Runtime / Integration / Docs (ADR)
> **Domain:** NPC / Social / City
> **Parallelizable:** CONDITIONAL (lock do gerador da TownScene e do FriendshipService)
> **Parallel group:** fable_batch10_city
> **Can run with:** F54, F55, F58, F59, F60 (locks disjuntos)
> **Must not run with:** F11, F19, F26, F37, F40 (TownScene/NPC/friendship/calendário)
> **Repo lock scope:** gerador da TownScene (interior da estalagem), FriendshipService (hook aditivo), calendário/projection (entradas de aniversário), docs/decisions (ADR novo)
> **Depends on:**
> - F26 (E35 — FriendshipService: presente genérico +3, caps diários)
> - F37 (E32 — calendário de eventos/festivais; projection do calendário)
> - F16 (executada — BedInteractable/dormir = day transition)
> - F11 (E26 — interiores da cidade; estalagem acessível)
> **Blocks:** N/A
> **Scope:** aniversário por NPC (calendário + presente ×2) + cama de hóspede paga na estalagem + ADR decidindo o destino do conceito "reputação".
> **Out of scope:** presentes preferidos por NPC, festas de aniversário, romance, sistema de reputação paralelo (o ADR decide o conceito, não cria sistema).

required_adrs: []
required_game_rules: [event_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

Três promessas de "cidade viva" estão soltas nas directions:

1. **Aniversários** — CITY_LAYOUT §28 coloca o Calendário da Praça mostrando "festivais,
   aniversários, luas"; a SOCIAL direction lista "presente em aniversário/festival" como
   fonte de relacionamento; e SEASONS deixa a pendência "definir se aniversário de NPC
   entra no primeiro pacote social ou fica futuro". Esta spec DECIDE: entra agora, como
   consumidor leve da amizade F26 (presente no aniversário vale ×2).
2. **Estalagem** — CITY_LAYOUT define a Taverna Panela-Funda (`bld_tavern_inn`, Gruta/
   Orlan) com "comida, rumor, hospedagem", a "Cama de hóspede" 2x2 e a regra: "Jogador
   não usa camas de NPC, exceto camas de hóspedes da estalagem se sistema de hospedagem
   for implementado". A F16 (executada) criou o `BedInteractable` na fazenda e deixou o
   hook explícito: "cama na cidade (estalagem — F19 hook)" fora de escopo. O hook está
   órfão — esta spec o fecha REUSANDO o BedInteractable com custo de diária.
3. **Reputação** — CITY_DESIGN cita "reputação" em vários pontos (hub social, visitas à
   fazenda, prefeitura, guarda) mas nenhum sistema a define; F26 implementou AMIZADE
   individual 0-5. Dois conceitos sociais concorrentes sem decisão = risco permanente de
   alguém implementar um tracker paralelo. Esta spec produz o ADR que decide (skill
   decision-rule-extraction); recomendação: amizade F26 absorve o conceito e o termo
   "reputação" é aposentado nos docs de cidade.

## Problema

Sem data de aniversário, o calendário (F20/F37) não tem o conteúdo social prometido e o
presente genérico da F26 nunca tem momento especial; sem cama na estalagem, dormir na
cidade é impossível (jogador preso ao day transition da fazenda — pior para sessões
longas de cidade/caverna) e o hook F16↔F19 segue órfão; sem o ADR, "reputação" continua
citada como se fosse sistema, convidando à duplicação do FriendshipService — exatamente
o que system-reuse-audit existe para impedir.

## Objetivo

Ao final desta spec: cada um dos 23 NPCs do roster tem data de aniversário determinística
(tabela estática season/dia — sem random), exposta na projection do calendário (F37) e no
dia: presente entregue vale pontos ×2 no FriendshipService (multiplicador aditivo, cap
diário preservado); a estalagem tem uma cama de hóspede interagível (BedInteractable F16
reusado) que cobra diária em ouro e executa o MESMO fluxo de dormir da fazenda; e o
ADR-NNNN "Reputação absorvida pela Amizade" está aprovado em docs/decisions/, com os
docs de cidade NÃO editados aqui (o ADR registra a decisão; a limpeza textual dos
directions é follow-up de docs).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md (§26 camas, §28 calendário, bld_tavern_inn)
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md (menções a reputação)
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md (23 NPCs — tabela-mestra)
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md (presente em aniversário)
.specs/a_implementar/fable/fable_26_spec_friendship_state_contract.md (API/caps)
.claude/rules/testing-quality-gate.md
.claude/skills/decision-rule-extraction/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- FriendshipService (F26 — presente genérico +3, 1×/dia/NPC; GetLevel/IsAtLeast);
- BedInteractable (F16 — dormir = day transition + SleepRecoveryCalculator);
- GameCalendarService (estação/dia); projection do calendário (F20/F37 — auditar shape);
- EconomyManager (cobrança da diária); gerador da TownScene + interiores (F11);
- roster de 23 NPCs com IDs canônicos (registry).
Não existe:
- datas de aniversário; multiplicador de presente; cama de hóspede na cidade;
  qualquer sistema de reputação (só menções em directions).
Auditar Fase 0:
- shape da projection do calendário (como F37 registra eventos — aniversário entra
  como entrada do mesmo tipo, não como sistema novo);
- ponto exato do ganho por presente no FriendshipService (multiplicador aditivo);
- interior da estalagem no gerador (F11 — onde a cama de hóspede entra; refs
  serializadas);
- preço da diária (proposta: 50g — alinhar com a régua de economia; documentar);
- próximo número de ADR livre em docs/decisions/ (ADR-0010 se sequencial).
```

## Engineering stories

```text
Como jogador, quero ver aniversários no calendário e presentear no dia certo valendo
  o dobro, para a amizade ter momentos planejáveis.
Como jogador, quero pagar uma diária e dormir na estalagem, para não ser obrigado a
  voltar à fazenda para virar o dia.
Como arquitetura, quero um ADR fechando o destino de "reputação", para ninguém criar
  um tracker social paralelo à amizade F26.
Como FriendshipService, quero o aniversário como multiplicador no MEU fluxo de
  presente, sem segundo caminho de pontos.
```

## Escopo

```text
Inclui:
- NpcBirthdayTable (estática, em código): npcId → (seasonId, dayOfSeason) para os 23
  NPCs do roster — distribuição editorial pelas 4 estações (sem random; datas estáveis);
- integração calendário: aniversários expostos na projection consumida pela aba
  Calendário/F37 (entrada do tipo existente; spoiler rules preservadas);
- presente ×2: no fluxo de presente do FriendshipService, se hoje é aniversário do
  npcId, pontos do presente ×2 (genérico +3 → +6); cap diário (1 presente/dia/NPC)
  PRESERVADO; NpcBirthdayGiftEvent (NOVO) para toast ("Hoje é aniversário de X!");
- cama de hóspede: BedInteractable reusado no interior da estalagem (gerador F11, refs
  serializadas) com gate de pagamento: confirmar → EconomyManager debita diária (50g
  proposta) → mesmo fluxo de dormir da F16; sem ouro = recusa com feedback;
- ADR-NNNN (docs/decisions/): "Reputação absorvida pela Amizade (F26)" — contexto,
  decisão (amizade individual 0-5 é o ÚNICO tracker social do v1; "reputação" vira
  termo aposentado; gates que directions atribuem a reputação leem amizade), follow-up
  de limpeza textual dos docs de cidade (delegado a tarefa de docs — não editar
  directions aqui);
- EditMode tests: tabela cobre os 23 NPCs sem duplicata de (season, dia) excessiva
  (máx. 1 NPC/dia), multiplicador só no dia certo, cap diário preservado com ×2,
  cobrança da diária (sem ouro = recusa; com ouro = debita 1×), dormir na estalagem
  dispara o mesmo day transition da fazenda.
```

## Fora de escopo

```text
Não inclui:
- presentes preferidos por NPC (catálogo de gostos — futuro, F26 já declarou);
- festa/cutscene de aniversário; convites; visitas à fazenda;
- romance/casamento (F46);
- sistema de reputação (o ADR mata o conceito — não há código de reputação);
- edição dos directions de cidade (follow-up de docs após o ADR);
- quarto privado/upgrade da estalagem (cama única de hóspede no v1).
```

## Regras de não duplicação

```text
Não criar segundo tracker social — aniversário é multiplicador DENTRO do fluxo de
presente do FriendshipService (F26).
Não criar segunda cama/fluxo de dormir — BedInteractable da F16 reusado com gate de
pagamento na frente.
Não criar segundo registro de eventos de calendário — entrada na projection existente.
Não criar código de "reputação" — o ADR decide o conceito em docs/decisions/.
```

## Critérios de aceite

### CA-1 Aniversários no calendário

- Os 23 NPCs têm data estável (tabela estática); a projection do calendário expõe o
  aniversário do dia/mês conforme o shape da F37.
- Evidência: EditMode tests da tabela (cobertura 23/23, datas válidas no calendário,
  máx. 1 aniversário/dia) + projection.

### CA-2 Presente ×2 idempotente no cap

- Presentear no aniversário dá pontos ×2; o cap 1 presente/dia/NPC continua valendo
  (2º presente no mesmo dia não pontua); fora do aniversário, pontos normais.
- Evidência: EditMode tests do multiplicador + cap com data sintética.

### CA-3 Cama da estalagem paga e funcional

- Interagir com a cama de hóspede cobra a diária 1× e executa o fluxo de dormir da F16
  (day transition + recuperação); sem ouro suficiente, recusa com feedback e sem
  débito.
- Evidência: EditMode tests de cobrança/recusa + cenário humano dormindo na cidade.

### CA-4 ADR de reputação aprovado

- docs/decisions/ADR-NNNN-reputation-absorbed-by-friendship.md existe, segue o formato
  dos ADRs do projeto, registra decisão + consequências + follow-up de docs, e nenhuma
  linha de código de "reputação" foi criada.
- Evidência: arquivo do ADR + diff sem sistema novo.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Social/
  NpcBirthdayTable.cs           (NOVO — tabela estática npcId → season/dia)
  NpcBirthdayService.cs         (NOVO — IsBirthdayToday(npcId); fonte p/ calendário e
                                 multiplicador; consulta GameCalendarService)
FriendshipService               (diff mínimo — multiplicador ×2 no fluxo de presente)
projection do calendário        (entrada de aniversário — shape F37, aditivo)
Assets/_Game/Scripts/City/
  InnBedPaymentGate.cs          (NOVO — IInteractable que cobra diária e delega ao
                                 BedInteractable/fluxo F16)
Assets/_Game/Scripts/Core/Events/NpcBirthdayGiftEvent.cs (NOVO)
gerador da TownScene/interiores (cama de hóspede na estalagem — refs serializadas)
docs/decisions/ADR-NNNN-reputation-absorbed-by-friendship.md (NOVO)
Assets/_Game/Tests/EditMode/NPC/BirthdayInnTests.cs (NOVO)
docs/validation/fable_57_spec_living_city_birthdays_inn_reputation_adr_execution_report.md
```

## Contratos

### Data contracts

- `NpcBirthdayTable`: 23 entradas {npcId, seasonId, dayOfSeason} — estática, datas
  válidas no calendário do jogo, máx. 1 NPC por dia.
- Diária da estalagem: constante (50g proposta — confirmada na Fase 0 com a régua de
  economia).

### Runtime contracts

- `NpcBirthdayService.IsBirthdayToday(npcId)`: compara com GameCalendarService —
  determinístico, sem estado salvo (re-derivável).
- FriendshipService: no ponto único de ganho por presente, pontos ×2 se
  IsBirthdayToday — cap diário INALTERADO; publica NpcBirthdayGiftEvent.
- `InnBedPaymentGate`: confirmação → EconomyManager.TrySpend(diária) → delega ao
  BedInteractable existente; falha de ouro = feedback, sem efeito.

### Event contracts

- `NpcBirthdayGiftEvent` (NOVO — npcId; toast escuta).
- Demais comunicações via eventos existentes (GameEventBus); dormir reusa o fluxo F16.

### Save contracts

- NENHUM campo novo: aniversário é derivado de tabela+calendário; o cap diário de
  presente já persiste via F26; a diária é transação econômica normal.

### UI contracts

- Aniversário aparece na aba Calendário (projection F37/F20); prompt da cama
  ("Dormir — 50g") + confirmação via fluxo modal existente; toasts via feedback
  existente. Sem tela nova.

## Sistemas afetados

```text
FriendshipService (multiplicador aditivo)
Calendário/projection (entrada de aniversário)
TownScene/interiores (cama de hóspede via gerador)
Economia (diária)
Event bus (NpcBirthdayGiftEvent)
docs/decisions (ADR novo)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/Social/** (novos)
Assets/_Game/Scripts/City/InnBedPaymentGate.cs (novo — pasta auditada na Fase 0)
FriendshipService (diff mínimo no ponto de presente — arquivo auditado na Fase 0)
projection do calendário (entrada aditiva — arquivo auditado na Fase 0)
Assets/_Game/Scripts/Core/Events/NpcBirthdayGiftEvent.cs (novo)
gerador da TownScene/interiores (cama — arquivo auditado na Fase 0)
docs/decisions/ADR-NNNN-reputation-absorbed-by-friendship.md (novo)
Assets/_Game/Tests/EditMode/NPC/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/** ; ProjectSettings/**
directions de cidade (CITY_*.md — limpeza textual é follow-up pós-ADR, não aqui)
BedInteractable.cs (reusar via gate — não modificar o fluxo de dormir)
SaveManager/seções de save (nenhum campo novo)
qualquer arquivo de "ReputationService" (proibido existir)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Shape da projection do calendário (F37); ponto de presente no FriendshipService;
interior da estalagem no gerador (F11); preço da diária; próximo número de ADR.

### Fase 1 — Aniversários
NpcBirthdayTable (23 NPCs) + NpcBirthdayService + entrada na projection + testes
(cobertura/validade/unicidade/projection).

### Fase 2 — Presente ×2
Multiplicador no FriendshipService + NpcBirthdayGiftEvent + testes (×2 só no dia,
cap preservado).

### Fase 3 — Estalagem
InnBedPaymentGate + cama no gerador (refs serializadas) + testes de cobrança/recusa +
regressão do dormir da fazenda.

### Fase 4 — ADR e fechamento
ADR-NNNN (decision-rule-extraction) + regeneração com evidência; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch10_city.
- Can run with: F54, F55, F58, F59, F60 (locks disjuntos).
- Must not run with: F11, F19, F26, F37, F40 (TownScene/NPC/friendship/calendário).
- Shared files/systems that require lock: gerador da TownScene, FriendshipService,
  projection do calendário, docs/decisions.
- Reason: toca o serviço de amizade e o gerador da cidade — superfícies da cadeia
  social/cidade; roda após F26/F37/F11 resolvidas.

## Impacto em save/load

```text
Does this change save schema? NO (aniversário re-derivável; cap diário já persiste
via F26; diária é transação econômica normal)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES — NpcBirthdayGiftEvent
Changes existing events: NO
Requires unsubscribe pattern: YES (consumidores de toast)
```

## Impacto em UI/Unity

```text
Changes UI: entrada no calendário + prompt/confirmação da cama (fluxos existentes)
Changes scenes: YES — TownScene/interior regenerado VIA GERADOR (evidência)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (tabela em código)
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: nascer um tracker de reputação paralelo no futuro.
Mitigação: ADR aprovado nesta spec é a barreira canônica; anti-regressão proíbe
ReputationService.

Risco: multiplicador furar o cap diário de presente (exploit).
Mitigação: ×2 aplicado DENTRO do fluxo com cap inalterado; teste de 2º presente.

Risco: cobrança da diária sem dormir (débito e falha do fluxo F16).
Mitigação: debitar SÓ após confirmação e delegação bem-sucedida; teste de recusa.

Risco: datas de aniversário colidindo com festivais (F37) no mesmo dia.
Mitigação: Fase 0 confere as datas de festival e desvia aniversários (tabela
editorial); teste de unicidade por dia.

Risco: dependências F26/F37/F11 ainda não executadas na ordem do batch.
Mitigação: regra spec_dependency_resolution — resolver a cadeia antes; esta spec é
P3 e roda no fim do grupo city.
```

## Rollback

```text
Remover service/tabela/gate/entrada de projection = cidade volta ao estado atual;
ADR permanece como decisão documentada (rollback do ADR = superseding ADR).
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar projection do calendário, ponto de presente F26, interior da
        estalagem, preço da diária, número do ADR.
- [ ] T002 — NpcBirthdayTable (23) + NpcBirthdayService + entrada no calendário +
        testes.
- [ ] T003 — Presente ×2 no FriendshipService + NpcBirthdayGiftEvent + testes
        (×2/cap).
- [ ] T004 — InnBedPaymentGate + cama no gerador + testes de cobrança/recusa +
        regressão do dormir.
- [ ] T005 — ADR de reputação + regeneração com evidência; csproj;
        run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (tabela/datas, multiplicador, cap, cobrança)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (presente comum F26 e dormir na fazenda F16 intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano presenteando em
  aniversário (toast + ×2) e dormindo na estalagem (débito + day transition)

## Definition of Done

```text
23 NPCs com aniversário estável visível no calendário; presente no dia vale ×2 com cap
preservado; cama de hóspede paga na estalagem reusando o fluxo F16 (hook F16↔F19
fechado); ADR de reputação aprovado (amizade absorve; termo aposentado; zero código de
reputação); nenhum campo de save novo; builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Presente genérico fora de aniversário continua +3 com cap 1×/dia/NPC (F26 intacta).
Dormir na fazenda (F16) inalterado; cama da estalagem é gate ADICIONAL.
Nenhum ReputationService/tracker social paralelo criado (ADR é canônico).
Projection do calendário compatível (entrada aditiva; spoiler rules preservadas).
Zero GameObject.Find em runtime (refs serializadas); eventos só via GameEventBus.
Directions de cidade não editadas nesta spec (follow-up de docs pós-ADR).
```
