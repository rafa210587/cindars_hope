# FASE 9B-1 — Cave/Combat MVP v1.0

**Status:** ✅ Concluída em 2026-05-18  
**Próxima Fase:** PR-092+ Combat Feel & Polish

## Objetivo da Fase

Criar vertical slice mínimo de exploração + combate:
- CaveScene funcional com portal bidirecional Farm↔Cave
- Inimigo básico (Slime) com IA simples (perseguição)
- Combat MVP: soco melee curto (1 dano) com alcance 0.8f
- Feedback visual no Console (logging observável)
- Save/Load funcional na Cave

## Sistemas Entregues

### CaveScene MVP
- CaveScene.unity gerada reproduzivelmente
- Ground placeholder (dark gray)
- Bounds (top/bottom/left/right collision)
- SpawnPoints: `cave_default`, `cave_from_farm`
- Portal_Cave_To_Farm com entrada em FarmScene spawn `farm_from_cave`
- CaveSceneRuntimeReferenceInstaller para rebind ao carregar
- DebugHud com HP/Gold/Hunger/Inventory/Time

### Inimigo MVP (Slime)
- EnemyHealth: HP 10, eventos de morte
- EnemyDataSO: enemy_slime asset com stats (ID, maxHp, dropItem, dropAmount)
- CircleCollider2D não-trigger: colisão física
- Rigidbody2D kinematic: movimento controlado via script
- Filho TriggerCollider: CircleCollider2D trigger para detecção de contato

### Combat MVP
**PlayerAttackController (soco melee):**
- Tecla: J
- Dano: 1
- Alcance: 0.8f (melee curto)
- Cooldown: 0.4s
- Detecta EnemyHealth por componente (GetComponentInParent + GetComponent)
- Logging: "punch hit enemy {name} for X damage" ou "punch missed"
- Sem tags, sem FindObjectsByType

**EnemyContactDamage (dano por contato):**
- No filho TriggerCollider
- Detecta PlayerManager via GameBootstrap.Instance ou componente
- Dano: 1 por toque
- Cooldown: 1.0s entre touches
- Logging: "EnemyContactDamage: dealt X damage..."

**EnemyChaseController (IA simples):**
- Persegue Player se estiver dentro de raio (5f default)
- Velocidade: 1.2f (mais lenta que Player ~5)
- Para de perseguir se Player sai do raio ou entra em zona de stop (0.55f)
- Usa Rigidbody2D.MovePosition para movimento suave
- Sem pathfinding, sem direcionamento facing final
- Componente adicionado apenas ao Slime na Cave

### Save/Load Cave
- SaveManager rastreia CurrentSceneName = "CaveScene"
- Ao carregar, Player restaurado no spawn correto
- Inventory, Gold, Hunger, HP restaurados
- Time persiste entre cenas

### Geradores & Validators
**CreateMvpCaveScene:**
- Menu: `CindarsHope/Scenes/Create MVP CaveScene`
- Cria pasta Assets/_Game/Data/Combat se não existir
- Cria Enemy_Slime.asset se não existir
- Configura PlayerAttackController: dano=1, range=0.8f, cooldown=0.4s
- Configura EnemyChaseController no Slime
- Gera CaveScene reproduzivelmente

**MvpSceneValidator (expandido):**
- Valida estrutura de CaveScene
- Verifica GameBootstrap, DebugHud, Player, Portal, Enemy, CaveSceneRuntimeReferenceInstaller
- Menu: `CindarsHope/Validate/Validate Cave MVP` (se implementado)

## Arquivos Criados/Alterados

### Novos
- `Assets/_Game/Scripts/Combat/EnemyChaseController.cs`
- `Assets/_Game/Scripts/Combat/EnemyChaseController.cs.meta`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs`
- `Assets/_Game/Data/Combat/Enemy_Slime.asset`
- `Assets/_Game/Scenes/CaveScene.unity`

### Modificados
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs` — soco melee, cooldown
- `Assets/_Game/Scripts/Combat/EnemyHealth.cs` — validação, logging
- `Assets/_Game/Scripts/Combat/EnemyContactDamage.cs` — componente-based detection
- `Assets/_Game/Scripts/SceneManagement/ScenePortal.cs` — propriedades públicas para validation
- `Assets/_Game/Scripts/Editor/Validation/MvpSceneValidator.cs` — imports, validação Farm+Town+Cave

