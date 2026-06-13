# Cindar's Hope — Seasons, Calendar, Weather & Lunar Direction

> **Status:** direction canônico de tempo, calendário, estações, clima e luas  
> **Local:** `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> - `docs/design/gameplay/pets/PETS_DIRECTION.md`  
> - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> **Função:** centralizar regras de passagem de tempo, calendário, estações, clima, eventos lunares e impactos sistêmicos globais.  
> **Não é spec implementável.** Specs futuras devem quebrar esta direção em arquivos menores em `.specs/a_implementar/`.

---

## 0. Regra anti-duplicação

Este documento define o sistema global de:

```text
tempo in-game;
relógio;
dia/noite;
sono;
passagem de dias;
calendário;
estações;
clima;
chuva;
neve;
tempestade;
névoa;
eventos lunares;
ciclos de Alihana, Senya e Nyx;
festivais;
impactos globais em fazenda, cidade, caverna, economia, NPCs, pets, companions, quests, Fonte e Mana.
```

Este documento não redefine:

```text
rotina individual de cada NPC;
residências;
camas;
waypoints;
interiores;
horários específicos de cada loja;
layout da cidade;
layout da fazenda;
eventos específicos de cada festival;
questlines completas;
balance final de crops;
balance final de economia;
UI final de calendário/clima/lua.
```

Fontes específicas continuam vencendo:

```text
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
  vence para rotina individual de NPC, camas, waypoints, prédios, interiores, portas, períodos do dia e schedules concretos.

CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
  vence para NPCs, serviços, romance eligibility, papéis funcionais e serviços concretos.

FARM_DESIGN_DIRECTION_v1.3.md
  vence para sistemas agrícolas, crops, irrigação, animais, Fonte na fazenda e expansão.

QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
  vence para uso narrativo dos fragmentos de Anya, Fonte, Arco da Memória e nível 100/101.

UI_UX_FULL_GAMEPLAY_DIRECTION.md
  vence para apresentação de relógio, calendário, clima, lua, notificações e feedback visual.

ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  vence para preço, BaseValue, restock, SellPoint, stock state e anti-arbitragem.
```

Regra de separação:

```text
Este documento define quando e por que o mundo muda.
Documentos de NPC/cidade/fazenda definem quem, onde e como cada entidade reage.
UI_UX define como isso aparece para o jogador.
Specs implementáveis conectam as três camadas.
```

---

## 1. Objetivo do sistema

O sistema de tempo deve sustentar a fantasia rural e a progressão profunda do jogo.

Ele precisa conectar:

```text
rotina de fazenda;
sono;
cansaço;
fome;
crops;
chuva;
irrigação;
animais;
pets;
NPC schedules;
lojas;
festivais;
pedidos/encomendas;
caverna;
luas;
Fonte de Anya;
Água Viva;
Mana;
main quest;
eventos raros;
economia;
restock;
save/load.
```

Regra de design:

```text
Tempo cria decisão.
Tempo não deve impedir o jogador de jogar.
```

Consequências práticas:

```text
Jogador deve sentir que cada dia importa.
Jogador não deve ser punido por explorar, conversar, plantar ou pescar em ritmo próprio.
Main quest pode usar calendário/lua/clima, mas deve dar pista e controle razoável.
Eventos raros devem enriquecer o mundo, não bloquear progresso essencial sem aviso.
```

---

## 2. Unidade base de tempo

## 2.1 Relógio in-game

Dia padrão:

```text
06:00 -> 02:00 = período jogável principal.
02:00 -> 06:00 = transição forçada, sono, colapso ou novo dia, salvo exceções de quest.
```

A cidade já usa blocos de agenda conceituais, que este documento adota como camada global:

```text
06:00-09:00 Morning
09:00-12:00 WorkStart
12:00-14:00 Midday
14:00-18:00 WorkAfternoon
18:00-21:00 Evening
21:00-00:00 Night
00:00-06:00 Sleep/LateNight
```

## 2.2 Escala inicial recomendada

Baseline de design:

```text
1 hora in-game = 60 segundos reais.
1 dia jogável de 06:00 a 02:00 = 20 minutos reais.
```

Motivo:

```text
permite rotina de fazenda sem pressa extrema;
mantém caverna com risco logístico;
permite ir à cidade, vender, socializar e voltar;
evita dias longos demais;
facilita balance inicial.
```

Valores finais podem mudar em playtest.

## 2.3 Pausa e tempo rodando

Tempo deve pausar em UI modal:

```text
pause/system menu;
inventário modal;
equipment modal;
skill tree;
crafting modal;
shop;
diálogo;
quest log;
social log;
calendar;
Fonte menu;
cutscene;
modal de confirmação;
decisão final.
```

Tempo pode continuar em gameplay ativo:

```text
gameplay normal;
farm work;
cidade andando;
caverna;
combate;
pesca;
mineração;
foraging;
interação curta sem modal.
```

Regra:

```text
Se a UI bloqueia input de gameplay, o tempo normalmente pausa.
Exceções devem ser explícitas por spec.
```

## 2.4 Transição de dia

O dia avança quando:

```text
jogador dorme em cama válida;
jogador colapsa por horário/cansaço;
evento narrativo força descanso;
transição especial de quest.
```

Processos de virada do dia:

```text
salvar jogo se regra permitir;
calcular crescimento de crops;
calcular água/irrigação;
aplicar morte de planta se sem água por regra;
processar shipping/SellPoint;
processar pagamentos pendentes;
processar restock diário/semanal;
atualizar pedidos/encomendas;
atualizar NPC schedules;
atualizar pets/companions;
atualizar clima do dia seguinte;
atualizar ciclo lunar;
atualizar eventos de calendário;
aplicar recuperação de HP/MP/Stamina;
aplicar recuperação ou penalidade de Cansaço;
recarregar Água Viva se condição permitir;
atualizar flags leves de mundo.
```

## 2.5 Tempo em interiores

Regra:

```text
Entrar em interior não pausa o tempo por padrão.
Abrir UI modal dentro do interior pausa.
Cutscene/diálogo modal pausa.
```

Motivo:

```text
Cidade e lojas precisam respeitar horário.
NPCs precisam mudar de estado por tempo.
Jogador deve planejar deslocamentos.
```

---

## 3. Sono, cansaço e horário limite

## 3.1 Sono voluntário

O jogador pode dormir em cama válida.

Baseline:

```text
Dormir antes de 21:00:
  permitido, mas o jogo deve confirmar.

