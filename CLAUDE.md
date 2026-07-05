# CLAUDE.md — Cindar's Hope

> Short router for Claude Code. Full rules in `AGENTS.md` and `.claude/rules/`.

---

## Project Identity

2D pixel art RPG + farm sim in Unity LTS/C#. World: Vaalara / Cindar's Hope / Dornecia.
Specs live in `.specs/`. Spec is the execution contract. Refinement is fallback only.

---

## Execution Routing (LER PRIMEIRO — comportamento padrão de toda sessão)

O loop principal (Opus, **sempre a última versão**) é reservado para **pensar, debater, planejar e propor** — design, refinamento e decisões. Para **qualquer trabalho de execução** (construir, editar, fazer, validar), NÃO execute no loop principal: delegue imediatamente a um subagent Sonnet (**última versão**) via Agent tool.

> Use sempre os aliases de modelo (`opus` / `sonnet` / `haiku`), que já resolvem para a versão mais nova. Nunca fixe um número de versão no `model:`.

- **Delegar a Sonnet** (subagent_type específico quando existir — já em Sonnet; senão `general-purpose` com `model: sonnet`): implementar/editar código, asset wiring, rodar validação, docs migration, EditMode tests, edição de catálogo/asset em massa, bugfix mecânico, buscas amplas no repo.
- **Modelo por tipo de subagent** (passe `model` no Agent tool): busca/exploração read-only (`Explore`) → `haiku`; pesquisa/execução genérica (`general-purpose`) → `sonnet`; agents de julgamento (`architecture-reviewer`, `game-design-reviewer`) → herdam Opus. Os demais agents do projeto já têm `model` no frontmatter.
- **Manter no loop principal (Opus):** responder perguntas, ler 1–2 arquivos para a conversa, planejar, decidir arquitetura/design, propor, explicar.
- **Regra prática:** pensar/debater/planejar/propor → loop principal (Opus). Qualquer execução — múltiplas edições/iterações, construção ou validação → subagent Sonnet.
- Ao delegar, dê ao subagent o escopo, arquivos permitidos e critério de pronto; ao voltar, revise o resultado no Opus.

Detalhes e racional de custo: `docs/project/COST_EFFICIENCY_GUIDE.md`.

## Default Reads (implementation task)

Read ONLY:

1. `CLAUDE.md` (this file)
2. `docs/project/CURRENT_STATE.md` — active queue, blockers, key paths (~80 lines)
3. The active spec (`.specs/a_implementar/spec_*.md`)
4. Files explicitly listed in the spec scope

## Spec Planning / Generation Reads

For spec planning, wave planning, or spec generation tasks, also read:

1. `docs/design/SPEC_SOURCE_MAP.md` — domain mapping and direction sources
2. `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` — SpecKit format and structure
3. `.specs/SPEC_GENERATION_ROADMAP_MASTER.md` — macro roadmap of planned specs
4. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution phases, taxonomy, governance
5. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation requirements by change type
6. `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — list of specs to create

## Spec Execution Reads (runtime/code changes)

For spec execution (implement-spec, validate-spec, finish-spec), also read:

1. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — phases and promotion rules
2. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation levels and evidence requirements
3. `.claude/rules/testing-quality-gate.md` — automated testing and human validation requirements
4. `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` — human validation checklist (batch at wave-end)

## Cost Efficiency

Para reduzir gasto de tokens (roteamento de modelo, cache de prompt, script vs reasoning): `docs/project/COST_EFFICIENCY_GUIDE.md`. Agents mecânicos rodam em Sonnet; use Opus no loop principal só para design/decisão.

## Conditional Reads (only if spec or prompt cites them)

- A specific refinement
- The immediately prior validation report listed as a dependency
- A specific architecture document needed by the spec
- A specific backlog item

## Do NOT Read By Default

```
PROJECT_LOG.md         — audit / reconciliation / regression only
ROADMAP.md             — planning new waves / creating specs only
docs/IMPLEMENTATION_STATUS.md  — audit only; use CURRENT_STATE.md instead
AGENTS.md (full)       — rules apply without reading whole file; stop conditions are enforced
GDD complete           — never by default
all refinements        — conditional only
archived/superseded specs
docs_old/**
unrelated validation reports
```

---

## Commands

| Command | When to use |
|---------|-------------|
| `/start-spec` | Plan a spec (no code) |
| `/audit-spec` | Phase 0 only — map what exists, risks, delta |
| `/implement-spec` | Execute a spec |
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
| `spec-execution` | Implementing any spec |
| `gameplay-test-scenario` | Creating human test plan for runtime/gameplay specs |
| `editmode-test-authoring` | EditMode unit tests para lógica determinística (save DTOs, fórmulas, quests, economy, catálogos) |
| `docs-governance` | Organizing/archiving docs |
| `unity-validation` | Compile + validator flow |
| `unity-validation-triage` | Classifying Unity/dotnet/log validation failures |
| `unity-asset-generation` | Running Unity editor asset generators with evidence |
| `editor-validator-authoring` | Criar/estender validators de editor para integridade de conteúdo (IDs duplicados, refs pendentes, ranges) |
| `bootstrap-wiring` | GameBootstrap / manager wiring |
| `runtime-bootstrap-pattern` | Padrão *RuntimeBootstrap de self-wiring em scene; sistemas com init scene-aware sem FindObjectOfType |
| `combat-data-wiring` | Weapon/Spell/StatusEffect databases |
| `enemy-ai-authoring` | Comportamento de inimigos — AI states, aggro, patrol, attack patterns, wiring na scene |
| `equipment-durability-repair` | Durabilidade, degradação e reparo de equipamento |
| `loot-table-authoring` | Drop tables, loot pools, raridade, wiring de loot em inimigos/baús |
| `ui-modal-stack` | ModalManager / input blocking |
| `ui-projection-pattern` | Padrão ViewModel/projection de UI — binding de dados para HUD/painéis; quando lógica e View são separadas |
| `hud-canvas-binding` | Metade de cena/Canvas do padrão de UI — montar hierarquia de Canvas e plugar a View no GameplayHudCanvas; fecha o "DEFERRED: binding do canvas" das specs de UI |
| `save-load-pattern` | Save/load data |
| `save-section-provider` | Adicionar nova seção ao save file (ISaveSection provider pattern) |
| `event-bus-pattern` | GameEventBus communication |
| `event-catalog-and-tracing` | Governança/observabilidade do GameEventBus — catálogo de eventos, checar duplicação antes de criar, tracer dev-only; ao criar/alterar evento em Core/Events |
| `non-regression-review` | Pre-closeout audit |
| `system-reuse-audit` | Auditar se sistema existente cobre a necessidade antes de criar novo; qualquer spec que mencione "criar serviço/manager/sistema" |
| `cave-stable-run-guard` | Any cave procedural change |
| `wave-integration-slice` | Any WAVE_INTEGRATION spec (scene binding, economy loop, HUD, NPC, skill effects, movement actions) |
| `player-ability-runtime` | Adding any non-slot player ability (Dash, Dodge, Block, roll, blink, sprint) |
| `npc-dialogue-authoring` | Creating or extending NPCs with dialogue, shop, or scene placement |
| `scene-interactable-wiring` | Adding IInteractable objects (crops, resources, fishing, chests) to scenes |
| `tilemap-world-rendering` | Renderizar o mundo 2D top-down certo — Tilemap+Rule Tiles no chão, casas como prefab com volume, Y-sort, import pixel-art, colisão CompositeCollider2D; ao ver grade/seam no chão ou o erro "Cannot generate 9 slice" |
| `crop-farming-systems` | Farm tile, ciclo de cultivo (plantar/regar/colher), estado do solo, yield sazonal |
| `quest-authoring` | Definição de quest, objetivos, condições, triggers, recompensas, save de estado |
| `time-calendar-weather` | Ciclo dia/estação/lunar, geração de clima, eventos de calendário |
| `inventory-transactions` | Add/remove/stack/split/move de items; capacidade de inventário; transações de slot de equipamento |
| `decision-rule-extraction` | Creating/changing ADRs and game_rules from specs/refinements |
| `docs-migration` | Moving specs/refinements to implementados/ with evidence |
| `implementation-closeout` | Final checklist for closing any relevant task |
| `state-machine-design` | Designing an FSM (player/enemy/boss/UI flow) when boolean flags start conflicting |
| `ability-effect-composition` | Designing how abilities/spells/status effects compose from reusable effects |
| `monobehaviour-decomposition` | Splitting a god-MonoBehaviour into thin adapter + pure-C# core |
| `object-pooling-pattern` | Pooling de objetos para spawn/despawn frequente (projéteis, drops, floating text, SFX) |
| `rng-and-determinism` | RNG com seed para conteúdo determinístico; quando não usar UnityEngine.Random |
| `progression-curve-design` | Curvas de XP, thresholds de level, scaling de stat; design de progressão |
| `economy-balance-tuning` | Preços, sinks/sources de gold, balance de shop; specs de tuning de economy |
| `audio-event-wiring` | Wiring SFX/music via GameEventBus (SfxEventBridge/SfxEventMap); any spec needing sound feedback |
| `localization-authoring` | Player-facing text via string key (LocalizationStringTable/LocalizationService); new dialogue/UI/quest text |
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
| `harness-authoring` | Criar nova skill, rule, agent, command ou hook — templates, gates de qualidade e checklist por tipo |
| `harness-audit` | Auditar artefatos do harness após waves grandes ou quando skill/agent parece não estar disparando |
| `delegated-execution` | Delegar execução a subagent Sonnet e VERIFICAR o resultado (no disco + rebuild); evita "narrou e não fez" / desvios omitidos |
| `pixel-art-prompt-authoring` | Autorar prompts EN de pixel art (NPCs/monstros/props) ancorados em Vaalara + D&D oficial; corrigir sprite gerado com cor/arma/anatomia errados; lote em tools/aseprite/prompts.json |
| `sprite-generation-pipeline` | OPERAR o pipeline de geração de sprites por IA (ComfyUI/SDXL local AMD): build de prompts, geração em lote resumível, pós (rembg+downscale), wiring no Unity; gerar/regenerar arte de inimigos/NPCs/props/player |
| `chatgpt-web-sprite-gen` | Gerar sprites de MUNDO (peças modulares, props, tiles, vegetação, landmarks) pela web do ChatGPT via Chrome MCP, no projeto "Sprites - Fazendeiro"; regra de ÂNGULO 3/4 top-down por componente + rate limit (pausa 10 min + retry). Usar quando o local não dá conta de peça modular isolada |
| `npc-walk-animation` | Gerar, AUDITAR (frames realmente animam a passada) e wire folhas de caminhada 5x5 dos NPCs; prompt de keyframes, montagem de auditoria, e o sistema NpcWalkAnimator + slicer (Inicializar Projeto). Usar ao criar/regerar/auditar walk sheets ou ligar a folha ao NPC |

---

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
| `performance-auditor` | Auditoria de C# runtime — allocations em Update, churn de Instantiate/Destroy, LINQ em hot paths |
| `test-author` | Escreve EditMode tests para lógica determinística quando o Testing Quality Gate exige |

---

## Stop Conditions

Stop and report to human if:

- Spec and `CURRENT_STATE.md` conflict
- Spec requires files outside its declared scope
- A mandatory validation fails with no documented path forward
- Task would move a spec to `implementados/` without required evidence
- Task would delete a document not in `DOCUMENT_DELETE_CANDIDATES.md`
- Task would manually edit `.unity` / `.prefab` / `.asset` YAML without spec authorization
- Runtime/code task lacks required Testing Quality Gate evidence or explicit justified residual risk
- Context requires reading PROJECT_LOG without audit/reconciliation/regression justification

---

## Invariant Rules

See `.claude/rules/RULES.md` for full list. Key non-negotiables:

- No `GameObject.Find()` / `FindObjectOfType()` at runtime
- No gameplay communication without `GameEventBus`
- No Unity refs in save DTOs
- No `CindarsHope.Debug` namespace
- No spec promoted without evidence
- No MVP/Play Mode PASS claim without evidence
- Runtime/code changes require automated tests, Play Mode scenario, or documented Testing Quality Gate justification
- Commits in Portuguese

---

*Updated: 2026-06-29 (reconciliação de índices do harness: 62 skills, 16 commands, 10 agents; 20 rules ativas + índice = 21 arquivos; 15 stubs históricos deletados e links repointados)*

---

## Conflict Resolution

- Spec vs. roadmap → follow spec
- Spec vs. refinement → follow spec
- Spec vs. CURRENT_STATE → stop and report
- PROJECT_LOG vs. CURRENT_STATE → prefer CURRENT_STATE, report mismatch

---

*Updated: 2026-06-29 (Skills index completo — 62 skills; reconciliação de índices do harness)*