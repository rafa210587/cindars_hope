# Auditoria complementar — Cave, física do mapa e movimento

**Resultado:** há uma causa técnica forte para o retângulo claro da Cave, objetos visuais sem collider próprio na Farm/Town e lacunas de movimento em NPCs/inimigos. Esta auditoria é estática; não houve controle da tela, Play Mode, alteração de cena nem aceite visual.

## Cave

- O playtest registrou blocos pretos no piso e um retângulo claro persistente (`EXPLORATORY_PLAYTEST_2026_09_27.md`). O asset `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/light_shaft.png` mede 1254×1254 e tem alpha 255 em todos os 1.572.516 pixels; o fundo claro quadriculado está embutido na imagem. Leitura de RGBA por Pillow: canto (241,241,242,255), centro (248,235,203,255). Logo, transparência do PNG não pode recortar o feixe.
- `Assets/_Game/Scripts/Cave/Runtime/CaveVignetteController.cs:29-46,138-177` aplica escala (3,5) ao sprite de 128 PPU: área aproximada de 29,4×49 unidades, sorting order 40 e alpha 0,6 com shader aditivo, ou 0,22 no fallback. Isso pode clarear uma área retangular extensa e corresponde ao sintoma, mas falta identificar o renderer/material da captura para confirmar causalidade em tela. O comentário do código presume névoa escura/transparente, enquanto o PNG atual é opaco e claro.
- `CaveTileMaterializer.cs:39-55,117-124,299-319` usa `FloorEdgeShadowTint` (0,62/0,60/0,58) em pisos adjacentes a parede; paredes de interior recebem (0,22/0,21/0,20), e a vinheta (`CaveVignetteController.cs:73-136`) desenha um retângulo preto com alpha crescente nas bordas. São candidatos à leitura de faixas/escuridão, sem evidência para atribuir os blocos pretos a um deles. As quatro texturas `floor_a/b/c/detail.png` examinadas são opacas, 128×128 e sem canto preto. É preciso comparar as células escuras com o tipo de tile, cor aplicada, overlay e coordenada da tela na mesma seed.
- Próxima prova: captura headless do frame e dump de sprites, tilemaps, cores e materiais no mesmo nível/seed; teste de asset deve rejeitar fundo opaco quando o feixe exigir recorte. Preservar snapshots e hash da stable run; não alterar geração, inimigos ou loot como correção visual.

## Farm e Town

- `FarmScene.unity` contém `Visual_SouthYardWestPost` e `Visual_SouthYardEastPost` apenas com Transform/SpriteRenderer; `FarmSettlementVisualComposer.Prop` não cria sólido para esses postes, e `FarmSettlementPhysicsContract.AllFenceSegments` não os inclui. Também não há collider próprio em `Visual_HomesteadHay`, `Visual_HouseCrates` e `Visual_ShedCrate`. Caixas/feno/cochos mais recentes possuem `Solid_*`; a conclusão não se estende a todos os props.
- `TownScene.unity` contém `TownWell`, `GardenFence_00` e `RiverBridgeArt_*` sem collider próprio. O censo de `TownKeyartComponentPhysics` isenta famílias sem receita física, então o PASS histórico de 963 suportes medidos e 2.661 isenções não prova que esses pontos bloqueiam ou deixam passar corretamente. O hash da TownScene mudou desde esse censo.
- Ausência no objeto não prova ausência de colisão no mapa: um collider de prédio, água ou footprint pode cobrir a base. Próxima prova headless: consultar `Physics2D.OverlapPoint`/`Collider2D.Cast` nos pés dos props, pilares, poço, cercas e corredores das pontes, com corpo do Player e física da cena atual; distinguir obstáculo decorativo de apoio sólido antes de editar geradores.

## NPCs e monstros

- `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleService.cs:493,568` retorna trajetória direta ao destino quando falha a rota no grafo. Isso permite tentativa contra obstáculo, apesar do caminho semanticamente inválido. Após esgotar replans, limpa o destino mantendo a ordem de movimento; verificar se causa nova tentativa/log repetido.
- `Assets/_Game/Scripts/NPC/NpcWanderer.cs:285` escolhe alvo aleatório sem testar alcance físico; pode insistir em posição separada por obstáculo.
- `Assets/_Game/Scripts/Enemy/EnemyMovementExecutor.cs:275` encerra movimentos especiais antes de aplicar avoidance. `Assets/_Game/Scripts/Enemy/EnemyLocalAvoidance.cs:142` usa raios pontuais, que não representam a largura do collider. São riscos de colisão/travamento, não comportamento observado nesta auditoria.
- Próxima prova: PlayMode headless com corpos/colisores reais para rota sem grafo, wander atrás de obstáculo, corredor estreito e movimento especial contra parede; medir progresso, interpenetração, replans e logs. Teste jogado de percepção da IA fica pendente por pedido de Rafa.

## Encaminhamento e limites

As specs abertas `spec_playtest_world_readability_v1`, `spec_farm_scene_collision_navigation_v1`, `spec_town_components_doors_and_collision_v1` e `spec_town_npc_visuals_navigation_v1` cobrem partes do problema; nenhuma prova os pontos específicos acima. Movimento de monstros requer fatia própria se os probes confirmarem defeito. Não autoriza mudança de física, IA ou visual por hipótese.

**Validação neste turno:** leitura de cena/código/assets e amostragem RGBA do PNG; Unity validation: NOT RUN. Reason: auditoria estática sem execução de editor ou interação direta. Command attempted: nenhum. Residual risk: Unity compile e comportamento físico/visual não validados neste turno.
