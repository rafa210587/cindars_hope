# SPEC — Coesão pixel art da fazenda e revisão na câmera de gameplay

> **Spec ID:** `spec_farm_pixelart_cohesion_and_ingame_review_v1`
> **Status:** IN_PROGRESS — revisão G01–G04 SCOPED_PASS técnico; FarmScene final proportion_pass/02 entregue, aceite visual/humano final pendente
> **Wave:** FARM KEYART — continuação baseada em produção modular
> **Priority:** P1
> **Type:** Tooling / Validation / Art integration
> **Domain:** Farm / Player / Editor
> **Parallelizable:** CONDITIONAL — pesquisa/docs podem acompanhar; Unity e geração de cena são exclusivos
> **Parallel group:** FARM_PIXELART
> **Can run with:** documentação fora dos arquivos abaixo
> **Must not run with:** edição/geração concorrente de FarmScene, seus contratos ou captura
> **Repo lock scope:** arquivos §18; Unity Editor do projeto durante captura/regeneração
> **Depends on:** CURRENT_STATE; gerador/contratos atuais; referência keyart e laboratório farmer existentes
> **Blocks:** expansão de novas famílias artísticas para a fazenda inteira e aceitação visual final
> **Scope:** reconstruir e materializar a FarmScene com as seis regiões da keyart, arte GPT inspecionada, contratos espaciais coerentes, evidência de câmera real e circulação física; entregar a cena para revisão humana.
> **Out of scope:** balanceamento, save schema, novos sistemas de gameplay, reforma global do projeto, aprovação automática de arte.
> **Validation level alvo:** UNITY_VALIDATED para geração/captura; PLAYMODE com evidência específica, sem equivalência a aceitação humana
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

## 5. Contexto

O usuário autorizou continuar os ajustes de sprites, avaliar o personagem in-game e arrumar a fazenda inteira com base na pixel art. Pediu pesquisar métodos de jogos indies e adaptar a geração de imagens com GPT. A cena salva já contém elementos das correções existentes. A regeneração focal reconciliará o estado do gerador com a cena e permitirá avaliar a nova mistura de chão; não se presume que regenerar resolverá os achados visuais.

## 6. Problema

Uma comparação de sprites em navegador não demonstra leitura em movimento, escala de mundo ou sorting. A captura diagnóstica da FarmScene salva apresenta chão quadriculado/brilhante, água em degraus, árvores sobre cultivo e casa dominante. Alterar novamente o gerador sem confrontar código, hierarquia salva e captura pode duplicar correções e arriscar o trabalho concorrente.

## 7. Objetivo

Entregar a FarmScene reconstruída nas seis regiões da keyart, com composição e arte integradas, câmera real com personagem e contratos físicos verificados. A entrega técnica representa aproximação implementada da referência, não cópia exata nem aceitação humana. A proposta inicial de entregar somente baseline/piloto foi substituída pelo pedido humano de reconstrução integral.

## 8. Fontes e leitura mínima

- `AGENTS.md`, `docs/project/CURRENT_STATE.md`.
- `.agents/skills/spec-authoring/SKILL.md`, `.specs/_templates/SPEC_DEEP_TEMPLATE.md` e seu exemplo obrigatório, usados na autoria.
- `docs/art_catalog/FARM_GPT_PIXEL_ART_WORKFLOW.md`: fontes primárias e decisões adaptadas ao GPT.
- `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`.
- `.specs/a_implementar/spec_farm_scene_keyart_scale_physics_correction_v1.md`, `spec_farm_scene_keyart_playmode_acceptance_v1.md`, `spec_farmer_animation_visual_revision_v1.md`.
- Arquivos de §18; na implementação aplicar `spec-execution`, `tilemap-world-rendering`, `unity-asset-generation`, `unity-validation`, `implementation-closeout` e regras `testing-quality-gate`/`validation-truth` conforme a fase.

Pesquisa e síntese do método: concluídas no workflow citado acima. Fatia inicial implementada e validada conforme relatório individual; requisitos regionais e artísticos permanecem PENDING.

## 9. Estado atual — Phase 0 auditado em 2026-09-08

```text
Get-Content docs/project/CURRENT_STATE.md:
  Unity6000.5.7f1; quatro falhas farm preexistentes; cena/arte/gerador dirty a preservar.
Get-Content Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs:
  CaptureFarmTopDown fotografa cena salva com câmera temporária; não entra em PlayMode.
  RegenAndCapture chama CreateMvpFarmScene.CreateScene e depois captura diagnóstica.
rg Target/Bridge FarmSceneCompositionContract.cs:
  BridgeVisualLocalScale=(1.8,1.4,1); HomesteadRoofTargetWidth=8.5;
  CraftingVisualTargetHeight=1.5; BridgePassageProbe=(15,3).
rg TreeVisualTargetHeight CreateMvpFarmScene.cs:
  alvo atual4.6, dividido pela altura nativa da arte para criar árvores.
rg public static WorldTilemapGround.cs/FarmDecorationPlanner.cs:
  Tilemap, tiles, polígonos/transições e planner com IsForbiddenCell já existem.
Inspeção baseline diagnóstico pelo orquestrador:
  chão quadriculado, água brilhante/em degraus, árvores sobre cultivo, casa dominante.
Auditoria do gerador atual:
  a cena salva já contém FarmSpatialCollision, FarmDecoration, Visual_CentralFieldSoil,
  espantalhos, SolidBody e CoopPen. Dirty não demonstra falta de regeneração;
  regenerar reconcilia o estado do gerador e materializa a nova mistura de chão,
  sem garantia prévia de resolver os defeitos visuais;
  WorldSpriteLibrary.Load pode reimportar meta; GetTile pode criar _TileAssets.
```

### Reconciliação explícita com specs anteriores

A spec histórica de escala exige craft1.8/casa7.5; CURRENT_STATE exige preservar a continuação farm e aponta falhas herdadas. Esta continuação autorizada registra como baseline os valores atuais1.5/8.5 e ponte1.8x1.4, sem restabelecer constantes históricas. Mudanças futuras de escala exigem comparação real e atualização localizada do contrato/teste correspondente. A captura diagnóstica atual não satisfaz a spec histórica de aceitação PlayMode. Esta spec não declara aquela concluída nem bloqueia coleta de evidência visual pelo FAIL global preexistente: cada falha permanece identificada.

Antes de executar, reconfirmar git status dos arquivos permitidos, hash da cena/meta/tileassets e exclusividade do Unity. Se uma edição concorrente surgir, não sobrescrever: reconciliar o diff primeiro.

## 13. Regras de não duplicação

Reusar `CreateMvpFarmScene`, `FarmSceneCapture`, `WorldTilemapGround`, `FarmDecorationPlanner`, `FarmSceneCompositionContract` e `FarmSceneSpatialContract`. Não criar Tilemap/gerador/câmera de gameplay/catálogo de arte paralelos. Não impor12frames ou96px de produção: seis–oito poses coerentes são referência de trabalho; doze e maior resolução continuam hipóteses experimentais. O procedimento GPT é descrito no workflow, não em novos hooks/agentes redundantes.

## 15. Arquitetura alvo — criar versus modificar

CRIAR:
- `docs/art_catalog/FARM_GPT_PIXEL_ART_WORKFLOW.md`: fontes, contrato, pipeline e gates.
- `docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md`: evidência e estado por fase.
- `art/farm-pixelart-review/manifest.json`: proveniência, métricas e decisões dos candidatos.
- `art/farm-pixelart-review/preview.html`: comparação modular do piloto de chão e referência de escala.
- Opcional `Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs`: somente se a sessão assíncrona tornar FarmSceneCapture pouco coeso; implementa ciclo de captura Editor, não comportamento runtime.

MODIFICAR:
- `FarmSceneCapture.cs`: acrescentar entrada batch assíncrona para câmera real, manter API diagnóstica existente.
- Gerador e contratos listados em §18: somente após regeneração/inspeção revelarem defeito ainda atual; cada alteração requer achado, método e resultado esperado no relatório antes da edição.
- Cena/meta/tileassets: materialização exclusivamente via Unity API, dentro das condições §18.

## 16. Contratos e saídas

### 16.1 Entrada de captura PlayMode

Adicionar a `CindarsHope.Editor.Dev.FarmSceneCapture`:

```csharp
public static void CaptureFarmGameplayBatch();
```

Invocação, com `$unityExe` resolvido pela instalação atual e `$projectPath` raiz absoluta:

```powershell
& $unityExe -batchmode -projectPath $projectPath -executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmGameplayBatch -logFile "$projectPath/docs/validation/playmode/farm_gameplay_capture.log"
```

**Sem `-quit`**, pois o método agenda transições; a ferramenta sai após completar ou falhar. Não usar `-nographics` para renderizar. Não criar menu novo. Em sessão interativa, não encerrar o Editor; a saída batch só se aplica a `Application.isBatchMode`.

Saída proposta em `docs/validation/playmode/farm_gameplay_review/`:
- `spawn.png`, `bridge.png`, `cultivation.png` na câmera de gameplay real.
- `capture-metadata.json`: `status`, `startedUtc`, `finishedUtc`, `scenePath`, `sceneSha256Before`, `sceneSha256After`, `unityVersion`, `saveRoot`, `saveIsolation`, `saveHashesBefore`, `saveHashesAfter`, `views[]` com `id`, `utc`, `playerPosition`, `cameraPosition`, `orthographicSize`, `width`, `height`, `path`, `cameraObjectPath`.
- Registro explícito de bootstrap, estabilização de câmera e limitações. Teleporte temporário entre vistas é apenas enquadramento; não prova travessia física.

Logs de sucesso: `Farm gameplay capture: PASS (3/3)` e `Persistent files unchanged: PASS`. Exit0 somente se três arquivos e metadata existirem e hashes estiverem preservados; exit1 em falha. Timeout120s após entrada em PlayMode: registrar motivo e sair em falha.

