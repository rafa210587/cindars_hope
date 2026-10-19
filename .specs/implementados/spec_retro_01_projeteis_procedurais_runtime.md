# Retro-Spec 01 — Projéteis Procedurais Runtime (fallback sem prefab)

> **Spec ID:** `spec_retro_01_projeteis_procedurais_runtime`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Combat / Projéteis / Visual procedural
> **Código que documenta:**
> - `Assets/_Game/Scripts/Combat/Weapon/ProjectileVisualStyle.cs`
> - `Assets/_Game/Scripts/Combat/Weapon/ProjectileVisualAnimator.cs`
> - `Assets/_Game/Scripts/Combat/Weapon/RuntimeProjectileFactory.cs`
> - `Assets/_Game/Scripts/Combat/EnemyProjectileBehaviour.cs`
> - `Assets/_Game/Scripts/Combat/Weapon/ProjectileSpawnService.cs` (integração fallback)
> **Evidência de execução:** `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md` (BUILD_VALIDATED_WITH_HUMAN_UNITY_ACTION_REQUIRED)
> **Supersedida/complementada por:** arte 2D autorada futura substituirá os sprites procedurais; o caminho de gameplay (ProjectileBehaviour) permanece.

---

# /speckit.specify

## Contexto

Na fase pré-arte, ataques de arco e magias morriam nos erros `BowHasNoProjectilePrefab` / `SpellHasNoProjectilePrefab` quando nenhum prefab autorado existia. A entrega de gameplay expansion (2026-06-12) removeu esse beco sem saída: qualquer disparo sem prefab gera um projétil totalmente funcional em runtime, com visual procedural (orbe pulsante ou haste de flecha), trilha e o MESMO `ProjectileBehaviour` usado por prefabs autorados — o caminho de gameplay é idêntico nos dois casos.

## Comportamento implementado

### 1. Estilos visuais (`ProjectileVisualStyle`)

Enum com 4 valores: `Auto`, `Arrow`, `MagicBolt`, `SkillBolt`.

- `Auto` resolve por tipo de dano: `DamageType.Physical` → `Arrow`; qualquer outro → `MagicBolt`.
- `SkillBolt` é usado pelos executores de skill (`ProjectileSkillEffectExecutor`, ver retro-spec 04).

### 2. Fábrica runtime (`RuntimeProjectileFactory.Create(style, damageType)`)

Constrói um `GameObject` `Projectile_{estilo}_{tipoDano}` com:

- `Rigidbody2D`: `gravityScale = 0`, `CollisionDetectionMode2D.Continuous`.
- `CircleCollider2D` trigger com raio `0.12`.
- `SpriteRenderer` com `sortingOrder = 5` e sprite procedural em cache estático:
  - Flecha: sprite sólido 32×32, escala `(0.55, 0.12, 1)` (haste alongada);
  - Bolt: sprite circular 32×32 com borda suave (alpha = `clamp01((radius - dist) / 2)` nos 2 pixels externos), escala uniforme `0.32`.
- Tints por tipo de dano (bolt): Fire `(1, 0.45, 0.15)`, Ice `(0.45, 0.8, 1)`, Toxic `(0.45, 0.85, 0.3)`, Lightning `(1, 0.95, 0.4)`, Arcane `(0.72, 0.45, 0.95)`, True `(0.95, 0.95, 0.95)`, default `(0.85, 0.8, 0.7)`. Flecha sempre `(0.78, 0.62, 0.38)`.
- `TrailRenderer` (material `Sprites/Default` em cache): flecha `time=0.08s`, largura inicial `0.05`; bolt `time=0.22s`, largura inicial `0.18`; largura final 0; cor do tint com alpha 0.65 → 0; `sortingOrder = 4`.
- `ProjectileVisualAnimator` configurado com `pulse = !isArrow`, `spin = false`.
- `ProjectileBehaviour` (o componente de gameplay padrão do projeto).

Sprites e material usam `HideFlags.HideAndDontSave` e cache estático — funcionam em player builds (sem sprites builtin do editor).

### 3. Animação em voo (`ProjectileVisualAnimator`)

Animação local-only (nunca afeta física):

- Defaults serializados: `_pulseFrequency = 9`, `_pulseAmplitude = 0.16`, `_flickerAmplitude = 0.12`, `_spinDegreesPerSecond = 540` (spin desabilitado por default).
- Onda: `sin((Time.time + phaseOffset) * freq * 2π)`; `phaseOffset` aleatório em `[0, 6.28)` por instância (dessincroniza pulsos — aleatoriedade visual transiente, permitida pelo contrato de determinismo).
- Pulso: `localScale = baseScale * (1 + wave * amplitude)`.
- Flicker: multiplica RGB por `1 + wave * flickerAmplitude` (clamp01), alpha preservado.
- `Configure(renderer, pulse, spin)`: `pulse = false` zera pulso e flicker (flechas mantêm haste estável).

### 4. Fallback no spawn service (`ProjectileSpawnService.SpawnProjectile`)

Ordem de decisão:

1. Direção com `sqrMagnitude < 0.001` → erro `DirectionZero`.
2. `request.Prefab != null` → `Object.Instantiate` do prefab autorado (caminho original SPEC_06).
3. `request.Prefab == null` → `RuntimeProjectileFactory.Create(request.VisualStyle, request.DamageType)` posicionado em `SourcePosition + Direction.normalized * SpawnOffset`.
4. Projétil sem `ProjectileBehaviour` → erro `MissingProjectileBehaviour` + destruição.
5. `SetMaxHits(request.MaxHits)`; inicialização com ou sem status effect (`InitializeWithStatus` quando `StatusEffect != null && StatusApplyChance > 0`).

### 5. Projétil inimigo (`EnemyProjectileBehaviour.SpawnTowards`)

