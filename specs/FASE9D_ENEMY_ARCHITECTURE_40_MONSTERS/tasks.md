# Tasks — FASE9D Enemy Architecture 40+ Monsters

> **Feature:** FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-113 — Enemy architecture contracts

### Escopo

Criar contratos base.

### Arquivos esperados

- `EnemyArchetype.cs`
- `EnemyMovementMode.cs`
- `DamageElement.cs`
- `StatusEffectType.cs`
- `EnemyActionType.cs`
- `EnemyActionPhase.cs`
- `IEnemyAction.cs`
- `EnemyActionContext.cs`

### Critérios

- [ ] Compila.
- [ ] Nenhum gameplay alterado.
- [ ] Nenhuma cena alterada.

---

## PR-114 — Enemy profiles data

### Escopo

Criar ScriptableObjects de profiles.

### Arquivos esperados

- `EnemyMovementProfileSO.cs`
- `EnemyActionDataSO.cs`
- `EnemyElementProfileSO.cs`
- `EnemyStatusProfileSO.cs`
- `ElementModifier.cs`
- `EnemyStatusApplication.cs`

### Critérios

- [ ] CreateAssetMenu funciona.
- [ ] IDs validados.
- [ ] Valores negativos normalizados/alertados.

---

## PR-115 — EnemyActionController MVP

### Escopo

Criar controller de seleção de ações.

### Critérios

- [ ] Escolhe ação por range/prioridade/cooldown.
- [ ] Respeita ação em execução.
- [ ] Não usa tags nem Find.
- [ ] Logs mostram ação escolhida.

---

## PR-116 — EnemyMovementController policy-based

### Escopo

Criar movimento por profile.

### Critérios

- [ ] ChaseTarget funciona.
- [ ] KeepDistance funciona.
- [ ] FleeFromTarget funciona.
- [ ] Stationary funciona.
- [ ] Leash respeitado.

---

## PR-117 — Vulnerability windows

### Escopo

Criar janelas de vulnerabilidade.

### Critérios

- [ ] EnemyVulnerabilityController abre/fecha janela.
- [ ] Multiplicador default 1.5x aplicado.
- [ ] Logs indicam janela ativa.

---

## PR-118 — Element damage calculation

### Escopo

Adicionar cálculo de dano elemental.

### Critérios

- [ ] DamageElement existe no DamageRequest ou extensão compatível.
- [ ] EnemyElementProfileSO modifica dano.
- [ ] Vulnerability entra na conta final.
- [ ] Logs mostram cálculo.

---

## PR-119 — Status receiver MVP

### Escopo

Criar receptor de status.

### Critérios

- [ ] Slow reduz movimento temporariamente.
- [ ] Poison causa dano ao longo do tempo.
- [ ] Burn causa dano ao longo do tempo.
- [ ] Stun curto bloqueia ação/movimento.
- [ ] Reaplicação renova duração, sem stack infinito.

---

## PR-120 — Slime Leap Attack arquitetura nova

### Escopo

Migrar Slime para action architecture.

### Critérios

- [ ] Slime usa EnemyActionController.
- [ ] Slime usa MovementProfile.
- [ ] Slime executa LeapAttack.
- [ ] Slime abre vulnerabilidade no recovery.
- [ ] Combat loop atual não regride.

---

## PR-121 — Segundo monstro para validar reutilização

### Escopo

Criar Cave Spitter ou inimigo equivalente.

### Critérios

- [ ] Usa o mesmo EnemyActionController.
- [ ] Usa KeepDistance.
- [ ] Ataca à distância.
- [ ] Pode aplicar status.
- [ ] Não possui controller específico gigante.

---

## PR-122 — Enemy architecture validator

### Escopo

Validar dados de inimigos.

### Critérios

- [ ] Detecta EnemyDataSO sem movement profile.
- [ ] Detecta EnemyDataSO sem action.
- [ ] Detecta timings inválidos.
- [ ] Detecta IDs duplicados.
- [ ] Detecta status/element profile inválido.

---

## Smoke test final

- [ ] Slime persegue e pula.
- [ ] Slime vulnerável toma 1.5x.
- [ ] Segundo monstro mantém distância.
- [ ] Segundo monstro dispara.
- [ ] Elementos alteram dano.
- [ ] Status funciona.
- [ ] Save/load não quebra.
- [ ] Console sem erro vermelho.
