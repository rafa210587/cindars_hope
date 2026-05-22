# Cindar's Hope — Fase 6: Backlog de Histórias v1.2
# Ã‰pico: FARM (Fazenda)

> **Fase 6 FARM — v1.1. User stories preservadas; status alinhado com Fase 7 já gerada e Fase 8 como próxima execução.*
> **Status:** ✅ FARM detalhado — specs MVP já geradas na Fase 7
> **Ãšltima atualização:** 2026-05-16
> **Próxima ação:** Implementar Fase 8 usando `FASE7_SPEC_MVP_FARM_v2.2.md`

---

## NOTA DE SINCRONIZAÃ‡ÃƒO v1.1

Este backlog continua válido como fonte das user stories FARM. A etapa de geração de specs já ocorreu em `FASE7_SPEC_MVP_FARM_v2.2.md`; portanto, este arquivo não deve mais dizer que aguarda revisão para gerar specs.

## HANDOFF

### Plano macro
| # | Fase | Status |
|---|---|---|
| 1–5 | Ideação → Ambiente | ✅ Concluídas / ambiente a validar localmente |
| 6 | **Quebra de Histórias FARM** | ✅ Este documento |
| 7 | Spec Kit MVP Fazenda | ✅ Specs geradas |
| 8 | MVP — Core Loop Fazenda | ðŸ”„ Próxima execução |
| 9–13 | Execução completa → Launch | â³ Pendente |

### Convenção de prioridade (MoSCoW)
- **M** Must Have — MVP não funciona sem isso
- **S** Should Have — importante, mas MVP sobrevive sem
- **C** Could Have — desejável, entra depois do MVP
- **W** Won't Have (agora) — deferido para versão futura

### Convenção de evolução
Cada story tem um label de camada:
- **[MVP]** — versão mínima funcional (placeholder, sem UI polish)
- **[V2]** — primeira evolução pós-MVP
- **[FULL]** — versão completa conforme GDD

---

## Ã‰PICO FARM-000 — Setup Inicial da Fazenda

### FARM-001 Â· Cena da Fazenda existe e o jogador aparece nela
**Prioridade:** M — Must Have
**Camada:** MVP

Como jogador,
quero abrir o jogo e ver meu personagem na fazenda
para que eu possa começar a interagir com o mundo.

**Critérios de aceite:**
- Dado que o jogo é iniciado
- Quando a FarmScene carrega
- Então o personagem jogador aparece no centro da tela
- E o chão da fazenda está visível (placeholder: tiles cinza-claro)
- E a câmera segue o personagem ao se mover
- E o personagem responde Ã s teclas WASD / setas

**Notas técnicas:**
- Placeholder: sprite azul 32x48 para o jogador
- Tilemap de chão: tiles cinza-claro (`#C8A464` no futuro)
- Cinemachine 2D com follow suave

---

### FARM-002 Â· Jogador se move pela fazenda
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero mover meu personagem pela fazenda com WASD
para que eu possa me locomover entre as áreas.

**Critérios de aceite:**
- Dado que estou na FarmScene
- Quando pressiono WASD ou setas direcionais
- Então o personagem se move na direção pressionada com velocidade base
- E colide com as bordas do mapa (não sai da fazenda)
- E [MVP] sem animação de walk — sprite estático se move
- E [V2] sprite muda de direção (flip horizontal esquerda/direita)
- E [FULL] animação de walk com 6 frames nas 4 direções

---

### FARM-003 Â· Fazenda tem áreas delimitadas
**Prioridade:** M
**Camada:** MVP → V2

Como jogador,
quero ver a fazenda dividida em áreas reconhecíveis
para que eu saiba onde plantar, onde ficam as árvores e onde fica o lago.

**Critérios de aceite (MVP):**
- Dado que a FarmScene carrega
- Quando olho para o mapa
- Então existe uma área de canteiros (tiles diferentes de chão)
- E existe uma borda de mapa (tiles de parede/limite)
- E [MVP] tudo em placeholder: diferentes tons de cinza para cada área

