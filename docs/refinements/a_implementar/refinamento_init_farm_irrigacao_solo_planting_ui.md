# refinamento_init_farm_irrigacao_solo_planting_ui

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_farm_irrigacao_solo_planting_ui.md`  
> **Objetivo:** completar o loop de plantio/solo/irrigação/UX agrícola além do MVP atual.

---

## 1. Estado atual

FarmPlot permite plantar seed selecionada na hotbar, crescer por dias e colher via harvest data.

Evidência:

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md
```

---

## 2. Gaps

- Não há irrigação.
- Não há estado de solo preparado/limpo/fértil/seco/molhado.
- Não há uso real de hoe/watering can para plantar/regenerar solo.
- Plantio depende de seed selecionada na hotbar, sem planting menu.
- Visual de crescimento é por cor, não por sprite/stage.
- Não há sazonalidade/clima.
- Não há fertilizante.

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

### Ações

- Hoe prepara solo.
- Watering Can irriga.
- Seed só planta em solo preparado.
- Crescimento só avança se irrigado ou regra de chuva futura.
- Harvest retorna plot para estado configurável.

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
- highlight de tile interagível;
- preview de seed selecionada.

---

## 4. Arquivos prováveis

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
- [ ] Planta só cresce quando condições mínimas forem atendidas.
- [ ] Save/load preserva estado do solo e irrigação.
- [ ] Visual de stage usa dados do seed quando disponível.
- [ ] Prompts e feedback orientam o jogador.

---

## 6. Validação

1. Tentar plantar sem arar: deve bloquear.
2. Arar com hoe: estado vira `TilledDry`.
3. Regar: estado vira `TilledWet`.
4. Plantar seed: estado vira `PlantedWet` ou equivalente.
5. Avançar dia sem água: não cresce ou aplica regra definida.
6. Salvar/carregar durante cada estado.
