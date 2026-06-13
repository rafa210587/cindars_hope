# SPEC — Débitos: Limpeza de Débitos de Código Declarados (slice mode, TODOs, evento órfão)

> **Spec ID:** `fable_66_spec_code_debt_cleanup_slice_mode`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Runtime / Refactor / Debt
> **Domain:** Farm / Combat / Cave / Player
> **Parallelizable:** NO (toca arquivos de 4 domínios — superfície larga)
> **Parallel group:** N/A
> **Can run with:** F67 (docs-only)
> **Must not run with:** F61 (smoke de build exige runtime estável), F62/F63/F64/F65 (arquivos compartilhados: HUD/Farm/CaveDeathResolver/Player)
> **Repo lock scope:** FarmPlot, Farm/Integration, CaveDeathResolver, PlayerBlockController, PlayerCombatController, Core/Events (PlayerHitEvent), Editor/Validation (DebugLoadoutProvisioner)
> **Depends on:**
> - F32 (E18 — catálogo de itens; o fluxo final de ferramentas/itens substitui o slice mode)
> - REFINAMENTO DO KIT INICIAL PENDENTE — o slice mode SÓ sai depois que o kit inicial
>   canônico existir (GATE EXPLÍCITO, ver Escopo/Riscos)
> **Blocks:** fechamento honesto da dívida técnica do MVP; F61 smoke confiável
> **Scope:** fechar cada débito declarado no código (com teste de caracterização antes), aposentar PlayerHitEvent, resolver TODOs do CaveDeathResolver, documentar STAMINA_BLOCK_DEBT.
> **Out of scope:** refactors não declarados, débitos de skills (ActiveSkillExecutionController — onda própria), criar o kit inicial (refinamento), novos sistemas.

required_adrs: [ADR-0007-event-bus-gameplay-communication.md, ADR-0005-cave-stable-run-and-replay.md]
required_game_rules: [farm_rules.md, combat_rules.md, cave_rules.md, death_anya_corpse_rules.md]

---

# /speckit.specify

## Contexto

O código carrega débitos DECLARADOS com marcador, cada um verificado no repo:

