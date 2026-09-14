---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Skills fase 21: matriz de aceitação alcançável — proposta v1

> 2026-09-12. Refinamento para revisão de design; não altera a spec aprovada nem autoriza
> mudanças de gameplay. Evidência de execução: [relatório da fase 21](../../validation/skills_sdd_v1/balance/PHASE_09_BALANCE_ACCEPTANCE_REPORT.md).

## Resultado pretendido e fato observado

O jogador deve poder escolher uma fantasia de combate coerente com Kanthor/Kaand, as luas ou Anya
nos estágios de caverna correspondentes, pagando pontos, recursos e risco. Em Farm e Town, Telisandra
e Thoren devem apoiar preparação, retirada e ofício sem eliminar provisões ou criar renda infinita.
A aceitação de equilíbrio precisa comparar **builds que um save real consegue comprar e equipar**.

A [spec aprovada da fase 21](../../../.specs/a_implementar/spec_skills_21_balance_acceptance_v1.md)
fixa R1/R3 em L20/L50 para nove builds. O PlayMode de compra pela API real, usando apenas os pontos
por nível, materializou **0/28** combinações ofensivas; outras **8/8** linhas de Telisandra/Thoren
não tinham ação ofensiva no manifesto. O smoke de assets passou, mas ele verifica existência,
nível mínimo e rank autorado; não comprova orçamento, compra, equipar ou combate. Não há TTK medido.

## Regras e limites verificáveis

`PlayerProgressionRules.CalculateTotalSkillPointsAtLevel` dá `floor(level/2)` pontos:
L20 = 10; L50 = 25. [quest_rules.md](../../game_rules/quest_rules.md) concede mais um ponto
em cada Ato concluído; Atos 1 e 2 podem elevar L20 a 11 e L50 a 27, respectivamente.
`SkillTierRules` requer 5/11/18/26 pontos **gastos na mesma árvore** antes dos tiers 2/3/4/5.
Cada compra de nó ou rank custa outro ponto. Os níveis abaixo são limites inferiores analíticos,
não substituem uma compra PlayMode com quests, pré-requisitos e equipamento reais.

| Conteúdo atual do manifesto | Pontos mínimos R1/R3 | Primeiro nível sem bônus de Ato | Consequência |
|---|---:|---:|---|
| Corte Giratório, Tier 2 | 6/8 | 12/16 | Pode representar melee em L20 |
| Flecha Sangrante, Tier 3 após Perfuração | 13/15 | 26/30 | Não cabe em L20, mesmo com Ato 1 |
| Rajada Gélida, Tier 3 | 12/14 | 24/28 | Não cabe em L20, mesmo com Ato 1 |
| Capstone de combate, Tier 5 | pelo menos 27/29 | 54/58 | R1 em L50 só com Atos 1+2; R3, no mínimo, L54 com ambos |

Os capstones de Telisandra e Thoren dependem, respectivamente, de `survival.sinal_retirada`
e `crafting.marca_eficiencia`, hoje `NotYetExecutable`. `SkillPurchaseService` barra sua compra
mesmo com pontos suficientes. Nenhuma dessas contas prova dano, cura, TTK ou economia equilibrados.

## Decisões propostas para revisão

**P21-M1 — preservar a progressão atual.** Não injetar pontos extras na fixture e não reduzir o
gate Tier 5 para fazer a matriz passar. Isso preserva o custo de especialização e a utilidade das
recompensas dos Atos. Alterar a curva ou todos os thresholds exigiria rebalancear cinco árvores,
respec e save; é uma mudança maior que o problema da matriz.

**P21-M2 — separar fantasia inicial de capstone.** Em L20, comparar ações pré-capstone realmente
compráveis: melee pode manter Corte Giratório; ranged/magic precisam de ações de tier acessível,
com IDs escolhidos e conferidos na spec revisada. Tiro Carregado é o ranged ofensivo alcançável,
mas seu `DamagePerRank=0` também é forçado na execução da carga; R1/R3 podem ter resultado
idêntico. Isso é uma falha de progressão a medir, não um PASS de equilíbrio. Alihana pode somar
Longo Alcance, Nyx Presa Marcada; Senya ranged não tem eco elemental antes do capstone.
Prisão de Gelo aproxima o controle de Anya e Chama Breve a ofensiva de Senya, sem afirmar
que cura/eco do Tier 5 já existem. Os nomes Kanthor/Kaand/Alihana/Senya/Nyx/Anya
descrevem a intenção da build nesse estágio; **não** afirmam que a variante capstone foi ativada.
R1/R3 aqui se referem à ação comprada. A comparação deve preservar risco, recurso e equipamento
coerentes com o nível.

**P21-M3 — testar capstones em estágio legal próprio.** Em L50, completar os Atos 1+2 pela
progressão real antes de comprar capstone R1 e registrar essa condição no JSON/CSV. Incluir
capstone R3 em L54 ou posterior, somente depois de o runner provar a compra com orçamento,
pré-requisitos, rank da ação e slot/equipamento reais. Não rotular L50/R3 capstone como jogável.

**P21-M4 — manter suporte como critério bloqueado.** Telisandra/Thoren precisam de pré-requisitos
executáveis, cenários Farm/Town/Cave pertinentes e métricas próprias de sustain/renda; não forçar
uma ação ofensiva nesses dois perfis apenas para preencher as 36 linhas. A spec de fase 21 só
pode aceitar AC01 para elas após esse fechamento funcional.

**P21-M5 — não substituir inimigos indisponíveis por catálogo.** Elite L20 não está autorado na
banda 2; os Boss de bandas 2/4 do manifesto não são os Boss dos gates reais. Marcar linhas como
indisponíveis até conteúdo/wiring e cenário de encontro aprovados; não gerar um alvo artificial
para inflar cobertura. Isso protege a comparação de TTK de uma fixture sem encontro jogável.

## Escopo candidato para revisão da spec, plan e tasks

1. Revisar AC01/T02 para distinguir ações pré-capstone L20, capstone R1 L50 com dois Atos e
   capstone R3 L54+; deixar as nove identidades explícitas, inclusive suporte bloqueado.
2. Ajustar o plano do runner para aplicar progressão de quests, comprar pela API real, equipar
   item/slot e registrar orçamento e razões de indisponibilidade por linha.
3. Manter AC02/AC03/T04 como gate de telemetria e inimigos reais; exigir Common/Elite/Boss
   alcançáveis antes de comparar medianas. AC04/AC05 aguardam runs e renda canônica.
4. Reexecutar a matriz, os testes determinísticos e o PlayMode gráfico; só então considerar
   tuning numérico e desbloqueio da fase 22 de UI.

## Questões materiais ainda abertas

- Quais ações específicas de tier baixo representam cada arquétipo ranged/magic em L20 sem
  apagar sua identidade? Escolher por uso/contrajogo, não só por um ID que passa no gate.
- O conteúdo deve criar um Elite na banda 2 e bosses de gate nas bandas 2/4, ou a aceitação
  deve usar encontros reais de outros níveis? Isso muda o cenário, não só o teste.
- Quando os pré-requisitos de Telisandra/Thoren deixam de ser `NotYetExecutable`? Até lá,
  AC01 e o gate da UI permanecem abertos.
