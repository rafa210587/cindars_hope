# SPEC — Mundo: 8 Festivais Sazonais + Eventos de Pico Lunar + Eventos Aleatórios

> **Spec ID:** `fable_37_spec_festivals_lunar_events_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P2
> **Type:** Runtime / Content
> **Domain:** World / Time
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A (locks condicionais de cena/cidade)
> **Can run with:** specs que não toquem WorldEventService, CreateMvpTownScene nem consumidores de DayStartedEvent
> **Must not run with:** F19, F11 (cena/cidade locks)
> **Repo lock scope:** WorldEventService (novo), CreateMvpTownScene (decoração de festival), DayStartedEvent consumers
> **Depends on:**
> - F15 (clima), WAVE 02 (calendário/luas), F32 (itens de festival)
> **Blocks:** F20 (detalhe do dia mostra), F25 (Mirena prato de festival)
> **Scope:** calendário de festivais + efeitos de pico lunar + pool de eventos aleatórios.
> **Out of scope:** minigames de festival (barracas dão itens/buffs por interação simples), decoração rica (placeholder), aniversários de NPC.

required_adrs: []
required_game_rules: [time_rules.md]

---

# /speckit.specify

## Contexto

QUEST_CATALOG §festivais define 8 festivais — 2 por estação, em dias fixos do calendário
de 28 dias: Plantio (Primavera d7), Caravana (Primavera d21), Luas (Verão d14), Torneio
(Verão d28), Colheita (Outono d7), Véus (Outono d21), Vigília (Inverno d14) e Ano Novo
(Inverno d28). A decisão Q5.1 fixa o modelo lunar: cada lua fica visível 8-10 dias por
estação com 1 noite de PICO, e os picos têm efeitos mecânicos nomeados (Lua de Cinza =
+spawn undead, Lua Verde = crops +1 estágio...). A decisão Q5.2 aprova MUITOS eventos
aleatórios de mundo (mercador raro, chuva de estrelas, infestação...).

Nada disso existe no repo. O calendário (GameCalendarService/LunarCycleService — WAVE 02)
e o clima (WorldWeatherService — F15) já rodam e publicam DayStartedEvent; falta a camada
de EVENTOS DE MUNDO que torna os dias diferentes entre si. Objetivo desta spec: um
WorldEventService que agenda festivais (determinístico pelo calendário), aplica efeitos
de pico lunar e sorteia eventos aleatórios diários de forma determinística por dia-seed
(StableHash(worldSeed|dia)) — tudo derivável, SEM save novo.

## Problema

Sem festivais, picos lunares e eventos aleatórios, o calendário e as luas (WAVE 02) são
decorativos: nenhum dia é mecanicamente diferente do outro. O detalhe do dia (F20) não
tem o que mostrar, a Mirena não tem prato de festival para servir (F25), as falas
condicionais de festival (F28) nunca disparam, e as decisões Q5.1/Q5.2 ficam sem
materialização. Se cada efeito de pico for implementado de forma espalhada (ifs dentro de
spawn/farm/economia), o acoplamento cresce e o determinismo fica inauditável.

## Objetivo

Ao final desta spec, o projeto deve ter um WorldEventService que, a cada DayStartedEvent,
resolve {festival hoje?, pico lunar hoje?, evento aleatório?} de forma 100% determinística
(calendário + StableHash(worldSeed|dia)), expõe flags/efeitos consumíveis pelos sistemas
existentes via hooks nomeados, monta/desmonta 3 barracas de festival na praça e anuncia
tudo por toast/mural — sem persistir nenhum estado novo (tudo recomputável) e sem alterar
clima (F15 continua dono do clima).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (§festivais)
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md (luas)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§5 luas/eventos)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- GameCalendarService / LunarCycleService (WAVE 02) — donos de data/estação/lua;
- DayStartedEvent — gatilho único de início de dia;
- WorldWeatherService (F15) — dono do clima (não duplicar);
- CaveEnemySpawnPlanner — ponto de hook para +spawn (pico de Cinza/infestação);
- FarmPlot — ponto de hook para avanço de crop (pico de Verde);
- praça da cidade (gerador CreateMvpTownScene) — local das barracas.
Não existe:
- festivais (calendário, flags, barracas);
- efeitos de pico lunar (nenhum hook mecânico);
- eventos aleatórios diários (pool, pesos, sorteio por seed).
Auditar Fase 0:
- API real do LunarCycleService — o conceito de "noite de PICO" é modelável com o ciclo
  existente? Mapear os 4 picos nomeados do catálogo ao modelo real e documentar qualquer
  adaptação no execution report;
- ponto de extensão da praça no gerador (onde as 3 barracas nascem/desmontam);
- StableHash existente (padrão do projeto) para o sorteio diário.
```

