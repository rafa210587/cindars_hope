# Operação web com ferramentas disponíveis

## Sessão e envio
- Descubra o browser/tab pelo mecanismo documentado da sessão. Use destino solicitado ou
  contexto existente observado; não embuta URL pessoal ou ID fixo de projeto nesta skill.
- Leia a documentação da ferramenta de controle disponível antes de usar seletores ou ações.
  APIs legadas de Chrome, acesso a DOM e download autenticado não são capacidades presumidas.
- Anexe/selecione referência quando suportado e confirme que o prompt foi enviado uma vez.
  Um placeholder ou imagem anterior na página não comprova conclusão do novo turno.
- Observe estado com esperas limitadas, compatíveis com a ferramenta e mensagens de progresso.
  Não imponha teclas, formato em linha única ou seletores estáticos sem evidência da interface atual.

## Entrega
- Use download/export suportado pela ferramenta/interface e confira o arquivo efetivo.
  Não contorne políticas de mídia, sessão ou URLs com fetch inventado.
- Abra a imagem baixada: sujeito, versão e conteúdo precisam corresponder ao turno esperado.
- Preserve original em staging adequado: mundo usa `art/world_gpt/raw/`; NPC walk usa
  `art/npc_anim_gpt/raw/`. Confirme colisões de nomes antes de substituir arquivo existente.
- Objeto isolado pode exigir alpha; tile contínuo normalmente preenche a área.
  Não remova fundo de textura automaticamente. Pós-processamento usa ferramenta existente
  inspecionada e autorizada; nunca alegue ter usado script de scratchpad ausente.

## Falhas e limite de tentativa
- Diferencie limite de uso, envio não confirmado, rede, recusa e render incompleto pelo estado observado.
- Em falha transitória, faça no máximo uma nova tentativa depois de verificar que não há geração ativa
  nem resultado já entregue. Se persistir, reporte estado e preserve fila para retomada.
- Para rate limit, respeite o prazo informado. Se a espera ultrapassar a janela de trabalho,
  registre quando poderá retomar; não use cooldown fixo de dez minutos nem loop ilimitado.
- Retomada agendada só se solicitada/autorizada pelo usuário, via ferramenta de automação disponível.
- Para recusa, não repetir o mesmo pedido em loop; registre a limitação do canal.
