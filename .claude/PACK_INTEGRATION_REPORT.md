# Relatório de Integração — game_csharp_claude_code_pack

> Análise do pack genérico `game_csharp_claude_code_pack.zip` vs. o `.claude/` atual do Cindar's Hope, e o que foi portado/criado. Documento voltado ao humano (rafa); as rules/skills criadas estão em inglês para casar com o resto do harness.
> Data: 2026-06-20.

---

## 1. Resumo executivo

O pack é um kit **genérico e engine-agnóstico** de boas práticas de jogos em C# (Unity/Godot/engine própria), seguindo a filosofia oficial da Anthropic: *rules* para contexto constante, *skills* para procedimentos, *commands* para atalhos, *hooks* para enforcement determinístico.

O `.claude/` deste projeto **já é muito mais maduro em processo e governança** (spec lifecycle, validation truth gate, testing quality gate, docs governance, cave stable run, 17 hooks ativos, 10 agents, ~35 skills). Copiar o pack inteiro **pioraria** o repo: criaria rules/skills duplicados, exatamente o que a própria skill `system-reuse-audit` proíbe.

Então a decisão foi **cirúrgica**: portar só as lacunas reais de *craft de engenharia* que o projeto não tinha, adaptadas aos idiomas reais daqui (GameEventBus, ScriptableObjects, projections, `FailureReason`, validators, save por id). Nada de duplicar o que já existe.

---

## 2. O que o pack traz (inventário)

| Categoria | Itens |
|-----------|-------|
| Rules (11) | operational-discipline, architecture-gameplay, csharp-style, data-driven-content, error-handling-resilience, multiplayer-networking, performance-memory, save-load-versioning, security-and-files, testing, + path-scoped (unity/godot/tests) |
| Skills (16) | ability-system-design, data-driven-content-pass, error-resilience-pass, event-driven-gameplay, game-architecture-review, game-code-review, game-feature-spec, gameplay-state-machine, issue-to-tasks, multiplayer-sanity-review, performance-pass, pre-pr-validation, refactor-monobehaviour, save-system-review, test-plan-gameplay |
| Commands (8) | game-feature, arch-review, game-code-review, perf-pass, error-pass, refactor-monobehaviour, test-plan, pre-pr |
| Hooks (6) | guard-destructive, guard-secrets, guard-large-files, dotnet-format-after-write, dotnet-test-on-stop, changed-file-audit |
| Templates (4) | adr, bug-report, feature-spec, game-system-checklist |
| Examples (7) | ability-system-skeleton, event-bus, object-pool, state-machine, result-pattern, save-versioning, pure-csharp-core-layout |

**Qualidade do pack:** alta e limpa. Os exemplos em C# são corretos (core puro, sem dependência de engine). A filosofia ("não jogue tudo no CLAUDE.md; mova procedimento pra skill") é a mesma que este projeto já adota.

---

## 3. Matriz de decisão (pack → projeto)

Legenda: ✅ já coberto (não duplicar) · 🟡 lacuna parcial · 🟢 **lacuna real → criei** · ⚪ não se aplica

