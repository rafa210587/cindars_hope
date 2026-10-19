# Cindar's Hope — Catálogo de Papéis de Companion (Bônus + Ações por Papel)

> **Status:** documento canônico de direção — catálogo de papéis de companion
> **Local:** `docs/design/gameplay/companions/COMPANION_ROLES_CATALOG_v1.0.md`
> **Origem:** decisão **3.1 (CUSTOM)** de `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`
> **Depende de (canônico):**
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md` (§9-11 papéis; §29-33 combate/balance; §42-44 abilities; §56-59 data assets)
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (3.1 catálogo; 3.2/3.3 stances/comandos/Suporter; 3.4 equipment base; 3.5/3.6/3.7 downed/revive/permadeath; 3.10 DPS; 3.11 bond)
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
> - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
> **Função:** para CADA papel de companion, definir explicitamente (a) bônus de combate, (b) bônus fora de combate, (c) conjunto de ações executáveis, (d) stance default sugerida e se expõe o modo **Suporter**.
> **Não é spec implementável.** As specs `14_spec_companion_*` convertem este catálogo em dados (`CompanionRoleProfileSO`), runtime (CompanionBrain/assist) e validações de Unity.

---

## 0. Escopo e regra anti-duplicação

Este documento **não** redefine:

```text
o sistema de companion (vive em COMPANIONS_DIRECTION.md);
fórmulas finais de dano/HP/MP/Stamina (PLAYER_DERIVED_ATTRIBUTES);
roster concreto de NPCs nem quais NPCs serão companions (CITY_NPC_ROSTER);
jobs de fazenda concretos (FARM_DESIGN_DIRECTION + COMPANIONS_DIRECTION §17-20);
loot tables (LOOT_CRAFTING_ECONOMY);
romance/casamento detalhado (SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION).
```

Este catálogo **define apenas**: o conjunto canônico de papéis, e por papel o BÔNUS (combate + fora de combate), o CONJUNTO DE AÇÕES, a STANCE default e a exposição do modo Suporter. Quando um valor numérico aparecer, ele é **direção de balance** (referência para `CompanionRoleProfileSO`/`CompanionBalanceProfileSO`), nunca fórmula final — a fórmula final continua em `PLAYER_DERIVED_ATTRIBUTES`.

Regra de número de companions iniciais (decisão 3.1):

```text
O número de companions iniciais NÃO é fixado aqui.
É decidido em spec posterior, depois deste catálogo, junto ao roster da cidade.
Piso anotado em 3.1: pelo menos o Explorador (Scout) como primeiro desbloqueio guiado.
```

---

## 1. Stances, comandos e modo Suporter (decisões 3.2 / 3.3)

Antes do catálogo por papel, fixam-se os controles universais que CADA papel referencia em sua linha "stance default" e "expõe Suporter".

### 1.1 Comandos (universais, todos os papéis)

```text
Ficar/Esperar   — companion fica na posição atual; não segue, não puxa pack.
Seguir          — companion acompanha o jogador respeitando FollowDistance/leash.
Atacar alvo marcado — companion foca o inimigo marcado pelo jogador (override temporário da stance).
```

### 1.2 Stances de combate (3 universais — decisão 3.3)

```text
Agressivo   — ataca qualquer inimigo num raio de até 12 tiles do jogador.
Defensivo   — DEFAULT — só ataca quem chega a <=4 tiles do jogador OU quem ataca o jogador.
Passivo     — não ataca ninguém por iniciativa própria (só obedece "Atacar alvo marcado").
```

### 1.3 Modo Suporter (modo de papel, não 4ª stance universal — decisão 3.3)

```text
Suporter NÃO é uma stance universal.
É um MODO exposto APENAS por papéis de suporte (cura/buff/cleanse/barreira).
Quando em Suporter, o papel prioriza cura/buff/cleanse sobre dano, dentro de MP/cooldown/limites.
Papéis sem capacidade de suporte NÃO expõem o modo Suporter.
```

Cada papel abaixo declara, no campo **Stance default** e **Expõe Suporter?**, como participa desses controles.

---

## 2. Visão geral dos papéis (índice canônico)

Consolidado de `COMPANIONS_DIRECTION` §9 (lista de roles), §10 (papéis na fazenda), §11 (papéis na caverna). Agrupados por eixo dominante; um companion pode ter `PrimaryRole` + `SecondaryRole` (ver §57 da direction).

| # | Papel (canônico) | Eixo dominante | Banda DPS (3.10) | Expõe Suporter? |
|---|---|---|---|---|
| 3.1 | **Fighter** | Combate ofensivo | comum 25% / especialista 40% | não |
| 3.2 | **Guardian** | Combate defensivo/tank | comum 25% (baixo) | não |
| 3.3 | **Healer** | Combate suporte (cura) | baixo (suporte) | **sim** |
| 3.4 | **Alchemist** | Combate suporte (status/buff) + fazenda | baixo (suporte) | **sim** |
| 3.5 | **Scout** (Explorador) | Exploração/combate leve | comum 25% | não |
| 3.6 | **Researcher** | Conhecimento/lore | baixo (suporte) | não |
| 3.7 | **MusicianSupport** | Combate suporte (buff/moral) | baixo (suporte) | **sim** |
| 3.8 | **FarmWorker** (Plantador/Colhedor) | Fazenda | n/a (fazenda) | não |
| 3.9 | **Forager** | Fazenda/exploração | baixo (não-combate) | não |
| 3.10 | **Miner** (Minerador) | Fazenda/caverna | baixo (não-combate) | não |
| 3.11 | **AnimalCaretaker** (Tratador) | Fazenda | n/a (fazenda) | não |
| 3.12 | **Crafter / Builder** (Artesão/Construtor) | Fazenda/processamento | n/a (fazenda) | não |
| 3.13 | **Merchant** | Econômico/social | n/a | não |

> Bandas de DPS são **canônicas da decisão 3.10**: comum **25%**, especialista **40%** do DPS esperado do jogador no mesmo estágio (`CompanionBalanceProfileSO.DpsContributionTargets`). Papéis de suporte/fazenda ficam **abaixo** da banda comum por definição.

---

# PARTE A — Papéis de combate

## 3.1 Fighter

```text
Eixo: combate ofensivo. PrimaryRole de combate típico.
Stance default: Defensivo. Expõe Suporter? Não.
DPS alvo (3.10): comum 25%; quando especialista ofensivo, 40% com fragilidade/cooldown/risco.
```

**Bônus de combate**

```text
contribuição de DPS dentro da banda 25%/40% (CompanionBalanceProfileSO.DpsContributionTargets);
pressão sobre o alvo (ajuda a fechar janelas de vulnerabilidade do inimigo);
aplica o hook do Marcador de Presa como "aliado invocado" (decisão 1.10), sem dano duplicado;
pequena passiva de combate por bond (perks nos níveis 2/4 — decisão 3.11).
```

**Bônus fora de combate**

```text
nenhum bônus de produção; presença defensiva em escolta/quest;
pode acelerar limpeza de inimigos em jobs de fazenda perigosos (se a spec permitir job de defesa).
```

**Conjunto de ações**

```text
Ataque básico corpo a corpo / à distância (conforme arma equipada — equip base, decisão 3.4);
Atacar alvo marcado;
Recuar quando HP < RetreatThreshold (downed/retreat — decisão 3.5);
Tentar reviver o player derrotado (30% — decisão 3.6) se ativo no momento da derrota.
```

---

## 3.2 Guardian

```text
Eixo: combate defensivo/tank limitado.
Stance default: Defensivo. Expõe Suporter? Não.
DPS alvo (3.10): comum 25% (baixo — prioriza proteção, não dano).
```

**Bônus de combate**

```text
intercepta hit ocasional direcionado ao jogador (COMPANIONS_DIRECTION §31);
taunt curto APENAS se o sistema de threat permitir (gera threat controlada — §34);
redução de dano em janela curta (anti-stagger leve);
bloqueio momentâneo de passagem.
```

Limites (§31, não-negociáveis):

```text
não mantém aggro permanente de boss;
não trava inimigo sem custo;
não segura pack inteiro sozinho;
não ignora guard break / boss mechanics.
```

**Bônus fora de combate**

```text
escolta segura em quest/expedição (reduz risco de o player ser flanqueado);
nenhuma produção de fazenda.
```

**Conjunto de ações**

```text
Interceptar (corpo entre inimigo e jogador, janela curta);
Taunt curto (se threat habilitado);
Reduzir dano recebido pelo jogador em janela (DefensiveAssist);
Atacar alvo marcado (dano baixo);
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

