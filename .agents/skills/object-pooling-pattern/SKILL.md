---
name: object-pooling-pattern
description: Introduz object pooling para spawns de alto churn (projectiles, floating damage text, drops, enemy waves) sem quebrar os contratos dos spawn services existentes. Use ao tocar em ProjectileSpawnService, spawners, ou qualquer código sinalizado pelo performance-auditor por churn de Instantiate/Destroy.
---

# Skill: Object Pooling Pattern

**Baseline do projeto:** ainda NÃO há pooling em lugar nenhum. `ProjectileSpawnService.SpawnProjectile()` faz `Object.Instantiate(prefab)` / `Object.Destroy()` por shot; `CaveEnemySpawner` instancia waves. O combat de bow/spell torna isso GC churn por ataque.

## Contrato do pool

```csharp
namespace CindarsHope.Core.Pooling
{
    /// Plain C# pool keyed by prefab (or visual-style for runtime-built projectiles).
    public class GameObjectPool
    {
        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        private readonly GameObject _prefab;
        private readonly Transform _parent;   // inactive container, set once by bootstrap

        public GameObject Get(Vector3 position)
        {
            GameObject go = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, _parent);
            go.transform.position = position;
            go.SetActive(true);
            return go;
        }

        public void Release(GameObject go)
        {
            go.SetActive(false);
            _inactive.Push(go);
        }
    }
}
```

## Regras inegociáveis ao fazer pooling neste projeto

1. **Reset completo de state no Get ou Release.** Um `ProjectileBehaviour` poolado deve re-executar o equivalente a `Initialize(...)`: direction, speed, range traveled, damage, status effect, **hit counter (`SetMaxHits`) e quaisquer acumuladores per-flight**. Um projectile poolado que lembra hits antigos é um bug de gameplay, não de perf.
2. **Substitua `Destroy(this.gameObject)` por `Release` via callback/event** — o projectile não pode conhecer o pool; o spawn service é dono dele. Mantenha os contratos `ProjectileSpawnResult`/`ProjectileSpawnRequest` inalterados para que os callers (BowArrowAttackService, SpellCastService) fiquem intocados.
3. **Sem global search para achar o pool** (rule: unity-architecture). O pool vive em/sob o GameBootstrap e é injetado no spawn service.
4. **Higiene de trail/particle:** chame `TrailRenderer.Clear()` / `ParticleSystem.Clear()` no reuse, ou o projectile faz teleport-streak pela tela.
5. **Higiene de physics:** zere `Rigidbody2D.velocity`/`angularVelocity` no Release; re-habilite os colliders se foram desabilitados no impact.
6. **Não poole one-shots** (boss intro FX, objetos one-per-day). Poole só o churn per-attack/per-wave.
7. **Cap + prewarm:** faça prewarm do tamanho típico de burst (ex.: 8 projectiles); permita growth; nunca hard-fail num pool vazio.

## Validação

- EditMode: testes de Get/Release/reuse com reset de state (a parte de C# puro).
- Play Mode human scenario: dispare 50+ arrows/spells, verifique que não há stale trails, nenhum double-hit de projectile reciclado, e que o profiler mostra zero GC alloc por shot (skill: gameplay-test-scenario).
