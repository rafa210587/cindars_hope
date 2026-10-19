# Auditoria da fazenda antes do fechamento — 2026-09-09

**Resultado: NÃO PRONTA PARA FECHAMENTO.** Auditoria concluída; correções não executadas nesta rodada.
Há divergência entre o bloqueio previsto e os corpos físicos de estufa, celeiro e galinheiro.

## Escopo e evidência atual

Cena salva: `Assets/_Game/Scenes/FarmScene.unity`.
SHA256 antes/depois: `751A9263324F2A322135E9900E969FC5B22F73955DB31D629A592296FFACF803`.
Unity 6000.5.7f1; captura Play Mode entre 20:17:28Z e 20:17:49Z em 09/09/2026.
Nenhuma regeneração de cena nem correção de gameplay realizada. Dois agentes auditaram
física/bordas e NPCs separadamente; integrador conferiu os achados centrais, a arte e os resultados.

- [Metadados Play Mode](playmode/farm_preclose_audit_20260909/gameplay/capture-metadata.json): PASS,
  oito câmeras, 16 rotas físicas e seis seleções de interação, zero erros runtime registrados.
- [Log Play Mode](playmode/farm_preclose_audit_20260909/gameplay.log), exit 0.
- [Mapa de colisores](playmode/farm_preclose_audit_20260909/diagnostic/farm_capture_colliders.png)
  e [log de captura](playmode/farm_preclose_audit_20260909/colliders.log), exit 0.
- [Keyart aprovada](../art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png) aberta
  e comparada com composição e capturas atuais de ponte, lago, montanha, bosque e animais.

O amarelo no mapa mostra **bounds retangulares dos colliders não trigger**, não o contorno exato
dos polígonos. Os retângulos do rio/lago não significam que toda sua área esteja bloqueada.
SaveInput foi desabilitado em memória; cena reaberta sem salvar. Pasta `saves` verificou zero
arquivos antes/depois; não foi exercitado save/load. Não há aprovação humana implícita.

## Inventário físico

| Categoria | Bloqueia? | Observação |
|---|---|---|
| Perímetro | Sim | Quatro BoxCollider2D contínuos, faces internas x ±32 e y ±22 |
| Montanha | Sim | Polígono ao norte |
| Lago e rio | Sim | Polígonos; rio tem corredor da ponte, lago tem recorte do cais |
| Ponte | Passagem livre no corredor | Aproximação nos dois lados passou no probe de rotas |
| Casa | Sim | Paredes e bloqueador da porta; lógica da porta pode desabilitar bloqueador |
| Estufa | Não há sólido próprio | Visual, host e plots com triggers; contraria footprint bloqueante |
| Celeiro e galinheiro | Não há sólido próprio | Colliders de interação são triggers; não impedem atravessar o prédio |
| Bancada, forja e cozinha | Sim | Trigger de interação + filho SolidBody |
| Árvores TreeNode | Sim, no tronco | Copa visual não é o volume inteiro do bloqueio |
| TreeResource_01 | Trigger | Não assumir que toda árvore tem o mesmo tipo de colisão |
| Fonte, shipping, sell, processamento | Triggers | Sem bloqueio sólido por esses colliders |
| Minérios, pedras coletáveis e forage examinados | Triggers | Interação não equivale a obstáculo físico |
| Arbustos, flores, cercas, troncos e rochas decorativos | Não | Decoração visual pode ser atravessável intencionalmente |
| Zrix | Sim, pequeno corpo nos pés | Trigger de interação separado do sólido |

Fonte: creator, contrato espacial e inspeção read-only da cena serializada. Vegetação não deve
receber collider em toda a copa; props pequenos decorativos não precisam virar obstáculos.

## Achados antes de fechar

### P1 — prédios sem bloqueio próprio

[Contrato](../../Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs:87) marca estufa e
área de animais como Building bloqueante. O [creator dos animais](../../Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs:1983)
usa triggers; a [estufa](../../Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs:2077)
não ganha corpo sólido. Cena salva confirma em linhas 13935, 23008 e 85669; captura de colliders
não mostra corpo próprio desses edifícios.

Correção indicada: adicionar footprint sólido coerente com paredes/base, preservando portas,
triggers, plots e IDs. Revalidar aproximação, tentativa de atravessar paredes e interação.
Não usar collider cobrindo o telhado inteiro nem reutilizar água vizinha como “prova” de parede.

