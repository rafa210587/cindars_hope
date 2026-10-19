# SPEC — Bestiário: Aba Codex Completa (fichas, narração e spoiler tiers)

> **Spec ID:** `fable_45_spec_bestiary_ui_full_codex`
> **Status:** A implementar
> **Wave:** FABLE Batch 9
> **Priority:** P2
> **Type:** UI / Integration
> **Domain:** Bestiary / UI
> **Parallelizable:** NO
> **Parallel group:** N/A (UI canvas lock)
> **Can run with:** specs sem UI/canvas e sem bestiário
> **Must not run with:** F14, F20, F38 (painel único de abas), F21 (provedor de conhecimento), F43 (HUD events)
> **Repo lock scope:** `UI/Runtime/**` (painel de abas F14), `Bestiary/**` (leitura), EnemyDataSO (campo aditivo via gerador)
> **Depends on:**
> - `fable_21_spec_bestiary_knowledge_runtime` (EnemyKnowledgeService + IsVisible/SpoilerTier)
> - `fable_14_spec_ui_canvas_screens_integration_runtime` (painel de 8 abas + RuntimeUiBuilder + UiFocusController)
> - `fable_33_spec_bestiary_data_expansion_60_creatures` (64 fichas geradas + SpoilerTier por criatura)
> **Blocks:** N/A
> **Scope:** a aba Bestiário do painel único vira codex completo — lista por banda/família, silhuetas, ficha por categorias desbloqueadas, narração e completude.
> **Out of scope:** Combat HUD de conhecimento, tooltips de equipamento/spell (F14/F22 cobrem), search, notas do jogador, arte final de retratos.

required_adrs: []
required_game_rules: [ui_modal_rules.md, cave_rules.md]

---

# /speckit.specify

## Contexto

A F21 entrega o conhecimento por descoberta (EnemyKnowledgeService, categorias do
BESTIARY_KNOWLEDGE_DISCOVERY §6, gate `IsVisible(enemyId, categoria, tier)`, SpoilerTier
0-4) e declarou "UI completa = futura; aba F14 lista nome+tier". A F14 (emenda 2026-06-12)
entrega o painel único de 8 abas com a aba Bestiário nascendo como placeholder
(HUD_LAYOUT_SCENES §4). O CAVE_BESTIARY_CATALOG v1.0 tem texto de NARRAÇÃO autoral por
criatura (campo "Narração" das 64 fichas) e fixa SpoilerTiers (comuns 0-1, minibosses 2,
bosses 3, os Quatro = 4). Esta spec fecha o gap: o placeholder vira o codex aprovado.

## Problema

Sem a UI do codex, todo o sistema de descoberta da F21 é invisível: o jogador acumula
conhecimento que nunca vê, a narração autoral das 64 fichas (investimento do catálogo) não
chega ao jogo, e o painel de abas fica com um buraco "em breve" permanente. Pior: qualquer
UI improvisada que mostre dados do EnemyDataSO direto (sem IsVisible) vaza spoiler de
bosses e dos Quatro — violação direta das regras de spoiler do BESTIARY_KNOWLEDGE §20.

## Objetivo

Ao final desta spec, a aba Bestiário do painel único deve listar as 64 entradas agrupadas
por banda/família com silhueta+"???" para não-descobertos, abrir ficha por criatura
mostrando SOMENTE categorias desbloqueadas (F21 IsVisible), exibir o texto de narração do
catálogo quando a identidade estiver descoberta, mostrar contador de completude e ser
totalmente navegável por teclado (UiFocusController F14) — sem nenhum dado acima do
SpoilerTier permitido.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md (§6 categorias; §15 UI; §20 spoiler tiers)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (narração por criatura; PARTE J SpoilerTiers)
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md (§4 painel de 8 abas)
.claude/skills/ui-modal-stack/SKILL.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (estado esperado pós F14/F21/F33 — confirmar na Fase 0):
- painel único de 8 abas (F14) com aba Bestiário placeholder; RuntimeUiBuilder;
  UiFocusController; ModalManager/stack;