### 16.2 Preservação de estado

Não invocar save/load de perfil humano. Investigar autosave/bootstrap antes de entrar em PlayMode; usar isolamento temporário suportado pelo projeto ou desativação temporária e verificável dos produtores de save na sessão Editor. Registrar mecanismo. Se não houver caminho confiável, marcar captura `NOT RUN`, sem arriscar o perfil. Hashes antes/depois verificam integridade, mas não substituem prevenção. Não salvar cena durante ou após a captura; restaurar targetTexture e objetos temporários em finally/saída.

### 16.3 Manifest de arte

`schemaVersion`, `referencePath`, `referenceSha256`, `nativeGridStatus`, `cameraBaselinePath`, `candidates[]` com `id`, `family`, `rawPath`, `promptPath`, `sha256`, `width`, `height`, `alphaStatus`, `tileSize`, `ppu`, `footpoint`, `paletteNotes`, `perspectiveNotes`, `connectionsStatus`, `visualStatus`, `decisionReason`. Medidas desconhecidas usam null/UNMEASURED; nunca se inferem pixels nativos do tamanho CSS.

Eventos/save/UI runtime: N/A, pois nenhuma API de gameplay ou schema novo é autorizada.

## 17. Sistemas afetados

Ferramentas Editor, geração FarmScene, arte modular, captura, evidência visual. Contratos de cultivo/navegação só são afetados se um achado posterior justificar correção localizada.

## 18. Arquivos permitidos e condições

```text
.specs/a_implementar/spec_farm_pixelart_cohesion_and_ingame_review_v1.md
docs/art_catalog/FARM_GPT_PIXEL_ART_WORKFLOW.md
docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md
docs/validation/playmode/farm_gameplay_review/**
docs/validation/playmode/farm_gameplay_capture.log
docs/validation/playmode/farm_capture*.png
art/farm-pixelart-review/** (raw/, prompts/, backups/, manifest e preview)
Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs
Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs (+meta Unity, somente se necessário)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs
Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs
Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs
Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs
Assets/_Game/Scenes/FarmScene.unity (somente regeneração Unity API com backup anterior)
Assets/_Game/Art/Generated/World/**/*.meta (somente efeitos de reimportação pelo gerador, com backup)
Assets/_Game/Art/Generated/World/_TileAssets/** (somente tiles gerados Unity API, com snapshot prévio)
```

Diretório confirmado por leitura de WorldTilemapGround.TileAssetDir: Assets/_Game/Art/Generated/World/_TileAssets. O backup deve preservar conteúdo prévio e manifest de existência/hashes: arquivos criados depois são distinguíveis de arquivos alterados. Não usar `Inicializar Projeto` global. Executar somente `FarmSceneCapture.RegenAndCapture`, que chama o gerador da Farm. A autorização é de materialização da cena atual e seus efeitos declarados, não de substituir PNGs em Assets. Importação de nova arte exige emenda com caminhos exatos, candidato inspecionado e contrato de importação.

## 19. Arquivos proibidos

Demais cenas/prefabs/assets, PNGs existentes em Assets, save humano, runtime de Player/Combat/Save, Packages, ProjectSettings, docs_old. Sem YAML manual, git reset/stash/clean, commits de trabalho alheio ou mudança global de importação.

## 20. Plano por fase e edição

### Fase0 — preservar e reconciliar

Registrar hashes e copiar FarmScene, metas World e `_TileAssets` existentes para `art/farm-pixelart-review/backups/<timestamp>/`. Registrar arquivos ausentes. Captura da cena salva já obtida é baseline diagnóstico, não PlayMode. Nenhuma restauração automática sobre alterações concorrentes.

### Fase1 — regenerar o estado já implementado

Rodar `FarmSceneCapture.RegenAndCapture` via Unity batch com `-quit` (método síncrono). Inspecionar full/closeup/animals. Registrar quais problemas desapareceram e quais permanecem. Exceção focal autorizada nesta revisão: corrigir a mistura de grama em CreateFarmGroundTexture antes dessa única regeneração, conforme emenda abaixo; o relatório separa essa mudança nova das correções que já estavam no gerador dirty. Pattern: `unity-asset-generation`, proprietário `CreateMvpFarmScene`.

### Fase2 — capturar câmera real

Em `FarmSceneCapture`, acrescentar `CaptureFarmGameplayBatch` que valida estado, registra setup e inicia sessão assíncrona. Usar playModeStateChanged/update com estado persistente entre domain reload quando necessário; remover callbacks ao encerrar. Resolver câmera/player pela cena carregada e referências já existentes; helper Editor pode percorrer raízes, sem introduzir buscas runtime.

```text
validate scene + exclusive editor + prevent saves
record hashes and scene setup
open saved FarmScene without regeneration
persist capture stage across domain reload; enter PlayMode
wait for bootstrap/player/camera readiness and stable camera
for spawn, bridge, cultivation:
  position player temporarily; wait follow camera to settle
  render actual gameplay camera; preserve projection and restore targetTexture
  write PNG + measured metadata
exit PlayMode; verify scene/save hashes; detach callbacks
write status; batch-only Exit(0/1)
```

Spawn usa posição real inicial; bridge usa probe atual do contrato; cultivo usa ponto livre validado pelo contrato após inspeção, registrado em metadata. Não fabricar câmera top-down para alegar gameplay. Captura de caminhada em movimento pode acrescentar sequência curta documentada; não promover estudo12frames por uma pose estática.

### Fase3 — contrato visual e piloto GPT

Em `manifest.json`, preencher medidas de câmera/grid antes de gerar. Criar família piloto de chão com base repetível, variações e bordas/cantos aplicáveis, conforme workflow. Preservar raw+prompt, inspecionar alpha/conexões. Em preview, mostrar mosaico repetido e personagem na escala medida. Uso de imagem GPT é geração de candidatos; aceitação permanece explícita.

### Fase4 — seção real e expansão regional

Antes de importar piloto, emendar §18 com paths exatos e informar candidato/PPU/pivot. Reusar `WorldTilemapGround` na seção spawn–ponte–cultivo; avaliar nova captura real. Somente então estender kit/ajustes às regiões chão/cultivo, água/ponte, montanha, homestead, bosque e animais. Em `FarmDecorationPlanner.IsForbiddenCell`, corrigir exclusões somente se o defeito persistir após regen. Em gerador/contratos, alterar somente o método/campo associado ao achado registrado; atualizar o teste comportamental proprietário, não uma suíte global de valores.

### Fase5 — relatório/closeout

Relatório com: baseline, backup manifest, comandos/exit codes, capturas, contratos medidos, candidatos/verdicts, alterações por região, testes aplicáveis, pendências, risco residual e aceitação humana. Aplicar `finish-spec`; não promover enquanto a fase regional/PlayMode pertinente estiver pendente.

## 21. Ordem segura

Baseline salvo → backup → captura real baseline quando executável → correção focal da mistura de grama + uma regeneração → inspeção/captura real atualizada → contrato → piloto de chão → seção real → expansão regional → aceitação. A numeração das fases agrupa responsabilidades; esta é a ordem operacional. Capturas podem compartilhar uma sessão Unity se isso preservar isolamento e evidência por estágio. Pesquisa/documentação podem rodar em paralelo; mutações Unity não.

## 14. Critérios binários e DoD

| Critério | Comando/evidência e saída esperada |
|---|---|
| Backup anterior a regen | `Get-FileHash` dos arquivos originais comparado ao manifest: `Backup integrity: PASS`; registrar contagem real, sem número presumido |
| Regeneração focal | `-executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture -quit`: exit0 e três mensagens `[FarmSceneCapture] salvo:`; diff listado por path |
| Captura gameplay | comando §16.1: exit0, `Farm gameplay capture: PASS (3/3)`, PNGs e metadata correspondentes |
| Cena/saves preservados na captura | manifesto antes/depois: `Persistent files unchanged: PASS`; divergência resulta FAIL e explicação |
| Piloto rastreável | inspeção de manifest e arquivos: `Raw and prompt provenance: PASS`; cada candidato aponta PNG/prompt existentes e hash |
| Junções e escala avaliadas | preview inspecionado e relatório com `Tile connections: PASS/FAIL` e `In-game scale: PASS/FAIL`; PASS exige evidência visual concreta |
| Fazenda completa | seis linhas regionais no relatório com PASS e evidência, além de rota humana concluída; qualquer PENDING impede closeout artístico |

As mensagens novas são contrato esperado da implementação, não resultados já obtidos.

## 23. Edge cases e falhas

- Autosave inicia no bootstrap: impedir a escrita antes de PlayMode; sem isolamento comprovado, NOT RUN.
- Domain reload perde callbacks/estado: persistir estágio Editor e limpar ao completar/timeout, evitando execução eterna.
- Camera/player ausentes ou câmera Cinemachine não estabiliza: FAIL contextual, sem câmera substituta apresentada como real.
- Gerador reimporta metas/cria tiles: backup prévio inclui efeitos conhecidos e diff posterior; não apagar arquivos novos automaticamente.
- Dirty scene ou outro Unity ativo: preservar estado e não disputar lock; registrar impedimento.
- GPT retorna fundo opaco/células erradas: candidato reprovado/revisado; não importar como aprovado nem inventar alpha.
- Arte maior encobre caminho/cultivo: tratar footprint/sorting com prova física, não apenas diminuir sprite até caber.
- FAIL farm preexistente: manter identificação separada; não mudar assert apenas para obter verde.

## 22. Gates proporcionais

A própria execução Unity bem-sucedida comprova compile da ferramenta; não repetir compile-only sem necessidade. Captura exige cenário PlayMode acima e integridade; não exige suíte global. Se métodos puros novos tiverem regras não triviais, adicionar testes nomeados antes de implementá-los e emendar paths; não criar teste espelho de constantes. Mudanças espaciais posteriores selecionam fixtures/validators já proprietários da região. Registro obrigatório de NOT RUN inclui Reason, Command attempted e Residual risk. Captura automatizada não equivale à navegação humana, à animação12frames integrada ou à aceitação visual final.

