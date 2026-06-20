---
name: runtime-bootstrap-pattern
description: Padroniza como runtime systems se auto-conectam às scenes (o idiom *RuntimeBootstrap) sem global searches proibidos. Use em specs de wiring de orphan-system (fable_15, fable_16) ou em qualquer novo scene-runtime binding.
---

# Skill: Runtime Bootstrap Pattern

## Estado atual (honesto)

O projeto desenvolveu um idiom de fato: classes `*RuntimeBootstrap` (Quest, CraftingStation, NpcSchedule, FarmDailyGoal, PlayerMovementAction) que localizam suas dependências com `FindObjectOfType` — o que viola a rule `unity-architecture` §1. ~14 runtime files carregam esse débito. **Uma decisão humana está pendente: abençoar uma versão restringida do idiom ou agendar a sua remoção.** Até lá:

## Regras para QUALQUER novo scene-runtime binding

1. **Não copie o pattern `FindObjectOfType`.** O hook `runtime-code-guard` o sinaliza em código novo.
2. Ordem de resolution para dependências:
   1. Serialized reference atribuída pelo scene creator (`Editor/SceneCreation/CreateMvp*Scene.cs` faz o wiring) — preferido;
   2. GameBootstrap injection (`Core/Bootstrap/GameBootstrap.cs` guarda a ref e injeta via método explícito) — para managers cross-scene;
   3. registro via `[RuntimeInitializeOnLoadMethod]` onde o sistema registra A SI MESMO num static service point no load (precedente WAVE07: InventoryPanelController/CharacterEquipmentPanelController) — para controllers que precisam existir antes do scene wiring.
3. **Dependência ausente = wiring error barulhento, nunca fallback search silencioso:**

```csharp
Debug.LogError($"WiringError: scene={gameObject.scene.name} object={name} component={GetType().Name} missingField=_questService affectedId={questId}");
```

4. Os scene creators são a fonte de verdade do wiring: ao adicionar um serialized field, atualize o gerador `CreateMvp*Scene` correspondente na mesma mudança (skill: scene-interactable-wiring) e documente qualquer Inspector wiring necessário do humano.
5. Conecte-se ao gameplay flow via eventos do GameEventBus, não fazendo polling do state de outros sistemas no `Update()`.

## Ao tocar num *RuntimeBootstrap EXISTENTE

- Spec scope o inclui → migre seus lookups para a ordem de resolution acima e remova as chamadas `FindObjectOfType`.
- Spec scope NÃO o inclui → deixe-o, mas liste-o no execution report sob known debt.

## Checklist para specs de wiring de orphan-system (estilo fable_15/16)

- [ ] Auditar qual manager/service está órfão e quem deveria ser dono do seu lifecycle (GameBootstrap vs. scene creator).
- [ ] Fazer o wiring pela ordem de resolution acima; sem novos global searches.
- [ ] `*WiringTests` EditMode test que assegura completude do registro (precedente: `OrphanSystemsWiringTests`).
- [ ] Human Play Mode scenario para o comportamento conectado (skill: gameplay-test-scenario).
