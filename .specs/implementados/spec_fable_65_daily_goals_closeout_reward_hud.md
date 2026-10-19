# SPEC — Farm: Daily Goals — Recompensa + Painel no HUD + Expansão de Metas

> **Spec ID:** `fable_65_spec_daily_goals_closeout_reward_hud`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Runtime / UI / Integration
> **Domain:** Farm / UI / Economy
> **Parallelizable:** CONDITIONAL (lock do GameplayHudCanvas e do FarmDailyGoalService)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F61, F63, F64, F67
> **Must not run with:** F62 (ambas adicionam widgets no GameplayHudCanvas — mesma superfície), F66 (toca arquivos de Farm — coordenar)
> **Repo lock scope:** FarmDailyGoalService, GameplayHudCanvas/views (WI-23), PlayerProgressionManager (consumo AddXp)
> **Depends on:**
> - WI-24 (existente — FarmDailyGoalService com 2 metas, eventos e Capture/Restore)
> - F42 (executada — curva de XP cap 100; a recompensa de XP usa AddXp na curva real)
> - F32 (E18 — catálogo de itens; CONDICIONAL: recompensa v1 é ouro+XP; item entra se
>   E18 existir)
> - F13 (executada — save debt closure; verificar na Fase 0 se SAVE_LOAD_DAILY_GOAL_DEBT
>   foi fechado; senão fechá-lo aqui)
> **Blocks:** N/A
> **Scope:** recompensa por meta concluída (ouro pequeno + XP), widget compacto de metas no HUD, expansão para 4-6 metas/dia.
> **Out of scope:** streaks/bônus semanais, metas de caverna/combate, UI de histórico, rebalance econômico amplo, metas configuráveis pelo jogador.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md, ADR-0006-save-data-contracts-simple-dtos.md]
required_game_rules: [farm_rules.md, save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

WAVE_INTEGRATION_24 entregou o FarmDailyGoalService com débito declarado e documentado:
`DAILY_GOAL_REWARD_DEFERRED` (P3 — "Gold/item reward on completion deferred to economy
balance phase") e `DAILY_GOAL_HUD_DISPLAY_DEFERRED` (P3 — "No dedicated HUD panel for
goal display; feedback toast only"), ambos em
docs/validation/WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md e no execution report.

O estado real do serviço (Farm/Runtime/FarmDailyGoalService.cs): 2 metas hardcoded
(`daily_goal_first_harvest`, `daily_goal_sell_first_crop`), reset em DayStartedEvent,
progresso por CropHarvestedEvent/EconomyTransactionCompletedEvent, eventos
DailyGoalProgressedEvent/DailyGoalCompletedEvent publicados, CaptureSaveData/
RestoreFromSaveData prontos — e um campo `Claimed` no estado que NUNCA é setado (a
recompensa que não existe). O WI-24 também registrou SAVE_LOAD_DAILY_GOAL_DEBT (P2 —
SaveManager não chama Capture/Restore) e DAILY_GOAL_EDITMODE_TESTS (P2) — a F13 pode
ter fechado o primeiro; auditar.

Para recompensa: PlayerProgressionManager.AddXp(int) existe e já é usado por quests; o
canal de toast (WI-23) anuncia conclusões; o GameplayHudCanvas tem o padrão de views
(StatusBars/QuestTracker/etc.) para o widget novo. A expansão para 4-6 metas/dia segue
a direção de farm design e as decisões registradas do domínio (tabela auditada na
Fase 0 — fonte: FARM_DESIGN/decisões; valores propostos abaixo são v1 conservador).

## Problema

Metas diárias sem recompensa são checklist morto: o jogador completa "primeira colheita"
e nada acontece além de um toast. Sem painel no HUD, ele nem sabe que as metas existem
(só descobre pelo toast depois do fato). Com 2 metas, o sistema não orienta um dia de
jogo. E o campo Claimed órfão é débito de design declarado esperando dono.

## Objetivo

Ao final desta spec: (1) meta concluída concede recompensa automática — ouro pequeno +
XP via AddXp (valores por tabela única; Claimed setado; idempotente por dia, inclusive
após reload); (2) um widget compacto no GameplayHudCanvas lista as metas do dia com
check de conclusão (padrão das views WI-23, escondível junto com o HUD); (3) o catálogo
passa de 2 para 4-6 metas/dia (ex.: regar N plots, colher N, vender N, quebrar 1 recurso,
falar com 1 NPC — tabela final auditada contra a direção de farm na Fase 0); (4) débitos
WI-24 do domínio fechados ou re-registrados com dono (save hook, testes).

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs (estado real; campo Claimed)
docs/validation/WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md (débitos declarados)
docs/validation/WAVE_INTEGRATION_24_SAVE_LOAD_FARM_DAILY_GOAL_MATRIX.md (save debt)
docs/validation/WAVE_INTEGRATION_24_FARM_LOOP_DEPTH_execution_report.md (deferred work)
Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs (AddXp)
Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs + Views/** (padrão de view)
docs/game_rules/farm_rules.md
direção de farm design (tabela de metas — caminho auditado na Fase 0)
.claude/rules/testing-quality-gate.md
.claude/skills/economy-balance-tuning/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- FarmDailyGoalService (definições/estado/reset/eventos/Capture-Restore; campo Claimed
  órfão — ESTE é o gancho da recompensa);
- DailyGoalProgressedEvent/DailyGoalCompletedEvent (gatilhos prontos);
- PlayerProgressionManager.AddXp (canal único de XP);
- canal de ouro do player (PlayerManager.AddGold — auditar nome exato);
- GameplayHudCanvas + padrão de views WI-23 (QuestTrackerHudView como referência de
  widget compacto);
- HudVisibilityController (widget some junto com o HUD).
Não existe:
- recompensa/Claimed funcional; widget de metas; metas além das 2; possivelmente o hook
  de save (F13 — auditar).
Auditar Fase 0:
- SAVE_LOAD_DAILY_GOAL_DEBT fechado pela F13? (se não: fechar AQUI via provider/hook);
- tabela canônica de metas na direção de farm (4-6 metas, eventos disponíveis p/ cada);
- valores de recompensa (ouro pequeno por meta; XP coerente com curva F42 — propor
  tabela v1 e registrar como tuning humano pendente);
- evento de "regar"/"quebrar recurso"/"falar com NPC" existem? (metas só podem usar
  eventos REAIS — meta sem evento fica fora da v1).
```

## Engineering stories

```text
Como jogador, quero ver as metas do dia no HUD ao acordar, para planejar meu dia de
  fazenda sem abrir menus.
Como jogador, quero ouro e XP ao concluir uma meta, para o checklist valer o desvio.
Como economia do jogo, quero recompensas pequenas e idempotentes por dia, para metas
  não virarem exploit de farm de ouro.
Como save, quero Claimed persistido, para reload não pagar a mesma meta duas vezes.
```

## Escopo

```text
Inclui:
- recompensa por meta concluída (no DailyGoalCompletedEvent, dentro do serviço):
  - tabela única {goalId → goldReward, xpReward} (v1 conservador: 10-25 ouro, 5-15 XP
    por meta — valores finais auditados/registrados como tuning pendente);
  - ouro via canal existente do player; XP via PlayerProgressionManager.AddXp;
  - Claimed = true ao pagar; pagamento SÓ se !Claimed (idempotente — reload no meio do
    dia não re-paga; coberto pelo Capture/Restore existente);
  - toast de recompensa pelo canal WI-23 ("Meta concluída: +15 ouro, +10 XP");
- DailyGoalsHudView (widget compacto no GameplayHudCanvas, padrão views WI-23):
  - lista das metas do dia com check (concluída) e progresso (n/m quando aplicável);
  - alimentado por DailyGoalProgressed/Completed + snapshot GetCurrentGoals();
  - respeita HudVisibilityController; sem input próprio (read-only);
- expansão do catálogo p/ 4-6 metas/dia (SOMENTE metas com evento real existente —
  auditadas na Fase 0; candidatas: regar N plots, colher N cultivos, vender N itens,
  coletar 1 recurso da fazenda, falar com 1 NPC);
  - RequiredProgress > 1 suportado (o serviço já modela; metas atuais usam 1);
- fechamento dos débitos WI-24 do domínio:
  - SAVE_LOAD_DAILY_GOAL_DEBT: se F13 não fechou, ligar Capture/Restore ao fluxo de
    save (provider pattern);
  - DAILY_GOAL_EDITMODE_TESTS: testes do serviço entram AQUI;
- EditMode tests: recompensa idempotente (Completed→paga 1×; reload não re-paga),
  tabela íntegra (toda meta tem recompensa), reset diário limpa Claimed, progresso
  n/m, round-trip de save com Claimed, projection do widget.
```

## Fora de escopo

```text
Não inclui:
- streaks, bônus semanais, recompensas de item raro (futuro; item simples só se E18);
- metas de caverna/combate (domínio farm primeiro);
- UI de histórico/calendário de metas;
- rebalance da economia além da tabela pequena (tuning humano registrado);
- metas dependentes de eventos que não existem (ficam listadas como futuras no report).
```

## Regras de não duplicação

```text
Não criar segundo serviço de metas — estender o FarmDailyGoalService existente.
Não criar segundo canal de XP/ouro — AddXp/AddGold existentes (auditar nomes).
Não criar segundo padrão de widget — view WI-23 (QuestTrackerHudView como referência).
Não duplicar o toast — canal WI-23 existente.
Tabela de recompensa em UM lugar (definição da meta), nunca espalhada por handlers.
```

## Critérios de aceite

### CA-1 Recompensa idempotente

- Meta concluída paga ouro+XP exatamente 1× (Claimed); save no meio do dia + reload não
  re-paga; dia novo reseta Claimed e re-habilita.
- Evidência: EditMode tests (pagar 1×, round-trip com Claimed, reset diário).

### CA-2 Widget no HUD

- O HUD mostra as metas do dia com check/progresso, atualizado por eventos, respeitando
  a visibilidade do HUD; read-only.
- Evidência: EditMode test da projection + cenário humano do lote.

### CA-3 Catálogo 4-6 metas

- O dia oferece 4-6 metas, todas ligadas a eventos REAIS verificados, com
  RequiredProgress correto (n/m funciona).
- Evidência: EditMode test de integridade do catálogo + progresso sintético por evento.

### CA-4 Débitos WI-24 fechados

- SAVE_LOAD_DAILY_GOAL_DEBT fechado (por F13 ou aqui — evidência) e
  DAILY_GOAL_EDITMODE_TESTS fechado pelos testes desta spec; matrizes WI-24 anotadas.
- Evidência: hook de save no diff (ou prova de F13) + suite de testes + nota nas matrizes.

### CA-5 Economia contida

- Total diário máximo de recompensa documentado (soma da tabela) e registrado como
  tuning humano pendente; nenhuma meta paga sem Claimed.
- Evidência: tabela no report + teste de idempotência.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Runtime/
  FarmDailyGoalService.cs        (ESTENDIDO — recompensa/Claimed + catálogo 4-6 metas)
  FarmDailyGoalRewardTable.cs    (NOVO — tabela única {goalId → ouro, XP})
Assets/_Game/Scripts/UI/HUD/Views/
  DailyGoalsHudView.cs           (NOVO — widget compacto, padrão WI-23)
hook de save (provider/SaveManager — SE F13 não fechou; arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/Farm/FarmDailyGoalServiceTests.cs (NOVO)
docs/validation/WAVE_INTEGRATION_24_DAILY_GOAL_MATRIX.md (anotação de débito fechado)
docs/validation/fable_65_spec_daily_goals_closeout_reward_hud_execution_report.md
```

## Contratos

### Data contracts

- `FarmDailyGoalRewardTable`: entradas imutáveis {goalId, goldReward (int), xpReward
  (int)} — toda meta do catálogo TEM entrada (teste de integridade).
- Catálogo de metas: {goalId estável, DisplayName PT-BR, RequiredProgress, evento
  gatilho} — ids novos nunca renomeiam os 2 existentes.

### Runtime contracts

- Pagamento: ponto único no serviço — Completed && !Claimed → AddGold + AddXp + Claimed
  = true + toast; lógica extraída em método puro testável.
- `DailyGoalsHudView`: projection de GetCurrentGoals() + eventos; zero estado próprio
  persistido.

### Event contracts

- Consome: DailyGoalProgressedEvent/DailyGoalCompletedEvent/DayStartedEvent (existentes)
  + eventos gatilho das metas novas (auditados). Adds events: NO (toast pelo canal
  existente). Unsubscribe pareado.

### Save contracts

- FarmDailyGoalsSaveData existente JÁ carrega Claimed — campos novos só se a auditoria
  exigir (aditivos, defaults seguros). Hook Capture/Restore ligado ao save se F13 não
  fez. Sem referências Unity.

### UI contracts

- Widget read-only no GameplayHudCanvas; respeita HudVisibilityController; sem modal,
  sem input; empty state (dia sem metas — não deve ocorrer, mas não quebra).

## Sistemas afetados

```text
FarmDailyGoalService (recompensa + catálogo)
GameplayHudCanvas (widget novo — aditivo)
PlayerProgressionManager / canal de ouro (consumo)
Save (hook Capture/Restore se pendente)
Economia (fluxo pequeno de ouro novo — documentado)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs (estender)
Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalRewardTable.cs (novo)
Assets/_Game/Scripts/UI/HUD/Views/DailyGoalsHudView.cs (novo) + registro no canvas
  controller/binder (diff aditivo)
hook de save (SE necessário — provider novo ou diff mínimo auditado)
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/** (report + anotação nas matrizes WI-24)
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (YAML manual)
Packages/** ; ProjectSettings/**
PlayerProgressionManager internals (consumir AddXp; curva F42 intacta)
Views WI-23 existentes (StatusBars/QuestTracker/etc. — intactas)
FarmPlot e fluxo de crop (gatilhos via eventos; zero mudança no farm core — F66 é dona
  dos débitos de FarmPlot)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Status do save debt (F13?); tabela canônica de metas na direção de farm; eventos reais
disponíveis por meta candidata; nomes exatos AddGold/AddXp; valores v1 da recompensa.

### Fase 1 — Recompensa
RewardTable + pagamento idempotente (Claimed) + toast + testes (1×, reload, reset).

### Fase 2 — Catálogo
4-6 metas com eventos reais + RequiredProgress n/m + testes de integridade/progresso.

### Fase 3 — Widget e fechamento
DailyGoalsHudView + registro no canvas + hook de save (se pendente) + anotação das
matrizes WI-24; csproj; run_strict_validation; execution report (tuning pendente
registrado).
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch11_shipping.
- Can run with: F61, F63, F64, F67.
- Must not run with: F62 (GameplayHudCanvas compartilhado), F66 (arquivos de Farm).
- Shared files/systems that require lock: FarmDailyGoalService, GameplayHudCanvas
  (registro de views), seção de save de daily goals.
- Reason: estende um serviço vivo e a superfície de HUD que F62 também toca.

## Impacto em save/load

```text
Does this change save schema? POSSIBLE (apenas campos aditivos se a auditoria exigir;
  Claimed JÁ existe no DTO)
Does this add a save section? NO (seção existente; hook ligado se F13 não ligou)
Does this require migration? NO (defaults seguros)
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (view e serviço assinam eventos; OnDisable pareado)
```

## Impacto em UI/Unity

```text
Changes UI: YES — widget novo no GameplayHudCanvas (aditivo)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: recompensa dupla (Completed re-disparado / reload no meio do dia).
Mitigação: Claimed como guarda única + persistência existente + testes de idempotência.

Risco: exploit de ouro (metas re-completáveis no mesmo dia).
Mitigação: Completed trava progresso (comportamento atual preservado) + total diário
documentado + tuning humano registrado.

Risco: meta nova sem evento real (meta morta no HUD).
Mitigação: Fase 0 verifica evento por meta; sem evento → fora da v1 (lista futura).

Risco: colisão com F62 no canvas.
Mitigação: Must not run with F62; registro de view aditivo coordenado.

Risco: save debt deixado órfão de novo.
Mitigação: CA-4 obriga fechar OU provar fechado (F13) — sem terceiro adiamento.

Risco: widget poluir o HUD.
Mitigação: compacto, read-only, respeita HudVisibility; cenário humano avalia.
```

## Rollback

```text
Remover a view + tabela de recompensa (metas voltam a checklist sem pagamento) +
reverter extensão do catálogo. DTO com Claimed já existia — saves não quebram.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar save debt/F13, tabela de metas da direção, eventos reais,
        AddGold/AddXp, valores v1.
- [ ] T002 — RewardTable + pagamento idempotente + toast + testes.
- [ ] T003 — Catálogo 4-6 metas (eventos reais) + n/m + testes.
- [ ] T004 — DailyGoalsHudView + registro + hook de save (se pendente) + anotação
        WI-24; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (pagamento idempotente, catálogo, reset diário,
  round-trip com Claimed)
- Requires EditMode tests: YES (fecha também DAILY_GOAL_EDITMODE_TESTS do WI-24)
- Requires PlayMode automated or final human scenario: YES (lote final — dia completo:
  acordar, ver widget, completar metas, receber recompensas, dormir, reset)
- Requires regression test: YES (2 metas originais + Capture/Restore intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano do dia completo com
  recompensas pagas 1× e widget atualizando

## Definition of Done

```text
Recompensa ouro+XP idempotente por meta (Claimed funcional), widget compacto no HUD
(padrão WI-23, read-only), catálogo 4-6 metas com eventos reais, débitos WI-24 do
domínio fechados/provados (save hook + testes), total diário documentado com tuning
humano registrado, builds 0E, run_strict_validation exit 0, execution report criado.
```

## Anti-regressão

```text
As 2 metas originais (ids/comportamento) intactas; ids nunca renomeados.
Capture/Restore existente compatível (Claimed já era campo do DTO).
Views WI-23 existentes intactas (registro aditivo da view nova).
Curva de XP (F42) e canais AddXp/AddGold inalterados (consumo apenas).
Nenhum segundo serviço de metas; tabela de recompensa em ponto único.
Eventos só via GameEventBus; unsubscribe pareado; zero GameObject.Find em runtime.
```
