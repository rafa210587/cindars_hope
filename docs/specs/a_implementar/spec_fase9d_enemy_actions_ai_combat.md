# SPEC FUTURA — FASE9D Enemy Actions AI Combat

> Origem histórica: $Source
> Status: A implementar / restante não implementado
> Observação: conteúdo histórico preservado; não duplicar capacidades já consolidadas em specs implementadas.

---

## Escopo preservado

# FASE 9D — Enemy Actions / AI Combat Spec v1.0

> **Status:** spec futura para estruturar ações de monstros.  
> **Base:** FASE 9B/9C — Cave Combat MVP + Tools/Farm/Combat Refinement.  
> **Objetivo:** criar uma arquitetura extensível para monstros executarem ações como pulo, investida, ataque Ã  distância, fuga, patrulha e habilidades especiais.

---

## 1. Decisão de arquitetura

A arquitetura deve ser parecida conceitualmente com Java:

- interface define contrato;
- implementações concretas representam comportamentos;
- controller/orquestrador decide qual ação executar;
- dados ficam fora do código, em ScriptableObject.

Em Unity, evitar herança profunda. Preferir composição:

```text
Enemy GameObject
â”œâ”€â”€ EnemyHealth
â”œâ”€â”€ EnemyChaseController
â”œâ”€â”€ EnemyActionController
â”œâ”€â”€ EnemyLeapAttackAction
â”œâ”€â”€ EnemyContactAttackAction
â””â”€â”€ EnemyRangedAttackAction opcional
```

Cada ação é um componente independente que implementa uma interface comum.

---

## 2. Objetivo do sistema

Permitir que diferentes inimigos tenham listas diferentes de ações.

Exemplos:

| Inimigo | Ações |
|---|---|
| Slime | perseguir, pular no player, voltar, dano por contato |
| Bat | voar, avançar em linha, recuar |
| Rat | correr, morder, fugir com HP baixo |
| Mushroom | ficar parado, cuspir projétil, soltar esporo |
| Golem | andar lento, bater área, empurrar |
| Mage | manter distância, lançar magia, teleport curto |

---

## 3. Princípio principal

A ação do monstro deve ser dividida em:

1. **Decisão** — posso executar agora?
2. **Preparação** — telegraph/aviso.
3. **Execução** — movimento, ataque ou efeito.
4. **Resolução** — aplicar dano, spawnar projectile, mudar estado.
5. **Recovery** — tempo vulnerável/retorno.
6. **Cooldown** — impedir spam.

Esse padrão vale para pulo do Slime, ataque Ã  distância, magia e ataques especiais.

---

## 4. Interface base

### 4.1 IEnemyAction

```csharp
public interface IEnemyAction
{
    string ActionId { get; }
    int Priority { get; }
    bool IsRunning { get; }

    bool CanStart(EnemyActionContext context);
    void StartAction(EnemyActionContext context);
    void TickAction(float deltaTime, EnemyActionContext context);
    void CancelAction(EnemyActionContext context);
}
```

### 4.2 EnemyActionContext

```csharp
public readonly struct EnemyActionContext
{
    public readonly GameObject Enemy;
    public readonly Transform EnemyTransform;
    public readonly Rigidbody2D EnemyRigidbody;
    public readonly Transform Target;
    public readonly float DistanceToTarget;
    public readonly Vector2 DirectionToTarget;
    public readonly EnemyDataSO EnemyData;
}
```

Observação: `EnemyActionContext` pode carregar referências runtime porque não é save data nem evento. Eventos e save continuam usando IDs/tipos simples.

---

## 5. Controller/orquestrador

### 5.1 EnemyActionController

Responsável por:

- manter lista de ações disponíveis;
- consultar `CanStart`;
- escolher ação por prioridade;
- executar `StartAction`;
- chamar `TickAction` enquanto a ação roda;
- bloquear ações conflitantes;
- retomar chase/idle quando a ação termina.

### 5.2 Regras

- Apenas uma ação ofensiva principal por vez.
- Ação em execução pode bloquear chase.
- Ação pode permitir movimento passivo se fizer sentido.
- Ação pode ser cancelável ou não cancelável.
- Ações não devem buscar objetos globais com `Find`.
- Alvo deve ser injetado pelo gerador ou installer de cena.

---

## 6. Tipos de ação

### 6.1 EnemyActionType

```csharp
public enum EnemyActionType
{
    None,
    Chase,
    ContactDamage,
    LeapAttack,
    DashAttack,
    RangedAttack,
    MagicAttack,
    AreaAttack,
    Summon,
    Flee,
    Patrol,
    Guard
}
```

