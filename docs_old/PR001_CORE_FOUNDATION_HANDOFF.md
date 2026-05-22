# Cindar's Hope — PR-001 Core Foundation

## Objetivo

Criar a fundação mínima de eventos para o MVP Fazenda.

## Arquivos criados

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs
```

## Namespace

```csharp
CindarsHope.Core
CindarsHope.Core.Events
```

## Decisões técnicas

- `GameEventBus` é estático.
- Eventos são `readonly struct`.
- Eventos carregam apenas valores simples e IDs persistíveis.
- Eventos não carregam `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`.
- `Subscribe<T>()` retorna `IDisposable`, mas o padrão Unity continua sendo `Subscribe` em `OnEnable` e `Unsubscribe` em `OnDisable`.
- `Publish<T>()` usa snapshot para permitir unsubscribe durante dispatch.
- Exceções de listeners são logadas com `Debug.LogException` sem impedir os demais listeners.
- `Clear<T>()` e `ClearAll()` existem para testes/debug, não para gameplay normal.

## Teste manual obrigatório

1. Copiar a pasta `Assets/` deste pacote para a raiz do projeto Unity.
2. Abrir o Unity.
3. Esperar recompilar scripts.
4. Confirmar Console sem erro.
5. Confirmar que os arquivos aparecem em `Assets/_Game/Scripts/Core`.

## Exemplo de uso

```csharp
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

public class ExampleDayListener : MonoBehaviour
{
    private void OnEnable()
    {
        GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
    }

    private void OnDayStarted(DayStartedEvent evt)
    {
        Debug.Log($"Novo dia: {evt.DayNumber}");
    }
}
```

## Próximo PR sugerido

`PR-002 — Data contracts e registries`.