# /speckit.tasks

- [x] Registrar backup/hashes e baseline diagnóstico preservado.
- [x] Regenerar somente FarmScene e inspecionar três vistas atualizadas.
- [x] Implementar captura PlayMode assíncrona com prevenção de save e cleanup.
- [x] Capturar spawn/ponte/cultivo com câmera real; verificar hashes.
- [x] Medir contrato de grid/câmera e registrar comparação farmer–fazenda.
- [ ] SUPERSEDED: kit piloto novo de chão — candidato único rejeitado; estratégia atual reutiliza tiles existentes.
- [x] Emendar paths e integrar arte inspecionada na cena real: casa, píer/barco e cascata; grama rejeitada não importada.
- [x] Expandir e avaliar as seis regiões; circulação física automatizada16/16 PASS antes da correção final exclusivamente de sorting, com captura final renovada pelo owner de validação.
- [ ] Registrar gates proporcionais, pendências e revisão humana; executar finish-spec.

## Emenda executável — mistura de chão focal (2026-09-08, antes da implementação)

Auditoria complementar encontrou grama64×64px com128PPU, célula0.5unidade; o comentário de48px em FarmScaleContract não demonstra a resolução dos assets atuais. Câmera atual usa ortho8.5 e snap128, parâmetros a registrar novamente na captura real. A variante `ground_grass_b` tem hash igual à base. A variante `ground_grass_a` tem médiaRGB aproximadamente[61.8,76.5,5.5], enquanto a base tem[84,99.9,3.1]; a alternância de15% de A contribui para blocos de valor escuro. Essa métrica explica uma hipótese de contraste, não prova qualidade visual sem captura.

Edição exata autorizada em `CreateMvpFarmScene.CreateFarmGroundTexture()`: substituir somente a chamada a `WorldTilemapGround.PaintGrass` pela composição das APIs existentes `SpriteWorldSize`, `GetOrCreateLayer` e `PaintRectWeighted`. Declarar pesos nomeados no proprietário Farm: `FarmGrassBaseWeight = 85`, `FarmGrassFlowerWeight = 10`, `FarmGrassPebbleWeight = 5`. Manter o mesmo parent, gridName WorldGrid, layer Ground, sorting0, center(-1,-1) e size(72,52).

```text
base = ground_grass (já resolvida no método)
cellSize = SpriteWorldSize(base)
tilemap = GetOrCreateLayer(ground.transform, WorldGrid, Ground, cellSize, 0, Ground)
defs = [base:85, ground_grass_flower:10, ground_grass_pebble:5]
remover somente variantes ausentes das listas; base sempre permanece
PaintRectWeighted(tilemap, spritesDisponiveis, pesosCorrespondentes, centroExistente, tamanhoExistente)
```

Não mudar `WorldTilemapGround.PaintGrass`: outros mundos, incluindo Town, continuam com sua mistura atual. Não mudar grid, aragem, física ou seed da distribuição. Não criar API/pattern novo para três pesos editoriais. Sem edição em `Assets/` de PNG nesta etapa.

DoD da fatia: compile comprovado pela execução Unity; inspeção do Tilemap FarmGround mostra zero células cuja sprite seja ground_grass_a ou ground_grass_b, apenas as três famílias autorizadas; log/relatório registra `Farm ground palette: PASS` com contagens medidas. Capturas antes/depois mostram a diferença e a avaliação do chão, sem inferir que o kit final já existe. O diff confirma PaintGrass global intacto. Teste automatizado novo não é necessário para essa simples composição reversível: o cenário de geração/inspeção real cobre o contrato e a ausência de alteração dos outros mundos.

- [x] Aplicar mistura85/10/5 somente em CreateFarmGroundTexture e registrar diff.
- [x] Gerar uma vez, verificar famílias reais no Tilemap e comparar capturas.


## Emenda executável — paridade visual do ShippingBin entre Edit e Play (2026-09-08)

Evidência nova da captura PlayMode atual: resolução640×480, câmera ortho8.5; sprite do player `idle_right_04`,41×76px,128PPU e escala2 produz altura mundial1.1875 e altura projetada aproximada33.53px. Esses valores são medidos nesse enquadramento, não um contrato permanente para toda resolução. No spawn(24,3), o ShippingBin fica grande e oculta o player. Corrigir essa oclusão tem prioridade sobre aumentar frames ou pixels do personagem.

Causa identificada: `CreateMvpFarmScene.CreateShippingBin()` aplica o perfil via `AttachApplicator` ao root ShippingBin e depois chama `ApplyUniformBespokeScale(root, sprite, 2f)`. Em PlayMode o `Awake` do applicator reaplica o perfil e substitui o ajuste visual do root. A captura diagnóstica já usava alvo2unidades; a correção restaura paridade Edit/Play, não aumenta/reduz o alvo artístico aprovado nessa captura.

### Edição autorizada, exclusivamente em CreateShippingBin

- Manter root, perfil/applicator, trigger, posição, collider, interactable e IDs existentes.
- No caminho em que o PNG existe, criar filho `Visual`, colocar nele o SpriteRenderer e aplicar a escala visual somente ao filho.
- Declarar alvo nomeado `ShippingBinVisualTargetHeight = 2f` no proprietário Editor da geração. Usar a escala positiva efetiva do root: `ApplyUniformBespokeScale(visual, sprite, ShippingBinVisualTargetHeight / root.transform.lossyScale.y)`. Conferir antes que a escala seja finita e maior que zero; falha de invariante produz diagnóstico contextual e não uma divisão inválida. Um helper local de compensação só se necessário para manter coesão, sem API runtime nova.
- Root não recebe mais o bespoke override. Confirmar que o mesmo perfil determina sua escala em Edit e Play.
- Configurar `SortPointPivot` e Y-sorting do filho de modo consistente com o apoio visual e o contrato existente. Não assumir que mover o renderer preserva automaticamente o sorting.
- Quando o PNG estiver ausente, manter o fallback builtin no root e seu comportamento atual.

### DoD e captura

Estender os dados de vista da captura, sem criar um sistema paralelo: incluir `shippingBinVisualWorldHeight`, `shippingBinRootScale`, `shippingBinVisualScale` e identificação do renderer. Metadados de persistência distinguem `persistentRoot` e `saveRoot`, sendo o segundo a pasta `saves` sob o primeiro. A evidência de falha anterior de medição/hash permanece histórica; não reescrever seu status retroativamente.

Critério geométrico: renderer visual do ShippingBin tem altura mundial2.0±0.02 em Edit e Play; log/relatório esperado `ShippingBin visual parity: PASS`. Captura spawn com câmera real deve permitir ver o player, sem oclusão pelo bin; registrar `Spawn player visibility: PASS/FAIL` com imagem. Geometria correta sem inspeção visual não satisfaz esse segundo critério. Diff confirma collider/tamanho, posição, perfil e IDs preservados; compare suas propriedades antes/depois. Executar nova regeneração focal e captura após a edição, sem importar PNG novo.

- [x] Corrigir child Visual e compensação de escala no ShippingBin, preservando root/fallback.
- [x] Capturar altura/escala do ShippingBin e verificar paridade2.0±0.02unidades.
- [x] Inspecionar visibilidade do player no spawn e registrar evidência atual, preservando falhas históricas.

## Evidência da primeira fatia — closeout parcial

[Relatório individual](../../docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md): captura real3/3, chão sem A/B, ShippingBin2u e player visível; scene/saves preservados durante captura. Pesquisa concluída. Status IN_PROGRESS; promoção NO. O piloto único de grama foi documentado e rejeitado (FAIL_NATIVE_GRID/REVIEWED_REJECTED), sem importação; kit de bordas/cantos e exportação nativa não foram realizados. Todas as seis regiões continuam PENDING. Movimento, integração12frames, navegação e aceitação humana NOT RUN.

- [x] Gerar e inspecionar um candidato de grama com raw/prompt/hash/preview; rejeição registrada.
- [x] Registrar gates desta fatia e decisão finish-spec de não promoção.

## Emenda autorizada — reconstrução integral pela keyart (2026-09-09)

O pedido humano atual corrige o objetivo da fatia anterior: entregar a FarmScene materializada com as seis regiões reconstruídas, não encerrar em baseline/piloto. A restrição de CURRENT_STATE de não corrigir farm incidentalmente fica reconciliada explicitamente: esta tarefa agora é a correção farm autorizada, preservando IDs e saves e sem tocar outras cenas.

Plano executável: mover rio/ponte para leste (centro x27), casa para(16,9), estufa(23,8), cultivo em dois blocos centrais, prédios sul alinhados em(-16,-9,-2,5), floresta apenas oeste com clareiras e caminhos conectados de aproximadamente2u. Corrigir montanha para contorno irregular, água azul profundo, solo e trilhas coesos via tint local dos Tilemaps. Reusar sprites e APIs existentes; não gerar raster por código. Culturas decorativas existentes continuam explicitamente visuais; não criar estado de plantação fictício. Casa preserva interior percorrível e roof reveal; eliminar piso visível fora da silhueta se encontrado.

Arquivos adicionais permitidos, pela relocação canônica e testes: `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs`; `Assets/_Game/Tests/EditMode/Farm/FarmSceneSpatialContractTests.cs`; `Assets/_Game/Tests/EditMode/Farm/FarmSceneCompositionContractTests.cs`; `Assets/_Game/Tests/EditMode/Farm/FarmLevel1LayoutContractTests.cs`; `Assets/_Game/Tests/EditMode/Farm/FarmLandmarkCompositionTests.cs`; `Assets/_Game/Tests/EditMode/Editor/FarmDecorationPlannerTests.cs`. Atualizar testes de geometria afetados pelo novo layout sem remover invariantes de acessibilidade.