Dormir entre 21:00 e 00:00:
  normal.

Dormir entre 00:00 e 02:00:
  permitido, mas com recuperação menor ou cansaço residual, conforme balance futuro.

Após 02:00:
  colapso/retorno forçado para cama ou Fonte, conforme contexto.
```

## 3.2 Colapso por horário

Regra inicial:

```text
02:00 é o limite padrão do dia jogável.
```

Se o jogador ainda estiver ativo:

```text
fora da caverna:
  colapsa e acorda em casa/cama, com penalidade leve.

na caverna:
  colapsa e retorna por regra de caverna/Fonte/checkpoint, com penalidade maior.

em evento de quest:
  spec da quest define exceção.
```

## 3.3 Cansaço

Cansaço é desgaste acumulado, diferente de Stamina.

Tempo interage com Cansaço assim:

```text
trabalhar até tarde aumenta risco de Cansaço residual;
dormir tarde reduz recuperação;
chuva, frio, calor e noite podem modificar ganho de Cansaço;
Nyx pode reduzir ganho de Cansaço à noite em contextos específicos;
Água Viva pode reduzir Cansaço de forma rara e limitada.
```

## 3.4 Fome

Fome interage com tempo por consumo diário.

Regras futuras possíveis:

```text
fome pode cair por hora ou por ações;
comer cedo pode proteger contra ganho de Cansaço;
comer tarde pode recuperar menos;
festivais e taverna podem criar buffs temporários;
inverno pode aumentar valor de comida quente;
verão pode aumentar valor de bebidas/itens refrescantes.
```

---

## 4. Calendário

## 4.1 Estrutura do ano

Baseline canônico:

```text
4 estações por ano.
28 dias por estação.
112 dias por ano.
```

Estações iniciais:

```text
Primavera
Verão
Outono
Inverno
```

Nomes finais podem ser adaptados para Vaalara depois, mas specs iniciais podem usar nomes comuns.

## 4.2 Semana

Baseline canônico:

```text
7 dias por semana.
4 semanas por estação.
```

Os nomes dos dias podem ser definidos depois.

Por enquanto:

```text
DayOfWeek 1-7
ou
nomes canônicos futuros ligados ao panteão/localidade.
```

Não bloquear implementação inicial esperando nomes perfeitos.

## 4.3 Calendário público

O calendário deve registrar:

```text
dia;
estação;
ano;
clima previsto;
evento lunar conhecido;
festivais;
aniversários futuros se existirem;
restock especial;
encomendas com prazo;
eventos de cidade conhecidos;
eventos de quest descobertos;
dias de loja fechada se houver.
```

Eventos secretos não aparecem até serem descobertos.

## 4.4 Calendário como objeto de mundo

A cidade deve ter calendário público em praça/quadro.

Possíveis fontes de calendário:

```text
calendário da praça;
quadro público;
loja de sementes;
prefeitura/cartório;
taverna para rumores/eventos;
Guilda das Estradas para clima/caverna;
UI do player após descoberta.
```

---

## 5. Estações

## 5.1 Primavera

Função:

```text
estação de início, recuperação, plantio e retomada rural.
```

Efeitos possíveis:

```text
maior variedade de crops iniciais;
chuva moderada;
festivais agrícolas simples;
NPCs mais presentes na praça/mercado;
boa estação para tutorial de cultivo;
Fonte pode parecer mais viva após Fragmento da Água;
foraging de flores, brotos e ervas;
pets passam mais tempo fora;
primeiros eventos de Thandra.
```

Mecânicas possíveis:

```text
crops comuns de ciclo curto;
fertilizante simples mais disponível;
pedidos de sementes/vegetais;
primeira competição leve de produção;
maior chance de chuva que Verão;
menor chance de tempestade que Outono.
```

## 5.2 Verão

Função:

```text
estação de produtividade alta, calor, risco de seca e eventos sociais.
```

Efeitos possíveis:

```text
crops de alto valor;
mais necessidade de irrigação;
chance de onda de calor;
animais precisam de cuidado maior;
NPCs podem mudar rotina para sombra/taverna/lago;
festivais de mercado, comida e música;
Senya pode ter mais eventos festivos/caóticos.
```

Mecânicas possíveis:

```text
solo seca mais rápido em Heat;
Stamina/Cansaço pode ser afetado por calor;
bebidas/comidas refrescantes ganham valor;
chuva menos frequente, tempestade mais marcante;
festivais aumentam demanda/preço de comida;
crops mágicas/lunares podem ter mutação em evento de Senya.
```

## 5.3 Outono

Função:

```text
estação de colheita, preparo, memória e transição.
```

Efeitos possíveis:

```text
crops robustas;
mais eventos de encomenda;
festivais de colheita;
maior presença de rumores;
Alihana pode ter eventos de sonho/memória;
boa estação para quests de Cindar/Memória;
vento e névoa mais frequentes;
foraging de cogumelos, raízes e frutos.
```

Mecânicas possíveis:

```text
pedidos de qualidade/quantidade;
maior uso de processadores;
festival de colheita;
chance maior de Fog;
rotas de NPC podem priorizar taverna/mercado;
Jardim das Estátuas pode reagir em noites específicas.
```

## 5.4 Inverno

Função:

```text
estação de escassez, introspecção, caverna e segredos.
```

Efeitos possíveis:

```text
menos crops comuns ao ar livre;
maior valor de estufa/processamento;
mais peso para caverna, mineração, pesca e crafting;
loja noturna/Nyx pode ficar mais relevante;
NPCs passam mais tempo em interiores;
festivais de luz, juramento, memória e proteção;
Fonte pode reagir visualmente de modo mais silencioso.
```

Mecânicas possíveis:

```text
crops externas limitadas;
estufa ganha importância;
Snow não rega;
Cold aumenta Cansaço externo;
comidas quentes têm valor maior;
caverna vira fonte relevante de recursos;
Nyx e segredos podem ter mais eventos;
animais precisam de abrigo/ração.
```

---

## 6. Clima

## 6.1 Tipos de clima iniciais

Tipos previstos:

```text
Sunny
Cloudy
Rain
Storm
Fog
Wind
Heat
Cold
Snow
```

Baseline inicial recomendável:

```text
Sunny
Cloudy
Rain
Storm
Snow
Fog
```

## 6.2 Sunny

Efeitos:

```text
sem irrigação automática;
rotina normal de NPCs;
bom para trabalho externo;
maior clareza visual;
clima neutro de gameplay.
```

Mecânicas possíveis:

```text
maior chance de NPC em área externa;
pets fora de abrigo;
mercado mais movimentado;
pesca normal;
caverna sem modificador climático.
```

## 6.3 Cloudy

Efeitos:

```text
sem irrigação automática;
menor intensidade visual;
pode preceder Rain/Storm;
clima bom para eventos de cidade.
```

Mecânicas possíveis:

```text
previsão menos precisa em early game;
NPCs seguem rotina normal;
alguns crops sensíveis a calor sofrem menos;
foraging leve.
```

## 6.4 Rain

Efeitos:

```text
molha áreas externas;
rega crops externas;
não molha estufa/interiores;
reduz necessidade de irrigação manual;
altera rotina de NPCs;
reduz presença de NPCs em áreas externas;
pode afetar pet/companion behavior;
pode alterar chance de peixe;
pode alterar certos recursos de foraging.
```

Chuva não deve:

```text
regar áreas cobertas;
ativar irrigação mágica automaticamente;
substituir sistemas avançados de irrigação;
resolver seca narrativa se evento disser o contrário.
```

## 6.5 Storm

Tempestade é clima mais forte que chuva.

Efeitos possíveis:

```text
rega crops externas;
reduz NPCs externos;
fecha ou altera certas rotas;
aumenta chance de galhos/pedras/recursos no dia seguinte;
pode bloquear pesca específica;
pode tornar caverna mais perigosa ou mais recompensadora;
pode gerar evento raro de Fonte/Água Viva se combinado com lua.
```

Evitar no baseline:

```text
destruição aleatória severa de crops;
dano a prédios sem sistema de reparo;
punição pesada sem aviso.
```

## 6.6 Snow

Neve é clima de inverno.

Efeitos:

```text
não rega crops comuns;
pode cobrir solo;
reduz crops externas;
muda foraging;
muda rotina de NPCs;
pode fortalecer eventos de silêncio/Nyx;
pode reduzir ritmo de cidade.
```

Mecânicas possíveis:

```text
animais ficam mais dependentes de abrigo;
NPCs usam interiores;
lojas podem abrir mais tarde em Storm/Snow forte;
mineração/crafting ganham valor;
pesca de inverno pode ter tabela própria.
```

## 6.7 Fog

Névoa é clima de mistério.

Efeitos:

```text
reduz visibilidade leve;
aumenta rumores;
pode ativar eventos de Alihana/Nyx;
pode alterar entrada de caverna;
pode revelar passagens, estátuas ou inscrições em momentos de quest.
```

Mecânicas possíveis:

```text
Jardim das Estátuas reage;
rumores especiais na taverna;
Yael/Sethra com diálogo especial;
inscrições aparecem sob Alihana;
Pedra Negra fica mais perceptível sob Nyx.
```

## 6.8 Heat

Calor é modificador de Verão ou evento de Senya.

Efeitos possíveis:

```text
aumenta consumo de Stamina em tarefas externas, se balance permitir;
aumenta necessidade de água/irrigação;
pode aumentar Cansaço;
pode alterar rotina de NPCs para interiores/lago/taverna;
pode aumentar demanda por comida/bebida específica.
```

## 6.9 Cold

Frio é modificador de Inverno ou evento de altitude/caverna.

Efeitos possíveis:

```text
aumenta Cansaço em exterior;
reduz crescimento externo;
valoriza roupas/comida/buffs;
pode alterar caverna e pets;
pode gerar demanda por carvão, madeira, comida quente e roupas.
```

---

## 7. Previsão do tempo

O jogador deve poder descobrir o clima do dia seguinte por:

```text
calendário/quadro público;
NPC de sementes/agricultura;
observação de céu;
serviço futuro;
habilidade/upgrade futuro.
```

Baseline:

```text
mostrar previsão de amanhã após o jogador interagir com calendário ou quadro público.
```

Não revelar:

```text
eventos secretos;
luas ocultas não descobertas;
eventos de quest não iniciados;
condições exatas de Mana antes da lore.
```

Possíveis níveis de previsão:

```text
Nível 0:
  sem previsão automática.

