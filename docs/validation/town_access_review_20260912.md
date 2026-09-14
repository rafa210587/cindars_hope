# Town access review — 2026-09-12

> Histórico de diagnóstico. O resultado vigente está na [continuação de interação e entradas](town_interaction_followup_20260912.md): r7 PASS 23/23, após correção do registro multicollider e do probe. Os estados r2/r3 abaixo não são o resultado final.

## Estado da evidência

**FAIL no r2; correção do preparo ainda NOT RUN.** A execução `town_all_entries_20260912_223639/town-all-entries.json` parou em `House_Temple`, entrada 1/23: blocker fechado e sweep bloqueado medidos, mas o seletor externo não escolheu a porta. O JSON antigo não registrou objeto selecionado, distância ou lista de candidatos; portanto não prova se faltou registro do trigger ou alcance. O probe agora entra no sensor por `Rigidbody2D.MovePosition` e registra esses diagnósticos em falha. Nova execução sob coordenação da raiz é necessária; nenhum critério de PASS foi reduzido.

**FAIL no r3; correção direcional ainda NOT RUN.** `town_all_entries_20260912_224130/town-all-entries.json` mostra Temple parado em `(-32, 17.35)` mirando `(-32, 18.29)` através do blocker fechado; o sweep autoritativo parte de `DoorApproach=(-32, 17)` rumo ao interior `(-32, 21.5)`. A preparação usava `DoorSide` para calcular o lado externo, mas esse lado diverge da geometria materializada nesse lote. O probe agora deriva a direção externa de `DoorApproach - centro físico do blocker`. O timeout não foi aumentado nem o blocker ignorado; exige-se novo PlayMode para verificar seleção e ciclo completo.

O probe percorre os 23 lotes `walk_in_reveal`/`shop_hours_gated` (AnimalYard é a única exceção `exterior_only`). Mantém NPCs ativos, desabilita temporariamente só o `PlayerController` concorrente e usa o `Rigidbody2D`/`Collider2D` reais: `MovePosition` em `FixedUpdate` atravessa o vão, testa ocupação, fecha/reabre e sai. Mede blocker/animação, sweeps, reveal/recobrimento e o suporte no subtree de cada móvel. A interação é selecionada pelo `InteractionSystem` real após ticks físicos, não por `CanInteract`. O relógio real é posto em 12h para a abertura física das lojas e restaurado. C10 fechado/serviço segue `NOT_RUN` até roteiro dedicado.

## Lacunas concretas encontradas no código

- O comentário em `CreateMvpTownScene.cs:1639` e `:1661` ainda diz que os móveis são “andáveis”, mas `CreateInteriorProp` cria `BoxCollider2D` não-trigger para tudo exceto tapete em `:3089-3098`. A implementação vence o comentário; remover/corrigir essa contradição evita manutenção errada.
- Quase todos os interiores são compostos pelo mesmo núcleo genérico cama+mesa (`:1640-1656`); funções especiais só acrescentam altar/banco ou balcão (`:1675-1683`). Não há, nesse gerador, composição distinta comprovável para prisão, arquivo, registry, guarita ou animais. Isso não é defeito de collider, mas não satisfaz por si só “coerente com a função”.
- `House_AnimalYard` recebe piso de grama (`:1542`) e é excluído de telhado/interior/reveal (`:1902-1906`): a exceção exterior está coerente; não criar interior/telhado fictício.
- A lista de renderers do reveal é filtrada por `isHero` (`:1892-1900`). O probe falhará objetivamente se qualquer walk-in não tiver piso/móveis configurados nessa lista.

## Resultado esperado do runner

PASS exige 23/23 entradas; qualquer lote não alcançado, animação que libera antes do final, reveal por posição externa, móvel sem suporte visível/sólido ou saída física bloqueada permanece FAIL/NOT_RUN no JSON, sem promoção implícita.
