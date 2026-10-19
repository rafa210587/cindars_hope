---
name: enemy-ai-authoring
description: Cria enemy AI behaviors, boss phases, pack coordination e enemy moves usando o pattern vivo de EnemyBrain + action/telegraph database. Use para fable_04 (threat/pack coordination), fable_05 (cave boss phase AI), fable_24 (enemy moves/elite affixes) ou qualquer mudança de enemy behavior.
---

# Skill: Autoria de Enemy AI

## O sistema VIVO (use este)

`EnemyBrain` (`Assets/_Game/Scripts/Enemy/EnemyBrain.cs`) é o brain de runtime:

- **State machine informal**: enum `EnemyBrainState` (Idle/patrol/chase/retreat...), decisões num tick (`_decisionTickSeconds` = 0.3s default), NÃO por frame.
- **Actions data-driven**: `EnemyActionSetDatabaseSO` → `EnemyActionSetSO` → `EnemyActionSO` (SPEC 13D), com cooldowns por action rastreados em `EnemyActionRuntime`.
- **Telegraphs**: `EnemyTelegraphProfileDatabaseSO` + `EnemyTelegraphController` — ataques se anunciam antes de acertar.
- **Profiles**: `EnemyDataSO` (stats), `EnemyMovementProfileSO`, `EnemyVulnerabilityProfileSO` (14A-FIX4).
- **Packs/spawn**: `EnemySpawnPackSO`, `EnemySpawnProfileSO`, `EnemySpawnResolver` (ecology), `EnemyRoomSizeClass`.
- Executores de movimento: `EnemyChaseController` / `EnemyPatrolController` (Combat/), contact damage à parte.

> **RESOLVED 2026-06-12:** o órfão `AIBehaviorSO` (+ o dead field `EnemyDataSO.aiBehaviorId` e 3 assets gerados) foi RETIRED — fable_04/05/24 proíbem explicitamente estruturas de AI paralelas e constroem apenas sobre EnemyBrain (fable_05 define seu próprio `BossPhaseProfileSO`). Se você encontrar restos de `aiBehaviorId` em YAML `.asset` antigo, o Unity os ignora; não recrie o field.

## Regras para novos behaviors

1. **Novo behavior = data primeiro.** Prefira um novo `EnemyActionSO` + entry num action set a novos branches hardcoded no EnemyBrain. Valores de tuning vivem em SOs/profiles, nunca como magic numbers (tuning serializado estilo `_leapCooldownSeconds` é o idiom existente para params de nível brain).
2. **Toda damaging action precisa de um telegraph** (profile no telegraph database) — um windup que o player consegue ler. Sem telegraph = NEEDS_REWORK por fairness de combat.
3. **Decisões no tick, reações em events.** Não adicione logic por frame no brain; o decision tick de 0.3s é o budget. Physics fica nos movement controllers.
4. **Boss phases (fable_05)**: modele cada phase como um state explícito com entry conditions em health thresholds (ex.: 100/60/30%), transitions de via única (sem regressão de phase a menos que a spec diga), swap de action set por phase (`_activeActionSet`) e um momento telegrafado de phase-transition (janela de invulnerabilidade + visual cue). Persista a phase atual no cave snapshot se o boss pode ser deixado no meio da luta (rule: cave-stable-run).
5. **Pack coordination (fable_04)**: coordenação via dados determinísticos compartilhados (roles de spawn-pack, flank side seeded — a alternância existente `s_nextBlinkFlankSide` é o precedente), NÃO via enemies procurando uns aos outros na scene (rule: unity-architecture). A composição do pack vem seeded do spawn resolver (rule: cave-stable-run — mesmo level, mesmo pack).
6. **Determinism**: qualquer escolha random que afete um cave level saved/revisitado usa o pattern de seeded RNG (skill: rng-and-determinism). Visual jitter pode usar UnityEngine.Random.

## Testes

EnemyBrain é um MonoBehaviour — extraia as RULES de decisão para C# puro (ex.: um phase-threshold resolver, action-eligibility evaluator) para que a logic de fable_04/05 ganhe EditMode tests (skill: editmode-test-authoring): entry de phase nos thresholds exatos, sem regressão de phase, gating de action cooldown, determinismo da atribuição de pack role. Behavior vivo de chase/feel vai para um human Play Mode scenario (skill: gameplay-test-scenario).

## Fechamento

- Cobertura de validator para novas entries de action/telegraph (skill: editor-validator-authoring): toda action num set existe no action database; toda action com damage tem um telegraph profile.
- Evidência de asset generation para novos SO assets (rule: unity-assets).
