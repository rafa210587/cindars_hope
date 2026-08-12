# Execution Report — spec_content_city_schedule_colliders_facades_v1

> Spec: `.specs/a_implementar/spec_content_city_schedule_colliders_facades_v1.md`
> Status: `BUILD_VALIDATED` (código) — `DEFERRED_TO_HUMAN` (regeneração de cena + Validar Projeto)
> Data: 2026-08-12

## Fase 0 — Auditoria (decisiva)

Lidas as duas condições exatas em `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs`:

1. **"Only the 8 internal trees carry trunk colliders"** (linha 129-130):
   ```csharp
   Check("Only the 8 internal trees carry trunk colliders",
       CountTreeColliders(scene) == 8, ref passCount, ref failCount);
   ```
   `CountTreeColliders` (linha 231) conta, na cena inteira, quantos GameObjects cujo nome começa com
   `"TownTree_"` têm um componente `BoxCollider2D` anexado. Predicado exato: **exatamente 8** objetos
   `TownTree_*` com `BoxCollider2D`, em toda a cena — nenhum a mais, nenhum a menos.

2. **"Every v8 building has a replaceable semantic facade placeholder"** (linha 131-133):
   ```csharp
   Check("Every v8 building has a replaceable semantic facade placeholder",
       CountNamedPrefix(scene, "SemanticPlaceholder_") >= TownCityLayout.BaselineHouseCount,
       ref passCount, ref failCount);
   ```
   `CountNamedPrefix` conta GameObjects (qualquer profundidade) cujo nome começa com
   `"SemanticPlaceholder_"`. Predicado exato: contagem **>= `TownCityLayout.BaselineHouseCount`** (24,
   confirmado em `TownCityLayout.cs:165` e batendo com os 24 lotes de `AllBuildings`).

### Cruzamento com `CreateMvpTownScene.cs` (estado antes da correção)

- **Árvores:** `CreateTownTree` (linha ~2778) aplicava `BoxCollider2D` quando `isInternalTree == true`,
  definido como
  `treeIndex >= TownTreePositions.Length - ScatteredTreePositions.Length - InteriorWallTreeCount`.
  Isso cobre **dois** blocos de árvores no fim do array (`ScatteredTreePositions`, 8 entradas
  deliberadamente "internas à cidade" — comentário original as chama assim — **mais**
  `InteriorWallTreeCount`, a fileira interna encostada na muralha, dezenas de árvores). Resultado:
  muito mais que 8 `TownTree_*` recebiam collider → `CountTreeColliders(scene) > 8` → **FAIL**.
- **Facade placeholder:** `CreateSemanticBuildingPlaceholder` (cria `SemanticPlaceholder_<token>`) só
  era chamada de dentro de `CreateBuildingIdentityDetails`, por sua vez só chamada quando
  `!hasModularKit` (linha 1597 original), onde `hasModularKit = modularVariant != '\0'`. Porém
  `GetHouseModularVariant` (linha 1215-1230) **sempre** devolve um char não-nulo de
  `ModularHouseVariants = {'A','B','C'}` para qualquer nome não-vazio — o comentário da linha 1209
  confirma: *"TODOS os lotes usam o kit modular de 3 partes"*. Logo `hasModularKit` é sempre `true`
  para as 24 casas reais, e o placeholder **nunca era instanciado** → `CountNamedPrefix(scene,
  "SemanticPlaceholder_") == 0` → **FAIL** (0 < 24).

### Decisão Fase 0: **Caminho A — conteúdo/gerador divergente do predicado do validador**

Evidência de que a cena diverge do design pretendido (não o validador que está obsoleto):
- O comentário original do próprio gerador já nomeia `ScatteredTreePositions` como "árvores
  espalhadas DENTRO da cidade" (8 entradas) — batendo exatamente com o número "8" do check. O bug é
  que `isInternalTree` também capturava a fileira da muralha, um conjunto MUITO maior e não-intencional
  para collider.
- O marcador `SemanticPlaceholder_` já existe como conceito no código (`CreateSemanticBuildingPlaceholder`,
  com token semântico por arquétipo de prédio — templo, ferraria, mercado etc.) — só estava morto por
  trás de uma condição (`!hasModularKit`) que nunca é verdadeira. Não é um validador obsoleto pedindo
  algo que não faz mais sentido; é um placeholder que o próprio gerador já sabia criar mas ficou
  inalcançável quando o kit modular {A,B,C} virou o padrão universal.

Nenhuma evidência de que a cena já reflete o design correto e o validador é que drifou → **default A
confirmado**, sem baixar nenhuma contagem.

