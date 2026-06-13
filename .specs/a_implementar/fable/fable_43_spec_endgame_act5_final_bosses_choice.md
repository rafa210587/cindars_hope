# SPEC — Endgame: Ato 5, os Quatro do Nível 101 e a Escolha Final da Fonte

> **Spec ID:** `fable_43_spec_endgame_act5_final_bosses_choice`
> **Status:** A implementar
> **Wave:** FABLE Batch 9
> **Priority:** P1
> **Type:** Runtime / Content / Integration
> **Domain:** Quests / Cave / Narrative
> **Parallelizable:** NO
> **Parallel group:** N/A (SOLO — fecha a cadeia quest+cave)
> **Can run with:** N/A
> **Must not run with:** qualquer spec de quest (F34/F35/F36) ou de caverna (F05/F09/F44)
> **Repo lock scope:** QuestRegistry/geradores de quest, `MainProgression/**`, FonteRuntimeService, boss runtime/spawner, HUD events
> **Depends on:**
> - `fable_36_spec_main_quest_acts_2_4` (atos 2-4 + flags de marco)
> - `fable_05_spec_cave_boss_phase_ai_runtime` (boss phase AI)
> - `fable_33_spec_bestiary_data_expansion_60_creatures` (assets dos 4 finais, dormantes)
> - `fable_17_spec_fonte_anya_physical_interactable_runtime` (Fonte hospeda fragmentos)
> - `fable_34_spec_quest_sources_infrastructure` (fonte Main + skill point por ato)
> **Blocks:** pós-game expandido / new game+ (futuro)
> **Scope:** quests do Ato 5 (endgame), gate 100→101, orquestração dos 4 encontros finais e execução da escolha Proteger/Selar/Usar com 3 epílogos como estados de mundo.
> **Out of scope:** cutscenes, voice, arte final, novas áreas de pós-game, new game+.

required_adrs: []
required_game_rules: [cave_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

A F36 entregou os atos 2-4 e declarou explicitamente: "Ato 5/escolha final/Ithryndor
scripted = spec futura". Esta é essa spec. O QUEST_CATALOG (PARTE B §7) numera os encontros
finais como `mq_act4_03_vel_karaum`, `mq_act4_04_broken_remembrance` e
`mq_act4_05_final_choice` — os IDs do catálogo são canônicos e DEVEM ser mantidos; "Ato 5"
é o rótulo operacional do plano para a fatia endgame (a sequência do nível 101 + escolha).
O CAVE_BESTIARY_CATALOG (PARTE I) fixa os 4 encontros do pós-100, em sequência com
recuperação controlada: Vel-Karaúm (construct, Huge, HP 7000) → Cindrathel (espírito que
ESPELHA a build do jogador, HP 5500) → Archivist of Silence (aberração, HP 9000, luta de
INFORMAÇÃO que remove elementos do HUD) → Ithryndor, the Buried Dawn (dragão ancestral
Gargantuan 4×4, HP 14000, condicionado à escolha final). Decisões humanas (FABLE_DECISOES
§1): dragão ancestral aprovado como segredo do 101; Pedra Negra = opção A revelada
gradualmente (os atos 2-4 já fazem a revelação; aqui ela culmina).

## Problema

Os contratos puros do endgame (`FinalChoiceService`, `MainProgressionSection` com
`Level100GateState`/`Level101AccessState`/`FinalChoiceStatus`) existem desde as waves de
contrato mas NADA os consome em runtime: não há quest que leve ao 101, não há orquestração
dos 4 encontros, a escolha final não é executável e os 3 epílogos não existem como estado
de mundo. Sem esta spec o jogo não tem final — a main quest termina no gate 100 (F36) e
todo o investimento dos contratos MainProgression fica morto.

## Objetivo

Ao final desta spec, o jogador que concluiu o Ato 4 (Draconic Elder derrotado, Litania
completa) deve conseguir: destravar o nível 101, enfrentar os 4 encontros em sequência com
recuperação controlada, executar a escolha Proteger/Selar/Usar via `FinalChoiceService`
existente (confirmação forte), ver Ithryndor responder conforme a escolha
(aliado/sono/última luta) e cair num estado PostGame mínimo persistido — sem recriar
nenhum contrato MainProgression e sem quebrar o stable-run da caverna.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (PARTE I — os Quatro; fichas do Draconic Elder)
docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md (PARTE B §7 Ato 4; §2 recompensas)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§1 dragão/Pedra Negra; §6 skill point por ato)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- MainProgression/ COMPLETO como contrato puro: FinalChoiceService (EvaluateFinalChoice com
  idempotência/preview/token "FINAL_CHOICE_CONFIRMED", CanUnlockLevel101), EndgameContracts
  (FinalChoiceType Protect/Seal/Use, EndingEffectProfile com os 3 perfis canônicos,
  ArchivistRevealState), MainProgressionSection/Service, MainAct (inclui PostGame),
  FragmentStateRecord (MainFragmentType.Hope/Life);