- [x] Reconciliar âncoras e polígonos de água/ponte/construções.
- [x] Reposicionar seis regiões, limpar plantio/prédios e remover vegetais decorativos incompatíveis com cultivo real.
- [x] Pintar trilhas e contorno de montanha; aplicar paleta local, com limites geométricos de tiles documentados.
- [x] Regenerar FarmScene via Unity API, testar contratos e capturar visão geral + seis regiões + câmera real; renovação final de sorting sob owner Unity.
- [x] Inspecionar e iterar diferenças observadas; registrar limitações de arte/movimento sem declarar aceite humano.

Paths adicionais desta mesma reconstrução: `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneNavigation.cs`, `Assets/_Game/Tests/EditMode/Farm/FarmSceneNavigationContractTests.cs` (probes por contratos); `Assets/_Game/Art/Generated/World/building/farmhouse_keyart_v2.png` e seu `.meta` (candidato GPT somente após inspeção root, import Unity API Point/None/noMip/FullRect/128PPU/bottom-center, original preservado). Captura/validador possuem owner dedicado para vistas das seis regiões e teste físico de circulação.

Captura desta reconstrução:8/8vistas(spawn,bridge,cultivation,homestead,forest,mountain,animals,lake), mais rotas físicas automatizadas separadas de aceite humano/input real. Sprite farmhouse_v2 usa pivot bottomcenter do importer e compensação child pela margem103px, largura visível1146/1374 medida, sem transformar PNG.

Owner de validação pode criar `Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs` e meta, helper Editor de BFS/casts reais com limite12000células, semruntime novo. Culturas estáticas foram removidas para que aragem/plantação reais não sejam sobrepostas por sprites permanentes; blocos estarão prontos ao cultivo e não representarão maturidade fictícia.

Arte adicional autorizada nesta reconstrução: `Assets/_Game/Art/Generated/World/props/fishing_dock_keyart_v1.png` e `.meta`, cópia byte exata do raw inspecionado. Import Point/None/noMip/128PPU/FullRect/bottomcenter pelo importer existente. Posicionar passarela e deck em terra na margem noroeste e barco sobre água; não abrir passagem física sobre água nem mudar serviço/save de pesca.

### Segunda passada visual — correções verificadas na captura regen01

A captura mostrou piso/paredes com pivot bottomcenter tratado como center, píer com sortingOrder0 porque TrySetSortingLayer só aplica ordem no fallback, estufa elevada, cogumelo ampliado pela escala do root FishingSpot e decoração sem normalização de tamanho. Corrigir no gerador; trocar caminhos para ground_sand existente com tint local e sobrepor pequenas pedras neutras ao contorno de água. Não modificar PNGs.

Paths adicionais autorizados: `Assets/_Game/Scripts/World/RoofRevealController.cs`; `Assets/_Game/Tests/EditMode/World/RoofRevealControllerTests.cs` e meta. Extensão opcional `_interiorRenderers` com array vazio por padrão, `Configure(..., SpriteRenderer[] interiorRenderers = null)` preservando chamadas existentes. Fora da casa: interior renderers desabilitados; dentro: habilitados enquanto telhado oculto, sem modificar suas cores. Colliders/percurso continuam ativos. Testar transições com múltiplos colliders do player e compatibilidade com chamadas legadas. Nenhuma modificação de cena Town.

Arte final adicional autorizada pelo humano via direção desta reconstrução: `Assets/_Game/Art/Generated/World/props/waterfall_keyart_v1.png` e `.meta`, raw GPT inspecionado887×1774RGBA. Importerexistente Point/None/noMip/FullRect/128PPU/bottomcenter, semeditarPNG. Cascata estreita sobre nascente norte(27,17), altura visível4.4u com margem inferior157px compensada; colisão de água/montanha permanece. Não criar cascata secundária sem relevo correspondente.

### Correção final de sorting observada em regen04

Flores/arbustos tinham World order1 acima das árvores World0, produzindo folhagem sobre copas. No gerador, cobertura rasteira da família foliage agora usa Ground4; props em pé, poço e espantalhos usam World0 e Y-sort pelo pivot. Mesmo helper cobre decoração planejada e acentos manuais. Sem mudança de física, conteúdo runtime ou arquivos de imagem. Regenerar e capturar novamente a cena final; testes geométricos/físicos prévios podem ser reutilizados conforme diff, sem declarar imagem antiga como evidência do novo sorting.


## Entrega técnica da reconstrução integral

As seis regiões receberam implementação e inspeção pelo orquestrador: chão/cultivo, água/ponte, montanha, homestead, bosque e prédios sul. Veredito regional: PASS de implementação aproximada da referência; nenhuma dessas marcações representa aceite humano. A FarmScene materializada é a entrega desta rodada, superando explicitamente a fatia inicial de chão/ShippingBin. A última regeneração/captura atualiza somente sorting de folhagem/props; o relatório individual do owner identifica os hashes finais e as evidências correspondentes.

Diferenças preservadas e explícitas: canteiros começam vazios para aceitar cultivo real; bordas da água e montanha continuam mais geométricas por usarem tiles; densidade e alguns assets divergem da keyart. Casa, píer/barco e cascata GPT foram integrados como assets novos, preservando raw e originais. O piloto anterior de grama continua rejeitado; kit novo de bordas/cantos não foi criado e não é requisito pendente da estratégia atual de reutilização de tiles existentes. Os itens históricos de piloto/seção real permanecem registrados como SUPERSEDED pela reconstrução integral, sem alegar produção de arte inexistente.

Status geral IN_PROGRESS e promoção NO enquanto a aceitação humana final exigida pelo DoD não ocorrer. Isso não rebaixa a entrega técnica para baseline: a reconstrução foi executada e a cena está entregue para revisão.

### Correção funcional visual da ponte — captura gameplay04

O probe físico16/16 comprovou passagem, mas a imagem mostrou player sobre água abaixo do deck: sprite471×345 com pivot bottomcenter e centro do deck aproximadamente(235,140) em coordenadas de imagem. O centro do deck fica205px acima da base. Manter root Bridge_01(27,3), escala e footprint existentes; colocar renderer no filho Visual e compensar sua posição por `-(sprite.bounds.min + sprite.bounds.size * deckNormalizedAnchor)`, com âncora(.5,205/345). Ground order5 mantém o deck sob o player World; não deslocar colliders para mascarar erro visual. Captura final da ponte deve provar apoio dos pés no deck, separadamente do PASS físico.

Refino pelo frame real: player colliderheight1.125 exige foot/root27,2.5 no corredor físico[2,4]; apoiar o deck na mesma latitude2.5, aplicando offset visual mundial-0.5 convertido à escala local do root. Root(27,3) e geometria permanecem; não usar esse ajuste para modificar passagem.


## Reabertura artística por rejeição humana — 2026-09-09

O usuário rejeitou montanha, árvores, lago e a aproximação visual anterior. Os PASSs técnicos/regionais anteriores são evidência histórica de implementação e física, não representam aceite. Após a continuação descrita abaixo, a NOVA versão está EM REVISÃO / IN_PROGRESS: a rejeição anterior não é automaticamente atribuída à nova imagem ainda não avaliada pelo humano. Os seis PASSs aproximados anteriores não autorizam encerramento.

Plano desta revisão: substituir grade de árvores e máscara retangular larga por agrupamentos irregulares de coníferas, preservando TreeNode IDs; alturas variadas e clareiras articuladas aos caminhos reais. FarmDecorationPlanner passa a compor manchas de cobertura rasteira em vez de sorteio uniforme por todo o mapa. Lago recebe agrupamentos variados de rochas e tom teal, sem inventar juncos/lírios ausentes da biblioteca. Caminhos recebem vegetação de borda em grupos sem bloquear circulação. O novo módulo de penhasco GPT será inspecionado e integrado em caminho versionado declarado antes da cópia; substituirá a faixa repetitiva apenas visual, preservando colisão canônica. Não alterar ponte funcional, save schema ou outras cenas. Não declarar prontidão por teste.

Arquivos autorizados continuam gerador, FarmDecorationPlanner e sua fixture existente; se necessário criar `Assets/_Game/Scripts/Editor/Art/FarmLandscapeVisualComposer.cs` (+meta) para composição visual Editor coesa de módulos/rochas. Novas posições de árvores exigem renovar rotas físicas. Conteúdo de arte permanece via imagegen e cópia byte exata; nenhuma edição raster por código.

- [x] Redistribuir bosque e cobertura rasteira em agrupamentos e avaliar comparação na nova versão.
- [x] Refazer composição visual das margens e tom da água; inspecionar junções e apoio do player na ponte.
- [x] Integrar módulos GPT de penhasco inspecionados, substituindo a faixa repetitiva anterior.
- [x] Regenerar/capturar com owner Unity exclusivo e reavaliar visualmente; revisão do orquestrador não equivale a aceite humano.

Assets inspecionados desta revisão, antes da cópia: `Assets/_Game/Art/Generated/World/props/cliff_keyart_v1.png` (+meta), raw `art/farm-pixelart-review/raw/cliff-keyart-v1.png`,2172×724RGBA,bboxalpha>128(17,27,2155,693); `props/reeds_keyart_v1.png` (+meta), raw `raw/reeds-keyart-v1.png`,1313×1198RGBA,bbox(309,176,1118,1075). PNGs byte-exatos, Point/None/noMip/FullRect/128PPU/bottomcenter. Cliff usa maxTextureSize4096 somente neste asset para não reduzir automaticamente2172px; sem editar importer global. Módulos sobrepostos de cerca5u de altura visível substituem a faixaCliff; mantêm collider canônico. Reeds em grupos ao longo das margens, altura0.9–1.3u.

