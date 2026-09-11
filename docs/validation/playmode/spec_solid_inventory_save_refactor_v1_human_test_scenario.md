# Cenário humano — refatoração de inventário e escrita de save

Status: NOT RUN / DEFERRED_TO_FINAL_VALIDATION. Tempo estimado: 10–15 minutos.
Objetivo: confirmar interação visual e persistência preservadas; nenhum comportamento novo esperado.

## Scene e estado inicial

Town/Farm existentes, UI de inventário disponível. Usar perfil/save descartável de teste;
não sobrescrever progresso real. Separar duas pilhas do mesmo item, uma pilha de outro item,
um slot vazio e um equipamento equipado. Não regenerar cenas durante este teste.

## Fluxo principal

1. Mover pilha inteira para slot vazio: quantidade e ícone acompanham, origem fica vazia.
2. Mesclar pilhas do mesmo item: destino respeita stack máximo e excesso fica na origem.
3. Trocar itens diferentes: ícones/quantidades trocam, posições não desaparecem.
4. Dividir pilha ímpar: metade inteira no primeiro slot vazio, total conservado.
5. Conferir atualização imediata de tooltip/contagens; sem refresh duplicado visível.

## Recusas

1. Tentar mover item equipado ou mesclar em pilha cheia: feedback atual de recusa, sem perda.
2. Tentar dividir item único ou inventário sem slot vazio: sem mutação.
3. Fechar/reabrir inventário: valores consistentes; foco/modal continuam funcionando.

## Save/load

1. Salvar o perfil de teste, alterar a disposição, salvar novamente e recarregar.
2. Conferir disposição, quantidades, equipamento e demais progressos do perfil.
3. Em Town, verificar oferta/entrega normal de suprimentos da Thalindra; ID/quest iguais ao anterior.
4. Não corromper/remover save manualmente neste smoke; recuperação é coberta pelos testes isolados.

## Console e aceite

Nenhum erro inesperado de inventory/save/wiring. Registrar warnings preexistentes separadamente.

- [ ] Move/merge/swap/split preservam totais e apresentação.
- [ ] Recusas não alteram estado.
- [ ] Save/load preserva disposição/equipamento e quest.
- [ ] UI/foco/tooltip não apresentam regressão.
- [ ] Console sem erros novos.

Tester/data/resultado: ainda não executado. Evidências visuais e observações a preencher pelo humano.
