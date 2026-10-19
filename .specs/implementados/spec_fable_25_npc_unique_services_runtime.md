# SPEC — NPCs: Serviços Únicos por Personagem (Análise, Banho, Encomenda, Caça...)

> **Spec ID:** `fable_25_spec_npc_unique_services_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P2
> **Type:** Runtime / Integration
> **Domain:** NPC / Economy
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_7
> **Can run with:** specs que não toquem NpcShopController/diálogo (locks abaixo)
> **Must not run with:** F28, F26 (NpcShopController/diálogo locks)
> **Repo lock scope:** NpcShopController, TownNpcDialogueLibrary, serviços novos
> **Depends on:**
> - F21 (GrantKnowledge p/ Thalindra)
> - F26 (gates de amizade)
> - F16 (fadiga p/ banho)
> **Blocks:** N/A
> **Scope:** 8 serviços únicos dos NPCs do roster (CITY_NPC_ROSTER v1.1 §serviços).
> **Out of scope:** romance (sistema próprio futuro), serviços que exijam sistemas inexistentes (transporte rápido → dormante).

required_adrs: []
required_game_rules: [npc_rules.md, economy_rules.md]

---

# /speckit.specify

## Contexto

O roster canônico (`CITY_NPC_ROSTER_SERVICES_DIRECTION` v1.1) define serviço único por
NPC, ancorado no propósito/lore de cada personagem (Thalindra pesquisadora de arquivo,
Brumdar ferreiro, Yael comerciante noturna de livros/segredos, Eiran tratador de
animais...). A decisão humana Q10.x aprovou TODAS as 3 vias de recompensa de
relacionamento juntas: diálogos diferentes + bônus + serviços especiais. Hoje os NPCs
só têm shop/diálogo genérico (NpcShopController + TownNpcDialogueLibrary) — nenhum
serviço único existe.

Esta spec adiciona a camada de serviços como OPÇÕES no fluxo Conversar existente
(árvore de diálogo), cada serviço com gate de amizade (F26) e/ou flag de quest, custo
e efeito mecânico real nos sistemas já implementados (F21 conhecimento, F16 fadiga,
F12 animais, F31 identificação...). O princípio de UX é descoberta > ocultação: opção
gated aparece DESABILITADA com o motivo visível, nunca some.

## Problema

Sem serviços únicos, a amizade (F26) não tem payoff mecânico — os níveis sobem sem
destravar nada — e os NPCs continuam intercambiáveis (só shop/diálogo), contra a
direção do roster. Sistemas já prontos ficam sem consumidor: GrantKnowledge (F21) não
tem via de jogo além do grind, a fadiga (F16) não tem remoção paga, itens
unidentified_* (F31) não têm identificador. Implementar fora da árvore Conversar
criaria um segundo fluxo de diálogo paralelo — duplicação proibida.

## Objetivo

Ao final desta spec, o projeto deve ter um registro/executor de serviços de NPC
(NpcServiceDefinition: id, npcId, gates {friendshipMin, questFlag}, custo, efeito) com
os 8 serviços do escopo acessíveis no Conversar dos NPCs corretos, gates funcionais
(opção desabilitada com motivo), efeitos reais integrados aos sistemas existentes e
pendências persistidas (encomenda, contrato semanal, prato do dia) — sem criar segundo
fluxo de diálogo e sem tocar romance.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/npcs_city/CITY_NPC_ROSTER_DIRECTION (v1.1 — seção por NPC)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§10)
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (preços)
.claude/rules/testing-quality-gate.md
.claude/skills/npc-dialogue-authoring/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- NpcShopController com árvore Conversar (TownNpcDialogueLibrary);
- QuestFlagService (gates de flag);
- InventoryManager/EconomyManager (custos/consumo de itens);
- FriendshipService (F26 — gates de amizade);
- FatigueSystem (F16 — alvo do banho);
- EnemyKnowledgeService (F21 — alvo da análise);
- DayStartedEvent (calendário — encomendas/semana/prato do dia).
Não existe:
- nenhum serviço único; registry/executor; persistência de pendências.
Auditar Fase 0:
- como a árvore Conversar registra opções condicionais (ponto de extensão);
- mapear cada serviço ao npcId canônico do registry/roster (tabela-mestra PARTE C);
  nomes de NPC citados no escopo que divirjam do roster v1.1 devem ser resolvidos
  contra o roster na auditoria, sem alterar o efeito do serviço;
- pontos de hook existentes: custo de reparo (Brumdar), alimentação de animais (F12),
  remoção de fadiga (F16), identificação (F31).
```

