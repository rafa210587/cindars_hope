---
required_adrs: [ADR-0007-event-bus-gameplay-communication]
required_game_rules: [combat_rules.md, event_rules.md]
---

# SPEC 18 - Combat Movement, Projectiles, Spells and Melee Visuals Runtime

> Spec ID: spec_combat_movement_projectiles_melee_visuals_runtime
> Status: A implementar
> Ordem de execucao: 18
> Tipo: Combat/Runtime/Animation/VFX/Input/Data
> Fonte de refinamento: `docs/refinements/a_implementar/pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md`
> Depende de: SPEC 09, SPEC 10, SPEC 11, SPEC 12, SPEC 16 e SPEC 17/input-modal closeout quando aplicavel
> Fora de escopo: block/parry, heavy attack, combos complexos, lock-on, mouse aim, gamepad completo, AoE complexa, boss-specific attacks, arte final polida, economia completa de municao, craft de flechas, quiver e tipos avancados de flecha.

---

# /speckit.specify

## O QUE

Evoluir o combate runtime do jogador para que ataques e movimentos tenham comportamento visual e fisico claro:

1. Flechas disparam como projeteis visuais em 8 direcoes, com alcance 10 world units.
2. Magias ofensivas disparam como projeteis visuais em 8 direcoes.
3. Fireball existe para teste, aplica dano de fogo e status Burn quando suportado pelo sistema atual.
4. Ataques melee possuem fases de wind-up, active e recovery, com slash visual e hitbox.
5. Ataques melee nao travam o personagem, mas reduzem movimento em 30% durante o ataque.
6. Dash existe como skill ja comprada/liberada para teste, move 6 world units em velocidade 2x e para em colliders/inimigos.
7. Dodge e refinado para mover 3 world units em velocidade 3x e ganhar i-frame.
8. Personagem inicia com loadout minimo de teste: espada, arco, flechas, armaduras e item/spell de Fireball.
9. Modal/UI aberta bloqueia movimento, ataque, cast, dash e dodge.

## POR QUE

A base atual de combate ja existe, mas parte do combate ainda e abstrata/placeholder. O jogador precisa enxergar e sentir:

- flecha viajando;
- magia viajando;
- melee acertando com hitbox clara;
- dash ofensivo;
- dodge defensivo com invulnerabilidade curta;
- loadout pronto para testar sem configuracao manual pesada.

## ESCOPO

Inclui:

- runtime visual de projeteis;
- comportamento de projectile hit/expire;
- integracao de arco/flecha com projectile;
- integracao de spells projectile com SpellDataSO;
- Fireball e Burn;
- melee hitbox/timing/slash;
- dash skill liberada;
- dodge com i-frame;
- loadout inicial de teste;
- validator Editor;
- checklist Play Mode.

Nao inclui:

- mouse aim;
- lock-on;
- combos;
- heavy attack;
- block/parry;
- ammo economy completa;
- craft de flechas;
- magias de area;
- balanceamento final;
- arte final.

## DECISOES APROVADAS

| Tema | Decisao |
|---|---|
| Spec | 18 |
| Flecha range | 10 world units |
| Direcao flecha/magia | 8 direcoes, com diagonal normalizada |
| Mouse aim | Fora por enquanto |
| Dash | Skill, mas inicia comprada/liberada para teste |
| Dash distancia | 6 world units |
| Dash velocidade | MoveSpeed * 2.0 |
| Dash colisao | Para em parede/obstaculo/inimigo |
| Dash i-frame | Nao por default |
| Dodge distancia | 3 world units |
| Dodge velocidade | MoveSpeed * 3.0 |
| Dodge i-frame | Sim |
| Melee movimento | Reduz movimento em 30%; nao trava |
| Fireball | Deve existir para teste |
| Magia status | Aplica status quando configurada |
| Loadout inicial | Armas, arco, flechas, armaduras e Fireball |

---

