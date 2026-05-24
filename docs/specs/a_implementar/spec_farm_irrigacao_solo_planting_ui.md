# SPEC - Farm irrigacao, solo e planting UI

> Spec ID: spec_farm_irrigacao_solo_planting_ui
> Status: A implementar
> Ordem de execucao: 04
> Depende de: 00, 01, 02, 03
> Bloqueia: 05, 09, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar estados de solo, irrigacao, hoe/watering can, crescimento condicionado por agua, save/load e menu contextual agricola por tile.
> Fora de escopo: clima/chuva real, fertilizante, sazonalidade, stamina final, planting menu global, UI final completa do jogo e docs_old.

Fontes absorvidas:
- specs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_farm_irrigacao_solo_planting_ui.md

---

# /speckit.specify

## Contexto

O farm MVP ja possui `FarmPlot`, `FarmPlotRegistry`, `SeedDataSO`, plantio por seed selecionada, crescimento por dias e colheita via harvest data. O loop ainda nao possui irrigacao real, estado rico de solo, uso completo de hoe/watering can nem UX agricola suficiente.

A spec 03 de Inventory define slots, painel de itens e inventory save v2. Esta spec 04 deve consumir seeds a partir do inventory, sem exigir que a seed esteja na mao/hotbar.

## Problema

O fluxo atual de plantio e simples demais para sustentar farm gameplay:

- nao ha irrigacao;
- nao ha estado de solo cru/arado/molhado/plantado/pronto;
- plantar depende de seed selecionada na hotbar;
- jogador nao recebe opcoes contextuais claras no tile;
- visual de crescimento ainda pode depender de cor/fallback;
- save/load precisa preservar solo, agua, seed e crescimento.

## Objetivo

Implementar um fluxo agricola jogavel baseado em estados de plot, menu contextual vertical acima do tile e acoes validas por estado: arar, molhar, plantar seeds disponiveis no inventory e colher.

## Decisoes aprovadas

- Plantio nao exige seed na mao.
- Ao interagir com `E` em um tile agricola, abrir menu contextual pequeno acima do tile com acoes validas.
- O menu contextual e vertical e navegavel por `WASD`.
- Seeds listadas no menu devem vir do inventory atual.
- Acoes contextuais incluem, conforme estado: arar solo, molhar solo, plantar Seed A/B/C, colher.
- Seed so e consumida depois que o plantio for validado e executado com sucesso.
- Sem agua, planta nao cresce no MVP, mas tambem nao morre.
- Agua reseta apos aplicar crescimento do dia.
- Harvest normal retorna plot para `TilledDry`.
- `RegrowDays` opcional entra agora.
- `StageSprites` opcional entra agora com fallback visual atual.
- Fertilizante, clima/chuva e sazonalidade ficam fora do MVP.
- Stamina final fica fora desta spec; somente hooks/campos opcionais podem ser preparados.

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

- `Blocked`: tile nao interagivel para farm.
- `Raw`: solo cru, pode receber Hoe.
- `TilledDry`: solo arado seco, pode receber Watering Can ou seed.
- `TilledWet`: solo arado molhado, pode receber seed.
- `PlantedDry`: plantado seco, pode receber Watering Can.
- `PlantedWet`: plantado molhado, elegivel para progresso no fim do dia.
- `ReadyToHarvest`: cultura pronta para colher.
- `Dead`: reservado para futuro; nao precisa ser produzido por falta de agua no MVP.

## Menu contextual agricola

Ao pressionar `E` olhando/interagindo com um plot valido, abrir `FarmPlotActionMenu` ou equivalente.

Posicionamento:

- pequeno overlay acima do tile alvo;
- vertical;
- nao desloca HUD principal;
- fecha ao confirmar acao, apertar `Esc`, sair de alcance ou apertar `E` novamente se nada for confirmado.

Input:

```text
E abre menu ou confirma acao selecionada quando menu esta aberto.
W/S navegam verticalmente.
A/D podem ser ignorados ou usados como alias de navegacao se houver subopcoes futuras.
Enter ou Space confirmam.
Esc cancela/fecha.
```

Enquanto o menu estiver aberto:

- movimento do player deve ser bloqueado ou ignorado;
- tool/attack/interact do mundo nao deve disparar em paralelo;
- apenas input contextual do menu deve ser processado.

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

## Acoes

### Arar solo

- Acao valida em `Raw`.
- Exige Hoe disponivel conforme sistema atual de tools.
- Resultado: `Raw -> TilledDry`.
- Nao deve depender de seed ativa.

### Molhar solo

- Acao valida em `TilledDry` e `PlantedDry`.
- Exige Watering Can disponivel conforme sistema atual de tools.
- Resultado:

