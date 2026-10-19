# SPEC 14A-FIX2 - Spawn Density, Combat Feedback e Floating Damage Numbers - 2026-05-29

## Resumo

Fix incremental sobre SPEC 14A com três objetivos:

1. **Spawn density +~20%**: `MinEnemiesPerLevel` 12→14, `MaxEnemiesPerLevel` e `DefaultMaxEnemies` 20→24.
2. **Combat logs com nome e EnemyId**: `EnemyContactDamage` agora inclui `SourceName` e `SourceEnemyId` no log.
3. **Floating damage numbers para player**: `PlayerDamagedEvent` publicado após cada hit de contato; `FloatingDamageNumberDisplayer` exibe número em vermelho na posição do player.

## Escopo

### Arquivos alterados

- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` — constantes de spawn ajustadas
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` — default `_maxEnemiesPerLevel` = 24
- `Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs` — `PlayerDamagedEvent` adicionado
- `Assets/_Game/Scripts/Combat/EnemyContactDamage.cs` — log melhorado + publicação de `PlayerDamagedEvent`
- `Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs` — subscribe/unsubscribe `PlayerDamagedEvent`, handler em vermelho

### Arquivos criados

- `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AFix2CombatFeedback.cs`
- `docs/validation/SPEC14A_FIX2_SPAWN_DENSITY_COMBAT_FEEDBACK_VALIDATION_20260529.md` (este arquivo)

## Detalhes das mudanças

### 1. Spawn Density

```csharp
// CaveEnemySpawnPlanner.cs
private const int DefaultMaxEnemies = 24;   // era 20
private const int MinEnemiesPerLevel = 14;  // era 12
private const int MaxEnemiesPerLevel = 24;  // era 20

// CaveRuntimeMaterializer.cs
[SerializeField] private int _maxEnemiesPerLevel = 24;  // era 20
```

O método `ResolveTargetEnemyCount` não foi alterado — a faixa agora é 14–24 em vez de 12–20.

### 2. Combat Log

```csharp
// EnemyContactDamage.cs — antes
Debug.Log($"EnemyContactDamage: dealt {_enemyData.contactDamage} damage to player. HP should update through PlayerManager.");

// depois
Debug.Log($"CombatLog: EnemyContactDamage. SourceName={_enemyData.DisplayName}, SourceEnemyId={_enemyData.enemyId}, Target=Player, Damage={_enemyData.contactDamage}.");
```

### 3. PlayerDamagedEvent

Novo evento em `StatusAndDamageEvents.cs`:

```csharp
public class PlayerDamagedEvent
{
    public int DamageAmount { get; }
    public UnityEngine.Vector3 WorldPosition { get; }
    public string SourceId { get; }
    public string SourceName { get; }
    ...
}
```

Publicado em `EnemyContactDamage.OnTriggerStay2D` logo após `DamageHP`:

```csharp
GameEventBus.Publish(new PlayerDamagedEvent(
    _enemyData.contactDamage,
    collision.transform.position,
    _enemyData.enemyId,
    _enemyData.DisplayName));
```

### 4. Floating Numbers — Player

`FloatingDamageNumberDisplayer` agora subscreve `PlayerDamagedEvent` em `OnEnable` e desinscreve em `OnDisable`. O handler `DisplayPlayerDamage` chama `CreateFloatingNumber` com `Color.red`.

## Regras invioláveis respeitadas

- Sem `GameObject.Find/FindObjectOfType/FindObjectsByType` em runtime.
- Sem sistema paralelo de dano ou event bus.
- Sem novo package.
- Sem serialização de Unity refs em DTO.
- `GameEventBus.Publish/Subscribe` usado para comunicação.
- Unsubscribe em `OnDisable`.

## Fora de escopo (explicitamente não implementado)

- SPEC 18 — não iniciado.
- Healing numbers — não alterado.
- Critical system completo — não alterado.
- UI final de combate — não alterado.
- Boss fights / boss rewards — não alterados.
- Respawn e redistribuição pós-morte — não alterados.

## Validações estáticas executadas

- Leitura de `CaveEnemySpawnPlanner.cs`: constantes confirmadas (14/24/24).
- Leitura de `CaveRuntimeMaterializer.cs`: `_maxEnemiesPerLevel = 24` confirmado.
- Leitura de `StatusAndDamageEvents.cs`: `PlayerDamagedEvent` presente.
- Leitura de `EnemyContactDamage.cs`: log atualizado, `GameEventBus.Publish` confirmado.
- Leitura de `FloatingDamageNumberDisplayer.cs`: subscribe/unsubscribe `PlayerDamagedEvent` e `Color.red` confirmados.

## Validações pendentes

- `dotnet build .\Assembly-CSharp.csproj --no-restore` — ver seção de build.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` — ver seção de build.
- `tools/docs/validate_docs.ps1` — executar após commit.
- `tools/unity/RunUnityCompileValidation.ps1` — pendente; requer Unity Editor fechado ou batchmode.
- Play Mode — pendente; requer Unity Editor aberto.

## Validação do editor

Após compilação no Unity, executar:

```text
CindarsHope → Validation → Validate SPEC 14A-FIX2 - Spawn Density and Combat Feedback
```

Critérios de aprovação em Play Mode:

- Cave Level 1: `CreatedEnemies` entre 14 e 24.
- Player recebe dano de contato: número vermelho flutua acima do player.
- Inimigo recebe dano: número branco/colorido flutua acima do inimigo.
- Log inclui `CombatLog: EnemyContactDamage. SourceName=...`.
- Sem `ArgumentException` ou `NullReferenceException` em combate.
