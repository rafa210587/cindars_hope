# Execução — revisão visual do fazendeiro

Data:2026-09-08. Spec: [spec_farmer_animation_visual_revision_v1](../../.specs/a_implementar/spec_farmer_animation_visual_revision_v1.md).
Status: **CODE_COMPLETE**, com **SCOPED_PASS** do laboratório. Arte: **aprovação humana PENDING**; promoção: **NO**. Nenhuma integração Unity autorizada/executada nesta etapa.

## Acceptance criteria extracted

Preservar562 fontes; entregar estudos frontais de caminhada/idle/espada/arco, revisar pesado; disponibilizar A/B/C, referência corporal72px e reprodução lenta; documentar cinco vereditos sem converter falhas artísticas em aprovação. A spec, com plano/tasks, foi salva antes da geração/implementação. Iteraçõesv4/v5 e apoio manual foram acrescentados ao contrato antes das respectivas edições.

## Existing systems audit

Reutilizados `art/farmer-animation-review/build_preview.py`, inventário281PNGs e dois estudos anteriores. Nenhum novo sistema Unity, pipeline de edição raster ou biblioteca instalado. Revisão visual dos originais:31poses frontais. Oito direções mantidas para reprodução; nova autoria frontal conforme plano de estabilizar referência antes de expandir ângulos.

## Spec Compliance Matrix

| Requisito | Implementação/evidência | Resultado |
|---|---|---|
| 14.1 fontes | Manifesto original pinado;562/562 hashes iguais | OK |
| 14.2 estudos | Quatro ações novas com8poses; caminhada em3tentativas; pesado anterior revisado;8variantes totais | OK |
| 14.3 timing | Pesos/crops validados; arco original0,25s para cooldown0,5s; estudos sem eventos de dano | OK |
| 14.4 apresentação | CUA cinco ações72px, pause/step/play/desaceleração; guia de apoio corrigida e reinspecionada | OK |
| 14.5 review | REVIEW_V2.md com cinco ações, defeitos, poses observadas e vereditos | OK |
| 14.6 falhas de input | Seis contratos negativos/escopo do verifier | OK |
| Aprovação artística | CaminhadaV5, idle e pesado em avaliação; espada/arco e walkV3/V4 reprovados | PENDING / FAIL artístico |
| Integração | Assets/cenas/runtime preservados | N/A nesta spec |

## Validation

Modo: **SCOPED**, somente artefatos/contratos do laboratório. Python bundled `C:/Users/Rafa/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe`; Pillow para leitura de métricas, sem edição raster. Geração das seis folhas por built-in imagegen; prompts e PNGs preservados localmente.

Comandos executados, todos exit0 na rodada final:

```powershell
& $reviewPython art/farmer-animation-review/build_preview.py
& $reviewPython art/farmer-animation-review/verify_review.py --self-test
& $reviewPython art/farmer-animation-review/verify_review.py --project-root . --check all --output art/farmer-animation-review/review-verification.json
```

`$reviewPython` corresponde ao caminho bundled acima. Sintaxe Python/JavaScript também conferida. Saída final:

```text
SOURCES=562 UNCHANGED=562
NEW_FRONT_STUDIES=4 FRAMES_EACH=8 ACTIONS=5
TIMING_CONTRACTS=PASS
PRESENTATION_CONTRACTS=PASS
ACTION_REVIEWS=5 HUMAN_ART_APPROVAL=PENDING
SCOPED_PASS: farmer-animation-review artifacts/contracts only
CONTRACT_TESTS=6/6
```

Self-tests: SourceHashMismatch, CropOutOfBounds, InvalidWeight, UnsafeSourcePath, MissingActionReview e ScopedPassPreservesArtFail. O último assegura que sucesso técnico não apaga arte FAIL. A rodada intermediária falhou legitimamente enquanto o bloco de revisão ainda não existia; somente após preenchê-lo com a inspeção real houve PASS.

Evidência local: `art/farmer-animation-review/review-verification.json`, com hashes dos inputs. Root conferiu status/exit/checks e comparou os hashes novamente: **0 diferenças**. Digest das562 fontes: `e3be93faf2bb6d6541e0ee110d003a986cc11037f7cf75b045de211ccf9c3f37`.

