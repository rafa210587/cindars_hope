# FABLE — Refinamento v3: Skills, Itens, Companions e Boas Práticas (pré-execução)

> **Status:** AGUARDANDO RESPOSTAS DO DONO DO PROJETO
> **Origem:** 4 auditorias de design de 2026-06-12 (pedido direto do dono: "todas as skills
> estão bem definidas? todos os itens? como os companions lutam/equipam/recebem comandos?
> boas práticas de game dev que faltam?").
> **Como responder:** igual v1/v2 — por número; "default" aceita todos os ★ do bloco.
> **Efeito:** Bloco 1 trava F29/F33; Bloco 2 trava F32 (e a cadeia F06/F12/F22/F23/F26/
> F31/F48/F49/F50 que referencia IDs dela); Bloco 4 trava F14/F20/F56/F58 e o julgamento
> do checkpoint M1; Bloco 3 NÃO trava o lote (companions seguem wave futura — só destrava
> as specs 14_* quando chegar a hora).

---

## DIAGNÓSTICO RESUMIDO (das auditorias)

```text
SKILLS (70 nós canônicos): 19 DEFINIDAS (27%) · 33 PARCIAIS (47%) · 18 INDEFINIDAS (26%).
  18 efeitos sem CONSUMIDOR (skill existe, nenhum sistema declara aplicá-la);
  3 contradições canônicas (rank cap, pontos por ato, game_rule obsoleto).
ITENS (~118 + extras): ~70% com ID+BV+fonte. F32 NÃO É EXECUTÁVEL hoje: poções sem
  receita, itens mágicos da F31 fora do catálogo, gift_* fantasma, gear Mithril+ sem IDs,
  ~20 drops do bestiário órfãos, conflito Gold ×2.0 vs ×2.2.
COMPANIONS: bem definidos! COMPANIONS_DIRECTION.md canônica + código WAVE 05 (bond/jobs/
  save). Faltam: NPCs concretos, comandos básicos, stances, números de combate.
BOAS PRÁTICAS: projeto ACIMA da prática comum em economia/telemetria/save/governança.
  Lacunas na camada de APRESENTAÇÃO: pausa do relógio, feel pass, opções, escala de arte.
GAME_RULES OBSOLETOS (stop condition latente!): skill_tree_rules.md (cap 50, 5 slots,
  turnos) e inventory_equipment_rules.md (1 acessório, 20 slots) conflitam com as specs.
```

---

## BLOCO 1 — SKILLS (TRAVA F29 catálogo e F33; calibra F02/F27)

**1.1** Rank cap: estático ou dinâmico?
- A) Estático por nó (skill T1 capa em rank 2 para sempre; ranks 3-5 das T1 são removidos)
- B) Dinâmico por profundidade da árvore (cap de TODAS sobe conforme o tier desbloqueado:
  T1 aberto→cap 2 … T4/5→cap 5) ★ — única leitura que torna alcançáveis os ranks listados

**1.2** Custo por rank: A) 1 ponto por rank, inclusive capstones ★ B) custo crescente
(refaz o orçamento de 50 pontos)

**1.3** Pré-requisitos por nó além do tier gating? A) Não — tier é o único gate ★
B) Sim (gerar cadeias para os 70 nós)

**1.4** Dano base/cooldown final das ativas: onde mora o número?
- A) Adendo numérico único (tabela skill × rank → dano/cooldown/custo) anexado à
  SKILL_ACTION_MOVEMENT_TABLE ANTES da F29 — vira contrato dos executores ★
- B) Implementador escolhe dentro das faixas e playtest ajusta

**1.5** Lista fechada de skills dormentes (`NotYetExecutable`)?
- A) Pré-decidir na spec: executáveis = Corte Amplo, Golpe de Ruptura, Projétil Arcano,
  Flecha Perfurante, Toque Restaurador; dormentes = Marcador de Presa, Armadilha, Selo de
  Proteção, Disparo de Interrupção, Disparo Carregado, Descanso Curto ★
- B) Critério do implementador na Fase 4

