# Harness — catálogo sob demanda

Fonte canônica de descoberta: este índice. Leia apenas a seção e o artefato correspondentes ao trabalho; links não exigem carregamento recursivo.
Skills: `skills/<id>/SKILL.md`; commands: `commands/<id>.md`; agents: `agents/<id>.md`.
No Codex, os mesmos IDs ficam em `.agents/skills/` e `.codex/agents/`; commands também são skills.
## Commands

| Command | When to use |
|---------|-------------|
| `/start-spec` | Plan a spec (no code) |
| `/audit-spec` | Phase 0 only — map what exists, risks, delta |
| `/implement-spec` | Deprecated; usar `/execute-spec-strict` |
| `/validate-spec` | Run validations after implementation |
| `/finish-spec` | Closeout — phase-aware, no auto-promote |
| `/reconcile-status` | Audit inconsistencies (may read PROJECT_LOG) |
| `/plan-wave` | Plan next FASE/wave (reads ROADMAP) |
| `/bugfix` | Fix a specific bug |
| `/docs-health` | Run docs validation only |
| `/review-non-regression` | Audit diff for violations |
| `/execute-spec-strict` | Strict single-spec execution with validation gates |
| `/loop-spec-batch-strict` | Batch execution loop (use fable/ queue) |
| `/resolve-spec-dependency-chain` | Resolve same-batch spec dependencies |
| `/run-editmode-tests` | Rodar EditMode tests (Unity Test Runner) |
| `/validate-unity` | Unity compile/log validation only |
| `/audit-harness` | Audita o harness (.claude/) — skills/agents ausentes do índice, refs obsoletas, tamanho, qualidade |

---

## Skills

Use `.claude/skills/<name>/SKILL.md` when task matches:

| Skill | When |
|-------|------|
| `refinement-authoring` | Refine ideas through evidence, world fit, alternatives and balance before specifying |
| `spec-planning` | Technical plan for an existing spec: contracts, per-file edits, ownership and validation |
| `spec-task-authoring` | Traceable tasks from spec and plan; cross-stage consistency and readiness review |
| `pixel-art-animator` | Author animation poses, durations, tags and linked cels through Aseprite; adapted upstream with local consumer contracts |
| `spec-authoring` | Author executable specs proportionate to risk; short maintenance format or detailed blueprint for complex integration |
| `spec-execution` | Implementing any spec |
| `gameplay-test-scenario` | Creating human test plan for runtime/gameplay specs |
| `editmode-test-authoring` | EditMode unit tests para lógica determinística (save DTOs, fórmulas, quests, economy, catálogos) |
| `docs-governance` | Organizing/archiving docs |
| `unity-validation` | Select applicable compile, test and asset gates; reuse verified evidence and report limitations |
| `unity-mcp-operations` | Operate an available Unity MCP: instance identity, mutation ownership, reload and uncertain-result recovery |
| `unity-performance-profiling` | Measure reproducible Unity performance; distinguish static hypotheses from Player/Editor profiling evidence |
| `unity-validation-triage` | Classifying Unity/dotnet/log validation failures |
| `unity-asset-generation` | Running Unity editor asset generators with evidence |
| `editor-validator-authoring` | Criar/estender validators de editor para integridade de conteúdo (IDs duplicados, refs pendentes, ranges) |
| `bootstrap-wiring` | GameBootstrap / manager wiring |
| `runtime-bootstrap-pattern` | Manutenção do legado *RuntimeBootstrap; composição nova segue composition root/installers e a spec, sem global searches |
| `combat-data-wiring` | Weapon/Spell/StatusEffect databases |
| `enemy-ai-authoring` | Comportamento de inimigos — AI states, aggro, patrol, attack patterns, wiring na scene |
| `equipment-durability-repair` | Durabilidade, degradação e reparo de equipamento |
| `loot-table-authoring` | Drop tables, loot pools, raridade, wiring de loot em inimigos/baús |
| `ui-modal-stack` | ModalManager / input blocking |
| `ui-projection-pattern` | Padrão ViewModel/projection de UI — binding de dados para HUD/painéis; quando lógica e View são separadas |
| `hud-canvas-binding` | Bind or repair the existing gameplay Canvas; verify layout, localized text, event actions and modal focus |
| `save-load-pattern` | Save/load data |
| `save-section-provider` | Adicionar nova seção ao save file (ISaveSection provider pattern) |
| `event-bus-pattern` | GameEventBus communication |
| `event-catalog-and-tracing` | Governança/observabilidade do GameEventBus — catálogo de eventos, checar duplicação antes de criar, tracer dev-only; ao criar/alterar evento em Core/Events |
| `non-regression-review` | Read-only diff review for architecture, scope, save, lifecycle and gameplay regressions before closeout |
| `system-reuse-audit` | Auditar se sistema existente cobre a necessidade antes de criar novo; qualquer spec que mencione "criar serviço/manager/sistema" |
| `cave-stable-run-guard` | Any cave procedural change |
| `wave-integration-slice` | Any WAVE_INTEGRATION spec (scene binding, economy loop, HUD, NPC, skill effects, movement actions) |
| `player-ability-runtime` | Adding any non-slot player ability (Dash, Dodge, Block, roll, blink, sprint) |
| `npc-dialogue-authoring` | Creating or extending NPCs with dialogue, shop, or scene placement |
| `scene-interactable-wiring` | Adding IInteractable objects (crops, resources, fishing, chests) to scenes |
| `tilemap-world-rendering` | 2D world Tilemap, Rule Tiles, sorting, collision and pixel-art import; ground seams or stretched structures |
| `crop-farming-systems` | Crop, soil, watering, fertilizer and harvest behavior driven by canonical day transitions |
| `quest-authoring` | Definição de quest, objetivos, condições, triggers, recompensas, save de estado |
| `time-calendar-weather` | Canonical clock, day, season, year, weather, forecast and lunar transitions |
| `inventory-transactions` | Atomic stack, capacity, split, move, merge and transfer changes with event/save consistency |
| `decision-rule-extraction` | Creating/changing ADRs and game_rules from specs/refinements |
| `docs-migration` | Moving specs/refinements to implementados/ with evidence |
| `implementation-closeout` | Final checklist for closing any relevant task |
| `state-machine-design` | Designing an FSM (player/enemy/boss/UI flow) when boolean flags start conflicting |
| `ability-effect-composition` | Designing how abilities/spells/status effects compose from reusable effects |
| `monobehaviour-decomposition` | Splitting a god-MonoBehaviour into thin adapter + pure-C# core |
| `solid-refactoring` | Refactor cohesive responsibilities, dependencies and SOLID contracts while preserving behavior |
| `object-pooling-pattern` | Pooling de objetos para spawn/despawn frequente (projéteis, drops, floating text, SFX) |
| `rng-and-determinism` | RNG com seed para conteúdo determinístico; quando não usar UnityEngine.Random |
| `progression-curve-design` | Curvas de XP, thresholds de level, scaling de stat; design de progressão |
| `economy-balance-tuning` | Preços, sinks/sources de gold, balance de shop; specs de tuning de economy |
| `audio-event-wiring` | Wiring SFX/music via GameEventBus (SfxEventBridge/SfxEventMap); any spec needing sound feedback |
| `localization-authoring` | Add or migrate player-facing text through existing localization keys; preserve system IDs and visible fallback |
| `crafting-recipe-authoring` | Adding recipes, unlock/gate, processing jobs, save of in-progress jobs (Craft/ vs Crafting/ alert) |
| `skill-tree-authoring` | Skill tree nodes, purchase/respec, active slot wiring, skill save/load |
| `player-needs-survival` | Stamina/hunger/fatigue consumption, regen, thresholds, sleep, eat, save of needs |
| `input-gamepad-routing` | Input focus routing (modal guard), new key bindings, gamepad path (future) |
| `registry-catalog-pattern` | Static catalog/registry com const IDs, TryGet, IReadOnlyList All; acessar dados de catálogos via C# puro |
| `data-catalog-authoring` | Criação em bulk de *DataSO com editor generators, validators e evidência; specs de conteúdo em massa (itens, inimigos, skills) |
| `action-feedback-pipeline` | Publicar PlayerActionFeedbackEvent → HUD toast + SFX automático; qualquer recusa de ação ou feedback de gameplay |
| `fail-state-recovery-design` | Design de fail states — perda, respawn, recovery, anti-softlock; specs de morte/KO/colapso ou qualquer consequência de fracasso |
| `loop-hierarchy-design` | Hierarquia Farm/Town/Cave — perfil de risco, fluxo de recursos, classificação de features; specs que adicionam feature de gameplay ou economy |
| `game-feel-checklist` | Feedback sensorial de ação — recusas expostas, juice (shake/flash), audio, toast; specs que adicionam ação ou mudam feel de gameplay |
| `observability-and-logging` | Logging diagnóstico em runtime — one-shot guards p/ log spam, formato de wiring-error, prefixo por sistema, debug overlay; ao adicionar log de erro/aviso ou investigar log repetido |
| `editor-tooling-orchestration` | Orquestrador 1-clique de geradores/cenas (best-effort, log por passo) + higiene de menu CindarsHope/Archive; ao adicionar MenuItem ou consolidar setup multi-passo |
| `boot-integration-smoke` | Verificação estática/EditMode do boot-wiring (GameBootstrap, *RuntimeBootstrap, subscribers) antes de depender de Play Mode; specs runtime que fechariam só em "Play Mode deferred" |
| `harness-authoring` | Create or update skills, rules, agents, commands or hooks using conditional references and reusable templates |
| `harness-audit` | Auditar artefatos do harness após waves grandes ou quando skill/agent parece não estar disparando |
| `delegated-execution` | Delegate a slice with ownership; inspect current artifacts and evidence without redundant rebuilds |
| `pixel-art-prompt-authoring` | Write English pixel-art prompts from approved direction and references; correct visual drift using the available pipeline |
| `sprite-generation-pipeline` | OPERAR o pipeline de geração de sprites por IA (ComfyUI/SDXL local AMD): build de prompts, geração em lote resumível, pós (rembg+downscale), wiring no Unity; gerar/regenerar arte de inimigos/NPCs/props/player |
| `chatgpt-web-sprite-gen` | Gerar sprites de MUNDO (peças modulares, props, tiles, vegetação, landmarks) pela web do ChatGPT via Chrome MCP, com ferramentas disponíveis; ângulo 3/4 top-down por componente e recuperação limitada em rate limit. Usar quando o local não dá conta de peça modular isolada |
| `npc-walk-animation` | Gerar, AUDITAR (frames realmente animam a passada) e wire folhas de caminhada 5x5 dos NPCs; prompt de keyframes, montagem de auditoria, e o sistema NpcWalkAnimator + slicer (Inicializar Projeto). Usar ao criar/regerar/auditar walk sheets ou ligar a folha ao NPC |
| `pixel-art-direction` | Define visual direction, native scale and palette before generating art |
| `aseprite-authoring` | Edit existing pixel art as layered Aseprite candidates through CLI/Lua while preserving source, animation and import contracts |
| `visual-asset-review` | Review static sprites and actual transparency against an approved reference |
| `sprite-animation-review` | Review contacts, gait, timing and frame continuity |
| `sprite-scene-integration` | Integrate approved sprites; check import, scale, sorting and collision |

