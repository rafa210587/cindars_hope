# SPEC - Farm irrigacao, solo e planting UI

> Spec ID: spec_farm_irrigacao_solo_planting_ui
> Status: Implementado parcial
> Ordem de execucao: 04
> Depende de: 00, 01, 02, 03
> Bloqueia: 05, 09, 17
> Tipo: Runtime/UI
> Fonte: .specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar estados de solo, irrigacao, hoe/watering can, crescimento condicionado por agua, save/load e menu contextual agricola por tile.
> Fora de escopo: clima/chuva real, fertilizante, sazonalidade, stamina final, planting menu global, UI final completa do jogo, automacao/sprinklers, Packages, ProjectSettings e docs_old.
> Evidencia: `Assets/_Game/Scripts/Farm/FarmPlot.cs`, `Assets/_Game/Scripts/Farm/FarmPlotState.cs`, `Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs`, `Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs`

## Resultado da implementacao 2026-05-24

Implementado parcial:

- `FarmPlotState` agora cobre `Raw`, `TilledDry`, `TilledWet`, `PlantedDry`, `PlantedWet`, `ReadyToHarvest`, `Blocked` e `Dead`.
- `FarmPlotSaveData` persiste estado, seed, progresso, agua, regrow e dia.
- `SeedDataSO` recebeu `RequiresWater`, `RegrowDays` e `SeasonTags` como campos seguros/futuros.
- `FarmPlot` abre menu contextual vertical por `E`, navega por `W/S`, confirma por `E`/`Enter`/`Space` e fecha por `Esc`.
- Acoes implementadas: arar, molhar, plantar seed do inventory e colher.
- Plantio revalida estado, seed e inventory; a seed so e consumida apos validacao de sucesso.
- Crescimento so avanca em `PlantedWet`; agua reseta para seco no avanco de dia.
- Colheita sem regrow retorna para `TilledDry`; com `RegrowDays > 0` retorna para `PlantedDry`.
- Movimento e interacao sao ignorados enquanto o menu agricola esta aberto.

Pendencias reais:

- UI ainda e IMGUI/minima, nao Canvas final.
- Play Mode manual completo e validacao visual ainda pendentes.
- Stamina/custo de acao fica para spec 09.

Fontes absorvidas:
- specs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_farm_irrigacao_solo_planting_ui.md

---

# /speckit.specify

## Contexto

O farm MVP ja possui `FarmPlot`, `FarmPlotRegistry`, `SeedDataSO`, plantio simples, crescimento por dias e colheita via harvest data. Esta spec adiciona irrigacao, estados de solo, uso de hoe/watering can e menu contextual agricola sem quebrar o loop atual.

A spec 03 de Inventory define slots, painel de itens e inventory save v2. Esta spec deve consumir seeds a partir do inventory, sem exigir que a seed esteja na mao/hotbar.

## Pre-condicoes

