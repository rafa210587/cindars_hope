# Execution Report — Gameplay Expansion Slice (2026-06-12)

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]

Status: **BUILD_VALIDATED_WITH_HUMAN_UNITY_ACTION_REQUIRED**

Escopo executado por solicitação humana direta (não é spec de registry): expansão de gameplay
cobrindo projéteis, comportamentos de inimigos, densidade da caverna, mercador errante,
skills ativas reais, TownScene (praça/estátua/casas/mercados), diálogos expandidos,
FarmScene (campos de plantio + entrada da caverna).

---

## Acceptance criteria extracted

| # | Critério | Status | Evidência |
|---|----------|--------|-----------|
| 1 | Projéteis visíveis/animados para magias e arco | OK | `RuntimeProjectileFactory`, `ProjectileVisualAnimator`, fallback em `ProjectileSpawnService`; bloqueios `BowHasNoProjectilePrefab`/`SpellHasNoProjectilePrefab` removidos |
| 2 | Mais inimigos na caverna | OK | `CaveEnemySpawnPlanner`: min 14→16, max 24→32, escala por profundidade até cap 44 |
| 3 | Comportamentos variados de inimigos | OK | `EnemyBrain`: Leaper, PhaseShortBlink, BurrowAmbush, Retreat (low-HP de Swarm/Ranged/Caster), GuardHold com retorno ao posto, patrulha ancorada no spawn; ações Ranged/Cast agora disparam `EnemyProjectileBehaviour` esquivável; melee re-checa range pós-windup |
| 4 | Mercador errante na caverna com aparição aleatória | OK | `CaveWanderingMerchant`: chance 22%/nível derivada de `CaveWorldSeed+CaveRunSeed+CaveLevel+salt` (estável por run, ADR-0005); stock determinístico; trade via `BuyItemPoint`/`SellAllPoint` (eventos de economia existentes) |
| 5 | Skills conectadas a ações reais | OK | 21 efeitos reais registrados (7 melee, 9 projéteis, 1 gadget, 3 restauros); 9 permanecem feedback-only com motivo documentado; cooldown por skill via `SkillEffectResult.CooldownSeconds` |
| 6 | TownScene: NPCs espalhados, casas, árvores, praças, estátua, mercados | OK (gerador) | `CreateMvpTownScene`: 23 NPCs em distritos (±16x±12), praça central com estátua do guerreiro (espada bastarda + escudo), 12 casas, 24 árvores, barraca de mercado por NPC vendedor, bounds de wander por NPC |
| 7 | Diálogos melhorados | OK (código) | `TownNpcDialogueLibrary`: 23 NPCs × 13 nós com personalidade; menu editor `RebuildTownNpcDialogues`; NPCs de loja ganharam opção "Conversar" que percorre a árvore |
| 8 | Farm: distribuição, áreas de plantio, entrada da caverna | OK (gerador) | `CreateMvpFarmScene`: 24 canteiros (2 campos), Zone_CropField ampliada, `CaveEntranceInteractable` real substituindo portal legado |

## Existing systems audit

Reutilizados sem reescrita: GameEventBus, ProjectileSpawnService/ProjectileBehaviour,
EnemyHealth/KnockbackController/HitFlashController/EnemyTelegraphController, EnemyBrain
(estendido), CaveEnemySpawnPlanner (estendido), CaveRunManager, BuyItemPoint/SellAllPoint,
EconomyManager (via eventos), SkillEffectRegistry/ActiveSkillExecutionController,
NpcController/NpcShopController/DialogueModal, ModalManager, SceneTransitionRouter,
CaveEntranceInteractable (WAVE16), geradores de cena.

Novos arquivos runtime (9): ProjectileVisualStyle, ProjectileVisualAnimator,
RuntimeProjectileFactory, EnemyProjectileBehaviour, CaveWanderingMerchant,
MeleeStrikeSkillEffectExecutor, ProjectileSkillEffectExecutor, SelfRestoreSkillEffectExecutor,
TownNpcDialogueLibrary. Novo editor (1): RebuildTownNpcDialogues. Testes (3 arquivos, 18 testes).

Nenhum sistema paralelo criado; nenhum `GameObject.Find` em runtime; comunicação por
GameEventBus; nenhum DTO de save alterado.

## Spec Compliance Matrix

