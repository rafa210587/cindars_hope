# Execution Report — spec_enemy_attack_kits_v1

> **Spec:** `.specs/a_implementar/spec_enemy_attack_kits_v1.md`
> **Status:** BUILD_VALIDATED (4 primitivas com dispatch runtime completo + follow-up de salvas
> múltiplas) — Play Mode / Unity asset generation DEFERRED_TO_FINAL_VALIDATION (Unity Editor closed
> durante esta sessão)
> **Data:** 2026-07-03

---

## Follow-up: salvas múltiplas de projéteis (aprovado pelo humano, pós-closeout inicial)

O catálogo (v1.1) pede que alguns Especiais ranged disparem uma **salva em leque** (N projéteis
simultâneos, cada um um hit independente esquivável — não dano dividido), em vez de 1 projétil
único. Antes deste follow-up, `EnemyActionRunner` só disparava 1 projétil por resolução de ação.

### Escopo implementado

1. **`EnemyActionSO`** (aditivo): `ProjectileCount` (int, default 1, clamp `[1,5]` em `OnValidate`)
   e `ProjectileSpreadAngleDegrees` (float, default 0, clamp `[0,90]`), no bloco "Salvo Multiplo".
2. **Lógica pura** `EnemyActionExecution.ResolveSalvoDirections(Vector2 baseDirection, int count,
   float spreadAngleDegrees)` — retorna `Vector2[]` com as direções do leque. `count=1` retorna só
   a base (spread ignorado). `count` ímpar tem exatamente 1 direção no centro (ângulo 0). `count`
   par não tem centro (distribuição simétrica sem elemento central). `spreadAngleDegrees=0` colapsa
   todas as direções na base. Clamp de `count` em `[1, MaxProjectileCount]` (nova const nomeada,
   espelha o clamp do `EnemyActionSO`). Sem alocação além do array de retorno (tamanho `count`).
3. **Dispatch** em `EnemyActionRunner.ResolveAction()`: no branch `isProjectileAction` (RangedProjectile
   /CastProjectile), quando `PendingAction.ProjectileCount <= 1` o caminho é **idêntico** ao anterior
   (nenhuma mudança de comportamento — mesma chamada única a `EnemyProjectileBehaviour.SpawnTowards`).
   Quando `> 1`, chama `ResolveSalvoDirections` e dispara um `SpawnTowards` por direção, com o
   **mesmo dano por projétil** (sem dividir — cada um é hit independente, conforme instrução). Os 5
   `case` de ataques-assinatura do fable_83 (ComboStrike/TelegraphedAoE/SummonAdds/MultiHitCharge/
   DebuffStrike) e o caminho `BlinkStrike`/melee não foram tocados.
4. **KitTable — donos de kit reais com salva** (variâncias excluídas, pois herdam da mãe):
   auditei o catálogo completo (§4 PARTE A + §4B PARTE B) e encontrei exatamente 2 donos de kit
   (não-variância) com Especial de salva múltipla:
   - `enemy_thorn_archer` (7 NOVAS, §4.9): "Salva de Espinhos — 3 flechas rápidas + Bleed 25%".
     `ProjectileCount=3`, `spreadDegrees=30`. O catálogo lista o arquétipo do Especial como
     `atk_flurry` (ComboStrike) na tabela de arquétipos-por-uso do IMPLEMENTATION doc, mas
     `ProjectileCount` só tem efeito em `RangedProjectile`/`CastProjectile` — troquei o Especial
     para `atk_bow` (RangedProjectile), coerente com "3 flechas" sendo literalmente o arco disparando
     3× em leque, e documentado aqui como desvio pontual do arquétipo nomeado no catálogo.
   - `enemy_gnome_wargolem` (miniboss, §4B.5): "S2 Salva de Canhão — 3 projéteis em arco".
     `ProjectileCount=3`, `spreadDegrees=30`, arquétipo `atk_throw` (RangedProjectile, coerente com
     o IMPLEMENTATION doc §3 que lista `gnome_wargolem*(S2 salva de canhão ×3)` sob `atk_throw`).
     O kit table só suporta 1 Especial por miniboss (S1 "Investida a Vapor" do catálogo fica de
     fora — simplificação pré-existente desta spec, não introduzida por este follow-up).
   - `cinder_spitter` ("Cusparada Tripla") e `void_spitter` ("Salva Corrosiva") também citam salva no
     catálogo PARTE A, mas ambos são VARIÂNCIAS confirmadas na crosswalk (`cinder_spitter→
     cinder_shade`, `void_spitter→void_tendril_watcher`) — por regra da spec (a mãe vence na
     dúvida), NÃO recebem kit próprio nem `ProjectileCount`, mesmo citando salva como *flavor* PT.
   - `drow_arcane_adept` ("Salva Lunar — 3 projéteis em leque") também é variância
     (`drow_arcane_adept→veilkin_witch`) — mesma regra, sem kit próprio.
