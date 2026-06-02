# Cindar's Hope — Pré-refinamento da Visão Geral do Jogo v1.0

> **Status:** pré-refinamento para discussão e evolução documental  
> **Origem:** leitura da visão geral consolidada atual  
> **Destino recomendado no repo:** `docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_VISAO_GERAL_JOGO_v1.0.md`  
> **Objetivo:** transformar a visão geral em um documento mais forte para produto, lore, sistemas, regras, escopo e futuras specs.  
> **Não é spec implementável.** Antes de virar tarefa para Codex/Claude Code, cada bloco deve ser quebrado em spec própria com escopo, arquivos permitidos, critérios de aceite e validação.

---

## 1. Diagnóstico do documento atual

O documento atual cumpre bem a função de **mapa geral do jogo**. Ele consolida nome, gênero, mundo, pilares, MVP, sistemas principais, arquitetura, pipeline de arte e backlog macro.

O ponto a melhorar é que ele ainda está mais próximo de uma **síntese** do que de uma **visão refinada de produto**. Algumas seções dizem o que existe, mas ainda não deixam suficientemente claro:

- qual é a promessa emocional do jogo;
- qual é a fantasia central do jogador;
- como cada sistema se conecta com os demais;
- quais regras são definitivas e quais são hipóteses;
- onde a lore vira mecânica;
- onde a mecânica reforça a lore;
- quais decisões ainda precisam virar refinamentos próprios;
- quais partes são MVP, V2 e FULL;
- quais features são obrigatórias para a identidade do jogo e quais são expansões.

A recomendação é evoluir o documento em duas camadas:

```text
1. Documento-mãe de visão geral
   → curto, estável, lido para entender o jogo.

2. Refinamentos por domínio
   → documentos específicos para lore, farm, cidade, caverna, combate, companions, economia, UI/UX e progressão.
```

---

## 2. Decisão de organização documental

Não devemos colocar toda a lore, todos os sistemas e todas as regras dentro de um único arquivo carregado por agentes de código.

A estrutura recomendada é:

```text
docs/design/
  VISAO_GERAL_CINDARS_HOPE.md
  LORE_WORLD_BIBLE.md
  GAMEPLAY_PILLARS.md
  NARRATIVE_TONE_AND_RULES.md

docs/refinements/a_implementar/pre_refinamentos/
  PRE_REFINAMENTO_VISAO_GERAL_JOGO_v1.0.md
  PRE_REFINAMENTO_LORE_ANYA_LUAS_DEUSES.md
  PRE_REFINAMENTO_FARM_FULL.md
  PRE_REFINAMENTO_CIDADE_NPCS_REPUTACAO.md
  PRE_REFINAMENTO_CAVERNA_100_NIVEIS.md
  PRE_REFINAMENTO_COMPANIONS_JOBS_MORTE.md
  PRE_REFINAMENTO_PROGRESSAO_SKILL_TREE.md
  PRE_REFINAMENTO_UI_UX_FULL_GAMEPLAY.md
```

O documento de visão geral deve continuar enxuto, mas cada domínio precisa de um refinamento próprio para não virar um arquivo gigante e pouco útil.

---

# PARTE A — Refinamento do documento-mãe

## 3. Identidade do jogo

### Versão atual

Cindar's Hope é um jogo single-player 2D pixel art que mistura farm sim, RPG de progressão e dungeon crawler procedural.

### Refinamento recomendado

**Cindar's Hope** é um farm sim RPG de fantasia leve em pixel art, ambientado em Vaalara, onde o jogador transforma uma pequena fazenda nos arredores de uma cidade em uma base de produção, comércio, crafting, vínculos sociais e preparação para explorar uma caverna procedural perigosa.

A identidade do jogo nasce da tensão entre dois ritmos:

1. **Rotina cozy:** plantar, colher, pescar, vender, conversar, dormir, expandir e cuidar do espaço.
2. **Risco de aventura:** entrar na caverna, enfrentar criaturas, perder recursos, encontrar relíquias, alcançar checkpoints e descobrir vestígios de uma verdade antiga ligada a Anya.

A fazenda não é só decoração. Ela é a infraestrutura que sustenta a aventura. A caverna não é só combate. Ela é a origem dos recursos raros que transformam a fazenda, os workshops, a cidade e a progressão do jogador.

