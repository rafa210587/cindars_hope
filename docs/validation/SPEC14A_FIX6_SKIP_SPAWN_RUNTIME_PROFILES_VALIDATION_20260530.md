# SPEC 14A-FIX6 — Skip Spawn / Runtime Profiles Validation

Date: 2026-05-30
Branch: dev
Focus: andares 30/45/60/75/90 vazios via debug skip; integração real de SizeProfile, MovementProfile, ActionSet e VulnerabilityProfile.

## Causa raiz confirmada

Após FIX4 e FIX5 corrigirem o **código** (locks por nível, databases injetadas, escalas por SizeProfile), os logs do usuário ainda mostravam:

- `ProfilesTotal=40`
- Level 30/45/60/75 → `EnemySpawnPlan empty`
- `MovementType=LegacyChase`
- `ActionSetResolved=False`
- `VulnerabilityResolved=False`
- `VisualScale=1,00`

A causa única é **drift de assets**: os 40 `EnemySpawnProfileSO` no disco foram criados antes do FIX5 e ainda têm `RequiredBossGateProgress = "boss_gate_level_15"` (para ice), `"boss_gate_level_30"` (fire), `"boss_gate_level_45"` (ruins). FIX5 removeu esse campo do código gerador, mas só re-executando `Regenerate All Enemy Data` o `PopulateProfile` regrava `asset.RequiredBossGateProgress = ""`. Sem isso:

- `TryRejectProfile` rejeita todos os 8 perfis de ice em level 30 com "Required boss gate progress 'boss_gate_level_15' is missing." (sem boss derrotado, `BossGateProgressIds` é vazio)
- Resultado: `EnemySpawnPlan empty` para level 30/45/60/75/90.

Os mesmos sintomas para Movement/ActionSet/Vulnerability/Size derivam do generator nunca ter rodado após FIX5 → bancos vazios, materializer wired com 40 profiles, ChaseController ativo como fallback.

## Arquivos alterados (FIX6)

