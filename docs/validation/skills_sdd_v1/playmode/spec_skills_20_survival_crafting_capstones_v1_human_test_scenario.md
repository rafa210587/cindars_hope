# Cenário humano — capstones Telisandra e Thoren

**Status:** NOT RUN  
**Plataforma:** Windows / Unity 6000.5.7f1  
**Tempo estimado:** 15–20 minutos

## Objetivo

Confirmar em jogo que Nascido da Caverna e Forja Viva produzem efeitos perceptíveis, respeitam seus
limites por run/dia e preservam escolha, inventário e save sem bloquear ações comuns.

## Estado inicial

- Build de teste com acesso a Farm, Town e Cave.
- Telisandra R3 e Thoren R1/R2/R3 disponíveis por respec ou save de QA.
- Ingredientes para um equipamento durável, um pão/consumível com payload elegível e dois crafts
  comuns. Deixar também espaço para preencher o inventário.
- Console limpo antes de cada cenário.

## 1. Telisandra na Cave

1. Entre em uma run nova e registre o `runId`/seed mostrados pelo diagnóstico de QA.
2. Reduza HP de 30% para 24%. Confirme ativação única antes da janela de Último Fôlego.
3. Observe por 10 s: custo de corrida/dodge reduzido, dodge com +0,4 tile e resistências apenas das
   famílias ambiental/mental.
4. Saia de combate durante o buff e compare a cura natural: 2 HP/s; após o buff: 1 HP/s.
5. Cruze também STA<15% e Exausto na mesma run. Confirme que não reativa.
6. Saia e volte ao mesmo nível. Confirme layout, inimigos, recursos, seed e consumo preservados.
7. Salve/carregue. Confirme que a carga continua consumida. Após derrota e nova run, confirme rearm.

## 2. Thoren: escolha e progressão

1. Abra o modal de crafting com a carga diária disponível.
2. Deixe `Não usar` selecionado e faça um craft comum. Confirme sucesso e carga ainda disponível.
3. Em R1, selecione Qualidade e fabrique equipamento: output Q1 e durabilidade base 100→105.
4. Em novo dia e R2, selecione Qualidade: output Q2 e durabilidade 100→110.
5. Em outro novo dia e R2, selecione Poupar Material; escolha um ingrediente comum com quantidade
   maior que um. Confirme economia exata de uma unidade e pelo menos uma unidade consumida.
6. Tente usar novamente no mesmo dia, inclusive em outra estação. Confirme que a carga não reaparece.

## 3. Thoren R3, falha e retry

1. Em R3, escolha Qualidade e fabrique equipamento base 100. Confirme output Q2 e durabilidade 119
   após qualidade e bônus adicional.
2. Em novo dia, escolha Poupar Material para equipamento base 100. Confirme material economizado e
   durabilidade 108.
3. Inicie um job temporizado com Forja Viva e cancele. Confirme ingredientes restaurados e carga livre.
4. Repita o job, preencha o inventário antes da coleta e tente coletar. Confirme output aguardando na
   estação e carga reservada, ainda não consumida.
5. Libere um slot e colete. Confirme um único output e carga consumida uma única vez.
6. Salve enquanto outro job aprimorado está em andamento; carregue, conclua e colete. Confirme que
   escolha, output e reserva foram preservados.

## 4. Consumível funcional

1. Use a receita registrada de pão ou outro consumível com payload numérico positivo, em lote de até cinco.
2. Em R3 + Qualidade, confirme variante Q2+potência: `BaseValue` igual ao item base e payload
   `base × 1,35 × 1,08` (pão Hunger 30→44).
3. Tente lote seis ou output sem payload escalável. Confirme recusa da Forja Viva antes de reservar a
   carga; selecione `Não usar` e confirme que o craft normal continua disponível.

## Resultado esperado

- Nenhum crash, exception ou erro de console.
- Feedback de escolha e indisponibilidade é legível; craft comum nunca fica bloqueado.
- Usos únicos persistem e não são rearmados por load/respec.
- Cancelamento, inventário cheio e retry não perdem nem duplicam recursos/output.
- Revisita da Cave não altera a stable run.

## Checklist

- [ ] Telisandra ativa uma vez por run e respeita precedência/regen.
- [ ] Revisita e save/load preservam stable run e consumo.
- [ ] Opt-out de Thoren funciona.
- [ ] R1/Q1, R2/Q2 e economia de material conferem.
- [ ] R3 entrega 119/108 nos dois caminhos de equipamento.
- [ ] Cancelamento e inventory-full retry são atômicos.
- [ ] Consumível Q1/Q2/R3 tem potência visível e preço de venda igual ao item base.
- [ ] Nenhum ERROR/Exception no console.

**Overall:** PASS / FAIL / BLOCKED