### Regra de identidade

Toda feature nova deve responder pelo menos uma destas perguntas:

- fortalece a fazenda como base de produção?
- fortalece a cidade como hub social/econômico?
- fortalece a caverna como risco/recompensa?
- fortalece a progressão do jogador?
- fortalece a presença de Vaalara como mundo próprio?
- cria uma decisão interessante para o jogador?

Se não responder, a feature deve ser adiada ou removida.

---

## 4. Promessa do jogo

### Refinamento necessário

O documento atual descreve sistemas, mas ainda falta uma promessa explícita.

### Proposta

A promessa de Cindar's Hope é:

> Construir uma vida produtiva em um mundo de fantasia leve, usando a fazenda, a cidade e os companions como base para enfrentar uma caverna cada vez mais perigosa, enquanto pequenos sinais revelam que a esperança morta de Vaalara talvez ainda deixe ecos no mundo.

Essa promessa deve guiar arte, gameplay, narrativa e escopo.

### Implicações práticas

- O jogador deve sempre ter algo útil para fazer em 5–10 minutos.
- O jogo deve alternar segurança e risco.
- A fazenda deve dar sensação de controle e crescimento.
- A caverna deve dar sensação de incerteza e recompensa.
- A cidade deve dar sensação de pertencimento e humor.
- A lore deve aparecer por pistas, NPCs, itens, lugares e eventos; não por paredes longas de texto.

---

## 5. Pilares refinados

## 5.1 Pilar 1 — Fazenda como base viva

A fazenda é o espaço mais seguro do jogador. Ela começa simples e funcional, mas deve evoluir para uma base produtiva com múltiplas fontes de recurso.

### Mecanismos centrais

- canteiros;
- plantio;
- colheita;
- árvores;
- pesca;
- animais;
- fertilizante;
- mineração inicial;
- workshops;
- estufa;
- expansão de terreno;
- alocação de companions.

### Regras de design

- Toda produção deve ter uso claro: venda, comida, crafting, quest, upgrade ou preparação para caverna.
- O jogador não deve ser punido por não otimizar perfeitamente a fazenda.
- A automação por companions deve reduzir repetição, não eliminar a participação do jogador.
- A fazenda deve ser legível visualmente: canteiro vazio, crescendo, pronto, irrigado, fertilizado e bloqueado precisam ser distinguíveis.

### Lacunas atuais

- Falta definir irrigação.
- Falta definir qualidade de colheita.
- Falta definir se planta morre fora da estação.
- Falta definir se há stamina separada de fome.
- Falta definir animais além de vacas.
- Falta definir como fertilizante altera rendimento ou tempo.

---

## 5.2 Pilar 2 — Cidade como hub social e econômico

A cidade Cindar's Hope não deve ser apenas uma tela de lojas. Ela deve ser o lugar que reage ao progresso do jogador.

### Funções da cidade

- vender produção;
- comprar sementes;
- comprar materiais;
- comprar animais;
- comprar blueprints;
- aceitar quests;
- receber rumores;
- recrutar companions;
- acessar templo e estalagem;
- desbloquear upgrades de fazenda;
- revelar lore em pequenas doses.

### Regras de design

- Cada NPC precisa ter função mecânica, tom de voz e pequena conexão com o mundo.
- Diálogos devem ser curtos e contextuais.
- Reputação deve liberar vantagens práticas, não só texto.
- O humor deve vir de personalidade, burocracia, comércio, medo da caverna e pequenas contradições do cotidiano.

### Lacunas atuais

- Falta definir rotina diária dos NPCs.
- Falta definir tabela de reputação detalhada.
- Falta definir primeira leva de quests.
- Falta definir como cada NPC reage às luas.
- Falta definir quais NPCs são recrutáveis no MVP/V2/FULL.

---

## 5.3 Pilar 3 — Caverna como risco, recompensa e mistério

A caverna é o motor de progressão rara. Ela deve fornecer materiais, inimigos, checkpoints, mercadores, biomas e pistas de lore.

### Mecanismos centrais

- geração procedural;
- 100 níveis;
- biomas;
- inimigos;
- loot;
- recursos raros;
- checkpoints;
- bosses;
- mercadores da Guilda das Estradas;
- Pedra de Retorno;
- influência de luas, seasons e dia/noite;
- objetivo final no nível 100.

