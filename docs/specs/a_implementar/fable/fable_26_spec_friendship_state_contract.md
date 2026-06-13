# SPEC — Amizade: Estado por NPC + Pontos + Níveis + Save (contrato consumível)

> **Spec ID:** `fable_26_spec_friendship_state_contract`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P2
> **Type:** Runtime / Save
> **Domain:** NPC
> **Parallelizable:** NO (save schema lock)
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** F07, F12, F13, F21, F42 (seções de save), F25/F28 (consumidores)
> **Repo lock scope:** `NPC/Friendship/**`, `GameSaveData` (seção nova)
> **Depends on:**
> - WAVE NPC base (NpcDefinition/registry)
> - F13 (save schema estável)
> **Blocks:**
> - F25 (gates de serviço)
> - F28 (pools por amizade)
> - F35 (cadeias gated)
> **Scope:** FriendshipService com pontos/níveis 0-5, fontes de ganho e persistência.
> **Out of scope:** romance/casamento (futuro), presentes com preferência por NPC (catálogo de gostos = futuro; presente genérico entra), UI de corações rica (número no diálogo basta).

required_adrs: []
required_game_rules: [npc_rules.md]

---

# /speckit.specify

## Contexto

O roster canônico (`CITY_NPC_ROSTER_DIRECTION` v1.1) define os 23 NPCs com IDs estáveis
(tabela-mestra PARTE C: npc_corvus...npc_maelor), estados de relacionamento (§4),
casais fixos (§5) e candidatos a romance (§6). As decisões humanas
(`FABLE_DECISOES_RESPOSTAS_v1.0.md` §10) aprovaram as recompensas de relacionamento
em 3 vias juntas (diálogos diferentes + bônus + serviços especiais). Mas não existe
NENHUM estado de relacionamento em runtime — nenhum sistema sabe "quão amigo" o
jogador é de um NPC.

Esta spec é deliberadamente um CONTRATO mínimo e sólido: pontos por interação, níveis
0-5 com thresholds fixos, decaimento OFF (decisão de simplicidade), caps diários
anti-exploit e persistência — que F25 (gates de serviço), F28 (pools de diálogo por
amizade) e F35 (cadeias gated) consomem por API. Romance, presentes preferidos e UI
rica de corações ficam explicitamente para o futuro; a exposição no jogo é uma linha
"Amizade: nível N" no cabeçalho do Conversar.

## Problema

Sem o FriendshipService, três specs ficam bloqueadas sem API alvo: F25 não tem gate
de amizade para serviços, F28 não tem nível para selecionar pools de diálogo, F35 não
tem gate para cadeias. Se cada consumidor criasse seu próprio tracker, haveria estados
divergentes de relacionamento e save fragmentado. E sem caps diários, qualquer fonte
de pontos vira exploit de spam (conversar/comprar em loop até nível máximo no dia 1).

## Objetivo

Ao final desta spec, o projeto deve ter um FriendshipService no bootstrap com API
estável (GetLevel/GetPoints/AddPoints/IsAtLeast), níveis 0-5 com thresholds
10/30/60/100/150, 4 fontes de ganho integradas com caps diários, evento de mudança de
nível e seção de save aditiva — permitindo que F25/F28/F35 consumam amizade por API,
sem romance, sem decaimento e sem UI rica.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/npcs_city/CITY_NPC_ROSTER_DIRECTION (v1.1 — relações)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§10)
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- NpcDefinition/NpcRegistry (IDs canônicos dos NPCs);
- NpcShopController (pontos de interação: conversa, compra);
- QuestRegistry (conclusão de side quest — fonte de pontos);
- SaveManager padrão WI-18 (seções/providers);
- GameEventBus;
- GameCalendarService (dia absoluto para caps diários).
Não existe:
- friendship (estado, serviço, save, evento, fontes de ganho).
Auditar Fase 0:
- IDs canônicos dos 23 NPCs no registry (conferir com a tabela-mestra do roster);
- pontos de hook: primeira conversa do dia (diálogo), conclusão de side quest
  (QuestRegistry), entrega de presente genérico (itens gift_* F32), compra na loja;