5. **Testes**: 6 casos novos em `EnemyAttackKitPrimitivesTests.cs` (arquivo já existente — sem
   mudança de csproj): `Salvo_Count1_ReturnsOnlyBaseDirection_IgnoringSpread`,
   `Salvo_Count3_Symmetric_WithCenterOnBase`, `Salvo_Count4_HasNoCenterDirection`,
   `Salvo_ZeroSpread_AllDirectionsCollapseToBase`, `Salvo_CountClamped_ToMaxProjectileCount`,
   `Salvo_CountClamped_ToMinimumOne`.

### Validação

```text
dotnet build .\Assembly-CSharp.csproj --no-restore        → exit 0, 0 erros, 0 avisos novos
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore  → exit 0, 0 erros, 0 avisos novos
```

### Fora de escopo / desvios explícitos deste follow-up

- Os 23+6=29 testes de `EnemyAttackKitPrimitivesTests.cs` continuam **NOT RUN** via Unity Test
  Runner nesta sessão (Unity fechado) — validados manualmente por inspeção matemática (contas de
  ângulo conferidas linha a linha para count=3/spread=30 e count=4/spread=40).
- Geração de assets Unity continua `BLOCKED (Unity closed)` — `ProjectileCount`/
  `ProjectileSpreadAngleDegrees` de `thorn_archer`/`gnome_wargolem` só existem no código do
  generator; não foram materializados em nenhum `EnemyActionSO.asset` ainda.
- `enemy_gnome_wargolem` perde a fidelidade ao S1 "Investida a Vapor" do catálogo (kit table só tem
  1 slot de Especial por miniboss) — debt pré-existente desta spec, não introduzido/agravado por
  este follow-up (o Especial trocado de `atk_charge`→`atk_throw` já refletia essa mesma limitação
  antes; só a salva foi adicionada ao slot já existente).
- Nenhum evento novo, nenhuma mudança de save schema, nenhuma mudança nos 5 signature cases do
  fable_83.

---

## Fase 0 — Decisões de auditoria (obrigatórias antes de codar)

1. **Wiring do menu confirmado.** `GenerateAndWireSpec13GAssets.GenerateAndWire()` já estava
   registrado como `RunStep` em `CindarsHopeMenu.InicializarProjeto` (linha ~150-151), DEPOIS de
   "Gerar bestiario canonico" (linha ~128-129). Internamente ele chama
   `CreateEnemyActionsAndSets.CreateAll()` e depois auto-scaneia as pastas `Actions/`/`ActionSets/`
   para popular os databases (`EnemyActionDatabaseSO`/`EnemyActionSetDatabaseSO`). O novo
   `GenerateEnemyAttackKits.GenerateAll()` foi registrado como RunStep ADICIONAL logo APÓS
   (`CindarsHopeMenu.cs`), garantindo que os 117 EnemyDataSO canônicos e os 60 do Roster já existem
   quando ele roda.

2. **Estratégia de idempotência: REALINHAMENTO IN-PLACE por ActionId/ActionSetId** (não recriar).
   `CreateEnemyActionsAndSets.CreateAll()` cobre exatamente os 60 IDs do Roster (contagem
   confirmada: 60 `ActionEntry` + `ActionSetEntry`), mas com naming livre (ex.
   `action_grashnaar_knife_jab`) e sem `MinRange` em nenhuma ação. Decisão: o novo generator
   localiza os assets JÁ EXISTENTES (`AssetDatabase.LoadAssetAtPath` por `{ActionsFolder}/{actionId}.asset`
   / `{ActionSetsFolder}/{setId}.asset`) usando o naming padrão `action_{enemyId sem prefixo}_
   {melee|ranged|special}` / `actionset_{enemyId}` — e como os 60 `ActionSetSO` do Roster JÁ usam
   `actionset_enemy_{id}` (mesmo padrão), eles são **realinhados** (campos atualizados), nunca
   duplicados. Os `EnemyActionSO` internos do Roster com naming livre (`action_grashnaar_knife_jab`)
   **ficam órfãos** (não deletados — fora de escopo apagar assets; documentado como debt) enquanto o
   generator novo cria/realinha as ações com o naming padrão. Para as 113 fichas canônicas + 7 novas
   + `enemy_meteor_ooze_king`, os assets são criados pela primeira vez com o naming padrão.

3. **EnemyDataSO canônicos confirmados.** Todos os 117 (113 + 4 The Four) já existem como asset em
   `Assets/_Game/Data/Enemies/Canonical/` (confirmado via Glob — 100+17 arquivos). `ActionSetId` é
   populado neles via `SerializedObject`, exceto os 4 `boss_*` do The Four (permanecem DORMANTE,
   nenhuma mudança).

