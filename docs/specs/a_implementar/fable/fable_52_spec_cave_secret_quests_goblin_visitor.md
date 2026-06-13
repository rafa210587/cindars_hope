# SPEC — Secretas da Caverna: as 8 scq_* Autoradas + o Goblin Visitante da Fazenda

> **Spec ID:** `fable_52_spec_cave_secret_quests_goblin_visitor`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Runtime / Content / Integration
> **Domain:** Quests / Cave / NPC
> **Parallelizable:** NO (cadeia quest — QuestRegistry lock)
> **Parallel group:** N/A (ordem na cadeia quest: após F34/F36; nunca junto de F35/F51/F53)
> **Can run with:** F48/F49 (combate/itens — superfícies disjuntas)
> **Must not run with:** F10, F34, F35, F36, F43, F51, F53 (cadeia quest), F28 (diálogo), F37 (pool de eventos)
> **Repo lock scope:** QuestRegistry/geradores de quest, SecretQuestOffer (F34), CaveWanderingMerchant, interactables de monstro pacífico, pool de eventos da fazenda
> **Depends on:**
> - `fable_34_spec_quest_sources_infrastructure` (E38 — API OfferSecretQuest + source CaveSecret + aba Secretas)
> - `fable_33_spec_bestiary_data_expansion_60_creatures` (E21 — flag [NÃO-AGRESSIVA]: Scrounger King, Goblin Warchief, Silence Warden)
> - `fable_36_spec_main_quest_acts_2_4` (E41 — flags de ato; warden_offering/thrall_name alimentam o Ato 3)
> - `fable_37_spec_festivals_lunar_events_runtime` (E32 — pool de eventos aleatórios da fazenda p/ o goblin visitante)
> **Blocks:** sistema de pets futuro (gancho do dragon_egg); vendedores fixos de nível adicionais
> **Scope:** autorar as 8 quests secretas scq_* do catálogo sobre a API OfferSecretQuest da F34 (ofertantes: mercador errante + monstros pacíficos F33) e o goblin visitante ocasional da fazenda (evento raro determinístico, diálogo próprio, ligação à scq_goblin_truce).
> **Out of scope:** infraestrutura de canal (F34), contratos do Zrix (F51), sistema de pets (dragon_egg é cosmético + gancho), main quest em si (apenas alimenta flags), arte/voice.

required_adrs: [ADR-0005-cave-stable-run-and-replay.md, ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [cave_rules.md, farm_rules.md, event_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

O QUEST_CATALOG (§12, decisão Q6.1 — "a caverna também pede") fixa as 8 secretas do v1,
todas SEM marcador, descobertas "conversando, oferecendo, hesitando antes de atacar":

```text
scq_scrounger_bargain — Old Scrounger King: 3 itens Rare+ → anel + ele vira VENDEDOR fixo do nível.
scq_merchant_list_1/2/3 — mercador errante pede 5 glowcap / 3 frost_core / 1 wyrmling_scale
  → desconto permanente de 10% + item raro (1× por run cada, estável por seed).
scq_warden_offering — oferecer Água Viva ao Silence Warden → pacífico para sempre +
  nymirian_engraving (alimenta o Ato 3 sem ser obrigatória).
scq_goblin_truce — vencer o Duelo ritual do Warchief SEM que o pack morra → banda neutra
  por 1 run + warchief_crest (o goblin da fazenda menciona seu nome).
scq_thrall_name — um Thrall purificado sussurra um nome → levar à Mara (lore do Ato 3).
scq_dragon_egg — o ovo frio no ninho da Ashwing → chocar na incubadora da Nimble
  (cosmético no v1; gancho do sistema de pets futuro).
```

As regras de conexão (§13) mandam: "as secretas ALIMENTAM a main (engraving, o nome do
thrall) sem nunca serem obrigatórias". E a decisão humana Q1.1 (FABLE_DECISOES §1) aprova
o **goblin visitante ocasional na FAZENDA** "para diálogo + quest em algum ato" — raças
malvistas não residem na cidade, mas o mundo conversa com a fazenda.

A F34 entrega a infraestrutura: `SecretQuestOffer.OfferSecretQuest(questId)` como ponto
único de oferta, consumido pelo mercador errante (15% determinístico) e por interactables
de monstro pacífico (flag [NÃO-AGRESSIVA] da F33: Old Scrounger King, Goblin Warchief,
Silence Warden); secretas só listam no log após descobertas. Esta spec é o CONTEÚDO: as 8
definições, condições, recompensas e os ofertantes ligados — mais o goblin visitante como
evento raro da fazenda (pool F37 ou chance determinística em DayStarted).

## Problema

Sem esta spec, a API de secretas da F34 não tem nenhum consumidor de conteúdo: as
criaturas não-agressivas da F33 são apenas bichos que não atacam, o mercador errante só
vende, o Ato 3 perde os afluentes opcionais (engraving, nome do thrall), e a decisão Q1.1
do goblin visitante fica sem implementação. A 5ª fonte do catálogo existe como canal e
fica vazia — 8 das ~86 quests do v1 mortas.

## Objetivo

Ao final desta spec, cada uma das 8 scq_* deve ser descobertável e completável pelo
ofertante canônico (mercador errante → merchant_list_1/2/3; Scrounger King →
scrounger_bargain; Silence Warden → warden_offering; Goblin Warchief → goblin_truce;
Thrall purificado → thrall_name; ninho da Ashwing → dragon_egg), com recompensas e efeitos
de mundo do catálogo (vendedor fixo, desconto permanente, pacífico para sempre, banda
neutra 1 run, flags que alimentam o Ato 3, cosmético do ovo) — e o goblin visitante deve
aparecer raramente na fazenda (determinístico por dia-seed), com diálogo próprio que
referencia a scq_goblin_truce. Tudo sobre a API F34, sem canal novo e sem tornar nada
obrigatório para a main.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md (§12 secretas, §13 conexões)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§1 Q1.1 goblin visitante; raças)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (fichas não-agressivas; Ashwing)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/npc-dialogue-authoring/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (pós-F34/F33/F36/F37):
- SecretQuestOffer.OfferSecretQuest(questId) — ponto único de oferta (F34);
- QuestRegistry/QuestManager/QuestFlagService + instâncias/recompensas (F34);
- CaveWanderingMerchant (aparição determinística; oferta secreta 15% via F34);
- criaturas F33 com flag não-agressiva (Scrounger King, Goblin Warchief, Silence Warden)
  e fichas (Ashwing/ninho, Thrall purificável);
