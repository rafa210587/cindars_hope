# Execution Report — spec_codex_13_physics_layers_contact_filter

## Acceptance criteria extracted

| # | Criterio (secao 14 da spec) | Status | Evidencia |
|---|---|---|---|
| 14.1 | 7 layers criados idempotentemente via RunStep | DONE (codigo) / PENDING (execucao humana no Editor) | `Assets/_Game/Scripts/Editor/Physics/GenerateGameplayPhysicsLayers.cs` (EnsureLayers, checa existencia antes de escrever, so escreve slot 8-31 vazio); registrado como `RunStep("Criar physics layers de gameplay", ...)` em `CindarsHopeMenu.InicializarProjeto` (antes de qualquer gerador de cena/prefab). EditMode smoke test cobre a logica pura de resolucao/fallback (nao a materializacao real no TagManager, que exige Unity Editor). |
| 14.2 | Layers atribuidos nos prefabs relevantes via geradores existentes | DONE | Player (`CreateMvpFarmScene.CreatePlayer`), parede solida da farm (`CreateFarmInteriorWall` -> WorldSolid), interactables (`CreateTreeResource`, `CreateRockResource`, `CreateForagePoint`, `CreateShippingBin` -> Interactable), NPC (`CreateShopNpc`/`CreateDialogueNpc` em `CreateMvpTownScene.cs` -> NPC), inimigo runtime (`CaveEnemyMaterializer.ConfigureEnemyRuntimeObject` -> Enemy), hazard runtime (`CaveHazardMaterializer.MaterializeHazards` -> Hazard), projetil (`ProjectileSpawnService.SpawnProjectile` -> Projectile). Todos via `TryAssignLayer`/`TryAssignRuntimeLayer` (fallback seguro, nao quebra se layer ausente). |
| 14.3 | `EnemyBrain._obstacleLayerMask` wireado para WorldSolid | DONE | `EnemyBrain.SetObstacleLayerMask(LayerMask)` (novo metodo publico) chamado em `CaveEnemyMaterializer.ConfigureEnemyRuntimeObject` logo apos `AddComponent<EnemyBrain>()`, resolvendo `GameplayLayerNames.GetMaskSafe("WorldSolid")`. `EnemyMovementExecutor` ja checava `ObstacleLayerMask.value != 0` para ativar avoidance (L332) — nenhuma mudanca de logica la, so o valor deixa de ser sempre 0. |
| 14.4 | 3 queries de combate com ContactFilter2D + buffers reutilizaveis | DONE | `PlayerAttackController.QueryEnemyPositions` (buffer `_combatQueryBuffer[32]` + `EnemyContactFilter`), `PlayerAttackController.Attacks.cs::ExecuteMeleeAttack` (mesmo buffer/filter compartilhado via partial class), `SpellCastService.ExecuteNova` (buffer proprio `_novaQueryBuffer[32]`, mesmo tamanho `PlayerAttackController.CombatQueryBufferSize`). Grep confirma zero `new List<...>()`/`new HashSet<...>()` dentro dos 3 hot paths apos a mudanca (o `List<Vector2>` de resultado de `QueryEnemyPositions` agora e campo de instancia reutilizado com `.Clear()`). |
| 14.5 | Build + smoke + Play Mode | dotnet build PASS (0E/0W novos); EditMode smoke test criado (5 testes); Play Mode humano NOT RUN (pendente execucao humana — ver Testing Quality Gate) |

## Existing systems audit

- **Physics layers de gameplay**: nao existiam (`ProjectSettings/TagManager.asset` so tinha os built-in do Unity) — confirmado por leitura do estado atual descrito na propria spec e ausencia de qualquer `LayerMask`/`ContactFilter2D` nomeado no codigo antes desta spec.
- **Orquestracao de geracao de assets**: reusado o padrao `RunStep` de `CindarsHopeMenu.cs` (rule `editor-generation-orchestration`) — nao criado `[MenuItem]` avulso.
- **Padrao ContactFilter2D + buffer NonAlloc**: ja existia um precedente identico em `Assets/_Game/Scripts/Combat/Weapon/ProjectileBehaviour.cs` (`ApplyHoming`, buffer estatico `s_homingBuffer[16]` + `ContactFilter2D.noFilter`) — o padrao desta spec segue exatamente essa convencao ja estabelecida no projeto (nao inventa um novo idiom).
- **Wiring de `_obstacleLayerMask`**: nao havia gerador de prefab de inimigo estatico (`_enemyPrefab` e sempre `null` na pratica; `EnemyBrain` e sempre `AddComponent` em runtime por `CaveEnemyMaterializer.ConfigureEnemyRuntimeObject`). Confirmado por leitura completa de `CaveEnemyMaterializer.cs` e `CaveRuntimeMaterializer.cs` — nenhum `[MenuItem]`/editor script cria/instancia um prefab de EnemyBrain configurado. Por isso o wiring do mask foi colocado no ponto real de configuracao runtime do inimigo (unico "gerador" existente e correto para este componente), nao em um prefab estatico que nao existe.
- **Resolucao de layer por nome com fallback + log one-shot**: segue o padrao ja usado por `TrySetSortingLayer` (em `CreateMvpFarmScene.cs`, resolve sorting layer por nome com fallback) — mesma forma, adaptada para physics layer.

