# SPEC 18 - Combat Movement, Projectiles, Spells and Melee Visuals Runtime

> Spec ID: spec_combat_movement_projectiles_melee_visuals_runtime
> Status: A implementar
> Ordem de execucao: 18
> Tipo: Combat/Runtime/Animation/VFX/Input
> Fonte de refinamento: `docs/refinements/a_implementar/pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md`
> Depende de: SPEC 09, SPEC 10, SPEC 11, SPEC 12, SPEC 16 e SPEC 17/input-modal closeout quando aplicavel
> Fora de escopo: ammo, block/parry, heavy attack, combos complexos, lock-on, mouse aim completo, gamepad completo, AoE complexa, boss-specific attacks, arte final polida.

---

## 1. Objetivo

Evoluir o combate do jogador para que ataques e movimentos tenham expressao visual e comportamento runtime claro:

- flechas viajam visualmente ate o alvo;
- magias ofensivas viajam visualmente ate o alvo;
- ataques melee possuem timing, slash visual e hitbox ativa;
- dash e dodge sao movimentos distintos, com stamina, cooldown, colisao e input lock;
- tudo usa os contratos existentes de dano, stamina, mana, equipment, skill actions e eventos.

---

## 2. Pre-condicoes

Antes de implementar, revalidar no repo:

```text
Assets/_Game/Scripts/Player/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Player/Combat/PlayerSpellCaster.cs
Assets/_Game/Scripts/Combat/ProjectileBehaviour.cs
Assets/_Game/Scripts/Combat/DamageCalculator.cs
Assets/_Game/Scripts/Combat/DamageRequest.cs
Assets/_Game/Scripts/Combat/DamageType.cs
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Stamina/**
Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs
Assets/_Game/Scripts/Skills/SkillActionExecutor.cs
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/UI/**
```

Nao criar novo sistema paralelo de dano, stamina, mana, equipment ou skill action.

---

## 3. Decisoes fechadas para implementacao inicial

| Tema | Decisao |
|---|---|
| Numero da spec | 18 |
| Flechas | Projectile visual linear, 4 direcoes, sem ammo |
| Magias | Projectile visual linear por elemento, sem AoE inicial |
| Mouse aim | Fora do escopo inicial |
| Diagonal | Fora do escopo inicial; 4 direcoes cardinais |
| Melee | Wind-up, active frames, recovery, hitbox frontal e slash visual |
| Dash | 6 world units para frente, MoveSpeed * 2, sem i-frame por padrao |
| Dodge | 3 world units lateral/tras/input valido, MoveSpeed * 3, sem i-frame por default |
| Stamina | Dash/dodge consomem stamina; valores data-driven |
| Cooldown | Dash/dodge/projeteis/melee respeitam cooldowns existentes ou novos configs |
| Colisao | Movimento especial para ao bater em parede/obstaculo |
| UI/modal | Modal aberto bloqueia ataque, cast, dash, dodge e movimento |
| Dano | Sempre via DamageRequest/DamageCalculator existentes |
| Eventos | Payloads simples, sem referencias Unity |

---

## 4. Escopo funcional

### 4.1 Projectile visual para flechas

Implementar/expandir `ProjectileBehaviour` para suportar:

- origem;
- direcao cardinal;
- velocidade;
- range;
- lifetime;
- layer de alvos;
- layer de obstaculos;
- dano por `DamageRequest`;
- despawn ao colidir, expirar ou atingir range;
- VFX de impacto opcional;
- rotacao do sprite conforme direcao.

Critérios:

- arco equipado dispara flecha visivel;
- flecha se move no mundo;
- flecha acerta primeiro inimigo valido;
- flecha nao atravessa parede;
- flecha desaparece ao fim do alcance.

### 4.2 Projectile visual para magias

Expandir spell casting para instanciar projectile visual quando a magia for do tipo projectile.

Critérios:

- magia consome mana;
- magia respeita cooldown;
- projectile usa visual por elemento;
- projectile aplica DamageType correto;
- spell falha de forma segura se faltar mana, cooldown ou prefab/config.

### 4.3 Ataque melee com hitbox e slash visual

Implementar runtime de ataque melee com fases:

```text
Wind-up -> Active -> Recovery
```

Critérios:

- Q/E continuam acionando ataques conforme slots/maos existentes;
- E preserva prioridade de interacao quando houver candidato interagivel;
- durante active frame, hitbox frontal detecta inimigos;
- cada inimigo recebe dano no maximo uma vez por ataque;
- slash visual aparece na direcao correta;
- AttackSpeed influencia cooldown/duracao conforme contrato existente;
- dano passa por DamageCalculator.

### 4.4 Dash

Implementar dash como movimento ofensivo/engage:

```text
Distancia: 6 world units
Velocidade: MoveSpeed * 2.0
Direcao: facing direction atual
Input sugerido: Left Shift ou action configuravel
I-frame: false por default
```