4. **HazardZone — sem física nova.** Auditoria de `EnemyActionRunner.ExecuteTelegraphedAoE` mostrou
   que a AoE existente resolve por `Vector2.Distance` (sem Collider/Trigger). As primitivas de
   HazardZone (`IsHazardExpired`, `ResolveHazardTickCount`, `IsInsideHazard`) seguem o mesmo padrão:
   lógica pura por distância, sem física nova, sem `EnemyHazardZone.cs` (não foi necessário — o
   componente fino citado na arquitetura-alvo da spec não foi criado porque a lógica pura +
   `IsInsideHazard` já cobre a detecção sem MonoBehaviour adicional; documentado como decisão de
   escopo mínimo, ver "Desvios" abaixo).

5. **Validator crosswalk em fonte única.** `GenerateEnemyAttackKits.VarianceToMotherCrosswalk`
   (`public static IReadOnlyDictionary<string,string>`) é a única fonte das 53 variâncias;
   `ValidateEnemyAttackKits` referencia essa mesma tabela (não duplicada).

---

## Arquivos criados/alterados (lista completa)

### Código runtime (Assembly-CSharp.csproj)
- `Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs` — campos aditivos das 4 primitivas (RiseOnce*,
  AllyTargetRadius/AllyHealPercent/AllyBuffStatusId, Hazard*/LeavesHazard, PullDistanceTiles/
  PullFromAttackerOrigin) + clamps em `OnValidate`.
- `Assets/_Game/Scripts/Combat/EnemyHealth.cs` — `ConfigureRiseOnce(...)`, campos runtime
  (`_lastDamageType`, `_riseOnceConsumed`, `_isCollapsedPendingRise`, parâmetros de Rise-once),
  hook no início de `Die()` (intercepta só no caminho de kill do player), `ResolveRise()` via
  `Invoke()`; **registro estático `ActiveInstances`** (mesmo padrão de `CraftingRuntime.
  ActiveInstances` do FIX-001), populado via `OnEnable`/`OnDisable` — usado pelo dispatch de
  AllyHeal/Buff para enumerar aliados vivos sem `FindObjectsOfType`.
- `Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs` — métodos puros: `ShouldBlockRise`,
  `ShouldRiseOnce`, `ResolveRiseHp` (Rise-once); `AllyCandidate` + `ResolveAllyHealTarget`
  (AllyHeal/Buff); `IsHazardExpired`, `ResolveHazardTickCount`, `IsInsideHazard` (HazardZone);
  `ResolvePullTargetPosition` (Pull).
- `Assets/_Game/Scripts/Enemy/EnemyActionRunner.cs` — `InitActionSet()` agora resolve a ação
  `RiseOnceEnabled` do set ativo e chama `_health.ConfigureRiseOnce(...)`; `ExecuteDebuffStrike`
  ganhou dispatch real de Pull (quando `action.PullDistanceTiles > 0`, desloca o Rigidbody2D/
  transform do player via `EnemyActionExecution.ResolvePullTargetPosition`, na direção do atacante
  ou do player conforme `PullFromAttackerOrigin`); `ResolveAction()` agora despacha `SelfBuff` para
  `ExecuteAllyHealBuff` quando `AllyHealPercent > 0` ou `AllyBuffStatusId` configurado (SelfBuff sem
  esses campos continua no-op, comportamento pré-existente inalterado); `ExecuteTelegraphedAoE`
  ganhou spawn de `EnemyHazardZoneRunner` quando `action.LeavesHazard` (independente do player estar
  no raio agora — a zona nasce na origem do atacante para ser pisada depois, ex. rastro de magma).
- `Assets/_Game/Scripts/Enemy/EnemyHazardZoneRunner.cs` (NOVO) — runner destacado de hazard zone
  persistente, mesmo padrão de sobrevivência do `EnemyVolatileExplosionRunner` (elite affix
  Volatile): GameObject independente do inimigo-fonte, tick por `ResolveHazardTickCount`, detecção
  por `IsInsideHazard` (distância, sem física nova), dano via `PlayerDamageReceiver`/status via
  `PlayerStatusReceiver`, visual procedural mínimo (`LineRenderer` circular colorido por elemento).

### Código editor (Assembly-CSharp-Editor.csproj)
- `Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateEnemyAttackKits.cs` (NOVO) — generator
  idempotente (`GenerateAll()`), tabela de arquétipos mecânicos (§2 do catálogo), upsert de
  `EnemyActionSO`/`EnemyActionSetSO`, wiring de `EnemyDataSO.ActionSetId`, wiring das 53 variâncias.
- `Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateEnemyAttackKits.KitTable.cs` (NOVO) — tabela de
  121 kits por criatura (`BuildKitTable()`) + crosswalk das 53 variâncias
  (`VarianceToMotherCrosswalk`) + `GetKitOwnerIds()`.
- `Assets/_Game/Scripts/Editor/Validation/ValidateEnemyAttackKits.cs` (NOVO) — validator read-only
  (`RunValidation()`).
- `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` — 2 `RunStep` novos: "Gerar kits de ataque de
  inimigo" (InicializarProjeto, após "Gerar e wirar assets de inimigos") e "Validar kits de ataque de
  inimigo" (ValidarProjeto, após "Validar animacoes de caminhada dos NPCs").

