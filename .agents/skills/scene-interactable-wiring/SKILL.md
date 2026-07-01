---
name: scene-interactable-wiring
description: Adiciona um novo objeto IInteractable a uma scene usando CreateScene editor scripts, reward strategy e depletion guard. Use em qualquer tarefa que adicione crops, resources, fishing spots, chests ou outros objetos interativos em FarmScene, CaveScene ou TownScene.
---

# Skill: Wiring de Interactable em Scene

Esta skill cobre a adição de objetos `IInteractable` a uma scene seguindo o padrão de CreateScene editor scripts, com reward strategy e depletion guard.

## Quando usar

A tarefa exige:
- Adicionar um novo tipo de objeto interativo (tree, rock, forage, chest, spring, fishing spot)
- Fazer o wiring da entrega de reward para interação do player (item drop, currency, XP)
- Adicionar depletion state (resource deplete após harvest, respawna depois de um tempo)
- Registrar um interactable em um CreateScene editor script

## Quando NÃO usar

- Interactable é NPC dialogue/shop → use `npc-dialogue-authoring`
- Interactable é um UI modal → use `ui-modal-stack`
- A mudança é puramente em dados de ScriptableObject (sem comportamento runtime) → use `combat-data-wiring` ou `bootstrap-wiring`

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. Report de interactable anterior relevante (ex.: `docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md`)

---

## Arquitetura central

### Contrato IInteractable

```csharp
public interface IInteractable
{
    bool CanInteract { get; }
    string InteractionPrompt { get; }
    void Interact(GameObject interactor);
}
```

**Sempre implemente esta interface.** Não crie sistemas de interação paralelos.

### Interactables existentes para reusar ou estender

| Tipo | Classe | Arquivo |
|------|-------|------|
| Crop | `FarmPlot` | `Assets/_Game/Scripts/Farm/FarmPlot.cs` |
| Tree | `TreeResourceInteractable` | `Assets/_Game/Scripts/Farm/Interactables/` |
| Rock | `RockResourceInteractable` | mesmo |
| Forage | `ForageResourceInteractable` | mesmo |
| Fishing | `FishingSpot` | `Assets/_Game/Scripts/Farm/FishingSpot.cs` |

Se o novo tipo for estruturalmente idêntico a um destes, **estenda** ele (via subclass ou config parameter). NÃO crie uma classe paralela.

---

## Reward strategy

### Caminho canônico de reward

```csharp
// Always go through InventoryManager.AddItem
var inventory = GameBootstrap.Instance?.InventoryManager;
if (inventory == null) return;

var added = inventory.AddItem(itemId, amount);
if (added)
{
    SetDepleted(true);   // only deplete if AddItem succeeded
    PublishFeedback(itemId, amount);
}
```

**Regra**: A depletion acontece SOMENTE no sucesso do `AddItem`. Se o inventory estiver cheio ou o item ID for inválido, o resource NÃO deplete — o player pode tentar de novo.

### Guard de ClampedAmount

```csharp
var actualAmount = Mathf.Clamp(amount, 1, maxStack);
var added = inventory.AddItem(itemId, actualAmount);
```

Sempre faça clamp do amount. Nunca passe valores sem clamp.

---

## Depletion state

```csharp
public bool IsDepleted { get; private set; }

public bool CanInteract => !IsDepleted;

private void SetDepleted(bool depleted)
{
    IsDepleted = depleted;
    // Optional: visual update (hide sprite, show stump, etc.)
    UpdateVisual(depleted);
    
    if (depleted && _respawnTime > 0f)
        StartCoroutine(RespawnAfterDelay(_respawnTime));
}

private IEnumerator RespawnAfterDelay(float seconds)
{
    yield return new WaitForSeconds(seconds);
    SetDepleted(false);
}
```

O respawn time é `TODO_INTEGRATION_NOT_FINAL` a menos que a spec o defina.

---

## Feedback

```csharp
private void PublishFeedback(string itemId, int amount)
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent($"Coletado: {itemId} x{amount}"));
    // If item pickup event exists:
    // GameEventBus.Publish(new ItemPickedUpEvent(itemId, amount));
}
```

---

## Wiring via CreateScene (NÃO scene YAML)

**Nunca edite o YAML do `.unity` diretamente.** Registre novos interactables no CreateScene editor script relevante:

```csharp
// In CreateMvpFarmScene.cs (or equivalent)
private void CreateResourceInteractables()
{
    CreateTreeResource("TreeResource_01", new Vector2(3f, -2f));
    CreateTreeResource("TreeResource_02", new Vector2(5f, -4f));
    // Add new interactable here:
    CreateRockResource("RockResource_01", new Vector2(8f, -1f));
}

private void CreateTreeResource(string name, Vector2 position)
{
    var go = new GameObject(name);
    go.transform.position = position;
    go.transform.SetParent(_resourceContainer);
    var interactable = go.AddComponent<TreeResourceInteractable>();
    interactable.Configure("item_material_wood", amount: 3, respawnTime: 120f);
}
```

Depois de adicionar ao CreateScene, documente nas wiring instructions:
```
docs/validation/WAVE_INTEGRATION_<N>_HUMAN_UNITY_<SLUG>_WIRING_INSTRUCTIONS.md
```

---

## Quando reusar FishingSpot vs. criar nova

Use o `FishingSpot.cs` existente se:
- A interação é "ficar perto da água e apertar interact"
- O reward são itens de peixe (fish)
- A posição é um ponto fixo adjacente à água

Crie uma nova classe se:
- A mecânica de interação é fundamentalmente diferente (minigame, com timing, etc.)
- O tipo de reward é completamente diferente
- A spec exige explicitamente uma nova classe

---

## Interaction trigger (lado do Player)

O sistema de interação do player lê `IInteractable.CanInteract` e chama `Interact()`. Verifique que o player tem:
```csharp
// PlayerInteractionController or equivalent
if (_nearbyInteractable != null && _nearbyInteractable.CanInteract)
{
    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
        _nearbyInteractable.Interact(gameObject);
}
```

Se o player interaction controller ainda não existe → documente:
```
PLAYER_INTERACTION_CONTROLLER_DEBT
```

---

## Debt tags

```
TODO_INTEGRATION_NOT_FINAL
RESPAWN_TIME_BALANCE_PENDING
DEPLETION_SAVE_DEFERRED          — depleted state not persisted across sessions
PLAYER_INTERACTION_CONTROLLER_DEBT
INTERACTABLE_ANIMATION_DEFERRED  — no depletion visual yet
```

---

## Validação

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

O checklist humano de Play Mode deve cobrir:
- Andar até o interactable → o prompt aparece
- Apertar a tecla de interact → item adicionado ao inventory
- Resource deplete após o harvest
- Tentar interagir com resource depletado → nada acontece
- Se respawn configurado → resource reaparece após o delay
- Inventory cheio → resource NÃO deplete

---

## Regressões comuns

- Depletar na chamada do `AddItem`, e não no sucesso do `AddItem` → item perdido se o inventory estiver cheio
- Esquecer o guard de `ClampedAmount` → stack overflow no `AddItem`
- Criar um novo interaction controller em vez de implementar `IInteractable`
- Editar o scene YAML em vez do CreateScene script
- Não publicar o feedback event → o player não tem indicação do que aconteceu

## Quando parar e reportar

- O scene YAML precisa ser editado e não existe CreateScene script → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`
- O novo tipo de interactable exige um novo physics layer ou tilemap → `BLOCKED` (exige ProjectSettings)
- A spec exige save/load do depletion state → use também a skill `save-load-pattern`
