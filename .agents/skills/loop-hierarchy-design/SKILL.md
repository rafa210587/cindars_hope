---
name: loop-hierarchy-design
description: Hierarquia dos três loops do jogo (Farm / Town / Cave) com perfis de risco, fluxo de recursos e classificação de features. Usar quando spec adiciona nova feature de gameplay, economy, progression ou quando a feature precisa de localização no loop correto.
---

# Skill: Hierarquia de Loops de Gameplay

O jogo tem três loops com risco e tempo crescentes. Uma feature mal localizada no loop errado cria dissonância (ex.: punição severa numa atividade de farm segura) ou torna o risco irrelevante (ex.: recovery fácil demais na cave). Antes de projetar uma feature, classifique-a no loop correto.

## Quando usar

- Spec adiciona nova feature de gameplay e não fica claro onde ela vive.
- Spec mexe em economy (preços, drops, recompensas) sem considerar de qual loop o recurso vem.
- Spec cria nova zona, evento, ou atividade recorrente.
- Spec desequilibra um loop (recompensa muito alta, penalidade muito baixa ou alta).
- Spec menciona "recurso raro", "grind", "recompensa", "risco", "loop de jogo".

## Os três loops

### Loop 1 — Farm (base segura, longo prazo)

| Aspecto | Característica |
|---|---|
| **Cena** | FarmScene |
| **Risco** | Baixo — sem combate, sem morte |
| **Horizonte** | Dias → semanas de jogo |
| **Atividades** | Plantar, regar, colher, criar animais, processar alimentos, crafting básico |
| **Produz** | Comida (stamina, fome), materiais básicos (madeira, fibra, couro), ouro via venda |
| **Consome** | Sementes, ferramentas, água, tempo de jogo, stamina do player |
| **Fail state** | Crop morre se não regada; animal produz menos sem alimentação |
| **Perfil de reward** | Previsível, steady income — baixa variância |

### Loop 2 — Town (hub social, médio prazo)

| Aspecto | Característica |
|---|---|
| **Cena** | TownScene |
| **Risco** | Baixo-médio — sem combate, mas recursos limitados (ouro, reputação) |
| **Horizonte** | Dias → semanas |
| **Atividades** | Compras, NPC services, quests, relacionamentos, crafting avançado, estalagem |
| **Produz** | Equipamento, upgrades, conhecimento (quests revelam mapas), relações NPC |
| **Consome** | Ouro, reputação, tempo de jogo |
| **Fail state** | Reputação negativa bloqueia serviços; ouro zero bloqueia upgrades |
| **Perfil de reward** | Progressivo via relacionamento — baixa variância, alto impacto longo prazo |

### Loop 3 — Cave (zona de risco, curto prazo)

| Aspecto | Característica |
|---|---|
| **Cena** | CaveScene |
| **Risco** | Alto — combate, morte, perda de inventário e XP |
| **Horizonte** | Horas de jogo (uma run) |
| **Atividades** | Combat, exploração, boss fights, loot, recovery de corpse |
| **Produz** | Materiais raros, itens únicos, ouro em volume, XP de combate |
| **Consome** | Comida (stamina/fome), equipamento (durabilidade), saúde |
| **Fail state** | Morte → corpse mechanic (inventário/ouro/XP perdidos, recuperáveis) |
| **Perfil de reward** | Alta variância — pode ser muito rico ou perder tudo |

## Fluxo de recursos entre loops

```
FARM ─────────────────────────────────────────► CAVE
  Comida (stamina, fome durante run)
  Itens de cura processados na farm

CAVE ─────────────────────────────────────────► TOWN / FARM
  Materiais raros (forja de armas, upgrades)
  Ouro (compras no town)
  Drops únicos (componentes de receitas avançadas)

TOWN ─────────────────────────────────────────► CAVE
  Equipamento reparado/upgradado
  Quests que direcionam para níveis específicos
  Serviços que melhoram performance na cave

FARM ◄────────────────────────────────────────► TOWN
  Crops vendidas → ouro
  Sementes compradas no town
  NPC recipes usam crops da farm
```

## Como classificar uma nova feature

```
A feature tem risco de vida (morte/KO)?
  → SIM: pertence à CAVE ou a um evento especial com perfil de cave
  → NÃO: continua

A feature é recorrente e previsível (todo dia)?
  → SIM: pertence à FARM
  → NÃO: continua

A feature envolve NPCs, serviços, ou compras?
  → SIM: pertence ao TOWN
  → NÃO: evento especial, festival, quest-specific
```

## Perguntas de design por loop

**Para features de Farm:**
- O recurso produzido alimenta a Cave (comida) ou o Town (venda)?
- O fail state (crop morta, animal doente) tem custo proporcional ao tempo investido?
- A mecânica é repetível indefinidamente sem se tornar grind puro?

**Para features de Town:**
- O serviço/item comprado habilita progressão em qual loop?
- O custo em ouro é calibrado para quantas runs de cave bem-sucedidas?
- O NPC relationship gate faz sentido como progressão de longo prazo?

**Para features de Cave:**
- O loot drop justifica o risco de morte (calibrado ao nível)?
- A feature funciona com o corpse mechanic (player pode perder itens da feature na morte)?
- A feature preserva o contrato de stable-run? (skill: cave-stable-run-guard)

## Anti-patterns

| Anti-pattern | Problema | Correção |
|---|---|---|
| Feature de farm com risco de morte | Quebra o perfil seguro do farm loop; frustra jogadores que usam farm para descomprimir | Mover para cave ou criar evento separado com opt-in |
| Recurso raro dropável só no farm | Desvincula progression da cave; cave perde relevância | Materiais raros vêm da cave; farm produz consumíveis |
| Ouro fácil no farm sem sink no town | Inflação; town perde relevância | Calibrar sink de ouro no town (serviços, upgrades, estalagem) |
| Recovery trivial na cave | Morte sem consequência; tensão desaparece | Manter custo de morte relevante (skill: fail-state-recovery-design) |
| Upgrade de cave comprado no farm | Curto-circuita o loop town; town perde função | Upgrades de cave ficam no town (blacksmith, forja) |

## Quando NÃO usar

- Feature de UI/menu pura sem loop de gameplay → não tem classificação de loop.
- Feature de sistema (save, validação, editor tool) → não pertence a nenhum loop.
- Feature de lore/cinematica sem recurso ou risco → neutro em relação a loops.

## Relacionados

- `(skill: economy-balance-tuning)` — calibrar preços e rewards dentro de cada loop
- `(skill: fail-state-recovery-design)` — como projetar a penalidade da cave
- `(skill: progression-curve-design)` — como unlock gating mapeia para os loops
- `(skill: cave-stable-run-guard)` — contratos que a cave deve preservar
- `(skill: player-needs-survival)` — recursos de farm (comida) consumidos na cave
