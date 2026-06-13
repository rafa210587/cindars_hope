# SPEC — Mundo: Marcas dos Deuses (altares com bônus diário em caverna, cidade e fazenda)

> **Spec ID:** `fable_68_spec_world_god_marks_altars`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Runtime + Content
> **Domain:** World / Cave / City / Farm
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem geração procedural da caverna nem cenas de cidade/fazenda
> **Must not run with:** F22 (special rooms da caverna), F23 (relíquias — regra de não-stack), F09/F19 (cenas cidade), F40 (jardim das estátuas)
> **Repo lock scope:** `Cave/Runtime/**` (special rooms), `World/Altars/**` (NOVO), `Player/Conditions/**` (buffs diários)
> **Depends on:**
> - F22/E22 (special rooms da caverna — as Marcas de caverna SÃO special rooms)
> - F23/E23 (relíquias divinas — regra de não-stack por deus)
> - E06 (PlayerConditionService — executada; buffs diários entram como condições)
> - F19/E25 (cena da cidade 48×42 — âncoras dos santuários urbanos)
> - F37/E32 (calendário/eventos — picos lunares e festivais condicionam Marcas 9/10)
> **Blocks:** N/A
> **Scope:** 11 Marcas dos Deuses (5 caverna + 5 cidade/fazenda + exceção de Anya), oração 1/dia, bônus ≤5% até dormir, não-stack com relíquia do mesmo deus.
> **Out of scope:** templo de Kanthor (F40 cobre), altares CONSTRUÍVEIS pelo jogador (CITY §8 — wave futura), novos deuses, quests de devoção.

required_adrs: []
required_game_rules: [cave_rules.md, farm_rules.md, event_rules.md, save_rules.md]

> Nota: catálogo canônico das 11 Marcas no APÊNDICE LORE A.5 de
> `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md` (decisão 3.4 do Refinamento v2).

---

# /speckit.specify

## Contexto