**Evolução:**
- [V2] Tiles visuais distintos: terra para canteiros, grama para área livre
- [FULL] Arte completa com paleta laranja/bronze da fazenda

---

## Ã‰PICO FARM-010 — Sistema de Plantio

### FARM-011 Â· Jogador pode selecionar um canteiro vazio
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero clicar ou interagir com um canteiro vazio
para que eu possa plantar uma semente nele.

**Critérios de aceite:**
- Dado que estou próximo de um canteiro vazio
- Quando pressiono E (tecla de interação)
- Então o canteiro é selecionado (highlight visual)
- E [MVP] abre menu de texto com sementes disponíveis no inventário
- E [V2] abre UI de grade com ícones de sementes

---

### FARM-012 Â· Jogador planta uma semente no canteiro
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero escolher uma semente do meu inventário e plantá-la
para que ela comece a crescer.

**Critérios de aceite:**
- Dado que selecionei um canteiro e tenho sementes no inventário
- Quando escolho uma semente no menu
- Então a semente é removida do inventário
- E o canteiro muda para o sprite do estágio 0 da planta
- E o dado de crescimento é iniciado (dia atual salvo no canteiro)
- E [MVP] sprite estágio 0: cubo verde-escuro 32x32
- E [FULL] sprite correto do SeedDataSO.GrowthStageSprites[0]

---

### FARM-013 Â· Planta cresce com o passar dos dias
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero que minha planta cresça automaticamente com o passar dos dias
para que eu possa colhê-la quando estiver pronta.

**Critérios de aceite:**
- Dado que uma semente está plantada
- Quando o evento DayStartedEvent é publicado
- Então o contador de dias da planta avança 1
- E quando o contador atinge SeedDataSO.GrowthDays
- Então o sprite muda para o estágio final (planta pronta)
- E [MVP] apenas 2 estágios: não-pronta e pronta
- E [V2] estágios intermediários conforme GrowthStageSprites
- E [MVP] só plantas diurnas/neutras crescem (sem verificação de período ainda)
- E [FULL] verifica período (dia/noite) e season antes de crescer

**Notas técnicas:**
- Ouve DayStartedEvent — não usa Update()
- Dado persistido no FarmSaveData (mesmo no MVP simples)

---

### FARM-014 Â· Jogador colhe a planta pronta
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero interagir com uma planta pronta para colhê-la
para que os itens vão para o meu inventário.

**Critérios de aceite:**
- Dado que uma planta está no estágio final (pronta)
- Quando pressiono E próximo a ela
- Então os itens de colheita vão para o inventário (MinYield a MaxYield)
- E o canteiro volta ao estado vazio
- E o evento CropHarvestedEvent é publicado
- E [MVP] yield fixo (MinYield) sem fertilizante
- E [FULL] yield aleatório entre Min e Max, fertilizante aplica multiplicador

---

### FARM-015 Â· Jogador pode ver o estado de todos os canteiros
**Prioridade:** S
**Camada:** V2

Como jogador,
quero ver de relance o estado de cada canteiro (vazio, crescendo, pronto)
para que eu saiba o que precisa de atenção.

**Critérios de aceite:**
- Dado que estou na FarmScene
- Quando olho para os canteiros
- Então canteiros prontos têm um indicador visual diferente (brilho, ícone)
- E canteiros crescendo mostram o sprite do estágio atual
- E canteiros vazios estão claramente distinguíveis

---

### FARM-016 Â· Sementes têm período de crescimento (dia/noite)
**Prioridade:** S
**Camada:** V2

Como jogador,
quero que sementes noturnas só cresçam Ã  noite
para que eu precise pensar sobre quando plantar.

**Critérios de aceite:**
- Dado que uma semente noturna está plantada
- Quando o DayStartedEvent dispara durante o dia
- Então o contador desta planta NÃƒO avança
- Quando o NightStartedEvent dispara
- Então o contador avança 1
- E [FULL] durante lua Alihana, sementes noturnas têm yield +20%

---

