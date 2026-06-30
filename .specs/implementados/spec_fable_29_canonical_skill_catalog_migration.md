# SPEC — Skills: Migração para o Catálogo Canônico (~70 nós, 5 árvores, tiers/ranks)

> **Spec ID:** `fable_29_spec_canonical_skill_catalog_migration`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P1
> **Type:** Runtime / Data / Migration
> **Domain:** Skills
> **Parallelizable:** NO — SOLO (toca skill tree + executores + save de skills)
> **Parallel group:** N/A (execução solo)
> **Can run with:** N/A
> **Must not run with:** QUALQUER outra spec
> **Repo lock scope:** `Skills/**`, SkillTreeManager, geradores de skill assets, save de skills
> **Depends on:**
> - F02 (provider de stats derivados)
> - F27 (perfect block hook)
> - F01 (status effects)
> - F42 (cap 100 / economia de pontos)
> **Blocks:**
> - F39 (classe inferida lê investimento por árvore)
> **Scope:** os ~70 nós canônicos como assets + efeitos passivos roteados + migração de save.
> **Out of scope:** REMODELAR o sistema (CANCELADO — decisão v1.1 Parte D); skills ativas novas além das canônicas com executor existente; VFX.

required_adrs: []
required_game_rules: [player_rules.md]

---

# /speckit.specify

## Contexto

`PLAYER_SKILL_TREES_DIRECTION` define o catálogo canônico INTEIRO de skills do jogador:
~70 nós nomeados em 5 árvores (Guerreiro/Caçador/Místico/Lavrador/Vínculo), com tiers
desbloqueados a 0/5/11/18/26 pontos gastos NA árvore, rank caps por profundidade
(Tier 1 → rank 2; Tier 2 → rank 3; Tier 3 → rank 4; Tier 4/5 → rank 5) e capstones
exclusivos em pares (Kanthor×Kaand no Melee; Anya×Senya no Magic). As tabelas por árvore
da direction (PARTE B-F) trazem, por skill: tier, ranks, tipo (Passive/Modifier/Active/
Unlock/Capstone), mecânica numérica por rank e feedback de HUD — somando 70 nós
(14 Melee + 13 Ranged + 15 Magic + 14 Survival + 14 Crafting).

A `SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0` dá o corpo físico de cada ativa
(telegraph/deslocamento/shape/execução/recovery/custo) e o `BALANCE_CURVES_DIRECTION_v1.0`
confirma a economia: cap 100 com 1 ponto a cada 2 níveis = 50 pontos (estrutura canônica
intacta — F42).

O runtime atual tem árvore genérica com poucos nós placeholder: `SkillTreeManager`
(pontos/compra/respec), `SkillDataSO`, executores da F-slice (MeleeStrike/Projectile/
SelfRestore), `ActiveSkillExecutionController` (slots R/T/Y/G), provider de DerivedStats
(F02) e perfect block hook (F27). Decisão humana vinculante (v1.1 Parte D): REMODELAR o
sistema está CANCELADO — esta spec MIGRA o conteúdo para o sistema existente.

## Problema

Sem o catálogo canônico, a skill tree do jogo é placeholder: tiers não existem (qualquer
nó comprável a qualquer momento), rank caps não existem, capstones exclusivos não existem
e as passivas não produzem efeito real nos sistemas. F39 (classe inferida) depende de
investimento por árvore real para funcionar. Pior: saves existentes carregam IDs de skills
placeholder que deixarão de existir — sem migração explícita, o load quebra ou silencia
pontos gastos do jogador.

## Objetivo

