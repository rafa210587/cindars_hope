# FASE 9C — Tools, Farm Actions e Combat Refinement v1.0

> **Status:** spec futura aprovada para orientar próximas waves.  
> **Base:** `dev` após Farm/Town/Cave/Combat MVP.  
> **Tipo:** documento de design técnico + backlog executável.  
> **Não implementa runtime:** este arquivo define a estratégia, contratos, critérios e ordem de PRs.

---

## 1. Objetivo

Refinar os sistemas que hoje estão funcionais, mas ainda simplificados:

1. **Plantio** — sair de plantio automático de seed para plantio controlado por ferramenta, seed ativa e regras claras.
2. **Colheita** — diferenciar colheita manual, colheita com ferramenta e bônus por tier.
3. **Árvores** — exigir machado para corte real e limitar fallback sem ferramenta.
4. **Pesca/mineração/coleta** — consolidar ações de mundo sob um modelo único de ferramenta.
5. **Ferramentas** — introduzir `ToolDataSO`, `ToolTier`, `ToolRequirement`, `EquipmentManager` e save/load de equipamento.
6. **Combate** — evoluir de soco hardcoded para armas equipáveis.
7. **Esquiva** — adicionar dodge lateral/para trás com cooldown e janela curta de invulnerabilidade.
8. **Armas** — suportar três tipos: corpo a corpo, distância física e magia à distância.

---

## 2. Estado real atual

### 2.1 Sistemas já existentes

O repo já possui:

- `FarmPlot` com estados `Empty`, `Growing`, `Ready`.
- `TreeNode` cortável com hits e madeira.
- `FishingSpot` básico exigindo item por ID.
- `InventoryManager` por IDs estáveis.
- `ItemDataSO` com `Category` e `IsEquippable`.
- `SaveManager` JSON com scene tracking.
- `GameBootstrap` persistente.
- `CaveScene` com Slime.
- `EnemyDataSO`, `EnemyHealth`, `EnemyChaseController`, `EnemyContactDamage`.
- `PlayerAttackController` com ataque melee simples na tecla `J`.
- `DamageRequest`, `KnockbackRequest`, `HitFlashController`, `KnockbackController`.

### 2.2 Gaps atuais

| Sistema | Gap |
|---|---|
| Plantio | Seed é escolhida automaticamente; sem ferramenta ativa; sem preparo de solo. |
| Colheita | Não diferencia mão/ferramenta/tier; sem bônus de yield. |
| Árvores | Não exige machado; qualquer interação progride corte. |
| Pesca | Exige item por string, mas não usa `ToolDataSO`. |
| Equipamento | Não há `EquipmentManager`; ferramenta/arma não são slots persistidos. |
| Combate | Ataque é soco hardcoded; não há arma equipada. |
| Ranged | Não há projétil físico. |
| Magic | Não há magia/projétil mágico. |
| Dodge | Não há esquiva lateral/para trás. |

---

## 3. Diretriz de design

### 3.1 Ferramenta como progressão

Ferramentas devem ser parte central da progressão. Uma ferramenta define:

- se a ação é permitida;
- quanto ela rende;
- quantos hits/tempo exige;
- quanto custo de fome/stamina aplica;
- se há área de efeito;
- quais recursos ficam desbloqueados;
- quais bônus são aplicados.

### 3.2 Sem ferramenta correta

Sem a ferramenta correta, o jogador não deve conseguir substituir totalmente a ferramenta. Pode haver fallback mínimo para evitar frustração inicial, mas esse fallback deve ser pequeno, limitado e não explorável.

| Ação | Sem ferramenta correta | Resultado esperado |
|---|---|---|
| Cortar árvore | Sem machado | Coleta fallback mínimo, sem reduzir a árvore. |
| Colher crop simples | Sem foice | Yield mínimo, sem bônus. |
| Colher crop raro | Sem foice adequada | Falha ou perde parte do yield. |
| Plantar seed básica | Sem enxada | Permitido com penalidade pequena. |
| Plantar seed avançada | Sem enxada | Bloqueado. |
| Minerar | Sem picareta | Não coleta minério real. |
| Pescar | Sem vara | Bloqueado. |

### 3.3 Tiers

| Tier | Nome | Função |
|---:|---|---|
| 0 | Improvised | fallback fraco, temporário, baixa eficiência. |
| 1 | Basic | libera ação mínima. |
| 2 | Copper | reduz custo/tempo/hits ou adiciona pequeno bônus. |
| 3 | Iron | melhora eficiência e desbloqueia recursos médios. |
| 4 | Steel | melhora área, yield e velocidade. |
| 5 | Arcane | interage com recursos mágicos, luas e efeitos especiais. |

