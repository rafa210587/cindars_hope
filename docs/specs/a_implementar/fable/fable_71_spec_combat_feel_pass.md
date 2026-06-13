# SPEC — Combat Feel Pass (hit-stop, screen shake e evento de supressão de HUD)

> **Spec ID:** `fable_71_spec_combat_feel_pass`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Runtime
> **Domain:** Combat / Feel
> **Parallelizable:** YES
> **Parallel group:** janela LIVRE (só consome eventos existentes — espelha F58)
> **Can run with:** qualquer spec que não toque Combat feedback nem Camera principal
> **Must not run with:** specs que reescrevam HitFlashController/KnockbackController ou o rig da câmera
> **Repo lock scope:** `Combat/Feel/**` (novo), `Combat/Events/**` (evento novo), assinatura na câmera principal
> **Depends on:**
> - F43 (consumidora do `HudSuppressionChangedEvent` — esta spec PROVÊ o evento; F43 espera por ele)
> - GameEventBus (existente — `DamageAppliedEvent`, `PlayerDamagedEvent`, eventos de boss já publicados)
> **Blocks:**
> - `fable_43_spec_endgame_act5_final_bosses_choice` (a luta do Archivist consome o `HudSuppressionChangedEvent` definido aqui)
> **Scope:** camada de "juice" de combate puramente consumidora de eventos: hit-stop curto em golpe pesado/quebra de postura, screen shake leve em boss/ataque carregado, e o contrato `HudSuppressionChangedEvent` que a F43 usa para ocultar/restaurar o HUD do boss final.
> **Out of scope:** números de dano flutuantes (JÁ EXISTEM — ver Estado atual do repo), hit-flash (existe), knockback (existe), partículas, áudio/SFX, animação de arte, rebind, gamepad.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

Decisão 4.2-A★ do Refinamento v3 (`docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`): criar a
**fable_71 "combat feel pass"** como camada de feedback de combate que só consome eventos já
publicados (janela LIVRE, mesmo perfil de risco da F58). O texto original da decisão listava
"números de dano flutuantes (fecha o fantasma da F43)" entre os itens — porém a re-auditoria
de 2026-06-13 confirmou que os números de dano **já existem e já estão wired** em runtime
(ver Estado atual do repo). Portanto este escopo entrega APENAS o que a re-auditoria
confirmou ausente: hit-stop (freeze-frame) curto, screen shake leve e o evento
`HudSuppressionChangedEvent` que a F43 pressupõe mas que ainda não existe no código.

## Problema

O combate dá feedback de impacto incompleto. Hit, flash e knockback existem, mas não há
"peso" tátil em golpes pesados (nenhum hit-stop) nem ênfase em momentos de boss/ataque
carregado (nenhum screen shake). Além disso, a `fable_43` (endgame, luta do Archivist of
Silence) escreve no escopo dela que o HUD é ocultado por fase via um evento
`HudSuppressionChangedEvent` publicado no `GameEventBus` — mas esse evento **não existe** em
nenhum arquivo `.cs`; só aparece em docs/specs. Sem ele, a F43 não tem contrato para suprimir
e restaurar o HUD (incluindo os números de dano que já existem). Esta spec fecha as duas
lacunas de uma vez, sem recriar nada do feedback já presente.

## Objetivo

