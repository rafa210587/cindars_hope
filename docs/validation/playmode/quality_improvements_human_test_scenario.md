# Cenário integrado — melhorias de qualidade

Status: NOT RUN pelo humano nesta sessão. Complementa as fixtures automatizadas;
não exige repetir o smoke inteiro por spec.

1. Abrir inventário com uma unidade consumível. Usar: quantidade zera, efeito acontece
   uma vez e mensagem contém o ID/nome esperado. Item sem handler fica indisponível.
2. Adicionar itens a uma pilha parcial com slot vazio anterior: completar parcial antes
   de preencher vazio. Inventário cheio recusa sem perder/duplicar itens.
3. Dividir, mover, mesclar e remover pilhas; conferir quantidade total, equipment/binding,
   atualização de HUD e retorno de foco ao fechar o painel.
4. Soltar item com spawner disponível: pilha surge no mundo e sai uma vez do inventário.
   Guard de spawner ausente tem teste automatizado e não requer quebrar cena de produção.
5. No Editor, com cenas salvas carregadas e uma edição ainda dirty, executar validação.
   Conferir cenas/seleção/dirty preservados; problemas reais devem aparecer no resumo.
   Testar falhas em fixtures/cópias temporárias, sem apagar gates de cenas de produção.
6. Registrar resultado por passo, versão/commit ou fingerprint e captura somente quando
   necessária para evidência visual. Falha/NOT RUN continua explícito.
