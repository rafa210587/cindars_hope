# Cindar's Hope — Balance Curves Direction v1.1

> **Status:** documento canônico de curvas de progressão e balanceamento
> **Local:** `docs/design/gameplay/combat/BALANCE_CURVES_DIRECTION_v1.0.md`
> **Depende de:**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` (cap 100, escala por tipo)
> - `PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md` (fórmulas de atributos — VENCE em fórmula)
> - `PLAYER_SKILL_TREES_DIRECTION.md` (economia de 50 skill points — preservada)
> - `CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md` (fichas que consomem estas curvas)
> **Função:** fixar as curvas de XP, level cap, multiplicadores por tipo de criatura,
> alvos de TTK e as tabelas de validação que todo playtest deve conferir.
> **Não é spec implementável.**

---

## 0. Regra anti-duplicação

```text
Este documento define CURVAS e ALVOS. Não define fórmula de atributo (PLAYER_DERIVED vence),
não define custo de ação (COMBAT_CORE vence) e não define stats de criatura individual
(BESTIARY_CATALOG vence — já com os multiplicadores daqui aplicados).
```

---

# PARTE A — Progressão do jogador

## 1. Level cap 100, sem mexer nos skill points

A decisão humana pediu cap 100 — e a melhor proposta é a que NÃO muda nada na economia de
skills: o canon já dizia "1 ponto a cada 2 níveis, 50 pontos no endgame". Cinquenta pontos
a cada dois níveis É o nível 100. O cap alto vira régua natural da caverna: o nível do
jogador conversa com o nível do andar.

```text
Cap: 100.
SkillPoints: 1 a cada 2 níveis = 50 no cap (estrutura canônica intacta).
Bônus: +1 por ATO da main concluído (4) e +1 por marco de bestiário (a cada 10 entradas
"Estudadas", máx 5) → teto efetivo 59. Os tiers/rank caps das árvores continuam decidindo
onde esses pontos doem.
RÉGUA: player de nível N joga confortável na banda que contém o andar N da caverna.
```

## 2. Curva de XP

```text
XP para subir do nível N → N+1:   XPnext(N) = 60 × N^1.5   (arredondado a 10)

Marcos de sensação:
  lv 1→2: 60        lv 10: ~1.9k     lv 25: ~7.5k
  lv 50: ~21k       lv 75: ~39k      lv 99: ~59k
  Total acumulado 1→100 ≈ 2.4M.

Mix de fontes desejado (telemetria deve confirmar):
  kills ~55% · quests ~30% · descoberta ~15%
  (descoberta: andar novo +25×banda; entrada de bestiário Estudada +50; receita nova +20).