Ao final desta spec o jogo deve: (1) aplicar um hit-stop curto (40-60 ms via `Time.timeScale`,
com restauração garantida) ao conectar um golpe pesado ou ao quebrar postura; (2) aplicar um
screen shake leve à câmera principal em ataque carregado/golpe de boss; (3) publicar o
contrato `HudSuppressionChangedEvent` no `GameEventBus`, com a fase 0 = tudo visível, para que
a F43 possa ocultar e SEMPRE restaurar o HUD do boss final (os números de dano, que já
existem, são suprimidos/restaurados por esse mesmo evento — não recriados). Tudo isso
consumindo eventos existentes, sem escrever em sistemas de feedback que já estão wired.

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (4.2-A★ — combat feel pass; janela LIVRE)
docs/game_rules/combat_rules.md (modelo de dano; eventos de combate via event bus)
docs/specs/a_implementar/fable/fable_43_spec_endgame_act5_final_bosses_choice.md (consumidora do HudSuppressionChangedEvent — não recriar números de dano)
.claude/skills/game-feel-checklist/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
```

## Estado atual do repo

```text
EXISTE e NÃO RECRIAR (confirmado pela re-auditoria 2026-06-13):
- Números de dano flutuantes — JÁ EXISTEM e JÁ ESTÃO WIRED. Não criar sistema novo:
  - Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs (assina
    GameEventBus.Subscribe<DamageAppliedEvent> e <PlayerDamagedEvent>; séries 14A-FIX);
  - Assets/_Game/Scripts/Combat/FloatingNumberBehavior.cs (comportamento do popup);
  - Assets/_Game/Scripts/Combat/DamagePopupAnchor.cs (âncora de posição do popup).
- Hit-flash — JÁ EXISTE: Assets/_Game/Scripts/Combat/HitFlashController.cs. Não recriar.
- Knockback — JÁ EXISTE: Assets/_Game/Scripts/Combat/KnockbackController.cs. Não recriar.
- GameEventBus (Publish/Subscribe) e os eventos de combate (DamageAppliedEvent,
  PlayerDamagedEvent) — consumir; não criar canal paralelo.

NÃO EXISTE (confirmado ausente — escopo desta spec):
- hit-stop / freeze-frame (nenhum uso de Time.timeScale em runtime hoje);
- screen shake / camera shake (nenhum no projeto);
- HudSuppressionChangedEvent (só citado em docs/specs; sem definição .cs em runtime).

AUDITAR Fase 0:
- de qual evento extrair "golpe pesado / quebra de postura" para disparar o hit-stop
  (DamageAppliedEvent já carrega o suficiente? há flag de heavy/posture break? se não, usar
  um campo já existente do evento ou um limiar de dano — definir na Fase 0, SEM inventar
  evento novo de combate);
- qual é a câmera principal e como obtê-la sem busca global (ref serializada / bootstrap);
- de qual evento de boss extrair "ataque carregado" (telegraph/charged já publicado pela AI
  de inimigo? se não houver evento adequado, screen shake fica limitado ao golpe pesado e a
  parte de boss é DEFERRED — documentar honestamente, nunca inventar evento de boss).
```

## Engineering stories

```text
Como jogador, quero sentir um micro-congelamento (40-60 ms) ao acertar um golpe pesado, para
  que o impacto tenha peso tátil.
Como jogador, quero um leve tremor de tela em ataque de boss/carregado, para enfatizar o perigo.
Como F43 (Archivist of Silence), quero um evento HudSuppressionChangedEvent no GameEventBus
  para ocultar e SEMPRE restaurar o HUD do boss final por fase — sem recriar os números de
  dano, que já existem e já assinam os eventos de dano.
Como sistema de feedback existente, quero NÃO ser duplicado: hit-flash, knockback e números
  de dano permanecem como estão.
```

## Escopo

```text
Inclui:
- CombatHitStopController (Combat/Feel/, NOVO): assina o evento de dano existente; ao detectar
  golpe pesado / quebra de postura (critério definido na Fase 0 a partir do evento existente),
  aplica Time.timeScale = ~0 por 40-60 ms (config serializada com clamp 40-60) e RESTAURA o
  timeScale anterior de forma GARANTIDA (mesmo se reentrante: guarda o valor original 1×; não
  empilha; restauração em coroutine + OnDisable/OnDestroy); usa tempo NÃO-escalado para medir
  a duração; respeita pausa/modal (não congela por cima de uma pausa já ativa);