Terceiro asset autorizado desta revisão: `Assets/_Game/Art/Generated/World/props/lily_pads_keyart_v1.png` (+meta), raw `art/farm-pixelart-review/raw/lily-pads-keyart-v1.png`,1428×1102RGBA,bboxalpha>128(342,234,1086,868). Somente candidato exec-fe4e637c... aprovado para avaliação; versões com halo/checker permanecem rejeitadas e não importadas. Lírios em pequenos grupos sobre água rasa do lago, largura visível0.6–1.1u e Ground4; sem colisão ou mudança de pesca.

Piloto de água calma autorizado para avaliação contextual: `Assets/_Game/Art/Generated/World/tiles/ground_water_keyart_v1.png` (+meta) e raw `art/farm-pixelart-review/raw/water-calm-keyart-v1.png`, imagemGPT1254×1254. Não usar dimensão natural como grid: manter Tilemap de água0.5u e mesmas células rasterizadas; Tile asset via WorldTilemapGround.GetTile e SetTransformMatrix por célula escalam sprite ao tamanho da célula. Anchorbottom para casar pivot; nenhuma edição raster, nenhuma mudança de máscara/collider. Primeira captura decide leitura/repetição; não declarar aprovação só por importar.

Margem contínua autorizada: `Assets/_Game/Art/Generated/World/props/shore_bank_keyart_v1.png` (+meta), raw `art/farm-pixelart-review/raw/shore-bank-keyart-v1.png`,2172×724RGBA,bbox(47,207,2112,522),maxTextureSize4096local. Cadeia de módulos com overlaps, orientação grassladoexterno decidida por probes de água em ambosnormais; não desenhar borda interna na união rio/lago. Ground4 sob deck5, sem collider. Mesmo módulo pode ser avaliado em bordas de trilha com probes de união dos caminhos, mantendo corredores abertos. Acentos decorativos da faixa norte não podem flutuar à frente do penhasco; nós mineráveis funcionais preservados. Câmera Editor adicional autorizada em FarmSceneCapture:1600×1000,ortho22,centro(0,0), aspect1.6 (não igual à keyart1.777), mantém overview e gameplay.

Correção óptica autorizada: Assets/_Game/Scripts/World/TreeNode.cs serializa healthyTint com default legado e Configure(..., Color? healthyTint = null); somente CreateMvpFarmScene passa Color.white, preservando arte saudável em Awake/restore/regrowth. Assets/_Game/Tests/EditMode/World/TreeNodeVisualTests.cs (+meta) cobre opt-in e comportamento legado; owner capture implementation, sem mudanças de save schema.

Grama v2 autorizada antes da cópia: Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png (+meta), raw art/farm-pixelart-review/raw/ground-grass-keyart-v2.png. Candidato1254square em Tilemap próprio do chão na dimensão natural1254/128u, sem misturar variantes64px, anchorbottom, tint local leve. Mantém cobertura72x52 e não altera grid de gameplay/aragem/colisão. Piloto v1 rejeitado permanece preservado. Aceite de textura, seams e densidade de pixel depende da próxima captura.

Correções após captura orgânica02: validar a fronteira da união ao longo de toda a largura das peças (extremos/quartos/centro), reduzir peças junto às junções sem mínimo2.8u e omitir atravessamentos internos. Retirar células VISUAIS de água acima17.5, mantendo contrato/colliders; cascata representa descarga. Realocar quatro LockedOreNodes para pé da montanha y15.5, preservando IDs, escala e interação; nova captura comprova apoio visual e acesso. Escopo: FarmLandscapeVisualComposer.cs e CreateMvpFarmScene.cs; TreeNode e capture congelados.

Correção focal de Homestead após Planner FAIL: compor quatro manchas autoradas de flores/arbustos nos flancos oeste da casa e no jardim superior entre casa/estufa, com afastamento vigente2u, sem mexer em portas, approaches ou clearance global. Preservar limites5..48; não perseguir apenas mínimo5. FarmDecorationPlanner.Plan adiciona grupos determinísticos após cobertura geral, com mesma rejeição IsForbiddenCell. Fixture Assets/_Game/Tests/EditMode/Editor/FarmDecorationPlannerTests.cs informa count no erro do intervalo. Captura e nova execução focal pelo owner validam resultado.

### Evidência e closeout desta continuação — 2026-09-09

- [x] TreeNodeVisualTests3/3 e composição4/4PASS no focal01; primeira falha Homestead preservada como10/11.
- [x] Corrigir Homestead sem reduzir intervalo5..48; rerun Planner4/4PASS e19decorações materializadas.
- [x] Regeneração04 da continuação via Unity,10diagnósticos e câmera extra fixa; diagnóstico automático completo preservado.
- [x] PlayMode atual8/8capturas e16/16aproximações físicas,6529nós,zero runtimeErrors; cena/saves preservados.
- [x] Inspeção de composição e oito frames pelo orquestrador, auditoria independente e comparativo atualizado.
- [ ] Receber veredito humano sobre a NOVA versão e executar ações/input/animação em movimento; capturas não satisfazem esse critério.

Evidência: [relatório individual atualizado](../../docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md), [metadata da continuação](../../docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/gameplay_01/capture-metadata.json). Hash da cena: `7e2ffd6919591e634444d943ae8eb26d1eff4aeca886a8f03f0b8cbe60a2ea2c`. Nova versão EM REVISÃO; Unity SCOPED_PASS; spec IN_PROGRESS; promoção NO. Composição/props/vegetação continuam aproximações e canteiros estão vazios intencionalmente; não declarar animação12frames integrada.

O [audit de identidade](../../docs/validation/playmode/farm_keyart_delivery_20260909/continuation_art_01/scene-identity-review.json) registra50→71TreeNodes:50índices anteriores preservados e21antes ausentes agora ativos. A reconstrução altera densidade de recursos, não só aparência; não alegar contagem estável nem economia inalterada. Nenhum perfil humano foi carregado/salvo na prova. O preview usa overview automático completo primeiro; recorte fixo22 é alternativo e pode cortar topo do portal. Diagnósticos automáticos antes/depois têm zoom diferente por mudança de bounds, apesar da resolução igual.

## Fechamento coordenado F01–F05 — autorização humana de subtasks

Executar integração final por ownership independente e revisão do root contra keyart após cada captura. Preservar 71 árvores existentes, IDs, saves, colliders e ponte funcional; não reabrir sistemas nem acrescentar recursos coletáveis. Snapshot CURRENT_STATE de08/09 permanece histórico; autorização humana vigente reconcilia explicitamente as correções farm com as falhas anteriores citadas nele.

- [x] F01 — Terreno/montanha/água: farm_keyart_rebuild audita capturas regen04/gameplay01 de continuation_art_01 e corrige apenas margens retas/finas, continuidade das transições e contraste incoerente comprovados. Paths: Assets/_Game/Scripts/Editor/Art/FarmLandscapeVisualComposer.cs e Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs. Critério: margem de água contínua com leitura orgânica, sem faixas internas nem água acima da cascata; volume rochoso coerente e acesso à caverna preservado.
- [x] F02 — Bosque/acessos: mesmo owner, Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs e integração CreateMvpFarmScene.cs. Critério: 71 árvores/IDs preservados, coníferas e clareiras legíveis, caminhos e aproximação da caverna sem oclusão visual artificial; não alterar físicos para encobrir erro gráfico. Contraste avaliado contra grama/keyart, não por valores isolados.
- [x] F03 — Casa/cultivo/animais: captureworker possui exclusivamente NOVO Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs (+meta). Composição Editor de assets existentes, sem novos sistemas, sem plantas falsas sobre solo arável, sem colliders novos ou alterações de save. Critério: casa/estufa, quatro edifícios sul e canteiros leem como assentamento vivido, portas/approaches visíveis e acessíveis. Assinatura acordada com F04 antes de integrar; não editar gerador do outro owner.
- [x] F04 — Integração: farm_keyart_rebuild chama novo helper em CreateMvpFarmScene.cs após receber contrato e revisar diff; preservar bootstrap, hierarquia funcional e alterações concorrentes. Congelar C# em conjunto antes de abrir Unity.
- [x] F05 — Unity/visual: validator exclusivo gera cena, captura overview/regiões e8vistas de gameplay, valida16rotas físicas e testes focais afetados; root compara contra docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png e retorna achados até resolução. Paths de evidência em docs/validation/playmode/farm_keyart_delivery_20260909/; Assets/_Game/Scenes/FarmScene.unity(+meta) somente via Unity. PASS de teste não implica aceite artístico/humano; promoção continua dependente dos gates aplicáveis e da revisão final.

Regra de coordenação: F01/F02 e F03 podem editar arquivos separados em paralelo; ninguém abre Unity durante edição. Não acrescentar arte nova sem necessidade concreta verificada na captura e geração autorizada pelo root. DoD final distingue entrega técnica, comparação visual do root e aceite humano.

F04 inclui correção de densidade visual dos canteiros autorizada pelo root: PaintCentralFieldSoil em CreateMvpFarmScene usa VisualFieldGrid2u e ground_soil64px escalado por matrix4, pivotbottom. Mesmos retângulos14x6 alinhados à grade2, preservando FarmTileGrid/aragem/runtime e regiões físicas. Captura contextual decide legibilidade dos sulcos; não gerar PNG nem plantar decoração imóvel.
F04 contrato acordado: FarmSettlementVisualComposer.Create(Transform parent), namespace CindarsHope.Editor.Art, chamado após CreateFarmDecoration com parent FarmDecoration (origem, escala1). Helper cria FarmSettlementDetails com composição visual; nenhum root funcional deslocado.

