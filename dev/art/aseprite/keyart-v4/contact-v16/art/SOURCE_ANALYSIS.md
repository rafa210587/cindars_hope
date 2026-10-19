# Contact v16 — análise de chão e estrada

Status: proposta offline, nenhuma arte candidata gerada ou aplicada. Autoria depende da spec v16 do root e da calibração de `pixel-audit.md`. Ownership desta fatia: somente `contact-v16/art/`.

## Fontes atuais abertas

| Fonte | Canvas / alpha | SHA256 | Contrato atual |
| --- | --- | --- | --- |
| `Assets/_Game/Art/Generated/World/tiles/ground_path_aseprite_v1.png` | 64×64 RGBA, alpha255 integral | DD33C3A632985A8E1AD8038BBB22D2263EE40C1149FCA785A89E34ED74E9EE89 | FarmPathPalette:32PPU, atlas16×16 de slices4×4; contorno feito pelo tilemap |
| `Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png` | 1254×1254 RGB, opaco | 6B5419E57A843071D26F1B2F58FC1F4D079D170E5BAF4278945ADD3231299E4A | Import atual128PPU, célula1254/128=9.796875u; FarmGround tint(.8,.85,.78) |

Referência aberta: `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png`. Evidência atual aberta: `docs/validation/farm_keyart_v4/gameplay_stage15/homestead.png`,640×480. As construções têm grupos de cor que explicam tábuas, pedras e telhas; o caminho atual distribui marcas pequenas de contraste semelhante por toda sua área. A grama combina grão fino no fundo com muitos tufos repetidos embutidos. Na referência, centros de trilha deixam intervalos de terra legíveis e a vegetação mais forte concentra-se em margens e conjuntos.

O auditor confirmou câmera gameplay ortho8.5:28.2353pixels/u. Portanto1pixel da estrada ocupa nominalmente0.88235pixel de tela e1pixel do arquivo de grama0.22059pixel. Isto mede amostragem, não o tamanho dos clusters visuais: a grama tem blocos e tufos com vários pixels. Um tratamento por pixel isolado ou resize global não resolve automaticamente a hierarquia e pode ampliar a diferença com player/construções. As demais medidas comparativas pertencem ao relatório do auditor.

## Uma proposta limitada

Após autorização da spec, criar somente um estudo Aseprite com baseline preservado e camada de correção:

1. Estrada64×64: manter cores médias e areia quente; atenuar grãos isolados no centro e manter4–6 grupos irregulares de2–5pixels entre áreas tranquilas. Não criar faixas direcionais, porque o atlas é compartilhado por caminhos em todas as direções. Preservar os4pixels externos sem alterações, alpha integral, dimensão e orientação. Esse estudo é o candidato completo da estrada.
2. Grama: examinar um recorte central256×256 da fonte, somente como amostra diagnóstica. Atenuar contraste de pequenos grãos do fundo e alguns tufos próximos; preservar3–4 conjuntos maiores assimétricos e a vegetação já existente. O recorte não pode substituir o tile1254×1254. Uma eventual entrega integral requer manter canvas original e suas bordas; não está autorizada nesta análise.

Não modificar pixels por blur/interpolação, PPU, importer, tints, máscaras de caminho, buffers, árvores, farming, IDs, colliders ou layout. Não alterar o shader nem introduzir materiais. A preservação da borda mantém o baseline de costura, não prova que a costura anterior era perfeita.

## Verificação proposta

Comparar baseline/amostra a100% e na densidade de gameplay, usando a mesma câmera e enquadramento. A estrada deve ter mais intervalos calmos entre marcas; a grama deve parar de competir com os detalhes úteis dos props. Reabrir o candidato nativo, conferir canvas/alpha/borda e hashes originais. Conferência offline não certifica aparência integrada nem flicker durante movimento.

Unity validation: NOT RUN. Reason: análise offline sem geração ou integração autorizada nesta etapa. Command attempted: nenhum. Residual risk: aparência em jogo ainda depende da captura e integração do root.