---

## 4. Modelo de ferramentas

### 4.1 ToolType

```csharp
public enum ToolType
{
    None,
    Hoe,
    Sickle,
    Axe,
    Pickaxe,
    FishingRod,
    WateringCan,
    ForagingGlove
}
```

### 4.2 ToolTier

```csharp
public enum ToolTier
{
    Improvised = 0,
    Basic = 1,
    Copper = 2,
    Iron = 3,
    Steel = 4,
    Arcane = 5
}
```

### 4.3 ToolDataSO

```csharp
[CreateAssetMenu(fileName = "ToolData", menuName = "CindarsHope/Data/Tool")]
public class ToolDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ItemDataSO Item;
    public ToolType ToolType;
    public ToolTier Tier;

    [Header("Action")]
    public int ActionPower = 1;
    public float ActionCooldownSeconds = 0.4f;
    public float HungerCostMultiplier = 1f;

    [Header("Efficiency")]
    public float EfficiencyMultiplier = 1f;
    public float YieldMultiplier = 1f;
    public int FlatYieldBonus = 0;

    [Header("Area")]
    public int AreaWidth = 1;
    public int AreaHeight = 1;

    [Header("Combat")]
    public bool CanUseAsWeapon = false;
    public int ToolDamage = 1;
}
```

### 4.4 ToolRequirement

```csharp
[Serializable]
public class ToolRequirement
{
    public ToolType RequiredToolType;
    public ToolTier MinimumTier = ToolTier.Basic;

    public bool AllowFallbackWithoutTool;
    public string FallbackItemId;
    public int FallbackAmount = 1;
    public bool FallbackConsumesAction = true;
    public bool FallbackProgressesTarget = false;

    public string FailureMessage;
}
```

### 4.5 ToolDatabaseSO

```csharp
[CreateAssetMenu(fileName = "ToolDatabase", menuName = "CindarsHope/Database/Tools")]
public class ToolDatabaseSO : DataRegistrySO<ToolDataSO> {}
```

---

## 5. EquipmentManager

### 5.1 Responsabilidade

Criar `EquipmentManager` persistente para:

- manter ferramenta ativa;
- manter arma ativa;
- validar item equipado;
- resolver `ToolDataSO` por ID;
- resolver `WeaponDataSO` por ID;
- expor dados para ações de farm, mundo e combate;
- persistir no save/load;
- publicar eventos de troca.

### 5.2 API esperada

```csharp
public class EquipmentManager : MonoBehaviour
{
    public string EquippedToolId { get; private set; }
    public string EquippedWeaponId { get; private set; }

    public bool TryEquipTool(string toolItemId);
    public bool TryEquipWeapon(string weaponItemId);

    public bool TryGetEquippedTool(out ToolDataSO toolData);
    public bool TryGetEquippedWeapon(out WeaponDataSO weaponData);

    public bool HasRequiredTool(ToolRequirement requirement, out ToolDataSO toolData);

    public EquipmentSaveData CaptureSaveData();
    public void RestoreFromSaveData(EquipmentSaveData saveData);
}
```

### 5.3 Save

```csharp
[Serializable]
public class EquipmentSaveData
{
    public string EquippedToolId;
    public string EquippedWeaponId;
}
```

Adicionar em `GameSaveData`:

```csharp
public EquipmentSaveData Equipment = new EquipmentSaveData();
```

### 5.4 Eventos

```csharp
public readonly struct ToolEquippedEvent
{
    public readonly string ToolId;
    public readonly ToolType ToolType;
    public readonly int Tier;
}
```

```csharp
public readonly struct WeaponEquippedEvent
{
    public readonly string WeaponId;
    public readonly WeaponType WeaponType;
}
```

---

## 6. Farm Actions Refinement

### 6.1 Plantio

#### Regra futura

Plantio deve depender de:

- plot vazio/preparado;
- seed selecionada;
- seed disponível no inventário;
- ferramenta ativa ou fallback;
- regras de crescimento da seed.

#### FarmPlotState recomendado

```csharp
public enum FarmPlotState
{
    Empty,
    Prepared,
    Growing,
    Ready,
    Withered
}
```

#### Regras de plantio