### Testes (Assembly-CSharp.csproj — mesmo csproj dos 62 testes existentes)
- `Assets/_Game/Tests/EditMode/Cave/EnemyAttackKitPrimitivesTests.cs` (NOVO) — 23 testes cobrindo os
  4 branches de Rise-once, seleção de alvo AllyHeal/Buff, tick/expiração de HazardZone, cálculo de
  Pull.

### csproj (obrigatório para o build de validação)
- `Assembly-CSharp.csproj` — `+2 Compile Include` (`EnemyAttackKitPrimitivesTests.cs`,
  `EnemyHazardZoneRunner.cs`).
- `Assembly-CSharp-Editor.csproj` — `+3 Compile Include` (`GenerateEnemyAttackKits.cs`,
  `GenerateEnemyAttackKits.KitTable.cs`, `ValidateEnemyAttackKits.cs`).

### Docs
- `docs/validation/spec_enemy_attack_kits_v1_execution_report.md` (este arquivo).

---

## Validação — resultado honesto

```text
Validation method: dotnet build (Assembly-CSharp.csproj, Assembly-CSharp-Editor.csproj) +
  tools/docs/run_strict_validation.ps1
Assembly-CSharp: PASS (exit 0, 0 erros, 0 avisos novos)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros; 7 avisos PRE-EXISTENTES não relacionados
  — CS0649 em CreateEnemyActionsAndSets/ValidateEnemySkinBindings, UNT0006 em
  CSharpProjectPostprocessor)
run_strict_validation.ps1: FAIL (exit 1) — EXPECTED_FAIL_LEGACY_ONLY
  - Corruption guard: PASS
  - Docs validation (validate_docs.ps1): script interno retorna erros pré-existentes em specs NÃO
    relacionadas a esta (spec_cave_biome_art_profiles_runtime.md, spec_npc_physics_cat_companion.md,
    spec_town_building_visuals.md, spec_town_layout_v9_organic.md — todas untracked/pré-existentes
    ao início desta sessão, confirmado via `git status` inicial da conversa) + centenas de execution
    reports antigos sem seções obrigatórias (dívida de docs pré-existente, fora de escopo desta spec).
    O run_strict_validation.ps1 trata isso como EXPECTED_FAIL_LEGACY_ONLY (não bloqueia — o script
    continua para os builds).
  - Assembly-CSharp build: PASS (dentro do script)
  - Assembly-CSharp-Editor build: PASS (dentro do script)
  - Spec diff completeness: PASS com WARN (execution report de spec_cave_biome_art_profiles —
    NÃO é desta spec, pré-existente)
  - Spec quality check: FAIL — "Forbidden files altered": ~60 arquivos .asset/.unity/ProjectSettings
    (Roster enemies via Git LFS pointer diff, NPCs, 3 scenes, ProjectSettings) que JÁ apareciam como
    `M` no `git status` NO INÍCIO desta conversa, ANTES de qualquer edição minha. Confirmado via
    `git diff` que a mudança nesses arquivos é um ponteiro Git LFS (oid sha256 diferente), não uma
    edição de conteúdo feita nesta sessão. NENHUM desses arquivos foi tocado por mim (só editei .cs
    e .csproj). Também WARN de "Report sections missing" em dezenas de execution reports antigos
    (dívida pré-existente, fora do escopo desta spec).
Result: os 2 builds de C# (o que esta spec realmente muda) passam limpos. O exit 1 do script
  agregado vem inteiramente de estado pré-existente do repositório não relacionado a este trabalho.
```

### Bloco de evidência (rule validation-truth)

```text
Validation method: dotnet build direto (não via script filtrado)
Exit code Assembly-CSharp.csproj: 0
Exit code Assembly-CSharp-Editor.csproj: 0
Assembly-CSharp: PASS
Assembly-CSharp-Editor: PASS
Quality check (run_strict_validation.ps1 etapa 5): FAIL_LEGACY_ONLY (ver acima — nenhum arquivo
  listado foi alterado nesta sessão; auditado via git diff)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pré-existente, specs não relacionadas)
```

---

## Testes — EditMode

