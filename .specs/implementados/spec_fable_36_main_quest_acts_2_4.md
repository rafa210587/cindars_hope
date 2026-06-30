# SPEC — Quests: Main Quest Atos 2-4 (Pedra Negra revelada gradualmente)

> **Spec ID:** `fable_36_spec_main_quest_acts_2_4`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P1
> **Type:** Content / Integration
> **Domain:** Quests / Narrative
> **Parallelizable:** NO (QuestRegistry lock — última da cadeia quest)
> **Parallel group:** N/A (lock da cadeia quest)
> **Can run with:** N/A
> **Must not run with:** F34, F35, F10
> **Repo lock scope:** main quest data, QuestFlagService (flags de ato), geradores de quest
> **Depends on:**
> - F34 (fonte Main + skill point/ato), F35 (cruzamentos), F10 (objetivos variados), F33 (bosses de gate)
> **Blocks:** N/A (endgame/final choice = spec futura pós-validação)
> **Scope:** atos 2, 3 e 4 do QUEST_CATALOG como quests encadeadas com marcos de mundo.
> **Out of scope:** Ato 5/final choice/Ithryndor scripted (spec futura), cutscenes, voice.

required_adrs: []
required_game_rules: [quest_rules.md]

---

# /speckit.specify

## Contexto

Decisão Q1.x: a Pedra Negra é a opção A revelada GRADUALMENTE pelos atos. QUEST_CATALOG
§main define os atos: Ato 1 (chegada/forja — existe parcialmente: auditar), Ato 2
(o veio corrompido — níveis 11-25), Ato 3 (a cidade sob pressão + Nymirian), Ato 4
(as portas profundas — gates 26-60). Cada ato: 4-6 quests, marco de mundo (flag que F28
diálogos e F25 serviços leem), +1 skill point (F34).

O catálogo (Parte B) nomeia as quests canônicas que esta spec materializa:
Ato 2 "O Arco da Memória" (QuestLevel 35): mq_act2_01_records_of_silver →
02_the_veiled_visitor (Vaelrion chega) → 03_song_below (Liora) → 04_gate_of_frost
(Rimelock Colossus, gate 30) → 05_fragment_of_memory (Fragmento da MEMÓRIA → respec).
Ato 3 "A Pedra que Sussurra" (QuestLevel 65): mq_act3_01_blackstone_ledger (investigação
na cidade) → 02_thrall_mercy (purificar 3 Thralls — voltam como aldeões) →
03_the_quiet_priest (escolha: prender/exilar — flag que retorna no final) →
04_warden_of_silence (chega o NPC Nymiriano) → 05_fragment_of_life (gate 70 →
Fragmento da VIDA → purificação).
Ato 4 "A Esperança Enterrada" (QuestLevel 90): mq_act4_01_litany_complete →
02_the_jailer (Draconic Elder, gate 100) → 03_vel_karaum → 04_broken_remembrance.
A escolha final (mq_act4_05_final_choice — Arquivista/Ithryndor) fica FORA desta spec
(spec futura: `fable_43`). Objetivo: materializar atos 2-4.

## Problema

Sem os atos 2-4, a main quest termina no Ato 1: a Pedra Negra nunca é revelada, os marcos
de mundo (act_N_done) que F28 (falas pós-ato) e F25 (gates de serviço) esperam consumir
não existem, os 3 skill points restantes da main (decisão Q6.2: +1 por ato) ficam
inalcançáveis e os bosses de gate (F33) não têm quest que os contextualize. Cadeias side
(F35) que citam atos ficam permanentemente gateadas.

## Objetivo

