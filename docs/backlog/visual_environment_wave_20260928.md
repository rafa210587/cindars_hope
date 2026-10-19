# Plano de wave — polimento visual dos ambientes

**Estado:** proposta de planejamento, sem implementação ou aceite visual. Base: capturas Unity de 27/09, keyarts aprovadas da Fazenda e Cidade, e rejeição humana do ajuste de paleta. O protótipo posterior de inlay na praça não apareceu na captura e foi descartado. Este plano não contabiliza esses passes como melhoria.

## Direção visual e limite

Usar [Stardew Valley](https://www.stardewvalley.net/media/) como referência de **leitura imediata do espaço cotidiano** (fazenda/cidade) e [Children of Morta](https://deadmage.com/games/children-of-morta/) como referência de **profundidade, atmosfera e separação de personagens/ameaças** (sobretudo Cave). São inferências de direção visual feitas a partir das imagens oficiais, não instruções para copiar sprites, paletas ou layouts. A identidade de Cindar's Hope continua ancorada nas [keyart da Fazenda](../art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png) e [Cidade](../art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png).

O problema atual é de **forma e composição antes de cor**. A [Cidade atual](../validation/visual_polish_20260927/evidence/town-overview-after.png) tem vias retas e muito gramado homogêneo entre construções; o [recorte atual da Fazenda](../validation/visual_polish_20260927/evidence/farm-after-path.png) tem caminho dominante em faixa larga e fileiras rígidas de props; a [Cave atual](../validation/visual_polish_20260927/evidence/cave-after-palette.png) revela quadrados de material e inimigos escuros pouco separados do piso. As duas capturas históricas da Cave não usam o mesmo seed e não constituem comparação antes/depois.

## Contrato de arte da wave

1. Definir uma folha de direção **em escala real de jogo**: tamanho aparente do pixel, perspectiva de chão/parede/prop, altura relativa ao personagem, clusters, contorno, sombra de contato e hierarquia de contraste. Registrar faixas de cor por material em contexto diurno/noturno, sem obrigar todas as cenas a usar a mesma paleta. Keyart gerada é referência de composição, não atlas pronto nem imagem de fundo.
2. Produzir **famílias modulares** no Aseprite: chão-base, borda/transição, variação interna e detalhe de contato. Manter `.aseprite` em camadas, dimensão/pivô/PPU documentados, alpha verdadeiro e export nativo; importar Point, sem compressão/mipmaps. ChatGPT/imagegen pode propor silhuetas e famílias de material, mas nenhuma imagem entra diretamente no jogo sem redução/edição, teste de repetição e revisão na câmera.
3. Construir profundidade por ordem de camadas e sombras de contato coerentes com props existentes. Na Cave, luz precisa aumentar a leitura de parede/entrada/ameaças; escurecimento não pode esconder emendas. Decoração baixa (mato/arbusto/flor) continua sem collider. Collider, rota, porta e interação seguem o contrato espacial existente.

## Ordem de produção (um dono de cena/Unity por vez)

| Etapa | Entrega verificável | Esforço indicativo* | Spec/contrato |
|---|---|---:|---|
| 0. Baseline e direção | Uma captura **fixa** de gameplay e uma visão geral atuais por área; mesma câmera/resolução/tempo, e mesmo seed na Cave. Folha de escala/perspectiva/materiais com 3 discrepâncias prioritárias por área. Separar defeito visual de física/UX. | 2–4 dias | Specs visuais ativas; captura da Cidade segue a regra de aceite v2. |
| 1. Piloto Fazenda | Refazer **um trecho completo** casa → bancas → ponte: largura, curva, borda grama/terra, sombra sob props e margem d'água. Autorizar a família modular apenas se o trecho, no mesmo frame, for visivelmente mais orgânico e manter circulação/ponte. | 3–5 dias | `spec_farm_pixelart_cohesion_and_ingame_review_v1`; reconciliar com `spec_farm_scene_keyart_macro_composition_v1` e `spec_farm_scene_keyart_visual_corrections_v1`. |
| 2. Fazenda inteira | Aplicar a família aprovada às rotas e seis regiões da keyart; corrigir proporções dos marcos e repetição de vegetação. Áreas cultiváveis vazias continuam espaço de gameplay, sem lavoura falsa. | 4–7 dias | `spec_farm_enclosed_valley_keyart_v2` e specs Farm citadas. |
| 3. Cidade | Tratar a **rede** de vias e não só a fonte: bordas curvas/chanfradas, bolsões entre bairros, praça conectada, água/lago e muralha legíveis. Depois unificar famílias de casas, bancas e sombra de contato. Preservar 24 casas, NPCs, âncoras, interiores e grafo físico. | 7–12 dias | `spec_town_keyart_fidelity_v1`, depois `spec_town_native_architecture_v1`; gate `spec_town_integrated_acceptance_v1`. |
| 4. Cave | Trocar a alternância de pisos completos com limites quadrados por **base contínua + variações internas/overlays de borda compatíveis**. Resolver encontro piso/parede, contraste de inimigos e leitura de recursos/saídas em duas seeds. Reusar bordas, litter e luz existentes; preservar snapshots/revisita. | 5–8 dias | `spec_cave_visual_polish_runtime`; **amendar escopo do piso** antes de implementar, pois a spec atual cobre borda/decor/luz, mas não esse tratamento de material. |
| 5. Integração | Captura fixa + revisão em escala de gameplay e mapa inteiro; passagens com personagem à frente/atrás de props, dia/noite quando aplicável, combate na Cave; correções finais limitadas aos defeitos observados. | 3–5 dias | Gates de cada spec e revisão humana final. |

\*Estimativa de planejamento para um artista técnico/implementador com Unity e Aseprite, **24–41 dias úteis** no total; depende da quantidade de arte nova e da disponibilidade de validação. O item genérico de 50–70 h no backlog antigo não cobre esta reconstrução ambiental. Não é prazo fechado.

## Critério para avançar de etapa

- **Piloto primeiro:** apresentar ao humano a captura anterior e a nova **do mesmo enquadramento**, mais uma visão geral, sem zoom/crop favorável. Mostrar também o `.aseprite`/atlas modular. Se o usuário não perceber melhora de forma/material em 5 segundos, reprovar o piloto; métricas de RGB ou teste verde não substituem esse julgamento.
- **No máximo três defeitos por rodada:** registrar o que mudou e o que permanece. Após duas tentativas sem melhora observável, interromper a abordagem e decidir outra técnica antes de mais geração/captura. A experiência do inlay invisível entra como tentativa descartada, não como avanço.
- **Cena integrada:** testar repetição do tile em área grande, pixel aparente junto ao personagem, limites de água/parede, footpoint/oclusão, legibilidade da rota e do objeto interativo. Captura isolada de sprite não aprova o ambiente.
- **Gates técnicos:** geração pelo Editor (sem YAML manual), import e referências, compilação/teste focal, colisão e portas/rotas afetadas. Cave exige seed/snapshot/revisit e comparação visual no **mesmo seed** se o materializer mudar. Nenhuma promoção com falha de gate relevante; separar falha preexistente.
- **Aceite visual:** Cidade segue a rubrica v2 (dois revisores >=80, mínimos por categoria e bloqueadores resolvidos). Fazenda e Cave precisam de revisão humana explícita da cena, além dos testes; não inventar percentual de similaridade com jogos comerciais.

## Dependências, riscos e sequência imediata

- As specs ativas já cobrem Fazenda e Cidade; não gerar novas por reflexo. Antes do piloto, fixar a folha de direção e conferir locks/alterações locais. O mesmo gerador e Unity não podem receber edições de duas lanes ao mesmo tempo. Arte isolada pode ser preparada em paralelo apenas sem tocar o lock da cena.
- A referência da Cave citada pela spec em `art/world_gpt/raw/cave_guides/` não está presente neste checkout. Localizar a fonte aprovada ou marcar fidelidade à keyart como **NOT RUN**; a legibilidade do render atual ainda pode ser julgada. A emenda da spec deve preservar o contrato FASE9F de stable run.
- Há falhas já registradas de acesso/conectividade na Cidade e validação integrada parcial. Mudanças de geometria de vias exigem reavaliar essas rotas; não atribuir automaticamente falhas antigas à arte nova nem declarar aprovação física a partir de screenshot.
- **Primeiro pacote a executar depois deste plano:** baseline reproduzível, folha de direção e piloto da rota casa→ponte da Fazenda. Só após uma comparação perceptível e aprovada expandir o atlas para o mapa; Cidade e Cave seguem na ordem acima.