| Arquivo | Mudança |
|---|---|
| `Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs` | `BuildDiagnosticSummary` totalmente reescrito; agora reporta `ProfilesAfterLevel/Biome/Environment/FactionLock/Faction/RequiredBossGate/Room`, idem para Packs, + `TopRejectedProfiles` com os 3 motivos mais comuns. Diagnóstico também emitido quando o pack é selecionado mas produz 0 inimigos, ou quando profiles individuais passam filtros mas geram 0. |
| `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` | Novas propriedades públicas: `HasResolvedActionSet`, `ResolvedActionCount`, `HasResolvedMovementProfile`, `HasResolvedVulnerabilityProfile`, `MovementType`, `ResolvedActionSetId`, `ResolvedMovementProfileId`, `ResolvedVulnerabilityProfileId`. Refletem estado **real** pós-`InitActionSet`, não só presença de IDs. |
| `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` | `CombatLog: EnemyRuntimeConfigured` agora usa `brain.HasResolvedActionSet`, `brain.ResolvedActionCount`, `brain.MovementType`. Inclui `EnemyDataLevel=CaveBand`, `Faction`, `MovementProfileId`, `ActionSetId`, `VulnerabilityProfileId`, `SizeProfileId`, `MovementProfileResolved`, `ActionSetResolved`, `VulnerabilityResolved`, `SizeProfileResolved`, `ColliderRadius`. Adiciona `Debug.LogError` quando IDs presentes não resolvem (Movement, ActionSet, Size). |
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateAndWireSpec13GAssets.cs` | Novo método `AssertPostGenerationInvariants` executado ao final: falha com `Debug.LogError` se ProfilesTotal ≤ 40, qualquer perfil tem `RequiredBossGateProgress` não-vazio, PacksTotal < 25, materializer wired com count diferente do disco, ou qualquer database principal sub-populado. |
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateRoster40EnemyData.cs` | Docstring e menu atualizados (`Create Canonical Enemy Roster`). Classe mantém nome para não quebrar `GenerateAndWireSpec13GAssets`. |
| `Assets/_Game/Scripts/Editor/Validation/ValidateEnemyCaveSpawnCoverage.cs` | Reescrito: levels críticos (30/45/60/75/90) viram `LogError` se falham. Checa `ProfilesTotal>=60`, stale `RequiredBossGateProgress` em qualquer perfil, `PacksTotal>=25`. Menu: `CindarsHope/Validate/Enemy Cave Spawn Coverage`. |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AEnemyRuntimeIntegration.cs` | Menu renomeado para `CindarsHope/Validate/Enemy Runtime Integration`. Quatro checks adicionais: `ValidateSpawnProfilesCount` (>=60), `ValidateNoStaleBossGateProgress`, `ValidateSizeProfileScaleVariation` (Tiny não pode 1.00, Large/Huge/Boss não podem 1.00), `ValidateDatabasesPopulated` (ActionSet >=50, Action >=60, Movement >=8, Vuln >=8, Size >=6, Telegraph >=6). |

## Diagnóstico esperado pós-execução do generator

Para **level 30** após `Regenerate All Enemy Data`:

```text
ProfilesTotal=60          (era 40)
ProfilesAfterLevel=8      (ice band, sem mudança)
ProfilesAfterBiome=8
ProfilesAfterEnvironment=8
ProfilesAfterFactionLock=8 (lock_after_gate_15 unlocked at level>=16)
ProfilesAfterFaction=8     (beast/duergar/ice_cult/undead_stronger não bloqueadas)
ProfilesAfterRequiredBossGate=8  ← KEY: era 0 antes do generator rodar
ProfilesAfterRoom=8
```

Antes da execução do generator, `ProfilesAfterRequiredBossGate=0` é o sinal definitivo do bug, agora visível.

## Critérios de aceite (revalidar)

| Critério | Como verificar | Estado esperado |
|---|---|---|
| Level 30/45/60/75/90 geram monstros | `Validate > Enemy Cave Spawn Coverage` | Todos PASS |
| ProfilesTotal != 40 | Validator runtime integration | >= 60 |
| MovementType != LegacyChase | CombatLog | `GroundChase`/`KiteRanged`/etc |
| ActionSetResolved=True | CombatLog | True, ActionsCount>=1 |
| VulnerabilityResolved=True | CombatLog | True |
| Tiny SpriteScale != 1.00 | Validator size variation | 0.65 |
| Large/Huge/Boss != 1.00 | Validator size variation | 1.35/1.80/2.20 |
| EnemyChaseController off com Brain | CombatLog | `HasLegacyChase=False` |

## Validações executadas

- `dotnet build Assembly-CSharp.csproj`: PASSOU — 0 erros, 0 avisos.
- `dotnet build Assembly-CSharp-Editor.csproj`: PASSOU — 0 erros, 2 CS0649 pré-existentes.
- `tools/docs/validate_docs.ps1`: pendente.
- Unity batchmode + Play Mode: requerem o usuário executar:
  1. `CindarsHope > Generate > Enemy Runtime Data > Regenerate All Enemy Data`
  2. `CindarsHope > Validate > Enemy Runtime Integration`
  3. `CindarsHope > Validate > Enemy Cave Spawn Coverage`
  4. Play Mode com debug skip para levels 30, 45, 60, 75, 90.

## Pendências

- Usuário executar Regenerate All Enemy Data (refaz assets para limpar `RequiredBossGateProgress` em 24 perfis e popular bands 6-7).
- Usuário rodar validators e confirmar PASS.
- Usuário rodar Play Mode em 30/45/60/75/90 e confirmar logs com `ActionSetResolved=True`, `MovementType≠LegacyChase`, `VisualScale` variando.

## Riscos residuais

- Assets gerados procedualmente (60 EnemySpawnProfileSO + 60 EnemyDataSO + 20 ActionSetSO + 20 ActionSO) precisam ser commitados pelo usuário após `Regenerate All Enemy Data`, ou re-gerados toda nova clonagem.
- Se o usuário executar `Regenerate All Enemy Data` com Unity em outra cena (não a CaveScene), `WireCaveScene` abre/fecha a cena automaticamente; certificar que não há alterações não-salvas em outra cena para evitar perda.
- `BlackstoneWyvern` (lock_after_gate_90) só aparece em level >= 91; níveis abaixo não veem essa faction. Validator de coverage marca isso como esperado.