## 3.3 Healer

```text
Eixo: combate suporte (cura).
Stance default: Defensivo. Expõe Suporter? SIM.
DPS alvo (3.10): baixo (suporte) — fica abaixo da banda comum.
```

**Bônus de combate** (em modo Suporter, COMPANIONS_DIRECTION §30)

```text
cura pouca/moderada o jogador (e companion, se sistema permitir);
remove status leve (StatusCleanse);
aplica barreira curta (BuffShort);
reduz Fear/ConfusionLite leve.
```

Limites obrigatórios (§30 — "não pode criar imortalidade"):

```text
custo de MP por cura;
cooldown alto;
range curto;
cast time + interrupção possível;
limite de usos por combate/run (CompanionBalanceProfileSO.HealingCooldownRules).
```

**Bônus fora de combate**

```text
acelera recuperação de Injured de outro companion (descanso/cura — §38), sem virar reset grátis;
não substitui o curandeiro da cidade nem a Fonte de Anya.
```

**Conjunto de ações**

```text
Curar jogador (Suporter, gated por MP/cooldown/cast);
Limpar status leve (StatusCleanse);
Barreira curta (BuffShort);
Atacar alvo marcado (dano baixo, só fora do Suporter);
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

## 3.4 Alchemist

```text
Eixo: combate suporte (status/poções/buff curto) + processamento de fazenda.
Stance default: Defensivo. Expõe Suporter? SIM.
DPS alvo (3.10): baixo (suporte).
```

**Bônus de combate** (COMPANIONS_DIRECTION §11)

```text
aplica status/buffs curtos ao jogador ou debuff curto ao inimigo;
poções/efeitos breves dentro de cooldown;
não controla permanentemente (regra de status: sem controle permanente).
```

**Bônus fora de combate** (papel também de fazenda — §10)

```text
processa poções/fertilizantes autorizados na estação de alquimia (job de fazenda);
1 poção/consumível gratuito por dia se o vínculo permitir (BondGainRules / FarmJobBonus), respeitando limites de economia;
não cria item do nada — exige input e estação real (§19).
```

**Conjunto de ações**

```text
Aplicar buff curto / status (Suporter);
Arremessar poção de efeito breve;
Job de fazenda: processar poção/fertilizante (estação + input + tempo + stamina);
Atacar alvo marcado (dano baixo);
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

