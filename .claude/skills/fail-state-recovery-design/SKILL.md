---
name: fail-state-recovery-design
description: Pattern de design de fail states — o que o player perde, onde respawna, o que é preservado, como recupera e o que previne softlock. Usar em specs de morte/KO/colapso, penalidade de falha, recovery mechanics, checkpoint design, ou qualquer feature com consequência de fracasso.
---

# Skill: Design de Fail State & Recovery

Fail states mal projetados criam dois problemas opostos: sem penalidade o risco não existe; penalidade excessiva frustra e bloqueia o player. O projeto usa o **corpse mechanic** da cave como referência canônica — morte tem custo alto, mas o progresso é *recuperável*, não perdido permanentemente.

## Quando usar

- Spec define o que acontece quando o player morre, colapsa, ou falha uma condição.
- Spec adiciona nova zona/loop com perfil de risco (novos dungeons, trials, eventos temporários).
- Spec implementa checkpoint, respawn point, ou recovery mechanic.
- Spec muda a severidade de uma penalidade existente.
- Spec menciona "morte", "KO", "colapso", "penalidade", "respawn", "recovery", "lose on fail".

## As cinco perguntas de design

Antes de implementar qualquer fail state, responda:

| Pergunta | Errado (não responder) | Canônico (cave death) |
|---|---|---|
| **O que o player perde?** | "tudo" ou "nada" sem design intencional | inventário + ouro + equipamento + XP do nível atual |
| **Para onde vai?** | tela de game over sem continuidade | respawn na Anya Fountain (dentro da cave) |
| **O que é preservado?** | nada (frustrante) ou tudo (sem custo) | progresso de cave level, quests, relações com NPCs |
| **Como recupera o que perdeu?** | não tem recovery (pune demais) | corpse mechanic — corpo na posição da morte, recuperável |
| **O que previne softlock?** | não tem fallback | Anya Fountain é sempre acessível; recovery parcial é possível |

## Sistema canônico: morte na cave

```
Player HP = 0
  → PlayerDeathController.Publish(PlayerDiedEvent { SceneName })
  → CaveDeathResolver.Execute():
       Cria Corpse { CorpseId, Position, CaveLevel, CaveSeed, gold, items[], equipment[] }
       Limpa inventário, equipamento, ouro do player
       Reseta XP para início do nível atual (XpResetToLevelStartEvent)
       Redistribui inimigos (CaveEnemiesRedistributionRequestedEvent)
  → Player respawna em Anya Fountain (in-cave, não sai da cave)

Player chega ao Corpse (IInteractable)
  → CorpseRecoveryManager.RecoverCorpse()
       Tenta restaurar inventário → se cheio: PartiallyRecovered
       Tenta reequipar armaduras → se impossível: vai para inventário
       Restaura ouro
  → Status: Active → Recovered (ou PartiallyRecovered se inventário cheio)
  → CorpseRecoveredEvent / CorpsePartiallyRecoveredEvent
```

**Arquivos canônicos:**
- `PlayerDeathController` — detecta HP = 0, publica `PlayerDiedEvent`
- `CaveDeathResolver` — executa a política de morte
- `CaveDeathPolicy` — configuração dos campos (o que perder/criar)
- `CorpseRecoveryManager` — recovery completa ou parcial
- `DeathSaveData` — persiste o corpse entre sessões

## Colapso por fadiga (fail state diferente)

Colapso às 02:00 via `FatigueSystem` → `PlayerCollapseEvent`. Penalidade menor que morte: player é teletransportado para cama, sem perda de inventário. Reutilize `FatigueSystem` para novos thresholds de fadiga — não reimplemente o timer.

## Checklist de design antes de implementar

```
[ ] Penalidade definida (o que se perde): items / ouro / XP / progresso / nada
[ ] Recovery definida: corpse / checkpoint / nada / parcial
[ ] Respawn point definido: in-zone / town / farm / cama
[ ] Anti-softlock: player pode sempre progredir após fail?
[ ] Saves protegidos: boss fight ou evento crítico bloqueia save? (CaveBossFightSaveGate)
[ ] Corpse/estado persistido no save: DeathSaveData ou equivalente
[ ] Evento publicado: o sistema downstream sabe que houve fail?
```

## Pattern de implementação

```csharp
// 1. Detectar fail state via evento
private void OnHpChanged(HPChangedEvent evt)
{
    if (_isDead || evt.CurrentHp > 0) return;  // guard contra dupla morte
    _isDead = true;
    GameEventBus.Publish(new PlayerDiedEvent { SceneName = _currentScene });
}

// 2. Resolver (separado da detecção — responsabilidade única)
private void OnPlayerDied(PlayerDiedEvent evt)
{
    if (evt.SceneName != "CaveScene") return;  // só age se for no contexto certo
    _resolver.Execute();  // CaveDeathResolver — pure C#, testável
}

// 3. Recovery como IInteractable
public class CorpseInteractable : MonoBehaviour, IInteractable
{
    public void Interact() => _recoveryManager.RecoverCorpse(_corpseId);
}
```

## Quando NÃO usar

- Fail state sem custo nenhum (tela de game over + retry) → não há design de recovery, só reiniciar.
- Spec que remove penalidade existente (nerf de morte) → mudança de balance, não de pattern; consulte `economy-balance-tuning`.
- Fail state de UI/menu (fechar sem salvar) → não é gameplay fail state.

## Relacionados

- `(skill: player-needs-survival)` — colapso por fome/fadiga
- `(skill: cave-stable-run-guard)` — respawn não pode mudar o seed da cave
- `(skill: save-load-pattern)` — `DeathSaveData` persiste corpse entre sessões
- `(skill: scene-interactable-wiring)` — corpse como `IInteractable`
- `(rule: id-stability)` — `CorpseId` é GUID gerado na criação, nunca renomeado
- `(rule: unity-architecture)` — `PlayerDiedEvent` via GameEventBus; `CaveDeathResolver` é pure C#
