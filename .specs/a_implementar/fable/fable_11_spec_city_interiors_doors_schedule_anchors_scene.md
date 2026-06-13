# SPEC — Cidade: Portas Funcionais, Interiores Mínimos e Âncoras de Schedule por Hora

> **Spec ID:** `fable_11_spec_city_interiors_doors_schedule_anchors_scene`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco D (mundo)
> **Priority:** P2
> **Type:** Runtime / Integration
> **Domain:** City
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_D
> **Can run with:** fable_01, fable_04, fable_09, fable_12
> **Must not run with:** fable_10 (CreateMvpTownScene compartilhado), fable_14
> **Repo lock scope:** `CreateMvpTownScene.cs`, `Assets/_Game/Scripts/NPC/Schedule/**`
> **Depends on:**
> - WI-25 (NpcScheduleProfile/Service/Anchor — código pronto, cena nunca wired)
> - WAVE 02 (GameTimeManager — hora do dia)
> - slice 2026-06-12 (TownScene v2 com casas/distritos)
> **Blocks:** N/A
> **Scope:** fechar TIME_BLOCK_DEBT e SCENE_WIRING_DEBT da WI-25 com portas/interiores/âncoras geradas.
> **Out of scope:** interiores ricos/mobília, pathfinding com obstáculos, schedules por clima/lua (15_spec futura).

required_adrs: []
required_game_rules: [city_rules.md]

---

# /speckit.specify

## Contexto

`CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md` é canônica para rotina individual: NPCs têm
blocos por hora (trabalho/almoço/casa/noite), camas, portas e waypoints. A WI-25 criou TODO
o runtime de schedule (`NpcScheduleProfile/Block/Anchor/Service/RuntimeBootstrap` +
`NpcTownRosterRegistry`) mas fechou com dois débitos explícitos: **TIME_BLOCK_DEBT**
(schedule por dia apenas, sem blocos por hora) e **SCENE_WIRING_DEBT** (`NpcScheduleAnchor`
jamais colocado em cena). O slice 2026-06-12 criou 12 casas como fachadas decorativas sem
porta funcional. `GameTimeManager` (WAVE 02) já expõe a hora do dia.

## Problema

23 NPCs com profiles de schedule que nunca executam: a cidade é estática o dia inteiro,
casas não servem para nada, e a loja noturna da Yael (identidade do design) funciona igual
ao meio-dia. Os débitos da WI-25 bloqueiam o "cidade viva" prometido pela direction.

## Objetivo

Ao final desta spec, o gerador da TownScene deve criar âncoras de schedule (work/home/social)
por NPC + porta funcional por casa (teleport interior mínimo de 1 sala), o
`NpcScheduleService` deve resolver blocos por hora (manhã/tarde/noite/madrugada) movendo
NPCs entre âncoras, e NPCs "em casa" devem ficar indisponíveis para interação (com feedback),
fechando os dois débitos da WI-25.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/validation/WAVE_INTEGRATION_25_TOWN_NPC_SCHEDULES_DIALOGUE_REPORT.md
.claude/rules/testing-quality-gate.md
.claude/skills/scene-interactable-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- NPC/Schedule/: NpcScheduleProfile, NpcScheduleBlock, NpcScheduleAnchor,
  NpcScheduleRuntimeState, NpcScheduleService, NpcScheduleRuntimeBootstrap (WI-25)