## Engineering stories

```text
Como jogador, quero que cada NPC ofereça um serviço único ligado ao seu ofício,
  para que a cidade tenha personalidade mecânica além das lojas.
Como FriendshipService (F26), quero que serviços usem IsAtLeast(npcId, level) como
  gate, dando payoff concreto aos níveis de amizade.
Como jogador, quero ver a opção gated DESABILITADA com o requisito ("Amizade 3
  necessária"), para descobrir o que existe sem spoiler de como tudo funciona.
Como save, quero pendências (encomenda da Yael, contrato semanal, prato do dia usado)
  persistidas em seção pequena e aditiva.
```

## Escopo

```text
Inclui:
8 serviços (NpcServiceDefinition: id, npcId, gates {friendshipMin, questFlag}, custo, efeito):
1. Thalindra — Análise de Criatura: 80g + 1 parte do monstro → GrantKnowledge(categoria
   faltante mais valiosa) (F21);
2. Brumdar — Reparo com Desconto: amizade 3+ → reparo −30% (hook no custo existente);
3. Yael — Encomenda de Livro: 200g → após 3 dias chega livro de conhecimento (banda à
   escolha; consome DayStartedEvent);
4. Sereth — Banho Termal: 50g → remove Fatigue acumulada + buff Rested (+10% stamina regen
   até dormir) (F16/F01);
5. Eiran — Pasto Premium: amizade 4+ → animais alimentados automaticamente por 3 dias (F12);
6. Kael — Contrato de Caça Pessoal: semanal, alvo elite da banda atual, recompensa 2×
   (registra quest dinâmica via QuestRegistry — padrão board F34 se existir, senão flag);
7. Mirena — Prato do Dia: 1×/dia grátis com amizade 2+ → comida com buff aleatório do dia;
8. Veska — Identificação de Relíquia: itens unidentified_* (F31) → revela por 120g.
- toda opção aparece DESABILITADA com motivo quando gated (descoberta > ocultação);
- EditMode tests: cada gate, cada efeito (com serviços mockados), encomenda 3 dias.
```

## Fora de escopo

```text
Não inclui:
- romance (sistema próprio futuro);
- serviços que exijam sistemas inexistentes (transporte rápido → dormante documentado);
- UI nova além das opções de diálogo no fluxo Conversar;
- balance fino de preços (valores do escopo/ITEM_CATALOG são canônicos nesta spec);
- serviços além dos 8 listados (roster completo = specs futuras).
```

## Regras de não duplicação

```text
Não criar segundo fluxo de diálogo — opções entram na árvore Conversar existente.
Não duplicar gates — amizade via FriendshipService (F26), flags via QuestFlagService.
Não duplicar efeitos — executor chama os sistemas existentes (F21/F16/F12/F31) por
  interface; nenhum efeito reimplementado localmente.
Serviços de NPCs sem sistema-alvo ficam dormentes documentados.
```

## Critérios de aceite

### CA-1 8 serviços acessíveis com gates

- Os 8 serviços aparecem no Conversar dos NPCs corretos; gates de amizade/flag
  funcionais (fechado antes, aberto depois).
- Evidência: EditMode tests por gate + cenário humano do lote.

### CA-2 Integração com F21 (análise)

- Análise da Thalindra consome 80g + 1 parte do monstro e chama GrantKnowledge com a
  categoria faltante mais valiosa; idempotência herdada da F21.