Critérios:

- dash consome stamina;
- se stamina insuficiente, nao inicia;
- dash respeita cooldown;
- dash bloqueia movimento normal enquanto ativo;
- dash para em obstaculo;
- dash nao inicia durante modal, death, stun, dodge, dash ou cast/attack nao cancelavel.

### 4.5 Dodge

Refinar dodge existente:

```text
Distancia: 3 world units
Velocidade: MoveSpeed * 3.0
Input: Space
Direcao: input atual; sem input, direcao oposta ao facing direction
I-frame: false por default, hook data-driven futuro
```

Critérios:

- dodge consome stamina;
- dodge respeita cooldown;
- dodge bloqueia movimento normal enquanto ativo;
- dodge para em obstaculo;
- dodge nao inicia durante modal, death, stun, dash, dodge ou ataque/cast nao cancelavel.

---

## 5. Arquitetura sugerida

### 5.1 Novos componentes/classes permitidos

```text
Assets/_Game/Scripts/Combat/CombatProjectileVisualDataSO.cs
Assets/_Game/Scripts/Combat/ProjectileImpactVfxResolver.cs
Assets/_Game/Scripts/Player/Combat/PlayerCombatMovementController.cs
Assets/_Game/Scripts/Player/Combat/MeleeAttackRuntime.cs
Assets/_Game/Scripts/Player/Combat/MeleeHitbox.cs
Assets/_Game/Scripts/Player/Combat/PlayerCombatStateController.cs
Assets/_Game/Scripts/Core/Events/PlayerDashStartedEvent.cs
Assets/_Game/Scripts/Core/Events/PlayerDashEndedEvent.cs
Assets/_Game/Scripts/Core/Events/PlayerMeleeAttackStartedEvent.cs
Assets/_Game/Scripts/Core/Events/PlayerMeleeAttackEndedEvent.cs
Assets/_Game/Scripts/Core/Events/ProjectileHitEvent.cs
Assets/_Game/Scripts/Core/Events/ProjectileExpiredEvent.cs
Assets/_Game/Scripts/Editor/Validation/ValidateSpec18CombatMovementProjectiles.cs
```

### 5.2 Arquivos que podem ser modificados

```text
Assets/_Game/Scripts/Player/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Player/Combat/PlayerSpellCaster.cs
Assets/_Game/Scripts/Combat/ProjectileBehaviour.cs
Assets/_Game/Scripts/Equipment/Data/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Data/SpellDataSO.cs
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs
```

A lista deve ser revalidada pelo agente conforme estado real do repo antes de implementar.

### 5.3 Proibido

- Criar novo DamageCalculator paralelo.
- Criar novo sistema de stamina paralelo.
- Criar novo sistema de mana paralelo.
- Usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime.
- Serializar GameObject/Transform/MonoBehaviour em save.
- Colocar regra de gameplay dentro de UI.
- Quebrar prioridade de interacao do E.
- Alterar specs implementadas sem necessidade.

---

## 6. Eventos

Criar somente se ainda nao existirem.

```csharp
public readonly struct PlayerDashStartedEvent
{
    public readonly string SourceId;
    public readonly float Distance;
    public readonly float SpeedMultiplier;
}

public readonly struct PlayerDashEndedEvent
{
    public readonly string SourceId;
    public readonly bool InterruptedByCollision;
}

public readonly struct PlayerMeleeAttackStartedEvent
{
    public readonly string WeaponId;
    public readonly string HandSlot;
}

public readonly struct PlayerMeleeAttackEndedEvent
{
    public readonly string WeaponId;
    public readonly string HandSlot;
}

public readonly struct ProjectileHitEvent
{
    public readonly string SourceId;
    public readonly string ProjectileId;
    public readonly string TargetId;
}

public readonly struct ProjectileExpiredEvent
{
    public readonly string SourceId;
    public readonly string ProjectileId;
    public readonly string Reason;
}
```

Eventos podem ser ajustados ao padrao real do projeto, mantendo payload simples.

---

## 7. Dados e defaults sugeridos

### 7.1 Dash

```text
DashDistance = 6.0
DashSpeedMultiplier = 2.0
DashStaminaCost = 30
DashCooldownSeconds = 1.2
DashHasIFrames = false
```

### 7.2 Dodge

```text
DodgeDistance = 3.0
DodgeSpeedMultiplier = 3.0
DodgeStaminaCost = 15
DodgeCooldownSeconds = 0.6
DodgeHasIFrames = false
```

### 7.3 Arrow projectile

```text
ProjectileRange = 6.0
ProjectileSpeed = 10.0
ProjectileLifetime = 1.0
ProjectileRadius = 0.15
CanPierce = false
```

### 7.4 Magic projectile

```text
ProjectileRange = SpellDataSO.Range
ProjectileSpeed = 8.0-12.0 conforme elemento
ProjectileLifetime = Range / Speed + margem pequena
CanPierce = false
```

