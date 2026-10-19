# Execution Report — spec_cleanup_uac_serialization_6000_5_v1

> **Status:** `BUILD_VALIDATED_WITH_WARNINGS` — Casos 2 e 3 resolvidos pela causa raiz; Caso 1 (DialogueLineCondition, UAC1001 x5) **NAO resolvido** — Fase 0 confirmou que exige o caminho de maior risco (par bool+valor) fora do escopo de arquivos permitidos, conforme instrução explícita da tarefa: "se for esse o caminho e sair do escopo permitido, PARE e reporte em vez de expandir".

## Fase 0 — Auditoria (os 2 greps decisivos)

### (a) DialogueLineCondition/DialogueLineSelector: Unity-serializada ou só C#?

**Resultado: Unity-serializada de fato (SO real em disco).**

- `Assets/_Game/Scripts/NPC/DialogueTreeSO.cs:12` — `public List<DialogueNode> Nodes` num `ScriptableObject` real (`[CreateAssetMenu]`), gravado em `.asset` pelo menu editor "CindarsHope/NPCs/Rebuild Town NPC Dialogues".
- `DialogueNode` (`[Serializable]`) carrega `List<ConditionalDialogueLine> ConditionalLines` → `ConditionalDialogueLine.Condition` é `DialogueLineCondition`.
- Os campos nullable são efetivamente POPULADOS por geradores hoje: `TownNpcDialogueLibrary.cs:1304-1318` (`Season =`, `Weather =`, `MinFriendship =`) e `Assets/_Game/Scripts/NPC/Dialogue/RomanceDialoguePool.cs` (também atribui os mesmos eixos).
- Conclusão: caso 1 cai no ramo de **maior risco** do §15 — para eliminar UAC1001 de verdade seria preciso converter cada nullable em par `bool HasX` + valor, reescrever `IsMet`/`Specificity`, e atualizar **todos os pontos que constroem `DialogueLineCondition`** (pelo menos `TownNpcDialogueLibrary.cs` e `RomanceDialoguePool.cs`, fora da lista de "Arquivos permitidos" da spec).

### (b) SaveManager._corpseRecoveryManager: é LIDO em algum método?

**Resultado: SIM, é lido (mas nunca é atribuído em lugar nenhum).**

```
Assets/_Game/Scripts/Save/SaveManager.cs:84   [SerializeField] private CorpseRecoveryManager _corpseRecoveryManager;
Assets/_Game/Scripts/Save/SaveManager.cs:267  _deathProvider = new DeathSectionProvider(_corpseRecoveryManager);
```

Nenhuma outra ocorrência de `_corpseRecoveryManager =` no arquivo. `CorpseRecoveryManager` (`Assets/_Game/Scripts/Player/Death/CorpseRecoveryManager.cs`) é classe C# pura, sem `[Serializable]`, sem construtor sem parâmetros (exige `PlayerManager`, `IInventoryRuntime`, `IEquipmentRuntime`) — o Unity **nunca conseguiria** popular esse `[SerializeField]` via Inspector/cena/prefab. `GameBootstrap` mantém sua própria instância (`_corpseRecoveryManager` privado, exposto como `object CorpseRecoveryManager`, criada via `IPlayerRuntime.CreateCorpseRecoveryManager`) e `DeathSystemBootstrap` lê `bootstrap.CorpseRecoveryManager` diretamente — nenhum caminho rebinda o campo do `SaveManager`. Ou seja: o campo é lido, mas **sempre resolve para `null`** hoje (comportamento pré-existente, fora de escopo consertar).

## Decisão por caso