# /speckit.clarify

## Perguntas resolvidas

### 1. Flecha deve manter range 6 ou aumentar?

Resolvido: range 10.

### 2. Dash e habilidade base ou skill?

Resolvido: e skill, mas deve iniciar comprada/liberada para teste.

### 3. Dodge deve ter i-frame?

Resolvido: sim, implementar i-frame agora.

### 4. Melee trava o jogador?

Resolvido: nao trava; reduz movimento em 30%.

### 5. Projeteis aceitam diagonal?

Resolvido: sim, 8 direcoes com diagonal normalizada.

### 6. Mouse aim entra agora?

Resolvido: nao.

### 7. Dash atravessa inimigos?

Resolvido: nao. Para em collider/inimigo.

### 8. Magias aplicam status?

Resolvido: sim, quando configuradas. Fireball deve aplicar Burn se StatusEffectManager suportar o efeito de forma segura.

### 9. Personagem inicia com itens de combate?

Resolvido: sim. Deve iniciar com espada, arco, flechas, armaduras e item/spell de Fireball.

## Regras para ambiguidades restantes

- Se ja existir sistema de unlock por item, usar para Fireball. Se nao existir, iniciar Fireball desbloqueada e manter `item_spell_tome_fireball` como hook.
- Se ja existir suporte limpo a ammo, consumir `item_arrow_basic`. Se nao existir, implementar consumo minimo seguro ou registrar como pendencia se ameaçar escopo.
- Se ActiveSkillSlots suportar preload seguro, equipar Dash no slot `R`. Se nao suportar, deixar Dash liberado por hotkey temporaria/testavel e registrar pendencia.
- Se StatusEffectManager nao tiver DoT seguro, aplicar Burn como status/marcador/evento e registrar DoT como pendencia. Nao criar manager paralelo.

---

# /speckit.plan

## Arquitetura alvo

```text
Player
├── PlayerAttackController
│   ├── dispara melee
│   ├── dispara ranged weapon projectile
│   └── preserva prioridade de E/interacao
├── PlayerSpellCaster
│   └── cria spell projectile visual
├── PlayerCombatMovementController
│   ├── Dash
│   └── Dodge + i-frame
├── PlayerCombatStateController
│   ├── Normal
│   ├── Attacking
│   ├── Casting
│   ├── Dashing
│   ├── Dodging
│   ├── Invulnerable
│   └── BlockedByModal/Dead/Stunned
└── MeleeAttackRuntime
    ├── Wind-up
    ├── Active hitbox
    └── Recovery

ProjectileBehaviour
├── Movement 8-direcoes
├── Range/lifetime
├── Target collision
├── Obstacle collision
├── DamageRequest
├── Optional StatusEffectId
└── Impact VFX
```

## Contratos existentes a preservar

- `DamageCalculator` continua fonte de calculo de dano.
- `DamageRequest` continua DTO de entrada de dano.
- `DamageType` continua enum de tipo de dano.
- `StatusEffectManager` continua dono de status.
- `Stamina` existente continua dono de custo de dodge/dash.
- `ManaManager`/spell casting existente continua dono de mana.
- `Equipment` existente continua dono de armas/armaduras.
- `InventoryManager` existente continua dono dos itens.
- `SkillTree`/`ActiveSkillSlots` existentes continuam donos de skill comprada/equipada.
- `GameEventBus` continua canal de eventos.

## Dados novos ou expandidos

### CombatProjectileVisualDataSO

Criar se fizer sentido no estado real do codigo.

Campos sugeridos:

```text
ProjectileId
ProjectilePrefab
DamageType
ProjectileSpeed
ProjectileRange
ProjectileRadius
ProjectileLifetime
ImpactVfxPrefab
StatusEffectId
CanPierce
MaxPierceTargets
```

### WeaponDataSO expandido

Adicionar campos sem quebrar dados existentes:

```text
UsesProjectile
ProjectileVisualData
ProjectileRangeOverride
AmmoItemId
AmmoConsumedPerShot
MeleeWindupSeconds
MeleeActiveSeconds
MeleeRecoverySeconds
MeleeMoveSpeedMultiplier = 0.70
MeleeHitboxSize
MeleeHitboxOffset
SlashVisualPrefab
```

### SpellDataSO expandido

Adicionar campos sem quebrar dados existentes:

```text
UsesProjectile
ProjectileVisualData
StatusEffectId
StatusApplyChance
```

### PlayerCombatMovementConfigSO

Criar config data-driven:

```text
DashDistance = 6.0
DashSpeedMultiplier = 2.0
DashStaminaCost = 30
DashCooldownSeconds = 1.2
DashHasIFrames = false

DodgeDistance = 3.0
DodgeSpeedMultiplier = 3.0
DodgeStaminaCost = 15
DodgeCooldownSeconds = 0.6
DodgeHasIFrames = true
DodgeIFrameDurationSeconds = 0.18
```

## Itens e dados iniciais

Usar ids existentes se ja existirem. Se nao existirem, criar assets/data equivalentes.

```text
item_weapon_training_sword
item_weapon_basic_bow
item_arrow_basic
item_armor_cloth_chest
item_armor_cloth_legs
item_armor_cloth_boots
item_spell_tome_fireball
spell_fireball
status_burn
skill_dash_forward
skill_action_dash_forward
```

## Loadout inicial esperado

New game deve iniciar com:

```text
item_weapon_training_sword x1
item_weapon_basic_bow x1
item_arrow_basic x99
item_armor_cloth_chest x1
item_armor_cloth_legs x1
item_armor_cloth_boots x1
item_spell_tome_fireball x1
spell_fireball desbloqueada ou liberavel
skill_dash_forward comprada/liberada
skill_action_dash_forward testavel, preferencialmente no slot R
```

## Eventos

Criar somente se nao existirem.

```csharp
public readonly struct PlayerDashStartedEvent
{
    public readonly string SourceId;
    public readonly float Distance;
    public readonly float SpeedMultiplier;
    public PlayerDashStartedEvent(string sourceId, float distance, float speedMultiplier)
    {
        SourceId = sourceId;
        Distance = distance;
        SpeedMultiplier = speedMultiplier;
    }
}

public readonly struct PlayerDashEndedEvent
{
    public readonly string SourceId;
    public readonly bool InterruptedByCollision;
    public PlayerDashEndedEvent(string sourceId, bool interruptedByCollision)
    {
        SourceId = sourceId;
        InterruptedByCollision = interruptedByCollision;
    }
}

public readonly struct PlayerInvulnerabilityStartedEvent
{
    public readonly string SourceId;
    public readonly float DurationSeconds;
    public PlayerInvulnerabilityStartedEvent(string sourceId, float durationSeconds)
    {
        SourceId = sourceId;
        DurationSeconds = durationSeconds;
    }
}

public readonly struct PlayerInvulnerabilityEndedEvent
{
    public readonly string SourceId;
    public PlayerInvulnerabilityEndedEvent(string sourceId) => SourceId = sourceId;
}

public readonly struct PlayerMeleeAttackStartedEvent
{
    public readonly string WeaponId;
    public readonly string HandSlot;
    public PlayerMeleeAttackStartedEvent(string weaponId, string handSlot)
    {
        WeaponId = weaponId;
        HandSlot = handSlot;
    }
}

public readonly struct PlayerMeleeAttackEndedEvent
{
    public readonly string WeaponId;
    public readonly string HandSlot;
    public PlayerMeleeAttackEndedEvent(string weaponId, string handSlot)
    {
        WeaponId = weaponId;
        HandSlot = handSlot;
    }
}

public readonly struct ProjectileHitEvent
{
    public readonly string SourceId;
    public readonly string ProjectileId;
    public readonly string TargetId;
    public ProjectileHitEvent(string sourceId, string projectileId, string targetId)
    {
        SourceId = sourceId;
        ProjectileId = projectileId;
        TargetId = targetId;
    }
}

public readonly struct ProjectileExpiredEvent
{
    public readonly string SourceId;
    public readonly string ProjectileId;
    public readonly string Reason;
    public ProjectileExpiredEvent(string sourceId, string projectileId, string reason)
    {
        SourceId = sourceId;
        ProjectileId = projectileId;
        Reason = reason;
    }
}
```

