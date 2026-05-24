# SPEC - Hunger, stamina, status, tempo e balance

> Spec ID: spec_hunger_stamina_status_balance
> Status: A implementar
> Ordem de execucao: 09
> Depende de: 00-08
> Bloqueia: 10, 11, 12, 14, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Integrar fome com stamina, buffs/debuffs simples, HUD de necessidades/status, passagem de tempo in-game, pause-aware time e eventos de tempo para outros sistemas.
> Fora de escopo: sistema completo de dano elemental/resistencias, combate final, sprint/dodge final, clima, sono/cama final, calendario complexo, romance/friendship, UI final consolidada, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_hunger_stamina_status_balance.md

---

# /speckit.specify

## Contexto

`HungerManager` ja reduz fome por movimento/dia, restaura fome por comida e aplica dano quando fome chega a zero.

Evidencia atual:

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md
```

Specs 04-07 ja introduzem acoes que precisam consumir stamina como arar, molhar, plantar, colher, cortar arvores, pescar e futuramente combater. Ate aqui nao existe spec dedicada a passagem de tempo; portanto esta spec passa a ser dona do MVP de tempo in-game, dia/noite, pausa e eventos de tempo.

## Pre-condicoes

Implementar runtime somente depois de specs 02-08 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Craft/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
```

Se modal/pause state ainda nao existir, criar o minimo necessario nesta spec para o tempo pausar corretamente quando HUD/modal de gameplay estiver aberto.

## Problema

Gaps atuais:

- nao ha stamina real;
- fome nao afeta stamina regen, movimento ou eficacia de acoes;
- comidas nao restauram stamina nem aplicam buffs simples;
- nao ha status temporario minimo;
- nao ha HUD consolidada de hunger/stamina/status;
- nao ha passagem de tempo in-game definida;
- nao ha sistema de notificacao de tempo para outros sistemas reagirem;
- farm/crafting/shop/NPC/world precisam de eventos de tempo consistentes para day transition, restock, crescimento, regrowth e timers.

## Objetivo

Implementar sistema minimo e robusto de necessidades/tempo:

- stamina com max/current, regen, gasto e save/load;
- fome integrada a stamina, movimento e dano por fome zero;
- custos de stamina para acoes de farm/world/fishing/combat MVP;
- comida restaurando fome e/ou stamina;
- status temporarios simples;
- HUD com barras empilhadas e status abaixo;
- passagem de tempo com dia de 10 minutos e noite de 5 minutos;
- tempo pausado quando jogo estiver pausado ou modal/HUD interativa estiver aberta;
- eventos de tempo para outros sistemas se plugar e reagir.

## Decisoes aprovadas

- `MaxStamina = 100`.
- `CurrentStamina` inicia em 100.
- Stamina salva/carrega.
- Stamina regenera com delay de 1 segundo apos gasto.
- Regen base: 15 por segundo.
- No `Hunger = 0`, stamina regen nao zera: fica em 2 pontos por segundo.
- Sem stamina suficiente, a acao nao executa e mostra feedback simples.
- Custos de stamina devem ser 4x os valores base sugeridos.
- HUD deve mostrar uma barra embaixo da outra: Hunger, Stamina e abaixo os status ativos.
- Dia dura 10 minutos reais de gameplay in-game.
- Noite dura 5 minutos reais de gameplay in-game.
- O tempo in-game pausa quando houver pause ou HUD/modal interativa aberta.
- Criar sistema/eventos de notificacao de tempo para outros sistemas reagirem.
- Crafting, dialogue e shop nao consomem stamina neste MVP.
- Ao iniciar novo dia, stamina volta para MaxStamina; hunger nao volta automaticamente.

## Stamina

Criar ou equivalente:

```text
StaminaManager
StaminaChangedEvent
StaminaEmptyEvent
StaminaSpendRequestedEvent opcional
StaminaSpendFailedEvent opcional
```

Valores iniciais:

```text
MaxStamina = 100
StartingStamina = 100
BaseRegenPerSecond = 15
RegenDelayAfterSpendSeconds = 1.0
```

Regras:

- Stamina nunca fica abaixo de 0 nem acima do maximo efetivo.
- Acoes validam stamina antes de executar.
- Se stamina insuficiente, acao falha sem alterar estado de mundo/inventory/farm/combat.
- Regen nao ocorre durante acoes que bloqueiam regen, salvo se configurado.
- Regen respeita modificadores de fome/status.

## Custos de stamina

Custos MVP aprovados, ja multiplicados por 4:

```text
Arar solo: 16
Molhar solo: 8
Plantar seed: 4
Colher: 4
Cortar arvore / hit: 24
Pescar / cast: 20
Attack / punch MVP: 20
Dodge / roll futuro: 40
Sprint futuro: drain por segundo, valor a definir em spec futura
```

