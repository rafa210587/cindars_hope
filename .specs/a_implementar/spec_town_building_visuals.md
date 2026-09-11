# SPEC — Town Building Visuals (kits 3-partes por arquétipo)

**Status:** SUPERSEDED — não executar; substituída por `spec_town_native_architecture_v1` em 2026-09-10
**Tipo:** arte + asset wiring (editor-only) — 2 tracks: geração GPT + wiring no gerador
**Dependências:** Rendering v2 (concluída 2026-07-03) e `spec_town_layout_v9_organic.md` (relayout).
**Origem:** Pedido do usuário (2026-07-03): toda construção em 3 partes — (1) BASE/paredes, (2) TELHADO que some quando o player entra (RoofReveal), (3) PORTA que abre/fecha.

> Este documento preserva o inventário histórico. A meta fixa de 15 kits/45 imagens e a dependência do
> layout 120×90 não representam o estado 160×112 atual. Não executar nem usar para reverter assets.

---

## Estado atual (auditoria 2026-07-03)

- **6/24 lotes completos**: kit modular A/B/C (`Assets/_Game/Art/Generated/World/building/houses_modular/`: `walls_X_topdown` + `roof_X_aerial` + `door_X`) — usados pelas residências.
- **18/24 lotes em fallback**: telhado `roof_redtile` tiled + `door_wood` genérica (os retângulos gigantes na tela).
- Staging não integrado: `art/world_gpt/raw/gpt_landmark_temple.png`, `gpt_landmark_townhall.png`, `art/world_gpt/sliced/temple_modular_*` e `modular_angles_*` — são prédios inteiros/ângulos, NÃO separados em 3 partes; servem de referência visual, não de asset final.

## Decisão de formato

Todo arquétipo ganha um **kit de 3 PNGs** no padrão do precedente `houses_modular`:
- `walls_<kit>.png` — fachada + laterais vistas no ângulo 3/4 top-down do player, com o vão da porta VAZADO;
- `roof_<kit>.png` — telhado completo visto de cima (aerial), cobre o footprint;
- `door_<kit>.png` — folha de porta isolada, mesma escala da abertura.

Regras técnicas: mesma proporção do lote (spec de layout), PPU 64 (novo padrão — kit A/B/C referência), fundo transparente, paleta Stardew+Children of Morta do projeto. Pivot é corrigido automaticamente pelo `WorldSpritePivotImportStep` (pasta `building/` já coberta).

## Kits a gerar via ChatGPT web (skill `chatgpt-web-sprite-gen`, projeto "Sprites - Fazendeiro")

Ordem de prioridade (bateladas; rate limit = pausa 10 min + retry):

**Batch 1 — landmarks/hubs:** `temple` (igreja de Kanthor, vitral azul, cruz/sino), `townhall` (câmara, brasão), `market` (hall com toldo/vitrines).
**Batch 2 — ofícios:** `forge` (chaminé + brasa), `bakery` (forno/pretzel), `inn` (taverna, caneca), `alchemy` (frascos), `workshop`, `tannery` (peles penduradas).
**Batch 3 — infra:** `warehouse` (celeiro grande), `guard` (guarnição de pedra escura, ameias), `archive` (biblioteca), `registry` (cívico), `watermill` (roda d'água), `manor` (mansão nobre, telhado roxo).

Total: 15 kits × 3 partes = 45 imagens. Staging em `art/world_gpt/raw/` com prefixo `gpt_kit_<kit>_<parte>.png`, pós-processamento (rembg + downscale) pelo pipeline existente, destino final `Assets/_Game/Art/Generated/World/building/kits/<kit>/`.

## Wiring no gerador (após arte existir)

1. `WorldSpriteLibrary`: novo accessor `BuildingKit(kit, parte)` com fallback para o kit A/B/C.
2. `CreateMvpTownScene.CreateWalkInHouse`: mapa arquétipo→kit (`Temple→temple`, `Forge→forge`, `Residential/Rural→A/B/C cíclico`, etc.); usar `walls_/roof_/door_` do kit no lugar do fallback tiled. As 3 partes mantêm o contrato de sorting: walls=World, roof=Roof (RoofReveal), door=World com épsilon de pivot.
3. Muralha: trocar quads por `wall_stone` + merlões, torres com sprites (base=World, topo=Roof) — se sprite de torre não existir, adicionar `tower` ao Batch 3.
4. Nenhum sprite novo referenciado sem fallback gracioso + log de wiring claro se ausente.

## Validação
- Builds runtime+editor exit 0.
- Regenerar via `CindarsHope/Inicializar Projeto`; zero logs de sprite ausente; Play Mode: entrar numa casa de cada batch (telhado some, porta abre), player nunca invisível.

## Critérios de aceite
- [ ] 15 kits gerados, pós-processados e importados em `building/kits/`.
- [ ] 24/24 lotes renderizando com kit 3-partes (zero fallback tiled).
- [ ] Muralha/torres com sprites reais.
- [ ] Builds + regeneração + Play Mode OK.