- flags de ato (F36) e pool de eventos aleatórios diários (F37);
- diálogo condicional (F28) e roster de NPCs da fazenda/cidade.
Não existe:
- as 8 definições scq_* (condições/recompensas/efeitos de mundo);
- interactable de oferta nos monstros pacíficos (conversa/oferta em vez de combate);
- efeitos de mundo: vendedor fixo de nível (Scrounger), desconto permanente do mercador,
  Warden pacífico permanente, banda neutra 1 run, incubadora da Nimble (cosmético);
- goblin visitante da fazenda (evento raro + diálogo + ligação à truce).
Auditar Fase 0:
- shape do interactable de criatura pacífica (F33 entregou diálogo/inspeção? criar mínimo);
- como o desconto do mercador é aplicado (shop pricing — campo existente?);
- mecanismo de purificação de Thrall (existe? se não, condição da scq_thrall_name usa o
  gancho disponível e documenta);
- pool F37 aceita evento "goblin_visitor"? (preferência: pool F37; fallback: chance
  determinística própria em DayStarted);
- registro de "duelo sem matar o pack" (kills por encontro — evento de morte por inimigo).
```

## Engineering stories

```text
Como jogador, quero descobrir que o rei dos catadores negocia — e ganhar um vendedor no fundo da caverna.
Como jogador, quero que oferecer Água Viva ao Warden mude o mundo para sempre (e ajude o Ato 3).
Como jogador, quero vencer o duelo do Warchief sem chacinar o pack — e ouvir um goblin na
minha fazenda mencionar meu nome.
Como main quest, quero afluentes opcionais (engraving, nome) sem nunca virarem pré-requisito.
Como mundo, quero segredos que só existem para quem hesita antes de atacar.
```

## Escopo

```text
Inclui:
- 8 definições scq_* (IDs canônicos do catálogo) registradas como source CaveSecret,
  descobertas APENAS via OfferSecretQuest (nunca listadas antes);
- ofertantes: CaveWanderingMerchant oferta merchant_list_1/2/3 em sequência (1× por run
  cada, estável por seed — regra do catálogo); interactable de oferta nos pacíficos F33
  (prompt de conversa; Scrounger/Warden/Warchief); ninho da Ashwing (interactable de
  coleta do ovo, 1×); Thrall purificado oferece thrall_name (gancho auditado na Fase 0);