## Fase 1 — Implementação (Caminho A)

Arquivo editado: `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` (único arquivo
tocado; `ValidateFableCitySchedule.cs` e `TownCityLayout.cs` **não foram alterados** — caminho B não se
aplicou).

### 1. Trunk colliders — só as 8 `ScatteredTreePositions`

```csharp
// antes
bool isInternalTree = treeIndex >= TownTreePositions.Length - ScatteredTreePositions.Length - InteriorWallTreeCount;

// depois
int scatteredTreesStart = TownTreePositions.Length - ScatteredTreePositions.Length - InteriorWallTreeCount;
bool isInternalTree = treeIndex >= scatteredTreesStart &&
                       treeIndex < scatteredTreesStart + ScatteredTreePositions.Length;
```

Agora `isInternalTree` é verdadeiro só para o intervalo exato das 8 `ScatteredTreePositions` (o bloco do
meio, entre o border ring e a fileira interna da muralha). A fileira da muralha e a floresta externa
continuam **sem** collider (evita travar pathing do player/NPC — edge case da spec §23).

### 2. Facade placeholder — instanciado sempre, para os 24 prédios

```csharp
bool hasModularKit = modularVariant != '\0';

// NOVO — incondicional, antes do bloco !hasModularKit
CreateSemanticBuildingPlaceholder(house.transform, name, size, doorSide);

if (!hasModularKit)
{
    CreateBuildingIdentityDetails(house.transform, name, size, doorSide, archetype, baseColor);
    ...
}
```

A chamada duplicada que existia dentro de `CreateBuildingIdentityDetails`
(`CreateSemanticBuildingPlaceholder(details.transform, houseName, size, doorSide);`, antiga linha 1924)
foi removida para não gerar dois marcadores por casa nos (hipotéticos) casos sem kit — agora há
exatamente **um** `SemanticPlaceholder_*` por prédio, para os 24 prédios de `TownCityLayout.AllBuildings`.

Nenhuma mudança em `TownCityLayout.cs` foi necessária (o marcador não depende de nenhuma constante
nova lá).

## Fase 2 — Geração (Inicializar Projeto)

**BLOCKED / DEFERRED_TO_HUMAN.** Nenhum processo Unity Editor foi detectado ativo no momento da
verificação (`Get-Process` sem match para `*Unity*`), mas a recriação da `TownScene.unity` é destrutiva
e o comando `CindarsHope/Inicializar Projeto` só roda de forma segura dentro do Editor/batchmode
(rule `unity-assets` — sem batchmode paralelo; rule `editor-generation-orchestration` — geração só pelos
3 comandos canônicos). Esta tarefa é de edição de código; a regeneração da cena fica para o humano.

## Fase 3 — Validação (Validar Projeto)

**BLOCKED / DEFERRED_TO_HUMAN** — mesma razão da Fase 2; `ValidateFableCitySchedule` só roda contra a
`TownScene.unity` já regenerada.

### DEFERRED_TO_HUMAN — passos exatos

1. Fechar/garantir que nenhum outro processo está segurando `Temp/obj` (já não há Unity aberto no
   momento da checagem, mas confirme antes de rodar).
2. Abrir o projeto no Unity Editor 6000.5.7f1.
3. Menu `CindarsHope/Inicializar Projeto` (recria as 3 cenas, incluindo `TownScene.unity` — destrutivo,
   por design).
4. Menu `CindarsHope/Validar Projeto`.
5. Conferir no Console/log que `ValidateFableCitySchedule`:
   - `PASS: Only the 8 internal trees carry trunk colliders`
   - `PASS: Every v8 building has a replaceable semantic facade placeholder`
   - `FAIL: 0` no total (nenhum outro check regrediu — anchors 84, roof reveals >=24, portas >=24,
     floors >=24, House_*/stalls/trees floors, landmarks, spawn IDs, stable buildings).
6. `git diff` deve mostrar só `Assets/_Game/Scenes/TownScene.unity` como alterado pela regeneração
   (rule `unity-assets` — evidência de asset gerado).

## Validação de compilação (rodada nesta sessão)

Não existe `Assembly-CSharp-Editor.csproj` neste checkout (projeto modularizado — ver
`memory/project-codex-modularization-verified.md`); o assembly de editor equivalente é
`CindarsHope.Editor.csproj`, que referencia `Assets/_Game/Scripts/Editor/**` (inclui o arquivo editado).

