# SPEC — Loadout completo e guia operacional do playtest humano

> **Spec ID:** `spec_playtest_complete_loadout_and_guide_v1`
> **Status:** A implementar
> **Wave:** WAVE VALIDATION — Smoke humano final
> **Priority:** P1
> **Type:** Tooling / Validation / Docs
> **Domain:** Inventory / Equipment / Farm / Validation
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** specs que editem `DebugLoadoutProvisioner`, `RepairPlayerStartingItems` ou `PLAYTEST_SIMPLES.md`
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs`, `Assets/_Game/Tests/EditMode/Editor/**`, `docs/validation/playmode/PLAYTEST_SIMPLES.md`
> **Depends on:** `docs/project/HANDOFF_CODEX_2026-08-12.md`; `docs/project/CURRENT_STATE.md`; `WAVE_INTEGRATION_06A_DEBUG_LOADOUT_ITEM_USE_AUDIT.md`
> **Blocks:** smoke humano e baixa das specs `PLAYTEST_ONLY`
> **Scope:** corrigir o provisionador existente para entregar/equipar os itens reais necessários ao smoke e transformar `PLAYTEST_SIMPLES.md` em roteiro executável.
> **Out of scope:** arte; balance; novas ferramentas; mudanças de save; cenas/prefabs; dívida de `WeaponId` das picaretas; `ProjectilePrefab` da Fireball.
> **Validation level alvo:** BUILD_VALIDATED; Play Mode humano pendente e explicitamente guiado.
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto
O smoke humano é o gate para aproximadamente 29 specs. O usuário iniciou o teste e não encontrou uma enxada utilizável. O tooling atual já provisiona itens em Play Mode, mas contém IDs legados inexistentes e não põe a enxada canônica na hotbar.

## 6. Problema
`DebugLoadoutProvisioner.SmokeLoadout` usa `item_tool_hoe_basic` e `item_tool_watering_can_basic`, ausentes do catálogo atual. Os IDs reais são `item_shop_tool_hoe_basic` e `item_shop_tool_watering_can_basic`; a enxada real aparece apenas como fallback sem hotbar. Assim, um save existente pode continuar sem as ferramentas necessárias e o roteiro não informa menu, teclas, slot, interação ou pré-condições.

## 7. Objetivo
Ao final, executar uma vez `Provision Farm Smoke Loadout` em Play Mode entrega um conjunto conhecido e suficiente para o roteiro, associa as ferramentas essenciais à hotbar, equipa a enxada e imprime sucesso/falhas de forma verificável; o guia informa exatamente como executar e reportar cada passo.

## 8. Fontes obrigatórias lidas
- `AGENTS.md`
- `docs/project/HANDOFF_CODEX_2026-08-12.md`
- `docs/project/CURRENT_STATE.md`
- `.agents/skills/spec-authoring/SKILL.md`
- `.agents/skills/system-reuse-audit/SKILL.md`
- `.claude/rules/testing-quality-gate.md`
- `.claude/skills/spec-execution/SKILL.md`
- `Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs`
- `Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- `Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs`
- `docs/validation/playmode/PLAYTEST_SIMPLES.md`
- `docs/validation/WAVE_INTEGRATION_06A_DEBUG_LOADOUT_ITEM_USE_AUDIT.md`

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-12)
```text
git branch --show-current -> dev
git status --short -> somente ?? .codex/config.toml.bak-20260812-1934 (fora do escopo)
rg DebugLoadoutProvisioner -> único provisionador existente; verdict: REUSE/EXTEND
Assets de item existentes:
  Item_Shop_Hoe_Basic.asset -> Id item_shop_tool_hoe_basic; IsEquippable=1
  Item_Shop_Watering_Can_Basic.asset -> Id item_shop_tool_watering_can_basic; IsEquippable=1
  Item_Cana_Basica.asset -> Id item_tool_fishing_rod_basic; IsEquippable=1
  item_tool_pickaxe_iron.asset e item_tool_pickaxe_steel.asset -> IsEquippable=1
DebugLoadoutProvisioner atual:
  hotbar 0..3 usa quatro IDs legados possivelmente desconhecidos;
  item_shop_tool_hoe_basic existe, mas HotbarSlot=-1;
  TryEquipFirstKnownTool equipa a primeira hoe conhecida.
RepairPlayerStartingItems já garante hoe/regador para jogo novo, mas não corrige saves existentes.
PLAYTEST_SIMPLES.md tem 11 tópicos, sem setup detalhado, teclas, slots ou itens por etapa.
```
A Fase 0 de execução deve repetir os greps de IDs e confirmar que nenhum ID canônico mudou.

## 13. Regras de não duplicação
- Não criar manager, service, menu ou segundo provisionador: estender `DebugLoadoutProvisioner`.
- Não criar novos itens de ferramenta: reutilizar os `ItemDataSO` existentes.
- Não alterar `EquipmentManager`, `FarmTillingInputController`, inventário ou hotbar runtime.
- Não editar `.asset` manualmente nem gerar cenas.

## 15. Arquitetura alvo — classes a CRIAR vs MODIFICAR
```text
CRIAR:
  Assets/_Game/Tests/EditMode/Editor/DebugLoadoutDefinitionTests.cs
    — valida estaticamente IDs, quantidades, slots e ordem de equipamento do kit.

MODIFICAR:
  Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs
    — substituir aliases mortos por IDs reais, expor definição pura/testável e emitir resumo binário.
  docs/validation/playmode/PLAYTEST_SIMPLES.md
    — adicionar setup, controles, item/slot requerido, ações, resultado e reporte por etapa.
```

## 16. Contratos, dados e eventos

### 16.1 Tooling contracts
```csharp
internal readonly struct DebugLoadoutEntry
{
    public string Id { get; }
    public int Amount { get; }
    public int HotbarSlot { get; }
    public ToolType ToolType { get; }
}

internal static IReadOnlyList<DebugLoadoutEntry> SmokeLoadout { get; }
internal static bool TryValidateSmokeLoadout(out string failureReason);
public static void ProvisionSmokeLoadout();
```
`TryValidateSmokeLoadout` deve ser puro e rejeitar: ID vazio, quantidade menor que 1, slot duplicado/fora de 0..5 e ausência de Hoe/WateringCan/Pickaxe/FishingRod, sementes, arma, munição, consumível e materiais.

### 16.2 Runtime contracts
N/A — tooling Editor reutiliza `InventoryManager`, `HotbarState` e `EquipmentManager.EquipTool` existentes.

### 16.3 Event contracts
N/A — nenhuma comunicação de gameplay nova.

### 16.4 Save contracts
N/A — o provisionador altera apenas o estado runtime já persistível pelos providers existentes.

### 16.5 UI/docs contract
Cada etapa do guia deve conter: `Pré-condição`, `Item/slot`, `Ação exata`, `Esperado`, `Se falhar`, `Resultado: [ ] OK [ ] FALHOU`.

## 17. Sistemas afetados
Editor QA tooling / Inventory / Hotbar / Equipment / Farm smoke / documentação de Play Mode / EditMode tests.

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs
Assets/_Game/Tests/EditMode/Editor/DebugLoadoutDefinitionTests.cs
docs/validation/playmode/PLAYTEST_SIMPLES.md
docs/validation/spec_playtest_complete_loadout_and_guide_v1_execution_report.md
.specs/a_implementar/spec_playtest_complete_loadout_and_guide_v1.md
```

## 19. Arquivos proibidos
```text
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Farm/**
Packages/**
ProjectSettings/**
docs_old/**
```

# /speckit.plan

## 20. Estratégia de implementação

### Fase 0 — Reconfirmação
Repetir a busca dos sete IDs canônicos no catálogo/assets, reler `DebugLoadoutProvisioner` e confirmar o único sistema existente.

### Fase 1 — Definição testável do kit
Em `DebugLoadoutProvisioner`, substituir a tupla privada por `DebugLoadoutEntry`; usar os IDs canônicos reais. Reservar slots: 1 enxada, 2 regador, 3 picareta de ferro, 4 vara de pesca, 5 arma, 6 sementes (índices 0..5). Incluir materiais e consumíveis fora da hotbar. `TryValidateSmokeLoadout` valida a definição sem acessar Unity runtime.

### Fase 2 — Provisionamento e equipamento
Em `ProvisionSmokeLoadout`, validar o contrato antes de obter managers. Adicionar cada item conhecido, associar a hotbar mesmo quando o item já existia e equipar explicitamente a enxada canônica ao final. Se um item obrigatório for desconhecido/não adicionado, o resumo deve imprimir `REQUIRED FAILURES (N)`; sucesso completo imprime `READY FOR PLAYTEST`.

Pseudo-código:
```text
validate definition or log error + return
for each entry:
  if unknown -> requiredFailures += id; continue
  add item or accept pre-existing
  if present and slot >= 0 -> bind hotbar
equip canonical hoe only when inventory contains it
log READY iff requiredFailures == 0, otherwise REQUIRED FAILURES (N)
```

### Fase 3 — Testes
Criar `DebugLoadoutDefinitionTests` seguindo `(skill: editmode-test-authoring)`:
- `SmokeLoadout_ContainsCanonicalFarmToolsInExpectedSlots`: hoe=0, watering=1, pickaxe=2, fishing=3.
- `SmokeLoadout_ContainsPlantingAndCombatRequirements`: sementes, arma, munição, consumível e materiais presentes.
- `SmokeLoadout_HasUniqueValidHotbarSlotsAndPositiveAmounts`: valida retorno verdadeiro e `failureReason` vazio.
- `SmokeLoadout_DoesNotContainKnownLegacyAliases`: nenhum `item_tool_hoe_basic`, `item_tool_watering_can_basic`, `item_wood`, `item_stone`.

### Fase 4 — Guia
Reescrever `PLAYTEST_SIMPLES.md` preservando os 11 resultados funcionais, precedidos por setup obrigatório. Especificar como provisionar, selecionar hotbar 1..6, trocar/equipar ferramentas, plantar/regar, navegar e capturar erros. Não declarar Play Mode como executado.

### Fase 5 — Validação e relatório
Compilar `CindarsHope.Editor.csproj` e `CindarsHope.Tests.EditMode.csproj` com restore; executar testes Unity somente se o Editor estiver fechado. Registrar comandos, exit codes e risco residual humano.

## 21. Ordem de execucao segura
1. Reconfirmar Phase 0. 2. Editar provisionador. 3. Criar testes. 4. Atualizar guia. 5. Builds modulares. 6. EditMode se seguro. 7. Relatório.

## 14. Critérios de aceite — binários com Definition of Done

### 14.1 Ferramentas reais e hotbar determinística
- Resultado: kit contém hoe/regador/picareta/vara reais nos índices 0/1/2/3 e não contém aliases mortos conhecidos.
- DoD: `DebugLoadoutDefinitionTests.SmokeLoadout_ContainsCanonicalFarmToolsInExpectedSlots` e `SmokeLoadout_DoesNotContainKnownLegacyAliases` passam; saída NUnit contém `Passed` para ambos.

### 14.2 Kit cobre todas as categorias do smoke
- Resultado: kit contém sementes, ferramenta de plantio/rega/mineração/pesca, arma, munição, cura/comida e materiais de crafting/venda.
- DoD: `SmokeLoadout_ContainsPlantingAndCombatRequirements` passa; saída NUnit contém `Passed`.

### 14.3 Provisionador informa prontidão real
- Resultado: nenhuma falha obrigatória produz falso sucesso; kit completo imprime `READY FOR PLAYTEST`.
- DoD: grep em `DebugLoadoutProvisioner.cs` encontra literalmente `READY FOR PLAYTEST` e `REQUIRED FAILURES`; build de `CindarsHope.Editor.csproj` retorna exit code 0.

### 14.4 Guia é executável sem conhecimento implícito
- Resultado: setup e cada um dos 11 testes contêm ação e esperado; fazenda identifica hoe, regador, sementes e slots.
- DoD: inspeção de `PLAYTEST_SIMPLES.md` encontra as seis labels do contrato e onze campos `Resultado:`; relatório registra contagem `11/11`.

### 14.5 Compilação modular
- Resultado: Editor e Tests.EditMode compilam.
- DoD: `dotnet build CindarsHope.Editor.csproj` e `dotnet build CindarsHope.Tests.EditMode.csproj` retornam exit code 0 e `0 Error(s)`.

## 22. Validação e gates
- `dotnet build CindarsHope.Editor.csproj` com restore — exit 0 obrigatório.
- `dotnet build CindarsHope.Tests.EditMode.csproj` com restore — exit 0 obrigatório.
- Se Unity Editor estiver fechado: `tools/unity/RunUnityEditModeTests.ps1` via `Start-Process -Wait`; resultado exit 0 e testes nomeados verdes.
- Play Mode: `NOT RUN` até o humano executar o novo guia e reportar `OK/FALHOU`.

## 23. Edge cases / falhas
- Save já contém o item: não depender de `AddItem`; associar hotbar se `HasItem` for verdadeiro.
- Inventário cheio: listar o item obrigatório em `REQUIRED FAILURES`, sem imprimir prontidão falsa.
- ID removido do catálogo: `IsKnownItem` falha e o resumo aponta o ID exato.
- `SaveManager.HotbarState` nulo: adicionar itens, registrar falha de hotbar e não declarar kit pronto.
- `EquipmentManager` nulo: registrar falha de equipamento e não declarar a hoe equipada.
- Slot duplicado ou fora de faixa: falhar em `TryValidateSmokeLoadout` antes de mutar o inventário.
- Unity Editor aberto: não iniciar batchmode; registrar EditMode como `NOT RUN` com risco residual.

# /speckit.tasks

## Depende de

- `docs/project/HANDOFF_CODEX_2026-08-12.md` e `docs/project/CURRENT_STATE.md`.
- Provisionador, inventário, hotbar e equipamento existentes, auditados na §9.

## Bloqueia

- Execução confiável do playtest humano e baixa das specs `PLAYTEST_ONLY`.

## Tasks

1. Reconfirmar os IDs e o sistema existente.
2. Corrigir e validar a definição do loadout.
3. Provisionar hotbar e equipar a enxada com falha explícita.
4. Adicionar testes EditMode da definição.
5. Atualizar o guia de 11 etapas.
6. Executar validações e registrar evidência honesta.
