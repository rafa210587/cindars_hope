# SPEC — Cave: artefatos de luz e faixas visuais

> Spec ID: spec_playtest_cave_render_artifacts_v1
> Status: PARTIAL; revisão 2, gates visuais headless PASS, aceite humano e replay pendentes.
> Type: Runtime/visual
> Domain: Cave
> Priority: P1
> Parallelizable: no · Repo lock: CaveVignetteController e testes Cave correspondentes.
> Depende de: stable run FASE9F; `spec_playtest_world_readability_v1` continua responsável pelo aceite visual jogado.
> Bloqueia: aceite D15 da Cave. Ordem de execucao: antes do aceite visual integrado.
> required_adrs: [] · required_game_rules: []

# /speckit.specify

## Objetivo e estado real (Phase 0)

O playtest de 27/09 relatou retângulo claro e faixas/blocos escuros na Cave. `light_shaft.png` mede 1254×1254, tem alpha 255 em todos os 1.572.516 pixels e carrega xadrez claro no RGB; `CaveVignetteController.CreateLightShaft` o escala em (3,5), cobrindo cerca de 29×49 unidades. O probe PlayMode gráfico em `TestResults/playtest-cave-render/` comprovou que os blocos viram magenta ao trocar temporariamente o fundo da câmera: são lacunas transparentes, não tint escuro. No mesmo dump, `tile_floor_c` tem alpha GPU 255..255, mas sprite pivot `(64,0)` e bounds center y=+0,5; `floor_a/b/detail` têm pivot `(64,64)`. O importer de `floor_c.png.meta` tem `alignment:7` (BottomCenter), os demais `alignment:0` (Center), deslocando `floor_c` uma célula acima do destino. Dois reparos via TextureImporter falharam porque `GeneratedSpriteImporter.CaveBiomeTileFileNames` omitia `floor_c.png` e o AssetPostprocessor restaurava BottomCenter a cada reimport. O amendment e roadmap FASE9F nos caminhos citados por AGENTS/skill não existem neste checkout; a regra de snapshot está descrita em AGENTS, CURRENT_STATE e no audit `ref_pr170_192_cave_stable_run_pre_implementation_audit.md`.

## Requisitos e aceite

- **AC01**: nenhum feixe renderizado pode carregar fundo opaco quadriculado ou ter bounds maiores que 3×5 unidades. Na Cave com profile de luz, fundo transparente nos cantos; com profile sem luz, não se cria feixe.
- **AC02**: saída visual depende apenas de profile e entrada do nível; não muda layout, geração, spawn, save ou snapshot. Reentrada da mesma seed preserva conteúdo; o shader faltante usa fallback seguro.
- **AC03**: `floor_c` reimportado pelo Editor com `SpriteAlignment.Center`, sprite pivot `(64,64)` e bounds center y=0 como as demais tiles de piso. Nova captura gráfica com fundo magenta não mostra lacunas nas células walkable cobertas por `floor_c`.

## Escopo

Permitidos: `Assets/_Game/Scripts/Cave/Runtime/CaveVignetteController.cs`, `Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs` para classificar `floor_c` como tile Cave, `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/floor_c.png.meta` somente via TextureImporter API, reparo idempotente `Assets/_Game/Scripts/Editor/Validation/RepairCaveFloorSpriteAlignment.cs`, teste focal em `Assets/_Game/Tests/EditMode/Cave/`, diagnóstico no smoke existente `Assets/_Game/Tests/PlayMode/Composition/Phase21GraphicalSmokePlayModeTests.cs`, esta spec e relatório próprio `docs/validation/spec_playtest_cave_render_artifacts_v1/REPORT.md`. Não editar YAML manualmente, CaveTileMaterializer, geração, exits, inimigos, loot, resource nodes, save/IDs. Não controlar tela nem jogar nesta rodada.

# /speckit.plan