- FonteRuntimeService + FonteState (FinalizedProtected/Sealed/Used) + FonteAnyaSection (F17);
- boss phase AI (F05) + CaveBossSpawner/CaveLevelRuntimeController;
- assets dos 4 finais gerados dormantes (F33: boss_vel_karaum, boss_cindrathel,
  boss_archivist_of_silence, boss_ithryndor);
- fonte Main de quest + skill point por ato (F34); atos 2-4 e flags act_N_done (F36);
- classe inferida (F39, se executada) e loadout do player (EquipmentManager, skills ativas).
Não existe:
- quests mq_act4_03..05 materializadas; gate runtime 100→101; orquestrador da sequência 101;
  mecânicas especiais (espelho de build, supressão de HUD); execução/persistência da escolha;
  epílogos; PostGame state.
Auditar Fase 0: MainProgressionSection já é persistida em alguma seção de save? Shape real
dos assets F33 dos 4 finais; como o nível 101 é representado no gerador (F09).
```

## Engineering stories

```text
Como jogador, quero que derrotar o Draconic Elder e completar a Litania destrave o 101.
Como jogador, quero enfrentar os 4 encontros em sequência com recuperação controlada entre eles.
Como jogador, quero que Cindrathel use a MINHA build contra mim — vencer a mim mesmo.
Como jogador, quero que o Archivist remova meu HUD por fase e me force a lutar pelo que aprendi.
Como jogador, quero escolher Proteger/Selar/Usar com confirmação forte e ver o mundo responder.
Como sistema de save, quero o estado do endgame persistido em tipos simples (flags/enums).
```

## Escopo

```text
Inclui:
- gerador GenerateEndgameQuests (Editor): mq_act4_03_vel_karaum, mq_act4_04_broken_remembrance,
  mq_act4_05_final_choice conforme catálogo (IDs canônicos), encadeadas por flag a partir de
  mq_act4_02_the_jailer (F36); XP fixo por ato (QuestLevel 90) + skill point do ato final (F34);
- gate 100→101: derrotar Draconic Elder → Level100GateState=Entered; com Fragmento da VIDA
  integrado, CanUnlockLevel101 (serviço EXISTENTE) → Level101AccessState=Unlocked; entrada
  do 101 só com Unlocked (bloqueio com mensagem caso contrário);
- EndgameSequenceController (novo, runtime): orquestra os 4 encontros no nível 101 em arenas
  sequenciais (materialização mínima: mesma arena rearmada por encontro), com janela de
  recuperação controlada entre encontros (sem respawn de adds, consumíveis permitidos);
- mecânica Cindrathel (espelho de build): config determinística lida do loadout REAL do
  player no início do encontro (classe inferida F39, arma equipada, skills ativas nos slots)
  → mapeada para moves/fases do boss AI F05 via tabela MirrorBuildMapper (pura, testável);
- mecânica Archivist (luta de informação): evento novo HudSuppressionChangedEvent(fase,
  conjunto de widgets ocultos) publicado por fase do boss; HUD existente assina e
  oculta/restaura (barra de HP, minimapa, números de dano); restauração GARANTIDA em
  morte/vitória/saída (teste + unsubscribe);
- escolha final: interação na Fonte (F17) abre confirmação forte (3 opções + warning text
  do EndingEffectProfile + token) → FinalChoiceService.EvaluateFinalChoice (EXISTENTE);
- Ithryndor condicional (catálogo): Protect = desperta como aliado, SEM luta (cena de
  diálogo/estado); Seal = luta parcial cerimonial (1 fase, sem morte do player ser esperada);
  Use = luta completa de 4 fases (boss AI F05);
- epílogos como estado de mundo: PostGameWorldState = ending_protect|ending_seal|ending_use
  + flags de quest (F36 pattern) consumidas por diálogos (F28) e serviços; aplicação dos
  policies do EndingEffectProfile LIMITADA ao v1: FonteState finalizado, texto de epílogo no
  quest log, caverna permanece acessível (CavePostGamePolicy v1 = manter estável);
- PostGame mínimo: CurrentAct=PostGame, save persiste MainProgressionSection (seção aditiva
  se a Fase 0 confirmar que ainda não persiste), main quest fechada no log;
