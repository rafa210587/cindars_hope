# refinamento_spec14a_fix2_spawn_density_combat_feedback_damage_numbers

> Status: Refinamento detalhado a implementar
> Spec relacionada: `docs/specs/a_implementar/spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md`
> Tipo: Cave Spawn Density / Combat Logs / Floating Damage Numbers
> Depende de: SPEC 14A/14B implementadas e inimigos materializando na cave.

---

## 1. Contexto

Os inimigos voltaram a aparecer na cave após o wiring dos assets da SPEC 13G/14A.

Agora há três ajustes de gameplay/feedback necessários:

1. Aumentar em aproximadamente 20% a quantidade de criaturas por cave level.
2. Padronizar logs de combate para sempre indicar o nome da criatura quando a criatura for origem ou alvo do dano.
3. Implementar dano flutuante acima de quem recebeu dano, para player e inimigos.

---

## 2. Problemas observados

### 2.1 Densidade de inimigos

A densidade atual está funcional, mas pode ficar baixa para a sensação de exploração/risco.

Regra desejada:

```text
Aumentar em ~20% a quantidade de criaturas materializadas por cave level.
```

Se o range atual efetivo for 12-20, o novo alvo deve ser aproximadamente:

```text
14-24
```

O ajuste deve preservar:

- seed determinística;
- estabilidade por run;
- room size constraints;
- faction locks;
- max enemies por sala/pack;
- warnings quando não houver espaço suficiente.

### 2.2 Logs de combate sem nome da criatura

Exemplo atual:

```text
EnemyContactDamage: dealt 3 damage to player. HP should update through PlayerManager.
```

Esse log não informa qual criatura causou o dano.

Regra desejada:

```text
Todo log de combate precisa conter DisplayName e EnemyId quando envolver criatura.
```

Exemplo desejado:

```text
CombatLog: EnemyContactDamage. SourceName=Rato de Basalto, SourceEnemyId=enemy_stone_rat, Target=Player, Damage=3.
```

Quando o alvo for inimigo:

```text
CombatLog: Hit enemy. TargetName=Rato de Basalto, TargetEnemyId=enemy_stone_rat, Damage=12, HP=8->0/20.
```

### 2.3 Dano flutuante ausente

Não foi encontrado contrato/spec clara para damage numbers flutuantes.

Regra desejada:

```text
Quando qualquer entidade toma dano, aparecer número de dano acima dela.
```

MVP:

- dano em inimigo: número aparece acima do inimigo;
- dano no player: número aparece acima do player;
- heal futuro pode aparecer em verde ou com sinal +, mas não é obrigatório agora;
- critical/weak/resist/imune podem ser hooks, mas não precisam de arte final.

---

## 3. Decisões de design

### 3.1 Densidade

Preferir ajustar configuração data-driven/serialized:

```text
CaveRuntimeMaterializer._maxEnemiesPerLevel
EnemySpawnRequest.MaxEnemies
EnemySpawnPackSO.MaxTotalEnemies
Spawn density setting futura
```

Não hardcodar em vários pontos.

Se houver um único default `_maxEnemiesPerLevel = 20`, trocar para 24.

Se houver mínimo interno de 12, ajustar para 14 se existir.

### 3.2 Logs

Criar ou reutilizar helper para logs de combate, por exemplo:

```text
CombatLogFormatter
```

Não é obrigatório se o ajuste ficar simples, mas deve evitar repetir lógica ruim.

Campos mínimos para logs envolvendo inimigos:

```text
SourceName
SourceEnemyId
TargetName
TargetEnemyId
Damage
DamageType quando disponível
HP before/after quando disponível
EnemyInstanceId quando disponível
CaveLevel quando disponível
```

Se a entidade for player:

```text
Target=Player
ou Source=Player
```

### 3.3 Floating damage numbers

Criar sistema simples e central:

```text
FloatingDamageNumberManager
FloatingDamageNumberView
```

Preferência:

- manager persistente ou scene-level, wired pelo GameBootstrap/scene generator;
- pode criar fallback seguro se ausente;
- usar Canvas world-space ou TextMeshPro, conforme o padrão disponível no projeto;
- se TextMeshPro não estiver disponível, usar Unity UI/Text ou TextMesh sem adicionar pacote.

Não adicionar package novo.

### 3.4 Eventos

Usar eventos existentes quando possível:

- `DamageAppliedEvent` para dano em inimigos;
- `HPChangedEvent` para player, se não houver PlayerDamagedEvent;
- criar `PlayerDamagedEvent` somente se necessário e sem quebrar compatibilidade;
- criar/ajustar evento de dano com payload simples se o evento atual não carregar posição/nome suficiente.

Payloads não devem carregar GameObject/Transform/MonoBehaviour.

---

## 4. Critérios de pronto

A spec está pronta quando:

- cave level com inimigos tem densidade aproximadamente 20% maior;
- logs de contato inimigo -> player mostram nome e EnemyId da criatura;
- logs de dano player -> inimigo mostram nome e EnemyId da criatura;
- logs de morte/XP/loot continuam com nome e EnemyId;
- dano flutuante aparece acima do player quando ele toma dano;
- dano flutuante aparece acima do inimigo quando ele toma dano;
- sistema não cria package novo;
- sistema não usa busca global runtime proibida;
- há validator ou teste editor mínimo;
- há documento de validação.
