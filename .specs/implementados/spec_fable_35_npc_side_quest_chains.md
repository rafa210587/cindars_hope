# SPEC — Quests: 12 Cadeias de Side Quest por NPC (conteúdo do catálogo)

> **Spec ID:** `fable_35_spec_npc_side_quest_chains`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P2
> **Type:** Content / Integration
> **Domain:** Quests / NPC
> **Parallelizable:** NO (QuestRegistry lock — após F34)
> **Parallel group:** N/A (lock da cadeia quest)
> **Can run with:** N/A
> **Must not run with:** F34, F10, F36 (cadeia quest)
> **Repo lock scope:** geradores de quest, QuestRegistry data, TownNpcDialogueLibrary (ofertas)
> **Depends on:**
> - F34 (fonte Npc), F26 (gates de amizade), F32 (itens de recompensa)
> **Blocks:** F36 (algumas cadeias cruzam com atos)
> **Scope:** as 12 cadeias do QUEST_CATALOG (3-5 quests cada, ~45 quests) como dados+flags.
> **Out of scope:** cutscenes, quests que exijam sistemas inexistentes (adaptar objetivo OU dormante documentado), romance.

required_adrs: []
required_game_rules: [quest_rules.md, npc_rules.md]

---

# /speckit.specify

## Contexto