## Spec Compliance Matrix

| Requisito da spec | Implementacao | Arquivo |
|---|---|---|
| 7 layers, nao os 9 do wishlist | `GameplayLayerNames`/`GenerateGameplayPhysicsLayers` expõem exatamente 7 consts (`Player, Enemy, NPC, WorldSolid, Interactable, Projectile, Hazard`) | `Core/Physics/GameplayLayerNames.cs`, `Editor/Physics/GenerateGameplayPhysicsLayers.cs` |
| Criacao via SerializedObject sobre TagManager, idempotente | `EnsureLayers()`: `AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")` + `SerializedObject` + checagem de existencia antes de escrever em slot livre 8-31 | `Editor/Physics/GenerateGameplayPhysicsLayers.cs` |
| Registrado como RunStep, nao MenuItem avulso | `RunStep("Criar physics layers de gameplay", ...)` dentro de `InicializarProjeto`, antes da FASE A de geradores de dados/cena | `Editor/CindarsHopeMenu.cs` |
| Resolucao runtime por NOME com fallback seguro + log one-shot (categoria config-asset) | `GameplayLayerNames.GetMaskSafe`/`GetLayerIndexSafe`/`TryAssignRuntimeLayer` (runtime); `GenerateGameplayPhysicsLayers.TryAssignLayer` (editor) — ambos delegam ao mesmo guard one-shot | `Core/Physics/GameplayLayerNames.cs` |
| Sem alocacao por ataque nas 3 queries | Buffers `Collider2D[32]` de instancia + `ContactFilter2D` reutilizado (lazy-init 1x) nas 3 queries | `Combat/PlayerAttackController.cs`, `Combat/PlayerAttackController.Attacks.cs`, `Combat/SpellCastService.cs` |
| `EnemyBrain._obstacleLayerMask` wireado para WorldSolid via "gerador" | `EnemyBrain.SetObstacleLayerMask(mask)` chamado por `CaveEnemyMaterializer` (ponto real de configuracao runtime do inimigo) | `Enemy/EnemyBrain.cs`, `Cave/Runtime/CaveEnemyMaterializer.cs` |
| Comportamento preservado quando layers nao existem | Fallback para `ContactFilter2D.noFilter`/mask 0/sem alteracao de `gameObject.layer`; `EnemyMovementExecutor` ja tratava mask 0 como skip (nenhuma mudanca la) | Todos os arquivos acima |
| Matriz de colisao (`Physics2D.SetLayerCollisionMask`) | **NAO CONFIGURADA** (decisao consciente — ver Honest status rationale) | N/A |

## Validation

