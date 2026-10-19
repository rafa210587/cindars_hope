# Equilíbrio reproduzível e aceitação integrada das builds

> **Spec ID:** spec_skills_08_equilibrio_aceitacao_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P1
> **Type:** Validation
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [00](spec_skills_00_decisoes_catalogo_v1.md), [01](spec_skills_01_avatar_execucao_v1.md), [02](spec_skills_02_pontos_respec_save_v1.md), [03](spec_skills_03_dados_rank_readiness_v1.md), [04](spec_skills_04_passivas_capstones_v1.md), [05](spec_skills_05_mecanicas_ativas_v1.md), [06](spec_skills_06_ui_canvas_skills_v1.md), [07](spec_skills_07_pixelart_feedback_v1.md)
> **Blocks:** Nenhuma
> **Validation level alvo:** Evidência integrada Unity/Play Mode e aceitação humana
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

O modelo analítico identificou exploits e curvas perigosas, mas não executa física, C#, IA ou percepção visual. Equilíbrio global ainda não pode ser afirmado.

**Escopo:** Comparar números aprovados, builds legais e comportamento real em Farm/Town/Cave, com critérios de aceitação e evidência rastreável.

**Fora do escopo:** Não declarar equilíbrio aprovado porque compila ou porque o script Node passou; não aumentar conteúdo para esconder falta de papel de uma skill.

### Contexto que deve ser preservado

RPG de ação classless em Vaalara/Dornécia, cinco árvores e quatro slots; Farm/Town/Cave se complementam.
D&D inspira papéis e ferramentas, sem copiar classes, d20 ou regras de descanso.
Anya/Fonte/Fruto Mana não viram substitutos genéricos de recursos comuns; mistérios de lore permanecem.
IDs e save existentes são preservados. Estado canônico atual prevalece sobre contagens históricas.

### Phase 0 observada

Auditoria de código desta sessão: [relatório](../../../docs/validation/SKILLS_CAPABILITIES_AUDIT_2026_09_09.md).
Baseline analítica: [relatório de equilíbrio](../../../docs/validation/skills_balance_v1/BALANCE_REVIEW.md).
O fato que motiva esta fatia é o problema descrito acima; não é evidência de correção.
Na execução, reconfirmar os arquivos abaixo com `rg -n` para as classes/métodos citados.
Se houver drift, atualizar spec/plan/tasks antes de alterar comportamento. Não reexecutar backlog antigo para reverter CURRENT_STATE.

### Critérios de aceitação

- **AC01:** Toda build de teste tem testemunho de compra que respeita tier/prérequisitos/rank e orçamento; dois capstones R1 a 54 pontos não são marcados ilegais automaticamente.
- **AC02:** Resultados do modelo são comparados com runtime sob entradas explicitadas; cada divergência tem causa ou pendência.
- **AC03:** Duplicatas/troca não geram vantagem fora de D01; comércio não gera ciclo lucrativo; crafting respeita teto aprovado.
- **AC04:** Há evidência humana de legibilidade/controle/identidade e resultados de gameplay; ausência de Play Mode/arte mantém aprovação correspondente pendente.

# /speckit.plan

## Plan — contratos e solução