Projétil disparado por inimigos (ações `RangedProjectile`/`CastProjectile` do `EnemyBrain` — ver retro-spec 02):

- Cria via `RuntimeProjectileFactory.Create(MagicBolt, damageType)` e **substitui** o `ProjectileBehaviour` (lado do player) por `EnemyProjectileBehaviour` — o projétil inimigo não pode ferir inimigos.
- Nome `EnemyProjectile_{sourceEnemyId}`; origem deslocada `0.45` na direção do alvo; rotação alinhada ao ângulo do vetor.
- Velocidade linear `direction.normalized * max(1, speed)`; dano `max(1, damage)`; alcance `max(1, range)` — autodestruição quando `distance(pos, spawnPos) > range`.
- No `OnTriggerEnter2D` com `PlayerController` (hit único via `_hasHit`): dano via `PlayerDamageReceiver.ApplyDamage(playerManager, damage, sourceEnemyId, damageType)` (redução central F03/F18 por Defense + resistência), publica `PlayerDamagedEvent`, mostra número flutuante (`FloatingDamageNumberDisplayer`), aplica knockback via `KnockbackController` quando `knockbackForce > 0`, destrói o projétil.

## Critérios de aceite (verificáveis no código atual)

1. `ProjectileSpawnService` nunca falha por ausência de prefab: requests com `Prefab == null` criam projétil procedural.
2. `RuntimeProjectileFactory` produz GameObject com Rigidbody2D (gravidade 0, continuous), trigger circular raio 0.12, sprite/tint por estilo+tipo de dano, trail e `ProjectileBehaviour`.
3. Bolts pulsam e flickeram; flechas não pulsam (Configure com `pulse:false`).
4. `EnemyProjectileBehaviour` dana apenas o player, via `PlayerDamageReceiver.ApplyDamage` + `PlayerDamagedEvent`, e nunca reusa o `ProjectileBehaviour` do player.
5. Nenhum `GameObject.Find` em runtime; sprites/material em cache estático funcionam em build.

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `Combat/Weapon/ProjectileVisualStyle.cs` | Enum de arquétipo visual (Auto/Arrow/MagicBolt/SkillBolt) |
| `Combat/Weapon/RuntimeProjectileFactory.cs` | Fábrica estática de projéteis procedurais (sprite, trail, física, behaviour) |
| `Combat/Weapon/ProjectileVisualAnimator.cs` | Animação local de pulso/flicker/spin (visual-only) |
| `Combat/Weapon/ProjectileSpawnService.cs` | Serviço central de spawn (SPEC_06) com fallback procedural |
| `Combat/EnemyProjectileBehaviour.cs` | Variante inimigo→player do projétil (espelha o pipeline de dano do player) |

## Contratos

- `RuntimeProjectileFactory.Create(ProjectileVisualStyle, DamageType) : GameObject` — estático, puro quanto a dependências de cena.
- `ProjectileSpawnRequest.VisualStyle` e `ProjectileSpawnRequest.MaxHits` — campos adicionados ao request existente do SPEC_06.
- `EnemyProjectileBehaviour.SpawnTowards(origin, direction, speed, range, damage, damageType, knockbackForce, sourceEnemyId, sourceEnemyName) : EnemyProjectileBehaviour` (null se direção ~zero).
- Eventos publicados: `PlayerDamagedEvent` (via GameEventBus) no hit inimigo→player.
- Logs de contrato: `CombatLog: ProjectileSpawned. Success=False, ErrorCode=...` nos erros do spawn service.

## Decisões e invariantes

- **Mesmo gameplay path com ou sem prefab**: o fallback adiciona apenas visual; `ProjectileBehaviour` é idêntico.
- **Aleatoriedade visual-only**: `Random.value` aparece apenas na fase do pulso — não toca conteúdo persistente/determinístico (compatível com ADR-0005 / cave stable run).
- **Sem assets**: nada é gravado em disco; texturas com `HideAndDontSave` — zero impacto em AssetDatabase.
- **Projétil inimigo não reusa o behaviour do player** (destruído e substituído) — evita friendly fire invertido.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Criar enum `ProjectileVisualStyle` (Auto/Arrow/MagicBolt/SkillBolt) em `CindarsHope.Combat.Weapon`.
2. Criar `RuntimeProjectileFactory` estático com sprites procedurais cacheados (círculo soft-edge 32px; retângulo sólido 32px), tints por `DamageType`, trail e os componentes físicos com os valores da seção "Comportamento implementado".
3. Criar `ProjectileVisualAnimator` com pulso/flicker/spin e `Configure(renderer, pulse, spin)`.
4. Estender `ProjectileSpawnRequest` com `VisualStyle` (default Auto) e `MaxHits`; no `ProjectileSpawnService`, trocar o erro de prefab ausente pelo fallback da fábrica.
5. Remover os bloqueios `BowHasNoProjectilePrefab`/`SpellHasNoProjectilePrefab` dos serviços de ataque de arco/magia.
6. Criar `EnemyProjectileBehaviour` com `SpawnTowards` (substituindo o `ProjectileBehaviour` da fábrica) e hit player-only via `PlayerDamageReceiver`.
7. Integrar no `EnemyBrain.ResolveAction` para ações `RangedProjectile`/`CastProjectile` (ver retro-spec 02).

## Débitos conhecidos

- Sem testes de caracterização para a fábrica (visual procedural depende de Play Mode humano; cenário em `docs/validation/playmode/gameplay_expansion_2026_06_12_human_test_scenario.md`).
- Arte 2D autorada pendente — quando existir, prefabs voltam a ter prioridade (caminho 1 do spawn service já cobre).
- `Shader.Find("Sprites/Default")` pode retornar null em builds com stripping agressivo (trail sem material — degradação apenas visual).
