# SPEC — Fazenda: Lotes Compráveis de Expansão (3 lotes + escritura)

> **Spec ID:** `fable_41_spec_farm_lot_expansions`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P3
> **Type:** Runtime / Editor / Scene
> **Domain:** Farm / Economy
> **Parallelizable:** NO (gerador de cena lock — após F40)
> **Parallel group:** N/A (lock dos geradores de cena)
> **Can run with:** N/A
> **Must not run with:** F09, F11, F40 (geradores)
> **Repo lock scope:** `CreateMvpFarmScene.cs`, FarmLot service (novo), save (campos aditivos)
> **Depends on:**
> - F40 (padrão de distritos data-driven), F32 (item escritura), decisão Q12.2
> **Blocks:** N/A
> **Scope:** fazenda com 3 lotes expansíveis comprados na prefeitura.
> **Out of scope:** construções novas nos lotes (F12 abrigos usam zona atual), terraform.

required_adrs: []
required_game_rules: [farm_rules.md, economy_rules.md]

---

# /speckit.specify

## Contexto

Decisão Q12.2: a fazenda mantém a área inicial e cresce por COMPRA DE LOTES.
HUD_LAYOUT_SCENES §4 define a estrutura: área inicial + 3 lotes de expansão —
Norte = plantio extra (12 plots), Leste = pasto para o 2º abrigo, Oeste = pomar
(árvores frutíferas). FARM_LAYOUT_SCALE_BUILDINGS confirma o padrão de áreas de
expansão bloqueadas dentro da mesma cena (zonas cercadas que abrem na compra).

Objetivo: os lotes nascem no gerador já cercados e com placa ("Lote à venda"), com o
conteúdo interno GERADO mas DESATIVADO; a escritura (deed_lot_n — itens F32) é vendida
na prefeitura/mural F34 ou na Veska (definir na Fase 0 pelo que existir após F40); usar
a escritura destrava o lote (remove a cerca, ativa os interactables internos — via
referências serializadas pelo gerador, sem Find) e o estado persiste em campos aditivos
da seção farm do save.

## Problema

Sem os lotes, a decisão Q12.2 fica sem materialização: a fazenda não tem progressão
espacial nem sink de ouro de médio prazo (as escrituras de 2500-4000g são um dos
principais sumidouros de economia do v1). Se o destravamento for implementado com
GameObject.Find para achar cercas/conteúdo, viola a regra de runtime global search; se o
estado não persistir, o jogador perde lotes comprados ao recarregar; se um save legado
quebrar ao não ter a seção, a fazenda atual regride.

## Objetivo

Ao final desta spec, o projeto deve ter 3 lotes cercados gerados na fazenda
(norte/leste/oeste), escrituras compráveis (deed_lot_north 2500g / deed_lot_east 4000g /
deed_lot_west 3500g — 1 cada), destravamento idempotente via uso da escritura (referências
serializadas, sem Find), pomar sazonal no lote oeste e persistência por campos aditivos
na seção farm — com save legado carregando tudo travado e sem erro.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md (§4)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§12)
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CreateMvpFarmScene v2 (24 plots, zonas, entrada da caverna) — ATUALIZAR para gerar
  os lotes, nunca criar segundo gerador;
- FarmPlot (plots do lote norte reusam o mesmo componente);
- InventoryManager (consumo da escritura) e EconomyManager (compra);
- save farm (seção existente — campos aditivos entram nela);
- padrão use-item do AnimalReleaseHandler (F12) — modelo do UseDeed;
- padrão de resource node (modelo das árvores do pomar).
Não existe:
- lotes/cercas/placas, escrituras, FarmLotService, pomar sazonal.
Auditar Fase 0:
- onde vender a escritura: prefeitura (F40 executada? prédio existe?) — senão Veska;
- padrão real de use-item (AnimalReleaseHandler) e de resource node sazonal.
```

## Engineering stories

```text
Como jogador, quero comprar a escritura e destravar o lote NA HORA, para sentir a
  progressão espacial da fazenda.
Como gerador de cena, quero gerar o conteúdo dos lotes desativado com referências
  serializadas, para o destravamento não usar nenhum Find em runtime.
Como sistema de save, quero o estado dos lotes em campos aditivos da seção farm, para
  saves legados carregarem tudo travado sem erro.
Como economia, quero as escrituras como sink de 2500/4000/3500g, para o ouro de
  médio prazo ter destino.
```

## Escopo

```text
Inclui:
- gerador: 3 lotes cercados (norte/leste/oeste do layout §4) com placa interactable
  ("Lote à venda — escritura na prefeitura"); conteúdo interno gerado mas DESATIVADO
  (plots/área de abrigo/4 árvores frutíferas);
- FarmLotService (bootstrap): estado por lote {Locked, Owned}; UseDeed(lotId) → destrava
  (ativa conteúdo, desativa cerca — referências serializadas do gerador, sem Find);
- escrituras (F32 itens): deed_lot_north 2500g / deed_lot_east 4000g / deed_lot_west 3500g
  — venda na prefeitura (ou Veska), 1 cada;
