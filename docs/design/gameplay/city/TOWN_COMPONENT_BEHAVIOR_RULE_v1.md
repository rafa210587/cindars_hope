# Town — contrato dos componentes visuais e físicos

Status: refinamento operacional solicitado em2026-09-10; ainda não implementado/aceito. Spec: `.specs/a_implementar/spec_town_components_doors_and_collision_v1.md`.

## Regra central

Um objeto deve ter aparência, suporte físico e comportamento coerentes. Ser visível não implica automaticamente ser interagível. Ser sólido exige collider na sua base; telhado/copa/sombra não equivalem a uma parede ocupando a imagem inteira. A classificação deve ser explícita no gerador e no censo de validação, não inferida de número de SpriteRenderers.

| Componente | Física | Comportamento e leitura |
|---|---|---|
| Parede/fachada | Base sólida, vão real na porta | Não atravessar lateral/fundo; volume elevado pode encobrir por sorting |
| Porta/folha | Blocker fechado; passagem livre aberta | Animação one-shot, dobradiça fixa, sem porta fechada pintada na fachada; preservar interação dos dois lados |
| Telhado/cobertura | Sem collider volumétrico de telhado | Revelar interior quando o corpo entra, recobrir quando sai; não por alcance de interação |
| Piso interior | Caminhável | Mostrar piso/móveis no mesmo lugar, sem teleporte implícito |
| Móvel, banca, banco, caixa sólida | Collider na base ocupada | Manter corredor e área de aproximação; não inventar ação de interação quando não existe |
| Árvore | Tronco/base sólida, copa sem barreira | Personagens passam atrás/à frente com sorting; nenhuma árvore física flutua longe do collider |
| Pedra, muro, cerca, pilar | Suporte sólido correspondente | Cercas formam perímetro com aberturas, não obstáculos soltos ou invisíveis na rua |
| Fonte/canteiro elevado | Bacia/borda/base sólida | Caminhar ao redor, não atravessar o centro; arco visual e shape concordam |
| Rio/lago | Água bloqueada | Margem legível; nenhuma faixa seca invisível sobre água |
| Ponte/cais | Deck caminhável, laterais quando desenhadas como barreira | Conectar margens/acesso; collider da água não corta o deck |
| NPC/Player | Footbox e triggers separados | Perfil Town local; colisão mundial preservada no piloto; sprites proporcionais e animators existentes |
| Grama/flor rasteira/sombra/efeito | Sem bloqueio | Decoração explicitamente classificada; não transformar cada pixel decorativo em obstáculo |

## Porta e interior

A imagem fechada aprovada continua como referência imutável. A versão jogável separa fachada/vão e folha; pode usar camadas/frames/Animator segundo a API compartilhada, mas não esconder a porta pintada com um retângulo mal alinhado. Abrir e fechar têm movimento perceptível com registro consistente. O estado final da arte concorda com o collider. Repetir interação e NPC atravessar durante a transição não deixa estado impossível; a política de reversão/fechamento seguro vem do runtime compartilhado revisado.

O interior deve ser utilizável: entrada desobstruída, piso e mobiliário visíveis ao entrar, colisões dos móveis, caminho de volta e telhado restaurado. Uma captura do exterior com o collider desligado não comprova nada disso. A captura automatizada deve observar passos de física normais e a API pública; teleporte de enquadramento, quando usado, é declarado e não é chamado de caminhada por input.

## Censo e testes

Cada objeto sólido materializado terá identidade/categoria, renderer de suporte, colliders e estado esperado; cada exceção decorativa terá motivo. Revalidar a cena com esses shapes finais, não com obstáculos omitidos de um grafo simplificado. Verificar duas entradas, todas as portas e work anchors, NPCs, troncos, pontes/cais, currais e móveis. Registrar arte estática, reprodução animada externa, PlayMode e teste humano como evidências distintas.

O resultado visual segue a regra de proximidade v2. Pontos de fidelidade não são concedidos por quantidade de colliders ou frames; gates físicos também não podem ser ignorados porque a imagem ficou mais bonita.