## 3.5 Scout (Explorador)

```text
Eixo: exploração + combate leve. Piso de 3.1: primeiro desbloqueio guiado.
Stance default: Defensivo. Expõe Suporter? Não.
DPS alvo (3.10): comum 25%.
```

**Bônus de combate** (COMPANIONS_DIRECTION §11)

```text
alerta de perigo (telegrafa ataque/emboscada de inimigo ao jogador);
melhor posicionamento (mantém distância por papel, ajuda a não puxar pack);
dano leve à distância.
```

**Bônus fora de combate**

```text
treasure hint limitado (ExplorationHint / LootHint — §44, §47): aponta baú/recurso próximo, sem abrir sozinho;
trap hint: sinaliza armadilha (não aciona trap voluntariamente — §24);
revela um pouco de fog/mapa local se o sistema de reveal existir.
```

**Conjunto de ações**

```text
Marcar perigo/armadilha (ExplorationHint);
Apontar tesouro/recurso próximo (LootHint, sem abrir);
Ataque leve à distância;
Atacar alvo marcado;
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

## 3.6 Researcher

```text
Eixo: conhecimento / lore / bestiário.
Stance default: Passivo (foco em conhecimento, evita combate). Expõe Suporter? Não.
DPS alvo (3.10): baixo (suporte).
```

**Bônus de combate**

```text
bestiary hint: revela vulnerabilidade/fraqueza do inimigo presente (acelera leitura de janela de TTK);
nenhum dano relevante esperado.
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §11)