Regras:

- Farm actions da spec 04 devem consultar stamina antes de mudar estado do plot.
- Tree hit e fishing cast da spec 05 devem consultar stamina antes de executar.
- Attack/punch MVP consome stamina se existir no runtime atual.
- Sprint/dodge/roll ficam hooks futuros, sem implementar movimento final nesta spec.
- StaminaCost pode ficar em data/config quando aplicavel, nao hardcoded em multiplos sistemas.

## Hunger integration

Faixas de fome:

```text
Hunger >= 70: normal
Hunger 30-69: stamina regen normal
Hunger 10-29: stamina regen -40%
Hunger 1-9: stamina regen -70%, move speed -15%
Hunger = 0: stamina regen fixa em 2 por segundo, dano periodico controlado
```

Regras:

- Fome critica afeta gameplay de forma previsivel.
- `Hunger = 0` causa dano periodico controlado, nunca por frame.
- Se HP chegar a zero por fome, morte segue fluxo existente/futuro de morte.
- Hunger nao deve ser restaurada automaticamente ao novo dia.
- Novo dia restaura stamina, nao fome.

Dano por fome zero:

```text
StarvingDamageAmount: configuravel
StarvingDamageIntervalSeconds: configuravel, sugestao inicial 10s
```

## Food / consumables

Expandir item/consumable data com campos simples:

```text
HungerRestore
StaminaRestore
StatusEffectIds[] opcional
BuffDurationSeconds opcional
```

Regras:

- Comida pode restaurar hunger e/ou stamina.
- Consumir comida deve respeitar inventory slots da spec 03.
- Se consumo falhar, nao aplicar restore/status.
- Buffs completos de combate/elementos ficam para specs 10-11.

## Status temporarios MVP

Criar status simples ou equivalente:

```text
WellFed
Exhausted
Starving
StaminaRegenBuff
MoveSpeedBuff
```

Regras:

- Status podem alterar stamina regen, move speed ou exibir estado visual.
- Status temporarios persistem em save com `StatusEffectId` e `RemainingSeconds` quando ativos.
- Nao implementar pipeline completo de dano elemental/resistencia nesta spec.
- Status da spec 11 deve poder evoluir ou substituir estes contratos sem quebrar save.

## Passagem de tempo in-game

Criar ou equivalente:

```text
GameTimeManager
GameTimeState
GameTimeBalanceSO
GamePauseState
```

Duração aprovada:

```text
DayDurationRealSeconds = 600   # 10 minutos
NightDurationRealSeconds = 300 # 5 minutos
FullCycleRealSeconds = 900     # 15 minutos
```

Estados minimos:

```text
Day
Night
Paused
```

Dados minimos:

```text
CurrentDay
CurrentPhase
PhaseElapsedSeconds
CycleElapsedSeconds
IsTimePaused
```

Regras:

- Dia dura 10 minutos de tempo in-game ativo.
- Noite dura 5 minutos de tempo in-game ativo.
- Ao fim do dia, emitir evento de transicao para noite.
- Ao fim da noite, incrementar `CurrentDay` e emitir novo dia.
- Tempo nao avanca quando pausado.
- Tempo nao avanca quando modal/HUD interativa bloquear gameplay.
- HUD puramente informativa nao deve pausar tempo so por estar visivel.
- Modais/painels como InventoryPanel, DialogueModal, ShopMenu/Buy/Sell, CraftingModal e menus de pause devem pausar tempo.
- Time scale global do Unity nao deve ser usado de forma que quebre UI/timers de modal; preferir pausa logica do `GameTimeManager`.

## Eventos/notificacoes de tempo

Criar eventos via `GameEventBus` para outros sistemas se plugar:

```text
GameTimeTickEvent
GamePhaseChangedEvent
DayStartedEvent
DayEndedEvent
NightStartedEvent
NightEndedEvent
GameTimePausedEvent
GameTimeResumedEvent
```

Campos sugeridos:

```text
CurrentDay
CurrentPhase
PhaseElapsedSeconds
CycleElapsedSeconds
NormalizedPhaseTime
Reason opcional para pause/resume
```

Regras:

- `GameTimeTickEvent` deve ter frequencia controlada, nao publicar todo frame se isso gerar ruido.
- Sugestao MVP: tick a cada 1 segundo de tempo in-game ativo.
- Farm growth, shop restock, tree regrowth, crafting timers e future schedules devem poder ouvir eventos de tempo.
- Sistemas nao devem consultar tempo via singleton hardcoded se puderem reagir por evento.

## HUD

HUD deve mostrar, de cima para baixo ou em bloco vertical fixo:

```text
Hunger bar
Stamina bar
Status icons/list abaixo das barras
```

