# Resumo — Specs Não-Implementadas / Órfãs (auditoria de código, 2026-08-12)

> Leitura + verificação no código (Grep/Glob) das 12 specs listadas abaixo. Nenhuma spec foi
> editada, nenhum commit foi feito, nenhum sub-agente foi usado (tarefa somente-leitura).

## Visão geral

Das 12 specs auditadas, nenhuma está de fato pronta e esquecida — todas ficaram paradas por um
de três motivos bem distintos, e o `docs/project/CURRENT_STATE.md` (entrada de 2026-06-29) já
documentava 10 delas com o motivo exato antes desta auditoria confirmar no código. **Duas** (`spec_town_building_visuals`,
`spec_village_orders_board`) são **NOT_IMPLEMENTED** genuíno: zero classes/arte no repo, e a
própria spec já traz a auditoria "estado atual" reconhecendo isso — não são órfãs, são fila de
trabalho real ainda não puxada. **Sete** das oito specs de WAVE 04 UI (`04_spec_ui_*`) e as três
de WAVE 05 (`05_spec_*`) caem em **ORFA_CONTRACT_ONLY**: existe um ViewModel/serviço C# puro com
EditMode tests passando, mas **nenhuma View/Canvas/MonoBehaviour liga esse contrato a uma tela ou
loop de gameplay real** — o padrão do projeto (`ui-projection-pattern` + `hud-canvas-binding`)
exige as duas metades, e só a primeira foi feita. Duas (`storage_chest_transfer`,
`weapon_armor_detail_drawer`) nem chegaram a ter execution report — ficaram **BLOCKED** antes de
qualquer código. Em nenhum caso há duplicação de sistema ou coisa a descartar por design ruim; o
veredito predominante é **IMPLEMENTAR** (fechar o binding de Canvas que falta), com uma única
oportunidade real de **FUNDIR** (a UI de confirmação genérica deveria nascer junto do
`04_spec_ui_input_focus_modal_routing_runtime`, de quem depende, em vez de como spec solta).

## Tabela