```text
+XP/progresso de bestiário ao identificar criaturas (BESTIARY_KNOWLEDGE_DISCOVERY);
leitura de ruínas / lore hints (ExplorationHint de conteúdo);
ajuda a identificar drops (§47), sem dobrar loot.
```

**Conjunto de ações**

```text
Identificar criatura (revela vulnerabilidade + ganha conhecimento de bestiário);
Ler ruína / lore hint;
Identificar drop;
Atacar alvo marcado (dano baixo);
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

## 3.7 MusicianSupport

```text
Eixo: combate suporte (buff/moral).
Stance default: Defensivo. Expõe Suporter? SIM.
DPS alvo (3.10): baixo (suporte).
```

**Bônus de combate** (COMPANIONS_DIRECTION §11)

```text
buff curto de grupo (jogador + companion/pet quando existirem);
moral: redução de Fear / ConfusionLite leve;
buffs curtos com cooldown, sem stack permanente.
```

**Bônus fora de combate**

```text
buff social/moral leve em eventos de fazenda/cidade (sabor; sem poder mecânico obrigatório);
pode melhorar humor de jobs (pequeno ganho de bond/relação em atividades de grupo).
```

**Conjunto de ações**

```text
Tocar buff curto (BuffShort, Suporter);
Reduzir Fear/ConfusionLite (StatusCleanse leve);
Atacar alvo marcado (dano baixo, fora do Suporter);
Recuar / downed / tentativa de revive (3.5/3.6).
```

---

# PARTE B — Papéis de fazenda e econômicos

> Papéis abaixo são primariamente de fazenda/economia. Em caverna, comportam-se como combate leve/Defensivo, mas seu valor está fora de combate. Jobs exigem **área marcada + ferramenta/estação + tempo + stamina + vínculo** (COMPANIONS_DIRECTION §17-20) e respeitam os limites de automação (§19).

## 3.8 FarmWorker (Plantador / Colhedor)

```text
Eixo: fazenda. Stance default em caverna: Defensivo. Expõe Suporter? Não.
DPS alvo: n/a (papel de produção).
```

**Bônus de combate**

```text
mínimo; defesa básica se acompanhar em área de risco.
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §10, §20)

```text
Plantador: reduz chance de erro de plantio; consome menos tempo na área marcada;
Colhedor: coleta crops maduras na área permitida dentro do StaminaBudget;
qualidade do job melhora com vínculo/ferramenta/estação.
```

**Conjunto de ações**

```text
Plantar na área marcada (FarmJobBonus);
Colher crops maduras na área marcada;
Respeitar horário/stamina/storage autorizado;
(Em caverna) seguir e defender-se passivamente.
```

---

## 3.9 Forager

```text
Eixo: coleta/forrageio (fazenda + exploração). Stance default em caverna: Defensivo. Expõe Suporter? Não.
DPS alvo: baixo (não-combate).
```

**Bônus de combate**

```text
mínimo; pode coletar recurso seguro durante a exploração sem puxar pack.
```

**Bônus fora de combate**

```text
coleta recursos de superfície/forrageáveis na área permitida;
melhor chance de componente comum específico (sem invalidar LootTableSO — §47);
não dobra drop nem gera recurso fora do estado real do mundo (§19, §47).
```

