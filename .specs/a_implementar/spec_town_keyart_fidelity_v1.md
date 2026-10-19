# SPEC — Town Keyart Fidelity v1

**Spec ID:** `town_keyart_fidelity_v1`  
**Status:** A_IMPLEMENTAR — autorização humana explícita em 2026-09-09  
**Type:** WAVE_INTEGRATION / Editor scene generation + visual assets  
**Domain:** TownScene / world art  
**Priority:** P0  
**Parallelizable:** assets Aseprite e layout com ownership separado; Unity sequencial, um owner por execução  
**Repo lock:** Town creator, town layout, town-only world assets, TownScene and town captures  
**Executor:** subagente de implementação; agente raiz atua somente como validador

**Ordem de execucao:** reavaliação Town ativa, iterativa; execução Unity coordenada com Farm  
**Depende de:** `spec_town_spatial_expansion_and_access_v1`; keyart e contratos Town existentes listados abaixo; não depende da conclusão visual Farm  
**Bloqueia:** TOWN.C1, TOWN.N1 e `spec_town_integrated_acceptance_v1`

required_adrs: []
required_game_rules: [city_rules]

# /speckit.specify

## Objetivo

Corrigir a macrocomposição e a identidade espacial da TownScene — praça, caminhos, água, muralha,
curral, vegetação e set dressing — preservando NPCs, IDs, interiores, interações e física. Esta spec
entrega sua categoria visual ao gate TOWN.G1; somente o gate integrado pode declarar aderência >=80.
O aceite histórico de 82 foi retirado.

## Referências obrigatórias

- Keyart: `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`
- Baseline real: `docs/validation/playmode/town_capture_full.png`
- Proporção: `docs/design/gameplay/city/CITY_KEYART_PROPORTION_RULE_v1.md`
- Acceptance vigente: `docs/design/gameplay/city/TOWN_KEYART_ACCEPTANCE_RULE_v2.md`
- Layout existente: `Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`
- Creator: `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`

## Phase 0 vigente

- Cena pós-expansão: 160×112; `1 unit = 1 tile`; construções +20%; captura fixa comparável.
- Baseline preservada: 24 casas, 29 NPCs materializados, 28 NPCs agendados, 84 anchors e 2 spawns.
- `TownLayoutTests`: 22/22 PASS antes desta spec.
- `ValidateFableCitySchedule`: 21/21 PASS antes desta spec.
- Score independente vigente: ~68/100 (faixa 64–72), não >=80.
- Débito exclusivo desta spec: vias ainda ortogonais, praça/vazios pouco densos, borda uniforme,
  margem d'água blocada, curral sem vida, landmark ocluído e set dressing repetitivo.
- Arquitetura nativa pertence a TOWN.A1; portas/interiores/colisão a TOWN.C1; NPCs a TOWN.N1.