Implementar runtime somente se as specs 02 e 03 estiverem realmente implementadas. Se save migration ou inventory slots ainda nao existirem, registrar bloqueio e nao implementar runtime desta spec.

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Farm/FarmPlotRegistry.cs
Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Save/**
```

## Problema

O fluxo atual ainda nao suporta:

- solo cru/arado/seco/molhado/plantado/pronto;
- irrigacao;
- plantio por lista de seeds do inventory;
- menu contextual no tile;
- save/load completo de estado do solo, agua, seed e progresso;
- crescimento condicionado por agua.

## Objetivo

Implementar um fluxo agricola jogavel baseado em estados de plot, menu contextual vertical acima do tile e acoes validas por estado: arar, molhar, plantar seeds disponiveis no inventory e colher.

## Decisoes aprovadas

- Plantio nao exige seed na mao.
- `E` em tile agricola abre menu contextual pequeno acima do tile.
- Menu e vertical, usa `W/S` para navegar, `E`/`Enter`/`Space` para confirmar e `Esc` para cancelar.
- Seeds listadas no menu vêm do inventory atual.
- Seed so e consumida depois de plantio validado e executado com sucesso.
- Sem agua, planta nao cresce no MVP, mas tambem nao morre.
- Agua reseta depois de aplicar crescimento do dia.
- Colheita normal retorna plot para `TilledDry`.
- `RegrowDays` opcional entra agora.
- `StageSprites` opcional entra agora com fallback visual atual.
- Clima/chuva, fertilizante, sazonalidade e stamina final ficam fora do MVP.

## Estados de plot

Usar ou equivalente:

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

- `Raw`: pode receber Hoe e virar `TilledDry`.
- `TilledDry`: pode receber Watering Can ou seed.
- `TilledWet`: pode receber seed.
- `PlantedDry`: pode receber Watering Can.
- `PlantedWet`: pode progredir no avanco do dia.
- `ReadyToHarvest`: pode ser colhido.
- `Dead`: reservado para futuro; nao e produzido por falta de agua no MVP.

## Menu contextual agricola

Opcoes por estado:

```text
Raw:
- Arar solo, se Hoe disponivel.

TilledDry:
- Molhar solo, se Watering Can disponivel.
- Plantar <SeedName> para cada seed disponivel no inventory.

TilledWet:
- Plantar <SeedName> para cada seed disponivel no inventory.

PlantedDry:
- Molhar solo, se Watering Can disponivel.

PlantedWet:
- Opcional: mostrar status "Ja irrigado".

ReadyToHarvest:
- Colher.

Blocked:
- Feedback de bloqueado ou nenhuma acao.
```

Se nao houver acao valida, exibir feedback simples em vez de abrir menu vazio.

Enquanto o menu estiver aberto, movimento/interacao/ataque/tool use do player nao devem disparar em paralelo. Ao fechar o menu, input normal volta.

## Acoes

### Arar

```text
Raw -> TilledDry
```

Exige Hoe disponivel conforme sistema atual de tools.

### Molhar

```text
TilledDry -> TilledWet
PlantedDry -> PlantedWet
```

Exige Watering Can disponivel conforme sistema atual de tools.

### Plantar

Valido em `TilledDry` ou `TilledWet`.

Fluxo obrigatorio:

```text
Revalidar estado do plot
Revalidar seed no inventory
Validar SeedDataSO
Aplicar estado plantado
Consumir 1 seed somente apos sucesso
```

Resultado:

```text
TilledDry + seed -> PlantedDry
TilledWet + seed -> PlantedWet
```

Se qualquer passo falhar, seed permanece no inventory e plot nao muda.

### Colher

Valido em `ReadyToHarvest`.

```text
Sem regrow: ReadyToHarvest -> TilledDry
Com RegrowDays > 0: ReadyToHarvest -> PlantedDry com regrow reiniciado
```

## Crescimento

Ordem no avanco do dia:

```text
1. PlantedWet aplica progresso.
2. Se completou crescimento, vira ReadyToHarvest.
3. Plots molhados restantes resetam para estado seco equivalente.
4. Estado final e persistido.
```

`PlantedDry` nao cresce e nao morre no MVP.

## SeedDataSO

Expandir com campos opcionais/necessarios:

```text
GrowthStages ou DaysToGrow
StageSprites opcional
RequiresWater default true
RegrowDays opcional
SeasonTags futuro
```

Se `StageSprites` estiver vazio, usar fallback visual atual. Seeds existentes nao devem quebrar por campos novos vazios/default.

## Save/load

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

Nao serializar `SeedDataSO`, `Sprite`, `GameObject`, `Transform`, `MonoBehaviour`, `Collider` ou `Rigidbody`.

Se `SeedId` nao existir no database ao carregar, falhar de forma segura e logar erro claro.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- movimento basico do player quando menu contextual esta fechado;
- interacao `E` fora de plots agricolas;
- save/load atual de farm, world, inventory e cave;
- inventory slots/capacity e migration v1->v2 da spec 03;
- consumo seguro de itens do inventory;
- hotbar/HUD existente;
- colheita MVP ja existente;
- eventos existentes `SeedPlantedEvent` e `CropHarvestedEvent`, se existirem;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se uma integracao anterior ainda nao suportar alguma acao, bloquear a acao com feedback/pendencia clara em vez de criar fallback que duplica regra ou perde estado.

## Criterios de aceite

- Plot distingue `Raw`, `TilledDry`, `TilledWet`, `PlantedDry`, `PlantedWet` e `ReadyToHarvest`.
- `E` em plot valido abre menu contextual vertical acima do tile.
- Menu mostra apenas acoes validas para o estado atual.
- `W/S` navegam no menu.
- `E`, `Enter` ou `Space` confirmam acao.
- `Esc` cancela/fecha.
- Movimento do player nao conflita com menu aberto.
- Arar, molhar, plantar e colher funcionam por estado.
- Plantar consome seed somente apos sucesso.
- Planta molhada cresce no avanco do dia.
- Planta seca nao cresce e nao morre no MVP.
- Colheita normal retorna plot para `TilledDry`.
- Regrow opcional funciona se `RegrowDays > 0`.
- Save/load preserva estado de solo, agua, seed e progresso.
- Visual de stage usa `StageSprites` quando disponivel e fallback quando ausente.
- Nenhum item de invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Farm/Data/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
.specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md
```

Logica de farm deve ficar fora de MonoBehaviour pesado quando possivel. MonoBehaviours fazem ponte Unity/runtime.

## Ordem segura de implementacao

1. Revalidar estado atual e dependencias.
2. Criar/ajustar `FarmPlotState` sem quebrar comportamento MVP.
3. Implementar save/load de novos campos com defaults seguros.
4. Implementar acoes runtime arar/molhar/plantar/colher sem UI nova.
5. Implementar crescimento por agua e reset de agua.
6. Implementar menu contextual agricola.
7. Integrar menu com inventory/tools.
8. Atualizar visual/fallback de stages.
9. Rodar validacoes e atualizar tracking.

## Fluxos

### Abrir menu contextual

```text
Player pressiona E em plot
Resolver plot alvo
Resolver estado atual
Consultar inventory para seeds disponiveis
Consultar tools disponiveis/permitidas
Construir lista de acoes validas
Abrir menu acima do tile
Bloquear input de movimento enquanto menu esta aberto
```

### Confirmar acao

```text
Selecionar acao
Revalidar plot e recursos
Executar acao se ainda valida
Publicar evento quando aplicavel
Fechar menu
Restaurar input do player
```

### Avanco de dia

```text
Para cada plot
Se PlantedWet, aplicar progresso
Se completou crescimento, ReadyToHarvest
Resetar agua dos plots molhados restantes
Salvar estado atualizado
```

## Eventos candidatos

Usar existentes ou criar seguindo padrao `*Event`:

```text
FarmPlotStateChangedEvent
SoilTilledEvent
PlotWateredEvent
SeedPlantedEvent
CropHarvestedEvent
FarmActionMenuOpenedEvent opcional
FarmActionMenuClosedEvent opcional
```

Nao duplicar evento se ja existir equivalente.

## Compatibilidade com specs anteriores

### Inventory

- Seeds sao lidas do inventory por ID.
- Plantio consome 1 seed somente apos sucesso.
- Se inventory nao puder remover seed, plantio falha sem alterar plot.

### Save migration

- Novos campos de plot devem ter defaults seguros em saves existentes.
- Se precisar de migration, usar infraestrutura da spec 02.

### Tools

- Hoe e Watering Can devem usar contratos existentes de tools quando disponiveis.
- Nao criar sistema paralelo de tools.
- Se tool ainda nao existir de forma compativel, bloquear acao e registrar pendencia.

### UI/HUD

- Menu contextual agricola e UI localizada do tile, nao UI final do jogo.
- Nao substituir HUD/hotbar/inventory panel.

## Riscos de regressao

- Menu contextual conflitar com movimento/interacao.
- Seed ser consumida antes de falha de plantio.
- Planta seca crescer por erro de persistencia.
- Save/load perder `SeedId` ou progresso.
- Tools duplicarem regras de stamina antes da spec 09.
- Visual de stage quebrar fallback atual quando `StageSprites` estiver vazio.

## Mitigacao

- Revalidar estado do plot e inventory no momento da confirmacao.
- Consumir seed somente apos validacao de sucesso.
- Bloquear input do player enquanto menu estiver aberto.
- Manter fallback visual atual se sprites novos estiverem ausentes.
- Usar defaults seguros no load.
- Registrar pendencias reais sem marcar como completo.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `FarmPlot`, `FarmPlotRegistry`, `SeedDataSO`, inventory e tools atuais.
- [ ] Confirmar que specs 02 e 03 estao implementadas antes de runtime.
- [ ] Criar/ajustar `FarmPlotState`.
- [ ] Implementar defaults seguros para saves existentes.
- [ ] Implementar estados Raw/TilledDry/TilledWet/PlantedDry/PlantedWet/ReadyToHarvest.
- [ ] Implementar acoes Arar, Molhar, Plantar e Colher.
- [ ] Garantir que Plantar consome seed somente apos sucesso.
- [ ] Criar menu contextual agricola vertical acima do tile.
- [ ] Integrar menu com `E`, `W/S`, `Enter/Space` e `Esc`.
- [ ] Bloquear movimento/interacao enquanto menu esta aberto.
- [ ] Listar seeds disponiveis no inventory como opcoes de plantio.
- [ ] Implementar crescimento condicionado por agua.
- [ ] Implementar reset de agua apos crescimento do dia.
- [ ] Implementar `RegrowDays` opcional.
- [ ] Implementar `StageSprites` opcional com fallback.
- [ ] Persistir estado do plot em save/load.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
.specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Estados de solo/plantio implementados.
- Menu contextual agricola implementado acima do tile.
- Seeds do inventory aparecem como opcoes de plantio.
- Arar/molhar/plantar/colher funcionam por estado.
- Crescimento depende de agua no MVP.
- Save/load preserva estado completo do plot.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Abrir menu com `E` em plot `Raw`.
2. Navegar com `W/S`.
3. Arar `Raw -> TilledDry`.
4. Abrir menu em `TilledDry` e ver `Molhar solo` + seeds do inventory.
5. Molhar `TilledDry -> TilledWet`.
6. Plantar seed do inventory e validar consumo seguro.
7. Avancar dia com planta molhada e validar crescimento.
8. Avancar dia com planta seca e validar que nao cresce nem morre.
9. Colher `ReadyToHarvest` e validar retorno para `TilledDry` ou regrow.
10. Salvar/carregar durante cada estado.
11. Validar que HUD normal nao e deslocada/destruida pelo menu contextual.
12. Validar que movimento do player volta ao fechar menu.

## Criterio para marcar como implementada

Esta spec so pode ser movida para `.specs/implementados/` se:

- todos os criterios de aceite principais forem atendidos;
- as validacoes obrigatorias forem executadas ou impedimento for registrado claramente;
- docs de status, registries, refinements e `PROJECT_LOG.md` forem atualizados;
- nenhuma regressao das specs 02 e 03 for detectada.
