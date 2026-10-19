# Implementation Plan - Specs 06 a 10

## Status Atual
- ✅ Specs 02-05: Completas
- 🔨 Spec 06: Fundação 80% (faltam integrações de cenas)
- ❌ Specs 07-10: Não iniciadas

---

## SPEC 06 - Economy Shop Stock Pricing UI

### Implementado:
- ✅ ShopManager com stock finito e restock diário
- ✅ Modais (DialogueModal, ShopMenuModal, ModalManager)
- ✅ BuyPanel e SellPanel com transações atômicas
- ✅ NpcShopController para orquestração
- ✅ SaveManager integrado
- ✅ Validação e testes

### Faltando:
- [ ] Executar CreateShopTestAssets script para gerar ativos
- [ ] Executar CreateNpcPrefabs script para gerar NPCs
- [ ] Integrar NPCs em TownScene
- [ ] Testar fluxo completo em Play Mode
- [ ] Remover/deprecar farm buy/sell
- [ ] Validação final Unity ✓ (em progresso...)

**Tempo estimado:** 2-3 horas

---

## SPEC 07 - NPCs Towns Housing

### Escopo:
- Expandir sistema de NPCs com rotinas/agenda
- Dialogos expandidos por NPC
- Sistema de housing/hospedagem
- Pip expandido com mais comportamento

### Bloqueadores:
- Spec 06 deve estar 100% completa

**Tempo estimado:** 4-6 horas

---

## SPEC 08 - Quests Dialogue Trees

### Escopo:
- Sistema de quests MVP
- Quest givers com dialogue trees
- Validação de quest conditions
- Recompensas (XP, items, gold)

### Bloqueadores:
- Specs 06-07 devem estar completas

**Tempo estimado:** 6-8 horas

---

## SPEC 09 - Stamina System

### Escopo:
- Stamina bar do player
- Ações que consomem stamina (tool use, combat)
- Regeneração em descanso
- Alerta visual quando baixo

### Bloqueadores:
- Specs 02-06 devem estar completas

**Tempo estimado:** 3-4 horas

---

## SPEC 10 - Equipment Durability Damage Combat

### Escopo:
- Sistema de durabilidade expandido
- Dano escalável por weapon/armor
- Combat MVP com inimigos
- Loot baseado em defeitas

### Bloqueadores:
- Specs 02-09 devem estar completas

**Tempo estimado:** 6-8 horas

---

## Próximas Ações (Imediatas)

1. ✅ Validar Spec 06 com Unity (em progresso)
2. ✅ Corrigir erros encontrados
3. ⏳ Executar scripts de geração de ativos (após validação passar)
4. ⏳ Testar em Play Mode
5. ⏳ Commit final Spec 06
6. ⏳ Iniciar Spec 07

---

## Checklist de Validação por Spec

### Validação Obrigatória (CLAUDE.md):
- [ ] `dotnet build .\Assembly-CSharp.csproj` → 0 erros
- [ ] `RunUnityCompileValidation.ps1` → sem erros CS
- [ ] `ScanUnityLogs.ps1` → nenhum erro crítico
- [ ] Play Mode manual → fluxo funciona

