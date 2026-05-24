# ref_farm_irrigacao_solo_planting_ui

> Status: Implementado parcial
> Spec relacionada: `docs/specs/implementados/spec_farm_004_irrigacao_solo_planting_ui.md`
> Objetivo: completar o loop de plantio/solo/irrigacao/UX agricola alem do MVP atual.

## Resultado da implementacao 2026-05-24

Entregue:

- Estados de solo/plantio expandidos.
- Menu contextual agricola no proprio `FarmPlot`.
- Arar, molhar, plantar via inventory e colher.
- Crescimento condicionado por agua.
- Save/load dos novos campos de plot.
- `WateringCan` integrado ao ciclo debug de tools.

Pendencias:

- UI final Canvas.
- Play Mode manual completo.
- Custos de stamina na spec 09.

---

## 1. Estado atual

FarmPlot permite plantar seed selecionada na hotbar, crescer por dias e colher via harvest data.

Evidencia:

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md
```

---

## 2. Gaps

- Nao ha irrigacao.
- Nao ha estado de solo cru/arado/seco/molhado/plantado/pronto.
- Nao ha uso real de hoe/watering can para preparar e irrigar solo.
- Plantio depende de seed selecionada na hotbar.
- Nao ha menu contextual no tile com opcoes validas.
- Visual de crescimento ainda pode depender de cor/fallback.
- Nao ha save completo de solo, agua, seed e progresso.
- Nao ha sazonalidade/clima.
- Nao ha fertilizante.

---

## 3. Decisoes aprovadas

- Plantio nao exige seed na mao/hotbar.
- Ao pressionar `E` em um plot valido, abrir menu contextual pequeno acima do tile.
- Menu contextual e vertical.
- `W/S` navegam no menu.
- `E`, `Enter` ou `Space` confirmam.
- `Esc` cancela/fecha.
- Enquanto o menu esta aberto, movimento/interacao do player devem ser bloqueados ou ignorados.
- Menu mostra somente acoes validas para o estado atual do plot.
- Seeds listadas no menu vêm do inventory atual.
- Seed so e consumida depois que o plantio for validado e executado com sucesso.
- Sem agua, planta nao cresce no MVP, mas tambem nao morre.
- Agua reseta apos aplicar crescimento do dia.
- Harvest normal retorna plot para `TilledDry`.
- `RegrowDays` opcional entra agora.
- `StageSprites` opcional entra agora com fallback visual atual.
- Fertilizante, clima/chuva e sazonalidade ficam fora do MVP.
- Stamina final fica fora desta spec; apenas hooks opcionais podem ser preparados.

---

## 4. Estados esperados de plot

```text
Blocked
Raw
TilledDry
TilledWet
PlantedDry
PlantedWet
ReadyToHarvest
Dead
```

Regras:

- `Blocked`: tile nao interagivel para farm.
- `Raw`: solo cru, pode receber Hoe.
- `TilledDry`: solo arado seco, pode receber Watering Can ou seed.
- `TilledWet`: solo arado molhado, pode receber seed.
- `PlantedDry`: plantado seco, pode receber Watering Can.
- `PlantedWet`: plantado molhado, elegivel para progresso no fim do dia.
- `ReadyToHarvest`: cultura pronta para colher.
- `Dead`: reservado para futuro; nao precisa ser produzido por falta de agua no MVP.

---

## 5. Menu contextual agricola

Ao pressionar `E` olhando/interagindo com plot valido, abrir `FarmPlotActionMenu` ou equivalente.

Posicionamento:

- pequeno overlay acima do tile alvo;
- vertical;
- nao desloca HUD principal;
- fecha ao confirmar acao, apertar `Esc`, sair de alcance ou apertar `E` novamente se nada for confirmado.

Opcoes por estado:

```text
Raw:
- Arar solo, se Hoe disponivel/permitida.

TilledDry:
- Molhar solo, se Watering Can disponivel/permitida.
- Plantar <SeedName> para cada seed disponivel no inventory.

TilledWet:
- Plantar <SeedName> para cada seed disponivel no inventory.

PlantedDry:
- Molhar solo, se Watering Can disponivel/permitida.

PlantedWet:
- Opcional: mostrar status "Ja irrigado" sem acao obrigatoria.

ReadyToHarvest:
- Colher.

