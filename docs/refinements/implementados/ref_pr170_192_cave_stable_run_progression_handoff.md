# REF — PR170-192 cave stable run progression handoff

> Origem histórica: `docs_old/audits/PR170_192_CAVE_STABLE_RUN_PROGRESSION_HANDOFF.md`
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# PR-170 a PR-192 — Cave Stable Run Progression Handoff

> **Status:** criado como handoff alvo antes da implementação.  
> **Branch alvo:** `feature/pr-170-192-cave-stable-run-progression`

---

## Objetivo

Implementar pacote FASE9F Cave Stable Run com replay de snapshots por nível visitado, ranges finais de inimigos/resource nodes e progressão base até boss gate level 15.

---

## Regra central

CaveLevel já visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.

ForwardExit/BackExit não podem regenerar layout/composição.

Somente novo jogo, KO/morte ou debug explícito podem criar nova run procedural.

---

## Próximo passo

Executar o roadmap:

```text
docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md
```

---

## Validação final obrigatória

- Unity compila sem erro vermelho.
- CaveScene é regenerável via menu.
- FarmScene/TownScene são regeneráveis se necessário.
- CaveLevel revisitado mantém layout, inimigos e resource nodes.
- Save/load preserva snapshots.
- KO/morte regenera run.
- Checkpoints persistem após KO/morte.