### Regras de design

- Cada ida à caverna deve ter risco real de perda.
- Cada avanço de checkpoint deve parecer uma conquista.
- Recursos da caverna devem alimentar upgrades da fazenda, crafting e equipamentos.
- A caverna deve ficar mais estranha conforme o jogador desce.
- A lore de Anya deve aparecer mais fortemente em profundidades específicas, sem explicar tudo cedo demais.

### Lacunas atuais

- Falta definir biomas e faixas de níveis finais.
- Falta definir bosses.
- Falta definir o que existe no nível 100.
- Falta definir como checkpoints são ativados.
- Falta definir se a caverna tem salas especiais.
- Falta definir como o jogador recupera corpo/itens após morte, se essa ideia continuar.

---

## 5.4 Pilar 4 — Companions como multiplicadores de capacidade

Companions não são só pets ou NPCs cosméticos. Eles devem permitir ao jogador especializar a rotina.

### Funções

- morar na fazenda;
- executar jobs;
- acompanhar na caverna;
- ter atributos;
- poder morrer;
- ser ressuscitado;
- eventualmente ter quests próprias.

### Regras de design

- Companion deve ampliar possibilidades, não jogar sozinho pelo player.
- Apenas 1 companion ativo na caverna mantém decisão estratégica.
- Jobs devem ter tradeoffs.
- Morte de companion deve ter peso, mas não ser traumática ou irreversível no escopo atual.
- Fonte da Fazenda / Fonte de Anya deve ser o ponto de ressurreição, conectando mecânica e lore.

### Lacunas atuais

- Falta definir custo de ressurreição.
- Falta definir se companion tem inventário.
- Falta definir afinidade.
- Falta definir limite de eficiência por job.
- Falta definir se companion pode falhar em tarefa.
- Falta definir como Explorador retorna da caverna.

---

## 5.5 Pilar 5 — Crafting como ponte entre rotina e aventura

Crafting deve conectar produção da fazenda, recursos da caverna e preparação para exploração.

### Workshops

- Forja;
- Alquimia;
- Carpintaria;
- Costura.

### Regras de design

- Crafting não acontece pela mochila.
- Workshops têm 3 níveis.
- Cada nível deve liberar novas receitas e melhorar a utilidade da fazenda/caverna.
- Receitas devem ter papel claro: utilidade, progressão, economia, combate ou exploração.
- Blueprints devem criar motivos para explorar e interagir com NPCs.

### Lacunas atuais

- Falta matriz completa de receitas.
- Falta custo de construção dos workshops.
- Falta custo de upgrade.
- Falta definir tempo de crafting.
- Falta definir se crafting é instantâneo no V2 ou sempre tem fila.
- Falta definir papel do companion Artesão.

---

## 6. Lore refinada

## 6.1 Vaalara

Vaalara deve aparecer como mundo vivo, mas a primeira versão jogável deve focar em Dornecia e Cindar's Hope.

### Regra

Não abrir o mundo inteiro cedo demais. O jogo deve sugerir que Vaalara é maior, mas o recorte jogável inicial precisa permanecer claro.

### Funções de Vaalara no jogo

- justificar raças diversas;
- sustentar panteão;
- explicar luas;
- dar nomes próprios a itens, sementes e materiais;
- conectar Cindar's Hope a um mundo maior;
- permitir expansão futura sem quebrar o escopo inicial.

## 6.2 Dornecia

Dornecia precisa ganhar identidade própria.

### Falta definir

- clima predominante;
- economia regional;
- relação com outras regiões;
- por que Cindar's Hope existe onde existe;
- se a caverna é conhecida, amaldiçoada, explorada ou evitada;
- quem governa ou regula a cidade;
- como a religião aparece no cotidiano.

### Proposta inicial

Dornecia pode ser uma região de fronteira agrícola-comercial, com terras férteis, estradas ativas e exploração mineral instável. Cindar's Hope cresce porque fica perto de três coisas valiosas:

1. terras cultiváveis;
2. rota de comércio;
3. caverna com recursos raros.

Essa combinação explica cidade, fazenda, mercadores e perigo.

## 6.3 Cindar's Hope

A cidade precisa de uma razão simbólica para o nome.

