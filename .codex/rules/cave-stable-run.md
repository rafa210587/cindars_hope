# Rule: Cave Stable Run

Qualquer mudança procedural/runtime da cave precisa preservar o stable-run contract da FASE9F.

## Leitura mínima antes de mudanças na cave

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`

## Invariante

Dentro do mesmo `CaveRunSeed`, um `CaveLevel` revisitado não pode dar reroll em:

- layout;
- entrance/exit;
- enemy composition/count/positions/IDs;
- resource node composition/count/positions/IDs;
- depleted resource state;
- boss/miniboss state.

## Mudanças de seed permitidas apenas em

- new game;
- KO/death/defeat;
- comando explícito de debug run regeneration.

`ForwardExit` e `BackExit` nunca podem mudar o `CaveRunSeed`.

## Requisitos de implementação

- Use deterministic seeds baseados em `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`.
- Não use GUIDs aleatórios ou timestamps para stable runtime content IDs.
- Se a persistência de snapshot completo estiver fora de escopo, documente essa limitação e mantenha o próximo slice de SPEC 14 claro.
