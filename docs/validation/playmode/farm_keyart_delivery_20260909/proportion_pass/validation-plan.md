# G04 — proporções, ponte e perímetro

Preparação read-only concluída. Unity NOT RUN nesta rodada: aguardando freeze conjunto e release do root. Nenhuma edição de C#, PNG ou YAML pelo validator.

## Baseline preservada

- Cena SHA256 `8E68982B60724243FBEF52823CF6B1C9343797788C4237B03166B9C3F940AB6C`.
- `before_diagnostic/` copia closing_pass/regen_04; `before_gameplay/` copia closing_pass/gameplay_04, com snapshot real da cena, logs, comandos, metadados e imagens.
- Backup `art/farm-pixelart-review/backups/20260909_112511-proportion-pass`: 366 arquivos, 149 PNGs, zero divergências entre hashes originais/copias. Manifesto em baseline-inputs.json. Fonte copiada representa o instante de preparação; hashes dos inputs efetivamente executados permanecem no process.json anterior.
- Baseline técnica: 8 vistas, 16 rotas, 6529 nós, quatro seleções reais; CustomAxis/Y nas oito vistas. Histórico de falhas e diferenças residuais de replay permanece em closing_pass.

## Medidas e riscos encontrados

`baseline-scale-inventory.json` usa exclusivamente a cena copiada e os PNGs/metas copiados. Extrai transforms, GUIDs, BoxCollider2D e dimensões de sprites simples; alpha >=0,05 descarta margens transparentes. Não mede portas desenhadas dentro de uma imagem nem substitui renderer.bounds lido no Unity. Sprites tiled são excluídos da estimativa.

Player: altura de sprite 1,1875u (33,53px na MainCamera640x480,ortho8,5), collider sólido0,640625×1,125u. ShippingBin: renderer2u, conteúdo opaco1,920u. Proporções opacas relativas à altura do player:

| Objeto | Altura opaca (u) | Alturas do player |
|---|---:|---:|
| Casa/roof | 7,017 | 5,91 |
| Barn | 6,801 | 5,73 |
| Coop | 4,837 | 4,07 |
| Estufa | 6,302 | 5,31 |
| Queijaria/vinho | 5,32 | 4,48 |
| Craft3 | 2,02 | 1,70 |
| Poço visual | 2,80 | 2,36 |
| Fonte | 4,076 | 3,43 |
| ShippingBin | 1,920 | 1,62 |
| Ponte, projeção vertical completa | 3,598 | 3,03 |

Estes são valores observados, não targets de design. Projeção vertical da ponte inclui perspectiva/deck; não representa altura física. Sua largura opaca6,398u excede o corredor físico4,6u. Root/worker devem definir novas dimensões com passagem visual apoiada emy2,5.

Roots de craft atuais têm scale aproximada0,60–0,675. SolidBody local0,75×0,6 produz cerca0,45–0,51×0,36–0,41u no mundo. Separar visual exige preservar conscientemente a geometria mundial. A porta da casa baseline está em(16;5,99), trigger1,6×2,22, blocker1×0,22. Seleção real passou com pés(16;4,5); não perder essa aproximação ao ajustar a imagem/volume.

Quatro limites sólidos habilitados são confirmados no snapshot: Top/Bottom66×1 com centrosy±22,5; Left/Right1×46 com centrosx±32,5. Faces internas emx±32/y±22. G03 deve comunicar esses bloqueios visualmente, preservar saída Townx30..32/y1,5..4,5 e portal Cave. As16rotas não provam ausência de toda possível lacuna no perímetro: conferir colliders/footprints serializados e enquadramento contextual. Alteração de seis árvores exige novas rotas, IDs únicos e conjunto71 preservado; árvores decorativas externas não podem virar recursos acidentais.

## Gates selecionados pela matriz canônica

1. Freeze e release explícitos; zero Unity concorrente. Snapshot/inputs antes e depois de cada execução. Regeneração via Editor API, dez diagnósticos, exit e scan único: compile e geração reportados separadamente.
2. Root revisa overview/regiões e escala/apoio/oclusão. Imagens rejeitadas são preservadas; nenhum PASS artístico por testes ou relato de worker.
3. Reavaliar equivalência dos11 testes focais (TreeNode3,Composition4,Planner4). Reusar grupos com inputs pertinentes equivalentes. Se Composition mudar para nova escala, executar somente esse filtro e outros contratos realmente afetados; não repetir global/compile-only redundante.
4. Play vigente: oito vistas originais MainCamera,16 rotas reais com paths/nós válidos, quatro seleções reais sem executar ações, CustomAxis/Y em cada vista. Nova escala deve ter evidência do player junto às portas e ponte; coleta complementar read-only pelo captureowner após autorização. Não alterar os probes para produzir PASS.
5. Medir novos objetos/colisores em Unity se a coleta for ampliada; comparação com este snapshot. Conferir71 TreeIndex/GUIDs/IDs e deltas autorizados de seis posições; limites/portal/corredor preservados.
6. Ponte nova raw==asset SHA256, Point/None/noMip, alpha, PPU/pivot e limite de import. Reusar imports inalterados por hashes de PNG/meta; preservar149PNGs anteriores, salvo substituição explicitamente autorizada.
7. Cena antes/depois/corrente==metadata e snapshot; zero erros runtime, saves preservados. Reuso de testes não implica reuso de screenshots com hash antigo. Docs consolidados pelo owner de closeout quando seus inputs estabilizarem.

Resultado atual: PREPARED, não SCOPED_PASS da nova rodada. Residual risk: Unity/cena nova ainda não existem; screenshots/seleção não demonstram input humano, execução de receitas, abertura das portas ou animação em movimento. Poço é decorativo. Aceite humano permanece separado da revisão visual root.