- como obter o dia absoluto do GameCalendarService (chave dos caps diários).
```

## Engineering stories

```text
Como F25/F28/F35, quero uma API única (GetLevel/IsAtLeast) para gates de amizade,
  sem criar trackers paralelos.
Como jogador, quero ganhar amizade por ações cotidianas (conversar, ajudar, presentear,
  comprar) e ver o nível no diálogo.
Como anti-exploit, quero caps diários por fonte (1ª conversa do dia, 1 presente/dia/NPC,
  1 compra/dia) para que amizade não suba por spam.
Como save, quero a amizade persistida em seção aditiva com tipos simples e load legado
  resultando em todos nível 0.
```

## Escopo

```text
Inclui:
- FriendshipService (bootstrap): GetLevel(npcId), GetPoints(npcId), AddPoints(npcId, n, fonte);
- níveis: 0=Desconhecido(0) 1=Conhecido(10) 2=Cordial(30) 3=Amigo(60) 4=Próximo(100)
  5=Confidente(150);
- fontes de ganho: primeira conversa do dia +1; side quest do NPC concluída +8; presente
  genérico (item gift_* F32) +3 (1×/dia/NPC); compra na loja do NPC +1 (1×/dia);
- anti-exploit: caps diários por fonte (teste);
- evento FriendshipLevelChangedEvent(npcId, nível) → toast "X agora é seu amigo";
- save: FriendshipSaveData {List {npcId, points, lastTalkDay, lastGiftDay, lastPurchaseDay}}
  padrão WI-18, default vazio;
- exposição no diálogo: linha "Amizade: nível N" no cabeçalho do Conversar (placeholder UI);
- API de gate: IsAtLeast(npcId, level) — consumível por flag/gate;
- EditMode tests: thresholds, caps diários, idempotência por dia, round-trip, load legado.
```

## Fora de escopo

```text
Não inclui:
- romance/casamento (futuro — usa candidatos do roster §6 quando vier);
- presentes com preferência por NPC (catálogo de gostos = futuro; presente genérico entra);
- aniversários/eventos sociais;
- decaimento de amizade (decisão de simplicidade: OFF);
- UI de corações rica (número no diálogo basta);
- mais fontes de ganho (exatamente 4 — não inflar).
```

## Regras de não duplicação

```text
Não criar segundo tracker de relacionamento — F25/F28/F35 consomem APIs deste serviço.
Não inflar: 4 fontes de ganho apenas.
Não duplicar dia/calendário — caps diários usam o dia absoluto do GameCalendarService.
Não criar segundo caminho de save — seção WI-18 padrão.
```

## Critérios de aceite

### CA-1 Cap diário de conversa

- Conversar a 1ª vez no dia dá +1 ponto; 2ª conversa no mesmo dia dá +0 (cap);
  no dia seguinte volta a dar +1.
- Evidência: EditMode tests com dias sintéticos.

### CA-2 Thresholds e evento

- Thresholds corretos (10/30/60/100/150) com FriendshipLevelChangedEvent publicado
  exatamente na transição de nível (uma vez por transição).
- Evidência: EditMode tests de threshold + publicação única.

### CA-3 Persistência

- Round-trip preserva pontos e marcadores de dia; load legado (sem seção) = todos os
  NPCs nível 0, sem erro.
- Evidência: EditMode tests de round-trip e load legado.

### CA-4 API de gate consumível

- IsAtLeast(npcId, level) responde corretamente em todos os níveis e para npcId
  desconhecido (false/nível 0) — contrato estável para F25/F28/F35.
- Evidência: EditMode tests da API de gate.

### CA-5 Demais fontes com caps

- Side quest concluída +8 (por quest, idempotente); presente genérico +3 com cap
  1×/dia/NPC; compra +1 com cap 1×/dia.
- Evidência: EditMode tests por fonte e cap.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Friendship/
  FriendshipService.cs           (NOVO — pontos/níveis/caps/gate)
  FriendshipSaveData.cs          (NOVO — DTO seção WI-18)
SaveManager (wiring aditivo da seção)
Assets/_Game/Scripts/Core/Events/
  NpcEvents (FriendshipLevelChangedEvent — aditivo)
Hooks pontuais: diálogo (1ª conversa), QuestRegistry (side quest), presente (gift_*),
  shop (compra)
Assets/_Game/Tests/EditMode/City/
  FriendshipTests.cs             (NOVO)
docs/validation/
  fable_26_spec_friendship_state_contract_execution_report.md
```

