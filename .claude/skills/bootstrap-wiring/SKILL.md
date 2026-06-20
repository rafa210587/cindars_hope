---
name: bootstrap-wiring
description: Faz o wiring de novos managers, databases e services no GameBootstrap com serialized refs e scene repair. Use em qualquer tarefa que adicione um novo manager, database SO ou service que precise de wiring no GameBootstrap.
---

# Skill: Wiring de Bootstrap

## Quando usar

A tarefa toca em:
- `GameBootstrap.cs` (adicionar fields ou properties)
- Instanciação / injeção de manager
- Referências de ScriptableObject database
- Scene creators (`FarmSceneCreator`, `TownSceneCreator`, `CaveSceneCreator`)
- Qualquer novo sistema que outros sistemas acessem via `GameBootstrap.Instance`

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` (estado atual)

## Não ler por padrão

```
All scenes
All prefabs
Full architecture docs
```

## Procedimento

### Adicionando um novo Database ou Manager ao Bootstrap

1. Adicione `[SerializeField] private <TypeSO> _<field>;` depois do field existente relacionado
2. Adicione `public <TypeSO> <Property> => _<field>;` depois da property existente relacionada
3. Commit: o field do Inspector ficará null até o wiring ser feito no Unity Editor
4. Adicione uma nota ao execution report:
   ```
   Inspector wiring required: assign <TypeSO>.asset in GameBootstrap inspector
   ```

### Wiring de Consumer (acesso a partir de um MonoBehaviour)

Padrão preferido:
```csharp
private void Start()
{
    var bootstrap = GameBootstrap.Instance;
    _database = bootstrap?.SpecificDatabase;
    if (_database == null)
        Debug.LogWarning("Missing database wiring in GameBootstrap", this);
}
```

**NUNCA use:**
```csharp
// PROHIBITED
var bootstrap = FindObjectOfType<GameBootstrap>();
var bootstrap = GameObject.Find("Bootstrap").GetComponent<GameBootstrap>();
```

### Atualizações de Scene Creator

Se um scene creator precisar do novo manager:
1. Encontre o scene creator da scene relevante
2. Adicione o lookup depois do resolve do GameBootstrap
3. Use o repair menu `CindarsHope/Repair and Validate Project` para aplicar

### Editor Validators

Se um validator precisar checar o novo wiring:
1. Encontre `CombatDatabaseValidator` ou similar
2. Adicione null check e `report.AddIssue(...)` para o asset ausente
3. Use `ValidationSeverity.Warning` para assets ainda não criados (esperado até o wiring no Inspector)

## Regras

- Sem `GameObject.Find()` ou `FindObjectOfType()` — nunca
- Apenas serialized refs
- Null-check no consumer com LogWarning claro incluindo contexto de scene/component/field
- O wiring no Inspector é responsabilidade humana; documente no execution report

## Validação

Depois do wiring em C#:
```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Esperado: 0 errors. O novo field começa como null — isso é esperado.

Unity validator (Phase 2, se a spec exigir):
- `CindarsHope/Validate/Combat/Validate Combat Databases`
- Vai mostrar warning para assets sem wiring

## Regressões comuns

- Usar `FindObjectOfType<GameBootstrap>()` no consumer
- Adicionar o field mas esquecer de adicionar a property
- Não documentar o requisito de wiring no Inspector
- Adicionar ao scene creator errado

## Quando parar e reportar

- `GameBootstrap.cs` está fora do escopo da spec → pare, reporte
- Scene YAML precisaria de edição manual → use o repair menu no lugar