| Ferramenta | Efeito |
|---|---|
| Sem Hoe | Só planta seed básica; aplica +1 dia de crescimento ou yield menor. |
| Hoe Basic | Planta normalmente. |
| Hoe Copper | Reduz custo de fome ou pequena chance futura de preservar seed. |
| Hoe Iron | Prepara/plantar múltiplos plots futuramente. |
| Hoe Steel+ | Área maior. |

#### Critério MVP

- O comportamento automático `seed_wheat` antes de `seed_carrot` deve ser removido.
- Seed deve vir de seleção explícita mínima: seed ativa, hotbar futura ou UI textual temporária.
- Sem Hoe: só `seed_wheat` pode ser plantada, com penalidade.

### 6.2 Colheita

#### Ferramenta

`Sickle`.

| Ferramenta | Efeito |
|---|---|
| Sem Sickle | Colhe yield mínimo de crops simples. |
| Sickle Basic | Yield normal do `SeedDataSO`. |
| Sickle Copper | `FlatYieldBonus +1` para crops básicos. |
| Sickle Iron | `YieldMultiplier`. |
| Sickle Steel+ | Colhe em área pequena futuramente. |

#### Critério MVP

- Colheita manual ainda permitida para trigo/cenoura.
- `Sickle Basic` mantém comportamento atual.
- `Sickle Copper+` adiciona bônus simples configurável.

### 6.3 Árvores

#### Ferramenta

`Axe`.

| Ferramenta | Efeito |
|---|---|
| Sem Axe | Coleta fallback mínimo, sem `HitsTaken`. |
| Axe Basic | Aplica 1 hit por ação. |
| Axe Copper | Aumenta madeira ou reduz hits. |
| Axe Iron | `ActionPower = 2`. |
| Axe Steel+ | Futuro corte em área. |

#### Alterações em TreeDataSO

```csharp
public ToolRequirement ChopRequirement;
public int BaseWoodAmount;
public int RequiredActionPower;
public int FallbackWoodAmount;
public string FallbackItemId;
```

#### Critério MVP

- Sem Axe: jogador recebe no máximo 1 recurso simples por árvore/dia ou por cooldown, sem cortar a árvore.
- Com Axe: comportamento atual equivalente.
- Com tier maior: mais `ActionPower` ou mais yield.

### 6.4 Pesca

Migrar de `_requiredToolId` em `FishingSpot` para `ToolRequirement`.

| Ferramenta | Efeito |
|---|---|
| Sem FishingRod | Não pesca. |
| Rod Basic | Pesca comum. |
| Rod Copper | Menor tempo ou +chance futura. |
| Rod Iron+ | Peixes raros futuramente. |

### 6.5 Mineração futura

`ResourceNode` da Cave deve seguir o mesmo modelo:

- `Pickaxe` necessária para minério.
- Sem pickaxe: talvez pedra comum, sem consumir node.
- Tiers desbloqueiam minério superior.

---

## 7. Action Resolution Pattern

Para evitar duplicação em `FarmPlot`, `TreeNode`, `FishingSpot` e `ResourceNode`, criar um serviço puro:

```csharp
public readonly struct ToolActionContext
{
    public readonly string ActionId;
    public readonly ToolRequirement Requirement;
    public readonly ToolDataSO EquippedTool;
}
```

```csharp
public readonly struct ToolActionResult
{
    public readonly bool Success;
    public readonly bool UsedCorrectTool;
    public readonly bool UsedFallback;
    public readonly int ActionPower;
    public readonly float YieldMultiplier;
    public readonly int FlatYieldBonus;
    public readonly string Message;
}
```

```csharp
public static class ToolActionResolver
{
    public static ToolActionResult Resolve(ToolActionContext context);
}
```

Benefício:

- regra de ferramenta fica centralizada;
- ações de farm/mundo ficam menores;
- fácil testar depois;
- evita hardcode duplicado.

---

## 8. Combate com armas

### 8.1 WeaponType

```csharp
public enum WeaponType
{
    Melee,
    RangedPhysical,
    RangedMagic
}
```

### 8.2 WeaponDataSO

