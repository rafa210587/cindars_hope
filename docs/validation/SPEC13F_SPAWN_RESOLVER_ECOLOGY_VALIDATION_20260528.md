# SPEC 13F - Spawn Resolver / Ecology / Faction Locks Validation - 2026-05-28

## Resumo

Implementado o recorte SPEC 13F como camada data-driven de resolução de spawn, sem materializar inimigos na cave e sem implementar snapshot/runtime final da SPEC 14.

Entregue:

- `EnemySpawnProfileSO`
- `EnemySpawnPackSO`
- `EnemyFactionLockSO`
- `EnemySpawnRequest`
- `EnemySpawnResult`
- `EnemySpawnCandidate`
- `EnemySpawnResolver`
- `EnemyRoomSizeClass`
- eventos simples de resolução/warning/pack selecionado
- gerador Editor para dados de ecologia/spawn
- validator Editor para SPEC 13F

## Profiles / packs / locks preparados

O gerador `CindarsHope > SPEC 13 > Create Spawn Resolver Ecology Data` prepara:

- 40 spawn profiles canônicos da SPEC 13C/13F.
- 17 packs obrigatórios por band.
- 7 faction locks.

Assets `.asset` criados nesta execução: 0. A materialização depende do menu Unity, seguindo o mesmo padrão das SPECs 13A-13E.

## Packs criados no gerador

- `pack_stone_fauna_basic`
- `pack_grashnaar_kobold_scouts`
- `pack_blackroot_growth`
- `pack_fungal_colony`
- `pack_urudakh_trappers`
- `pack_nyx_ambush`
- `pack_frozen_beasts`
- `pack_duergar_patrol`
- `pack_ice_guardians`
- `pack_ember_swarm`
- `pack_kaand_warband`
- `pack_lava_guard`
- `pack_furnace_guard`
- `pack_rune_shards`
- `pack_gnome_ruin_tinkerers`
- `pack_oathless_dead`
- `pack_puzzle_guardians`

High tier hooks citados na spec (`pack_drow_court`, `pack_ninrorin_broken_echoes`, `pack_corrupted_draconic_nest`, `pack_blackstone_wyvern_arena`) ficaram preparados via locks, mas não foram criados como packs ativos porque os inimigos high tier correspondentes ainda não estão estabilizados no roster atual.

## Faction locks preparados

- `lock_default_low_tier`
- `lock_after_gate_15`
- `lock_after_gate_30`
- `lock_after_gate_45`
- `lock_after_gate_60`
- `lock_after_gate_75`
- `lock_after_gate_90`

## Regras implementadas no resolver

- Determinismo por `EnemySpawnRequest.Seed` usando `System.Random`.
- Filtro por `CaveLevelMin` / `CaveLevelMax`.
- Filtro por `BiomeTags`.
- Filtro por `EnvironmentTags`.
- Filtro por `FactionLockId`.
- Filtro por `RequiredBossGateProgress`.
- Filtro por `EnemyRoomSizeClass`.
- Filtro por `EnemySizeClass`.
- Packs têm prioridade sobre candidatos individuais.
- Sem fallback silencioso quando não há candidate.
- `Warnings` e `RejectedCandidates` retornam causa legível.
- `MaxEnemies` é respeitado.
- `MaxCountPerRoom` é respeitado em seleção individual.

## Cenários cobertos pelo validator

`ValidateSpec13SpawnResolverEcology` codifica cenários para:

- seed fixa gerar resultado estável;
- band 1-10 com stone packs;
- band 11-25 com fungal packs;
- band 26-40 com ice packs + gate 15;
- band 41-55 com fire packs + gate 30;
- band 56-70 com ruins packs + gate 45;
- Large/Huge/Boss filtrados por sala pequena;
- lock fechado filtrando faction/gate;
- lock aberto permitindo faction/gate;
- request sem candidates retornando warning claro.

## Validações executadas

- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Sucesso, 0 erros, 0 avisos.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Sucesso, 0 erros.
  - 3 avisos pré-existentes em `CreateEnemyActionsAndSets.cs`.
- `git diff --check`
  - Sucesso.
- `.\tools\docs\validate_docs.ps1`
  - Sucesso.
- Busca por uso proibido de busca global runtime nos arquivos SPEC 13F via `Select-String`
  - Sem ocorrências.
- `.\tools\unity\RunUnityCompileValidation.ps1`
  - O script retornou exit code 1, mas o log registrou `Batchmode quit successfully` e `Exiting batchmode successfully now`.
  - Não houve `error CS` / `warning CS` relacionados à SPEC 13F.
- `.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`
  - Falhou por mensagens pré-existentes de assemblies `*-firstpass.dll` e package test assemblies "not valid".
  - Essas mensagens não apontam erro de compilação C# do recorte.

## Validações pendentes

- Reexecutar/limpar validação Unity se o projeto exigir gate estrito sem warnings de assemblies `not valid`.
- Executar no Unity:
  - `CindarsHope > SPEC 13 > Create Spawn Resolver Ecology Data`
  - `CindarsHope > Validation > Validate SPEC 13F - Spawn Resolver Ecology`
- Validar Play Mode apenas quando SPEC 14 integrar materialização final dos spawn plans.

## Riscos residuais

- Assets de spawn profiles/packs/locks ainda dependem de geração via menu Unity.
- Roster 13B e 13C continuam divergentes; 13F usa o roster canônico requerido pelos packs desta spec.
- O resolver retorna plano lógico; materialização, snapshot, respawn comum e redistribuição pós-morte continuam fora deste recorte e pertencem à SPEC 14.
- Faction locks usam IDs de gate/story preparados, mas a integração final com progresso de boss gates fica para SPEC 14.

## Fora de escopo confirmado

- Cave generation/runtime completo não foi implementado.
- Snapshot completo de inimigos por sala não foi implementado.
- Respawn de inimigos comuns por 2 dias não foi implementado.
- Redistribuição pós-morte não foi implementada.
- Boss gate completion, unique rewards e boss fights não foram implementados.
- UI não foi implementada.
- Flying runtime não foi implementado.
