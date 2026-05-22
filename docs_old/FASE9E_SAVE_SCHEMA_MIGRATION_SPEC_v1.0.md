# FASE 9E — Save Schema, Migration e Persistência Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** FASE9E_SAVE_SCHEMA_MIGRATION  
> **Base:** Save/Load JSON mínimo + pickups persistentes + Equipment/Hotbar + Damage/Status + Item Taxonomy.  
> **Objetivo:** definir versão de schema, migração, compatibilidade com saves antigos e persistência de inventário, equipamento, hotbar, mundo, pickups, status, farm, cave e progressão do personagem.

---

## 1. Problema

As próximas fases adicionam equipment, hotbar, mãos, active seed, ammo, consumível selecionado, status ativos, atributos, level, XP, stacks reais, cave state e estado de farm/world.

Sem schema versionado, qualquer evolução pode quebrar saves antigos ou criar dados inconsistentes.

---

## 2. Decisões fechadas

| Questão | Decisão |
|---|---|
| SchemaVersion | obrigatório em todo save novo |
| Load com campos ausentes | tolerante, com defaults seguros |
| Migration | incremental, versão por versão |
| Ammo slot sem ammo no inventário | limpar slot no load e quando quantidade chegar a 0 |
| Inimigos comuns da cave | respawnam ao entrar novamente ou no novo dia |
| Status de inimigos comuns | não salva no MVP |
| Status do player | salva |
| Boss/inimigo persistente futuro | pode salvar status |
| FarmSaveData | baseline a partir da cena no primeiro save/load e depois persiste alterações |
| Drop de item | atualiza estado em memória imediatamente; persiste no próximo save normal/troca de cena/dormir |
| Posição do player | última scene + última posição global |
| Inventário | agregado até split/drop parcial; migrar para stacks reais quando split real entrar |
| Level/XP | salvar desde já com defaults seguros |

---

## 3. Princípios de save

- Nunca salvar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider` ou `Rigidbody`.
- Salvar apenas IDs estáveis, números, bools, enums, posições simples e DTOs.
- IDs inválidos limpam estado ou ignoram entrada com warning.
- Save deve continuar carregando mesmo com campos novos ausentes.
- Migration não deve depender de cena Unity.

---

## 4. Versões propostas

| Versão | Conteúdo |
|---:|---|
| v1 | save mínimo atual: player, dia, gold, inventory agregado, pickups |
| v2 | Equipment, Hotbar, LeftHand, RightHand, ActiveSeed, selected consumable |
| v3 | Attributes, Level, XP, unspent points, ActiveStatuses |
| v4 | Farm/world state: plots, crops, trees, resource nodes |
| v5 | Cave state MVP |

---

## 5. SaveData vNext

```csharp
[Serializable]
public class SaveData
{
    public int SchemaVersion = SaveSchema.CurrentVersion;
    public string SavedAtUtc;

    public PlayerSaveData Player;
    public WorldSaveData World;
    public InventorySaveData Inventory;
    public EquipmentSaveData Equipment;
    public HotbarSaveData Hotbar;
    public List<PickupSaveData> Pickups;
    public List<ActiveStatusSaveData> ActiveStatuses;
    public FarmSaveData Farm;
    public CaveSaveData Cave;
}
```

---

## 6. PlayerSaveData

```csharp
[Serializable]
public class PlayerSaveData
{
    public string SceneId;
    public float PositionX;
    public float PositionY;
    public int CurrentHp;
    public int MaxHp;
    public int CurrentHunger;
    public int MaxHunger;

    public int Strength = 1;
    public int Dexterity = 1;
    public int Intelligence = 1;

