# FASE 9D — Enemy Architecture para 40+ monstros v1.1

> **Status:** spec futura revisada.  
> **Base:** `FASE9D_ENEMY_ACTIONS_AI_COMBAT_SPEC_v1.0.md`.  
> **Objetivo:** reorganizar a arquitetura de inimigos para suportar pelo menos 40 tipos de monstros com ações, movimento, elementos, status negativos e janelas de vulnerabilidade.

---

## 1. Decisão principal

Não criar uma classe específica gigante para cada monstro.

Errado:

```text
SlimeController
BatController
RatController
MushroomController
GolemController
...
```

Correto:

```text
EnemyController + EnemyDataSO + EnemyActionDataSO + EnemyMovementProfileSO + EnemyElementProfileSO
```

Cada monstro deve ser montado por dados e composição.

Exemplo:

```text
Slime
├── EnemyController
├── EnemyHealth
├── EnemyMovementController
├── EnemyActionController
├── EnemyVulnerabilityController
├── EnemyStatusReceiver
├── EnemyLeapAttackAction
└── EnemyContactAttackAction
```

A diferença entre 40 monstros deve vir principalmente dos assets de dados, não de 40 scripts diferentes.

---

## 2. Camadas da arquitetura

| Camada | Responsabilidade |
|---|---|
| EnemyDataSO | identidade, HP, drops, links para perfis |
| EnemyMovementProfileSO | como o inimigo se movimenta e qual distância quer manter |
| EnemyActionDataSO | ações disponíveis: melee, leap, ranged, magic, special |
| EnemyElementProfileSO | vulnerabilidades, resistências e imunidades |
| EnemyStatusProfileSO | status que pode aplicar ou resistir |
| EnemyActionController | escolhe ação com base em contexto e prioridade |
| EnemyMovementController | executa chase, kite, keep distance, flee, patrol |
| EnemyVulnerabilityController | controla janelas de dano ampliado |
| EnemyStatusReceiver | recebe poison, burn, slow, stun etc. |

---

## 3. Enemy archetypes

Para 40 monstros, usar arquétipos.

| Arquétipo | Comportamento |
|---|---|
| Chaser | corre até o jogador e ataca perto |
| Leaper | prepara e pula no jogador |
| Dasher | investe em linha reta |
| Kiter | mantém distância X e ataca de longe |
| Turret | fica quase parado e dispara |
| Ambusher | espera aproximação e ataca rápido |
| Swarmer | fraco, rápido, vem em grupo |
| Tank | lento, muito HP, bate forte |
| Caster | mantém distância e usa magia/status |
| Summoner | invoca inimigos menores |
| Support | cura/bufa outros inimigos |
| Bomber | aproxima, explode ou deixa área perigosa |
| Guardian | protege ponto/área e não persegue muito longe |
| Fleeing | foge quando HP baixo |

Cada monstro pode combinar arquétipos.

Exemplos:

| Monstro | Arquétipo primário | Arquétipo secundário |
|---|---|---|
| Slime | Leaper | Chaser |
| Bat | Dasher | Swarmer |
| Rat | Chaser | Fleeing |
| Spore Mushroom | Turret | Caster |
| Cave Imp | Kiter | Caster |
| Stone Golem | Tank | Guardian |
| Wisp | Caster | Kiter |

---

## 4. Movimento e distância desejada

Todos os inimigos devem ter uma política de movimento.

### 4.1 EnemyMovementMode

```csharp
public enum EnemyMovementMode
{
    None,
    ChaseTarget,
    KeepDistance,
    FleeFromTarget,
    PatrolArea,
    GuardArea,
    OrbitTarget,
    Stationary
}
```

### 4.2 EnemyMovementProfileSO

```csharp
[CreateAssetMenu(fileName = "EnemyMovementProfile", menuName = "CindarsHope/Combat/Enemy Movement Profile")]
public class EnemyMovementProfileSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public EnemyMovementMode MovementMode;

    [Header("Pace")]
    public float MoveSpeed = 1.2f;
    public float Acceleration = 20f;
    public float StopDistance = 0.6f;

    [Header("Distance Policy")]
    public float DesiredDistance = 1.0f;
    public float MinDistance = 0.5f;
    public float MaxDistance = 5.0f;

    [Header("Leash")]
    public float DetectionRadius = 5f;
    public float LeashRadius = 8f;
    public bool ReturnToSpawnWhenLeashed = true;

    [Header("Rhythm")]
    public float PauseBetweenMovesSeconds = 0f;
    public float DirectionChangeIntervalSeconds = 0.5f;
}
```

### 4.3 Interpretação