Payloads podem ser ajustados ao padrao real do projeto, mantendo tipos simples.

## Input

| Input | Acao |
|---|---|
| Q | ataque mao esquerda / arma conforme sistema atual |
| E | interacao se houver candidato; senao ataque mao direita conforme sistema atual |
| Space | dodge |
| R | preferencial para dash skill testavel, se ActiveSkillSlots permitir |
| Shift | hotkey auxiliar de dash apenas se necessario para teste e sem conflito |
| WASD/setas | movimento/direcao de ataque/projetil |

## Regras de input lock

- Modal aberto bloqueia WASD, Q, E combat, Space, R/T/Y/G, Shift e cast.
- Toast nao bloqueia gameplay.
- Confirmation dialog bloqueia tudo abaixo.
- Death/stun bloqueia tudo.
- Diagonal deve ser normalizada.

---

# /speckit.tasks

## [CODE] Projectile runtime

- [ ] **SPEC18-C01:** Revalidar `ProjectileBehaviour.cs` existente e adaptar sem quebrar contratos atuais.
- [ ] **SPEC18-C02:** Implementar movimento linear em 8 direcoes com vetor normalizado.
- [ ] **SPEC18-C03:** Implementar controle de range por distancia percorrida; flecha default = 10 world units.
- [ ] **SPEC18-C04:** Implementar lifetime seguro para evitar projectile infinito.
- [ ] **SPEC18-C05:** Implementar colisao com alvo valido e obstaculo.
- [ ] **SPEC18-C06:** Aplicar dano via `DamageRequest`/`DamageCalculator` existente.
- [ ] **SPEC18-C07:** Aplicar `StatusEffectId` quando projectile/spell configurar status.
- [ ] **SPEC18-C08:** Publicar `ProjectileHitEvent` e `ProjectileExpiredEvent` ou equivalentes existentes.
- [ ] **SPEC18-C09:** Rotacionar sprite/visual conforme uma das 8 direcoes.

## [CODE] Ranged weapon / flecha

- [ ] **SPEC18-C10:** Expandir `WeaponDataSO` ou criar subdata segura para projectile visual.
- [ ] **SPEC18-C11:** Configurar arco basico com projectile de flecha, range 10, dano fisico e velocidade data-driven.
- [ ] **SPEC18-C12:** Criar ou reutilizar `item_arrow_basic`.
- [ ] **SPEC18-C13:** Se seguro, consumir 1 `item_arrow_basic` por disparo. Se nao seguro, registrar pendencia sem bloquear projectile visual.
- [ ] **SPEC18-C14:** Falha de disparo sem flecha deve gerar feedback/evento simples, sem exception.

## [CODE] Spell projectile / Fireball

- [ ] **SPEC18-C15:** Expandir `SpellDataSO` ou subdata segura para projectile visual.
- [ ] **SPEC18-C16:** Criar/configurar `spell_fireball` com `DamageType.Fire`.
- [ ] **SPEC18-C17:** Criar/configurar `status_burn` usando `StatusEffectManager` existente.
- [ ] **SPEC18-C18:** Fireball deve aplicar Burn se status system suportar; se DoT nao for seguro, aplicar marcador/status e documentar pendencia.
- [ ] **SPEC18-C19:** Fireball deve consumir mana e respeitar cooldown.
- [ ] **SPEC18-C20:** Fireball deve viajar em 8 direcoes com projectile visual.
- [ ] **SPEC18-C21:** Criar ou reutilizar `item_spell_tome_fireball`; se unlock por item nao existir, iniciar Fireball desbloqueada e manter item como hook.