- EditMode tests: gate 100→101 (regras do serviço existente), idempotência da escolha
  (AlreadyApplied), branch de Ithryndor por escolha, MirrorBuildMapper determinístico,
  contrato de supressão de HUD (publica/restaura), skill point do ato final 1×.
```

## Fora de escopo

```text
Não inclui: cutscenes/voice/arte; novas áreas de pós-game (NewAreas do perfil Use fica como
flag para spec futura); new game+; rebalance fino dos 4 (HP/DMG entram do catálogo);
qualquer mudança no gerador procedural além do hook de entrada do 101.
```

## Regras de não duplicação

```text
Não recriar FinalChoiceService/EndgameContracts/MainProgressionSection — consumir os existentes.
Não criar segundo fluxo de main quest — gerador entra na fonte Main do F34.
Não criar segundo boss AI — os 4 consomem F05 (fases) + mecânica especial por cima.
Não criar canal novo de HUD — supressão via GameEventBus consumida pelo HUD existente.
Não tocar no seed/stable-run: o 101 usa o mesmo contrato determinístico (ADR-0005).
```

## Critérios de aceite

### CA-1 Gate 100→101
- Elder derrotado + Fragmento da Vida → 101 destravado; sem isso, entrada bloqueada com aviso.
- Evidência: testes das regras (CanUnlockLevel101) + cenário humano.

### CA-2 Sequência dos 4 com recuperação controlada
- Os 4 encontros ocorrem em ordem canônica; janela de recuperação entre eles; morte na
  sequência segue o fluxo de derrota normal (sem softlock — sequência re-entrável).
- Evidência: testes do orquestrador (máquina de estados) + cenário humano.

### CA-3 Mecânicas especiais
- Cindrathel reflete classe/arma/skills reais do player (mapper determinístico testado);
  Archivist oculta widgets por fase e SEMPRE restaura ao fim (teste de restauração).
- Evidência: MirrorBuildMapperTests + HudSuppressionTests.

### CA-4 Escolha final e epílogos
- Proteger/Selar/Usar só com confirmação forte; aplicada 1× (AlreadyApplied na repetição);
  Ithryndor responde conforme (sem luta / parcial / 4 fases); PostGameWorldState + FonteState
  persistem e sobrevivem a save/load.
- Evidência: testes de idempotência/branch + round-trip de save.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/Runtime/
  EndgameSequenceController.cs   (NOVO — máquina de estados dos 4 encontros)
  MirrorBuildMapper.cs           (NOVO — puro: loadout → config Cindrathel)
  FinalChoiceRuntimeAdapter.cs   (NOVO — Fonte interactable → FinalChoiceService → save/eventos)
Assets/_Game/Scripts/Events/ (ou pasta canônica de eventos)
  HudSuppressionChangedEvent.cs, FinalChoiceResolvedEvent.cs, EndgameEncounterChangedEvent.cs
Assets/_Game/Scripts/Editor/Quests/GenerateEndgameQuests.cs (NOVO)
Assets/_Game/Tests/EditMode/MainProgression/{EndgameSequenceTests,MirrorBuildMapperTests,FinalChoiceRuntimeTests}.cs
docs/validation/fable_43_spec_endgame_act5_final_bosses_choice_execution_report.md
```

## Contratos

### Data contracts
Tabela de epílogo v1: EndingEffectProfile EXISTENTE (sem campos novos). Config do espelho:
struct simples {classeInferida, weaponFamily, activeSkillIds[]} → moves F05.
### Runtime contracts
EndgameSequenceController: `Begin()`, `AdvanceEncounter()`, `CurrentEncounter`,
re-entrável após derrota. FinalChoiceRuntimeAdapter: monta FinalChoiceRequest com token.
### Event contracts
`HudSuppressionChangedEvent(int fase, string[] widgetsOcultos)`;
`FinalChoiceResolvedEvent(string endingId)`; `EndgameEncounterChangedEvent(string bossId, int index)`.
### Save contracts
MainProgressionSection persistida (auditar Fase 0; se faltar, seção aditiva padrão WI-18,
tipos simples apenas — enums/strings/flags). Sem refs Unity.
### UI contracts
Confirmação forte reutiliza o modal de confirmação existente (F14); texto de warning por
perfil; epílogo = entrada de lore no quest log (campo F36).

## Sistemas afetados