### FARM-017 Â· Sementes têm season válida
**Prioridade:** C
**Camada:** FULL

Como jogador,
quero que sementes fora da season correta não cresçam
para que eu precise planejar meu plantio por estação.

**Critérios de aceite:**
- Dado que uma semente tem Season[] ValidSeasons definido
- Quando a season atual não está na lista
- Então a planta não avança crescimento
- E uma UI indica "fora de estação" no canteiro
- E durante Inverno, plantas em solo descoberto morrem
- E a Estufa permite plantar qualquer semente em qualquer season

---

## Ã‰PICO FARM-020 — Inventário

### FARM-021 Â· Jogador tem inventário básico
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero ter um inventário para guardar sementes e itens colhidos
para que eu possa usá-los nas próximas ações.

**Critérios de aceite:**
- Dado que o jogo inicia
- Então o jogador começa com 5 sementes de Trigo e 3 de Cenoura (MVP hardcodado)
- E o inventário suporta até 20 slots distintos
- Quando colho uma planta
- Então os itens aparecem no inventário
- E [MVP] inventário acessível por tecla I → lista de texto (Item: Qtd)
- E [V2] grade visual com ícones 32x32
- E [FULL] hotbar de acesso rápido + grade completa

---

### FARM-022 Â· Itens se acumulam (stack)
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero que itens do mesmo tipo se acumulem num único slot
para que meu inventário não fique cheio rapidamente.

**Critérios de aceite:**
- Dado que tenho 5 Trigos no inventário
- Quando colho mais 3 Trigos
- Então o slot de Trigo mostra 8
- E só cria novo slot quando o stack está cheio (ItemDataSO.MaxStack)

---

### FARM-023 Â· Jogador usa item do inventário
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero selecionar um item do inventário e usá-lo
para que eu possa plantar sementes ou consumir comida.

**Critérios de aceite:**
- Dado que abri o inventário (tecla I)
- Quando seleciono uma semente e pressiono "usar"
- Então o jogo entra no modo de plantio (selecionar canteiro)
- Quando seleciono comida e pressiono "usar"
- Então a barra de fome é restaurada e o item é consumido

---

## Ã‰PICO FARM-030 — Sistema de Tempo (MVP Simplificado)

### FARM-031 Â· Dia avança com botão (MVP) ou automaticamente (V2+)
**Prioridade:** M
**Camada:** MVP → V2

Como jogador,
quero que os dias passem para que minhas plantas cresçam
para que eu possa progredir no jogo.

**Critérios de aceite (MVP):**
- Dado que estou na FarmScene
- Quando pressiono TAB (ou botão "Dormir")
- Então o dia avança 1
- E o DayStartedEvent é publicado
- E todas as plantas avançam crescimento
- E [MVP] sem ciclo visual de dia/noite — só o número do dia muda na UI

**Critérios de aceite (V2):**
- Dado que o jogo está rodando
- Quando 20 minutos reais passam (configurável no GameConfigSO)
- Então o dia avança automaticamente
- E o visual da luz muda conforme a hora (TimeVisualSystem)

**Critérios de aceite (FULL):**
- Ciclo completo: hora, dia, mês, lua, season
- Plantas crescem na hora correta (dia vs noite)
- NPCs têm horário de funcionamento

---

### FARM-032 Â· UI mostra dia atual e hora
**Prioridade:** S
**Camada:** MVP (simplificado)

Como jogador,
quero ver o dia e hora atuais na tela
para que eu possa planejar minhas ações.

**Critérios de aceite:**
- Dado que estou em qualquer cena
- Então um HUD no canto superior mostra "Dia X"
- E [MVP] só mostra o número do dia
- E [V2] mostra hora + ícone de sol/lua
- E [FULL] mostra dia, mês, lua ativa, season

---

## Ã‰PICO FARM-040 — Ãrvores

### FARM-041 Â· Fazenda começa com 4 árvores
**Prioridade:** S
**Camada:** MVP

Como jogador,
quero ver árvores na minha fazenda desde o início
para que eu possa coletar madeira.

