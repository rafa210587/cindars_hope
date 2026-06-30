# SPEC — Machado de Ferro no Inventário Inicial (testabilidade do arquétipo Heavy)

> **Spec ID:** `fable_86_spec_starting_iron_axe_for_heavy_testing`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Animação & Combat Feel
> **Priority:** P2 (testabilidade / suporte à validação Play Mode da fable_84/85)
> **Type:** Tooling (editor) + Data (starting inventory)
> **Domain:** Player / Combat / Tooling
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem `RepairPlayerStartingItems.cs`, `CindarsHopeMenu.cs` ou `Data/Config/PlayerData.asset`
> **Must not run with:** qualquer spec que altere o inventário inicial, o orquestrador de menu `CindarsHope/Inicializar Projeto`, ou `PlayerData.asset`
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs`, `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`, `Assets/_Game/Data/Config/PlayerData.asset`
> **Depends on:**
> - `fable_84` — `WeaponAttackArchetypeMapper.FromWeaponType(WeaponType.Axe) == Heavy` (para o ataque acionar o arquétipo Heavy).
> - `fable_85` — arte do Heavy em `Resources/PlayerSprites/attack_heavy/` (para a animação aparecer; sem ela, o ataque cai em fallback Sword — ainda assim o item é equipável).
> **Blocks:** N/A (habilitador de teste; não bloqueia outras specs).
> **Scope:** Adicionar **1× Machado de Ferro** (`item_weapon_axe_iron`) ao inventário inicial do player via um método idempotente `RepairPlayerStartingItems.EnsureStartingAxe()` registrado como `RunStep` no comando `CindarsHope/Inicializar Projeto`, para que o humano consiga equipar um machado e validar a animação Heavy em Play Mode.
> **Out of scope:** loja vendendo machado; tabela de loot/drop; comando de debug "dar item" genérico; outras armas/arquétipos; balance de stats do machado; qualquer mudança de runtime de gameplay.

---

## 5. Contexto

A `fable_84` (animação por arquétipo) e a `fable_85` (arte do Heavy) tornam o golpe de machado animável em jogo: equipar uma arma `Axe`/`Hammer` deve tocar a animação Heavy. Para **validar isso em Play Mode**, o humano precisa de um machado **equipável** na mão do player. A cadeia item→weapon→arquétipo já existe e está correta:

- `Assets/_Game/Data/Items/item_weapon_axe_iron.asset` — `Id: item_weapon_axe_iron`, `IsEquippable: 1`, `Category: Weapon`, `WeaponId: weapon_axe_iron`.
- `Assets/_Game/Data/Combat/Weapons/weapon_axe_iron.asset` — `Type: Axe`.
- `EquippedItemResolver` resolve `item_weapon_axe_iron` → `weapon_axe_iron` (`WeaponDataSO`, Axe) → `WeaponAttackArchetypeMapper` → `Heavy`.

O único gap: **o machado não está no inventário inicial** e não há comando de debug para dar itens. O `RepairPlayerStartingItems` já garante outros itens iniciais (Lágrima da Deusa, Flechas, Arco, Enxada, Regador, TestStarterKit) via o padrão idempotente `EnsureStartingItem(id, amount)`, chamado pelo `CindarsHope/Inicializar Projeto`. Basta estender esse padrão.

### Estado atual do repo

| Artefato | Estado |
|---|---|
| `item_weapon_axe_iron.asset` | EXISTE (equipável, ligado a weapon_axe_iron). |
| `weapon_axe_iron.asset` (Type Axe) | EXISTE. |
| `RepairPlayerStartingItems.cs` | EXISTE. Método helper `EnsureStartingItem(itemId, amount)` idempotente. NÃO tem método/const para machado. |
| `CindarsHopeMenu.cs` (`InicializarProjeto`) | EXISTE. Registra os `EnsureStarting*` como `RunStep`. NÃO registra machado. |
| `PlayerData.asset` (`StartingItems`) | EXISTE. NÃO contém o machado. É mutado por editor code (SerializedObject/SetDirty), nunca à mão. |

---

## 6. Problema

Sem um machado no inventário inicial (e sem comando de dar-item), não há como o humano equipar um `Axe` em Play Mode para validar a animação Heavy entregue pela fable_84/85. O trabalho de código + arte do Heavy fica **não testável manualmente**, travando a validação final humana do lote.

---

## 7. Objetivo

Ao final desta spec, rodar `CindarsHope/Inicializar Projeto` (ou `Reparar e Reconstruir`) garante **1× Machado de Ferro** no inventário inicial do player, de forma idempotente, permitindo equipá-lo (painel de equipamento, tecla `L`) e atacar (`E`/`Q`) para validar a animação Heavy — sem tocar `.unity`/`.prefab`/`.asset` à mão, sem alterar runtime de gameplay e sem duplicar nenhum item já garantido.

---

## 8. Fontes obrigatórias lidas

```text
.specs/a_implementar/fable/fable_84_spec_player_attack_anim_archetype.md
.specs/a_implementar/fable/fable_85_spec_player_heavy_archetype_art_integration.md
Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs
Assets/_Game/Data/Items/item_weapon_axe_iron.asset   (apenas confirmar id/IsEquippable/WeaponId)
.claude/rules/editor-generation-orchestration.md
.claude/rules/unity-assets.md
```

---

## 9. User stories / engineering stories

- Como humano testador, quero começar com 1 machado no inventário para equipar e validar a animação Heavy em Play Mode.
- Como `RepairPlayerStartingItems`, quero um `EnsureStartingAxe()` no mesmo padrão idempotente dos outros itens iniciais.
- Como orquestrador `Inicializar Projeto`, quero registrar a garantia do machado como mais um `RunStep` best-effort.

---

## 10. Escopo

Inclui:

- Constantes `IronAxeItemId = "item_weapon_axe_iron"` e `IronAxeStartingAmount = 1` em `RepairPlayerStartingItems.cs`, seguindo o padrão das consts existentes (ex.: WoodBow).
- Método `public static void EnsureStartingAxe()` que chama `EnsureStartingItem(IronAxeItemId, IronAxeStartingAmount)`.
- Um `RunStep("Garantir Machado de Ferro (1x) no inventario inicial", () => RepairPlayerStartingItems.EnsureStartingAxe())` registrado em `CindarsHopeMenu.InicializarProjeto()` junto dos outros `EnsureStarting*` (respeitar o namespace real lido no arquivo).
- Confirmar via `dotnet build Assembly-CSharp-Editor.csproj` exit 0.
- Execution report `docs/validation/fable_86_execution_report.md`.

---

## 11. Fora de escopo

Não inclui:

- Adicionar machado a loja/loot/baú.
- Comando de debug genérico de dar item.
- Outras armas (Hammer/Spear/Dagger/Staff/Wand) no inventário inicial.
- Mudança de stats/balance do machado.
- Qualquer runtime de gameplay, save schema, cena, prefab ou evento.

---

## 12. Regras de não duplicação

- Reusar `EnsureStartingItem(id, amount)` existente — não criar novo mecanismo de starting inventory.
- Registrar exatamente **um** `RunStep` para o machado; não duplicar nem criar `[MenuItem]` avulso (rule editor-generation-orchestration: tudo via os 3 comandos canônicos).
- Não criar novo item/weapon de machado — `item_weapon_axe_iron` já existe.
- Não mutar `PlayerData.asset` à mão; usar o caminho editor existente (`EnsureStartingItem`).

---

## 13. Critérios de aceite

### 13.1 Método e consts adicionados

- `RepairPlayerStartingItems.cs` contém `IronAxeItemId = "item_weapon_axe_iron"`, `IronAxeStartingAmount = 1` e `EnsureStartingAxe()` chamando `EnsureStartingItem(...)`, no estilo dos métodos vizinhos.

### 13.2 Registrado no Inicializar Projeto

- `CindarsHopeMenu.InicializarProjeto()` tem um `RunStep` que invoca `EnsureStartingAxe()`, ao lado dos outros `EnsureStarting*`.
- Nenhum `[MenuItem]` avulso novo (só o `RunStep`).

### 13.3 Build

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` exit 0 (é editor code).
- `dotnet build Assembly-CSharp.csproj --no-restore` exit 0 (não deve regredir o runtime).