Ao final desta spec, o projeto deve ter os atos 2, 3 e 4 do catálogo como quests
encadeadas por flag (IDs `mq_act<N>_<n>`), cada ato com marco de mundo (act_N_done),
+1 skill point idempotente (hook F34), registro de lore que avança o entendimento da
Pedra Negra, e Nymirian conversável no Ato 3 — sem tocar no Ato 5/escolha final
(deferidos à `fable_43`).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (§main acts)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§1 Pedra Negra/Nymirian, §5)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (bosses de gate citados)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- main quest Ato 1 (auditar IDs/flags reais — fable_10);
- QuestFlagService (fonte única de flags);
- bosses de gate (F33) — objetivos referenciam os IDs reais deles;
- fonte Main + hook de skill point por ato (F34);
- tipos de objetivo variados (F10);
- diálogo condicional (F28) e serviços (F25) — consumidores dos marcos.
Não existe:
- atos 2-4 (nenhuma mq_act2/3/4_*);
- flags de marco (act_2_done, act_3_done, act_4_done);
- gating de profundidade NARRATIVO por ato (aviso soft);
- Nymirian como NPC conversável (spawn por flag);
- registros de lore por ato no quest log.
Auditar Fase 0:
- estado REAL do Ato 1 (quantas quests, IDs, flags) — completar lacunas dele dentro do
  escopo se <4 quests;
- IDs reais dos bosses de gate F33 citados (Rimelock Colossus/gate 30, Draconic
  Guardian/gate 70, Draconic Elder/gate 100) — objetivos só referenciam IDs existentes;
- viabilidade das quests pós-100 do Ato 4 (mq_act4_03/04) contra o conteúdo F33 real —
  documentar no report qualquer adaptação;
- campo aditivo de lore no detalhe de quest (F14) — confirmar ponto de exibição.
```

## Engineering stories

```text
Como jogador, quero que cada ato termine com um registro de lore que avança o mistério da
  Pedra Negra, para sentir a revelação gradual (decisão Q1.x).
Como diálogo de NPC (F28), quero flags act_N_done confiáveis, para trocar falas pós-ato.
Como sistema de skills (F34), quero +1 skill point idempotente por ato concluído, para
  nunca duplicar ponto em reload.
Como jogador no Ato 3, quero conversar com o Nymirian na caverna, para acessar a lore do
  povo de Cindar (decisão Q1.5).
```

## Escopo

```text
Inclui:
- gerador GenerateMainQuestActs: mq_act<N>_<n> conforme catálogo (objetivos: alcançar
  nível X da caverna, derrotar boss de gate Y, coletar amostra Z, falar com NPC W,
  decisão simples por diálogo);
- flags de marco: act_N_done → consumidas por F28 (falas pós-ato) e F25 (gates);
- revelação gradual: cada ato termina com "registro" (texto lore no quest log) que
  avança o entendimento da Pedra Negra (textos do catálogo);
- Nymirian: aparece como NPC de diálogo no Ato 3 (definição do roster; spawn via flag
  em local da caverna — interactable, sem schedule);
- gating de profundidade NARRATIVO (soft): descer além do gate do ato atual mostra aviso
  1× ("as portas adiante permanecem seladas" — gate físico já existe nos bosses de gate);
- +1 skill point por ato (F34 hook — verificar idempotência);
- EditMode tests: encadeamento dos atos, flags de marco, skill point 1×/ato, lore
  desbloqueado por ato, oferta do ato N+1 só com act_N_done.
```

## Fora de escopo

```text
Não inclui:
- Ato 5/escolha final/dragão = spec futura (decisão consciente: validar 2-4 antes) —
  a escolha final (mq_act4_05_final_choice) e o endgame scripted ficam na fable_43;
- cutscenes e voice acting;
- escolta scriptada complexa (objetivos adaptados aos tipos existentes — padrão F35);
- arte/cenário novo para o Nymirian (interactable placeholder);
- balance final de XP/ouro (fórmula BALANCE_CURVES §7 é a régua).
```

## Regras de não duplicação

```text
Não criar segundo fluxo de main — fonte Main do F34 é a única.
Não criar segundo registro de flags — QuestFlagService é o dono (flags act_N_done).
Não recriar bosses de gate — objetivos referenciam IDs do F33 (validados por F30).
Não criar sistema novo de lore — campo aditivo no detalhe de quest existente (F14).
Não duplicar o gate físico de profundidade — o aviso é narrativo (1×), o gate real são
  os bosses de gate.
