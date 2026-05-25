# SPEC — Bugfix UI/Input/Shop/Sell Bundle

**Status:** A implementar  
**Tipo:** Bugfix bundle (4 bugs independentes consolidados)  
**Data de criação:** 2026-05-25  
**Implementador esperado:** Claude  

---

## Ordem de execucao

**Executar após:** SPEC 09 (Hunger/Stamina), SPEC 10 (Equipment), SPEC 11 (Damage), SPEC 12 (Combat)  
**Ordem:** 13 (posterior a SPEC 12, anterior a SPEC 16 Skill Trees)  
**Sequência recomendada:** Bugfix A → Bugfix B → Bugfix C → Bugfix D

---

## Depende de

1. **GameBootstrap.ModalManager** (deve ter `HasActiveModal` property)
2. **ModalManager.PushModal/TryPopModal** (stacking de modais)
3. **SellableItemPolicy.IsSellable()** (validação de items vendáveis)
4. **ItemDataSO.BaseValue** (precificação)
5. **ShopManager.TryGetSession/TrySellItem/TryBuyItem** (transações)
6. **InventoryManager.Items** (slots de inventário)

---

## Bloqueia

1. **SPEC 16 (Skill Trees)** — Até que Bugfix A (modal blocking) esteja estável
2. **SPEC 17 (UI final consolidada)** — Este bugfix prepara terreno para refactor UI final

---

# /speckit.specify

---

## Objetivo

Resolver um pacote de 4 bugs encontrados em validação humana parcial de UI/input/shop/sell:

1. **Bugfix A**: Entrada (WASD) bloqueada quando modal está aberto
2. **Bugfix B**: Vendedores abrem loja sem itens para comprar
3. **Bugfix C**: Ao selecionar vender, itens do inventário não aparecem
4. **Bugfix D**: Algumas HUDs/modais ocupam espaço demais

Objetivo: Melhorar UX sem refazer UI final (que pertence a SPEC 17).

---

## Escopo

### Bugfix A — Input Lock Global de Modais

**Problema:**
Jogador continua andando com WASD quando inventário, diálogo, loja ou outro modal está aberto.

**Solução esperada:**
1. `PlayerController.ReadMoveInput()` retorna `Vector2.zero` quando input de mundo bloqueado.
2. Bloquear enquanto:
   - `ModalManager.HasActiveModal == true`, ou
   - `FarmPlot.IsAnyActionMenuOpen == true`
3. Preservar `LastFacingDirection` (não muda por WASD bloqueado).
4. Não publicar `PlayerStepEvent` por movimento zero.

**Critérios de aceite:**
- Abrir inventário (`I`): WASD não move personagem
- Conversar NPC: WASD não move personagem
- ShopMenu/Buy/Sell: WASD não move personagem
- `E`, `Space`, `Enter` dentro UI não interagem com mundo
- Fechar modal: movimento volta normalmente

### Bugfix B — Vendedores com Itens

**Problema:**
Alguns vendedores (ShopMenuModal → BuyPanel) mostram lista vazia, sem feedback claro.

**Solução esperada:**
1. Auditar cada `ShopDataSO` dos NPCs vendedores (TownScene):
   - Verificar se `ShopDataSO.Items` não está vazio
   - Verificar se cada item tem `ItemId` válido
   - Verificar se `BaseD ailyStock > 0`
   - Verificar se item existe no `ItemDatabaseSO`

2. Garantir wiring correto em `NpcShopController`:
   - `ShopDataSO` atribuído
   - `ShopManager`, `PlayerManager`, `InventoryManager` injetados
   - Diálogo/Shop modals ligados

3. Em `BuyPanel.PopulateItems()`:
   - Se lista vazia, mostrar feedback visível: `Sem itens disponíveis.`
   - Logar warning com diagnóstico claro

4. Criar/ajustar validator para validar:
   - `ShopDataSO` todos os items com ID válido
   - Stock > 0
   - Referências não-nulas

