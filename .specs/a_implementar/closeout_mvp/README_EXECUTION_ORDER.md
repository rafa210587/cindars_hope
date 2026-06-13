# Cindar's Hope — MVP Closeout Specs

> Pacote gerado em 2026-06-01 para fechar specs parciais/não concluídas antes de avançar para novas features.

## Fonte de verdade

Este pacote foi derivado de:

- `.specs/SPEC_EXECUTION_ORDER.md`
- `docs/IMPLEMENTATION_STATUS.md`
- `docs/backlog/reorg_architecture_residual_backlog.md`
- specs ativas em `.specs/a_implementar/`
- specs parciais já promovidas em `.specs/implementados/`

## Regra de numeração

As specs originais continuam com seus números históricos `00-17`.

Este pacote usa **SPEC_18+** para evitar reabrir ou sobrescrever specs antigas:

- SPEC_18: validação baseline e cleanup operacional.
- SPEC_19: fechamento save/inventory/farm/world partials.
- SPEC_20: equipment/durability/environment/loot.
- SPEC_21: damage/status/elements/resistances.
- SPEC_22: player combat/weapons/spells/skill actions.
- SPEC_23: enemy AI/roster/bestiary/faction locks.
- SPEC_24: cave runtime/generation/checkpoints/boss gates.
- SPEC_25: cave entry/death/Anya/corpse recovery.
- SPEC_26: skill trees/active slots/respec Anya.
- SPEC_27: visual scale/camera/sprite profiles.
- SPEC_28: UI/UX full gameplay.
- SPEC_29: final MVP acceptance/promotion.

## Regra global

Não implementar nova feature ampla antes da SPEC_18 passar.

Se uma spec encontrar regressão em gameplay já validado, parar e corrigir antes de seguir.

## Ordem segura

```text
SPEC_18
  -> SPEC_19
  -> SPEC_20
  -> SPEC_21
  -> SPEC_22
  -> SPEC_23
  -> SPEC_24
  -> SPEC_25
  -> SPEC_26
  -> SPEC_27
  -> SPEC_28
  -> SPEC_29
```

## Matriz de paralelização

| Spec | Paralelização | Pode executar junto com | Condição | Motivo |
|---|---:|---|---|---|
| SPEC_18 | Não | Nenhuma | Primeira sempre | Define baseline e valida se o repo está estável. |
| SPEC_19 | Não | Nenhuma | Depois da 18 | Toca save/inventory/farm/world, base de quase tudo. |
| SPEC_20 | Não | Nenhuma | Depois da 19 | Equipment/loot pode afetar combat, inventory e save. |
| SPEC_21 | Não | Nenhuma | Depois da 20 | Damage/status/resistance afeta combat, enemies e spells. |
| SPEC_22 | Não | Nenhuma | Depois da 21 | Player combat é crítico e toca input/Q/E/Space. |
| SPEC_23 | Parcial, só análise | SPEC_26 análise | Implementação sequencial | Enemy data pode ser analisado em paralelo com skills, mas implementação pode tocar save/UI/cave. |
| SPEC_24 | Não | Nenhuma | Depois da 23 | Cave depende de enemy spawn/bestiary. |
| SPEC_25 | Não | Nenhuma | Depois da 24 | Death/Anya/corpse depende de cave runtime. |
| SPEC_26 | Parcial, só análise | SPEC_23 análise | Implementação depois da 25 se usar Anya; antes só análise | Respec depende de Anya e save. |
| SPEC_27 | Sim, limitada | SPEC_23 análise ou SPEC_26 análise | Apenas se não editar scenes/camera/cave runtime em paralelo | Visual pode causar regressão em câmera/bounds. |
| SPEC_28 | Não | Nenhuma | Depois da 19-27 | UI final expõe todos os sistemas. |
| SPEC_29 | Não | Nenhuma | Última sempre | Aceitação integrada e promoção documental. |

## Validação mínima por spec

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
tools/docs/validate_docs.ps1
```

Se a spec alterar runtime Unity, scene creator, prefab, ScriptableObject, validator ou menu editor, rodar no Unity:

```text
CindarsHope/Repair and Validate Project
CindarsHope/Validate/Combat/Validate Projectile Prefabs
CindarsHope/Validate/Combat/Validate Combat Databases
CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP
CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP
```

## Política de status

Cada spec deve criar relatório em:

```text
docs/validation/spec_mvp_closeout_<NN>_<slug>_execution_report.md
```

Atualizar:

```text
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md, somente se status real mudar
```

## Stop conditions globais

Parar se:

- Unity não compilar.
- Build C# falhar.
- Validator crítico falhar.
- Play Mode detectar erro crítico novo.
- A spec exigir reescrever sistema fora do escopo.
- A implementação exigir editar YAML de scene/prefab manualmente sem Unity Editor.
- A spec conflitar com comportamento já validado de loja, hotbar, bow/arrow, fireball, save/load ou scene transitions.
