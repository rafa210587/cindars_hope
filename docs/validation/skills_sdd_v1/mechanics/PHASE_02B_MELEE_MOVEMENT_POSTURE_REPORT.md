# Skills SDD v1 — Fase 02B: movimento, posture e provocação melee

**Spec:** `spec_skills_12_melee_movement_posture_v1`  
**Data:** 2026-09-10  
**Resultado:** PASS

## Comportamento validado

- Avanço, Arrancada, Salto, Grito e Quebra-Guarda usam os dados autorados por rank.
- Movimento delegado a `PlayerMovementDisplacementResolver`; trigger de inimigo não bloqueia dash e parede sólida limita o deslocamento.
- Salto recusa destino ocupado antes do gasto.
- Bônus de posture depende de recovery/block; Quebra-Guarda conserva posture ×3.
- Taunt usa resistência Normal 100%, Elite 60% e Boss 30% somente em janela explícita.
- DR por alvo aplica 100/60/30% e quatro segundos de imunidade após a terceira aplicação elegível.
- Falha anterior ao commit não gasta recurso; interrupção depois do commit conserva custo e cooldown.

## Correções encontradas durante a validação

1. O primeiro compile revelou um namespace ausente no teste novo; corrigido antes da evidência final.
2. O adaptador de reação dependia da ordem de `Awake`; passou a resolver seus componentes de forma idempotente.
3. O overlap melee não incluía colliders trigger usados por inimigos. O filtro agora inclui triggers e continua selecionando apenas `EnemyHealth`.
4. O cenário de dash foi isolado do movimento autônomo do inimigo e separa o alvo atravessado do alvo atingido no arco frontal final.

## Evidência fresca

- Geração e wiring do catálogo: `Logs/skills-phase12-generate2.log` — PASS; 66 nós, 5 árvores e 31 ações.
- Skills EditMode final: `Logs/skills-phase13-editmode-final.xml` — 73/73 PASS.
- PlayMode final integrado foundation + melee + ranged: `Logs/skills-phase13-regression-playmode-final.xml` — 29/29 PASS.
- Revisão independente: COMPLIANT, sem blocker ou warning residual; risco de manutenção LOW.

## Limites

- A seleção explícita de ponto do Salto já existe no resolver, mas o controlador de input atual fornece direção frontal; targeting visual pertence à fase de apresentação.
- Balanceamento comparativo ainda depende da `spec_skills_21_balance_acceptance_v1`; os números permanecem tuning v1 até essa aceitação.