## Engineering stories

```text
Como jogador, quero que dias de festival sejam visivelmente diferentes (barracas, falas,
  prato especial), para o calendário ter significado.
Como sistema de spawn da caverna, quero um hook nomeado de pico lunar, para aplicar
  +spawn undead sem conhecer o WorldEventService.
Como sistema de save, quero que festivais/picos/eventos sejam deriváveis de
  calendário+seed, para não persistir nada novo.
Como UI (F20/mural), quero a lista dos próximos festivais CONHECIDOS, para o jogador
  planejar a semana sem spoiler de eventos desconhecidos.
```

## Escopo

```text
Inclui:
- WorldEventService (bootstrap): consome DayStartedEvent → resolve {festival hoje?,
  pico lunar hoje?, evento aleatório? (StableHash(worldSeed|dia))};
- 8 festivais (dias fixos do catálogo — 2/estação: Plantio P-d7, Caravana P-d21,
  Luas V-d14, Torneio V-d28, Colheita O-d7, Véus O-d21, Vigília I-d14, Ano Novo I-d28):
  flag festival_active(id) → F28 falas, Mirena prato especial (F25), 3 barracas na praça
  (interactables temporários: comida grátis, jogo de pesca simplificado = item aleatório,
  brinde) — spawn/despawn pelo serviço;
- picos lunares (efeitos do catálogo §luas): Cinza=+30% undead no spawn dessa noite,
  Verde=crops avançam +1 estágio, Âmbar=+10% preços de venda no dia, Pálida=fonte dá
  Água Viva extra (F17) — cada efeito = hook nomeado no sistema alvo;
- eventos aleatórios diários (pool inicial 8: mercador raro na praça, chuva de estrelas
  [buff sorte +5% loot raro], infestação [+20% spawn banda 1], viajante com fofoca [lore],
  preço de cultivo em alta [+25% venda de 1 cultivo], dia nublado calmo [-spawn], achado
  na praia do lago [item], dia de feira [stock dobrado]) — peso e efeito data-driven;
- anúncio: toast no início do dia + mural (F34) lista próximos festivais CONHECIDOS;
- save: NO (tudo deriva de calendário+seed — recomputável);
- EditMode tests: agenda determinística, resolução de pico, pool de eventos por seed,
  flags de festival, efeitos com serviços sintéticos.
```

## Fora de escopo

```text
Não inclui:
- minigames reais de festival (barracas dão item/buff por interação simples);
- decoração rica de festival (placeholder visual);
- aniversários de NPC;
- quests de festival (fq_* da Parte F do catálogo — specs de quest consomem as flags);
- mudanças no sistema de clima (F15 é o dono);
- balance final dos efeitos (valores do catálogo são a régua inicial).
```

## Regras de não duplicação

```text
Não duplicar clima — WorldWeatherService (F15) é o dono; eventos de mundo não geram clima.
Não criar segundo calendário — GameCalendarService/LunarCycleService são a fonte de
  data/estação/lua; o WorldEventService só DERIVA deles.
Não criar save novo — festivais/picos/eventos são deriváveis (calendário + worldSeed).
Não espalhar ifs de efeito — 1 hook nomeado por sistema alvo (padrão F23), nunca o
  sistema alvo consultando o WorldEventService por conta própria em vários pontos.
Não criar segundo fluxo de toast — GameplayFeedbackService/fluxo existente anuncia.
```

## Critérios de aceite

### CA-1 — Festival no dia certo

- No dia fixo do catálogo o festival ativa: flag festival_active(id) setada, 3 barracas
  na praça interagíveis; no dia seguinte tudo despawna e a flag limpa.
- Evidência: EditMode test de agenda (data → festival esperado para os 8) + cenário
  humano com 1 festival via debug time skip.

### CA-2 — Pico de Lua Verde avança crops

- Na noite de pico de Lua Verde, crops plantados avançam +1 estágio via hook nomeado.
- Evidência: EditMode test com FarmPlot sintético (estágio antes/depois do pico).

### CA-3 — Determinismo do evento aleatório

