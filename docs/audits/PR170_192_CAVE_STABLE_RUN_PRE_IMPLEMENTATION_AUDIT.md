# PR-170 a PR-192 — Cave Stable Run Pre-Implementation Audit

> **Data:** 2026-05-20  
> **Base:** `dev`  
> **Escopo:** registrar regra estável antes da implementação do pacote FASE9F Cave Stable Run.

---

## Estado observado

- Cave procedural já existe e materializa visualmente.
- `CameraFollow2D` existe e deve ser validado em Farm/Town/Cave.
- ForwardExit/BackExit possuem lógica de avanço/retorno, mas ainda precisam de validação Unity e hardening.
- `CaveGeneratedLevel` existe, mas não representa snapshot persistido de level visitado.
- `CaveSaveData` existe, mas ainda não contém `VisitedLevels`/snapshots.
- Enemy spawn atual usa spawn points do generated level, sem range final 12–20 e sem snapshot de composição.
- Resource nodes existem, mas precisam de replay estável e range final 4–10.

---

## Lacuna principal

A cave precisa deixar de ser apenas deterministicamente gerada por seed no momento da entrada e passar a registrar snapshot por level visitado dentro da run.

Sem isso, o jogo não consegue garantir formalmente que:

- nível visitado mantém composição;
- inimigos não rerollam;
- resource nodes não rerollam;
- save/load preserva exploração;
- daily refresh não troca conteúdo.

---

## Regra de implementação

Todo trabalho de Cave procedural deve seguir:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/amendments/stable_run_replay.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`

---

## Riscos

- Reescrever materializer para snapshot pode afetar visual procedural já existente.
- Save migration precisa ser tolerante a saves antigos.
- HUD precisa diferenciar nível gerado pela primeira vez vs replay de snapshot.
- FASE9G deve ser preparada por campos, mas não implementada integralmente neste pacote.

---

## Recomendação

Executar PR-170 a PR-192 em branch única:

```text
feature/pr-170-192-cave-stable-run-progression
```

Abrir PR único contra `dev` ao final.