```

## Critérios de aceite

### CA-1 — Atos 2-4 encadeados

- Atos 2-4 completos (4-6 quests cada) encadeados por flag; oferta do ato N+1 só aparece
  com act_N_done.
- Evidência: EditMode tests de encadeamento + log do gerador com contagem por ato.

### CA-2 — Marco, skill point e lore por ato

- Concluir um ato seta act_N_done, concede +1 skill point (idempotente — nunca 2× no
  mesmo ato, inclusive após reload) e desbloqueia o registro de lore do ato.
- Evidência: EditMode tests de marco/skill point/lore.

### CA-3 — Nymirian no Ato 3

- O NPC Nymiriano fica conversável via flag a partir do Ato 3 (interactable na caverna,
  sem schedule), conforme definição do roster.
- Evidência: teste de spawn condicionado por flag + cenário humano.

### CA-4 — Ato 1 auditado/completado

- Ato 1 auditado e completado a ≥4 quests (lacunas fechadas dentro do escopo).
- Evidência: seção de auditoria do Ato 1 no execution report + testes de regressão do Ato 1.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Quests/
  GenerateMainQuestActs.cs          (NOVO — tabela data-driven dos atos 2-4)
Assets/_Game/Scripts/Quests/ (ou namespace existente da main)
  hook de flags de marco + aviso de profundidade (aditivo nos serviços existentes)
Assets/_Game/Scripts/Cave/ (spawn do Nymirian por flag — interactable)
Assets/_Game/Tests/EditMode/Quests/
  MainActsTests.cs                  (NOVO)
docs/validation/
  fable_36_spec_main_quest_acts_2_4_execution_report.md
```

## Contratos

### Data contracts
QuestDefinitions `mq_act<N>_<n>` conforme catálogo (QuestLevel fixo por ato: 35/65/90);
registros de lore por ato como campo aditivo no detalhe de quest; flags `act_2_done`,
`act_3_done`, `act_4_done` no QuestFlagService.

### Runtime contracts
Hook de skill point por ato (F34) com verificação de idempotência (1×/ato, estável após
reload); aviso de profundidade soft exibido 1× por ato (flag de "aviso mostrado");
spawn do Nymirian condicionado a flag (interactable, sem schedule, sem Find runtime).

### Event contracts
ActCompletedEvent (novo — publicado no turn-in final do ato; consumido por toast/feedback).

### Save contracts
N/A novo — quests, flags e skill points persistem pelas seções existentes. O campo de
lore é derivado de flags (não persiste texto).

### UI contracts
Lore exibida no detalhe da quest (tela F14 — campo aditivo); toast de marco de ato via
fluxo de feedback existente.

## Sistemas afetados

