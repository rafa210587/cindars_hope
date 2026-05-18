# Next Waves Roadmap v1.0

**Escrita em:** 2026-05-18  
**Baseada em:** FASE 9B-1 Cave/Combat MVP ✅

## Decisão Estratégica

Após FASE 9B-1, decidimos **NÃO avançar diretamente para arte final**, mas sim:

1. **Consolidar MVP** — limpar warnings, hardening técnico
2. **Combat Feel** — melhorar sensação de combate (feedback, knockback, FX)
3. **UI Real MVP** — Inventory/Shop/Crafting visual (não apenas debug HUD)
4. **Conteúdo** — primeira quest, NPC dialogue
5. **Visual Slice** — integração de tudo
6. **Fase 10** — arte final, polish, release

### Justificativa

- **Riscos técnicos ainda abertos:** warnings de Sorting Layer, Input Manager deprecated
- **Gameplay incompleto:** sem UI real, combate sem feeling, sem quest/progression
- **Arte desalinhada:** antes de gastar tempo com sprites finais, validar que mecaniquinha funciona
- **Próxima arte:** após validar loop completo, fazer art pass com escopo claro

---

## Sequência Recomendada

### Wave PR-092 — Limpar Warnings

**Objetivo:** zerar warnings críticos, tornar ambiente estável

**Tarefas:**
- [ ] Sorting Layer warnings: adicioná-las a ProjectSettings ou remover warnings nos geradores
- [ ] Input Manager deprecated: preparar Input System (sem implementar completamente)
- [ ] SceneSpawnPoint warnings: ajustar geradores para não criar warnings
- [ ] Revisar Console da Cave/Farm/Town em Play Mode

**Arquivos:**
- ProjectSettings/TagManager.asset (layers)
- Geradores (CreateMvpFarmScene, CreateMvpTownScene, CreateMvpCaveScene)
- PlayerController.cs, PlayerAttackController.cs (Input)

**Estimativa:** 1-2 PRs

---

### Wave PR-093 — Combat Feel MVP

**Objetivo:** soco melee + dano por contato sinta-se real (feedback, knockback simples)

**Tarefas:**
- [ ] Knockback simples: Push player away 0.5f ao receber dano (1 frame)
- [ ] Invulnerability frames: 0.5s após tomar dano (sem receber dano extra)
- [ ] Visual feedback: cor flash no Player/Slime ao receber dano
- [ ] SFX placeholder: log detalhado de hit/miss/death (Visual studio para som depois)
- [ ] Soco feedback: screen shake 0.1f, knockback 0.3f no Slime

**Novos Componentes:**
- HitFlashController: cor flash ao tomar dano
- KnockbackController: aplica knockback simples via Rigidbody2D

**Arquivos:**
- PlayerAttackController.cs — adicionar knockback ao inimigo
- EnemyContactDamage.cs — adicionar knockback ao player
- EnemyHealth.cs — HitFlash ao tomar dano
- PlayerManager.cs — invulnerability frames

**Estimativa:** 2-3 PRs

---

### Wave PR-094 — Cave Hardening

**Objetivo:** Cave funcional com drops, resource nodes, mais inimigos

**Tarefas:**
- [ ] Drops: Enemy_Slime droppa item_wood ao morrer (usar EnemyKilledEvent)
- [ ] Resource Nodes: rochas que podem ser coletadas (similar a TreeNode)
- [ ] Mais inimigos: adicionar Bat ou Rat
- [ ] Spawn multiplayer: CaveScene gera 2-3 Slimes
- [ ] Coleta de drops: Player caminha sobre item, input E para pegar

**Arquivos:**
- CreateMvpCaveScene.cs — gerar 2-3 Slimes, adicionar resource nodes
- ResourceNode.cs (novo, similar TreeNode)
- CreateMvpCaveScene.cs — configurar drops (já funciona via EnemyKilledEvent)
- PlayerPickupController (novo) — pickup via E

**Estimativa:** 2-3 PRs

---

### Wave PR-095 — Inventory UI Real MVP

**Objetivo:** UI visual real para inventário (não HUD debug)

**Tarefas:**
- [ ] Canvas/Inventory Panel: mostra items com ícones
- [ ] Grid layout: 4x5 slots, drag-drop simples (ou click-swap)
- [ ] Item quantity: stacks aparecem com número
- [ ] Hotkey I para toggle Inventory
- [ ] Compatibilidade cross-scene: salva estado da UI (aberta/fechada)

**Componentes:**
- InventoryUI (novo)
- InventorySlotUI (novo)
- InventoryInputHandler (novo)

**Estimativa:** 2-3 PRs

---

### Wave PR-096 — Shop UI Real MVP

**Objetivo:** UI visual real para compra/venda (não debug HUD)

**Tarefas:**
- [ ] Shop Panel: lista de items à venda com preço e ícone
- [ ] Sell All Panel: lista de items do player com preço de venda
- [ ] Buy: click item → confirmar quantidade → dar ouro, receber item
- [ ] Sell All: lista com quantidade, click vender tudo → ouro
- [ ] Pip visual: NPC com sprite, caixa de diálogo "Olá, quer comprar?"

