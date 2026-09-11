---
name: chatgpt-web-sprite-gen
description: Conduz geração de sprites pela interface web do ChatGPT quando esse canal for solicitado ou escolhido no escopo. Confere referência, envio, arquivo entregue e falhas sem presumir APIs de browser.
---

# Skill: Geração de Sprites via ChatGPT Web

O projeto possui staging de arte gerada; o acesso web depende das ferramentas presentes na sessão.

**Regra central: usar a interface e capacidades realmente disponíveis, preservando referência e origem da entrega.**

## Quando usar
- O usuário pede geração via ChatGPT web ou esse canal foi escolhido para uma peça/lote.

## Checklist essencial
- [ ] Brief e referência do alvo definidos antes do envio.
- [ ] Ferramenta de browser disponível e sua documentação consultada.
- [ ] Envio confirmado; nova imagem identificada pelo conteúdo.
- [ ] Arquivo entregue aberto e origem registrada.
- [ ] Falhas tratadas com limite; fila pendente preservada.

## Procedimento
1. Se necessário, defina o brief com [pixel-art-direction](../pixel-art-direction/SKILL.md).
2. Prepare o prompt com [pixel-art-prompt-authoring](../pixel-art-prompt-authoring/SKILL.md).
3. Leia [operação web](references/web-operation.md) quando for efetivamente gerar ou baixar.
4. Revise a entrega com [visual-asset-review](../visual-asset-review/SKILL.md); folhas animadas também precisam de `sprite-animation-review`.
5. Registre staging e peças restantes. Uma geração entregue não equivale a asset importado.

## Quando NÃO usar
- Apenas escrever prompt, revisar imagem ou integrar Unity.
- Canal nativo ou local já escolhido pelo usuário: usar a ferramenta/skill desse canal.

## Quando parar e reportar
Sem browser compatível, informe a limitação; não invente API nem troque o canal solicitado silenciosamente.
Rate limit ou erro persistente deixa peças pendentes; não agenda retomada nem cria gasto adicional fora do pedido.

## Saída esperada
Referência/prompt enviados, imagem e origem verificadas, path de staging quando disponível,
resultado da revisão e pendências. Não prometer sucesso na primeira geração nem custo menor sem medição.
