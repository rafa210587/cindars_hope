# SPEC 14A Fix - Enemy Wiring Validation - 2026-05-29

## Resumo

Fix de wiring pós-SPEC 14A/14B para o erro:

```text
CaveRuntimeMaterializer: Enemy spawn profiles not assigned. Run SPEC 13G asset generation and wire CaveRuntimeMaterializer.
```

Também tratado o caminho normal para evitar:

```text
GameBootstrap created missing BestiaryManager on '_Bootstrap'. Scene should serialize this reference on next scene generation.
```

## Causa raiz confirmada

Validação estática local confirmou:

- `CaveRuntimeMaterializer.MaterializeEnemies` existe e é chamado.
- `Assets/_Game/Scenes/CaveScene.unity` possui `CaveRuntimeMaterializer._enemyDatabase` atribuído.
- `Assets/_Game/Scenes/CaveScene.unity` possui `_enemySpawnProfiles: []`, `_enemySpawnPacks: []` e `_enemyFactionLocks: []`.
- `Assets/_Game/Scenes/CaveScene.unity` possui `GameBootstrap._bestiaryManager: {fileID: 0}`.
- `Assets/_Game/Data/EnemySpawn/` não existia no workspace no momento da validação.
- `Assets/_Game/Data/Bestiary/` não existia no workspace no momento da validação.
- `Assets/_Game/Data/Enemies/Roster/` não existia no workspace no momento da validação.

Conclusão: o bug vem de assets SPEC 13G não gerados/commitados e CaveScene não rewireada após a SPEC 14A/14B. Não é ausência de `MaterializeEnemies`.

## Arquivos alterados

- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs`
- `Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateAndWireSpec13GAssets.cs`
- `Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateAndWireSpec13GAssets.cs.meta`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AEnemySpawnMaterialization.cs`
- `Assembly-CSharp-Editor.csproj`
- `docs/validation/SPEC14A_FIX_ENEMY_WIRING_VALIDATION_20260529.md`

## Assets gerados/atualizados

Assets gerados nesta execução: 0.

