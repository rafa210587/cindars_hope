---
name: skill-tree-authoring
description: Autoria de nós de skill tree (custo, prereqs, tier gating, capstone), purchase/respec, active slot validation e save. Usar em specs de skill tree, equipamento de skill ativa, respec ou progressão de habilidade.
---

# Skill: Autoria de Skill Tree

O sistema de skill tree do projeto (fable_29, WAVE_INTEGRATION_10/11) é composto de serviços puros (`SkillPurchaseService`, `SkillRespecService`, `SkillTierRules`) e estado serializado (`SkillTreeState`, `SkillTreeSaveData`). O `SkillTreeManager` (MonoBehaviour) coordena todos no runtime e é a interface principal. O `ActiveSkillExecutionController` (teclas 1-4) é o caminho canônico de execução de skills ativas — `ActiveSkillSlots` (teclas R/T/Y/G) foi APOSENTADO e existe apenas para save retrocompatível.

## Quando usar

- Spec adiciona ou modifica nó de skill tree (`SkillNodeDataSO`).
- Spec implementa purchase, rank-up ou respec de skill.
- Spec valida active slot (equipar skill desbloqueada nos slots 1-4).
- Spec que mencione "skill point", "tier", "capstone", "prerequisite", "respec", "active slot".
- Spec de save/load que inclua compras de skill e slots ativos.

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `SkillPurchaseService` | Lógica de purchase/rank-up pura; gates: tier (`SkillTierRules`), prereqs, exclusividade de capstone, level mínimo, pontos disponíveis; publica `SkillNodePurchasedEvent`/`SkillPurchaseFailedEvent` |
| `SkillRespecService` | Respec: 1 free; depois custa 250g (configurável); `TryRespec(state, level, ref gold)` → `SkillTreeState.FullRespec(totalPoints)` |
| `SkillTreeState` | Estado mutable: pontos disponíveis, compras, ranks, slots, variants; `IsPurchased`, `GetRank`, `PointsSpentInTree`, `DynamicRankCap` |
| `SkillTreeSaveData` | DTO de save (tipos simples + ids de nó) |
| `SkillTierRules` | Constantes de tier: `TierPointThresholds` (pontos gastos na árvore para desbloquear tier 2, 3…) |
| `SkillTierEquipGate` | Gate de equipamento por tier (item requer tier 2+) |
| `ActiveSkillSlots` | **APOSENTADO** (sem input ativo); mantido só para save retrocompatível; use `ActiveSkillExecutionController` (1-4) para execução |
| `SkillTreeManager` | MonoBehaviour coordenador; `IsNodePurchased`, `NodeIndex`, `GetTotalSkillPoints`, `AddSkillPoints` |
| `SkillPassiveApplicator` | Aplica modificadores passivos ao player via `SkillModifierHooks`/`SkillEffectAggregator` |
| `SkillRespecService` (const) | `FreeRespecCount = 1`, `DefaultRespecCostGold = 250` |
| `DefaultSkillCatalog` | 69 nós / 5 árvores / 34 skills ativas; fonte de dados do runtime |

## Procedimento

### Criar nó de skill tree

Criar `SkillNodeDataSO` asset. Campos obrigatórios:

```
SkillNodeId: string único (slug, ex.: "node_warrior_heavy_strike_1")
TreeId: "warrior" | "hunter" | "mystic" | "farmer" | "bond"
Tier: int (1..5; tier 1 = always unlocked; tier 2+ requer pontos gastos na árvore)
SkillPointCost: int (1+ por purchase)
MinimumPlayerLevel: int (0 = sem restrição)
PrerequisiteNodeIds: string[] (nodes que devem estar comprados antes)
SkillCategory: Passive | EquippableSkill | MovementAction
UnlockedSkillActionId: string (se EquippableSkill — id do SkillActionSO para os slots 1-4)
CapstoneVariants: List<string> (se capstone exclusivo — lista de variantes; exige escolha no purchase)
```

Adicionar ao `SkillNodeDatabaseSO` (campo `Nodes`).

### Purchase de nó

```csharp
var service = new SkillPurchaseService(skillTreeManager.NodeIndex.Values);
if (service.TryPurchase(nodeId, state, playerLevel, out string reason))
{
    // SkillNodePurchasedEvent publicado internamente
}
else
{
    Debug.Log($"Purchase failed: {reason}");
    // SkillPurchaseFailedEvent publicado internamente
}
```

### Rank-up

```csharp
service.TryRankUp(nodeId, state, out string reason);
// Rank cap dinâmico: state.DynamicRankCap(treeId) — aumenta com tier desbloqueado
```

### Respec

```csharp
var respecService = new SkillRespecService(); // 250g padrão
bool ok = respecService.TryRespec(state, playerLevel, ref gold);
// Se ok: state.FullRespec chamado internamente; SkillDerivedStatsChangedEvent publicado
```

### Active slot (equipar skill 1-4)

O `ActiveSkillExecutionController` (teclas 1-4, patch WI-11) é o caminho ativo. Para equipar via evento:

```csharp
GameEventBus.Publish(new ActiveSkillSlotAssignedEvent(slotIndex, skillActionId));
// ActiveSkillSlots.OnSlotAssigned persiste no estado; ActiveSkillExecutionController lê no Update
```

Validação de "skill desbloqueada?" acontece no `ActiveSkillSlots.TrySetSkillInSlot` via `SkillTreeManager.IsNodePurchased`.

### Save/load

```csharp
// Captura (via SkillTreeManager ou provider):
var saveData = skillTreeManager.CaptureSaveData();   // SkillTreeSaveData (tipos simples)
// Restore:
skillTreeManager.LoadFromSaveData(saveData);
```

`SkillTreeSaveData` contém apenas ids de nó e inteiros — sem refs Unity.

## Testes

- `SkillPurchaseService` e `SkillRespecService` são classes puras → EditMode:
  - Purchase com pontos insuficientes → false + "Not enough skill points."
  - Purchase sem prereq → false + "Missing prerequisite 'X'."
  - Tier gate: `SkillTierRules.IsTierUnlocked(tier, pointsSpent)` testável diretamente.
  - Respec: free count 1; depois deduz ouro; `state.AvailableSkillPoints == totalForLevel`.
- `SkillTreeState` é mutável mas determinístico → EditMode.

## Regras

- **Nunca** usar `ActiveSkillSlots` para nova lógica de input — está APOSENTADO (ver comentário no código).
- Tier gating é **obrigatório** via `SkillTierRules` — não contornar com check manual de "pontos gastos".
- Capstone exclusivo exige `CapstoneVariants` preenchido; `TryPurchase` recusa sem `chosenVariant`.
- Save de skill tree inclui SEMPRE: compras (`IsPurchased`), ranks, pontos disponíveis e slots ativos.
- Passivas derivadas (`SkillPassiveApplicator`) são aplicadas após purchase/respec via `SkillDerivedStatsChangedEvent` — não aplicar manualmente.

## Relacionados

- `(skill: progression-curve-design)` — curva de custo por tier e skill points por nível
- `(skill: ui-projection-pattern)` — tela de skill tree (SkillTreeGameplayPanelController)
- `(skill: save-load-pattern)` — `SkillTreeSaveData` com tipos simples + ids
- `(skill: editmode-test-authoring)` — purchase/respec/tier gate são lógica determinística
- `(rule: unity-architecture)` — eventos via GameEventBus; save DTOs sem refs Unity