**Conjunto de ações**

```text
Forragear recurso marcado;
Indicar recurso forrageável próximo (LootHint leve);
(Em caverna) seguir e defender-se passivamente.
```

---

## 3.10 Miner (Minerador)

```text
Eixo: fazenda (pedreira) + caverna (mineração). Stance default em caverna: Defensivo. Expõe Suporter? Não.
DPS alvo: baixo (não-combate).
```

**Bônus de combate**

```text
mínimo; presença útil em caverna por minerar nós seguros.
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §10)

```text
coleta pedras/minérios na área permitida (especialmente pedreira late game);
melhora throughput de mineração com ferramenta/vínculo;
respeita o estado real dos nós de recurso (não minera nó depletado/fora de área).
```

**Conjunto de ações**

```text
Minerar nó marcado na área permitida;
(Em caverna) minerar nós seguros sem puxar pack;
Respeitar ferramenta/stamina/horário/storage.
```

---

## 3.11 AnimalCaretaker (Tratador)

```text
Eixo: fazenda (animais/pets). Stance default em caverna: Defensivo. Expõe Suporter? Não.
DPS alvo: n/a (produção).
```

**Bônus de combate**

```text
mínimo.
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §10, §20)

```text
cuida de animais/pets: alimentação e coleta de produtos;
alto vínculo melhora chance de produto animal de qualidade;
interage com o sistema de pets quando existir (não substitui o pet).
```

**Conjunto de ações**

```text
Alimentar animais na área/estação permitida;
Coletar produtos animais;
Respeitar horário/stamina/vínculo.
```

---

## 3.12 Crafter / Builder (Artesão / Construtor)

```text
Eixo: fazenda (processamento + construção). Stance default em caverna: Defensivo. Expõe Suporter? Não.
DPS alvo: n/a (produção).
```

**Bônus de combate**

```text
mínimo.
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §10, §20)

```text
Artesão (Crafter): opera workshop/processadores simples; acelera processamento (exige estação + input);
Construtor (Builder): ajuda em construção/movimento de estruturas, se permitido;
qualidade/velocidade melhoram com vínculo e estação.
```

**Conjunto de ações**

```text
Operar estação de processamento (input -> output, com tempo/stamina);
Auxiliar em construção/movimento de estrutura autorizada;
Respeitar estação construída / storage / horário.
```

---

## 3.13 Merchant

```text
Eixo: econômico/social. Stance default em caverna: Passivo. Expõe Suporter? Não.
DPS alvo: n/a.
```

**Bônus de combate**

```text
nenhum esperado; companion frágil em caverna (evita combate, Passivo).
```

**Bônus fora de combate** (COMPANIONS_DIRECTION §41, §48)

```text
melhores serviços/descontos se o NPC for comerciante e o vínculo permitir;
pode identificar valor de drop / facilitar venda;
benefício de vínculo não vira pay-to-win social obrigatório (§41).
```

**Conjunto de ações**

```text
Oferecer desconto/serviço aprimorado por vínculo;
Avaliar/identificar valor de item;
(Em caverna) manter-se em Passivo, focar em segurança.
```

---

# PARTE C — Mapeamento para dados e balance

## 4. Para CompanionRoleProfileSO (asset esperado, §56 da direction)

Cada papel deste catálogo deve gerar 1 `CompanionRoleProfileSO` com, no mínimo:

```text
RoleId                  (Fighter, Guardian, Healer, Alchemist, Scout, Researcher,
                         MusicianSupport, FarmWorker, Forager, Miner, AnimalCaretaker,
                         Crafter, Builder, Merchant)
DefaultStance           (Aggressive | Defensive | Passive) — ver coluna "Stance default"
ExposesSupporterMode    (bool) — true só para Healer, Alchemist, MusicianSupport
CombatBonusTags         (DPS band, intercept, taunt, heal, cleanse, barrier, buff, hint...)
OutOfCombatBonusTags    (farm job bonus, forage, mine, caretake, craft, build,
                         lore/bestiary xp, loot hint, merchant discount...)