Tentativa executada:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -batchmode -quit -nographics -projectPath . -executeMethod CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.GenerateAndWire -logFile .\Logs\spec14a-fix-generate-wire.log
```

Resultado:

```text
Aborting batchmode due to fatal error:
It looks like another Unity instance is running with this project open.
Multiple Unity instances cannot open the same project.
```

Pendência objetiva: fechar a instância aberta do Unity e executar o menu/método:

```text
CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets
```

Esse método executa:

- `Create Default Enemy Profiles`
- `Create Roster 40 Enemy Data`
- `Create Enemy Actions and Sets`
- `Create Bestiary Entries 40`
- `Create Spawn Resolver Ecology Data`
- atualização de `Assets/_Game/Data/Combat/EnemyDatabase.asset`
- wiring de `CaveRuntimeMaterializer` na `CaveScene`
- wiring de `GameBootstrap._bestiaryManager` na `CaveScene`

## Como o CaveRuntimeMaterializer recebe EnemySpawnProfiles

O gerador de cena `CreateMvpCaveScene` já carrega:

```text
Assets/_Game/Data/EnemySpawn/Profiles
Assets/_Game/Data/EnemySpawn/Packs
Assets/_Game/Data/EnemySpawn/FactionLocks
```

Agora ele também loga contagens quando os assets SPEC 13G estiverem ausentes.

Foi criado o editor closeout `GenerateAndWireSpec13GAssets`, que:

- gera/reutiliza os assets SPEC 13G;
- abre `Assets/_Game/Scenes/CaveScene.unity`;
- atribui `_enemySpawnProfiles`;
- atribui `_enemySpawnPacks`;
- atribui `_enemyFactionLocks`;
- salva a cena.

## Como o EnemyDatabaseSO é conectado

`GenerateAndWireSpec13GAssets` garante `Assets/_Game/Data/Combat/EnemyDatabase.asset` e reescreve seu array `_items` com os `EnemyDataSO` encontrados em:

```text
Assets/_Game/Data/Enemies
Assets/_Game/Data/Combat
```

Depois atribui esse asset em `CaveRuntimeMaterializer._enemyDatabase`.

## Como o BestiaryManager foi corrigido no Bootstrap

`CreateMvpCaveScene` agora adiciona `BestiaryManager` no `_Bootstrap` e serializa `GameBootstrap._bestiaryManager`.

`GenerateAndWireSpec13GAssets` também repara a `CaveScene` existente: se o `_Bootstrap` não tiver `BestiaryManager`, adiciona o componente e atribui a referência no `GameBootstrap`.

`GameBootstrap.EnsurePersistentBestiaryManager()` permanece como fallback, mas o caminho normal deve ser a referência serializada na cena.

## Validações executadas

- Leitura estática de `CaveRuntimeMaterializer`, `GameBootstrap`, `CreateMvpCaveScene` e `CaveScene.unity`.
- Contagem de assets:
  - `Assets/_Game/Data/EnemySpawn`: ausente.
  - `Assets/_Game/Data/Bestiary`: ausente.
  - `Assets/_Game/Data/Enemies/Roster`: ausente.
  - `Assets/_Game/Data/Combat/EnemyDatabase.asset`: presente.
- Tentativa de Unity batchmode com `GenerateAndWireSpec13GAssets.GenerateAndWire`: bloqueada por instância Unity aberta.
- `tools/docs/validate_docs.ps1`: PASSED.
- `git diff --check`: PASSED.
- Busca de uso proibido em runtime/editor tocado:
  - Novos usos de `FindObjectsByType` existem apenas em arquivos `Assets/_Game/Scripts/Editor/**`, permitidos para editor tooling.
  - Uso runtime existente encontrado em `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` (`FindObjectsByType<ResourceNode>()`), não introduzido por este fix.

## Validações não executadas

- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`: a primeira tentativa revelou erro de namespace em `ValidateSpec14AEnemySpawnMaterialization.cs`, corrigido para `CindarsHope.Combat.EnemyDatabaseSO`. Não foi possível rerodar após o ajuste final porque a execução escalada foi bloqueada pelo limite da sessão.
- `dotnet build .\Assembly-CSharp.csproj --no-restore`: não executado nesta etapa final pelo mesmo motivo.
- `tools/unity/RunUnityCompileValidation.ps1`: pendente; Unity aberto bloqueia batchmode.
- `tools/unity/ScanUnityLogs.ps1`: pendente.
- Play Mode: pendente.

## Riscos residuais

- A cena atual ainda não está rewireada enquanto o método `GenerateAndWire` não rodar dentro do Unity.
- Os assets `.asset` de roster, bestiary e spawn ecology ainda precisam ser materializados pelo Unity Editor.
- O bug de Play Mode pode continuar aparecendo até a cena ser salva após o wiring.

## Próximo passo obrigatório

Fechar a instância aberta do Unity e executar:

```text
CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets
CindarsHope/Validation/Validate SPEC 14A - Enemy Spawn Materialization
```

Depois validar Play Mode:

- CaveScene nível 1.
- Sem warning de spawn profiles ausentes.
- Sem fallback de `BestiaryManager`.
- `CreatedEnemies > 0`.
- `EnemySpawnedEvent`/`EnemySeenEvent`.
- Bestiary FirstSeen.

---

## Update 2026-05-29 (sessão 29d) — Assets gerados, EnemyBrain.Configure corrigido

### Correções desta sessão

**1. EnemyBrain.Configure(EnemyDataSO) — compile error corrigido**

`CaveRuntimeMaterializer:907` chama `brain.Configure(enemyData)`. O SPEC 13D rewrite do EnemyBrain não possuía esse método, causando erro de compilação. Adicionado:

```csharp
// Assets/_Game/Scripts/Enemy/EnemyBrain.cs
public void Configure(EnemyDataSO data)
{
    _enemyData = data;
}
```

**2. GenerateAndWireSpec13GAssets.Execute() — batchmode entry**

```csharp
// Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateAndWireSpec13GAssets.cs
public static void Execute() => GenerateAndWire();
```

Permite: `Unity.exe -batchmode -executeMethod CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.Execute`

**3. Assets YAML criados via PowerShell**

Script: `tools/unity/GenerateSpawnEcologyAssets.ps1`

Resultado confirmado:

```text
Profiles: 40  Packs: 17  Locks: 7
```

Pastas criadas com `.meta`:
- `Assets/_Game/Data/EnemySpawn/`
- `Assets/_Game/Data/EnemySpawn/Profiles/`
- `Assets/_Game/Data/EnemySpawn/Packs/`
- `Assets/_Game/Data/EnemySpawn/FactionLocks/`

### Wiring da CaveScene — ainda bloqueado

```text
Asset generation via Unity batchmode: BLOQUEADO
Razão: Unity Editor ainda aberto com o projeto
```

### Ação necessária (única pendência)

Com o Unity Editor aberto, aguardar recompilação e executar o menu:

```text
CindarsHope → SPEC 13 → Generate And Wire SPEC 13G Assets
```

Os 64 assets já existem no disco — o menu fará apenas populate + wiring da CaveScene + serialize BestiaryManager.