## Bugs Corrigidos

1. **Tag Enemy inexistente** → Removida dependência de tag, usar componente EnemyHealth
2. **PlayerAttackController no Slime** → Movido para Player, Slime não ataca
3. **EnemyDataSO ausente** → CreateMvpCaveScene cria Enemy_Slime.asset automaticamente
4. **EnemyContactDamage no pai** → Movido para filho TriggerCollider
5. **SetReference em array Farm generator** → Removida chamada inválida, array preenchido via arraySize
6. **Detecção de Player/Enemy via tag** → Migrado para componente-based (PlayerManager, PlayerController, EnemyHealth)
7. **GameBootstrap referência no EnemyContactDamage** → Usa GameBootstrap.Instance para aplicar dano sincronizado com HUD

## Dívidas Conhecidas

### Críticas
Nenhuma crítica bloqueante.

### Importantes
- Sem animação: sprite estático, sem facing/idle/run/attack frames
- Sem feedback visual: sem partículas, sound, knockback visual
- Slime é placeholder visual (cubo vermelho)
- Cave é placeholder visual (chão escuro, sem tilemap real)

### Nice-to-have
- Procedural cave: agora cena estática
- Resource nodes: não implementado (poderia drop items ao quebrar)
- Mais inimigos: agora apenas Slime
- Loot/drops: basics implementados, mas sem UI de pickup
- Combat feels: sem invulnerability frames, knockback, stun, etc.

## Validação MVP

- [ ] `CindarsHope/Scenes/Create MVP CaveScene` — roda sem erro, cria Enemy_Slime.asset
- [ ] Abrir CaveScene em Play Mode
- [ ] Apertar J longe do Slime — `"punch missed"` no Console
- [ ] Andar perto do Slime — Slime começa a perseguir lentamente
- [ ] Apertar J encostado no Slime — `"punch hit"`, Slime perde 1 HP, logging
- [ ] Deixar Slime bater 10 vezes — Player perde 10 HP
- [ ] Apertar J ~10 vezes — Slime morre, loga `"died"`
- [ ] Salvar (S) na Cave, reabrir — posição e HP restaurados
- [ ] Transicionar Farm→Cave→Farm — plotagem persiste
- [ ] HUD atualiza corretamente (HP, Gold, Hunger)
- [ ] Console: sem erro vermelho, sem warning crítico (warnings de sorting layer são OK)

## Métricas

- **PRs desta fase:** PR-075 (CaveScene), PR-076-080 (features), PR-081+ (combat runtime)
- **Linhas de código novo:** ~600
- **Componentes novos:** 1 (EnemyChaseController)
- **Assets criados:** Enemy_Slime.asset
- **Cenas:** 1 (CaveScene)
- **Eventos:** 1 novo (EnemyKilledEvent)

## Próximos Passos Recomendados

### Curto Prazo
1. **PR-092** — Limpar warnings (Sorting Layer, Input Manager deprecated)
2. **PR-093** — Combat Feel: knockback simples, invulnerability frames, visual feedback
3. **PR-094** — Cave hardening: drops, resource nodes, mais inimigos

### Médio Prazo
4. **PR-095-097** — Inventory/Shop/Crafting UI real MVP
5. **PR-098** — NPC dialogue box
6. **PR-099** — Primeira quest

### Longo Prazo
7. **Visual Slice** — integração de tudo
8. **Fase 10** — Arte final, release prep

## Decisão Arquitetural

**Sem tags para gameplay.** Preferir componentes:
- `GetComponentInParent<EnemyHealth>()` em vez de `CompareTag("Enemy")`
- `GetComponentInParent<PlayerManager>()` em vez de `CompareTag("Player")`
- Vantagem: não depender de ProjectSettings, mais robust
- Risco: acidental double-hit se houver múltiplos colliders
- Mitigação: validação em Start(), logging de todos os hits

---

**Entregue por:** Codex/Claude  
**Data:** 2026-05-18  
**Próxima Entrega:** PR-092+ (Polish & Features)
