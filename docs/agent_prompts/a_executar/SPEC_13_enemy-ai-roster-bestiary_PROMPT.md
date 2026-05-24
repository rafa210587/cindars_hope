# Prompt para Claude Code/Codex — Cindar's Hope

Use este prompt dentro do repositório local `rafa210587/cindars_hope`, preferencialmente partindo da branch `dev` atualizada.

## Regra central desta rodada

O humano só fará validação manual no final de tudo. Portanto:

- Não pare no meio pedindo validação humana.
- Não use "validar manualmente depois" como critério de pronto intermediário.
- Rode as validações automatizadas/documentais disponíveis.
- Quando Play Mode exigir interação humana e você não conseguir executar, registre formalmente como `NOT RUN`, com motivo, comando tentado e risco residual.
- Prepare evidências e checklist para a validação humana final, mas siga para fechar a spec dentro do possível.

## Leitura mínima obrigatória

Leia antes de alterar qualquer arquivo:

```text
AGENTS.md
CLAUDE.md
PROJECT_LOG.md                         # topo/entradas recentes
docs/IMPLEMENTATION_STATUS.md
docs/operations/AGENT_EXECUTION_PROTOCOL.md
docs/specs/SPEC_SOURCE_OF_TRUTH.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
memory/MEMORY.md
memory/project_skills_available.md
memory/feedback_working_method.md
```

## Regras invioláveis

- Fonte oficial de specs: `docs/specs/`. Não recriar `specs/` nem `spec/` na raiz.
- Não usar `GameObject.Find()`, `FindObjectOfType()` ou busca global em runtime para wiring de sistema.
- Comunicação de gameplay deve passar por `GameEventBus` quando cruzar sistemas.
- Dados de jogo/balanceamento devem ficar em `ScriptableObject`, não hardcoded em `MonoBehaviour`.
- Save deve persistir IDs e tipos simples. Nunca serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider`, `Rigidbody` ou refs Unity.
- Save editável deve usar `Application.persistentDataPath`; não usar `StreamingAssets`.
- Criar assets Unity via Editor script/menu idempotente, não YAML manual complexo.
- Não ampliar escopo para V2/FULL além do necessário para fechar esta spec.
- Não fazer `git push`, abrir PR, merge, reset hard, stash ou clean sem autorização explícita.
- Commits locais em português.

## Skills obrigatórias a usar

Use explicitamente os padrões abaixo de `memory/project_skills_available.md` quando aplicável:

1. `SPEC Validation Pattern` — rodar e interpretar validação Unity.
2. `Scene Wiring Validation Pattern` — conferir GameBootstrap, scene installers e scripts de criação de cena.
3. `Unity Asset Creation Pattern` — criar SOs/assets/configs via Editor script idempotente.
4. `Save/Load Data Pattern` — persistir apenas DTOs simples e IDs.
5. `Event Publishing Pattern` — eventos desacoplados via `GameEventBus`.
6. `Spec Closure / Registry Reconciliation Pattern` — atualizar status, registries, docs e log sem contradição.
7. `Play Mode Manual Validation Checklist` — registrar o checklist final mesmo se o Play Mode não puder ser executado por você.

## Comandos de validação obrigatórios

Se alterar docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Se alterar runtime Unity/C# ou assets/cenas:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

Se o caminho do Unity local for diferente, descubra o caminho instalado ou registre `NOT RUN` formalmente.

## Encerramento obrigatório

Ao terminar, entregue e registre em `PROJECT_LOG.md`:

```text
Arquivos alterados
Resumo técnico
Specs/refinements lidos
Skills usadas
Validações executadas
Validações NOT RUN, com motivo
Pendências reais
Risco residual
Commit local criado
Checklist Play Mode para validação humana final
```


---

# SPEC 13 — Enemy AI, Roster, Bestiary e Faction Locks

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-13-enemy-ai-roster-bestiary
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Spec alvo

```text
docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
```

## Dependências diretas

Depende de 12. Bloqueia 14, 15 e 17.

## Objetivo desta execução

Finalizar inimigos data-driven com roster, abilities, telegraphs, bestiary persistente, faction locks e ecologia de spawn.

## Passo 0 — reconciliação obrigatória

Antes de implementar qualquer coisa:

1. Leia a spec alvo inteira.
2. Leia o refinement relacionado, se existir.
3. Leia os arquivos de código reais nas áreas permitidas.
4. Compare o status da spec em:
   - `docs/specs/SPEC_EXECUTION_ORDER.md`
   - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
   - `docs/IMPLEMENTATION_STATUS.md`
   - `PROJECT_LOG.md`
5. Se houver contradição, considere o código real como fonte operacional e atualize os documentos no encerramento.
6. Não refaça sistema que já existe; feche os gaps reais.

## Gaps que devem ser fechados ou formalmente reclassificados

- Auditar código real; registry pode dizer parcial, análise anterior dizia roster vazio.
- Roster de 40+ EnemyDataSO ou dividir formalmente em MVP fechado + backlog se a spec permitir.
- Abilities data-driven por monstro.
- Vulnerability window por monstro.
- Telegraph visual claro.
- Tamanhos e movement profiles formalizados.
- Bestiary persistente com unlock por encontro/kill.
- Faction locks e ecologia de spawn integradas ao cave spawn plan.
- XP reward balanceado.
- Play Mode com múltiplos roles: melee, ranged, tank, fast, status.

## Áreas permitidas para alteração

- `Assets/_Game/Scripts/Combat/**`
- `Assets/_Game/Scripts/Enemies/**`
- `Assets/_Game/Scripts/Enemy/**`
- `Assets/_Game/Scripts/Cave/**`
- `Assets/_Game/Scripts/UI/**`
- `Assets/_Game/Scripts/Save/**`
- `Assets/_Game/Scripts/Core/Events/**`
- `Assets/_Game/Scripts/**/Editor/**`
- `Assets/_Game/Data/Enemies/**`
- `Assets/_Game/Data/Creatures/**`
- `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`

Pode alterar documentação de status/registry/log necessária para fechar a spec:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/refinements/implementados/ref_implementados_map.md
docs/refinements/a_implementar/ref_futuro_map.md
```

Não altere `docs_old/**` salvo se a spec pedir explicitamente auditoria histórica; neste caso, apenas referencie, não migre em massa.

## Critérios de aceite

- EnemyDataSO é fonte única de stats/abilities.
- Inimigos atacam/movem/telegrafam sem exceptions.
- Bestiary persiste por IDs.
- Spawn respeita faction locks/ecologia.
- XP/drops integram progressão/loot.

## Validação Unity e Play Mode

Use as skills obrigatórias:

- `SPEC Validation Pattern` para compilar e escanear logs.
- `Scene Wiring Validation Pattern` se tocar managers, cenas, bootstrap, installers ou scripts de criação de cena.
- `Unity Asset Creation Pattern` para assets/SOs/configs.
- `Play Mode Manual Validation Checklist` para registrar o fluxo funcional desta spec.

Formato obrigatório no resumo final:

```text
PLAY MODE TEST: SPEC 13 — Enemy AI, Roster, Bestiary e Faction Locks
Scene used:
Steps executed:
Expected result:
Observed result:
Bugs found:
Passed: YES/NO/NOT RUN
Evidence:
```

## Commit

Crie commit local em português, por exemplo:

```bash
git add <arquivos>
git commit -m "feat: finalizar spec 13 - enemy ai roster bestiary"
```

Não faça push.