| Item do pack | Equivalente no projeto | Veredito |
|---|---|---|
| rule architecture-gameplay | unity-architecture (invariantes), mas sem *seleção de patterns* | 🟢 `gameplay-design-patterns.md` |
| rule csharp-style | **nada** | 🟢 `csharp-style.md` |
| rule error-handling-resilience | **nada** (sem taxonomia de erro) | 🟢 `error-handling-resilience.md` |
| rule security-and-files | no-unsafe-git + protected paths, mas sem regra de **segredos** | 🟢 `security-and-files.md` |
| rule data-driven-content | data-catalog-authoring, economy-* | ✅ |
| rule performance-memory | agent performance-auditor + skill object-pooling-pattern | ✅ |
| rule save-load-versioning | skill save-load-pattern + save-section-provider + DTO rule | ✅ |
| rule testing | testing-quality-gate (mais maduro aqui) | ✅ |
| rule multiplayer-networking | — (jogo single-player) | ⚪ |
| rule operational-discipline | context-reading-policy + CLAUDE.md | ✅ (essência já existe) |
| skill gameplay-state-machine | enemy-ai-authoring (só boss phases) | 🟢 `state-machine-design` |
| skill ability-system-design | combat-data-wiring/player-ability (wiring, não design) | 🟢 `ability-effect-composition` |
| skill refactor-monobehaviour | **nada** | 🟢 `monobehaviour-decomposition` |
| skill game-architecture-review | agent architecture-reviewer | ✅ |
| skill game-code-review | /code-review + non-regression-review | ✅ |
| skill performance-pass | agent performance-auditor | ✅ |
| skill save-system-review | save-load-pattern | ✅ |
| skill event-driven-gameplay | event-bus-pattern | ✅ |
| skill game-feature-spec | /start-spec + SPEC template | ✅ |
| skill test-plan-gameplay | gameplay-test-scenario + editmode-test-authoring | ✅ |
| skill error-resilience-pass | non-regression-review (mas sem lente de erro) | 🟡 coberto pela nova rule + review |
| skill multiplayer-sanity-review | — | ⚪ |
| hook guard-secrets | **nada** | 🟢 `guard-secrets.ps1` |
| hook guard-large-files | **nada** | 🟢 `guard-large-files.ps1` |
| hook guard-destructive | pre-bash-guard + permissions.ask | ✅ |
| hook dotnet-format-after-write | — (Unity gerencia; arriscado auto-formatar) | ⚪ deliberado |
| hook dotnet-test-on-stop | /run-editmode-tests (Unity, não dotnet test) | ⚪ deliberado |
| hook changed-file-audit | detect-change-scope | ✅ |
| templates (adr/spec/etc.) | ADRs + SPEC_IMPLEMENTABLE_TEMPLATE | ✅ |
| examples/*.cs | código real do jogo | ⚪ (gerariam código morto num projeto Unity) |

---

## 4. O que eu criei (e por quê)

### Rules novas (`.claude/rules/`)

1. **`csharp-style.md`** — o projeto tinha invariantes de arquitetura mas **nenhuma** regra de estilo C# geral. Cobre nomes/imutabilidade/coleções/async. Ponto crítico: alinhei a sinalização de falha à convenção que já vi no projeto (`bool TryX` + `FailureReason` via evento de HUD) e **proíbo introduzir um `Result<T>` paralelo** sem `system-reuse-audit` — porque isso seria um sistema duplicado.
2. **`error-handling-resilience.md`** — a melhor ideia do pack. Taxonomia de 4 categorias (gameplay esperado / config-asset / infra / bug-invariante), cada uma mapeada a um sistema que já existe aqui (FailureReason, validators 60-pattern, save atômico, fail-fast com log de wiring). Dá uma lente de erro que nenhum agent/skill atual tinha.
3. **`gameplay-design-patterns.md`** — *seleção* de pattern (qual usar quando), mapeada a precedentes reais (`BossPhaseLogic`, `ProceduralSfxFactory`, reward strategy, `StatusEffectDatabase`, projections). Não duplica `unity-architecture` (que é invariante); complementa.
4. **`security-and-files.md`** — o projeto protegia git e paths Unity, mas não tinha regra de **segredos / arquivos sensíveis / saves reais**. Dá respaldo ao novo hook de segredos.

### Skills novas (`.claude/skills/`)

5. **`state-machine-design`** — FSM geral (Enter/Tick/Exit, política de transição inválida, núcleo puro testável em EditMode). O `enemy-ai-authoring` só cobria fases de boss sobre o `EnemyBrain`; faltava a estrutura de estado em si.
6. **`ability-effect-composition`** — *design* de habilidades/efeitos componíveis (contexto com seed, pipeline validar→custo→cooldown→executar→efeitos, modifiers como decorators, persistir o **resultado**). Distinto de `combat-data-wiring` (popula DBs) e `player-ability-runtime` (dash/block de movimento).
7. **`monobehaviour-decomposition`** — como quebrar um god-MonoBehaviour em adapter fino + núcleo puro **sem quebrar serialização de cena/prefab** (não reordenar `[SerializeField]`, rodar `system-reuse-audit` antes, validar a cada passo). O projeto tem a ética de "MonoBehaviour fino" mas nenhuma skill de *como chegar lá*.

### Hooks novos (`.claude/hooks/`) — testados, 9/9 casos passando

8. **`guard-secrets.ps1`** — bloqueia (PreToolUse Edit/Write) conteúdo que parece segredo real (chaves AWS/Google, private keys, tokens Slack/GitHub, `sk-...`, credenciais hardcoded). Mesmo estilo defensivo dos hooks daqui (exit 0 em qualquer ambiguidade, exit 2 + stderr pra bloquear). Escaneia só o conteúdo **novo**, então editar arquivo legado com match não gera ruído.
9. **`guard-large-files.ps1`** — bloqueia um `Write` único > 1 MB (dump/log/blob acidental). Limite ajustável no topo do script.

### Wiring e índices

- **`settings.json`** — liguei os 2 hooks no matcher `Edit|Write` que já existia (junto do `protected-path-guard`). Validei: JSON continua válido, 3 hooks no matcher. **Esta é a única mudança que altera comportamento automático** — revise com `/hooks` se quiser.
- **`rules/RULES.md`** — registrei as 4 rules novas (categoria "Code Craft, Resilience & Safety", itens 12–15) e os 2 hooks na tabela de enforcement.
- **`CLAUDE.md`** — adicionei as 3 skills novas à tabela de Skills.

---

## 5. O que eu deliberadamente NÃO portei (e por quê)

- **Quase todas as skills/commands do pack** (game-code-review, performance-pass, save-system-review, game-feature-spec, etc.): já existem como agents/commands/skills aqui, muitas vezes mais maduros. Duplicar violaria `system-reuse-audit`.
- **`dotnet-format-after-write` e `dotnet-test-on-stop`**: o projeto é Unity. Auto-formatar `.cs` pode brigar com o estilo/asmdef e `dotnet test` não é o caminho (a validação é Unity batchmode + EditMode via `/run-editmode-tests`). Auto-rodar no Stop seria lento e frágil aqui.
- **rule/skill de multiplayer**: jogo single-player. Mantive como "não se aplica" (fácil de adicionar se um dia houver co-op).
- **examples/*.cs**: são ótimos como referência, mas colocá-los em `Assets/` viraria código morto (ou pior, compilado). Ficam disponíveis no zip extraído se você quiser consultar.
- **Não commitei nada.** As mudanças estão no working tree pra você revisar. (Regra do projeto: commit só quando você pedir.)

---

## 6. Sugestões adicionais (além do pack — pra você decidir)

Lacunas que notei e que valem virar trabalho, mas que **não** criei por estarem fora do escopo "integrar o pack" ou por precisarem de decisão sua:

1. **Spec de fundação de áudio** — a skill `game-feel-checklist` já avisa: o projeto tem **zero código de áudio**. Tem até `AudioManager.cs.meta` aparecendo no `git status` (arquivos `.meta` órfãos). Uma spec de fundação de áudio (channels, SFX por evento, crossfade de música) destravaria todo o "juice" das specs de combate/farm. Há indícios de que já começou (`Assets/_Game/Scripts/Audio/*`).
2. **Object pooling de verdade** — `performance-auditor` e a skill `object-pooling-pattern` já existem, mas o baseline ainda é "sem pooling em lugar nenhum" e `ProjectileSpawnService` faz Instantiate/Destroy por tiro. Boa candidata a spec dedicada.
3. **`Result<T>` vs `FailureReason`** — decisão de craft: hoje a sinalização de falha é por `bool`+`FailureReason` (string). Funciona, mas strings de razão são frágeis pra i18n e pra teste. Vale um ADR decidindo entre padronizar a string-key (enum + tabela de localização) ou adotar um `Outcome` leve. Deixei a rule `csharp-style` neutra e exigindo `system-reuse-audit` antes de qualquer wrapper novo.
4. **Hook de "no `Debug.Log` solto em runtime"** — complementaria o `error-handling-resilience` (logs precisam de contexto). Dá pra fazer um PostToolUse leve que sinaliza `Debug.Log("...")` sem contexto em `Assets/_Game/Scripts/**`. Não fiz pra não gerar ruído sem você querer.
5. **Skill de networking** (se um dia houver co-op): o pack tem `multiplayer-sanity-review` pronto pra adaptar.

---

## 7. Como reverter / revisar

- Todas as mudanças estão sob `.claude/` (nenhum arquivo de jogo, doc canônico ou YAML Unity foi tocado).
- Arquivos novos: 4 rules, 3 skills, 2 hooks, este relatório.
- Arquivos editados: `settings.json`, `rules/RULES.md`, `CLAUDE.md`.
- Para reverter o wiring de hooks: remova os 2 blocos `guard-*` do matcher `Edit|Write` em `settings.json`.
- Para ver o pack original: `C:\Users\Rafa\Downloads\_game_pack_extracted\`.