**Critérios de aceite:**
- Dado que a FarmScene carrega pela primeira vez
- Então 4 árvores aparecem em posições fixas na fazenda
- E [MVP] sprite: retângulo verde 32x64 (tronco marrom + copa verde)
- E cada árvore começa no nível 4 de corte (cheia)

---

### FARM-042 Â· Jogador corta árvore e recebe madeira
**Prioridade:** S
**Camada:** MVP

Como jogador,
quero cortar uma árvore com minha ferramenta
para que eu receba madeira para craftar.

**Critérios de aceite:**
- Dado que estou próximo de uma árvore
- Quando pressiono E
- Então a árvore perde 1 nível de corte
- E o jogador recebe madeira (TreeDataSO.WoodPerChop[nivel])
- E [MVP] ferramenta básica: sempre 10 madeiras por corte
- E [FULL] quantidade depende da ferramenta equipada e do nível da árvore
- E quando a árvore atinge nível 0, o sprite desaparece
- E o TreeChoppedEvent é publicado

---

### FARM-043 Â· Ãrvores crescem novamente após 5 dias
**Prioridade:** S
**Camada:** V2

Como jogador,
quero que árvores cortadas cresçam novamente
para que minha fonte de madeira seja renovável.

**Critérios de aceite:**
- Dado que uma árvore foi completamente cortada (nível 0)
- Quando 5 dias in-game passam
- Então a árvore volta ao nível 4 (completa)
- E o sprite é restaurado

---

### FARM-044 Â· Jogador pode plantar novas árvores (até 14)
**Prioridade:** C
**Camada:** V2

Como jogador,
quero plantar mudas de árvore para aumentar minha produção de madeira
para que eu possa ter mais recursos.

**Critérios de aceite:**
- Dado que tenho uma muda de árvore no inventário
- Quando seleciono um espaço livre adequado e planto
- Então a árvore aparece no nível 1
- E cresce 1 nível a cada 5 dias até nível 4
- E o limite máximo é 14 árvores na fazenda

---

## Ã‰PICO FARM-050 — Pesca

### FARM-051 Â· Lago existe na fazenda
**Prioridade:** S
**Camada:** MVP

Como jogador,
quero ver um lago na minha fazenda
para que eu possa pescar nele.

**Critérios de aceite:**
- Dado que a FarmScene carrega
- Então uma área de lago é visível (tiles azuis)
- E o jogador não pode andar sobre os tiles de água
- E existe uma borda de lago onde o jogador pode pescar

---

### FARM-052 Â· Jogador pesca no lago
**Prioridade:** S
**Camada:** MVP

Como jogador,
quero pescar no lago da fazenda
para que eu obtenha peixe para comida e poções.

**Critérios de aceite:**
- Dado que estou na borda do lago com uma cana de pescar equipada
- Quando pressiono E
- Então começa uma mini-espera (3 segundos)
- E um peixe é adicionado ao inventário
- E o FishCaughtEvent é publicado
- E [MVP] sempre o mesmo peixe (Peixe Comum), sem variação
- E [V2] tipo de peixe varia por período (dia/noite) e season
- E [FULL] mini-game de timing para pegar o peixe

---

## Ã‰PICO FARM-060 — Fome

### FARM-061 Â· Jogador tem barra de fome
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero ter uma barra de fome que vai diminuindo
para que eu precise comer para manter meu personagem funcionando.

**Critérios de aceite:**
- Dado que o jogo inicia
- Então o jogador começa com fome = 100/100
- E a fome diminui 1 unidade a cada 10 passos (MVP simplificado)
- E [FULL] 1 unidade por passo fora de combate, 3 por passo em combate
- Quando a fome chega a 30
- Então a velocidade de movimento é reduzida em 30%
- Quando a fome chega a 0
- Então o jogador perde HP lentamente (1 HP por 5 segundos)
- E a UI mostra a barra de fome em vermelho

---

### FARM-062 Â· Jogador come para restaurar fome
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero comer itens do inventário para restaurar minha fome
para que eu consiga continuar jogando.

