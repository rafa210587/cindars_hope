# Farm v20 — vegetação baixa

Status: INTEGRATED_SCOPED_PASS. [Antes/depois](index.html). [Spec/plan/tasks](../../../../.specs/a_implementar/spec_farm_groundcover_v20.md).

Adicionados 27 arbustos baixos e 54 tufos da vegetação existente, distribuição determinística e espaçamento mínimo 2,4u entre os novos acentos. Alturas 0,65–0,9u e 0,3–0,45u. Reutiliza TryRegionalPlant com máscara do retângulo opaco contra caminhos, água, cultivos, construções, cercas e acessos; área frontal dos animais reservada adicionalmente. Não altera arte, PPU, runtime ou save.

Critérios: mais cobertura baixa OK (81 objetos); sem física OK (zero colliders); abaixo do jogador OK (Ground/4); caminhos protegidos OK (guard existente na geração + testes); árvores preservadas OK (71 transforms com hash idêntico ao v19).

Validação: Unity 6000.5.7f1, `FarmSceneCapture.RegenAndCapture`, exit 0, generate.log. `FarmDecorationPlannerTests;FarmSceneNavigationContractTests`: 15/15 PASS, editmode.xml/log. Scene-check.json confirma 81 renderers/order4 e zero colliders por leitura da cena serializada; nenhuma edição manual YAML. Árvores: trees-after.json SHA256 487BF17A4A6103C6A0B1180EC03BFE0A0D743ED72B337C53CF6661C244A7F7FD. Capturas generated/ na mesma câmera da v19.

Play Mode: NOT RUN nesta rodada visual sem física nova. Testes não substituem aceitação humana da densidade e legibilidade. Promoção NO: aceitação visual humana pendente; não há afirmação de aprovação global da fazenda.

Revisão independente visual: sem bloqueadores; caminhos, entradas e cultivos continuam legíveis. Limitação estética: os tufos reaproveitam arbustos reduzidos e parecem pequenas moitas arredondadas, sem nova arte específica de lâminas de grama.