Ao final desta spec, o projeto deve ter os ~70 SkillDataSO canônicos gerados por editor,
cada passiva roteada por um `SkillEffectRoute` tipado até o sistema real correspondente,
tier gating por pontos gastos na árvore (0/5/11/18/26), rank caps, exclusividade de
capstones, ativas canônicas mapeadas aos 3 executores existentes e migração de save com
refund total dos pontos de IDs antigos não-mapeáveis — sem remodelar o sistema e sem
criar segundo manager.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/player_character/PLAYER_SKILL_TREES_DIRECTION.md (catálogo INTEIRO)
docs/design/gameplay/skills/SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md (números por ativa)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§7-8)
docs/design/gameplay/balance/BALANCE_CURVES_DIRECTION_v1.0.md (cap 100 → 50 pontos)
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/rules/save-dto-simple-types-only.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- SkillTreeManager (pontos/compra/respec) — recebe tiers/caps/exclusão por extensão;
- SkillDataSO — recebe campos aditivos (route/payload/tier/rankCap/exclusiveWith/flags);
- executores F-slice: MeleeStrike / Projectile / SelfRestore;
- ActiveSkillExecutionController (active slots R/T/Y/G);
- DerivedStats provider (F02) — destino das passivas de stat;
- perfect block hook (F27) — destino de procs de block.
Não existe:
- catálogo canônico (~70 nós) como assets;
- tiers por pontos-gastos-na-árvore; rank caps; exclusividade de capstone;
- EffectRoutes tipados + agregador de passivas;
- migração de save de skills.
Auditar Fase 0:
- shape do save de skills atual (IDs antigos a migrar, formato da seção, version);
- IDs/contagem dos SkillDataSO placeholder existentes (tabela de de-para ou refund);
- assinatura dos executores (parâmetros aceitos para mapear a tabela de movimento).
```

## Engineering stories

```text
Como jogador, quero que cada ponto gasto produza efeito real (stamina, crit, farm, ouro), não um número morto.
Como jogador, quero tiers que recompensem investimento profundo numa árvore (0/5/11/18/26).
Como jogador, quero a escolha dramática de capstone (Kanthor OU Kaand; Anya OU Senya) com confirmação.
Como sistema de save, quero migração com refund total — nenhum ponto do jogador é perdido.
```

## Escopo

```text
Inclui:
- SkillEffectRoute enum (StatModifier, StaminaCostModifier, CritModifier, ToolModifier,
  FarmModifier, EconomyModifier, UnlockAction, ActiveSkill, EventHook) + payload tipado
  em SkillDataSO (campos aditivos — nenhum campo existente removido/renomeado);
- SkillEffectAggregator: soma passivas compradas → publica no DerivedStats provider e nos
  hooks pontuais (mesma técnica F23 — ponto único de aplicação por sistema consumidor);
- tier gating: pontos GASTOS na árvore ≥ {0,5,11,18,26} para Tiers 1-5; rank caps por nó
  (1-5, conforme profundidade da direction: T1→2, T2→3, T3→4, T4/5→5);
- capstones exclusivos: comprar Kanthor bloqueia Kaand (e vice-versa), Anya bloqueia
  Senya (e vice-versa), com confirmação explícita antes da compra; desbloqueio só por
  respec (regra canônica da Fonte de Anya);
- gerador editor: ~70 SkillDataSO do catálogo (IDs skill_<tree>_<name>) + validação de
  contagem por árvore contra a direction;
- ativas canônicas mapeadas aos 3 executores existentes com os parâmetros da
  SKILL_ACTION_MOVEMENT_TABLE (telegraph/deslocamento/shape/execução/recovery/custo);
  ativas sem executor viável → asset criado com flag NotYetExecutable (dormante, com
  tooltip explicativo — nunca executor paralelo novo);
- migração de save: IDs antigos não-mapeáveis → refund total (pontos de volta ao pool,
  log de migração), version bump na seção existente;
- EditMode tests: tier gating, rank caps, exclusividade de capstones, aggregator
  (mínimo 3 rotas distintas), migração com refund.
```

## Fora de escopo

```text
Não inclui:
- REMODELAR o sistema (CANCELADO — decisão v1.1 Parte D; 50 pontos / 1 por 2 níveis
  preservados — F42);