- EnemyKnowledgeService no BestiaryManager (F21): contadores, categorias, IsVisible,
  BestiaryKnowledgeUnlockedEvent, save;
- 64 EnemyDataSO/BestiaryEntryId gerados (F33) com SpoilerTier;
- gerador de bestiário (CreateBestiaryEntries* — base para o campo de narração).
Não existe:
- BestiaryScreenView (aba codex); campo de narração nos dados (catálogo é só doc);
  silhuetas; contador de completude; agrupamento por banda/família na UI.
Auditar Fase 0: se F33 já populou algum campo de texto narrativo; shape real da aba
placeholder; como F14 expõe registro de abas; sprites disponíveis para silhueta
(tint preto do sprite existente é aceitável no v1).
```

## Engineering stories

```text
Como jogador, quero folhear o bestiário por família e ver o que já descobri de cada criatura.
Como jogador, quero silhuetas e "???" para o que ainda não vi — descobrir é o jogo.
Como jogador, quero ler a narração de cada criatura quando a identifico — o codex é lore.
Como main quest, quero que o Arquivista e os Quatro fiquem ocultos até o tier permitir.
Como jogador de teclado, quero navegar lista→ficha→voltar sem mouse.
```

## Escopo

```text
Inclui:
- BestiaryScreenView (NOVA — substitui o placeholder da aba 5 do painel F14, mesmo padrão
  Canvas programático/RuntimeUiBuilder): layout lista (esquerda) + ficha (direita);
- lista agrupada por banda da caverna e família (dados do EnemyDataSO/F33), ordenação
  estável; entrada não-descoberta = silhueta (sprite com tint preto) + "???"; descoberta =
  nome + ícone; entradas acima do SpoilerTier permitido NEM aparecem na lista (regra §20);
- ficha por criatura renderizando SOMENTE categorias com IsVisible true (F21): identidade,
  família/habitat/faixa, comportamento, ataques observados, vulnerabilidades/resistências
  descobertas, drops conhecidos, contagem de derrotas; categoria bloqueada = linha "???";
- narração: campo NarrationText ADITIVO no EnemyDataSO populado pelo gerador F33 estendido
  (textos do catálogo, 64 fichas) OU tabela estática BestiaryNarrationTable(enemyId→texto)
  se estender o gerador for inviável na Fase 0 — decisão documentada; exibida apenas com
  identidade descoberta;
- contador de completude no topo: "Vistos X/64 · Documentados Y/64" (documentado = todas as
  categorias core desbloqueadas; definição fixada em constante testada); entradas de tier
  oculto não contam no denominador visível (anti-spoiler — denominador = entradas visíveis);
- filtros mínimos v1 (§15.2 subset): All / Seen / Defeated / por família (dropdown/ciclo);
- navegação por teclado: setas navegam lista, Enter abre ficha, Esc volta/fecha (stack F14),
  Tab muda de aba (contrato F14 preservado); focus order via UiFocusController;
- empty state: "Nenhuma criatura observada ainda." quando nada descoberto;
- atualização reativa: assina BestiaryKnowledgeUnlockedEvent para rebind quando aberta
  (unsubscribe ao fechar);
- EditMode tests: projection da lista (agrupamento/ordenação/ocultação por tier),
  ficha respeita IsVisible por categoria, completude (numerador/denominador), narração só
  com identidade, empty state, filtros determinísticos.