```csharp
[CreateAssetMenu(fileName = "WeaponData", menuName = "CindarsHope/Combat/Weapon")]
public class WeaponDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ItemDataSO Item;
    public WeaponType WeaponType;

    [Header("Damage")]
    public int Damage = 1;
    public float Range = 0.8f;
    public float CooldownSeconds = 0.4f;
    public float KnockbackForce = 2.5f;

    [Header("Projectile")]
    public string ProjectilePrefabId;
    public float ProjectileSpeed = 6f;
    public float ProjectileLifetimeSeconds = 2f;

    [Header("Costs")]
    public string AmmoItemId;
    public int AmmoCost = 0;
    public int MpCost = 0;
    public int HungerCost = 0;

    [Header("Magic")]
    public string ElementId;
}
```

### 8.3 WeaponDatabaseSO

```csharp
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "CindarsHope/Database/Weapons")]
public class WeaponDatabaseSO : DataRegistrySO<WeaponDataSO> {}
```

### 8.4 PlayerCombatController

Substituir gradualmente `PlayerAttackController` por `PlayerCombatController`.

Responsabilidades:

- ler tecla de ataque;
- buscar arma equipada;
- se não houver arma, usar `Unarmed`;
- disparar ataque de acordo com `WeaponType`;
- aplicar cooldown;
- publicar evento de resultado.

### 8.5 Melee

MVP:

- overlap circular ou arco simples;
- usa `DamageRequest`;
- dano, range, cooldown e knockback vêm do `WeaponDataSO`.

### 8.6 RangedPhysical

MVP:

- arco ou besta dispara `ProjectileController`;
- projétil se move em linha reta;
- ao bater em `EnemyHealth`, chama `TakeDamage(DamageRequest)`;
- opcionalmente consome ammo.

### 8.7 RangedMagic

MVP:

- varinha/cajado dispara projétil mágico;
- usa `MpCost` se MP existir;
- se MP ainda não existir, usar cooldown e custo de fome temporário;
- preparar futura interação com elemento/lua/bioma.

---

## 9. Dodge

### 9.1 Input

| Input | Resultado |
|---|---|
| `Space` | Dodge para trás baseado no facing atual. |
| `A + Space` | Dodge lateral esquerda. |
| `D + Space` | Dodge lateral direita. |
| `S + Space` | Dodge para trás. |
| `W + Space` | Opcional: dodge para frente curto; pode ficar desabilitado no MVP. |

### 9.2 Regras

- duração: 0.18s a 0.25s;
- cooldown: 0.7s a 1.0s;
- invulnerabilidade parcial: primeiros 0.12s a 0.18s;
- bloqueia ataque durante dodge;
- bloqueia interação durante dodge;
- não atravessa colisores sólidos;
- usa `Rigidbody2D.MovePosition`;
- deve ser simples e previsível.

### 9.3 PlayerDodgeController

```csharp
public class PlayerDodgeController : MonoBehaviour
{
    public bool IsDodging { get; private set; }
    public bool IsInvulnerable { get; private set; }
}
```

### 9.4 Eventos

```csharp
public readonly struct PlayerDodgeStartedEvent
{
    public readonly Vector2 Direction;
    public readonly float DurationSeconds;
}
```

```csharp
public readonly struct PlayerDodgeEndedEvent
{
    public readonly Vector2 Position;
}
```

---

## 10. Eventos novos planejados

| Evento | Quando publicar |
|---|---|
| `ToolEquippedEvent` | ferramenta equipada. |
| `WeaponEquippedEvent` | arma equipada. |
| `ToolActionResolvedEvent` | ação de ferramenta resolvida com sucesso/fallback/falha. |
| `PlayerAttackStartedEvent` | ataque iniciado. |
| `PlayerAttackResolvedEvent` | ataque acertou/errou. |
| `ProjectileSpawnedEvent` | projétil criado. |
| `ProjectileHitEvent` | projétil acertou alvo. |
| `SpellCastEvent` | magia disparada. |
| `PlayerDodgeStartedEvent` | dodge iniciado. |
| `PlayerDodgeEndedEvent` | dodge finalizado. |

---

## 11. IDs e convenções

| Tipo | Prefixo recomendado | Exemplo |
|---|---|---|
| Item ferramenta | `item_tool_` | `item_tool_axe_basic` |
| Tool data | `tool_` | `tool_axe_basic` |
| Item arma | `item_weapon_` | `item_weapon_sword_wood` |
| Weapon data | `weapon_` | `weapon_sword_wood` |
| Projectile | `projectile_` | `projectile_arrow_basic` |
| Spell | `spell_` | `spell_spark_basic` |

Regra: save sempre guarda IDs simples, nunca referência Unity.

---

## 12. Sequência de PRs recomendada

### Wave 9C-A — Tools foundation

