# FarmScene — fechamento técnico e revisão visual

Resultado vigente: **SCOPED_PASS**, com revisão visual do orquestrador. Aceite humano da arte e sessão completa por input permanecem pendentes. Cena materializada: `Assets/_Game/Scenes/FarmScene.unity`, SHA256 `8e68982b60724243fbef52823cf6b1c9343797788c4237b03166b9c3f940ab6c`.

## Evidência final

- `regen_04`: geração via Unity API e dez diagnósticos; processo e scan exit 0.
- `gameplay_04`: oito vistas reais da MainCamera, 16/16 rotas com collider real (6529 nós), quatro seleções reais após callbacks da física. Porta, bancada, forja e fogão selecionados; nenhuma ação executada artificialmente.
- Todas as oito vistas registram `CustomAxis` e eixo `(0,1,0)`. Novo adapter `CameraTransparencySort2D` restaura o override ao habilitar a câmera. O gate verifica a configuração real, sem aplicá-la pelo teste.
- Zero erros de runtime; hash da cena antes/depois/arquivo atual igual e saves preservados. Snapshots da cena e metadados estão nas pastas de execução. Nenhum Unity permaneceu aberto.
- `replay_01`: reabertura e captura da mesma cena, sem regeneração nem repetição de Play. Oito de dez PNGs são byte-idênticos, incluindo todos os seis recortes regionais. Os dois overviews variam 142 e 200 pixels; no overview completo a diferença fica numa pequena sobreposição de árvores ao sudoeste (bbox 281,941–293,972). Não constitui prova de determinismo integral da imagem. Ver `replay-comparison.json` e `replay-pixel-metrics.json`.
- Reuso dos 11 testes focais da continuação anterior por seis inputs equivalentes; novo comportamento de câmera coberto pelo gate Play atual. Nenhuma suíte global ou compile-only redundante. Ver `reused-evidence-review.json` e `final-audit.json`.

## Correções que a validação exigiu

1. `gameplay_01`: seleção da porta falhou, três estações passaram. A geometria deixava só 0,025 unidade de aproximação válida. Trigger local ampliado, mantendo blocker e alcance global; `gameplay_02` passou no mesmo probe.
2. Revisão visual encontrou sobreposição variável de copas. Sprites, posições e escalas das 71 árvores estavam preservados; modo ortográfico empatava profundidades.
3. `gameplay_03` ainda registrou Orthographic, embora o gate antigo desse PASS. O override aplicado apenas no Editor não persistiu no carregamento. Essa rodada não comprova a correção de sorting. O adapter de câmera e o gate explícito corrigiram a lacuna em `gameplay_04`.

## Revisão visual e limites

O orquestrador abriu a referência, overview, recortes regionais e oito frames de gameplay. Conferiu montanha/caverna, bosque/fonte, margens/lago/ponte, cultivo, casa/estufa e edifícios sul. Sulcos legíveis, detalhes fora das entradas, personagem apoiado no deck e oclusão por profundidade. A cena continua uma adaptação: canteiros vazios de novo jogo, alguns modelos e densidades de pixel diferentes da keyart. A ampliação no preview não acrescenta detalhe.

O poço continua visual; a prova não demonstra abastecimento. Seleção não demonstra abrir porta, produzir receitas, input humano ou animação em movimento. O aumento anterior de 50 para 71 árvores permanece documentado no relatório principal; esta rodada preservou essas 71, sem novos recursos. Nenhum save humano foi carregado ou gravado.

Spec permanece IN_PROGRESS / promoção NO por critérios humanos e animações ainda pendentes. F01–F05 desta entrega têm implementação e evidência técnica concluídas; nenhuma aprovação humana foi inferida. Não houve commit, push ou alteração global de GraphicsSettings/Town.
