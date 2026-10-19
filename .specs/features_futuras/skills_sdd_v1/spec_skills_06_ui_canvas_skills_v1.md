# Tela de habilidades em Canvas com compra, rank, variante e slots

> **Spec ID:** spec_skills_06_ui_canvas_skills_v1
> **Status:** RASCUNHO — implementação não autorizada
> **Wave:** SKILLS_SDD_V1
> **Priority:** P1
> **Type:** UI
> **Domain:** Skills
> **Revision:** 1 — 2026-09-09
> **Parallelizable:** NO — serial por padrão; só delegar ownership disjunto após fechar contratos
> **Repo lock scope:** arquivos listados no Plan; novos paths propostos só após revisão do escopo
> **Depends on:** [02](spec_skills_02_pontos_respec_save_v1.md), [03](spec_skills_03_dados_rank_readiness_v1.md)
> **Blocks:** [07](spec_skills_07_pixelart_feedback_v1.md), [08](spec_skills_08_equilibrio_aceitacao_v1.md)
> **Validation level alvo:** Unity compile + testes aplicáveis + Play Mode; visual quando aplicável
> **Executor:** Codex ou Claude; mesma spec e mesmas regras
> **Stage:** Spec + Plan + Tasks redigidos; pendências abaixo impedem promoção automática

# /speckit.specify

## Spec — problema e resultado

O caminho instalado usa SkillTreeGameplayPanelController/IMGUI; o Canvas existente não fecha o fluxo. Rank-up/variante não são acessíveis, o HUD mistura teclas e referências de views podem estar vazias.

**Escopo:** Um fluxo instalado em Canvas para cinco árvores, detalhes, compra, rank, escolha real de variante, respec e quatro slots coerentes com execução.

**Fora do escopo:** Sem lógica de compra duplicada na UI, nova moeda, catálogo paralelo de ícones ou alegação de gamepad funcional sem validar o input instalado.

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

Reconfirmação desta autoria: PresentationRuntimeInstaller.Install em DomainRuntimeInstallers.cs chama SkillTreeGameplayPanelController.Install(owner).
SkillTreePanel.cs já tem OnEnable/OnDisable pareando AddListener/RemoveListener dos botões existentes; estender esse ciclo para novos callbacks, sem duplicar assinaturas.

- **AC01:** O jogador compra, aumenta rank e escolhe/cancela variante pela tela; estado real e UI permanecem iguais após reabrir.
- **AC02:** Slots 1–4 mostram a mesma ação, ícone, custo, recarga e motivo de recusa do controller; não aparecem bindings legados R/T/Y/G.
- **AC03:** Modal bloqueia ações/movimento do jogador e fecha por Esc; abrir/fechar repetido não duplica callbacks.
- **AC04:** Farm/Town/Cave têm uma tela instalada, refs obrigatórias não nulas e ausência de camada IMGUI concorrente.

# /speckit.plan

## Plan — contratos e solução

Consumir APIs existentes do SkillTreeManager: TryPurchaseNode(nodeId, level, chosenVariant, out feedback), TryRankUpNode(nodeId, out feedback), TryAssignActiveSlot(slotIndex, skillActionId).
Consumir EvaluateNode/EvaluateSlot de 03. Equipar e executar são operações distintas: cooldown impede uso conforme política, não bloqueia equip por inferência.
Preservar seleção por nodeId, nunca somente índice.
Estender SkillTreeMenuViewModel com estado de rank, cap, requisito/variante, FailureKey e IDs de ação. Reusar tipos existentes onde houver campo equivalente.
Propor `void SkillTreePanel.RefreshDisplay()` como ponte de projeção→views, sem regras de negócio.
Preservar OnEnable/OnDisable: cada novo listener/evento de variante, rank, slots e domínio deve ter remoção simétrica; rebind não adiciona outro handler.
Fluxo de variante: selecionar nó → mostrar variantes com descrição/contrapartida → escolher → confirmar → chamar a sobrecarga já existente. Cancelar não compra. Não aceitar painel que apenas diga 'pendente'.
Ícone vem de SkillNodeDataSO.Icon/SkillActionSO.Icon, com política de precedência documentada; fallback diagnosticável até integração 07.
Gerar Canvas/prefabs por ferramenta Editor no padrão existente; não editar YAML manualmente. Antes de remover IMGUI, provar que o Canvas foi instalado e suas referências foram preenchidas.

### Ownership e fontes concretas

MODIFICAR somente os arquivos de código/arte explicitamente previstos nas tasks, depois de promover o pacote.
Arquivos de lore e relatórios listados como fontes são somente leitura, salvo atualização documental expressamente descrita.
CRIAR somente contratos/entregáveis/testes nomeados no plano; caminhos ainda não identificados são pendência, não autorização por wildcard.

- `Assets/_Game/Scripts/UI/Skills/SkillTreePanel.cs`
- `Assets/_Game/Scripts/UI/Skills/SkillTreeMenuViewModel.cs`
- `Assets/_Game/Scripts/UI/Skills/SkillTreeGameplayPanelController.cs`
- `Assets/_Game/Scripts/UI/Skills/SkillTreeInputHandler.cs`
- `Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs`
- `Assets/_Game/Scripts/UI/HUD/Views/ActiveSkillSlotsHudView.cs`
- `Assets/_Game/Scripts/UI/HUD/ActiveSkillSlotProjection.cs`
- `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs`

Padrões aplicáveis: **ui-projection-pattern, hud-canvas-binding, ui-modal-stack, input-gamepad-routing, localization-authoring**. Carregar as skills pelo gatilho, sem copiar suas instruções para o runtime.
Reusar os sistemas citados. Não criar managers/catálogos paralelos para evitar a integração existente.
Testes C# novos ficam na assembly apropriada em `Assets/_Game/Tests/EditMode/` ou `Assets/_Game/Tests/PlayMode/`, nunca em Scripts.

