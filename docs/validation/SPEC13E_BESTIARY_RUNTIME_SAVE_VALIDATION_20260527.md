# SPEC 13E - Bestiary Runtime/Save Validation - 2026-05-27

## Resumo

Implementado o recorte SPEC 13E para Bestiary runtime/save por IDs:

- `BestiaryManager` agora mantém estado runtime em memória e captura/restaura `BestiarySaveData`.
- `GameSaveData` ganhou seção `Bestiary`.
- `SaveManager` captura e restaura Bestiary junto ao save/load existente.
- `GameBootstrap` garante um único `BestiaryManager` persistente no bootstrap e o injeta no `SaveManager`.
- `EnemyBestiaryEntrySO` foi criado como contrato textual de bestiary.
- Foi criado gerador Editor para entries do roster oficial 13B.
- Foi criado validator Editor para SPEC 13E.

## Quantidade de entries

- Entries textuais codificadas no gerador: 44 ids do roster 13B atual.
- Assets `.asset` criados nesta execução: 0, porque a geração depende do menu Unity `CindarsHope > SPEC 13 > Create Bestiary Entries 40`.
- O validator exige pelo menos 40 entries linkadas quando os assets do roster/bestiary forem gerados.

## Eventos integrados

`BestiaryManager` assina e remove assinatura corretamente:

- `EnemySpawnedEvent` -> FirstSeen.
- `EnemySeenEvent` -> FirstSeen.
- `EnemyDamagedEvent` -> garante entry vista quando houver dano legado.
- `DamageAppliedEvent` -> descobre fraquezas/resistências/imunidade e vulnerability window quando `DamageResult` expõe os dados.
- `EnemyKilledEvent` -> incrementa `KillCount` e descobre drop direto do evento.
- `EnemyLootRolledEvent` -> descobre drops quando houver roll/drop explícito.

Evento ajustado:

- `BestiaryEntryUpdatedEvent` agora inclui `EnemyId`, `UpdateType` e `BestiaryEntryId`, todos tipos simples.

## Save DTOs criados/alterados

- `BestiarySaveData`
- `BestiaryEntrySaveData`
- `GameSaveData.Bestiary`

Campos persistidos por entry:

- `EnemyId`
- `FirstSeen`
- `KillCount`
- `DropsDiscovered`
- `WeaknessesDiscovered`
- `ResistancesDiscovered`
- `VulnerabilityWindowDiscovered`
- `LastSeenCaveLevel`

Todos usam IDs/tipos simples. Nenhuma referência Unity é serializada.

## Integrações runtime

- `Combat.EnemyHealth` agora preserva `DamageRequest.TargetId` ao publicar `DamageAppliedEvent`.
- `Combat.EnemyHealth` passa `EnemyVulnerabilityState.Multiplier` para `DamageCalculator`, permitindo discovery de vulnerability window via `DamageResult.WasVulnerable`.
- Não foi criado sistema paralelo de enemy runtime, dano, loot, cave snapshot ou save.

## Validações executadas

- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Primeira tentativa sandbox: falhou por `Access to the path Temp\obj\... is denied`.
  - Reexecutado escalado: sucesso, 0 erros, 0 avisos.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Sucesso, 0 erros.
  - 3 avisos pré-existentes em `CreateEnemyActionsAndSets.cs`.
- `rg "GameObject\.Find|FindObjectOfType|FindObjectsByType|StreamingAssets" ...`
  - Sem ocorrências nos arquivos runtime alterados/criados.
- `git diff --check`
  - Primeira tentativa sandbox: falhou por erro Git/MSYS `CreateFileMapping ... Win32 error 5`.
  - Reexecutado escalado: sem problemas reportados.
- `.\tools\docs\validate_docs.ps1`
  - Sucesso.
- `.\tools\unity\RunUnityCompileValidation.ps1`
  - Não concluiu por ambiente: Unity informou que outra instância já está com o projeto aberto.
  - Log: `Logs/unity-compile-validation.log`.
- `.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`
  - Encontrou apenas a falha fatal de ambiente por batchmode abortado; não houve evidência de `error CS` no scan manual desta execução.
- `Select-String` por `error CS` / `warning CS` em `Logs/unity-compile-validation.log`
  - Sem ocorrências.

## Validações pendentes

- Fechar a instância Unity aberta e rerodar Unity batchmode/compile pelo script oficial.
- Executar no Unity:
  - `CindarsHope > SPEC 13 > Create Roster 40 Enemy Data`, se os assets ainda não existirem.
  - `CindarsHope > SPEC 13 > Create Bestiary Entries 40`.
  - `CindarsHope > Validation > Validate SPEC 13E - Bestiary Runtime Save`.
- Play Mode:
  - Spawnar inimigo testável.
  - Confirmar FirstSeen.
  - Matar inimigo e confirmar KillCount.
  - Confirmar DropsDiscovered quando drop/evento existir.
  - Confirmar Weaknesses/Resistances quando `DamageResult` expuser multiplicador.
  - Acertar durante vulnerability window e confirmar `VulnerabilityWindowDiscovered`.
  - Salvar/carregar e confirmar persistência.

## Riscos residuais

- Assets do roster 13B e bestiary ainda dependem de geração via menu Unity.
- SPEC 13B e SPEC 13C continuam usando rosters diferentes; este recorte respeita o roster 13B solicitado.
- Descoberta de resistência/fraqueza depende de `DamageResult.CombatResistanceMultiplier`; pontos de dano que ainda não passam profile de resistência não revelarão esses campos.
- `LastSeenCaveLevel` está suportado no DTO/API, mas os eventos atuais não carregam cave level.

## Fora de escopo confirmado

- SPEC 13F SpawnResolver/ecology/faction locks não foi implementada.
- Cave snapshot não foi implementado.
- UI final do Bestiary não foi implementada.
- Boss fight não foi implementada.
- Balance final de loot/drop não foi alterado.