**Critérios de aceite:**
- Dado que tenho comida no inventário
- Quando seleciono o item e uso (tecla U ou menu de inventário)
- Então a fome é restaurada conforme o valor do alimento
- E o item é consumido do inventário
- E [MVP] Trigo restaura 20 de fome, Peixe Comum restaura 35

---

## Ã‰PICO FARM-070 — Save e Load

### FARM-071 Â· Jogo salva ao pressionar "Dormir" (MVP)
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero que o jogo salve quando durmo
para que eu não perca progresso ao fechar o jogo.

**Critérios de aceite:**
- Dado que pressiono TAB (botão Dormir no MVP)
- Então o SaveManager serializa o estado atual em JSON
- E o arquivo é salvo em Application.persistentDataPath/saves/slot_1.json
- E uma mensagem "Jogo salvo" aparece na tela por 2 segundos
- E [MVP] slot único (slot_1.json)
- E [V2] menu de seleção de 3 slots antes de salvar
- E [FULL] save automático também em mudança de cena

---

### FARM-072 Â· Jogo carrega save existente ao iniciar
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero que ao abrir o jogo meu progresso anterior seja restaurado
para que eu continue de onde parei.

**Critérios de aceite:**
- Dado que existe um arquivo de save em Application.persistentDataPath/saves/slot_1.json
- Quando o jogo inicia (BootScene)
- Então o SaveManager lê o JSON e restaura o estado
- E o jogador aparece na FarmScene com o inventário salvo
- E os canteiros estão no estado salvo (plantados/crescendo/prontos)
- E o número do dia é o salvo
- E [MVP] se não há save, começa novo jogo com estado padrão

---

### FARM-073 Â· Jogador pode escolher entre múltiplos slots
**Prioridade:** S
**Camada:** V2

Como jogador,
quero ter 3 slots de save independentes
para que eu possa ter partidas diferentes ou recomeçar sem perder a anterior.

**Critérios de aceite:**
- Dado que estou na tela inicial
- Quando seleciono "Continuar"
- Então vejo os 3 slots com: nome do slot, dia atual, tempo jogado
- Quando seleciono um slot vazio
- Então começa um novo jogo naquele slot
- Quando seleciono um slot com save
- Então carrega aquele progresso

---

## Ã‰PICO FARM-080 — Expansão da Fazenda

### FARM-081 Â· Fazenda começa com tamanho 1x
**Prioridade:** M
**Camada:** MVP

Como jogador,
quero começar com uma fazenda de tamanho base
para que eu tenha espaço inicial para plantar e crescer.

**Critérios de aceite:**
- Dado que é a primeira vez jogando
- Então a fazenda tem tamanho 1x (área base definida no tilemap)
- E há espaço para 9 canteiros, 4 árvores e o lago
- E há uma borda visível indicando o limite da fazenda

---

### FARM-082 Â· Jogador pode expandir a fazenda (1x → 2x)
**Prioridade:** C
**Camada:** V2

Como jogador,
quero expandir minha fazenda ao atingir os requisitos
para que eu tenha mais espaço para plantar e construir.

**Critérios de aceite:**
- Dado que sou nível 5, tenho 800 de ouro e 200 de madeira
- Quando interajo com o NPC construtor (Hund ou Gurd) na cidade
- Então a opção de expandir aparece
- Quando confirmo
- Então os recursos são descontados
- E o tilemap da fazenda expande para 2x o tamanho
- E novos canteiros e espaço de árvore ficam disponíveis
- E o FarmExpandedEvent é publicado

---

## Ã‰PICO FARM-090 — Fertilizante e Vacas

### FARM-091 Â· Fertilizante pode ser aplicado num canteiro
**Prioridade:** C
**Camada:** V2

Como jogador,
quero aplicar fertilizante em um canteiro antes de plantar
para que minha colheita tenha maior rendimento.

