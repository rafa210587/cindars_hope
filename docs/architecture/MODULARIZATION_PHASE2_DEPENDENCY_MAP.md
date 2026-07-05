# Mapa de Dependências — Fase 2

Data: 2026-07-05

Branch: `dev`

Status: `VALIDATED_PENDING_PUBLICATION`

## Objetivo

Registrar os acoplamentos que precisam ser resolvidos antes da primeira `.asmdef`. Este documento
não autoriza mover scripts nem alterar namespaces; ele define a fronteira e o gate da Fase 3.

Comando reproduzível:

```powershell
& .\tools\architecture\Get-ModularizationDependencySnapshot.ps1
```

## Resultado do snapshot

| Métrica | Valor |
|---|---:|
| `.asmdef` no projeto | 0 |
| arquivos C# com nome predefinido de assembly hardcoded | 0 |
| tipos `internal` declarados em `Scripts/**` | 18 |
| acessos compilados de testes a tipos runtime `internal` | 2 tipos / 3 arquivos |
| arestas entre módulos de pasta/namespace | 206 |
| pares de dependência mútua | 49 |

## Tipos `internal` que bloqueiam futura separação de testes

| Tipo | Definição | Consumidores |
|---|---|---|
| `CaveEnemyMaterializer` | `Cave/Runtime/CaveEnemyMaterializer.cs` | `CaveRuntimeMaterializerDecompositionTests.cs` |
| `CaveTileMaterializer` | `Cave/Runtime/CaveTileMaterializer.cs` | `CaveRuntimeMaterializerDecompositionTests.cs`, `CaveBiomeArtProfilesTests.cs` |

Outras menções a tipos `internal` em Editor/tests eram comentários, paths ou strings de validação,
não acessos compilados. Antes de criar uma assembly de testes, escolher explicitamente entre:

1. extrair a lógica pura testada para um tipo público pequeno em Foundation/World;
2. manter o tipo interno e adicionar `InternalsVisibleTo` somente para a assembly de testes;
3. testar por uma API pública existente.

Não tornar materializers inteiros públicos apenas para satisfazer testes.

## Pares de dependência mútua atuais

O scanner lexical encontrou estes 49 pares. Eles não significam necessariamente um ciclo de tipos
em todos os arquivos, mas impedem transformar as pastas atuais diretamente em assemblies:

```text
Cave↔Combat, Cave↔Core, Cave↔Enemy, Cave↔Save, Cave↔SceneManagement
Combat↔Core, Combat↔Enemy, Combat↔Player, Combat↔Skills
Core↔Craft, Core↔Economy, Core↔Enemy, Core↔Equipment, Core↔Farm, Core↔Inventory
Core↔Locations, Core↔NPC, Core↔Player, Core↔Save, Core↔Skills, Core↔UI, Core↔World
Craft↔Save, Craft↔UI, Economy↔Save, Equipment↔Inventory, Equipment↔Save
Farm↔Interaction, Farm↔Player, Farm↔Save, Fonte↔MainProgression
Inventory↔Player, Inventory↔Save, Locations↔Player, Narrative↔Quests
NPC↔Quests, NPC↔Save, NPC↔UI, NPC↔World
Player↔Save, Player↔Skills, Player↔UI, Player↔World
Quests↔Save, Quests↔UI, Save↔UI, Save↔World, SceneManagement↔World, UI↔World
```

Conclusão: não criar `.asmdef` por pasta atual. A primeira assembly deve ser uma fatia Foundation
nova/curada, sem tentar converter `Core` inteiro.

## Matriz de referências permitidas

| Assembly alvo | Pode referenciar |
|---|---|
| `CindarsHope.Foundation` | BCL; nenhum Unity; nenhuma camada superior |
| `CindarsHope.Gameplay` | Foundation |
| `CindarsHope.World` | Foundation e contratos puros de Gameplay |
| `CindarsHope.Application` | Foundation, Gameplay e World |
| `CindarsHope.Adapters.Unity` | Application/ports, Foundation e Unity |
| `CindarsHope.Presentation` | Application/projeções, Foundation e Unity UI |
| `CindarsHope.Composition` | todas as assemblies runtime para wiring |
| `CindarsHope.Editor` | runtime público necessário e UnityEditor; nunca o inverso |
| `CindarsHope.Tests.*` | assembly sob teste; acesso interno apenas por decisão registrada |

Regras:

- runtime nunca referencia Editor ou Tests;
- Foundation não contém `MonoBehaviour`, `ScriptableObject`, `GameObject`, `Vector*` ou `Time`;
- save DTOs continuam com IDs/tipos simples;
- eventos de domínio puros podem migrar para Foundation; event bus Unity/global não migra no
  primeiro lote;
- nomes de namespaces permanecem inicialmente para evitar migração serializada desnecessária;
- uma assembly nova deve reduzir ciclos; não é permitido criar referência circular para “fazer
  compilar”.

## Ordem segura para a Fase 3

1. selecionar 5–15 tipos puros sem Unity e sem dependências reversas;
2. registrar GUID/path antes de mover qualquer arquivo;
3. resolver os dois acessos internos de Cave separadamente;
4. criar somente `CindarsHope.Foundation.asmdef`;
5. atualizar descoberta de projetos e executar todos os builds;
6. executar EditMode completo e verificar missing scripts em cenas;
7. só ampliar a assembly depois de um commit verde e reversível.

## Gate da Fase 2

- zero nome predefinido de assembly hardcoded em C#;
- builder e hook descobrem N projetos;
- mapa de internos/ciclos documentado;
- naming e referências permitidas definidos;
- nenhuma `.asmdef` criada;
- builds e EditMode mantêm o baseline.

Resultado do gate em 2026-07-05:

- dependency snapshot: exit 0;
- builder multi-project: 2 projetos, exit 0, 0 warnings, 0 erros;
- architecture ratchet: exit 0;
- EditMode completa: 81 testes, 68 PASS, 13 falhas preexistentes;
- falhas adicionadas/removidas em relação ao baseline: 0/0;
- `.asmdef`: 0.

Evidência: `TestResults/modularization-phase2-full-editmode.xml`.