## Arquivos permitidos

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs`
- `Assets/_Game/Scripts/Editor/Art/WorldSpriteLibrary.cs`, somente para props/foliage/água já existentes.
- `Assets/_Game/Art/Generated/World/props/town_*`
- `Assets/_Game/Art/Generated/World/foliage/town_*`
- respectivos `.meta`, import settings e Tile assets gerados via Editor API.
- `Assets/_Game/Scenes/TownScene.unity`, somente por regeneração canônica.
- `docs/validation/playmode/town_capture*.png`
- relatório de execução/score em `docs/validation/`.
- `art/town-keyart-rework/composition/**`: overlays, máscaras e evidência; nenhuma fachada/roof/porta.
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartGround.cs`, `TownKeyartGeometry.cs`,
  `TownKeyartSetDressing.cs` e `TownKeyartSceneArt.cs`; nenhum helper de building/door/component.
- `Assets/_Game/Animations/World/town_*` e assets town-only de água/roda/luz, somente se reutilizarem
  componentes de animação existentes e não moverem colliders.
- `Assets/_Game/Scripts/Editor/Dev/TownSceneCapture.cs`: captura comparável e recortes de detalhe, preservar setup de cenas, saídas versionadas e evidência de colliders.
- `Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs` e `TownKeyart*Tests.cs`: regressão de geometria e contratos efetivos, sem enfraquecer testes para maquiar resultado.
- `Assets/_Game/Scripts/Editor/Validation/ValidateTownKeyart*.cs`: verificação de cena real, colliders, portas, NPC anchors, IDs e importação. APIs Editor, sem alterar comportamento de runtime.
- `Assets/_Game/Scripts/Editor/Dev/TownKeyartPlayModeCapture.cs` e `Assets/_Game/Scripts/Editor/Validation/TownKeyartLivePhysicsProbe.cs`: evidência em PlayMode com cena/câmera/ator reais, sem gravar saves ou cena. Reusar o mecanismo de sessão Editor existente como referência, não editar arquivos Farm. Capturas posicionadas e sondagens de collider não equivalem a travessia por input nem ação de interação executada.
- Regras town-only de proporção/aceite e relatório de reavaliação em docs; não editar skills compartilhadas sem defeito reproduzido na instrução.
- `Assets/_Game/Data/TownVisualScale/VisualScaleProfile_town_*.asset` e metas via Editor API: clones explícitos dos perfis existentes, fora do lookup compartilhado `Data/Scale`, somente para o piloto de proporção Town autorizado nesta reavaliação. Não editar os perfis originais nem o applicator/animators de runtime.
- `art/world_gpt/raw/town_*` e `art/evidence/proportion-audit03.md`: proveniência dos candidatos do ChatGPT normal e auditoria de proporção que motivou o piloto.

## Proibido

- FarmScene, CaveScene, runtime/save schema, lógica de agenda/diálogo/economia.
- renomear `House_*`, NPC IDs, anchor IDs, spawn IDs ou portais.
- remover NPC, casa, banca, interior, collider ou interação para melhorar a imagem.
- imagem única de fundo, keyart esticada, edição manual de YAML ou `SpriteRenderer Tiled` para chão grande.
- sobrescrever alterações concorrentes fora do lock.

# /speckit.plan

## Estratégia obrigatória

1. Abrir referência e baseline; registrar score inicial pelo acceptance rule.
2. Reusar os assets detalhados existentes de templo, prefeitura e ferraria; auditar assets de cidade antes de gerar novos.
3. Reestruturar visualmente praça e caminhos com Tilemap, mantendo o grafo físico aprovado.
4. Dar identidade visual aos distritos com arquitetura, jardins, cercas, props, luzes e água/margens.
5. Reduzir a moldura uniforme da floresta com clareiras/rochas/elevação sem abrir os bounds físicos.
6. Regenerar a cena, validar contratos, capturar com a mesma câmera e produzir score detalhado.
7. Materializar loops ambientais town-only de água/roda e iluminação contextual pelo sistema de tempo/
   clima existente. Curral recebe leitura de uso por props/loops existentes, sem nova simulação animal.

## Refinamento residual — 2026-09-10

- Preservar o grafo físico aprovado da expansão; organicidade é tratamento de borda, ramificações,
  bolsões e materiais, não remoção de acessos.
- Reduzir vazios com pocket spaces funcionais e props narrativos, mantendo clearances do Player×NPC.
- Praça deve ter leitura circular/radial, ponto focal, permanência e conexões com quatro quadrantes.
- Água deve ter margem irregular, cais transitável, leitura de fluxo e conexão visual com o moinho.
- Muralha/borda deve alternar pedra, vegetação e clareiras sem abrir o limite físico.
- Curral deve comunicar uso rural sem criar nova simulação animal nesta spec.
- Corrigir oclusão da escadaria do templo e qualquer prop sobre faixa crítica de porta/fachada.

## Critérios de aceite

- CA-1: composição espacial atinge `>=18,75/25`; praça, água e materiais/terreno atingem cada
  `>=9/15`; microdetalhe atinge `>=6/10`; sem blocker de macrocomposição da regra v2. O total >=80
  pertence a TOWN.G1.
- CA-2: `TownLayoutTests` retorna `total=22; passed=22; failed=0` ou mais testes, todos passando.
- CA-3: `ValidateFableCitySchedule` retorna `PASS: 21 | FAIL: 0`.
- CA-4: log de geração informa `clearance=0 lote-lote=0 lote-via=0 porta-sem-acesso=0` e `LakeWater audit: lotes=0 vias=0`.
- CA-5: captura real 1536x1024 existe, é posterior às mudanças e foi comparada à keyart aberta.
- CA-6: `git diff` não contém mudanças fora dos arquivos permitidos causadas pelo executor.
- CA-7: baseline e revisões preservadas com hash; câmera comparável documentada para antes/depois, sem crédito por zoom, ocultação ou crop de defeitos.
- CA-8: Aseprite candidates reabertos, alpha real e fontes imutáveis verificadas; export em escala de jogo aprovado antes de promover. Recortes isolados de objetos da keyart são permitidos como candidatos editáveis, nunca chão/bairro/imagem de fundo disfarçados de objeto.
- CA-9: física efetiva verificada na cena gerada para troncos, água, muralha, prefeitura, portas e anchors; nenhum PASS de runtime/sorting baseado só em captura estática ou grafo lógico. Limites de PlayMode explicitamente registrados.
- CA-10: captura em PlayMode e sondagem com collider real devem registrar diferenças após Awake, câmera, NPCs ativos, erros de runtime, alcance consultado e hashes de cena/saves antes/depois. Nenhuma chamada de save/load, teste destrutivo, persistência de teleporte ou falso claim de input/interação real.
- CA-11: piloto de escala Town deve manter componentes de animação/corpo no mesmo root e referências de perfil exclusivamente locais à Town; comparar geometria física mundial dos colliders/offsets/triggers antes/depois contra o estado original esperado após Awake (não o Player preview1,3), tolerância0,01u. Clones com ColliderScale0 evitam multiplicação repetida. Perfis/fontes globais intactos por hash. Confirmar escala pós-Awake e isolamento Farm→Town→Farm; inferência estática não equivale a transição executada.
- CA-12: água e roda executam loops visíveis e contínuos, luzes respondem ao período existente e nenhum
  frame animado move/ativa collider. Clima é integração com o sistema atual, sem regra nova nesta spec.

## Edge cases

- Asset ausente: usar placeholder coerente temporário e registrar perda no score; não inventar referência quebrada.
- Nova arte: importar Point/None/no mipmaps e verificar PPU/pivô separadamente do collider.
- Prop decorativo: collider somente quando houver motivo de gameplay; decoração não pode estreitar corredor.
- Captura headless: executar com dispositivo gráfico; `-nographics` não suporta o TilemapRenderer desta cena.
- Resultado `<80`: não fechar a spec; entregar captura e déficits para um novo passe/agente.

# /speckit.tasks

- [x] Preservar baseline, reabrir aceite histórico e publicar rubric v2.
- [x] Executar revisão independente da baseline e da revisão02; rejeitar resultados abaixo da meta.
- [x] Produzir candidatos Aseprite editáveis e capturas Unity versionadas01–03.
- [ ] Congelar antes da wave uma baseline reproduzível de performance da rota do gate TOWN.G1.
- [ ] Corrigir macrocomposição, proporções espaciais e set dressing dos déficits03.
- [ ] Integrar água/roda/luzes/curral pelo pipeline ambiental existente e validar sorting/colliders.
- [ ] Regenerar e validar geometria/agenda/cena materializada do estado final, com todas as fixtures pertinentes passando.
- [ ] Executar e revisar evidência real PlayMode; registrar limites e pendências humanas separadamente.
- [ ] Obter os mínimos de composição/terreno/microdetalhe por revisão independente e entregar score
  parcial rastreável ao gate TOWN.G1.
- [ ] Entregar composição/terreno sem reivindicar aceite global antes de TOWN.G1.
