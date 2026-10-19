# Cave render artifacts — execução parcial

**Spec:** `spec_playtest_cave_render_artifacts_v1` · **Data:** 2026-09-27 · **Status:** headless PASS; aceite humano/replay pendentes, sem promoção.

O `light_shaft.png` atual tem 1254×1254 pixels, todos com alpha 255, e xadrez claro gravado. Escalado em (3,5) a 128 PPU, o SpriteRenderer ocupava ~29,4×49 unidades e podia lavar um retângulo da Cave. `CaveVignetteController` agora usa, quando o profile habilita feixe, um sprite procedural cacheado de 64×64 RGBA/64 PPU, com fundo transparente e escala final de 3×5 unidades. O profile sem sprite continua sem feixe. Tentativas de remover o fundo da imagem por geração visual produziram bordas coloridas e foram descartadas; o PNG original não foi alterado.

| Critério | Evidência | Resultado |
|---|---|---|
| AC01, recorte e bounds | `CaveLightShaftPresentationTests` pós-reimport 3/3 PASS (`TestResults/playtest-cave-import-editmode.xml`); PlayMode gráfico 1/1 PASS (`TestResults/playtest-cave-render-playmode.xml`), dump confirma feixe 3×5 | PASS headless; aceite humano pendente |
| AC02, stable run | Diff limitado a apresentação e teste; `CaveGeneratedLevel`, geração, saída, inimigos, nós e save intocados | Inspeção estática PASS; replay/revisit em Unity NOT RUN |
| AC03, faixas/blocos escuros | Reparador Editor API PASS, `.meta` alignment0, dump Unity pivot `(64,64)`/bounds center y=0; captura magenta pós-fix em `TestResults/playtest-cave-render/`, 0/778 quadrantes de células walkable amostrados magenta | PASS headless |

O retângulo claro não aparece mais na captura, e o dump mostra `CaveEntranceLightShaft` em bounds 3×5 com sprite RGBA gerado. Antes da correção, os blocos persistiam sem vinheta e sem renderers World; com background magenta, viravam magenta, provando lacunas de render. `tile_floor_c` tinha pivot `(64,0)`, bounds center y=+0,5 e textura alpha GPU `255..255`; as outras tiles de piso tinham pivot `(64,64)` e center y=0. O AssetPostprocessor `GeneratedSpriteImporter` omitia `floor_c.png` da lista de tiles e forçava BottomCenter em toda reimportação. Dois reparos iniciais falharam por esse reset. Após incluir `floor_c.png` no carve-out canônico, `RepairCaveFloorSpriteAlignment.Repair` concluiu PASS e gerou `.meta` com alignment0; o sprite Unity agora tem pivot `(64,64)`, bounds center y=0.

Na nova captura magenta, uma análise amostral de 778 quadrantes de células walkable visíveis encontrou 0 magenta. A imagem normal não mostra as faixas retangulares anteriores. A seed/layout do novo frame é distinta da anterior, mas cada captura foi correlacionada com o seu próprio dump de Tilemap; não se afirma identidade de layout entre runs. Captura headless não substitui aceite humano de legibilidade.

**Unity validation:** reparo `TestResults/playtest-cave-floor-import-repair.log` PASS/exit 0; EditMode `TestResults/playtest-cave-import-editmode.xml` 3/3 PASS/exit 0; PlayMode gráfico `TestResults/playtest-cave-render-playmode.xml` 1/1 PASS/exit 0. **Residual risk:** replay/revisit e aceite visual humano não executados; sem controle de tela. `git diff --check`: PASS.
