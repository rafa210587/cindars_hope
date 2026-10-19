# Benchmark pequeno de roteamento e eficiência — 2026-09-12

## Estado e limites

Este conjunto contém 12 exercícios controlados com caminhos reais do repositório. Não são pedidos de alteração, resultados históricos nem execução de produção. Todos os registros em [tasks.jsonl](tasks.jsonl) começam como NOT RUN; não há economia, resultado ou telemetria alegados.

fixture_exercise só pode virar real_production numa rodada futura com autorização, diff e evidência anexados. Nunca agregar exercício e produção. Não rodar modelos, Unity, build ou mutação de runtime para preencher esta versão.

## Protocolo pareado

Baseline e candidato recebem o mesmo enunciado, snapshot/revisão, fontes, estado sujo declarado, autorização e rubrica; um braço não vê o resultado do outro. Alternar a ordem baseline/candidato por pares. Baseline usa o modelo configurado sem seleção explícita. Candidato usa a hipótese de .claude/rules/task-model-routing.md somente se o ID estiver disponível; senão registrar inherited_or_unknown, sem alegar mudança. Portanto, esta comparação testa a política completa “configuração atual versus roteamento candidato”, e não atribui causalidade isolada a um nome de modelo.

Escalar apenas por violação de contrato, dificuldade não resolvida ou repetição da mesma falha após uma correção focada. Infraestrutura indisponível é triagem, não escalada. Registrar por braço: bytes dos inputs fornecidos, bytes/linhas de saída, tempo de parede, tentativas e evidência. token_usage, cache_usage e cost só recebem telemetria nativa; tamanho de arquivo não é proxy. Experimentos específicos de formato de saída ou recuperação de símbolos usam modelo fixo nos dois braços: somente uma variável pode mudar por comparação.

## Amostra

B01–B10 são desenvolvimento/calibração. Podem esclarecer enunciado e rubrica, mas não mudar a hipótese de roteamento após observar resultados. B11–B12 são held-out: a rubrica fica congelada e os resultados reservados até congelar protocolo e coletor. IDs no mesmo arquivo não constituem blind real; o executor deve manter os resultados held-out fora da análise até esse ponto. Os 12 cobrem docs, bug delimitado, refactor puro, save, wiring, visual estático, animação, falha de validação, reuso de evidência, workspace sujo, símbolo e cross-system.

A decisão esperada em cada linha é uma decisão segura e revisável, não previsão de sucesso. Visual/animação requerem revisão humana quando houver mudança; texto ou compile não a substituem. Save preserva IDs e DTOs simples; workspace sujo preserva alterações de outros owners.

## Aceitação

Uma execução entra na comparação somente se:

- respeita os caminhos/autorização e invariantes (sem Find*, comunicação gameplay direta ou referências Unity em DTO);
- aplica todos os gates da linha e evidencia PASS, FAIL ou NOT RUN; SCOPED_PASS nunca vira global;
- preserva teste/gate exigido para código; em visual/animação registra a revisão humana;
- reutiliza evidência apenas com comando, versão/configuração, inputs, exit, XML/log e diff equivalentes.

Segurança e qualidade são eliminatórias, não compensáveis por tempo, bytes, tentativas ou custo. A métrica principal é a taxa de aprovação/falha de todos os casos elegíveis por braço, inclusive os que falharem ou terminarem NOT RUN por motivo registrado; custo ausente não exclui caso. Medianas de elapsed_ms, tentativas, bytes e telemetria disponível entre pares que passaram nos dois braços são métricas secundárias, explicitamente condicionadas à aprovação e nunca usadas para ocultar falhas. Não há meta de economia nesta amostra.

## Formato mínimo de rodada futura

Acrescentar um objeto por braço em uma cópia versionada do JSONL, sem framework novo. Campos mínimos: run_id, case_id, arm (baseline|candidate), execution_kind, commit_or_snapshot, host_model_requested, host_model_effective, prompt_sha256, declared_dirty_paths, started_at_utc, elapsed_ms, retries, input_bytes, output_bytes, output_lines, outcome (PASS|FAIL|NOT_RUN|ABORTED), gate_evidence, token_usage, cache_usage, cost e notes. Campos de telemetria podem ser null; output truncado deve constar em notes.

Antes da rodada, congelar snapshot, data, host, prompt e estado sujo. Repetir somente por falha transitória comprovada e manter retries. Mudança de fontes, prompt, gates ou rubrica exige nova versão do fixture e invalida a comparação pareada anterior.
