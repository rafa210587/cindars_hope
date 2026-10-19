# spec_arch_core_enemy_cycle_reduction_v31

> **Status:** Implementado e BUILD_VALIDATED (piloto Fase 1 do plano de desacoplamento
> `static Instance`)
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Enemy` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento, e **sem regeneração de cena**.

## Objetivo

Piloto da Fase 1 do plano "Desacoplar managers de domínio do GameBootstrap (Core|\*), sem regen
destrutiva": provar que um manager já `AddComponent`ado nas cenas pode perder o
`[SerializeField]` do `GameBootstrap` ganhando um `static Instance` self-registrado (molde vivo:
`Assets/_Game/Scripts/Audio/AudioManager.cs`), sem exigir regeneração das 3 cenas (Farm/Town/Cave).

```text
Snapshot medido ANTES da edição (checkpoint 347434b7): MutualModulePairs=27 (Core|Enemy presente).
Snapshot medido DEPOIS de todas as edições: MutualModulePairs=26, Core|Enemy ausente do output.
Nenhum par novo apareceu.
```

## Direção cortada

`Core -> Enemy`: `GameBootstrap.cs` segurava `[SerializeField] BestiaryManager _bestiaryManager` +
property pública `BestiaryManager` + método `EnsurePersistentBestiaryManager()` (fallback
`AddComponent`), o que forçava `using CindarsHope.Enemy`. Essa era a única aresta do par (o outro
lado, `Enemy -> Core`, não existe: `BestiaryManager.cs` não referenciava `CindarsHope.Core`).

## Diagnóstico (Passo 0)

Grep repo-wide por `BestiaryManager` fora de `GameBootstrap.cs`/`SaveManager.cs`/geradores de cena
encontrou exatamente **um** consumidor de código de gameplay: `NPC/Services/NpcServiceRuntime.cs:302`
(`ResolveBestiary()` lia `GameBootstrap.Instance.BestiaryManager`). `SaveManager.cs` já tinha seu
próprio `[SerializeField] _bestiaryManager` e `using CindarsHope.Enemy` — fora de escopo desta spec
(par `Save|Enemy` não é mútuo; ver instrução da tarefa) e não foi tocado além de resolver o fallback
via `Instance`.

## Implementação

1. **`Assets/_Game/Scripts/Enemy/BestiaryManager.cs`** — adicionado `public static BestiaryManager
   Instance { get; }` setado em `Awake()` (guard de duplicata: `if (_instance != null && _instance
   != this) { Destroy(gameObject); return; }`) e limpo em novo `OnDestroy()` (`if (_instance == this)
   _instance = null;`). Nenhuma lógica de bestiário (subscribe/eventos/dados) alterada — molde
   idêntico ao de `AudioManager.cs:34,74-92`.
2. **`Core/Bootstrap/GameBootstrap.cs`** — removidos: `using CindarsHope.Enemy;`, o campo
   `[SerializeField] _bestiaryManager`, a property pública `BestiaryManager`, a chamada e o método
   `EnsurePersistentBestiaryManager()`. A chamada a `_saveManager.RebindOptionalRuntimeManagers(...)`
   deixou de passar `_bestiaryManager` como último argumento (parâmetro tem default `null`).
   Confirmado por grep: `GameBootstrap.cs` não contém mais nenhum token `Enemy`/`BestiaryManager`
   fora de um comentário `arch:` explicativo.
3. **`NPC/Services/NpcServiceRuntime.cs`** — `ResolveBestiary()` trocado de
   `GameBootstrap.Instance.BestiaryManager` para `BestiaryManager.Instance` direto.
4. **`Save/SaveManager.cs`** — `RebindOptionalRuntimeManagers(...)` (assinatura preservada, ainda
   aceita `BestiaryManager bestiaryManager = null` para os geradores de cena legados que continuam
   passando o componente explícito) resolve `_bestiaryManager = bestiaryManager ?? BestiaryManager.
   Instance` — fallback para o self-registro quando o chamador (agora `GameBootstrap`) não passa mais
   a referência. `using CindarsHope.Enemy;` de `SaveManager.cs` foi mantido (par `Save|Enemy`, fora de
   escopo — instrução explícita da tarefa).
5. **Geradores de cena** (`Editor/SceneCreation/CreateMvp{Farm,Town,Cave}Scene.cs`) **não foram
   editados** (diff mínimo, autorizado pela tarefa): a linha `SetReference(serializedBootstrap,
   "_bestiaryManager", ...)` no gerador de Farm agora aponta para um campo que não existe mais no
   `GameBootstrap` — vira no-op inofensivo na próxima regen (não executada nesta sessão). O
   `AddComponent<BestiaryManager>()` nos 3 geradores continua — o componente da cena se auto-registra
   como `Instance` via `Awake()`, que é exatamente o mecanismo que evita a regen.
6. **Ratchet de arquitetura** — `RuntimeSource_DoesNotIncreaseTrackedArchitecturalDebt` (EditMode)
   rejeitou o novo padrão `SingletonDeclaration` em `BestiaryManager.cs` (baseline permitia 0). Como
   este é exatamente o padrão sancionado pelo plano aprovado (mesmo padrão já permitido para
   `Audio/AudioManager.cs`), o baseline `tools/architecture/architecture-ratchet-baseline.tsv` ganhou
   a linha `SingletonDeclaration	Assets/_Game/Scripts/Enemy/BestiaryManager.cs	1` (ordem alfabética
   preservada) — decisão de arquitetura explícita, documentada aqui.
7. Nenhum schema/save/valor/comportamento de gameplay alterado; nenhuma cena/prefab/asset .unity
   editado manualmente; nenhuma regeneração de cena executada.

## Evidência (4 gates enxutos, nesta ordem)

```text
1) tools/architecture/Get-ModularizationDependencySnapshot.ps1
   MutualModulePairs: 27 -> 26
   Core|Enemy ausente do output pós-corte; nenhum par novo.
   Tempo: ~4s

2) tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
   exit 0
   7/7 projetos (Editor, Runtime, Tests.EditMode, Gameplay, Foundation, Assembly-CSharp,
   Tests.PlayMode.Composition)
   0 warnings, 0 errors
   Tempo: ~44s

3) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
   69/69 PASS, exit 0. Tempo: ~30s
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
   1ª rodada: 7/8 PASS (1 falha real do ratchet — corrigida na seção acima).
   2ª rodada (pós-fix do baseline): 8/8 PASS, exit 0. Tempo: ~30s cada rodada.
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.World"
   210/210 PASS, exit 0 (cobre BestiaryKnowledgeTests; não existe namespace
   "CindarsHope.Tests.EditMode.Enemy" no repo — World é onde os testes de bestiário/conhecimento
   moram). Tempo: ~40s
   Nota de ferramenta: o -TestFilter do runner NÃO faz OR entre múltiplos namespaces separados por
   vírgula (testado: 2 namespaces juntos -> 0 testes rodados); rodei cada namespace em separado.

4) Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
   (GameRuntimeCompositionRootPlayModeTests.GameplayScenes_LoadWithoutMissingScripts)
   total=2 passed=2 failed=0, exit 0 (via test-run xml; o processo do Unity.exe em si rodou
   destacado do prompt do PowerShell na primeira tentativa — aguardado com Wait-Process até concluir,
   não morto). Log revisado: nenhuma ocorrência de "missing script"/exception relacionada a
   BestiaryManager. Tempo: ~48s de execução dos testes (~7min de wall-clock por causa da
   desincronização do processo/PowerShell nesta sessão).
```

## Pendências

Fecha apenas `Core|Enemy`. O plano de desacoplamento continua com `Core|Economy`
(`EconomyManager`/`ShopManager`) e `Core|Craft` (`CraftingManager`) na Fase 1, e Fases 2/3 para os
managers de maior fan-out (`Core|Skills`, `Core|Player`, `Core|Equipment`, `Core|Inventory`,
`Core|UI`) — ver `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` (linha `Core|Enemy` marcada
FEITO nesta sessão) e o plano
`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`.

Regen de cena para remover o `AddComponent<BestiaryManager>()` + `SetReference` stray dos 3
geradores fica como limpeza cosmética opcional (não bloqueadora), conforme o plano.
