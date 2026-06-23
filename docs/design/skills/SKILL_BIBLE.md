# Bíblia de Skills — Cindar's Hope (índice)

> **Status:** PROPOSTA (design, sem código). Contrato de design que alimenta as specs de implementação
> (`fable_70` catálogo/executores, `fable_71` HUD). Aguarda revisão humana.
> **Data:** 2026-06-23.
> **Origem:** decisões fechadas em [`SKILL_CATALOG_DESIGN_REVIEW_v1.0.md`](../SKILL_CATALOG_DESIGN_REVIEW_v1.0.md) (seção 7).
> **Fontes de verdade dos números:** `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs` e
> `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs`.

Esta bíblia descreve **cada skill** das 5 árvores ao ponto de não restar dúvida: passivas com efeito/rank/cap/condição, e ativas com a forma exata de uso (alvo, alcance, dano, custo, cooldown, movimento, animação) **mais um Q&A fechado por skill** (sem alvo? em boss? empilha? trava o movimento? falta de recurso?).

## Documentos por árvore

| Árvore | Arquivo | Passivas | Ativas | Capstone | Cortes/Merges |
|---|---|---|---|---|---|
| Melee | [SKILL_BIBLE_melee.md](skills/SKILL_BIBLE_melee.md) | 5 | 7 | 1 (XOR Kanthor/Kaand) | corta `guarded_block` |
| Ranged | [SKILL_BIBLE_ranged.md](skills/SKILL_BIBLE_ranged.md) | 5 | 5 | 1 | — |
| Magic | [SKILL_BIBLE_magic.md](skills/SKILL_BIBLE_magic.md) | 4 | 7 | 1 (XOR Anya/Senya) | — |
| Survival | [SKILL_BIBLE_survival.md](skills/SKILL_BIBLE_survival.md) | 8 | 5 | 1 | corta `emergency_roll`; merge `sinal_retirada`→Disengage |
| Crafting | [SKILL_BIBLE_crafting.md](skills/SKILL_BIBLE_crafting.md) | 8 | 4 | 1 | corta `mecanismo_campo`; merge `field_patch`+`quick_repair`→Reparo de Campo; merge `marca_eficiencia`→Eficiência; 4 passivas-fantasma convertidas |

> Nota: o link da tabela é relativo a esta pasta; este índice vive em `docs/design/skills/SKILL_BIBLE.md`.
> (Os arquivos por árvore estão na mesma pasta `docs/design/skills/`.)

## Legenda do template (idêntico em todos os arquivos)

Cada skill segue, nesta ordem: **Cabeçalho (nome + id)** · **Árvore / Tier / Prereq / Custo SP / Tipo** · **Status no saneamento** (mantida | já-implementada | nova(implementar) | mesclada | convertida | cortada) · **Resumo** · **Descrição completa** · **Mecânica (ATIVA ou PASSIVA)** com todos os campos de comportamento · **Q&A fechado**.

## Resumo do saneamento aplicado

- **5 árvores mantidas** (Melee / Ranged / Magic / Survival / Crafting). Survival e Crafting saneadas.
- **3 cortes:** `melee_guarded_block`, `survival_emergency_roll`, `crafting.mecanismo_campo` (Block=Shift e Dodge=Space seguem como abilities puras; mecanismo era vago).
- **3 merges:** `sinal_retirada`→buff Disengage (Survival); `marca_eficiencia`→buff Eficiência (Crafting); `field_patch`+`quick_repair`→Reparo de Campo (Crafting).
- **4 passivas-fantasma de Crafting convertidas** para efeito real (nada comprável em 0%).
- **Lunge-strikes permanecem nos slots** (são ataques com gap-closer; o HUD guard só pega Dash/Dodge/Block puros).

## Dependências de implementação consolidadas (fable_70)

Itens da bíblia que dependem de sistema ainda inexistente e fecham em **fable_70**:
- **Cobrança de stamina/mana** das ativas (hoje não cobrado → balance fictício).
- **Executor de Buff** (elemental_ward, marked_prey/debuff, Disengage, Eficiência).
- **Executor de Spawn/Zona** (isca/decoy, slowing_sigils).
- **Mecânica de carga** (charged_shot — hold/release).
- **Reuso de durability** (Reparo de Campo) e **reuso de FarmCrop** (irrigador).
- **Posture/stagger**, **taunt/aggro**, **payoffs de capstone XOR**, e os efeitos reais das passivas convertidas.

A HUD que expõe as ativas (cooldown, custo, botão de uso) é a **fable_71**.