1. `FarmPlot.cs:54-56` — `_temporarySequentialSliceMode` + `_temporarySequentialSeedId`,
   marcado "TODO_INTEGRATION_NOT_FINAL: WAVE_INTEGRATION_05 smoke hook. Remove when
   final farm tool/equipment flow covers the whole crop loop"; há ainda o bypass de
   stamina em `FarmPlot.cs:225` ("Stamina cost is always bypassed here for
   skill-triggered water").
2. `Farm/Integration/FarmResourceInteractable.cs` — adapter smoke inteiro marcado
   TODO_INTEGRATION_NOT_FINAL (linhas 7/36/52: fallback feedback-only sem inventário,
   AddItem false deixa recurso Available), gerado por `CreateMvpFarmScene.cs:1369`.
3. `Editor/Validation/DebugLoadoutProvisioner.cs:11` — provisioner debug
   TODO_INTEGRATION_NOT_FINAL para smoke das WAVEs 04/05/06.
4. `Cave/Death/CaveDeathResolver.cs:199-227` — QUATRO TODOs: ReplaceActiveCorpse vazio
   ("will be integrated with CorpseRecoveryManager"), GetCurrentGameDay() retorna 1
   fixo, GetCurrentGameTime() retorna 0 fixo ("Get from game time manager"),
   GetCurrentLayoutHash() retorna vazio ("Get from cave runtime").
5. `Core/Events/PlayerHitEvent.cs` — evento SEM NENHUM publisher (verificado: zero
   `new PlayerHitEvent` no repo); PlayerCombatController assina um evento que nunca
   chega; o report da fable_27 já apontou a aposentadoria.
6. `PlayerBlockController.cs:145` — `// STAMINA_BLOCK_DEBT`: guard silencioso
   `if (_staminaManager == null) return;` — block sem stamina wired drena nada.

Regra de método vinculante: TESTE DE CARACTERIZAÇÃO ANTES de mexer — cada débito ganha
um teste que pinna o comportamento atual relevante; só então o débito fecha. E o gate
do slice mode: `_temporarySequentialSliceMode` SÓ é removido depois que o kit inicial
canônico (refinamento pendente) definir como o jogador obtém as ferramentas/sementes —
remover antes quebraria o único caminho jogável do loop de crop.

## Problema

Débito declarado e não fechado vira mentira institucional: o marcador diz "temporário"
há N waves. Concretamente: o resolver de morte grava dia 1/hora 0 em todo corpo
(corrompe dados de morte), corpos antigos nunca são substituídos pelo caminho do
resolver, um evento fantasma sugere um pipeline de dano que não existe (e engana
qualquer spec de combate futura), e o slice mode mantém dois caminhos de farm
divergentes. Cada um é pequeno; juntos minam a confiança no código.

## Objetivo

Ao final desta spec: (1) CaveDeathResolver com dados reais — dia/hora do gerenciador de
tempo real, substituição de corpse antigo delegada ao CorpseRecoveryManager
(SetActiveCorpse já implementa replace — ligar e remover o método morto), layout hash
real ou remoção documentada do campo; (2) PlayerHitEvent aposentado (evento + assinatura
mortos removidos; pipeline real de dano documentado onde a assinatura estava);
(3) STAMINA_BLOCK_DEBT fechado: wiring garantido pelo bootstrap OU erro de wiring
logado alto (nunca guard silencioso); (4) slice mode de FarmPlot e adapter
FarmResourceInteractable: REMOVIDOS se o gate do kit inicial estiver satisfeito na
execução; senão, débito re-registrado com dono/condição explícita (sem fingir
fechamento); (5) DebugLoadoutProvisioner anotado como ferramenta editor permanente OU
removido se órfão — decisão auditada; (6) cada mudança precedida de teste de
caracterização.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs (slice mode + bypass de stamina)
Assets/_Game/Scripts/Farm/Integration/FarmResourceInteractable.cs (adapter smoke)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (quem gera os adapters)
Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs
Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs (4 TODOs)
Assets/_Game/Scripts/Player/Death/CorpseRecoveryManager.cs (SetActiveCorpse já faz replace)
Assets/_Game/Scripts/Core/Events/PlayerHitEvent.cs + Player/PlayerCombatController.cs
  (assinatura órfã) + Combat/PlayerDamageReceiver.cs (pipeline real de dano)
Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs (STAMINA_BLOCK_DEBT)
docs/validation/fable_27_spec_perfect_block_posture_runtime_execution_report.md
  (aposentadoria do PlayerHitEvent — se o nome divergir, localizar o report da F27)
gerenciador de tempo real (GameTimeManager/WorldTime — nome auditado na Fase 0)
CaveRunManager (nível da morte real)
.claude/rules/cave-stable-run.md (resolver é superfície de cave)
.claude/rules/testing-quality-gate.md (regra de bugfix/regressão)
.claude/skills/editmode-test-authoring/SKILL.md
.claude/skills/non-regression-review/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CorpseRecoveryManager.SetActiveCorpse (replace de corpse JÁ implementado — o TODO do
  resolver é só ligação);
- gerenciador de tempo/dia (DayStartedEvent existe; owner do dia/hora auditado);
- CaveRunManager (CurrentCaveLevel já consumido pelo resolver);
- PlayerDamageReceiver (pipeline real de dano do player);
- pipeline de stamina (StaminaManager.TrySpendStamina — usado no próprio block).
Não existe:
- publisher de PlayerHitEvent (evento morto);
- dia/hora/layout reais no resolver; substituição de corpse via resolver;
- kit inicial canônico (refinamento pendente — GATE do slice mode).
Auditar Fase 0:
- GATE: kit inicial canônico existe? (refinamento + E18) — decide remoção × re-registro
  do slice mode/adapter;
- owner real de dia/hora (API pública consumível pelo resolver);
- GetCurrentLayoutHash: algum consumidor real do campo? (remover campo × ligar de fato);
- DebugLoadoutProvisioner: alguém usa? (menu/validador) — permanente anotado × remoção;
- wiring real do _staminaManager do block nas cenas geradas (o débito é de wiring).
```

## Engineering stories

```text
Como dados de morte, quero dia/hora reais no corpo, para corpse recovery e qualquer
  lógica futura de expiração não nascerem corrompidas.
Como spec futura de combate, quero o pipeline de dano sem eventos fantasmas, para não
  construir em cima de um caminho que nunca dispara.
Como loop de farm, quero UM caminho de interação (o final), para o slice mode não
  divergir do fluxo real para sempre.
Como auditor, quero cada débito fechado com teste de caracterização, para a limpeza não
  introduzir regressão silenciosa.
```

## Escopo

```text
Inclui (ordem: caracterizar → fechar → testar):
- CaveDeathResolver:
  - GetCurrentGameDay/GetCurrentGameTime → owner real de tempo (API auditada);
  - ReplaceActiveCorpse morto → REMOVER e garantir que o caminho usa
    CorpseRecoveryManager.SetActiveCorpse (que já faz replace + evento);
  - GetCurrentLayoutHash → ligar à fonte real do cave runtime OU remover o campo com
    justificativa (decisão auditada — sem stub eterno);
  - testes: corpo criado carrega dia/hora reais; morte com corpo ativo anterior publica
    CorpseReplacedEvent (caracterização do replace);
- PlayerHitEvent:
  - remover o evento, a assinatura órfã no PlayerCombatController e atualizar o
    comentário do PlayerDamageReceiver (pipeline real documentado);
  - caracterização: dano ao player continua fluindo pelo caminho real (teste existente
    ou novo em PlayerDamageReceiver);
- STAMINA_BLOCK_DEBT (PlayerBlockController:145):
  - trocar guard silencioso por erro de wiring logado alto (padrão do projeto: cena,
    objeto, campo faltante) + garantir wiring no gerador de cena/bootstrap;
  - caracterização: block COM stamina wired drena/cancela como hoje (pinnar);
- slice mode (GATED — só com kit inicial canônico + E18 na execução):
  - remover _temporarySequentialSliceMode/_temporarySequentialSeedId e os bypasses
    (incluindo o de stamina da linha 225 SE o fluxo final cobrir) de FarmPlot;
  - remover/substituir FarmResourceInteractable pelo fluxo final + diff no gerador
    CreateMvpFarmScene + regeneração com evidência;
  - SE o gate falhar: re-registrar o débito com dono/condição em
    docs (report + marcador atualizado com a referência) — NUNCA fingir fechamento;
- DebugLoadoutProvisioner: decisão auditada (ferramenta editor permanente → renomear
  marcador p/ comentário de propósito; órfão → remover);
- teste de caracterização ANTES de cada mudança (regra de método — diff de teste
  precede diff de produção no histórico de commits da spec).
```

## Fora de escopo

```text
Não inclui:
- débitos de Skills (ActiveSkillExecutionController/FeedbackOnly/FarmCropSkillEffect —
  TODO_INTEGRATION_NOT_FINAL próprios, onda de skills);
- criar o kit inicial (refinamento humano pendente);
- refactors oportunistas não declarados ("já que estou aqui");
- mudanças de regra de stamina/block/farm (comportamento preservado por caracterização);
- débito FindObjectOfType dos *RuntimeBootstrap (decisão humana pendente — rule
  unity-architecture).
```

## Regras de não duplicação

```text
Não reimplementar replace de corpse — CorpseRecoveryManager.SetActiveCorpse é o caminho.
Não criar segundo owner de tempo — consumir o existente (auditado).
Não criar pipeline novo de dano — remover o fantasma; o real (PlayerDamageReceiver) fica.
Não criar segundo fluxo de farm — remover o temporário SÓ quando o final cobrir (gate).
```

## Critérios de aceite

### CA-1 Resolver com dados reais

- Corpos criados registram dia/hora do owner real de tempo; ReplaceActiveCorpse morto
  removido; morte com corpo anterior ativo publica CorpseReplacedEvent; layout hash
  ligado ou removido com justificativa.
- Evidência: testes de caracterização + diff sem os 4 TODOs.

### CA-2 PlayerHitEvent aposentado

- Zero referências a PlayerHitEvent no repo; assinatura órfã removida; comentário do
  pipeline real atualizado; dano ao player segue funcionando (teste).
- Evidência: grep vazio + teste do pipeline real.

### CA-3 Block sem guard silencioso

- _staminaManager ausente gera erro de wiring logado alto (cena/objeto/campo); wiring
  garantido no gerador/bootstrap; comportamento com stamina wired inalterado (pinnado).
- Evidência: teste de caracterização + diff do guard + wiring no gerador.

### CA-4 Slice mode resolvido honestamente

- COM gate satisfeito: slice mode/adapter removidos, fluxo final cobre o loop, cena
  regenerada com evidência. SEM gate: débito re-registrado com dono/condição explícita
  no report e marcador atualizado — declarado como NÃO fechado.
- Evidência: diff + evidência de regeneração OU seção de re-registro no report.

### CA-5 Caracterização antes

- Cada débito fechado tem teste de caracterização criado ANTES da mudança (visível no
  histórico de commits da spec).
- Evidência: ordem dos commits + suite de testes.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs   (TODOs fechados)
Assets/_Game/Scripts/Core/Events/PlayerHitEvent.cs     (REMOVIDO)
Assets/_Game/Scripts/Player/PlayerCombatController.cs  (assinatura órfã removida)
Assets/_Game/Scripts/Combat/PlayerDamageReceiver.cs    (comentário do pipeline real)
Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs (guard → wiring error)
Assets/_Game/Scripts/Farm/FarmPlot.cs                  (slice mode removido SE gate OK)
Assets/_Game/Scripts/Farm/Integration/FarmResourceInteractable.cs (removido/substituído SE gate OK)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (diff correspondente)
Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs (anotado ou removido)
Assets/_Game/Tests/EditMode/{Cave,Combat,Player,Farm}/ (testes de caracterização)
docs/validation/fable_66_spec_code_debt_cleanup_slice_mode_execution_report.md
```

## Contratos

### Data contracts

- Corpse passa a carregar GameDay/GameTime REAIS (tipos já existentes no modelo — sem
  mudança de shape; só a fonte muda de stub para real).

### Runtime contracts

- Resolver consome owner de tempo e CorpseRecoveryManager via referências injetadas
  (padrão atual do resolver — sem Find).
- Block: wiring ausente = erro logado alto (nunca silencioso) — regra
  unity-architecture (#1, log de wiring).

### Event contracts

- Removes events: PlayerHitEvent (morto — zero publishers). CorpseReplacedEvent passa a
  ser alcançável pelo caminho do resolver. Nenhum evento novo.

### Save contracts

- Nenhuma mudança de schema. CorpseSaveData inalterado (dia/hora já eram campos — agora
  com valores reais; saves antigos com dia 1/hora 0 continuam válidos).

### UI contracts

- N/A (zero UI nesta spec).

## Sistemas afetados

```text
Cave death (resolver — dados reais)
Combat/Player (evento removido; pipeline documentado; block wiring)
Farm (slice mode/adapter — GATED)
Editor tooling (provisioner; gerador da FarmScene)
```

## Arquivos permitidos

```text
Os listados na Arquitetura alvo (e SOMENTE eles) +
Assets/_Game/Tests/EditMode/** ; docs/validation/** ; csproj includes
owner de tempo: APENAS leitura/consumo (zero mudança no owner)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (YAML manual — cena SÓ via gerador + regeneração)
Packages/** ; ProjectSettings/**
Skills/Runtime/Effects/** (débitos da onda de skills — fora)
CorpseRecoveryManager (consumir SetActiveCorpse; zero mudança interna)
StaminaManager / regras de stamina (consumo apenas)
Qualquer arquivo sem débito declarado (sem refactor oportunista)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria e gates
GATE do kit inicial (decide CA-4); owner de tempo; consumidores de layout hash; uso do
provisioner; wiring real do block nas cenas geradas. Relatório de decisão por débito.

### Fase 1 — Caracterização
Testes pinnando: replace de corpse, dia/hora no corpo (atual: stub), dano ao player
pipeline real, block com stamina. Commit de testes ANTES.

### Fase 2 — Fechamentos sem gate
Resolver (tempo real, replace via manager, layout hash), PlayerHitEvent removido,
block guard → wiring error + gerador, provisioner decidido.

### Fase 3 — Slice mode (gated) e fechamento
SE gate OK: remoção do slice mode/adapter + gerador + regeneração com evidência;
SENÃO: re-registro com dono/condição. csproj; run_strict_validation; execution report
com a tabela débito → decisão → evidência.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F67 (docs-only).
- Must not run with: F61, F62, F63, F64, F65 (arquivos/superfícies compartilhados em
  4 domínios).
- Shared files/systems that require lock: FarmPlot, Farm/Integration, CaveDeathResolver,
  PlayerBlockController, PlayerCombatController, Core/Events, gerador FarmScene.
- Reason: spec horizontal — toca pontos quentes de Farm/Cave/Combat/Player que as
  demais specs do batch também tocam.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (corpos antigos com dia 1/hora 0 continuam válidos)
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: REMOVES PlayerHitEvent (zero publishers — evento morto;
  assinatura órfã removida junto)
Requires unsubscribe pattern: YES (assinatura removida de forma pareada)
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: via gerador (wiring do block; adapters do farm SE gate OK) — regeneração
  com evidência
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final — loop de crop, block, morte na
  caverna com corpo substituído)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: remover o slice mode sem o fluxo final cobrir (quebra o único caminho jogável).
Mitigação: GATE explícito (kit inicial + E18) decidido na Fase 0; sem gate → re-registro
honesto, nunca remoção.

Risco: regressão silenciosa ao fechar débito (comportamento dependia do stub).
Mitigação: teste de caracterização ANTES de cada mudança (CA-5); non-regression review.

Risco: PlayerHitEvent ter publisher por reflexão/cena que o grep não pegou.
Mitigação: Fase 0 confirma (grep + cenas geradas); compile + testes pegam assinatura
quebrada; rollback trivial.

Risco: owner de tempo errado (dois relógios no projeto).
Mitigação: auditoria Fase 0 identifica o canônico (o mesmo que publica DayStarted).

Risco: resolver é superfície de cave (estabilidade FASE9F).
Mitigação: rule cave-stable-run lida; mudanças não tocam seed/layout/IDs — só dados de
corpo; skill cave-stable-run-guard no review.

Risco: spec horizontal colidir com o batch inteiro.
Mitigação: Parallelizable NO; rodar isolada (antes ou depois das vizinhas).
```

## Rollback

```text
Cada débito é um commit isolado (caracterização + fechamento) — rollback por débito via
git revert sem afetar os demais. Slice mode gated nunca é removido sem o fluxo final
provado. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: gates e decisões por débito (kit inicial?, owner de tempo, layout
        hash, provisioner, wiring do block) — tabela no report.
- [ ] T002 — Testes de caracterização (replace, dia/hora, dano real, block) — commit
        ANTES dos fechamentos.
- [ ] T003 — CaveDeathResolver: tempo real + replace via manager + layout hash
        decidido + testes.
- [ ] T004 — PlayerHitEvent removido + comentário do pipeline + teste; block guard →
        wiring error + gerador; provisioner decidido.
- [ ] T005 — Slice mode/adapter: remoção gated com regeneração OU re-registro honesto;
        csproj; run_strict_validation; execution report (tabela débito→decisão→evidência).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (dados do corpo, replace, guard de block, fluxo de
  farm se gate OK)
- Requires EditMode tests: YES (caracterização ANTES + regressão depois — regra de
  bugfix do testing-quality-gate)
- Requires PlayMode automated or final human scenario: YES (lote final — crop loop,
  block drena stamina, morte dupla na caverna substitui corpo)
- Requires regression test: YES (núcleo da spec — todo débito fechado tem caracterização)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: suite de caracterização verde antes/depois +
  cenário humano dos 3 fluxos tocados

## Definition of Done

```text
Tabela débito → decisão → evidência completa no report; resolver sem TODOs (tempo real,
replace ligado, layout hash decidido); PlayerHitEvent extinto com pipeline documentado;
block sem guard silencioso (wiring garantido); slice mode removido com regeneração OU
re-registrado com dono/condição; provisioner decidido; caracterização precede cada
mudança; builds 0E; run_strict_validation exit 0.
```

## Anti-regressão

```text
Comportamento de farm/block/morte preservado byte-a-byte exceto os stubs fechados
(provado por caracterização).
Cave stable run intacto (zero mudança de seed/layout/IDs — rule FASE9F).
CorpseRecoveryManager sem mudança interna; CorpseSaveData sem mudança de shape.
Saves antigos (corpos com dia 1/hora 0) continuam carregando.
Nenhum refactor fora dos arquivos com débito declarado.
Eventos só via GameEventBus; zero GameObject.Find novo; zero guard silencioso novo.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. GATE DO KIT INICIAL RESOLVIDO (2.1): o kit canônico foi aprovado (ver fable_63 emenda-D).
   A remoção do slice mode está DESTRAVADA — condicionada apenas à implementação do kit.
```