F05 extensão autorizada de seleção real de interação (owner captureworker): editar Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs e criar Assets/_Game/Scripts/Editor/Validation/FarmInteractionSelectionProbe.cs (+meta). Smoke stateful após8fotos: HouseDoorInteractable deFarmHouse/Door e CraftingPoint dos três roots reais workbench/forge/cooking. Usar endpointBFS e busca local2u com Cast/Overlap do player real; esperar ao menos2FixedUpdates e verificar GetCurrentInteractable==alvo. Não executar Interact nem forçar RegisterCandidate. Registrar quatro resultados, evaluated e status explícitos; restaurar posição/velocidade e preservar cena/saves. Critério4seleçõesPASS, sem mascarar ausência/alcance/seleção errada. Isso comprova seleção automática real do alvo, não prova execução de crafting, input humano ou todos os sistemas de gameplay. Nenhum runtime edit autorizado por esse gate.

Correção comprovada F05 porta FarmHouse: blocker emy5.99 comheight.22 tem limite frontal5.88; playerbody1.125 acima do pé permite rootY≤4.755. Trigger anteriorheight1.62 começa5.18 e seletor global.45 a partir do pé exige rootY≥4.73: janela de apenas.025u, eliminada na prática por skin/grade de busca. Ajustar SOMENTE CreateFarmHouseDoor em CreateMvpFarmScene.cs: triggerheight2.22 (FarmWallThickness+profundidade2u), mantendo centro5.99, largura, blocker, walls, runtime e alcance global. Novo limite frontal4.88 permite seleção desde4.43 até4.755: janela.325u, incluindo posição física4.5 já testada. Critério: gate original com SearchStep.25 seleciona HouseDoorInteractable após callbacks reais; não introduzir candidato analítico especial para mascarar a fricção. IDs, posição da porta e save intactos.

F05 correção de oclusão determinística: GraphicsSettings global contém TransparencySortMode2 (Orthographic), enquanto AxisY gravado é inativo; MainCamera farm não tinha override. DLL Unity6000.5.7f1 confirma enum2Orthographic/3CustomAxis. Árvores World/order0/Z0 empatam nesse modo. Comparação read-only baselinepréclosing contra cenaatual02 encontrou71TreeIndex e ZERO diferenças de spriteGUID/posição/escala; não existe snapshot .unityclosing01, portanto não alegar comparação estrutural01vs02. Fix farm-local em CreateMvpFarmScene.cs: camera.transparencySortMode=CustomAxis e transparencySortAxis=Vector3.up. Owner capture aplica mesmo contrato à câmera temporária em Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs. Não alterar ProjectSettings/Town/runtime genérico. Nova captura deve conferir ordem das copas e oclusão player; física e IDs preservados.
Evidência F05: FarmPlayModeCaptureSession.cs acrescenta por View string transparencySortMode e Vector3 transparencySortAxis lidos da MainCamera durante Capture, comprovando override real em PlayMode. Nenhum gate/suite novo nem alteração do probe.

F05 persistência real do sorting após gameplay03: os oito registros reportam Orthographic apesar do override no gerador; configuração Editor isolada não sobreviveu ao carregamento. Reuse audit: busca transparencySort/TransparencySort e pasta Camera encontrou apenas setters Editor, WorldSortingLayers (constantes de nomes), CameraFollow2D (movimento) e CameraScaleController (zoom). Não existe adapter de sorting runtime. Autorizar NOVO Assets/_Game/Scripts/Camera/CameraTransparencySort2D.cs (+meta), MonoBehaviour fino RequireComponent(Camera)/DisallowMultipleComponent, OnEnable configura somente Camera local em CustomAxis/Vector3.up. Anexar exclusivamente MainCamera da Farm em CreateMvpFarmScene.cs; não mudar ProjectSettings, Town, outros adapters ou criar serviço. Editar FarmPlayModeCaptureSession.cs para exigir oito Views com mode CustomAxis e eixoY correto, lidos da câmera real; teste NÃO configura o modo. Esse gate PlayMode é evidência comportamental necessária, além da captura de oclusão; nenhum teste espelho adicional. Resultado gameplay03 é histórico insuficiente para sorting, não aceite final.


Fechamento documental preparado: F01–F04 implementados; F05 aguarda confirmação root de closing_pass/regen_04, gameplay_04 e replay_01. Os quatro checks de implementação não equivalem a aceite artístico/humano. O execution report preserva FAIL histórico de porta01 e insuficiência de sorting03 (Orthographic), e documenta reuso11testes por6inputs. Não promover antes da revisão final aplicável.


### Fechamento técnico vigente F01–F05

Root confirmou closing_pass/regen_04, gameplay_04 e replay_01: exit0/scan0,8vistas reais,16rotas físicas,4seleções reais,0runtimeErrors e8ViewsCustomAxis/Y. Hash da cena preservado8e68982b60724243fbef52823cf6b1c9343797788c4237b03166b9c3f940ab6c; saves preservados. Root revisou overview+8frames e aprovou oclusão/pés/entradas para esta entrega técnica. Replay conserva8/10PNGs byte-idênticos e todas6regiões idênticas; dois overviews têm diferenças residuais142/200pixels em pequena árvoreSW com empateY, portanto não há promessa de determinismo visual integral. Onze testes focais reutilizados por6inputs de hashes iguais. Fonte congelada.

F01–F05 estão executados e tecnicamente SCOPED_PASS. FarmScene entregue para revisão humana; statusgeral IN_PROGRESS e promoçãoNO. Falhas porta01 e sorting03Orthographic permanecem históricas no execution report. Não considerar essa aprovação do root como aceite humano, teste de input/animação ou execução de receitas. Campos vazios para cultivo real e divergências de composição/arte perantekeyart continuam explícitos. Evidência final: docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md.

## Revisão G — proporções, ponte e limites naturais (autorizada pelo humano em 2026-09-09)

Pedido: manter a direção visual aprovada como boa, corrigir proporções das construções e de todos os objetos perante o personagem, melhorar substancialmente a ponte, espalhar mais vegetação e demarcar extremidades inacessíveis. Esta autorização sucede F01–F05; não implica aceite integral da arte anterior.

- [x] G01: medir altura visível do player e ratios de portas, construções, bancada/baús/poço/fonte/árvores/ponte; calibrar grupos autorados em CreateMvpFarmScene.cs e, se necessário, FarmSettlementVisualComposer.cs com ownership explícito. Preservar player/câmera, IDs e funções. Não reduzir só sprite deixando colisão invisível incompatível: ajustar geometria local pelos contratos se necessário, documentando antes/depois.
- [x] G02: ponte nova de perfil baixo, passagem horizontal legível e escala adequada ao player. Autorizar geração GPT de Assets/_Game/Art/Generated/World/props/bridge_keyart_v2.png (+meta), raw art/farm-pixelart-review/raw/bridge-keyart-v2.png, prompt/proveniência. Imagem importada byte-exata Point/None/noMip; não editar raster por código. Root inspeciona candidato antes de integrar. Reusar corredor físico x[24.7,29.3],y[2,4], suporte do pé y2.5, mantendo river split e seleção/rotas. Integração no gerador sob owner Tesla, instrução do root; se escala canônica precisar ajuste, FarmSceneCompositionContract.cs e fixture correspondente são permitidos.
- [x] G03: helper Editor novo Assets/_Game/Scripts/Editor/Art/FarmPerimeterVisualComposer.cs (+meta), owner captureworker; compor bordas naturais com assets existentes, predominantemente fora faces internas x±32/y±22. Preservar portal da cave e vista/corredor town emx30..32,y1.5..4.5. Vegetação baixa em manchas internas sem árvores grandes atravessáveis: árvores sólidas internas exigem footprint claro e validação, não criar novos TreeNodeIDs nesta rodada. Galhos não podem ocultar player nos corredores; sem mudanças de save ou economia. Assinatura Create(Transform parent), costura no gerador pelo owner Tesla.
- [x] G04: Unity exclusivo validator, após freeze. Backup desta baseline; scene somente via Editor API. Evidência em docs/validation/playmode/farm_keyart_delivery_20260909/proportion_pass/. Capturar overview/regiões e contexto real do player; ampliar FarmSceneCapture.cs/FarmPlayModeCaptureSession.cs para métricas de objetos se necessário com ownership combinado. Verificar8vistas,16rotas,4seleções reais e CustomAxis/Y. Reusar testes/imports inalterados, executar testes focais só para contratos alterados. Root compara escala/apoio/oclusão e bordas, devolve defeitos. Sem alegação de teste humano por fotos.

Targets de escala serão fixados após medidas do baseline em tabela no relatório, evitando redimensionamento arbitrário global. Campos aráveis, saídas, porta corrigida e adapter de sorting devem ser preservados. Mudanças de perímetro serão visuais sobre bloqueios existentes sempre que suficiente; sem aumentar quantidade de recursos por acidente.

G01/G03 refinamento aprovado: FarmDecorationPlanner.PlanFarmTreePositions() reutiliza bosque determinístico68 e realoca somente IDs62..67 para3pares próximos(-1,13),(8,12.5),(-15,-12.8); ponto sugerido(-13,-11) rejeitado por cruzar trilha. Busca local mantém IsForbiddenCell e distância mínima1.6u de demais troncos; Trees registry68+perímetro3=71IDs. Fixture Assets/_Game/Tests/EditMode/Editor/FarmDecorationPlannerTests.cs cobre preservação dos62primeiros, determinismo,6posições válidas e separação; PlanForestTreePositions continua contrato bosque puro. Sem novos recursos/IDs.

G01 física craft medida: novosalpha1.5target workbench1.696×1.440u,forge1.402×1.446,cooking1.423×1.442; sólidos antigos0.45–0.51×0.36–0.41u cabem na nova arte, mas offsetantigo punha base abaixo dela. Preservar dimensões mundiais e assentar mínimoY na margemalpha8px medida dos3sprites. Trigger frontalheight1.7u permite origem nos pés e corpo1.125u do player selecionar sem ampliar alcanceglobal. Reduzir somente renderers em filhos; revalidar4seleções/16rotas.