---

## 8. Arte placeholder

Criar placeholders simples se os assets ainda nao existirem:

```text
Assets/_Game/Sprites/Combat/Projectiles/Projectile_Arrow_32x8.png
Assets/_Game/Sprites/Combat/Projectiles/Projectile_ArcaneBolt_16x16.png
Assets/_Game/Sprites/Combat/Projectiles/Projectile_FireBolt_16x16.png
Assets/_Game/Sprites/Combat/Projectiles/Projectile_IceShard_16x16.png
Assets/_Game/Sprites/Combat/VFX/VFX_Impact_Physical_16x16.png
Assets/_Game/Sprites/Combat/VFX/VFX_Impact_Arcane_16x16.png
Assets/_Game/Sprites/Combat/Melee/Slash_Sword_Front_32x32.png
Assets/_Game/Sprites/Combat/Melee/Slash_Sword_Side_32x32.png
Assets/_Game/Sprites/Combat/Movement/Dodge_Dust_16x16.png
```

Import obrigatorio:

```text
PPU 32
Filter Mode Point
Compression None
Generate Mip Maps false
```

---

## 9. Validacao automatica

Criar validator Editor `ValidateSpec18CombatMovementProjectiles` verificando:

- `ProjectileBehaviour` existe e possui campos/configs esperados;
- pelo menos um WeaponDataSO de bow aponta para projectile visual/config;
- pelo menos uma SpellDataSO projectile aponta para visual/config;
- Player possui controller de combat movement;
- eventos novos compilam;
- nenhum uso novo de `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime;
- assets placeholder essenciais existem ou sao geraveis por Editor tool.

---

## 10. Checklist Play Mode humano

Executar no final do pacote:

1. Abrir cena jogavel com player e inimigo.
2. Equipar arco ou usar arma ranged configurada.
3. Disparar flecha.
4. Confirmar flecha visivel viajando ate o inimigo.
5. Confirmar dano no inimigo e floating number.
6. Confirmar flecha desaparece ao atingir parede ou fim do alcance.
7. Castar magia projectile com mana suficiente.
8. Confirmar projectile elemental visivel e dano correto.
9. Tentar cast sem mana/cooldown e confirmar falha segura.
10. Atacar melee com Q/E.
11. Confirmar slash visual, hitbox, dano uma vez por ataque e cooldown.
12. Abrir modal/inventory/shop e confirmar que WASD/ataque/dash/dodge nao vazam para gameplay.
13. Fechar modal e confirmar gameplay volta.
14. Pressionar Space com stamina suficiente e confirmar dodge 3 units em 3x.
15. Pressionar dash input e confirmar dash 6 units em 2x.
16. Confirmar dash/dodge param em colisao.
17. Confirmar stamina reduz e cooldown impede spam.
18. Confirmar console sem erros novos.

---

## 11. Prompt base para agente executor

```md
Leia primeiro:
- CLAUDE.md
- AGENTS.md
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
- docs/specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md
- docs/specs/a_implementar/spec_combat_movement_projectiles_melee_visuals_runtime.md

Implemente somente:
SPEC 18 - Combat Movement, Projectiles, Spells and Melee Visuals Runtime

Objetivo:
Evoluir o combate visual/runtime sem criar sistemas paralelos: flechas/magias como projeteis visuais, melee com hitbox/timing/slash, dash e dodge com stamina/cooldown/colisao/input lock.

Regras:
- Nao recriar DamageCalculator, Stamina, Mana, Equipment ou SkillAction.
- Nao usar GameObject.Find/FindObjectOfType/FindObjectsByType em runtime.
- E deve preservar prioridade de interacao.
- UI/modal aberto deve bloquear movimento, ataque, cast, dash e dodge.
- Criar validator Editor para SPEC 18.
- Se precisar gerar placeholder visual, criar via Editor tool ou assets simples com import pixel art correto.

Ao final, entregue:
- arquivos alterados;
- resumo tecnico curto;
- validacoes executadas;
- checklist Play Mode pendente;
- riscos residuais.
```

---

## 12. Definition of Done

- Projeto compila sem erros.
- Validator SPEC 18 passa.
- Nao ha novas ocorrencias proibidas de busca global runtime.
- Flecha visual funciona em Play Mode.
- Magia projectile visual funciona em Play Mode.
- Melee slash/hitbox funciona em Play Mode.
- Dash e dodge funcionam em Play Mode.
- Modal/input lock nao regride.
- Eventos usam payloads simples.
- Documentacao de validacao criada em `docs/validation/`.

---

## 13. Riscos residuais aceitos

- Arte ainda pode ser placeholder.
- Balanceamento fino de stamina/cooldown/duracao pode ficar para spec futura.
- I-frames podem ficar desligados por default.
- Mouse aim e diagonal ficam fora deste recorte.
- Play Mode humano final sera feito ao fim do pacote, conforme fluxo atual do projeto.
