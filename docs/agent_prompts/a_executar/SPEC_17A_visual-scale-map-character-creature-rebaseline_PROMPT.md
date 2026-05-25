# Prompt para Claude Code/Codex — Cindar's Hope

**Use o fluxo oficial:**

```text
/implement-spec spec_17a_visual-scale-rebaseline
```

Isso delegará ao comando `/implement-spec` que orquestra validações, regressão e closeout automático.

**Consulte:**
- `.claude/commands/implement-spec.md` — Fluxo completo
- `.claude/skills/spec-execution/SKILL.md` — Pattern de execução

**Regras:** Implementar somente SPEC 17A, não ampliar escopo, validação humana no final.

---

## SPEC 17A — Visual Scale, Map Size, Character & Creature Size Rebaseline

Criar e implementar o rebaseline de escala visual e espacial do jogo: mapas 4x área, player 2x, categorias formais de tamanho para criaturas, ajustes de colliders, ranges e offsets sem quebrar arquitetura ou save.

Consulte `docs/specs/a_implementar/spec_visual_scale_map_character_creature_rebaseline.md` ou `docs/specs/implementados/spec_visual_scale_map_character_creature_rebaseline.md` para detalhes completos.