| Spec | O que é | Estado real no código | Veredito | Por quê |
|---|---|---|---|---|
| `spec_town_building_visuals.md` | Kits de 3 partes (walls/roof/door) para 15 arquétipos de prédio da cidade + wiring no `WorldSpriteLibrary`/`CreateMvpTownScene`. | NOT_IMPLEMENTED. Só existe o kit A/B/C (`houses_modular/`, 6/24 lotes); pasta `building/kits/` está vazia; sem staging em `art/world_gpt/raw/gpt_kit_*`; `CreateMvpTownScene` ainda usa fallback `roof_redtile`. A própria spec já documenta essa auditoria (linha 10-14). | IMPLEMENTAR | Fila de trabalho de arte real e explícita (18/24 lotes em fallback visível na tela); não é duplicação nem escopo duvidoso, só não foi puxada ainda. |
| `spec_village_orders_board.md` | Quadro de encomendas da vila como `QuestInstance` (fonte `Board`) reaproveitando 100% o pipeline de quest existente. | NOT_IMPLEMENTED. Zero arquivos `VillageOrder*`/`VillageOrdersBoardService` no repo; nenhuma referência em `Quests/`, `Core/Events/` ou cena. | IMPLEMENTAR | Spec bem desenhada (reuso explícito do QuestService/Board/RewardApplicator, sem sistema paralelo); só não foi executada. Único risco é ficar obsoleta se o roadmap de economia da vila for repriorizado — vale confirmar prioridade antes de puxar. |
| `04_spec_ui_empty_error_confirmation_patterns_runtime.md` | Padrão transversal de empty/error/blocked state + confirmação leve/forte (`PendingConfirmationAction`) para todas as telas da WAVE 04. | ORFA_CONTRACT_ONLY (pior caso do lote: nem classe existe). Nenhuma classe `PendingConfirmationAction`/`ConfirmationDialog`/`ConfirmationPrompt` no repo. `CURRENT_STATE.md` já lista como um dos 11 `CONTRACT_ONLY`. | IMPLEMENTAR (ou FUNDIR com `04_spec_ui_input_focus_modal_routing_runtime`) | É o padrão-base do qual todas as outras telas de UI dependem (`Blocks: all UI screens`); sem ele, toda a wave 04 fica sem contrato de confirmação real. Faz sentido nascer junto do focus/modal routing de quem depende, não como spec solta. |
| `04_spec_ui_storage_chest_transfer_runtime.md` | Tela modal de storage/chest (dois grids, split/sort/transfer, empty state). | BLOCKED. Nenhuma classe `StorageChest`/`ChestTransfer`/`StorageUI`; nem sequer há execution report (`CURRENT_STATE.md`: "2 SEM execution report"). | IMPLEMENTAR | Escopo claro e depende só de UI + inventory/storage já existentes; nunca chegou a ser tentada. |
| `04_spec_ui_weapon_armor_detail_drawer_runtime.md` | Drawer de detalhe de arma/armadura (stats, material, durabilidade, known interactions gated por bestiário). | BLOCKED. Nenhuma classe `WeaponArmorDetailDrawer`/`EquipmentDetailDrawer`; sem execution report. | IMPLEMENTAR | Mesma situação da anterior — nunca iniciada, mas bloqueia equipment compare, repair/upgrade UI e bestiary UI (`Blocks` na spec), então tende a virar gargalo se outras UIs de equipamento avançarem primeiro. |
| `04_spec_ui_equipment_compare_runtime.md` | Comparison drawer de equipamento (antes/depois, requisitos, known vulnerabilities). | ORFA_CONTRACT_ONLY declarado como `CONTRACT_ONLY` no `CURRENT_STATE.md`; nenhuma classe `EquipmentCompare`/`ComparisonDrawer` encontrada no código (nem o contrato chegou a existir). | IMPLEMENTAR | Depende de `04_spec_ui_inventory_items_tooltips_runtime` (que tem ViewModel parcial) — resolver a cadeia junto faz mais sentido que puxar isolado. |
| `04_spec_ui_fonte_menu_flow_runtime.md` | Menu da Fonte como projection fragment-gated (esconde Anya/final/nível 101 até o fragmento certo). | ORFA_CONTRACT_ONLY. `FonteMenuViewModel.cs` existe e é usado por `MenuProjectionValidator` + `MenuProjectionTests` (EditMode), mas **nenhuma View/Canvas** liga o ViewModel a uma tela navegável na FarmScene. | IMPLEMENTAR | O contrato/projection está pronto e testado — falta só a metade `hud-canvas-binding` (Canvas + View MonoBehaviour). Menor esforço do lote para fechar. |
| `04_spec_ui_inventory_items_tooltips_runtime.md` | Grid de inventário, detail drawer e tooltip padrão com anti-error para item quest/equipado. | ORFA_CONTRACT_ONLY. `ItemTooltipViewModel.cs` (com `InventoryEquipmentTooltipTests`) e `InventoryTooltipViewModel.cs` existem, mas o segundo não tem nenhum uso/teste e nenhum dos dois está ligado a uma View real. | IMPLEMENTAR | Bloqueia equipment compare, shop sell, storage/chest UI e hotbar — é a spec de maior alavancagem para desbloquear o resto da wave 04 se for puxada primeiro. |
| `04_spec_ui_spell_magic_detail_runtime.md` | Detail de spell/magia (MP, cooldown, requisitos, active slot assignment). | ORFA_CONTRACT_ONLY. `SpellDetailViewModel.cs` existe mas sem nenhum teste e sem nenhuma outra referência no repo (nem validator, nem View). | IMPLEMENTAR | Contrato mais "solto" do lote (nem teste tem) — antes de fechar o binding, vale um passe rápido de `editmode-test-authoring` para cobrir o ViewModel. |
| `05_spec_companion_farm_job_board_automation_runtime.md` | Job board de automação controlada de companions na fazenda (área, horário, stamina budget, output rules). | BLOCKED (dependency chain não executada, conforme `CURRENT_STATE.md`). Código author-completo: `CompanionFarmJobDefinition/Assignment`, `JobBoardState`, `CompanionJobBoardService`, `CompanionFarmJobType` + `CompanionJobBoardTests` — mas nenhuma AI/behaviour de companion em cena realmente executa os jobs (`JobBoardState`/`CompanionJobBoardService` só são referenciados entre si e pelo teste). | IMPLEMENTAR | A lógica pura está pronta e testada; falta o lado runtime (companion tick executando o job) e as specs de dependência (`eligibility_bond`, `animals_housing`, `buildings_construction`) que travam a cadeia. Resolver a cadeia de dependência antes de puxar isolada. |
| `05_spec_farm_building_footprints_placement_grid_runtime.md` | Footprint + validação determinística de placement de construções na fazenda. | BLOCKED (dependency chain). Código pronto: `BuildingDefinition`, `FarmBuildingDefinition`, `PlacementValidator`, `ConstructionJob/Request` + `BuildingPlacementValidatorTests` — mas nenhuma UI/modo de construção chama `PlacementValidator` em runtime; a única outra classe "BuildingDefinition"-like no repo (`CityBuildingDefinition`) é de um domínio diferente (cidade), não duplicação. | IMPLEMENTAR | Validador determinístico pronto e testado; falta o construction-mode UI que o consome. Não fundir com `CityBuildingDefinition` — são domínios (farm vs. city) genuinamente separados. |
| `05_spec_farm_level1_layout_fixed_anchors_runtime.md` | Contrato de anchors fixos (Fonte, lago, caverna, saída pra cidade) do layout inicial da fazenda. | BLOCKED (dependency chain). `FarmLevel1LayoutContract.cs` existe com `ValidateFarmLevel1LayoutContract` (rodado por `CindarsHope/Validar Projeto`) e `FarmLevel1LayoutContractTests`, mas **`CreateMvpFarmScene` (o gerador da cena) não referencia o contrato** — o validador só audita a cena depois de gerada, não a orienta durante a criação. | IMPLEMENTAR | Contrato correto e validado (double-check pós-geração funciona), mas o gerador de cena deveria consumir o contrato como fonte de verdade em vez dos dois ficarem dessincronizados por design. Baixo risco, ajuste focado no `CreateMvpFarmScene`. |

## Contagem por veredito

- **IMPLEMENTAR:** 12
- **FUNDIR:** 0 (única menção de fusão é secundária, dentro do veredito IMPLEMENTAR do `empty_error_confirmation_patterns`)
- **DESCARTAR/REPENSAR:** 0

Nenhuma das 12 specs auditadas tem escopo duplicado, obsoleto ou duvidoso — o gargalo em todo o
lote de WAVE 04/05 é sistemático: contratos C# puros com EditMode tests foram entregues, mas o
binding de Canvas/View (metade "física" do `ui-projection-pattern`) nunca foi feito, e as 3 specs
de WAVE 05 estão presas numa cadeia de dependência que não foi resolvida em ordem.