AllowedActions          (lista de ações desta página)
ForbiddenActions        (de COMPANIONS_DIRECTION §24/§29-33: não puxar pack, não abrir
                         baú sozinho, não acionar trap, não segurar boss, sem imortalidade)
DpsBandRef              -> CompanionBalanceProfileSO.DpsContributionTargets (25% / 40% / suporte)
```

## 5. Bandas de DPS canônicas (decisão 3.10)

```text
Comum:        25% do DPS esperado do jogador no mesmo estágio.
Especialista: 40% (com fragilidade/cooldown/risco).
Suporte/Fazenda: abaixo da banda comum (DPS baixo por definição).
Vivem em CompanionBalanceProfileSO.DpsContributionTargets.
```

Estes valores **substituem/precisam** a faixa 15%-35% / 35%-50% do `COMPANIONS_DIRECTION` §29 para fins de balance ativo: a **decisão 3.10 é a banda canônica** (25% comum / 40% especialista). Ver nota de emenda na própria direction (EMENDA 2026-06-13-V3 prevista pela decisão 3.4/3.10).

## 6. Stances/Suporter por papel (resumo para runtime)

| Papel | Stance default | Expõe Suporter? |
|---|---|---|
| Fighter | Defensivo | não |
| Guardian | Defensivo | não |
| Healer | Defensivo | **sim** |
| Alchemist | Defensivo | **sim** |
| Scout | Defensivo | não |
| Researcher | Passivo | não |
| MusicianSupport | Defensivo | **sim** |
| FarmWorker | Defensivo | não |
| Forager | Defensivo | não |
| Miner | Defensivo | não |
| AnimalCaretaker | Defensivo | não |
| Crafter / Builder | Defensivo | não |
| Merchant | Passivo | não |

> Lembrete (3.2): Agressivo = ataca tudo em <=12 tiles do jogador; Defensivo (default) = só quem chega a <=4 tiles ou ataca o jogador; Passivo = não ataca por iniciativa.

---

# PARTE D — Nota de reconciliação do enum (decisão 3.1 + achado de re-auditoria 143)

> **Achado confirmado pela re-auditoria:** o enum de runtime e os flags estão desalinhados com o vocabulário de papéis deste catálogo. Esta seção registra a direção de reconciliação, sem alterar código (tarefa de docs).

## 7. Estado atual no código

```text
Assets/_Game/Scripts/Companions/CompanionRole.cs  (enum [Flags] CompanionRole):
  None, FarmCompanion, CaveCompanion, QuestCompanion, SocialCompanion
  -> são CONTEXTOS de elegibilidade ("onde o companion pode atuar"), NÃO os papéis
     funcionais (Fighter/Healer/Scout...) deste catálogo.

Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs:
  CanBeFarmCompanion, CanBeCaveCompanion, CanBeQuestCompanion, CanBeSocialCompanion,
  CanBeRomanceCompanion, CanBeSpouseCompanion  (+ locks)
  -> Romance e Spouse existem como bools de elegibilidade, mas NÃO há entrada
     Romance/Spouse no enum CompanionRole.
```

## 8. Direção de reconciliação (para a spec de companion data contract — WAVE 14)

```text
1. SEPARAR dois conceitos distintos (não confundir contexto com papel funcional):
   a) CONTEXTO de elegibilidade  = "onde/como o companion pode ser usado"
      (Farm / Cave / Quest / Social [+ Romance / Spouse]).
   b) PAPEL FUNCIONAL            = "o que ele faz" — os 14 papéis deste catálogo
      (Fighter, Guardian, Healer, Alchemist, Scout, Researcher, MusicianSupport,
       FarmWorker, Forager, Miner, AnimalCaretaker, Crafter, Builder, Merchant).

