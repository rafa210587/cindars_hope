# Execution Report — fable_86 (Machado de Ferro no Inventário Inicial)

> **Spec:** `.specs/a_implementar/fable/fable_86_spec_starting_iron_axe_for_heavy_testing.md`
> **Data:** 2026-06-29
> **Status:** BUILD_VALIDATED — pendente execução do menu no Unity + Play Mode (DEFERRED_TO_FINAL_VALIDATION)

---

## Fase 0 — Auditoria

- `item_weapon_axe_iron.asset` confirmado: equipável, `WeaponId: weapon_axe_iron` (Type Axe). ✅
- `RepairPlayerStartingItems.EnsureStartingItem(id, amount)` confirmado idempotente; namespace real `CindarsHope.EditorTools.Repair`. ✅
- Machado não estava no inventário inicial nem no `TestStarterKit`. ✅

## Fase 1 — Consts + método

`Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs`:
- Adicionadas consts `IronAxeItemId = "item_weapon_axe_iron"`, `IronAxeStartingAmount = 1` (ao lado das consts do WoodBow).
- Adicionado `public static void EnsureStartingAxe()` chamando `EnsureStartingItem(IronAxeItemId, IronAxeStartingAmount)`, no estilo dos métodos vizinhos.

## Fase 2 — RunStep no menu

`Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`:
- Registrado `RunStep("Garantir Machado de Ferro (1x) no inventario inicial", () => RepairPlayerStartingItems.EnsureStartingAxe())` ao lado do arco, **nos dois** comandos canônicos: `Inicializar Projeto` e `Reparar e Reconstruir`.
- Nenhum `[MenuItem]` avulso novo (rule editor-generation-orchestration respeitada).

## Fase 3 — Build (truth-gate, verificado pelo orquestrador)

```text
dotnet build Assembly-CSharp-Editor.csproj --no-restore  → EXIT 0 (0 erros, 3 warnings preexistentes)
dotnet build Assembly-CSharp.csproj --no-restore         → EXIT 0 (0 erros)
EnsureStartingAxe definido em RepairPlayerStartingItems.cs: 1
EnsureStartingAxe registrado em CindarsHopeMenu.cs (Inicializar + Reparar): 2
```

## Fase 4 — Execução do menu + Play Mode

```text
Status: DEFERRED_TO_FINAL_VALIDATION
- Rodar CindarsHope/Inicializar Projeto (ou Reparar e Reconstruir) no Unity → adiciona 1x Iron Axe ao PlayerData.asset (via EnsureStartingItem, SerializedObject — sem editar YAML à mão).
- Play Mode: tecla L (Equipamento) → Mão direita → Equipar → Iron Axe; atacar com E → publica PlayerMeleeSwingEvent com archetype Heavy.
  - Com a fable_85 importada: toca a animação do machado de dois gumes.
  - Sem a arte importada ainda: fallback Sword (sem crash) — mas o item é equipável e o arquétipo é Heavy.
- Idempotência: rodar o menu 2x não duplica o machado.
```

---

## Resultado

```text
Build: PASS (Editor exit 0, runtime exit 0)
Wiring: PASS (EnsureStartingAxe + 2 RunSteps)
Execução do menu / Play Mode: DEFERRED_TO_FINAL_VALIDATION (humano no Unity)
Arquivo proibido alterado: NENHUM (PlayerData.asset só via EnsureStartingItem rodado no Unity)
Status: BUILD_VALIDATED
```
