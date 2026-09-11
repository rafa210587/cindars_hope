# Consumidores de passivas e gatilhos reais de capstone

> **Spec ID:** spec_skills_04_passivas_capstones_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [00](spec_skills_00_decisoes_catalogo_v1.md), [02](spec_skills_02_pontos_respec_save_v1.md), [03](spec_skills_03_dados_rank_readiness_v1.md)
> **Blocks:** [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

Há nove passivas sem efeito consumidor; hooks de ouro/harvest/craft/tool sem consumo externo e bônus condicionais tratados como incondicionais. Variantes são salvas mas não aplicadas como condutas distintas.

**Escopo:** Fechar as passivas e capstones escolhidas em 00 com consumidores reais, condições observáveis e stacking definido.

**Fora do escopo:** Não habilitar todos os hooks com multiplicadores genéricos; não converter variantes divinas em simples bônus universais.

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

- **AC01:** Cada passiva promovida a operacional tem consumer e teste de antes/depois, condição falsa/verdadeira e remoção no respec.
- **AC02:** Capstones aplicam só a variante comprada e gatilho aceito; reset/scene/load não deixam bônus órfão.
- **AC03:** Crafting/comércio usam fórmula aprovada e passam teste de ciclo de compra/venda e limites de throughput.
- **AC04:** Revalidação de equipamento após respec não permite bônus de tier indevido e não perde item.

# /speckit.plan

## Plan — contratos e solução

Reusar SkillEffectAggregator, SkillPassiveApplicator e SkillModifierHooks como limites existentes. O plano final deve nomear cada ponto de consumo antes da promoção.
Matriz mínima por nó: condição → evento/consulta → modificador → alvo → duração → remoção → stack → evidência.
Prioridade:
1. corrigir condições de dual wield/heavy/kiting/safe step/arcane mastery;
2. avaliar melee_dodge_training, ranged_projectile_tuning, survival_status_recovery;
3. fechar crafting_material_eye/salvage_method/shop_sense/pack_order/durable_finish com consumer real de cada domínio;
4. implementar melee_capstone_battle_rhythm e variantes, sem recriar um sistema de stats.

Para capstone de mana, D06 deve escolher janela deslizante ou timer reiniciado. Janela verdadeira: remover amostras anteriores a t−6; somar apenas gasto elegível; avaliar limiar; consumir gatilho uma vez segundo lockout definido. Não contar custo recusado, regen ou refund.
Capstone variante depende da escolha salva, e respec remove estado transitório e bônus.
Crafting e comércio exigem regra de combinação antes do wiring: redução aditiva de 75% equivale a 4x throughput, não 75% a mais de produção. Multiplicadores simétricos de compra/venda podem gerar arbitragem.
Equip gate deve operar sobre itemId de conteúdo, resolvendo itemInstanceId antes da consulta, e reavaliar após respec sem destruir item.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/SkillEffectAggregator.cs`
- `Assets/_Game/Scripts/Skills/SkillPassiveApplicator.cs`
- `Assets/_Game/Scripts/Skills/SkillModifierHooks.cs`
- `Assets/_Game/Scripts/Skills/SkillTierEquipGate.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeState.cs`
- `Assets/_Game/Scripts/Player/PlayerController.cs`

Padrões aplicáveis: **ability-effect-composition, event-bus-pattern, economy-balance-tuning, progression-curve-design**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Mapa de consumo:** Em SkillEffectAggregator/SkillModifierHooks/SkillPassiveApplicator, mapear payloads e localizar consumidores específicos; acrescentar seus paths e assinaturas ao plano antes de código.
2. **Condicionais:** Modificar os ramos de agregação para aplicar dual wield/heavy/kiting/safe step/arcane mastery apenas no contexto elegível, sem contaminar stats persistentes.
3. **Economia e ofício:** Conectar apenas os hooks decididos em D04 aos consumidores nomeados; provar fórmula e não duplicar desconto em UI e transação.
4. **Capstones:** Aplicar variantes e acumulador temporal de D06; limpar assinaturas/estado no ciclo de vida e no respec.
5. **Equipamento:** Em SkillTierEquipGate e consumer de equipamento identificado, resolver ID de conteúdo e revalidar preservando inventário.
6. **Regressão:** Testar condições, desativação, variantes, economia e item; atualizar estado de cada nó na matriz 00.

### Falhas, migração e limites

Plano ainda depende de D04/D06 e do mapeamento de arquivos consumidores fora de Skills. Não está pronto para execução; a primeira task de planejamento resolve ownership antes de promover.

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
| [ ] | T01 | Dependências da spec | Mapa de consumo — Em SkillEffectAggregator/SkillModifierHooks/SkillPassiveApplicator, mapear payloads e localizar consumidores específicos; acrescentar seus paths e assinaturas ao plano antes de código. | AC01 |
| [ ] | T02 | T01 | Condicionais — Modificar os ramos de agregação para aplicar dual wield/heavy/kiting/safe step/arcane mastery apenas no contexto elegível, sem contaminar stats persistentes. | AC01 |
| [ ] | T03 | T02 | Economia e ofício — Conectar apenas os hooks decididos em D04 aos consumidores nomeados; provar fórmula e não duplicar desconto em UI e transação. | AC03 |
| [ ] | T04 | T03 | Capstones — Aplicar variantes e acumulador temporal de D06; limpar assinaturas/estado no ciclo de vida e no respec. | AC02 |
| [ ] | T05 | T04 | Equipamento — Em SkillTierEquipGate e consumer de equipamento identificado, resolver ID de conteúdo e revalidar preservando inventário. | AC04 |
| [ ] | T06 | T05 | Regressão — Testar condições, desativação, variantes, economia e item; atualizar estado de cada nó na matriz 00. | AC01, AC02, AC03, AC04 |
| [ ] | T07 | T06 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **ConditionalPassive_Ineligible_NoBonus:** condição falsa não altera resultado; verdadeira altera pelo valor aprovado.
- **Respec_RemovesPassiveAndCapstoneState:** nenhum modificador residual após reset.
- **ManaWindow_ExcludesOldSpend:** custos 25 em t=0,5,10; janela de 6 s em t=10 soma 50, nunca 75, se D06 escolher deslizante.
- **TradeCycle_NoPositiveReturn:** para cada preço/reputação/arredondamento aprovado, vender recompra não aumenta ouro.
- **EquipmentRespec_PreservesItem:** itemInstanceId continua no inventário; requisito é avaliado pelo itemId.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Plano ainda depende de D04/D06 e do mapeamento de arquivos consumidores fora de Skills. Não está pronto para execução; a primeira task de planejamento resolve ownership antes de promover.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
Consumidores, condições e progressão propostas por nó estão no [refinamento funcional v3](../../../docs/refinements/a_implementar/ref_skill_catalog_functional_contract_v3.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.