- condições/entregas: scrounger_bargain = entregar 3 itens raridade Rare+ (validação por
  raridade de item F32); merchant_lists = 5 glowcap / 3 frost_core / 1 wyrmling_scale;
  warden_offering = entregar item_agua_viva; goblin_truce = vencer duelo com 0 mortes no
  pack (rastreio por evento de morte durante o encontro); thrall_name = levar "o nome" à
  Mara (flag + diálogo); dragon_egg = ovo → incubadora da Nimble (timer de dias; cosmético);
- efeitos de mundo (flags estáveis persistidas): scrounger_vendor_unlocked(level),
  merchant_discount_10 (aplicado no pricing do mercador), warden_peaceful_forever,
  goblin_band_neutral(run corrente — expira em nova run), warchief_crest/anel/itens raros
  (recompensas F32), nymirian_engraving + thrall_name_flag (consumidos pelo Ato 3 F36);
- goblin visitante da fazenda: evento raro no pool F37 (preferência) ou chance
  determinística StableHash(worldSeed|dia) ~5% em DayStarted; spawn de NPC goblin na
  fazenda por 1 dia (gerador/bootstrap — sem YAML), diálogo próprio (F28 pool) com
  variações: padrão, pós-truce (menciona o nome do jogador), durante ato avançado;
- EditMode tests: oferta 1×/run estável por seed (merchant lists), descoberta só via API
  (nunca pré-listada), condição de raridade Rare+, duelo sem morte do pack (eventos
  sintéticos), idempotência de recompensas/efeitos com save/load, chance determinística
  do goblin por dia-seed, flags de mundo persistidas.
```

## Fora de escopo

```text
Não inclui:
- infraestrutura de canal/oferta (F34) e contratos do Zrix (F51);
- sistema de pets (dragon_egg fica cosmético + flag de gancho);
- combate/AI do duelo do Warchief (encontro existente F33/F05 — esta spec só rastreia
  a condição "pack vivo");
- residência de goblin na cidade (Q1.1 PROÍBE — visitante da fazenda apenas);
- arte final/voice; minigame de incubadora (timer simples de dias).
```

## Regras de não duplicação

```text
Toda oferta passa por SecretQuestOffer (F34) — nenhum caminho paralelo de descoberta.
Recompensas/XP pela fórmula escalada F34 (ponto único) + itens F32 por ID.
Diálogos pelo sistema condicional F28 (pools/flags) — não inventar sistema de fala novo.
Evento do goblin pelo pool F37 se disponível — não criar segundo scheduler de eventos.
Flags pelo QuestFlagService — não criar repositório novo de estado de mundo.
IDs canônicos scq_* — nunca renomear.
```

## Critérios de aceite

### CA-1 As 8 descobertas pelo ofertante certo

- Cada scq_* só entra no log após oferta do ofertante canônico via API; aba Secretas
  lista apenas descobertas.
- Evidência: EditMode tests por quest (oferta sintética → descoberta) + projection.

### CA-2 Merchant lists estáveis por seed

- As 3 listas são ofertadas em sequência, cada uma 1× por run, mesmo resultado para o
  mesmo seed/run; completar concede desconto permanente 10% aplicado no pricing real.
- Evidência: EditMode tests (sequência/1×/seed) + teste do pricing com flag.

### CA-3 Efeitos de mundo persistem

- Warden pacífico para sempre, vendedor do Scrounger e desconto sobrevivem a save/load;
  banda neutra do goblin_truce expira ao iniciar NOVA run (e não antes).
- Evidência: round-trip tests das flags + teste de expiração por nova run.

### CA-4 Duelo honesto

- goblin_truce só completa se o Warchief for derrotado com 0 mortes no pack durante o
  encontro; qualquer morte invalida (sem softlock — duelo reofertável na próxima run).
- Evidência: EditMode tests com eventos sintéticos de morte/vitória.

### CA-5 Goblin visitante determinístico

- Mesmo worldSeed + mesmo dia = mesma decisão de visita; o goblin aparece na fazenda com
  diálogo próprio e variação pós-truce; nunca aparece na cidade.
- Evidência: EditMode test da chance por dia-seed + pools de diálogo por flag.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/SecretQuests/
  SecretQuestCatalog.cs          (NOVO — as 8 definições/condições/recompensas)
  SecretQuestWorldEffects.cs     (NOVO — flags de mundo: vendor/desconto/pacífico/neutro)
  PackDuelTracker.cs             (NOVO — duelo sem morte do pack, puro/testável)
Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs (ADITIVO — sequência das 3 listas)
interactable de oferta de criatura pacífica (NOVO ou aditivo — auditado Fase 0)
Assets/_Game/Scripts/Farm/ (GoblinVisitorEvent — pool F37 ou DayStarted determinístico)
diálogos do goblin/Mara/Nimble (F28 pools — aditivo)
Assets/_Game/Tests/EditMode/Quests/SecretQuestsTests.cs (NOVO)
docs/validation/fable_52_spec_cave_secret_quests_goblin_visitor_execution_report.md
```

