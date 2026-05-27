# SPEC - Visual/world scale, camera e sprite profiles

> Spec ID: spec_visual_world_scale_camera_sprite_profiles
> Status: A implementar
> Ordem de execucao: 17A
> Depende de: 00-16
> Bloqueia: 17
> Tipo: World/Visual Runtime
> Fonte: docs/specs/ como fonte unica.
> Escopo: Ajustar escala visual e espacial do jogo antes da UI final: cidade e fazenda com mais area util, cavernas com salas/corredores maiores, personagens/monstros/arvores/objetos visualmente maiores, camera coerente, colliders/footprints/interacao/ranges ajustados e sem quebrar save, combate, farm, cave generation ou UI futura.
> Fora de escopo: refazer arte final, criar biomas novos, criar conteudo novo de cidade/fazenda/cave, alterar regras de combate/skills, reescrever procedural da cave alem dos parametros de tamanho, gamepad, minimap, UI final, Packages, ProjectSettings e docs_old.

---

# /speckit.specify

## Contexto

O jogo esta visualmente pequeno. O ajuste desejado nao e apenas aproximar a camera: cidade, fazenda, cavernas, personagens, monstros, arvores e objetos precisam parecer e/ou ocupar mais espaco no jogo.

Decisoes aprovadas:

```text
Cidade: aproximadamente 4x maior em area util.
Fazenda: aproximadamente 4x maior em area util.
Cavernas: aproximadamente 2x maiores em area util.
Corredores da cave: mais largos.
Salas da cave: maiores.
Personagens, monstros, arvores e objetos: maiores visualmente, com profiles por tipo.
```

Esta spec entra antes da UI final porque tamanho visual/camera/context hints afetam HUD, interaction hints, shop/dialogue, crafting, corpse, checkpoints e menus.

## Problema

Se apenas aplicar `transform.localScale = 4` em tudo, o jogo pode quebrar:

- player pode ocupar tiles demais;
- colliders ficam errados;
- interacao pode pegar alvo errado;
- farm pode ficar apertada;
- caves podem spawnar inimigos/objetos em parede;
- melee/bow/magic ranges podem ficar incoerentes;
- camera pode mostrar pouco ou muito mapa;
- UI/context hint pode ficar mal posicionada;
- save positions e snapshots podem ficar inconsistentes.

A solucao deve separar:

```text
Escala visual
Escala logica/grid
Collider/footprint
Interaction radius
Combat ranges
Camera framing
Map/cave dimensions
```

## Objetivo

Criar uma camada data-driven de escala visual e espacial para:

- aumentar cidade/fazenda em area util;
- aumentar caves em area util;
- alargar corredores e salas da cave;
- aumentar visualmente player, NPCs, inimigos, arvores, pedras, baus, pickups e objetos interativos;
- preservar grid/logica de gameplay;
- ajustar colliders/footprints/interactions sem depender apenas do sprite scale;
- ajustar camera para leitura confortavel;
- garantir que combate, farm, cave, save e UI futura nao sejam quebrados.

## Decisoes aprovadas

### Escala de mapas

```text
Town/Fazenda: target 4x area util.
Cave: target 2x area util.
```

Interpretacao:

- 4x area util nao significa necessariamente multiplicar cada eixo por 4.
- Para area 4x, o padrao recomendado e multiplicar largura e altura por aproximadamente 2x.
- Se multiplicar largura e altura por 4x, a area vira 16x, o que e escopo grande demais para MVP.

Portanto:

```text
Cidade/Fazenda MVP: largura 2x e altura 2x, gerando ~4x area.
Cave MVP: largura/altura ou room/corridor budget ajustado para ~2x area total, nao 4x por eixo.
```

### Escala visual

Separar escala visual de escala logica.

```text
Sprite visual scale pode ser maior.
Collider pode ser menor ou independente.
Interaction radius tem config propria.
Attack range tem config propria.
Tile/grid continua como unidade logica principal.
```