```

## Fora de escopo

```text
Não inclui: search; notas do jogador; "origem do conhecimento/confiança" (§15.2 futuro);
Combat HUD de conhecimento (§15.4 — explicitamente não implementar); tooltips de
equipamento (F14/F22); retratos ilustrados (sprite atual + tint basta); localização.
```

## Regras de não duplicação

```text
Não criar segundo provedor de conhecimento — TODA visibilidade vem de F21 IsVisible.
Não criar segundo painel/canvas — registrar a view na aba existente do painel F14.
Não duplicar dados das fichas — ler EnemyDataSO/registry existentes (F33).
Não recriar builder/focus — RuntimeUiBuilder/UiFocusController da F14.
```

## Critérios de aceite

### CA-1 Lista com descoberta progressiva
- Criatura nunca vista = silhueta+"???"; vista = nome; tier acima do permitido = ausente
  da lista; agrupamento por banda/família estável.
- Evidência: testes de projection + cenário humano.

### CA-2 Ficha gated por categoria
- Ficha mostra apenas categorias IsVisible; matar 5 (threshold F21) faz drops aparecerem
  ao reabrir/rebind; nenhuma categoria bloqueada renderiza conteúdo real.
- Evidência: testes de binding com knowledge sintético.

### CA-3 Narração e completude
- Narração do catálogo aparece só com identidade descoberta; contador X/Y consistente com
  o estado de conhecimento (teste com cenários sintéticos).
- Evidência: testes + inspeção do gerador/tabela (64 textos presentes).

### CA-4 Navegação e contrato de modal
- Teclado navega lista↔ficha; Esc respeita o stack; Tab cicla abas; gameplay input
  bloqueado com painel aberto (contrato F14 intacto).
- Evidência: testes de focus + checklist humano.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Runtime/Screens/
  BestiaryScreenView.cs        (NOVA — aba codex)
  BestiaryCodexProjection.cs   (NOVA — pura: knowledge+data → modelo da lista/ficha)
Assets/_Game/Scripts/Bestiary/BestiaryNarrationTable.cs (ALTERNATIVA — só se gerador inviável)
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs (campo NarrationText aditivo — se via gerador)
Assets/_Game/Scripts/Editor/Bestiary/<gerador F33> (popular narração)
Assets/_Game/Tests/EditMode/UI/BestiaryCodexTests.cs
docs/validation/fable_45_spec_bestiary_ui_full_codex_execution_report.md
```

## Contratos

### Data contracts
`NarrationText` (string) aditivo no EnemyDataSO via gerador, OU tabela estática
enemyId→texto. Projection model: listas/strings simples (testável sem Unity).
### Runtime contracts
`BestiaryCodexProjection.Build(knowledgeService, entries, filtro)` pura e determinística;
view só renderiza o modelo.
### Event contracts
N/A novos — assina `BestiaryKnowledgeUnlockedEvent` (F21) com unsubscribe ao fechar.
### Save contracts
N/A — UI não persiste estado (regra canônica); conhecimento já persiste na F21.
### UI contracts
View registrada na aba 5 do painel F14: `Open()/Close()/Rebind()`; Esc/Tab/atalho direto
seguem o contrato do painel; empty state explícito.

## Sistemas afetados

```text
UI painel de abas (F14), Bestiary knowledge (leitura F21), EnemyDataSO/gerador (F33),
Event bus (assinatura), testes EditMode.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Runtime/Screens/{BestiaryScreenView,BestiaryCodexProjection}.cs
Assets/_Game/Scripts/UI/Runtime/<registro da aba — ponto único do painel F14>
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs (campo aditivo) OU Assets/_Game/Scripts/Bestiary/BestiaryNarrationTable.cs
Assets/_Game/Scripts/Editor/Bestiary/** (gerador de narração)
Assets/_Game/Tests/EditMode/UI/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab manuais ; *.asset manual (regenerar via gerador/AssetDatabase apenas)
Packages/** ; ProjectSettings/**
EnemyKnowledgeService/BestiaryManager (F21 — consumir, não alterar regras)
ModalManager ; DamageCalculator ; geradores de cena
```

## Estratégia de implementação

