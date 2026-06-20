---
name: ui-modal-stack
description: Implementa e valida UI modal usando o stack do ModalManager, input blocking e comportamento de Esc-close. Use em qualquer tarefa que toque UI modal, inventory screen, shop, equipment, skill tree, pause, death screen ou Anya UI.
---

# Skill: UI Modal Stack

Esta skill cobre a implementação e validação de UI modal sobre o stack do `ModalManager`, garantindo input blocking e fechamento por Esc.

## Quando usar

A tarefa toca:
- `ModalManager` (push/pop/clear)
- Panels de inventory, equipment, shop, skill tree
- Pause screen, death screen
- UI de cutscene/conversa da Anya
- Input blocking enquanto o modal está aberto
- Tecla Esc fechando o modal do topo

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/UI/` — ModalManager e panels relevantes

## Não ler por padrão

```
All scene files
All prefab files
Unrelated UI scripts
```

## Invariantes centrais

### Esc sempre fecha o modal do topo

```csharp
// ModalManager must handle Esc to pop top modal
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && _stack.Count > 0)
        PopModal();
}
```

### Input bloqueado enquanto o modal está aberto

Quando qualquer modal está aberto:
- O movimento do player deve ser desabilitado
- O input de attack deve ser desabilitado
- Use `GameEventBus.Publish(new ModalOpenedEvent())` / `ModalClosedEvent()`
- Os input handlers do player fazem subscribe e se desabilitam

### Comportamento do stack

```csharp
// Open
ModalManager.Instance.Push(panelGameObject);

// Close top
ModalManager.Instance.Pop();

// Close all
ModalManager.Instance.Clear();
```

### Sem modal mismatch

- Nunca `Push` sem um caminho de `Pop` correspondente
- Fechar o game/scene deve chamar `Clear()`
- Cada modal deve ter exatamente um trigger de "close" (Esc, botão X, ou chamada explícita de close)

## Validação (checklist de Play Mode — Phase 3)

Checks manuais:
- [ ] Esc fecha somente o modal mais ao topo
- [ ] O player não consegue se mover enquanto o modal está aberto
- [ ] O player não consegue atacar enquanto o modal está aberto
- [ ] Abrir modals aninhados (ex.: shop a partir do inventory) funciona corretamente
- [ ] Fechar um modal aninhado retorna ao parent, não ao game principal
- [ ] A transição de scene limpa todos os modals

## Regressões comuns

- O modal abre mas nunca dá pop (input block infinito)
- O player ainda consegue se mover com o inventory aberto
- Esc fecha todos os modals em vez de só o do topo
- `GameEventBus.Publish(new ModalClosedEvent())` ausente no pop

## Quando parar e reportar

- O `ModalManager` usa `FindObjectOfType` — viola a regra no-global-search
- O state do modal persiste entre scene loads sem clear explícito