## [CODE] Melee runtime

- [ ] **SPEC18-C22:** Criar `MeleeAttackRuntime` ou adaptar controller existente para fases `WindUp`, `Active`, `Recovery`.
- [ ] **SPEC18-C23:** Criar `MeleeHitbox` com deteccao frontal conforme facing/input direction.
- [ ] **SPEC18-C24:** Garantir hit uma vez por alvo por ataque.
- [ ] **SPEC18-C25:** Reduzir movimento do player em 30% durante ataque: multiplier 0.70.
- [ ] **SPEC18-C26:** Exibir slash visual na direcao correta.
- [ ] **SPEC18-C27:** AttackSpeed deve continuar influenciando cooldown/duracao conforme contrato atual.
- [ ] **SPEC18-C28:** Preservar prioridade de interacao do E.

## [CODE] Dash

- [ ] **SPEC18-C29:** Criar `PlayerCombatMovementController` ou adaptar movement controller existente.
- [ ] **SPEC18-C30:** Implementar dash com distancia 6, velocidade MoveSpeed * 2.0 e direcao em 8 direcoes.
- [ ] **SPEC18-C31:** Dash consome stamina e respeita cooldown.
- [ ] **SPEC18-C32:** Dash para em parede/obstaculo/inimigo.
- [ ] **SPEC18-C33:** Dash nao causa dano por padrao.
- [ ] **SPEC18-C34:** Criar/configurar `skill_dash_forward` e `skill_action_dash_forward`.
- [ ] **SPEC18-C35:** Marcar dash como comprado/liberado em new game para teste.
- [ ] **SPEC18-C36:** Se seguro, equipar dash no ActiveSkillSlot `R`. Se nao, fornecer hotkey auxiliar documentada.
- [ ] **SPEC18-C37:** Publicar eventos de dash start/end.

## [CODE] Dodge + i-frame

- [ ] **SPEC18-C38:** Refinar dodge existente no Space.
- [ ] **SPEC18-C39:** Dodge move 3 world units em MoveSpeed * 3.0.
- [ ] **SPEC18-C40:** Dodge usa input direction; sem input, recua contra facing direction.
- [ ] **SPEC18-C41:** Dodge aceita 8 direcoes normalizadas.
- [ ] **SPEC18-C42:** Dodge consome stamina e respeita cooldown.
- [ ] **SPEC18-C43:** Dodge para em collider/inimigo/obstaculo.
- [ ] **SPEC18-C44:** Implementar i-frame data-driven, default 0.18s.
- [ ] **SPEC18-C45:** Durante i-frame, dano recebido deve ser ignorado/evitado via pipeline existente, sem criar sistema paralelo de HP.
- [ ] **SPEC18-C46:** Publicar eventos de invulnerabilidade start/end.

## [CODE] Loadout inicial

- [ ] **SPEC18-C47:** Criar/reutilizar dados dos itens iniciais: sword, bow, arrows, armors, fireball tome.
- [ ] **SPEC18-C48:** Garantir new game com loadout inicial de teste.
- [ ] **SPEC18-C49:** Equipar armadura inicial se EquipmentManager suportar preload seguro.
- [ ] **SPEC18-C50:** Garantir save/load compativel com os novos itens/dados.
- [ ] **SPEC18-C51:** Nao quebrar saves existentes; usar defaults/migrations se necessario.

## [UI/VFX/ART]