### Camera

Camera deve ser ajustada depois de aplicar escala visual/espacial.

- Nao resolver tudo apenas com zoom.
- Usar camera para enquadramento final.
- Camera deve mostrar area suficiente para farm, cidade, combate e cave.

## Scale profiles

Criar/consolidar profiles data-driven:

```text
VisualScaleProfileSO
WorldScaleConfigSO
CameraScaleConfigSO
CaveGenerationScaleConfigSO
MapScaleConfigSO
```

### VisualScaleProfileSO

Campos minimos:

```text
ProfileId
DisplayName
EntityCategory
VisualScale
ColliderScale
FootprintSize
InteractionRadius
SelectionRadius
NameplateOffset
HintOffset
ShadowScale opcional
```

Categorias:

```text
Player
NPC
EnemyTiny
EnemySmall
EnemyMedium
EnemyLarge
EnemyHuge
EnemyBoss
TreeSmall
TreeMedium
TreeLarge
RockSmall
RockMedium
Pickup
Chest
Workbench
Forge
CookingStation
FarmObject
CavePortal
CheckpointPortal
Corpse
```

### Valores iniciais sugeridos

```text
Player VisualScale: 2.0
NPC VisualScale: 2.0
EnemyTiny VisualScale: 1.0
EnemySmall VisualScale: 1.5
EnemyMedium VisualScale: 2.0
EnemyLarge VisualScale: 3.0
EnemyHuge VisualScale: 4.0
EnemyBoss VisualScale: 4.0 a 5.0
TreeSmall VisualScale: 2.0
TreeMedium VisualScale: 3.0
TreeLarge VisualScale: 4.0
RockSmall VisualScale: 1.5
RockMedium VisualScale: 2.0
Pickup VisualScale: 1.5
Chest VisualScale: 2.0
Workbench/Forge/CookingStation VisualScale: 2.0
Portal/CheckpointPortal VisualScale: 2.0 a 3.0
Corpse VisualScale: 2.0
```

Regra:

- Esses valores sao default testaveis, nao balance final.
- O executor deve criar config data-driven para ajuste rapido sem hardcode.

## Map scale policy

### Cidade

Objetivo:

```text
Cidade com aproximadamente 4x area util atual.
```

Implementacao recomendada:

- Se mapa atual for manual/tilemap: expandir bounds em X/Y para aproximadamente 2x por eixo.
- Se mapa for procedural/configurado: criar `TownMapScaleConfigSO` ou parametros equivalentes.
- Manter pontos importantes acessiveis.
- Reposicionar NPCs/shops/Pip com anchors estaveis.
- Criar mais espaco de circulacao entre objetos, casas, lojas, NPCs e caminhos.

Regras:

- Nao precisa criar conteudo novo para preencher tudo.
- Areas vazias temporarias sao aceitaveis se nao bloquearem gameplay.
- Evitar cidade gigante vazia demais: pode usar zonas bloqueadas/decorativas futuras.
- Pip ainda deve conseguir andar ate o jogador quando ele entra na cidade.

### Fazenda

Objetivo:

```text
Fazenda com aproximadamente 4x area util atual.
```

Implementacao recomendada:

- Expandir area walkable e area de plots.
- Reposicionar entrada da cave, Fonte de Anya, checkpoint portal, workstations e objetos fixos.
- Manter 2 fishing spots fixos da fazenda.
- Garantir que plots continuam salvando por IDs/coords estaveis.

Regras:

- Nao quebrar save/load de farm plots.
- Se mudar coordenadas de plots existentes, criar migration/compatibilidade ou manter ids antigos.
- Workstations da spec 07 devem continuar prontas na fazenda.
- Entrada da cave e portal de checkpoint da spec 14 devem continuar proximos e claros.
- Fonte de Anya da spec 15 deve continuar proxima da entrada da cave.

### Cavernas

Objetivo:

```text
Cavernas com aproximadamente 2x area util.
Corredores mais largos.
Salas maiores.
```

