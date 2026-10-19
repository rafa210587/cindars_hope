# Farm e wiring — reconciliação de 2026-09-27

Status: correções locais validadas; menu global FAIL; aceite humano NOT RUN.

O contrato de agrupamento sul ainda limitava gaps a 5.5u/span a 15.5u, apesar da
spec_farm_enclosed_valley_keyart_v2, ambient v15 K3, autorizar X=-13.1,-7.2,-1.0,4.9.
Os limites agora são 6.2u/18u com tolerância numérica de 0.001u; cinco casos verificam
posições autorizadas, excesso de gap/span, sobreposição e inversão. Nenhuma cena ou
posição foi alterada. Fireball recebeu o prefab existente pelo reparador Editor;
comparação do conteúdo LFS confirmou alteração somente de ProjectilePrefab.

## Evidência executada

Unity 6000.5.7f1 (017862109af0); artefatos em `TestResults/farm-wiring-reconciliation/`.
Seleção: SPEC_VALIDATION_MATRIX_MASTER, pure logic/bugfix e SO/wiring.

- Baseline EditMode: 45/46, FAIL no agrupamento sul. `baseline.xml`/`baseline.log`.
- Final EditMode: **56/56 PASS**, Unity/runner exit 0, scanner PASS. `final.xml`/`final.log`.
- Filtro final: `FarmSceneCompositionContractTests;FarmSceneSpatialContractTests;FarmSceneNavigationContractTests;FarmLevel1;FarmDecorationPlannerTests`.
- Comando: `pwsh -File tools/unity/RunUnityEditModeTests.ps1 -TestFilter '<filtro acima>' -ResultsPath TestResults/farm-wiring-reconciliation/final.xml -LogFile TestResults/farm-wiring-reconciliation/final.log -TimeoutSeconds 240`.
- Reparador: `-executeMethod CindarsHope.EditorTools.Repair.RepairProjectilePrefabReferences.RepairViaCommandLine`, exit 0, `projectile-repair.log`; arco já correto.
- Menu: `-executeMethod CindarsHope.Editor.CindarsHopeMenu.ValidateProjectBatch`, exit 1, `validators.log`. Ambos os batches usaram `-batchmode -quit -nographics -projectPath <repo> -logFile <log>` via UnityValidation.Common.ps1, timeout 240s.
- Passo de projectile prefabs: **PASS, zero issues**. Não comprova cast em PlayMode.
- As quatro falhas Farm de 08/09 não reapareceram nos testes selecionados atuais.
- Diff check PASS; cenas, settings e trabalhos preexistentes preservados. Sem commit/promoção.

## Pendências confirmadas

- Picaretas iron/steel: catálogo cria Category=Weapon/IsEquippable, mas gerador de
  WeaponData filtra apenas `item_weapon_*`, excluindo `item_tool_pickaxe_*`.
  Corrigir exige fechar seleção/mapeamento e dados mecânicos das duas ferramentas;
  nenhum WeaponId fictício, categoria ou dano foi inventado nesta rodada.
- Transições: revisão independente read-only confirmou que ValidateSceneTransitions
  aceita apenas SceneTransitionGate/SceneSpawnAnchor. Cenas usam ScenePortal/
  SceneSpawnPoint, CaveEntranceInteractable e saída/spawn procedural da Cave.
  Os oito erros não comprovam oito transições quebradas. Corrigir o validador exige
  testar contratos reais e destinos/spawns ausentes, preservando IDs e stable run.
- Farm layout: menu acusa ausência do objeto `Bounds` (check legado v6/64x44).
  Reconciliar com a fronteira física atual; não criar um objeto vazio para obter PASS.
- Docs/strict, PlayMode integrado/humano e build Player não executados nesta rodada.
  Permanecem pendentes circulação real, entrada/saída Cave e cast de Fireball.

## SHA256 dos inputs alterados

- FarmSceneCompositionContract.cs: `9D5C826B75FF9BB2C6D042BCD7EEA7A2CD6ED88D02E4C904BBE9DB2B350B59B2`
- FarmSceneCompositionContractTests.cs: `0B33FE49985F3735DECD96559703283A2383178143E7C46CE8C57A37EAAA27DE`
- spell_fireball.asset: `5301EC0235FA2422E09174EC45436819B265E0F7D667D67FC505745D7DE068DC`
