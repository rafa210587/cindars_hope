# Localization runtime usage

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