Parametros minimos:

```text
RoomMinWidth
RoomMaxWidth
RoomMinHeight
RoomMaxHeight
CorridorMinWidth
CorridorMaxWidth
LevelBoundsPadding
SpawnSafeRadius
InteractableSpacing
EnemyPackSpacing
ResourceSpacing
FishingSpotSafeRadius
BossArenaMinSize
```

Recomendacao inicial:

```text
RoomMinWidth: aumentar ~1.5x
RoomMaxWidth: aumentar ~2x
RoomMinHeight: aumentar ~1.5x
RoomMaxHeight: aumentar ~2x
CorridorMinWidth: pelo menos 2 tiles, ideal 3 tiles
CorridorMaxWidth: 3 a 5 tiles
BossArenaMinSize: aumentar ~2x
```

Regras:

- Cave continua com 100 niveis macro.
- Checkpoints continuam em 15/30/45/60/75/90.
- Snapshot replay deve continuar deterministico por RunId + LevelIndex.
- LayoutHash deve mudar apenas por versao/config nova de generation, nao por nondeterminismo.
- Inimigos, resources, pickups, fishing spots e portals devem usar safe anchors validos.
- Fishing spot cave continua 10% por level e maximo 1 por level.
- Boss gates/checkpoints nao podem spawnar em corredor estreito ou parede.

## Character/object scale policy

### Player

Regras:

- Player visual deve ficar maior, default 2x.
- Collider nao deve necessariamente ser 2x.
- Footprint deve permitir passagem em corredores de cave.
- Interaction origin deve ajustar para novo sprite.
- Weapon/tool origin e hitbox/hurtbox devem ser revisados.
- Nameplate/hint offset deve subir junto com sprite.

### NPCs

Regras:

- NPCs default 2x.
- Dialogue interaction radius ajustado.
- Pip movement deve considerar novo collider/radius.
- Lojistas devem continuar acessiveis.

### Enemies

Regras:

- Usar size class da spec 13:

```text
Tiny
Small
Medium
Large
Huge
Boss
```

- Cada EnemyDataSO deve referenciar profile de escala ou size class.
- Collider e attack range devem ser coerentes com tamanho.
- Vulnerability window e telegraph devem continuar visiveis.
- Damage numbers devem posicionar acima do sprite maior.

### Trees/resources/objects

Regras:

- Arvores maiores devem ter collider/interaction separado do sprite.
- Tree hit point/interaction point deve ficar claro.
- Workstations maiores nao podem bloquear caminhos essenciais.
- Pickups maiores nao podem atrapalhar navegação.
- Chests/corpses/portals devem ficar legiveis e interagiveis.

## Camera policy

Criar/consolidar:

```text
CameraScaleController
CameraScaleConfigSO
```

Campos minimos:

```text
DefaultOrthographicSize
FarmOrthographicSize
TownOrthographicSize
CaveOrthographicSize
BossArenaOrthographicSize
MinOrthographicSize
MaxOrthographicSize
FollowOffset
DeadZone opcional
BoundsPadding
```

Regras:

- Camera deve respeitar bounds de mapa/cave.
- Camera deve mostrar player + contexto suficiente.
- Cave camera pode ser um pouco mais proxima que farm/town, desde que nao prejudique combate ranged.
- Boss arena pode usar zoom out leve.
- Camera nao deve depender de `Camera.main` se houver alternativa configurada.

Valores iniciais devem ser calibrados em Play Mode, nao hardcoded como finais.

## Gameplay ranges impacted

Ao aumentar escala visual/espacial, revisar:

```text
InteractionRadius
ToolUseRange
MeleeAttackRange
BowRange
SpellRange
PickupRadius
NPCDialogueRadius
CorpseRecoveryRadius
CheckpointPortalRadius
CraftingStationRadius
FishingInteractionRadius
EnemyAggroRadius
EnemyAttackRange
ProjectileSpawnOffset
DamageNumberOffset
```

