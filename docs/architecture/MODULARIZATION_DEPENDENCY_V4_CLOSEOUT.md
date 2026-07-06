# Closeout — Narrative/Quest Cycle Reduction v4

Data: 2026-07-05
Status: `IMPLEMENTED_BUILD_VALIDATED`

## Mudança

`NarrativeIds` saiu do adapter Narrative e passou à assembly pura `CindarsHope.Gameplay`, no
namespace `CindarsHope.Gameplay.Narrative`. O catálogo mantém os sete IDs persistidos e o mesmo GUID.

`NarrativeRuntimeBootstrap` continua dependendo de Quest para composição. `QuestRegistry` agora
depende apenas do contrato Gameplay, removendo a direção reversa `Quests -> Narrative`.

## Evidências

- 13/13 testes narrativos;
- EditMode 2.707/2.707 em `TestResults/dependency-v4-full.xml`;
- PlayMode 2/2 em `TestResults/dependency-v4-playmode.xml`;
- seis assemblies 0 erros/0 warnings; ratchet PASS;
- snapshot: 1.573 arquivos, 220 edges, 47 pares mútuos, 29 internal e 3 crossings;
- GUID e valores validados antes/depois.

O validador documental permanece em exit 1 apenas pelas dívidas legadas já registradas; nenhum
arquivo desta spec apareceu entre as falhas.

## Não regressão

Nenhum ID, valor, flag, quest, objetivo, recompensa, save, cena, prefab, texto ou ordem de bootstrap
foi alterado. A mudança é exclusivamente de ownership e direção de dependência.