Blocked:
- Nenhuma acao ou feedback de bloqueado.
```

Se nao houver acao valida, exibir feedback simples em vez de abrir menu vazio.

---

## 6. Acoes

### Arar solo

- Valida em `Raw`.
- Exige Hoe disponivel conforme sistema atual de tools.
- Resultado: `Raw -> TilledDry`.

### Molhar solo

- Valida em `TilledDry` e `PlantedDry`.
- Exige Watering Can disponivel conforme sistema atual de tools.
- Resultado:

```text
TilledDry -> TilledWet
PlantedDry -> PlantedWet
```

### Plantar seed

- Valida em `TilledDry` ou `TilledWet`.
- Lista seeds existentes no inventory.
- Nao exige seed na mao/hotbar.
- Ao confirmar `Plantar <SeedName>`:
  1. validar que o plot ainda esta em estado plantavel;
  2. validar que a seed ainda existe no inventory;
  3. validar `SeedDataSO`;
  4. aplicar estado plantado;
  5. consumir 1 seed somente apos sucesso.

Resultado:

```text
TilledDry + seed -> PlantedDry
TilledWet + seed -> PlantedWet
```

### Colher

- Valida em `ReadyToHarvest`.
- Gera harvest conforme `SeedDataSO`/harvest data.
- Se cultura sem regrow: `ReadyToHarvest -> TilledDry`.
- Se cultura com `RegrowDays > 0`: voltar para `PlantedDry` com contador de regrow reiniciado.

---

## 7. Crescimento

Regra MVP:

- planta avanca crescimento no fechamento/avanco do dia somente se estava `PlantedWet` naquele ciclo;
- `PlantedDry` nao avanca;
- falta de agua nao mata planta no MVP;
- apos aplicar crescimento, agua reseta:

```text
TilledWet -> TilledDry
PlantedWet -> PlantedDry, exceto se virou ReadyToHarvest
```

Se a planta completar o ultimo estagio:

```text
PlantedWet -> ReadyToHarvest
```

---

## 8. Dados

Expandir `SeedDataSO`:

```text
GrowthStages ou DaysToGrow
StageSprites opcional
RequiresWater default true
RegrowDays opcional
SeasonTags futuro
```

Regras:

- `StageSprites` e opcional;
- se nao houver sprites por stage, usar fallback visual atual;
- `SeasonTags` fica apenas preparado/futuro, sem regra de bloqueio nesta spec.

---

## 9. Save/load

Persistir por plot usando IDs e tipos simples:

```text
PlotId
State
SeedId
GrowthStage ou GrowthProgressDays
IsWatered ou State molhado/seco
RegrowRemainingDays opcional
LastUpdatedDay opcional
```

Regras:

- save/load deve restaurar solo cru/arado/molhado/plantado/pronto;
- save/load deve restaurar seed plantada e progresso;
- nao serializar `SeedDataSO`, `Sprite`, `GameObject`, `Transform`, `MonoBehaviour`, `Collider` ou `Rigidbody`;
- se `SeedId` nao existir no database ao carregar, plot deve falhar de forma segura e logar erro claro.

---

## 10. Fora de escopo

```text
Clima/chuva real
Fertilizante
Sazonalidade bloqueando plantio
Planting menu global
Stamina final
UI final consolidada do jogo
Automacao/sprinklers
```

Hooks opcionais de custo podem existir, mas custo real de stamina pertence a `spec_hunger_stamina_status_balance`.

---

## 11. Definition of Done

- [ ] Plot distingue `Raw`, `TilledDry`, `TilledWet`, `PlantedDry`, `PlantedWet` e `ReadyToHarvest`.
- [ ] `E` em plot valido abre menu contextual vertical acima do tile.
- [ ] Menu mostra apenas acoes validas para o estado atual.
- [ ] `W/S` navegam no menu.
- [ ] `E`, `Enter` ou `Space` confirmam acao.
- [ ] `Esc` cancela/fecha.
- [ ] Movimento do player nao conflita com menu aberto.
- [ ] `Raw -> Arar solo -> TilledDry`.
- [ ] `TilledDry -> Molhar solo -> TilledWet`.
- [ ] `TilledDry/TilledWet -> Plantar seed do inventory -> PlantedDry/PlantedWet`.
- [ ] Plantar consome seed somente apos sucesso.
- [ ] Planta molhada cresce no avanco do dia.
- [ ] Planta seca nao cresce e nao morre no MVP.
- [ ] Colheita normal retorna plot para `TilledDry`.
- [ ] Regrow opcional funciona se `RegrowDays > 0`.
- [ ] Save/load preserva estado de solo, agua, seed e progresso.
- [ ] Visual de stage usa `StageSprites` quando disponivel e fallback quando ausente.
- [ ] Validacao documental e Unity compile validation registradas.

---

## 12. Validacao

1. Pressionar `E` em `Raw` e ver opcao `Arar solo`.
2. Navegar menu com `W/S`.
3. Confirmar com `E`, `Enter` ou `Space`.
4. Arar: `Raw -> TilledDry`.
5. Pressionar `E` em `TilledDry` e ver `Molhar solo` + seeds do inventory.
6. Molhar: `TilledDry -> TilledWet`.
7. Plantar seed do inventory e validar consumo somente apos sucesso.
8. Avancar dia molhado e validar crescimento.
9. Avancar dia seco e validar que nao cresce nem morre.
10. Colher `ReadyToHarvest` e validar retorno para `TilledDry` ou regrow.
11. Salvar/carregar durante cada estado.
12. Validar que HUD normal nao e deslocada/destruida pelo menu contextual.
13. Validar Unity compile validation e docs validation.