**Critérios de aceite:**
- Dado que tenho fertilizante no inventário
- Quando seleciono um canteiro vazio e uso o fertilizante
- Então o canteiro fica marcado como "fertilizado"
- E quando planto nele, o multiplicador FertilizerYieldMultiplier é aplicado
- E o visual do canteiro muda levemente (terra mais escura)

---

### FARM-092 Â· Vacas produzem fertilizante diariamente
**Prioridade:** C
**Camada:** V2

Como jogador,
quero ter vacas na minha fazenda que produzem fertilizante
para que eu tenha fonte renovável de fertilizante.

**Critérios de aceite:**
- Dado que comprei uma vaca na cidade e tenho expansão de pasto
- Quando o DayStartedEvent dispara
- Então cada vaca adiciona 1 fertilizante ao inventário da fazenda
- E o limite é 2 vacas sem pasto expandido, 6 com

---

## MAPA DE EVOLUÃ‡ÃƒO — FARM

```
MVP (Fase 8)
â”œâ”€â”€ FARM-001 Cena existe, personagem aparece
â”œâ”€â”€ FARM-002 Movimento WASD
â”œâ”€â”€ FARM-003 Ãreas delimitadas (placeholder)
â”œâ”€â”€ FARM-011 Selecionar canteiro
â”œâ”€â”€ FARM-012 Plantar semente
â”œâ”€â”€ FARM-013 Planta cresce (avança dia com TAB)
â”œâ”€â”€ FARM-014 Colher planta
â”œâ”€â”€ FARM-021 Inventário (lista de texto)
â”œâ”€â”€ FARM-022 Stacking de itens
â”œâ”€â”€ FARM-023 Usar item do inventário
â”œâ”€â”€ FARM-031 Dia avança com TAB
â”œâ”€â”€ FARM-032 UI: número do dia
â”œâ”€â”€ FARM-041 4 árvores na fazenda
â”œâ”€â”€ FARM-042 Cortar árvore = madeira
â”œâ”€â”€ FARM-051 Lago existe
â”œâ”€â”€ FARM-052 Pescar (básico, sem mini-game)
â”œâ”€â”€ FARM-061 Barra de fome
â”œâ”€â”€ FARM-062 Comer para restaurar fome
â”œâ”€â”€ FARM-071 Save ao dormir (slot único)
â””â”€â”€ FARM-072 Load ao iniciar

V2 (pós-MVP)
â”œâ”€â”€ FARM-002 Animação walk + flip de sprite
â”œâ”€â”€ FARM-015 Indicador visual de canteiros prontos
â”œâ”€â”€ FARM-016 Período dia/noite afeta crescimento
â”œâ”€â”€ FARM-031 Tempo automático (sem botão TAB)
â”œâ”€â”€ FARM-032 UI: hora + ícone sol/lua
â”œâ”€â”€ FARM-043 Ãrvores crescem de volta em 5 dias
â”œâ”€â”€ FARM-044 Plantar novas árvores (até 14)
â”œâ”€â”€ FARM-052 Variação de peixe por período/season
â”œâ”€â”€ FARM-073 3 slots de save
â”œâ”€â”€ FARM-082 Expansão fazenda 1x → 2x
â”œâ”€â”€ FARM-091 Fertilizante em canteiros
â””â”€â”€ FARM-092 Vacas produzem fertilizante

FULL (versão completa)
â”œâ”€â”€ FARM-002 Animação completa 4 direções
â”œâ”€â”€ FARM-013 Múltiplos estágios visuais de crescimento
â”œâ”€â”€ FARM-017 Sementes têm season válida
â”œâ”€â”€ FARM-031 Ciclo completo: hora, mês, lua, season
â”œâ”€â”€ FARM-052 Mini-game de pesca com timing
â”œâ”€â”€ FARM-061 Fome 3x mais rápida em combate
â”œâ”€â”€ FARM-071 Save automático por mudança de cena
â””â”€â”€ FARM-082 Expansões até 4x + sub-expansão pasto
```

---

## DECISÃ•ES TOMADAS (aprovadas pelo Rafa)

