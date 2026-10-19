# Skills SDD v1 — validação da fase mecânica 1B

> **Spec:** `spec_skills_10b_action_timing_commit_v1`  
> **Data:** 2026-09-10  
> **Status:** PASS

## Resultado

- M1 usa 0,10/0,10/0,20 e M2 usa 0,20/0,18/0,38 a partir dos assets vivos.
- Readiness ocorre antes do windup; custo, efeito e cooldown são confirmados uma vez no commit.
- `Active` só é publicado depois do commit aceito. Falha no commit volta a Idle sem feedback visual falso.
- Dano, modal, morte e troca de cena cancelam antes do commit sem custo; recuperação bloqueia outro cast.
- Cooldown continua compartilhado por `actionId`, inclusive ao trocar ou duplicar slot.
- O registry em `Resources/Skills/SkillRuntimeCatalogRegistry` conecta 5 árvores, 66 nós e 31 actions geradas ao runtime.

## Evidência Unity

| Gate | Resultado | Evidência |
|---|---:|---|
| Geração + compile | PASS | `Logs/skills-phase10b11-generate-wired3.log` |
| EditMode Skills | PASS — 61/61 | `Logs/skills-phase10b11-editmode-final6.xml` |
| PlayMode foundation | PASS — 14/14 | `Logs/skills-phase10b-foundation-playmode-final2.xml` |

## Revisão independente

PASS não regressivo. A primeira auditoria bloqueou o closeout por evento Active prematuro, dependência
Equipment→Combat e assets gerados desconectados. Os três pontos foram corrigidos e as suítes foram repetidas.

