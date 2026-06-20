---
name: cave-stable-run-guard
description: Guardrails para edits procedurais/runtime da cave sob a regra de stable run da FASE9F. Use antes de editar qualquer código de procedural, materialization, runtime state, snapshot, spawn, resource node, exit, checkpoint ou save da cave.
---

# Skill: Guard de Stable Run da Cave

Dentro do mesmo `CaveRunSeed`, um `CaveLevel` já visitado não pode re-rolar layout, inimigos nem resource nodes; use sempre seeds determinísticos e IDs estáveis.

## Leitura mínima

Leia estes primeiro:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

## Regra central

Dentro do mesmo `CaveRunSeed`, um `CaveLevel` visitado anteriormente deve manter:

- layout;
- entrance e exit;
- enemy composition;
- enemy count;
- enemy positions;
- enemy IDs/types;
- resource node composition;
- resource node count;
- resource node positions;
- depleted node state;
- boss/miniboss state quando aplicável.

O conteúdo procedural só pode mudar em:

- new game;
- KO/death/defeat;
- explicit debug run regeneration.

`ForwardExit` e `BackExit` não podem mudar o `CaveRunSeed`.

## Checklist de implementação

- [ ] Não re-rolar o conteúdo de um level visitado ao re-entrar.
- [ ] Manter os seeds determinísticos: `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`.
- [ ] Usar IDs estáveis para runtime content; não gerar GUIDs aleatórios para stable run content.
- [ ] Salvar apenas IDs/dados de DTO simples.
- [ ] Não serializar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider` ou `Rigidbody`.
- [ ] O materializer deve materializar a partir de dados gerados/snapshot, não inventar gameplay content silenciosamente.
- [ ] Se o suporte completo a snapshot estiver fora de escopo, documentar essa limitação explicitamente.

## Red flags

Pare e reveja o escopo se a mudança:

- muda `CaveRunSeed` no uso de portal;
- adiciona `Guid.NewGuid()` ou timestamp a IDs de instância de enemy/resource;
- chama random sem um seed determinístico;
- modifica o save schema da cave sem migration;
- adiciona respawn/redistribution/boss completion quando a spec exclui isso;
- usa runtime scene search para fazer wiring de sistemas da cave.

## Fechamento

Ao fechar uma tarefa de cave, reporte:

- como a estabilidade do run é preservada;
- se o comportamento de first-visit vs revisit mudou;
- se o snapshot/save foi alterado;
- o que resta para a próxima slice da SPEC 14.
