# Closeout — Runtime Maintainability Rework v2

Data: 2026-07-05
Branch: `dev`
Status: `IMPLEMENTED_BUILD_VALIDATED`

## Entregas

1. lifecycle/input centralizados no composition root;
2. progresso e persistência dinâmica de quests extraídos;
3. lifecycle de loja/NPC e política da Thalindra tornados puros;
4. actions e movimentos especiais de inimigos roteados por strategies compartilhadas;
5. assembly pura `CindarsHope.Gameplay` criada com dependência unidirecional;
6. catálogo read-only de 30 action skills extraído do controller;
7. executor de skills migrado de auto-bootstrap para composition root.

## Evidência final

- `CindarsHope.Foundation`, `CindarsHope.Gameplay`, `CindarsHope.Runtime`, `CindarsHope.Editor`,
  `CindarsHope.Tests.EditMode` e `CindarsHope.Tests.PlayMode.Composition`: exit 0, 0E/0W;
- EditMode: 2.703/2.703 em `TestResults/maintainability-ui-data-full.xml`;
- PlayMode: 2/2 em `TestResults/maintainability-ui-data-playmode.xml`;
- ratchet: PASS; RuntimeInitialize 62/63, singleton 55/55, direct input 207/207;
- snapshot: 1.570 C# files, 216 folder edges, 49 mutual pairs, 29 internal types,
  3 internal test crossings;
- GUIDs dos três scripts movidos para Gameplay preservados.

`tools/docs/validate_docs.ps1` foi executado e retornou exit 1 por dívida preexistente: headers das
specs `spec_enemy_attack_kits_v1`, `spec_npc_physics_cat_companion`, `spec_town_building_visuals` e
`spec_town_layout_v9_organic`, além dos falsos positivos históricos de placeholder. Nenhum arquivo
novo deste rework apareceu como falha.

## Não regressão

Não foram alterados IDs, quantidades, save schema, cenas, prefabs, preços, balanceamento, diálogos,
roster ou layouts. `OnGUI`, placeholders e componentes de cena legados sem substituto comprovado não
foram apagados. As alterações paralelas de animação/sprites, ProjectSettings, `.slnx` e ferramentas
não pertencem a este rework.

## Próximas specs, não continuação implícita

- decompor os 49 pares mútuos por domínio, um recorte por spec;
- substituir telas `OnGUI` somente após equivalência Canvas e PlayMode visual;
- migrar o componente legado de active skills da Farm ao regenerar a cena com gate próprio;
- extrair autoria de skills para data assets apenas com migração/validação de IDs.
