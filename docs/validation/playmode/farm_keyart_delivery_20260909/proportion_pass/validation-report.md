# G04 — validação de proporções, ponte e perímetro

Resultado técnico: **SCOPED_PASS**. Cena final `Assets/_Game/Scenes/FarmScene.unity`, SHA256 `EB848C096E5E13CD015E61516EE0E7E11374708CBB22B37CF22CB9515F723D16`. A revisão visual final do root é registrada separadamente; aceite humano e execução por input não são inferidos.

## Critérios e evidência vigente

| Gate | Resultado | Evidência |
|---|---|---|
| Compile Editor e geração via Unity API | PASS | regen_02/process.json, runner-result.json e unity.log; exit 0/scan 0; dez PNGs e snapshot |
| Planner e Composition afetados | PASS 9/9 | focal-tests_01.xml/log: Planner 5 + Composition 4; runner exit 0 e scan incorporado PASS |
| Reuso após ajuste visual da borda | PASS de equivalência | focal-reuse-review.json: quatro inputs iguais |
| TreeNodeVisualTests | Reuso 3/3 PASS | final-audit.json: fixture/runtime iguais aos hashes de continuation_art_01/focal-tests_01-inputs.json; esses três casos passaram no XML daquela rodada, apesar do aggregate histórico FAIL do Planner |
| Play MainCamera | PASS, oito vistas | gameplay_02/capture-metadata.json, oito PNGs, process/log/scan exit 0 |
| Sorting real | PASS 8/8 | CustomAxis e eixo (0,1,0) nas oito vistas; adapter real, teste não aplica modo |
| Rotas físicas | PASS 16/16 | 6559 nós visitados, collider real e paths; parâmetros registrados em physicalRoutes |
| Seleção real de interação | PASS 4/4 | Porta, workbench, forge e cooking; callbacks reais e GetCurrentInteractable; sem Interact ou injeção |
| Identidade e recursos | PASS | scene-identity-review.json: mesmos 71 IDs, apenas seis posições 62..67 alteradas; mesmos sprites, escalas e dados |
| Perímetro externo | PASS estrutural | 203 componentes Transform/SpriteRenderer, zero Collider/TreeNode; quatro bloqueios de limite preservados |
| Ponte nova | PASS import/raw | bridge-import-review.json: RGBA 2172×724, alpha 0..255, raw==asset SHA, Point/None/noMip/128 PPU, DefaultTexturePlatform 4096 |
| Arte anterior | PASS de integridade | reused-art-inputs.json: 149 PNGs e 149 metas byte-idênticos à baseline |
| Cena, fonte e saves | PASS | final-audit.json: hash atual==metadata antes/depois==snapshot; sourceDrift 0; conjuntos de hashes dos saves iguais; zero runtimeErrors |

Unity 6000.5.7f1 usado exclusivamente pelo validator. Nenhum Unity permaneceu aberto. Invoke-FarmValidation.ps1 registra comandos, timestamps e sourceInputs e executa um único scan por log. O Test Runner existente executou o filtro `CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests;CindarsHope.Tests.EditMode.Farm.FarmSceneCompositionContractTests`, com outputs focal-tests_01.xml e focal-tests_01.log. Compile-only, .NET e suíte global não foram repetidos, conforme matriz e escopo.

## Baseline e deltas

Baseline closing_pass/regen_04 e gameplay_04 preservada em before_diagnostic e before_gameplay. Backup de 366 arquivos e 149 PNGs verificado em baseline-inputs.json. Hash anterior: `8E68982B60724243FBEF52823CF6B1C9343797788C4237B03166B9C3F940AB6C`. Nenhum asset antigo foi substituído; a ponte nova é aditiva.

baseline-scale-inventory.json e after-scale-inventory.json usam o mesmo método read-only: cena serializada, PNG/meta, transforms hierárquicos e alpha >=0,05. Inventário final: 27 sprites simples e 28 BoxCollider2D; sprites tiled excluídos. Valores são estimativas da dimensão projetada na cena, não medidas de uma porta desenhada ou altura física 3D. Player baseline/runtime: 1,1875u, mantido.

| Objeto | Alturas do player antes | Depois |
|---|---:|---:|
| Craft | ~1,70 | ~1,21 |
| Caixote | 1,62 | 1,01 |
| Poço | 2,36 | 1,85 |
| Fonte | 3,43 | 2,86 |

Ponte opaca: 6,398×3,598u → 5,606×1,401u. Casa preservada; coop, barn, estufa e processamento tiveram reduções moderadas. Root decide a adequação visual desses ratios no contexto.

Os sólidos de craft preservaram as dimensões mundiais anteriores e tiveram o centro ajustado à base visual, com triggers novos. Por isso novas rotas e seleções foram necessárias. A casa mantém trigger 1,6×2,22 e blocker 1×0,22; seleção novamente em (16;4,5). Limites internos x±32/y±22 permanecem. A redistribuição de seis árvores mudou posições sem aumentar recursos; plantas externas são visuais.

regen_01/gameplay_01 passaram tecnicamente e tiveram escala/ponte revisadas pelo root. A revisão pediu retirar mini-cliffs repetidos da borda; tentativa preservada. regen_02/gameplay_02 representam essa correção. Inventários 01→02 são iguais após ordenar por path; a remoção dos sprites de perímetro não alterou objetos medidos. Não houve falha técnica nova nesta rodada. Falhas históricas de porta/sorting continuam em closing_pass.

## Limites explícitos

- Resolução da textura da ponte efetivamente exposta pelo runtime: **NOT QUERIED**. A coleta atual mede Player/ShippingBin. DefaultTexturePlatform 4096 foi verificado e o gerador aplica maxTextureSize 4096 + SaveAndReimport. O campo legado raiz 2048 do meta e plataformas 2048 sem override estão registrados, sem confundi-los com override ativo. Root aceitou esta evidência de import sem tooling adicional.
- Fotos, rotas e seleção não comprovam input humano, ações de crafting, abertura das portas, roof reveal durante caminhada ou animação em movimento. O poço continua decorativo.
- SaveInput foi desativado em memória antes do Play; nenhum save/load foi chamado. Hashes cobrem o diretório saves, sem alteração. Não é teste de round-trip de save.
- Rotas específicas não demonstram todos os deslocamentos possíveis; o perímetro foi conferido estruturalmente e é objeto da inspeção visual do root. Build Player/IL2CPP e humano: NOT RUN, pois não são gates desta revisão de cena.
- Docs consolidados são responsabilidade do closeout; este relatório não declara GLOBAL_PASS documental nem promoção da spec.