- CameraShakeController (Combat/Feel/, NOVO): tremor leve da câmera principal (ref serializada
  ou via bootstrap, SEM GameObject.Find/FindObjectOfType) em ataque carregado/golpe de boss;
  amplitude e duração leves, configuráveis, com retorno determinístico à posição/rotação
  original; no-op silencioso se a câmera não estiver wired (log de wiring claro, nunca busca
  global de fallback);
- HudSuppressionChangedEvent (Combat/Events/ ou pasta canônica de eventos, NOVO): struct/record
  simples no GameEventBus com (int fase, string[] widgetsOcultos). Fase 0 = conjunto vazio =
  tudo visível. Esta spec DEFINE e DOCUMENTA o contrato e PUBLICA fase 0 (visível) em
  início/fim; quem consome (HUD do boss final, incluindo os números de dano já existentes) é a
  F43. Restauração total é o estado padrão e deve ser sempre alcançável (fase 0).
- EditMode tests: clamp 40-60 ms do hit-stop; restauração do timeScale após a janela e em
  OnDisable; não-empilhamento de hit-stops simultâneos; no-op de shake sem câmera; shape e
  defaults do HudSuppressionChangedEvent (fase 0 = vazio = tudo visível).
```

## Fora de escopo

```text
- Números de dano flutuantes (JÁ EXISTEM — FloatingDamageNumberDisplayer/FloatingNumberBehavior/
  DamagePopupAnchor — NÃO recriar);
- hit-flash (HitFlashController existe) e knockback (KnockbackController existe);
- assinatura/consumo do HudSuppressionChangedEvent pelo HUD (é da F43);
- partículas, áudio/SFX, vibração, animação de arte;
- novos eventos de combate ou de boss (apenas consumir os existentes);
- rebind/input, gamepad, pausa do relógio (F14).
```

## Regras de não duplicação

```text
Não criar segundo sistema de números de dano / hit-flash / knockback — todos já existem e
estão wired (ver Estado atual do repo). Não criar canal de comunicação fora do GameEventBus.
Não inventar evento novo de combate/boss para alimentar hit-stop/shake — extrair de evento já
publicado (Fase 0); se faltar gatilho de boss, a parte de boss fica DEFERRED, não inventada.
Sem GameObject.Find/FindObjectOfType (ref serializada ou bootstrap para a câmera).
```

## Critérios de aceite

### CA-1 — Hit-stop curto e seguro
- Golpe pesado / quebra de postura provoca hit-stop de 40-60 ms; o `Time.timeScale` é sempre
  restaurado ao valor original após a janela e também em OnDisable/OnDestroy; hit-stops
  simultâneos não empilham nem deixam o jogo congelado.
- Evidência: EditMode tests (clamp, restauração, não-empilhamento) + cenário humano.

### CA-2 — Screen shake leve
- Ataque carregado / golpe de boss produz tremor leve na câmera principal, com retorno à
  posição/rotação original; sem câmera wired = no-op com log de wiring (nunca busca global).
- Evidência: EditMode test do no-op + cenário humano (feel) — boss DEFERRED se Fase 0 não
  achar gatilho de boss, documentado.

### CA-3 — Contrato HudSuppressionChangedEvent
- O evento existe no GameEventBus com (int fase, string[] widgetsOcultos); fase 0 = conjunto
  vazio = tudo visível; números de dano (que já existem) são suprimidos/restaurados por esse
  evento, NÃO recriados. A F43 é a consumidora.
- Evidência: EditMode test do shape/defaults + nota explícita de que F43 consome.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Feel/CombatHitStopController.cs    (NOVO)
Assets/_Game/Scripts/Combat/Feel/CameraShakeController.cs       (NOVO)
Assets/_Game/Scripts/Combat/Events/HudSuppressionChangedEvent.cs (NOVO — ou pasta canônica de eventos confirmada na Fase 0)
Assets/_Game/Tests/EditMode/Combat/CombatFeelTests.cs           (NOVO)
docs/validation/fable_71_spec_combat_feel_pass_execution_report.md
```