2. O enum [Flags] CompanionRole atual descreve CONTEXTO, não papel funcional.
   Recomendação: renomear conceitualmente para CompanionContext (ou manter o nome
   e documentar que representa contexto), e criar um enum/identificador SEPARADO
   para o PAPEL FUNCIONAL, alimentado por CompanionRoleProfileSO (§4 deste doc).

3. Romance/Spouse: NÃO virar valor do enum de papel funcional. São elegibilidade
   social (já em CompanionEligibilityFlags) e/ou contexto. Decisão 3.1 fecha:
   "Romance/Spouse permanecem como flags de elegibilidade; o papel de combate/fazenda
   NÃO depende de romance" (consistente com COMPANIONS_DIRECTION §49 — companion de
   combate não é restrito a romance). Para alinhar o enum [Flags] de contexto com os
   flags, considerar adicionar RomanceCompanion/SpouseCompanion como contextos
   (espelhando CanBeRomance/SpouseCompanion), mantendo papel funcional fora dele.

4. CompanionManagerSaveData (achado 142 da re-auditoria) é só DTO, sem
   SaveSectionProvider. A spec de companion deve incluir o provider para round-trip
   do estado de papel/stance/bond. (Fora do escopo deste catálogo de design.)
```

Regra de ouro da reconciliação:

```text
Contexto (Farm/Cave/Quest/Social/Romance/Spouse) responde "ONDE pode atuar".
Papel funcional (Fighter/Healer/...) responde "O QUE faz".
Os dois eixos são ortogonais e NÃO devem ser fundidos no mesmo enum.
```

---

# PARTE E — Decisões fechadas neste catálogo

```text
14 papéis canônicos definidos com bônus (combate + fora) e conjunto de ações.
3 stances universais: Agressivo (<=12 tiles) / Defensivo default (<=4 tiles ou quem ataca) / Passivo.
Suporter é MODO de papel, exposto só por Healer, Alchemist e MusicianSupport.
Comandos universais: Ficar/Esperar, Seguir, Atacar alvo marcado.
DPS canônico (3.10): 25% comum / 40% especialista / suporte abaixo — em CompanionBalanceProfileSO.
Stance default de exploração/quest: Defensivo (Passivo para Researcher e Merchant).
Número de companions iniciais NÃO é fixado aqui; piso = Scout como primeiro desbloqueio.
Reconciliação do enum: contexto (Farm/Cave/Quest/Social/Romance/Spouse) e papel funcional são eixos separados; Romance/Spouse continuam como elegibilidade, não papel funcional.
Limites de COMPANIONS_DIRECTION §24/§29-33 valem para todos os papéis (não puxar pack, sem imortalidade, sem segurar boss, sem abrir baú/trap sozinho).
```

---

# PARTE F — Pendências abertas (para specs WAVE 14)

```text
Fixar quais NPCs do roster (CITY_NPC_ROSTER) assumem cada papel e qual é PrimaryRole/SecondaryRole.
Fixar o conjunto inicial de 3-5 companions (piso: Scout) — spec posterior.
Gerar 1 CompanionRoleProfileSO por papel com DefaultStance, ExposesSupporterMode, AllowedActions, ForbiddenActions, DpsBandRef.
Implementar a separação contexto vs. papel funcional no enum (CompanionRole atual = contexto).
Validar bandas de DPS (25%/40%/suporte) contra CAVE_COMBAT_BALANCE.
Validar bônus de job de fazenda (Plantador/Colhedor/Minerador/Tratador/Artesão/Construtor/Alquimista) contra FARM_DESIGN_DIRECTION e a economia.
Incluir CompanionManagerSaveData no SaveSectionProvider (achado 142).
Confirmar com o humano se "Suporter" deveria ser 4ª stance universal (interpretação atual: modo de papel — ver 3.3).
```

---

*Criado: 2026-06-13 — implementa a decisão 3.1 (CUSTOM) de FABLE_DECISOES_RESPOSTAS_v3.0. Catálogo de papéis; não é spec implementável. Specs 14_spec_companion_* convertem em dados/runtime/validações.*
