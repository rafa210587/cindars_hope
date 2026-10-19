# REF — PR153 cave procedural handoff

> Origem histórica: `docs_old/audits/PR153_CAVE_PROCEDURAL_HANDOFF.md`
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# PR-153 Cave Procedural Handoff

Status: implementado parcial em pacote unico PR-141 a PR-153.

## Implementado

- Cave procedural contracts.
- Generator puro deterministico por `CaveWorldSeed + CaveRunSeed + CaveLevel`.
- Cave seeds via `CaveRunManager`.
- `CaveLevelRuntimeController` com log de layout e evento de entrada.
- Debug HUD com status procedural da cave quando rebundado.
- Regeneracao debug com `Shift+R`.
- `CaveCheckpointService` com checkpoints `1, 15, 30, 45, 60, 75, 90`.
- ResourceNode contracts.
- ResourceNode rules e runtime MVP com ferramenta/tier/fallback.
- Save/load cave MVP via `CaveSaveData`.
- Validator Cave MVP expandido para runtime procedural e ResourceNodes debug.

## Parcial ou pendente

- KO real regenerando run.
- Boss.
- Biome progression.
- Enemy spawn real por layout.
- Loot tables completas.
- Daily refresh.
- Faction locks FASE9G.
- Minimap, baus e arte final.
- Stamina real em ResourceNode.

## Validacao Unity pendente

- Regenerar CaveScene pelo menu `CindarsHope/Scenes/Create MVP CaveScene`.
- Rodar `CindarsHope/Validate/Validate Cave MVP`.
- Entrar em Play Mode na CaveScene.
- Confirmar HUD com `Cave: procedural MVP`, `WorldSeed` e `RunSeed`.
- Confirmar log `Cave generated`, rooms, enemy points e resource points.
- Testar `Shift+R`: RunSeed muda, WorldSeed permanece, layout/log muda.
- Testar ResourceNodes debug com Pickaxe/Axe/fallback.
- Testar save/load de seeds e nodes depletados.

## Proximo pacote recomendado

Implementar KO real regenerando run, spawn de inimigos a partir do layout, ResourceNode stamina real, daily refresh e primeira camada de biome progression.



