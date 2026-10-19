# SPEC — Itens Mágicos: Não-Identificados + Identificação + Efeitos Únicos

> **Spec ID:** `fable_31_spec_magic_items_unidentified_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 4
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** Inventory / Cave
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_4
> **Can run with:** specs que não tocam ItemDataSO, inventário, tooltip/uso de item ou loot de baú
> **Must not run with:** F07, F08 (SpellCastService/inventory locks)
> **Repo lock scope:** ItemDataSO (campos aditivos), tooltip/uso de item, loot de baú
> **Depends on:**
> - F08 (consumíveis/efeitos de uso)
> - F06 (loot resolver p/ baús raros)
> **Blocks:**
> - F25 (Veska identifica)
> - F32 (gerador inclui os mágicos)
> **Scope:** estado unidentified por instância + 8 itens mágicos do catálogo com efeito real.
> **Out of scope:** maldições, encantamento pelo player, itens mágicos equipáveis além dos 8 (acessórios = F23).

required_adrs: []
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

O ITEM_CATALOG §19 define itens mágicos achados na caverna que chegam NÃO-identificados
("Pingente Estranho") e só revelam nome/efeito após identificação — pela Veska (serviço
de 120g — F25) ou por pergaminho `scroll_identify`. É a fantasia clássica de loot
misterioso: o baú raro entrega um objeto vago, e a decisão de pagar/gastar para saber o
que é faz parte do loop da caverna.

O repo já tem as peças que esta spec consome: `ItemDataSO`/`ItemDatabase`/`InventoryManager`
(stacks), uso de consumível (F08), baús da caverna com loot determinístico (F06) e
tooltips (IMGUI hoje, Canvas com F14). O ponto técnico decisivo, a auditar na Fase 0, é
que os stacks provavelmente NÃO têm metadata por instância — e a solução escolhida evita
criá-la: o padrão PAR DE ITENS (`unidentified_trinket_N` ↔ item real), em que identificar
é um swap 1:1 no inventário. Zero schema novo de save.

O que fica fora: maldições, encantamento pelo player e itens mágicos equipáveis além dos
8 (acessórios são da F23).

## Problema

Sem identificação e sem os 8 itens mágicos, os baús raros da caverna entregam apenas
recursos previsíveis — não há mistério, não há motivo de visita à Veska (F25 fica sem o
serviço central dela) e o F32 (gerador do catálogo) não tem os campos/IDs dos mágicos
para materializar. Criar metadata por instância nos stacks para resolver isso seria
mudança invasiva de save/schema, desproporcional ao benefício.

## Objetivo

Ao final desta spec, o projeto deve ter o padrão par-de-itens de identificação
(unidentified ↔ real, swap 1:1 via `IdentifyItem`), os 8 itens mágicos do catálogo com
efeitos reais roteados pelos sistemas existentes (4 de uso, 4 passivos via
`ItemPassiveTracker`), e as fontes (baús raros nível 10+ e rotação semanal determinística
da Veska) — sem metadata de instância, sem schema novo de save e com o stable-run da
caverna intacto.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§19)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§3-4)
.claude/rules/testing-quality-gate.md
.claude/rules/cave-stable-run.md
.claude/skills/spec-execution/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- ItemDataSO / ItemDatabase / InventoryManager (stacks);
- uso de consumível com efeitos (F08);
- baús da caverna com loot determinístico (F06);
- tooltips IMGUI/Canvas (F14);
- DayService / sistema de reparo / sistema de luz (destinos dos hooks passivos).
Não existe:
- identificação de itens; efeitos únicos; os 8 itens mágicos;
- tracker de passivos por presença no inventário;
- rotação semanal de estoque da Veska.
Auditar Fase 0:
- stacks têm metadata por instância? (premissa: NÃO — se não tiverem, o padrão par de
  itens unidentified_X → X na identificação resolve por troca de item, zero schema novo);
- shape do tooltip (onde esconder nome/efeito);
- pontos de hook: luz (candle/lantern), slots de inventário (pouch), reparo (whetstone),
  DayService (hourglass).
```

## Engineering stories

```text
Como jogador, quero achar um "Pingente Estranho" num baú raro e decidir se pago para identificá-lo.
Como jogador, quero que cada item mágico identificado tenha efeito real e único, não flavor text.
Como InventoryManager, quero identificar por swap 1:1 de item, sem metadata de instância nova.
Como F25 (Veska), quero um serviço IdentifyItem(stack) pronto para consumir.
```

## Escopo