| MovementMode | Regra |
|---|---|
| ChaseTarget | tenta chegar até StopDistance |
| KeepDistance | aproxima se longe, recua se perto, mantém DesiredDistance |
| FleeFromTarget | foge quando condição ativa, ex: HP baixo |
| PatrolArea | anda em pontos/área enquanto sem alvo |
| GuardArea | persegue só dentro do leash |
| OrbitTarget | circula alvo a uma distância |
| Stationary | não anda; usa ações à distância ou área |

---

## 5. Ações de inimigo

### 5.1 IEnemyAction

```csharp
public interface IEnemyAction
{
    string ActionId { get; }
    EnemyActionType ActionType { get; }
    int Priority { get; }
    bool IsRunning { get; }

    bool CanStart(EnemyActionContext context);
    void StartAction(EnemyActionContext context);
    void TickAction(float deltaTime, EnemyActionContext context);
    void CancelAction(EnemyActionContext context);
}
```

### 5.2 EnemyActionType

```csharp
public enum EnemyActionType
{
    None,
    ContactMelee,
    Bite,
    Claw,
    LeapAttack,
    DashAttack,
    RangedProjectile,
    MagicProjectile,
    AreaAttack,
    ConeAttack,
    GroundHazard,
    ApplyStatus,
    Summon,
    HealAlly,
    BuffAlly,
    ShieldSelf,
    Teleport,
    Flee,
    Enrage
}
```

### 5.3 EnemyActionDataSO

```csharp
[CreateAssetMenu(fileName = "EnemyAction", menuName = "CindarsHope/Combat/Enemy Action")]
public class EnemyActionDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public EnemyActionType ActionType;
    public int Priority = 0;

    [Header("Range")]
    public float MinRange = 0f;
    public float MaxRange = 1.5f;
    public bool RequiresLineOfSight = false;

    [Header("Timing")]
    public float PrepareSeconds = 0.25f;
    public float ActiveSeconds = 0.25f;
    public float RecoverySeconds = 0.25f;
    public float CooldownSeconds = 1f;

    [Header("Damage")]
    public int Damage = 1;
    public DamageElement Element = DamageElement.Physical;
    public float KnockbackForce = 0f;

    [Header("Movement")]
    public float MoveSpeed = 4f;
    public float MaxTravelDistance = 2f;
    public bool ReturnToStartPosition = false;

    [Header("Projectile")]
    public string ProjectileId;
    public float ProjectileSpeed = 6f;
    public float ProjectileLifetimeSeconds = 2f;

    [Header("Status")]
    public EnemyStatusApplication[] StatusApplications;

    [Header("Vulnerability")]
    public bool OpensVulnerabilityWindow = true;
    public EnemyActionPhase VulnerablePhase = EnemyActionPhase.Recovering;
    public float VulnerableDamageMultiplier = 1.5f;
}
```

---

## 6. Action selection

`EnemyActionController` não deve escolher ações aleatoriamente sem regra.

Ele deve considerar:

- distância do player;
- linha de visão quando aplicável;
- cooldown;
- HP atual;
- status ativo;
- prioridade;
- se já existe ação em execução;
- se o inimigo está em recovery/stun;
- se o alvo está dentro do leash.

### 6.1 EnemyActionContext

```csharp
public readonly struct EnemyActionContext
{
    public readonly string EnemyId;
    public readonly GameObject Enemy;
    public readonly Transform EnemyTransform;
    public readonly Rigidbody2D EnemyRigidbody;
    public readonly Transform Target;
    public readonly float DistanceToTarget;
    public readonly Vector2 DirectionToTarget;
    public readonly EnemyDataSO EnemyData;
    public readonly float CurrentHpPercent;
    public readonly bool HasLineOfSight;
}
```

### 6.2 Seleção por score

Cada ação pode gerar score:

```text
score = Priority
      + range match bonus
      + HP condition bonus
      + target state bonus
      - cooldown penalty
```

MVP pode começar só com prioridade + range + cooldown.

---

## 7. Elementos, vulnerabilidades e resistências

### 7.1 DamageElement

```csharp
public enum DamageElement
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Earth,
    Wind,
    Water,
    Light,
    Shadow,
    Arcane
}
```

### 7.2 EnemyElementProfileSO

```csharp
[CreateAssetMenu(fileName = "EnemyElementProfile", menuName = "CindarsHope/Combat/Enemy Element Profile")]
public class EnemyElementProfileSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ElementModifier[] Modifiers;
}
```

### 7.3 ElementModifier

```csharp
[Serializable]
public struct ElementModifier
{
    public DamageElement Element;
    public float DamageMultiplier;
}
```

Exemplos:

| Perfil | Fire | Ice | Lightning | Physical |
|---|---:|---:|---:|---:|
| Slime comum | 1.0 | 1.2 | 1.0 | 1.0 |
| Slime de fogo | 0.5 | 1.5 | 1.0 | 1.0 |
| Golem de pedra | 1.0 | 1.0 | 0.8 | 0.7 |
| Wisp sombrio | 1.0 | 1.0 | 1.0 | 0.5 |

### 7.4 Damage calculation

Ordem sugerida:

```text
base damage
× element multiplier
× vulnerability window multiplier
× status modifiers
× difficulty modifiers futuros
= final damage
```

---

## 8. Status negativos

### 8.1 StatusEffectType

```csharp
public enum StatusEffectType
{
    None,
    Poison,
    Burn,
    Bleed,
    Slow,
    Stun,
    Root,
    Weakness,
    Vulnerable,
    Fear,
    Silence,
    Confusion
}
```

### 8.2 EnemyStatusApplication

```csharp
[Serializable]
public struct EnemyStatusApplication
{
    public StatusEffectType StatusType;
    public float Chance;
    public float DurationSeconds;
    public int Power;
}
```

### 8.3 EnemyStatusReceiver

Responsável por:

- receber status;
- aplicar duração;
- impedir stack indevido;
- modificar movimento/dano/recebimento de dano;
- expirar status.

### 8.4 PlayerStatusReceiver futuro

Como monstros também aplicam status no jogador, o player precisa ter receptor equivalente.

MVP pode começar com:

- Slow;
- Poison;
- Burn;
- Stun curto.

---

## 9. Janela de vulnerabilidade

Todo monstro deve poder abrir uma janela de vulnerabilidade em algum momento.

Exemplos:

| Ação | Janela vulnerável |
|---|---|
| Slime Leap | recovery depois de cair |
| Bat Dash | após passar reto |
| Golem Slam | depois de bater no chão |
| Mage Cast | durante cast longo |
| Mushroom Spore | depois de soltar esporo |

### 9.1 EnemyVulnerabilityController

```csharp
public class EnemyVulnerabilityController : MonoBehaviour
{
    public bool IsVulnerable { get; private set; }
    public float CurrentDamageMultiplier { get; private set; }

    public void OpenWindow(string sourceActionId, float durationSeconds, float damageMultiplier);
    public void CloseWindow(string sourceActionId);
}
```

### 9.2 Regra de dano

Se o jogador acerta durante a janela:

```text
final damage *= 1.5
```

O multiplicador deve vir dos dados da ação, com default 1.5.

### 9.3 Feedback visual futuro

Quando vulnerável:

- outline/brilho curto;
- sprite piscando;
- ícone pequeno;
- animação de exaustão.

No MVP, pode ser só mudança de cor ou log.

---

## 10. EnemyDataSO reorganizado

`EnemyDataSO` atual deve evoluir para apontar perfis.

```csharp
[CreateAssetMenu(fileName = "EnemyData", menuName = "CindarsHope/Combat/Enemy Data")]
public class EnemyDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;

    [Header("Health")]
    public int MaxHp = 10;

    [Header("Profiles")]
    public EnemyMovementProfileSO MovementProfile;
    public EnemyElementProfileSO ElementProfile;
    public EnemyStatusProfileSO StatusProfile;
    public EnemyActionDataSO[] Actions;

    [Header("Drops")]
    public string DropItemId;
    public int DropAmount = 1;

    [Header("Meta")]
    public EnemyArchetype[] Archetypes;
    public int ThreatLevel = 1;
}
```

### 10.1 EnemyArchetype

```csharp
public enum EnemyArchetype
{
    Chaser,
    Leaper,
    Dasher,
    Kiter,
    Turret,
    Ambusher,
    Swarmer,
    Tank,
    Caster,
    Summoner,
    Support,
    Bomber,
    Guardian,
    Fleeing
}
```

---

## 11. Exemplo completo: Slime

### EnemyDataSO

```text
Id: enemy_slime
DisplayName: Slime
MaxHp: 10
MovementProfile: movement_slime_chaser
ElementProfile: element_profile_slime_basic
Actions:
  - enemy_action_slime_leap
  - enemy_action_contact_damage_basic
DropItemId: item_wood
DropAmount: 1
Archetypes: Leaper, Chaser
ThreatLevel: 1
```

### MovementProfile

```text
Id: movement_slime_chaser
MovementMode: ChaseTarget
MoveSpeed: 1.2
StopDistance: 0.7
DetectionRadius: 5
LeashRadius: 8
```

### Leap action