Nível 1:
  previsão do dia seguinte no calendário público.

Nível 2:
  previsão de 2-3 dias via upgrade/serviço.

Nível 3:
  previsão lunar/climática avançada ligada a quest, NPC ou artefato.
```

---

## 8. Ciclo lunar

## 8.1 Princípio

As três luas de Vaalara são sistemas de gameplay.

Elas afetam:

```text
fazenda;
cidade;
caverna;
economia;
Fonte;
Água Viva;
Mana;
crops lunares;
quests;
pets;
rumores;
loja noturna;
magia;
crafting;
eventos raros.
```

Elas não são decoração.

## 8.2 Modelo de ciclo recomendado

Para evitar complexidade excessiva, usar eventos lunares em vez de simular órbitas completas no começo.

Modelo inicial:

```text
A cada 7 dias há um evento lunar menor.
A cada 14 dias há um evento lunar médio.
A cada 28 dias há um evento lunar maior de estação.
```

Distribuição sugerida dentro de uma estação de 28 dias:

```text
Dia 7  — Alihana menor
Dia 14 — Senya menor/média
Dia 21 — Nyx menor/média
Dia 28 — Evento sazonal/lunar maior
```

Alternativa futura:

```text
ciclo independente por lua;
sobreposição rara;
eclipse/alinhamento;
três luas em evento de main quest.
```

Baseline não precisa simular astronomia real.

## 8.3 Visibilidade e descoberta

No começo:

```text
jogador sabe que há luas;
mas nem todos os efeitos são explicados.
```

Após progresso:

```text
calendário pode mostrar eventos lunares conhecidos;
NPCs comentam;
Fonte reage;
loja noturna muda;
crops lunares ficam mais compreensíveis.
```

Regra:

```text
Luas podem afetar o mundo antes de o jogador entender totalmente.
Mas efeitos que bloqueiam progressão precisam ser descobertos, sinalizados ou previsíveis.
```

---

## 9. Alihana

Domínios jogáveis:

```text
mistério;
música;
profecia;
sonhos;
silêncio;
sementes raras;
eventos da Fonte;
pistas de Anya;
memória;
Cindar.
```

Efeitos sistêmicos:

```text
aumenta chance de sonhos;
pode revelar inscrições;
pode fortalecer Água Viva;
pode favorecer crops noturnas;
pode aumentar chance de sementes raras;
pode atrair mercadores raros;
pode abrir eventos suaves de Fonte;
pode tornar certas memórias visíveis.
```

Main quest:

```text
Fragmento da Água;
Fragmento da Memória;
sonhos com Cindar;
pistas da Fonte;
diários legíveis apenas sob condição lunar.
```

Fazenda:

```text
crops noturnas;
Água Viva mais estável;
Mana pode ter chance maior de florescer se outros requisitos existirem;
pet pode ficar calmo;
Fonte pode brilhar de modo suave.
```

Cidade:

```text
músicos, sonhos, praça mais silenciosa;
Jardim das Estátuas reage;
Thalindra, Corvus, Yael ou NPCs de lore podem ter diálogos especiais;
mercador raro pode aparecer com seed/crop raro.
```

Caverna:

```text
lagos subterrâneos brilham;
inscrições nymirianas aparecem;
certos inimigos ficam menos agressivos ou mais raros;
baús/eventos de memória podem aparecer;
caminhos de lore podem ficar visíveis.
```

---

## 10. Senya

Domínios jogáveis:

```text
caos;
festas;
paixão;
magia;
mutação;
preços altos por demanda;
eventos imprevisíveis.
```

Efeitos sistêmicos:

```text
aumenta instabilidade mágica;
pode aumentar mutação de crops mágicas;
pode alterar preços em festas/eventos;
pode aumentar criaturas caóticas;
pode tornar crafting mágico instável;
pode aumentar risco/recompensa.
```

Main quest:

```text
Fragmento da Vida;
risco da Mana;
Água Viva reagindo de forma instável;
rituais que podem dar errado.
```

Fazenda:

```text
crops mágicas/lunares podem sofrer mutação;
fertilizantes arcanos podem ter efeito maior e risco maior;
Mana não deve florescer automaticamente só por Senya;
processadores mágicos podem ganhar bônus/risco futuro.
```

Cidade:

```text
festas;
taverna mais ativa;
preços de comida/evento podem variar;
NPCs impulsivos mudam rotina;
eventos sociais mais barulhentos.
```

Caverna:

```text
mais inimigos caóticos;
mais magia instável;
maior chance de elite/modificador;
recompensas raras com risco maior;
status effects mágicos podem aparecer com mais frequência.
```

---

## 11. Nyx

Domínios jogáveis:

```text
noite;
segredos;
morte;
sombra;
caverna;
criaturas noturnas;
exaustão/cansaço noturno;
itens secretos;
culto;
memória apagada.
```

Efeitos sistêmicos:

```text
ativa loja noturna/rumores;
aumenta chance de itens secretos;
pode reduzir ganho de Cansaço à noite em certas ações;
fortalece crops sombrias;
gato pode reagir com bônus especial;
criaturas noturnas podem aparecer de dia na caverna;
eventos de Pedra Negra ficam mais prováveis.
```

Main quest:

```text
culto de Nyx;
Sethra;
Fragmento da Memória;
Ato 3;
loja noturna;
NPC esquece;
Pedra Negra.
```

Fazenda:

```text
gato reage;
crops sombrias podem crescer melhor;
Fonte pode turvar ou emitir reação silenciosa;
Água Viva pode reagir a corrupção;
rumores de sombra podem surgir sem combate direto na fazenda.
```

Cidade:

```text
loja noturna ativa;
rumores secretos;
Jardim das Estátuas muda;
Sethra/Yael têm diálogos especiais;
NPCs evitam certas áreas;
alguns serviços podem fechar cedo.
```

Caverna:

```text
mais inimigos de sombra/noite;
Pedra Negra mais ativa;
itens secretos;
risco de memória/corrupção maior;
atalhos ou inscrições ocultas podem aparecer.
```

---

## 12. Alinhamentos lunares raros

Eventos raros:

```text
Alihana + Nyx:
  memória e segredo;
  inscrições antigas;
  sonho inquieto;
  Jardim das Estátuas;
  risco de lembrar algo que deveria estar esquecido.

