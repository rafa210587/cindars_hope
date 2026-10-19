# Ambient v15 — candidatos de arte

Entrega fora de Assets, sem alterações Unity ou runtime. Abrir `preview.html` para os GIFs e folhas. Cada nome abaixo possui `.aseprite` em camadas, `.png` horizontal, `.gif` e `.json` Aseprite array, sem trim.

| Nome | Canvas por quadro | Quadros | Duração por quadro | Folha |
| --- | --- | --- | --- | --- |
| fountain_ambient_v15 | 170 × 159 | 5 | 140 ms | 850 × 159 |
| cascade_ambient_v15 | 68 × 56 | 5 | 140 ms | 340 × 56 |
| fish_jump_v15 | 48 × 48 | 8 | 120 ms | 384 × 48 |

Fonte e cascata preservam canvas e alpha do original em todos os quadros. Seus pixels fora da máscara de água são idênticos ao original; estrutura, pedras e pontos de apoio não se movem. Cada arquivo nativo inclui uma camada oculta com a fonte preservada. Os hashes das fontes estão no `manifest.json`.

Na fonte, os brilhos sobem pelo jato central, descem nos jatos laterais e formam arcos curtos de impacto no tanque. Na cascata, as faixas de brilho avançam para baixo em fases periódicas e a espuma varia apenas dentro da água existente. Não há deslocamento da silhueta inteira.

O peixe percorre saída, subida, ápice, descida, entrada, respingo e dissipação; o oitavo quadro é inteiramente transparente. Ponto de contato com a água: **(24, 34) em origem superior esquerda**, equivalente a **(24, 14) em origem inferior esquerda**, pivot normalizado **(0.5, 0.2916667)**. PPU sugerido 18.3, escala 1. O intervalo invisível de 20 segundos pertence ao clip Unity; o GIF de revisão repete apenas a ação de 0.96 segundo.

## Evidência e limites

- Quadros exportados abertos e examinados: identidade, progressão de poses e estrutura fixa.
- Nativos reabertos: dimensões, camadas, tags, duração; último quadro do peixe transparente.
- Comparação pixel a pixel da fonte e cascata: alpha invariável e RGBA da estrutura invariável nos cinco quadros.
- JSON e GIF: contagem e durações conferidas; todos os quadros adjacentes contêm mudanças reais, incluindo a transição final ao primeiro.
- Todas as folhas têm alpha binário real, sem pixels parcialmente transparentes. Estatísticas por quadro e hashes no `manifest.json`.
- Playback contínuo observado: **NOT RUN** por este executor; revisão realizada por poses estáticas e metadados, sem alegar validação de fluidez.
- Unity validation: **NOT RUN**. Reason: integração e janela Unity pertencem ao root. Command attempted: nenhum. Residual risk: proporção, fluidez e composição em cena ainda requerem a captura do integrador.

`author.lua` registra a autoria Aseprite; `verify-native.lua` verifica arquivos nativos; `inspect-delivery.py` faz somente leitura de imagens e metadados. As subpastas `geometry/` e `wiring/` pertencem aos outros executores e não fazem parte desta entrega de arte.