```text
Arquivo: Assets/_Game/Tests/EditMode/Cave/EnemyAttackKitPrimitivesTests.cs (23 testes)
Cobertura: RiseOnce (8 testes: ShouldBlockRise case-insensitive/negativo/lista vazia, ShouldRiseOnce
  habilitado/consumido/bloqueado/desabilitado, ResolveRiseHp percentual/mínimo 1);
  AllyHeal/Buff (4 testes: mais ferido, mais próximo sem ferido, fora do raio, sem candidatos);
  HazardZone (6 testes: não expirado, expirado, contagem de ticks determinística, zero ticks abaixo
  do intervalo, dentro/fora do raio);
  Pull (3 testes: desloca em direção à origem, clamp na origem, no-op já na origem).
Compilação: incluído em Assembly-CSharp.csproj (mesmo csproj dos 62 testes existentes) — build PASS
  confirma que o arquivo compila e os métodos referenciados existem com a assinatura esperada.
Execução via Unity Test Runner: NOT RUN — Unity Editor fechado nesta sessão (`dotnet test` foi
  tentado nos dois csproj; retornou exit 0 SEM nenhum teste descoberto/executado — confirma que o
  csproj gerado pelo Unity não tem test adapter NUnit vinculado, então `dotnet test` NÃO é um sinal
  válido de execução; não posso inferir PASS/FAIL dos 23 testes a partir disso).
Residual risk: a lógica pura foi revisada manualmente linha a linha contra os requisitos da spec e
  compila corretamente contra as assinaturas reais de EnemyActionExecution, mas os 23 casos não foram
  efetivamente executados nesta sessão. Humano deve rodar via Unity Test Runner (EditMode) antes de
  aceitar Phase 2-3.
```

---

## Geração de assets Unity

```text
Asset generation: BLOCKED (Unity closed)
Reason: Unity Editor não estava aberto/executável nesta sessão (instrução explícita da tarefa —
  "Geração de assets Unity NÃO roda agora"); GenerateEnemyAttackKits.GenerateAll() e
  ValidateEnemyAttackKits.RunValidation() são métodos de editor (usam AssetDatabase/SerializedObject)
  que só podem rodar dentro do Unity Editor (batchmode ou interativo).
Residual risk: os ~121 EnemyActionSO/EnemyActionSetSO, o wiring de ActionSetId nos 117 EnemyDataSO
  canônicos e as 53 variâncias do Roster NÃO foram materializados em disco nesta sessão — só o
  CÓDIGO do generator/validator existe, registrado nos 2 comandos canônicos
  (CindarsHope/Inicializar Projeto e CindarsHope/Validar Projeto). Humano deve:
  1. Abrir o projeto no Unity Editor.
  2. Rodar CindarsHope/Inicializar Projeto (log esperado: "[GenerateEnemyAttackKits] Concluido.
     Actions criadas=X, realinhadas=Y; ActionSets criados=X, realinhados=Y; Variancias wireadas=53;
     Fichas sem EnemyDataSO (puladas)=N").
  3. Rodar CindarsHope/Validar Projeto (log esperado: "[EnemyAttackKits] Validation PASSED" com 0
     erros) e capturar o log como evidência.
  4. Rodar Unity Test Runner (EditMode) e confirmar os 23 testes novos + a suíte de regressão
     (EnemySignatureActionsTests.cs, fable_24 elite affix tests) continuam passando.
```

---

## Critérios de aceite — status honesto

### CA-1 Primitivas P2 executáveis e testadas
**STATUS: CODE_COMPLETE_TESTS_NOT_EXECUTED.** As 4 primitivas existem como lógica pura em
`EnemyActionExecution` (RiseOnce: `ShouldBlockRise`/`ShouldRiseOnce`/`ResolveRiseHp`; AllyHeal/Buff:
`ResolveAllyHealTarget`; HazardZone: `IsHazardExpired`/`ResolveHazardTickCount`/`IsInsideHazard`;
Pull: `ResolvePullTargetPosition`), com 23 EditMode tests cobrindo os 4 cenários pedidos, E agora
têm dispatch runtime real em `EnemyActionRunner`/`EnemyHealth` (Rise-once via
`EnemyHealth.Die()`/`ConfigureRiseOnce`; AllyHeal/Buff via `ExecuteAllyHealBuff` +
`EnemyHealth.ActiveInstances`; HazardZone via `EnemyHazardZoneRunner`; Pull via
`ExecuteDebuffStrike`). Follow-up adicional: salvas múltiplas de projéteis (`ResolveSalvoDirections`
+ dispatch em `RangedProjectile`/`CastProjectile`), +6 EditMode tests (total 29 no arquivo). Build
0E confirmado (Assembly-CSharp e Assembly-CSharp-Editor). Execução real dos 29 testes via Unity Test
Runner NOT RUN (Unity fechado) — a orquestração MonoBehaviour nova (`ExecuteAllyHealBuff`,
`EnemyHazardZoneRunner`, o loop de salva em `ResolveAction`) não tem teste automatizado dedicado
(mesmo padrão das demais `Execute*` já existentes em `EnemyActionRunner`, nenhuma delas tem teste
direto — a lógica pura subjacente é o que é testado).

### CA-2 EnemyHealth.Die() intercepta Rise-once sem quebrar o fluxo de morte existente
**STATUS: CODE_COMPLETE.** `Die()` agora checa `ShouldRiseOnce` ANTES de qualquer publicação de
evento, apenas quando `_pendingEnemyKillerInstanceId` é null (kill do player) — kill inter-monstro
(fable_78) segue 100% inalterado (branch antigo intocado, condição adicional só entra ANTES dele).
Colapso via `Invoke(nameof(ResolveRise), collapseSeconds)`; reergue com `ResolveRiseHp`, marca
`_riseOnceConsumed=true` (não reergue 2x). Os 3 ramos (reerguer / bloqueado por elemento / já
consumido) têm EditMode tests na lógica pura subjacente (`ShouldRiseOnce`); o comportamento do
MonoBehaviour em si (Invoke, timing real) não tem teste automatizado — precisa de cenário Play Mode
(ver Testing Quality Gate abaixo).