```text
Id: enemy_action_slime_leap
ActionType: LeapAttack
Priority: 10
MinRange: 0.8
MaxRange: 3.0
PrepareSeconds: 0.30
ActiveSeconds: 0.25
RecoverySeconds: 0.35
CooldownSeconds: 1.50
Damage: 1
Element: Physical
KnockbackForce: 1.5
MoveSpeed: 5.5
MaxTravelDistance: 2.2
OpensVulnerabilityWindow: true
VulnerablePhase: Recovering
VulnerableDamageMultiplier: 1.5
```

---

## 12. Exemplo: inimigo à distância

### Cave Spitter

```text
Id: enemy_cave_spitter
Archetypes: Turret, Caster
MovementMode: KeepDistance
DesiredDistance: 4.0
Actions:
  - spit_poison_projectile
  - flee_short_when_player_close
ElementProfile:
  Poison: 0.5
  Fire: 1.5
StatusApplications:
  Poison 35%, 5s, power 1
Vulnerability:
  durante Recovery após cuspir
```

---

## 13. Exemplo: inimigo mágico

### Shadow Wisp

```text
Id: enemy_shadow_wisp
Archetypes: Kiter, Caster
MovementMode: OrbitTarget
DesiredDistance: 3.5
Actions:
  - shadow_bolt
  - blink_away
ElementProfile:
  Shadow: 0.3
  Light: 1.7
  Physical: 0.6
StatusApplications:
  Weakness 25%, 4s
Vulnerability:
  durante PrepareSeconds do shadow_bolt
```

---

## 14. PRs recomendados

### PR-113 — Enemy architecture contracts

Criar:

- `EnemyArchetype`
- `EnemyMovementMode`
- `DamageElement`
- `StatusEffectType`
- `EnemyActionType`
- `EnemyActionPhase`
- `IEnemyAction`
- `EnemyActionContext`

### PR-114 — Enemy profiles data

Criar:

- `EnemyMovementProfileSO`
- `EnemyElementProfileSO`
- `EnemyStatusProfileSO`
- `EnemyActionDataSO`
- `ElementModifier`
- `EnemyStatusApplication`

### PR-115 — EnemyActionController MVP

Criar controller que:

- escolhe ação por range/prioridade/cooldown;
- respeita ação em execução;
- injeta contexto;
- não usa tags nem Find.

### PR-116 — EnemyMovementController policy-based

Substituir chase fixo por movement profile:

- chase;
- keep distance;
- flee;
- stationary;
- guard area.

### PR-117 — Vulnerability windows

Criar:

- `EnemyVulnerabilityController`;
- multiplicador 1.5 default;
- integração com dano recebido;
- feedback debug.

### PR-118 — Element damage calculation

Criar cálculo:

- base damage;
- element profile;
- vulnerability window;
- status modifiers futuros.

### PR-119 — Status receiver MVP

Criar status mínimos:

- Slow;
- Poison;
- Burn;
- Stun curto.

### PR-120 — Slime Leap Attack usando arquitetura nova

Migrar Slime para:

- `EnemyActionController`;
- `EnemyMovementProfileSO`;
- `EnemyActionDataSO`;
- `EnemyVulnerabilityController`.

### PR-121 — Segundo monstro para validar reutilização

Adicionar um monstro à distância ou kiter para provar que a arquitetura escala.

Sugestão:

- Cave Spitter com projétil venenoso.

### PR-122 — Enemy architecture validator

Validar:

- todo EnemyDataSO tem movement profile;
- todo EnemyDataSO tem pelo menos uma ação;
- ranges/timings positivos;
- element profile válido;
- status applications válidas;
- actions com IDs únicos.

---

## 15. Critérios de aceite da arquitetura

- Criar Slime e Cave Spitter sem scripts específicos gigantes.
- Ambos usam o mesmo `EnemyActionController`.
- Ambos usam movement profile.
- Ambos usam action data.
- Slime tem janela vulnerável no recovery do pulo.
- Cave Spitter mantém distância e ataca à distância.
- Dano elemental modifica dano recebido.
- Status negativo pode ser aplicado ao player.
- Console sem erro vermelho.
- Sem dependência de tags.
- Sem `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` em runtime.

---

## 16. Decisão final

A arquitetura de inimigos deve ser orientada a dados e composição.

O padrão é semelhante ao Java no conceito de interface/implementações, mas em Unity deve ser:

```text
interfaces pequenas
+ MonoBehaviours componíveis
+ ScriptableObjects de dados
+ controller de seleção
+ perfis reutilizáveis
```

Isso permite escalar para 40+ monstros sem criar 40 árvores de herança e sem duplicar lógica de ataque, movimento, elementos e status.