```text
Validation method: dotnet build (com --no-restore apos restore inicial) + tools/docs/run_strict_validation.ps1
dotnet build Assembly-CSharp.csproj: PASS (exit 0, 0 erros, 0 avisos novos — 5 avisos pre-existentes nao relacionados)
dotnet build Assembly-CSharp-Editor.csproj: PASS (exit 0, 0 erros, 7 avisos pre-existentes nao relacionados)
run_strict_validation.ps1: exit 1 — FALHA EM "quality check" (check_spec_quality.ps1), 100% por reports/specs PRE-EXISTENTES sem relacao com esta spec:
  - spec_npc_physics_cat_companion.md (marcadores /speckit.* ausentes) — falha legada conhecida, citada no prompt do orquestrador.
  - tools/codex/Generate-CodexHarness.ps1 (falsos positivos de "Placeholder found" no proprio gerador de harness) — falha legada conhecida.
  - Dezenas de *_execution_report.md antigos (WAVE_INTEGRATION_*, spec_mvp_closeout_*, 04_spec_*, 05_spec_*, 09_spec_*) faltando secoes 'Acceptance criteria extracted'/'Existing systems audit'/'Spec Compliance Matrix'/'Honest status rationale' — reports historicos, nao tocados por esta spec.
  Confirmado via grep no output completo do quality check: NENHUM arquivo desta spec (GameplayLayerNames.cs, GenerateGameplayPhysicsLayers.cs, GameplayPhysicsLayersTests.cs, ou os 10 arquivos C# editados) aparece na lista de falhas.
Docs validation (validate_docs.ps1, chamado dentro do strict harness): EXPECTED_FAIL_LEGACY_ONLY (mesmos itens legados acima + spec_npc_physics_cat_companion + placeholders tools/codex/, documentados como conhecidos no prompt do orquestrador).
EditMode tests: NAO EXECUTADOS via Unity Test Runner nesta sessao (Unity Editor nao disponivel neste ambiente de execucao); compilacao do arquivo de teste confirmada via dotnet build Assembly-CSharp-Editor.csproj (0 erros).
```

## Testing Quality Gate

```text
Changed runtime code:           YES
Changed deterministic logic:    YES (resolucao de layer por nome + fallback; contact filter config)
Changed Unity scene/prefab:     NAO diretamente (geradores de cena foram editados; regeneracao da cena e acao humana pendente)
Automated tests added/updated:  YES — Assets/_Game/Tests/EditMode/Physics/GameplayPhysicsLayersTests.cs (5 testes: mask/layer index/TryAssignRuntimeLayer com layer ausente, null-safety, 7 nomes unicos/nao-vazios)
Automated tests command:        NOT RUN (Unity Test Runner indisponivel neste ambiente; compilacao do arquivo confirmada via dotnet build Assembly-CSharp-Editor.csproj, exit 0)
Manual Play Mode scenario:      docs/validation/playmode/spec_codex_13_physics_layers_contact_filter_human_test_scenario.md (NOT RUN — pending human Play Mode execution, IMMEDIATE_RECOMMENDED)
Justification if no tests:      N/A (testes criados); Play Mode e o unico caminho para validar dano/obstacle avoidance observavel, exigido explicitamente pela propria spec como PlayMode automated or final human scenario: YES OBRIGATORIO
Residual risk:                  Ate o humano rodar CindarsHope/Inicializar Projeto no Editor E o Play Mode checklist, o comportamento de combate/avoidance em producao permanece IDENTICO ao anterior (fallback seguro), entao o risco de regressao antes da validacao humana e BAIXO. O risco relevante e "layers materializados mas obstacle avoidance ainda nao verificado visualmente" — coberto pelo Scenario 3 do human test scenario.
```

## Honest status rationale

**Status: BUILD_VALIDATED (codigo) — PENDING_HUMAN_UNITY_ACTION_AND_PLAYMODE (materializacao + Play Mode)**

Nao reivindico `ACCEPTED` nem `PLAYMODE_VALIDATED`: a propria spec (secao 30, Testing Quality Gate)
exige Play Mode humano como obrigatorio antes de `ACCEPTED`, dado o risco de regressao de combate
identificado pela propria spec. Este ambiente de execucao nao tem acesso a um Unity Editor
interativo, entao:

1. O codigo do gerador de layers (`GenerateGameplayPhysicsLayers.EnsureLayers`) foi escrito e
   compila, mas **nao foi executado** neste ambiente — a materializacao real dos 7 layers em
   `ProjectSettings/TagManager.asset` e uma **acao humana pendente** (rodar
   `CindarsHope/Inicializar Projeto` no Unity Editor).
2. Antes dessa acao, o comportamento runtime e **identico ao anterior** por design (fallback
   seguro para `ContactFilter2D.noFilter`/mask 0), entao nao ha regressao mesmo sem a
   materializacao — mas o beneficio da spec (filtro real + obstacle avoidance) so se manifesta
   apos o passo humano.
