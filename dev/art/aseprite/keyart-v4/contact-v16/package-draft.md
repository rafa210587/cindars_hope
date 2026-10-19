# /speckit.specify

# Farm v16 — contato físico, pesca e coerência de pixels

> Revisão:1 · Status:DRAFT_PENDING_PIXEL_AUDIT
> Autorização: pedido humano para consolidar spec/plan/tasks e executar este pacote.
> Tipo: manutenção visual + interação existente · Prioridade:alta · Domínio:Farm
> Baseline:stage15 + ambient_runtime15; não representa aceitação visual global.
> Depende de:ref_farm_contact_pixel_consistency_fishing_v1 (refinamento, não bloqueio de execução).
> Bloqueia:fechamento visual da Farm. Ordem de execucao:contatos → materiais → integração.
> Executor:root orquestra; candidatos independentes; integração Unity serial com freeze de Assets.

## Spec

Objetivo: remover contatos visualmente falsos e pesca no centro do cais, harmonizando a leitura
dos pixels sem aumentar indiscriminadamente a resolução nem mudar proporções/layout aprovados.

### Estado real

Fonte possui SolidBasin4.6×1.6 e trigger separado; pivot visual central fica1.865u acima do apoio.
Lago/rios já têm sólidos. Pesca usa ring±.96u em torno(13.56,-8.75), fora da ponta y=-10.798.
Dock/boat são filhos da mesma root escalada6; movê-la deslocaria arte. A observação anterior
comprovou cinco frames na fonte/cascata e dois saltos20s, mas não contato player/prop em movimento.
Chão/estrada apresentam granulação fina enquanto construções têm sombras suaves; medidas de
densidade e lista exata de materiais entram neste pacote antes de sua promoção à fila executável.

### Requisitos e aceitação

- AC1 Contatos: nas8aproximações da fonte e4da confluência, pés não entram em pedra/bacia/água;
  circulação externa sem enrosco; fonte interagível e respawn livre. Provar com collider real e PNGs.
- AC2 Profundidade: player à frente aparece à frente; estruturas altas podem ocultar ao passar
  atrás; água/espuma não sobrepõem player em chão seco. Todas5poses registradas, sem deslocar arte.
- AC3 Pesca: interação possível na ponta navegável e recusada na entrada/centro/laterais/água.
  Preservar ID, tabelas, stamina, clima e default de outros FishingSpots; simular seleção real.
- AC4 Densidade: tabela de canvas,PPU,scale e pixels efetivos para player,ground,road,house,barn,
  fonte,dock,cascade e vegetação discrepante; comparar a mesma câmera, sem confundir zoom de preview.
- AC5 Materiais: candidatos locais em Aseprite reduzem ruído isolado do chão/estrada e alinham
  clusters/sombras com atores/construções; sem blur ou aumento de detalhe artificial; inspecionar1×.
  Integrar somente candidato visualmente melhor que baseline, preservar originais e footprints.
- AC6 Antirregressão:0cultivos iniciais, mesmos IDs/71árvores, deck/boat imóveis, save intacto;
  fonte5frames0.7s e peixe20s preservados. PNGPoint/None/no-mips, sem novo manager de gameplay.

Fora do escopo:Town,Skills,Cave procedural,PlayerWalkAnimator,novo minigame de pesca,expansão de
mapa,resize global,regeneração completa keyart,novos sistemas de save ou geração de conteúdo.

# /speckit.plan

## Plan

### P1 — Interação opt-in

Modificar `Assets/_Game/Scripts/World/FishingSpot.cs`: proposto
`ConfigureStanceZone(BoxCollider2D zone)` e campo serializado opcional. Quando presente, usar zona
para CanInteract e rechecá-la antes de iniciar cast em Interact; sem zona conservar anel atual.
Medir pés segundo contrato existente do player, sem GameObject.Find e sem transformar trigger
em obstáculo. Creator só instancia/wira a zona específica Farm. Padrão:scene-interactable-wiring.

Modificar `CreateMvpFarmScene.CreateFishingSpot`: substituir quatro triggersFarm por zona na ponta;
manter root/IDs/visualchildren em seus transformsworld. Registrar stance/casttarget separadamente
em `FarmLevel1LayoutContract` quando necessário. Não mover todo FishingSpot.

### P2 — Contato e ordenação

Modificar `CreateMvpFarmScene.CreateFonteAnya` e `FarmSettlementPhysicsContract` somente conforme
overlay medido. Preferir referência de ordenação no apoio via agrupamento Unity quando suficiente;
alternativa: pivotnormalizado(.5,43/159) com compensação visual e nova versão de clip persistente.
Escolher uma estratégia no pacote final; não afrouxar guard de subasset em BuildClip.
Compartilhar geometria física e não-arabilidade; corrigir respawn se a base corrigida o interceptar.

`FarmSceneSpatialContract`/`FarmLandscapeVisualComposer`: ajustar apenas seam comprovadamente
fora da barreira. Não acrescentar collider em fish/foam. Reusar padrões sprite-scene-integration.

### P3 — Materiais

Sourcefiles e valores alvo definidos pelo audit de densidade, antes da execução. Candidatos
em `dev/art/aseprite/keyart-v4/contact-v16/art/`; Assets finais versionados e wiring pelo gerador,
sem sobrescrever fontes GPT. Aseprite layered, alpha e dimensões preservados salvo delta medido.

### P4 — Evidência proporcional

Estender probe Editor existente ou helper específico compacto para contato/seleção. Um root roda
EditMode direcionado e PlayMode isolado; sem repetir41s de peixe se seus inputs não mudarem.
Testes propostos `FishingStanceZoneTests`: tipaccepted/centerrejected/legacyunchanged/directcastguard.
Preservar testes existentes `FarmForageFishingTests`, `FishingV2Tests` e contratos físicos afetados.
Output `docs/validation/farm_keyart_v4/contact_v16/`:log,XML,metadata,antes/depois eHTMLcomparativo.

Riscos:viewport/escala da preview enganam medição; evitar screenpixelratio sem câmera igual.
Spritesubassetsnão podem ter pivot alterado silenciosamente; preferir groupouversão própria.
Root escalada exige conversão local/world. Relatórios PASS anteriores não certificam estética.

# /speckit.tasks

## Tasks

- [ ] T1 Audit de densidade e contato; fechar sourcefiles e estratégia sorting(AC1,2,4).
- [ ] T2 Revisar consistência, completar pacote e promover à fila; autorização humana já existe.
- [ ] T3 Implementar zona de pesca opt-in e testes comportamentais(AC3,6), apósT2.
- [ ] T4 Corrigir sorting/contato fonte e seams confirmados(AC1,2,6), apósT2 e medição.
- [ ] T5 Produzir/revisar amostra Aseprite e integrar materiais aprovados no review(AC4,5), apósT2.
- [ ] T6 Uma integração serial+gates afetados+capturas(AC1–6), apósT3–5 e freeze.
- [ ] T7 Revisão independente, HTML de entrega, relatório e finish-spec; não declarar95%global.

Execução ainda não iniciada enquanto este documento estiver DRAFT. Candidatos/medidas offline
podem ser preparados. Erros de infraestrutura não autorizam sobrescrever trabalho de terceiros.