Decisão 3.4 do Refinamento v2: o dono aprovou altares dos deuses espalhados pelo mundo
("tlvz possa ter algum altar nas cavernas de finan q da algum bonus, quero elementos dos
deuses na cidade, fzenda e caverna de alguma forma, pensando na lore"). O catálogo foi
consolidado (apêndice A.5): 11 Marcas, uma por deus maior do panteão, com a exceção
canônica inviolável — **Anya nunca tem altar nem bônus de oração; só a Fonte**.

A infra já reserva o espaço: `CAVE_DESIGN_DIRECTION` §17-19 prevê "altares quebrados" e
"marcas de deuses" como special rooms (0-2 por nível); a F22 implementa special rooms;
a F23 cria as relíquias divinas (portáteis) com as quais as Marcas (fixas) NÃO stackam.

## Problema

O panteão de Vaalara (11 deuses) existe só em texto. Fora o templo de Kanthor (F40) e a
Fonte, nenhum deus tem presença física/jogável na cidade, fazenda ou caverna.

## Objetivo

Implementar as 11 Marcas: interagível "orar" (1/dia por Marca), aplica buff diário ≤5%
que expira ao dormir, com persistência por flag diária no save e regra de não-stack com
a relíquia do mesmo deus (o maior efeito vale). Marcas de caverna entram na geração como
special rooms com seed determinística (rule cave-stable-run). Marcas urbanas/rurais são
interactables fixos nas cenas.

## Catálogo vinculante (apêndice A.5 — resumo mecânico)

```text
CAVERNA (special rooms, 0-1 por nível, raras):
  mark_finan_coin      Moeda Enterrada    qualquer banda   +5% gold find (1 dia)
  mark_thoren_anvil    Bigorna Fria       banda 41-55      -5% perda de durabilidade (1 dia)
  mark_kaand_stone     Pedra do Desafio   41-55 / 71-85    +3% dano causado E recebido (1 dia)
  mark_nyx_pool        Poço Sem Lua       banda 71-85      +5% chance de itens secretos (SÓ à noite)
  mark_tandra_root     Raiz Trançada      banda 11-25      +5% drops de partes Beast (1 dia)

CIDADE/FAZENDA (interactables fixos de cena):
  mark_thandra_niche   Nicho da Colheita  fazenda          oferenda 1 crop -> +2% Silver amanhã
  mark_kanthor_oath    Pedra de Juramento adro do templo   +2% block stability (1 dia)
  mark_merithus_seal   Selo de Merithus   cartório/envio   +2% valor do shipping do dia
  mark_alihana_mirror  Espelho de Alihana lago da fazenda  noite de Alihana: sonho-pista + +5% semente rara
  mark_senya_mast      Mastro de Senya    praça             festival/pico: +5% XP de magia (1 dia)

EXCEÇÃO (sem bônus — lore apenas):
  mark_anya_waters     As Três Águas      Jardim das Estátuas + Fonte + salas Anya Echo
                       NENHUM bônus de oração; texto de lore + cura limitada JÁ canônica do Anya Echo.
```

Regras transversais:
- orar = 1 interação/dia por Marca (flag diária, reseta ao dormir/virar dia);
- efeito dura até o sono (mesmo ciclo das condições diárias de E06);
- NÃO stacka com a relíquia do mesmo deus (F23): aplica-se o MAIOR efeito, nunca soma;
- Marcas condicionais: Nyx só à noite; Alihana só em noite de pico de Alihana;
  Senya só em festival/pico de Senya (consome estado do calendário F37).

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md (3.4 + APÊNDICE A.5 — catálogo vinculante)
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md (§17-19 special rooms)
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md (§6-8 — Anya sem altar; locais)
docs/specs/a_implementar/fable/fable_22_* (special rooms) e fable_23_* (relíquias)
docs/game_rules/cave_rules.md · farm_rules.md · event_rules.md · save_rules.md
.claude/rules/cave-stable-run.md (seed determinística — OBRIGATÓRIO)
.claude/skills/scene-interactable-wiring/SKILL.md · cave-stable-run-guard/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- PlayerConditionService (E06 executada) — buffs diários entram como condição nomeada;
- pipeline de special rooms (F22 — dependência; auditar contrato na Fase 0);
- relíquias divinas (F23 — dependência; ler a regra de não-stack de lá);
- GameTimeManager/calendário (dia, noite, sono) + F37 (picos/festivais);
- IInteractable + prompt de interação (padrão das cenas);
- StableHash do cave runtime (cave-stable-run — usar para sortear Marca por nível).
Não existe:
- qualquer altar/Marca, AltarService, flags de oração, buffs por deus.
Auditar Fase 0:
- contrato real das special rooms da F22 (como registrar um tipo novo);
- como F23 expõe "efeito ativo por deus" para o não-stack;
- nomes reais dos eventos de sono/virada de dia (event_rules.md).
```

## Engineering stories

```text
Como jogador, quero orar numa Marca 1x/dia e ganhar um bônus pequeno e temático até dormir.
Como geração da caverna, quero Marcas como special rooms determinísticas por seed
  (revisitar o nível mostra a MESMA Marca no MESMO lugar — cave-stable-run).
Como sistema de relíquias (F23), quero que Marca+relíquia do mesmo deus nunca somem.
Como save, quero flags diárias simples (string ids + bool/dia) — DTO sem refs Unity.
```

## Escopo

```text
Inclui:
- GodMarkCatalogSO (11 entradas: id, deus, local, efeito, condição, texto de lore);
- GodMarkAltarInteractable (IInteractable): orar -> valida condição (noite/pico/festival/
  oferenda) -> aplica condição diária via PlayerConditionService -> seta flag do dia;
- GodMarkDailyState (save section): flags de oração do dia + buffs ativos (ids simples);
- integração caverna: registrar os 5 tipos de Marca no pipeline de special rooms da F22,
  sorteio determinístico por StableHash(seed, level, "god_mark"), respeitando bandas;
- integração cidade/fazenda: âncoras nomeadas para os 5 interactables fixos (criação de
  cena via editor script autorizado — padrão scene-interactable-wiring);
- regra de não-stack com F23 (consulta o efeito da relíquia equipada do mesmo deus);
- mark_anya_waters: interactable SÓ de lore (zero bônus) nos 3 locais;
- EditMode tests: 1/dia por Marca, expiração ao dormir, não-stack, condições (noite/pico/
  festival/oferenda), determinismo do sorteio por seed, DTO round-trip.
```

## Fora de escopo

```text
Templo de Kanthor (F40); altares construíveis (CITY §8 — futuro); arte final; quests de
devoção; qualquer mudança nos efeitos das relíquias F23.
```

## Regras de não duplicação

```text
Buff diário = condição do PlayerConditionService (NÃO criar segundo sistema de buff).
Special room = pipeline da F22 (NÃO criar segundo spawner). Sorteio = StableHash existente
(NUNCA GUID/timestamp — cave-stable-run). Interação = IInteractable padrão.
```

## Critérios de aceite

### CA-1 — Catálogo completo e fiel
- As 11 Marcas existem com efeitos/condições exatamente como no apêndice A.5; Anya sem
  qualquer bônus de oração.
- Evidência: GodMarkCatalogSO + teste que valida o catálogo contra a tabela.

### CA-2 — Oração diária com expiração
- Orar aplica o buff 1x/dia; segunda interação no mesmo dia recusa com feedback; dormir
  expira buff e reseta flags.
- Evidência: EditMode tests.

### CA-3 — Não-stack com relíquias
- Marca + relíquia do mesmo deus → vale só o maior efeito.
- Evidência: EditMode test com os 4 deuses que têm relíquia (Kanthor/Kaand/Anya/Alihana).

### CA-4 — Determinismo na caverna
- Mesmo CaveRunSeed + nível → mesma Marca (ou ausência) no mesmo lugar em revisita;
  bandas respeitadas (Nyx 71-85, Tandra 11-25 etc.).
- Evidência: EditMode test de sorteio determinístico (2 gerações = mesmo resultado).

### CA-5 — Condicionais funcionam
- Nyx recusa de dia; Alihana exige noite de pico; Senya exige festival/pico; Thandra
  consome 1 crop como oferenda.
- Evidência: EditMode tests dos 4 casos.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/World/Altars/GodMarkCatalogSO.cs            (NOVO)
Assets/_Game/Scripts/World/Altars/GodMarkAltarInteractable.cs    (NOVO)
Assets/_Game/Scripts/World/Altars/GodMarkService.cs              (NOVO — regras 1/dia, não-stack, expiração)
Assets/_Game/Scripts/World/Altars/GodMarkSaveData.cs             (NOVO — DTO simples)
Assets/_Game/Scripts/Cave/Runtime/<integração special room F22>  (EDIT — registrar tipo god_mark)
Assets/_Game/Scripts/Editor/SceneCreation/<criadores cidade/fazenda> (EDIT — âncoras dos 5 fixos)
Assets/_Game/Tests/EditMode/World/GodMarkTests.cs                (NOVO)
```

## Contratos
- Runtime: `GodMarkService.TryPray(markId)` → resultado (ok/já orou/condição não satisfeita);
  buffs como condições nomeadas `god_mark_<deus>` no PlayerConditionService.
- Event: consome eventos de sono/virada de dia (existentes); publica `GodMarkPrayedEvent`
  (novo, GameEventBus) para UI/telemetria.
- Save: seção `godMarks` (flags do dia + buffs ativos por id) — simple types only.

## Sistemas afetados
```text
Cave special rooms (F22), Conditions (E06), Relíquias (F23 — leitura), Calendário (F37),
cenas cidade/fazenda (âncoras), save.
```
## Arquivos permitidos
```text
Arquitetura acima; Assets/_Game/Tests/EditMode/World/**; docs/validation/**; csproj includes
```
## Arquivos proibidos
```text
*.unity/*.prefab/*.asset manual (cenas SÓ via editor script com permissão); Packages/**;
ProjectSettings/**; efeitos das relíquias F23.
```
## Estratégia de implementação
```md
### Fase 0 — Auditar contratos: special rooms F22, não-stack F23, eventos de dia/sono, âncoras de cena.
### Fase 1 — Catálogo + Service + SaveData + testes (1/dia, expiração, não-stack, condicionais).
### Fase 2 — Integração caverna (special room determinística) + teste de determinismo.
### Fase 3 — Interactables fixos cidade/fazenda via editor scripts (com aprovação pontual).
### Fase 4 — csproj; run_strict_validation; report.
```
## Paralelização
- CONDITIONAL — após F22/F23 (janela P2+) e E06 (feita); não com specs de caverna/cena.
## Impacto em save/load
```text
YES — seção nova aditiva `godMarks` (flags diárias). Migração: ausente = nunca orou.
```
## Impacto em eventos
```text
Novo: GodMarkPrayedEvent. Consome: sono/virada de dia, calendário (picos/festivais).
```
## Impacto em UI/Unity
```text
UI: prompt de interação + toast do buff (padrão existente) | Play Mode final: YES |
DEFERRED_TO_FINAL_VALIDATION (cenas exigem editor scripts autorizados)
```
## Riscos técnicos
```text
Risco 1: contrato da F22 divergir do assumido → Fase 0 audita antes de codar.
Risco 2: sorteio quebrar stable-run → usar SÓ StableHash existente + teste de determinismo.
Risco 3: stack indevido com relíquias → regra centralizada no GodMarkService + teste.
```
## Rollback
```text
Remover World/Altars/**, registro do tipo special room e seção de save (aditiva).
```

# /speckit.tasks

## Tasks
```md
- [ ] T001 — Fase 0 (contratos F22/F23/eventos/âncoras).
- [ ] T002 — Catálogo + Service + SaveData + testes core.
- [ ] T003 — Special room determinística na caverna + teste de determinismo.
- [ ] T004 — Interactables fixos cidade/fazenda (editor scripts autorizados).
- [ ] T005 — csproj; run_strict_validation; report.
```
## Validações obrigatórias
```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```
## Testing Quality Gate
- Changed deterministic logic: YES | EditMode: YES | PlayMode/human: YES (interação nas 3 cenas) | Save: YES (round-trip)
- Minimum evidence: testes (CA-1..CA-5) + cenário humano orando em caverna/cidade/fazenda

## Definition of Done
```text
11 Marcas fiéis ao apêndice A.5; 1/dia + expiração ao dormir; não-stack F23; determinismo
cave-stable-run testado; Anya sem bônus; builds 0E; report.
```
## Anti-regressão
```text
Special rooms F22 existentes inalteradas; relíquias F23 intocadas; nenhum GUID/timestamp
no sorteio; save aditivo (seções antigas intactas).
```
