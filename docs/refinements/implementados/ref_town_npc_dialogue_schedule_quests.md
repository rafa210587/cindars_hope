# REF IMPLEMENTADO - Town NPC dialogue, schedule e quests

> Status: Implementado completo
> Spec: `.specs/implementados/spec_town_npc_dialogue_schedule_quests.md`
> Origem: `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_town_npc_dialogue_schedule_quests.md`
> Data: 2026-05-24

## Decisoes executadas

- Pip permanece recepcionista e passa a usar dialogo ramificado; nao abre loja nem inicia quest.
- Os dois lojistas continuam usando o fluxo comercial da SPEC 06.
- Um wanderer de lore foi formalizado com area limitada e pausa durante interacao.
- Dialogue reutiliza o `DialogueModal`/`ModalManager` existente.
- Save de NPC persiste apenas IDs, posicao e `HasMet`, sem referencias Unity.
- Quests, relationship e schedule avancado permaneceram fora do runtime.

## Validacao e risco residual

`CreateMvpTownScene.CreateScene` materializou a cena e `MvpSceneValidator.ValidateSpec08Scene` passou em batchmode com `*** Tundra build success`. `ScanUnityLogs.ps1` ainda falha nas linhas conhecidas dos assemblies `firstpass`. Play Mode manual permanece pendente para UX e save/load interativo. O log reporta duplicidade de `item_crop_wheat` disparada pelo inicializador da SPEC 07, sem relacao com os contratos NPC desta spec.
