# SpecKit â€” FASE9D Enemy Architecture 40+ Monsters

> **Feature:** FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS_v1.1.md`

---

## 1. User story

Como designer/dev de Cindar's Hope, quero uma arquitetura de inimigos orientada a dados para criar 40+ monstros com movimentos, ataques, elementos, status e janelas de vulnerabilidade sem duplicar scripts por monstro.

---

## 2. Objetivos funcionais

### O1 â€” Inimigos por composiÃ§Ã£o

Monstros devem ser montados por componentes e dados, nÃ£o por classes gigantes especÃ­ficas.

### O2 â€” Movement profiles

Cada monstro deve poder perseguir, manter distÃ¢ncia, fugir, patrulhar, orbitar ou ficar parado.

### O3 â€” Action profiles

Cada monstro deve ter lista de aÃ§Ãµes: melee, leap, dash, ranged, magic, area, summon, support etc.

### O4 â€” Element profiles

Cada monstro deve ter vulnerabilidades, resistÃªncias e imunidades elementais.

### O5 â€” Status effects

Monstros devem poder aplicar ou receber status negativos.

### O6 â€” Vulnerability windows

Toda aÃ§Ã£o relevante deve poder abrir janela de vulnerabilidade com dano ampliado, default 1.5x.

### O7 â€” Escalabilidade

Adicionar um novo monstro deve exigir principalmente novos assets, nÃ£o novo controller especÃ­fico.

---

## 3. Non-goals

Fora de escopo:

- criar os 40 monstros agora;
- pathfinding avanÃ§ado;
- Ã¡rvore de comportamento completa;
- animaÃ§Ãµes finais;
- VFX finais;
- balanceamento final;
- procedural spawn;
- boss AI complexa.

---

## 4. Regras de negÃ³cio

### R1 â€” NÃ£o criar controller especÃ­fico por monstro

Evitar `SlimeController`, `BatController`, `GolemController` como lÃ³gica principal.

### R2 â€” Todo inimigo tem movimento explÃ­cito

Todo `EnemyDataSO` deve apontar para `EnemyMovementProfileSO`.

### R3 â€” Todo inimigo tem ao menos uma aÃ§Ã£o

Todo `EnemyDataSO` deve ter ao menos uma `EnemyActionDataSO`.

### R4 â€” AÃ§Ã£o tem fases

AÃ§Ã£o deve ter preparaÃ§Ã£o, ativo, recovery e cooldown quando aplicÃ¡vel.

### R5 â€” Vulnerabilidade Ã© data-driven

A janela vulnerÃ¡vel deve ser definida nos dados da aÃ§Ã£o ou perfil.

### R6 â€” Elementos modificam dano

Dano final considera elemento + perfil elemental + vulnerabilidade.

### R7 â€” Status nÃ£o stacka infinitamente

Status negativos devem ter regra de duraÃ§Ã£o/renovaÃ§Ã£o clara.

---

## 5. Entidades funcionais

- `EnemyArchetype`
- `EnemyMovementMode`
- `EnemyActionType`
- `EnemyActionPhase`
- `DamageElement`
- `StatusEffectType`
- `IEnemyAction`
- `EnemyActionContext`
- `EnemyMovementProfileSO`
- `EnemyActionDataSO`
- `EnemyElementProfileSO`
- `EnemyStatusProfileSO`
- `EnemyActionController`
- `EnemyMovementController`
- `EnemyVulnerabilityController`
- `EnemyStatusReceiver`

---

## 6. CritÃ©rios de aceite

### CA1 â€” Contracts

Enums, interfaces e contextos existem e compilam.

### CA2 â€” Profiles

Movement, action, element e status profiles podem ser criados via CreateAssetMenu.

### CA3 â€” Action controller

EnemyActionController escolhe aÃ§Ã£o por range, prioridade e cooldown.

### CA4 â€” Movement controller

EnemyMovementController executa ao menos ChaseTarget, KeepDistance, FleeFromTarget e Stationary.

### CA5 â€” Vulnerability window

EnemyVulnerabilityController abre janela e aplica multiplicador 1.5x quando ativo.

### CA6 â€” Element damage

Dano recebido considera DamageElement e EnemyElementProfileSO.

### CA7 â€” Status receiver

EnemyStatusReceiver suporta Slow, Poison, Burn e Stun curto no MVP.

### CA8 â€” Slime migrado

Slime usa EnemyActionController, MovementProfile, LeapAttack e VulnerabilityController.

### CA9 â€” Segundo monstro

Um monstro de distÃ¢ncia/kiter valida reutilizaÃ§Ã£o sem controller especÃ­fico novo.

### CA10 â€” Validator

Validator detecta EnemyDataSO sem movement profile, sem action, com timings invÃ¡lidos ou IDs duplicados.

---

## 7. DependÃªncias

- `EnemyDataSO`
- `EnemyHealth`
- `DamageRequest`
- `KnockbackController`
- `HitFlashController`
- `PlayerManager`
- `GameBootstrap`
- `GameEventBus`
- futura integraÃ§Ã£o com `PlayerCombatController`

---

## 8. Observabilidade MVP

Logs/HUD debug devem permitir ver:

- aÃ§Ã£o escolhida;
- inÃ­cio/fim da aÃ§Ã£o;
- janela vulnerÃ¡vel aberta/fechada;
- status aplicado;
- elemento usado;
- multiplicador final de dano.

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs_old/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS_v1.1.md` estiver lido.
- Estado real em `dev` tiver sido validado.

