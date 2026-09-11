# Skills — fase UI 2: loadout e HUD de habilidades

> **Spec ID:** spec_skills_23_skill_loadout_hud_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+UI / Skills+Presentation / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_22_skill_tree_canvas_v1  
> **Blocks:** spec_skills_24_ui_pixel_art_v1  
> **Validation:** EditMode+PlayMode+capturas gráficas

# /speckit.specify

## Spec

Criar uma tela dedicada para equipar habilidades e uma faixa HUD real para os quatro slots canônicos 1–4.

- **AC01:** a lista inclui apenas ações compradas e exibe nome, rank, custo, cooldown, requisito e readiness authored Operational/Dormant com reason key; `EffectPending` pertence a passivas e não é inferido pela falta de arte.
- **AC02:** selecionar, substituir, mover e limpar slots 1–4 funciona por mouse e teclado. Replace exige confirmação; mover para ocupado oferece swap ou cancel. Falha deixa ambos intactos e publica uma única atualização coerente.
- **AC03:** duplicatas são permitidas para preservar liberdade de loadout, mas recebem aviso visual e compartilham o cooldown por `actionId`, inclusive após troca de slot.
- **AC04:** loadout é o modo/tab “Equipar” do modal U criado na fase 22, com foco e retorno próprios e sem segundo modal. Ele é a autoridade explícita de equipamento.
- **AC05:** a HUD mostra quatro slots reais com tecla, ícone/placeholder, nome curto, rank e custo; projeta Empty, Ready, Cooldown, NoMP, NoStamina, Dormant, InvalidContext e InvalidTarget.
- **AC05a:** prioridade visual determinística: Empty encerra; Dormant precede contexto/alvo; Cooldown precede recurso; InvalidContext precede InvalidTarget; NoMP/NoStamina seguem o recurso autorado; Ready é fallback. Overlays secundários não substituem o motivo primário.
- **AC06:** cooldown atualiza o fill sem reconstruir coleções por frame. Alterações de slot usam eventos e a View não remove listeners que não possui.
- **AC07:** save/reload preserva slots. Cooldown compartilhado persiste por `actionId` durante a sessão e não é adicionado ao save nesta fase; limpar/substituir/swap é atômico.
- **AC08:** HUD e modal funcionam nas três cenas, respeitam `HudVisibilityChangedEvent` e não duplicam Canvas/EventSystem. A faixa é materializada no Canvas visível; `AttachViews()` não cria uma cópia headless.
- **AC09:** reconciliar documentação legada de cinco slots para quatro slots canônicos.

Fora: pixel art final, gamepad e animações de ação.

# /speckit.plan

Reusar `TryAssignActiveSlot`, `TryClearActiveSlot`, eventos de slot, `SkillTreeState`,
`ActiveSkillExecutionController`, `ActiveSkillSlotProjection`, `GameplayHudViewModel` e modal stack. Consolidar
o ViewModel duplicado de SkillTree no contrato de loadout/HUD. Adicionar uma transação pura para replace/swap.
Materializar tela e faixa no Canvas vivo. Somente o fill de cooldown pode atualizar por frame; estados de recurso,
contexto e alvo vêm de uma projeção pura. Nome curto usa truncamento visual com tooltip do nome authored completo.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [ ] | T01 | Criar projection/builder de catálogo equipado e readiness | AC01, AC03, AC05 |
| [ ] | T02 | Implementar modal de loadout e transações de slot | AC02–AC04, AC07 |
| [ ] | T03 | Materializar faixa HUD event-driven | AC05–AC06, AC08 |
| [ ] | T04 | Validar ausência de autoassign e consolidar contratos duplicados | AC04, AC06 |
| [ ] | T05 | Atualizar docs, testes e capturas nas três cenas/resoluções | AC07–AC09 |
