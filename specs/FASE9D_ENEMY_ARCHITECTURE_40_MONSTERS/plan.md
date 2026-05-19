# Plan — FASE9D Enemy Architecture 40+ Monsters

> **Feature:** FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar arquitetura incremental:

1. contratos;
2. profiles data-driven;
3. controller de ação;
4. movement policy;
5. vulnerability windows;
6. element damage;
7. status receiver;
8. migrar Slime;
9. criar segundo monstro para validar reutilização;
10. validator/handoff.

---

## 2. Arquitetura proposta

### Novas pastas

```text
Assets/_Game/Scripts/Combat/Enemies/
Assets/_Game/Scripts/Combat/Enemies/Actions/
Assets/_Game/Scripts/Combat/Enemies/Movement/
Assets/_Game/Scripts/Combat/Elements/
Assets/_Game/Scripts/Combat/Status/
Assets/_Game/Data/Enemies/
Assets/_Game/Data/Enemies/Actions/
Assets/_Game/Data/Enemies/MovementProfiles/
Assets/_Game/Data/Enemies/ElementProfiles/
Assets/_Game/Data/Enemies/StatusProfiles/
```

### Componentes

- `EnemyActionController`
- `EnemyMovementController`
- `EnemyVulnerabilityController`
- `EnemyStatusReceiver`
- ações específicas reutilizáveis como `EnemyLeapAttackAction`, `EnemyProjectileAttackAction`, `EnemyAreaAttackAction`

### Dados

- `EnemyMovementProfileSO`
- `EnemyActionDataSO`
- `EnemyElementProfileSO`
- `EnemyStatusProfileSO`

---

## 3. Integrações

### EnemyDataSO

Evoluir para apontar profiles e actions.

### EnemyHealth

Aplicar cálculo de dano com elemento e vulnerabilidade.

### CreateMvpCaveScene

Configurar Slime via profiles e action controller.

### Validator

Adicionar validação de enemy profiles e action data.

---

## 4. Riscos

| Risco | Mitigação |
|---|---|
| Arquitetura grande demais | PRs pequenos, começar com contratos e Slime |
| Quebrar Slime atual | manter fallback de chase/contato até Leap estar validado |
| Overengineering | implementar só modos usados por Slime + segundo monstro no MVP |
| Status virar sistema grande | começar com Slow, Poison, Burn, Stun curto |
| Elementos afetarem balanceamento | usar multiplicadores simples e logs debug |

---

## 5. Testes manuais mínimos

- Slime persegue player.
- Slime escolhe Leap em range.
- Slime abre janela vulnerável no recovery.
- Ataque durante vulnerabilidade causa 1.5x.
- Segundo monstro mantém distância.
- Segundo monstro ataca à distância.
- Elementos alteram dano.
- Burn/Poison/Slow/Stun funcionam no MVP.
- Validator detecta dados inválidos.

---

## 6. Fora de escopo técnico

- 40 monstros completos.
- Boss AI.
- Behavior tree visual.
- Pathfinding avançado.
- Animação final.
- VFX final.
- Balanceamento final.
