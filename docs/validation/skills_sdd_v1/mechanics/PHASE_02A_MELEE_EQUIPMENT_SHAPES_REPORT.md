# Skills SDD v1 — validação da fase mecânica 2A

> **Spec:** `spec_skills_11_melee_equipment_shapes_v1`  
> **Data:** 2026-09-10  
> **Status:** PASS

## Resultado

- Corte da Mão Secundária exige adaga melee leve e íntegra em `LeftHand`, antes de cobrar stamina.
- As três adagas canônicas aceitam mão esquerda/direita e recebem durabilidade ao primeiro equip.
- O alcance 1,2 é medido desde o caster; o arco frontal não ganha alcance oculto.
- Corte Giratório é simétrico em 360°, atinge seis, aplica 100/100/100/70/70/70% e deduplica colliders.
- Whiff continua sendo commit válido; offhand removida ou quebrada no windup cancela sem custo/cooldown.

## Evidência Unity

| Gate | Resultado | Evidência |
|---|---:|---|
| Geração de items | PASS | `Logs/skills-phase11-items-generate.log` |
| EditMode Skills | PASS — 61/61 | `Logs/skills-phase10b11-editmode-final6.xml` |
| PlayMode melee + Cave | PASS — 7/7 | `Logs/skills-phase11-shapes-playmode.xml` |

## Revisão independente

PASS. O revisor confirmou forma, cooldown autorado, gate real na Cave, commit, wiring de dados e fronteira
Equipment/Combat após as correções. A aceitação visual pertence às fases posteriores.