CUA: preview em `http://127.0.0.1:8766/preview.html`, tab2. Inspecionados walkV5 apoios1/5 e passagem3, idle2/7, espada7, arco5 e heavy5, além das folhas completas. Confirmados controles, tempos e corpo72pixelsCSS estimados. Após correção da guia manual, walk e heavy reinspecionados; console retornou0erros. Isso não é aprovação humana nem teste Unity.

Unity validation: NOT RUN
Reason: somente laboratório externo e estudos de arte; nenhuma alteração de Assets/runtime.
Command attempted: nenhum, gate inaplicável ao escopo.
Residual risk: integração, escala física, colisão, interrupções e sincronização de ataques não foram validadas no jogo.

Falhas globais preexistentes descritas em CURRENT_STATE permanecem fora deste escopo; nenhum GLOBAL_PASS foi declarado. Sem commits/push.

## Honest status rationale

O trabalho da spec foi executado como **estudo revisável**, incluindo rejeições artísticas. A caminhadaV5 aproxima as poses de passagem, mas não foi aprovada como natural: transferência e braços ainda são contidos. Idle torna a piscada explícita, mas respiração/identidade precisam de revisão. Pesado ganhou tempo de antecipação/retorno. Espada/arco novos têm defeitos concretos e não devem substituir originais.

Closeout `/finish-spec`: implementação do laboratório CODE_COMPLETE, evidência scoped vigente, promoção NO porque aceitação artística humana não ocorreu. Tasks executadas não significam sprites finais. Próximo passo humano: comparar as alternativas na prévia e escolher o que merece uma etapa de produção; expansão de direções e integração continuam posteriores.

## Arquivos e acesso

- [Prévia local](../../art/farmer-animation-review/preview.html), [avaliação detalhada](../../art/farmer-animation-review/REVIEW_V2.md), [evidência](../../art/farmer-animation-review/review-verification.json).
- Builder/verifier e saídas HTML/JSON; manifesto8variantes; PNG/JSON/prompt de walkV3/V4/V5, idleV1, espadaV1 e arcoV1; JSON do pesado revisado.
- `art/` é ignorado pelo Git. Artefatos existem nesta máquina; spec/report não tornam esses arquivos disponíveis em um clone. `.gitignore` não foi alterado, e não foi feito add forçado.

## Continuação — frames e resolução, 2026-09-08

Apêndice da spec registrado antes da execução. Gerado estudo frontal de12poses, com PNG/prompt preservados e crops observados. Manifesto agora contém9variantes. Builder inclui comparação V5/8poses versus12poses com fase sincronizada e ciclo comum de0,8s:10 e15FPS, respectivamente. Faixa adicional mostra a mesma pose em48/72/96/144pixelsCSS estimados. Esses tamanhos não são exportações de nova arte nativa.

CUA: fases50%,23,6% e29% examinadas, alturas72/96px comparadas, quatro tamanhos visíveis e íntegros, playback0,5× acionado; console sem erros. A nova pose4 aproxima os pés de uma parada;4/10, braços contidos e mudança de rosto continuam pontos de atenção. Recomendação: continuar estudo12poses na caminhada; comparar96px com referência72px; corrigir poses/timing de idle e ataques antes de aumentar indiscriminadamente a contagem. Detalhes e custos teóricos em [Frames e resolução](../../art/farmer-animation-review/FRAME_RESOLUTION_REVIEW.md).

Após consolidar a inspeção, executado novamente o comando `verify_review.py --project-root . --check all --output art/farmer-animation-review/review-verification.json`: exit0, SCOPED_PASS; `SOURCES=562 UNCHANGED=562`; `FRAME_COMPARISON=8/12 CYCLE=0.8 RESOLUTIONS=48/72/96/144`. Self-tests6/6 da rodada anterior reutilizados; não são testes de naturalidade. Evidência inclui hash do novo relatório. Unity continua NOT RUN pelo mesmo escopo externo. Arte12f REVIEW_PENDING, aprovação humana PENDING e promoção NO. Nenhum original substituído.