## Agents

Delegate via `.claude/agents/`:

| Agent | Role |
|-------|------|
| `spec-implementer` | Code specs |
| `docs-curator` | Document governance |
| `unity-validator` | Validation only |
| `non-regression-auditor` | Regression audit |
| `architecture-reviewer` | Pre-wave architecture |
| `bugfix-investigator` | Bug investigation |
| `asset-wiring-specialist` | Unity data / prefab wiring |
| `game-design-reviewer` | Design review de specs de gameplay antes da implementação (loops, economy, pacing, failure states) |
| `performance-auditor` | Read-only performance review; separate static risks from measured CPU/GPU/UI/memory evidence |
| `test-author` | Suíte de testes independente, extensa ou especializada; testes pequenos podem ficar com implementador |
| `pixel-art-scene-reviewer` | Independent visual review of assets, animation and scene assembly when actual evidence is accessible |

---


## Rules
- [Bounded visual iteration](rules/visual-iteration-budget.md): scene/reference revisions; show evidence, select a small round, reuse pertinent gates and stop low-return retries.

- [Task model routing](rules/task-model-routing.md): before execution/delegation, select an economical model by slice risk, apply supported overrides and escalate without weakening gates.

[Invariantes canônicas](rules/RULES.md). Leia as rules aplicáveis ao escopo; cópias Codex em `.codex/rules/`.