| Caso | Decisão | Racional |
|---|---|---|
| 2 — `FestivalRegistry.Festival` | Adicionar `[System.Serializable]` à struct aninhada | Fix direto pela causa raiz; `InitializeDefaultFestivals()` só popula quando `Count==0`, então defaults inalterados |
| 3 — `SaveManager._corpseRecoveryManager` | Remover `[SerializeField]`, manter campo privado comum | Lido (não pode ser deletado sem quebrar compile), mas nunca atribuível pelo Unity de qualquer forma — o `[SerializeField]` era morto por construção. "Resolver via runtime service" mudaria comportamento observável (out of scope: "mudar comportamento de save") |
| 1 — `DialogueLineCondition` nullable fields | **NÃO ALTERADO — reportado como deferred** | Fase 0 confirmou Unity-serialização real + geradores fora do escopo permitido; caminho de correção correta (par bool+valor) exige tocar `TownNpcDialogueLibrary.cs` e `RomanceDialoguePool.cs`, que não estão em "Arquivos permitidos". Supressão via pragma foi explicitamente proibida pela spec (§23) — portanto nenhuma ação foi tomada, e não 5 dos 7 warnings permanecem |

## Arquivos modificados

- `Assets/_Game/Scripts/Save/SaveManager.cs` — removido `[SerializeField]` de `_corpseRecoveryManager` (linha ~84), com comentário explicando a causa raiz e por que o campo continua `null` (comportamento preservado).
- `Assets/_Game/Scripts/World/Calendar/FestivalRegistry.cs` — adicionado `[System.Serializable]` à struct aninhada `Festival`.

## Arquivos criados

- `Assets/_Game/Tests/EditMode/World/Calendar/FestivalRegistryTests.cs` — caracterização do Caso 2: os 3 festivais default (Planting/Harvest/Market, dias 14/56/98) e o caso "sem festival" permanecem idênticos após `[Serializable]` na struct.
- `docs/validation/spec_cleanup_uac_serialization_6000_5_v1_execution_report.md` (este arquivo).

Nota: `DialogueLineCondition.IsMet`/`Specificity` já têm caracterização extensa e pré-existente em `Assets/_Game/Tests/EditMode/City/DialogueConditionsPoolsTests.cs` (não modificado, pois o Caso 1 não foi alterado — nenhum novo teste necessário ali).

## Arquivos NÃO modificados (explicitamente fora do escopo tomado)

- `Assets/_Game/Scripts/NPC/DialogueLineCondition.cs` — nenhuma mudança (ver decisão do Caso 1).
- `Assets/_Game/Scripts/NPC/DialogueLineSelector.cs` — nenhuma mudança (só seria tocado se o Caso 1 exigisse; não exigiu ação).
- `Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs`, `Assets/_Game/Scripts/NPC/Dialogue/RomanceDialoguePool.cs` — geradores; tocá-los para o Caso 1 sairia do "Arquivos permitidos" da spec — não tocados.

## Validação

### Compile (dotnet build, csproj na raiz — Unity ABERTO)

```
dotnet build .\Assembly-CSharp.csproj --no-restore          → primeira tentativa: exit 1 (CS2012, DLL locked pelo Unity aberto — falha ambiental transiente, ver rule reference-unity-open-csproj-transient-build-errors)
dotnet build .\Assembly-CSharp.csproj --no-restore /t:Rebuild → exit 0 (retry, forçando rebuild completo)
dotnet build .\CindarsHope.Tests.EditMode.csproj              → exit 0 (precisou de restore — sem --no-restore; sem erros de pacote depois)
```

Comando final (rebuild completo, honesto):
```
PS> dotnet build .\Assembly-CSharp.csproj --no-restore /t:Rebuild
...
6 Aviso(s)
0 Erro(s)
EXIT_Assembly-CSharp_Rebuild=0
```