```text
Inclui:
- padrão PAR DE ITENS (sem metadata de instância): unidentified_trinket_N (nome vago,
  tooltip "???") ↔ item real; identificar = swap 1:1 no inventário;
- 8 itens mágicos (catálogo): pendant_of_echoes (mostra HP de inimigos — flag HUD),
  lantern_of_true_sight (revela mimics/ambush em raio 6 — flag p/ F24),
  pouch_of_holding (+6 slots inventário), whetstone_eternal (reparo 1×/dia grátis),
  candle_of_the_depths (luz não consumível, raio maior), bell_of_warding (1 uso:
  inimigos em raio 8 fogem 5s), mirror_of_return (1 uso: teleporta à entrada do nível),
  hourglass_of_dawn (1 uso: adianta para 6h — consome DayService);
- fontes: baús raros nível 10+ (peso no loot resolver F06) + Veska vende 1 unidentified
  por semana (rotação determinística);
- identificação: serviço IdentifyItem(stack) — consumido por F25 (Veska, 120g) e pelo
  pergaminho scroll_identify;
- efeitos contínuos (pendant/lantern/pouch/candle): ItemPassiveTracker no inventário
  (presença do item ativa flag — ponto único por efeito, hooks nomeados nos sistemas
  existentes);
- EditMode tests: swap de identificação, cada efeito de uso, pouch +slots (ganho/perda/
  overflow), rotação determinística da Veska.
```

## Fora de escopo

```text
Não inclui:
- maldições;
- encantamento pelo player;
- itens mágicos equipáveis além dos 8 (acessórios = F23);
- arte/ícones finais;
- a UI/serviço da Veska em si (F25 consome o IdentifyItem daqui);
- materialização dos assets no gerador do catálogo (F32 inclui os mágicos).
```

## Regras de não duplicação

```text
Não criar metadata de instância em stacks (o par de itens resolve).
Efeitos passam pelos sistemas existentes (luz, slots de inventário, reparo, DayService)
via hooks nomeados — nenhum sistema paralelo de buff/efeito.
Não criar segundo resolver de loot — peso novo no resolver F06.
Não criar segundo fluxo de uso de consumível — efeitos de uso registrados no fluxo F08.
```

## Critérios de aceite

### CA-1 Loop de identificação completo

- Baú raro (nível 10+) dropa unidentified; o tooltip não revela nome/efeito ("???");
  identificar troca 1:1 pelo item real.
- Evidência: EditMode tests de swap + peso de loot; cenário humano no lote final.

### CA-2 Pouch of Holding seguro

- +6 slots ao possuir, −6 ao perder; com inventário cheio na perda, recusa drop de forma
  segura (overflow vai para "mochila transbordando" — drop block, nada é destruído).
- Evidência: EditMode tests de ganho/perda/overflow.

### CA-3 Mirror respeita stable-run

- mirror_of_return teleporta à entrada do MESMO nível — sem regerar layout/inimigos/
  recursos (stable-run intacto, regra cave-stable-run).
- Evidência: EditMode test do efeito + replay/snapshot inalterado.

### CA-4 Rotação determinística da Veska

- A unidentified ofertada pela Veska é determinística por semana (seed por semana):
  mesma semana = mesma oferta; semana seguinte = rotação.
- Evidência: EditMode test com seeds de semanas distintas.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Items/
  ItemIdentificationService.cs  (NOVO — IdentifyItem(stack): swap 1:1 + evento)
  ItemPassiveTracker.cs         (NOVO — presença no inventário → flags nomeadas)
Assets/_Game/Scripts/Items/ItemDataSO.cs
  (campos aditivos: isUnidentified, identifiedItemId, passiveFlag)