Entregáveis propostos em `docs/validation/skills_sdd_v1/`: balance-runtime-comparison.md, build-scenarios.md, visual-acceptance.md e closeout.md.
Campos por cenário: revisão do catálogo, hashes, seed, nível/pontos, sequência legal de compras, equipamento, stats, alvo/resistência, rank/variante, política de cooldown, duração, inputs, resultado esperado, medido, evidência e conclusão.
Comparar baseline e candidato com as mesmas entradas. Informar tolerância antes de medir.
Matriz mínima: especialista melee, ranged, magic; sobrevivência/ofício; híbrido legal; build sem investimento; casos limites de capstone. Não assumir que uma build é legal apenas pela soma de pontos.
Cobrir: regeneração sustentável de mana, quatro cópias de cura, alternância entre curas distintas, combo de crafting, compra/venda sob reputação/arredondamento, pierce/fan contra grupos, marca com cast, gatilho de mana com pool variável.
Os limiares numéricos de diversão/TTK e sinks são decisões de 00, não números inventados durante o teste.
Falhas de integração são separadas de balanceamento: skill sem efeito não é 'fraca' para fins de ajuste.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `docs/validation/skills_balance_v1/balance-model.mjs`
- `docs/validation/skills_balance_v1/balance-results.json`
- `docs/validation/skills_balance_v1/BALANCE_REVIEW.md`
- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillTierRules.cs`
- `docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md`

Padrões aplicáveis: **progression-curve-design, economy-balance-tuning, gameplay-test-scenario, unity-validation**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Fixtures:** Em build-scenarios.md, congelar builds legais e condições comparáveis; obter expectativas do contrato 00.
2. **Modelo:** Atualizar balance-model.mjs apenas se o schema aprovado mudou, preservando análise baseline e hashes; rodar e registrar saída real.
3. **Runtime:** Medir os mesmos cenários no Unity após implementação das dependências, registrar custos/efeitos/recargas e divergências em balance-runtime-comparison.md.
4. **Humano:** Executar cenários de Farm/Town/Cave e UI com capturas/vídeos; registrar observações e aprovação por critério em visual-acceptance.md.
5. **Closeout:** Revisar evidência por spec e atualizar closeout.md com PASS/FAIL/NOT RUN e motivo real; promover apenas fatias que cumpriram seus gates.

### Falhas, migração e limites

Depende das filhas implementadas de 05, não apenas da conclusão documental da spec guarda-chuva. Limiares e tolerâncias precisam estar escritos antes dos testes.

Ausência de dependência obrigatória deve ser diagnosticada; não esconder falha por fallback de busca global.
Não publicar sucesso antes do efeito/commit correspondente. Não salvar referências Unity.
Sem edição manual de YAML. Wiring/assets exigem ferramenta Editor e evidência de geração.
Cave visited-level snapshots permanecem intactos; esta spec não autoriza regeneração.
Leitura de cena/testes antigos não prova Play Mode vigente.

# /speckit.tasks

## Tasks — revisão 1

Todos os itens estão **não executados**. Tarefas de pesquisa/decisão fecham o Plan; não autorizam preencher lacunas enquanto se implementa.

| Done | ID | Dependência | Arquivo/método e edição — ownership | Cobertura |
|---|---|---|---|---|
| [ ] | T01 | Dependências da spec | Fixtures — Em build-scenarios.md, congelar builds legais e condições comparáveis; obter expectativas do contrato 00. | AC01 |
| [ ] | T02 | T01 | Modelo — Atualizar balance-model.mjs apenas se o schema aprovado mudou, preservando análise baseline e hashes; rodar e registrar saída real. | AC01, AC02 |
| [ ] | T03 | T02 | Runtime — Medir os mesmos cenários no Unity após implementação das dependências, registrar custos/efeitos/recargas e divergências em balance-runtime-comparison.md. | AC02, AC03 |
| [ ] | T04 | T03 | Humano — Executar cenários de Farm/Town/Cave e UI com capturas/vídeos; registrar observações e aprovação por critério em visual-acceptance.md. | AC04 |
| [ ] | T05 | T04 | Closeout — Revisar evidência por spec e atualizar closeout.md com PASS/FAIL/NOT RUN e motivo real; promover apenas fatias que cumpriram seus gates. | AC01, AC02, AC03, AC04 |
| [ ] | T06 | T05 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **LegalBuildWitnesses:** todas as sequências de compras satisfazem as regras reais.
- **ModelRuntimeAgreement:** diferença dentro da tolerância definida por cenário ou divergência explicitamente reprovada.
- **ExploitMatrix:** duplicata/troca/cura/commerce/crafting testados contra regras aprovadas.
- **HumanAcceptance:** cada AC visual tem captura ou vídeo e decisão humana; sem evidência permanece pendente.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Depende das filhas implementadas de 05, não apenas da conclusão documental da spec guarda-chuva. Limiares e tolerâncias precisam estar escritos antes dos testes.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.