- save: campos aditivos na seção farm (ownedLots — lista de IDs);
- pomar (lote oeste): 4 árvores com colheita sazonal simples (interactable, fruta por
  estação — reusa padrão de resource node);
- EditMode tests: destravar idempotente, save round-trip, conteúdo inativo até compra,
  preço/consumo da escritura.
```

## Fora de escopo

```text
Não inclui:
- construções novas nos lotes (F12 — abrigos usam a zona atual; o lote leste só abre o
  PASTO para o 2º abrigo futuro);
- terraform/edição de terreno;
- arte final de cercas/placas (placeholder);
- mais lotes além dos 3 do layout §4;
- balance final dos preços (2500/4000/3500g é a régua aprovada).
```

## Regras de não duplicação

```text
Não criar segundo sistema de compra — o shop existente vende a escritura.
Não criar segundo padrão de use-item — seguir o AnimalReleaseHandler (F12).
Não criar segundo gerador de fazenda — atualizar o CreateMvpFarmScene existente.
Não criar seção de save nova — campos ADITIVOS na seção farm existente.
Não criar segundo padrão de colheita — pomar reusa o padrão de resource node.
Não usar Find/FindObjectOfType — referências serializadas pelo gerador.
```

## Critérios de aceite

### CA-1 — Lotes travados no início

- Em jogo novo, os 3 lotes nascem cercados com placa informativa e TODO o conteúdo
  interno inativo (plots/pasto/árvores não interagíveis).
- Evidência: EditMode test de estado inicial + inspeção no cenário humano.

### CA-2 — Compra + destravamento persistente

- Comprar a escritura (preço correto, 1 cada) e usá-la destrava o lote na hora (cerca
  some, conteúdo ativa) e o estado persiste após save/load (round-trip de ownedLots).
- Evidência: EditMode tests de preço/consumo, destravamento idempotente e round-trip.

### CA-3 — Pomar sazonal

- Após destravar o lote oeste, as 4 árvores frutíferas são colhíveis com fruta por
  estação (padrão de resource node).
- Evidência: EditMode test da regra sazonal + cenário humano colhendo.

### CA-4 — Save legado seguro

- Save legado (sem os campos novos) carrega com todos os lotes travados, sem erro e sem
  afetar a fazenda atual.
- Evidência: EditMode test de seção legada (campos ausentes → default Locked).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/
  FarmLotService.cs                 (NOVO — bootstrap; estado {Locked, Owned}; UseDeed)
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateMvpFarmScene.cs             (ATUALIZADO — 3 lotes cercados + conteúdo inativo +
                                     referências serializadas)
itens deed_lot_* via pipeline de itens F32 (gerador de itens — evidência)
Assets/_Game/Tests/EditMode/Farm/
  FarmLotsTests.cs                  (NOVO)
docs/validation/
  fable_41_spec_farm_lot_expansions_execution_report.md
```

## Contratos

### Data contracts
3 lotes com IDs estáveis (lot_north, lot_east, lot_west) conforme layout §4
(Norte = 12 plots extras, Leste = pasto do 2º abrigo, Oeste = pomar 4 árvores);
itens deed_lot_north (2500g) / deed_lot_east (4000g) / deed_lot_west (3500g) no
catálogo F32 — estoque 1 cada no ponto de venda.

### Runtime contracts
FarmLotService (bootstrap) é o dono do estado por lote {Locked, Owned};
UseDeed(lotId) é idempotente (usar 2ª vez não duplica efeito) e opera SOMENTE via
referências serializadas pelo gerador (cerca, conteúdo, placa) — zero Find em runtime.
Placa é interactable informativo enquanto Locked. Ponto de venda definido na Fase 0
(prefeitura F40 se existir; senão Veska).

### Event contracts
FarmLotUnlockedEvent (novo — publicado no destravamento; consumido por toast/feedback).

### Save contracts
Campos ADITIVOS na seção farm existente: ownedLots (lista de IDs string). Sem seção
nova, sem migração: campos ausentes em save legado = default tudo Locked (CA-4).
Nenhuma referência Unity persistida (apenas IDs).

### UI contracts
Placa do lote (interactable informativo) + confirmação de uso da escritura (fluxo
use-item existente) + toast de destravamento via feedback existente.

## Sistemas afetados

