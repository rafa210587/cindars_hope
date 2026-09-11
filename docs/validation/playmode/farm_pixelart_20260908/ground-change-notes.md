# Correção focal da mistura de chão — 2026-09-08

Spec: `spec_farm_pixelart_cohesion_and_ingame_review_v1`, emenda executável 85/10/5 anterior à edição.

## Escopo e preservação

Somente `CreateMvpFarmScene.CreateFarmGroundTexture` e três constantes privadas foram alterados. A composição reusa `SpriteWorldSize`, `GetOrCreateLayer` e `PaintRectWeighted`: base 85, flores 10, pedras 5. Variantes ausentes são excluídas com warning contendo FarmGround e o nome da variante; base ausente preserva o retorno com warning existente. Os pesos disponíveis são renormalizados pelo compositor existente.

Grid WorldGrid, camada Ground, sorting 0/Ground, centro (-1,-1), extensão (72,52), tamanho de célula derivado do sprite e hash determinístico de distribuição permanecem iguais. Nenhum collider ou PNG foi editado. `WorldTilemapGround.PaintGrass`, usado por outros mundos, permanece intacto nesta fatia.

O gerador já estava dirty. Backup anterior à edição: `art/farm-pixelart-review/backups/CreateMvpFarmScene.before-ground-85-10-5.cs`. O diff contra esse backup contém apenas três constantes e o método indicado, incluindo a correção de comentários antigos que ainda descreviam SpriteRenderer.

SHA256 do backup: `8B5BBFA97BE4DF0242ECB4890001FBA20BF126F79A2860D5A1EF2559DFD07A1D`.

SHA256 do gerador após esta fatia: `2C5835341E98FE25B4D0FC5EB70E8B238E35E297DDE6FC39E45CF49854E62ADB`.

SHA256 de WorldTilemapGround antes/depois, idêntico: `3EE93212A8F98DE9FA840BB78DCB6CFAC8BAD1823D0FB5A64E67E2499F9A1BF6`.

## Evidência e estado

Revisão estática: PASS para escopo, fallback e preservação dos argumentos espaciais. Comando: `git diff --no-index -- art/farm-pixelart-review/backups/CreateMvpFarmScene.before-ground-85-10-5.cs Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`. Exit 1 é esperado por conter diferenças.

Unity validation: NOT RUN nesta fatia

Reason: execução Unity exclusiva pertence ao validador da rodada integrada; nenhuma segunda instância foi iniciada.

Command attempted: nenhum nesta fatia; planejado `-executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture -quit` pela rodada integrada.

Residual risk: Unity compile not validated locally nesta fatia; distribuição materializada e melhoria visual ainda requerem geração/captura.

Não foi criado teste que espelha os três pesos. A evidência comportamental exigida pela spec é a geração real com contagem de famílias: zero sprites ground_grass_a/b e somente base/flower/pebble, seguida de comparação visual. `Farm ground palette: PASS` ainda não obtido neste registro.

## Continuação focal — ShippingBin

Emenda ShippingBin lida antes da edição. Backup adicional `art/farm-pixelart-review/backups/CreateMvpFarmScene.before-shipping-visual.cs` tem SHA256 `2C5835341E98FE25B4D0FC5EB70E8B238E35E297DDE6FC39E45CF49854E62ADB`; inclui a correção de chão anterior. O segundo diff altera exclusivamente CreateShippingBin e a constante ShippingBinVisualTargetHeight.

O perfil permanece na raiz. Quando o PNG existe, o renderer passa ao filho Visual em posição local zero; o filho recebe escala uniforme `2 / (root.lossyScale.y * sprite.bounds.size.y)`. Assim, a altura mundial calculada é 2 tanto após AttachApplicator no Editor como após Awake reaplicar o mesmo perfil. Escala inválida da raiz gera exceção contextual antes da divisão. Fallback builtin continua no root. Posição (24,1), trigger, collider local (1.3,1.1), interactable, IDs e perfil não foram editados. A escala mundial do collider no Editor agora corresponde à raiz de perfil, como já ocorria em Play; não alegar que o override visual antigo preservava essa paridade.

O renderer mantém World/0, SpriteSortPoint.Pivot e o mesmo ponto de apoio mundial. O meta existente usa alignment7 (BottomCenter); spritePivot serializado(.5,.5) não substitui esse preset. GraphicsSettings registra m_TransparencySortMode2 e eixoY; nenhuma mudança global de sorting foi feita. Inspeção de oclusão na câmera real continua necessária.

FarmPlayModeCaptureSession foi estendido somente na medição: helper AddSpriteMeasures reaproveita as medidas existentes, Player continua obrigatório antes de acrescentar renderers do ShippingBin encontrado pelas raízes da cena, e scaleRootLossyScale registra a escala do applicator. Para o bin, ler altura em View.sprites[].spriteWorldBoundsSize.y, escala visual em localScale e identificação em objectPath. Backup da captura: `art/farm-pixelart-review/backups/FarmPlayModeCaptureSession.before-shipping-measure.cs`. A correção anterior do escopo de hashes de saves foi preservada.

SHA256 do gerador desta continuação: `0F7B85AB88C6CA9009B2F7AA57E21FA3FF4B74C902705194A4AA3913EA778CF4`; captura: `A705817D1DBFF37DD23A1BFE99AA67DE2DDC0743F380F4EBDCDDD6187CFD7206`.

Unity validation: NOT RUN nesta fatia, pelo mesmo ownership exclusivo da rodada integrada. Paridade geométrica é raciocínio estático; `ShippingBin visual parity: PASS` e visibilidade do player ainda exigem medição/captura real. Não houve edição de raster, scene YAML ou perfil de escala.