## Contratos

### Data contracts

- 8 QuestDefinitions (IDs canônicos, source CaveSecret, QuestLevel por banda do ofertante).
- Flags estáveis: scrounger_vendor_unlocked_<level>, merchant_discount_10,
  warden_peaceful_forever, goblin_band_neutral_run, warchief_crest_owned,
  nymirian_engraving_owned, thrall_name_known, dragon_egg_incubating/hatched — strings/bools.

### Runtime contracts

- Ofertas: consumidores chamam `SecretQuestOffer.OfferSecretQuest(id)` (F34) — mercador
  (sequência 1/2/3 por run-seed), interactables pacíficos, ninho, Thrall.
- `PackDuelTracker`: Begin(encounterId, packIds[]) / OnEnemyDied(id) / Evaluate() — puro.
- `SecretQuestWorldEffects`: aplica/consulta flags; pricing do mercador lê
  merchant_discount_10; spawner/AI lê warden_peaceful_forever e goblin_band_neutral_run
  (integração cirúrgica nos pontos existentes).
- GoblinVisitor: decisão por pool F37 (evento "goblin_visitor") ou
  StableHash(worldSeed|dia) — spawn de NPC temporário na fazenda via bootstrap/gerador.

### Event contracts

- `SecretQuestDiscoveredEvent(questId)` (se F34 ainda não publicar — auditar; não duplicar).
- Consome: morte de inimigo, vitória de encontro, DayStartedEvent, nova run — via bus,
  unsubscribe obrigatório.

### Save contracts

- Flags pelo QuestFlagService (persistido); incubadora = flag + dia de início (ints);
  banda neutra = flag de RUN (limpa em nova run — mesmo ciclo do CaveRunSeed). Sem refs
  Unity; sem seção nova.

### UI contracts

- Aba Secretas (F34) lista descobertas; diálogos via F28; toast de descoberta via evento.
  Sem tela nova.

## Sistemas afetados

```text
Quests (definições/flags/ofertas — consumo F34)
Cave runtime (mercador, interactables pacíficos, ninho — cirúrgico)
Farm (goblin visitante 1 dia)
NPC/diálogo (pools F28: goblin, Mara, Nimble)
Economia (desconto no pricing do mercador)
Save (flags persistidas) / Event bus
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/SecretQuests/** (novos)
Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs (aditivo — sequência de listas)
interactable de criatura pacífica (arquivo auditado na Fase 0 — aditivo ou novo)
Assets/_Game/Scripts/Farm/** (GoblinVisitor — evento/spawn temporário)
pools de diálogo F28 (aditivo) e pool de eventos F37 (entrada nova apenas)
pontos de pricing do mercador e spawn/AI (cirúrgico — flags lidas)
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/** ; ProjectSettings/**
SecretQuestOffer/QuestManager core (consumo — nunca reescrever o canal F34)
Geradores procedurais/seeds da caverna (ADR-0005)
Conteúdo F35/F36/F51/F53; cidade (goblin NUNCA na cidade — Q1.1)
SaveManager core
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Interactable pacífico (F33), pricing do mercador, mecanismo de Thrall purificado, pool
F37, rastreio de mortes por encontro, evento de descoberta na F34.

### Fase 1 — Catálogo e ofertas
8 definições scq_* + ofertas ligadas (mercador sequência 1×/run por seed; pacíficos;
ninho; Thrall) + testes de descoberta/sequência.

### Fase 2 — Condições e efeitos de mundo
Condições (Rare+, entregas, PackDuelTracker, incubadora) + SecretQuestWorldEffects
(vendor/desconto/pacífico/neutro-por-run) + integrações cirúrgicas + testes.

### Fase 3 — Goblin visitante
Evento raro determinístico (pool F37 ou dia-seed) + spawn temporário na fazenda +
diálogos F28 (padrão/pós-truce/ato) + testes de determinismo.

### Fase 4 — Fechamento
Round-trips de flags; regressões (main nunca depende de secreta); csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F48/F49.
- Must not run with: F10/F34/F35/F36/F43/F51/F53 (cadeia quest), F28 (pools de diálogo),
  F37 (pool de eventos), F09/F44 (caverna).
- Shared files/systems that require lock: QuestRegistry/ofertas, CaveWanderingMerchant,
  pools F28/F37, pricing do mercador.
- Reason: escreve conteúdo na cadeia quest e toca mercador/diálogo/eventos — superfícies
  compartilhadas com toda a família social.

## Impacto em save/load

```text
Does this change save schema? NO além dos mecanismos existentes (flags QuestFlagService;
instâncias F34)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO (flags/ints/IDs)
```

## Impacto em eventos

```text
Adds events: CONDITIONAL — SecretQuestDiscoveredEvent só se F34 não o publicar (auditar)
Changes existing events: NO (consumo: morte/vitória/DayStarted/nova run)
Requires unsubscribe pattern: YES (trackers/efeitos assinam o bus)
```

## Impacto em UI/Unity

```text
Changes UI: NO (aba Secretas F34; diálogos F28; toasts existentes)
Changes scenes: NO (spawn temporário via bootstrap/gerador — sem YAML manual)
Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador apenas (definições/diálogos, se asset-based)
Requires Play Mode final validation: YES (descobrir 1 secreta + visita do goblin)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: secreta virar pré-requisito de main (violação §13).
Mitigação: Ato 3 CONSOME flags se existirem (caminho alternativo garantido — F36);
teste de regressão da main sem nenhuma secreta.

