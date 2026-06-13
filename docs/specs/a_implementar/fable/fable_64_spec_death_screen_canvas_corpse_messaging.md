# SPEC — UI: Tela de Morte Canvas + Mensagem de Corpse Recovery

> **Spec ID:** `fable_64_spec_death_screen_canvas_corpse_messaging`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P2
> **Type:** Runtime / UI / Integration
> **Domain:** UI / Death
> **Parallelizable:** CONDITIONAL (cadeia UI canvas — lock do padrão F14)
> **Parallel group:** fable_batch11_shipping
> **Can run with:** F61, F62, F63, F65, F67
> **Must not run with:** F14 (RuntimeUiBuilder/cadeia de telas canvas — mesma superfície), F66 (toca CaveDeathResolver — coordenar)
> **Repo lock scope:** UI/Death/**, RuntimeUiBuilder (consumo F14), fluxo DeathScreenOpened/Closed
> **Depends on:**
> - F14 (E44 — RuntimeUiBuilder/padrão canvas; CONDICIONAL: se E44 pendente, usar o
>   padrão canvas já existente no repo — ModalBase/views WI-23 — e documentar)
> - Fluxo death/corpse legado (existente — CorpseRecoveryManager/CaveDeathResolver/
>   AnyaRespawnService/DeathSystemBootstrap)
> **Blocks:** aceitação do loop de morte/recuperação no MVP (mensagem ao jogador)
> **Scope:** portar DeathScreenController de IMGUI para Canvas exibindo causa/local, itens deixados no corpo + nível da caverna, instrução de recuperação e ações reais.
> **Out of scope:** mudar regras de morte/penalidade/corpse (só EXIBIR), respawn novo, animações/arte final, segunda opção de respawn (futuro), CorpseRecoveryModal (intacto).

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [death_anya_corpse_rules.md, ui_modal_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

O estado real verificado: `UI/Death/DeathScreenController.cs` é IMGUI (OnGUI +
GUI.Window 901) com o texto "You died." e dois botões — "Respawn at Anya's Fountain" e
"Return to Town" — que fazem AMBOS a mesma coisa: `Dismiss()` (publicar
DeathScreenClosedEvent). A tela não diz o que o jogador perdeu, onde, nem como recuperar
— numa morte de caverna, a informação mais importante do jogo naquele momento.

A ironia: os DADOS JÁ EXISTEM. CaveDeathResolver publica CorpseCreatedEvent e
CavePlayerDeathResolvedEvent {CorpseId, CaveRunId, CaveLevel} e calcula XpLost;
CorpseRecoveryManager mantém ActiveCorpse (Corpse: CorpseId, GoldAmount, lista de
CorpseItem, SceneName, Status) com eventos de replaced/recovered/partially-recovered.
PlayerDiedEvent carrega SceneName. Ninguém entrega isso ao jogador.

O comentário do próprio arquivo declara o débito: "Uses OnGUI as functional fallback;
replace with Canvas prefab for polish." A F14 (E44) define o padrão canvas
(RuntimeUiBuilder); WI-23 entregou o GameplayHudCanvas com views canvas. O respawn real
permanece em DeathSystemBootstrap/AnyaRespawnService — esta spec NÃO muda regra de
morte, só a SUPERFÍCIE de comunicação.

## Problema

Jogador morre na caverna → tela diz "You died." e oferece dois botões idênticos. Ele não
sabe: que itens/ouro ficaram num corpo, em que nível da caverna o corpo está, que dá
para recuperar voltando lá, nem qual a diferença entre os botões (não há). Isso quebra a
proposta central do loop de risco da caverna (morte recuperável) por pura falta de
mensagem — e IMGUI está fora do padrão de UI do projeto (canvas, navegação por teclado,
ModalManager).

## Objetivo

Ao final desta spec, a tela de morte é Canvas (padrão F14/E44), 100% navegável por
teclado, exibindo: causa/local da morte (overworld × caverna + nível), resumo do corpo
(N itens + X ouro deixados, nível da caverna do corpo), instrução de recuperação
("volte ao nível N para recuperar"), XP perdido quando houver, e ações REAIS: Respawn na
Fonte (caminho existente do DeathSystemBootstrap) e slot de ação futura desabilitado com
rótulo honesto. O IMGUI morre; eventos DeathScreenOpened/Closed preservados (consumidores
intactos).

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/UI/Death/DeathScreenController.cs (IMGUI atual — substituir)
Assets/_Game/Scripts/UI/Death/CorpseRecoveryUIController.cs + CorpseRecoveryModal.cs
  (superfícies vizinhas — NÃO duplicar)
Assets/_Game/Scripts/Player/Death/CorpseRecoveryManager.cs (ActiveCorpse/Corpse/CorpseItem)
Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs (eventos publicados; XpLost)
Assets/_Game/Scripts/Core/Events/** (PlayerDiedEvent, CavePlayerDefeatedEvent,
  CorpseCreatedEvent, CavePlayerDeathResolvedEvent, DeathScreenOpened/ClosedEvent)
docs/game_rules/death_anya_corpse_rules.md (regras canônicas — exibir, não mudar)
docs/specs/a_implementar/fable/fable_14_spec_ui_canvas_screens_integration_runtime.md
  (padrão RuntimeUiBuilder E44)
.claude/rules/testing-quality-gate.md
.claude/skills/ui-modal-stack/SKILL.md
.claude/skills/event-bus-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- fluxo de morte real: DeathSystemBootstrap/AnyaRespawnService (respawn),
  CaveDeathResolver (corpo/penalidade), CorpseRecoveryManager (estado do corpo);
- eventos: PlayerDiedEvent{SceneName}, CavePlayerDefeatedEvent{CaveLevel},
  CorpseCreatedEvent{CorpseId,SceneName}, CavePlayerDeathResolvedEvent{CorpseId,
  CaveRunId,CaveLevel}, DeathScreenOpenedEvent/DeathScreenClosedEvent;
- CorpseRecoveryModal/CorpseRecoveryUIController (recuperação NO corpo — outra tela);
- padrão canvas (E44 RuntimeUiBuilder; fallback: ModalBase/views WI-23);
- ModalManager (input bloqueado com tela aberta).
Não existe:
- tela de morte canvas; mensagem do que foi perdido/onde/como recuperar; distinção
  real entre as ações; exibição de XP perdido.
Auditar Fase 0:
- quem CONSOME DeathScreenOpened/ClosedEvent (contrato preservado — mapear);
- o que os botões DEVERIAM fazer: caminho real de respawn (DeathSystemBootstrap/
  AnyaRespawnService — qual API/evento dispara o respawn de fato);
- snapshot do corpo no momento da morte (ActiveCorpse via manager × payload dos
  eventos — escolher fonte única; manager é autoritativo);
- E44 entregue? (define o builder; senão padrão canvas WI-23 + débito documentado).
```

## Engineering stories

```text
Como jogador morto na caverna, quero ver o que deixei para trás e em que nível, para
  decidir voltar a buscar.
Como jogador morto, quero instrução explícita de como recuperar o corpo, para a
  mecânica de corpse recovery existir de fato na minha cabeça.
Como jogador, quero que o botão de respawn faça o que diz, para a tela não mentir.
Como fluxo de morte existente, quero a tela só EXIBINDO meus dados e eventos, para
  nenhuma regra de morte mudar de lugar.
```

## Escopo

```text
Inclui:
- DeathScreenCanvasController (substitui o IMGUI; mesmo gatilho por eventos):
  - construção canvas programática (RuntimeUiBuilder E44; fallback padrão WI-23);
  - assina PlayerDiedEvent/CavePlayerDefeatedEvent (gatilhos atuais preservados);
  - publica DeathScreenOpenedEvent/DeathScreenClosedEvent (contrato preservado);
  - navegável por teclado (foco/Enter/Esc NÃO fecha — morte exige escolha; auditar
    regra death_anya_corpse_rules);
- DeathScreenViewModel (projection pura testável) montando a partir dos dados reais:
  - causa/local: overworld {SceneName} × caverna {CaveLevel};
  - corpo: N itens + ouro do ActiveCorpse (CorpseRecoveryManager autoritativo) +
    nível da caverna do corpo (CavePlayerDeathResolvedEvent/Corpse);
  - instrução: "Seus itens estão num corpo no nível N — volte para recuperá-los";
  - XP perdido quando o resolver reportar;
  - estados vazios explícitos (morreu sem nada a perder → mensagem coerente);
- ações reais:
  - "Respawn na Fonte" → dispara o caminho REAL de respawn existente (API/evento do
    DeathSystemBootstrap/AnyaRespawnService auditado na Fase 0) + fecha;
  - slot futuro desabilitado com rótulo honesto (sem segundo botão mentiroso);
- remoção do OnGUI/GUI.Window do DeathScreenController (IMGUI morre nesta tela);
- EditMode tests: projection (overworld × caverna × com/sem corpo × com/sem XP perdido),
  estados vazios, contrato de eventos (abre 1× por morte; fecha publica Closed),
  ação de respawn invoca o caminho auditado (mock/abstração).
```

## Fora de escopo

```text
Não inclui:
- mudar QUALQUER regra de morte/penalidade/corpse (death_anya_corpse_rules intactas);
- CorpseRecoveryModal/recuperação no corpo (tela vizinha intacta);
- segunda opção real de respawn (slot futuro desabilitado);
- arte final/animações (canvas programático);
- save/load (nada novo persiste — a tela é projeção de estado existente).
```

## Regras de não duplicação

```text
Não criar segundo caminho de respawn — disparar o existente
(DeathSystemBootstrap/AnyaRespawnService), nunca reimplementar.
Não duplicar a CorpseRecoveryModal — tela de morte INFORMA; recuperação acontece no
corpo (fluxo existente).
Fonte única do estado do corpo: CorpseRecoveryManager (não re-derivar de eventos
soltos quando o manager está disponível).
Não criar segundo padrão de canvas — RuntimeUiBuilder (E44) ou padrão WI-23 documentado.
```

## Critérios de aceite

### CA-1 Tela canvas substitui IMGUI

- Nenhum OnGUI/GUI.Window na tela de morte; tela canvas navegável por teclado abre nos
  mesmos gatilhos (PlayerDiedEvent/CavePlayerDefeatedEvent) e preserva
  DeathScreenOpened/ClosedEvent.
- Evidência: diff (OnGUI removido) + EditMode tests do contrato de eventos.

### CA-2 Mensagem completa da morte de caverna

- Morte na caverna exibe: nível da morte, N itens + ouro deixados no corpo, nível do
  corpo e instrução de recuperação; XP perdido quando houver.
- Evidência: EditMode tests da projection com corpo sintético + cenário humano.

### CA-3 Morte overworld coerente

- Morte fora da caverna exibe causa/local (SceneName) e estado vazio coerente quando
  não há corpo/perda.
- Evidência: EditMode tests (overworld, sem perda).

### CA-4 Ações honestas

- "Respawn na Fonte" dispara o caminho real de respawn (auditado) e fecha a tela; o
  slot futuro aparece desabilitado com rótulo honesto; nenhum botão sem efeito real.
- Evidência: EditMode test (ação → caminho mockado) + cenário humano.

### CA-5 Regras de morte intactas

- Zero diff em CorpseRecoveryManager/AnyaRespawnService/regras de penalidade;
  CaveDeathResolver no máximo expõe dado já calculado (diff aditivo mínimo, coordenado
  com F66).
- Evidência: diff review + anti-regressão.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Death/
  DeathScreenCanvasController.cs (NOVO — canvas, gatilhos/eventos preservados)
  DeathScreenViewModel.cs        (NOVO — projection pura: causa/corpo/instrução/ações)
  DeathScreenController.cs       (REMOVIDO ou esvaziado p/ shim — decisão Fase 0 pelo
                                  menor diff de wiring; IMGUI morre)
Assets/_Game/Tests/EditMode/UI/DeathScreenCanvasTests.cs (NOVO)
docs/validation/fable_64_spec_death_screen_canvas_corpse_messaging_execution_report.md
```

## Contratos

### Data contracts

- `DeathScreenViewModel`: {Cause, LocationLabel, HasCorpse, CorpseItemCount, CorpseGold,
  CorpseCaveLevel, XpLost, RecoveryInstruction, Actions[]} — tipos simples, projection
  pura.

### Runtime contracts

- Gatilhos preservados: PlayerDiedEvent / CavePlayerDefeatedEvent abrem; abrir publica
  DeathScreenOpenedEvent; fechar publica DeathScreenClosedEvent (consumidores mapeados
  na Fase 0 intactos).
- Estado do corpo lido do CorpseRecoveryManager (fonte autoritativa) via referência
  injetada (bootstrap/serialized — nunca Find).
- Ação Respawn → API/evento real do fluxo existente (abstraído p/ teste).

### Event contracts

- Adds events: NO (contrato existente preservado). Tudo via GameEventBus; unsubscribe
  pareado.

### Save contracts

- Nenhum. Nada novo persiste.

### UI contracts

- Canvas padrão E44 (fallback WI-23 documentado); input de gameplay bloqueado com a
  tela aberta; Esc NÃO fecha (morte exige escolha — conferir death_anya_corpse_rules);
  foco inicial no Respawn; estados vazios explícitos.

## Sistemas afetados

```text
UI/Death (tela substituída)
Fluxo de morte (consumido — zero mudança de regra)
CorpseRecoveryManager (leitura)
CaveDeathResolver (no máximo exposição aditiva de dado já calculado)
ModalManager/input routing (tela bloqueia gameplay)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs (novo)
Assets/_Game/Scripts/UI/Death/DeathScreenViewModel.cs (novo)
Assets/_Game/Scripts/UI/Death/DeathScreenController.cs (remoção/shim)
CaveDeathResolver.cs (diff aditivo MÍNIMO de exposição de dado — coordenado com F66)
ponto de wiring da tela (diff mínimo — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/UI/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (YAML manual)
Packages/** ; ProjectSettings/**
CorpseRecoveryManager/AnyaRespawnService/DeathSystemBootstrap (regras — só consumir)
CorpseRecoveryModal/CorpseRecoveryUIController (tela vizinha intacta)
Regras de penalidade/perda (death_anya_corpse_rules — exibir, nunca mudar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Consumidores de DeathScreenOpened/Closed; caminho real de respawn; fonte do snapshot do
corpo; E44 entregue?; regra Esc na morte; decisão remoção × shim do controller antigo.

### Fase 1 — Projection
DeathScreenViewModel pura + testes (caverna/overworld/com-sem corpo/com-sem XP/vazios).

### Fase 2 — Tela canvas
DeathScreenCanvasController (builder E44 ou padrão WI-23) + gatilhos/eventos
preservados + navegação por teclado + ação real de respawn.

### Fase 3 — Fechamento
Remoção do IMGUI + wiring + testes de contrato; csproj; run_strict_validation;
execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch11_shipping.
- Can run with: F61, F62, F63, F65, F67.
- Must not run with: F14 (cadeia canvas/builder), F66 (CaveDeathResolver compartilhado).
- Shared files/systems that require lock: UI/Death/**, RuntimeUiBuilder (consumo),
  CaveDeathResolver (diff aditivo).
- Reason: substitui uma tela viva consumindo o padrão canvas compartilhado e lê um
  arquivo que F66 também toca.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO (DeathScreenOpened/Closed preservados)
Changes existing events: NO
Requires unsubscribe pattern: YES (controller assina 2-3 eventos; OnDisable pareado)
```

## Impacto em UI/Unity

```text
Changes UI: YES — tela de morte IMGUI → Canvas
Changes scenes: NO (construção programática; wiring via bootstrap existente)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote final — morrer na caverna e no overworld)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: quebrar consumidores de DeathScreenOpened/ClosedEvent.
Mitigação: mapa de consumidores na Fase 0 + contrato preservado + teste.

Risco: botão de respawn disparar caminho errado/duplicado.
Mitigação: caminho ÚNICO auditado (DeathSystemBootstrap/AnyaRespawnService); ação
abstraída e testada; nunca reimplementar respawn.

Risco: dados do corpo dessincronizados (evento antigo × manager atual).
Mitigação: CorpseRecoveryManager como fonte única; eventos só como gatilho.

Risco: E44 pendente na execução.
Mitigação: fallback documentado no padrão canvas WI-23 (ModalBase/views) — a tela não
espera o builder; migração trivial depois.

Risco: colisão com F66 no CaveDeathResolver.
Mitigação: Must not run with F66; diff aqui é aditivo mínimo de exposição.

Risco: duas mortes em sequência rápida (re-abrir/duplicar tela).
Mitigação: guarda de estado aberto (já existe no IMGUI — preservar) + teste abre-1×.
```

## Rollback

```text
Restaurar o DeathScreenController IMGUI (git) e remover os 2 arquivos novos. Nenhuma
regra de morte/penalty foi tocada; nenhum schema mudou. Consumidores de eventos voltam
ao comportamento anterior automaticamente.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: consumidores dos eventos, caminho real de respawn, fonte do corpo,
        E44?, regra Esc, remoção × shim.
- [ ] T002 — DeathScreenViewModel pura + testes (matriz caverna/overworld/corpo/XP).
- [ ] T003 — DeathScreenCanvasController + navegação + ação real + contrato de eventos.
- [ ] T004 — Remoção do IMGUI + wiring + testes de contrato; csproj;
        run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (projection da morte, contrato de eventos, ação)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — morrer na
  caverna com itens, ler a mensagem, respawnar, recuperar o corpo; morrer no overworld)
- Requires regression test: YES (fluxo de morte/respawn/corpse recovery inalterado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano do loop completo
  morte → mensagem → respawn → recuperação

## Definition of Done

```text
Tela de morte Canvas navegável por teclado com causa/local, resumo do corpo (itens/
ouro/nível), instrução de recuperação, XP perdido e ação real de respawn; IMGUI
removido; eventos DeathScreenOpened/Closed preservados; zero mudança de regra de morte;
builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Fluxo de morte/respawn/penalidade byte-idêntico em comportamento (só a superfície muda).
CorpseRecoveryModal e recuperação no corpo intactas.
DeathScreenOpened/ClosedEvent publicados nos mesmos momentos (consumidores mapeados).
Nenhum segundo caminho de respawn; nenhum OnGUI novo no projeto.
Eventos só via GameEventBus; unsubscribe pareado; zero GameObject.Find em runtime.
```
