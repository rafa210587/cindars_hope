# Human Play Mode Scenario — fable_63 (New Game Intro + Main Quest Hook)

> Pre-requisito: regenerar a FarmScene no Unity Editor (gerador CreateMvpFarmScene) para a
> carta (BedLetter) existir em cena. Executar com um SAVE NOVO (sem slot_1.json carregado).

## A. Intro 1x com skip (CA-1)

1. Na tela de titulo, escolher **New Game**.
2. ESPERADO: a sequencia de abertura abre (3-5 telas de texto PT-BR). O player NAO se move
   enquanto a intro esta aberta (input de gameplay bloqueado / modal Dialogue ativo).
3. Pressionar **E/Enter** avanca uma tela por vez; na ultima, [E/Enter] = "Comecar".
4. ESPERADO ao terminar: a intro fecha e o gameplay libera (player move).
5. Reabrir o jogo (Continue do mesmo save) ou dormir/reload.
   ESPERADO: a intro **NAO** reaparece (1x/save).
6. New Game de novo + na 1a tela pressionar **Esc**.
   ESPERADO: a intro pula inteira e libera o gameplay imediatamente.

## B. Carta na cama (CA-3)

1. Apos a intro, ir ate a cama (canto da casa, ~(-5,-7.6)).
2. ESPERADO: ao lado da cama ha a carta (BedLetter) com prompt "Ler a carta".
3. Interagir: ESPERADO um toast/feedback com o texto da carta apontando Corvus na cidade.
4. Reinteragir: ESPERADO prompt muda para "Reler a carta" (estado lido persistido).
5. Confirmar que interagir com a CAMA (dormir) e com a CARTA sao acoes distintas (sem
   sobreposicao do trigger).

## C. Oferta idempotente da mq_act1_00 (CA-2)

1. Apos a intro, avancar o 1o dia (dormir na cama OU TAB de debug).
2. Abrir o Diario de Missoes (J).
   ESPERADO: a missao "Cartas de Cindar's Hope" (mq_act1_00) esta ativa, objetivo TalkToNpc Corvus.
3. Salvar, sair para o titulo, Continue, avancar outro dia.
   ESPERADO: a missao NAO foi duplicada (continua uma so; nao reabre se ja concluida).

## D. Ponte para Corvus (CA-4)

1. Ir a cidade e falar com Corvus.
   ESPERADO: o objetivo TalkToNpc completa; a mq_act1_00 fica pronta/concluida e concede a
   flag de conclusao.
2. ESPERADO: a cadeia mq_act1_01 (Corvus) so fica disponivel apos mq_act1_00 concluida
   (prerequisite).

## E. Ordem do 1o dia (anti-empilhamento)

1. ESPERADO no 1o dia: intro fecha PRIMEIRO; depois eventuais hints (F62); a carta e pull
   (o jogador interage quando quiser) — nenhuma tela empilha sobre outra.

## Checklist de input/modal (regras gerais)

- [ ] open/close da intro (E/Enter/Esc)
- [ ] Esc fecha/pula de qualquer tela
- [ ] input de gameplay bloqueado com a intro aberta (player parado)
- [ ] sem movimento do player enquanto modal aberto
- [ ] estado vazio: New Game sem save anterior funciona
- [ ] reload nao re-exibe a intro