```text
Gerador da fazenda (CreateMvpFarmScene — lotes cercados + refs serializadas)
Farm runtime (FarmLotService novo; FarmPlot reusado nos plots do lote norte)
Economia/shop (venda das escrituras — prefeitura ou Veska)
Inventário (consumo da escritura — padrão use-item F12)
Save (campos aditivos na seção farm)
Feedback/toast (FarmLotUnlockedEvent)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/** (novo: FarmLotService — aditivo)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (atualizar)
pipeline de itens F32 (escrituras deed_lot_* — via gerador, com evidência)
Assets/_Game/Tests/EditMode/Farm/** ; docs/validation/** ; csproj includes
campos aditivos na seção farm do save (somente aditivos)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (geração só via gerador/AssetDatabase)
Packages/**
ProjectSettings/**
CreateMvpTownScene (F40 é a dona)
SaveManager core / outras seções de save (somente campos aditivos na seção farm)
sistema de construções F12 (consumir padrão use-item, não alterar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Ponto de venda (prefeitura F40 existe? senão Veska); padrão use-item do
AnimalReleaseHandler; padrão de resource node sazonal; seção farm do save.
### Fase 1 — Gerador
3 lotes cercados (norte/leste/oeste §4) + placa + conteúdo interno gerado DESATIVADO
(12 plots N, pasto E, 4 árvores W) + referências serializadas para o FarmLotService.
### Fase 2 — Serviço e persistência
FarmLotService {Locked, Owned} + UseDeed idempotente + FarmLotUnlockedEvent + campos
aditivos (ownedLots) na seção farm + restauração no load.
### Fase 3 — Escrituras e pomar
Itens deed_lot_* (2500/4000/3500g, 1 cada) à venda no ponto definido; pomar sazonal
(4 árvores, fruta por estação — padrão resource node).
### Fase 4 — Testes e closeout
FarmLotsTests (idempotência, round-trip, conteúdo inativo, preço/consumo, save legado);
regeneração com evidência; csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F09, F11, F40 (geradores de cena — esta roda APÓS F40)
- Shared files/systems that require lock: `CreateMvpFarmScene.cs`, FarmLot service
  (novo), save (campos aditivos)
- Reason: edita gerador de cena compartilhado e a seção farm do save; depende do padrão
  de distritos data-driven e da prefeitura criados pela F40.

## Impacto em save/load

```text
Does this change save schema? YES — campos ADITIVOS na seção farm (ownedLots: lista de IDs)
Does this add a save section? NO (seção farm existente)
Does this require migration? NO (campos ausentes = default Locked — save legado seguro)
Does this persist Unity references? NO (apenas IDs string)
Round-trip obrigatório: comprar/destravar → save → load → lote continua Owned.
```

## Impacto em eventos

```text
Adds events: YES — FarmLotUnlockedEvent (toast de destravamento)
Changes existing events: NO
Requires unsubscribe pattern: YES (consumidores do FarmLotUnlockedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: placa informativa + confirmação de uso (fluxos existentes)
Changes scenes: YES — FarmScene regenerada VIA GERADOR (evidência obrigatória)
Changes prefabs: NO
Changes ScriptableObjects/assets: itens deed via pipeline F32 (evidência)
Requires Play Mode final validation: YES (lote)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: destravamento via Find em runtime (violação de regra).
Mitigação: referências cerca/conteúdo/placa serializadas pelo gerador + revisão.
Risco: UseDeed duplicar efeito (re-uso, reload).
Mitigação: idempotência testada (2º uso = no-op; reload mantém Owned 1×).
Risco: save legado quebrar sem os campos novos.
Mitigação: campos aditivos com default Locked + teste de seção legada (CA-4).
Risco: ponto de venda indefinido (prefeitura pode não existir se F40 atrasar).
Mitigação: decisão na Fase 0 — prefeitura se existir, senão Veska (ambos aprovados).
Risco: pomar criar segundo padrão de colheita.
Mitigação: reusar padrão de resource node existente (auditado na Fase 0).
```

## Rollback

```text
Lotes não geram (gerador revertido) = fazenda atual intacta.
FarmLotService removível; campos aditivos ignorados por loads antigos.
Não apagar save real do usuário.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar ponto de venda (prefeitura/Veska) + padrão use-item + resource node.
- [ ] T002 — Gerador: 3 lotes cercados + placas + conteúdo inativo + refs serializadas.
- [ ] T003 — FarmLotService + UseDeed idempotente + FarmLotUnlockedEvent + save aditivo
        (ownedLots) + testes.
- [ ] T004 — Pomar sazonal (4 árvores); escrituras (2500/4000/3500g, 1 cada) à venda;
        evidência de geração; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (estado de lote, save round-trip, regra sazonal)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (fazenda atual — área inicial intacta)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano comprando e
  destravando 1 lote

## Definition of Done

```text
3 lotes compráveis (norte/leste/oeste) com conteúdo inativo até a compra; escrituras
2500/4000/3500g (1 cada) à venda; destravamento idempotente sem Find (refs
serializadas); persistência por campos aditivos (round-trip); pomar sazonal; save
legado carrega travado sem erro.
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
Fazenda inicial (24 plots, zonas, entrada da caverna) intacta.
Zero GameObject.Find/FindObjectOfType em runtime (refs serializadas).
Save legado compatível (campos ausentes = tudo Locked, sem erro).
Nenhuma referência Unity persistida no save (apenas IDs).
Shop/inventário sem mudança de contrato (escritura é item comum do fluxo existente).
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. Fazenda nível máximo (5.3): gated APENAS por dinheiro e recursos — NENHUM gate de
   caverna/fragmento. Remover qualquer menção a marco de caverna do escopo.
```