- NpcTownRosterRegistry (23 NPCs, zonas, movement profiles)
- GameTimeManager (hora/dia — auditar API exata de hora)
- CreateMvpTownScene v2 (TownHouseSpecs com 12 casas, distritos, NPCs posicionados)
- NpcWanderer (bounds por NPC), NpcController/NpcShopController
Parcial:
- NpcScheduleProfile: estrutura de blocos existe mas só per-day (TIME_BLOCK_DEBT)
Não existe:
- âncoras em cena; portas funcionais; interiores; indisponibilidade por schedule
```

## Engineering stories

```text
Como NPC, quero ir da banca (08-18h) para a taverna (18-22h) e para casa (22-06h).
Como jogador, quero ver a porta da casa e, à noite, ouvir "Fulano já se recolheu" em vez de loja aberta.
Como Yael, quero o inverso: indisponível de dia, banca ativa 20h-02h.
Como gerador de cena, quero âncoras criadas automaticamente por NPC a partir do roster.
```

## Escopo

```text
Inclui:
- estender NpcScheduleProfile com 4 blocos-hora padrão (Work/Social/Home/Night) com horários
  por arquétipo (lojista, guarda, noturno, errante) derivados do NpcTownRosterRegistry;
- NpcScheduleService: resolver bloco corrente pela hora do GameTimeManager e mover NPC para a
  âncora-alvo (MovePosition suave via NpcWanderer target override — método novo SetDestination);
- CreateMvpTownScene: gerar 3 NpcScheduleAnchor por NPC (work = posição atual; home = porta da
  casa do distrito; social = taverna/praça por arquétipo) + associar casa→NPCs;
- portas funcionais: DoorInteractable (NOVO IInteractable) nas 12 casas → teleporta para
  interior mínimo gerado (sala 6x5 com cama/mesa placeholder + porta de saída), interiores
  posicionados em faixa y>+40 fora do playfield (mesma cena — sem scene transition);
- indisponibilidade: NpcController/NpcShopController consultam
  NpcScheduleService.IsAvailable(npcId) → prompt vira "<nome> nao esta disponivel agora."
  (CanInteract false com feedback via InteractionPromptChangedEvent);
- Yael/Maelor com blocos invertidos (NightOnly do roster);
- EditMode tests: resolução de bloco por hora, mapeamento âncora por arquétipo,
  disponibilidade por horário (Yael 14h = false, 22h = true).
```

## Fora de escopo

```text
Não inclui: pathfinding com desvio de obstáculo (movimento direto + teleport fallback se
preso > 5s); mobília/interior rico; portas trancadas por amizade; schedules por
clima/lua/estação (15_spec futura); festivais.
```

## Regras de não duplicação

```text
Não criar segundo sistema de schedule — fechar débitos do WI-25 no código existente.
Não criar cena de interior separada — interiores na própria TownScene (sem mudança no
SceneTransitionRouter/Build Settings).
Não recriar wander — NpcWanderer ganha SetDestination.
```

## Critérios de aceite

### CA-1 Blocos por hora (fecha TIME_BLOCK_DEBT)
- Service resolve Work/Social/Home/Night pela hora; transições nos horários do arquétipo.
- Evidência: testes com horas sintéticas para 4 arquétipos.

### CA-2 Âncoras geradas (fecha SCENE_WIRING_DEBT)
- Gerador cria >= 3 âncoras por NPC associadas por npcId; validator de cena confirma cobertura
  dos 23. Evidência: editor validator novo + log do gerador.

### CA-3 Portas e interiores
- Cada casa tem DoorInteractable que teleporta para interior com saída de volta; câmera segue.
- Evidência: cenário humano + validator (12 casas → 12 interiores → 24 portas pareadas).

### CA-4 Disponibilidade honesta
- NPC em Home/Night (não-noturno) não abre loja/diálogo e informa o motivo; Yael inverte.
- Evidência: testes de IsAvailable + cenário humano.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Schedule/NpcScheduleProfile.cs   (blocos-hora)
Assets/_Game/Scripts/NPC/Schedule/NpcScheduleService.cs   (resolução + movimento)
Assets/_Game/Scripts/NPC/NpcWanderer.cs                   (+SetDestination)
Assets/_Game/Scripts/NPC/{NpcController,NpcShopController}.cs (IsAvailable gate)
Assets/_Game/Scripts/World/DoorInteractable.cs            (NOVO)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (âncoras/interiores/portas)
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs (NOVO)
Assets/_Game/Tests/EditMode/City/NpcScheduleBlocksTests.cs
```