## Contratos

### Data contracts

Estado por NPC: {npcId:string, points:int, lastTalkDay:int, lastGiftDay:int,
lastPurchaseDay:int}. Níveis derivados dos pontos: 0=Desconhecido(0), 1=Conhecido(10),
2=Cordial(30), 3=Amigo(60), 4=Próximo(100), 5=Confidente(150). Dias = dia absoluto do
GameCalendarService.

### Runtime contracts

`FriendshipService` (bootstrap): `GetLevel(npcId)`, `GetPoints(npcId)`,
`AddPoints(npcId, n, fonte)` (aplica caps por fonte/dia), `IsAtLeast(npcId, level)`.
npcId desconhecido = nível 0/false (sem exceção). Decaimento OFF.

### Event contracts

Novo (aditivo em NpcEvents): `FriendshipLevelChangedEvent(npcId, nível)` → toast
"X agora é seu amigo". Nenhum evento existente muda.

### Save contracts

Seção nova aditiva `FriendshipSaveData` { List {npcId, points, lastTalkDay,
lastGiftDay, lastPurchaseDay} } — padrão WI-18, default vazio, tipos simples, sem
migration, sem refs Unity. Load legado = lista vazia (todos nível 0).

### UI contracts

Linha "Amizade: nível N" no cabeçalho do Conversar (placeholder — UI rica de corações
é futura). Nenhuma tela nova.

## Sistemas afetados

```text
NPC (serviço novo + hooks de interação)
Save/load (seção nova aditiva)
Event bus (+1 evento)
Diálogo (linha de cabeçalho + hook de 1ª conversa)
Quests (hook de conclusão de side quest)
Shop (hook de compra)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/Friendship/**
Assets/_Game/Scripts/Core/Events/NpcEvents (aditivo)
SaveManager wiring aditivo da seção
Hooks pontuais: diálogo/QuestRegistry/presente/shop (1 ponto cada)
Assets/_Game/Tests/EditMode/City/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida)
Packages/**
ProjectSettings/**
NpcRegistry/NpcDefinition core (consumir IDs, não alterar)
GameCalendarService (consumir dia absoluto, não alterar)
Sistemas consumidores F25/F28/F35 (eles consomem depois; não antecipar integração)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar IDs canônicos dos 23 NPCs no registry (conferir tabela-mestra do roster),
pontos de hook (diálogo/quest/presente/compra) e API de dia absoluto do calendário.

### Fase 1 — Núcleo
FriendshipService com pontos, thresholds 0-5, caps diários por fonte e IsAtLeast —
lógica pura testável.

### Fase 2 — Fontes de ganho
4 hooks integrados: 1ª conversa do dia (+1), side quest do NPC (+8), presente
genérico gift_* (+3, 1×/dia/NPC), compra na loja (+1, 1×/dia).

### Fase 3 — Save e evento
FriendshipSaveData (round-trip/legado) + FriendshipLevelChangedEvent + toast.

### Fase 4 — Exposição e fechamento
Linha "Amizade: nível N" no cabeçalho do Conversar; testes completos; csproj;
run_strict_validation; report.
```

## Paralelização

- Parallelizable: NO (save schema lock)
- Parallel group: N/A
- Must not run with: F07, F12, F13, F21, F42 (seções de save — um por vez); F25/F28
  (consumidores — executar depois desta)
- Shared files/systems that require lock: `NPC/Friendship/**`, `GameSaveData`
  (seção nova)
- Reason: adição de seção de save é serializada entre specs; consumidores dependem
  da API estável desta spec.

## Impacto em save/load