**Warnings remanescentes (6, todos esperados/documentados):**
1-5. `UAC1001` em `DialogueLineCondition.cs` (Season, Weather, MinFriendship, TimeBand, MinRomanceStage) — Caso 1, deferred conforme decisão acima.
6. `CS0649` em `SaveManager.cs:90` ("_corpseRecoveryManager nunca é atribuído") — consequência honesta e esperada de remover `[SerializeField]` de um campo que, na prática, já era sempre `null` (o compilador C# agora enxerga o que o Unity mascarava). Não é um warning UAC; não muda comportamento.

**UAC1010 (os 2 do Caso 2 e Caso 3): ZERO — confirmado pelo rebuild acima.**
**UAC1001 (os 5 do Caso 1): ainda 5 — não tratados, decisão documentada.**

```
Assembly-CSharp: PASS (exit 0, rebuild completo)
CindarsHope.Tests.EditMode: PASS (exit 0, inclui o novo FestivalRegistryTests.cs compilando)
```

Não existe `Assembly-CSharp-Editor.csproj` na raiz (arquitetura modular atual usa `CindarsHope.Editor.csproj`, incluído automaticamente como dependência do build acima — compilou como parte do grafo, 0 erros).

### EditMode (execução real dos testes)

`Assets/_Game/Tests/EditMode/World/Calendar/FestivalRegistryTests.cs` compila limpo (verificado acima) mas a **execução** via Unity Test Runner em batchmode está **BLOQUEADA** porque o Editor está aberto (regra `unity-assets.md` §3: sem batchmode paralelo).

```
DEFERRED_TO_HUMAN: rodar Window > General > Test Runner > EditMode no Editor 6000.5 e confirmar
FestivalRegistryTests.InitializeDefaultFestivals_ProducesThe3CanonicalFestivals_OnDefaultDays verde.
```

### "0 warnings UAC" no Console do Editor

```
DEFERRED_TO_HUMAN: recompilar no Editor Unity 6000.5.7f1 e confirmar no Console que:
  - UAC1010 (Festival / _corpseRecoveryManager): 0 (antes: 2)
  - UAC1001 (DialogueLineCondition): ainda 5 (Caso 1 não tratado — esperado)
  Total esperado no Console: 5 (não 0 — ver Honest status rationale).
```

## Testing Quality Gate

```text
Changed runtime code:           YES (SaveManager.cs, FestivalRegistry.cs)
Changed deterministic logic:    NO (nenhuma lógica mudou; apenas atributos de serialização)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES (FestivalRegistryTests.cs — novo)
Automated tests command:        dotnet build .\CindarsHope.Tests.EditMode.csproj (compila; execução real DEFERRED_TO_HUMAN)
Manual Play Mode scenario:      NOT REQUIRED (nenhum comportamento de gameplay muda)
Justification if no tests:      N/A para Casos 2/3 (cobertos). Caso 1 não foi alterado — teste pré-existente (DialogueConditionsPoolsTests.cs) já cobre IsMet/Specificity e continua válido sem mudança.
Residual risk:                  SaveManager._corpseRecoveryManager segue sempre null em runtime (bug pré-existente, não introduzido nem corrigido aqui — DeathSectionProvider(null) já era o comportamento antes desta spec). Caso 1 (5 warnings UAC1001) permanece no Console até uma spec futura autorizada a tocar os geradores de diálogo.
```

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: os 2 dos 3 casos endereçáveis dentro do escopo de arquivos permitidos foram corrigidos pela causa raiz (não supressão), com compile 0 erros e teste de caracterização novo para o caso com comportamento observável (Festival defaults). O Caso 1 foi investigado a fundo na Fase 0, confirmado como exigindo o caminho de maior risco (mudança de forma pública + atualização de geradores fora do escopo permitido), e — seguindo a instrução explícita da tarefa — **não foi alterado**; a decisão e o racional estão documentados aqui em vez de expandir escopo ou suprimir o warning via pragma (proibido pela spec §23).

## Remaining work (para uma spec futura, se aprovada)

Para zerar os 5 UAC1001 restantes seria necessário (spec separada, escopo maior):
1. Converter cada `Season?`/`WeatherType?`/`int? MinFriendship`/`DialogueTimeBand?`/`int? MinRomanceStage` em par `bool HasX` + valor não-nullable em `DialogueLineCondition.cs`.
2. Reescrever `IsMet`/`Specificity` para usar os novos pares.
3. Atualizar todos os inicializadores de objeto em `TownNpcDialogueLibrary.cs` (linhas 1304-1318 e possivelmente outras) e `RomanceDialoguePool.cs` (e qualquer outro gerador que crie `DialogueLineCondition` — grep completo necessário nessa spec futura).
4. Adicionar smoke de diálogo ao checklist de Play Mode (per §22 da spec atual).