### 13.4 Resultado em jogo (Play Mode — final)

- Após `Inicializar Projeto` (ou `Reparar e Reconstruir`) no Unity, o player começa com 1× Iron Axe no inventário.
- Equipar (tecla `L` → Mão direita → Equipar → Iron Axe) e atacar (`E`) → animação Heavy toca (com a fable_85 importada) ou fallback Sword (sem a arte) — mas o item é equipável e o arquétipo publicado é `Heavy`.
- Idempotência: rodar `Inicializar Projeto` duas vezes não duplica o machado (1×, não 2×).

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs   ← alterar (const + método)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs                        ← alterar (1 RunStep)
docs/validation/fable_86_execution_report.md                          ← criar
```

`Assets/_Game/Data/Config/PlayerData.asset` é mutado **apenas** pela execução editor de `EnsureStartingItem` (humano roda `Inicializar Projeto` no Unity), nunca editado à mão por esta spec.

## 15. Arquivos proibidos

```text
Assets/**/*.unity        ← proibido
Assets/**/*.prefab       ← proibido
Assets/**/*.asset (edição manual de YAML) ← proibido (PlayerData.asset só via EnsureStartingItem rodado no Unity)
Assets/_Game/Scripts/Combat/**   ← proibido (runtime de combate não muda)
Assets/_Game/Scripts/Player/**   ← proibido
Packages/**, ProjectSettings/**, docs_old/**, docs/archive/**
```

---

## 16. Estratégia de implementação

```text
Fase 0 — Ler RepairPlayerStartingItems.cs (padrão das consts/métodos e namespace real) e CindarsHopeMenu.cs (onde os EnsureStarting* viram RunStep). Confirmar item_weapon_axe_iron existe e IsEquippable.
Fase 1 — Adicionar consts + EnsureStartingAxe() em RepairPlayerStartingItems.cs.
Fase 2 — Registrar o RunStep em CindarsHopeMenu.InicializarProjeto() (namespace correto).
Fase 3 — dotnet build Editor + runtime, exit 0 ambos.
Fase 4 — Execution report. A execução do menu (que muta PlayerData.asset) + Play Mode = DEFERRED_TO_FINAL_VALIDATION (humano no Unity).
```

---

## 17. Impacto em save/load, eventos, UI

```text
Save schema: NO   | Save section: NO   | Migration: NO   | Persiste Unity ref: NO
Eventos: NO (nenhum novo/alterado)
UI: NO (painel de equipamento existente já cobre equipar)
Cenas: NO | Prefabs: NO | ScriptableObjects via código: PlayerData.asset (via EnsureStartingItem, rodado no Unity)
Requires Play Mode final validation: YES | Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

---

## 18. Riscos e rollback

| Risco | Mitigação |
|---|---|
| Duplicar o machado em runs repetidas | `EnsureStartingItem` já é idempotente (checa presença antes de adicionar) — reusar, não reimplementar. |
| Namespace errado no RunStep | Ler o namespace real de `RepairPlayerStartingItems` no arquivo antes de registrar. |
| Mutação de `PlayerData.asset` fora do Unity | Não mutar à mão; só via `Inicializar Projeto` no Unity (deferido ao humano). |

**Rollback:** remover as consts + `EnsureStartingAxe()` e o `RunStep`; re-rodar `Inicializar Projeto`/`Reparar` não removerá o machado já adicionado de um `PlayerData.asset` existente (rollback total exige remover o item do array via editor) — risco baixo, item de teste.

---

## 19. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
dotnet build .\Assembly-CSharp.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity / Play Mode: DEFERRED_TO_FINAL_VALIDATION (rodar `Inicializar Projeto` e validar o equip + ataque). Se Unity bloqueado, registrar `NOT RUN` com motivo.

---

## 20. Testing Quality Gate

```text
Changed deterministic logic: NO (apenas registro de 1 item inicial via helper existente)
Requires EditMode tests: NO (sem lógica nova; EnsureStartingItem já existe e é idempotente)
Requires PlayMode automated or final human scenario: YES (equipar + atacar)
Requires regression test: NO (não altera runtime)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Minimum validation evidence for ACCEPTED:
  - dotnet build Editor + runtime exit 0
  - Play Mode humano: player começa com 1 machado, equipa e ataca, arquétipo Heavy publicado (animação Heavy com a fable_85, ou fallback Sword sem a arte)
```

---

## 21. Anti-regressão

```text
- Não duplicar itens iniciais já garantidos (Lágrima, Flechas, Arco, Enxada, Regador, TestStarterKit).
- Não criar [MenuItem] avulso (tudo via os 3 comandos canônicos — rule editor-generation-orchestration).
- Não alterar runtime de combate/player.
- Não editar PlayerData.asset à mão.
- Idempotência preservada: 1 machado, não N.
```

---

## 22. Notas para execução posterior

- Quando os outros arquétipos tiverem arte (`thrust`/`dagger`/`cast`), o mesmo padrão pode dar Spear/Dagger/Staff iniciais para teste — porém isso é decisão de balance/onboarding, não desta spec (que é só testabilidade do Heavy).
- Um comando de debug genérico "dar item" (sob `CindarsHope/Dev/`) seria mais escalável para testar qualquer arma, mas está fora de escopo aqui (maior superfície). Registrado como possível follow-up.