```text
Does this change save schema? YES — seção nova aditiva (FriendshipSaveData)
Does this add a save section? YES (owner: FriendshipService)
Does this require migration? NO (default vazio; load legado = todos nível 0)
Does this persist Unity references? NO (npcId/ints simples)
Restore order: após NpcRegistry disponível (padrão WI-18).
```

## Impacto em eventos

```text
Adds events: YES — FriendshipLevelChangedEvent(npcId, nível)
Changes existing events: NO
Requires unsubscribe pattern: YES (hooks que assinam eventos de quest/shop/diálogo)
```

## Impacto em UI/Unity

```text
Changes UI: YES — linha "Amizade: nível N" no cabeçalho do Conversar (placeholder)
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: caps diários usando relógio errado (hora local/tempo real).
Mitigação: usar dia absoluto do GameCalendarService como única chave de dia (testes).
Risco: pontos duplicados pela mesma side quest (re-trigger).
Mitigação: idempotência por questId na fonte de quest (teste CA-5).
Risco: consumidores (F25/F28/F35) lerem estado interno em vez da API.
Mitigação: API pública mínima documentada (GetLevel/GetPoints/AddPoints/IsAtLeast);
  estado interno privado.
Risco: npcId divergente entre hooks e registry.
Mitigação: Fase 0 valida IDs contra a tabela-mestra do roster.
```

## Rollback

```text
Remover seção/hooks; consumidores caem em gate-aberto default false (IsAtLeast=false).
Seção de save desconhecida é ignorada com segurança em load.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar NPC IDs/registry + pontos de hook + dia absoluto do calendário.
- [ ] T002 — Service + níveis (10/30/60/100/150) + caps diários + IsAtLeast + testes.
- [ ] T003 — 4 fontes de ganho integradas (conversa/quest/presente/compra) + testes.
- [ ] T004 — Save (round-trip/legado) + FriendshipLevelChangedEvent/toast + linha de
        diálogo; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (thresholds, caps, idempotência por dia)
- Requires EditMode tests: YES (thresholds, caps diários, idempotência, round-trip,
  load legado, API de gate)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (diálogo/shop atuais intactos; load legado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano subindo amizade
  a nível 1

## Definition of Done

```text
Amizade funcional e persistida (níveis 0-5, thresholds 10/30/60/100/150) com caps
anti-exploit por fonte/dia; evento de nível + toast; linha no diálogo; API estável
(GetLevel/GetPoints/AddPoints/IsAtLeast) pronta p/ F25/F28/F35.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Diálogo e shop atuais intactos (hooks são aditivos).
Sem referências Unity no save; saves antigos carregam (todos nível 0).
Exatamente 4 fontes de ganho (não inflar).
Decaimento permanece OFF.
Nenhum tracker paralelo de relacionamento em consumidores.
Comunicação só via GameEventBus; sem GameObject.Find em runtime.
```

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v3.0.md, decisão 2.4 CUSTOM + re-auditoria de código 2026-06-13, acréscimo 2)

> Esta emenda preserva TODO o conteúdo anterior. Ela altera SOMENTE a fonte de ganho
> "presente genérico", que deixa de ser **+3 fixo** e passa a **LER o gosto do NPC** na
> nova matriz de gostos. Em conflito com o texto original acima, esta emenda vence.

```text
1. DECISÃO 2.4 (A + CUSTOM): cada NPC reage de forma DIFERENTE a presentes. O "presente
   genérico +3 fixo" do escopo original é SUBSTITUÍDO por leitura do gosto do NPC em
   docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md (NOVO doc, artefato A6).
   "presentes com preferência por NPC = futuro" (Out of scope / Fora de escopo originais)
   deixa de valer: o gosto por NPC ENTRA nesta spec.