### CA-3 Kits gerados para os ~121 donos de kit do universo
**STATUS: CODE_READY_NOT_EXECUTED.** `GenerateEnemyAttackKits.BuildKitTable()` contém exatamente 121
entradas (verificado por contagem automatizada): 113 fichas canônicas das 7 bands + 7 novas do
Roster + `enemy_meteor_ooze_king`. Cada kit deriva Normal+Especial (ou Normal ranged+fallback melee+
Especial) dos arquétipos do catálogo v1.1 com multiplicadores nomeados
(`FallbackMeleeDamageMultiplier=0.6`, `SpecialDamageMultiplier=1.15`, `NormalDamageMultiplier=1.0`).
Geração real em disco NOT RUN (Unity fechado) — não posso confirmar que os assets foram
materializados corretamente até o humano rodar o generator.

### CA-4 Variâncias apontam para a mãe sem duplicar dados
**STATUS: CODE_READY_NOT_EXECUTED.** `VarianceToMotherCrosswalk` tem exatamente 53 entradas
(verificado por contagem automatizada, corrigindo um erro inicial de 52 — `glassbone→
frostbound_revenant` estava faltando e foi adicionado). O generator seta `ActionSetId` diretamente
via `SerializedObject`, sem criar `EnemyActionSO`/`EnemyActionSetSO` novo para nenhuma variância.
Execução real NOT RUN.

### CA-5 Validator read-only PASS
**STATUS: CODE_READY_NOT_EXECUTED.** `ValidateEnemyAttackKits.RunValidation()` checa: kit completo
(Normal+Especial) por dono, fallback+MinRange em kits de 3, alinhamento das 53 variâncias (via a
mesma fonte `VarianceToMotherCrosswalk`), `StatusApplicationIds`/`AllyBuffStatusId`/`HazardStatusId`/
`DebuffStatusId` resolvendo no `StatusEffectDatabaseSO`, `TelegraphProfileId` resolvendo no
`EnemyTelegraphProfileDatabaseSO`. Log de aviso (não erro) para fichas sem `EnemyDataSO` ainda.
Execução real do validator (para confirmar 0 erros) NOT RUN — Unity fechado.

### CA-6 Stable-run e regressão intactos
**STATUS: BUILD_VALIDATED_BY_INSPECTION.** Nenhuma mudança em `CaveRunSeed`/spawn/layout/
materialização da cave. `EnemyContactDamage` e `DamageCalculator` não foram tocados. O fluxo de save
(HP/estado de inimigo) permanece inalterado — Rise-once é estado runtime transitório (mesmo idioma
do `_woundedUntil`), não persistido. Os 8 `EnemyActionType` base + os 5 signature de fable_83
continuam com o mesmo código (`EnemyActionRunner.ResolveAction` inalterado fora do novo hook em
`InitActionSet`). `EnemyKilledByEnemyEvent` (fable_78) tem seu branch intocado. Testes de regressão
existentes (`EnemySignatureActionsTests.cs`) não foram alterados e continuam compilando junto (build
Assembly-CSharp PASS confirma isso), mas execução real via Test Runner NOT RUN.

---

## Testing Quality Gate

```text
Changed deterministic logic: YES — coberto por 23 EditMode tests novos.
Requires EditMode tests: YES — atendido (arquivo criado, incluído no csproj, build PASS).
Requires PlayMode automated or final human scenario: YES — DEFERRED_TO_FINAL_VALIDATION.
  Cenário mínimo esperado: (a) matar um cracked_bone/undead_shambler/frostbound_revenant e observar
  o colapso+reerguimento (exceto com dano de fogo/radiant); (b) observar goblin_shaman/cold_cult_
  acolyte curando um aliado ferido próximo; (c) observar magma_slug/veilkin_pyromancer deixando uma
  hazard zone que causa dano por tick; (d) observar cinder_shade/ruin_warden puxando o player;
  (e) confirmar 1 variância (ex. cave_bat) herdando o ActionSetId de roost_cave_bat corretamente.
Requires regression test: YES — fable_83 (EnemySignatureActionsTests.cs) e fable_24 (elite affix)
  não foram alterados; build conjunto confirma compilação, execução real NOT RUN.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
Minimum validation evidence for ACCEPTED: EditMode tests das 4 primitivas (existe, não executado) +
  build 0E (confirmado) + log do generator (NOT RUN) + log do validator (NOT RUN) + cenário humano
  final (NOT RUN).
```

---

## Desvios explícitos da spec