### Perguntas pendentes

- Quem foi Cindar?
- Cindar é pessoa, família, lugar, santo, fundador ou evento?
- Por que “Hope”?
- A esperança está ligada a Anya?
- A cidade nasceu depois da morte de Anya ou antes?

### Proposta de direção

Cindar pode ter sido um fundador, explorador, curandeiro ou sobrevivente associado à primeira tentativa de transformar a região em assentamento seguro. “Hope” pode ser o nome dado após um evento de quase abandono, quando a cidade sobreviveu por pouco graças a um milagre, colheita, fonte ou relíquia ligada a Anya.

Essa decisão merece refinamento próprio.

---

## 7. Anya, luas e deuses

## 7.1 Anya

Anya é o maior gancho de lore atual. O documento já diz que ela está morta, mas ainda falta definir como isso afeta o jogo.

### Regras sugeridas

- Anya não aparece diretamente como NPC comum.
- Anya não deve resolver problemas pelo jogador.
- A presença dela deve ser sentida por vestígios.
- Cura, renascimento e ressurreição devem carregar custo ou ambiguidade.
- A Fonte da Fazenda pode ser um vestígio de Anya.
- Itens de cura podem conter fragmentos de esperança, não “poder divino pleno”.

### Manifestações possíveis

- ruínas na caverna;
- sonhos curtos ao dormir;
- flores raras próximas à Fonte;
- falas do Padre Corvus;
- nomes de poções;
- eventos em noites de Alihana;
- fragmentos de texto em itens antigos;
- eco visual em momentos de ressurreição de companion.

### Lacunas

- Anya morreu como?
- Quem sabe disso?
- A cidade sabe ou só religiosos sabem?
- O jogador pode descobrir a verdade?
- A morte de Anya tem ligação com o nível 100?
- A Fonte é dela, de um templo antigo ou de outra força?

## 7.2 Luas

As luas já têm bons efeitos, mas precisam de regras operacionais.

### Refinamento sugerido

| Lua | Gameplay | Narrativa | Risco |
|---|---|---|---|
| Alihana | bônus a sementes noturnas, mercadores raros | sonhos, profecias, música, ecos de Anya | informações ambíguas |
| Senya | magia mais forte, caos, preços maiores | festas, impulsos, instabilidade social | inimigos caóticos |
| Nyx | criaturas noturnas de dia, segredos, exaustão menor | lojas noturnas, itens secretos | perigo oculto |

### Regras

- A lua deve afetar mais de um sistema.
- O efeito deve ser previsível depois que o jogador aprende.
- O jogador deve conseguir planejar em torno da lua.
- A UI precisa mostrar a lua ativa de forma clara no V2/FULL.

## 7.3 Deuses

O panteão atual é rico, mas precisa ser usado com parcimônia.

### Regra

No jogo base, cada deus deve aparecer primeiro como:

- nome em item;
- frase de NPC;
- bênção;
- loja;
- templo;
- evento pequeno;
- símbolo visual.

Evitar despejar todo o panteão em exposição inicial.

---

# PARTE B — Refinamento por sistema

## 8. Fazenda — faltas e refinamento

### O que está bem definido

- plantio base;
- 10 sementes planejadas;
- árvores;
- pesca;
- fertilizante;
- expansão;
- estufa;
- vacas;
- integração com caverna.

### O que falta definir melhor

#### 8.1 Solo e irrigação

Decisão pendente: haverá rega manual?

Opções:

1. **Sem rega:** mais simples, mais cozy, menos repetição.
2. **Rega diária:** mais clássico, mas aumenta microgestão.
3. **Irrigação por upgrade:** MVP sem rega; V2/FULL introduz irrigação automatizada.

Recomendação: não incluir rega no MVP. Para FULL, usar irrigação como upgrade de fazenda/workshop, não como obrigação diária pesada.

#### 8.2 Qualidade de colheita

Falta definir se existe qualidade:

- comum;
- boa;
- excelente;
- lunar;
- rara.

Recomendação: adiar qualidade para V2/FULL. No MVP, item simples por ID.

#### 8.3 Fertilizante

Falta decidir se fertilizante:

- reduz tempo;
- aumenta yield;
- aumenta qualidade;
- altera chance de mutação;
- permite crescimento fora de condição.