```text
dotnet restore .\Assembly-CSharp.csproj        → exit 0
dotnet build .\Assembly-CSharp.csproj --no-restore
  1ª tentativa → exit 1 (arquivos Temp/obj travados — erro transiente conhecido,
                 ver memory/reference-unity-open-csproj-transient-build-errors.md)
  2ª tentativa (retry) → exit 0, 0 erros, 6 warnings pré-existentes (SaveManager/DialogueLineCondition,
                 não relacionados a este diff)
dotnet build .\CindarsHope.Editor.csproj --no-restore → exit 0, 0 erros, 0 warnings
```

## Arquivos modificados

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs   (16 inserções, 6 remoções)
docs/validation/spec_content_city_schedule_colliders_facades_v1_execution_report.md  (novo — este arquivo)
```

Nenhum outro arquivo do repo foi tocado. `git status` no início da sessão já mostrava 4 arquivos
modificados e 3 não rastreados de trabalho alheio pré-existente
(`GenerateCanonicalStatusEffects.cs`, `SaveManager.cs`, `FestivalRegistry.cs`,
`FestivalRegistryTests.cs`, 2 docs de outra spec) — nenhum deles foi alterado, revisado ou commitado
por esta tarefa.

## Testing Quality Gate

```text
Changed runtime code:           NO (editor-only script, Assets/_Game/Scripts/Editor/**)
Changed deterministic logic:    YES (contagem/índice de árvores internas; instanciação de marcador)
Changed Unity scene/prefab:     NÃO NESTA SESSÃO (TownScene.unity só muda via Inicializar Projeto,
                                 deferido ao humano)
Automated tests added/updated:  NO
Automated tests command:        NOT RUN — lógica é 100% Editor/SceneCreation (gerador de cena), fora
                                 do escopo de EditMode tests (que cobrem lógica de runtime/domínio);
                                 a evidência de correção É o próprio ValidateFableCitySchedule rodando
                                 contra a cena regenerada (Fase 3, deferida ao humano)
Manual Play Mode scenario:      NOT REQUIRED (mudança é geração/validação de editor, sem gameplay
                                 runtime novo — smoke visual já é DEFERRED_TO_FINAL_VALIDATION pela
                                 própria spec, seção 22)
Justification if no tests:      geração de cena via CreateMvpTownScene é validada pelo validator de
                                 editor dedicado (ValidateFableCitySchedule), não por EditMode tests —
                                 padrão já estabelecido pelas specs fable_11 anteriores
Residual risk:                  o predicado dos 2 checks foi lido e cruzado linha a linha com o código;
                                 risco residual é só de regeneração (Fase 2) e leitura do log de
                                 validação (Fase 3), ambos deferidos ao humano com passos exatos acima
```

## Validation Truth block

```text
Validation method: dotnet build (Assembly-CSharp.csproj + CindarsHope.Editor.csproj)
Exit code: 0 (ambos, após retry no primeiro por lock transiente do Temp/obj)
Assembly-CSharp: PASS
CindarsHope.Editor (equivalente a Assembly-CSharp-Editor neste checkout modularizado): PASS
Docs validation: NOT RUN (fora do escopo desta spec de código — sem mudança em docs canônicos além
                 deste próprio report)
Unity batchmode (Inicializar Projeto / Validar Projeto): NOT RUN — DEFERRED_TO_HUMAN (ver seção acima)
Result artifact: este arquivo
```

## Honest status rationale

Código pronto e compilando (`BUILD_VALIDATED` para o diff de C#). O nível-alvo da spec é
`UNITY_VALIDATED` (validador de editor rodando 0 falhas) — isso exige regenerar a `TownScene.unity`
via `Inicializar Projeto` e depois `Validar Projeto`, ambos batchmode Unity que este agente de edição
de código não deve/pode disparar sozinho (regra `editor-generation-orchestration` + risco de
regeneração destrutiva concorrente com o Editor do humano). Por isso o status fica
`BUILD_VALIDATED` para o código, com `UNITY_VALIDATED` explicitamente `DEFERRED_TO_HUMAN` — não
reivindicado como PASS sem evidência (`validation-truth`).

## O que NÃO foi feito

- `CindarsHope/Inicializar Projeto` — não rodado (batchmode/Editor, deferido).
- `CindarsHope/Validar Projeto` — não rodado (batchmode/Editor, deferido); portanto os 2 checks-alvo
  ainda não têm evidência de PASS pós-fix, só a análise estática do predicado vs. código corrigido.
- `docs/validation/**` — nenhum outro doc tocado além deste report.
- Nenhum commit/push foi feito (fora de escopo desta tarefa).
- Nenhum sub-agente foi disparado.
