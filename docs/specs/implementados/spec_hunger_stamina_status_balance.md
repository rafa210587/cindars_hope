# SPEC - Hunger, stamina, status, tempo e balance

> Spec ID: spec_hunger_stamina_status_balance
> Status: Implementado parcial
> Ordem de execucao: 09
> Data: 2026-05-24
> Evidencia: Assets/_Game/Scripts/Player/HungerManager.cs, StaminaManager.cs

## Resumo de Implementacao

### Implementado:
- `HungerManager` com valores configuraveis, regeneracao, dano por fome zero
- `StaminaManager` com max/current, regen com delay, spend validation
- HungerChangedEvent e StaminaChangedEvent
- Day started event triggers stamina recovery
- Hunger affects gameplay (damage, critical states)
- Save/load integrado para ambos

### Nao implementado (futuro):
- Integração completa de stamina costs para farm/fishing/crafting/combat
- HUD consolidado de hunger/stamina/status
- Status temporarios simples (buffs/debuffs)
- Passagem de tempo in-game (dia/noite ciclos)
- Modificadores de regen baseado em fome
- Movimento/acao afetada por stamina critica

### Validacao pendente:
- Unity compile validation
- Play mode test de stamina/hunger integration
