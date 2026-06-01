# AGENTS.md - Cindar's Hope

## Claude Code Project Structure

Este projeto usa `.claude/` para operacionalizar tarefas via comandos, skills e agentes:

- **Commands** (`.claude/commands/`) — Fluxos: `/start-spec`, `/validate-unity`, `/finish-spec`, `/review-non-regression`, `/docs-health`
- **Skills** (`.claude/skills/`) — Padrões acionáveis: spec-execution, unity-validation, docs-migration, non-regression-review, save-load-pattern, event-bus-pattern, implementation-closeout
- **Agents** (`.claude/agents/`) — Especializados: spec-implementer, unity-validator, docs-curator, non-regression-auditor, architecture-reviewer
- **Settings** (`.claude/settings.json`) — Permissões versionadas e hooks de segurança

Consulte `.claude/` para operações. **Regras fundamentais permanecem em AGENTS.md, CLAUDE.md e `docs/operations/`.**

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, regiao Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resolucao 1280x720.
Geracao de codigo: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify -> Plan -> Tasks -> Implement.

## Fonte unica de specs

A unica fonte oficial de specs e:

```text
docs/specs/
```

A pasta raiz `specs/` foi removida e nao deve ser recriada.
A pasta raiz `spec/` tambem nao deve ser recriada.

Para implementar qualquer feature, o agente deve usar:

1. `docs/specs/SPEC_EXECUTION_ORDER.md`.
2. A spec alvo em `docs/specs/a_implementar/spec_*.md`.
3. O pre-refinamento relacionado em `docs/refinements/a_implementar/pre_refinamentos/`, quando existir.
4. Specs implementadas dependentes em `docs/specs/implementados/`, apenas quando citadas.

## Padrões e Skills Reutilizáveis

Antes de executar tarefas, consultar:

- `memory/MEMORY.md` - índice de padrões provados
- `memory/feedback_working_method.md` - método sequencial para SPECS com validação real-time
- `memory/project_skills_available.md` - 8 skills reutilizáveis para tarefas comuns

### Skills Disponíveis
- **SPEC Validation Pattern**: Validar compilação após cada fase com triage de erros por categoria
- **Namespace Consolidation**: Resolver conflitos de classes duplicadas
- **Bootstrap Integration Pattern**: Wiring de novos managers em GameBootstrap
- **Event Publishing Pattern**: Comunicação descentralizada via GameEventBus
- **Using Directive Organization**: Ordem padrão de imports
- **DamageRequest Construction**: Padrão para criar requisições de dano
- **Save/Load Data Pattern**: Persistência correta (IDs simples, nunca refs Unity)

## Context Reading Policy

Para tarefas de implementacao, ler somente:

1. `AGENTS.md` ou `CLAUDE.md`
2. `docs/00_PROJECT/CURRENT_STATE.md` — contexto de execucao primario (~80 linhas)
3. A spec alvo
4. Arquivos explicitamente citados pela spec
5. O relatorio de validacao imediatamente anterior, somente se listado como dependencia

Nao ler por padrao:

- `PROJECT_LOG.md` — somente para: auditoria, reconciliacao, investigacao de regressao, pedido humano explicito
- `ROADMAP.md` — somente para: planejamento de novas waves, criacao de specs, repriorizacao
- GDD completo
- refinements antigos
- specs arquivadas ou superseded
- relatorios de validacao nao relacionados
- `docs_old/**`

### Resolucao de conflitos

- Spec vs. roadmap → seguir a spec.
- Spec vs. refinement → seguir a spec.
- Spec vs. CURRENT_STATE → parar e reportar a inconsistencia ao humano.
- PROJECT_LOG vs. CURRENT_STATE → preferir CURRENT_STATE e reportar a divergencia.

---

## Fluxo operacional

Antes de qualquer tarefa, seguir:

- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
- Consultar memory se tarefa é similar a anteriores

Ao finalizar implementacao de spec:

- atualizar spec implementada;
- atualizar refinement implementado;
- atualizar registries afetados;
- atualizar maps de refinements afetados;
- atualizar `docs/IMPLEMENTATION_STATUS.md`;
- atualizar `PROJECT_LOG.md`;
- rodar `tools/docs/validate_docs.ps1`.
- se a tarefa alterou runtime/Unity, rodar tambem:
  - `tools/unity/RunUnityCompileValidation.ps1`;
  - `tools/unity/ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`.