2. NÍVEIS DE GOSTO E DELTA (substitui o +3 fixo da fonte "presente genérico"):
       loved    = +12   (item raro/pessoal; 1-3 por NPC; por LovedItemIds)
       liked    = +6    (por tag de gosto; LikedItemTags)
       neutral  = +2    (DEFAULT: qualquer item Giftable não classificado)
       disliked = -2    (DislikedItemTags) — afinidade NEGATIVA pequena
       hated    = -6    (HatedItemTags) — afinidade NEGATIVA alta (REDUZ a amizade)
   Precedência de classificação (mais forte vence): hated > disliked > loved(id) >
       liked(tag) > neutral(default). (Alinha SOCIAL_RELATIONSHIP_ROMANCE §8.2.)
   Pontos de amizade nunca caem abaixo de 0 (clamp no piso do nível 0).
   NOVO caso a testar: presente HATED reduz pontos (pode rebaixar nível → publicar
       FriendshipLevelChangedEvent na descida também, não só na subida).

3. REUSO OBRIGATÓRIO (re-auditoria acréscimo 2): a struct NpcGiftPreferences já EXISTE em
   Assets/_Game/Scripts/NPC/NpcDefinition.cs (LikedItemTags, LovedItemIds, DislikedItemTags,
   DailyGiftLimit) — hoje CÓDIGO MORTO. NÃO criar uma segunda struct.
       ESTENDER NpcGiftPreferences com: List<string> NeutralItemTags; List<string> HatedItemTags.
       REUTILIZAR a tag ItemTag.Giftable (Assets/_Game/Scripts/Items/ItemTag.cs, hoje sem item)
           como porteiro do "presenteável". Item sem Giftable = recusa silenciosa (0 ganho/perda).
       O cap diário usa DailyGiftLimit (default 1) + o marcador lastGiftDay por NPC já previsto
           no save desta spec (sem novo caminho de save).

4. ESCOPO/FORA DE ESCOPO — emendados:
       Inclui (adiciona): classificação do presente pelo gosto do NPC (matriz A6) com 5 níveis
           e delta variável; extensão de NpcGiftPreferences (Neutral/Hated); leitura das
           GiftPreferences no FriendshipService ao presentear.
       Continua FORA: romance/casamento (RomanticGiftTags/MarriageGiftTags — SOCIAL §8.1);
           presentes proibidos (ForbiddenGiftTags); multiplicador de qualidade aplicado
           (Prata/Ouro = +0% por enquanto); descoberta gradual do gosto (UI/codex); UI rica
           de corações; aniversários/festival. As 4 fontes de ganho seguem 4 (presente continua
           sendo UMA fonte — só muda o valor de +3 fixo para o delta por gosto).

5. DEPENDÊNCIA DE DADOS (gating honesto): a aplicação real depende de A4/ITEM_CATALOG criar as
   tags gift_* e marcar itens com ItemTag.Giftable, e de popular NpcGiftPreferences por NPC a
   partir da matriz A6. Enquanto A4/A6 não materializarem itens/tags, o presente cai no DEFAULT
   neutral (+2). NÃO declarar o gosto por NPC "validado" sem itens/tags reais no catálogo.

6. CRITÉRIO DE ACEITE ADITIVO — CA-6 (gosto por NPC):
       - Item LovedItemId do NPC → +12; item com tag liked → +6; item sem classificação → +2;
         item com tag disliked → -2; item com tag hated → -6 (e pode rebaixar nível).
       - Precedência hated > disliked > loved > liked > neutral respeitada.
       - Item sem ItemTag.Giftable → recusado, 0 ganho/perda.
       - npcId sem GiftPreferences populada → fallback neutral (+2), sem exceção.
       - Evidência: EditMode tests por nível + precedência + clamp em 0 + descida de nível.

7. ANTI-REGRESSÃO ADITIVO:
       - O delta de presente passa a ser variável; as OUTRAS 3 fontes (conversa +1, quest +8,
         compra +1) e seus caps NÃO mudam.
       - FriendshipLevelChangedEvent agora pode disparar em SUBIDA E DESCIDA de nível.
       - Nenhuma struct paralela de gosto (reusar NpcGiftPreferences).
       - Itens sem Giftable nunca afetam amizade.

8. DÉBITO REGISTRADO: o wiring leitura/escrita/geração/validação de NpcGiftPreferences é débito
   confirmado pela re-auditoria (struct morta). Detalhado na §6 da NPC_GIFT_TASTE_MATRIX_v1.0.md.
```

