---
name: ui-projection-pattern
description: Constrói UI usando a convenção de projection/ViewModel do projeto — projections de estado em C# puro com enums + computed properties, views MonoBehaviour finas, EditMode tests. Use para qualquer nova tela, elemento de HUD ou panel (fable_14 UI canvas integration, fable_20 calendar HUD, fable_38 minimap).
---

# Skill: Padrão de Projection de UI

A UI do projeto segue MVVM-lite ("projections", WAVE 04/11): 20+ ViewModels com ~96 EditMode tests. Siga esta convenção para toda nova tela.

## O padrão (de CraftingRecipeViewModel e similares)

```csharp
namespace CindarsHope.UI.<Area>
{
    /// <summary>Recipe state projection for UI display.</summary>
    public class <Thing>ViewModel        // pure C# — NO UnityEngine.UI, no MonoBehaviour
    {
        public enum <Thing>State          // explicit state enum, not bool soup
        {
            KnownCraftable, KnownMissingMaterials, KnownLockedBySkill, UnknownHidden, ...
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public <Thing>State State { get; set; } = <Thing>State.UnknownHidden;

        // computed properties derive presentation answers from State
        public bool CanCraft => State == <Thing>State.KnownCraftable;
        public bool IsLocked => State == ...;
        public string LockReason { get; set; }   // human-readable reason, never silent
    }
}
```

Camadas:

| Camada | Tipo | Vive em | Testado por |
|---|---|---|---|
| ViewModel / Projection / Model | pure C# class | `Scripts/UI/<Area>/` | EditMode (`Tests/EditMode/UI/<Area>/`) |
| Builder/Service que preenche o VM a partir do game state | pure C# | mesma área | EditMode |
| View (panel controller, binding de Text/Image) | MonoBehaviour | mesma área | cenário humano de Play Mode |

## Regras

1. **ViewModel = zero dependências de Unity UI.** Apenas usings de domínio (ex.: `CindarsHope.Craft.Data`). Sprites/cores resolvem na View por ID/state.
2. **State enum em vez de booleans.** Um enum captura estados mutuamente exclusivos; computed bools derivam dele. Previne combinações impossíveis (locked AND craftable).
3. **Todo estado disabled/locked/error carrega uma reason string** que a view pode mostrar (convenção existente: `StateDescription`, `LockReason`). Sem botões silenciosamente desabilitados.
4. **Views fazem rebuild a partir de events, nunca poll.** Faça subscribe aos events do GameEventBus (skill: event-bus-pattern), refaça o rebuild do VM, refaça o rebind. Faça unsubscribe no disable.
5. **Telas modais** passam pelo stack do ModalManager + roteamento de input focus (skill: ui-modal-stack; `InputFocusModalRoutingModel` é ele próprio uma projection testada).
6. **Estados empty/error/confirmation** fazem parte do design do VM desde o dia um (precedente existente: padrões empty/error/confirmation da SPEC 04; testes como `MenuProjectionTests`).

## Testes obrigatórios (skill: editmode-test-authoring)

Para cada VM/projection: derivação de state por input de game-state (um teste por state), consistência de computed property, reason preenchida para todo state não-acionável, projection de coleção vazia. Precedentes: `CraftingRecipeViewModelTests`, `MenuProjectionTests`, `InventoryEquipmentTooltipTests`, `HudNotificationDebugProjectionTests`.

A View (MonoBehaviour) ganha um cenário humano cobrindo open/close, Esc, focus, input blocking (rule: testing-quality-gate seção UI).
