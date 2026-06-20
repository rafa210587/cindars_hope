# Padrão de Autoria do Harness — Cindar's Hope

> Fonte de verdade para escrever e padronizar **skills, commands, rules, hooks e agents** em `.claude/`.
> Idioma da prosa: **português (PT-BR)**. Termos técnicos, conceitos e identificadores: **inglês** (ver glossário).
> Em agents, preserve `name` e `tools` no frontmatter (são funcionais); traduza só a prosa.

---

## 1. Princípio de idioma

- **Traduza a prosa** (explicações, instruções, "quando usar", "por quê", "saída esperada").
- **NÃO traduza termos e conceitos** — engine, padrões de design, jargão de dev, e qualquer identificador de código.
- Regra prática: se a palavra apareceria igual num PR review de um time internacional de Unity, mantenha em inglês.

## 2. O que NUNCA traduzir (preservar byte a byte)

- `name:` de skills e qualquer valor de frontmatter que seja id (kebab-case).
- Identificadores de código: classes, métodos, campos, namespaces, enums (`GameEventBus`, `BossPhaseLogic`, `FailureReason`, `ISaveSectionProvider`…).
- Caminhos de arquivo/pasta (`Assets/_Game/Scripts/...`, `.specs/`, `docs/validation/...`).
- Blocos de código, comandos PowerShell, regex, JSON, nomes de cmdlet.
- Nomes de skills/rules/commands/hooks/agents referenciados (`(skill: editmode-test-authoring)`, `rule: unity-architecture`).
- Tokens canônicos de status: `BUILD_VALIDATED`, `NEEDS_REWORK`, `CONTRACT_ONLY`, `PLAYMODE_VALIDATED`, etc.
- Ids de spec/wave: `fable_27`, `FASE9F`, `wave_integration_08`, `ADR-0015`.
- Nomes de arquivo das rules (a rule fica em PT por dentro, mas o arquivo `validation-truth.md` continua com esse nome — docs/ apontam pra ele).

## 3. Glossário — termos que FICAM em inglês

**Engine/Unity:** Unity, Godot, MonoBehaviour, ScriptableObject (SO), GameObject, Transform, Component, Prefab, Scene, Inspector, SerializeField, Awake, Start, Update, FixedUpdate, LateUpdate, OnValidate, Coroutine, Rigidbody2D, Physics2D, AssetDatabase, PrefabUtility, SerializedObject, MenuItem, batchmode, Play Mode, EditMode, asmdef, GUID, meta, prefab/scene YAML.

**Padrões/conceitos:** state machine, FSM, Strategy, Command, Factory, Object Pool, pooling, Adapter, Ports, Decorator, Modifier, Null Object, Flyweight, projection, ViewModel, event bus, save/load, DTO, migration, seed, RNG, determinism, hot path, allocation, GC, boxing, closure, LINQ, telegraph, affix, buff, debuff, cooldown, loot table, spawn/despawn, knockback, stack, idempotency.

**Dev/processo:** build, commit, PR, diff, hook, skill, command, rule, spec, wave, exit code, stderr, stdout, frontmatter, kebab-case, id, namespace, enum, interface, readonly, `async void`, `TryGet`, bool/string/int/float, baseline, checklist.

> Palavras de ligação que TÊM tradução natural devem ser traduzidas: avoid→evite, prefer→prefira, before→antes, after→depois, never→nunca, reuse→reusar, missing→ausente, etc. Mantenha "save", "build", "hook" mesmo quando viram verbo ("salvar o estado" pode; mas "rodar o build" mantém build).

## 4. Mapa de seções canônicas (EN → PT)

Use estes títulos quando o conteúdo se encaixar. Não force seções vazias; preserve todo o conteúdo específico.

