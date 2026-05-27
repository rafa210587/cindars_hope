# refinamento_combat_movement_projectiles_melee_visuals

> Status: Refinamento detalhado a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_combat_movement_projectiles_melee_visuals_runtime.md`
> Ordem de execucao: 18
> Depende de: SPEC 09, SPEC 10, SPEC 11, SPEC 12, SPEC 16 e SPEC 17/input-modal closeout quando aplicavel
> Objetivo: evoluir o combate runtime com projeteis visuais, magias visuais com status, ataques melee com animacao/hitbox e movimentos de dash/dodge com stamina, cooldown, colisao, input lock e loadout inicial de teste.

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
- magias precisam aplicar status quando configuradas, especialmente fireball/burn;
- ataques melee precisam ter animacao, timing e hitbox clara;
- dodge existe como movimento simples, mas precisa ser refinado com i-frame;
- dash precisa existir como skill comprada/liberada para teste;
- o personagem precisa iniciar com loadout minimo para testar combate sem setup manual pesado;
- os movimentos especiais precisam respeitar input lock, stamina, cooldown, colisao e UI/modal state.

---

## 3. Decisoes finais do Rafa

| Tema | Decisao |
|---|---|
| Numero da spec | SPEC 18 |
| Distancia da flecha | 10 world units |
| Dash | E skill, mas deve iniciar comprada/liberada para teste |
| Dodge i-frame | Sim, ganha i-frame agora |
| Movimento durante melee | Nao trava; reduz movimento em 30% |
| Direcao de flecha/magia | 8 direcoes: cardinais + diagonais |
| Mouse aim | Fora por enquanto |
| Dash e inimigos | Para no collider/inimigo; nao atravessa |
| Magias e status | Magias aplicam status quando configuradas |
| Fireball | Criar item/spell para liberar/testar Bola de Fogo |
| Loadout inicial | Personagem inicia com armas, arco, flechas e armaduras para teste |

---

## 4. Regra central

```text
Dano continua no pipeline existente.
Visual, timing, trajetoria, hitbox, status configurado e movimento entram nesta spec.
```

Nao recriar:

- DamageCalculator;
- Stamina;
- Mana;
- Inventory;
- Equipment;
- SkillTree;
- ActiveSkillSlots;
- SaveManager.

A SPEC 18 deve integrar os sistemas existentes e, quando necessario, criar apenas adaptadores pequenos.

---

## 5. Separacao refinamento vs spec

Este documento guarda contexto, decisoes e regras de design.

A spec executavel fica em:

```text
docs/specs/a_implementar/spec_combat_movement_projectiles_melee_visuals_runtime.md
```

Separacao:

```text
Refinamento = intencao, comportamento, decisoes, riscos e tradeoffs.
Spec = escopo fechado, arquivos-alvo, tasks, aceite, validacao e prompt executor.
```

---

## 6. Flechas / projeteis fisicos

### 6.1 Intencao

Flecha deve ser um objeto visivel que sai do jogador, viaja pelo mundo, colide com inimigo ou obstaculo, aplica dano uma vez e desaparece.

### 6.2 Comportamento desejado

| Aspecto | Decisao |
|---|---|
| Origem | Player + offset frontal/mao |
| Direcao | Input direcional ativo; se nao houver input, facing direction atual |
| Direcoes validas | 8 direcoes, incluindo diagonais normalizadas |
| Movimento | Linear |
| Alcance | 10 world units |
| Velocidade inicial | Data-driven no WeaponDataSO ou ProjectileVisualDataSO |
| Colisao alvo | Primeiro alvo valido recebe dano |
| Colisao obstaculo | Despawn ao bater em parede/collider solido |
| Hit multiple targets | Nao no recorte inicial |
| Piercing | Hook futuro por flag/data |
| Mouse aim | Fora do recorte inicial |
| Visual | Sprite rotacionado conforme direcao |
| Impacto | VFX simples no hit/despawn |

### 6.3 Flechas como item

O personagem deve iniciar com flechas no inventario para teste de loadout.

Decisao de escopo:

```text
SPEC 18 pode implementar consumo minimo de flecha para arco se a integracao com InventoryManager for direta e segura.
Nao implementar economia completa de ammo, crafting de flechas, quiver, diferentes tipos de flecha ou UI avancada de munição.
```

Regra recomendada:

- arco exige `item_arrow_basic`;
- ao disparar, remove 1 flecha;
- se nao houver flecha, ataque falha com feedback/evento simples;
- se o consumo criar risco alto, manter flecha como item inicial de teste e registrar consumo como pendencia explicita.

### 6.4 Dados desejados

Campos possiveis em WeaponDataSO ou sub-data de projectile:

```text
ProjectilePrefab
ProjectileSpeed
ProjectileRange = 10
ProjectileRadius
ProjectileDamageMultiplier
ProjectileLifetime
ProjectileImpactVfx
CanPierce = false
MaxPierceTargets = 0
AmmoItemId = item_arrow_basic
AmmoConsumedPerShot = 1
```

---

## 7. Magias visuais com status

### 7.1 Intencao

Magias de ataque devem aparecer no mundo como projeteis ou efeitos visuais, usando o mesmo pipeline de dano e recursos do SpellDataSO.

### 7.2 Fireball obrigatoria para teste

Criar uma Bola de Fogo testavel.

Loadout inicial deve conter um item de desbloqueio ou prova visual:

```text
item_spell_tome_fireball
```

Decisao de comportamento:

- se ja existir fluxo de desbloqueio de spell por item, o item libera `spell_fireball`;
- se nao existir fluxo seguro, o personagem pode iniciar com `spell_fireball` ja desbloqueada e o item fica no inventario como placeholder/hook documental;
- nao bloquear o teste por ausencia de UI final de unlock.

### 7.3 Tipos iniciais

| Spell | Visual | Movimento | DamageType | Status |
|---|---|---|---|---|
| Fireball | bola/projetil de fogo | Linear 8 direcoes | Fire | Burn |
| Arcane Bolt | orb/bolt arcano | Linear 8 direcoes | Arcane | Opcional: ArcaneMark futuro |
| Ice Shard | estilhaco de gelo | Linear 8 direcoes | Ice | Chill/Frozen hook |
| Lightning Spark | raio curto/rapido | Linear 8 direcoes | Lightning | Shock hook |

Para a implementacao inicial, Fireball + Burn e obrigatorio. Os demais podem ficar como hooks ou placeholders se ja existirem dados.

### 7.4 Status Burn

Status minimo para Fireball:

```text
StatusEffectId = status_burn
Tipo: dano ao longo do tempo ou marcador visual conforme StatusEffectManager existente permitir
Duracao sugerida: 3s-5s
Tick sugerido: 1 dano/s ou equivalente ja suportado pelo sistema atual
Stack: nao stackar no MVP; renovar duracao
```

Se o sistema atual nao tiver dano por tick implementado de forma segura, aplicar status como marcador com evento/visual e registrar DoT como pendencia. Nao criar um segundo StatusEffectManager.

### 7.5 Comportamento desejado

| Aspecto | Decisao |
|---|---|
| Custo | Mana via SpellDataSO |
| Cooldown | SpellDataSO/ActiveSkillSlots |
| Direcao | Input direcional ativo; fallback facing direction |
| Direcoes validas | 8 direcoes normalizadas |
| Alcance | SpellDataSO.Range |
| Velocidade | SpellDataSO ou ProjectileVisualDataSO |
| Status effect | Aplicar StatusEffectId quando configurado |
| VFX impacto | Por elemento |
| Area of effect | Fora do recorte inicial |
| Mouse aim | Fora do recorte inicial |

---

## 8. Ataque melee com animacao e hitbox

### 8.1 Intencao

Ataques melee devem ter sensacao fisica: preparacao, frame ativo, slash visual, hitbox e recovery.

### 8.2 Fases

| Fase | Papel | Duracao inicial sugerida |
|---|---|---|
| Wind-up | Preparar o golpe | 0.08s-0.15s |
| Active | Hitbox aparece e aplica dano | 0.08s-0.12s |
| Recovery | Retorno ao controle normal | 0.15s-0.30s |

### 8.3 Movimento durante ataque

Decisao final:

```text
Melee nao trava o jogador.
Durante o ataque, velocidade do jogador e reduzida em 30%.
Movimento efetivo = MoveSpeed * 0.70.
```

Aplicar durante wind-up, active e recovery, salvo se WeaponDataSO definir outra curva no futuro.

### 8.4 Hitbox

| Arma | Hitbox inicial |
|---|---|
| Unarmed | pequeno box frontal |
| Sword | arco/box frontal medio |
| Axe futuro | arco mais largo, recovery maior |
| Spear futuro | box estreito longo |

### 8.5 Regras

- Hitbox aplica dano uma vez por alvo por ataque.
- Hitbox segue facing direction e aceita coerencia com diagonal se houver input diagonal.
- Dano usa DamageRequest/DamageCalculator existentes.
- Slash visual nao deve ser a fonte da verdade do dano; ele acompanha o timing.
- AnimationEvent pode ser hook futuro, mas controller deve funcionar por timer para reduzir fragilidade.
- E continua com prioridade de interacao quando houver candidato interagivel.

---

## 9. Dash

### 9.1 Intencao

Dash e movimento ofensivo/engage e deve ser diferente de dodge.

Decisao final:

```text
Dash e uma skill, mas deve iniciar comprada/liberada para teste.
```

### 9.2 Comportamento inicial

| Aspecto | Decisao |
|---|---|
| Desbloqueio | Skill comprada/liberada no new game para teste |
| Direcao | Facing direction atual ou input direcional ativo, se existir |
| Direcoes validas | 8 direcoes normalizadas |
| Distancia | 6 world units |
| Velocidade | MoveSpeed * 2.0 |
| Custo | Stamina medio/alto |
| Cooldown | Maior que dodge |
| Input | Skill action/active slot; pode ter hotkey auxiliar para teste se ja existir padrao |
| Colisao | Para em parede, obstaculo ou inimigo |
| I-frame | Nao por padrao, salvo skill futura especifica |
| Dano | Nao causa dano por padrao; hook futuro para skill ofensiva |

### 9.3 Skill liberada para teste

Recomendacao de dado:

```text
skill_dash_forward
skill_action_dash_forward
```

Regras:

- deve aparecer como comprada/desbloqueada no estado inicial;
- se ActiveSkillSlots suportar preload, equipar em um slot padrao para teste, preferencialmente `R`;
- se nao for seguro alterar preload de slots, manter input auxiliar documentado para teste e registrar pendencia.

### 9.4 Colisao com inimigos

Dash para ao tocar inimigo/collider.

Nesta spec, dash nao atravessa inimigo e nao causa dano automaticamente.

---

## 10. Dodge

### 10.1 Intencao

Dodge deve ser movimento defensivo curto, rapido e com i-frame.

Decisao final:

```text
Dodge ganha i-frame agora.
```

### 10.2 Comportamento inicial

| Aspecto | Decisao |
|---|---|
| Direcao primaria | Input atual WASD/setas |
| Sem input | Recuar em direcao oposta ao facing direction |
| Direcoes validas | 8 direcoes normalizadas |
| Distancia | 3 world units |
| Velocidade | MoveSpeed * 3.0 |
| Custo | Stamina baixo/medio |
| Cooldown | Curto |
| Input | Space |
| Colisao | Para em obstaculo/inimigo/collider |
| I-frame | Sim |
| Dano | Nao causa dano |

### 10.3 I-frame

Recomendacao inicial:

```text
DodgeIFrameDurationSeconds = 0.18
```

O i-frame deve ser data-driven.

Regras:

- durante i-frame, dano recebido deve ser ignorado ou marcado como evitado pelo pipeline existente;
- nao criar um segundo sistema de HP;
- publicar evento ou flag simples de invulnerabilidade temporaria;
- UI/VFX podem reagir, mas gameplay deve funcionar sem UI.

### 10.4 Direcoes permitidas

Permitidas:

- esquerda;
- direita;
- tras;
- diagonais defensivas;
- direcao do input se o jogador pressionar uma direcao valida.

Dodge para frente e aceito se o input estiver para frente, mas a diferenca mecanica fica no alcance: dodge curto/rapido com i-frame, dash longo/ofensivo sem i-frame por padrao.

---

## 11. Loadout inicial de teste

O personagem deve iniciar com itens suficientes para validar a SPEC 18 sem setup manual pesado.

### 11.1 Itens iniciais sugeridos

```text
item_weapon_training_sword
item_weapon_basic_bow
item_arrow_basic x99
item_armor_cloth_chest
item_armor_cloth_legs
item_armor_cloth_boots
item_spell_tome_fireball
```

Se o projeto ja possuir ids equivalentes, usar os existentes e nao duplicar.

### 11.2 Equipamento inicial

Recomendacao:

- espada equipada em uma mao;
- arco disponivel no inventario/equipavel;
- armadura inicial equipada se EquipmentManager ja suportar preload seguro;
- Fireball desbloqueada ou liberavel pelo item;
- dash skill comprada/liberada e testavel.

### 11.3 Regra de save/load

Se alterar estado inicial, garantir que:

- new game recebe loadout;
- save existente nao quebra;
- migrations ou defaults preservam compatibilidade;
- itens sao IDs estaveis, nao referencias Unity em JSON.

---

## 12. Input e estados de controle

### 12.1 Estados necessarios

```text
Normal
Attacking
Casting
Dashing
Dodging
Invulnerable
Stunned/Dead
ModalBlocked
```

### 12.2 Regras

- Modal aberto bloqueia ataque, cast, dash, dodge, hotbar e movimento.
- Interacao em E continua tendo prioridade sobre ataque da mao direita quando houver candidato de interacao.
- Dash/dodge nao podem iniciar durante cast/attack salvo se data permitir cancelamento no futuro.
- Durante dash/dodge, input direcional normal nao deve competir com movimento especial.
- Death/stun bloqueia tudo.
- Diagonal deve ser normalizada para evitar velocidade maior.

---

## 13. Eventos desejados

Reutilizar eventos existentes quando ja existirem.

Eventos novos/expandidos possiveis:

```text
PlayerDashStartedEvent
PlayerDashEndedEvent
PlayerInvulnerabilityStartedEvent
PlayerInvulnerabilityEndedEvent
PlayerMeleeAttackStartedEvent
PlayerMeleeAttackHitEvent
PlayerMeleeAttackEndedEvent
ProjectileSpawnedEvent
ProjectileHitEvent
ProjectileExpiredEvent
SpellProjectileSpawnedEvent
StatusAppliedBySpellEvent
```

Regras:

- eventos carregam tipos simples e IDs;
- nao carregar GameObject/Transform/MonoBehaviour no payload;
- VFX/UI podem reagir aos eventos, mas gameplay principal nao deve depender de UI.

---

## 14. Arte e animacao

### 14.1 Placeholders iniciais

| Asset | Tamanho | Observacao |
|---|---:|---|
| Projectile_Arrow_32x8.png | 32x8 | Rotacionavel para 8 direcoes |
| Projectile_Fireball_16x16.png | 16x16 | Obrigatorio |
| Projectile_ArcaneBolt_16x16.png | 16x16 | Hook/placeholder |
| Projectile_IceShard_16x16.png | 16x16 | Hook/placeholder |
| VFX_Impact_Physical_16x16.png | 16x16 | Burst curto |
| VFX_Impact_Fire_16x16.png | 16x16 | Burst curto |
| VFX_Status_Burn_16x16.png | 16x16 | Indicador simples |
| Slash_Sword_Front_32x32.png | 32x32 | Overlay slash |
| Slash_Sword_Side_32x32.png | 32x32 | Overlay slash |
| Dash_Afterimage_32x48.png | 32x48 | Opcional |
| Dodge_Dust_16x16.png | 16x16 | Opcional |
| Dodge_IFrame_Glint_16x16.png | 16x16 | Opcional |

### 14.2 Regras de import

- PPU 32.
- Filter Mode Point.
- Compression None.
- Generate Mip Maps false.
- Nao copiar assets de jogos existentes.

---

## 15. Fora de escopo desta frente

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
- Economia completa de municao.
- Craft de flechas.
- Quiver, tipos avancados de flecha ou UI completa de ammo.

---

## 16. Criterios de pronto do refinamento

Este refinamento esta pronto para execucao quando a spec executavel contiver:

- evolucao de projectile visual sem sistema paralelo;
- flecha com range 10;
- flecha/magia em 8 direcoes;
- magia visual usando SpellDataSO;
- Fireball com status Burn;
- melee com fases e hitbox;
- melee com movimento reduzido em 30%, sem travar;
- dash como skill comprada/liberada para teste;
- dodge com i-frame;
- starter loadout com armas, arco, flechas, armaduras e item/fireball;
- stamina/cooldown/collision/input lock;
- eventos simples;
- validacao por build, validator e checklist Play Mode;
- escopo pequeno o bastante para agente executar sem reescrever SPEC 11/12/16/17.

---

## 17. Pontos sem duvida bloqueante

Nao ha duvida bloqueante para gerar a spec.

Pontos que devem ser tratados como decisoes de implementacao segura pelo agente:

1. Se ja existir sistema de unlock por item, usar para Fireball. Se nao existir, iniciar Fireball desbloqueada e manter item como hook.
2. Se ja existir suporte limpo a ammo, consumir `item_arrow_basic`. Se nao existir, criar consumo minimo e seguro; se isso ameaçar escopo, registrar pendencia sem bloquear projectile visual.
3. Se ActiveSkillSlots suportar preload seguro, equipar Dash no slot `R`. Se nao suportar, deixar Dash liberado por hotkey temporaria/testavel e registrar pendencia.
4. Se StatusEffectManager nao tiver DoT seguro, aplicar Burn como status visual/eventual e registrar DoT como pendencia; nao criar status manager paralelo.