G01 targets aplicados (altura de canvas, alpha medido na captura): crafting1.5u,ShippingBin1.25u,poço2.2u,fonte3.5u,coop4.5u,barn6.2u,processing4.9u,estufa5.8u. Casa permanece largura visível8.5u para preservar porta≥player e cobertura do footprint7u. Roots físicos preservados nas construções; crafting migra renderer paraVisual, root1 com geometria mundial explicitamente recomposta; processingrootmantido e sóchildVisualreduzido. FarmSceneCompositionContract constants atualizadas; teste focal requerido. G03 helperperímetro revisado e chamado sobFarmDecoration apósSettlement; nenhumresourceID novo.
G02 integrado bridge_keyart_v2 alpha1890x472px: largura VISÍVEL5.6u, escala mundialuniforme, altura1.40u; compensação nofilhoVisual para rootlegado1.8x1.4 não distorcerarte. Deckcentro(1086,369) desdebottom emcanvas2172x724 ancorado(27,2.5). Root/corredor físicos e contrato de transformação legado preservados; maxTextureSize4096 somente novoasset.


CloseoutG preparado: G01–G03 implementados e revisados pelo root na rodada01. G04 aguarda evidênciafinalproportion_pass/regen_02 e gameplay_02 após remoção visual dos mini-cliffs repetidos no perímetro. Nove testes focais01 são válidos pelos inputs pertinentes; nenhum novo claim de humano/input/animação. Executionreport contém targets versusmedidas, com casa mantida porporta e71TreeIDs preservados.


### Resultado vigente revisãoG

G01–G04 SCOPED_PASS técnico. Última cena: proportion_pass/regen_02, hash eb848c096e5e13cd015e61516ee0e7e11374708cbb22b37cf22cb9515f723d16. Gameplay02:8vistas,16rotasPASS6559nós,4seleçõesreaisPASS,0runtimeErrors,CustomAxis/Y e cena/saves preservados; processosregen02/Play02exit0/scan0. Nove testesfocais01 válidospor4hashesinalterados; semnovaexecução fictícia. Root revisouoverview/animais/ponte02 e8frames01; único delta02remove mini-cliffs repetidos.71TreeIDs preservados com6posiçõesredistribuídas, perímetroexteriorsemrecursos/colisoresnovos. SpecpermaneceIN_PROGRESS/promoçãoNO: aceitehumano, inputs, execução de ações e animação emmovimento pendentes. Evidência atual noexecutionreport; rodadasF eG01 preservadas como histórico.

## Revisão H — caminhos, clareira da caverna e vegetação atravessável

Autorização humana de 09/09/2026: melhorar ruas, árvores/arbustos sem física e posição da entrada da caverna, confrontando a keyart canônica. Baseline técnica G02 hash EB848C096E5E13CD015E61516EE0E7E11374708CBB22B37CF22CB9515F723D16. O snapshot CURRENT_STATE de08/09 permanece histórico; esta revisão dá continuidade autorizada à farm, sem corrigir incidentalmente outros domínios.

Plano: comparar visualmente os caminhos retos/largos e a entrada exterior com a referência; ajustar os polígonos existentes e sua transição para grama; deslocar a entrada exterior para a direita dentro da montanha e abrir a clareira de chegada; compor vegetação pequena sem colisão, mantendo leitura do personagem e acesso aos destinos. Reusar Tilemaps, sprites, helpers e contratos existentes. Não alterar CaveScene, geração procedural, snapshots/stable run, economia ou esquema de save. A posição visual e o trigger exterior devem concordar, nunca mover só a ilustração.

- [x] H01 — Owner farm_keyart_rebuild: FarmDecorationPlanner.cs, FarmLandscapeVisualComposer.cs, CreateMvpFarmScene.cs e contratos de farm explicitamente envolvidos na entrada/spawn/footprints. Registrar coordenadas antes/depois antes de implementar. Caminhos com largura variável, cantos suaves e ramificações de terra coerentes com keyart, preservando canteiros, ponte, portas e funções. Entrada exterior e ponto de retorno/abordagem coerentes; 71 TreeNode IDs preservados, realocação estritamente necessária para abrir clareira documentada. Testes focais existentes nos contratos/planner atualizados conforme invariantes reais, sem enfraquecer física para obter PASS.
- [x] H02 — Owner farm_capture_implementation: reusar FarmSettlementVisualComposer.cs para grupos de arbustos baixos e árvores pequenas decorativas nas laterais de caminhos/clareiras. Apenas Transform/SpriteRenderer, nenhum collider/TreeNode/recurso. Alturas e grupos definidos por comparação; não colocar grandes troncos atravessáveis no meio da circulação. Preservar solo cultivável, aproximações, passagem da ponte e visibilidade de portas/personagem. Integrar no helper já chamado pelo gerador, sem sistema paralelo.
- [x] H03 — Owner farm_delivery_validation: backup G02, depois freeze conjunto antes de Unity exclusivo. Evidência em docs/validation/playmode/farm_keyart_delivery_20260909/path_cave_pass/. Cena somente por Editor API. Verificar compilação por regeneração/testes pertinentes,8vistas de gameplay,16rotas e4seleções reais; conferir alvo da nova entrada exterior e ponto de retorno, IDs preservados e vegetação sem colisão. Atualizar somente tooling Editor de captura/probe se novos pontos exigirem, sem mudar o gate para esconder bloqueio. Reusar testes de inputs inalterados.
- [x] H04 — Root compara keyart e capturas de caminhos/bosque/montanha/casa/água/sul e devolve defeitos; captureworker atualiza delivery.html e documento visual com baselineG02 e resultadoH. Closeout distingue SCOPED_PASS, revisão visual do root e aceite humano/input/animação pendentes. Não afirmar réplica exata nem promover a spec com critérios ainda pendentes.

Execução de H01/H02 liberada após propostas concretas de coordenadas recebidas pelo root. Owners não estão sozinhos: preservar working tree e arquivos de outros agentes. Nenhum raster edit por código; se a arte existente for insuficiente, geração GPT e inspeção do root antes da integração.

H02 proposta autorizada antes da implementação: reutilizar FarmSettlementVisualComposer.cs com método privado para sete núcleos candidatos em(-2,10),(9,8),(-15,-3.5),(8.5,-4),(-5,-9),(1,-9),(-21,-12). Cada núcleo irregular combina2–4sprites existentes: bush_leafy0.3–0.5u, flower_patch0.2–0.3u e no máximo uma tree_sapling0.7–0.9u. Ground4, sem Collider/TreeNode, deixando player à frente. Conferir bounds/arestas com FarmDecorationPlanner.IsForbiddenCell para respeitar caminhos novos, solo, água e approaches; omitir conflitos sem forçar densidade. Preservar faixa reservada da cave x[-26,-16],y[12,20]. Perímetro e recursos não são alterados por H02.

H03 extensão autorizada antes da implementação: owner captureworker pode editar Assets/_Game/Scripts/Editor/Validation/FarmInteractionSelectionProbe.cs e, somente se necessário, Assets/_Game/Scripts/Editor/Dev/FarmPlayModeCaptureSession.cs. Acrescentar quinto alvo CaveEntranceInteractable da entrada EXTERIOR da Farm, usando route canônica cave_entrance_approach e posição dos contratos atualizados pelo owner H01. Mesma seleção real GetCurrentInteractable após física, sem Interact ou transição de cena; preservar os quatro alvos anteriores, alcance/budget e isolamento de saves. Entrada alvo em(-19.5,17), retorno(-19.5,14.5), conforme contrato H01. Owner Unity recebe a mudança explícita de4para5seleções; o gate não altera CaveScene/procedural/stable run.

H01 coordenadas aprovadas antes da edição: CaveEntrance(-19.5,17), farm_from_cave(-19.5,14.5), CaveMouthMin(-21.5,15.25), tamanho4x3.5; Board_Zrix(-16.5,15.5). Canonicalizar literais exteriores em Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs e consumir em Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs; FarmSceneSpatialContract.cs deriva boca pelo contrato. Não modificar IDs/targetSpawn/cave runtime.

PathsH01: CreateMvpFarmScene.cs (entrada/retorno/mural/craftground), FarmDecorationPlanner.cs (AuthoredPaths/PathApproaches/reserva de clareira), FarmLandscapeVisualComposer.cs (somente bordas de trilhas se necessário), FarmLevel1LayoutContract.cs. Testes autorizados: Assets/_Game/Tests/EditMode/Farm/FarmLevel1LayoutContractTests.cs, FarmSceneSpatialContractTests.cs, FarmSceneNavigationContractTests.cs; Assets/_Game/Tests/EditMode/Editor/FarmDecorationPlannerTests.cs. Validators/probes existentes derivam contrato; mudanças adicionais de testes/validação só quando apontarem invariante relevante, não para ocultar falhas.

Trilhas aprovadas: cave→(-18,13)→(-12.5,10)→poço(-7,8), largura2–2.4u; ramo fonte(-24,4)→(-25,9)→(-24,11)→(-18,13), largura1.5–2u. Remover ramal vertical antigo até(-27.5,17), arredondar junções/cantos por vértices, preservar caminho central entre canteiros e ponte física. Craft troca retângulo12x3 por ramais de circulação em polígonos, sem deslocar estações. Garantir clareira sem copas diante da boca/retorno: posições dos71IDs preservadas quando possível, realocações necessárias documentadas por comparação de cena; não prometer62posiçõesfixas se o novo corredor as invalidar.

H01 correção após regen01 FAIL: TreeID43 não coube na bolsa preferida perto da nova clareira. Preservar primeiras160tentativas idênticas porID e só então buscar nas demais bolsas existentes oeste/sudoeste em ordem determinística, até160porbolsa. Nenhum ID removido, nenhuma redução de separação/bounds/clearance, nenhuma invasão da reserva da cave. A falha permanece em path_cave_pass/regen_01; nova regeneração deve comprovar capacidade71IDs e registrar deltas.