Regras:

- Nao mudar balance de dano nesta spec.
- Ajustar ranges apenas para coerencia espacial.
- Registrar qualquer range alterado no log.

## Save/load compatibility

Riscos:

- Farm plots por coordenada podem mudar.
- NPC/object anchors podem mudar.
- Cave snapshots antigas podem nao bater com nova geracao.
- Corpse/cave positions podem ficar invalidas se layout mudar.

Regras:

- Persistir IDs/anchors estaveis sempre que possivel.
- Se mapa manual mudar, manter migration ou fallback para nearest safe anchor.
- Cave generation deve ter `GenerationConfigVersion`.
- Snapshots antigas com config anterior devem continuar carregando ou serem invalidadas com fallback claro.
- Save DTOs nao devem serializar Unity refs.

## UI impact

Esta spec nao implementa UI final, mas prepara offsets/dimensoes para spec 17.

Ajustar/fornecer dados para:

```text
ContextHint offset
Nameplate offset
DamageNumber offset
Interaction marker offset
Selection outline size
Health bar above enemy offset
```

## Anti-regressao

Esta spec nao pode quebrar:

- Inventory/drop/pickup da spec 03/05;
- Farm plots/planting/watering da spec 04;
- World activities/fishing/trees da spec 05;
- NPC/shop/dialogue/Pip da spec 06/08;
- Crafting/workstations da spec 07;
- Hunger/stamina/time pause da spec 09;
- Equipment/hands/colliders da spec 10;
- Player combat Q/E/R/T/Y/G da spec 12;
- Enemy AI/size/vulnerability da spec 13;
- Cave snapshots/checkpoints/boss gates da spec 14;
- Death/corpse/Anya da spec 15;
- Skill tree active slots da spec 16;
- Save migration da spec 02.

## Criterios de aceite