**1.6** Consumidores das rotas Farm/Economy/Tool (18 efeitos órfãos):
- A) F29 publica hooks NOMEADOS (IGoldDropModifier, IHarvestYieldModifier,
  ICraftCostModifier...) e cada spec consumidora (F06/F17/F31/F48/F49/F55) ganha 1 linha
  de escopo "consome hook X"; até lá tooltip declara "efeito pendente" ★
- B) Adiar essas skills para as waves dos sistemas (quebra a contagem 70)

**1.7** Triggers de capstone — fechar números agora?
- A) Sim: sequência de MP = janela 6s; Kanthor cura 3% HP se HP<100% senão +10 Stamina;
  Telisandra = HP<25% OU Stamina<15% OU "Exausto" ★
- B) Capstones entram dormentes, tuning depois

**1.8** Atualizar `skill_tree_rules.md` ANTES da F29? A) Sim — reescrever do canon
(cap 100, 1pt/2 níveis, 4 slots, respec via Fonte) antes da execução, senão toda spec de
skill bate na stop condition de conflito ★ B) Junto da F29 no mesmo lote

**1.9** "+1 skill point por ATO de main quest" (decisão v1) × invariante "exatamente 50"
(F42): A) mantém +1/ato (teto vira ~55; rebalancear tiers) B) converter a recompensa de
ato em outra moeda (item/receita/stat) — preserva o invariante 50 ★

**1.10** Marcador de Presa menciona pet/companion (escopo proibido): A) reescrever para
"dano do jogador e aliados invocados, quando existirem" — companions herdam depois ★
B) manter e marcar como dormente

**1.11** Efeitos colaterais de respec: A) limpa active slots; itens já craftados via
unlock PERMANECEM usáveis; buffs de capstone cancelados ★ B) respec invalida também itens
de tiers não mais desbloqueados (punitivo)

## BLOCO 2 — ITENS (TRAVA F32 e a cadeia de dados inteira)

**2.1** Multiplicador BV da qualidade Gold: A) catálogo vence — 3 níveis, Silver ×1.5 /
Gold ×2.0; corrigir F32 (×2.2); tabela Q0-Q4 da economia fica para sistemas futuros ★
B) F32 vence (×2.2) e Q0-Q4 vira canônico

**2.2** Receitas de poções/óleos (alquimia da Ozzra): A) emendar o ITEM_CATALOG com
ingredientes canônicos usando drops/crops existentes, validados pela fórmula ×1.5-3.0 ★
B) poções/óleos só COMPRADOS no v1 (craft pós-v1)

**2.3** Itens mágicos da F31 (8 + trinkets + scroll_identify): A) emendar o catálogo com
seção própria (BV/raridade) e corrigir a citação "§19" ★ B) F31 como fonte canônica
desses itens (exceção documentada)

**2.4** Presentes gift_*: A) definir 4-6 itens gift no catálogo (gift_wildflowers 25g,
gift_carved_charm 60g...) vendidos por 1-2 NPCs ★ B) eliminar gift_* — presente = qualquer
item Giftable, +3 fixo

**2.5** Essências: A) 6 (catálogo + decisão v1) — corrigir o "8" da F32 ★ B) 8 (quais 2 novas?)

**2.6** Gear tier alto (Mithril/Bromeciana/Pedra Negra/Meteórica): A) emendar catálogo
com a lista nominal craftável (~14 itens, BV pela fórmula da economia) ★ B) modelo
"item base + MaterialTag" (menos itens, mais código; muda F49)

**2.7** Enums do código (WeaponType sem Hammer/Wand/Tool; ItemCategory sem Armor/Shield/
Accessory/Relic/Essence/AnimalProduct): A) adicionar valores explícitos save-safe via
emenda F32/F03 ★ B) cortar hammers/wands do v1 e mapear o resto em Misc/Magic

**2.8** ~20 drops órfãos do bestiário (trap parts, grimoire page, twin cores, gems...):
A) emendar catálogo §11 com todos (BV por banda, EN-only, resolver "stabilized blackstone"
× "pedra_negra_estabilizada") + uso declarado ★ B) emendar o bestiário para usar só a
lista atual (perde sabor)

**2.9** Ingredientes "água"/"carne"/"milk": A) água = recurso infinito de cozinha; criar
item_material_meat (drop Beast ~18g); milk = qualquer leite; berry = crystal_berry ★
B) água vira item; carne = grub_meat; cake exige cow_milk