Recomendação inicial:

```text
MVP: sem fertilizante.
V2: fertilizante aumenta yield.
FULL: fertilizantes especializados podem alterar qualidade, tempo ou mutações.
```

#### 8.4 Animais

Hoje só vacas estão citadas. Falta definir:

- onde comprar;
- custo;
- alimentação;
- produção diária;
- limite;
- se podem adoecer;
- se precisam de curral/pasto;
- relação com fertilizante.

Recomendação: vacas entram após o MVP, ligadas ao pasto e fertilizante.

---

## 9. Cidade — faltas e refinamento

### O que está bem definido

- lista inicial de NPCs;
- papéis funcionais;
- reputação 0–100;
- cidade como hub.

### O que falta

#### 9.1 Rotina dos NPCs

Cada NPC precisa de:

- horário de abertura;
- local principal;
- variação por dia/noite;
- reação por lua;
- diálogo contextual;
- função de loja/quest/serviço.

#### 9.2 Reputação

A reputação precisa de fontes e drenagens.

Fontes possíveis:

- vender itens localmente;
- completar quests;
- entregar recursos solicitados;
- salvar companion;
- derrotar boss;
- desbloquear checkpoint;
- doar item raro ao templo/biblioteca.

Perdas possíveis:

- abandonar quest;
- passar muitos dias sem interagir;
- vender itens suspeitos;
- falhar evento crítico;
- escolhas narrativas futuras.

#### 9.3 Quests iniciais

Sugestão de primeiras quests V2:

| Quest | NPC | Objetivo | Função |
|---|---|---|---|
| Primeira colheita | Sylveth | entregar Trigo | ensina plantio/venda |
| Madeira para reparo | Nimble | entregar madeira | ensina árvores |
| Peixe para a estalagem | Gruta | entregar Peixe Comum | ensina pesca |
| Pedra de Retorno | Zrix | comprar/testar item | introduz caverna |
| O nome de Anya | Padre Corvus | encontrar eco/relíquia | introduz lore |

---

## 10. Caverna — faltas e refinamento

### O que está bem definido

- 100 níveis;
- procedural;
- checkpoints a cada 10;
- recursos renováveis;
- biomas;
- mercadores;
- influência de tempo/luas;
- combate;
- companion ativo.

### O que falta

#### 10.1 Estrutura dos 100 níveis

Sugestão inicial:

| Faixa | Bioma | Função |
|---|---|---|
| 1–10 | Gruta Baixa | onboarding, pedra, cobre, slime |
| 11–25 | Galerias Fúngicas | veneno, cogumelos, recursos alquímicos |
| 26–40 | Cavernas Geladas | frio, gelo, Flor-de-Gelo |
| 41–55 | Veios de Fogo | calor, minério raro, Girassol de Fogo |
| 56–70 | Ruínas Subterrâneas | lore, constructs, fragmentos antigos |
| 71–85 | Abismo de Nyx | segredos, inimigos noturnos, baixa visibilidade |
| 86–99 | Santuário Partido | ecos de Anya, bosses fortes, recursos finais |
| 100 | Núcleo indefinido | decisão narrativa central |

Essa tabela é proposta, não decisão fechada.

#### 10.2 Checkpoints

Falta decidir:

- ativação automática ou manual;
- custo de ativação;
- se checkpoint permite teleport;
- se checkpoint salva progresso;
- se checkpoint fica em sala segura;
- se checkpoint tem ligação com Anya ou com a Guilda das Estradas.

Recomendação: checkpoints devem ser permanentes, ativados ao alcançar sala segura a cada 10 níveis.

#### 10.3 Nível 100

Não decidir agora em definitivo, mas criar hipóteses:

1. Templo partido de Anya.
2. Fonte original da esperança.
3. Prisão de algo que causou a morte de Anya.
4. Núcleo mineral vivo que sustenta Cindar's Hope.
5. Escolha final entre restaurar esperança, poder ou estabilidade.

---

## 11. Combate — faltas e refinamento

### O que está definido

- tempo real;
- físico e mágico;
- inimigos com HP;
- loot;
- status;
- bosses.

### O que falta

- hitbox/hurtbox padrão;
- esquiva;
- bloqueio;
- stamina ou cooldown;
- magia e MP;
- escalonamento de dano;
- resistências;
- telegraph de ataques;
- como companions entram no combate;
- penalidade exata de morte;
- recuperação de corpo/itens, se houver.

