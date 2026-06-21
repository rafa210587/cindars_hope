---
name: localization-authoring
description: Todo texto voltado ao player via string key (zero hardcode em UI/diálogo); convenções de LocalizationStringTable e LocalizationService. Usar em specs de diálogo, UI, quest text e em qualquer código que exiba texto visível ao player — especialmente a partir de fase P4 em diante (ADR-0012).
---

# Skill: Autoria de Localização

O projeto adota localização via tabela de strings desde a fase P4 (ADR-0012). A fonte de verdade é `LocalizationStringTable` (dicionário estático lazy-init, PT-BR) e o ponto de acesso é `LocalizationService` (`Get(id)`, `TryGet(id, out value)`, `Has(id)`). Texto hardcoded em UI e diálogo é débito explícito (registrado em ADR-0012 como "Registered Debt" pré-P4); texto **novo** deve sempre usar o sistema.

## Quando usar

- Spec adiciona texto novo visível ao player (toast, label, diálogo, nome de quest, prompt de interação).
- Spec de diálogo (fable_70, F35, F36) que declare nós de texto novos.
- UI screen que renderize string derivada de ID (status effect, classe, slot de skill).
- Qualquer código que hoje ainda usa hardcode (`"Amizade: nivel"`, toast inline) e precise ser migrado.
- Checklist de closeout: verificar se texto novo entrou na tabela.

## Por que existe

Texto hardcoded em código de gameplay impede tradução futura sem mexer em lógica, cria ruído em testes e viola a separação entre dados e comportamento. A regra de `FailureReason` virar key (`LocalizationService.Get(reason)` na view em vez de string bruta) é o padrão canônico do projeto.

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `LocalizationStringTable` (`CindarsHope.Localization`) | Dicionário `string id → string PT-BR`; lazy-init; `TryGetValue(id, out v)`, `Contains(id)`, `OverrideForTests(dict)` |
| `LocalizationService` | Acesso público: `Get(id)`, `TryGet(id, out v)`, `Has(id)` — fallback determinístico: key ausente retorna o próprio id (nunca null, nunca exceção) |
| `LocalizationEntry` | DTO interno: `Id` + `Value`; `IIdentifiedData` |

**Convenção canônica de id** (declarada em `LocalizationStringTable.SeedEntries()`):

```
{domain}.{spec_ou_npc}.{slot}
  quest.first_supplies.title
  quest.first_supplies.desc
  dialogue.pip.greeting
  ui.friendship.level_label
  ui.birthday.gift_toast        ← usa {0} para placeholder
  interact.zrix_board.prompt
  contract.cave.milestone.title
```

- Domínio: `quest.`, `dialogue.`, `ui.`, `interact.`, `contract.`
- Separador: ponto; segmentos em `snake_case`; sem espaços.

## Procedimento

### Registrar texto novo

Em `LocalizationStringTable.SeedEntries()` (ou partial futura):

```csharp
yield return new LocalizationEntry("ui.meu_sistema.label", "Texto em PT-BR");
yield return new LocalizationEntry("quest.minha_quest.title", "Titulo da Quest");
```

Nunca hardcode a string na view ou no ViewModel.

### Resolver na view / ViewModel

```csharp
// Caso simples (key sempre presente):
string label = LocalizationService.Get("ui.meu_sistema.label");

// Caso com fallback explícito:
if (LocalizationService.TryGet("quest.minha_quest.title", out var title))
    _titleText.text = title;
else
    _titleText.text = "quest.minha_quest.title"; // fallback visível

// FailureReason como key (padrão canônico):
string msg = LocalizationService.Get(failureReason); // se não tiver key, retorna o próprio reason
```

**Nunca** retornar string vazia para key ausente — `LocalizationService.Get` já garante fallback = id.

### Placeholder dinâmico (plurais, nomes)

Use `string.Format` ou interpolação APÓS resolver a key:

```csharp
// "Hoje e aniversario de {0}!" → "Hoje e aniversario de Pip!"
string toast = string.Format(LocalizationService.Get("ui.birthday.gift_toast"), npcName);
```

Nunca embutir o valor dinâmico diretamente na key.

### Testar

`LocalizationStringTable.OverrideForTests(dict)` substitui a tabela inteira para o teste (reset com `null`):

```csharp
LocalizationStringTable.OverrideForTests(new Dictionary<string, string>
{
    { "quest.minha_quest.title", "Titulo Teste" }
});
Assert.AreEqual("Titulo Teste", LocalizationService.Get("quest.minha_quest.title"));
LocalizationStringTable.OverrideForTests(null); // cleanup
```

## Regras

- Todo texto visível ao player a partir de P4 usa key → `LocalizationService.Get(key)`.
- **Nunca** `string.IsNullOrEmpty(LocalizationService.Get(id))` para detectar key ausente — o fallback é o próprio id (sempre não-vazio para ids não-vazios). Use `LocalizationService.Has(id)` ou `TryGet`.
- Key ausente é **dado visível ao player** (retorna o id como fallback) — não deve chegar a produção. Registrar como débito se a key ainda não existir na tabela.
- Não criar um segundo sistema de i18n, um `ResourceManager` Unity ou um `Addressables` de texto — ADR-0012 bane localization frameworks em v1.
- Texto pré-P4 (ex.: `DialogueNode.Text` hardcoded em SOs, `TownNpcDialogueLibrary`) é débito registrado — não migrar sem spec explícita.
- `LocalizationStringTable` e `LocalizationService` são classes estáticas puras (sem Unity, sem GameEventBus, sem save).

## Saída esperada (checklist de closeout)

```text
Texto novo voltado ao player: SIM/NÃO
Keys registradas em LocalizationStringTable: SIM/NÃO APLICÁVEL
Convenção de id seguida: SIM/NÃO
LocalizationService.Get usado na view/ViewModel: SIM/NÃO
Hardcode de string removido/ausente: SIM
Teste de round-trip (OverrideForTests): SIM/NÃO APLICÁVEL
```

## Quando NÃO usar

- Texto de **debug/log** interno (`Debug.Log`, `UnityEngine.Debug`) → não localizar; esses nunca chegam ao player.
- **IDs de sistema** (item IDs, npcIds, evento IDs) → são identificadores, não texto de UI; cobertos pela rule `id-stability`.
- Texto hardcoded **pré-P4 existente** → é registered debt (ADR-0012); não refatorar a menos que a spec inclua isso explicitamente no escopo.
- Texto de **editor tools** e validators (só visto por devs) → não precisa de localização.

## Relacionados

- ADR-0012 — decisão canônica de localization
- `(skill: npc-dialogue-authoring)` — diálogo localizado via keys
- `(skill: ui-projection-pattern)` — a view resolve a key no ViewModel
- `(skill: editmode-test-authoring)` — testar tabela via `OverrideForTests`
