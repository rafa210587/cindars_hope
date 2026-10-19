# /speckit.specify — Narrative/Quest Cycle Reduction v4

Status: `IMPLEMENTED_BUILD_VALIDATED`

## Ordem de execucao

ARCH.RUNTIME.V4

## Depende de

- `.specs/implementados/spec_arch_dependency_cycle_reduction_v3.md`

## Bloqueia

- novos IDs narrativos persistidos dentro de adapters Unity ou do domínio Quests.

required_adrs: []
required_game_rules: [save_rules, stable_id_rules]

## Objetivo

Eliminar o par mútuo `Narrative|Quests` movendo apenas o catálogo puro `NarrativeIds` para a
assembly Gameplay. Valores persistidos, ordem de quest, bootstrap e comportamento permanecem iguais.

## Baseline e não regressão

- 48 pares mútuos; `NarrativeRuntimeBootstrap -> Quests` e `QuestRegistry.MainQuestHook -> Narrative`;
- sete consumidores de `NarrativeIds`;
- GUID `0c1174d84fa2a024f860a149754e8754` deve ser preservado;
- os sete valores string e nomes públicos não podem mudar;
- EditMode 2.707/2.707 e PlayMode 2/2 devem permanecer verdes.

# /speckit.plan

Mover o contrato puro com GUID preservado, atualizar imports, travar IDs/assembly por teste e
re-medir o grafo.

# /speckit.tasks

- [x] `NarrativeIds` movido para Gameplay com GUID e valores preservados;
- [x] `Narrative|Quests` removido sem novo par mútuo;
- [x] gates completos verdes;
- [x] documentação e registry reconciliados.

## Resultado

- GUID `0c1174d84fa2a024f860a149754e8754` e sete strings preservados;
- EditMode 2.707/2.707; PlayMode 2/2; seis assemblies 0E/0W; ratchet PASS;
- snapshot: 1.573 arquivos, 220 edges e 47 pares mútuos.