Alihana + Senya:
  sonho profético instável;
  magia e memória;
  crops lunares raras;
  Água Viva instável, mas poderosa.

Senya + Nyx:
  mutação sombria;
  Pedra Negra instável;
  caverna perigosa;
  loot raro com risco maior.

Alihana + Senya + Nyx:
  evento raro de estação ou main quest;
  Fonte reage;
  Mana pode florescer se todos os outros requisitos existirem;
  nível 100/101 pode exigir ou simular esse alinhamento.
```

Não usar alinhamento triplo como evento comum.

---

## 13. Relação com NPC schedules

A rotina individual dos NPCs fica em:

```text
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Este documento define modificadores globais.

NPC schedules devem poder responder a:

```text
hora do dia;
dia da semana;
estação;
clima;
festival;
evento lunar;
quest state;
loja aberta/fechada;
romance/social futuro;
main quest state;
chuva;
tempestade;
neve;
Nyx night events.
```

Regras:

```text
Não duplicar rotina individual aqui.
Este documento só define tags e condições globais que schedules podem consumir.
```

Exemplo conceitual:

```text
Se Weather = Rain:
  NPCs que trabalham ao ar livre usam RainFallbackWaypoint.

Se LunarEvent = Nyx:
  Loja Noturna pode abrir inventário especial.
  Yael/Sethra podem ter diálogo específico.

Se Season = Winter:
  NPCs passam mais tempo em interiores.
```

