# SpecKit — FASE9G Enemy Combat Roles, AI, Status & Movesets

> **Feature:** `FASE9G_ENEMY_COMBAT_ROLES_AI_STATUS`  
> **Status:** amendment funcional aprovado para planejamento.  
> **Fonte de design:** `docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md`  
> **Complementa:** `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY`

---

## 1. User story

Como jogador, quero que cada criatura da cave tenha comportamento, movimentação, ataque, status e leitura visual próprios, para que o combate action seja variado, justo e coerente com a ecologia/facção do nível.

---

## 2. Objetivos funcionais

### O1 — Enemy combat profile

Todo inimigo deve possuir ou herdar perfil de combate com:

```text
RoleId
MovementPatternId
CombatBehaviorIds
DamageTypeIds
StatusIds
Resistances
Vulnerabilities
Scaling profile
```

### O2 — Combate action legível

Ataques especiais exigem telegraph, cooldown e janela de resposta.

### O3 — Status IDs reservados

IDs de Burn, Poison, Bleed, Chill, Freeze, Fear, Blind, Curse, Shadow Mark, Arcane Mark, Corruption, Stun, Knockback, Root, Webbed, ArmorUp, Enrage, Lifesteal, Silence e Confusion devem existir como contrato de design.

### O4 — Bosses com 3 fases

Todo boss candidate da cave deve ter 3 fases descritas.

### O5 — Minibosses com 2 mecânicas

Todo miniboss candidate deve ter 2 mecânicas especiais descritas.

### O6 — Armas elementais humanoides

Humanoides podem usar armas elementais e ter chance baixa de dropar versões danificadas/reparáveis.

### O7 — Aerial fallback

Criaturas aéreas precisam de fallback terrestre/placeholder quando arena/pathing não suportar voo real.

---

## 3. Hardening rules

### H1 — Telegraph obrigatório

Todo ataque especial precisa de aviso visual/temporal.

### H2 — Cooldown mínimo

Ataques especiais não podem ser spammados.

### H3 — Status budget

| Tier | Status permitidos |
|---|---:|
| Common | 0–2 |
| Strong | 1–2 |
| Elite | 1–3 |
| Miniboss | 2–4 |
| Boss | 3+ por fases |

### H4 — Spawn composition budget

Sala comum não deve combinar excesso de elites/controllers/casters sem orçamento.

### H5 — No unavoidable chain control

Controle forte não deve manter jogador sem ação continuamente.

### H6 — DamageCalculator único

Todo dano/status deve usar o fluxo do DamageCalculator/Status system definido nas specs anteriores.

---

## 4. Entidades funcionais

- `EnemyCombatProfileSO`
- `MovementPatternId`
- `CombatBehaviorId`
- `StatusId`
- `DamageTypeId`
- `WeaponAffixId`
- `EnemyRoleId`

---

## 5. Critérios de aceite

### CA1 — Perfil de combate

Cada inimigo da FASE9G possui ou herda perfil de combate.

### CA2 — Telegraph

Ataques especiais possuem telegraph ou janela clara de resposta.

### CA3 — Controle justo

Status de controle não encadeiam sem recovery window.

### CA4 — Boss phases

Cada boss candidate tem 3 fases na spec.

### CA5 — Miniboss mechanics

Cada miniboss tem 2 mecânicas especiais.

### CA6 — Aerial fallback

Criaturas aéreas têm fallback para dash/leap/projectile placeholder.

### CA7 — Armas elementais data-driven

Affixes e drops elementais são data-driven, não hardcoded.

### CA8 — Debug futuro

DebugHud deve conseguir mostrar Role, MovementPattern, CombatBehavior, StatusIds e DamageTypes quando implementado.

---

## 6. Non-goals

Fora deste amendment:

- IA final de produção;
- animações finais;
- balance final de dano/HP;
- arte/sprites finais;
- sistema completo de dodge do player;
- hitbox/hurtbox avançado;
- bosses finais totalmente implementados;
- VFX/SFX finais de status e telegraphs.

---

## 7. Dependências

- FASE9G Cave Bestiary/Faction Locks/Portal Ecology.
- FASE9F Cave procedural/resources/encounters.
- FASE9E Damage/Status/Formula.
- FASE9E Save Schema/Migration.
- FASE9E Player Level Up/Progression.

---

## 8. Pronto para Plan quando

- FASE9F procedural foundation existir.
- FASE9G enemy ecology/faction locks estiverem sendo implementados.
- O time decidir implementar EnemyCombatProfileSO/data-driven combat profiles.