- [ ] **SPEC18-A01:** Criar placeholder `Projectile_Arrow_32x8.png` ou gerar via Editor tool.
- [ ] **SPEC18-A02:** Criar placeholder `Projectile_Fireball_16x16.png`.
- [ ] **SPEC18-A03:** Criar placeholder `VFX_Impact_Physical_16x16.png`.
- [ ] **SPEC18-A04:** Criar placeholder `VFX_Impact_Fire_16x16.png`.
- [ ] **SPEC18-A05:** Criar placeholder `Slash_Sword_Front_32x32.png` e `Slash_Sword_Side_32x32.png`.
- [ ] **SPEC18-A06:** Criar placeholder opcional `Dodge_Dust_16x16.png` e/ou `Dodge_IFrame_Glint_16x16.png`.
- [ ] **SPEC18-A07:** Garantir import pixel art: PPU 32, Point, Compression None, Generate Mip Maps false.
- [ ] **SPEC18-V01:** Impact VFX aparece ao projectile bater.
- [ ] **SPEC18-V02:** Slash visual aparece durante active frame melee.
- [ ] **SPEC18-V03:** Dodge i-frame tem feedback visual simples se seguro.

## [VALIDATION]

- [ ] **SPEC18-T01:** Criar `ValidateSpec18CombatMovementProjectiles.cs`.
- [ ] **SPEC18-T02:** Validator confirma projectile configs de bow/fireball.
- [ ] **SPEC18-T03:** Validator confirma player com combat movement controller ou equivalente.
- [ ] **SPEC18-T04:** Validator confirma dash skill liberavel/testavel.
- [ ] **SPEC18-T05:** Validator confirma loadout inicial com sword, bow, arrow, armor e fireball hook.
- [ ] **SPEC18-T06:** Validator confirma ausencia de novas buscas globais proibidas em runtime.
- [ ] **SPEC18-T07:** `dotnet build Assembly-CSharp.csproj` PASS.
- [ ] **SPEC18-T08:** `dotnet build Assembly-CSharp-Editor.csproj` PASS.
- [ ] **SPEC18-T09:** `tools/docs/validate_docs.ps1` PASS.
- [ ] **SPEC18-T10:** Criar checklist em `docs/validation/SPEC18_COMBAT_MOVEMENT_PROJECTILES_VALIDATION_<date>.md`.

---

# /speckit.implement

## Ordem recomendada de PRs pequenos

### PR 18A - Projectile visual foundation

Escopo:

- projectile movement 8 direcoes;
- range/lifetime/collision;
- damage request;
- impact/expired events;
- validator parcial.

Nao incluir dash/dodge/melee ainda.

### PR 18B - Bow arrow + loadout ranged

Escopo:

- bow config;
- arrow item;
- arrow range 10;
- starter bow/arrows;
- optional ammo consumption seguro;
- Play Mode checklist parcial.

### PR 18C - Spell projectile + Fireball Burn

Escopo:

- spell projectile;
- fireball;
- burn/status integration;
- fireball tome/hook;
- mana/cooldown validation.

### PR 18D - Melee timing + hitbox + slash

Escopo:

- wind-up/active/recovery;
- melee movement multiplier 0.70;
- hit once per target;
- slash visual.

### PR 18E - Dash skill + Dodge i-frame

Escopo:

- dash as purchased/test skill;
- dodge 3 units 3x;
- i-frame 0.18s;
- stamina/cooldown/collision;
- modal/input lock.

### PR 18F - Starter combat loadout + final validation

Escopo:

- armors;
- starting equipment/inventory;
- save/load compatibility;
- full validator;
- docs/validation.

---

# CODEX_PROMPT