H01 correção visual após regen02: caminhos diagonais rejeitados pelo root por raster de1u; PaintPath em CreateMvpFarmScene.cs passa a VisualPathGrid exclusivo de0.25u, reusando RasterizePolygonCells e sprite sand com escala proporcional. Nenhuma alteração de WorldTilemapGround/global ou física. FarmDecorationPlanner.IsTreeCanopyClearOfCave também protege a projeção de copas sobre o segmento(-18,12.5)→(-11,9.8); manter71IDs, fallback e distâncias. Compor pequeno grupo de Meadow no sudoeste filtrado pelas máscaras/biome existentes, corrigindo focal11<12 sem reduzir intervalo. Preservar evidência regen02/Play01(8vistas,16rotas,5seleções PASS) e focal30/31 FAIL; nova captura e teste necessários antes de aceitar. Atualizar apenas comentários posicionais obsoletos do gerador. Testes focais do planner existentes cobrem nova reserva e contagem.

H04 acabamento autorizado pelo root após inspeção da referência e Play01: Board_Zrix ainda usa GetBuiltinSprite roxo, um marcador provisório visível ao lado da caverna. Gerar via GPT novo Assets/_Game/Art/Generated/World/props/notice_board_keyart_v1.png (+meta), raw/prompt/proveniência em art/farm-pixelart-review/. Root inspeciona transparência e estilo antes de liberar. Owner H01 integra somente visual em CreateZrixContractBoard, preservando IDs, trigger, conteúdo/interação e profile físico; filho visual uniformemente dimensionado e apoiado no root, altura contextual inicial1.6u. ImportPoint/None/noMip. Scope não cria sistema/quest nem requer executar interação do mural.
H04 integração antes de código: mural aprovado RGBA1254², alpha>128 bbox(167,186)-(1088,1068). Child Visual em World/order0, escala mundial uniforme para882px visíveis=1.6u, apoio em pixel(627.5,186) contado do canto inferior. Preservar root/profile/trigger/postings. Reserva de copas pode incluir círculo visual do mural em FarmDecorationPlanner.IsTreeCanopyClearOfCave; manter71IDs e separação, registrar realocações finais. Regenerar e capturar após wiring.

Fechamento H técnico: path_cave_pass/regen_04/gameplay_02, cena1c6311a582901d2f69a60a3d3e90e43940e95c3d18e8d1485ca3c7944f764844, exit0/scans sem erros,8vistas CustomAxis/Y,16rotas físicas(6455nós),5seleções reais e0runtimeErrors; cena/saves preservados. Planner03=5/5PASS,26casos restantes reutilizados por7inputs equivalentes (31válidos, não uma nova suíte31/31). Identidade71IDs/29posições alteradas vsG, sem drift sprite/escala/TreeData;15jardins semCollider/Node; mural byteExact/importPASS. Histórico Tree43/Meadow11/raster1u permanece documentado, corrigido pela evidência posterior. H01–H04 implementados; SCOPED_PASS técnico, spec IN_PROGRESS/promoçãoNO, revisão humana e input/animação/transição de cave não executados. Execution report contém provas e limites.
Root revisou overviewregen04 e8/8framesPlay02 e aprovou a adaptação para entrega: cave/clareira/mural e diagonal legíveis, raster fino, canteiros/portas/ponte preservados. Permanecem diferenças de regularidade das trilhas, densidade de clutter e campos vazios para cultivo real. Aceite humano pendente, sem promoção.

## Revisão I — cais sobre a água e acabamento das trilhas

Pedido humano vigente: proporções ainda sob revisão, cais/barco aparentam fora do lago e estrada precisa acabamento fino. Baseline H final1C6311A582901D2F69A60A3D3E90E43940E95C3D18E8D1485CA3C7944F764844. Esta continuação autorizada reconcilia o snapshot histórico CURRENT_STATE; não implica aceite humano final de G/H.

- [x] I01 — farm_keyart_rebuild possui CreateMvpFarmScene.cs somente cais/FishingSpot e contratos farm de footprint/navegação estritamente envolvidos. Medir sprite com alpha e ancorar gangway norte na margem NW do Lake, plataforma e barco dentro da água; NÃO rotacionar sprite90graus pois deitaria postes/perspectiva. Target inicial altura visível4–4.2u versus player1.1875u. Candidato apoio norte perto(10,-8), coordenada final definida pelas medidas antes de editar. Preservar IDs, arte existente, cave, recursos e save. Pequeno footprint caminhável exclusivamente deck/gangway pode recortar collider do lago para coerência visual/física; água e barco seguem inacessíveis. Reusar contratos e políticas existentes, sem sistema paralelo. FishingSpot junto a ponto acessível, sem ampliar alcance global. Testes existentes de navegação/footprint devem provar deck acessível e água adjacente bloqueada; o orquestrador confere sprite/apoio na captura.
- [x] I02 — farm_capture_implementation possui FarmDecorationPlanner.cs AuthoredPaths e FarmLandscapeVisualComposer.cs apenas acabamento de caminhos (demais métodos de margem d'água ficam com I01, coordenar antes de tocar arquivo compartilhado). Refinar junções T, transições de largura, cotovelos e ligação ao cais com polígonos autorados e bordas existentes, preservando visual grid.25u, cultivo e acessos. Plano concreto antes de editar; evitar novos contornos retangulares, não gerar raster por código. Preservar71TreeIDs e minimizar deslocamentos, documentando os necessários.
- [x] I03 — farm_delivery_validation exclusivo Unity após freeze conjunto. Backup H e evidência em docs/validation/playmode/farm_keyart_delivery_20260909/dock_path_pass/. Cena por Editor API; compile/testes focais dos contratos afetados, rotas incluindo acesso ao deck e água adjacente, seleção real FishingSpot se alterado. Atualizar probe Editor existente quando necessário, com ownership captureworker e critério explícito; nenhuma simulação que force candidatos ou oculte física. Reusar checks inalterados. Capturar8vistas e closeup lago com player no acesso/deck, registrar posição realmente verificada.
- [x] I04 — root compara keyart e capturas finais, devolve defeitos de escala/água/pés/ruas. Atualizar preview com baselineH e resultadoI, relatório com proporções efetivamente alteradas, evidências e limites. Declaração scoped; não encerrar visual global ou promover spec enquanto restar aceite humano/input/ações/anim.

Owners não estão sozinhos; preservar dirty anterior. Antes da execução, anexar coordenadas e arquivos adicionais estritamente necessários. Root responde que rodada de proporções G foi executada mas cais/encaixe e acabamento I ainda precisam correção, sem apresentar esse histórico como qualidade artística integral concluída.
I01 blueprint antes da edição: manter sprite upright, altura visível4.2u, ancorar pixel raw(550,205top)/(550,1058bottom) no mundo(10,-8). Deck físico x[9,10.95],y[-11.15,-9.3], pescoço x[9.55,10.45] até a margem NW original. Lake visual não muda; LakeCollisionPath recorta apenas esse acesso/deck. FishingSpot root(10,-10), preservando profile6 e mecanismos trigger/edge. _fishingSpotId vazio anterior resolvia farm_spot_7_-10 (RoundToInt6.7/-9.5, empate para par); serializar const com esse ID antes da mudança. Paths exatos: Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs (âncoras/ID), Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs (contorno físico cais), Assets/_Game/Scripts/Farm/Scene/FarmSceneNavigationRaster.cs (consulta mesmo contorno), Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (somente cais/pesca/colisorLake), Assets/_Game/Tests/EditMode/Farm/FarmSceneNavigationContractTests.cs (acesso livre e água adjacente/boat bloqueados). Chandra possui PaintPath/AuthoredPaths e gate seleção de pesca; não disputar esses hunks. Nenhum CaveScene/save schema/resourceID alterado.
I02 blueprint aprovado antes da edição: chanfros locais de 0.5–0.75u nas junções oeste/leste do cultivo e craft, mantendo corredor central1.5–2u e polígonos aráveis. Ramal da borda leste (6.2,-7.2) ao apoio canônico DockEntrance(10,-8), exclusivamente em terra até o gangway. CreatePathBanks passa a segmentar trechos expostos da união dos polígonos, cobrindo as pontas das junções sem criar bordas internas; espessura visual menor somente em caminhos. Preservar CreateBoundaryStrips/água e raster.25; areia existente mantida nesta rodada. Sem novos colliders/IDs; registrar eventual reposicionamento determinístico de árvores decorrente do mask compartilhado.
I03 blueprint Editor: Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs mantém16rotas, mas fishing_approach exige atingir centro canônico do deck com tolerância0.1u, em vez de aproximação1.25u; três consultas de ocupação com collider real em água oeste/leste/sul do deck devem ser bloqueadas e registradas separadamente. FarmInteractionSelectionProbe.cs acrescenta FishingSpot como sexto alvo com callbacks/CanInteract/GetCurrentInteractable reais, mantendo cinco anteriores, sem Interact. FarmPlayModeCaptureSession.cs posiciona somente vista lake no centro do deck, usando o mesmo teste de sobreposição e metadados reais; não altera câmera/player/save/runtime. Unity exclusivo validator após freeze.

Fechamento I técnico: dock_path_pass/regen_01/gameplay_01, cena751a9263324f2a322135e9900e969fc5b22f73955db31d629a592296ffacf803, exit0/scans sem erros críticos,32/32focais atuais PASS.8vistas/16rotas(6465nós)/6seleções/3águas adjacentes bloqueadas/0runtimeErrors, cena/saves preservados. Deck alcançado0.1u; seleção FishingSpot semInteract/peixe capturado.71árvores semdelta peranteH,15jardins preservados, IDlegado pesca serializado. Root inspecionou overview/água e8frames e aprovou encaixe/escala/pés/caminhos para entrega. Permanecem regularidade autorada/dirtuniforme vskeyart e aceitehumano/input/ações/animação pendentes. I01–I04 técnicos concluídos, spec IN_PROGRESS/promoçãoNO. Rodada de proporções executada; cais corrigido nesta I. Relatório de execução contém artefatos e limites.