### Regras sugeridas

- Combate deve ser simples de ler.
- Inimigos devem ter padrões claros.
- Status deve ser visível na UI.
- Magia deve ser útil, mas consumir recurso real.
- Morte deve doer, mas não travar progresso.

---

## 12. Companions — faltas e refinamento

### O que está definido

- vários companions na fazenda;
- 1 ativo na caverna;
- jobs;
- morte;
- ressurreição.

### O que falta

#### 12.1 Jobs

Cada job precisa de:

- input necessário;
- output esperado;
- frequência;
- risco;
- eficiência;
- limitação;
- interação com atributo/afinidade.

Exemplo:

| Job | Input | Output | Risco | Observação |
|---|---|---|---|
| Plantador | sementes disponíveis | canteiros plantados | baixo | não escolhe estratégia complexa |
| Colhedor | plantas prontas | itens no baú/inventário | baixo | pode deixar popup/log |
| Pescador | cana + lago | peixe | baixo | eficiência por tempo |
| Lenhador | árvores ativas | madeira | baixo | respeita limite de corte |
| Minerador | área de rocha | pedra/cobre | médio | pode cansar/retornar menos |
| Artesão | receita + ingredientes | item craftado | baixo | usa fila |
| Explorador | permissão + nível | recursos da caverna | alto | pode voltar ferido ou falhar |

#### 12.2 Morte e ressurreição

A Fonte da Fazenda / Fonte de Anya deve ser refinada.

Perguntas:

- Ressurreição custa ouro?
- Custa item raro?
- Tem cooldown?
- Afeta afinidade?
- Gera evento narrativo?
- O companion lembra da morte?

---

## 13. Progressão — faltas e refinamento

### O que está definido

- XP;
- level up;
- atributos;
- skill tree em 3 ramos;
- progressão de fazenda/caverna.

### O que falta

#### 13.1 Fontes de XP

Proposta:

| Fonte | XP |
|---|---|
| Colher crop | baixo e constante |
| Pescar | baixo/médio |
| Cortar árvore | baixo/médio |
| Craftar item novo | médio |
| Completar quest | médio/alto |
| Derrotar inimigo | variável |
| Chegar a checkpoint | alto |
| Derrotar boss | muito alto |

#### 13.2 Skill points

Decisão já refinada em conversas anteriores: SkillPoint a cada 2 níveis, 4 active slots, capstones e respec na Fonte de Anya. Isso deve ser integrado ao documento de progressão, se ainda não estiver no repo.

#### 13.3 Regras

- Progressão não deve obrigar combate para jogador que prefere farm, mas a caverna deve acelerar acesso a recursos raros.
- Produção, combate físico e magia devem ser caminhos viáveis.
- Respec deve existir para reduzir medo de experimentar.

---

## 14. UI/UX — faltas e refinamento

O documento atual cita HUD e inventário, mas não define experiência de uso completa.

### Telas necessárias

- HUD principal;
- inventário;
- hotbar;
- equipamentos;
- skill tree;
- crafting;
- loja;
- diálogo;
- quests/journal;
- mapa/minimapa;
- morte/respawn;
- save/load;
- entrada da caverna;
- companion management.

### Regras de UX

- Toda ação principal deve ter feedback imediato.
- Todo item deve ter nome, ícone, categoria, descrição e uso claro.
- Menus devem ser navegáveis por teclado/controle no futuro.
- UI do MVP pode ser textual, mas não deve bloquear arquitetura futura de UI visual.
- A hotbar deve ser o ponto principal de ação rápida.

---

# PARTE C — Regras e mecanismos transversais

## 15. Regra de escopo MVP/V2/FULL

Qualquer sistema deve declarar a camada:

```text
MVP  = mínimo funcional, placeholder aceito, UI textual aceita.
V2   = primeira versão jogável completa, com UX melhor e integração básica.
FULL = versão final planejada, com polish, balanceamento, arte final e integração profunda.
```

Regra: se uma feature não é necessária para fechar o loop `plantar → colher → vender → salvar → carregar`, ela não entra no MVP Fazenda.

---

## 16. Regra de lore jogável