**Critérios de aceite:**
- Vendedor de sementes mostra itens
- Vendedor geral mostra itens
- Compra reduz ouro
- Stock reduz
- Lista vazia mostra mensagem clara

### Bugfix C — Venda Lista Itens

**Problema:**
`SellPanel.PopulateItems()` não lista itens do inventário, ou lista incompleta.

**Solução esperada:**
1. `SellPanel.PopulateItems()` itera `InventoryManager.Slots` (não apenas agregado).
2. Excluir:
   - Slot vazio
   - Item equipado
   - Item sem `ItemDataSO`
   - Item com `BaseValue <= 0`
   - Key item / Quest item
   - Ferramenta essencial
   - Item bloqueado por policy

3. Revisar `SellableItemPolicy`:
   - Regra padrão: `BaseValue > 0` e categoria permitida
   - Whitelist/blacklist apenas como override explícito
   - Garantir crops, fish, wood, materiais comuns aparecem

4. Após vender:
   - Remover quantidade correta
   - Atualizar ouro
   - Remover linha se stack = 0
   - Preservar bindings

5. Se sem itens vendáveis:
   - Mostrar mensagem: `Nenhum item vendável.`

**Critérios de aceite:**
- Trigo/cenoura/peixe aparecem para venda
- Materiais com `BaseValue > 0` aparecem
- Itens equipados não aparecem
- Vender 1 unidade reduz stack
- Vender tudo remove linha
- Lista vazia mostra mensagem

### Bugfix D — HUDs/Modais Compactas

**Problema:**
Algumas HUDs/modais ocupam espaço excessivo. Objetivo: compactar MVP.

**Solução esperada:**
1. `DialogueModal`:
   - Painel compacto (não full-screen)
   - Largura adequada para 1280x720
   - Altura baseada em texto
   - Wrap de texto

2. `ShopMenuModal`:
   - Painel contendo apenas Comprar/Vender/Sair
   - Tamanho baseado em opções
   - Highlight visível

3. `BuyPanel` / `SellPanel`:
   - Painel central compacto
   - Scroll se lista grande
   - Feedback visível

4. `FarmPlot` menu (se aplicável):
   - Janela pequena, perto do plot
   - Altura baseada em ações
   - Não cobre tela

**Não implementar:**
- Drag/drop
- UI final de inventário (SPEC 17)
- UI Toolkit
- Arte final

**Critérios de aceite:**
- Diálogo ocupa espaço necessário
- ShopMenu não full-screen
- Buy/Sell legíveis e compactos
- ESC/back funcionam

---

# /speckit.plan

## Fora de Escopo

- Skill tree / respec / unlock (SPEC 16)
- UI final consolidada (SPEC 17)
- Drag/drop, UI Toolkit, arte final
- Refazer inventário (SPEC 03 pendente UI)
- Refazer combate, cave, crafting

---

## Arquivos Permitidos