    public int Level = 1;
    public int CurrentXp = 0;
    public int XpToNextLevel = 100;
    public int UnspentAttributePoints = 0;
    public int UnspentSkillPoints = 0;
}
```

Defaults:

- atributos ausentes: 1;
- level ausente: 1;
- CurrentXp ausente: 0;
- XpToNextLevel ausente: 100;
- pontos não gastos ausentes: 0;
- SceneId inválido: fallback para FarmScene.

---

## 7. PlayerProgressionSaveData opcional

Implementação pode separar progressão:

```csharp
[Serializable]
public class PlayerProgressionSaveData
{
    public int Level = 1;
    public int CurrentXp = 0;
    public int XpToNextLevel = 100;
    public int UnspentAttributePoints = 0;
    public int UnspentSkillPoints = 0;
}
```

A fórmula de XP, level up, distribuição de pontos e skills será definida em outra spec. O save já reserva os campos para evitar nova quebra de schema.

---

## 8. InventorySaveData

Inventário agregado atual:

```csharp
[Serializable]
public class InventoryEntrySaveData
{
    public string ItemId;
    public int Amount;
}
```

Inventário por stacks reais futuro:

```csharp
[Serializable]
public class InventoryStackSaveData
{
    public string StackId;
    public string ItemId;
    public int Amount;
    public int SlotIndex;
}
```

Decisão:

- manter agregado até split/drop parcial real;
- quando split real entrar, migration converte entries agregadas para uma stack por item.

---

## 9. EquipmentSaveData

```csharp
[Serializable]
public class EquipmentSaveData
{
    public string EquippedToolId;
    public string EquippedPrimaryWeaponId;
    public string EquippedMagicId;
    public string EquippedAmmoItemId;
    public string EquippedConsumableItemId;
}
```

Regras:

- se ID inválido, limpar slot;
- se Bow equipado e Magic também salvo, Bow vence e Magic é limpo;
- se ammo não existe no inventário, limpar ammo slot.

---

## 10. HotbarSaveData

```csharp
[Serializable]
public class HotbarSaveData
{
    public List<string> SlotItemIds = new List<string>(); // tamanho 6
    public string LeftHandItemId;
    public string RightHandItemId;
    public string ActiveSeedId;
    public string SelectedConsumableItemId;
}
```

Regras:

- hotbar tem 6 slots;
- slots inválidos são limpos;
- ActiveSeedId precisa ser Seed;
- SelectedConsumableItemId precisa ser Consumable;
- se item não existe mais no inventário, limpar seleção.

---

## 11. PickupSaveData

```csharp
[Serializable]
public class PickupSaveData
{
    public string PickupId;
    public string SceneId;
    public int PickupIndex;
    public string ItemId;
    public int Amount;
    public float PositionX;
    public float PositionY;
    public bool IsCollected;
    public bool WasDroppedByPlayer;
}
```

Regras:

- pickups do cenário e drops do jogador usam o mesmo DTO;
- drop do jogador gera PickupId estável;
- ItemPickup não deve destruir GameObject para persistência, deve usar `IsCollected`;
- item ID inválido no load ignora pickup com warning.

---

## 12. ActiveStatusSaveData

```csharp
[Serializable]
public class ActiveStatusSaveData
{
    public string TargetId;
    public string SourceId;
    public string SceneId;
    public StatusEffectType StatusType;
    public DamageElement Element;
    public float RemainingSeconds;
    public int Power;
}
```

Regras:

- status expirado não salva;
- player status sempre pode ser restaurado;
- enemy status só deve restaurar para inimigos persistentes/bosses futuros;
- status de inimigos comuns da cave é ignorado no MVP.

---

## 13. FarmSaveData

```csharp
[Serializable]
public class FarmSaveData
{
    public List<FarmPlotSaveData> Plots;
    public List<TreeStateSaveData> Trees;
    public List<ResourceNodeSaveData> ResourceNodes;
}
```

```csharp
[Serializable]
public class FarmPlotSaveData
{
    public string PlotId;
    public bool IsTilled;
    public bool IsWatered;
    public string SeedId;
    public string CropId;
    public int GrowthDay;
    public int DaysUntilHarvest;
}
```

Baseline é criado a partir da cena no primeiro save/load e depois alterações persistem.

---

## 14. CaveSaveData

```csharp
[Serializable]
public class CaveSaveData
{
    public int CurrentLayer;
    public List<string> OpenedChestIds;
    public List<string> DepletedNodeIds;
    public List<string> DefeatedPersistentEnemyIds;
}
```

Decisão MVP:

- inimigos comuns respawnam ao entrar novamente ou no novo dia;
- mined nodes e chests podem persistir quando forem adicionados;
- boss/miniboss futuro deve salvar estado.

---

## 15. Migration pipeline

```csharp
public static class SaveMigrationPipeline
{
    public static SaveData Migrate(SaveData data)
    {
        while (data.SchemaVersion < SaveSchema.CurrentVersion)
        {
            data = MigrateOneVersion(data);
        }
        return data;
    }
}
```

Regras:

- cada migração sobe apenas 1 versão;
- migration só transforma DTO;
- logs registram versão origem/destino.

---

## 16. Defaults por migração

### v1 -> v2

Adicionar Equipment vazio, Hotbar com 6 slots vazios, LeftHand/RightHand vazios, ActiveSeed vazio e SelectedConsumable vazio.

### v2 -> v3

Adicionar Strength/Dexterity/Intelligence = 1, Level = 1, CurrentXp = 0, XpToNextLevel = 100, UnspentAttributePoints = 0, UnspentSkillPoints = 0 e ActiveStatuses vazio.

### v3 -> v4

Adicionar FarmSaveData vazio ou derivado da cena quando primeira vez.

### v4 -> v5

Adicionar CaveSaveData default com CurrentLayer = 1 e listas vazias.

---

## 17. Load validation

Após migration, validar:

- IDs de inventory/equipment/hotbar/pickups;
- active seed;
- selected consumable;
- status target/source quando possível;
- schema version atual;
- level >= 1;
- CurrentXp >= 0;
- XpToNextLevel > 0;
- UnspentAttributePoints >= 0;
- UnspentSkillPoints >= 0.

Quando inválido: limpar campo, ignorar entrada se necessário, logar warning e não quebrar load inteiro.

---

## 18. Save timing

Salvar em:

- ação manual/debug save;
- dormir/avançar dia;
- troca de cena importante;
- fechamento controlado futuro.

Drop de item atualiza estado em memória imediatamente e persiste no próximo save normal, troca de cena ou dormir.

---

## 19. Critérios de aceite

- Todo save novo tem SchemaVersion atual.
- Save antigo sem Equipment/Hotbar/Status/Progression carrega com defaults seguros.
- Migração ocorre versão por versão.
- IDs inválidos limpam slot/entrada e geram warning.
- Tool, weapon, magic, ammo e consumable equipados persistem.
- Hotbar 6 slots, LeftHand, RightHand e ActiveSeed persistem.
- Level, CurrentXp, XpToNextLevel, UnspentAttributePoints e UnspentSkillPoints persistem.
- Player status salva e restaura duração restante.
- Pickups coletados e drops do jogador persistem.
- Plots, crops, watered/tilled state e trees persistem.
- Inimigos comuns da cave têm regra clara de respawn.

---

## 20. Tasks sugeridas

- PR-155 — Save schema version contracts.
- PR-156 — Save migration v1-v2 equipment/hotbar.
- PR-157 — Save migration v2-v3 attributes/progression/status.
- PR-158 — Save migration v3-v4 farm/world state.
- PR-159 — Save migration v4-v5 cave state.
- PR-160 — Load validation hardening.
- PR-161 — Save/load smoke tests handoff.

---

## 21. Decisão final

O save deve ser versionado agora para suportar as próximas fases sem retrabalho. Mesmo que level up ainda ganhe uma spec própria, os campos de Level/XP/pontos entram desde já com defaults seguros.