---

## 14. Relação com lojas e economia

Calendário, clima e luas podem afetar economia.

Efeitos permitidos:

```text
restock semanal;
restock por estação;
itens sazonais;
preço especial em festival;
mercador raro de Finan;
loja noturna em Nyx;
sementes raras em Alihana;
itens mágicos instáveis em Senya;
demanda de encomendas por estação;
aumento temporário de demanda por comida, madeira, carvão, poções ou tecidos;
variação de estoque em tempestade/inverno.
```

Evitar:

```text
arbitragem fácil;
preço mudando de forma opaca;
restock ao abrir menu;
loja resetando sem day transition;
economia quebrada por Mana;
evento lunar dando dinheiro infinito.
```

Regra:

```text
Economy Pricing Stock Refresh continua vencendo para preço, BaseValue, restock, SellPoint e anti-arbitragem.
Este documento define apenas quando eventos sazonais, climáticos e lunares podem solicitar variação.
```

---

## 15. Relação com crops e agricultura

Crops podem depender de:

```text
estação;
água;
fertilizante;
qualidade do solo;
clima;
estufa;
lua;
Fonte;
Água Viva;
Mana;
corrupção.
```

Categorias futuras:

```text
crops comuns sazonais;
crops resistentes;
crops de estufa;
crops mágicas/lunares;
crops sombrias;
crops de festival;
Mana como cultivo especial raro.
```