```text
Quest registry/data (geração dos atos)
QuestFlagService (flags de marco)
Skill points (hook F34 — idempotência)
Diálogo F28 (falas pós-ato — consumidor) e serviços F25 (gates — consumidor)
Cave runtime (spawn do Nymirian + aviso de profundidade soft)
Quest log UI (lore no detalhe — F14)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Quests/GenerateMainQuestActs.cs (novo)
Assets/_Game/Scripts/Quests/** (hooks aditivos: marco/lore/aviso)
Assets/_Game/Scripts/Cave/** (spawn do Nymirian por flag — aditivo)
Assets/_Game/Tests/EditMode/Quests/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (geração só via gerador/AssetDatabase)
Packages/**
ProjectSettings/**
QuestFlagService core (consumir, não alterar contrato)
dados das cadeias side (F35) e do Ato 1 além da auditoria/complemento declarado
bosses de gate F33 (referenciar IDs, não alterar)
SaveManager / seções de save
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Estado real do Ato 1 (IDs/flags/contagem); IDs reais dos bosses de gate F33; viabilidade
das quests pós-100 do Ato 4; ponto de exibição de lore no detalhe de quest (F14).
### Fase 1 — Tabela data-driven
Tabela dos atos 2-4 (IDs/objetivos/lore do catálogo, QuestLevel 35/65/90) + complemento
do Ato 1 a ≥4 quests se necessário.
### Fase 2 — Gerador
GenerateMainQuestActs gera as quests; log com contagens por ato.
### Fase 3 — Runtime aditivo
Flags de marco + ActCompletedEvent + skill point idempotente (F34) + Nymirian por flag +
aviso de profundidade 1×.
### Fase 4 — Testes e closeout
MainActsTests (encadeamento, marcos, skill point 1×, lore, oferta N+1); F30;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F34, F35, F10 (mesma cadeia de QuestRegistry/geradores de quest —
  esta é a ÚLTIMA da cadeia quest)
- Shared files/systems that require lock: main quest data, QuestFlagService (flags de
  ato), geradores de quest
- Reason: consome o que F34/F35/F10 produzem; rodar em paralelo geraria conflito de
  registry e de flags.

## Impacto em save/load

```text
Does this change save schema? NO (quests/flags/skill points usam seções existentes)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Idempotência: +1 skill point por ato não pode duplicar após reload (teste obrigatório).
```

## Impacto em eventos

```text
Adds events: YES — ActCompletedEvent (toast de marco de ato)
Changes existing events: NO
Requires unsubscribe pattern: YES (consumidores de ActCompletedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: lore no detalhe da quest (F14 — campo aditivo) + toast de marco
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador (evidência obrigatória)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: dependência de bosses de gate reais inexistentes quebrar objetivos.
Mitigação: objetivos referenciam IDs F33 auditados na Fase 0 e validados pelo F30.
Risco: skill point duplicado por ato (reload/re-trigger).
Mitigação: hook F34 idempotente + EditMode test de 1×/ato.
Risco: regressão no Ato 1 ao completar lacunas.
Mitigação: auditoria Fase 0 + testes de regressão do encadeamento do Ato 1.
Risco: aviso de profundidade virar gate duro acidental.
Mitigação: aviso é informativo 1× — nenhum bloqueio de movimento; gate físico continua
  sendo exclusivamente os bosses de gate.
Risco: escopo vazar para o endgame (Ato 5/escolha final).
Mitigação: fronteira explícita — mq_act4_05_final_choice/Ithryndor ficam na fable_43.
```

## Rollback

```text
Gerador não roda: atos 2-4 não existem; Ato 1 intacto.
Hooks aditivos removíveis (flags/evento/aviso) sem afetar quests existentes.
Nenhuma seção de save nova para limpar.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar Ato 1 real + completar a 4+ (dentro do escopo).
- [ ] T002 — Tabela atos 2-4 (textos/lore do catálogo) + gerador GenerateMainQuestActs.
- [ ] T003 — Flags de marco + ActCompletedEvent + skill point idempotente + Nymirian +
        aviso de profundidade.
- [ ] T004 — Testes (MainActsTests) + F30; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gates de ato, idempotência de skill point)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (Ato 1)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano completando 1 quest de ato com marco visível

## Definition of Done

```text
Atos 2-4 jogáveis; revelação gradual (lore por ato); marcos consumíveis por
diálogo/serviços; skill points idempotentes; Nymirian conversável no Ato 3.
Ato 1 auditado a ≥4 quests; fronteira do endgame (fable_43) preservada.
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Ato 1 intacto (IDs/flags/encadeamento preservados).
Cadeias side (F35) e dailies inalteradas.
QuestFlagService sem mudança de contrato; F33 sem alteração.
Skill point por ato nunca duplicado após reload.
Nenhuma referência Unity em dados de quest; nenhum GameObject.Find em runtime.
```