**2.10** Roster de peixes para a F50: A) expandir para ~10 (1-2 por estação no açude +
2-3 de caverna por banda + 1 do Moonless Pool, BVs fixados) ★ B) manter 3 (tabelas variam
só quantidade/qualidade — empobrece a promessa sazonal)

**2.11** Durabilidade/upgrades: A) tabela derivada — DurabilityMax por classe×material
(bases 80 arma/150 armadura × modifiers) e custo de upgrade +N = (2N × material da banda)
+ (BV × 0.5N) ouro ★ B) autorar item a item durante F32/F49

**2.12** Atualizar `inventory_equipment_rules.md` ANTES da execução (4→7 slots de
equipamento com Ring/Amulet/Charm, nota do pouch +6)? A) Sim, via decision-rule-extraction ★
B) só no closeout da F23/F31 (risco de stop-and-report até lá)

## BLOCO 3 — COMPANIONS (NÃO trava o lote; destrava as specs 14_* futuras)

> Já é canônico (COMPANIONS_DIRECTION.md + código WAVE 05): companion = NPC da cidade com
> vínculo (não animal, não summon); 1 ativo na caverna; papéis Fighter/Guardian/Healer/
> Alchemist/Scout/Researcher/Musician; DPS 15-35% do jogador (35-50% especialista); downed
> SEM permadeath (Injured 1+ dia); equipment completo é futuro; não puxa packs novos;
> jobs de fazenda (9 tipos) JÁ implementados com bond/trust/save.

**3.1** Companions iniciais (reabre 8.1) — quantos/quais papéis?
- A) 3 NPCs: 1 Fighter/Guardian + 1 Healer/Alchemist + 1 Scout (o "companion Explorador"
  da quest da guilda como primeiro desbloqueio guiado) ★ — indicar os NOMES do roster
- B) 5 NPCs (+ 1 MusicianSupport + 1 Researcher)
- C) 1 piloto único (Explorador) para validar o sistema

**3.2** Comandos do jogador: A) só Seguir/Esperar B) Seguir/Esperar + Recuar + trocar
stance (4 comandos, 1 tecla: tap = seguir/esperar, hold = menu radial) ★ C) B + "atacar
alvo marcado" (exige UI de targeting)

**3.3** Stances de IA: A) nenhuma (brain fixo por papel) B) 3 — Defensivo (default) /
Agressivo / Passivo ★ C) 2 (Defensivo/Agressivo)

**3.4** Equipamento no sistema base: A) nada — escala só por vínculo/progressão ★
B) 1 slot de acessório/lembrança por quest pessoal em bond alto C) arma + acessório
(desaconselhado — viola a direction)

**3.5** Derrota do companion: A) Downed com janela de resgate → senão Retreat automático
→ Injured 1-2 dias ★ B) Retreat imediato a 0 HP C) permadeath opcional em modo difícil

**3.6** Quando o PLAYER é derrotado com companion ativo: A) companion escapa mas volta
Injured 1 dia (compartilha o custo da run) ★ B) sai ileso C) ileso mas perde bond

**3.7** "Ressurreição Dolorosa" da Fonte (conflito roster × direction): A) re-rotular como
recuperação acelerada de Incapacitated com custo progressivo de Água Viva (sem morte real) ★
B) companions PODEM morrer em eventos narrativos e a Fonte ressuscita C) cortar a função

**3.8** Fertilizante raro automático (executa o SIM da 5.5): A) toggle por job de Planter,
default OFF, consome do storage autorizado ★ B) sempre ativo quando designado C) só em
bond alto, sempre ativo

**3.9** Criaturas da caverna como companheiras: A) nunca B) não no base; 1 pet mágico
narrativo de late-game ligado à Fonte (caminho da PETS_DIRECTION, sem sistema de captura) ★
C) sistema de captura completo (desaconselhado)

**3.10** DPS alvo: A) 25% comum / 40% especialista, em CompanionBalanceProfileSO ★
B) piso (15/35%) — quase só utilidade C) teto (35/50%) — risco de trivializar early cave

