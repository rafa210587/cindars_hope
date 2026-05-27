# refinamento_combat_movement_projectiles_melee_visuals

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_combat_movement_projectiles_melee_visuals_runtime.md`
> Ordem de execucao sugerida: 18
> Depende de: SPEC 09, SPEC 10, SPEC 11, SPEC 12, SPEC 16 e SPEC 17/input-modal closeout quando aplicavel
> Objetivo: evoluir o combate runtime com projeteis visuais, magias visuais, ataques melee com animacao/hitbox e movimentos de dash/dodge com regras claras de stamina, cooldown, colisao e input lock.

---

## 1. Contexto

O projeto ja possui uma base parcial de combate do jogador.

Base existente relevante:

```text
Assets/_Game/Scripts/Player/Combat/PlayerAttackController.cs
Assets/_Game/Scripts/Player/Combat/PlayerSpellCaster.cs
Assets/_Game/Scripts/Skills/SkillActionExecutor.cs
Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs
Assets/_Game/Scripts/Combat/ProjectileBehaviour.cs
Assets/_Game/Scripts/Combat/DamageCalculator.cs
Assets/_Game/Scripts/Combat/DamageRequest.cs
Assets/_Game/Scripts/Combat/DamageType.cs
Assets/_Game/Scripts/Combat/StatusEffectManager.cs
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Stamina/**
Assets/_Game/Scripts/Core/Events/**
```

A SPEC 12 declarou como entregue parcialmente:

- combate player data-driven com Q/E attacks;
- prioridade de interacao preservada em E;
- dodge simples com stamina em Space, sem i-frames;
- bow/projectile system placeholder;
- spell casting com mana via SpellDatabaseSO;
- active skill slots R/T/Y/G;
- integracao com DamageCalculator, Equipment e Stamina.

A SPEC 11 declarou o pipeline formal de dano:

- DamageType;
- DamageRequest;
- DamageResult;
- DamageCalculator;
- resistencias/vulnerabilidades;
- status effects;
- floating damage numbers.

Portanto, este refinamento nao deve criar um novo sistema paralelo de dano ou combate. Deve evoluir o que existe.

---

## 2. Problema

O combate ainda tende a parecer abstrato/placeholder:

- flechas podem existir como placeholder, mas precisam viajar visualmente ate o alvo;
- magias precisam aparecer como bolts/orbs/projeteis visuais, nao dano instantaneo invisivel;
- ataques melee precisam ter animacao, timing e hitbox clara;
- dodge existe como movimento simples, mas precisa ser refinado;
- dash ainda precisa ser definido como movimento ofensivo separado;
- os movimentos especiais precisam respeitar input lock, stamina, cooldown, colisao e UI/modal state.

---

## 3. Decisoes aprovadas neste refinamento

### 3.1 Ordem correta

Esta frente deve ser tratada como SPEC 18.

Motivo:

```text
SPEC 17 = UI/UX closeout ampla e incrementos relacionados.
SPEC 18 = Combat movement, projectiles, spells and melee visuals.
```

Nao usar SPEC 21 para esta frente enquanto 18-20 ainda nao existirem.

### 3.2 Regra central

```text
Dano continua no pipeline existente.
Visual, timing, trajetoria, hitbox e movimento entram nesta spec.
```

### 3.3 Separacao refinamento vs spec

Este documento guarda contexto, decisoes e duvidas.

A spec executavel deve ser menor, objetiva e pronta para agente:

```text
Refinamento = intencao, comportamento, duvidas, tradeoffs.
Spec = escopo fechado, arquivos-alvo, tasks e validacao.
```

---

## 4. Flechas / projeteis fisicos

### 4.1 Intencao

Flecha deve ser um objeto visivel que sai do jogador, viaja em linha reta, colide com inimigo ou obstaculo, aplica dano uma vez e desaparece.

### 4.2 Comportamento desejado

| Aspecto | Decisao inicial |
|---|---|
| Origem | Player + offset frontal/mao |
| Direcao | Direcao atual/facing direction do player; futuro: aim por mouse/analogico |
| Movimento | Linear |
| Alcance MVP | 6 unidades/world units |
| Velocidade inicial | Data-driven no WeaponDataSO ou ProjectileVisualDataSO |
| Colisao | Primeiro alvo valido recebe dano |
| Obstaculo | Despawn ao bater em parede/collider solido |
| Hit multiple targets | Nao no MVP |
| Piercing | Hook futuro por flag/data |
| Ammo | Fora de escopo; SPEC 12 ja marcou bow sem ammo |
| Visual | Sprite rotacionado conforme direcao |
| Impacto | VFX simples no hit/despawn |

### 4.3 Dados desejados

Campos possiveis em WeaponDataSO ou sub-data de projectile:

```text
ProjectilePrefab
ProjectileSpeed
ProjectileRange
ProjectileRadius
ProjectileDamageMultiplier
ProjectileLifetime
ProjectileImpactVfx
CanPierce
MaxPierceTargets
```

### 4.4 Pontos a decidir depois

- A flecha deve mirar apenas na direcao cardinal do player ou aceitar diagonais?
- Mouse aim entra agora ou fica futuro?
- Flecha deve parar na primeira parede sempre?
- Flecha deve usar Rigidbody2D ou movement manual com raycast/circlecast?

Decisao inicial recomendada:

```text
Cardinal + Rigidbody2D/MovePosition ou movimento manual simples; sem mouse aim no MVP.
```

---

## 5. Magias visuais

### 5.1 Intencao

Magias de ataque devem aparecer no mundo como projeteis ou efeitos visuais, usando o mesmo pipeline de dano e recursos do SpellDataSO.

### 5.2 Tipos iniciais

| Tipo | Visual | Movimento | DamageType |
|---|---|---|---|
| Arcane Bolt | orb/bolt roxo/azul | Linear | Arcane |
| Fire Bolt | projétil quente | Linear | Fire |
| Ice Shard | cristal/estilhaço | Linear | Ice |
| Lightning Spark | bolt rapido | Linear | Lightning |

### 5.3 Comportamento desejado

| Aspecto | Decisao inicial |
|---|---|
| Custo | Mana via SpellDataSO |
| Cooldown | SpellDataSO/ActiveSkillSlots |
| Alcance | SpellDataSO.Range |
| Velocidade | SpellDataSO ou ProjectileVisualDataSO |
| Status effect | Hook por StatusEffectId opcional |
| VFX impacto | Por elemento |
| Area of effect | Fora do MVP desta spec; hook futuro |

### 5.4 Pontos a decidir depois

- Magia basica sempre viaja em linha reta ou pode mirar em alvo travado?
- Status elemental entra agora ou somente visual/dano?
- Magias de area entram nesta spec ou ficam numa SPEC posterior?

Decisao inicial recomendada:

```text
Magias lineares de alvo unico. Sem lock-on. Sem AoE. Status opcional somente se ja existir data segura.
```

---

## 6. Ataque melee com animacao e hitbox

### 6.1 Intencao

Ataques melee devem ter sensacao fisica: preparacao, frame ativo, slash visual e recovery.

### 6.2 Fases

| Fase | Papel | Duração inicial sugerida |
|---|---|---|
| Wind-up | Preparar o golpe; pode reduzir/travar movimento | 0.08s-0.15s |
| Active | Hitbox aparece e aplica dano | 0.08s-0.12s |
| Recovery | Retorno ao controle normal | 0.15s-0.30s |

### 6.3 Hitbox

| Arma | Hitbox inicial |
|---|---|
| Unarmed | pequeno box frontal |
| Sword | arco/box frontal medio |
| Spear futuro | box estreito longo |
| Axe futuro | arco mais largo, recovery maior |

### 6.4 Regras

- Hitbox deve aplicar dano uma vez por alvo por ataque.
- Hitbox deve seguir facing direction.
- Dano usa DamageRequest/DamageCalculator existentes.
- Slash visual nao deve ser a fonte da verdade do dano; ele acompanha o timing.
- AnimationEvent pode ser usado como opcional, mas o controller deve funcionar por timer para reduzir fragilidade.

### 6.5 Pontos a decidir depois

- Movimento deve travar 100% durante ataque ou reduzir velocidade?
- Ataque pode ser cancelado por dodge/dash?
- Q/E continuam mao esquerda/direita ou E fica apenas interacao quando ha candidato?

Decisao inicial recomendada:

```text
Durante wind-up/active, reduzir movimento para 40% ou travar por data. Recovery libera movimento parcial. E continua com prioridade de interacao.
```

---

## 7. Dash

### 7.1 Intencao

Dash deve ser movimento ofensivo/engage, diferente de dodge.

Definicao base pedida:

```text
Dash move pelo menos 6 passos/unidades para frente em velocidade 2x maior.
```

### 7.2 Comportamento inicial

| Aspecto | Decisao inicial |
|---|---|
| Direcao | Facing direction atual do player |
| Distancia | 6 world units/tiles de referencia |
| Velocidade | MoveSpeed * 2.0 |
| Custo | Stamina medio/alto |
| Cooldown | Maior que dodge |
| Input | Shift ou skill action; nao Space |
| Colisao | Para ao bater em parede/obstaculo |
| I-frame | Nao por padrao |
| Dano | Nao causa dano por padrao; hook futuro para skill |

### 7.3 Observacao de tuning

Se MoveSpeed for 5 e DashSpeed for 10, 6 units leva aproximadamente 0.6s. Pode parecer longo. Manter distance/speed data-driven para ajuste.

### 7.4 Pontos a decidir depois

- Dash deve atravessar inimigos ou parar neles?
- Dash ofensivo deve permitir follow-up attack mais rapido?
- Dash deve ser habilidade desbloqueavel na skill tree ou disponivel desde o inicio?

Decisao inicial recomendada:

```text
Dash disponivel como combat action basica ou skill ativa inicial, sem dano, sem atravessar parede, sem i-frame.
```

---

## 8. Dodge

### 8.1 Intencao

Dodge deve ser movimento defensivo curto e rapido.

Definicao base pedida:

```text
Dodge move 3 passos/unidades para direcoes opostas ou laterais em velocidade 3x.
```

### 8.2 Comportamento inicial

| Aspecto | Decisao inicial |
|---|---|
| Direcao primaria | Input atual WASD/setas |
| Sem input | Recuar em direcao oposta ao facing direction |
| Distancia | 3 world units/tiles de referencia |
| Velocidade | MoveSpeed * 3.0 |
| Custo | Stamina baixo/medio |
| Cooldown | Curto |
| Input | Space |
| Colisao | Para ao bater em obstaculo |
| I-frame | Opcional por data; default false para preservar SPEC 12 |
| Dano | Nao causa dano |

### 8.3 Direcoes permitidas

Permitidas:

- lateral esquerda;
- lateral direita;
- para tras;
- direcao do input se o jogador pressionar uma direcao valida.

Nao recomendado no refinamento inicial:

- dodge para frente como dash menor, para manter diferenca entre dash e dodge.

### 8.4 Pontos a decidir depois

- I-frame entra agora ou fica para uma spec de balanceamento?
- Dodge cancela ataque melee?
- Dodge consome input dentro de UI/modal? Resposta: nao; deve respeitar modal/input lock.

Decisao inicial recomendada:

```text
Dodge defensivo, Space, 3 units, 3x speed, sem i-frame por default, com flag futura para i-frame.
```

---

## 9. Input e estados de controle

### 9.1 Estados necessarios

```text
Normal
Attacking
Casting
Dashing
Dodging
Stunned/Dead
ModalBlocked
```

### 9.2 Regras

- Modal aberto bloqueia ataque, cast, dash, dodge, hotbar e movimento.
- Interacao em E continua tendo prioridade sobre ataque da mao direita quando houver candidato de interacao.
- Dash/dodge nao podem iniciar durante cast/attack salvo se data permitir cancelamento.
- Durante dash/dodge, input direcional normal nao deve competir com movimento especial.
- Death/stun bloqueia tudo.

---

## 10. Eventos desejados

Reutilizar eventos existentes quando ja existirem.

Eventos novos/expandidos possiveis:

```text
PlayerDashStartedEvent
PlayerDashEndedEvent
PlayerMeleeAttackStartedEvent
PlayerMeleeAttackHitEvent
PlayerMeleeAttackEndedEvent
ProjectileSpawnedEvent
ProjectileHitEvent
ProjectileExpiredEvent
SpellProjectileSpawnedEvent
```

Regras:

- Eventos carregam tipos simples e IDs.
- Nao carregar GameObject/Transform/MonoBehaviour no payload.
- VFX/UI podem reagir aos eventos, mas gameplay principal nao deve depender de UI.

---

## 11. Arte e animacao

### 11.1 Placeholders iniciais

| Asset | Tamanho | Observacao |
|---|---:|---|
| Projectile_Arrow_32x8.png | 32x8 | Rotacionavel |
| Projectile_ArcaneBolt_16x16.png | 16x16 | Sprite simples com glow fake pixel art |
| Projectile_FireBolt_16x16.png | 16x16 | Hook visual |
| Projectile_IceShard_16x16.png | 16x16 | Hook visual |
| VFX_Impact_Physical_16x16.png | 16x16 | Burst curto |
| VFX_Impact_Arcane_16x16.png | 16x16 | Burst curto |
| Slash_Sword_Front_32x32.png | 32x32 | Overlay slash |
| Slash_Sword_Side_32x32.png | 32x32 | Overlay slash |
| Dash_Afterimage_32x48.png | 32x48 | Opcional |
| Dodge_Dust_16x16.png | 16x16 | Opcional |

### 11.2 Regras de import

- PPU 32.
- Filter Mode Point.
- Compression None.
- Generate Mip Maps false.
- Nao copiar assets de jogos existentes.

---

## 12. Fora de escopo desta frente

- Sistema de ammo.
- Block/parry.
- Heavy attack.
- Combos complexos.
- Lock-on target.
- Mouse aim completo.
- Gamepad completo.
- Magias de area complexas.
- Boss-specific attacks.
- Balanceamento final numerico.
- Arte final polida.

---

## 13. Criterios de pronto do refinamento

Este refinamento esta pronto para virar spec quando a spec executavel contiver:

- evolucao de projectile visual sem sistema paralelo;
- magia visual usando SpellDataSO;
- melee com fases e hitbox;
- dash e dodge separados;
- stamina/cooldown/collision/input lock;
- eventos simples;
- validacao por build e checklist Play Mode;
- escopo pequeno o bastante para agente executar sem reescrever SPEC 11/12.

---

## 14. Decisoes pendentes para o Rafa revisar

1. Dash deve ser habilidade base desde o inicio ou skill desbloqueavel?
2. Dodge deve ter i-frame agora ou manter sem i-frame ate balanceamento?
3. Ataque melee deve travar movimento ou apenas reduzir velocidade?
4. Flechas e magias devem aceitar diagonal agora ou apenas 4 direcoes?
5. Mouse aim entra agora ou fica para futuro?
6. Dash atravessa inimigos ou para no primeiro collider?
7. Magias devem aplicar status ja nesta spec ou apenas dano visual por elemento?

Recomendacao para a primeira implementacao:

```text
Dash base ou skill simples; dodge sem i-frame; melee reduz movimento; projeteis 4 direcoes; sem mouse aim; dash nao atravessa parede; magias sem AoE e sem status obrigatorio.
```
