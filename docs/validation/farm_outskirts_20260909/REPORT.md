# Fazenda — continuidade da paisagem exterior

Spec: [spec_farm_outskirts_composition_v1](../../../.specs/a_implementar/spec_farm_outskirts_composition_v1.md).

## Acceptance criteria extracted

Popular acima, abaixo e dos lados; variar a composição além das fileiras de árvores; preencher o chão visto pela câmera; preservar a área jogável, seus NPCs e suas interações.

## Existing systems audit

Reutilizados FarmPerimeterVisualComposer, WorldTilemapGround, WorldSpriteLibrary e os dois capturadores existentes. A composição anterior tinha duas fileiras periféricas e solo visual de 72×52 unidades. A câmera acompanha o jogador sem clamp, permitindo ver além desse chão. A keyart aprovada orienta os materiais e a variedade, enquanto o pedido atual autoriza estender a paisagem além do enquadramento original.

## Spec Compliance Matrix

| Critério | Resultado |
|---|---|
| Composição variada nos quatro lados | PASS visual: bosques irregulares, clareiras, flores, arbustos e rochas |
| Solo cobre os enquadramentos de borda | PASS nos enquadramentos capturados; solo visual ampliado para 112×80 unidades |
| Sem novos bloqueios, NPCs e IDs preservados | Comparação independente vinculada abaixo; física preservada |
| Geração e captura em Unity | PASS; cena gerada pelo Editor API, capturas finais e consultas em PlayMode |

## Validation

Unity 6000.5.7f1. Geração final: `CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture` com `-batchmode -quit`, resultado em [regen-seam-final.log](regen-seam-final.log). Captura PlayMode: `CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmGameplayBatch` com `-batchmode`, sem `-quit`, resultado em [gameplay-final.log](gameplay-final.log) e [metadados finais](gameplay_final/capture-metadata.json). Executável: `C:/Program Files/Unity/Hub/Editor/6000.5.7f1/Editor/Unity.exe`; projeto: `D:/Projetos/Cindars_Hope/cindars_hope`.

Destinos configurados por `CINDARS_FARM_CAPTURE_OUTPUT` e `CINDARS_FARM_GAMEPLAY_OUTPUT`. A geração comprova compilação Editor; não houve execução redundante de compile-only nem suíte completa de testes sem relação com a mudança.

- 16 capturas PlayMode, incluindo quatro bordas e quatro cantos.
- 16/16 rotas físicas PASS e 6/6 seleções de interação PASS; zero erros runtime nos metadados.
- Consulta física utiliza collider real e varreduras; não simula caminhada. Seleção não executa crafting, pesca ou transição.
- A câmera do batch permaneceu 640×480, aspecto 4:3, mesmo com argumentos de resolução. Diagnósticos das quatro bordas foram renderizados em 1600×900, 16:9. Não afirmar PlayMode 16:9 executado.
- Hash da cena antes/depois do PlayMode e no arquivo final: `9f7b92e72d144728f8b0abae8fffd32f3a3778cdedb205e3dc7027818347a540`.
- Backup pré-geração: `FarmScene.before.unity.backup`, hash `751a9263324f2a322135e9900e969fc5b22f73955db31d629a592296ffacF803` (comparação hexadecimal sem distinção de caixa).

Revisão visual independente e inspeção do orquestrador confirmaram preenchimento dos quatro lados e correção da árvore suspensa ao norte. A estrada agora continua para leste; a emenda de grama vista no primeiro PlayMode foi corrigida e conferida na [captura final](gameplay_final/border_east.png). Os degraus na curva exterior são polimento P3, ainda perceptíveis, sem comprometer continuidade.

Comparação semântica final: [revisão independente](PHYSICS_INTERACTIONS_REVIEW.md) e [evidência](semantic-comparison.json). Os 171 componentes físicos e 54 componentes com campos de IDs estão preservados. O ramo periférico contém 620 SpriteRenderer e zero componentes físicos ou interativos. O alias legado `_craftingPoint` mudou de CookingStation para Forge; seu único consumidor chama `RebindCraftingManager`, atualmente vazio. Os arrays de plots/árvores mudaram de ordem, mantendo membros; o consumidor apenas injeta as mesmas referências em cada membro. Essas diferenças brutas são registradas, não descritas como igualdade byte a byte de todos os bindings.

## Arquivos e imagens

Código: `FarmPerimeterVisualComposer.cs` (distribuição exterior); `CreateMvpFarmScene.cs` (solo e continuação visual da estrada); `FarmSceneCapture.cs` e `FarmPlayModeCaptureSession.cs` (enquadramentos e destino de evidências). Cena: `Assets/_Game/Scenes/FarmScene.unity`, por geração Unity. Nenhuma imagem nova gerada ou import alterado; sprites existentes reutilizados. Sem commits nesta entrega.

- [Visão geral final](diagnostic_final/farm_capture.png).
- [Norte](diagnostic_final/farm_capture_region_border_north.png), [sul](diagnostic_final/farm_capture_region_border_south.png), [oeste](diagnostic_final/farm_capture_region_border_west.png), [leste](diagnostic_final/farm_capture_region_border_east.png).
- [Canto SE no jogo](gameplay_final/corner_se.png), [canto NW](gameplay_final/corner_nw.png).

## Honest status rationale

Docs global: FAIL, exit 1, 59 diagnósticos já existentes, nenhum menciona a spec outskirts. Evidência: [docs-validation.log](docs-validation.log). Índice regenerado com 443 specs. Não houve GLOBAL_PASS. A inspeção documental desta entrega verifica links e formato separadamente.

CODE_COMPLETE, com evidência Unity e verificações automatizadas no escopo; DEFERRED_TO_FINAL_HUMAN_VALIDATION. Não promover a spec para aceite humano. A alteração é visual e não encerra as pendências físicas da auditoria anterior. Teste humano de caminhada e aceitação visual: NOT RUN. Cenário final: caminhar pelos quatro lados e cantos, conferir chão contínuo, leitura dos limites e orientação da saída para a cidade. O exterior permanece inacessível; áreas de campo aberto ainda podem tornar os limites invisíveis perceptíveis. Validação global de docs registrada separadamente, sem converter dívida anterior em PASS.
