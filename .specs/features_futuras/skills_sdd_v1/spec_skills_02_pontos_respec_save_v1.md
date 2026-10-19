# Integridade de pontos, compras, respec e restauração

> **Spec ID:** spec_skills_02_pontos_respec_save_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P0
> **Type:** Save
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** Nenhuma dependência de mudança de design
> **Blocks:** [03](spec_skills_03_dados_rank_readiness_v1.md), [04](spec_skills_04_passivas_capstones_v1.md), [06](spec_skills_06_ui_canvas_skills_v1.md), [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

Compra/rank podem publicar sucesso antes do débito de progressão. Respec reconstrói pontos pelo nível e não restitui o saldo do ledger, perdendo bônus de atos. A migração de nós desconhecidos pode divergir do saldo restaurado.

**Escopo:** Compra/rank/respec consistentes com o saldo de progressão, reembolso exato e migração repetível de snapshots.

**Fora do escopo:** Sem reset por nível, novo save manager, fallback singleton para mascarar injeção ausente ou alteração do custo canônico do respec.

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

Reconfirmação de código nesta autoria: `rg -n 'TryPurchase|TryRankUp|TryRespec|RestoreFromSaveData|MigrateUnknown' Assets/_Game/Scripts/Skills/SkillTreeManager.cs` localizou compra em 131/139, rank em 166/172, respec em 237/239 e restore/migração em 294/315.
Leitura atual confirmou `_purchaseService.TryPurchase` antes de `_progressionManager.TrySpendSkillPoints(1)`.
SkillRespecService.TryRespec calcula `CalculateTotalSkillPointsAtLevel` antes de FullRespec.
SkillTreeSaveData v2 tem NodeRanks e PurchasedNodeIds, mas não registra custo histórico por rank.

- **AC01:** Compra e rank debitam exatamente o custo uma vez; falha de validação/commit não muda pontos, nós, ouro nem publica sucesso.
- **AC02:** Com 12 pontos gastos e 3 livres (incluindo bônus), respec resulta em 15 livres nos dois estados; nova compra de custo 1 deixa 14.
- **AC03:** Respec gratuito inicial e custo posterior vigente são preservados; cancelamento/ouro insuficiente não incrementam contador.
- **AC04:** Restaurar duas vezes o mesmo snapshot completo com nó removido produz o mesmo saldo e ranks; novo save normalizado mantém esse resultado.
- **AC05:** Uma política versionada define créditos para ID desconhecido com rank explícito, legado sem rank, duplicatas e dados inválidos; a tabela tem entradas/saídas exatas antes de implementar migração. Não extrapolar custo histórico ausente.

# /speckit.plan

## Plan — contratos e solução

Preservar APIs públicas existentes:
`bool SkillTreeManager.TryPurchaseNode(string nodeId, int playerLevel, string chosenVariant, out string feedback)`;
`bool TryRankUpNode(string nodeId, out string feedback)`;
`bool TryRespec(ref int gold, int playerLevel)`;
`void RestoreFromSaveData(SkillTreeSaveData data, int playerLevel)`.

Estender o serviço existente, sem manager paralelo:
`bool SkillPurchaseService.ValidatePurchase(string nodeId, SkillTreeState state, int playerLevel, string chosenVariant, out string failReason)`;
`bool ValidateRankUp(string nodeId, SkillTreeState state, out string failReason)`.
Ambas são consultas sem mutação/evento de sucesso. Aplicação e eventos ficam sob um único commit coordenado; preservar wrappers legados com comportamento definido.

Invariante: U_after = U_before − custo efetivamente comprado; respec U_after = U_before + custo histórico dos ranks removidos. Bônus já recebidos permanecem.
O exemplo de AC02 é uma fixture de conservação: 3 livres + 12 gastos = 15; compra de custo 1 deixa 14. Não é um resultado medido nem uma aprovação de balanceamento.
ValidatePurchase/ValidateRankUp são métodos NOVOS extraídos dos TryPurchase/TryRankUp existentes; a task T01 altera os existentes para extrair os novos, sem confundir as duas APIs.
Algoritmo: validar → preparar estado candidato e saldo → confirmar ambos → recomputar derivados → publicar sucesso. Falha antes do commit deixa estado, ouro e eventos de sucesso intactos.
Não usar GrantSkillPoints para rollback cego se isso emitir falso evento de recompensa; manter rollback silencioso dentro da transação existente.
Restore parte dos snapshots de progressão e skill daquela carga, normaliza IDs/ranks sem mutar o DTO fornecido e produz o mesmo resultado para a mesma carga. Recarregar o mesmo save não acumula reembolso. HashSet apenas em memória não prova essa propriedade.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/Skills/SkillTreeManager.cs`
- `Assets/_Game/Scripts/Skills/SkillPurchaseService.cs`
- `Assets/_Game/Scripts/Skills/SkillRespecService.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeState.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeSaveData.cs`
- `Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs`
- `Assets/_Game/Scripts/Save/Providers/ProgressionSectionProvider.cs`
- `Assets/_Game/Scripts/Save/Providers/SkillTreeSectionProvider.cs`

Padrões aplicáveis: **skill-tree-authoring, save-load-pattern, save-section-provider, event-bus-pattern**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Validação pura:** Em SkillPurchaseService.TryPurchase/TryRankUp, extrair validação sem efeitos; manter a mesma avaliação de tier, prereq e variante.
2. **Commit:** Em SkillTreeManager.TryPurchaseNode/TryRankUpNode, coordenar candidato/saldo antes dos eventos; em PlayerProgressionManager, reutilizar estado interno com rollback sem evento de grant se necessário.
3. **Respec:** Em SkillRespecService.TryRespec e SkillTreeState.FullRespec, receber total preservado calculado por saldo+gasto; coordenar devolução e ouro no manager antes dos eventos.
4. **Restore:** Em SkillTreeManager.RestoreFromSaveData/MigrateUnknownNodes, normalizar cópia do snapshot e reconciliar saldo; manter ProgressionSectionProvider antes de SkillTreeSectionProvider e injeção explícita.
5. **Verificação:** Testar falha de compra, variantes, bônus de ato, respec repetido e duas cargas completas; capturar eventos e comparar DTOs.

### Falhas, migração e limites

Refund de ID desconhecido precisa de custo histórico recuperável. Se o save antigo não fornecer custo/rank confiável, registrar política de migração explícita; não inventar créditos pelo nível.

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
| [ ] | T01 | Dependências da spec | Validação pura — Em SkillPurchaseService.TryPurchase/TryRankUp, extrair validação sem efeitos; manter a mesma avaliação de tier, prereq e variante. | AC01 |
| [ ] | T02 | T01 | Commit — Em SkillTreeManager.TryPurchaseNode/TryRankUpNode, coordenar candidato/saldo antes dos eventos; em PlayerProgressionManager, reutilizar estado interno com rollback sem evento de grant se necessário. | AC01 |
| [ ] | T03 | T02 | Respec — Em SkillRespecService.TryRespec e SkillTreeState.FullRespec, receber total preservado calculado por saldo+gasto; coordenar devolução e ouro no manager antes dos eventos. | AC02, AC03 |
| [ ] | T04 | T03 | Restore — Primeiro fechar tabela versionada de migração (rank explícito/legado/duplicata/inválido); depois, em SkillTreeManager.RestoreFromSaveData/MigrateUnknownNodes, normalizar cópia e reconciliar saldo; manter ordem dos providers e injeção explícita. | AC04, AC05 |
| [ ] | T05 | T04 | Verificação — Testar falha de compra, variantes, bônus de ato, respec repetido e duas cargas completas; capturar eventos e comparar DTOs. | AC01, AC02, AC03, AC04 |
| [ ] | T06 | T05 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **Purchase_FailedCommit_NoMutationOrSuccessEvent:** estados e saldo iguais ao snapshot e zero eventos de sucesso.
- **Respec_PreservesActPoints_AndNextPurchase:** 12+3 → 15 → 14.
- **Respec_InsufficientGold_IsAtomic:** saldo, ouro e RespecCount inalterados.
- **Restore_SameSnapshotTwice_IsDeterministic:** saldo/ranks/slots finais iguais; DTO de entrada permanece igual.
- **NormalizedSave_RoundTrip_NoExtraRefund:** save/load normalizado não aumenta saldo.
- **UnknownNodeMigration_MatchesVersionedPolicy:** fixtures de rank explícito, legado, duplicata e inválido correspondem exatamente à tabela de AC05; carga repetida não duplica crédito.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Refund de ID desconhecido precisa de custo histórico recuperável. Se o save antigo não fornecer custo/rank confiável, registrar política de migração explícita; não inventar créditos pelo nível.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.