hooks nomeados nos sistemas existentes (luz/slots/reparo/DayService — pontos auditados)
peso novo no loot resolver (F06) p/ baús raros nível 10+
rotação semanal no estoque da Veska (consumida por F25)
Assets/_Game/Tests/EditMode/Items/MagicItemsTests.cs (NOVO)
docs/validation/fable_31_spec_magic_items_unidentified_runtime_execution_report.md
(Os assets dos 8+N itens são materializados pelo gerador da F32, que esta spec destrava.)
```

## Contratos

### Data contracts

- `ItemDataSO` (aditivo): `isUnidentified` (bool), `identifiedItemId` (string — o par),
  `passiveFlag` (string — nome do hook contínuo). Nenhum campo existente alterado.
- IDs dos pares: `unidentified_trinket_N` ↔ item real (8 mágicos do catálogo).

### Runtime contracts

- `ItemIdentificationService.IdentifyItem(stack)`: valida par, executa swap 1:1 no
  inventário, publica evento; consumido por F25 (Veska) e por scroll_identify.
- `ItemPassiveTracker`: observa o inventário; presença/ausência do item liga/desliga a
  flag nomeada (pendant → HUD HP, lantern → reveal F24, pouch → +6 slots,
  candle → luz não consumível de raio maior). Ponto único por efeito.
- Efeitos de uso (bell/mirror/hourglass/whetstone) registrados no fluxo de consumo F08.

### Event contracts

- `ItemIdentifiedEvent` (NOVO — payload: itemId revelado) — consumido por toast/UI.

### Save contracts

- NO schema novo — itens identificados/não-identificados são stacks normais por ID;
  o par de itens elimina metadata de instância. Cooldown diário do whetstone e rotação
  da Veska derivam de dia/semana correntes (determinísticos), sem campo novo.

### UI contracts

- Tooltip de unidentified: nome vago + "???" no efeito (IMGUI atual e Canvas F14);
  nenhum painel novo.

## Sistemas afetados

```text
Inventory (stacks/slots/tooltip)
Loot de baú da caverna (peso F06)
Uso de consumível (F08)
Luz da caverna / reparo / DayService (hooks nomeados)
HUD (flag do pendant) / reveal de ambush (flag p/ F24)
Shop da Veska (rotação — consumida por F25)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Items/** (service/tracker/ItemDataSO aditivo)
ponto de peso do loot resolver (F06 — arquivo auditado na Fase 0)
pontos de hook nomeados (luz/slots/reparo/DayService — mínimos e auditados)
Assets/_Game/Tests/EditMode/Items/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/**
ProjectSettings/**
SaveManager / seções de save (zero schema novo)
SpellCastService / fluxo de inventário core além dos hooks (locks F07/F08)
Geração procedural da caverna (stable-run — mirror não regenera nada)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar stacks (metadata por instância? premissa: não), tooltip, baús/loot resolver e os
4 pontos de hook (luz/slots/reparo/DayService).

### Fase 1 — Identificação
Par unidentified↔real + ItemIdentificationService (swap 1:1) + ItemIdentifiedEvent +
testes.

### Fase 2 — Efeitos de uso
bell_of_warding / mirror_of_return / hourglass_of_dawn / whetstone_eternal no fluxo F08
+ testes (mirror com stable-run intacto).

### Fase 3 — Passivos
ItemPassiveTracker + 4 flags (pendant/lantern/pouch/candle) + overflow seguro do pouch
+ testes.

### Fase 4 — Fontes e fechamento
Peso em baús raros 10+ (F06) + rotação semanal determinística da Veska; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch_4.
- Can run with: specs que não tocam os arquivos do lock scope.
- Must not run with: F07, F08 (locks de SpellCastService/inventário).
- Shared files/systems that require lock: ItemDataSO (campos aditivos), tooltip/uso de
  item, loot de baú.
- Reason: campos aditivos em ItemDataSO e hooks no fluxo de uso conflitam com specs que
  editam os mesmos arquivos.

## Impacto em save/load

```text
Does this change save schema? NO (itens são stacks normais por ID — o par resolve)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES — ItemIdentifiedEvent (toast/UI)
Changes existing events: NO
Requires unsubscribe pattern: YES (ItemPassiveTracker observa inventário)
```

## Impacto em UI/Unity

```text
Changes UI: tooltip "???" apenas (sem painel novo)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NÃO nesta spec (F32 materializa via gerador)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: pouch shrink com inventário cheio destruir itens do jogador.
Mitigação: overflow vai para "mochila transbordando" (drop block) — recusa segura testada.

Risco: mirror_of_return violar o stable-run regerando conteúdo do nível.
Mitigação: teleporte à entrada do MESMO nível, sem reroll; teste + regra cave-stable-run.

Risco: efeitos contínuos espalharem ifs pelos sistemas.
Mitigação: ItemPassiveTracker com ponto único por efeito e hooks nomeados.

Risco: rotação da Veska não determinística (drift por load).
Mitigação: seed por semana (derivada do calendário), coberta por teste.
```

## Rollback

```text
Itens mágicos deixam de dropar (peso 0 no resolver) e a rotação da Veska desliga —
inventários existentes ficam com itens inertes mas válidos. Remover service/tracker/
hooks desfaz a spec; campos aditivos no ItemDataSO são inofensivos vazios.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar stacks/tooltip/baús + pontos de hook (luz/slots/reparo/DayService).
- [ ] T002 — Par unidentified↔real + IdentifyItem (swap 1:1) + ItemIdentifiedEvent + testes.
- [ ] T003 — 4 efeitos de uso (bell/mirror/hourglass/whetstone) no fluxo F08 + testes.
- [ ] T004 — 4 passivos (ItemPassiveTracker) + overflow seguro do pouch + testes.
- [ ] T005 — Fontes (peso de baú raro 10+ / rotação semanal da Veska); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (swap, efeitos, rotação por seed semanal)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (inventário — stacks/slots intactos sem os itens)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano identificando e
  usando 2 itens

## Definition of Done

```text
Identificação por par de itens funcionando (swap 1:1, tooltip "???"); 8 mágicos com
efeito real (4 uso + 4 passivos com hooks nomeados); fontes determinísticas (baú raro
10+ e rotação semanal da Veska); stable-run e inventário intactos; zero schema novo de
save; builds 0E; execution report criado.
```

## Anti-regressão

```text
Inventário existente intacto (stacks/slots/uso) — testes de regressão.
Stable-run da caverna intacto — mirror nunca regenera layout/inimigos/recursos.
Nenhuma metadata de instância em stacks; nenhum schema novo de save.
Loot determinístico preservado (peso novo não quebra seeds existentes).
Nenhuma referência Unity em dados; nenhum GameObject.Find em runtime.
```
