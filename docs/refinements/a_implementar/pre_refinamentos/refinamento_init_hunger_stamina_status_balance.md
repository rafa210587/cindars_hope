# refinamento_init_hunger_stamina_status_balance

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_hunger_stamina_status_balance.md`
> Objetivo: evoluir fome MVP para sistema integrado de stamina, status simples, buffs/debuffs, HUD, passagem de tempo e eventos de tempo.

---

## 1. Estado atual

`HungerManager` reduz fome por movimento/dia, restaura fome por comida e aplica dano quando fome chega a zero.

Evidencia:

```text
Assets/_Game/Scripts/Player/HungerManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md
```

Nao encontramos spec futura dedicada exclusivamente a passagem de tempo. Portanto, a spec 09 passa a ser dona do MVP de tempo in-game, dia/noite, pausa e eventos de tempo.

---

## 2. Gaps

- Nao ha stamina real.
- Fome nao afeta velocidade, stamina regen, dano ou defesa.
- Comidas nao aplicam buffs/debuffs alem de restaurar fome.
- Nao ha status de fome critica com efeitos persistentes.
- Balanceamento de perda por acao ainda e simples.
- Nao ha UI final/minima de hunger/stamina/status.
- Nao ha passagem de tempo in-game definida.
- Nao ha sistema de eventos de tempo para farm, shop, crafting, NPCs e cave reagirem.
- Tempo ainda nao pausa por modal/HUD interativa.

---

## 3. Decisoes aprovadas

- `MaxStamina = 100`.
- `CurrentStamina` inicia em 100.
- Stamina salva/carrega.
- Stamina regenera com delay de 1 segundo apos gasto.
- Regen base: 15 por segundo.
- No `Hunger = 0`, stamina regen fica em 2 pontos por segundo.
- Sem stamina suficiente, acao nao executa e mostra feedback simples.
- Custos de stamina sao 4x os valores base sugeridos.
- HUD mostra uma barra abaixo da outra: Hunger, Stamina e status ativos abaixo.
- Dia dura 10 minutos reais de gameplay in-game ativo.
- Noite dura 5 minutos reais de gameplay in-game ativo.
- Tempo in-game pausa quando houver pause ou HUD/modal interativa aberta.
- Criar sistema/eventos de tempo para outros sistemas se plugarem e reagirem.
- Crafting, dialogue e shop nao consomem stamina neste MVP.
- Ao iniciar novo dia, stamina volta para MaxStamina; hunger nao volta automaticamente.

---

## 4. Stamina

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
- Regen respeita modificadores de fome/status.

---

## 5. Custos de stamina

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

- Farm actions consultam stamina antes de mudar estado do plot.
- Tree hit e fishing cast consultam stamina antes de executar.
- Attack/punch MVP consome stamina se existir no runtime atual.
- Sprint/dodge/roll ficam hooks futuros.
- StaminaCost deve ficar em data/config quando aplicavel, nao hardcoded em multiplos sistemas.

---

## 6. Hunger integration

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
- Hunger nao e restaurada automaticamente ao novo dia.
- Novo dia restaura stamina, nao fome.

---

## 7. Food / consumables

Expandir item/consumable data com campos simples:

```text
HungerRestore
StaminaRestore
StatusEffectIds[] opcional
BuffDurationSeconds opcional
```

Regras:

- Comida pode restaurar hunger e/ou stamina.
- Consumir comida respeita inventory slots da spec 03.
- Se consumo falhar, nao aplicar restore/status.

---

## 8. Status temporarios MVP

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

---

## 9. Passagem de tempo in-game

Criar ou equivalente:

```text
GameTimeManager
GameTimeState
GameTimeBalanceSO
GamePauseState
```

Duração aprovada:

```text
DayDurationRealSeconds = 600
NightDurationRealSeconds = 300
FullCycleRealSeconds = 900
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

- Dia dura 10 minutos de tempo ativo.
- Noite dura 5 minutos de tempo ativo.
- Ao fim do dia, emitir transicao para noite.
- Ao fim da noite, incrementar `CurrentDay` e emitir novo dia.
- Tempo nao avanca quando pausado.
- Tempo nao avanca quando modal/HUD interativa bloquear gameplay.
- HUD puramente informativa nao pausa tempo.
- Modais/painels como InventoryPanel, DialogueModal, ShopMenu/Buy/Sell, CraftingModal e menus de pause pausam tempo.

---

## 10. Eventos/notificacoes de tempo

Criar eventos via `GameEventBus`:

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

- `GameTimeTickEvent` deve ter frequencia controlada.
- Sugestao MVP: tick a cada 1 segundo de tempo ativo.
- Farm growth, shop restock, tree regrowth, crafting timers e schedules futuros devem poder ouvir eventos de tempo.

---

## 11. HUD

HUD deve mostrar em bloco vertical:

```text
Hunger bar
Stamina bar
Status icons/list abaixo das barras
```

Regras:

- HUD nao e modal.
- HUD informativa nao pausa tempo.
- HUD reflete valores runtime reais.
- Status ativos aparecem abaixo das barras.
- Feedback de stamina insuficiente deve ser simples e visivel.

---

## 12. Save/load

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

Nao serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

---

## 13. Invariantes anti-regressao

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

---

## 14. Definition of Done

- [ ] Stamina existe, salva/carrega e publica eventos.
- [ ] Stamina inicia em 100 e respeita maximo 100 no MVP.
- [ ] Stamina regenera com delay de 1s apos gasto.
- [ ] Hunger afeta stamina regen e move speed conforme faixas aprovadas.
- [ ] `Hunger = 0` mantem stamina regen em 2 por segundo e aplica dano periodico controlado.
- [ ] Farm actions consomem stamina conforme custos definidos.
- [ ] Tree hit, fishing cast e attack/punch MVP consomem stamina quando aplicavel.
- [ ] Sem stamina suficiente, acao falha sem alterar estado parcial.
- [ ] Comidas podem restaurar hunger e/ou stamina.
- [ ] Status temporarios minimos funcionam e aparecem na HUD.
- [ ] HUD mostra Hunger bar, Stamina bar e status abaixo.
- [ ] Dia dura 10 minutos de tempo ativo.
- [ ] Noite dura 5 minutos de tempo ativo.
- [ ] Tempo pausa quando pause/modal/HUD interativa estiver aberta.
- [ ] Eventos de tempo permitem outros sistemas reagirem.
- [ ] Novo dia restaura stamina, mas nao hunger.
- [ ] Save/load preserva hunger, stamina, status e tempo.
- [ ] Invariantes anti-regressao preservadas.

---

## 15. Validacao

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
16. Validar Unity compile validation e docs validation.
