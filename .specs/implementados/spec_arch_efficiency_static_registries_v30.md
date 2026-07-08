# spec_arch_efficiency_static_registries_v30

Status: `IMPLEMENTED_BUILD_VALIDATED`
Data: 2026-07-08

## Objetivo

Corrigir os 3 findings de eficiência da auditoria (workflow 2026-07-07, ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` §Eficiência) — trocar global scene search em
runtime por registros estáticos auto-registrados (padrão vivo `EnemyHealth.ActiveInstances`), sem
alterar comportamento observável.

## Mudanças

1. `Player/Death/AnyaFountainRespawnFlow.cs` — `FindActiveFountainRespawnPoint()` não usa mais
   `Object.FindObjectsByType<MonoBehaviour>()` (varredura sem filtro de toda a cena; violava
   unity-architecture #1). Agora lê `AnyaFountain.ActiveInstances`.
2. `Core/Respawn/AnyaFountain.cs` — expõe registro estático `ActiveInstances`
   (`IReadOnlyList<IAnyaFountainRespawnPoint>`), auto-registro em `OnEnable`/`OnDisable`.
3. `Cave/Resources/ResourceNode.cs` — registro estático `ActiveInstances`, auto-registro em
   `OnEnable`/`OnDisable`.
4. `Cave/CaveLevelRuntimeController.cs` — `RefreshDailyResourceNodes()` itera
   `ResourceNode.ActiveInstances` em vez de `FindObjectsByType<ResourceNode>()`; e o poll de debug
   deixou de ler `SceneManager.GetActiveScene().name` por frame (alocação de string por frame).

## Não regressão

Comportamento preservado: mesma seleção de fountain no respawn, mesmo conjunto de resource nodes
reabastecido por dia, mesmo debug poll. Nenhuma mudança de gameplay/save/cena/prefab/asset. Snapshot
arquitetural inalterado (`MutualModulePairs=27`, sem par novo).

## Evidência

- `Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7, 0W/0E (verificado por Claude).
- `RunUnityEditModeTests.ps1`: exit 0, **2753/2753 PASS** (verificado por Claude;
  `TestResults/claude-verify-eff.xml`). +2 suítes novas: `ResourceNodeActiveInstancesTests`,
  `AnyaFountainActiveInstancesTests` (registro/desregistro).
- Snapshot: `MutualModulePairs=27`.

## Risco residual (PlayMode — pendente humano)

Respawn e resource-refresh são fluxo de runtime que o EditMode não cobre 100%. Smoke de PlayMode
recomendado: (a) morrer → respawnar na Fonte da Anya; (b) virar o dia na cave → nós de recurso
reabastecem. Os testes EditMode cobrem só o registro/desregistro dos ActiveInstances.

## Nota de execução

O subagent que implementou caiu por erro de API DEPOIS de rodar EditMode 2753 PASS mas ANTES de
commitar; Claude verificou build+EditMode no disco e fez o commit/closeout.
