# spec_arch_core_locations_cycle_reduction_v27

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Locations` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 2 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=31
- Par presente: Core|Locations

Depois:
- MutualModulePairs=30
- Par removido: Core|Locations
```

## Objetivo de direção

`Locations -> Core` (consumo do composition root, de contratos e de tipos utilitários de Core) é
layering correto e foi **preservado**. Só a direção `Core -> Locations` (o composition root
importando um tipo do domínio Locations) foi cortada, movendo o tipo trivial para dentro de Core.

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `using CindarsHope.Locations` e `CindarsHope.Locations.` em toda a pasta
`Assets/_Game/Scripts/Core/` confirmou exatamente **um** arquivo: `Core/Bootstrap/GameBootstrap.cs`
(`using CindarsHope.Locations;` + `[SerializeField] private AnyaFountain _anyaFountain;` +
`public AnyaFountain AnyaFountain => _anyaFountain;`).

`AnyaFountain.cs` foi lido por inteiro: MonoBehaviour trivial, guarda `FountainId` (string) e
expõe `RespawnPoint` (o próprio `Transform`), implementando `IAnyaFountainRespawnPoint` — interface
que **já vive em** `Assets/_Game/Scripts/Core/Respawn/IAnyaFountainRespawnPoint.cs` (herança do
corte anterior `Locations|Player` → `spec_arch_locations_player_cycle_reduction_v7`). O arquivo só
referenciava `CindarsHope.Core.Respawn` e `UnityEngine` — nenhum tipo de `CindarsHope.Locations` —
confirmando que mover a classe para Core não reintroduz o ciclo.

Grep repo-wide da classe `AnyaFountain` mapeou os consumidores fora do próprio arquivo:
`GameBootstrap.cs` (`Core`), `AnyaFountainInteractable.cs` (permanece em `Locations`, mesmo
namespace de origem — por isso não precisava de `using` antes do corte),
`Editor/SceneCreation/CreateMvpFarmScene.cs` (referência fully-qualified
`CindarsHope.Locations.AnyaFountain`), e a cena `FarmScene.unity` (referência serializada por GUID).
`AnyaFountainMenu.cs`, `AnyaFountainRespawnFlow.cs` e `CaveDeathEventHandler.cs` casavam com o grep
textual só por conterem a substring `"AnyaFountain"` em nomes/comentários — não referenciam a
classe `AnyaFountain` propriamente e não precisaram de `using` novo.

## Implementação

1. `AnyaFountain.cs` (+`.meta`) movido via `git mv` de `Assets/_Game/Scripts/Locations/` para
   `Assets/_Game/Scripts/Core/Respawn/` — GUID preservado, referência serializada da cena sobrevive
   sem edição de YAML. Namespace trocado de `CindarsHope.Locations` para `CindarsHope.Core.Respawn`
   (mesmo namespace da interface que já implementa). O `using CindarsHope.Core.Respawn;` de topo do
   próprio arquivo foi removido por ficar redundante/auto-referente após a mudança de namespace.
2. `GameBootstrap.cs`: `using CindarsHope.Locations;` removido; `using CindarsHope.Core.Respawn;`
   adicionado — Core→Core interno, sem novo edge cruzado no scanner de dependências.
3. `AnyaFountainInteractable.cs` (fica em `Locations`): ganhou `using CindarsHope.Core.Respawn;`
   para resolver `AnyaFountain`, já que deixou de estar no mesmo namespace — Locations→Core,
   direção já existente e correta (unchanged pelo scanner, pois já contava como edge Locations→Core
   via `IAnyaFountainRespawnPoint`).
4. `Editor/SceneCreation/CreateMvpFarmScene.cs`: referência fully-qualified
   `CindarsHope.Locations.AnyaFountain` atualizada para `CindarsHope.Core.Respawn.AnyaFountain`.
5. `CindarsHope.Runtime.csproj`: `<Compile Include>` do arquivo movido atualizado manualmente
   (path `Locations\AnyaFountain.cs` → `Core\Respawn\AnyaFountain.cs`; Unity não estava aberto para
   regenerar o csproj neste ambiente; arquivo é gitignored, não commitado).
6. Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset editado
   manualmente; nenhum comportamento de gameplay alterado.

## Achado fora de escopo (não corrigido)

`Assets/_Game/Scripts/Cave/Death/DeathSystemBootstrap.cs` tem `using CindarsHope.Locations;` morto
(não referencia nenhum tipo de `CindarsHope.Locations` no corpo do arquivo). Isso não contribui ao
par `Core|Locations` (é `Cave`, não `Core`), e `Cave/**` está explicitamente fora do escopo e das
proibições desta execução — não tocado.

## Nota de risco residual (não bloqueante)

`Assets/_Game/Scenes/FarmScene.unity:14589` ainda tem `m_EditorClassIdentifier:
Assembly-CSharp::CindarsHope.Locations.AnyaFountain` — campo cosmético do editor Unity usado para
desambiguar múltiplas classes por arquivo; a resolução real do componente é pela linha
`m_Script: {fileID: 11500000, guid: 30374dbf686cfec41a14bbdf3ada1eac, type: 3}`, cujo GUID foi
preservado pelo `git mv` do `.meta`. Não foi editado manualmente (regra `unity-assets` — sem YAML
edit sem autorização/necessidade comprovada); Unity deve regravar esse campo automaticamente na
próxima vez que a cena for salva pelo editor. O build 7/7 e os 2747/2747 EditMode PASS confirmam que
a referência resolve corretamente sem essa edição.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 31 -> 30
  Core|Locations removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-core-locations-editmode.xml -LogFile Logs\cut-core-locations.log
  exit 0
  2747/2747 PASS (0 failed)
```

## Pendências

Esta spec-filha fecha apenas `Core|Locations`. A modularização ampla não está concluída: ainda
restam 30 pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).
