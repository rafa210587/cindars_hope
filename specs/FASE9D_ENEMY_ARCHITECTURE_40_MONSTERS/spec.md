# SpecKit — FASE9D Enemy Architecture 40+ Monsters

> **Feature:** FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS_v1.1.md`

---

## 1. User story

Como designer/dev de Cindar's Hope, quero uma arquitetura de inimigos orientada a dados para criar 40+ monstros com movimentos, ataques, elementos, status e janelas de vulnerabilidade sem duplicar scripts por monstro.

---

## 2. Objetivos funcionais

### O1 — Inimigos por composição

Monstros devem ser montados por componentes e dados, não por classes gigantes específicas.

### O2 — Movement profiles

Cada monstro deve poder perseguir, manter distância, fugir, patrulhar, orbitar ou ficar parado.

### O3 — Action profiles

Cada monstro deve ter lista de ações: melee, leap, dash, ranged, magic, area, summon, support etc.

### O4 — Element profiles

Cada monstro deve ter vulnerabilidades, resistências e imunidades elementais.

### O5 — Status effects

Monstros devem poder aplicar ou receber status negativos.

### O6 — Vulnerability windows

Toda ação relevante deve poder abrir janela de vulnerabilidade com dano ampliado, default 1.5x.

### O7 — Escalabilidade

Adicionar um novo monstro deve exigir principalmente novos assets, não novo controller específico.

---

## 3. Non-goals

Fora de escopo:

- criar os 40 monstros agora;
- pathfinding avançado;
- árvore de comportamento completa;
- animações finais;
- VFX finais;
- balanceamento final;
- procedural spawn;
- boss AI complexa.

---

## 4. Regras de negócio

### R1 — Não criar controller específico por monstro

Evitar `SlimeController`, `BatController`, `GolemController` como lógica principal.

### R2 — Todo inimigo tem movimento explícito

Todo `EnemyDataSO` deve apontar para `EnemyMovementProfileSO`.

### R3 — Todo inimigo tem ao menos uma ação

Todo `EnemyDataSO` deve ter ao menos uma `EnemyActionDataSO`.

### R4 — Ação tem fases

Ação deve ter preparação, ativo, recovery e cooldown quando aplicável.

### R5 — Vulnerabilidade é data-driven

A janela vulnerável deve ser definida nos dados da ação ou perfil.

### R6 — Elementos modificam dano

Dano final considera elemento + perfil elemental + vulnerabilidade.

### R7 — Status não stacka infinitamente

Status negativos devem ter regra de duração/renovação clara.

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

## 6. Critérios de aceite

### CA1 — Contracts

Enums, interfaces e contextos existem e compilam.

### CA2 — Profiles

Movement, action, element e status profiles podem ser criados via CreateAssetMenu.

### CA3 — Action controller

EnemyActionController escolhe ação por range, prioridade e cooldown.

### CA4 — Movement controller

EnemyMovementController executa ao menos ChaseTarget, KeepDistance, FleeFromTarget e Stationary.

### CA5 — Vulnerability window

EnemyVulnerabilityController abre janela e aplica multiplicador 1.5x quando ativo.

### CA6 — Element damage

Dano recebido considera DamageElement e EnemyElementProfileSO.

### CA7 — Status receiver

EnemyStatusReceiver suporta Slow, Poison, Burn e Stun curto no MVP.

### CA8 — Slime migrado

Slime usa EnemyActionController, MovementProfile, LeapAttack e VulnerabilityController.

### CA9 — Segundo monstro

Um monstro de distância/kiter valida reutilização sem controller específico novo.

### CA10 — Validator

Validator detecta EnemyDataSO sem movement profile, sem action, com timings inválidos ou IDs duplicados.

---

## 7. Dependências

- `EnemyDataSO`
- `EnemyHealth`
- `DamageRequest`
- `KnockbackController`
- `HitFlashController`
- `PlayerManager`
- `GameBootstrap`
- `GameEventBus`
- futura integração com `PlayerCombatController`

---

## 8. Observabilidade MVP

Logs/HUD debug devem permitir ver:

- ação escolhida;
- início/fim da ação;
- janela vulnerável aberta/fechada;
- status aplicado;
- elemento usado;
- multiplicador final de dano.

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS_v1.1.md` estiver lido.
- Estado real em `dev` tiver sido validado.