- Cidade possui aproximadamente 4x area util em relacao ao estado anterior, preferencialmente 2x largura e 2x altura.
- Fazenda possui aproximadamente 4x area util em relacao ao estado anterior, preferencialmente 2x largura e 2x altura.
- Cavernas possuem aproximadamente 2x area util, com salas maiores e corredores mais largos.
- Player/NPCs/inimigos/arvores/objetos possuem escala visual maior por profile data-driven.
- Collider/footprint/radius nao dependem cegamente do sprite scale.
- Player 2x visual passa por corredores padrao da cave.
- Inimigos Tiny/Small/Medium/Large/Huge/Boss usam size profiles coerentes.
- Arvores e objetos grandes continuam interagiveis sem bloquear caminhos essenciais.
- Camera usa config por contexto: farm, town, cave, boss arena.
- Interaction ranges e offsets sao ajustados para o novo tamanho visual.
- Cave generation continua deterministica com snapshot replay.
- Save/load nao quebra por posicoes antigas; fallback para safe anchor existe quando necessario.
- UI futura tem offsets para hints/nameplates/damage numbers.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Camera/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Interaction/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Save/** somente compat/fallback
Assets/_Game/Data/Scale/**
Assets/_Game/Data/Camera/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Maps/**
```

## Ordem segura de implementacao

1. Auditar camera, player, NPCs, enemies, tilemaps/maps, cave generator, colliders e interactions atuais.
2. Criar configs data-driven de visual/world/camera scale.
3. Implementar `VisualScaleProfileSO` e aplicar por categoria sem hardcode.
4. Ajustar player/NPC/enemy/object visual scale com collider independente.
5. Ajustar cidade/fazenda para ~4x area util.
6. Ajustar cave generation para ~2x area util, corredores largos e salas maiores.
7. Ajustar camera configs por contexto.
8. Ajustar interaction/combat/pickup/corpse/portal/workstation radii.
9. Ajustar offsets para hints/nameplates/damage numbers.
10. Garantir save/load compatibility/fallback safe anchor.
11. Validar Play Mode em farm, town e cave.
12. Atualizar docs/status/log.

## Riscos

- Aumentar mapas pode criar vazio excessivo.
- Aumentar sprites pode quebrar collider/interacao.
- Cave maior pode afetar performance ou spawn density.
- Camera muito proxima pode prejudicar navegacao.
- Camera muito distante pode voltar ao problema de tudo pequeno.
- Save positions antigas podem cair fora de bounds.

## Mitigacoes

- Separar escala visual de collider/footprint.
- Usar profiles data-driven.
- Usar anchors estaveis e nearest safe anchor fallback.
- Aumentar area util sem multiplicar conteudo obrigatoriamente.
- Medir com cena de teste antes de aplicar em tudo.
- Validar ranges e offsets em Play Mode.

---

# /speckit.tasks

## Tasks

- [ ] Auditar escala/camera/tilemap/player/NPC/enemy/cave atuais.
- [ ] Criar `VisualScaleProfileSO`.
- [ ] Criar `WorldScaleConfigSO`.
- [ ] Criar `CameraScaleConfigSO`.
- [ ] Criar `CaveGenerationScaleConfigSO`.
- [ ] Criar/ajustar profiles para Player, NPC, EnemyTiny/Small/Medium/Large/Huge/Boss, Trees, Rocks, Pickups, Chests, Workstations, Portals e Corpse.
- [ ] Aplicar escala visual data-driven sem acoplar collider cegamente.
- [ ] Ajustar cidade para ~4x area util.
- [ ] Ajustar fazenda para ~4x area util.
- [ ] Reposicionar anchors fixos: cave entrance, checkpoint portal, Fonte de Anya, workstations, fishing spots, shops/NPCs.
- [ ] Ajustar cave generation para ~2x area util.
- [ ] Aumentar room min/max e corridor width.
- [ ] Ajustar spawn safe radius e resource spacing.
- [ ] Ajustar boss arena min size.
- [ ] Ajustar camera por contexto.
- [ ] Ajustar interaction radii e offsets.
- [ ] Ajustar damage number/nameplate/hint offsets.
- [ ] Implementar save/load fallback para nearest safe anchor quando necessario.
- [ ] Atualizar docs/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Camera/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Interaction/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Save/** somente compat/fallback
Assets/_Game/Data/Scale/**
Assets/_Game/Data/Camera/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Maps/**
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

- Cidade/fazenda/cave maiores conforme decisoes.
- Personagens, monstros, arvores e objetos maiores por profiles.
- Camera coerente por contexto.
- Colliders/interactions/ranges ajustados sem quebrar gameplay.
- Cave procedural deterministica e com salas/corredores maiores.
- Save/load com fallback seguro para posicoes invalidas.
- Specs 01-16 nao regredidas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Abrir fazenda e validar area util ~4x.
2. Validar player visual maior e collider coerente.
3. Validar movimento sem prender em objetos.
4. Validar cave entrance, checkpoint portal e Fonte de Anya reposicionados e interagiveis.
5. Validar workstations prontas e acessiveis.
6. Validar 2 fishing spots fixos da fazenda.
7. Abrir cidade e validar area util ~4x.
8. Validar Pip andando ate o jogador sem travar.
9. Validar lojistas/NPCs interagiveis.
10. Validar NPC ambulante se existir.
11. Entrar cave e validar area util ~2x.
12. Validar corredores mais largos e salas maiores.
13. Validar player passa em corredores.
14. Validar inimigos Tiny/Small/Medium/Large/Huge/Boss com escala coerente.
15. Validar spawn seguro de inimigos/resources/pickups/portals.
16. Validar combate melee/ranged/magic com ranges coerentes.
17. Validar damage numbers/hints/nameplates acima do sprite correto.
18. Validar camera em farm/town/cave/boss arena.
19. Salvar/carregar em farm/town/cave e validar posicoes ou fallback safe anchor.
20. Validar que snapshots da cave continuam deterministicas por seed/run.