## Contratos

### Data contracts
`HudSuppressionChangedEvent`: tipos simples apenas — `int Fase`, `string[] WidgetsOcultos`.
Fase 0 = `WidgetsOcultos` vazio = tudo visível. Sem refs Unity. Sem persistência (transiente).

### Runtime contracts
`CombatHitStopController`: assina o evento de dano existente; método interno
`ApplyHitStop(float ms)` com clamp 40-60 e restauração garantida do `Time.timeScale`.
`CameraShakeController`: `Shake(intensity, durationLeve)` com retorno determinístico; no-op
sem câmera. Ambos consomem eventos existentes — nenhum novo evento de combate/boss criado.

### Event contracts
Adiciona: `HudSuppressionChangedEvent` (publicado por esta spec apenas em fase 0/transições de
ciclo; consumido pela F43). Consome (sem alterar): `DamageAppliedEvent`, `PlayerDamagedEvent`
e o evento de boss/charged identificado na Fase 0. Requer unsubscribe nos OnDisable.

### Save/UI contracts
Save: N/A (tudo transiente). UI: esta spec NÃO altera o HUD; apenas define o evento que a F43
usa. Não recriar números de dano (existentes).

## Sistemas afetados

```text
Combat feedback (camada nova "Feel"), Câmera principal (assinatura de shake), Event bus
(evento novo de supressão de HUD). Números de dano / hit-flash / knockback: NÃO tocados.
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Feel/**           (novos controllers)
Assets/_Game/Scripts/Combat/Events/**         (evento novo — ou pasta canônica de eventos)
Assets/_Game/Tests/EditMode/Combat/**         (testes)
docs/validation/**                            (report + cenário humano)
csproj includes (Assembly-CSharp / Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manuais (ref de câmera via inspector é wiring de cena humano)
Packages/** ; ProjectSettings/**
FloatingDamageNumberDisplayer.cs / FloatingNumberBehavior.cs / DamagePopupAnchor.cs (existem)
HitFlashController.cs / KnockbackController.cs (existem)
HUD do boss / consumo do HudSuppressionChangedEvent (é da F43)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar: gatilho de "golpe pesado/quebra de postura" no evento de dano existente; gatilho de "boss/charged"; como obter a câmera principal sem busca global; pasta canônica de eventos.
### Fase 1 — HudSuppressionChangedEvent (struct simples + fase 0 default) + teste de shape.
### Fase 2 — CombatHitStopController (Time.timeScale, clamp 40-60, restauração garantida, não-empilhamento) + testes.
### Fase 3 — CameraShakeController (shake leve, retorno determinístico, no-op sem câmera) + teste de no-op; parte de boss DEFERRED se Fase 0 não achar gatilho.
### Fase 4 — csproj; run_strict_validation; execution report + cenário humano de feel.
```

## Paralelização

- Parallelizable: YES — janela LIVRE (só consome eventos existentes; espelha o perfil da F58).
- Parallel group: N/A
- Can run with: qualquer spec que não reescreva o feedback de combate existente nem o rig da câmera.
- Must not run with: specs que mexam em HitFlashController/KnockbackController/números de dano ou na câmera principal.
- Shared files/systems that require lock: câmera principal (assinatura), GameEventBus (evento novo).
- Reason: contrato novo de evento + assinatura leve na câmera; sem colisão com lógica de gameplay.

## Impacto em save/load

```text
Does this change save schema? NO.
Does this add a save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO (tudo transiente).
```

## Impacto em eventos

```text
Adds events: YES — HudSuppressionChangedEvent (consumido pela F43; publica fase 0 = visível).
Changes existing events: NO. Requires unsubscribe pattern: YES (controllers em OnDisable).
```

## Impacto em UI/Unity

