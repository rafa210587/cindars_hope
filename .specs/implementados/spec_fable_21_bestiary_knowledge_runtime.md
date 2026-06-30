# SPEC — Bestiário: Conhecimento por Descoberta (Runtime + Save)

> **Spec ID:** `fable_21_spec_bestiary_knowledge_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P2
> **Type:** Runtime / Save
> **Domain:** Bestiary / Cave
> **Parallelizable:** NO (save schema lock)
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** F07, F12, F13, F26, F42 (seções de save — um por vez)
> **Repo lock scope:** `Bestiary/**`, `GameSaveData` (seção nova), DamageCalculator hooks
> **Depends on:**
> - F06 (vulnerabilidades autoradas)
> - F33 (BestiaryEntryId nos 60)
> **Blocks:**
> - aba Bestiário (F14 placeholder → conteúdo)
> **Scope:** EnemyKnowledgeState com thresholds concretos, fontes externas e persistência.
> **Out of scope:** Bestiary UI rica (aba F14 lista entradas; UI completa = WAVE 22 futuras), pet/companion hints.

required_adrs: []
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

`BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md` define o modelo completo do sistema de
conhecimento por descoberta: um `EnemyKnowledgeState` progressivo por criatura
(Unknown → Seen → Fought → Defeated → RepeatedDefeated → Studied), categorias de
informação reveláveis de forma independente (Identity, BehaviorSummary, AttackList,
ElementVulnerability, DropsCommon, DropsRare, ResistanceTags etc. — §6 da direction) e
`SpoilerTier` 0-4 que gate a exibição conforme progresso de quest (§20). As decisões
humanas (`FABLE_DECISOES_RESPOSTAS_v1.0.md` §C1) fixaram os THRESHOLDS numéricos de
desbloqueio. O catálogo canônico (`CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md` §2.5)
atribui SpoilerTier por criatura: comuns 0-1, minibosses 2, bosses de gate 3, os Quatro
do 101 = tier 4, e fixa `BestiaryEntryId = enemy_id`.

Nada disso existe em runtime — o `BestiaryManager` atual é stub no bootstrap. Esta spec
entrega o NÚCLEO mecânico (observação de eventos, contadores, thresholds, fontes
externas, spoiler gate, save), deixando UI rica para specs futuras. A aba Bestiário da
F14 (hoje placeholder) e os tooltips de efetividade de equipment/spell consomem as APIs
criadas aqui. O escopo é pequeno porque a direction já isola "primeira entrega" (§21):
sem Bestiary Menu completo, sem scanning, sem pesquisa, sem companion hints.

## Problema

Sem o serviço de conhecimento, três sistemas ficam travados ou incorretos:
a aba Bestiário (F14) permanece placeholder sem conteúdo; os tooltips de efetividade
revelariam vulnerabilidades que o jogador nunca descobriu (spoiler e quebra da
filosofia de descoberta da direction); e os serviços de NPC que concedem conhecimento
(análise da Thalindra na F25, livros da Yael) não têm API alvo. Além disso, sem
persistência o progresso de descoberta se perderia a cada load, tornando o sistema
inútil em runs longas.

## Objetivo

Ao final desta spec, o projeto deve ter um `EnemyKnowledgeService` (hospedado no
`BestiaryManager` existente) que observa eventos de combate, acumula contadores por
`enemyId`, desbloqueia categorias pelos thresholds do §C1, expõe APIs de concessão
externa (`GrantKnowledge`) e de gate de spoiler (`IsVisible`), publica evento de
desbloqueio e persiste em seção própria de save — permitindo que F14 (aba), F22/F25
(consumidores) e tooltips usem conhecimento real, sem alterar autoração de
vulnerabilidades (F06) nem UI rica (futura).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (SpoilerTiers por criatura)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§C1 thresholds)
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- BestiaryManager (stub no bootstrap) — hospedar o serviço nele;
- BestiaryEntryId em EnemyDataSO (F33 garante os 60 preenchidos);
- EnemyKilledEvent, StatusEffectAppliedEvent (F01) e eventos de dano no GameEventBus;
- geradores de bestiário (CreateBestiaryEntries40);
- SaveManager com padrão de seção WI-18 (providers por seção, DTOs simples);
- QuestFlagService (fonte dos gates de quest para SpoilerTier).
Não existe:
- knowledge state, contadores por enemyId, thresholds, GrantKnowledge, spoiler gate,
  seção de save de bestiário, evento de desbloqueio.
Auditar Fase 0:
- shape do BestiaryManager atual e do CreateBestiaryEntries40 (o que o stub já expõe);
- quais eventos de dano existentes carregam enemyId + elemento + resultado
  (efetivo/resistido) — se faltar payload, documentar gap em vez de alterar evento
  fora de escopo;
- nomes das flags de quest usadas como gate por tier (mapear com QuestFlagService).
```

## Engineering stories

```text
Como jogador, quero que matar/observar criaturas revele gradualmente suas informações,
  para que a caverna recompense atenção e repetição.
Como serviço de NPC (F25), quero uma API GrantKnowledge(enemyId, categoria, fonte) para
  conceder conhecimento sem grind (análise da Thalindra, livros da Yael).
Como tooltip de efetividade (F14/F22), quero IsVisible(enemyId, categoria, tier) para
  nunca revelar vulnerabilidade não descoberta.
Como sistema de save, quero persistir apenas IDs e contadores simples por enemyId,
  sem referências Unity, com load legado seguro (bestiário vazio).
```

## Escopo

```text
Inclui:
- EnemyKnowledgeService (host no BestiaryManager existente): contadores por enemyId
  {kills, secondsFought, actionsSeen[], effectiveHits[elemento], resistedHits};
- Thresholds (C1): identificação=1 avistamento; behavior=ver ação 3× OU sofrer 1×;
  vulnerabilidade=3 hits efetivos do eixo; drops comuns=5 kills; raros/resistências=fonte
  externa OU 3 eventos "resistido";
- fontes externas: API GrantKnowledge(enemyId, categoria, fonte) — consumida pela análise
  da Thalindra (F25) e por livros (item key futura);
- SpoilerTier gate: API IsVisible(enemyId, categoria, tier) respeitando progresso de quest;
- eventos: BestiaryKnowledgeUnlockedEvent(enemyId, categoria, novoTier) → toast;
- save: BestiaryKnowledgeSaveData (List por enemyId, contadores+categorias) padrão WI-18;
- consumo: tooltips de efetividade (F14 equipment/spell) chamam IsVisible antes de revelar;
- EditMode tests: thresholds, idempotência, spoiler gate, round-trip, load legado.
```

## Fora de escopo

```text
Não inclui:
- Bestiary Menu completo / Bestiary HUD / scanning / pesquisa (direction §21 — futuras);
- UI rica da aba Bestiário (a aba F14 mostra lista nome+tier; conteúdo completo = WAVE 22);
- pet/companion knowledge hints;
- autoração de vulnerabilidades ou drops (F06 autora; o bestiário só REVELA);
- alteração do payload de eventos de combate existentes fora do necessário documentado;
- balance fino dos thresholds (valores C1 são canônicos nesta spec).
```

## Regras de não duplicação

```text
Não recriar BestiaryManager — hospedar o serviço nele.
Não autorar vulnerabilidades/drops (F06 autora; o bestiário REVELA dados existentes).
Não criar segundo caminho de save — usar o padrão de seção WI-18 do SaveManager.
Não criar segundo gate de spoiler — IsVisible é o único ponto de decisão de visibilidade.
UI completa fora (aba F14 mostra lista nome+tier).
```

## Critérios de aceite

### CA-1 Thresholds de descoberta por combate

- Matar 5 slimes revela drops comuns da criatura; 3 hits de fogo efetivos revelam
  ElementVulnerability (eixo fire); 1 avistamento revela identificação; ver a mesma ação
  3× OU sofrê-la 1× revela behavior; 3 eventos "resistido" revelam resistência.
- Evidência: EditMode tests cobrindo cada threshold com eventos sintéticos.

### CA-2 Fontes externas idempotentes

- GrantKnowledge(enemyId, categoria, fonte) concede a categoria sem grind (caminho da
  análise da Thalindra); chamadas repetidas não duplicam estado nem re-publicam evento.
- Evidência: EditMode test de concessão + idempotência.

### CA-3 Spoiler gate por tier

- Categorias com SpoilerTier 3+ ficam ocultas (IsVisible=false) até a flag de quest
  correspondente; teste cobre cada tier (0-4) com flags sintéticas.
- Evidência: EditMode tests do gate por tier.

### CA-4 Persistência e compatibilidade

- Round-trip de save preserva contadores e categorias desbloqueadas; load de save legado
  (sem a seção) resulta em bestiário vazio sem erro.
- Evidência: EditMode tests de round-trip e load legado.

### CA-5 Evento de desbloqueio

- Cada desbloqueio novo de categoria publica BestiaryKnowledgeUnlockedEvent(enemyId,
  categoria, novoTier) exatamente uma vez (consumido por toast existente).
- Evidência: EditMode test de publicação única por desbloqueio.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Bestiary/
  EnemyKnowledgeService.cs        (NOVO — lógica de contadores/thresholds/gate)
  EnemyKnowledgeState.cs          (NOVO — estado por enemyId, tipos simples)
  BestiaryKnowledgeSaveData.cs    (NOVO — DTO de save, padrão WI-18)
Assets/_Game/Scripts/Core/Events/
  BestiaryEvents.cs               (NOVO — BestiaryKnowledgeUnlockedEvent)
SaveManager (wiring da seção nova — provider aditivo)
BestiaryManager (host: instancia/expõe o serviço; permanece o único manager)
Assets/_Game/Tests/EditMode/World/
  BestiaryKnowledgeTests.cs       (NOVO)
docs/validation/
  fable_21_spec_bestiary_knowledge_runtime_execution_report.md
```

## Contratos

### Data contracts

`EnemyKnowledgeState` por enemyId: contadores {kills:int, secondsFought:float,
actionsSeen:List<string>, effectiveHits:Dictionary<elemento,int>, resistedHits:int} +
conjunto de categorias desbloqueadas (enum/string estável conforme direction §6).
Categorias usam nomes estáveis (Identity, BehaviorSummary, ElementVulnerability,
DropsCommon, DropsRare, ResistanceTags...). SpoilerTier por criatura vem do catálogo
(comuns 0-1, miniboss 2, boss 3, Quatro do 101 = 4).

### Runtime contracts

`EnemyKnowledgeService`: `RecordSighting(enemyId)`, observadores de eventos de combate
(kill / hit efetivo por elemento / hit resistido / ação vista), `GrantKnowledge(enemyId,
categoria, fonte)`, `IsVisible(enemyId, categoria, tier)`, `IsUnlocked(enemyId,
categoria)`. Hospedado no BestiaryManager (sem novo manager no bootstrap).

### Event contracts

Novo: `BestiaryKnowledgeUnlockedEvent(enemyId, categoria, novoTier)` publicado no
GameEventBus (consumido por toast). Consome eventos existentes: EnemyKilledEvent,
StatusEffectAppliedEvent, eventos de dano (efetivo/resistido) — sem alterá-los.

### Save contracts

Seção nova aditiva `BestiaryKnowledgeSaveData` { List de entradas {enemyId:string,
contadores simples, categorias desbloqueadas:List<string>} } — default vazio, sem
migration, sem refs Unity, padrão WI-18. Restore após registries de enemy disponíveis.

### UI contracts

N/A nesta spec — a aba Bestiário (F14) e tooltips consomem `IsVisible`/`IsUnlocked`;
nenhuma tela nova é criada aqui (justificativa: UI rica é WAVE 22 futura).

## Sistemas afetados

```text
Bestiary (núcleo novo)
Save/load (seção nova aditiva)
Event bus (evento novo + observação de eventos de combate)
Tooltips de efetividade (consumo futuro — F14/F22)
Serviços de NPC (consumo futuro — F25)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Core/Events/BestiaryEvents.cs
SaveManager (wiring aditivo da seção — arquivo do provider/registro de seções)
BestiaryManager (host do serviço)
Assets/_Game/Tests/EditMode/World/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida)
Packages/**
ProjectSettings/**
EnemyDataSO / autoração de vulnerabilidades e drops (domínio F06/F33)
Eventos de combate existentes (payload não muda nesta spec)
docs_old/** e demais paths legados
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar BestiaryManager/CreateBestiaryEntries40; mapear eventos de dano existentes
(payload enemyId/elemento/resultado) e flags de quest por tier. Não recriar nada.

### Fase 1 — Estado e thresholds
EnemyKnowledgeState + EnemyKnowledgeService com thresholds C1 puros (testáveis sem cena).

### Fase 2 — Observadores de eventos
Assinaturas no GameEventBus (kill / dano efetivo / resistido / ação vista) alimentando
os contadores; unsubscribe correto.

### Fase 3 — Fontes externas e spoiler gate
GrantKnowledge (idempotente) + IsVisible por tier consultando QuestFlagService.

### Fase 4 — Save
BestiaryKnowledgeSaveData + provider WI-18 + round-trip/load legado.

### Fase 5 — Evento, testes e relatório
BestiaryKnowledgeUnlockedEvent + toast; EditMode tests completos; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO (save schema lock)
- Parallel group: N/A
- Must not run with: F07, F12, F13, F26, F42 (cada um adiciona seção de save — um por vez)
- Shared files/systems that require lock: `Bestiary/**`, `GameSaveData`/SaveManager
  (seção nova), hooks de DamageCalculator/eventos de dano
- Reason: adicionar seção de save em paralelo a outra spec de save gera conflito de
  schema/provider; o lock garante serialização.

## Impacto em save/load

```text
Does this change save schema? YES — seção nova aditiva (BestiaryKnowledgeSaveData)
Does this add a save section? YES (default vazio, owner: BestiaryManager/serviço)
Does this require migration? NO (aditiva; load legado = bestiário vazio)
Does this persist Unity references? NO (IDs e tipos simples apenas)
Restore order: após registries de enemy/quest flags disponíveis (padrão WI-18).
```

## Impacto em eventos

```text
Adds events: YES — BestiaryKnowledgeUnlockedEvent(enemyId, categoria, novoTier)
Changes existing events: NO (apenas assina kill/dano/status existentes)
Requires unsubscribe pattern: YES (serviço assina GameEventBus no host)
```

## Impacto em UI/Unity

```text
Changes UI: NO (consumido por F14; toast existente exibe o evento)
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: custo por hit (alocação/lookup a cada evento de dano).
Mitigação: contadores em dicionário em memória; flush para DTO apenas no save.
Risco: eventos de dano atuais sem payload de elemento/resultado suficiente.
Mitigação: detectar na Fase 0; se faltar, documentar gap e implementar apenas o
  observável hoje (sem alterar eventos fora de escopo).
Risco: dupla publicação de desbloqueio (re-check de threshold).
Mitigação: desbloqueio registrado em set; teste de idempotência (CA-2/CA-5).
Risco: spoiler gate divergir entre tooltip e aba.
Mitigação: IsVisible é o único ponto de decisão (regra de não duplicação).
```

## Rollback

```text
Remover EnemyKnowledgeService/EnemyKnowledgeState/BestiaryKnowledgeSaveData e o registro
da seção; o jogo volta ao estado sem-bestiário (stub). Saves que contenham a seção são
ignorados com segurança (seção desconhecida não quebra load). Não apagar save real.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar BestiaryManager/geradores existentes + payload dos eventos de dano
        + flags de quest por tier.
- [ ] T002 — KnowledgeState + thresholds (C1) + testes EditMode puros.
- [ ] T003 — Observadores de eventos de combate (kill/dano efetivo/resistido/ação vista)
        com unsubscribe.
- [ ] T004 — GrantKnowledge (fontes externas, idempotente) + spoiler gate IsVisible.
- [ ] T005 — Save section WI-18 + testes round-trip/load legado.
- [ ] T006 — BestiaryKnowledgeUnlockedEvent + toast; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (thresholds, contadores, gate)
- Requires EditMode tests: YES (thresholds, idempotência, spoiler gate, round-trip, legado)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (load legado sem seção; eventos de combate intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano desbloqueando
  1 entrada por combate e 1 por análise

## Definition of Done

```text
Conhecimento por descoberta funcional e persistido (thresholds C1, GrantKnowledge,
IsVisible, evento de desbloqueio); bestiário REVELA, nunca cria dados; sem refs Unity
no save; saves antigos carregam (bestiário vazio). EditMode tests passando.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Bestiário REVELA, nunca autora vulnerabilidades/drops (F06 é a fonte).
Sem referências Unity no save (apenas IDs/contadores simples).
Saves antigos sem a seção carregam sem erro.
Eventos de combate existentes não mudam de payload.
Nenhum GameObject.Find/FindObjectOfType em runtime; comunicação só via GameEventBus.
Tooltips nunca exibem categoria com IsVisible=false.
```


---

## EMENDA 2026-06-12-B (auditoria de completude — VINCULANTE)

```text
1. SKILL POINTS POR MARCO DE BESTIÁRIO (BALANCE_CURVES §1, estava sem dono): +1 skill
   point a cada 10 entradas que atingirem o estado "Estudada", máximo 5 pontos no total.
   Idempotente (marco concedido 1× — persiste contador de marcos concedidos na seção de
   save desta spec) e usa o MESMO hook de concessão de pontos da F34/F42.
2. Teste obrigatório adicional: marco não re-concede após save/load.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
EMENDA-D adicional (6.7): recompensa por entrada FullyDocumented é MECÂNICA (+stats pequenos
contra a criatura documentada — ex.: +3% dano; definir magnitude na Fase 0); bosses revelam
a ficha APÓS DERROTA (não por quest).
```
