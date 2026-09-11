# Farm v20 — arbustos baixos e tufos

Status: INTEGRATED_SCOPED_PASS. Pedido humano autorizado: mais arbustos e graminhas pelo mapa da fazenda. Aceitação visual humana pendente.

## Spec
Adicionar cobertura vegetal baixa nas clareiras usando a arte existente. Sem colliders; Ground/4 abaixo do jogador. Preservar caminhos, cultivos, água, portas, poço, fonte, cercas e área dos animais. Manter árvores, proporções e sistemas existentes.

## Plan
Reusar FarmSettlementVisualComposer.TryRegionalPlant e suas máscaras de footprint. Distribuição determinística, espaçada e irregular dentro da clareira; alternar arbustos 0,65–0,9u e tufos 0,3–0,45u da vegetação aprovada. Não gerar imagens nem alterar resolução/importação. Gerar cena por FarmSceneCapture.RegenAndCapture e comparar enquadramentos v19/v20.

## Tasks
- [x] Adicionar distribuição ao composer; inspeção visual do sprite reutilizado.
- [x] Gerar cena: 27 arbustos e 54 tufos, zero colliders, máscaras protegidas aplicadas.
- [x] Testes existentes de decoração/navegação 15/15 PASS; capturas e HTML antes/depois.

Scope: Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs, FarmScene.unity via gerador, docs/validation/farm_keyart_v4/groundcover_v20/**, esta spec e status. Sem runtime ou testes novos que espelhem listas de posições. Root detém lock Assets+Editor, concedido por Skills nesta rodada. Aceitação visual humana permanece separada de validação automatizada.
