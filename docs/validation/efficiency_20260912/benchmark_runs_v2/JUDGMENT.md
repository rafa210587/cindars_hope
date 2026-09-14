# Julgamento independente — DECISION_ONLY v2

Rubrica aplicada: `benchmark_v2/rubric.json` (binária, sem crédito parcial), sobre as primeiras respostas arquivadas em `g1`–`g4`. Julgamento somente de decisões; nenhuma implementação ou execução Unity foi exigida ou inferida. Equivalência semântica foi aceita. O estado `NOT RUN` mencionado nas respostas refere-se à evidência dos cenários, não à disponibilidade das respostas.

| Caso | baseline | candidate | Evidência decisiva |
|---|---|---|---|
| B01 | PASS | PASS | Corrigem estimativa, limitam escopo documental, não confundem links com custo nem inventam diff. |
| B02 | PASS | PASS | Regressão não confirmada; distinguem captura/restauração e condicionam mudança a reprodução, EditMode e revisão. |
| B03 | PASS | **FAIL** | Baseline testa “oculto não descoberto, oculto descoberto”. Candidate propõe fronteira pura que recebe só “festivais e `GameDate`” e pede apenas “adicionar caso de festival oculto”; não requer testar ambos os estados nem explicita entrada de descobertas para a seleção. |
| B04 | PASS | PASS | Rejeitam `Transform`, apontam ausência de autoridade e condicionam alternativa simples à compatibilidade de saves. |
| B05 | PASS | PASS | Rejeitam duplicata, preservam guarda/instalação explícita e requerem diagnóstico de bind, lifecycle e smoke. |
| B06 | PASS | PASS | `.anim` não prova import do PNG; import e aceite visual ficam pendentes, com revisão humana separada. |
| B07 | PASS | PASS | Mantêm `SCOPED_PASS` técnico separado de arte pendente e teste físico sem resultado. |
| B08 | PASS | PASS | Cenário condicional é FAIL; warning não apaga asserção e XML ausente não sustenta resultado de teste específico. |
| B09 | PASS | PASS | Não reutilizam evidência após diff do validador; exigem equivalência e validação nova sem promover escopo. |
| B10 | PASS | PASS | Preservam dirty alheio, inspecionam alvo antes/depois e param se autoria do hunk não puder ser separada. |
| B11 | PASS | PASS | Propõem busca reproduzível e separam declaração, publicação e consumo reais de comentário exemplar. |
| B12 | PASS | PASS | Falta contrato/spec de magia; `Install` não implica aprendizado, e bus, unsubscribe, save e testes são condicionados. |

Resultado: baseline **12/12 PASS**; candidate **11/12 PASS, 1/12 FAIL**. Não é ranking de modelos, custo ou qualidade por comprimento. Nenhum participante foi alterado; este julgamento não valida runtime, arte ou testes do projeto.
