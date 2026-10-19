# spec_farm_outskirts_composition_v1

> Status: CODE_COMPLETE — evidência Unity no escopo; DEFERRED_TO_FINAL_HUMAN_VALIDATION.
> Authorization: execução autorizada pelo pedido humano de popular acima, abaixo e lados.
> Type: Tooling / visual scene maintenance
> Domain: FarmScene outskirts
> Ownership: executor do compositor; orquestrador valida capturas e integração.
> Ordem de execucao: composição → geração → capturas → revisão.
> Depende de: composição atual preservada.
> Bloqueia: aceite visual desta alteração.

required_adrs: []
required_game_rules: []

# /speckit.specify

## Objetivo e estado confirmado
O compositor existente FarmPerimeterVisualComposer forma duas fileiras repetidas de árvores nos lados e ao sul; o norte usa a montanha existente. Preencher a paisagem visível além da área jogável com continuidade de solo e agrupamentos variados de vegetação e pedra, com clareiras. Preservar a composição interna aprovada e a escala da arte. Referência: docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png e baseline docs/validation/playmode/farm_preclose_audit_20260909/diagnostic/.

# /speckit.plan

## Escopo
Permitidos: Assets/_Game/Scripts/Editor/Art/FarmPerimeterVisualComposer.cs, FarmLandscapeVisualComposer.cs e WorldTilemapGround.cs na mesma pasta; Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs somente wiring/solo exterior; Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs e FarmPlayModeCaptureSession.cs na mesma pasta para enquadramentos exteriores; FarmScene.unity exclusivamente por Editor API; relatório e evidência docs/validation/farm_outskirts_20260909/, PROJECT_LOG.md, docs/IMPLEMENTATION_STATUS.md e esta spec.
Reutilizar sprites e compositores existentes. Sem nova geração de imagens necessária nesta passagem. Não modificar colisões, NPCs, IDs, interações, saves, Town ou cave procedural. Preservar alterações concorrentes e copiar a cena anterior antes de gerar.

# /speckit.tasks

- [x] Substituir a repetição de fileiras por agrupamentos determinísticos e clareiras, cobrindo norte, sul, leste e oeste no enquadramento real.
- [x] Dar continuidade ao solo exterior para evitar o recorte abrupto e manter o acesso visual à saída leste.
- [x] Manter decoração exterior sem colisores e sem invadir área jogável; preservar marcos interiores.
- [x] Gerar cena via Unity, comparar capturas gerais e das quatro bordas, revisar erros de execução.

Evidência: [relatório e cenário humano pendente](../../docs/validation/farm_outskirts_20260909/REPORT.md). 16 capturas PlayMode, 16 rotas e 6 seleções PASS; 171 componentes físicos preservados; revisão visual independente. Sem promoção para aceite humano. Degraus da curva exterior permanecem polimento P3.

## Validação e risco
Compilação comprovada pela geração Unity bem-sucedida, capturas sem gizmos e revisão independente da composição. Executar captura PlayMode existente após integração para regressão de rotas/interação. Captura e consultas automatizadas não equivalem a caminhada humana. Cenário humano pendente: caminhar pelos quatro limites e observar continuidade, saída leste e ausência de oclusão indevida. Risco: excesso de decoração e emendas visíveis; corrigir por inspeção antes de entregar. Não declarar fazenda inteira encerrada: pendências físicas da auditoria anterior permanecem fora desta alteração visual.