Regras:

```text
Crops comuns seguem estação e água.
Crops lunares exigem condições especiais.
Mana pode ser plantado/cultivado em condição rara, mas não é crop comum.
Chuva molha áreas externas.
Estufa isola clima externo, salvo eventos especiais.
```

Mecânicas possíveis:

```text
crop cresce apenas em estação válida;
crop fora de estação morre ou não aceita plantio;
estufa ignora estação para crops comuns;
chuva aplica WateredState em tiles externos válidos;
Storm pode aplicar galhos/pedras no dia seguinte;
Heat pode exigir irrigação extra;
Cold/Snow pode bloquear crescimento externo;
Alihana pode beneficiar crops noturnas;
Senya pode mutar crops mágicas;
Nyx pode beneficiar crops sombrias;
Água Viva pode ser ingrediente raro, não irrigação comum.
```

---

## 16. Relação com pets

Pets podem reagir a:

```text
clima;
estação;
hora do dia;
lua;
caverna;
estado da Fonte;
NPC visitante;
quest state.
```

Exemplos:

```text
gato reage a Nyx;
cachorro pode alertar em tempestade;
pet fica mais tempo em abrigo durante chuva/neve;
pet pode dar hint de item raro em certos climas;
pet pode se aproximar da Fonte em evento lunar;
pet não deve resolver evento sozinho.
```

Pets continuam vencidos por `PETS_DIRECTION.md` para:

```text
bond;
mood;
energy;
home area;
routine;
cave behavior;
pet HUD;
save/load.
```

---

## 17. Relação com companions

Companions podem reagir a:

```text
clima;
estação;
hora;
evento lunar;
quest state;
caverna;
farm job;
visita;
romance futuro.
```

Exemplos:

```text
companion agricultor evita trabalho externo em tempestade;
companion minerador pode preferir caverna em inverno;
companion ligado a Nyx comenta noite/lua;
partner helper futuro respeita clima e orçamento global;
companion job externo pode falhar, atrasar ou trocar fallback em Storm/Snow.
```

Companions continuam vencidos por `COMPANIONS_DIRECTION.md`.

---

## 18. Relação com caverna

Clima e lua podem afetar a caverna, mas com moderação.

Efeitos permitidos:

```text
tabelas de spawn modificadas;
maior chance de elites;
recursos raros;
lagos subterrâneos brilhando;
Pedra Negra mais ativa;
eventos de memória;
inimigos noturnos em dia de Nyx;
instabilidade mágica em Senya;
inscrições reveladas em Alihana;
rotas secretas temporárias;
modificadores de boss futuro.
```

Evitar:

```text
caverna impossível por clima ruim;
bloqueio aleatório de progresso principal;
mecânica opaca sem feedback;
evento raro obrigatório sem calendário/pista.
```

Regra:

```text
Main quest pode exigir condição lunar específica, mas deve dar pista e controle suficiente ao jogador.
```

---

## 19. Relação com Fonte de Anya

A Fonte pode reagir a:

```text
fragmentos de Anya;
Água Viva;
Alihana;
Nyx;
Senya;
corrupção subterrânea;
Pedra Negra;
eventos de main quest;
clima forte;
alinhamento lunar raro.
```

Usos possíveis:

```text
recarregar Água Viva em evento lunar;
mostrar reflexos de memória;
escurecer/turvar em Nyx/corrupção;
ficar instável em Senya;
brilhar em Alihana;
desbloquear ritual futuro;
responder à recuperação de fragmentos;
exibir warning narrativo sem explicar tudo.
```

