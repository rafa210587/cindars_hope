---
name: rng-and-determinism
description: Usa RNG seeded por sistema para qualquer randomness que toque saves, loot, weather, spawns ou generation. Generaliza o padrão de stable-run da cave (FASE9F) para todos os sistemas. Use sempre que adicionar ou mudar randomness em qualquer ponto do gameplay.
---

# Skill: RNG e Determinism

O precedente do projeto está na geração da cave (stable-run, FASE9F): seeds determinísticos derivados de `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`; revisits nunca fazem reroll; o seed só muda em new game / KO / comando explícito de debug (rule: cave-stable-run). **Aplique a mesma disciplina em todos os outros sistemas.**

## Regras para qualquer novo randomness

1. **Nunca use `UnityEngine.Random` (global state) em sistemas cujo outcome é salvo ou revisitado** — loot rolls, weather generation, spawn composition, quality rolls, variação de NPC schedule. Global state significa que a chamada de qualquer outro sistema reordena os seus resultados.
2. **Um `System.Random` por sistema por escopo**, seeded a partir de componentes estáveis:
   ```csharp
   int seed = StableHash(worldSeed, "loot", enemyId, dayNumber);   // order-insensitive system salt
   var rng = new System.Random(seed);
   ```
   Use um string hash determinístico (ex.: FNV-1a sobre a chave composta) — **não** `string.GetHashCode()` (varia por runtime/process) e nunca GUIDs/timestamps para stable IDs (rule: cave-stable-run).
3. **Mesmo trigger, mesmo resultado:** reabrir o mesmo chest, reentrar no mesmo cave level, refazer o reroll do weather do mesmo dia depois do reload deve produzir outcomes idênticos. Se o reroll FOR o design (daily shop stock), o day number entra no seed.
4. **Faça o roll no momento da decisão e persista o OUTCOME** no save data — não persista o RNG state e não faça reroll no load.
5. **Randomness visual-only é exceção** (particle jitter, offsets de idle animation): `UnityEngine.Random` está ok ali — os outcomes nunca são salvos nem relevantes para o gameplay.
6. **Telegraph para RNG de alto risco:** chances de rare-drop ou critical que gateiam progressão devem ser inspecionáveis nos design docs (percentuais da drop table no loot SO, validados pelo catalog validator), nunca enterradas como magic numbers no código.

## Testes (skill: editmode-test-authoring)

- Mesmos inputs de seed → sequência/outcome idênticos (duas vezes no mesmo teste).
- Salt diferente por sistema → sequências diferentes (sem correlação cross-system).
- Outcome persistido: save→load→mesmo resultado sem reroll.

## Onde se aplica

fable_09 (variedade de cave biome layout — DEVE permanecer dentro do stable-run contract), fable_06/24 (loot tables, affixes), fable_31 (unidentified magic items), fable_37 (festivals/lunar events).
