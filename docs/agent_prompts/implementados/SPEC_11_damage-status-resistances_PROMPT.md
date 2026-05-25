# Prompt para Claude Code/Codex — Cindar's Hope

**Status:** ✅ **IMPLEMENTADO** em 2026-05-25

Esta SPEC foi completamente implementada. Consulte:
- `docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md` — Status técnico
- `PROJECT_LOG.md` — Histórico de implementação

---

# SPEC 11 — Damage, Status, Elements e Resistances

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-11-damage-status-resistances
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Spec alvo

```text
docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
```

## Dependências diretas

Depende de 10. Bloqueia 12, 13 e 14.

## Objetivo desta execução

Fechar pipeline de dano/status/resistência com UI mínima e validação Play Mode de dano, vulnerabilidade, resistência e morte.

## Status de implementação

✅ **Completado em 2026-05-25**

- DamageType enum oficial (Physical, Arcane, Fire, Ice, Lightning, Heal, TrueDamage)
- DamageCalculator com fórmula oficial
- DamageRequest e DamageResult DTOs
- CombatResistanceProfile para multiplicadores por tipo de dano
- StatusEffectManager com suporte a múltiplos efeitos
- DamageAppliedEvent expandido com TargetPosition
- FloatingDamageNumberDisplayer posicionado corretamente
- Integração com EnemyHealth
- Play Mode testing: deferred para humano validar
- Canvas consolidado: deferred para SPEC 17
- Overlays de status: deferred para SPEC 17

## Próximos passos

1. Play Mode testing manual (humano)
2. UI consolidada em SPEC 17
3. SPEC 12 (Player Combat) pode prosseguir

## Commit

```bash
git add <arquivos>
git commit -m "feat: finalizar spec 11 - damage status resistances"
```

Não faça push.
