# SPEC — NPC: Dar Presente com Reação por Gosto (loved/liked/neutral/disliked/hated)

> **Spec ID:** `fable_72_spec_npc_gift_giving_taste_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P4
> **Type:** Runtime
> **Domain:** NPC / Social
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A (consome a API e o save da F26 — deve rodar JUNTO/DEPOIS dela)
> **Can run with:** specs que não toquem `NPC/Friendship/**`, `NpcDefinition.cs`, `ItemTag.cs` nem a UI de diálogo/interação do NPC
> **Must not run with:**
> - F26 (`fable_26_spec_friendship_state_contract`) — ESTA spec usa `FriendshipService.AddPoints` e o marcador `lastGiftDay`/seção de save dela; F26 DEVE existir/rodar antes (mesma wave → resolver cadeia, não BLOQUEAR final)
> - qualquer spec que reescreva `NpcGiftPreferences`, a struct/registry de NPC ou a tag `ItemTag.Giftable`
> **Repo lock scope:** `NPC/Gifting/**` (novo), `NpcDefinition.NpcGiftPreferences` (extensão aditiva de 2 listas), `ItemTag.Giftable` (atribuição a itens `gift_*` via inicializador de itens, sem editar enum), assinatura de UI de presente no fluxo de interação de NPC
> **Depends on:**
> - F26/E35 (`fable_26_spec_friendship_state_contract`) — PROVÊ `FriendshipService` (API `AddPoints(npcId, delta, fonte)`, clamp em 0, `FriendshipLevelChangedEvent` em subida E descida pela emenda V3) e o marcador `lastGiftDay` por NPC na seção de save. ESTA spec NÃO reimplementa amizade; só implementa o FLUXO de presente que classifica o item e aplica o delta via essa API.
> - `docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md` (matriz de gostos por NPC — artefato A6; contrato de dados desta spec)
> - `NpcGiftPreferences` (struct existente, hoje código morto — `Assets/_Game/Scripts/NPC/NpcDefinition.cs:8`) — REUSAR/ESTENDER, não recriar
> - `ItemTag.Giftable` (`Assets/_Game/Scripts/Items/ItemTag.cs:15`) — REUSAR como porteiro do presenteável
> - GameEventBus (existente — `NpcInteractionStartedEvent`/`NpcInteractionEndedEvent` em `Core/Events/NpcInteractionEvents.cs`)
> **Blocks:** N/A (sistema folha; F25/F28/F35 consomem amizade pela API da F26, não por esta spec)
> **Scope:** a INTERAÇÃO de dar presente a um NPC e a resolução do nível de gosto. O jogador seleciona um item `Giftable` do inventário, escolhe "Dar presente" no NPC; o sistema checa o limite diário, classifica o item pelo gosto do NPC (matriz A6, por precedência), aplica o delta de amizade via `FriendshipService` da F26 (loved +12 / liked +6 / neutral +2 / disliked −2 / hated −6), consome o item, publica um evento de reação e fecha. Inclui: estender `NpcGiftPreferences` com `NeutralItemTags`/`HatedItemTags`; marcar os itens `gift_*` do catálogo com `ItemTag.Giftable`; popular `GiftPreferences` por NPC a partir da matriz; validador de completude da matriz; fallback `neutral` (+2) quando item/tag ainda não existe.
> **Out of scope:** estado/níveis/thresholds/persistência de amizade (são da F26 — esta spec só CHAMA `AddPoints`); romance/casamento (`RomanticGiftTags`/`MarriageGiftTags` — SOCIAL §8.1); presentes proibidos (`ForbiddenGiftTags`); multiplicador de qualidade Prata/Ouro aplicado (= +0% por enquanto, balance futuro); descoberta gradual do gosto (UI/codex); 2 presentes/semana e bônus de aniversário/festival; criação dos ITENS `gift_*` em si (são do ITEM_CATALOG/A4 — esta spec apenas marca os existentes como `Giftable` e aplica as tags `gift_*` aos itens já presentes); UI rica de corações.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [npc_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

Decisão **2.4 (A + CUSTOM)** do Refinamento v3 (`docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`,
linha 38 + acréscimo de re-auditoria 2 na linha 141): os `gift_*` são definidos no catálogo,
**MAS cada NPC reage de forma diferente** a presentes — em cinco níveis (loved / liked /
neutral / disliked / hated). `loved` dá afinidade alta; `hated` **REDUZ** a amizade. A decisão
explicita que **"F26 deixa de ser '+3 fixo' e passa a ler o gosto do NPC"**. Esse "ler o gosto e
aplicar o delta variável" foi capturado como emenda da F26 (`fable_26`, EMENDA 2026-06-13-V3,
itens 1-8) e o **contrato de dados** foi autorado na matriz
`docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md` (artefato A6, 23 NPCs).

A re-auditoria de código (2026-06-13) confirmou três fatos que tornam esta spec uma
INTEGRAÇÃO, não uma criação:

```text
1. NpcGiftPreferences JÁ EXISTE (Assets/_Game/Scripts/NPC/NpcDefinition.cs:8-14) com
   LikedItemTags, LovedItemIds, DislikedItemTags, DailyGiftLimit=1 — porém é CÓDIGO MORTO
   (nunca lida/escrita/gerada/validada). NpcDefinition.GiftPreferences (linha 45) já existe.
2. ItemTag.Giftable JÁ EXISTE (Assets/_Game/Scripts/Items/ItemTag.cs:15, valor 1L<<6) e
   ItemDefinition.EconomicFlags.CanGift já é derivado dela (ItemDefinition.cs:41) — mas
   NENHUM item recebeu a tag ainda (zero itens Giftable).
3. F26 (FriendshipService + seção de save + lastGiftDay por NPC) é a dona do ESTADO de
   amizade; esta spec é o FLUXO de presente que consome a API AddPoints e o marcador de dia.
```

Por isso a F26 fica com o ESTADO (pontos/níveis/save/cap diário base) e a **fable_72** entrega
o **ato de presentear**: a UI/interação de dar presente, a classificação do item pelo gosto do
NPC, o consumo do item, a aplicação do delta via `FriendshipService.AddPoints`, e a geração +
validação dos dados de gosto a partir da matriz A6. A separação evita inflar a F26 (save lock)
e mantém o fluxo de UI/inventário em uma spec de folha sem trava de schema.

## Problema

A matriz de gostos (A6) e a struct `NpcGiftPreferences` existem como **dados mortos**: não há
NENHUM caminho de runtime que (a) deixe o jogador escolher um item e oferecê-lo a um NPC, (b)
descubra se aquele item é loved/liked/neutral/disliked/hated para AQUELE NPC, (c) some o delta
correto de amizade respeitando a precedência, (d) respeite o limite diário, (e) consuma o item.
A struct sequer tem campos para `neutral`/`hated` (só `Liked`/`Loved`/`Disliked`), e nenhum
item carrega a tag `Giftable`. Sem esta spec, a decisão 2.4 CUSTOM permanece "no papel": a
emenda V3 da F26 prevê o delta variável, mas não há fluxo de presente que a alimente, nem
geração dos gostos por NPC, nem garantia de completude da matriz no runtime.

## Objetivo

Ao final desta spec, o jogo deve permitir: a partir da interação com um NPC, escolher um item
`Giftable` do inventário e DAR de presente; o sistema então (1) recusa silenciosamente itens
sem `ItemTag.Giftable` (0 ganho/perda, item não consumido); (2) recusa de forma amigável se o
limite diário do NPC (`DailyGiftLimit`, default 1, chave = dia absoluto via marcador
`lastGiftDay` da F26) já foi atingido (não consome o item, 0 ganho/perda); (3) caso contrário,
classifica o item pela precedência **hated > disliked > loved(id) > liked(tag) > neutral(default)**
contra `NpcGiftPreferences` daquele NPC, aplica o delta **loved +12 / liked +6 / neutral +2 /
disliked −2 / hated −6** via `FriendshipService.AddPoints(npcId, delta, GiftFromTaste)` (com o
clamp-em-0 e o `FriendshipLevelChangedEvent` — subida E descida — sendo responsabilidade da
F26), consome 1 unidade do item, marca o dia do presente e publica um evento de reação
(`NpcGiftReactionEvent`) para feedback de UI/diálogo. Adicionalmente: a struct
`NpcGiftPreferences` é estendida com `NeutralItemTags`/`HatedItemTags`; os itens `gift_*` do
catálogo recebem `ItemTag.Giftable` e as tags `gift_*` da §3 da matriz; cada NPC do roster tem
sua `GiftPreferences` populada a partir da matriz §4; um validador (estilo F30) garante a
completude. Quando o item/tag ainda não existe no catálogo, o presente cai no **fallback neutral
(+2)** — gating honesto, sem declarar o gosto "validado" sem dados reais.

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (decisão 2.4 A+CUSTOM, linha 38; acréscimo 2, linha 141 — reusar struct morta + ItemTag.Giftable)
docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md (A6 — vocabulário de tags §3, matriz por NPC §4, deltas §2, precedência §2.1, cap diário §2.2, débito §6)
docs/specs/a_implementar/fable/fable_26_spec_friendship_state_contract.md (EMENDA 2026-06-13-V3, itens 1-8 — provê FriendshipService.AddPoints, lastGiftDay, clamp, evento em subida/descida)
docs/decisions/ADR-0007-event-bus-gameplay-communication.md (comunicação de gameplay só via GameEventBus)
docs/game_rules/npc_rules.md (regras canônicas de NPC)
docs/game_rules/save_rules.md (DTO simples, IDs estáveis, load legado, sem refs Unity)
docs/game_rules/event_rules.md (contrato de eventos no GameEventBus)
.claude/skills/npc-dialogue-authoring/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/skills/editor-validator-authoring/SKILL.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
```

## Estado atual do repo

```text
EXISTE e NÃO RECRIAR (confirmado pela re-auditoria 2026-06-13):
- NpcGiftPreferences (struct C# pura) — Assets/_Game/Scripts/NPC/NpcDefinition.cs:8-14:
    public List<string> LikedItemTags    (liked, por tag)
    public List<string> LovedItemIds     (loved, por id de item)
    public List<string> DislikedItemTags (disliked, por tag)
    public int          DailyGiftLimit = 1
  → CÓDIGO MORTO. ESTENDER (aditivo) com NeutralItemTags e HatedItemTags. NÃO criar 2ª struct.
- NpcDefinition.GiftPreferences (campo) — NpcDefinition.cs:45 (já instanciado por default).
- ItemTag.Giftable — Assets/_Game/Scripts/Items/ItemTag.cs:15 (valor 1L<<6). NÃO editar o enum.
    Atribuída a ZERO itens hoje. → marcar os itens gift_* como Giftable (via inicializador de itens).
- ItemDefinition.Tags (ItemDefinition.cs:16) e ItemDefinition.EconomicFlags.CanGift derivado de
    ItemTag.Giftable (ItemDefinition.cs:41) — o porteiro "presenteável" JÁ está cabeado nos flags
    econômicos; reusar HasTag(ItemTag.Giftable) / CanGift, não criar 2º conceito de "presenteável".
- FriendshipService (F26) — DONO do estado de amizade: API AddPoints(npcId, delta, fonte),
    clamp em 0, FriendshipLevelChangedEvent (subida E descida pela emenda V3). NÃO reimplementar.
- Seção de save da amizade (F26) com marcador lastGiftDay por NPC — REUSAR como chave do cap diário.
    NÃO criar 2ª seção de save (esta spec NÃO adiciona schema de save).
- IInteractable (Assets/_Game/Scripts/Interaction/IInteractable.cs:5) — contrato de interação.
- NpcShopController (Assets/_Game/Scripts/NPC/NpcShopController.cs:21) — IInteractable VIVO do NPC,
    com DialogueModal + escolhas e GameEventBus.Publish(NpcInteractionStartedEvent) (linha 219).
    É o ponto de hook do menu de interação ("Dar presente" entra como nova escolha).
- NpcInteractionStartedEvent/NpcInteractionEndedEvent — Core/Events/NpcInteractionEvents.cs
    (readonly struct, tipos simples) — padrão de evento a espelhar.
- Validador estilo F30 — Assets/_Game/Scripts/Editor/Validation/ValidateItemAndShopData.cs
    (MenuItem + List<string> errors + return bool) — padrão a espelhar para o validador de matriz.

NÃO EXISTE (escopo desta spec):
- fluxo de "dar presente" (seleção de item Giftable → oferecer → resolver gosto → aplicar delta → consumir);
- campos NeutralItemTags/HatedItemTags em NpcGiftPreferences;
- classificador de gosto por precedência (hated>disliked>loved>liked>neutral);
- atribuição de ItemTag.Giftable a qualquer item; aplicação das tags gift_* aos itens;
- população de GiftPreferences por NPC a partir da matriz §4;
- validador de completude da matriz;
- NpcGiftReactionEvent (feedback do nível de gosto).

AUDITAR na Fase 0 (não inventar):
- Onde popular GiftPreferences por NPC: existe um inicializador/gerador de NpcDefinition
  (ItemDataInitializer cobre ITENS; conferir o equivalente de NPC). Se a fonte de NpcDefinition
  for um gerador editor, popular ali; se for SO autorado, documentar o caminho de geração honesto.
- Como o jogador SELECIONA o item para presentear: reusar a UI de inventário/seleção existente
  (mesma usada por consumir/equipar) ou abrir um seletor de itens Giftable. Definir na Fase 0 SEM
  criar 2º sistema de inventário; consumir 1 unidade via InventoryManager (API de remover item).
- Onde plugar a opção "Dar presente": no menu de escolhas do NpcShopController (root choices) ou
  num novo IInteractable de presente — preferir ESTENDER o fluxo de interação existente.
- Nome exato da API de gasto/remoção de item do InventoryManager (TryRemove/Remove/Consume) —
  confirmar assinatura antes de chamar.
- Confirmar que a F26 expõe AddPoints com uma "fonte" enum/strings e o marcador lastGiftDay
  acessível (GetLastGiftDay/SetLastGiftDay ou equivalente); se a F26 ainda não expôs o cap por
  fonte de presente, ESTA spec aplica o cap por dia consultando lastGiftDay + DailyGiftLimit.
```

## Engineering stories

```text
Como jogador, quero escolher um item do inventário e dar de presente a um NPC, para aumentar a amizade.
Como jogador, quero que NPCs diferentes reajam diferente: um item amado dá muito, um odiado PIORA a relação.
Como jogador, quero ser impedido (de forma amigável, sem perder o item) de dar mais presentes do que o limite do dia.
Como designer (matriz A6), quero que cada NPC do roster tenha gostos populados e validados, sem NPC "surdo" a presentes.
Como F26, quero ser a ÚNICA dona do estado de amizade: a fable_72 só me chama AddPoints com o delta resolvido.
Como struct existente (NpcGiftPreferences), quero ser ESTENDIDA (neutral/hated), não duplicada.
Como tag existente (ItemTag.Giftable), quero ser o ÚNICO porteiro do "presenteável" — itens sem ela não afetam amizade.
Como gating honesto, quero cair no fallback neutral (+2) enquanto o item/tag do gosto ainda não existe no catálogo.
```

## Escopo

```text
Inclui:
- [STRUCT] Estender NpcGiftPreferences (NpcDefinition.cs) com, de forma ADITIVA:
    public List<string> NeutralItemTags { get; set; } = new List<string>();
    public List<string> HatedItemTags   { get; set; } = new List<string>();
  Mantendo LikedItemTags/LovedItemIds/DislikedItemTags/DailyGiftLimit intactos. Sem refs Unity.
- [CLASSIFICADOR] GiftTasteResolver (NPC/Gifting/, NOVO, C# puro testável): dada uma
  NpcGiftPreferences + o ItemDefinition (id + tags gift_*), devolve o nível de gosto por
  PRECEDÊNCIA (hated > disliked > loved(id) > liked(tag) > neutral(default)) e o delta
  correspondente (loved +12 / liked +6 / neutral +2 / disliked −2 / hated −6). Deltas marcados
  como "proposta a calibrar" conforme matriz §2 (origem documentada). Item sem ItemTag.Giftable =
  NotGiftable (sem nível, sem delta). GiftPreferences nula/vazia = fallback neutral (+2).
- [INTERAÇÃO] GiftGivingInteraction (NPC/Gifting/, NOVO): orquestra o ato — recebe (npcId,
  itemId, ItemDefinition), valida Giftable, valida cap diário (DailyGiftLimit + lastGiftDay da
  F26 vs dia absoluto do calendário), classifica via GiftTasteResolver, aplica delta via
  FriendshipService.AddPoints(npcId, delta, fonte=GiftFromTaste), consome 1 unidade via
  InventoryManager, marca lastGiftDay, publica NpcGiftReactionEvent. Tudo via GameEventBus para
  o feedback; sem chamada direta MonoBehaviour→MonoBehaviour de gameplay.
- [HOOK UI] Opção "Dar presente" no menu de interação do NPC (estender NpcShopController root
  choices ou fluxo de interação equivalente), abrindo o seletor de itens Giftable do inventário
  (reusar UI existente; NÃO criar 2º inventário). Confirmação de seleção → GiftGivingInteraction.
- [EVENTO] NpcGiftReactionEvent (Core/Events/, NOVO, readonly struct): { string NpcId,
  string ItemId, GiftTasteLevel Level, int AppliedDelta } — feedback do nível de gosto p/ UI/diálogo.
- [TAGS/ITENS] Marcar os itens gift_* existentes do catálogo com ItemTag.Giftable e aplicar as
  tags gift_* da matriz §3 (no inicializador/gerador de itens — ItemDataInitializer ou equivalente
  auditado na Fase 0). NÃO criar itens novos (são do A4); só taguear os existentes.
- [GERAÇÃO] Popular NpcGiftPreferences de cada NPC do roster a partir da matriz §4 (no caminho de
  geração/definição de NpcDefinition auditado na Fase 0). Sem GameObject.Find em runtime.
- [VALIDADOR] GiftTasteMatrixValidator (Editor, estilo F30): todo NPC do roster tem GiftPreferences
  não vazia; todo NPC tem >=1 hated; toda tag gift_* citada na matriz existe no catálogo; todo
  LovedItemId existe; nenhum item loved/liked sem ItemTag.Giftable. Erros => return false.
- [EDITMODE TESTS] precedência de classificação; cada delta por nível; recusa sem Giftable;
  fallback neutral sem GiftPreferences; cap diário (bloqueio no mesmo dia, liberação no dia
  seguinte); consumo de 1 unidade; presente hated produz delta negativo; round-trip da extensão
  de struct (Neutral/Hated default vazio); shape/defaults do NpcGiftReactionEvent.
```

## Fora de escopo

```text
- Estado/níveis/thresholds/persistência/clamp/evento de NÍVEL de amizade (são da F26 — esta spec
  só chama AddPoints; clamp em 0 e FriendshipLevelChangedEvent em subida/descida são da F26).
- Adição de seção de save (esta spec NÃO altera schema; reusa lastGiftDay da F26).
- Criação dos ITENS gift_* (são do ITEM_CATALOG/A4) — esta spec só TAGUEIA os existentes.
- Romance/casamento: RomanticGiftTags/MarriageGiftTags/PolyCommitmentGiftTags (SOCIAL §8.1).
- Presentes proibidos (ForbiddenGiftTags) com penalidade narrativa.
- Multiplicador de qualidade Prata/Ouro aplicado (matriz §2.1: +0% por enquanto, balance futuro).
- Descoberta gradual do gosto ("favorito/odiado descoberto") — UI/codex futuro.
- 2 presentes/semana, aniversários, bônus de festival (SOCIAL §8.3 — futuro).
- UI rica de corações; animação de arte; áudio/SFX da reação.
- Outras 3 fontes de ganho da F26 (conversa/quest/compra) — não tocadas aqui.
```

## Regras de não duplicação

```text
- REUSAR e ESTENDER NpcGiftPreferences (NpcDefinition.cs:8) — proibido criar 2ª struct de gosto.
- ItemTag.Giftable (ItemTag.cs:15) é o ÚNICO porteiro do presenteável; usar HasTag(Giftable)/
  EconomicFlags.CanGift (ItemDefinition.cs:41) — não criar 2º conceito de "item de presente".
- FriendshipService (F26) é o ÚNICO dono do estado/delta de amizade — chamar AddPoints, NUNCA
  reimplementar pontos/níveis/clamp/evento de nível/seção de save.
- lastGiftDay (save da F26) é a ÚNICA chave do cap diário — não criar 2º caminho de save.
- Dia absoluto vem do GameCalendarService (já usado pela F26) — não duplicar relógio.
- Inventário: consumir item via InventoryManager existente — não criar 2º inventário/seletor.
- Comunicação de feedback só via GameEventBus (ADR-0007) — sem chamada direta de gameplay.
- Sem GameObject.Find/FindObjectOfType em runtime (refs serializadas / bootstrap / config).
```

## Critérios de aceite

### CA-1 — Classificação por precedência e delta correto
- Para um NPC com GiftPreferences populada: item em `LovedItemIds` → loved (+12); item com tag em
  `LikedItemTags` → liked (+6); item com tag em `DislikedItemTags` → disliked (−2); item com tag em
  `HatedItemTags` → hated (−6); item `Giftable` sem classificação → neutral (+2). Precedência
  **hated > disliked > loved(id) > liked(tag) > neutral**: um item que é loved por id E hated por
  tag resolve como **hated** (a rejeição vence). (Deltas = proposta a calibrar — matriz §2.)
- Evidência: EditMode tests por nível + teste explícito de empate hated×loved.

### CA-2 — Porteiro Giftable e fallback neutral
- Item SEM `ItemTag.Giftable` → recusa silenciosa: 0 ganho/perda de amizade, item NÃO consumido,
  sem `AddPoints`. NPC com `GiftPreferences` nula/vazia (ainda não populada) → fallback **neutral
  (+2)** para qualquer item Giftable, sem exceção.
- Evidência: EditMode tests (sem Giftable = no-op; GiftPreferences vazia = neutral).

### CA-3 — Limite diário (DailyGiftLimit + lastGiftDay)
- Dar presente quando o NPC ainda não recebeu presente no dia absoluto atual aplica o delta e marca
  o dia (`lastGiftDay` da F26). Segundo presente no MESMO dia (cap `DailyGiftLimit`, default 1) →
  recusa amigável: 0 ganho/perda, item NÃO consumido. No dia absoluto seguinte volta a ser aceito.
- Evidência: EditMode tests com dias absolutos sintéticos (mesmo dia bloqueia; dia+1 libera).

### CA-4 — Aplicação via F26 e consumo do item
- Quando o presente é aceito: `FriendshipService.AddPoints(npcId, delta, GiftFromTaste)` é chamado
  exatamente uma vez com o delta resolvido; 1 unidade do item é consumida do inventário; o
  `NpcGiftReactionEvent` é publicado com (NpcId, ItemId, Level, AppliedDelta). O clamp-em-0 e o
  `FriendshipLevelChangedEvent` (subida E descida) são de responsabilidade da F26 — esta spec NÃO
  os reimplementa; presente HATED produz delta negativo entregue à F26.
- Evidência: EditMode tests (AddPoints chamado 1× com delta certo; item consumido 1×; evento
  publicado; delta negativo no hated).

### CA-5 — Estrutura estendida e dados populados/validados
- `NpcGiftPreferences` tem `NeutralItemTags` e `HatedItemTags` (default lista vazia, round-trip
  preservado). Cada NPC do roster v1.1 tem `GiftPreferences` populada da matriz §4 (>=1 hated por
  NPC). O `GiftTasteMatrixValidator` retorna 0 erros: nenhuma tag gift_* citada ausente do
  catálogo, todo `LovedItemId` existe, nenhum item loved/liked sem `ItemTag.Giftable`, nenhum NPC
  com `GiftPreferences` vazia. Enquanto A4 não materializar item/tag, o validador reporta o débito
  como WARNING (não erro fatal) e o runtime cai no fallback neutral — gating honesto.
- Evidência: EditMode test da extensão (defaults vazios) + execução do validador (log/exit) +
  nota explícita de quais tags/itens ainda são débito do A4.

### CA-6 — Não-regressão de sistemas reusados
- F26 (pontos/níveis/save/cap das outras 3 fontes) intacta; `ItemTag.Giftable` (enum) inalterada;
  fluxo de compra/venda/diálogo do NpcShopController inalterado (a opção "Dar presente" é
  ADITIVA); nenhum 2º inventário/tracker de relacionamento/struct de gosto criado.
- Evidência: builds 0E + revisão anti-regressão no report (Spec Compliance Matrix).

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/NpcDefinition.cs                         (EDITAR — +NeutralItemTags/+HatedItemTags em NpcGiftPreferences, aditivo)
Assets/_Game/Scripts/NPC/Gifting/GiftTasteLevel.cs                (NOVO — enum: Loved/Liked/Neutral/Disliked/Hated/NotGiftable)
Assets/_Game/Scripts/NPC/Gifting/GiftTasteResolver.cs             (NOVO — classificador puro por precedência + delta)
Assets/_Game/Scripts/NPC/Gifting/GiftGivingInteraction.cs         (NOVO — orquestra ato; valida cap; chama F26; consome item; publica evento)
Assets/_Game/Scripts/Core/Events/NpcGiftReactionEvent.cs          (NOVO — readonly struct de feedback; OU aditivo em NpcInteractionEvents.cs se Fase 0 confirmar a pasta canônica)
Assets/_Game/Scripts/NPC/NpcShopController.cs                     (EDITAR — opção "Dar presente" aditiva no menu de escolhas; reusa DialogueModal/seletor de inventário)
Assets/_Game/Scripts/Editor/<gerador de NpcDefinition auditado>   (EDITAR — popular GiftPreferences por NPC a partir da matriz §4)
Assets/_Game/Scripts/Editor/ItemDataInitializer.cs (ou equivalente) (EDITAR — marcar itens gift_* com ItemTag.Giftable + aplicar tags gift_* §3)
Assets/_Game/Scripts/Editor/Validation/GiftTasteMatrixValidator.cs (NOVO — estilo F30: MenuItem + List<string> errors + return bool)
Assets/_Game/Tests/EditMode/City/GiftGivingTasteTests.cs          (NOVO)
docs/validation/fable_72_spec_npc_gift_giving_taste_runtime_execution_report.md
docs/validation/playmode/fable_72_human_test_scenario.md
```

> Nota: o caminho EXATO do gerador de NpcDefinition e do inicializador de itens é confirmado na
> Fase 0 (ver Estado atual do repo). Se a definição de NPC for SO autorado (não gerador), a
> população de GiftPreferences é documentada como caminho honesto e o que não for materializável
> sem editar `.asset` fica BLOQUEADO/DÉBITO documentado — nunca editar YAML manualmente.

## Contratos

### Data contracts
- `NpcGiftPreferences` (estendida, C# puro, sem refs Unity): `LikedItemTags`, `LovedItemIds`,
  `DislikedItemTags`, **+`NeutralItemTags`**, **+`HatedItemTags`** (`List<string>`, default vazio),
  `DailyGiftLimit` (`int`, default 1). Tags são strings `gift_*` (vocabulário §3); ids são item ids.
- `GiftTasteLevel` (enum save-safe-irrelevante — transiente, mas valores explícitos): `Loved`,
  `Liked`, `Neutral`, `Disliked`, `Hated`, `NotGiftable`.
- Deltas por nível (proposta a calibrar — matriz §2): Loved=+12, Liked=+6, Neutral=+2,
  Disliked=−2, Hated=−6. NotGiftable = sem delta (no-op).

### Runtime contracts
- `GiftTasteResolver.Resolve(NpcGiftPreferences prefs, string itemId, ItemTag tags, IEnumerable<string> giftTagsDoItem)`
  → `(GiftTasteLevel level, int delta)`. Puro/determinístico. Precedência:
  `Hated` (item tem tag em HatedItemTags) > `Disliked` (tag em DislikedItemTags) >
  `Loved` (itemId em LovedItemIds) > `Liked` (tag em LikedItemTags) > `Neutral` (default).
  Se `!tags.HasFlag(ItemTag.Giftable)` → `NotGiftable`. Se `prefs` nula/vazia → `Neutral`.
- `GiftGivingInteraction.TryGiveGift(npcId, itemId)` → resultado (Accepted/RefusedNotGiftable/
  RefusedDailyLimit): valida Giftable, cap diário (DailyGiftLimit vs lastGiftDay@diaAbsoluto),
  classifica, chama `FriendshipService.AddPoints(npcId, delta, GiftFromTaste)`, consome 1 unidade
  via InventoryManager, marca lastGiftDay, publica `NpcGiftReactionEvent`. Refs por injeção/
  serializadas (sem busca global).

### Event contracts
- Adiciona: `NpcGiftReactionEvent` (readonly struct; espelha `NpcInteractionEvents.cs`):
  `{ string NpcId, string ItemId, GiftTasteLevel Level, int AppliedDelta }`. Publicado SÓ quando o
  presente é aceito (delta aplicado). Consome (sem alterar): nenhum evento de entrada obrigatório —
  a interação é disparada pela escolha de UI; o resultado é exposto por este evento + pela F26
  (`FriendshipLevelChangedEvent`, que continua sendo da F26).
- NÃO altera `NpcInteractionStartedEvent`/`NpcInteractionEndedEvent` nem `FriendshipLevelChangedEvent`.

### Save/UI contracts
- Save: N/A — esta spec NÃO adiciona seção nem altera schema. O cap diário usa o marcador
  `lastGiftDay` por NPC já persistido pela F26 (reuso). Sem refs Unity em DTO (nenhum DTO novo).
- UI: opção "Dar presente" ADITIVA no menu de escolhas de interação do NPC; reusa o seletor de
  itens do inventário existente para escolher o item Giftable; feedback do nível via
  `NpcGiftReactionEvent` (texto/diálogo de reação é consumo de UI, placeholder mínimo aceitável).

## Sistemas afetados

```text
NPC (struct de gosto estendida + fluxo de presente + hook de interação)
NPC/Friendship (CONSUMIDO via API AddPoints — não alterado)
Inventário (consumo de 1 unidade do item via API existente)
Item catalog / tags (marcação Giftable + tags gift_* nos itens existentes)
Event bus (+1 evento NpcGiftReactionEvent)
Editor (gerador de NpcDefinition para popular GiftPreferences + validador de matriz)
Calendar (CONSUMIDO — dia absoluto para o cap diário — não alterado)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/NpcDefinition.cs            (apenas extensão aditiva de NpcGiftPreferences)
Assets/_Game/Scripts/NPC/Gifting/**                  (novos: resolver, interação, enum)
Assets/_Game/Scripts/NPC/NpcShopController.cs        (apenas opção "Dar presente" aditiva)
Assets/_Game/Scripts/Core/Events/**                  (evento novo de reação — ou pasta canônica confirmada na Fase 0)
Assets/_Game/Scripts/Editor/** (gerador de NpcDefinition + inicializador de itens auditados na Fase 0; validador novo)
Assets/_Game/Tests/EditMode/City/**                  (testes)
docs/validation/**                                   (report + cenário humano)
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida; wiring de cena/SO é humano)
Packages/** ; ProjectSettings/**
Assets/_Game/Scripts/Items/ItemTag.cs (enum Giftable já existe — NÃO editar o enum)
NPC/Friendship/** (FriendshipService/seção de save da F26 — CONSUMIR, não alterar)
GameCalendarService (consumir dia absoluto, não alterar)
NpcRegistry/NpcDefinition além da extensão aditiva de NpcGiftPreferences (não mexer no resto da classe)
UI rica de corações / consumo de FriendshipLevelChangedEvent (é da F26)
Criação de itens gift_* novos (é do ITEM_CATALOG/A4 — aqui só tagueia existentes)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria (sem código de runtime)
Confirmar: caminho de geração/definição de NpcDefinition (gerador editor vs SO autorado) p/
popular GiftPreferences; inicializador de itens p/ marcar Giftable + tags gift_* (ItemDataInitializer
ou equivalente); assinatura exata da API de remover/consumir item do InventoryManager; como a F26
expõe AddPoints(npcId, delta, fonte) e o acesso a lastGiftDay/dia absoluto; pasta canônica de
eventos; ponto de hook do "Dar presente" no NpcShopController; reuso do seletor de inventário.
Registrar quais tags/itens gift_* já existem no catálogo (resto = débito A4 → fallback neutral).

### Fase 1 — Struct + enum + resolver puro
Estender NpcGiftPreferences (NeutralItemTags/HatedItemTags). Criar GiftTasteLevel e
GiftTasteResolver (precedência + delta) — 100% C# puro, testável. EditMode tests de
classificação/precedência/delta/fallback/NotGiftable.

### Fase 2 — Interação + evento + consumo
GiftGivingInteraction (valida Giftable → cap diário → resolve → AddPoints F26 → consome 1
unidade → marca lastGiftDay → publica NpcGiftReactionEvent). NpcGiftReactionEvent (struct).
Hook "Dar presente" aditivo no NpcShopController (escolha + seletor de inventário reusado).
EditMode tests: cap diário, AddPoints 1× com delta, consumo 1×, delta negativo no hated, evento.

### Fase 3 — Dados (tags/itens + gostos por NPC) + validador
Marcar itens gift_* existentes com ItemTag.Giftable e aplicar tags gift_* §3 (inicializador).
Popular GiftPreferences por NPC a partir da matriz §4 (gerador). GiftTasteMatrixValidator
(completude: todo NPC com gostos + >=1 hated; tags existem; LovedItemId existe; loved/liked com
Giftable). Débito A4 reportado como WARNING (não erro fatal).

### Fase 4 — Validação e fechamento
csproj includes; run_strict_validation (exit 0); execução do validador (log/exit); execution
report com Spec Compliance Matrix + Testing Quality Gate; cenário humano de feel (dar loved/hated,
ver reação e número de amizade subir/descer). Sem claim ACCEPTED.
```

## Paralelização

- Parallelizable: CONDITIONAL — depende da F26 (API + save). Mesma wave → resolver a cadeia
  (executar/usar a F26 primeiro), nunca pivotar nem marcar BLOCKED final por dependência same-wave.
- Parallel group: N/A.
- Can run with: specs que não toquem `NPC/Friendship/**`, `NpcDefinition.cs`, `ItemTag.cs` nem o
  fluxo de interação/diálogo do NPC.
- Must not run with: F26 (provê API/save — ordenar antes) e qualquer spec que reescreva
  `NpcGiftPreferences`, o registry/definição de NPC ou a tag `ItemTag.Giftable`.
- Shared files/systems that require lock: `NpcDefinition.cs` (extensão aditiva), `NpcShopController.cs`
  (opção aditiva), API/save da F26 (consumo), inicializador de itens / gerador de NPC (Editor).
- Reason: esta spec ESTENDE estruturas compartilhadas e CONSOME a API/save da F26; não introduz
  schema de save próprio, mas escreve em arquivos partilhados de NPC/itens.

## Impacto em save/load

```text
Does this change save schema? NO — não adiciona seção nem campo de save; reusa lastGiftDay da F26.
Does this add a save section? NO (a seção de amizade é da F26).
Does this require migration? NO.
Does this persist Unity references? NO — NpcGiftPreferences é C# puro (strings/ints), sem refs Unity.
Note: GiftTasteLevel é transiente (não persistido); o estado persistido (pontos/lastGiftDay) é da F26.
```

## Impacto em eventos

```text
Adds events: YES — NpcGiftReactionEvent(NpcId, ItemId, GiftTasteLevel, AppliedDelta) publicado só no aceite.
Changes existing events: NO (NpcInteractionStarted/Ended e FriendshipLevelChanged intactos).
Requires unsubscribe pattern: YES — qualquer assinatura no hook de UI/interação faz unsubscribe em OnDisable.
```

## Impacto em UI/Unity

```text
Changes UI: YES — opção "Dar presente" aditiva no menu de interação do NPC + seletor de item reusado.
Changes scenes: NO (wiring de refs é humano de inspetor; agente não edita YAML).
Changes prefabs: NO.
Changes ScriptableObjects/assets: CONDITIONAL — marcação Giftable/tags gift_* e gostos por NPC só
  via inicializador/gerador editor (AssetDatabase/API), NUNCA edição manual de YAML; o que exigir
  edição direta de .asset fica BLOQUEADO/DÉBITO documentado.
Requires Play Mode final validation: YES (feel de dar presente, reação e variação de amizade).
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
```

## Riscos técnicos

```text
Risco: criar 2ª struct de gosto / 2º conceito de "presenteável" / 2º tracker de amizade.
  Mitigação: REUSAR NpcGiftPreferences (estender), ItemTag.Giftable (HasTag/CanGift) e
  FriendshipService.AddPoints; anti-regressão proíbe paralelos; skill system-reuse-audit na Fase 0.
Risco: dar pontos sem respeitar o cap diário (spam de presente).
  Mitigação: cap = DailyGiftLimit + lastGiftDay (F26) vs dia absoluto do GameCalendarService; teste CA-3.
Risco: precedência errada (loved de alto valor sobrepondo hated).
  Mitigação: ordem fixa hated>disliked>loved>liked>neutral (matriz §2.1); teste de empate hated×loved (CA-1).
Risco: aplicar delta negativo causando exceção/abaixo de 0 na amizade.
  Mitigação: clamp em 0 é da F26 (AddPoints); esta spec só entrega o delta negativo; teste do delta no hated.
Risco: itens gift_* ainda não existem no catálogo (débito A4) → falso "gosto validado".
  Mitigação: gating honesto — fallback neutral (+2); validador reporta tags/itens ausentes como
  WARNING; report nunca declara gosto por NPC "validado" sem itens/tags reais.
Risco: editar .asset/.prefab para popular gostos/tags.
  Mitigação: só via inicializador/gerador editor (AssetDatabase/API); manual YAML proibido; o que
  não der por API fica BLOQUEADO/DÉBITO documentado.
Risco: consumir item por API errada do InventoryManager (duplo consumo / não consome).
  Mitigação: confirmar assinatura na Fase 0; teste de consumo de 1 unidade exatamente (CA-4).
Risco: F26 ainda não implementada na mesma wave.
  Mitigação: resolver cadeia (F26 primeiro); até lá, GiftGivingInteraction usa a API da F26 como
  contrato — se a F26 não existir, marcar BLOCKED_BY_DEPENDENCY_PENDING (temporário), não falha final.
```

## Rollback

```text
Remover NPC/Gifting/**, o evento NpcGiftReactionEvent, a opção "Dar presente" do NpcShopController e
o validador desliga o fluxo de presente. As 2 listas extras de NpcGiftPreferences ficam inertes
(default vazio — sem efeito). A tag Giftable e as tags gift_* aplicadas aos itens permanecem
inofensivas (só habilitam CanGift). F26 (estado de amizade) e as outras 3 fontes permanecem
intactas. Nenhum save afetado (sem schema novo). A decisão 2.4 CUSTOM volta ao estado de débito.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar gerador de NpcDefinition + inicializador de itens; API de consumo do
        InventoryManager; AddPoints/lastGiftDay/dia absoluto (F26/calendário); pasta de eventos;
        hook do "Dar presente"; seletor de inventário reusável; tags/itens gift_* já existentes.
- [ ] T002 — Estender NpcGiftPreferences (NeutralItemTags/HatedItemTags, aditivo) + GiftTasteLevel
        + GiftTasteResolver (precedência + delta) + EditMode tests de classificação/precedência/
        delta/fallback/NotGiftable.
- [ ] T003 — GiftGivingInteraction (Giftable → cap diário → resolve → AddPoints F26 → consome 1
        unidade → marca lastGiftDay → publica evento) + NpcGiftReactionEvent + hook aditivo
        "Dar presente" no NpcShopController + EditMode tests (cap, AddPoints 1×, consumo, hated, evento).
- [ ] T004 — Marcar itens gift_* com ItemTag.Giftable + aplicar tags gift_* (§3) + popular
        GiftPreferences por NPC (§4) via gerador/inicializador editor + GiftTasteMatrixValidator
        (completude; débito A4 = WARNING).
- [ ] T005 — csproj includes; run_strict_validation; execução do validador; execution report
        (Spec Compliance Matrix + Testing Quality Gate) + cenário humano de feel.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed runtime code: YES | Changed deterministic logic: YES (precedência/delta/cap diário/classificação)
- Changed Unity scene/prefab/asset wiring: CONDITIONAL (tags/gostos via inicializador editor — sem YAML manual; ref de UI é wiring humano)
- Automated tests added/updated: YES (EditMode — precedência, deltas por nível, recusa sem Giftable,
  fallback neutral, cap diário, consumo de 1 unidade, delta negativo hated, AddPoints 1× com delta,
  shape do NpcGiftReactionEvent, defaults vazios da extensão de struct)
- Automated tests command: dotnet test (EditMode) / Unity Test Runner EditMode — registrar comando e resultado no report
- Manual Play Mode scenario: YES — docs/validation/playmode/fable_72_human_test_scenario.md
  (dar item loved → amizade sobe muito; dar item hated → amizade desce; 2º presente no dia = recusa
  sem perder item; item sem Giftable = recusa silenciosa)
- Justification if no automated tests: N/A (lógica determinística testável)
- Residual risk: deltas (+12/+6/+2/−2/−6) são PROPOSTA A CALIBRAR (matriz §2); aplicação real do
  gosto por NPC depende de A4 materializar itens/tags gift_* (até lá: fallback neutral) — documentar
  honestamente, sem declarar gosto por NPC "validado" sem itens/tags reais.

## Definition of Done

```text
Fluxo de "dar presente" funcional: seleção de item Giftable → oferta ao NPC → cap diário → classificação
por precedência (hated>disliked>loved>liked>neutral) → delta correto via FriendshipService.AddPoints (F26)
→ consumo de 1 unidade → NpcGiftReactionEvent. NpcGiftPreferences estendida (Neutral/Hated); itens gift_*
marcados Giftable + tags gift_* aplicadas; GiftPreferences populada por NPC da matriz §4; validador de
completude (0 erros, débito A4 = WARNING). Nenhuma struct/tracker/inventário/seção de save paralelo criado;
ItemTag enum inalterado; F26 e fluxo de compra/venda/diálogo intactos. Builds 0E; run_strict_validation
exit 0; execution report com Spec Compliance Matrix + cenário humano. Sem claim ACCEPTED.
```

## Anti-regressão

```text
- NpcGiftPreferences ESTENDIDA (nunca duplicada); LikedItemTags/LovedItemIds/DislikedItemTags/DailyGiftLimit intactos.
- ItemTag.Giftable é o único porteiro do presenteável (HasTag/CanGift); enum ItemTag inalterado.
- FriendshipService (F26) é o único dono de pontos/níveis/clamp/evento de nível/seção de save — só AddPoints é chamado.
- Cap diário usa exclusivamente lastGiftDay (F26) + dia absoluto do GameCalendarService — nenhum 2º caminho de save/relógio.
- As outras 3 fontes de amizade (conversa +1, quest +8, compra +1) e seus caps NÃO mudam.
- Item sem ItemTag.Giftable nunca afeta amizade nem é consumido.
- Presente hated entrega delta negativo à F26 (que clampa em 0 e publica nível em descida) — esta spec não clampa.
- Comunicação de feedback só via GameEventBus (ADR-0007); sem GameObject.Find/FindObjectOfType em runtime.
- Fluxo de compra/venda/diálogo do NpcShopController intacto (opção "Dar presente" é aditiva).
- Itens gift_* novos NÃO são criados aqui (são do A4); só taguear os existentes; gating honesto via fallback neutral.
```