| Inglês (variações) | PT canônico |
|---|---|
| Use When / When to use | `## Quando usar` |
| When NOT to use | `## Quando NÃO usar` |
| Why this skill exists / Why | `## Por que existe` |
| Required Reads / Reads | `## Leitura mínima` |
| Do Not Read (By Default) | `## Não ler por padrão` |
| Procedure / Steps / Execution Steps | `## Procedimento` |
| Output / Expected output / Output Format | `## Saída esperada` |
| Deliverable | `## Entregável` |
| Rules | `## Regras` |
| Validation | `## Validação` |
| Tests / Test expectations | `## Testes` |
| Existing systems / guardrails (reuse...) | `## Sistemas existentes (reusar, não duplicar)` |
| Decision matrix | `## Matriz de decisão` |
| Allowed Edits / Forbidden Edits | `## Edições permitidas` / `## Edições proibidas` |
| Common Regressions | `## Regressões comuns` |
| Stop Conditions / When to stop | `## Quando parar e reportar` |
| Failure Handling | `## Tratamento de falha` |
| Success Criteria | `## Critérios de sucesso` |
| Save interaction | `## Interação com save` |
| Closeout | `## Fechamento` |
| Applies to / Applies directly | `## Onde se aplica` |
| Related | `## Relacionados` |

## 5. Padrão por tipo de artefato

### Skills (`.claude/skills/<name>/SKILL.md`)

- Frontmatter: **só** `name` (inalterado, inglês) e `description` (PT, com gatilhos concretos). **Remover** `version` e `when_to_use` (dobrar o conteúdo útil de `when_to_use` dentro do `description`).
- `description`: 1–2 frases, terceira pessoa, dizendo **o que faz** e **quando usar** (gatilhos: nomes de specs, sistemas, situações). Termos em inglês.
- Título: `# Skill: <Título em PT>`.
- Corpo: 1 frase de contexto/precedente real do projeto, depois seções do mapa (§4).
- Manter todos os blocos de código, tabelas, `(skill: x)`/`(rule: y)` e referências a arquivos.

### Commands (`.claude/commands/<name>.md`)

- Sem frontmatter (padrão atual do projeto). Manter `# /<nome>` como título (não traduzir o nome).
- Manter `$ARGUMENTS`, caminhos, blocos PowerShell e tabelas de output verbatim. Traduzir só a prosa/labels.

### Rules (`.claude/rules/<name>.md`)

- Título: `# Rule: <Título em PT>`.
- Manter tabelas, seção de **enforcement**, nomes de hooks/comandos e todos os links/paths.
- **Stubs** (ex.: `> Consolidated into ...`): traduzir a prosa, manter a estrutura e o link de consolidação; `Invariant:` → `Invariante:`.
- `RULES.md` (índice): traduzir prosa e cabeçalhos; manter a numeração, todos os links, e as tabelas de enforcement intactas.
- Já em PT (`spec_quality_gate.md`, `windows_powershell_only.md`): só normalizar título/seções; não retraduzir.

### Hooks (`.claude/hooks/<name>.ps1`)

- **Traduzir SOMENTE:** linhas de comentário `#` e o texto human-readable dentro de mensagens (`Write-Error`, `[Console]::Error.WriteLine`, `Write-Host`).
- **NÃO alterar:** lógica, nomes de variáveis, regex, cmdlets, exit codes, chaves de JSON, fluxo de controle, strings que são identificadores/paths/regras (ex.: manter `Rule: validation-truth`).
- O comportamento deve ser idêntico. Todo hook é **re-testado** depois (parse + smoke test).

### Agents (`.claude/agents/<name>.md`)

- Frontmatter: manter `name` (inalterado, inglês) **e** `tools` (inalterado — é funcional); traduzir `description` para PT (com gatilhos, termos em inglês).
- Título: `# Agent: <Título em PT>`.
- Corpo: traduzir a prosa (Role/Capability/Why/Checklist/Output/Rules) e cabeçalhos pelo mapa §4. Manter identificadores, paths, refs `(skill:/rule:)`, blocos de código e tokens de status. Output Format / report templates: manter os labels canônicos em inglês.

## 6. Checklist de qualidade (pós-edição, por arquivo)

- [ ] `name:` (skills) inalterado; frontmatter sem `version`/`when_to_use`.
- [ ] Nenhum identificador, path, comando, regex ou token de status traduzido.
- [ ] Todos os links `(skill:/rule:)` e referências de arquivo preservados.
- [ ] Tabelas e blocos de código intactos (só prosa de célula traduzida).
- [ ] Sem perda de conteúdo: é tradução + reestruturação, **não** resumo.
- [ ] Hooks: lógica byte-idêntica; só comentário/mensagem em PT.

---

*Criado: 2026-06-20. Aplica-se a toda edição futura de skills/commands/rules/hooks.*
