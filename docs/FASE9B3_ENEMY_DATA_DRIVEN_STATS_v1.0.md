# FASE 9B-3 — Enemy Stats Data-Driven v1.0

> **Data:** 2026-05-18  
> **Status:** ✅ Implementado na `dev`  
> **Contexto:** pós FASE 9B-2 Cave/Combat Feel  
> **Objetivo:** preparar combate para múltiplos inimigos com características próprias sem criar subclasses por inimigo.

---

## 1. Motivação

O combate da Cave já funcionava para o Slime, mas os valores de gameplay estavam parcialmente engessados no código:

- força de knockback do soco do player;
- força de knockback causada pelo inimigo;
- velocidade de perseguição;
- raio de detecção;
- distância de parada;
- feedback visual;
- resistência a knockback.

Isso seria ruim para inimigos futuros, por exemplo:

| Inimigo | Força de empurrão causada | Resistência a empurrão | Velocidade | Observação |
|---|---:|---:|---:|---|
| Slime | 0 | 0 | baixa | inimigo fraco, tutorial |
| Lobo | 1 | 1 | média/alta | pressiona o player |
| Golem | 3 | alta | baixa | pesado, difícil de empurrar |
| Morcego | 0 | baixa | alta | frágil, evasivo |

A decisão foi manter **composição + ScriptableObject**, não herança por inimigo.

---

## 2. Decisão arquitetural

### Não adotado agora

Não criar classes especializadas como:

```csharp
SlimeEnemy : Enemy
WolfEnemy : Enemy
GolemEnemy : Enemy
```

Isso tende a duplicar lógica e espalhar exceções.

### Adotado

Usar componentes genéricos:

- `EnemyHealth`
- `EnemyContactDamage`
- `EnemyChaseController`
- `KnockbackController`
- `HitFlashController`
- `EnemyDropSpawner`

E mover variação de comportamento para `EnemyDataSO`.

Assim, novos inimigos podem ser criados principalmente por asset/configuração.

---

## 3. Contrato atual de EnemyDataSO

`EnemyDataSO` agora concentra atributos de combate, movimento, feedback e drops.

### Health

- `enemyId`
- `maxHp`

### Damage

- `contactDamage`
- `contactDamageCooldownSeconds`
- `contactKnockbackForce`

### Knockback Resistance

- `receivedKnockbackResistance`
- `receivedKnockbackMultiplier`

### Movement

- `moveSpeed`
- `detectionRadius`
- `stopDistance`

### Feedback

- `hitFlashColor`
- `hitFlashDuration`

### Drops

- `dropItemId`
- `dropAmount`

---

## 4. Contratos simples criados

### DamageRequest

Contrato para passar dano, origem e força de knockback em uma única chamada.

Campos:

- `Amount`
- `SourcePosition`
- `KnockbackForce`

Uso atual:

- `PlayerAttackController` cria `DamageRequest` ao socar.
- `EnemyHealth` aplica dano e calcula knockback final com base no `EnemyDataSO`.

### KnockbackRequest

Contrato para representar direção e força de knockback.

Campos:

- `Direction`
- `Force`

Uso atual:

- Preparado como contrato simples para evoluções futuras.
- `KnockbackController` mantém API compatível.

---

## 5. Fluxo atual de dano do player no inimigo

1. Player aperta `J`.
2. `PlayerAttackController` executa soco melee curto.
3. Detecta `EnemyHealth` por componente, sem tag.
4. Cria `DamageRequest` com:
   - dano do soco;
   - posição do player;
   - força de knockback do soco.
5. `EnemyHealth.TakeDamage(DamageRequest)`:
   - reduz HP;
   - dispara hit flash;
   - calcula knockback final:
     - `request.KnockbackForce * enemyData.receivedKnockbackMultiplier`;
   - aplica knockback se houver `KnockbackController`;
   - publica `EnemyKilledEvent` ao morrer.

---

## 6. Fluxo atual de dano do inimigo no player

1. Trigger filho do inimigo detecta contato.
2. `EnemyContactDamage` resolve `PlayerManager` via `GameBootstrap.Instance` ou componente.
3. Aplica `contactDamage`.
4. Aplica hit flash no player.
5. Aplica knockback no player apenas se:
   - `enemyData.contactKnockbackForce > 0`.

No Slime atual:

- `contactKnockbackForce = 0`
- Slime causa dano, mas não empurra o player.

---

## 7. Slime atual

Configuração esperada do `Enemy_Slime.asset`:

| Campo | Valor esperado |
|---|---:|
| `enemyId` | `enemy_slime` |
| `maxHp` | 10 |
| `contactDamage` | 1 |
| `contactDamageCooldownSeconds` | 1 |
| `contactKnockbackForce` | 0 |
| `receivedKnockbackResistance` | 0 |
| `receivedKnockbackMultiplier` | 1 |
| `moveSpeed` | 1.2 |
| `detectionRadius` | 5 |
| `stopDistance` | 0.55 |
| `dropItemId` | item válido, hoje `item_wood` se configurado pelo gerador |
| `dropAmount` | 1 |

---

## 8. Benefícios

- Menos hardcoding em componentes.
- Novos inimigos podem variar por dados.
- Slime, Lobo, Golem e Morcego podem compartilhar os mesmos componentes.
- Não precisa criar herança por inimigo para diferenças simples.
- Prepara balanceamento futuro sem mexer no código a cada ajuste.

---

## 9. Dívidas conhecidas

- `receivedKnockbackResistance` ainda é conceitual; o cálculo atual usa `receivedKnockbackMultiplier` diretamente.
- Ainda não há múltiplos inimigos reais na Cave.
- Ainda não há factory/spawner genérico de inimigos por `EnemyDataSO`.
- Ainda não há UI de debug para exibir stats do inimigo.
- Ainda não há animação/facing por inimigo.

---

## 10. Próximo passo recomendado

Sequência recomendada:

1. **PR-100 — Criar segundo inimigo placeholder**
   - exemplo: Lobo ou Morcego;
   - novo `EnemyDataSO`;
   - mesma arquitetura de componentes;
   - stats próprios.
2. **PR-101 — Enemy factory/helper no gerador da Cave**
   - reduzir duplicação no `CreateMvpCaveScene`;
   - criar inimigos a partir de dados.
3. **PR-102 — Cave validator multi-enemy**
   - validar Slime + novo inimigo;
   - validar `EnemyDataSO` atribuído;
   - validar componentes mínimos.
4. Depois disso, avançar para UI real ou primeira quest.

---

## 11. Checklist de validação

- [ ] Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- [ ] Slime persegue com valores do asset.
- [ ] Slime causa dano como antes.
- [ ] Slime não empurra player se `contactKnockbackForce = 0`.
- [ ] Player soca Slime.
- [ ] Slime recebe knockback via `receivedKnockbackMultiplier = 1`.
- [ ] Hit flash usa configuração do asset.
- [ ] Slime morre e dropa item.
- [ ] Console sem erro vermelho.

---

## 12. Regra para próximos inimigos

Para criar um novo inimigo, preferir:

1. Novo `EnemyDataSO`.
2. Mesmo conjunto de componentes.
3. Configuração por dados.
4. Só criar componente novo se houver comportamento realmente novo, não apenas número diferente.

Exemplo: Lobo mais rápido e com knockback deve ser só `EnemyDataSO`. Um inimigo que voa/desvia parede pode justificar componente novo, como `EnemyFlightController`.