- skills ativas novas além das canônicas com executor existente;
- executores novos (dormante > parallel system);
- VFX/arte/animação;
- UI nova (a tela F14 já prevê campos de tier/lock);
- balance fino pós-playtest.
```

## Regras de não duplicação

```text
NÃO remodelar: economia 50 pontos / 1 por 2 níveis preservada (F42).
Não criar segundo SkillTreeManager — estender o existente com tiers/caps/exclusão.
Não criar executores novos — ativa sem executor viável fica dormante (NotYetExecutable).
Não criar segundo provider de stats — passivas publicam no DerivedStats provider (F02).
Não criar segundo catálogo/registry de skills — assets gerados no fluxo existente.
```

## Critérios de aceite

### CA-1 Catálogo completo e contado

- A contagem de SkillDataSO por árvore bate com a direction (validador de catálogo).
- Evidência: log do gerador + validação de contagem por árvore (e log do F30 quando rodar).

### CA-2 Tier gating

- Tier 2 inacessível com 4 pontos gastos na árvore; acessível com 5; idem para os
  limiares 11/18/26.
- Evidência: EditMode test parametrizado nos 4 limiares.

### CA-3 Capstones exclusivos

- Kanthor comprado → Kaand bloqueado permanente (até respec) — e teste inverso;
  mesma regra para Anya×Senya; compra exige confirmação.
- Evidência: EditMode tests dos dois pares, ambos os sentidos.

### CA-4 Passiva com efeito real

- Passiva de stamina (ex. redução de custo) reduz o custo real de ataque via integração
  com o DerivedStats provider — não apenas um número no asset.
- Evidência: EditMode test de integração aggregator → provider → custo.

### CA-5 Migração segura

- Save antigo com IDs placeholder migra com refund total de pontos e zero exceções;
  log de migração registra cada ID refundado.
- Evidência: EditMode test de migração com save sintético antigo (round-trip).

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Skills/
  SkillEffectRoute.cs            (NOVO — enum + payload tipado)
  SkillEffectAggregator.cs       (NOVO — soma passivas → provider/hooks)
  SkillDataSO.cs                 (campos aditivos: route, payload, tier, rankCap,
                                  exclusiveWith, NotYetExecutable)
  SkillTreeManager.cs            (extensão: tier gating, rank caps, exclusividade,
                                  hook de migração/refund)
Assets/_Game/Scripts/Editor/Skills/
  GenerateCanonicalSkillCatalog.cs (NOVO — tabela data-driven dos ~70 nós + validação
                                    de contagem por árvore)
Assets/_Game/Tests/EditMode/Skills/
  CanonicalCatalogTests.cs       (NOVO — gating/caps/exclusão/aggregator/migração)
docs/validation/
  fable_29_spec_canonical_skill_catalog_migration_execution_report.md
```

## Contratos

### Data contracts

- `SkillEffectRoute` enum: StatModifier, StaminaCostModifier, CritModifier, ToolModifier,
  FarmModifier, EconomyModifier, UnlockAction, ActiveSkill, EventHook.
- `SkillDataSO` (aditivo): route + payload tipado por rank, tier (1-5), rankCap,
  exclusiveWith (id do capstone par), NotYetExecutable (bool), id `skill_<tree>_<name>`.

### Runtime contracts

- `SkillEffectAggregator`: recalcula no evento de compra/respec; publica agregados no
  DerivedStats provider (F02) e registra hooks pontuais (F27 perfect block, F01 status)
  — ponto único por sistema consumidor.
- `SkillTreeManager`: `CanPurchase(node)` considera tier (pontos gastos na árvore ≥
  {0,5,11,18,26}), rank cap e exclusividade; compra de capstone exige confirmação.

### Event contracts

- N/A novos — o aggregator assina os eventos de compra/respec já existentes no manager.

### Save contracts

- Seção de skills existente, version bump; migração na carga: ID antigo mapeável →
  preserva; não-mapeável → refund total (pontos de volta, log). Somente IDs/ints simples
  no DTO (regra save-dto-simple-types-only).

### UI contracts

