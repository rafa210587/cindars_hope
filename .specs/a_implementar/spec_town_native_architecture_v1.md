# SPEC — Town Native Architecture v1

**Spec ID:** `town_native_architecture_v1`  
**Status:** A_IMPLEMENTAR — especificada por pedido humano em 2026-09-10  
**Wave:** TOWN.A1 — arquitetura e consistência de pixel  
**Domain:** TownScene / pixel art / editor wiring  
**Priority:** P0  
**Ordem de execucao:** TOWN.A1; candidatos podem avançar em paralelo a TOWN.V1, Unity somente após layout estabilizado  
**Depende de:** `spec_town_spatial_expansion_and_access_v1`; contrato visual estabilizado de TOWN.V1  
**Bloqueia:** `spec_town_components_doors_and_collision_v1`, `spec_town_integrated_acceptance_v1`  
**Repo lock:** fontes `art/town-keyart-rework`, assets `town_*`, helpers `TownKeyartBuildingArt`/import  
**Must not run with:** outra regeneração Unity de Town/Farm

required_adrs: []
required_game_rules: [city_rules]

# /speckit.specify

## Objetivo

Substituir fachadas repetidas, borradas ou reamostradas por famílias arquitetônicas nativas e legíveis
na escala real da Town, preservando footprints, acessos, interiors walk-in, sorting e identidade dos
24 lotes. Esta spec sucede `spec_town_building_visuals.md`; não exige 45 imagens por quota.

## Phase 0

- Templo, câmara e ferraria já possuem leitura forte e devem ser reutilizados antes de gerar arte.
- Bakery, watermill e alchemy têm maior perda de nitidez/escala na captura vigente.
- Residências repetem a mesma fachada com variações de cor; isso não satisfaz variedade estrutural.
- O piloto de porta gable v08 demonstra cinco poses, mas não prova cobertura das famílias.
- Candidatos ChatGPT normal de bakery/mill são staging, não assets finais aprovados.

## Existing Systems Audit

Reusar `TownKeyartBuildingArt`, `TownKeyartSpriteImporter`, `WorldSpriteLibrary`,
`CreateMvpTownScene.CreateWalkInHouse`, `RoofRevealController` e `HouseDoorInteractable`.
Não criar segundo gerador, segundo registry de sprites ou sistema paralelo de reveal/porta.

## Escopo permitido

- `art/town-keyart-rework/**` para fontes `.aseprite`, scripts Lua, exports e evidência;
- `Assets/_Game/Art/Generated/World/building/town_*` e respectivos `.meta`;
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartBuildingArt.cs`;
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartSpriteImporter.cs`;
- `Assets/_Game/Scripts/Editor/Art/WorldSpriteLibrary.cs`, somente accessors necessários;
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`, somente wiring arquétipo→arte;
- testes/validadores town-only e `TownScene.unity` somente por regeneração canônica.

## Fora de escopo

Layout, posições, NPC runtime, save, diálogo/economia, colliders finais, interior dressing e Farm/Cave.

# /speckit.plan

1. Inventariar por lote: asset, resolução fonte, PPU, escala mundial, família, porta pintada, fallback.
2. Congelar os assets fortes; reconstruir primeiro bakery, watermill e alchemy em Aseprite.
3. Definir famílias por silhueta/material/função; variação apenas cromática não cria nova família.
4. Para cada família walk-in, exportar fachada com vão, roof separado e folha/frames compatíveis com
   TOWN.C1. Reabrir o `.aseprite` e validar alpha antes do wiring.
5. Integrar pelo gerador existente, com fallback explícito e erro de validação quando um lote usa arte
   não aprovada. Nunca usar imagem inteira da keyart como cenário ou edifício mascarado.
6. Capturar overview e detalhes com câmera fixa; revisar pixels em 100%, escala de jogo e sorting.

## Contratos verificáveis

- Cada um dos 24 lotes possui `buildingFamilyId`, fonte, export e estado de aprovação no manifesto.
- Landmarks/ofícios listados no refinamento têm silhueta distinta; residências usam no mínimo quatro
  famílias estruturais distribuídas, sem uma única família em mais de 50% das residências.
- Bakery, watermill e alchemy não usam scaling fracionário nem fonte menor ampliada no runtime.
- Todo export usa `Point`, `Compression None`, mipmaps off, alpha real e pivô/PPU previstos pelo projeto.
- Toda família walk-in entrega fachada sem porta pintada no estado aberto, roof e leaf alinháveis.
- Escada, porta, placa e fachada crítica permanecem legíveis; foliage/props não cobrem mais de 5% da
  área opaca crítica nem a faixa de acesso.

## Critérios de aceite

- [ ] A-1: manifesto cobre 24/24 lotes e zero fallback genérico não justificado.
- [ ] A-2: bakery, watermill e alchemy passam revisão nativa em 100% e na escala real do Unity.
- [ ] A-3: mínimo de quatro famílias residenciais estruturais; nenhuma domina >50% das residências.
- [ ] A-4: todos os walk-ins entregam facade/roof/door compatíveis com TOWN.C1 e sem porta duplicada.
- [ ] A-5: import validator retorna 0 violações de filter/compression/mipmap/PPU/pivô/alpha.
- [ ] A-6: duas revisões abrem fontes e capturas reais; arquitetura atinge >=15/20 e nenhum blocker v2.
- [ ] A-7: 24 `House_*`, IDs, footprints, escala +20%, NPCs, anchors, portais e interações preservados.

# /speckit.tasks

- [ ] Produzir inventário/manifesto e selecionar reuse, revise ou replace por lote.
- [ ] Autorizar direção das famílias e pilotos bakery/watermill/alchemy em escala de jogo.
- [ ] Autorar/revisar fontes Aseprite e exports nativos necessários, sem geração por quota.
- [ ] Integrar famílias no gerador existente e validar imports/sorting/oclusão.
- [ ] Regenerar Town uma vez sob lock e capturar overview + detalhes comparáveis.
- [ ] Obter revisão independente da categoria arquitetura e entregar para TOWN.C1.
