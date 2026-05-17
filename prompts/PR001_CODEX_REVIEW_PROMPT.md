# Prompt para Codex revisar PR-001

Leia primeiro:
- CLAUDE.md
- AGENTS.md
- docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md
- docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md

Revise somente o PR-001 — Core Foundation.

Arquivos permitidos para leitura/edição:
- Assets/_Game/Scripts/Core/GameEventBus.cs
- Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
- Assets/_Game/Scripts/Core/Events/GoldChangedEvent.cs
- Assets/_Game/Scripts/Core/Events/InventoryChangedEvent.cs
- Assets/_Game/Scripts/Core/Events/SeedPlantedEvent.cs
- Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
- Assets/_Game/Scripts/Core/Events/TreeChoppedEvent.cs
- Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
- Assets/_Game/Scripts/Core/Events/HungerChangedEvent.cs
- Assets/_Game/Scripts/Core/Events/GameSavedEvent.cs

Arquivos proibidos:
- Qualquer cena Unity
- Qualquer prefab
- Qualquer ScriptableObject asset
- Qualquer documento de design
- Qualquer sistema de Player, Inventory, Farm, Save ou UI

Verifique:
- Código compila no Unity.
- Eventos não carregam GameObject, Transform, MonoBehaviour ou ScriptableObject.
- Eventos carregam IDs e tipos simples.
- GameEventBus permite Subscribe, Unsubscribe e Publish tipados.
- Publish não quebra se um listener lançar exceção.
- Não há dependência de cena.
- Não há GameObject.Find, FindObjectOfType ou FindObjectsByType.

Não implemente PR-002.
Não implemente Player.
Não implemente Inventory.
Não implemente UI.
Não implemente Save.

Ao final, entregue:
- resumo do diff;
- se compilou ou não;
- riscos encontrados;
- próximo PR sugerido.