```text
Main progression / final choice, Quest registry (fonte Main), Cave runtime (entrada 101 +
boss spawner), Fonte runtime, HUD (supressão por evento), Save/load, Event bus, Skill points.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/** (novos runtime/adapters; contratos existentes SÓ se
  ajuste mínimo for inevitável e documentado)
Assets/_Game/Scripts/Editor/Quests/GenerateEndgameQuests.cs
Assets/_Game/Scripts/Cave/** (apenas hook de entrada do 101 + spawner dos 4 — cirúrgico)
Assets/_Game/Scripts/UI/** (apenas assinatura da supressão de HUD)
Assets/_Game/Scripts/Quests/** (campos/flags aditivos)
Assets/_Game/Tests/EditMode/MainProgression/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manuais (geradores/AssetDatabase apenas, se necessário)
Packages/** ; ProjectSettings/**
FinalChoiceService.cs / EndgameContracts.cs (consumir; não reescrever regras)
Geradores procedurais da caverna fora do hook do 101 (stable-run ADR-0005)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria: persistência da MainProgressionSection; assets F33 dos 4; representação do 101 no gerador; flags F36 reais.
### Fase 1 — Quests do endgame (gerador + flags + XP/skill point) encadeadas ao mq_act4_02.
### Fase 2 — Gate 100→101 runtime + EndgameSequenceController (máquina de estados + recuperação).
### Fase 3 — Mecânicas: MirrorBuildMapper (Cindrathel) + HudSuppressionChangedEvent (Archivist).
### Fase 4 — FinalChoiceRuntimeAdapter + branches de Ithryndor + epílogos/PostGame + save.
### Fase 5 — Testes EditMode, run_strict_validation, execution report, cenário humano final.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: nenhuma
- Must not run with: F34/F35/F36 (quest), F05/F09/F44 (cave), F14/F20 (HUD)
- Shared files/systems that require lock: QuestRegistry, MainProgression, boss runtime, HUD events, save
- Reason: fecha simultaneamente quest+cave+save+HUD — colisão com qualquer vizinho.

## Impacto em save/load

```text
Does this change save schema? YES (aditivo) — MainProgressionSection persistida (se a Fase 0
confirmar ausência) + flags de epílogo na seção de quest existente.
Does this add a save section? CONDITIONAL (Fase 0); padrão WI-18; default = endgame não iniciado.
Does this require migration? NO (campos aditivos, load legado com defaults).
Does this persist Unity references? NO.
```

## Impacto em eventos

```text
Adds events: YES — HudSuppressionChangedEvent, FinalChoiceResolvedEvent, EndgameEncounterChangedEvent.
Changes existing events: NO. Requires unsubscribe pattern: YES (HUD/controllers).
```

## Impacto em UI/Unity

```text
Changes UI: YES (confirmação forte + supressão de HUD + lore de epílogo)
Changes scenes: NO (hook via runtime/gerador) | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador apenas (quests)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: supressão de HUD não restaurada após morte → jogador cego permanente.
Mitigação: restauração centralizada em um único ponto (evento com fase 0 = tudo visível)
disparada em vitória/derrota/saída + teste dedicado.
Risco: escolha aplicada 2× corromper estado de mundo. Mitigação: idempotência já existe no
serviço (AlreadyApplied) — teste de regressão a cobre no fluxo runtime.
Risco: espelho de build com loadout vazio (player sem skills). Mitigação: fallback canônico
no mapper (build "guerreiro básico") testado.
Risco: 101 violar stable-run. Mitigação: mesma cadeia de seed determinística (ADR-0005);
nenhuma mudança de CaveRunSeed em ForwardExit/BackExit.
```

## Rollback

```text
Gerador de quests do endgame não roda → atos 1-4 intactos (F36). Remover controller/adapter
desliga a sequência; gate volta a bloquear no 100. Seção de save aditiva ignorada por loads
antigos. Nenhum save real apagado.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar persistência MainProgression, assets F33 dos 4, gerador do 101, flags F36.
- [ ] T002 — GenerateEndgameQuests (mq_act4_03..05, IDs canônicos) + XP/skill point do ato.
- [ ] T003 — Gate 100→101 runtime (consumir CanUnlockLevel101) + bloqueio com mensagem.
- [ ] T004 — EndgameSequenceController (4 encontros + recuperação + re-entrável) + testes.
- [ ] T005 — MirrorBuildMapper (Cindrathel) + testes determinísticos com fallback.
- [ ] T006 — HudSuppressionChangedEvent (Archivist) + assinatura no HUD + teste de restauração.
- [ ] T007 — FinalChoiceRuntimeAdapter + branches Ithryndor + epílogos/PostGame + save aditivo.
- [ ] T008 — Testes EditMode completos; csproj; run_strict_validation; execution report + cenário humano.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gate, escolha, mapper, máquina de estados)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (sequência completa + escolha — obrigatório)
- Requires regression test: YES (idempotência da escolha; restauração de HUD; stable-run do 101)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano completando a
  sequência e UM epílogo, com save/load após a escolha

