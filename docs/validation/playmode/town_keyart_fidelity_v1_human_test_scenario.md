# Town keyart — cenário de gameplay

Status: **NOT RUN — roteiro preparado, não é aceite humano**. Revisão da cena e hashes devem ser preenchidos somente após a rodada final. Não usar resultados de revisão anterior para aprovar a próxima.

## Feature Summary

Verificar que a cidade reorganizada mantém NPCs, portas/interiores e colisões coerentes com o que aparece na tela. A comparação artística usa separadamente `docs/design/gameplay/city/TOWN_KEYART_ACCEPTANCE_RULE_v2.md`.

## Scenes / Required Initial State

- TownScene gerada pelo creator canônico, usando os inputs da revisão indicada no relatório final.
- Player no spawn da cidade, sem modal aberto; câmera normal de gameplay, sem ajustar zoom para a imagem parecer melhor.
- Não carregar nem sobrescrever um save pessoal para este teste. Não acionar atalhos de save/load. A captura automatizada isola SaveInput em memória e compara hashes; isso não equivale a teste de persistência.
- Console disponível para identificar erros. Não desativar NPCs, colisores, água ou telhados para passar o cenário.

## Scenario 1 — Porta, interior e retorno (3–5 minutos)

1. Caminhar até uma casa residencial e confirmar a entrada visível. Repetir depois no templo e no moinho, cuja porta é assimétrica.
2. Com a porta fechada, tentar entrar: a folha deve bloquear somente o vão desenhado, sem parede invisível do lado de fora da fachada.
3. Interagir com a porta. A abertura deve ficar visualmente legível; não basta desativar o collider enquanto uma segunda porta pintada continua fechada.
4. Caminhar pelo vão. O telhado deve revelar o interior; piso, móveis e paredes internas devem aparecer agora, sem necessidade de teleporte.
5. Circular no espaço livre, sair pela mesma porta e fechá-la. O exterior deve voltar ao estado anterior; não pode sobrar banco/móvel flutuando sobre o telhado.

Esperado: entrada e saída contínuas na mesma cena; nenhuma mudança de escala do personagem ou bloqueio invisível; nenhum erro no Console.

## Scenario 2 — Água, cais, portão e curral (3–5 minutos)

1. Seguir da praça ao lago/moinho pelas vias desenhadas.
2. Tentar entrar na água por uma margem sem ponte: o personagem deve parar na borda, não metros antes nem depois dela.
3. Atravessar as pontes e entrar no cais. O deck deve ser percorrível; seus limites não devem permitir sair sobre a água. Confirmar que a roda do moinho não é confundida com a porta.
4. Passar pelo portão sul nos dois sentidos e contornar seus pilares. Verificar correspondência entre o pé de pedra visível e o obstáculo físico; sem quadrados de bloqueio isolados na estrada.
5. Entrar no curral e contornar os abrigos/cercas. Não deve existir divisória invisível ou mobília doméstica sem contexto; entrar no pátio não deve apagar arbitrariamente todos os anexos.

Esperado: caminhos livres onde o desenho indica passagem e sólidos coerentes onde indica obstáculo. Sem flores sobre água nem cercas gigantes isoladas servindo de falso portão.

## Scenario 3 — NPCs e profundidade (3–5 minutos)

1. Na praça e no mercado, aproximar-se de dois NPCs canônicos e verificar indicação de interação, diálogo/serviço esperado e retorno ao jogo sem travar o movimento.
2. Observar o peregrino extra e os comerciantes ativos. Confirmar que não foram substituídos por decoração ou ocultados para melhorar a captura.
3. Observar ao menos um deslocamento de NPC perto de porta/banca e um trecho de agenda. Não deve ficar preso na parede ou atravessar balcão.
4. Passar à frente e atrás do mesmo tronco, banca e prédio. O personagem deve ser desenhado na profundidade correta, sem flores ou bancos surgindo acima do telhado.

Esperado: personagens legíveis na câmera normal, interações preservadas e movimento sem oscilar/travar. O censo automatizado de28 IDs canônicos mais o extra complementa, mas não substitui, essa observação.

## Expected Results / Console Expectations

- Sem crash, erros ou exceptions inesperadas.
- Portas, revelação de interiores, água, pontes, cais e portão correspondem à apresentação visual.
- NPCs e serviços continuam funcionais; nenhum collider foi removido para maquiar acessibilidade.
- Capturas fixas e casts não são descritos como prova de caminhada por input ou de fluidez de animação.
- Save/load funcional: **fora desta rodada**, pois a mudança não altera o sistema de persistência. Comparação read-only dos saves antes/depois permanece obrigatória no runner automatizado.

## Pass/Fail Checklist

- [ ] Revisão/hash da TownScene e da captura final registrados.
- [ ] Porta fechada bloqueia; aberta permite passagem e mostra abertura coerente.
- [ ] Interior revela ao entrar e volta a ficar coberto ao sair.
- [ ] Templo e moinho têm entradas visíveis e alcançáveis.
- [ ] Água sólida, pontes/cais percorríveis e limites coerentes.
- [ ] Pilares/muralha e cercas correspondem aos obstáculos.
- [ ] NPCs, diálogos/serviços e um deslocamento de agenda observados.
- [ ] Profundidade correta diante/atrás de obstáculos.
- [ ] Nenhum erro inesperado no Console.

Overall: **NOT RUN**. Tester: pendente. Date: pendente. Platform: Windows/Unity, versão a registrar.

## Notes

O template citado pela skill em `docs/05_VALIDATION/playmode/PLAYMODE_TEST_SCENARIO_TEMPLATE.md` não foi encontrado no projeto atual. Foram usadas as seções requeridas pela skill na pasta canônica existente `docs/validation/playmode/`, sem recriar a árvore antiga. Duração estimada:9–15minutos. Aprovação humana não será inferida de ferramenta concluída ou checklist ainda vazio.