### Sequência técnica

1. **Projeção:** Em SkillTreeMenuViewModel e ActiveSkillSlotProjection, mapear consultas de 03 sem regras próprias de tier/rank; preservar nodeId selecionado.
2. **Interação:** Em SkillTreePanel, implementar RefreshDisplay e callbacks separados de comprar/rank/variante/slot; completar escolha e cancelamento de variante usando API existente.
3. **HUD:** Em ActiveSkillSlotsHudView e GameplayHudCanvasController, preencher quatro bindings e renderizar cooldown/FailureKey da consulta compartilhada.
4. **Instalação:** Em DomainRuntimeInstallers e SkillTreeInputHandler, instalar/abrir uma tela Canvas com refs geradas; desativar caminho IMGUI de SkillTreeGameplayPanelController após prova do binding.
5. **Arte e localização:** Consumir Icon dos SOs existentes e chaves localizadas; integrar arte final apenas na spec 07.
6. **Aceitação:** Testar compra/rank/variante cancelada e confirmada, respec e slots; abrir/fechar 10 vezes e percorrer três cenas.

### Falhas, migração e limites

Depende dos contratos 03. Arquivo do gerador Editor/prefab final precisa ser identificado e acrescentado ao allowed scope antes da execução; art placeholders não passam aceitação visual 07.

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
| [ ] | T01 | Dependências da spec | Projeção — Em SkillTreeMenuViewModel e ActiveSkillSlotProjection, mapear consultas de 03 sem regras próprias de tier/rank; preservar nodeId selecionado. | AC01, AC02 |
| [ ] | T02 | T01 | Interação — Em SkillTreePanel, implementar RefreshDisplay e callbacks de comprar/rank/variante/slot; completar variante pela API existente e parear cada listener em OnEnable/OnDisable. | AC01, AC03 |
| [ ] | T03 | T02 | HUD — Em ActiveSkillSlotsHudView e GameplayHudCanvasController, preencher quatro bindings e renderizar cooldown/FailureKey da consulta compartilhada. | AC02 |
| [ ] | T04 | T03 | Instalação — Em DomainRuntimeInstallers e SkillTreeInputHandler, instalar/abrir uma tela Canvas com refs geradas; desativar caminho IMGUI de SkillTreeGameplayPanelController após prova do binding. | AC03, AC04 |
| [ ] | T05 | T04 | Arte e localização — Consumir Icon dos SOs existentes e chaves localizadas; integrar arte final apenas na spec 07. | AC01, AC02 |
| [ ] | T06 | T05 | Aceitação — Testar compra/rank/variante cancelada e confirmada, respec e slots; abrir/fechar 10 vezes e percorrer três cenas. | AC01, AC02, AC03, AC04 |
| [ ] | T07 | T06 | Evidência e closeout: registrar testes, diff, critérios pendentes e revisão de não regressão conforme /finish-spec | AC01, AC02, AC03, AC04 |

### Testes e evidência esperada

- **Variant_Cancel_NoPurchase_ConfirmUsesChosenId:** cancelar mantém saldo; confirmar compra exatamente a variante escolhida.
- **ViewModel_ReadinessMatchesDomain:** campos de UI correspondem às consultas para comprado/bloqueado/rank cap/dormente.
- **Modal_Reopen10Times_OneCallback:** um clique produz uma mutação, zero movimento/ataque sob modal.
- **CanvasBinding_AllRequiredReferencesAssigned:** zero referência obrigatória nula e exatamente um painel ativo.
- **Canvas_EscapeClosesAndRestoresInput:** Esc fecha o modal, devolve foco ao gameplay e só então permite movimento/uso.
- **Canvas_ShowsCanonicalBindings:** slots exibem 1–4 e não R/T/Y/G; teclas legadas não disparam habilidades pela tela, sem proibir usos não relacionados dessas teclas no jogo.

Para testes EditMode implementados, usar `tools/unity/RunUnityEditModeTests.ps1 -TestFilter <fixture implementada>`.
Resultado esperado do runner: `UNITY_EDITMODE: PASS;` com `failed=0`; guardar XML/log reais.
Play Mode usa a assembly/cenário aplicável, identificando cena, build e resultado. Testes nomeados acima são especificações de comportamento, ainda não existem por terem sido escritos aqui.
A spec de validação também exige os relatórios/capturas definidos no contrato.

### Revisão de consistência e prontidão

- ACs possuem tasks e verificações nomeadas; nenhuma task está marcada concluída.
- Dependências são anteriores no lote; 05 exige decomposição adicional antes de implementação.
- Arquivos compartilhados impedem paralelismo automático. Serial é a ordem segura inicial.
- Depende dos contratos 03. Arquivo do gerador Editor/prefab final precisa ser identificado e acrescentado ao allowed scope antes da execução; art placeholders não passam aceitação visual 07.
- **Prontidão:** documento revisável; não promover enquanto pendências de contrato/ownership/decisão relevantes não estiverem fechadas e o Depth Gate não passar.
- **Autorização:** o pedido desta sessão é gerar e refinar specs/plan/tasks; não executar gameplay.

## Fontes de design

[Refinamento técnico v1](../../../docs/refinements/a_implementar/ref_skills_capabilities_gameplay_ui_pixelart_v1.md) e
[revisão de mundo/equilíbrio v2](../../../docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).
As decisões necessárias estão transcritas neste pacote; os refinamentos não precisam virar leitura histórica obrigatória para cada executor.
