# refinamento_init_farm_irrigacao_solo_planting_ui

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_farm_irrigacao_solo_planting_ui.md`
> **Objetivo:** completar o loop de plantio/solo/irrigaÃ§Ã£o/UX agrÃ­cola alÃ©m do MVP atual.

---

## 1. Estado atual

FarmPlot permite plantar seed selecionada na hotbar, crescer por dias e colher via harvest data.

EvidÃªncia:

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md
```

---

## 2. Gaps

- NÃ£o hÃ¡ irrigaÃ§Ã£o.
- NÃ£o hÃ¡ estado de solo preparado/limpo/fÃ©rtil/seco/molhado.
- NÃ£o hÃ¡ uso real de hoe/watering can para plantar/regenerar solo.
- Plantio depende de seed selecionada na hotbar, sem planting menu.
- Visual de crescimento Ã© por cor, nÃ£o por sprite/stage.
- NÃ£o hÃ¡ sazonalidade/clima.
- NÃ£o hÃ¡ fertilizante.

---

## 3. Escopo esperado

### Estados de plot

```text
Blocked
EmptyRaw
TilledDry
TilledWet
PlantedDry
PlantedWet
Ready
Dead
```

### AÃ§Ãµes

- Hoe prepara solo.
- Watering Can irriga.
- Seed sÃ³ planta em solo preparado.
- Crescimento sÃ³ avanÃ§a se irrigado ou regra de chuva futura.
- Harvest retorna plot para estado configurÃ¡vel.

### Dados

Expandir `SeedDataSO`:

```text
GrowthStages
StageSprites
RequiresWater
RegrowDays opcional
SeasonTags futuro
```

### UI/UX

- prompt contextual por estado;
- feedback quando tool errada for usada;
- highlight de tile interagÃ­vel;
- preview de seed selecionada.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Farm/FarmPlotState.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
Assets/_Game/Scripts/Tools/ToolUseController.cs
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Definition of Done

- [ ] Plot distingue solo cru, arado, irrigado, plantado e pronto.
- [ ] Hoe e watering can participam do fluxo.
- [ ] Planta sÃ³ cresce quando condiÃ§Ãµes mÃ­nimas forem atendidas.
- [ ] Save/load preserva estado do solo e irrigaÃ§Ã£o.
- [ ] Visual de stage usa dados do seed quando disponÃ­vel.
- [ ] Prompts e feedback orientam o jogador.

---

## 6. ValidaÃ§Ã£o

1. Tentar plantar sem arar: deve bloquear.
2. Arar com hoe: estado vira `TilledDry`.
3. Regar: estado vira `TilledWet`.
4. Plantar seed: estado vira `PlantedWet` ou equivalente.
5. AvanÃ§ar dia sem Ã¡gua: nÃ£o cresce ou aplica regra definida.
6. Salvar/carregar durante cada estado.