### 6.2 EnemyActionDataSO

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

    [Header("Timing")]
    public float PrepareSeconds = 0.25f;
    public float ActiveSeconds = 0.25f;
    public float RecoverySeconds = 0.25f;
    public float CooldownSeconds = 1f;

    [Header("Damage")]
    public int Damage = 1;
    public float KnockbackForce = 0f;

    [Header("Movement")]
    public float MoveSpeed = 4f;
    public float MaxTravelDistance = 2f;
    public bool ReturnToStartPosition = false;

    [Header("Projectile")]
    public string ProjectileId;
    public float ProjectileSpeed = 6f;
    public float ProjectileLifetimeSeconds = 2f;
}
```

---

## 7. Slime Leap Attack

### 7.1 Comportamento desejado

O Slime deve:

1. detectar player dentro de range;
2. parar por um curto tempo;
3. comprimir visualmente quando houver animação;
4. pular na direção do player;
5. causar dano se tocar durante janela ativa;
6. aterrissar;
7. voltar ao chase ou retornar Ã  posição original, conforme configuração;
8. entrar em cooldown.

### 7.2 Estados internos

```csharp
public enum EnemyActionPhase
{
    Idle,
    Preparing,
    Active,
    Recovering,
    Returning,
    Cooldown
}
```

### 7.3 EnemyLeapAttackAction

```csharp
public class EnemyLeapAttackAction : MonoBehaviour, IEnemyAction
{
    [SerializeField] private EnemyActionDataSO _actionData;
    [SerializeField] private Collider2D _hitbox;

    public string ActionId => _actionData != null ? _actionData.Id : string.Empty;
    public int Priority => _actionData != null ? _actionData.Priority : 0;
    public bool IsRunning { get; private set; }

    public bool CanStart(EnemyActionContext context) { ... }
    public void StartAction(EnemyActionContext context) { ... }
    public void TickAction(float deltaTime, EnemyActionContext context) { ... }
    public void CancelAction(EnemyActionContext context) { ... }
}
```

### 7.4 Dados iniciais sugeridos

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
KnockbackForce: 1.5
MoveSpeed: 5.5
MaxTravelDistance: 2.2
ReturnToStartPosition: false
```

---

## 8. Relação com animação

A lógica da ação deve ser código. A animação apenas representa visualmente os estados.

| Parte | Responsável |
|---|---|
| decidir se pula | código |
| calcular direção | código |
| mover no espaço | código/physics |
| aplicar dano | código |
| cooldown | código |
| squash antes do pulo | animação |
| stretch durante pulo | animação |
| landing visual | animação |
| som/impacto | audio/VFX depois |

Quando sprites existirem, `EnemyLeapAttackAction` pode acionar parâmetros no Animator:

```text
IsPreparingLeap
IsLeaping
IsRecovering
```

---

## 9. Eventos planejados

### EnemyActionStartedEvent

Payload:

- `EnemyId`
- `ActionId`
- `Position`

### EnemyActionResolvedEvent

Payload:

- `EnemyId`
- `ActionId`
- `HitTarget`
- `Damage`
- `Position`

### EnemyActionEndedEvent

Payload:

- `EnemyId`
- `ActionId`
- `WasCancelled`
- `Position`

Regra: eventos não carregam `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`.

---

## 10. PRs recomendados

### PR-113 — Enemy Action contracts

Criar:

- `IEnemyAction`
- `EnemyActionType`
- `EnemyActionPhase`
- `EnemyActionDataSO`
- `EnemyActionContext`
- eventos de EnemyAction

Fora de escopo:

- alterar Slime;
- alterar CaveScene;
- criar animações;
- criar sprites.

### PR-114 — EnemyActionController

Criar controller que:

- recebe lista de ações;
- monta contexto;
- escolhe ação por prioridade;
- executa tick;
- respeita ação em execução;
- não usa busca global.

### PR-115 — Slime Leap Attack MVP

Criar:

- `EnemyLeapAttackAction`
- asset `EnemyAction_Slime_Leap.asset`
- integração no `CreateMvpCaveScene`

Critérios:

- Slime pula quando player está em range.
- Slime não usa só dano por contato passivo.
- Dano ocorre na janela ativa do pulo.
- Cooldown impede spam.
- Console sem erro vermelho.

### PR-116 — Enemy action validator

Expandir validator para:

- validar `EnemyActionDataSO`;
- validar Slime com `EnemyActionController`;
- validar ao menos uma ação configurada;
- validar ranges/timings positivos.

### PR-117 — Enemy actions handoff

Documentar:

- como criar nova ação;
- como plugar em inimigo;
- como balancear ranges/timings;
- checklist de teste.

---

## 11. Padrões para novas ações

Toda ação nova deve responder:

1. Qual é o range mínimo/máximo?
2. Tem telegraph/preparação?
3. Quanto tempo fica ativa?
4. Quando aplica dano?
5. Pode ser cancelada?
6. Bloqueia chase?
7. Tem recovery?
8. Tem cooldown?
9. Usa projectile?
10. Usa animação/VFX opcional?

---

## 12. Critérios de aceite do pacote

- Slime tem pulo funcional por código.
- Ação é data-driven por `EnemyActionDataSO`.
- Ação é pluggable, não hardcoded só para Slime.
- Controller pode escolher entre ações.
- Dano por contato passivo pode continuar, mas o ataque principal do Slime vira Leap.
- Sem dependência de tags.
- Sem `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` em runtime.
- Eventos usam apenas IDs/tipos simples.
- Pronto para Bat/Rat/Mushroom terem ações próprias.

---

## 13. Decisão final

Sim, o modelo deve seguir a ideia de interface com implementações, semelhante ao que faríamos em Java. Em Unity, a forma recomendada é interface + componentes MonoBehaviour + ScriptableObjects de dados + controller de seleção.


## Regra de uso

Antes de implementar, reconciliar este material com docs/specs/implementados/, docs/refinements/implementados/ e o estado real do código.


