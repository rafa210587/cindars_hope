# Cindar's Hope — Main Quest Progression Refinement Direction

> **Status:** direção de refinamento jogável da main quest  
> **Local:** `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md`  
> - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`  
> - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`  
> - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`  
> **Função:** traduzir a lore principal em atos, fragmentos, eventos, mecânicas, gates, sistemas afetados e regras para specs futuras.  
> **Não é spec implementável.** Specs futuras devem quebrar isto em arquivos menores em `docs/specs/a_implementar/`.

---

## 0. Regra de uso

Este documento transforma a lore principal em estrutura jogável.

Ele define:

```text
atos;
fragmentos;
eventos principais;
sistemas afetados;
gates de progressão;
funções da Fonte;
uso da cidade;
uso da fazenda;
uso da caverna;
uso das luas;
papel dos NPCs principais;
tipos de quests futuras.
```

Ele não define:

```text
classes Unity;
ScriptableObjects finais;
arquivos de implementação;
cenas finais;
timelines finais;
diálogos completos;
valores numéricos finais;
balance definitivo;
validação Unity.
```

---

## 1. Estrutura macro

A main quest usa quatro atos:

```text
Ato 1 — Fonte e Esquecimento
Ato 2 — Cindar e o Arco da Memória
Ato 3 — Culto, Pedra Negra e Vida
Ato 4 — Nível 100/101 e Esperança
```

A progressão é híbrida:

```text
fazenda;
cidade;
caverna;
Fonte;
NPCs;
luas;
fragmentos;
bosses;
eventos de memória.
```

A main quest não deve ser apenas “descer até o nível 100”.

A caverna é eixo de profundidade, mas cidade e fazenda precisam importar em marcos narrativos.

---

## 2. Estrutura dos fragmentos

Fragmentos principais:

```text
1. Água
2. Memória
3. Vida
4. Esperança
```

Cada fragmento deve ter:

```text
local de descoberta;
NPC ou sistema que puxa a quest;
ameaça associada;
loop de gameplay;
função desbloqueada na Fonte;
mini-revelação de lore;
efeito visual na Fonte;
mudança perceptível no mundo.
```

Ordem canônica da main quest:

```text
Água -> Memória -> Vida -> Esperança
```

---

## 3. Fonte da Fazenda — progressão mecânica

### 3.1 Estado 0 — Fonte Adormecida

Função:

```text
respawn / ponto de retorno.
```

Evento de desbloqueio:

```text
primeira morte/desmaio/evento crítico do jogador.
```

Efeito narrativo:

```text
o jogador acorda na Fonte;
a cidade não entende o ocorrido;
Padre Corvus esquece a Litania do Primeiro Retorno.
```

Não desbloqueia:

```text
Água Viva;
respec;
purificação;
cura avançada.
```

### 3.2 Fragmento 1 — Água

Função desbloqueada:

```text
Água Viva limitada.
```

Usos possíveis:

```text
cura leve;
redução controlada de cansaço;
ritual simples;
purificação pequena;
ativação de pistas aquáticas;
interação com lagos especiais da caverna.
```

Gates possíveis:

```text
primeira faixa relevante da caverna;
primeira Pedra Negra;
primeiro ritual de Alihana;
primeiro lago subterrâneo.
```

Mudança visual:

```text
água volta a circular na Fonte;
brilho fraco;
som de água mais claro;
pequenos símbolos nymirianos aparecem.
```

Mini-revelação:

```text
a Fonte não é apenas antiga;
ela responde a fragmentos de Anya.
```

### 3.3 Fragmento 2 — Memória

Função desbloqueada:

```text
respec.
```

Justificativa de lore:

```text
a Fonte permite lembrar caminhos possíveis de vida e reorganizar escolhas.
```

Usos possíveis:

```text
respec de skill tree;
revelar inscrições apagadas;
recuperar trechos de diário de Cindar;
resistir a esquecimento leve;
desbloquear diálogo especial com NPCs afetados pela Pedra Negra.
```

Gates possíveis:

```text
quest urbana de esquecimento;
investigação com Padre Corvus;
chegada de Vaelrion;
ruína sob a cidade;
evento lunar de Alihana/Nyx.
```

Mudança visual:

```text
reflexos na água mostram cenas antigas;
ecos de Cindar;
símbolos aparecem e desaparecem.
```

Mini-revelação:

```text
Cindar era Nymiriana;
Anya não pode voltar inteira;
a cidade preservou vestígios sem saber.
```

### 3.4 Fragmento 3 — Vida

Função desbloqueada:

```text
purificação e cura avançada limitada.
```

Usos possíveis:

```text
purificar Água Viva corrompida;
reduzir penalidades graves da caverna;
curar eventos específicos de NPC/animal/solo;
proteger contra efeito raro de corrupção;
desbloquear ritual profundo da Fonte.
```

Gates possíveis:

```text
caverna profunda;
boss de Água Viva corrompida;
ritual de Sethra;
Pedra Negra drenando alma/memória;
evento de Senya.
```

Mudança visual:

```text
plantas crescem perto da Fonte;
água fica mais viva;
pequenas partículas verdes/douradas;
solo próximo reage.
```

Mini-revelação:

```text
Bromécia tentou converter vida em sistema;
a Pedra Negra drena alma, memória e Água Viva.
```

### 3.5 Fragmento 4 — Esperança

Função desbloqueada:

```text
decisão final.
```

Usos possíveis:

```text
proteger fragmentos;
selar Arco da Memória;
usar Arco parcialmente purificado;
definir estado final da Fonte;
definir relação final entre Fonte, Mana e caverna.
```

Gate:

```text
nível 100/101;
boss final Arquivista do Silêncio;
Arco da Memória;
alinhamento ou evento das três luas.
```

Mudança visual:

```text
Fonte assume estado final conforme escolha.
```

Mini-revelação:

```text
Anya não pode ser restaurada;
proteger esperança não é negar perda;
Cindar protegeu um futuro possível, não uma ressurreição completa.
```

---

## 4. Ato 1 — Fonte e Esquecimento

### 4.1 Objetivo narrativo

Introduzir:

```text
Fonte;
respawn;
memória apagada;
primeiro sinal de Anya;
primeira relação entre caverna e Pedra Negra.
```

### 4.2 Evento inicial

```text
Jogador chega à fazenda.
Fonte parece ruína antiga.
Jogador sofre morte/desmaio/acidente.
Jogador acorda na Fonte.
Padre Corvus esquece a Litania do Primeiro Retorno.
Fonte reage.
```

### 4.3 Sistemas usados

```text
fazenda inicial;
Fonte;
respawn;
cidade;
Padre Corvus;
primeira caverna;
primeiro evento de memória;
primeira Pedra Negra;
primeiros rumores.
```

### 4.4 Questline do ato

```text
1. Explorar fazenda.
2. Encontrar Fonte.
3. Sofrer evento de retorno.
4. Falar com Padre Corvus.
5. Investigar litania esquecida.
6. Entrar na caverna.
7. Encontrar água subterrânea estranha.
8. Encontrar primeira Pedra Negra.
9. Recuperar Fragmento da Água.
10. Fonte desbloqueia Água Viva limitada.
```

### 4.5 Threat design

A ameaça ainda deve ser discreta.

Usar:

```text
pequenos esquecimentos;
água escurecida;
NPC desconfortável;
criatura estranha na caverna;
Pedra Negra pequena;
sonho curto de Alihana.
```

Evitar:

```text
culto revelado cedo;
Vaelrion como vilão;
explicação completa de Anya;
nível 101 citado diretamente demais.
```

---

## 5. Ato 2 — Cindar e o Arco da Memória

### 5.1 Objetivo narrativo

Revelar:

```text
Cindar;
Nymirianos;
Memória;
Arco da Memória;
Vaelrion como aliado acadêmico ambíguo.
```

### 5.2 Entrada de Vaelrion

```text
Professor Vaelrion Aelth-Silberharth chega da capital.
Ele é Ninrorin, especialista em Elyndor.
Ele oferece ajuda técnica.
Ele demonstra arrogância sutil.
```

### 5.3 Sistemas usados

```text
cidade;
templo de Kanthor;
arquivos/diários;
ruínas sob cidade;
Fonte;
caverna intermediária;
eventos de Alihana;
diálogos de NPCs;
respec.
```

### 5.4 Questline do ato

```text
1. Padre Corvus admite que a oração não parece totalmente de Kanthor.
2. Vaelrion chega para estudar inscrições e ruínas.
3. Jogador encontra símbolos de Cindar.
4. Primeiros registros mostram Cindar como Nymiriana.
5. NPCs esquecem pequenos fatos pessoais.
6. Ruína urbana ou subterrânea revela o termo Arco da Memória.
7. Vaelrion interpreta o Arco como tecnologia.
8. Jogador recupera Fragmento da Memória.
9. Fonte desbloqueia respec.
```

### 5.5 Revelações

```text
Cindar não era só fundadora folclórica.
Cindar era última sacerdotisa Nymiriana local.
A Fonte guarda fragmentos.
O esquecimento é causado por drenagem de memória.
Existe uma estrutura profunda chamada Arco da Memória.
```

### 5.6 Risco narrativo

Vaelrion deve parecer útil.

A arrogância aparece, mas ele ainda não deve parecer vilão completo.

---

## 6. Ato 3 — Culto, Pedra Negra e Vida

### 6.1 Objetivo narrativo

Revelar:

```text
Sethra;
culto de Nyx;
Pedra Negra;
drenagem de alma/memória/Água Viva;
Vaelrion cruzando limites;
Fragmento da Vida.
```

### 6.2 Entrada de Sethra

Sethra já pode existir na cidade antes, como dona da loja noturna:

```text
A Vela Sem Chama
```

A revelação de que ela lidera o culto ocorre no Ato 3.

### 6.3 Sistemas usados

```text
loja noturna;
Nyx;
rumores;
caverna profunda;
Pedra Negra;
Água Viva corrompida;
Fonte;
purificação;
eventos de Senya;
eventos de esquecimento severo;
primeiros efeitos de alma drenada.
```

### 6.4 Questline do ato

```text
1. Loja noturna ganha importância.
2. Yael ajuda a separar Nyx do culto extremista.
3. Sethra é revelada como líder da célula local.
4. Culto afirma que esquecer é misericórdia.
5. Vaelrion usa conhecimento do culto para acessar camadas proibidas.
6. Culto usa Vaelrion para encontrar o Arco.
7. Pedra Negra passa a drenar alma, memória e Água Viva.
8. Jogador enfrenta evento/boss ligado à Água Viva corrompida.
9. Fragmento da Vida é recuperado.
10. Fonte desbloqueia purificação/cura avançada limitada.
```

### 6.5 Revelações

```text
Pedra Negra cultista drena alma, memória e Água Viva.
Sethra quer silenciar Anya.
Vaelrion quer usar o Arco.
Os dois estão abrindo o mesmo selo por motivos diferentes.
```

### 6.6 Mecânica de mundo

Eventos leves podem ocorrer por tempo:

```text
rumores;
NPCs esquecendo detalhes;
água turva;
itens noturnos diferentes;
gato reagindo a Nyx;
sonhos sob Alihana.
```

Eventos graves só avançam por marco de quest.

---

## 7. Ato 4 — Nível 100/101 e Esperança

### 7.1 Objetivo narrativo

Conectar tudo no nível 101 e entregar decisão final.

### 7.2 Sistemas usados

```text
caverna profunda;
boss gate nível 100;
nível 101;
Arco da Memória;
Vaelrion;
Sethra;
Pedra Negra;
Fonte;
Mana;
três luas;
boss final;
escolha final.
```

### 7.3 Estrutura

```text
1. Jogador abre boss gate do nível 100.
2. Derrota guardião/chefe do portão.
3. Acesso ao nível 101.
4. Nível 101 revela sobreposição de camadas.
5. Vaelrion ativa o Arco.
6. Sethra tenta silenciar os fragmentos.
7. Pedra Negra corrompe ambos os processos.
8. Surge o Arquivista do Silêncio.
9. Boss final.
10. Fragmento da Esperança é revelado.
11. Jogador escolhe Proteger, Selar ou Usar.
```

### 7.4 Nível 101 deve conter

```text
santuário nymiriano;
máquina bromeciana;
Arco de Elyndor;
Pedras Negras;
Água Viva corrompida;
raiz/estrutura de Mana;
ecos de Cindar;
fragmentos de Anya;
boss final.
```

### 7.5 Boss final

```text
O Arquivista do Silêncio
```

Natureza:

```text
Vaelrion + Arco da Memória + Pedra Negra + máquina bromeciana + ritual de Sethra.
```

---

## 8. Finais e efeitos sistêmicos

### 8.1 Final Proteger

Conceito:

```text
proteger fragmentos sem usá-los como ferramenta.
```

Efeitos possíveis:

```text
Fonte viva e estável;
Mana floresce raramente;
cidade preserva memória;
pós-game mais espiritual/natural;
menor uso de tecnologia bromeciana.
```

### 8.2 Final Selar

Conceito:

```text
reforçar o selo de Cindar.
```

Efeitos possíveis:

```text
corrupção contida;
Fonte mais limitada;
caverna mais estável;
alguns recursos avançados bloqueados ou reduzidos;
tom melancólico.
```

### 8.3 Final Usar

Conceito:

```text
usar parte do Arco purificado sem restaurar Anya.
```

Efeitos possíveis:

```text
mais tecnologia bromeciana disponível;
Mana floresce com mais previsibilidade;
caverna pós-game com novas áreas;
cidade progride materialmente;
risco narrativo de repetir Bromécia.
```

---

## 9. Mana como sistema

Regra atualizada:

```text
Mana pode ser plantado.
É difícil fazer florescer.
Não é poder absoluto.
Não é crop comum.
```

Mecânicas possíveis:

```text
Semente/Raiz de Mana;
solo especial;
Água Viva;
evento lunar;
proteção contra corrupção;
estação específica;
proximidade da Fonte;
requisitos de fragmentos;
tempo longo de crescimento;
baixa produção;
uso em poções/crafting/magia.
```

Anti-exploit:

```text
não vender em massa;
não produzir toda semana;
não substituir economia comum;
não banalizar MP/cura;
não virar requisito para todo sistema.
```

---

## 10. Uso das luas em quests

### 10.1 Alihana

Uso:

```text
memórias;
sonhos;
Cindar;
Fragmento da Água;
Fragmento da Memória;
pistas da Fonte.
```

Eventos:

```text
sonho com água;
inscrição revelada;
diário legível apenas sob lua;
visão curta de Cindar.
```

### 10.2 Senya

Uso:

```text
instabilidade;
mutação;
magia;
Fragmento da Vida;
risco da Mana.
```

Eventos:

```text
Água Viva reage de forma instável;
crops mágicas sofrem mutação;
criaturas caóticas aparecem;
ritual dá errado.
```

### 10.3 Nyx

Uso:

```text
culto;
esquecimento;
Pedra Negra;
loja noturna;
Fragmento da Memória;
Ato 3.
```

Eventos:

```text
NPC esquece;
loja muda;
gato reage;
Pedra Negra aparece;
rumor secreto abre.
```

---

## 11. Papel dos NPCs principais

### 11.1 Padre Corvus

Função:

```text
gatilho inicial;
ponte entre Kanthor e Anya esquecida;
autoridade abalada;
guardião de oração que não compreende totalmente.
```

Quest role:

```text
Litania do Primeiro Retorno;
investigação de memória;
conflito fé pública vs verdade apagada.
```

### 11.2 Vaelrion Aelth-Silberharth

Função:

```text
pesquisador;
intérprete de Elyndor;
mentor falso;
antagonista racional;
gatilho do Arco da Memória.
```

Quest role:

```text
traduções;
ruínas;
Arco da Memória;
quebra de limites;
boss final.
```

### 11.3 Sethra Veyl-Nocthar

Função:

```text
líder do culto de Nyx;
dona de loja noturna;
ideologia do esquecimento;
ameaça ritual.
```

Quest role:

```text
rumores;
Pedra Negra;
drenagem de memória;
rituais;
Ato 3.
```

### 11.4 Yael Noite-Mansa

Função:

```text
ponte com Nyx;
contraponto ao culto;
NPC social/romance possível;
interpretação não vilanesca de segredo, luto e silêncio.
```

Quest role:

```text
ajuda a entender Sethra;
diferencia Nyx de culto;
pode dar acesso a pistas noturnas.
```

---

## 12. Tipos de quests futuras

### 12.1 Main quests

```text
chegada à fazenda;
primeiro retorno pela Fonte;
oração esquecida;
primeira Pedra Negra;
Fragmento da Água;
chegada de Vaelrion;
investigação de Cindar;
Fragmento da Memória;
revelação de Sethra;
Água Viva corrompida;
Fragmento da Vida;
boss gate nível 100;
nível 101;
Arquivista do Silêncio;
escolha final.
```

### 12.2 Side quests conectadas

```text
NPCs esquecendo fatos pessoais;
restaurar trechos da Litania;
recuperar diário de Cindar;
investigar loja noturna;
purificar poço/lago;
ajudar Padre Corvus;
ajudar Yael contra Sethra;
investigar artefato de Elyndor;
cultivar primeira Mana;
proteger a Fonte em evento raro.
```

### 12.3 Quests sistêmicas

```text
desbloquear Água Viva;
desbloquear respec;
desbloquear purificação;
desbloquear cultivo especial de Mana;
desbloquear checkpoints profundos;
desbloquear entrada do nível 101.
```

---

## 13. Regras de design para futuras specs

### 13.1 Não fazer

```text
não restaurar Anya completamente;
não transformar Anya em NPC comum;
não fazer Mana virar crop comum de dinheiro infinito;
não fazer Vaelrion vilão caricato desde o começo;
não fazer Nyx ser simplesmente deusa má;
não fazer Sethra querer destruir o mundo sem motivo;
não explicar tudo por diálogo expositivo;
não fazer a caverna substituir cidade/fazenda;
não fazer a Fonte liberar tudo cedo;
não deixar romance/companions obrigatórios para main quest.
```

### 13.2 Fazer

```text
mostrar lore por consequência jogável;
usar eventos de memória;
usar Fonte como progressão;
usar cidade como camada social da main quest;
usar caverna como camada de horror;
usar luas como gatilhos pontuais;
usar NPCs para revelar perspectivas;
manter Anya fragmentada;
manter Cindar ambígua;
manter Bromécia útil e perigosa;
manter finais moralmente diferentes.
```

---

## 14. Pendências antes de specs implementáveis

Ainda falta decidir:

```text
nomes dos chefes intermediários;
em quais faixas da caverna cada fragmento aparece;
qual evento exato causa a primeira morte/desmaio;
como Vaelrion chega oficialmente;
qual é a relação exata entre Sethra e Yael;
se Padre Corvus pode ser companion/quest giver recorrente;
como a cidade reage a cada fragmento recuperado;
quais tecnologias bromecianas ficam disponíveis no final Usar;
quais limitações mecânicas do final Selar;
quais recompensas permanentes do final Proteger.
```

Quest log, Fonte UI, notificações de fragmentos, feedback de mundo, modal de decisão final e spoiler control são definidos em:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

## 15. Síntese operacional

```text
Main quest = recuperar/proteger 4 fragmentos de Anya.
Fonte = hub mecânico da progressão.
Cidade = camada social e memória apagada.
Fazenda = base sagrada e ponto de estabilidade.
Caverna = horror, recursos e acesso às verdades.
Vaelrion = conhecimento arrogante.
Sethra = esquecimento como falsa misericórdia.
Arco da Memória = máquina de preservação corrompida em drenagem.
Arquivista do Silêncio = boss final.
Final = Proteger / Selar / Usar.
```