**Componentes:**
- ShopUI (novo)
- SellUI (novo)
- BuyItemPointUI (novo)
- SellAllPointUI (novo)

**Estimativa:** 2-3 PRs

---

### Wave PR-097 — Crafting UI Real MVP

**Objetivo:** UI visual real para crafting (não debug HUD)

**Tarefas:**
- [ ] Crafting Panel: lista de receitas disponíveis com ingredientes e output
- [ ] Craft button: click recipe → decontar ingredientes → dar output
- [ ] Feedback: "Crafted successfully!" ou "Not enough materials"
- [ ] Hotkey K para toggle Crafting (no CraftingPoint)

**Componentes:**
- CraftingUI (novo)
- RecipeSlotUI (novo)

**Estimativa:** 1-2 PRs

---

### Wave PR-098 — NPC Dialogue MVP

**Objetivo:** caixa de diálogo simples NPC

**Tarefas:**
- [ ] Dialogue Box: texto em caixa preta, space para avançar
- [ ] NPC dialogue: ao interagir com NPC (E), mostra diálogo
- [ ] Multi-turn: Pip "Oi!" → "Quer comprar?" (simples linear)
- [ ] End dialogue: escape ou clique fora

**Componentes:**
- DialogueBoxUI (novo)
- DialogueController (novo)
- NpcTalkPoint.cs — integrar com DialogueController

**Estimativa:** 1-2 PRs

---

### Wave PR-099 — Primeira Quest MVP

**Objetivo:** quest linear simples Farm→Town→Cave

**Tarefas:**
- [ ] Quest system: rastreamento de estado (accepted, completed)
- [ ] Quest 1: "Trazer sementes de trigo do Pip para a Farm"
  - Falar com Pip: aceita quest
  - Comprar sementes: check
  - Plantar: check
  - Voltar para Pip: completa quest
- [ ] Reward: pequeno bônus (100 ouro)
- [ ] HUD: mostra quest atual

**Componentes:**
- QuestManager (novo)
- QuestUI (novo)
- Quest data assets

**Estimativa:** 2-3 PRs

---

### Milestone — Visual Slice

Após PR-099, temos:
- Integração completa (Farm→Town→Cave)
- UI real MVP
- Combate com feel
- Primeira quest
- Save/load funcional

**Apresentação possível:**
- Jogar início ao fim
- Completo visual (não apenas placeholder)
- Audio ainda placeholder
- Arte sprites ainda placeholder

**Estimativa:** 1 PR de integração + testes

---

### Fase 10 — Art Pass & Release

**Objetivos:**
- Sprites finais (todas as cenas, inimigos, items, UI)
- Audio (música, SFX)
- Otimização
- Release build

**Não está no escopo deste roadmap** — planejado separadamente após validação de gameplay.

---

## Critérios para Começar Art Pass

- [ ] Farm/Town/Cave estáveis (sem crashes, sem warnings críticos)
- [ ] UI mínima real implementada (não debug HUD)
- [ ] Combat loop validado (hit → feedback → loot → salvar)
- [ ] Primeira quest rodiço (de ponta a ponta, sem bugs)
- [ ] Lista final de sprites/assets fechada (não mudar mid-art)
- [ ] Performance OK (60 FPS em cenas padrão)

---

## Estimativa Total

| Wave | PRs | Semanas | Status |
|------|-----|---------|--------|
| PR-092 Warnings | 1-2 | 0.5 | 📋 Planejado |
| PR-093 Combat Feel | 2-3 | 1 | 📋 Planejado |
| PR-094 Cave Hardening | 2-3 | 1 | 📋 Planejado |
| PR-095 Inventory UI | 2-3 | 1 | 📋 Planejado |
| PR-096 Shop UI | 2-3 | 1 | 📋 Planejado |
| PR-097 Crafting UI | 1-2 | 0.5 | 📋 Planejado |
| PR-098 NPC Dialogue | 1-2 | 0.5 | 📋 Planejado |
| PR-099 First Quest | 2-3 | 1 | 📋 Planejado |
| Visual Slice | 1 | 0.5 | 📋 Planejado |
| **Total** | **~17 PRs** | **~7 weeks** | |

---

## Riscos & Mitigação

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| UI design não validado | Alto | Mockup/prototype antes de código |
| Input System migration | Médio | Preparar em PR-092, implementar gradualmente |
| Cross-scene UI state | Médio | Test save/load com UIs abertas |
| Asset naming conflict | Baixo | Naming convention clara, cleanup antes de art |

---

## Decisões Tomadas

1. **MVP first:** UI debug agora, visual depois
2. **Gameplay before art:** validar que funciona antes de gastar tempo em sprites
3. **No procedural cave yet:** cave estática por enquanto (procedural é Fase 11+)
4. **Quest system MVP:** linear simples, não branch/multiple choice (Fase 11+)
5. **Audio placeholder:** placeholder/silence, audio real em Fase 10

---

## Next Meeting

Agendar após PR-093 para validar combat feel e ajustar roadmap se necessário.

---

**Documento:** NEXT_WAVES_ROADMAP_v1.0.md  
**Criado:** 2026-05-18  
**Próxima Review:** 2026-06-01 (após PR-093)