| Requisito (pedido humano) | Implementação | Status |
|---------------------------|---------------|--------|
| Projéteis animados (arco/magia) | RuntimeProjectileFactory + ProjectileVisualAnimator + fallback no spawn service | OK |
| Mais inimigos por nível | CaveEnemySpawnPlanner min/max 16-32 + escala de profundidade (cap 44) | OK |
| Comportamentos variados | EnemyBrain: leap, blink, burrow, retreat, guard-return, patrol ancorado, projéteis inimigos | OK |
| Mercador errante aleatório | CaveWanderingMerchant determinístico por seed (22%/nível) | OK |
| Skills com ações reais | 21 executores reais + cooldown por skill | OK |
| TownScene distritos/praça/estátua/casas/mercados | CreateMvpTownScene v2 | OK (gerador; requer regeneração humana) |
| Diálogos expandidos | TownNpcDialogueLibrary 23×13 + RebuildTownNpcDialogues + "Conversar" em NPCs de loja | OK (assets requerem menu humano) |
| Farm plots + entrada caverna | 24 canteiros + CaveEntranceInteractable | OK (gerador; requer regeneração humana) |

## Cave stable-run compliance (ADR-0005 / cave_rules.md)

- Aparição/posição/estoque do mercador derivam de `StableHash(worldSeed|runSeed|level|salt)`.
- Sem GUID/timestamp/UnityEngine.Random em conteúdo persistente de nível.
- Densidade de inimigos continua derivada do level seed determinístico do planner.
- Randomness residual é apenas visual/transiente (fase de pulso de projétil, jitter de patrulha).

## Validation

```
Validation method: run_strict_validation.ps1
Exit code (builds): 0
Assembly-CSharp: PASS (0 erros, 0 warnings)
Assembly-CSharp-Editor: PASS (0 erros, 0 warnings no strict run)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (erros pré-existentes em specs/reports legados;
  nenhum erro novo introduzido por esta entrega)
Quality/diff gate: execution report presente (este arquivo); testes presentes
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (densidade de spawn, mercador errante, diálogo builder)
Changed Unity scene/prefab/asset wiring: NO (somente geradores editor; nenhuma edição YAML)
Automated tests added/updated: YES (18 testes EditMode em 3 arquivos:
  CaveWanderingMerchantTests, CaveEnemySpawnDensityTests, TownNpcDialogueLibraryTests)
Automated tests command: NOT RUN locally (Unity Test Runner indisponível na sessão;
  testes compilam no Assembly-CSharp com 0E/0W)
Manual Play Mode scenario: docs/validation/playmode/gameplay_expansion_2026_06_12_human_test_scenario.md
Justification if no automated tests: N/A (testes criados para a lógica determinística)
Residual risk: comportamento físico/visual (leap, blink, burrow, projéteis, layout de cena)
  não validado em Play Mode; ver cenário humano
```

## Human Unity actions required (antes do Play Mode)

1. `CindarsHope/Create Scenes/Town Scene` — regenera TownScene (distritos, praça, estátua, casas, mercados).
2. `CindarsHope/Create Scenes/Farm Scene` — regenera FarmScene (24 canteiros, entrada da caverna).
3. `CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)` — grava as 23 árvores de diálogo expandidas nos assets.
4. Executar o cenário humano (item 5) em Play Mode.

## Honest status rationale

BUILD_VALIDATED apenas: builds passam e a lógica determinística tem testes EditMode
compilando, mas (a) Unity batchmode/Test Runner não foi executado nesta sessão, (b) as
cenas regeneradas e os assets de diálogo exigem ação humana no Editor, (c) nenhuma
validação Play Mode foi executada. Nenhuma claim de PLAYMODE/ACCEPTED é feita.

## Remaining work / debts

- Efeitos feedback-only restantes: marked_prey, elemental_ward, slowing_sigils,
  sinal_retirada, isca_improvisada, irrigador_portatil, mecanismo_campo, marca_eficiencia,
  melee.block (bloqueio já existe como ação de movimento Left Shift).
- Status effects (bleed/slow) não anexados às skills por falta de IDs confirmados no
  StatusEffectDatabase (hook pronto via `statusEffectId`).
- `.meta` dos novos arquivos serão gerados pelo Unity na primeira abertura.
- CaveScene legada (`CreateMvpCaveScene`) mantém spawner simples para o smoke test;
  o caminho real de materialização (planner) é o beneficiado pelas mudanças.