#### PR-100 — Tool contracts

Criar:

- `ToolType.cs`
- `ToolTier.cs`
- `ToolDataSO.cs`
- `ToolDatabaseSO.cs`
- `ToolRequirement.cs`
- `ToolActionResolver.cs`
- eventos de tool.

Critérios:

- compila;
- sem gameplay alterado;
- CreateAssetMenu funcionando;
- sem alterações em cenas.

#### PR-101 — Tool assets MVP

Criar itens e tool data:

- Hoe Basic;
- Sickle Basic;
- Axe Basic;
- Pickaxe Basic;
- Fishing Rod Basic;
- ToolDatabase.

Atualizar validator para ToolDatabase.

#### PR-102 — EquipmentManager mínimo

Criar:

- `EquipmentManager`;
- save/load de equipment;
- HUD debug mostra ferramenta/arma;
- equipamento inicial via `PlayerDataSO` ou auto-equip por inventário inicial.

### Wave 9C-B — Farm actions

#### PR-103 — Plantio com ferramenta

- remover seed automática;
- usar seed ativa/debug selection;
- aplicar Hoe/fallback;
- publicar `ToolActionResolvedEvent`.

#### PR-104 — Colheita com ferramenta

- aplicar Sickle/fallback;
- yield por tier;
- manter compatibilidade com trigo/cenoura.

#### PR-105 — TreeNode com Axe

- sem Axe não corta árvore;
- fallback mínimo sem progresso;
- Axe tier altera `ActionPower`/yield.

#### PR-106 — FishingSpot com ToolRequirement

- migrar de item ID direto para ToolRequirement;
- FishingRod tier define eficiência futura.

### Wave 9C-C — Weapon foundation

#### PR-107 — Weapon contracts

Criar:

- `WeaponType.cs`
- `WeaponDataSO.cs`
- `WeaponDatabaseSO.cs`
- eventos de weapon/attack.

#### PR-108 — Weapon slot no EquipmentManager

- save/load de arma equipada;
- HUD debug exibe arma;
- arma inicial opcional.

#### PR-109 — PlayerCombatController melee

- substituir ataque hardcoded por data-driven;
- `Unarmed` mantém comportamento atual;
- `Melee` usa `WeaponDataSO`.

### Wave 9C-D — Dodge e range

#### PR-110 — PlayerDodgeController

- Space dodge;
- lateral/traseiro;
- cooldown;
- invulnerability curta;
- integração com contato de inimigo.

#### PR-111 — RangedPhysical projectile

- `WoodenBow`;
- `ProjectileController`;
- dano por colisão;
- cooldown/range data-driven.

#### PR-112 — RangedMagic projectile

- `BasicWand`;
- projétil mágico;
- custo por MP se existir, senão fallback por cooldown/fome;
- elemento preparado por `ElementId`.

---

## 13. Critérios de aceite do pacote 9C

Ao final:

- jogador tem ferramenta ativa;
- plantio respeita ferramenta/fallback;
- colheita aplica ferramenta/yield;
- árvore exige machado para corte real;
- pesca usa `ToolDataSO`;
- ferramenta tem tier e eficiência;
- save/load preserva ferramenta e arma;
- combate usa arma equipada;
- existem armas melee, ranged physical e ranged magic;
- dodge lateral/traseiro funciona;
- Slime continua validando loop de combate;
- Console sem erro vermelho;
- nenhum runtime usa `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`.

---

## 14. Fora de escopo desta fase

- Durabilidade de ferramenta.
- Skill tree.
- Combos avançados.
- Pathfinding avançado.
- Procedural cave.
- Arte final.
- UI final completa.
- Balanceamento final de economia.
- Input System migration completa.

---

## 15. Atualizações documentais relacionadas

Documentos que devem referenciar esta fase:

- `README.md`
- `docs/NEXT_WAVES_ROADMAP_v1.0.md`
- `docs/ARCH_fase4_v2.2.md`
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`
- `docs/GDD_v2.6.md`
- `docs/FASE6_INDEX_global_v1.2.md`
- `docs/FASE9B_CAVE_COMBAT_MVP_v1.0.md`
- `AGENTS.md`
- `CLAUDE.md`
- `PROJECT_LOG.md`

---

## 16. Decisão final

A FASE 9C deve vir antes de UI real final e antes de arte final, porque altera regras centrais de interação. Implementar UI/arte antes de estabilizar ferramentas, arma equipada e dodge criaria retrabalho.