- N/A novos — a tela de skill tree (F14) já prevê tier desbloqueado, rank atual/máximo,
  capstone exclusivo e opção bloqueada; esta spec preenche os dados que ela exibe.

## Sistemas afetados

```text
Skill tree (manager/assets/save)
DerivedStats provider (F02 — recebe agregados)
Executores de ativas (Melee/Projectile/SelfRestore — recebem parâmetros canônicos)
ActiveSkillExecutionController (slots R/T/Y/G)
Save/load (migração da seção de skills)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Editor/Skills/**
Assets/_Game/Tests/EditMode/Skills/**
seção de save de skills (migração — arquivo owner auditado na Fase 0)
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets SÓ via gerador editor)
Packages/**
ProjectSettings/**
Executores existentes além de consumir suas APIs (sem executor novo)
DerivedStats provider (publicar nele, não reescrevê-lo)
Qualquer sistema fora de Skills/save de skills
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar manager/save/IDs atuais: shape da seção de save, IDs placeholder existentes,
assinatura dos executores. Montar tabela de de-para (mapeável × refund).

### Fase 1 — Rotas e agregador
SkillEffectRoute + payload em SkillDataSO + SkillEffectAggregator publicando no
provider e nos hooks; testes de 3 rotas.

### Fase 2 — Gating
Tier gating (0/5/11/18/26) + rank caps + capstones exclusivos com confirmação; testes.

### Fase 3 — Catálogo
Gerador data-driven dos ~70 nós (tabela única, IDs skill_<tree>_<name>) + validação
de contagem por árvore.

### Fase 4 — Ativas
Mapeamento das ativas canônicas aos 3 executores com os números da
SKILL_ACTION_MOVEMENT_TABLE; demais ativas marcadas NotYetExecutable (dormantes).

### Fase 5 — Migração e fechamento
Migração de save (refund total + log + version bump) + testes; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO — SOLO.
- Parallel group: N/A.
- Must not run with: QUALQUER outra spec.
- Shared files/systems that require lock: `Skills/**`, SkillTreeManager, geradores de
  skill assets, save de skills.
- Reason: toca simultaneamente skill tree, executores e save de skills — superfícies
  compartilhadas com várias specs da fila (F39 lê o resultado).

## Impacto em save/load

```text
Does this change save schema? YES (campos/version na seção de skills existente)
Does this add a save section? NO (seção existente, version bump)
Does this require migration? YES (IDs antigos não-mapeáveis → refund total de pontos, log)
Does this persist Unity references? NO (apenas IDs e inteiros simples)
Owner/restore order: inalterados — mesma seção, mesmo owner.
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (aggregator assina compra/respec do manager)
```

## Impacto em UI/Unity

```text
Changes UI: NO (tela F14 mostra tiers/locks — campos já previstos)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: YES — somente via gerador editor (evidência obrigatória)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: catálogo enorme (~70 nós × ranks) virar código espalhado e inauditável.
Mitigação: gerador data-driven com tabela única ordenada pela direction, revisável
linha a linha.

Risco: migração destrutiva apagar progresso do jogador.
Mitigação: refund total é lossless em pontos (decisão segura); log de migração por ID;
teste de round-trip com save sintético antigo.

Risco: passiva roteada errado inflar/zerar stats silenciosamente.
Mitigação: aggregator com ponto único por sistema + testes de integração por rota.

Risco: exclusividade de capstone contornável por respec parcial.
Mitigação: regra de desbloqueio só por respec completo na Fonte (canônico) coberta
por teste dos dois sentidos.
```

## Rollback

```text
Flag UseLegacyCatalog mantém os assets antigos (preservados até a validação humana
final) e devolve o manager ao comportamento placeholder. Reverter os arquivos novos
(routes/aggregator/gerador/testes) desfaz a spec; a migração de save só roda quando o
catálogo canônico está ativo. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar manager/save/IDs atuais (tabela de-para mapeável × refund).
- [ ] T002 — Routes + aggregator + hooks (provider/F27/F01) + testes de 3 rotas.
- [ ] T003 — Tier gating (0/5/11/18/26) + rank caps + capstones exclusivos + testes.
- [ ] T004 — Gerador dos ~70 nós + validador de contagem por árvore.
- [ ] T005 — Mapeamento de ativas aos executores (tabela de movimento) + dormantes NotYetExecutable.
- [ ] T006 — Migração de save (refund total + log) + testes; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gating, caps, exclusividade, agregação, migração)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (migração de save com refund; comportamento placeholder
  preservado sob flag)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano comprando até tier 2
  e usando 1 ativa nova

## Definition of Done

```text
Catálogo canônico completo (~70 nós, contagem validada por árvore), gated por tiers
0/5/11/18/26 e rank caps, capstones exclusivos com confirmação, passivas com efeito real
via aggregator/provider, ativas mapeadas ou dormantes documentadas, migração de save com
refund total e log; sistema único (zero managers/executores paralelos); evidência de
geração de assets registrada; builds 0E; execution report criado.
```

## Anti-regressão

```text
Economia canônica intacta: cap 100, 1 ponto/2 níveis, 50 pontos (F42) — REMODELAR CANCELADO.
Nenhum ponto de jogador perdido na migração (refund total).
Respec existente continua funcionando (Fonte de Anya).
Slots R/T/Y/G e executores existentes inalterados em contrato público.
Nenhuma referência Unity em save; nenhum GameObject.Find em runtime.
Skill point nunca gasto sem confirmação em capstone exclusivo.
```

> **Nota de anti-regressão (atualizada pela EMENDA 2026-06-13-V3):** a linha "Slots R/T/Y/G
> inalterados" acima é **superada** pelo item 7 da emenda abaixo — o caminho real de slots
> passa a ser o `ActiveSkillExecutionController` (teclas 1-4) e o `ActiveSkillSlots.cs` legado
> (R/T/Y/G) é aposentado. Ler a emenda antes de tratar slots.

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md, BLOCO 1 + re-auditoria 2026-06-13)

> Esta emenda preserva TODO o conteúdo original acima. Em conflito, esta seção vence (mais nova).
> Decisões aplicadas: 1.1, 1.3, 1.4, 1.6, 1.10, 1.11 + acréscimos #1 e "5 modificadores mortos"
> da re-auditoria de código (`reaudit-code-v3`). A contagem canônica do catálogo passa de
> "~70" / "55" para **69 nós** (patch WI-11 confirmado pela re-auditoria).

```text
1. RANK CAP DINÂMICO POR PROFUNDIDADE (decisão 1.1 — ★):
   - O rank cap NÃO é mais fixo por nó isolado. Quando o jogador desbloqueia um tier na árvore
     (gastando pontos: 5/11/18/26 — ver item 6 desta emenda para os limiares atualizados pela
     F42), o cap de TODOS os nós já comprados naquela árvore SOBE para o cap daquele tier:
       Tier 1 aberto  -> cap de rank 2 em todos os nós da árvore;
       Tier 2 aberto  -> cap de rank 3;
       Tier 3 aberto  -> cap de rank 4;
       Tier 4/5 aberto -> cap de rank 5.
   - Substitui o texto original "rank caps por nó (T1->2, T2->3, T3->4, T4/5->5)" (Contexto e
     Escopo): aquele mapeamento permanece, mas o gatilho é o TIER DESBLOQUEADO NA ÁRVORE, não a
     profundidade estática do nó. SkillTreeManager.CanRankUp(node) consulta o tier MÁXIMO
     desbloqueado da árvore daquele nó.
   - CA-2 (Tier gating) ganha sub-critério CA-2b: ao desbloquear o Tier 2 (cap dinâmico), um nó
     de Tier 1 já no rank 2 passa a poder subir ao rank 3. EditMode test obrigatório.

2. PRÉ-REQUISITOS POR NÓ (decisão 1.3 — OVERRIDE, "B"):
   - Além do tier gating, cada nó passa a ter cadeia de pré-requisitos por nó (ex.: nó X exige
     nó Y rank >= R). SkillDataSO ganha campo aditivo `prerequisites` (lista de {skillId,
     minRank}) — somente IDs/inteiros simples (regra save-dto-simple-types-only não é afetada:
     é dado de catálogo, não de save).
   - As cadeias dos 69 nós são autoradas no ADENDO NUMÉRICO (ver item 3) — esta spec CONSOME a
     coluna de pré-requisitos do adendo; não a inventa. A `PLAYER_SKILL_TREES_DIRECTION` ganha
     coluna de pré-requisito por nó (autoria fora do escopo de runtime desta spec; é direction).
   - CanPurchase(node) passa a checar: tier desbloqueado E pré-requisitos satisfeitos E rank cap
     dinâmico E exclusividade de capstone. EditMode test obrigatório (nó com pré-req insatisfeito
     não-comprável; satisfeito comprável).

3. ADENDO NUMÉRICO COMO CONTRATO DOS EXECUTORES (decisão 1.4 — ★):
   - O dano base / cooldown / custo de cada ativa (tabela skill × rank) e as cadeias de
     pré-requisito dos 69 nós vivem num ADENDO NUMÉRICO ÚNICO anexado à
     `SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0` — autorado ANTES da execução desta spec.
   - Esse adendo é o CONTRATO dos executores: o mapeamento de ativas aos 3 executores (Fase 4 /
     T005) lê dano/cooldown/custo do adendo, não inventa números. Substitui a referência genérica
     "os números da SKILL_ACTION_MOVEMENT_TABLE" por "os números do ADENDO NUMÉRICO anexo".
   - Fonte obrigatória adicionada à lista da spec:
       docs/design/gameplay/combat/SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md
         + seu ADENDO NUMÉRICO (tabela skill×rank + coluna de pré-requisitos dos 69 nós).
   - Se o adendo não existir no working tree no momento da execução, esta spec fica
     BLOCKED_BY_DEPENDENCY_PENDING até o adendo ser anexado (não inventar números).

4. HOOKS NOMEADOS COM "EFEITO PENDENTE" (decisão 1.6 — ★):
   - O enum genérico SkillEffectRoute ganha rotas que publicam HOOKS NOMEADOS por interface, em
     vez de números soltos. Interfaces de hook a publicar (no mínimo):
       IGoldDropModifier      (consumidor futuro: F06 economia/loot)
       IHarvestYieldModifier  (consumidor futuro: F17 farm)
       ICraftCostModifier     (consumidor futuro: F31 craft)
       IToolEfficiencyModifier(consumidor futuro: F48/F49 ferramentas/recursos)
       (+ demais hooks nomeados conforme as passivas do catálogo: F55 etc.)
   - Enquanto o consumidor (F06/F17/F31/F48/F49/F55) ainda não existir, a passiva É comprável e o
     hook É publicado pelo SkillEffectAggregator, mas o tooltip do nó declara "efeito pendente"
     (campo de UI alimentado por flag no SkillDataSO; a tela F14 só exibe). NÃO marcar essas
     passivas como dormantes/NotYetExecutable — elas são passivas, não ativas; a dormência é só
     para ATIVAS sem executor (decisão 1.5, inalterada).
   - Cada spec consumidora ganha 1 linha de escopo "consome hook X" (emenda separada, fora desta
     spec). Aqui basta PUBLICAR o hook nomeado e o tooltip "efeito pendente".
   - Acréscimo ao CA-4 (Passiva com efeito real): vale para passivas cujo consumidor JÁ existe
     (ex.: rota de stamina -> DerivedStats provider F02). Para hooks de consumidor futuro, a
     evidência é "hook publicado + tooltip pendente", não efeito no sistema (EditMode test de
     publicação do hook nomeado).

5. MARCADOR DE PRESA — TEXTO CANÔNICO (decisão 1.10 — ★):
   - O efeito do nó "Marcador de Presa" passa a ler, no asset e no tooltip:
       "aumenta o dano do jogador E de aliados invocados, quando existirem".
   - A implementação roteia o modificador de dano de modo que companions/invocados herdem o hook
     automaticamente quando a WAVE 14 (companions em combate) chegar — SEM retrabalho no nó. Por
     ora, sem companion ativo, o efeito aplica só ao jogador; o texto canônico já é o final.

6. RESPEC PUNITIVO + REVALIDAÇÃO DE EQUIPAMENTO (decisão 1.11 — OVERRIDE, "B"):
   - O respec deixa de ser apenas refund de pontos: passa a ser PUNITIVO. Ao respecar, tiers que
     deixaram de estar desbloqueados (porque o investimento na árvore caiu abaixo do limiar)
     RE-BLOQUEIAM os itens cujo uso/equip dependia daquele tier.
   - Novo SISTEMA DE REVALIDAÇÃO DE EQUIPAMENTO no respec: itens de tiers re-bloqueados
     PERMANECEM no inventário, mas ficam NÃO-EQUIPÁVEIS / NÃO-USÁVEIS até o tier ser
     redesbloqueado. Não destrói item, não desequipa silenciosamente sem aviso — a confirmação de
     respec lista o que ficará bloqueado.
   - Escopo adicionado a esta spec: gancho de revalidação no fluxo de respec do SkillTreeManager,
     que consulta o estado de tiers pós-respec e marca itens afetados como bloqueados (a checagem
     "pode equipar?" do sistema de equipamento passa a respeitar esse gate — leitura de gate, não
     reescrita do sistema de equipamento).
   - Novo CA-6 (Respec punitivo): após respec que rebaixa um tier, um item antes equipável que
     dependia daquele tier fica não-equipável; após redesbloquear o tier, volta a equipável.
     EditMode test obrigatório (round-trip bloqueia/desbloqueia).
   - Atualiza a linha de anti-regressão "Respec existente continua funcionando": continua, MAS
     agora com revalidação punitiva de equipamento acoplada.

7. DESAMBIGUAÇÃO DOS DOIS SISTEMAS DE SLOT DE SKILL (re-auditoria, acréscimo #1):
   - O working tree tem DOIS sistemas de slot paralelos:
       MANTER:   Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
                 (teclas 1-4 — caminho REAL, patch WI-11).
       APOSENTAR: Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs
                 (teclas R/T/Y/G — legado).
   - Esta spec aposenta o `ActiveSkillSlots.cs` legado (remoção do componente do fluxo runtime /
     marcação como obsoleto, conforme padrão de aposentadoria do projeto — sem criar terceiro
     sistema). Todas as referências a "slots R/T/Y/G" na spec original são reinterpretadas como
     slots 1-4 do ActiveSkillExecutionController.
   - Anti-regressão revisada: o contrato público de slots vivo é o do ActiveSkillExecutionController
     (1-4). Nenhuma ativa canônica deve ser cabeada ao ActiveSkillSlots legado.

8. CINCO MODIFICADORES DE SKILL MORTOS (re-auditoria — decidir consumo OU aposentar):
   - Modificadores declarados em Assets/_Game/Scripts/Skills/SkillEnums.cs e atribuídos a nós em
     Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs, porém SEM consumidor:
       BowProjectileSpeedFlat, DualWieldAttackSpeedBonus, TwoHandedDamageBonus,
       DodgeCostReduction, StatusDurationReduction.
   - DECISÃO desta spec: para cada um, implementar o CONSUMO real em DerivedStatsCalculator
     (passando pelo SkillEffectAggregator -> DerivedStats provider F02), OU aposentar o
     modificador do enum E remover sua atribuição dos nós do DefaultSkillCatalog. NÃO deixar
     modificador morto atribuído a nó (número que não faz nada — viola o engineering story
     "cada ponto produz efeito real").
   - O execution report DEVE declarar, por modificador, qual rota foi tomada (consumido | aposentado)
     com o ponto de consumo (arquivo:método) ou a remoção (enum + nós afetados).
   - EditMode test: para os modificadores que forem CONSUMIDOS, teste de integração
     aggregator -> DerivedStatsCalculator confirmando efeito; para os APOSENTADOS, teste de que
     nenhum nó do catálogo os referencia.

9. CORREÇÃO DE COMENTÁRIO STALE "55 nodes" -> 69:
   - Assets/_Game/Scripts/Editor/Validation/ValidateSkillTreeRuntimeBinding.cs:24 tem o comentário
     "Verify DefaultSkillCatalog builds all 55 nodes and 5 trees". A contagem canônica real é
     69 nós (patch WI-11). Corrigir o comentário para 69 e alinhar qualquer asserção de contagem
     do validador ao número 69. Toda referência a "~70" / "55" no corpo original desta spec deve
     ser lida como 69 nós (14 Melee + 13 Ranged + 15 Magic + 14 Survival + 13 Crafting = 69;
     a contagem por árvore final é a do catálogo WI-11 / validador atualizado).
   - CA-1 (Catálogo completo e contado): a contagem validada por árvore deve somar 69, não 70/55.
```

### Critérios de aceite adicionados por esta emenda

```text
CA-2b: rank cap dinâmico — desbloquear o Tier 2 sobe o cap de um nó T1 já no rank 2 para 3
       (EditMode test).
CA-7 (pré-requisitos): nó com pré-requisito insatisfeito não é comprável; satisfeito é
       comprável (EditMode test, dois sentidos).
CA-8 (hooks nomeados): IGoldDropModifier / IHarvestYieldModifier / ICraftCostModifier /
       IToolEfficiencyModifier publicados pelo aggregator; tooltip "efeito pendente" quando o
       consumidor ainda não existe (EditMode test de publicação do hook).
CA-6 (respec punitivo): item dependente de tier rebaixado fica não-equipável após respec e
       volta equipável após redesbloqueio (EditMode test round-trip).
CA-MOD (modificadores mortos): cada um dos 5 declarado como consumido (com ponto de consumo) ou
       aposentado (sem atribuição em nó); nenhum nó referencia modificador morto (EditMode test).
CA-COUNT: contagem canônica do catálogo = 69 (validador alinhado; comentário stale corrigido).
```

### Arquivos permitidos adicionados por esta emenda

```text
Assets/_Game/Scripts/Skills/SkillEnums.cs (aposentar/ajustar modificadores mortos — item 8)
Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs (remover atribuições de modificador morto;
  alinhar contagem 69 — itens 8/9)
Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs (aposentadoria do legado — item 7)
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs (caminho real de
  slots — consumir, não reescrever contrato)
Assets/_Game/Scripts/Editor/Validation/ValidateSkillTreeRuntimeBinding.cs (corrigir 55->69 — item 9)
sistema de equipamento: SOMENTE leitura de gate de tier no "pode equipar?" (item 6 — não reescrever)
DerivedStatsCalculator: SOMENTE consumir modificadores (item 8 — publicar via aggregator/provider)
```

### Impacto em save/load (atualização)

```text
SkillDataSO ganha `prerequisites` (catálogo, não save) e flag de tooltip "efeito pendente".
Nenhuma referência Unity em save (inalterado).
O estado de "tier desbloqueado" que gate a equipabilidade (item 6) é DERIVADO de pontos gastos
na árvore (já persistidos); não introduz novo campo de save destrutivo. Documentar no report.
```

### Dependência adicionada

```text
Depends on (adicionado): ADENDO NUMÉRICO de skills anexo à SKILL_ACTION_MOVEMENT_TABLE
  (itens 3 e 2 desta emenda). Sem o adendo, a spec é BLOCKED_BY_DEPENDENCY_PENDING para as
  Fases 3 (catálogo: pré-requisitos) e 4 (ativas: números) — nunca inventar números/cadeias.
```