Reusar o gatilho `CaveBiomeArtResolver.TryGetLightShaftSprite`: profile com sprite continua habilitando o efeito, profile sem sprite continua sem efeito. Em `CaveVignetteController.CreateLightShaft`, trocar somente o sprite renderizado por um sprite RGBA gerado e cacheado (64×64, 64 PPU, Point, sem mipmaps), com cone dourado e alpha zero fora dele; escala de (3,5) passa a resultar em bounds de 3×5. Usar `Sprites/Default` e transparência real; se o shader não existir, o renderer padrão ainda deve compor alpha. O asset opaco permanece referenciado como sinal de habilitação até troca de arte aprovada, mas não participa do render. Canto/centro devem ser testáveis por pixels do sprite gerado; teste de integração inspeciona renderer/bounds após `ApplyVignetteAndLightShaft`. Não alterar semântica de materialização, stable run nem dados.

Pseudocódigo: para y,x em 64×64, distância vertical t = 1-y/63; largura cresce com t, centro x=0,5; alpha = atenuação nas bordas laterais × atenuação no início/fim vertical × 0,30; RGB = dourado suave. Sprite.Create centralizado em 64 PPU; aplicar ao renderer de luz; manter nome, posição e sorting existentes.

Gate único de aceitação: EditMode focal de alpha/bounds/profile ausente + PlayMode gráfico com captura e correlação da faixa com tile/renderer da mesma materialização, além de inspeção de diff para stable run. Ambos executados pelo coordenador; sem correlação da faixa, a spec permanece parcial. Relatório discrimina PASS/PARTIAL/NOT RUN por AC. Risco: sem captura final da Cave, não é possível afirmar que o retângulo e as faixas desapareceram no frame real.

O smoke PlayMode já existente `Phase21GraphicalSmokePlayModeTests.FarmTownCaveScenes_PresentGameplayOnGraphicsDevice_AndCaptureWhenRequested` entra Farm→Cave sem input direto e grava `phase21-CaveScene.png` com `CINDARS_SKILL_CAPTURE_DIR`. Acrescentar ao mesmo frame `phase21-CaveScene-render.txt` com LayoutHash, câmera, células ocupadas/tint dos Tilemaps e bounds/material dos sprites visíveis. Requer graphics device; `-nographics` é inválido para conclusão visual. Captura e dump são prova complementar, não substituem aceite humano.

Adicionar `floor_c.png` a `GeneratedSpriteImporter.CaveBiomeTileFileNames`, preservando a regra de PPU 128/Center para todos os tiles Cave e BottomCenter para objetos. `RepairCaveFloorSpriteAlignment.Repair` é `public static` para `-executeMethod`. Usa `TextureImporter.ReadTextureSettings`/`SetTextureSettings` para `SpriteAlignment.Center`, `SaveAndReimport`, relê importer e sprite e exige `(64,64)` no runtime. Não toca outras propriedades do PNG/import. O teste EditMode `FloorTiles_ImportedWithCenteredPivot_DoNotLeaveTilemapGaps` rejeita regressão das quatro variantes. Rodar reparo antes do teste; depois repetir a captura magenta sem tocar em tint/materializer.

# /speckit.tasks

- [x] **T01 / AC01-02**: cone RGBA cacheado em `CaveVignetteController`, tamanho nativo 1×1 e material padrão de SpriteRenderer, preservando gatilho opcional do profile; sem acesso a stable run.
- [x] **T02 / AC01-02**: `CaveLightShaftPresentationTests` 3/3 EditMode PASS pós-reimport (`TestResults/playtest-cave-import-editmode.xml`).
- [x] **T03 / AC03**: captura e dump gráfico executados; magenta confirma lacunas, floor_c importado com pivot inferior; tint não alterado.
- [x] **T04 / AC01-03**: relatório de evidência e `/finish-spec`; sem promoção com aceite visual pendente.
- [x] **T05 / AC03**: `RepairCaveFloorSpriteAlignment.Repair` PASS, exit 0; .meta gerado com alignment0 e pivot Unity (64,64), log `TestResults/playtest-cave-floor-import-repair.log`.
- [x] **T06 / AC03**: EditMode 3/3 PASS e PlayMode gráfico 1/1 PASS; na captura magenta pós-fix, 0/778 quadrantes amostrados em células walkable ficaram magenta. Aceite humano de legibilidade permanece pendente.

## Edge cases

Profile ausente/null não cria feixe. Reentrada usa o mesmo sprite estático sem RNG. Shader ausente não converte transparência do sprite em retângulo. Criação falha de textura retorna sem feixe e sem afetar nível. Elementos escuros intencionais de parede não devem ser apagados sem correlação com a falha observada.