Regra:

```text
Fonte não revela spoilers cedo.
Fonte UI só mostra funções desbloqueadas por fragmento.
```

---

## 20. Relação com Mana

Mana pode ser cultivado, mas é difícil florescer.

Condições possíveis:

```text
solo especial;
estação específica;
Água Viva;
proximidade da Fonte;
fragmentos recuperados;
evento lunar;
proteção contra corrupção;
tempo longo;
baixa produção.
```

Regra:

```text
Nenhuma lua sozinha faz Mana florescer.
Nenhum clima sozinho faz Mana florescer.
Mana não vira crop comum.
Mana não vira dinheiro infinito.
```

Possíveis combinações:

```text
Alihana:
  melhora estabilidade de florescimento.

Senya:
  aumenta risco de mutação/instabilidade.

Nyx:
  pode revelar problema/corrupção, não necessariamente ajudar.

Rain/Storm:
  não substitui Água Viva.

Spring/Autumn:
  podem ser mais favoráveis que Summer/Winter, se balance futuro decidir.
```

---

## 21. Relação com quests

Quest system deve poder usar condições:

```text
CurrentTime
Day
Weekday
Season
Year
Weather
TomorrowWeather
LunarEvent
MoonKnown
FestivalActive
QuestState
FonteStage
CaveDepthReached
NpcAvailability
ShopOpenState
```

Exemplos:

```text
diário legível apenas em Alihana;
loja noturna abre em Nyx;
ritual falha ou muda em Senya;
festival só acontece em dia específico;
caverna revela inscrição em névoa;
Fonte recarrega Água Viva em evento lunar conhecido;
Mana floresce se condição sazonal/lunar/Água Viva for cumprida.
```

Regra:

```text
Quest obrigatória não deve depender de evento raro sem calendário, pista e forma razoável de esperar.
```

---

## 22. Festivais

Festivais são eventos de calendário, não apenas decoração.

Tipos futuros:

```text
festival de plantio;
festival de colheita;
festival de comida/taverna;
festival de música/Alihana;
festival de juramento/Kanthor;
festival rural de Thandra;
festival de mercado/Finan;
festival noturno/Nyx, secreto ou semioculto;
evento caótico/festivo de Senya.
```

Regras:

```text
festival altera NPC schedules;
festival pode fechar lojas comuns;
festival pode abrir barracas temporárias;
festival pode alterar preços/demanda;
festival pode gerar social events;
festival pode ter quest hooks;
festival não deve quebrar rotina agrícola essencial sem aviso.
```

Mecânicas possíveis:

```text
minigame simples;
competição de crop/produto;
pedido temporário;
loja temporária;
diálogo especial;
romance/social boost futuro;
quest de lore;
evento lunar acoplado;
preços sazonais;
recompensa cosmética ou funcional leve.
```

---

## 23. Save/load

Persistir:

```text
CurrentDay
CurrentSeason
CurrentYear
CurrentTime
CurrentWeather
TomorrowWeather
ActiveLunarEvent
KnownLunarEvents
FestivalState
WeatherSeed
CalendarEventStates
PendingDayTransitionState
LastProcessedDay
DayTransitionVersion
```

Não persistir como fonte primária:

```text
preço final calculado;
texto de previsão renderizado;
NPC runtime object;
VFX atual do clima;
UI state transitório;
pathfinding em progresso;
forecast text final.
```

Load deve:

```text
restaurar tempo/calendário;
recalcular visual de clima;
resolver NPC schedule pelo horário;
resolver lojas pelo horário/restock state;
resolver crops pelo estado persistido;
resolver eventos lunares ativos;
resolver UI pelo estado atual, não salvar UI transitória.
```

---

## 24. Data assets futuros

Possíveis assets:

```text
TimeConfigSO
CalendarConfigSO
SeasonDataSO
WeatherDataSO
WeatherTableSO
WeatherForecastRuleSO
LunarCycleConfigSO
LunarEventDataSO
FestivalDataSO
CalendarEventSO
NpcScheduleModifierSO
CropSeasonRuleSO
FarmWeatherRuleSO
CaveWeatherLunarModifierSO
FonteLunarReactionSO
ManaBloomingRuleSO
ShopCalendarModifierSO
PetWeatherReactionSO
CompanionScheduleModifierSO
QuestTemporalConditionSO
```

### 24.1 TimeConfigSO

Campos conceituais:

```text
DayStartHour
LateNightHour
ForcedSleepHour
SecondsPerGameHour
PauseTimeInModalUI
AllowTimeInCave
AllowTimeInDialogue
AllowTimeInCombat
DayTransitionScenePolicy
```

### 24.2 CalendarConfigSO

Campos conceituais:

```text
DaysPerWeek
DaysPerSeason
SeasonsPerYear
SeasonIds
WeekdayIds
YearStartSeason
FestivalRules
BirthdayRulesFuture
```

### 24.3 SeasonDataSO