- Mesmo worldSeed + mesmo dia = mesmo evento aleatório (ou ausência), em qualquer
  recomputação; seeds diferentes variam.
- Evidência: EditMode test de determinismo (StableHash(worldSeed|dia)) com casos fixos.

### CA-4 — Mural sem spoiler

- O mural (F34) lista os próximos festivais CONHECIDOS; festivais/eventos desconhecidos
  permanecem ocultos (spoiler gate alinhado a F20).
- Evidência: EditMode test do filtro de conhecidos + inspeção no cenário humano.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/World/
  WorldEventService.cs              (NOVO — resolução diária determinística)
  WorldEventDefinitions.cs          (NOVO — tabelas data-driven: festivais, picos, pool)
Assets/_Game/Scripts/ (hooks aditivos nos sistemas alvo)
  Cave (spawn), Farm (crop), Economy (preços/stock), Fonte (Água Viva — F17)
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateMvpTownScene.cs             (ADITIVO — pontos das 3 barracas na praça)
Assets/_Game/Tests/EditMode/World/
  WorldEventsTests.cs               (NOVO)
docs/validation/
  fable_37_spec_festivals_lunar_events_runtime_execution_report.md
```

## Contratos

### Data contracts
Tabela de festivais (id, estação, dia, nome, prato F25, brinde) com os 8 do catálogo;
tabela de picos lunares (id da lua, efeito nomeado, magnitude: Cinza +30% undead,
Verde +1 estágio, Âmbar +10% venda, Pálida Água Viva extra); pool de 8 eventos
aleatórios com peso e payload de efeito — tudo data-driven em WorldEventDefinitions.

### Runtime contracts
WorldEventService resolve no DayStartedEvent: festival do dia (lookup calendário), pico
lunar (consulta LunarCycleService), evento aleatório (StableHash(worldSeed|dia) → roll no
pool ponderado). Expõe leitura: IsFestivalActive(id), ActiveLunarPeak, ActiveWorldEvent,
NextKnownFestivals(n). Cada efeito mecânico entra por 1 hook nomeado no sistema alvo
(padrão F23) — o sistema alvo não conhece o serviço.

### Event contracts
FestivalStartedEvent / WorldEventStartedEvent (novos — publicados na resolução diária;
consumidos por toast/mural/F28). Consome DayStartedEvent (existente, sem mudança).

### Save contracts
N/A — nenhum estado persiste; festival/pico/evento são recomputáveis de calendário +
worldSeed a qualquer momento (decisão explícita da spec: SEM save).

### UI contracts
Toast no início do dia (fluxo de feedback existente); mural (F34) lista próximos
festivais CONHECIDOS (spoiler gate F20); barracas são interactables temporários.

## Sistemas afetados

```text
World/Time (novo WorldEventService — deriva de calendário/luas)
Cave spawn (hook +undead/infestação/-spawn)
Farm (hook crop +1 estágio)
Economy (hook preços/stock — Âmbar, cultivo em alta, dia de feira)
Fonte (hook Água Viva extra — F17)
Diálogo F28 (flag festival_active) e Mirena F25 (prato especial) — consumidores
UI (toasts, mural F34, detalhe do dia F20 — consumidores)
Cena da cidade (pontos de barraca via gerador)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/World/** (novos: WorldEventService, WorldEventDefinitions)
Assets/_Game/Scripts/Cave/**, Farm/**, Economy/** (hooks nomeados aditivos)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (barracas — aditivo)
Assets/_Game/Tests/EditMode/World/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (cena só via gerador/AssetDatabase)
Packages/**
ProjectSettings/**
GameCalendarService / LunarCycleService core (consumir, não alterar contrato)
WorldWeatherService (F15 — dono do clima, não tocar)
SaveManager / seções de save (spec não persiste nada)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
API real do LunarCycleService (pico modelável? mapear os 4 picos nomeados); ponto de
extensão da praça; StableHash padrão do projeto; fluxo de toast/mural existente.
### Fase 1 — Definições e resolução determinística
WorldEventDefinitions (8 festivais com datas do catálogo, 4 picos, pool de 8 eventos
com pesos) + WorldEventService resolvendo o dia; testes de agenda/determinismo.
### Fase 2 — Festivais na praça
Flags festival_active(id) + 3 barracas (spawn/despawn pelo serviço, pontos do gerador) +
FestivalStartedEvent + anúncio por toast.
### Fase 3 — Picos lunares e eventos aleatórios
4 hooks nomeados (spawn/farm/economia/fonte) + pool de eventos com efeitos data-driven +
WorldEventStartedEvent; testes com serviços sintéticos.
### Fase 4 — Mural, testes e closeout
Mural lista próximos festivais conhecidos (spoiler gate); WorldEventsTests completos;
csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: N/A (locks condicionais de cena/cidade)
- Can run with: specs que não toquem WorldEventService, CreateMvpTownScene nem
  consumidores de DayStartedEvent
- Must not run with: F19, F11 (cena/cidade locks — mesmo gerador/praça)
- Shared files/systems that require lock: WorldEventService (novo), CreateMvpTownScene
  (decoração de festival), DayStartedEvent consumers
- Reason: as barracas tocam o gerador da cidade (conflito com F19/F11) e a resolução
  diária adiciona consumidor central de DayStartedEvent.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Tudo deriva de calendário + worldSeed (recomputável após load — teste de determinismo).
```

## Impacto em eventos

```text
Adds events: YES — FestivalStartedEvent, WorldEventStartedEvent
Changes existing events: NO (DayStartedEvent apenas consumido)
Requires unsubscribe pattern: YES (WorldEventService e consumidores de toast/mural)
```

## Impacto em UI/Unity

```text
Changes UI: toasts + mural (lista de festivais conhecidos) — consumidores existentes
Changes scenes: via gerador (pontos de barraca) | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador (evidência obrigatória se houver)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: hooks de efeito espalhados pelos sistemas alvo (acoplamento invisível).
Mitigação: 1 ponto único nomeado por sistema (padrão F23) + teste por hook.
Risco: modelo de pico não mapear na API real do LunarCycleService.
Mitigação: auditoria Fase 0 mapeia os 4 picos ao ciclo existente; adaptação documentada.
Risco: sorteio diário não determinístico (RNG global).
Mitigação: StableHash(worldSeed|dia) exclusivo + teste de recomputação idêntica.
Risco: barracas órfãs (festival termina e interactables ficam).
Mitigação: despawn pelo serviço no dia seguinte + CA-1 verifica limpeza.
Risco: spoiler de festivais/eventos desconhecidos no mural.
Mitigação: filtro de CONHECIDOS (spoiler gate F20) + teste do filtro.
```

## Rollback

```text
Serviço desligado (não registrado no bootstrap) = mundo atual sem eventos.
Hooks nomeados aditivos removíveis sem afetar os sistemas alvo.
Nenhum estado salvo para limpar (tudo derivável).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar lunar API + praça (pontos de barraca) + StableHash padrão.
- [ ] T002 — WorldEventDefinitions + WorldEventService (resolução determinística do dia)
        + testes de agenda/determinismo.
- [ ] T003 — 8 festivais: flags + 3 barracas (spawn/despawn) + FestivalStartedEvent.
- [ ] T004 — 4 picos lunares (hooks nomeados em spawn/farm/economia/fonte) + testes.
- [ ] T005 — Pool de 8 eventos aleatórios + mural/toasts (spoiler gate); csproj;
        run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (agenda, picos, sorteio por seed)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (dia normal — sem festival/pico/evento — inalterado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano em 1 festival e
  1 pico lunar (debug time skip)

## Definition of Done

```text
8 festivais nos dias fixos do catálogo com flags/barracas/anúncio; 4 picos lunares com
efeitos nomeados via hooks únicos; pool de 8 eventos aleatórios determinístico por
StableHash(worldSeed|dia); mural com spoiler gate; ZERO save novo (tudo derivável).
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Dia normal (sem festival/pico/evento) idêntico ao comportamento atual.
WorldWeatherService (F15) e calendário/luas (WAVE 02) sem mudança de contrato.
Nenhum estado novo persistido; save schema intacto.
Hooks nomeados não alteram comportamento dos sistemas alvo fora da janela do evento.
Nenhum GameObject.Find em runtime; comunicação via GameEventBus.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. EVENTO DE SECA (5.5): aprovado — em seca, a chuva pode falhar (sobrepõe o clima do dia;
   integra com F15/RainIrrigationRunner). Adicionar ao pool de eventos.
2. LOJA NOTURNA (6.2-B): só abre em PICO DE NYX + quest de desbloqueio — integrar com os
   picos lunares desta spec.
```