**3.11** Progressão de bond: A) Bond 0-5; 1 perk passivo nos níveis 2/4; JobRank/CaveRank
separados (estrutura já existe no código) ★ B) 0-10 granular C) bond + nível de combate
separado (desaconselhado)

**3.12** Posição no roadmap: A) manter WAVE 14; este refinamento converte as specs 14_*
de "future mapped" para spec-ready ★ B) antecipar a fatia de caverna C) antecipar só a
fatia de fazenda (fertilizante 5.5 + jobs)

## BLOCO 4 — BOAS PRÁTICAS / APRESENTAÇÃO (TRAVA F14/F20/F56/F58 e o checkpoint M1)

**4.1** Relógio do dia × menus (BLOQUEIA a cadeia de UI): com painel/diálogo/loja aberto,
o tempo do dia... A) PAUSA em qualquer modal/painel/diálogo/loja (padrão Stardew;
GameTimeManager ganha gate único; resolve a duplicidade MenuManager × PauseMenuController) ★
B) continua correndo, exceto no pause explícito (Esc)

**4.2** Feel pass de combate: A) criar fable_71 "combat feel pass" — números de dano
flutuantes (a F43 já referencia números que NENHUMA spec cria!), hit-stop curto 40-60ms
em heavy/posture break, screen shake leve em boss/charged; só consome eventos existentes,
janela LIVRE como F58 ★ B) adiar para a fase de arte (M1-M3 julgados com combate "seco";
emendar F43)

**4.3** Escala de arte canônica (ADR antes de qualquer sprite final): pixels por tile =
A) 32 px/tile (padrão SDV-like; Huge 3×3 = 96px; legível em 1080p) ★ B) 16 px/tile
(retrô, mais barato, menos detalhe p/ 60 criaturas) C) 48 px/tile (custo ~2×)

**4.4** Aba Sistema — escopo de opções do v1 (emenda F56): A) volumes Master/SFX/Música
persistidos + toggle Fullscreen/Windowed + dropdown de resolução; nada mais ★ B) manter
"volume placeholder" só (contradiz a decisão 7.2 do windowed opcional)

**4.5** Gamepad (registro no input_map da F67): A) v1 = teclado/mouse ONLY; gamepad é
spec pós-v1 ★ B) incluir no lote (desaconselhado: ~138 KeyCode legados)

**4.6** Dificuldade: A) única no v1, tuning por telemetria F59; sem assist mode (registrar
decisão) ★ B) assist mode mínimo (dano recebido 0.5×/1×) na aba Sistema

**4.7** Localização (ADR antes da onda P4 de ~150 quests): A) PT-BR hardcoded aceito para
TODO o v1 (ADR assume o custo de retrofit) ★ B) a partir da P4, textos de quest/diálogo
via tabela id→string (paga ~10% agora, não reabre 150 quests depois)

**4.8** Save corrompido (emenda de 3 linhas na F56): A) load falhou → oferecer
automaticamente o backup rolling ("Save danificado — restaurar backup de <data>?") ★
B) só mensagem de erro

**4.9** Estados musicais (emenda F58, só contrato): A) F58 inclui MusicState
(Calmo/Combate/Boss/Festival) + eventos de transição com crossfade placeholder; faixas
reais na fase de áudio ★ B) F58 fica SFX-only (reabre AudioManager depois)

---

## O que acontece com as respostas

| Bloco | Destravará |
|---|---|
| 1 | F29/F33 executáveis sem ambiguidade; adendo numérico de skills; game_rule skill_tree reescrito; emendas F17/F31/F48/F49/F55 (hooks) |
| 2 | F32 executável (raiz da cadeia de dados); emendas ITEM_CATALOG (poções, mágicos, gifts, tier alto, drops órfãos, peixes); game_rule inventory reescrito |
| 3 | Specs 14_spec_companion_* viram spec-ready (implementação segue gated p/ wave futura) |
| 4 | Emendas F14/F43/F56/F58; possível fable_71 (feel pass); ADRs de arte/localização/gamepad/dificuldade via F67 |

*Gerado em 2026-06-12 a partir de 4 auditorias de design (skills, itens, companions, boas
práticas). Responder aqui ou em mensagem; registrarei em FABLE_DECISOES_RESPOSTAS_v3.*
