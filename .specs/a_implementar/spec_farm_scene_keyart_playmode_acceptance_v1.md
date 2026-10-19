# SPEC — Aceitação humana e regressão da FarmScene keyart

> **Spec ID:** `spec_farm_scene_keyart_playmode_acceptance_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Closeout  
> **Priority:** P1  
> **Type:** Validation / Docs  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_CLOSEOUT  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que ainda altere FarmScene, seus colliders ou suas capturas  
> **Repo lock scope:** `docs/validation/playmode/**`, `docs/validation/spec_farm_scene_keyart_playmode_acceptance_v1_execution_report.md`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmScene*.cs`  
> **Depends on:** `spec_farm_scene_biome_decoration_v1`  
> **Blocks:** promoção do lote FARM KEYART para aceito  
> **Scope:** executar e registrar a prova humana de visual, física e loops de gameplay da FarmScene final.  
> **Out of scope:** consertar achados; achados abrem bugfix/spec nova.  
> **Validation level alvo:** PLAYMODE  
> **Executor:** Claude | Codex + humano no Unity Editor

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_biome_decoration_v1`.
- **Bloqueia:** promoção do lote FARM KEYART para aceito.

## 5. Contexto

As specs anteriores corrigem fonte espacial, física, composição, terreno, marcos e decoração. Nenhuma pode declarar a cena aceita apenas por build/captura editor: a câmera real, colisão do player e interações precisam ser verificadas em Play Mode.

## 6. Problema

Uma cena pode passar validators e ainda ter caminho aparentemente aberto bloqueado, prompt inacessível, sorting errado em câmera ou arte visualmente incompatível com a keyart.

## 7. Objetivo

Produzir uma evidência reproduzível que marque cada cenário como PASS/FAIL/NOT RUN e compare as capturas finais com a keyart, sem alegações falsas de validação.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; todas as specs deste lote; `.agents/skills/gameplay-test-scenario/SKILL.md`; `.agents/skills/unity-validation/SKILL.md`; `.agents/skills/implementation-closeout/SKILL.md`; `FarmSceneCapture.cs`; todos `ValidateFarmScene*.cs` criados no lote.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: FarmSceneCapture com saída full, homestead e animais em docs/validation/playmode/.
Existe: screenshots atuais de 14/08/2026 que comprovam baseline ainda divergente.
Existe: regras do projeto exigem registrar Unity validation NOT RUN com motivo/comando/risco quando bloqueada.
Ausente: cenário único de aceitação keyart que cobre a rota completa e checklist visual/físico.
```

## 13. Regras de não duplicação

Reusar `FarmSceneCapture`, validators e checklists de Play Mode existentes quando compatíveis. Não criar feature runtime, não editar a cena manualmente e não reexecutar gerador como parte implícita da captura sem registrar.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  docs/validation/playmode/spec_farm_scene_keyart_playmode_scenario.md — roteiro humano com passos, expected result e evidência.
  docs/validation/spec_farm_scene_keyart_playmode_acceptance_v1_execution_report.md — resultado por gate e riscos residuais.
MODIFICAR:
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmScene*.cs — apenas se um achado provar ausência objetiva de gate automático.
```

## 16. Contratos, dados e eventos

N/A justificado: spec de validação/documentação; não cria contrato runtime, evento, UI ou save.

## 17. Sistemas afetados

FarmScene / player movement / Physics2D / crop/animal/fishing/crafting interactions / captures / documentation.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Validation/ValidateFarmScene*.cs
docs/validation/playmode/**
docs/validation/spec_farm_scene_keyart_playmode_acceptance_v1_execution_report.md
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, runtime scripts, `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Gates automáticos

Rodar todos os validators de FarmScene e compile antes do Play Mode. Qualquer fail interrompe a validação humana e vira achado.

### Fase 1 — Captura de referência

Gerar full/homestead/animals após a regeneração canônica. O checklist visual compara: massa central de cultivo, montanha, bosque, homestead, animais, lago, margens, ausência de retângulos dominantes e densidade sem obstrução.

### Fase 2 — Rota humana

Do spawn: cruzar ponte; encostar em lake/river/mountain e confirmar bloqueio; alcançar casa, fonte, campo, craft, shipping, sell, animal pens, fishing, cave e town exit; plantar/regar um tile permitido e tentar em água/prédio; confirmar prompts/interações.

### Fase 3 — Relatório honesto

Tabela por item com status PASS/FAIL/NOT RUN, captura/log associado, build/validator exit code e risco residual. Um FAIL abre nova spec/bugfix; não é corrigido dentro desta spec de validação.

## Ordem de execucao

1. Validators; 2. compile; 3. regen registrado; 4. capturas; 5. roteiro Play Mode; 6. relatório; 7. `/finish-spec` apenas se todos gates previstos forem verdadeiros.

## 14. Critérios de aceite

### 14.1 Gates automáticos limpos

- Resultado: spatial, navigation, composition, terrain, landmarks e decoration validators passam.
- DoD: log contém seis linhas com `0 error(s)` ou `valid: 6/6`/`7/7`/`10/10` conforme cada validator.

### 14.2 Rota física humana completa

- Resultado: os 10 marcos são alcançados; água/montanha bloqueiam; ponte passa; aragem respeita regras.
- DoD: checklist contém exatamente `PASS` para os itens 1–16 da rota, com data e build usados.

### 14.3 Fidelidade visual verificável

- Resultado: três capturas finais existem e checklist visual não tem `FAIL` nos oito critérios de massa, escala, terreno, água, cultivo, bosque, homestead e densidade.
- DoD: relatório aponta os paths das três imagens e `Visual comparison: 8/8 PASS`.

### 14.4 Verdade de validação

- Resultado: qualquer item não executado aparece como `NOT RUN`, com motivo/comando/risco; nenhuma conclusão chama isso de PASS.
- DoD: relatório contém bloco `Unity validation: PASS` ou o bloco canônico `Unity validation: NOT RUN` completo.

## 23. Edge cases / falhas

- Editor aberto impede batchmode: registrar NOT RUN ou usar Play Mode humano, nunca inventar exit code.
- Camera capture difere do gameplay: anexar ambas as evidências e identificar a diferença.
- Falha em um único landmark: marcar FAIL, não compensar por resultado visual.
- Arte sem sprite/placeholder: registrar como FAIL visual mesmo que física passe.
- Dirty worktree externo: listar como risco ambiental sem atribuí-lo à spec.

## 22. Validação e gates

Esta é a evidência PLAYMODE do lote. O closeout só pode declarar aceitação quando todos os critérios estiverem PASS; do contrário, estado permanece `BUILD_VALIDATED` ou `NOT RUN` conforme a evidência.

# /speckit.tasks

- [ ] Executar as fases 0–3 na ordem da §21 e registrar a evidência exigida.