Regras:

- HUD nao e modal.
- HUD informativa nao pausa tempo.
- HUD deve refletir valores runtime reais.
- Status ativos aparecem abaixo das barras.
- Feedback de stamina insuficiente deve ser simples e visivel.
- Nao substituir HUD/hotbar/inventory final da spec 17; esta spec entrega HUD minima funcional.

## Balance data

Criar ou equivalente:

```text
PlayerNeedsBalanceSO
GameTimeBalanceSO
```

Campos sugeridos `PlayerNeedsBalanceSO`:

```text
MaxHunger
MaxStamina
StaminaRegenPerSecond
StaminaRegenDelaySeconds
StarvingDamageAmount
StarvingDamageIntervalSeconds
ActionStaminaCosts
HungerThresholds
HungerModifiers
```

Campos sugeridos `GameTimeBalanceSO`:

```text
DayDurationRealSeconds
NightDurationRealSeconds
TimeTickIntervalSeconds
PauseOnModal
```

## Save/load

Persistir com IDs e tipos simples:

```text
PlayerNeedsSaveData
- CurrentHunger
- CurrentStamina
- ActiveStatusEffects[]

StatusEffectSaveData
- StatusEffectId
- RemainingSeconds

GameTimeSaveData
- CurrentDay
- CurrentPhase
- PhaseElapsedSeconds
- CycleElapsedSeconds
```

Regras:

- Nao serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.
- Save/load deve restaurar hunger, stamina, status ativos e tempo in-game.
- Se status desconhecido for carregado, logar erro e ignorar com seguranca.
- Se tempo salvo for invalido, normalizar para dia atual seguro.

## UI/modal/pause integration

Esta spec deve se integrar ao modal stack da spec 06/07.

Regras:

- Quando modal interativo abrir, publicar/acionar pause logico do tempo.
- Quando ultimo modal interativo fechar, resumir tempo.
- Pause menu tambem pausa tempo.
- Nao pausar por HUD informativa.
- Nao deixar crafting/farm/shop/day transition avançar enquanto o tempo logico estiver pausado.

## Eventos existentes e compatibilidade

Preservar/reutilizar quando existirem:

```text
HungerChangedEvent
GoldChangedEvent
InventoryChangedEvent
DayStartedEvent se ja existir
```

Nao duplicar eventos com mesmo significado. Se evento antigo existir com assinatura insuficiente, criar v2/novo evento com nome claro e documentar compatibilidade.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- HungerManager atual e food restore existente;
- dano por fome, apenas controlar tick/intervalo;
- inventory consumables da spec 03;
- farm actions da spec 04;
- world actions/fishing/tree hit da spec 05;
- economy/shop/dialogue modal da spec 06;
- crafting modal/timers da spec 07;
- town/dialogue da spec 08;
- HUD/hotbar existentes;
- save migration e DTOs simples;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se uma acao nao puder consumir stamina com seguranca, bloquear com feedback/pendencia clara em vez de aplicar estado parcial.

## Criterios de aceite

