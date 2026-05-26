# Refinamento: Cave Entry, Death, Anya e Corpse Recovery

> Status: **IMPLEMENTADO** — 2026-05-25
> Spec relacionada: `docs/specs/implementados/spec_cave_entry_death_anya_corpse_recovery.md`
> Fonte original: `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_cave_entry_death_anya_corpse_recovery.md`

---

## O que foi implementado (SPEC 15)

### Foundation (Phase 1 — 23 arquivos criados):
- PlayerDeathController.cs — monitoramento de HP, detecção de morte via HPChangedEvent
- CaveDeathPolicy.cs — regras de comportamento de morte
- CaveDeathResolver.cs — orquestração: criação do corpse, transferência de itens/gold/equipment
- CorpseRecoveryManager.cs — gerenciamento de lifecycle do corpse (active/partial/recovered/replaced)
- CorpseInteractable.cs — interação no mundo para recuperar o corpse
- AnyaFountain.cs — localização e ponto de respawn
- AnyaRespawnService.cs — lógica de respawn (HP/Stamina/Mana restaurados)
- AnyaFountainInteractable.cs — handler de interação com a fonte
- 10 eventos do ciclo de morte/recovery publicados via GameEventBus

### Integration Wiring (Phase 2 — 4 arquivos criados):
- DeathSystemBootstrap.cs — orquestrador central de inicialização do sistema de morte
- CorpseSpawner.cs — materializa corpses como GameObjects na cave
- CorpseRecoveryUIController.cs — gerencia modal de recovery
- AnyaFountainUIController.cs — gerencia menu da fonte

### Finalization Fixes (2026-05-25 — 16 erros corrigidos):
- IInteractable contract implementado: AnyaFountainInteractable, CorpseInteractable
- ModalBase abstract class criado
- ModalManager.OpenModal<T>() implementado
- APIs alinhadas: CaveRunManager (CaveRunSeed, CurrentCaveLevel), PlayerProgressionManager (Level, CurrentXp)
- PlayerProgressionEvents criados (XpChanged, LevelChanged)
- ResetCurrentLevelXp() adicionado ao PlayerProgressionManager
- GetAllItems() / GetAllEquippedItems() / UnequipAll() adicionados
- Input bloqueado quando modal ativo (R, T, Y, G)
- FindObjectOfType() removido de todos os interactables

---

## Gaps identificados na spec — status final

| Gap | Status |
|-----|--------|
| Morte na cave com regra de perda total | ✅ Implementado |
| Corpse recovery único (último corpo) | ✅ Implementado |
| Segunda morte substitui corpse anterior | ✅ Implementado (CorpseReplacedEvent) |
| Fonte de Anya como ponto real de respawn | ✅ Implementado |
| Inventário/equipment/gold removidos na morte | ✅ Implementado |
| XP volta para início do nível atual | ✅ Implementado |
| Morte dispara redistribuição de inimigos | ✅ Evento publicado (CaveEnemiesRedistributionRequestedEvent) |
| Save/load preserva corpse ativo | ✅ Implementado (CorpseSaveData) |
| Morte fora da cave sem penalidade | ✅ CaveDeathResolver.IsDeathInCave() diferencia contexto |

## Pendentes (fora do escopo MVP)

- Play Mode humano validação completa do fluxo
- Corpse spawning em safe anchor exato (materialização no cave level correto)
- Recovery modal UI polish (spec 17)
- Balance tuning de penalidades de morte

---

## Evidências

- `docs/validation/SPEC15_FINALIZATION_SPEC16_PHASE0_VALIDATION_20260525.md`
- `Assets/_Game/Scripts/Player/Death/**` (23 arquivos)
- `Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs`
- `Assets/_Game/Scripts/UI/Death/**`
- `Assets/_Game/Scripts/UI/Locations/**`