Lore só deve entrar no jogo quando tiver pelo menos uma destas formas:

- lugar;
- item;
- NPC;
- diálogo curto;
- quest;
- mecânica;
- visual;
- evento;
- consequência.

Evitar lore puramente enciclopédica dentro do jogo. A world bible pode ser extensa, mas o jogador deve descobrir o mundo por uso, ambiente e interação.

---

## 17. Regra de sistemas conectados

Todo sistema novo deve declarar:

- input;
- output;
- eventos publicados;
- eventos consumidos;
- dados ScriptableObject necessários;
- campos salvos;
- UI/feedback necessário;
- teste manual mínimo;
- impacto em balanceamento;
- camada MVP/V2/FULL.

---

## 18. Regra de save

Nenhuma decisão de gameplay deve ser considerada fechada sem responder:

- precisa persistir?
- qual ID salva?
- qual DTO salva?
- como carrega?
- o que acontece se dado antigo não existir mais?
- precisa de SchemaVersion/migração?

---

## 19. Regra de arte

Todo asset visual relevante deve declarar:

- tamanho;
- pasta;
- nome;
- paleta;
- uso in-game;
- estado visual;
- se é placeholder, V2 ou final;
- import settings no Unity;
- referência por ScriptableObject ou prefab.

---

# PARTE D — Próximos refinamentos recomendados

## 20. Ordem sugerida

### 20.1 Refinamento 1 — Documento-mãe final

Criar/atualizar:

```text
docs/design/VISAO_GERAL_CINDARS_HOPE.md
```

Conteúdo:

- promessa do jogo;
- pilares;
- loop;
- escopo macro;
- regras de identidade;
- resumo de lore;
- links para refinamentos específicos.

### 20.2 Refinamento 2 — Lore jogável

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_LORE_ANYA_LUAS_DEUSES_v1.0.md
```

Foco:

- Cindar;
- origem do nome Cindar's Hope;
- Anya;
- Fonte;
- três luas;
- como lore vira gameplay.

### 20.3 Refinamento 3 — Farm full

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_FARM_FULL_v1.0.md
```

Foco:

- irrigação;
- fertilizante;
- qualidade;
- animais;
- estufa;
- expansão;
- automação.

### 20.4 Refinamento 4 — Cidade/NPCs/Reputação

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_CIDADE_NPCS_REPUTACAO_v1.0.md
```

Foco:

- rotina dos NPCs;
- quests;
- lojas;
- reputação;
- recrutamento;
- diálogos contextuais.

### 20.5 Refinamento 5 — Caverna 100 níveis

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_CAVERNA_100_NIVEIS_v1.0.md
```

Foco:

- biomas;
- bosses;
- checkpoints;
- recursos;
- mercadores;
- morte;
- nível 100.

### 20.6 Refinamento 6 — Progressão e skill tree

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_PROGRESSAO_SKILL_TREE_v1.0.md
```

Foco:

- XP;
- level;
- atributos;
- skill points;
- active slots;
- capstones;
- respec.

### 20.7 Refinamento 7 — UI/UX full gameplay

Criar:

```text
docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_UI_UX_FULL_GAMEPLAY_v1.0.md
```

Foco:

- HUD;
- inventário;
- equipamentos;
- hotbar;
- skill tree;
- crafting;
- lojas;
- entrada da caverna;
- morte;
- companion management.

---

## 21. Critério para promover este pré-refinamento

Este documento pode ser promovido para refinamentos específicos quando:

- as decisões sobre Cindar/Anya/Fonte forem separadas em lore;
- as regras de farm forem quebradas em documento próprio;
- a cidade tiver matriz de NPCs/lojas/rotinas;
- a caverna tiver pelo menos primeira proposta de biomas e checkpoints;
- progressão/skill tree estiver sincronizada com specs recentes;
- UI/UX tiver mapa de telas e fluxo.

---

## 22. Próxima ação recomendada

A próxima ação mais útil é refinar **Lore jogável: Cindar, Anya, Fonte, luas e deuses**, porque isso afeta:

- nome do jogo;
- sentido da cidade;
- Fonte de Anya;
- morte/ressurreição;
- caverna profunda;
- quests;
- tom narrativo;
- identidade visual e símbolos.

Depois disso, refinar Farm Full e Cidade/NPCs.