```

## 3. XP por kill (anti-farm embutido)

```text
XPganho = XPbase × clamp(1 + 0.08 × (lvlInimigo − lvlPlayer), 0.25, 2.0)
XPbase: ficha do bestiário (4 a 4500).
Inimigo 15+ níveis abaixo: trava no piso 0.25× e NÃO rola drop raro.
```

---

# PARTE B — Escala das criaturas

## 4. Multiplicadores por TIPO (decisão Q9.2 — nada de banda chapada)

A profundidade dá os números absolutos (fichas); o TIPO dá a forma da luta:

| Tipo/papel | HP | DMG | DEF | O que o jogador deve sentir |
|---|---|---|---|---|
| Swarm / Fodder | ×0.6 | ×0.8 | ×0.7 | morrem em 1-2 golpes certos; o perigo é o número |
| Chaser / Skirmisher | ×1.0 | ×1.0 | ×1.0 | a régua do baseline |
| Ranged / Caster | ×0.8 | ×1.2 | ×0.8 | doem de longe, derretem de perto |
| Tank / Guard / Construct | ×1.3 | ×0.9 | ×1.6 | paredes que exigem posture/counter |
| Elite | ×1.5 | ×1.25 | ×1.25 | mini-clímax de sala (+afixo) |
| Miniboss | ×4-6 | ×1.5 | ×1.5 | 2+ mecânicas, janela clara |
| Boss | ×10-14 | ×1.8 | ×1.8 | fases (modelo F05) |

*Os multiplicadores JÁ estão aplicados nas fichas do bestiário — esta tabela existe para
criar criaturas novas e para auditar desvios.*

## 5. Escala dentro da faixa de spawn

```text
HP: +12% por nível acima do mínimo da faixa.  DMG: +8% por nível.
DEF: fixa por criatura (mudar DEF muda a IDENTIDADE, não o desafio).
```

---

# PARTE C — Alvos de combate (TTK e dano recebido)

## 6. Time-to-kill alvo

Para um jogador NO NÍVEL da banda, com gear do tier da banda:

```text
Comum 3-6s · Elite 12-20s · Miniboss 45-90s · Boss 2-4min · Os Quatro do 101: 4-7min cada.
```

## 7. Dano recebido alvo

```text
Hit de comum: 4-8% do HP máximo do jogador.
Hit de elite: 10-15%.  Hit telegrafado de boss: 18-30% (o telegraph é o contrato).
Morte justa = 3+ erros seguidos, nunca 1 hit fora de execute explícito (Orc Warlord avisa).
```

## 8. Tabela de validação — dano esperado do jogador por banda (Q9.3)

| Banda | Lvl | Arma do tier | WeaponDmg | +Atributos≈ | Light hit≈ | DPS efetivo≈ | HP do comum (chaser) |
|---|---|---|---|---|---|---|---|
| 1-10 | 1-10 | Cobre/Ferro | 8-11 | +2-6 | 10-17 | 9-14 | 14-30 |
| 11-25 | 11-25 | Ferro/Aço | 11-12 | +6-14 | 17-26 | 14-22 | 24-70 |
| 26-40 | 26-40 | Aço (+Têmpera T1) | 12-14 | +14-24 | 26-38 | 22-32 | 52-110 |
| 41-55 | 41-55 | Aço refinado/Prata | 13-16 | +24-36 | 37-52 | 30-44 | 64-180 |
| 56-70 | 56-70 | Mithril | 15-18 | +36-50 | 51-68 | 42-58 | 76-260 |
| 71-85 | 71-85 | Liga bromeciana | 17-20 | +50-66 | 67-86 | 55-72 | 150-480 |
| 86-101 | 86-100 | Pedra Negra/Meteórica | 19-23 | +66-84 | 85-107 | 70-90 | 260-500 |

*(heavy ×1.45, charged ×1.65-1.90, crítico em janela ×1.5 — confere com os TTK do §6.
Esta tabela é o teste de mesa: se o playtest fugir dela, ajusta-se a criatura, não a curva.)*

---

# PARTE D — Recompensas de quest

## 9. Fórmula escalada (decisão Q6.2)

```text
QuestXP  = Base(tier) × (1 + 0.06 × QuestLevel)
QuestGold = idem, com a base de ouro do tier.

Bases:           XP   Ouro
  Daily (quadro)  20    50
  Side curta      50   100
  Side de cadeia 110   250
  Main (por quest)200  200
  Contrato cave   90   180
  Secreta        130     0  (paga em item raro — a caverna não usa moeda)

QuestLevel: daily = nível do player no aceite; side = nível de referência da cadeia;
main = 10/35/65/90 por ato. Main dá +1 skill point por ATO (no turn-in final).
```

---

# PARTE E — Pressão de recursos

## 10. A régua da run

```text
Run alvo: 6-10 andares por descida no nível adequado (a fadiga F16 e o bolso de consumíveis
são dimensionados para isso). Custos de stamina são FIXOS (canon); o que cresce é o pool
(CON/level) — o early sente cada dash, o late respira, e nunca vira irrelevante (regra-mãe).
Ouro/dia alvo no mid-game: 300-600 (farm) + 200-500 (caverna) — preços de gear/serviços
do catálogo de itens assumem esse fluxo.
```

---

# PARTE F — Decisões fechadas

```text
Cap 100 preservando a economia canônica de 50 skill points (+9 de bônus).
XPnext = 60×N^1.5; kills ~55% do XP; anti-farm por clamp 0.25-2.0.
Escala por TIPO de criatura, não por banda chapada (tabela §4 aplicada no bestiário).
TTK e dano recebido como ALVOS de telemetria — desvio ajusta criatura, não curva.
Recompensas de quest sempre escaladas por QuestLevel; main = +1 skill point/ato.
```

# PARTE G — Pendências

```text
Telemetria de Play Mode (Combat Core §53) para validar §6-§8 nas bandas 41+.
Curva de ouro fina (preços de tier alto vs fluxo/dia) após o catálogo entrar no jogo.
Decidir XP de festival/evento (proposto: como side curta do nível do player).
```