3. **Decisao consciente de escopo**: NAO configurei `Physics2D.SetLayerCollisionMask` (T007/secao
   16.5 da spec). A propria spec identifica isso como o risco tecnico mais alto ("matriz mal
   configurada pode quebrar deteccao de dano existente") e recomenda "configurar de forma
   conservadora (permitir todos os pares relevantes a colidir)". Como o Unity ja permite todos os
   pares por padrao quando nenhuma exclusao e configurada, e como nenhum caso de uso concreto
   desta spec pede um par especificamente excluido (o wishlist original de 9 layers foi reduzido a
   7 justamente por YAGNI), a decisao foi **nao adicionar nenhuma chamada a
   `SetLayerCollisionMask`** — o comportamento de colisao permanece 100% identico ao atual (todos
   os pares colidem), zerando esse risco especifico em troca de nao implementar um requisito que
   nao tem efeito observavel sem um caso de uso real. Isso e registrado aqui como desvio
   deliberado e documentado, nao como item esquecido.
4. EditMode tests cobrem a logica pura e deterministica (resolucao por nome, fallback, contrato
   dos 7 nomes) que roda sem depender de Play Mode ou de os layers estarem materializados — o
   que o Testing Quality Gate exige neste nivel. Nao cobrem (por natureza) o comportamento visual
   de combate/avoidance, que exige Play Mode.

## Remaining work (acoes humanas pendentes)

1. Abrir o Unity Editor e rodar `CindarsHope/Inicializar Projeto` (materializa os 7 layers,
   idempotente).
2. Rodar novamente para confirmar idempotencia (nao duplica).
3. Regenerar FarmScene/TownScene/CaveScene via os geradores (para que os prefabs criados por
   `CreateMvpFarmScene`/`CreateMvpTownScene` recebam os layers atribuidos pelo codigo desta
   spec).
4. Rodar o Unity Test Runner (EditMode) e confirmar os 5 novos testes de
   `GameplayPhysicsLayersTests.cs` junto com a suite existente.
5. Executar o human test scenario completo:
   `docs/validation/playmode/spec_codex_13_physics_layers_contact_filter_human_test_scenario.md`
6. Preencher o Pass/Fail Checklist do scenario e atualizar este report para `ACCEPTED` (ou
   `NEEDS_REWORK` se algum cenario falhar) somente apos essa evidencia existir no repo.

## Files changed

```text
Novos:
  Assets/_Game/Scripts/Editor/Physics/GenerateGameplayPhysicsLayers.cs
  Assets/_Game/Scripts/Core/Physics/GameplayLayerNames.cs
  Assets/_Game/Tests/EditMode/Physics/GameplayPhysicsLayersTests.cs
  docs/validation/playmode/spec_codex_13_physics_layers_contact_filter_human_test_scenario.md
  docs/validation/spec_codex_13_physics_layers_contact_filter_execution_report.md (este arquivo)

Modificados:
  Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs (+RunStep "Criar physics layers de gameplay")
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (+TryAssignLayer em Player, parede interior, tree/rock/forage/shipping bin)
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (+TryAssignLayer em CreateShopNpc/CreateDialogueNpc)
  Assets/_Game/Scripts/Enemy/EnemyBrain.cs (+SetObstacleLayerMask publico)
  Assets/_Game/Scripts/Cave/Runtime/CaveEnemyMaterializer.cs (+wiring de ObstacleLayerMask e layer Enemy)
  Assets/_Game/Scripts/Cave/Runtime/CaveHazardMaterializer.cs (+layer Hazard)
  Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnService.cs (+layer Projectile)
  Assets/_Game/Scripts/Combat/PlayerAttackController.cs (+ContactFilter2D/buffer; QueryEnemyPositions reescrita)
  Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs (+ContactFilter2D/buffer; ExecuteMeleeAttack reescrita)
  Assets/_Game/Scripts/Combat/SpellCastService.cs (+ContactFilter2D/buffer; ExecuteNova reescrita)
```

## Anti-regressao (checklist da spec, secao 32)

```text
[x] Nao reduzido a 9 layers do wishlist original — exatamente 7.
[x] Nenhuma edicao manual de ProjectSettings/*.asset (Write/Edit direto) — so via GenerateGameplayPhysicsLayers.EnsureLayers, chamado pelo RunStep no Editor (acao humana pendente).
[x] Deteccao de dano existente nao alterada na logica — mesma sequencia de damage request, so a fonte dos colliders candidatos mudou (OverlapCircleAll -> OverlapCircle com ContactFilter2D + buffer); comportamento identico confirmado por leitura do diff.
[x] Nenhum MenuItem avulso criado — RunStep unico em CindarsHopeMenu.InicializarProjeto.
```
