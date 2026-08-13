# Cenário humano — mover e mesclar pilhas no inventário

## Setup

1. Abra a `FarmScene` e entre em Play Mode.
2. Execute `CindarsHope > Integration > Debug > Provision Farm Smoke Loadout` uma vez.
3. Confirme no Console `READY FOR PLAYTEST`.

## Fluxo

1. Pressione `I`, selecione as sementes de cenoura e escolha `Split`.
2. Selecione uma das duas pilhas, escolha `Mover/Mesclar`, selecione a outra e confirme.
3. Confirme que a pilha de destino recebe a quantidade, a origem fica vazia quando couber e o total não muda.
4. Divida novamente a pilha. Escolha `Mover/Mesclar` e selecione um slot vazio; confirme que a pilha muda de posição sem mudar a quantidade.
5. Inicie outro `Mover/Mesclar` e pressione `Esc` antes de selecionar o destino; confirme que nenhuma pilha muda.
6. Tente mover uma pilha equipada; confirme que a operação é recusada com mensagem e sem alterar os slots.
7. Selecione uma espada ou ferramenta e pressione `E`; confirme que as ações aparecem em uma caixa pequena sobre a grade e `Usar (indisponivel)` fica desabilitado. Pressione `Esc` e confirme que somente a caixa fecha.
8. Selecione uma poção de vida, escolha `Usar` e confirme que uma unidade é consumida e o efeito ocorre.
9. Selecione um material (por exemplo, madeira), escolha `Dropar`, saia do inventário e confirme que o ícone do pickup aparece visivelmente no chão, próximo ao jogador. Aproxime-se até a HUD mostrar `Pegar`, pressione `E` e confirme que a quantidade retorna ao inventário.

## Resultado esperado

- A ação `Mover/Mesclar` aparece para uma pilha selecionada.
- `WASD`/setas e `E`/Enter funcionam para selecionar o destino.
- Movimentos, merges e cancelamentos preservam os totais do inventário.
- O submodal de ações mantém a grade visível e não interativa; `Esc` retorna à grade sem fechar o inventário.
- Uso sem handler não tenta executar o item; poção consumível funciona.
- Drop só remove a pilha quando o pickup é criado; o pickup fica visível sobre o terreno, mostra o prompt `Pegar` e `E` o devolve ao inventário.
- Não há erro vermelho no Console.

## Reporte

Informe `OK` ou `FALHOU`, com origem/destino, quantidades antes/depois, mensagem exibida e qualquer erro completo do Console.