QUEST_CATALOG §cadeias define 12 cadeias por NPC com epígrafes de voz, objetivos,
recompensas e gates (amizade/ato/estação). Cada cadeia expressa o propósito do NPC no
roster (Brumdar→forja, Thalindra→pesquisa, Yael→biblioteca...). As 12 cadeias do v1 são,
pelo catálogo (Parte C §9): Brumdar ("o ferro lembra"), Ozzra ("tudo borbulha por um
motivo"), Thalindra ("a poeira guarda"), Sylveth ("a terra responde a quem pergunta"),
Eiran ("bicho sente antes da gente"), Gruta ("barriga cheia, língua solta"), Dagna ("toda
pedra tem veio; é só ouvir"), Zrix ("desça devagar, suba inteiro"), Hund ("ordem é rotina
bem feita"), Mirela ("a costura conta a história do rasgo"), Tovin ("o carimbo protege
quem carimba") e Corvus ("a Fonte não esqueceu; nós esquecemos dela").

A regra das cadeias (catálogo §8) é fixa: q1 doméstica (apresenta o NPC) → q2 que toca o
mundo (caverna/cidade/lua) → q3 com marco (miniboss/banda/ato) e recompensa permanente —
a última quest destrava o serviço único do NPC (ex.: sq_brumdar_3 → Têmpera de Essência;
sq_ozzra_3 → LearnableScrolls; sq_tovin_3 → alvarás de lote da fazenda). Objetivo desta
spec: materializar essas cadeias como QuestDefinitions encadeadas (flag da anterior gateia
a próxima) ofertadas no diálogo via fonte Npc (F34).

## Problema

Sem as cadeias, os NPCs do roster não têm propósito jogável: a fonte Npc (F34), os gates
de amizade (F26) e os itens de recompensa (F32) existem como sistemas, mas não há conteúdo
que os conecte. Os serviços únicos (F25) ficam indestraváveis e a progressão social fica
sem objetivo. Se os objetivos do catálogo forem implementados sem auditoria dos tipos
existentes, o risco é corte silencioso de quests ou criação de tipos de objetivo paralelos.

## Objetivo

Ao final desta spec, o projeto deve ter as 12 cadeias do catálogo geradas como
QuestDefinitions encadeadas por flag (IDs `sq_<npc>_<n>`), ofertadas no diálogo do NPC
quando elegíveis (flag anterior + amizade mínima + ato quando citado), com recompensas
(gold/item/amizade +8/flag de serviço) e textos na voz do NPC, sem criar tipos de objetivo
novos nem segundo fluxo de oferta.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/quests_progression/QUEST_CATALOG_DIRECTION_v1.0.md (§cadeias — INTEIRO)
docs/design/gameplay/npcs_city/CITY_NPC_ROSTER_DIRECTION (v1.1 — propósito por NPC)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§5)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- F34 (fonte Npc/gates de oferta) — toda oferta de side passa por ela;
- tipos de objetivo de quest (kill/collect/deliver/talk/flag — WAVE 09/26);
- FriendshipService (F26) — fonte única de amizade/gates sociais;
- QuestFlagService — fonte única de flags;
- geradores de quest existentes (padrão data-driven a seguir);
- TownNpcDialogueLibrary + DialogueCondition (F28: RequiredFlag/MinFriendship).
Não existe:
- as 12 cadeias (nenhuma QuestDefinition sq_<npc>_<n>);
- ofertas "[!] Quest" no diálogo dos 12 NPCs;
- flags de serviço destravadas por cadeia (sq_<npc>_3_done).
Auditar Fase 0:
- matriz de viabilidade: tipos de objetivo disponíveis × objetivos do catálogo
  (cada objetivo do catálogo mapeado a um tipo existente, adaptado ou dormante);
- contagem alvo por cadeia: o catálogo nomeia 3 quests por cadeia (§9); a spec admite
  3-5 (~45) quando a adaptação de objetivos exigir desdobramento — registrar a contagem
  final na matriz cadeia×status do report;
- IDs reais de itens de recompensa (F32) e flags de serviço (F25) citados pelo catálogo.
```

## Engineering stories

```text
Como jogador, quero ver "[!] Quest" no diálogo de um NPC quando há cadeia elegível, para
  descobrir o propósito dele conversando.
Como NPC do roster, quero que minha cadeia termine destravando meu serviço único, para
  que amizade e quests tenham recompensa permanente.
Como QuestRegistry, quero as ~45 quests geradas por tabela data-driven com IDs estáveis
  sq_<npc>_<n>, para não divergir do catálogo.
Como agente executor, quero uma matriz de viabilidade objetivo×tipo antes de gerar, para
  nunca cortar objetivo silenciosamente.
```

## Escopo

```text
Inclui:
- gerador GenerateNpcQuestChains: ~45 QuestDefinitions (IDs sq_<npc>_<n> do catálogo)
  com objetivos mapeados aos tipos existentes, recompensas (gold/item/amizade +8/serviço
  unlock flag), gates (flag anterior + amizade mínima + ato quando citado);
- oferta no diálogo: opção "[!] Quest" no Conversar quando elegível (TownNpcDialogueLibrary
  + DialogueCondition F28 RequiredFlag/MinFriendship);
- objetivos sem tipo viável: ADAPTAR para tipo existente mantendo a narrativa (ex.:
  "escoltar" → "falar com X no local Y") OU dormante (flag NotYetOfferable + nota);
- textos: título/descrição/diálogo de oferta/conclusão na VOZ do NPC (epígrafes do catálogo);
- flags de serviço: cadeias que destravam serviços F25 setam as flags certas
  (sq_brumdar_3_done → têmpera F22);
- closeout: F30 valida refs de recompensa; matriz cadeia×status no report;
- EditMode tests: encadeamento por flag, gates compostos, recompensa de amizade, amostra
  de 3 cadeias completas (fluxo sintético aceitar→completar→próxima).
```

## Fora de escopo

```text
Não inclui:
- cutscenes ou scripts de cena;
- quests que exijam sistemas inexistentes sem adaptação (vão como dormantes documentadas);
- romance (cadeias sociais de romance são outra direction);
- main quest (F36) e dailies do quadro (outra fonte);
- balance final de recompensas (fórmula do BALANCE_CURVES §7 já é a régua);
- 2ª leva de cadeias (11 NPCs restantes — v2, conforme catálogo §8).
```

## Regras de não duplicação

```text
Não criar tipos de objetivo novos (adaptar OU dormante).
Não criar segundo fluxo de oferta — única via diálogo F28/F34.
Não criar segundo serviço de amizade — FriendshipService (F26) é o dono.
Não criar segundo registro de flags — QuestFlagService é o dono.
Não duplicar quests da main (F36) nem dailies — escopo é exclusivamente sq_*.
```

## Critérios de aceite

### CA-1 — 12 cadeias geradas com matriz de viabilidade

- As 12 cadeias do catálogo existem como QuestDefinitions encadeadas; nenhum objetivo do
  catálogo foi silenciosamente cortado (todo objetivo está mapeado, adaptado ou dormante).
- Evidência: matriz cadeia×status + matriz objetivo×tipo no execution report; log do gerador.

### CA-2 — Encadeamento e gates compostos

- sq_X_2 permanece invisível/inofertável até sq_X_1_done + amizade mínima; gates de ato
  são respeitados quando citados pelo catálogo.
- Evidência: EditMode tests de gate composto (flag+amizade+ato).

### CA-3 — Recompensas e flags de serviço

- Conclusão de quest dá +8 amizade (F26) e seta a flag correspondente; q3 destrava o
  serviço único do NPC (flag consumida por F25, ex.: sq_brumdar_3_done → têmpera F22).
- Evidência: EditMode tests de recompensa/flag; validação F30 das refs de item.

### CA-4 — Fluxo ponta a ponta

- 3 cadeias percorridas ponta a ponta em teste sintético (aceitar→completar→próxima→serviço).
- Evidência: NpcChainsTests com fluxo e2e sintético das 3 cadeias.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Quests/
  GenerateNpcQuestChains.cs        (NOVO — tabela data-driven das 12 cadeias)
Assets/_Game/Scripts/NPC/
  TownNpcDialogueLibrary.cs        (ofertas "[!] Quest" + condições F28)
Assets/_Game/Tests/EditMode/Quests/
  NpcChainsTests.cs                (NOVO)
docs/validation/
  fable_35_spec_npc_side_quest_chains_execution_report.md
```

## Contratos

### Data contracts
QuestDefinitions com IDs estáveis `sq_<npc>_<n>` (catálogo §9); recompensas referenciam
IDs de item do F32 e fórmula escalada do BALANCE_CURVES §7 (QuestLevel = nível de
referência da cadeia); flags `sq_<npc>_<n>_done` e flags de serviço consumidas por F25.

### Runtime contracts
Gates avaliados pelos serviços existentes: QuestFlagService (flag anterior/ato) +
FriendshipService (amizade mínima). Nenhuma API nova de gate.

### Event contracts
N/A — não cria eventos novos; usa o fluxo de aceitação/conclusão de quest existente.

### Save contracts
N/A novo — quests/flags persistem pelas seções existentes (quests normais). Nenhum campo
de save novo.

### UI contracts
Marcador "[!] Quest" como opção no diálogo Conversar (TownNpcDialogueLibrary), visível
apenas quando a DialogueCondition (RequiredFlag/MinFriendship) é satisfeita.

## Sistemas afetados

```text
Quest registry/data (geração)
Diálogo de NPC (ofertas condicionais)
Amizade (consumo de gates + recompensa +8)
Serviços de NPC F25 (flags de unlock)
Validação de refs (F30)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Quests/GenerateNpcQuestChains.cs (novo)
Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs (ofertas — aditivo)
Assets/_Game/Tests/EditMode/Quests/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (geração só via gerador/AssetDatabase)
Packages/**
ProjectSettings/**
QuestFlagService / FriendshipService (consumir, não alterar)
main quest data (F36) e geradores de daily/contrato (outras fontes)
SaveManager / seções de save
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Matriz objetivos do catálogo × tipos de objetivo existentes (viável/adaptar/dormante);
contagem alvo por cadeia; IDs reais de itens (F32) e flags de serviço (F25).
### Fase 1 — Tabela data-driven
Tabela das 12 cadeias (IDs, gates, recompensas, textos na voz do NPC com as epígrafes).
### Fase 2 — Gerador
GenerateNpcQuestChains gera as QuestDefinitions; log com contagens por cadeia.
### Fase 3 — Ofertas no diálogo
Opção "[!] Quest" + DialogueConditions; flags de serviço no turn-in da q3.
### Fase 4 — Testes e closeout
EditMode tests (gates, encadeamento, +8 amizade, 3 cadeias e2e); F30; matriz no report;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F34, F10, F36 (mesma cadeia de QuestRegistry/geradores de quest)
- Shared files/systems that require lock: geradores de quest, QuestRegistry data,
  TownNpcDialogueLibrary
- Reason: trava o registry de quests e as ofertas de diálogo compartilhadas; deve rodar
  depois de F34 e antes de F36.

## Impacto em save/load

```text
Does this change save schema? NO (quests/flags usam seções existentes)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO (conteúdo data-driven; sem listeners novos)
```

## Impacto em UI/Unity

```text
Changes UI: marcador [!] no diálogo (opção condicional — sem tela nova)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador (evidência obrigatória)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: volume de texto (45 quests) gerar inconsistência de voz.
Mitigação: tabela por NPC com as epígrafes do catálogo como base obrigatória.
Risco: objetivo do catálogo sem tipo viável ser cortado silenciosamente.
Mitigação: matriz de viabilidade explícita no report; adaptar OU dormante documentado.
Risco: flag de serviço errada deixar serviço F25 indestravável.
Mitigação: teste por cadeia que destrava serviço + validação F30 de refs.
Risco: cruzamento com atos (F36) criar dependência circular.
Mitigação: gates de ato só LEEM flags act_N_done; F36 roda depois (Blocks declarado).
```

## Rollback

```text
Gerador não roda (ou re-roda sem as cadeias): repo volta ao estado anterior.
Ofertas no diálogo são aditivas — remover as entradas restaura o Conversar atual.
Nenhuma flag/seção de save nova para limpar.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Matriz objetivos catálogo × tipos existentes (Fase 0) + IDs de item/flag.
- [ ] T002 — Tabela das 12 cadeias (textos na voz) + gerador GenerateNpcQuestChains.
- [ ] T003 — Ofertas no diálogo + gates compostos + flags de serviço (q3).
- [ ] T004 — Testes (3 cadeias e2e sintético) + F30; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gates compostos, encadeamento por flag)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (ofertas existentes do diálogo intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano completando 1 cadeia inteira

## Definition of Done

```text
12 cadeias ofertáveis e completáveis; gates corretos; serviços destravados; voz preservada.
Matriz de viabilidade no report (nenhum objetivo cortado silenciosamente).
Nenhum arquivo proibido alterado; testes EditMode em Assets/_Game/Tests/EditMode/Quests/.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Diálogos/ofertas existentes dos NPCs continuam funcionando (opção [!] é aditiva).
Main quest (Ato 1) e dailies intactas — nenhum ID existente alterado.
FriendshipService/QuestFlagService sem mudança de contrato.
Nenhuma referência Unity em dados de quest; nenhum GameObject.Find em runtime.
```