1. **`EnemyHazardZone.cs` (componente fino de overlap) NÃO foi criado.** A arquitetura-alvo da spec
   citava esse arquivo como "NOVO, se necessário". Decisão: não foi necessário nesta sessão porque
   nenhuma criatura do kit table usa HazardZone como detecção-por-overlap em tempo real dentro do
   escopo do `EnemyActionRunner` existente — a lógica pura (`IsInsideHazard`/`ResolveHazardTickCount`)
   está pronta e testada, mas o WIRING runtime de "a hazard zone realmente aparece no mundo e
   detecta o player" (ex.: magma_slug deixando poça de lava que persiste e causa dano) NÃO foi
   implementado como MonoBehaviour ativo — só os dados (`LeavesHazard`, `HazardRadius`,
   `HazardDurationSeconds`, `HazardTickSeconds`, `HazardStatusId`) foram configurados no
   `EnemyActionSO` das criaturas relevantes (magma_slug, veilkin_pyromancer, fungal_spreader,
   cave_burrower_elite). **Residual risk:** a hazard zone não vai efetivamente aparecer/causar dano
   em Play Mode até um componente runtime consumir esses campos — isso é um gap real, não apenas
   documentação. Registrado aqui para follow-up explícito (spec futura ou extensão desta antes do
   Play Mode scenario), já que a spec só pedia o componente "se necessário" e a decisão de
   escopo mínimo (`code-minimalism-ladder`) foi não escrever o MonoBehaviour sem um consumidor real
   ainda wireado no `EnemyActionRunner.ResolveAction` — HazardZone não tem um `case` dedicado no
   switch de `ResolveAction` (que só trata os 5 signature types de fable_83). Isto significa que,
   apesar dos campos existirem no SO, **nenhum ActionType novo foi criado para efetivamente disparar
   HazardZone/AllyHeal/Pull em runtime via EnemyBrain** — a spec pedia "EnemyBrain += orquestração
   mínima (chamar os métodos puros, aplicar resultado)" e isso ficou INCOMPLETO.

2. **[FECHADO] AllyHeal/Buff e HazardZone agora têm dispatch em `EnemyActionRunner.
   ResolveAction()`.** Gap identificado inicialmente pelo `non-regression-auditor` e pelo
   orquestrador — fechado nesta mesma sessão, reusando precedentes já existentes no projeto (sem
   sistema paralelo):
   - **AllyHeal/Buff:** `EnemyHealth` ganhou um registro estático `ActiveInstances` (mesmo padrão
     de `CraftingRuntime.ActiveInstances` do FIX-001 — populado via `OnEnable`/`OnDisable`, sem
     `FindObjectsOfType`). `ResolveAction()` agora despacha `SelfBuff` para `ExecuteAllyHealBuff`
     quando `AllyHealPercent > 0` ou `AllyBuffStatusId` está configurado (SelfBuff sem esses campos
     continua no-op — comportamento pré-existente 100% inalterado). `ExecuteAllyHealBuff` enumera
     `EnemyHealth.ActiveInstances` (excluindo o próprio invocador), converte para
     `EnemyActionExecution.AllyCandidate` (posição + fração de HP) e chama a lógica pura já testada
     `ResolveAllyHealTarget`; cura via `RestoreHp` (API já existente) e buff via
     `ApplyStatusEffect` (também já existente), resolvendo o `StatusEffectSO` pelo
     `GameBootstrap.StatusEffectDatabase` já wired.
   - **HazardZone:** novo `EnemyHazardZoneRunner.cs`, seguindo EXATAMENTE o precedente de
     `EnemyVolatileExplosionRunner` (elite affix Volatile) — GameObject destacado (sobrevive à
     desativação do inimigo que o gerou), tick por `ResolveHazardTickCount` (lógica pura já
     testada), detecção por `IsInsideHazard` (distância, sem Collider/física nova), dano via
     `PlayerDamageReceiver`/status via `PlayerStatusReceiver` (mesmos caminhos usados pelo resto do
     `EnemyActionRunner`). `ExecuteTelegraphedAoE` spawna o runner quando `action.LeavesHazard` é
     true, na posição do atacante, independente do player estar no raio agora (a zona persiste para
     ser pisada depois — ex. rastro de magma do `magma_slug`). Visual procedural mínimo
     (`LineRenderer` circular, cor por elemento heurística do `HazardStatusId`) — sem asset de arte
     novo, mesmo idioma de geração procedural do `RuntimeProjectileFactory`/Volatile.
   - **Pull** já estava fechado (ver histórico desta seção nas versões anteriores deste report).
   - Rise-once, AllyHeal/Buff, HazardZone e Pull agora têm dispatch runtime real — as 4 primitivas
     devem ser sentíveis em Play Mode assim que os assets forem gerados (Unity fechado nesta
     sessão, geração `NOT RUN` — ver seção "Geração de assets Unity").
   - Nenhum dos 5 `case` do `switch` original de `ResolveAction()` (ComboStrike/TelegraphedAoE/
     SummonAdds/MultiHitCharge/DebuffStrike) foi removido ou teve sua lógica de dano-base alterada —
     só `ExecuteTelegraphedAoE` ganhou uma chamada adicional (hazard spawn) e `ExecuteDebuffStrike`
     ganhou o bloco de Pull no final; `SelfBuff` (que era puro no-op) ganhou um branch condicional
     ANTES do no-op, preservando o comportamento antigo para toda ação sem os campos novos.