- Evidência: EditMode test de consumo + concessão (serviço com F21 mockado/real).

### CA-3 Pendências temporais

- Encomenda da Yael chega no dia 3 (consumo de DayStartedEvent sintético); contrato
  do Kael renova semanalmente; prato do dia limita 1×/dia.
- Evidência: EditMode tests com eventos de dia sintéticos.

### CA-4 Descoberta > ocultação

- Opção gated mostra o requisito ("Amizade 3 necessária"), não some da lista.
- Evidência: teste do estado desabilitado+motivo no modelo de opções.

### CA-5 Persistência de pendências

- Encomendas pendentes, contrato semanal e prato do dia usado sobrevivem a save/load;
  saves antigos carregam sem pendências.
- Evidência: EditMode tests de round-trip e load legado da seção.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Services/
  NpcServiceDefinition.cs        (NOVO — id, npcId, gates, custo, efeito)
  NpcServiceRegistry.cs          (NOVO — registro dos 8 serviços)
  NpcServiceExecutor.cs          (NOVO — valida gates/custos e despacha efeitos)
TownNpcDialogueLibrary           (opções condicionais no Conversar)
Hooks pontuais: custo de reparo (Brumdar), pasto (F12), banho (F16)
Assets/_Game/Tests/EditMode/City/
  NpcServicesTests.cs            (NOVO)
docs/validation/
  fable_25_spec_npc_unique_services_runtime_execution_report.md
