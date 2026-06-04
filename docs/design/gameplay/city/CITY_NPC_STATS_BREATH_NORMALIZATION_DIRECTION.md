# Cindar's Hope — City NPC Stats Breath Normalization Direction

> **Status:** direção canônica corretiva para normalização de stats de NPC  
> **Local:** `docs/design/gameplay/city/CITY_NPC_STATS_BREATH_NORMALIZATION_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> **Função:** resolver a inconsistência herdada em `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`, onde `Breath/Fôlego` ainda aparece como stat antigo de NPC.  
> **Não é spec implementável.** Specs futuras devem apenas aplicar esta normalização quando converter o roster em dados.

---

## 1. Correção canônica

`Breath/Fôlego` não existe como atributo, recurso, barra, custo, campo de save/load ou stat de NPC.

Esta decisão vale para:

```text
player;
companions;
pets;
NPCs;
inimigos;
HUD;
save/load;
ScriptableObjects futuros;
conversão futura de roster para dados.
```

`Breath` pode continuar apenas como nome narrativo de ataque de sopro de criatura, sem virar stat ou recurso.

---

## 2. Regra para ler o roster atual

Sempre que `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` mostrar uma linha neste formato antigo:

```text
HP X | MP Y | Stamina Z | Breath N
```

A leitura correta é:

```text
HP X | MP Y | Stamina Z
```

O valor de `Breath N` deve ser ignorado.

Não redistribuir esse valor para `HP`, `MP`, `Stamina`, Constituição, Destreza ou qualquer outro atributo sem uma decisão futura de balance.

---

## 3. Regra para specs futuras

Specs futuras que converterem NPCs em dados devem:

```text
não criar campo Breath;
não criar campo Fôlego;
não criar campo BR;
não criar barra de Breath/Fôlego;
não persistir Breath/Fôlego no save;
não expor Breath/Fôlego em HUD;
não usar Breath/Fôlego em balance;
manter somente HP, MP, Stamina e os atributos centrais.
```

Atributos válidos para NPCs:

```text
HP
MP
Stamina
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

---

## 4. Regra para futura limpeza do roster

Quando houver edição direta segura do arquivo grande `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`, aplicar a limpeza mecânica:

```text
1. Remover Breath/Fôlego da lista de stats.
2. Remover a linha da tabela que define Breath/Fôlego.
3. Remover `| Breath N` de todos os blocos de NPC.
4. Não alterar nomes, lore, romance, religião, quests ou números restantes.
5. Não rebalancear nenhum NPC nesta limpeza.
```

Esta limpeza é documental e não cria spec implementável.

---

## 5. Resultado esperado

Depois desta normalização, nenhuma spec futura deve interpretar `Breath/Fôlego` do roster antigo como fonte válida.

A fonte válida para atributos e recursos continua sendo:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```
