# AGENTS.md — Cindar's Hope

> Common rules for all agents. For routing, commands, and skills see `CLAUDE.md`.

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim em Unity LTS / C#.
Mundo: Vaalara, cidade Cindar's Hope, regiao Dornecia.
Spec source: `docs/specs/`. Pasta raiz `specs/` e `spec/` removidas — nao recriar.

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

Leitura minima para qualquer tarefa de implementacao:

1. `AGENTS.md` ou `CLAUDE.md`
2. `docs/00_PROJECT/CURRENT_STATE.md`
3. Spec ativa
4. Arquivos citados pela spec

Para detalhes de workflow: `.claude/commands/` e `.claude/skills/`.

Ao finalizar implementacao de spec, usar `/finish-spec` para closeout completo com evidencia.

Se a validacao Unity nao puder rodar, registrar:

```text
Unity validation: NOT RUN
Reason: <motivo>
Command attempted: <comando>
Residual risk: Unity compile not validated locally
```

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
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

Regra central:

- `CaveLevel` ja visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` nao podem regenerar layout, inimigos ou resource nodes.
- Procedural so muda em novo jogo, KO/morte/derrota do personagem ou comando debug explicito.
- Save de cave deve persistir snapshots com tipos simples e sem Unity refs.