```md
### Fase 0 — Auditar aba placeholder/registro de abas F14, shape F21/F33, decidir narração (gerador × tabela).
### Fase 1 — BestiaryCodexProjection pura + testes (agrupamento, tiers, completude, filtros).
### Fase 2 — Narração: estender gerador F33 (64 textos do catálogo) ou tabela; regenerar assets com evidência.
### Fase 3 — BestiaryScreenView (lista+ficha+silhueta+empty state) registrada na aba; focus/Esc/Tab.
### Fase 4 — Rebind por evento + testes de binding; run_strict_validation; report + cenário humano.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs sem UI e sem bestiário
- Must not run with: F14, F20, F38, F21, F43
- Shared files/systems that require lock: painel de abas F14, EnemyDataSO/gerador
- Reason: edita o registro de abas compartilhado e dados de inimigo.

## Impacto em save/load

```text
Does this change save schema? NO. Does this add a save section? NO.
Does this require migration? NO. Does this persist Unity references? N/A (sem save).
```

## Impacto em eventos

```text
Adds events: NO | Changes existing events: NO | Requires unsubscribe pattern: YES (view assina F21)
```

## Impacto em UI/Unity

```text
Changes UI: YES (núcleo) | Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: YES — via gerador (narração nos EnemyDataSO), com
evidência de geração (regra generated-asset-evidence)
Requires Play Mode final validation: YES (obrigatório para UI)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: vazamento de spoiler (ficha/lista mostrando dado bloqueado). Mitigação: projection
pura é o ÚNICO caminho de dados para a view + testes por tier/categoria.
Risco: 64 entradas degradarem o scroll/build da lista. Mitigação: construção sob demanda
da ficha (lista leve), padrão de container scrollável do builder F14.
Risco: gerador de narração dessincronizar do catálogo (typos/IDs). Mitigação: tabela
id→texto única no gerador + validador F30 (consistência de IDs) na validação.
Risco: F21/F33 divergirem do esperado na Fase 0. Mitigação: stop-and-report se contratos
IsVisible/BestiaryEntryId não existirem como especificado.
```

## Rollback

```text
Desregistrar a view devolve o placeholder "em breve" da aba; campo NarrationText aditivo é
inerte sem a view; tabela/gerador removíveis. Sem impacto em save/gameplay.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar aba F14, F21 IsVisible, F33 entries; decidir gerador×tabela p/ narração.
- [ ] T002 — BestiaryCodexProjection pura + testes (tiers, agrupamento, completude, filtros).
- [ ] T003 — Narração: 64 textos do catálogo no gerador/tabela + evidência de geração.
- [ ] T004 — BestiaryScreenView (lista+silhueta+ficha+empty state) + registro na aba + focus.
- [ ] T005 — Rebind por BestiaryKnowledgeUnlockedEvent + testes de binding.
- [ ] T006 — csproj; run_strict_validation; execution report + cenário humano.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (projection, completude, filtros)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (obrigatório para UI)
- Requires regression test: YES (contrato do painel F14 — Tab/Esc/bloqueio de input intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes de projection/binding + checklist humano
  (descobrir criatura → ver lista/ficha mudarem; boss tier oculto até flag)

## Definition of Done

```text
Aba codex funcional com 64 entradas gated por descoberta e spoiler tier; narração visível
pós-identificação; completude correta; navegação por teclado; placeholder substituído;
builds 0E; evidência de geração de assets; report; sem claim ACCEPTED.
```

## Anti-regressão

```text
Nada renderiza acima do SpoilerTier permitido (os Quatro = tier 4). Painel F14: Tab cicla,
Esc fecha topo, gameplay bloqueado. F21 não alterado (consumo apenas). Shop/demais abas
intactas. Nenhum GameObject.Find em runtime.
```

## Notas para execução posterior

```text
Search/filtros completos (§15.2), notas do jogador e Combat HUD de conhecimento (§15.4)
ficam para spec futura. Retratos ilustrados = fase de arte.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. (6.7-B/A): o codex exibe o bônus mecânico de FullyDocumented (+stats contra a criatura)
   e fichas de boss aparecem após a derrota.
```
