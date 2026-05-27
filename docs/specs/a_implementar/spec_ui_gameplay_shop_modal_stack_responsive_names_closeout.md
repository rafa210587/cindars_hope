# SPEC 17F - Modal Stack Hygiene, Responsive Shop UI e Short Item Names

**Status:** Implementado em codigo - compile Unity e Play Mode pendentes
**Ordem de execucao:** 17F
**Depende de:** SPEC 17E implementada em codigo
**Bloqueia:** fechamento humano final de shops/UI gameplay

## /speckit.specify

Eliminar `Modal type mismatch` em compra/venda e tornar os paines de shop legiveis em resolucoes menores, com nomes curtos nas linhas e detalhes separados.

## /speckit.plan

- Adicionar pop condicional da modal ativa e fechamento visual sem mutar stack.
- Corrigir `NpcShopController` para encerrar apenas o painel modal ativo.
- Adicionar formatacao curta/tooltip de itens e painel de detalhes para buy/sell.
- Atualizar o gerador da Town com scroll e layout compacto responsivo.
- Validar o contrato modal por Editor utility e registrar gates executados.

## /speckit.tasks

- [x] Registrar causa raiz e baseline.
- [x] Implementar higiene da modal stack.
- [x] Implementar nomes curtos/detalhes e layout de shop.
- [x] Implementar validator Editor de modal flow.
- [x] Executar validacoes automaticas disponiveis.
- [x] Registrar pendencias de Play Mode final.
