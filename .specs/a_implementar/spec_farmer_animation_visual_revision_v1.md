# SPEC — Revisão visual das animações do fazendeiro

> **Spec ID:** `spec_farmer_animation_visual_revision_v1`
> **Status:** CODE_COMPLETE — laboratório SCOPED_PASS; aprovação artística humana PENDING
> **Wave:** FARMER_REVIEW — estudos anteriores à integração
> **Priority:** P2 — validar movimento e identidade antes de substituir arte
> **Type:** Tooling / Validation
> **Domain:** Player / Combat / Art
> **Parallelizable:** CONDITIONAL
> **Parallel group:** FARMER_VISUAL_REVIEW
> **Can run with:** geração de estudos independentes, com ownership por PNG/JSON/prompt
> **Must not run with:** outro editor do builder, manifestos ou relatórios desta entrega
> **Repo lock scope:** `art/farmer-animation-review/`; esta spec; seu execution report
> **Depends on:** `art/farmer-animation-review/ANIMATION_REVIEW.md` e laboratório existente
> **Blocks:** somente uma futura decisão humana sobre produção/integração da arte
> **Scope:** revisar as cinco ações existentes e demonstrar novos estudos frontais no comparador
> **Out of scope:** Assets, import Unity, gameplay, dano, cooldown real, save, animações/direções adicionais
> **Validation level alvo:** CODE_COMPLETE + SCOPED_PASS de artefatos/contratos; aprovação artística humana pendente
> **Executor:** Codex ou Claude; geração pelos recursos imagegen autorizados
> **Autorização:** humano nesta sessão: “certo execute, faça uma spec antes, com plan e tasks e execute.” Autoriza executar este laboratório; não autoriza integrar sprites ao jogo.
> **required_adrs:** []
> **required_rules:** []

## 5. Contexto
O laboratório já reproduz os originais e dois estudos gerados. A próxima entrega amplia a avaliação para todas as ações e torna comparável sua leitura no tamanho do personagem. É uma revisão visual, sem exigência de aceitar ou importar os resultados.

## 6. Problema
Os ciclos originais de caminhada variam de 0,6 a 1 s; idle frontal possui 76 arquivos/10 imagens distintas. Estudos anteriores têm inconsistências de desenho, recortes estimados e fundos opacos. Funcionamento do visualizador não prova naturalidade, identidade consistente ou arte pronta.

## 7. Objetivo
Entregar quatro novos estudos frontais de oito poses, reavaliar o martelo existente e documentar as cinco ações com evidência de reprodução A/B/C, tamanho corporal de referência de 72 px e desaceleração. Defeitos observados devem produzir veredito artístico FAIL, mesmo quando os contratos do laboratório passam.