## Definition of Done

```text
Quests do endgame encadeadas; 101 destravável pelas regras existentes; 4 encontros
orquestrados com mecânicas especiais; escolha executável 1× com 3 epílogos persistidos;
PostGame mínimo; builds 0E; report com Spec Compliance Matrix; sem claim ACCEPTED.
```

## Anti-regressão

```text
Atos 1-4 (F36) intactos; IDs de quest do catálogo preservados; CaveRunSeed nunca muda por
portal (ADR-0005); HUD sempre restaurado; FinalChoiceService não reescrito; skill point por
ato continua 1×; save legado sem seção endgame carrega com defaults.
```

## Notas para execução posterior

```text
CavePostGamePolicy "NewAreas" (epílogo Use) fica como flag para spec futura de pós-game.
Esta spec não implementa cerimônia/celebração de epílogo — apenas estado de mundo + textos.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. EPÍLOGOS DETALHADOS (4.2/4.3): SELAR → eventos pós-final de ataque de criaturas poderosas
   à fazenda ameaçando a cidade (raids de epílogo); PROTEGER → cidade prospera visivelmente
   (decoração/NPCs/preços melhores). USAR → consolidação do mapeamento existente (cura,
   respec, buffs, purificações conforme liberação de Anya — apêndice lore v2).
2. NÍVEL 101 (4.5-B): é uma CaveScene ESPECIAL (não cena/arena dedicada).
3. TABELA DE GATES (4.4 APROVADA): 15 Cobre Temperado · 30 planta baú de corpse · 45 Aço
   Profundo · 60 Mithril Work · 75 Bromeciana · 90 Pedra Negra · 100 acesso 101 + Meteórica.
4. Vaelrion chega no ATO 2 após o primeiro boss (3.6); Sethra↔Yael rivais; Corvus giver recorrente.
```

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v3.0.md §4.2)

```text
1. NÚMEROS DE DANO DO HUD DO BOSS FINAL — JÁ EXISTEM, NÃO RECRIAR. A re-auditoria de
   2026-06-13 confirmou que os números de dano flutuantes já existem e estão wired em runtime
   (FloatingDamageNumberDisplayer + FloatingNumberBehavior + DamagePopupAnchor, assinando
   DamageAppliedEvent/PlayerDamagedEvent, séries 14A-FIX). A menção "remove ... números de
   dano" no escopo desta spec (mecânica Archivist, /speckit.specify) refere-se a SUPRIMIR e
   RESTAURAR esses números EXISTENTES — nunca criar um sistema novo de números de dano.

2. HudSuppressionChangedEvent PASSA A SER PROVIDO PELA fable_71 (DEPENDÊNCIA). O evento
   HudSuppressionChangedEvent(int fase, string[] widgetsOcultos), que esta spec lista nos
   "Event contracts" e na mecânica Archivist, NÃO é mais criado por esta spec: ele é DEFINIDO
   no GameEventBus pela nova fable_71 ("combat feel pass"). A F43 passa a ser CONSUMIDORA do
   evento. Fase 0 = conjunto de widgets oculto vazio = HUD totalmente visível (estado de
   restauração garantida). A barra de HP do boss, o minimapa e os números de dano (existentes)
   são ocultados/restaurados por esse evento, por fase, com restauração SEMPRE garantida em
   morte/vitória/saída.

3. AJUSTE DE DEPENDÊNCIA: adicionar fable_71 (combat feel pass) à lista de "Depends on" desta
   spec, como provedora do contrato HudSuppressionChangedEvent. A fable_71 está na FABLE Batch
   11 (janela LIVRE, só consome eventos existentes) e não introduz nenhum acoplamento de
   gameplay com a F43 além do contrato do evento.

4. ANTI-REGRESSÃO REFORÇADA: esta spec NÃO cria, recria ou duplica números de dano, hit-flash
   ou knockback (todos preexistentes). A supressão de HUD do Archivist usa exclusivamente o
   canal GameEventBus via HudSuppressionChangedEvent (provido pela fable_71). O teste de
   "restauração de HUD sempre garantida" (CA-3) permanece obrigatório, agora validando o
   consumo do evento da fable_71 (fase 0 = tudo visível).
```