```text
TilledDry -> TilledWet
PlantedDry -> PlantedWet
```

### Plantar seed

- Acao valida em `TilledDry` ou `TilledWet`.
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

- Acao valida em `ReadyToHarvest`.
- Gera harvest conforme `SeedDataSO`/harvest data.
- Se cultura sem regrow: `ReadyToHarvest -> TilledDry`.
- Se cultura com `RegrowDays > 0`: voltar para `PlantedDry` com contador de regrow reiniciado.

## Crescimento

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

## SeedDataSO

Expandir `SeedDataSO` com campos opcionais/necessarios:

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

Regras:

- save/load deve restaurar solo cru/arado/molhado/plantado/pronto;
- save/load deve restaurar seed plantada e progresso;
- nao serializar `SeedDataSO`, `Sprite`, `GameObject`, `Transform`, `MonoBehaviour`, `Collider` ou `Rigidbody`;
- se `SeedId` nao existir no database ao carregar, plot deve falhar de forma segura e logar erro claro.

## UI/feedback

- Highlight de tile interagivel.
- Prompt contextual curto, por exemplo: `E - Acoes`.
- Feedback se tool necessaria nao estiver disponivel.
- Feedback se nao houver seeds no inventory.
- Preview textual da seed no menu; preview visual pode ser futuro.

## Fora de escopo detalhado

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

## Criterios de aceite

- Plot distingue `Raw`, `TilledDry`, `TilledWet`, `PlantedDry`, `PlantedWet` e `ReadyToHarvest`.
- `E` em plot valido abre menu contextual vertical acima do tile.
- Menu mostra apenas acoes validas para o estado atual.
- `W/S` navegam no menu.
- `E`, `Enter` ou `Space` confirmam acao.
- `Esc` cancela/fecha.
- Movimento do player nao conflita com menu aberto.
- `Raw -> Arar solo -> TilledDry`.
- `TilledDry -> Molhar solo -> TilledWet`.
- `TilledDry/TilledWet -> Plantar seed do inventory -> PlantedDry/PlantedWet`.
- Plantar consome seed somente apos sucesso.
- Planta molhada cresce no avanco do dia.
- Planta seca nao cresce e nao morre no MVP.
- Colheita normal retorna plot para `TilledDry`.
- Regrow opcional funciona se `RegrowDays > 0`.
- Save/load preserva estado de solo, agua, seed e progresso.
- Visual de stage usa `StageSprites` quando disponivel e fallback quando ausente.
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
docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md
```

Logica de farm deve ficar fora de MonoBehaviour pesado quando possivel. MonoBehaviours fazem ponte Unity/runtime.

## Fluxos

### Abrir menu contextual

```text
Player pressiona E olhando/interagindo com plot
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
Selecionar acao no menu
Revalidar estado do plot e recursos no momento da confirmacao
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

### Save/load

```text
Salvar PlotId + State + SeedId + GrowthProgress + RegrowRemainingDays
Carregar por IDs simples
Rebind visual a partir de SeedDataSO/StageSprites se existir
Fallback visual se sprite ausente
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

## Riscos de regressao

- Menu contextual pode conflitar com input de movimento/interacao.
- Seed pode ser consumida antes do plantio falhar.
- Avanco de dia pode crescer planta seca se estado molhado/seco nao for persistido corretamente.
- Save/load pode perder `SeedId` ou progresso.
- Ferramentas podem duplicar regras de stamina antes da spec 09.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `FarmPlot`, `FarmPlotRegistry`, `SeedDataSO`, inventory e tools atuais.
- [ ] Criar/ajustar `FarmPlotState`.
- [ ] Implementar estados Raw/TilledDry/TilledWet/PlantedDry/PlantedWet/ReadyToHarvest.
- [ ] Implementar acoes Arar, Molhar, Plantar e Colher.
- [ ] Criar menu contextual agricola vertical acima do tile.
- [ ] Integrar menu com `E`, `W/S`, `Enter/Space` e `Esc`.
- [ ] Bloquear movimento/interacao enquanto menu esta aberto.
- [ ] Listar seeds disponiveis no inventory como opcoes de plantio.
- [ ] Consumir seed somente apos plantio bem-sucedido.
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
docs/specs/**
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
- Validacao documental e Unity registrada.

## Validacao

- `./tools/docs/validate_docs.ps1`
- `./tools/unity/RunUnityCompileValidation.ps1`
- `./tools/unity/ScanUnityLogs.ps1`
- Play Mode: abrir menu com E, navegar W/S, arar Raw, molhar TilledDry, plantar seed do inventory, validar consumo seguro, avancar dia molhado/seco, colher, save/load em cada estado.