## Contratos

### Runtime contracts
`NpcScheduleService.IsAvailable(npcId)`, `GetCurrentBlock(npcId, hour)`;
`NpcWanderer.SetDestination(Vector2 target, float arriveRadius)`;
`DoorInteractable` (par interior/exterior por targetPosition — sem scene load).
### Event contracts — `NpcScheduleBlockChangedEvent(npcId, block)` (NOVO).
### Save contracts — N/A (posição de NPC já é salva pelo NpcManager; bloco re-deriva da hora).
### UI contracts — prompt de indisponibilidade via fluxo existente.

## Sistemas afetados

```text
NPC schedule/movement, Town scene generator, Interaction, Time (consumo), Event bus
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/NpcEvents aditivos
Assets/_Game/Tests/EditMode/City/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset manual; Packages/ProjectSettings; SaveManager/GameSaveData;
SceneTransitionRouter/Build Settings
```

## Estratégia de implementação

```md
### Fase 0 — Auditar API de hora do GameTimeManager e save de posição de NPC.
### Fase 1 — Blocos-hora no profile + resolução no service + testes.
### Fase 2 — SetDestination + movimento por bloco + teleport fallback.
### Fase 3 — Gerador: âncoras + interiores + DoorInteractable + validator.
### Fase 4 — IsAvailable nos controllers + Yael/Maelor invertidos + validação estrita + report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: fable_10 (mesmo gerador), fable_14
- Reason: CreateMvpTownScene e controllers compartilhados.

## Impacto em save/load

```text
Does this change save schema? NO
```

## Impacto em eventos

```text
Adds events: YES (NpcScheduleBlockChangedEvent) | Changes existing: NO | Unsubscribe: YES
```

## Impacto em UI/Unity

```text
Changes scenes: via gerador (humano regenera TownScene) | Prefabs/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: NPC preso em collider a caminho da âncora. Mitigação: teleport fallback após 5s +
âncoras geradas em posições walkable (sem sobrepor casas/estátua).
Risco: interação iniciada exatamente na transição de bloco. Mitigação: NPC em interação
pausa schedule (SetInteractionPaused existente).
```

## Rollback

```text
Sem âncoras na cena o service permanece inerte (comportamento atual); remover DoorInteractable/interiores do gerador.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar GameTimeManager (hora) e save de NPC.
- [ ] T002 — Blocos-hora por arquétipo + resolução + testes.
- [ ] T003 — NpcWanderer.SetDestination + movimento por bloco.
- [ ] T004 — DoorInteractable + interiores no gerador.
- [ ] T005 — Âncoras por NPC no gerador + ValidateFableCitySchedule.
- [ ] T006 — IsAvailable nos controllers + inversão noturna.
- [ ] T007 — Testes; csproj; run_strict_validation; report (fecha 2 débitos WI-25).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (cidade viva observável)
- Requires regression test: YES (interação atual fora de schedule intacta até âncoras existirem)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com ciclo dia/noite completo

## Definition of Done

```text
TIME_BLOCK_DEBT e SCENE_WIRING_DEBT fechados; 12 casas com porta/interior; NPCs circulando
por hora; Yael noturna; builds 0E; report.
```

## Anti-regressão

```text
Diálogo/loja/quest flow atuais intactos quando NPC disponível.
Save de posição de NPC continua funcionando.
Sem GameObject.Find; interiores não interferem na câmera/bounds do playfield (y>+40 com bounds próprios).
```

## Notas para execução posterior

```text
Schedules por clima/lua: promover 15_spec futuras.
Portas trancadas por amizade/social: WAVE 17 futuras.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. LOJAS FECHADAS (6.4-A): porta BLOQUEADA com aviso de horário (não se entra fora do horário).
```
