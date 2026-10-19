# I — cais, pesca e acabamento dos caminhos

Status: PREPARED. Unity NOT RUN nesta rodada; aguardar freeze conjunto e release do root. Validator exclusivo, sem edição de runtime, PNG ou YAML.

## Baseline H preservada

- before_diagnostic = path_cave_pass/regen_04; before_gameplay = path_cave_pass/gameplay_02, com snapshots, logs, comandos e metadados.
- Cena SHA256 `1C6311A582901D2F69A60A3D3E90E43940E95C3D18E8D1485CA3C7944F764844`.
- Backup `art/farm-pixelart-review/backups/20260909_151601-dock-path-pass`: 372 arquivos, 151 PNGs, zero divergências de hash.

## Achados read-only e critério novo

O cais/barco é um único PNG de 1245×1263. Baseline: child Visual_FishingDock em (7;-12), scale mundial 0,68743287, PPU128, alpha >=0,05 com bbox (242,120)–(1215,1053). O conteúdo visível ocupa aproximadamente 5,226×5,011u, altura equivalente a 4,22 sprites do player (1,1875u). baseline-scale-inventory.json contém transforms e colliders efetivos.

dock-baseline-water-review.json projeta pixels alpha >=128 contra o polígono canônico do lago. Apenas 8,26% do sprite inteiro e 46,64% de um recorte aproximado do casco estão sobre esse polígono. A ROI do casco é explicitamente aproximada, não uma segmentação ou gate artístico; ela confirma a região do desencaixe para inspeção. Não houve edição raster.

FishingSpot baseline root (6,7;-9,5), trigger principal mundial 2,76×2,76 com centro (6,7;-8), não coincidente com root devido ao offset. CanInteract exige InventoryManager e faixa local outer 0,16/inner 0,055. _fishingSpotId está vazio e ResolveSpotId deriva a posição arredondada: mover o root sem preservar esse valor mudaria sua identidade. Blueprint I preserva explicitamente `farm_spot_7_-10`, sem schema novo.

O gate antigo fishing_approach pediu (5,7;-9,5) e alcançou (6;-8,5), com tolerância 1,25u. Isso não demonstrava andar no deck nem selecionar pesca. A foto lake mostrava player (3;-12), distante do apoio. As cinco seleções H excluíam pesca.

## Blueprint aprovado I01–I03

- Sprite upright, sem rotação 90°, altura visível 4,2u. Apoio raw (550,1058), contado de baixo, em DockEntrance (10;-8). FishingSpot root (10;-10), profile6 e ID legado explícito.
- Deck físico x[9;10,95], y[-11,15;-9,3], gangway x[9,55;10,45] conectado à margem NW. Lake visual preservado; LakeCollisionPath recorta apenas deck/acesso e NavigationRaster consome esse mesmo contorno.
- Caminhos: acabamento local e ramal em terra até DockEntrance; água e grade visual 0,25u preservadas. Máscaras compartilhadas podem reposicionar árvores; comparar os mesmos 71 IDs, sem esconder deltas.
- Mantidas 16 rotas. fishing_approach passa a exigir centro canônico do deck com tolerância 0,1u. Três consultas separadas com collider real devem confirmar bloqueio da água oeste/leste/sul do deck. Não considerar aproximação distante como chegada.
- Sexta seleção real: FishingSpot. Preservar casa, craft3 e cave; callbacks/CanInteract/GetCurrentInteractable reais, sem Interact, pescaria, input forçado ou save/load.
- Vista lake com player apoiado no deck; mesma MainCamera e parâmetros. Inspeção do root avalia pés, barco sobre água, encaixe do gangway, proporção e acabamento.

## Gates selecionados pela matriz

1. Backup, exclusividade e source freeze; geração via Unity API com dez diagnósticos, snapshot/hash e scan único. Falha de geração impede Play; preservar toda tentativa.
2. Focais por inputs: Planner, Level1Layout, Spatial e Navigation foram afetados; expectativa atual 31 anteriores + novo caso de deck/água = 32, confirmar XML em vez de presumir contagem. Reusar Composition/TreeNode apenas quando pertinente e por hashes, sem repetir global ou compile-only.
3. Play: oito vistas, 16 rotas com paths reais, três águas adjacentes bloqueadas, seis seleções e CustomAxis/Y. Casco/água requerem inspeção visual separada do PASS físico.
4. Auditoria antes/depois de sprite, alpha projetado, deck/collider/trigger/ID; mesmo contorno para raster e Lake físico. Não abrir água no restante do lago ou permitir circulação sobre o barco.
5. Preservar 151 PNGs/metas anteriores e registrar alterações autorizadas de assets derivados. Mesmo conjunto de 71 TreeIDs e contagem de jardins, documentando mudanças de posição causadas pelas máscaras.
6. Cena atual == metadata antes/depois == snapshot; fonte sem drift, saves preservados, zero erros runtime. Documento scoped diff/quality e links afetados após freeze documental; nada de auditoria global de dívida sem novo risco.

Limites: seleção não prova pescaria, consumo de item, minigame ou persistência de captura. BFS/fotos não provam input humano ou animação em movimento. Aceite humano permanece separado da revisão do root. Critério artístico só fica concluído após inspeção das imagens finais.