- Stamina existe, salva/carrega e publica eventos.
- Stamina inicia em 100 e respeita maximo 100 no MVP.
- Stamina regenera com delay de 1s apos gasto.
- Hunger afeta stamina regen e move speed conforme faixas aprovadas.
- `Hunger = 0` mantem stamina regen em 2 por segundo e aplica dano periodico controlado.
- Farm actions consomem stamina conforme custos definidos.
- Tree hit, fishing cast e attack/punch MVP consomem stamina quando aplicavel.
- Sem stamina suficiente, acao falha sem alterar estado parcial.
- Comidas podem restaurar hunger e/ou stamina.
- Status temporarios minimos funcionam e aparecem na HUD.
- HUD mostra Hunger bar, Stamina bar e status abaixo.
- Dia dura 10 minutos de tempo ativo.
- Noite dura 5 minutos de tempo ativo.
- Tempo pausa quando pause/modal/HUD interativa estiver aberta.
- Eventos de tempo permitem outros sistemas reagirem.
- Novo dia restaura stamina, mas nao hunger.
- Save/load preserva hunger, stamina, status e tempo.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Player/Status/**
Assets/_Game/Scripts/Time/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Balance/**
```

Managers/bridges Unity devem ser finos. Calculo de stamina, modificadores de fome, status, passagem de tempo e eventos devem ficar fora de MonoBehaviour pesado quando possivel.

## Ordem segura de implementacao

1. Revalidar HungerManager, PlayerManager, events, save e HUD atual.
2. Criar balance data (`PlayerNeedsBalanceSO`, `GameTimeBalanceSO`).
3. Criar `StaminaManager` e eventos.
4. Integrar stamina com save/load.
5. Integrar hunger thresholds com stamina regen/move speed.
6. Controlar dano por fome zero com intervalo.
7. Criar `GameTimeManager` pause-aware.
8. Criar eventos de tempo.
9. Integrar pause/modal stack com time pause.
10. Integrar custos de stamina em farm/world/fishing/combat MVP.
11. Expandir consumables para hunger/stamina restore e status simples.
12. Implementar HUD Hunger/Stamina/Status.
13. Validar anti-regressao e atualizar tracking.

## Fluxos

### Gastar stamina

```text
Sistema solicita SpendStamina(cost, actionId)
Validar CurrentStamina >= cost
Se falso: publicar falha/feedback e nao executar acao
Se verdadeiro: reduzir stamina, publicar StaminaChangedEvent, executar acao
```

### Regenerar stamina

```text
Se tempo/logica nao pausada
Se delay apos gasto ja passou
Calcular regen base + modificadores de hunger/status
Aplicar regen por deltaTime
Clamp 0..MaxStamina
Publicar evento em mudancas relevantes
```

### Avancar tempo

```text
Se IsTimePaused, nao avanca
Acumular active delta
Publicar GameTimeTickEvent a cada intervalo configurado
Se fim do Day, publicar DayEndedEvent e NightStartedEvent
Se fim da Night, publicar NightEndedEvent e DayStartedEvent, incrementar CurrentDay e restaurar stamina
```

### Modal pause

```text
Modal interativo abre
GameTimeManager pausa com Reason=Modal
Ultimo modal interativo fecha
GameTimeManager resume
```

### Consumir comida

```text
Selecionar comida no inventory
Validar item consumable
Remover item somente se efeito puder ser aplicado
Aplicar HungerRestore/StaminaRestore/status
Publicar eventos
```

## Riscos de regressao

- Acoes alterarem mundo antes de validar stamina.
- Tempo continuar passando em modal/pause.
- DayStartedEvent duplicar ou quebrar restock/growth.
- Fome zero aplicar dano por frame.
- Status simples conflitar com spec 11 futura.
- HUD de needs sobrepor modais ou hotbar.

## Mitigacao

- Validacao de stamina antes da acao.
- Pause logico centralizado.
- Eventos de tempo idempotentes por transicao.
- Dano por fome com intervalo configuravel.
- Status MVP com IDs simples e compatibilidade futura.
- HUD informativa sem capturar input.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar HungerManager, PlayerManager, UI, Save e eventos atuais.
- [ ] Confirmar specs 02-08 implementadas antes de runtime.
- [ ] Criar/ajustar `PlayerNeedsBalanceSO`.
- [ ] Criar/ajustar `GameTimeBalanceSO`.
- [ ] Criar `StaminaManager`.
- [ ] Criar eventos de stamina.
- [ ] Integrar stamina com save/load.
- [ ] Implementar modificadores de fome sobre stamina/move speed.
- [ ] Controlar dano por fome zero com intervalo.
- [ ] Criar/ajustar status temporarios MVP.
- [ ] Expandir consumables para hunger/stamina/status.
- [ ] Criar `GameTimeManager` com dia 10min e noite 5min.
- [ ] Criar eventos de tempo para outros sistemas.
- [ ] Integrar pause/modal stack com pausa logica de tempo.
- [ ] Integrar custos de stamina em farm/world/fishing/combat MVP.
- [ ] Implementar HUD Hunger/Stamina/Status empilhada.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Time/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Balance/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Stamina implementada e persistida.
- Hunger afeta stamina/move speed conforme regras.
- Tempo in-game implementado com dia/noite e pausa.
- Eventos de tempo publicados e documentados.
- HUD Hunger/Stamina/Status funcional.
- Acoes relevantes consomem stamina sem alterar estado parcial em falha.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Validar Stamina inicia em 100.
2. Usar farm action e validar custo de stamina.
3. Tentar acao sem stamina suficiente e validar que nada muda.
4. Cortar arvore e validar custo 24 por hit.
5. Pescar e validar custo 20 por cast.
6. Reduzir hunger para 10-29 e validar regen -40%.
7. Reduzir hunger para 1-9 e validar regen -70% e move speed -15%.
8. Reduzir hunger para 0 e validar regen 2/s + dano periodico controlado.
9. Comer item e validar HungerRestore/StaminaRestore.
10. Validar HUD com Hunger bar, Stamina bar e status abaixo.
11. Validar dia durando 10 minutos ativos e noite 5 minutos ativos.
12. Abrir InventoryPanel/DialogueModal/ShopModal/CraftingModal e validar que tempo pausa.
13. Fechar modal e validar que tempo resume.
14. Validar eventos DayStarted/DayEnded/NightStarted/NightEnded/GameTimeTick.
15. Salvar/carregar hunger, stamina, status e tempo.