| Decisão | Valor |
|---|---|
| Canteiros iniciais | 9 (grade 3x3) |
| Sementes iniciais | Lidas de ScriptableObject (não hardcodadas) |
| Tecla de interação | E (para tudo: canteiro, árvore, pesca, NPC) |
| Tecla de inventário | I |
| Tecla de dormir/avançar dia | TAB |
| HP inicial (MVP) | 100 fixo; atributos chegam em CHAR V2 |
| Morte por fome | Respawn no mesmo dia (sem penalidade de itens no MVP) |
| Ouro inicial | Sim — começa com ouro fixo (valor a definir na spec) |
| Venda no MVP | Sim — NPC de venda simples ou menu de venda na fazenda |

*Stories aprovadas para geração de spec na Fase 7.*

---

*Fase 6 FARM — v1.1. User stories preservadas; status alinhado com Fase 7 já gerada e Fase 8 como próxima execução.*


---

## ENRIQUECIMENTO v1.2 — Critérios de qualidade para stories FARM

As stories estão aprovadas como descrição funcional. Para execução com Codex, cada story deve ser lida com os critérios abaixo.

### 1. Critério de rastreabilidade

Cada story precisa mapear para:

```text
Story → Spec → PR Codex → arquivos alterados → teste manual → commit
```

Exemplo:

```text
FARM-012 Jogador planta uma semente
→ SPEC FARM-016 Menu de Plantio + SPEC FARM-012 CropTile
→ PR-008 Canteiros e plantio
→ CropTile.cs, PlantingMenu.cs, SeedDataSO.cs
→ teste: abrir menu, plantar trigo, semente diminui, plot muda visual
→ commit: feat: implementar plantio básico em canteiros
```

### 2. Critério de MVP real

Para cada story com marcação MVP/V2/FULL:

- Implementar somente a linha MVP.
- Não criar animação final se o MVP aceita placeholder.
- Não implementar season se a story diz que season é FULL.
- Não implementar UI de grade se a story diz que lista texto resolve MVP.

### 3. Critério de dados

Toda story que cita item, semente, árvore, peixe ou preço precisa usar `ScriptableObject` e ID estável.

Exemplo:

```text
Item_Trigo.asset
  ItemId = item_crop_wheat
  DisplayName = Trigo
  BaseValue = 5
  HungerRestore = 20
```

### 4. Critério de save

Toda story que muda estado persistente precisa declarar o que entra no save.

| Story | Estado que deve persistir |
|---|---|
| FARM-012 Plantio | SeedId, DaysGrown, PlotState por plot |
| FARM-014 Colheita | Inventário, plot vazio |
| FARM-021 Inventário | ItemId + Amount por slot |
| FARM-031 Dia | CurrentDay |
| FARM-042 Ãrvore | ChopLevel por árvore |
| FARM-052 Pesca | Inventário após peixe |
| FARM-061 Fome | CurrentHunger |
| FARM-SELL Venda | Gold e inventário |

### 5. Critério de evento

Toda story que afeta outro sistema deve publicar evento. Toda story que apenas consulta estado não precisa publicar evento.

| Ação | Evento |
|---|---|
| Avançar dia | `DayStartedEvent` |
| Plantar semente | `SeedPlantedEvent` |
| Planta pronta | `CropReadyEvent` |
| Colher | `CropHarvestedEvent` |
| Cortar árvore | `TreeChoppedEvent` |
| Pescar | `FishCaughtEvent` |
| Mudar fome | `HungerChangedEvent` |
| Mudar ouro | `GoldChangedEvent` |
| Salvar | `GameSavedEvent` opcional |

### 6. Ajuste recomendado de prioridade

Para MVP vertical, considerar `FARM-015` como **S/V2 visual**, mas não bloquear o MVP funcional. O que bloqueia o MVP é:

- selecionar canteiro;
- plantar;
- crescer;
- colher;
- salvar;
- restaurar.

O indicador visual bonito de todos os canteiros pode entrar depois do loop funcional, desde que o estado do canteiro esteja legível por placeholder.