Campos conceituais:

```text
SeasonId
DisplayName
CropTagsAllowed
WeatherWeights
ForageTableId
FishTableModifierId
FestivalIds
ShopSeasonTags
NpcScheduleTags
```

### 24.4 WeatherDataSO

Campos conceituais:

```text
WeatherId
DisplayName
SeasonWeights
CanWaterCrops
CanOccurInSeason
NpcScheduleTag
FarmModifierTags
CaveModifierTags
PetReactionTags
CompanionReactionTags
VFXProfileId
SFXProfileId
```

### 24.5 LunarEventDataSO

Campos conceituais:

```text
LunarEventId
MoonId
DisplayName
KnownByDefault
SeasonRules
DayRules
QuestUnlockRules
FarmModifierTags
CityModifierTags
CaveModifierTags
FonteReactionTags
ManaModifierTags
ShopModifierTags
PetModifierTags
CompanionModifierTags
```

### 24.6 CalendarEventSO / FestivalDataSO

Campos conceituais:

```text
EventId
DisplayName
Season
Day
StartTime
EndTime
LocationId
NpcScheduleOverrideId
ShopOverrideRules
QuestHooks
RewardRules
KnownByDefault
RepeatEveryYear
```

---

## 25. UI/UX

UI/UX é definido por:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

Este sistema precisa comunicar:

```text
hora;
dia;
estação;
clima atual;
previsão;
evento lunar conhecido;
festival;
evento de calendário;
loja fechada/aberta por horário;
quest esperando condição temporal;
Fonte reagindo a lua/clima.
```

Não comunicar cedo:

```text
eventos secretos;
Nyx oculto antes da descoberta;
condições exatas de Mana antes de lore;
evento do nível 101 antes de main quest.
```

---

## 26. Regras anti-exploit

```text
Jogador não deve manipular calendário para dinheiro infinito.
Restock não acontece ao abrir menu.
Dormir repetidamente não deve gerar recursos raros sem custo.
Eventos lunares não devem dar loot infinito.
Mana não deve florescer por spam de dias.
Chuva não deve substituir irrigação avançada em todos os contextos.
Previsão não deve revelar segredos.
Quest obrigatória não deve exigir espera opaca demais.
Tempo não deve correr em menus modais.
Festival não deve permitir compra/venda infinita com margem positiva fácil.
```

---

## 27. Specs futuras recomendadas

```text
spec_time_clock_day_transition_runtime.md
spec_calendar_season_year_runtime.md
spec_weather_generation_forecast_runtime.md
spec_rain_irrigation_crop_integration.md
spec_weather_farm_pet_companion_reactions.md
spec_lunar_cycle_event_runtime.md
spec_lunar_fonte_mana_reactions_future.md
spec_calendar_festivals_events_runtime.md
spec_npc_schedule_weather_lunar_modifiers.md
spec_shop_calendar_weather_lunar_modifiers.md
spec_cave_weather_lunar_modifiers_future.md
spec_calendar_ui_weather_lunar_display.md
spec_time_save_load_state.md
spec_calendar_quest_temporal_conditions.md
```

---

## 28. Decisões fechadas

```text
O jogo usa 4 estações por ano.
Cada estação tem 28 dias.
Ano tem 112 dias.
Semana tem 7 dias.
Dia jogável começa às 06:00.
02:00 é limite padrão de colapso/sono forçado.
Escala inicial recomendada: 1 hora in-game = 60 segundos reais.
Tempo pausa em UI modal.
Chuva molha áreas externas.
NPC rotina individual não é definida aqui; fica em City Layout/Schedule.
Este documento define tags globais de tempo/clima/lua para schedules consumirem.
As três luas são sistemas de gameplay, não decoração.
Alihana = memória, sonhos, Fonte, Cindar, Água Viva.
Senya = caos, magia, mutação, instabilidade.
Nyx = noite, segredo, loja noturna, Pedra Negra, esquecimento.
Mana pode depender de estação/lua/Água Viva/Fonte, mas nenhuma condição isolada basta.
Quest obrigatória com tempo/lua precisa dar pista e controle razoável ao jogador.
```

---

## 29. Pendências abertas

```text
Definir nomes canônicos das estações em Vaalara ou manter Primavera/Verão/Outono/Inverno.
Definir nomes dos dias da semana.
Definir se 02:00 será limite final ou se 00:00 aplica penalidade mais forte.
Definir se eventos lunares são fixos por calendário ou semi-randômicos por seed.
Definir tabela inicial de crops por estação.
Definir tabela inicial de clima por estação.
Definir quais festivais entram primeiro.
Definir se aniversário de NPC entra no primeiro pacote social ou fica futuro.
Definir se loja noturna abre toda noite ou apenas em Nyx/quest.
Definir se caverna tem previsão/alerta de modificador lunar.
Definir se o calendário mostra luas desde o início ou apenas após descoberta.
Definir quais efeitos de weather entram no pacote inicial e quais ficam future.
```