```text
Changes UI: NO (esta spec não altera o HUD; só define o evento que a F43 consome).
Changes scenes: NO (ref de câmera é wiring humano de inspetor, não edição YAML por agente).
Changes prefabs: NO | Changes ScriptableObjects/assets: NO.
Requires Play Mode final validation: YES (feel de hit-stop/shake).
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
```

## Riscos técnicos

```text
Risco: hit-stop não restaurar Time.timeScale → jogo congelado permanente.
  Mitigação: guarda valor original 1×, não empilha, mede com tempo NÃO-escalado, restaura em
  coroutine + OnDisable/OnDestroy; teste dedicado de restauração.
Risco: hit-stop sobrepor uma pausa/modal já ativa (timeScale=0) e "comer" a pausa.
  Mitigação: não aplicar hit-stop se já houver pausa ativa; restaurar para o valor capturado.
Risco: screen shake sem câmera wired → NullRef ou busca global proibida.
  Mitigação: ref serializada/bootstrap; ausência = no-op com log de wiring claro.
Risco: gatilho de boss/charged inexistente → tentação de inventar evento.
  Mitigação: parte de boss DEFERRED e documentada; nunca criar evento novo de combate/boss.
Risco: recriar números de dano por engano (texto antigo da decisão).
  Mitigação: Estado atual do repo fixa que eles JÁ EXISTEM; anti-regressão proíbe recriação.
```

## Rollback

```text
Remover os dois controllers e o evento desliga a camada de feel; números de dano, hit-flash e
knockback (preexistentes) permanecem intactos. F43 perde a dependência do evento (volta a ser
pendente até a fable_71 entrar). Nenhum save afetado.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: gatilho de golpe pesado/quebra de postura (evento existente); gatilho de boss/charged; câmera principal sem busca global; pasta canônica de eventos.
- [ ] T002 — HudSuppressionChangedEvent (struct simples, fase 0 default = visível) + teste de shape/defaults.
- [ ] T003 — CombatHitStopController (Time.timeScale 40-60 ms, restauração garantida, não-empilhamento) + testes.
- [ ] T004 — CameraShakeController (shake leve, retorno determinístico, no-op sem câmera) + teste; boss DEFERRED se sem gatilho.
- [ ] T005 — csproj; run_strict_validation; execution report + cenário humano de feel.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed runtime code: YES | Changed deterministic logic: YES (clamp/restauração/shape do evento)
- Changed Unity scene/prefab/asset wiring: NO (ref de câmera é wiring humano de inspetor)
- Automated tests added/updated: YES (EditMode — clamp, restauração, não-empilhamento, no-op, shape do evento)
- Manual Play Mode scenario: YES (feel de hit-stop/shake) — docs/validation/playmode/fable_71_human_test_scenario.md
- Justification if no automated tests: N/A
- Residual risk: feel subjetivo (amplitude/duração) só validável em Play Mode; parte de boss
  DEFERRED se Fase 0 não achar gatilho — documentar honestamente.

## Definition of Done

```text
Hit-stop 40-60 ms com restauração garantida; screen shake leve com retorno determinístico;
HudSuppressionChangedEvent definido no GameEventBus (fase 0 = visível) para a F43 consumir;
números de dano / hit-flash / knockback NÃO recriados; builds 0E; report com Spec Compliance
Matrix + cenário humano; sem claim ACCEPTED.
```

## Anti-regressão

```text
Não recriar números de dano (FloatingDamageNumberDisplayer/FloatingNumberBehavior/
DamagePopupAnchor), hit-flash (HitFlashController) nem knockback (KnockbackController) — todos
preexistentes. Time.timeScale sempre restaurado (nunca deixar o jogo congelado). Câmera sem
busca global. Nenhum evento de combate/boss novo além do HudSuppressionChangedEvent. HUD não
alterado por esta spec (consumo é da F43).
```
