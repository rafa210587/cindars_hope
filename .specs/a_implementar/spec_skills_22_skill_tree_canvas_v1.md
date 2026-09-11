# Skills — fase UI 1: árvore de habilidades Canvas

> **Spec ID:** spec_skills_22_skill_tree_canvas_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+UI / Skills+Presentation / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_21_balance_acceptance_v1  
> **Blocks:** spec_skills_23_skill_loadout_hud_v1  
> **Validation:** EditMode+PlayMode+capturas gráficas

# /speckit.specify

## Spec

Substituir atomicamente o painel IMGUI vivo por uma árvore Canvas modal, legível e dirigida por projeção.

- **AC01:** U abre uma única tela Canvas nas cenas Farm, Town e Cave; fechar devolve o foco ao gameplay e não move o jogador durante o modal.
- **AC02:** tabs das cinco árvores mostram grafo determinístico por tier e pré-requisito, com conexões e scroll/foco que mantêm o nó selecionado visível.
- **AC03:** cada nó projeta três eixos independentes: aquisição (Locked/Purchasable/Unaffordable/Owned/MaxRank), readiness (Operational/Dormant/EffectPending) e variante (None/ChoiceRequired/Chosen). Overlays compõem os eixos e cobrem Owned+Dormant, Owned+EffectPending e Purchasable+ChoiceRequired.
- **AC04:** detalhe mostra nome localizado, rank atual/cap, próximo ganho, custo em pontos, custo de ação, cooldown, forma/alcance, requisito de equipamento e motivo de bloqueio.
- **AC05:** compra e rank-up passam por `SkillNodePurchaseFlow`, cujo snapshot inclui operação, nodeId, rank/custo esperados e variante. O segundo confirm revalida tudo atomicamente; capstones exigem escolha exclusiva. Esc percorre Choice/Confirm→Detail→Graph→close sem empilhar outro modal.
- **AC06:** respec aparece apenas como orientação à Fonte de Anya; a tela não cria um segundo fluxo de respec.
- **AC07:** mouse e teclado são funcionais; gamepad fica fora do v1. Não há IDs internos visíveis; strings usam keys do `LocalizationService` quando existirem e fallback authored do SO.
- **AC08:** o cutover dá ao coordinator um único ownership de U/request, remove `SkillTreeGameplayPanelController` e `SkillTreeInputHandler`, proíbe a View de republicar o evento e atualiza o validador no mesmo delta.
- **AC09:** existe/reusa exatamente um EventSystem. Layout não corta conteúdo em 1280×720 e 1920×1080.
- **AC10:** abrir bloqueia movimento, ataque, slots e interação; fechar restaura todos. Ponto/compra/rank/respec/load atualizam a projeção por eventos, sem rebuild por frame.
- **AC11:** compra não autoequipa. A tela informa “comprada, não equipada”; equipamento pertence ao loadout seguinte.

Fora: equipar skills, HUD de slots, pixel art final e animações de ação.

# /speckit.plan

Reusar `SkillTreePanel`, `ModalBase`, `ModalManager`, `GameplayInputRouter`, `SkillNodePurchaseFlow`,
`SkillTreeManager`, registries e eventos. Refazer `SkillTreeMenuViewModel` como projeção imutável construída
do domínio. Criar coordinator/builder e layout de grafo na camada UI; não gravar coordenadas no save nem no
runtime state. Confirmação é overlay interno, pois o modal stack não aceita modal aninhado. Materializar o Canvas
via prefab/generator Editor, remover o autoassign e trocar o installer de forma atômica.

Arquivos principais:

- `Assets/_Game/Scripts/UI/Skills/SkillTreePanel.cs`
- `Assets/_Game/Scripts/UI/Skills/SkillTreeMenuViewModel.cs`
- `Assets/_Game/Scripts/UI/Runtime/SkillNodePurchaseFlow.cs`
- `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSkillTreeRuntimeBinding.cs`
- novos builder/coordinator/layout em `Assets/_Game/Scripts/UI/Skills/`
- prefab/assets gerados em `Assets/_Game/Prefabs/UI/Skills/`

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | Criar projeção imutável e layout determinístico do grafo | AC02–AC04 |
| [ ] | T02 | Implementar Canvas, detalhe, confirmação, rank e capstone | AC01–AC07, AC10 |
| [ ] | T03 | Fazer cutover atômico, EventSystem, modal/input e remover autoassign | AC01, AC08–AC11 |
| [ ] | T04 | Gerar prefab e atualizar validador | AC08–AC09 |
| [ ] | T05 | EditMode, PlayMode e capturas nas três cenas/resoluções | AC01–AC11 |
