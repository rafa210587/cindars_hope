# Cindar's Hope — Project Log

> Fonte operacional curta de continuidade do projeto.  
> Histórico completo preservado em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.

---

## 1. Handoff atual

### Estado real validado

- Repositório: `rafa210587/cindars_hope`.
- Branch de trabalho: `dev`.
- Branch default do GitHub: `main`.
- A `dev` contém MVPs de Farm, Town, Crafting, Save/Load, Cave/Combat básico, HUD debug, transições Farm/Town/Cave e docs/specs da FASE9E/FASE9F.
- `PROJECT_LOG.md` foi reduzido para handoff operacional curto.
- O histórico completo anterior foi arquivado sem perda intencional em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.

### Specs recentes aprovadas

- `docs/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`
- `docs/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`
- `docs/FUTURE_IDEAS_TODO_v1.0.md`
- `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`
- `docs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md`

### Próximo passo recomendado

Começar **FASE9F — Cave procedural/resources** com PRs pequenos:

1. PR-170 — Cave procedural contracts.
2. PR-171 — Cave procedural generator MVP.
3. PR-172 — Cave run regeneration on player defeat/KO.
4. PR-173 — Cave checkpoints a cada 15 níveis.
5. PR-174 — ResourceNode contracts com ferramenta/tier/stamina/fallback.

---

## 2. Protocolo obrigatório para agentes

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `AGENTS.md` e/ou `CLAUDE.md`.
3. Ler os documentos de referência do PR/tarefa.
4. Confirmar branch atual e escopo permitido.
5. Validar estado real no GitHub/repo antes de planejar.

Durante a tarefa:

1. Manter escopo pequeno.
2. Não implementar V2/FULL quando o PR é MVP.
3. Não alterar docs de design sem pedido explícito.
4. Não mexer em arquivos fora da lista permitida do PR.
5. Registrar dúvidas/desvios em vez de decidir silenciosamente.

Ao final de tarefa relevante:

1. Atualizar `PROJECT_LOG.md` com nova entrada curta.
2. Informar arquivos alterados.
3. Informar testes executados ou não executados.
4. Informar pendências, riscos e próximo passo recomendado.
5. Se a entrada ficar grande demais, criar novo archive em `docs/logs/` e manter este arquivo curto.

---

## 3. Estado consolidado curto

### Implementado no repo

- Core `GameEventBus` e eventos base.
- Data contracts e registries por ID.
- Farm MVP: plots, seeds, plantio, crescimento, colheita.
- Economy/Hunger/HUD debug.
- Save/load JSON local com cena atual e rebind cross-scene.
- World activities: árvores, pesca básica, pickups persistentes.
- Crafting MVP: receita de madeira processada e crafting point.
- Town MVP: portal Farm/Town, NPC Pip, compra/venda básica.
- Cave/Combat MVP: CaveScene, portal Farm/Cave, Slime, melee punch, chase, contact damage, drops, hit flash, knockback.
- Scene generators: FarmScene, TownScene, CaveScene.
- Validators de dados/cenas MVP.

### Especificado para próximas waves

- UI/Hotbar/Inventory/Equipment.
- Damage/Elementos/Status/Fórmula única.
- Item Taxonomy/IDs.
- Save Schema/Migration.
- Player Level Up/Progression.
- Cave/Resources/Encounters/Procedural progression.

---

## 4. Decisões FASE9F Cave

- Primeira entrada começa em `CaveLevel = 1`.
- Checkpoints permanentes a cada 15 níveis: `1, 15, 30, 45, 60, 75, 90`.
- Jogador pode escolher qualquer checkpoint liberado ao entrar na caverna.
- Cave usa `CaveWorldSeed` persistente e `CaveRunSeed` por run.
- Ao sofrer KO/derrota, a cave run é regenerada com nova `CaveRunSeed`; checkpoints permanecem.
- Cada nível deve ser grande, labiríntico e explorável.
- ResourceNode exige ferramenta correta, tier mínimo e consome stamina.
- Minério exige Pickaxe; sem pickaxe/tier suficiente, fallback gera `1x item_material_stone`, não entrega minério principal e não depleta node.
- Renovação diária só reseta nodes com `RespawnsDaily = true`.
- Baús ficam depois de nodes + enemies.
- Slime especial deve ter cor/visual diferente.
- Toda mudança de bioma tem boss poderoso e difícil.
- Recursos variam por nível/faixa; árvores subterrâneas podem dar madeiras melhores que exigem refinamento.

---

## 5. Checklist pendente

### Validação Unity geral

- [ ] Rodar `CindarsHope/Validate/Validate MVP Data`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP FarmScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP TownScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- [ ] Rodar validators de Farm/Town/Cave quando disponíveis.
- [ ] Testar Play Mode completo Farm → Town → Cave → Farm.
- [ ] Testar save/load em FarmScene.
- [ ] Testar save/load em TownScene.
- [ ] Testar save/load em CaveScene.
- [ ] Confirmar Console sem erro vermelho.

### Validação FASE9F futura

- [ ] CaveLevel 1 gera layout procedural grande.
- [ ] Cave run muda após KO/derrota.
- [ ] Checkpoints permanecem após KO/derrota.
- [ ] ResourceNode consome stamina.
- [ ] ResourceNode valida ferramenta/tier.
- [ ] Fallback sem pickaxe retorna apenas `1x item_material_stone`.
- [ ] Nodes `RespawnsDaily = true` renovam no novo dia.
- [ ] Slime especial tem cor diferente.
- [ ] Boss de bioma bloqueia avanço.

---

## 6. Histórico arquivado

O histórico completo antigo do `PROJECT_LOG.md` foi arquivado em:

```text
docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md
```

Esse arquivo preserva o log operacional anterior inteiro antes da redução do log raiz.

---

## 7. Log de atividades recente

## 2026-05-20 — Split operacional do PROJECT_LOG

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** reduzir `PROJECT_LOG.md` para handoff operacional curto e arquivar histórico completo.

### Alterações

- Criado archive completo em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md` reaproveitando o blob exato do `PROJECT_LOG.md` anterior.
- Substituído `PROJECT_LOG.md` por versão operacional curta.
- Mantidos links para specs FASE9E e FASE9F.
- Próximo passo recomendado atualizado para PR-170 da FASE9F.

### Testes

- [x] Archive criado a partir do blob antigo do `PROJECT_LOG.md`.
- [x] Novo `PROJECT_LOG.md` mantém handoff, decisões e próximos passos.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Validar no GitHub se o archive aparece corretamente em `docs/logs/`.
- Próximas entradas devem ser curtas; logs extensos devem ir para novos archives.

### Próximo passo recomendado

- Iniciar PR-170 — Cave procedural contracts.