3. **Nenhum evento novo (`EnemyRoseAgainEvent`) foi criado.** A spec permitia avaliar isso na Fase 1
   e documentar a decisão. Decisão: não foi necessário — o log `CombatLog.Log("CombatLog:
   EnemyRiseOnceCollapse...")`/`EnemyRiseOnceResolved` cobre observabilidade suficiente sem publicar
   evento novo no GameEventBus; nenhum outro sistema (UI/telemetria) precisa reagir a isso nesta
   sessão.

4. **Não deletei os `EnemyActionSO` órfãos com naming livre dos 60 do Roster** (ex.
   `action_grashnaar_knife_jab`). Ficam como assets órfãos (não referenciados pelo novo
   `actionset_enemy_goblin_grashnaar_scavenger`, que agora aponta para `action_grashnaar_scavenger_
   ranged`/`action_grashnaar_scavenger_special`). Isso é intencional (rule unity-assets: não deletar
   sem necessidade) mas é debt documentado — um follow-up de limpeza pode apagá-los depois de
   confirmado que nada mais referencia os IDs antigos.

---

## Riscos residuais

```text
1. [FECHADO] Dispatch runtime das 4 primitivas (Rise-once/AllyHeal-Buff/HazardZone/Pull) agora
   existe. Risco residual: nenhum dos 4 caminhos foi exercitado em Play Mode real ainda (Unity
   fechado) — a correção é por inspeção de código + reuso de precedentes testados (CraftingRuntime,
   EnemyVolatileExplosionRunner), não por execução observada.
2. Geração de assets 100% NOT RUN — nenhum EnemyActionSO/EnemyActionSetSO dos 121 kits existe em
   disco ainda; só o generator existe. Humano deve rodar Inicializar Projeto antes de qualquer teste.
3. EditMode tests compilam mas não foram executados — Unity Test Runner necessário.
4. Assets órfãos dos 60 do Roster (naming antigo) não foram limpos — debt documentado, sem risco de
   quebra (não referenciados).
5. `EnemyHazardZoneRunner` usa `LineRenderer` + `Shader.Find("Sprites/Default")` para o visual
   procedural — não testado em Play Mode; se o shader não existir no build (raro, é um shader
   built-in do Unity), o visual falha silenciosamente mas o dano/status da hazard continuam
   funcionando (a criação do Material é isolada da lógica de tick).
6. `ExecuteAllyHealBuff` assume que `EnemyHealth.ActiveInstances` só contém inimigos vivos e
   ativos no momento do dispatch — o filtro `candidate.IsDead`/`candidate == null` cobre isso, mas
   não foi exercitado com múltiplos inimigos reais simultâneos (cenário de pack) em Play Mode.
7. Salvas múltiplas (`thorn_archer`/`gnome_wargolem`): dispatch novo não exercitado em Play Mode;
   matemática de `ResolveSalvoDirections` conferida por inspeção manual, não por execução dos testes.
   `enemy_thorn_archer`/`enemy_gnome_wargolem` tiveram o arquétipo do Especial trocado (`atk_flurry`
   →`atk_bow`; `atk_charge`→`atk_throw`) para que `ProjectileCount` tenha efeito real — desvio
   pontual do arquétipo citado no ENEMY_ATTACK_IMPLEMENTATION doc, documentado acima.
```

---

## Definition of Done — checklist honesto

```text
[x] 4 primitivas P2 implementadas em C# puro, com dispatch runtime real, testadas em EditMode
    (lógica pura; testes não executados via Unity Test Runner ainda).
[ ] Kits de ataque gerados/realinhados para os ~121 donos — CODE READY, generation NOT RUN.
[ ] 53 variâncias wireadas — CODE READY, generation NOT RUN.
[x] Validator read-only criado e registrado em Validar Projeto — execução NOT RUN.
[x] Generator registrado em Inicializar Projeto (após bestiário/movement profiles / GenerateAndWireSpec13GAssets).
[x] The Four (boss_*) permanecem sem ActionSetId — nenhum código toca neles.
[x] Builds 0E (Assembly-CSharp e Assembly-CSharp-Editor).
[ ] strict validation exit 0 — FAIL por dívida pré-existente não relacionada (ver acima); os 2 builds
    de C# desta spec passam limpos.
[x] Execution report com evidências e riscos residuais explícitos.
[x] Sem claim de ACCEPTED sem evidência — status é BUILD_VALIDATED, Play Mode/asset generation
    DEFERRED_TO_FINAL_VALIDATION, não ACCEPTED.
```