```

## Contratos

### Data contracts

`NpcServiceDefinition` { serviceId:string, npcId:string, gates {friendshipMin:int,
questFlag:string}, custo {gold:int, itens}, efeito tipado }. IDs de NPC = registry
canônico (tabela-mestra do roster). Preços do escopo (80g/200g/50g/120g...) são
canônicos.

### Runtime contracts

`NpcServiceExecutor.TryExecute(serviceId)`: valida gates (FriendshipService.IsAtLeast +
QuestFlagService), valida/consome custos (InventoryManager/EconomyManager), despacha o
efeito pela interface do sistema-alvo (F21/F16/F12/F31, hooks de reparo/pasto).
Acoplamento controlado: executor chama serviços por interface (mockável em teste).

### Event contracts

Consome `DayStartedEvent` (encomenda 3 dias, semana do contrato, reset do prato do
dia). Nenhum evento novo; nenhum evento existente muda.

### Save contracts

Seção pequena aditiva `NpcServicesSaveData` { encomendas pendentes (npcId, banda,
diaEntrega), contrato semanal (alvo, semana, concluído), prato do dia usado (dia) } —
padrão WI-18, default vazio, tipos simples, sem migration.

### UI contracts

Opções de serviço na árvore Conversar existente: habilitada/desabilitada com motivo
("Amizade 3 necessária"). Confirmação de custo usa o padrão de diálogo existente.
Nenhuma tela nova.

## Sistemas afetados

```text
NPC/diálogo (opções no Conversar)
Amizade (consumo de gates F26)
Bestiário (GrantKnowledge F21), Fadiga (F16), Animais (F12), Identificação (F31)
Economia/Inventário (custos)
Save/load (seção pequena aditiva)
Calendário (consumo de DayStartedEvent)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/Services/**
TownNpcDialogueLibrary (registro de opções)
Hooks pontuais: custo de reparo (Brumdar), pasto (F12), banho (F16)
SaveManager wiring aditivo da seção NpcServicesSaveData
Assets/_Game/Tests/EditMode/City/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida)
Packages/**
ProjectSettings/**
FriendshipService/QuestFlagService core (consumir APIs, não alterar)
Sistemas-alvo (F21/F16/F12/F31) além do hook/interface pontual documentado
Fluxo de shop existente (compra/venda intactos)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar árvore Conversar (opções condicionais), mapear serviços→npcId canônico do
roster, e pontos de hook (reparo/animais/fadiga/identificação).

### Fase 1 — Núcleo
NpcServiceDefinition/Registry/Executor + gates (amizade/flag) + opção desabilitada
com motivo + consumo de custos. Testes com sistemas mockados.

### Fase 2 — Serviços 1-4
Análise (F21), Reparo com desconto (hook), Encomenda de livro (DayStartedEvent),
Banho termal (F16/F01) + testes.

### Fase 3 — Serviços 5-8
Pasto premium (F12), Contrato de caça (QuestRegistry/board F34 ou flag), Prato do dia
(1×/dia), Identificação (F31) + testes.

### Fase 4 — Persistência e fechamento
NpcServicesSaveData (round-trip/legado); csproj; run_strict_validation; report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: fable_batch_7
- Can run with: specs que não toquem NpcShopController/TownNpcDialogueLibrary
- Must not run with: F28, F26 (NpcShopController/diálogo locks; F26 é dependência —
  executar antes)
- Shared files/systems that require lock: NpcShopController, TownNpcDialogueLibrary,
  serviços novos
- Reason: F26/F28 editam o mesmo fluxo de diálogo/pools; conflito direto de arquivo.

## Impacto em save/load

```text
Does this change save schema? YES — seção pequena aditiva (NpcServicesSaveData)
Does this add a save section? YES (pendências: encomendas, contrato semanal, prato do dia)
Does this require migration? NO (default vazio)
Does this persist Unity references? NO (IDs/dias/strings simples)
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (consumo de DayStartedEvent pelo executor/registry)
```

## Impacto em UI/Unity

```text
Changes UI: YES — opções de diálogo no fluxo Conversar existente (sem tela nova)
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: acoplamento do executor a 6+ sistemas-alvo.
Mitigação: executor chama serviços por interface (mockável); 1 hook pontual por efeito.
Risco: nomes de NPC do escopo divergirem do roster v1.1.
Mitigação: Fase 0 mapeia serviço→npcId canônico pela tabela-mestra antes de implementar.
Risco: exploits temporais (prato do dia/contrato resetando por save-load).
Mitigação: persistir dia/semana absolutos do calendário na seção (testes CA-3/CA-5).
Risco: contrato de caça sem board F34 presente.
Mitigação: caminho declarado no escopo — usar flag se o padrão board não existir.
```

## Rollback

```text
Remover o registro das opções no Conversar desliga os serviços; registry/executor
ficam inertes. Seção de save desconhecida é ignorada com segurança em load.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar árvore Conversar + mapear serviços→npcId canônico + pontos de
        hook (reparo/animais/fadiga/identificação).
- [ ] T002 — Registry/executor + gates (amizade/flag) + opção desabilitada com motivo
        + custos (testes mockados).
- [ ] T003 — Serviços 1-4 (análise/reparo/livro/banho) + testes.
- [ ] T004 — Serviços 5-8 (pasto/caça/prato/identificação) + testes.
- [ ] T005 — Save de pendências (round-trip/legado); csproj; run_strict_validation;
        report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gates, custos, pendências temporais)
- Requires EditMode tests: YES (cada gate, cada efeito com serviços mockados,
  encomenda 3 dias, round-trip/legado)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (shop/diálogo atuais intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano usando 3 serviços
  (1 gated)

## Definition of Done

```text
8 serviços funcionais com gates (amizade/flag) e custos reais; opção gated visível
com motivo; pendências persistidas (encomenda/contrato/prato); árvore Conversar única;
efeitos via interfaces dos sistemas existentes.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Shop e diálogo genérico atuais intactos (compra/venda/conversar sem serviços).
Nenhum segundo fluxo de diálogo criado.
Gates sempre via FriendshipService/QuestFlagService (sem cópia local de regra).
Sem referências Unity na seção de save; saves antigos carregam.
Opções gated nunca somem (descoberta > ocultação).
Comunicação de gameplay só via GameEventBus; sem GameObject.Find em runtime.
```