```
Assets/_Game/Scripts/Player/PlayerController.cs
Assets/_Game/Scripts/UI/Modal/ModalManager.cs
Assets/_Game/Scripts/Interaction/InteractionSystem.cs
Assets/_Game/Scripts/UI/Shop/BuyPanel.cs
Assets/_Game/Scripts/UI/Shop/SellPanel.cs
Assets/_Game/Scripts/UI/Shop/ShopMenuModal.cs
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/Economy/ShopManager.cs
Assets/_Game/Scripts/Economy/ShopDataSO.cs
Assets/_Game/Scripts/Economy/SellableItemPolicy.cs
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/InventorySlot.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Farm/FarmPlot.cs
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scenes/TownScene.unity
Assets/_Game/Scenes/FarmScene.unity
Assets/_Game/Data/Economy/
Assets/_Game/Data/Items/
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

---

## Arquivos Proibidos

```
docs_old/**
Packages/**
ProjectSettings/**
Assets/Plugins/** (exceto se MCP)
specs/ (raiz) — não recrie
spec/ (raiz) — não recrie
.git/**
```

---

## Critérios de Aceite Globais

### Bugfix A
- [ ] Abrir inventário (`I`): WASD não move
- [ ] Conversar com NPC: WASD não move
- [ ] Abrir ShopMenu: WASD não move
- [ ] `E`, `Space`, `Enter` não interagem com mundo enquanto UI ativa
- [ ] Fechar modal: movimento volta

### Bugfix B
- [ ] Todos vendedores (sementes, geral, metais) mostram itens
- [ ] ShopDataSO todos têm items válidos
- [ ] Feedback claro se lista vazia
- [ ] Compra funciona

### Bugfix C
- [ ] Trigo/cenoura/peixe/madeira aparecem
- [ ] Materiais com valor aparecem
- [ ] Itens equipados não aparecem
- [ ] Venda remove item corretamente
- [ ] Feedback se sem itens

### Bugfix D
- [ ] Diálogo não cobre tela
- [ ] ShopMenu não full-screen
- [ ] Buy/Sell compactos
- [ ] Scroll funciona
- [ ] Modais aceitáveis

---

# /speckit.tasks

---

## Validações Obrigatórias

Após implementar todos 4 bugs:

1. **Docs validation:**
   ```
   .\tools\docs\validate_docs.ps1
   ```

2. **Scope detection:**
   ```
   .\.claude\hooks\detect-change-scope.ps1
   ```

3. **Required validations:**
   ```
   .\.claude\hooks\run-required-validations.ps1
   ```

4. **Non-regression review:**
   ```
   /review-non-regression
   ```

5. **Editor validators** (se criar):
   - Validator de ShopDataSO
   - Validator de SellPanel
   - Validator de bugfix bundle

Se Unity batchmode não rodar, registrar:
- Comando tentado
- Erro
- Reason
- Residual risk
- Marcar Play Mode como NOT RUN

---

## Checklist Play Mode (Validação Humana Final)

Quando terminado, user (Rafa) testará:

- [ ] Abrir inventário com `I` e verificar WASD não move
- [ ] Conversar com NPC e verificar WASD não move
- [ ] Abrir loja e testar Comprar
- [ ] Abrir loja e testar Vender com itens
- [ ] Verificar se itens aparecem corretamente
- [ ] Verificar se HUDs estão compactas
- [ ] Fechar todos modais e confirmar movimento normal
- [ ] Testar feedback de lista vazia (inventário ou loja)
- [ ] Verificar se `E` não abre interação enquanto modal
- [ ] Testar venda de múltiplas unidades e stacks

---

## Riscos Residuais

- **Play Mode testing:** Não rodável em batchmode; deferred para humano
- **UI Layouts:** Ajustes visuais podem precisar refinement pós-validação
- **Edge cases:** Itens especiais (key, quest, equipado) podem ter interações complexas
- **Modal stack:** Comportamento em stacking múltiplo (Buy/Sell sobre Shop) não testado em batchmode

---

## Definição de Pronto

- [ ] Todos 4 bugs implementados
- [ ] Docs validation PASS
- [ ] Non-regression PASS or OK warning
- [ ] Spec movida para `implementados/`
- [ ] Relatório final criado
- [ ] PROJECT_LOG.md atualizado
- [ ] Checklist Play Mode entregue para user

---

## Definition of Done

Entregar:
1. Spec movida para `docs/specs/implementados/spec_bugfix_ui_input_shop_sell_bundle.md`
2. Relatório: `docs/validation/BUGFIX_UI_INPUT_SHOP_SELL_VALIDATION_YYYYMMDD.md`
3. PROJECT_LOG.md atualizado
4. Checklist Play Mode para validação humana final

Sem validação humana no meio. Usar apenas validações automáticas (docs, compile, non-regression).

---

**Criado:** 2026-05-25  
**Executor esperado:** Claude (automated, sem pausas)  
**Validação humana:** Apenas no final (Play Mode checklist)