```md
Leia primeiro:
- CLAUDE.md
- AGENTS.md
- docs/IMPLEMENTATION_STATUS.md
- .specs/implementados/spec_damage_status_elements_resistances_runtime.md
- .specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md
- .specs/a_implementar/spec_combat_movement_projectiles_melee_visuals_runtime.md

Implemente somente:
SPEC 18 - Combat Movement, Projectiles, Spells and Melee Visuals Runtime

PR alvo:
<PR 18A/18B/18C/18D/18E/18F>

Objetivo:
Evoluir o combate visual/runtime sem criar sistemas paralelos.

Regras obrigatorias:
- Nao recriar DamageCalculator, Stamina, Mana, Equipment, Inventory, SkillTree ou ActiveSkillSlots.
- Nao usar GameObject.Find, FindObjectOfType ou FindObjectsByType em runtime.
- E deve preservar prioridade de interacao.
- UI/modal aberto deve bloquear movimento, ataque, cast, dash e dodge.
- Flechas/magias devem suportar 8 direcoes normalizadas.
- Flecha basica tem range 10.
- Dodge tem i-frame.
- Melee reduz movimento em 30%, nao trava.
- Dash e skill, mas deve iniciar comprada/liberada para teste.
- Fireball deve existir e aplicar Burn quando suportado pelo status system atual.
- Starter loadout deve permitir testar sword, bow, arrows, armor e fireball.

Ao final, entregue:
- arquivos alterados;
- resumo tecnico curto;
- validacoes executadas;
- checklist Play Mode pendente;
- riscos residuais.
```

---

# Definition of Done

- Projeto compila sem erros.
- Validator SPEC 18 passa.
- Nao ha novas ocorrencias proibidas de busca global runtime.
- Flecha visual funciona com range 10.
- Flecha aceita diagonal.
- Magia projectile visual funciona e aceita diagonal.
- Fireball existe e aplica Burn ou status equivalente seguro.
- Melee slash/hitbox funciona.
- Melee reduz movimento em 30%, sem travar.
- Dash existe como skill comprada/liberada para teste.
- Dash move 6 units em 2x e para em colliders/inimigos.
- Dodge move 3 units em 3x e tem i-frame.
- Starter loadout permite testar combate sem setup manual pesado.
- Modal/input lock nao regride.
- Eventos usam payloads simples.
- Documentacao de validacao criada em `docs/validation/`.

---

# Checklist Play Mode humano

1. Abrir cena jogavel com player e inimigo.
2. Confirmar starter loadout com sword, bow, arrows, armor e fireball hook.
3. Equipar arco ou confirmar arco disponivel.
4. Disparar flecha em direcao cardinal.
5. Disparar flecha em diagonal.
6. Confirmar flecha visivel viajando ate range 10 ou hit.
7. Confirmar flecha acerta inimigo e mostra dano.
8. Confirmar flecha para/desaparece em parede/obstaculo.
9. Castar Fireball em direcao cardinal.
10. Castar Fireball em diagonal.
11. Confirmar dano de fogo.
12. Confirmar Burn/status aplicado ou pendencia documentada se DoT nao existir.
13. Tentar cast sem mana/cooldown e confirmar falha segura.
14. Atacar melee com Q/E.
15. Confirmar slash visual.
16. Confirmar dano uma vez por alvo por ataque.
17. Confirmar player ainda se move durante melee, mas mais lento em 30%.
18. Confirmar dash comprado/liberado.
19. Usar dash e confirmar 6 units, 2x speed e parada em collider/inimigo.
20. Usar dodge e confirmar 3 units, 3x speed.
21. Durante dodge, confirmar i-frame evitando dano no periodo configurado.
22. Abrir modal/inventory/shop e confirmar que WASD/ataque/dash/dodge nao vazam para gameplay.
23. Fechar modal e confirmar gameplay volta.
24. Confirmar stamina reduz e cooldown impede spam.
25. Confirmar console sem erros novos.

---

# Riscos residuais aceitos

- Arte pode ser placeholder.
- Balanceamento numerico pode precisar ajuste apos Play Mode.
- Burn pode iniciar como status/marcador se DoT seguro nao existir.
- Ammo consumption pode ser minimo ou pendente se o inventario/equipment atual tornar o escopo arriscado.
- Mouse aim fica fora.
- Lock-on fica fora.
- Gamepad fica fora.
