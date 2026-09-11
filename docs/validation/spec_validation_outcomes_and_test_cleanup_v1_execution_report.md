# Execution report — validation outcomes and test cleanup

Data: 2026-09-08. Spec: `spec_validation_outcomes_and_test_cleanup_v1`.
Status: CODE_COMPLETE. Evidência final no
[relatório integrado](QUALITY_IMPROVEMENTS_EXECUTION.md).

validated_adrs: []
validated_game_rules: []

## Acceptance criteria extracted

Erro em log, exception, retorno inválido e ausência de configuração reprovam validação.
Warning permanece visível sem virar erro. Falha de um validator não impede os demais.
Cenas/gates/anchors ausentes e duplicados geram issues; componentes inativos entram na
inspeção; cenas alheias não suprem requisitos; somente cenas adquiridas pelo validator
podem ser fechadas. Limpeza de testes preserva proteções comportamentais.

## Existing systems audit

Reusados ValidationReport, IProjectValidator e ProjectValidationRunner. Adapter síncrono
captura logs legados com unsubscribe em finally; reports tipados seguem caminho existente.
Menu separa validação de geração/reparo e chama wrappers de shops/skills que expõem falhas.
ValidateProjectBatch devolve falha ao processo por exception e não abre diálogo.
FarmSceneLayoutV4 recebeu ownership/try-finally e erro para cena obrigatória ausente,
sem alterar checks/layout. City Schedule passou a reutilizar cena carregada ou abrir
aditivamente com fechamento somente da cena adquirida.
Transições validam cenas selecionadas e reusam cena carregada; nenhum YAML foi editado.

## Spec Compliance Matrix

| Critério | Mudança | Proteção |
|---|---|---|
| Resultado verdadeiro | Passed exige configuração e ausência de erro | Erro, warning, exception, null e listener removido |
| Suite continua e explica | Aggregate inclui erro/config ausente e diagnóstico completo | Próximo validator executa e summary contém causa |
| Cenas isoladas | Aquisição aditiva com ownership; inspeção separada | Preview scenes, inactive, duplicate, foreign scene, ausência; aquisição revisada estaticamente e exercitada em batch |
| Cortes sem perda duplicada | 8 atribuições/List; 8 escalas duplicadas; 1 falsa prova de integração | Defaults, tabela concreta, fórmulas e guards mantidos |
| Arquitetura em um owner | 3 scans source/GUID/predefined no PowerShell canônico | 17 contratos adversariais; 2 testes de assembly compilada mantidos |
| Saves inválidos | 4 casos agora usam managers válidos com sentinela | Stamina e efeito/duração/identidade permanecem intactos |

## Validation

Baseline full: `TestResults/quality-improvements/20260908-000202/`, 2904 casos,
2900 PASS e quatro falhas preexistentes de farm.
Runner characterization: 9/9 PASS. Fixture de cenas precisou substituir NewScene(Additive)
por NewPreviewScene para coexistir com a cena untitled do TestRunner.
Compilação e resultados finais no relatório integrado com XML, exit e fingerprints:
126/126 casos das fixtures alteradas PASS dentro do full 2911/2915. Batch real executou
todos os passos e reprovou lacunas de wiring; City Schedule 21/21 PASS, sem mudanças
nos fingerprints. Isso não prova todo estado dirty em memória do Editor.

Ratchet canônico executado via `run_strict_validation.ps1 -Gates corruption,architecture`:
exit 0, SCOPED_PASS, 24 GUIDs verificados e 1701 fontes inspecionadas para predefined
assembly names. Baselines reais intactas; teste de source duplicado removido do Unity
somente depois da proteção equivalente no executor canônico.

## Honest status rationale

Código concluído; ausência de nova falha no escopo só pode ser afirmada com a rodada final.
As quatro falhas de farm não foram excluídas nem tiveram expectativas relaxadas.
Checks de HUD por nome de método e reputação por nome de classe continuam candidatos:
exigem substituição por contrato comportamental, não exclusão automática.
Validação de transições prova presença/IDs/duplicação/destinos não vazios; não certifica
toda combinação semântica de destino/anchor. Adapter de logs atende validadores síncronos.
Menu completo e aceitação visual/humana devem ser diferenciados de fixtures automatizadas.
