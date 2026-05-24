# CLAUDE.md - Cindar's Hope

Arquivo operacional equivalente ao `AGENTS.md` para Claude/Codex.

## Fonte unica de specs

A unica fonte oficial de specs e:

```text
docs/specs/
```

A pasta raiz `specs/` foi removida e nao deve ser recriada. A pasta raiz `spec/` tambem nao deve ser recriada.

Para implementar qualquer feature, usar:

1. `docs/specs/SPEC_EXECUTION_ORDER.md`.
2. A spec alvo em `docs/specs/a_implementar/spec_*.md`.
3. O pre-refinamento relacionado em `docs/refinements/a_implementar/pre_refinamentos/`, quando existir.
4. Specs implementadas dependentes em `docs/specs/implementados/`, apenas quando citadas.

## Leitura minima

Antes de qualquer tarefa:

- `AGENTS.md` ou `CLAUDE.md`.
- `PROJECT_LOG.md` - somente topo/entradas recentes.
- `docs/IMPLEMENTATION_STATUS.md`.
- `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.

Nao ler por padrao:

- `docs_old/**`
- crosswalk completo
- GDD completo
- arquitetura completa
- todos os registries
- todos os refinements
- documentos historicos fora do alvo

## Regras de implementacao

- Nunca implementar feature sem spec aprovada em `docs/specs/a_implementar/`.
- Respeitar `docs/specs/SPEC_EXECUTION_ORDER.md`; nao antecipar specs bloqueadas.
- Alterar somente arquivos dentro do escopo da spec/refinement.
- Nao editar `docs_old/**`.
- Nao marcar nada como implementado sem evidencia no repo.
- Ao finalizar, atualizar spec implementada, refinement implementado, registries, maps, `docs/IMPLEMENTATION_STATUS.md` e `PROJECT_LOG.md` quando aplicavel.
- Rodar `tools/docs/validate_docs.ps1` quando documentacao for alterada.

## Regras inviolaveis de codigo

1. NUNCA usar `GameObject.Find()` ou `FindObjectOfType()`.
2. NUNCA criar comunicacao direta de gameplay; usar `GameEventBus.Publish()` e `Subscribe()`.
3. NUNCA hardcodar dados de balanceamento/conteudo em `MonoBehaviour`; usar ScriptableObject em `Assets/_Game/Data/`.
4. SEMPRE fazer unsubscribe em `OnDisable` ou `OnDestroy`.
5. NUNCA escrever logica de negocio pesada em `MonoBehaviour`; `MonoBehaviour` deve ser ponte Unity/runtime.
6. SEMPRE prefixar ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, `WeaponDataSO`, etc.
7. SEMPRE prefixar eventos: `DayStartedEvent`, `ItemCraftedEvent`, `ToolEquippedEvent`, etc.
8. SEMPRE commits em portugues.
9. Save deve persistir IDs e tipos simples, nunca referencias Unity.
10. Nao usar `StreamingAssets` para save editavel; usar `Application.persistentDataPath`.
11. Nao serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody` em DTOs de save.
12. Sprites: Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`.

## Git

Permitido:

- criar/usar branch local indicada;
- alterar somente arquivos permitidos no escopo;
- criar commits locais em portugues;
- entregar arquivos alterados, testes executados e pendencias.

Proibido sem autorizacao explicita:

- `git push`;
- abrir PR/MR;
- merge;
- deletar branches;
- `git stash`;
- `git clean`;
- `git reset --hard`;
- commitar fora do escopo.

## Namespace Debug proibido

NUNCA criar namespace chamado `Debug` dentro de `CindarsHope.*`.

Usar alternativas:

- `Runtime`
- `DebugTools`
- `Diagnostics`
- `Editor`

## Regra FASE9F - Cave Stable Run

Antes de alterar Cave procedural/stable run, ler:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

Regra central:

- `CaveLevel` ja visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` nao podem regenerar layout, inimigos ou resource nodes.
- Procedural so muda em novo jogo, KO/morte/derrota do personagem ou comando debug explicito.
- Save de cave deve persistir snapshots com tipos simples e sem Unity refs.