## 8. Fontes obrigatórias lidas
- `AGENTS.md`; `docs/project/CURRENT_STATE.md`; `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
- `.agents/skills/spec-authoring/SKILL.md`; `.specs/_templates/SPEC_DEEP_TEMPLATE.md`; exemplo profundo indicado pela skill.
- `.claude/rules/spec_quality_gate.md`; `art/farmer-animation-review/ANIMATION_REVIEW.md`.
- `art/farmer-animation-review/build_preview.py`, `inventory.json`, `candidate.json`, `heavy-candidate.json`.
- `Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs`: somente referência de seleção/timing; geração raster segue skill `imagegen`, sem Python para editar imagens.

## 9. Estado atual do repo — Phase 0 auditada em 2026-09-08
Comandos executados: `Get-ChildItem art/farmer-animation-review -File`; leitura do relatório/builder; Python bundled com `Path.rglob('*.png')`, `Counter` e comparação SHA-256 de cada entrada de `inventory.json.source_guard.files` contra o arquivo real.
Resultado: **281 PNGs, 562 fontes PNG/meta, 0 diferenças**; walk58, idle121, sword32, heavy32, bow38. Manifesto original: `e3be93faf2bb6d6541e0ee110d003a986cc11037f7cf75b045de211ccf9c3f37`.
Existem: `candidate_data(filename)` linha89, `build()` linha151, `DATA.candidates`, recortes explícitos, pesos/timing, controles sincronizados e preview HTML. Walk v1: quatro sequências de oito poses; heavy v1: oito poses, 0,6 s, recortes com altura420. Ausentes nesta auditoria: `verify_review.py`, `REVIEW_V2.md`, `candidates-v2.json` e os quatro estudos novos.
Os artefatos de `art/` existem localmente e aparentemente são ignorados pelo Git; não mudar `.gitignore` nem forçar inclusão. Spec/report versionáveis devem informar essa disponibilidade local.
A fase0 de execução reconfirma o mesmo manifesto antes de gerar; resultados válidos já conferidos podem ser reutilizados mediante inspeção, sem repetir builds/Unity.

## 13. Regras de não duplicação
Estender `build_preview.py`, `candidate_data`, `DATA.candidates` e os contratos de crops/pesos existentes. Não criar outro viewer, pipeline raster, manager, interface C#, teste Unity ou registry de gameplay. Manter os estudos v1 acessíveis; não sobrescrever PNGs anteriores nem tratar caixas alpha como posição dos pés quando armas/fundos interferem.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR
CRIAR: `candidates-v2.json` como lista de variantes; quatro PNGs/JSONs/prompts frontais; `verify_review.py` para integridade/contratos; `REVIEW_V2.md` e execution report.
MODIFICAR: `build_preview.py` carrega o manifesto e oferece referência corporal72px; `preview.html`, `preview-data.json`, `inventory.json` são saídas do mesmo builder. `heavy-candidate.json` pode receber timing/ancoragem de display revisados, com valores anteriores registrados no review.
N/A classes runtime: esta entrega é um artefato externo. Pattern: extensão do builder existente e configuração por dados; autoria de imagem via skill `imagegen`, revisão/closeout pela matriz canônica.

## 16. Contratos, dados e eventos
### 16.1 Manifesto e estudos
`candidates-v2.json`: objeto `{schema_version:1, source_manifest_sha256:string, studies:[{id:string,label:string,action:string,direction:string,config:string}]}`. IDs únicos; `config` relativo ao diretório do manifesto. Pelo menos seis variantes: walk/heavy anteriores e os quatro estudos novos. Ações canônicas: `walk`, `idle`, `attack_sword`, `attack_heavy`, `bow`.
JSON individual reutiliza `{source,cols,rows,rowlabels,crops?,frame_labels,duration_seconds,frame_weights}`. Novos estudos: oito frames frontais cada. `crops[row][frame]=[x,y,w,h]`, inteiros positivos e contidos no PNG; pesos finitos/positivos, um por frame, normalizados para a duração total. Fontes de estudo ficam dentro do laboratório.
Adicionar `body_height_px` positivo por sequência e `body_height_note` com landmarks/estimativa para exibição72px; não usar altura da arma/canvas como altura corporal. `art_status` admite `FAIL`, `REVIEW_PENDING`, `HUMAN_APPROVED`; o último exige referência de aprovação humana real. Fundo neutro opaco é permitido no estudo e deve permanecer explicitamente identificado.

`ground_y_px` opcional, número, lista por linha ou matriz por frame, registra apoio manual em coordenadas locais do crop. Deve ser finito e estar dentro da altura do crop. Com esse dado, `renderCandidate()` posiciona `y=base-ground_y_px*scale` e rotula apoio estimado; sem inferir pés pelo alpha. Campo adicionado após CUA mostrar guia distante dos pés no modo72px.
### 16.2 Interfaces do tooling
Preservar `candidate_data(filename)`; `build()` lê `candidates-v2.json`, mantém v1 e incorpora bytes PNG intactos. `verify_review.py --project-root PATH --check all|sources|manifest|timing|presentation|review [--output PATH]`; default `all`, saída opcional `review-verification.json` no laboratório. `--self-test` executa seis contratos pequenos com fixtures em memória/temporárias, sem Unity ou navegador externo.
Exit0 somente com checks selecionados satisfeitos; imprimir `SCOPED_PASS: farmer-animation-review artifacts/contracts only`. Exit1 imprime `SCOPED_FAIL:` com causas; argumento inválido exit2. Nunca imprimir GLOBAL_PASS, nem converter veredito artístico FAIL em aprovação. A saída JSON registra checks, inputs/hashes, exit e `human_art_approval: PENDING` enquanto não existir aprovação.
### 16.3 Eventos e 16.4 Save
N/A: nenhum evento, dano, projétil, cooldown, serialização ou dado de save é alterado. “Contato” e “Soltura” são rótulos visuais.
### 16.5 UI
A usa pixels/timing originais; B usa os mesmos pixels com timing experimental; C seleciona variante gerada. Preservar 5ações/8direções/fallback, pause/step/scrub/onion/guias, play global junto do candidato e link de análise. Modo “Corpo72px” usa escala `72/body_height_px` em todos os painéis, explicitando landmarks estimados; CSS não deve reescalar esse modo inadvertidamente. Oferecer 1×/0,5×/0,25× de velocidade e informar que redução/normalização são apenas display.

## 17. Sistemas afetados
Laboratório local de arte, análise de sprites e documentação. Nenhum sistema do jogo é implementado ou validado nesta etapa.

## 18. Arquivos permitidos
Todos os nomes abaixo de arte são relativos a `art/farmer-animation-review/`:
- `build_preview.py`, `verify_review.py`, `preview.html`, `preview-data.json`, `inventory.json`, `review-verification.json`.
- `candidates-v2.json`, `heavy-candidate.json`; os JSONs/PNGs históricos restantes são referências preservadas.
- `walk-front-v3.png`, `walk-front-v3.json`, `walk-front-v3-prompt.txt`.
- `walk-front-v4.png`, `walk-front-v4.json`, `walk-front-v4-prompt.txt`: iteração direcionada após v3 apresentar passagem de pernas insuficiente; preservar v3 como comparação.
- `walk-front-v5.png`, `walk-front-v5.json`, `walk-front-v5-prompt.txt`: correção localizada das duas poses de passagem e reordenação explícita de exibição após inspeção de v4.
- `idle-front-v1.png`, `idle-front-v1.json`, `idle-front-v1-prompt.txt`.
- `sword-front-v1.png`, `sword-front-v1.json`, `sword-front-v1-prompt.txt`.
- `bow-front-v1.png`, `bow-front-v1.json`, `bow-front-v1-prompt.txt`.
- `REVIEW_V2.md`; evidências de inspeção em `evidence-v2/` se necessárias.
- `.specs/a_implementar/spec_farmer_animation_visual_revision_v1.md`.
- `docs/validation/spec_farmer_animation_visual_revision_v1_execution_report.md`.
Novas tentativas preservam as rejeitadas; ampliar nomes desta lista na spec antes de escrever variantes adicionais. Execução concorrente exige ownership explícito por conjunto.

## 19. Arquivos proibidos
`Assets/**` incluindo PNG/meta, `.unity`, `.prefab`, `.asset`; `Packages/**`; `ProjectSettings/**`; `tools/**`; `.claude/**`; `.codex/**`; `.gitignore`; `docs_old/**`; logs/status globais e documentos não listados. Não instalar libs, criar commits/push, forçar Git add ou manipular raster por Python.

## 20. Estratégia de implementação — plano e tasks por edição
### Fase0 — contratos e referências
- [x] Auditar281 PNGs/562 hashes, ler preview/relatório e salvar esta spec antes de implementar.
- [x] Confirmar manifesto original pinado e dividir ownership entre autor dos estudos, editor do builder e revisor.
- [x] Em `candidates-v2.json`, registrar v1 e quatro variantes novas; reaproveitar schema individual e declarar altura corporal/estimativa.
### Fase1 — estudos visuais, sem integração
- [x] Gerar `walk-front-v3.png` com oito poses: contato esquerdo, apoio/descida, passagem, subida/recuperação, contato direito e fases espelhadas correspondentes; inspecionar alternância e fechamento real.
- [x] Gerar idle frontal8poses com respiração contida, espada frontal8poses e arco frontal8poses; preservar identidade/roupa/proporções de referência. Guardar prompts e bytes retornados.
- [x] Em cada JSON novo, definir crops observados, fases e pesos; nunca rotular uma pose “Contato” para fingir que a imagem o demonstra.
- [x] Reavaliar os oito frames do heavy existente: trajetória, guarda final/inicial, antecipação e retorno; ajustar somente JSON/display/timing quando sustentado pela inspeção.
### Fase2 — comparador e verificação
- [x] Em `build()`, substituir lista fixa de variantes pela leitura do manifesto; em `candidate_data()`, validar novos campos preservando crops/timing/PNG base64.
- [x] Em `drawFrame()`/`renderCandidate()`, aplicar modo corpo72px e manter transformação de display explícita; conservar desaceleração, alternância A/B/C e todos os originais.
- [x] Criar `verify_review.py`: validar fontes contra digest pinado, manifesto/paths/crops/pesos, byte parity incorporada, controles e cobertura do review; implementá-lo sem afirmar julgamento artístico.
### Fase3 — avaliação e entrega
- [x] Disponibilizar todas as direções existentes no viewer e inspecionar visualmente as cinco ações frontais; comparar os estudos frontais em72px, 1× e desacelerado. Registrar observações concretas e defeitos, não apenas métricas de delta. Revisão artística detalhada das outras direções fica após escolha da referência frontal, conforme sequência acordada com o humano.
- [x] Escrever `REVIEW_V2.md`: Proveniência, Cinco ações, Estudos/crops/timing, Inspeção72px, Vereditos, Limites e Próximas decisões humanas. Cada ação registra originais/direções, variante, frames examinados, observação, defeitos e art_status.
- [x] Executar contratos/gates pertinentes uma vez após os edits; registrar comando/stdout/exit no execution report com as cinco seções canônicas da matriz.

## 21. Ordem segura de execução
Spec salva → manifesto/ownership → geração por estudo → crops/metadados → builder → inspeção visual → review → verificação → report. Geração e implementação do viewer podem ocorrer em paralelo após os contratos; integrar manifestos somente com arquivos existentes. Não regenerar Assets nem promover aceitação humana.

## 14. Critérios de aceite — binários e DoD
Definir em PowerShell `$reviewPython = 'C:\Users\Rafa\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'`. Comando base: `& $reviewPython art/farmer-animation-review/verify_review.py --project-root .`.
| Critério | Resultado obrigatório | Comando adicional ao comando base / saída literal |
|---|---|---|
| 14.1 Fontes preservadas | 281 PNGs/562 fontes, zero diferenças do digest pinado | `--check sources` → `SOURCES=562 UNCHANGED=562` |
| 14.2 Estudos disponíveis | v1 preservados + quatro estudos novos de8frames, cinco ações cobertas | `--check manifest` → `NEW_FRONT_STUDIES=4 FRAMES_EACH=8 ACTIONS=5` |
| 14.3 Timing íntegro | pesos/crops válidos, soma da duração; arco original0,5→0,25; labels não implicam eventos | `--check timing` → `TIMING_CONTRACTS=PASS` |
| 14.4 Comparação revisável | A/B/C, corpo72px e desaceleração presentes; inspeção CUA registrada separadamente | `--check presentation` → `PRESENTATION_CONTRACTS=PASS` |
| 14.5 Cinco ações avaliadas | review contém observações/defeitos/veredito por ação; aprovação humana pendente explícita | `--check review` → `ACTION_REVIEWS=5 HUMAN_ART_APPROVAL=PENDING` |
| 14.6 Falhas detectadas | fixtures: SourceHashMismatch, CropOutOfBounds, InvalidWeight, UnsafeSourcePath, MissingActionReview, ScopedPassPreservesArtFail | `--self-test` → `CONTRACT_TESTS=6/6` |
O resultado14.5 atesta presença/consistência da evidência, não verdade automática de naturalidade. Arte FAIL com causa documentada é entrega válida de estudo; critério faltante não é PASS.

## 23. Edge cases / falhas
Fonte original mudou: interromper geração do baseline/PASS e registrar divergência; não recalibrar digest. Geração opaca/grade irregular: preservar PNG e informar limite; crops só para display. Pose ambígua, anatomia inconsistente ou pé sem contato: art_status FAIL e frames apontados. Crop corta arma: corrigir retângulo observado, sem redesenhar raster. Frame/peso/arquivo ausente ou vazio: verifier FAIL, sem preencher ficticiamente. Aprovação humana ausente: PENDING; nenhum pedido de integração é necessário para concluir os estudos. Falha da ferramenta de geração: registrar tentativa e deixar critério correspondente pendente, sem sintetizar sprites por código.

## 22. Validação e gates
Sintaxe Python/JS, checks específicos14.1–14.6 e inspeção via CUA pelo revisor são aplicáveis. A execução `--check all --output art/farmer-animation-review/review-verification.json` só informa SCOPED_PASS dos artefatos/contratos. Revisar diff/links da spec/report; não rodar Unity/.NET/docs global redundante para esta arte externa. Manter visíveis as falhas globais preexistentes de CURRENT_STATE, sem afirmar GLOBAL_PASS. Status máximo desta entrega sem aprovação humana: CODE_COMPLETE com evidência scoped e revisão artística pendente; não mover a spec a implementados automaticamente.

## Closeout 2026-09-08
Plano/tasks executados como estudos; nenhuma aceitação artística implícita. Promoção NO. Evidência: [execution report](../../docs/validation/spec_farmer_animation_visual_revision_v1_execution_report.md). Vereditos FAIL preservados; Assets e Unity não alterados.

## Continuação autorizada — quantidade de frames e resolução

Pedido humano: “avalia se n vale aumentar a qtidade de frames pixels, etc. Pode executar.”
Escopo adicional: estudo frontal12poses comparado à caminhadaV5 de8poses, sempre com ciclo0,8s; painel de leitura simultânea48/72/96/144pixelsCSS. Aumento de tamanho de display não significa detalhe novo, e uma geração diferente não isola causalmente apenas a contagem de frames. Não integrar no Unity.

Arquivos adicionais permitidos no laboratório: `walk-front-12f-v1.png`, `walk-front-12f-v1.json`, `walk-front-12f-v1-prompt.txt`, `FRAME_RESOLUTION_REVIEW.md`. Reutilizar manifesto/builder/verifier/saídas existentes; atualizar este report e esta spec.

Contratos: novo estudo usa12crops observados,12pesos positivos, body_height_px e ground_y_px. Em `renderCandidate()`, acrescentar painel independente que reproduz V5 e12f sincronizados por fase normalizada de0a1; fases iguais não garantem que desenhos correspondam. Duração comum configurável, padrão0,8s:8frames=10FPS,12frames=15FPS. Não interpolar/misturar pixels nem repetir frames e alegar novos desenhos. Strip de resolução usa mesma pose em48/72/96/144px estimados, nearest-neighbor no canvas, CSS1:1 e apoio explícito; mantém fontes intactas.

- [x] Gerar/inspecionar12poses e registrar defeitos honestos; preservar todos os estudos anteriores.
- [x] Exibir comparação8/12 com duração comum e quatro resoluções; manter todos os controles anteriores.
- [x] Inspecionar CUA e documentar custo relativo, limites e recomendação por ação em FRAME_RESOLUTION_REVIEW.md.
- [x] Executar checks afetados uma vez; confirmar562fontes intactas; atualizar report/status sem aprovação humana implícita.

DoD adicional: verifier `--check presentation` verifica controles/configs e12crops/pesos válidos, sem julgar fluidez. CUA comprova painel legível e playback; relatório distingue densidade visual, duração do ciclo e quantidade de poses. Custo teórico de pixels:12/8=1,5×;96/72 ao quadrado=1,78×; juntos2,67×, sob mesmas margens/formatos e sem confundir com medição real de VRAM.