Se a validacao Unity nao puder rodar por ambiente, permissao, Unity ausente ou timeout, registrar no resumo final e no `PROJECT_LOG.md`:

```text
Unity validation: NOT RUN
Reason: <motivo>
Command attempted: <comando>
Residual risk: Unity compile not validated locally
```

## Estrutura documental ativa

- `docs/design/` - design ativo do jogo.
- `docs/architecture/` - arquitetura ativa.
- `docs/operations/` - instrucoes operacionais, ambiente, politica de specs e handoff LLM.
- `docs/roadmap/` - roadmap ativo.
- `docs/backlog/` - backlog e ideias futuras.
- `docs/amendments/` - amendments ativos.
- `docs/validation/` - smoke tests e validacoes.
- `docs/specs/implementados/` - specs consolidadas do que ja existe no repo.
- `docs/specs/a_implementar/` - specs futuras aprovadas/consolidadas no padrao SpecKit.
- `docs/specs/SPEC_EXECUTION_ORDER.md` - ordem oficial de execucao.
- `docs/refinements/implementados/` - refinamentos, audits e handoffs implementados.
- `docs/refinements/a_implementar/pre_refinamentos/` - pre-refinamentos vivos.
- `docs_old/` - historico integral preservado; consultar para auditoria, nao editar como fonte ativa.

## Regras inviolaveis de codigo

1. NUNCA usar `GameObject.Find()` ou `FindObjectOfType()`.
2. NUNCA criar comunicacao direta entre sistemas quando a comunicacao for de gameplay; usar `GameEventBus.Publish()` e `Subscribe()`.
3. NUNCA hardcodar dados de jogo em `MonoBehaviour` quando forem dados de balanceamento/conteudo; usar ScriptableObject em `Assets/_Game/Data/`.
4. SEMPRE fazer unsubscribe em `OnDisable` ou `OnDestroy`.
5. NUNCA escrever logica de negocio pesada em `MonoBehaviour`; `MonoBehaviour` deve ser ponte Unity/runtime.
6. SEMPRE prefixar ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, `WeaponDataSO`, etc.
7. SEMPRE prefixar eventos: `DayStartedEvent`, `ItemCraftedEvent`, `ToolEquippedEvent`, etc.
8. SEMPRE commits em portugues.
9. NUNCA implementar feature sem spec aprovada em `docs/specs/a_implementar/`.
10. Sprites: SEMPRE importar com Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`.
11. Save deve persistir IDs e tipos simples, nunca referencias Unity.
12. Nao usar `StreamingAssets` para save editavel; usar `Application.persistentDataPath`.
13. Nao serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody` em DTOs de save.

## Git

Permitido ao agente:

- criar ou usar branch local indicada pelo humano;
- alterar somente arquivos explicitamente permitidos no escopo da tarefa;
- criar commits locais em portugues;
- atualizar `PROJECT_LOG.md` ao final de tarefa relevante;
- atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante;
- entregar ao humano a lista de commits, arquivos alterados, testes executados e testes pendentes.

Proibido sem autorizacao explicita:

- executar `git push`;
- abrir PR/MR;
- fazer merge;
- deletar branches locais ou remotas;
- executar `git stash`;
- executar `git clean`;
- executar `git reset --hard`;
- commitar arquivos fora do escopo permitido.

## Namespace Debug proibido

NUNCA criar namespace chamado `Debug` dentro de `CindarsHope.*`.

Usar alternativas:

- `Runtime`
- `DebugTools`
- `Diagnostics`
- `Editor`

## Regra FASE9F - Cave Stable Run

Antes de qualquer alteracao em Cave procedural/stable run, ler:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

Regra central:

- `CaveLevel` ja visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` nao podem regenerar layout, inimigos ou resource nodes.
- Procedural so muda em novo jogo, KO/morte/derrota do personagem ou comando debug explicito.
- Save de cave deve persistir snapshots com tipos simples e sem Unity refs.