Risco: duelo do Warchief sem rastreio confiável de pack (mortes fora do encontro).
Mitigação: PackDuelTracker com escopo por encounterId e janela begin/end; teste com
mortes fora do escopo (não invalidam).

Risco: desconto permanente aplicado 2× ou perdido em reload.
Mitigação: flag única consultada no ponto de pricing (não acumulativa) + round-trip test.

Risco: goblin spawnar na cidade ou em dia de festival quebrando cena.
Mitigação: spawn restrito à FarmScene; se pool F37 sortear conflito, regra de prioridade
do pool decide (F37 é dona); teste do filtro de cena.

Risco: warden_peaceful_forever conflitar com stable run (spawn plan por seed).
Mitigação: flag aplicada na materialização do comportamento (AI pacífica), nunca no
spawn plan/seed (ADR-0005); documentado e testado.
```

## Rollback

```text
Remover o registro das 8 definições e os pontos de oferta desativa o conteúdo; flags já
gravadas ficam inertes (efeitos de mundo desligados com os consumidores). O goblin some
removendo a entrada do pool/chance. Canal F34 e main quest intactos; nenhum save real
apagado.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar interactable pacífico, pricing do mercador, Thrall, pool F37, rastreio de mortes, evento F34.
- [ ] T002 — 8 definições scq_* + ofertas (mercador sequência/seed; pacíficos; ninho; Thrall) + testes.
- [ ] T003 — Condições (Rare+, entregas, PackDuelTracker, incubadora) + SecretQuestWorldEffects + integrações + testes.
- [ ] T004 — Goblin visitante (evento determinístico + spawn fazenda + diálogos F28) + testes.
- [ ] T005 — Round-trips/regressões (main independe de secretas); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (sequência por seed, chance do goblin, duelo, flags,
  idempotência)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — descobrir e
  completar 1 secreta; ver o goblin na fazenda)
- Requires regression test: YES (main completável sem nenhuma secreta; mercador continua
  vendendo; pacíficos continuam não-agressivos sem quest)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com
  scq_scrounger_bargain completa e visita do goblin observada

## Definition of Done

```text
8 scq_* descobríveis/completáveis pelos ofertantes canônicos via API F34; recompensas e
efeitos de mundo do catálogo persistidos (vendor, desconto, pacífico, neutro-por-run,
engraving/nome p/ Ato 3, ovo cosmético); goblin visitante raro determinístico na fazenda
com diálogo próprio e variação pós-truce; main nunca depende de secreta; builds 0E;
execution report com Spec Compliance Matrix.
```

## Anti-regressão

```text
IDs canônicos scq_* preservados; descoberta SÓ via OfferSecretQuest (F34).
Secretas jamais obrigatórias para main/side (regra §13 testada).
Goblin NUNCA na cidade (Q1.1); spawn restrito à fazenda.
Spawn plan/seeds da caverna intocados (ADR-0005); pacífico = comportamento, não spawn.
Flags só via QuestFlagService; tipos simples; saves legados com defaults.
Eventos só via GameEventBus; unsubscribe nos trackers; nenhum GameObject.Find.
```