### P2 — fechamento físico não acompanha necessariamente o obstáculo visual

Não foram encontradas brechas geométricas nas quatro paredes do perímetro. Portal de cidade
em (31,3) e entrada da caverna em (-19.5,17) são transições internas, não buracos nas bordas.
O [creator do perímetro](../../Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs:1852)
remove a cerca visual e mantém paredes físicas.

A captura atual mostra vegetação periférica também ao sul/leste, portanto não seria correto
concluir que faltam árvores apenas pela antiga rotina de treeline. O bloqueio é feito pela
parede invisível, não por toda essa vegetação; há faixas de grama junto a limites, especialmente
no leste. Ajustar alinhamento visual/colisão onde a grama sugere continuação acessível.
Não foi feita tentativa manual de fuga em cada ponto/canto nem execução do portal da cidade.

### P2 — validador de navegação tem critério de nome incompatível

[ValidateFarmSceneNavigation](../../Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneNavigation.cs:60)
reprova descendentes de FarmSpatialCollision que não começam com `Collision_`, enquanto o creator
gera `RiverCollider_AboveBridge` e `RiverCollider_BelowBridge` (linhas 2991/2998), presentes na cena.
Inconsistência confirmada por inspeção estática; esse validator específico não foi executado aqui.

Além disso, alcançabilidade em `CountReachableLandmarks` usa raster do contrato (linha 205),
e a checagem de prédio aceita interseção com qualquer sólido (linha 134). Isso não comprova
footprint físico do prédio. Corrigir critério/ownership sem afrouxar o teste para obter PASS.

### P2 — NPC preservado, diálogo não integrado

O único NPC fixo identificado nos contratos, HEAD local e cena atual é **npc_zrix — Zrix das Estradas**.
`NPC_Zrix_Farm` permanece ativo em (-26,8), com data, wander e animação ligados. Não há evidência
de remoção de outro NPC fixo entre essas versões. Isso não prova preservação de toda história antiga.

[Creator](../../Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs:2388) configura
Zrix como wander-only; `_dialogueModal` está vazio na cena (linha 81554), e
[NpcController](../../Assets/_Game/Scripts/NPC/NpcController.cs:321) encerra interação sem presenter.
Isso é uma limitação conhecida, não uma regressão de diálogo provada. Mural `Board_Zrix` é separado.
`NpcAppearFade` começa invisível até o renderer aparecer; percurso/diálogo do NPC não foram testados.
O visitante eventual em `GoblinFarmVisitor` mantém materialização adiada; não contar como NPC já presente.
Animais e decoração são categorias distintas; não foram validados criação/alimentação/coleta nesta rodada.

## O que os testes realmente comprovam

`FarmPhysicalRouteProbe` usa o collider real do player com OverlapCollider/Cast, filtro de layers
e busca em passos de 0,5 unidade. As 16 aproximações passaram, incluindo ponte, cais, caverna,
estufa e construções animais. Os três pontos de água ao redor do cais estavam bloqueados.
Esse resultado não prova que todas as paredes existem: um prédio atravessável também é alcançável.

O teste de seleção aguardou passos de física e consultou InteractionSystem real para casa,
bancada, forja, cozinha, entrada da caverna e pesca: 6/6 PASS. Não chamou Interact, não executou
transições nem pescou. Câmeras usam teleporte para enquadrar; não comprovam caminhada/fluidez.

## Avaliação visual e sequência recomendada

Ponte e cais estão conectados às margens nas capturas atuais; não reapresentar a versão antiga
do cais fora do lago como estado vigente. Montanha/caverna, bosque, campos, casa/estufa e lago
seguem a organização geral da referência. As ruas ainda têm recortes angulares e junções duras,
especialmente na aproximação da caverna; polimento visual permanece separado da correção física.

1. Corrigir sólidos próprios de estufa/celeiro/galinheiro e a prova do validator.
2. Rever footprint de props grandes (fonte/processamento) e alinhar limites com vegetação/rochas,
   mantendo arbustos e detalhe pequeno atravessáveis quando isso favorece a passagem.
3. Validar Zrix em câmera, wander e intenção de diálogo; provar portal de cidade e transição cave.
4. Fazer percurso manual de bordas/cantos, portas, ponte/cais e atividades antes da aceitação humana.

Esta auditoria não altera a cena nem promove as specs FARM KEYART a aceitas.
