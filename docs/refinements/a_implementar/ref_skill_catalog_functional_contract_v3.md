# Refinamento funcional das habilidades de Vaalara v3

> **Status:** APROVADO PARA EXECUÇÃO FASEADA — baseline v1; tuning final depende de PlayMode
> **Data:** 2026-09-10
> **Domínio:** Skills, Combat, Survival, Crafting, UI e Presentation
> **Escopo:** 66 nós canônicos: 31 ações equipáveis e 35 passivas/capstones
> **Base:** catálogo e runtime vivos após a fundação de skills; D01 já aprovada

## Resultado pretendido

Cada ponto gasto precisa produzir uma decisão compreensível e um efeito verificável. Uma ação só é
funcional quando o custo, targeting, execução, reação, feedback e progressão por rank formam o mesmo
contrato. Um executor genérico ou um toast não basta. Uma passiva só é funcional quando o sistema que
ela promete modificar realmente consome o modificador, respeita a condição e o remove no respec.

O conjunto preserva a identidade classless: o jogador mistura técnicas, magia, sobrevivência e ofício.
Kanthor, Kaand, Anya, Senya, Telisandra e Thoren dão significado às escolhas, sem impor classes, culto ou
juramento mecânico. A cosmologia e a fantasia de Vaalara, inspiradas no mundo de D&D do projeto, são
traduzidas para um action RPG legível em tempo real. Farm, Town e Cave oferecem usos diferentes; nenhuma
habilidade comum substitui a Fonte, Água Viva, Fruto Mana, provisões, ferramentas ou preparação.

## Evidência atual e classificação

O catálogo vivo possui 66 nós. Existem 31 ações mapeadas, porém várias entregam apenas parte do nome:
Grito não provoca, Corrente atravessa em linha em vez de saltar, Nuvem é um leque de projéteis, Instinto
não revela, Campo Seguro não cria campo e Bomba não consome uma bomba. Disparo Carregado tem executor
de projétil, mas o nó continua marcado dormente e não carrega. Reparo Rápido não tem rota de execução.

Os documentos antigos descrevem 70 nós, mas o catálogo executável e save-safe contém 66 IDs. Este
refinamento preserva os 66 como roster canônico atual. Os quatro conceitos removidos continuam material
de design e não devem ser recriados automaticamente. Magias aprendidas por tomo pertencem ao catálogo de
spells/actions e só viram nós da árvore mediante decisão explícita, migração de save e revisão do orçamento
de pontos.

Usaremos quatro estados visíveis na UI:

- **Operacional:** cumpre o contrato inteiro e possui evidência de runtime.
- **Parcial:** executa algo real, mas não cumpre uma parte que muda a decisão de uso.
- **Dormente:** conceito preservado, indisponível para compra/equip até existir implementação completa.
- **Passiva pendente:** o modificador existe no catálogo, mas o consumidor ainda não foi provado.

Legado comprado não é apagado. Se um nó precisar ficar dormente, o save preserva ID/rank e a UI explica
o motivo; respec continua podendo restituir o gasto. Não vender um efeito que ainda é apenas feedback.

## Decisões do refinamento

| ID | Regra | Estado |
|---|---|---|
| RF00 | Preservar os 66 IDs vivos; referências a 70 nós são fonte histórica, não autorização para restaurar conteúdo | Recomendado |
| RF01 | Pipeline único: readiness → alvo → custo → windup → commit → efeitos ordenados → cooldown → feedback | Recomendado |
| RF02 | Custo só é debitado quando a ação entra no commit; cancelamento antes do commit não cobra, depois do commit cobra | Recomendado |
| RF03 | Dano, custo, alcance, duração e forma vêm de dados por rank; preview e execução usam a mesma resolução | Recomendado |
| RF04 | Cooldown é compartilhado por `actionId`, inclusive cópia/troca de slot; usa tempo escalado | **Aprovado e implementado (D01)** |
| RF05 | Todos os ranks custam 1 ponto; gates por pontos gastos são T1=0, T2=5, T3=11, T4=18, T5=26 | Regra vigente |
| RF06 | Rank cap da árvore sobe 2/3/4/5 ao abrir T1/T2/T3/T4; ativas respeitam também seu cap autorado 3 ou 5 | Regra vigente |
| RF07 | Capstones usam 3 ranks: R1 identidade, R2 consistência, R3 especialização; ranks 4/5 não agregam decisões | **Aprovado para baseline v1 (D02)** |
| RF08 | Controle forte exige resistência/imunidade; DR por alvo depende da decisão D09; boss só reage em janela elegível | Recomendado |
| RF09 | Toda ação tem windup, active, recovery, whiff e recusa legíveis; áudio recebe evento, pois a fundação de áudio ainda é pendente | Recomendado |
| RF10 | Não adicionar nós novos antes de tornar operacional o repertório preservado; Contra-Ataque/Purificar são restaurações candidatas separadas | Recomendado |

O teto base vigente é 50 pontos no nível 100, um ponto a cada dois níveis. A game rule aceita acrescenta
um ponto por ato principal, chegando a aproximadamente 55 pontos efetivos. Uma árvore principal consome
aproximadamente 30–34 pontos e sua conclusão ampla 40–46. Abrir T5 em duas árvores exige 52 pontos gastos;
comprar R1 dos dois capstones exige 54. Portanto uma build extrema pode ter duas identidades R1, mas não
dois capstones completos: dois R3 exigiriam ao menos 58 pontos. O orçamento favorece uma árvore principal
com capstone completo e uma especialização parcial, sem proibir o híbrido extremo.

### Contrato de execução comum

`CanExecute` resolve estado, rank, variante, recurso, contexto e alvo sem mutar. O windup pode ser
cancelado por stagger, morte, troca de cena ou alvo inválido. O commit debita uma vez. Efeitos são pequenos
e ordenados: deslocar/criar área → dano → posture/knockback → status → eventos. Cooldown inicia apenas no
commit. Falhas publicam uma chave localizada. Mesmo contexto e seed produzem os mesmos alvos e procs.

Status não são redefinidos pela habilidade. Ela aplica IDs existentes; o receptor decide resistência,
refresh e imunidade. Pela game rule vigente existe uma instância de cada status por alvo, sem pilhas;
reaplicar substitui a instância e reinicia sua duração. Para impedir controle permanente, este refinamento
propõe em D09 que apenas controles fortes usem duração efetiva de 100%, 60%, 30% e depois imunidade de 4 s.
Até D09 ser aprovada e a regra canônica migrada, skills seguem o refresh simples vigente.

Independentemente de D09, controle aplicado por uma mesma skill em elite tem duração acumulada máxima de
`0,40 × cooldown` por ativação. Laço R3 fica limitado a 2,4 s, Grito a 3,2 s e Sigilos a 3,2 s; permanecer
no mesmo campo não renova esse orçamento. Uma nova ativação após cooldown abre novo orçamento. Boss não
recebe controle fora de janela elegível e, dentro dela, usa o limite explícito da própria skill.

## Perfis de movimento, animação e reação

Os tempos abaixo são envelopes de playtest e devem morar em profiles/SOs. Não são frames codificados no
MonoBehaviour. Em arte 12 fps, 0,08 s corresponde aproximadamente a um frame.

| Perfil | Windup / active / recovery | Leitura e reação |
|---|---|---|
| M1 corte curto | 0,10 / 0,10 / 0,20 s | antecipação do ombro, arco de lâmina, hit-stop 0,04 s; alvo pisca e recua |
| M2 giro | 0,20 / 0,18 / 0,38 s | giro corporal completo, trilha circular; whiff mantém recovery |
| M3 avanço | 0,15 / deslocamento 0,22 / 0,30 s | smear frontal, poeira e impacto; parede interrompe deslocamento |
| M4 salto | 0,28 / deslocamento+queda 0,30 / 0,45 s | sombra no chão, pose aérea e aterrissagem; stagger cancela antes da decolagem |
| R1 tiro | 0,12 / release / 0,22 s | arco tensiona, flecha/trilha visível, receptor inclina na direção do impacto |
| R2 carregado | 0,20–1,10 / release / 0,35 s | três níveis visuais de tensão; soltar cedo dispara versão fraca; stagger cancela |
| C1 conjuração rápida | 0,18–0,35 / release / 0,25 s | runa na mão, cor elemental e projétil distinto; impacto nunca é só círculo sólido |
| C2 conjuração pesada | 0,55–0,90 / 0,10 / 0,40 s | selo/área aparece antes do commit; interrupção e alcance ficam legíveis |
| U1 utilidade | 0,25–0,60 / aplicação / 0,25 s | ícone/outline do alvo e mudança de estado observável |
| O1 dispositivo | 0,35 / arremesso/instalação / 0,35 s | item físico sai da mão; consumo aparece no HUD; explosão/ativação tem telegraph próprio |

Cada ação publica eventos para animação/VFX/SFX/HUD. Impacto mínimo: contato visível, flash ou mudança de
silhueta, reação coerente, feedback de dano/status e resposta de whiff. A UI mostra custo, cooldown, rank,
forma, alvo requerido e razão da recusa antes do jogador gastar um ponto.

## Contrato das 31 ações equipáveis

Valores marcados **vigente** vêm do addendum/runtime. Valores **propostos** fecham a identidade prometida
e precisam de playtest antes de virarem dados finais. `Req.` inclui o pré-requisito direto do catálogo;
o gate de pontos do tier é cumulativo.

### Melee — compromisso, posição e postura

| Ação | Tier/ranks, requisito | Custo e números | Efeito, reação e apresentação | Estado/decisão |
|---|---|---|---|---|
| Corte da Mão Secundária | T2, R1–3; Fluxo de Duas Lâminas | 10 STA, `8 + 2 × (rank−1)`, CD 2,5 s, arco 140°×1,2 | Exige arma secundária leve; golpe M1. Um hit/alvo, recuo baixo. Sem offhand: `requires_offhand` | Parcial: dano funciona; falta gate do equipamento |
| Corte Giratório | T2, R1–3; Postura Guardada | 22 STA, `10 + 2 × (rank−1)`, CD 6 s, 360°×1,7 | M2, máximo 6 alvos, knockback radial leve; 70% do dano após o 3º alvo proposto | Núcleo operacional; falta cap/queda multi-alvo |
| Avanço de Aço | T2, R1–3; Pegada de Ferro | 22 STA, `12 + 3 × (rank−1)`, CD 6 s, avanço 2,5 + arco 110° | M3. Para no primeiro obstáculo; +25% posture contra alvo em recovery, sem dano extra universal | Parcial: avanço/hit funcionam; falta janela de recovery |
| Arrancada de Combate | T3, R1–3; Ímpeto de Duas Mãos | 20 STA, `8 + 2 × (rank−1)`, CD 5 s, avanço 3,0 + arco 100° | M3 de reposicionamento. Atravessa inimigo leve, não parede; golpe baixo e recovery curto | Parcial: diferenciar travessia/reposição do Avanço |
| Salto Devastador | T3, R1–3; Arrancada | 25 STA, `14 + 3 × (rank−1)`, CD 7 s, alcance 2,2 | M4. Escolhe ponto válido, acerta raio 1,4 na aterrissagem, +posture; risco no windup/recovery | Parcial: hoje é lunge plano, sem salto/área de queda |
| Grito de Desafio | T3, R1–3; Avanço de Aço | 18 STA, `6 + 1 × (rank−1)`, CD 8 s, raio 2,2 | U1. Dano mínimo+knockback; inimigos simples priorizam caster 3/4/5 s. Elite reduz duração; boss só perde estabilidade em janela. Repetição usa refresh vigente ou DR se D09 aprovada | Parcial: hoje só dano/knockback, sem taunt/estabilidade |
| Investida Quebra-Guarda | T4, R1–3; Grito | 26 STA, `16 + 4 × (rank−1)`, CD 9 s, avanço 2,0, posture×3 | M3 pesado. Se postura quebrar, publica critical window; alvo bloqueando recebe +25% posture, não +dano | Parcial: posture×3 funciona; falta condição de bloqueio/window |

### Ranged — alinhamento, cobertura e preparação

| Ação | Tier/ranks, requisito | Custo e números | Efeito, reação e apresentação | Estado/decisão |
|---|---|---|---|---|
| Disparo Carregado | T1, R1–5; Mira Longa | 20 STA runtime vs 38 STA addendum; CD 6 s; 20 dano runtime | R2. Carga 0,20–1,10 s; dano 8→20, alcance 5→9 e posture 0,5→1,25 interpolados. Custo no release: 12→26 STA proposto | Contradição: executor simples+dormant flag. Adotar curva de carga; 38 STA é alto demais para T1 |
| Linha Perfurante | T3, R1–3; Encaixe Rápido | 18 STA, `12 + 3 × (rank−1)`, CD 7 s, 10 tiles, 5 alvos | R1 pesado. Obstáculo sólido encerra; dano por alvo 100/80/64/51/41%, cada alvo uma vez | Parcial: perfura, mas falta queda explícita/obstáculo validado |
| Tiro Triplo | T3, R1–5; Disparo Carregado | 24 STA, `8 + 2 × (rank−1)` por flecha, CD 8 s, 28° | R1. Três flechas; mesmo alvo pode receber no máximo 2, segunda a 50%, evitando 3× burst encostado | Parcial: leque funciona; falta regra de sobreposição |
| Flecha Sangrante | T3, R1–3; Linha Perfurante | 16 STA, `14 + 3 × (rank−1)`, CD 6 s, alcance 8 | R1. Hit aplica Bleed; reaplicar substitui e reinicia a duração, sem empilhar dano. Alvo mostra corte vermelho e tick legível | Operacional no núcleo; falta refresh correto/arte final |
| Presa Marcada | T1, R1–3; **Mão Firme (`ranged_steady_hand`) proposta** | 10 STA, CD proposto 10 s, alcance 8 tiles, duração 6/8/10 s | U1/R1. Seleciona o inimigo válido mais próximo; +8/12/16% dano ranged do caster em skills e arco comum, com reveal. Reaplicar move a marca. Boss aceita bônus, não controle | Dormente. Amendment corrige o pré-requisito T3 inválido sem elevar o tier |

### Magic — elementos com formas diferentes

| Ação | Tier/ranks, requisito | Custo e números | Efeito, reação e apresentação | Estado/decisão |
|---|---|---|---|---|
| Fagulha Ígnea | T1, R1–5; Fio Arcano | 10 MP, `12 + 2 × (rank−1)`, CD 3 s, alcance 7 | C1 `0,18/0/0,25`. Hit direto; sem Burn base para preservar Chama. Pequena explosão visual sem AoE | Operacional no núcleo; sprite/VFX provisório |
| Chama Breve | T2, R1–3; Fagulha | 8 MP, `8 + 2 × (rank−1)`, CD 2,5 s | C1 `0,30/0,10/0,30`; cone 70°×2,5; até 4 alvos, 70% após primeiro; aplica uma vez/cast `status_burn_minor` | Parcial: hoje é segundo bolt e não aplica Burn |
| Laço de Gelo | T2, R1–3; Canalização Rápida | 14 MP, `10 + 2 × (rank−1)`, CD 6 s, alcance 7 | C1 `0,25/0/0,25`; Chill 50% por 3/4/5 s; alvo já Chilled recebe Slow 70% por 1 s, DR forte; limites elite/boss da spec 14 | Operacional no núcleo; nome não significa prisão total |
| Rajada Gélida | T3, R1–3; Laço (recomendado) | 16 MP, `3 × (6 + 1 × (rank−1))`, CD 6 s, alcance 6, spread 30° | C1 `0,30/0/0,30`; mesmo alvo no máximo 2, segundo 50%; Chill 3 s uma vez/alvo/cast, sem Slow forte | Operacional no núcleo; falta regra de sobreposição |
| Nuvem Tóxica | T3, R1–5; Fagulha | 18 MP, orçamento total `24 + 6 × (rank−1)`, CD 8 s | C2 `0,70/0,10/0,40`; ponto válido até 6; área raio 2 por 4 s, pulsos t=0/1/2/3; Poison uma vez por alvo/cast | Parcial: três projéteis contradizem nome/direção. Recomenda-se área persistente (D05) |
| Corrente Relâmpago | T3, R1–5; Laço | 20 MP, `12 + 2 × (rank−1)` inicial, CD 8 s, alcance inicial 9 | C1 `0,35/0/0,30`; até 4 alvos/3 tiles/LoE; 100/75/55/40%; construct vulnerável sofre posture 0,50×dano final | Parcial: hoje projétil perfurante, sem cadeia espacial |
| Guarda Elemental | T2, R1–3; Fio/Poço conforme árvore revisada | 18 MP, CD 14 s, 3/4/5 s | C2 `0,55/0,10/0,40`; -25/32/40% de 2 hits elementais diretos; DoT/físico/stagger não consomem; recast reinicia | Dormente/feedback-only |
| Sigilos Lentificantes | T4, R1–5; ramo elemental | 18 MP runtime, CD 8 s, raio 2,5 | C2 `0,70/0,10/0,40`; selo 4/5/6/7/8 s; Slow 20/24/28/32/35%; elite teto 3,2 s; boss só 10% em janela; uma DR/alvo/zona | Núcleo real; custo/Tier precisam reconciliar direção de selo |

### Survival — sobreviver e retirar-se, sem eliminar preparação

| Ação | Tier/ranks, requisito | Custo e números | Efeito, reação e apresentação | Estado/decisão |
|---|---|---|---|---|
| Último Fôlego | T4, R1–5; Recuperação Instintiva | sem custo, +40 HP runtime, CD 90 s | U1. Fica disponível por 5 s ao cruzar HP<25%; cura 25/30/35/40/45% HP máx, 1 uso por encounterId persistente | Parcial: restore existe, mas falta gate/percentual/estado de encontro |
| Sinal de Retirada | T2, R1–3; Passo Seguro | 15 STA proposto, CD 18 s, duração 5/6/7 s | U1. -30% custo de corrida/dodge e +12% move speed ao afastar-se de inimigo; atacar encerra | Dormente/feedback-only |
| Isca Improvisada | T2, R1–3; Passo Seguro | consome 1 isca, CD 12 s, duração 4/6/8 s | O1. Arremessa objeto; criaturas simples investigam. Elite ganha apenas desvio curto; boss ignora | Dormente; exige item/receita e AI elegível |
| Kit de Emergência | T3, R1–5; Sinal de Retirada | consome 1 `field_dressing`; cura `18 + 3 × (rank−1)%` do HP máx, CD 45 s | U1 canal 0,8 s, somente fora de combate e parado; dano cancela antes do commit e não consome item | Parcial: cura funciona, mas faltam item/gate/canal; mudança econômica D10 |
| Instinto de Sobrevivência | T3, R1–5; Isca | sem custo, CD 30 s | U1. Revela por 4/5/6/7/8 s recursos, perigos e interagíveis já gerados em 7 tiles; não restaura STA nem revela segredo/loot oculto | Parcial: restore atual é efeito substituto e deve dar lugar ao reveal prometido |
| Campo Seguro | T4, R1–5; Kit | consome 1 `camp_supply`, 1 uso por cave run, duração 8/10/12/14/16 s | C2 zona fora de combate: -50% fome/fadiga e +50% regen natural HP/STA/MP; sem pulso direto. Dano dissolve e não restitui item | Parcial: hoje restauração instantânea, sem zona/gates; mudança econômica D10 |

### Crafting e Farm — preparação material, sem magia disfarçada

| Ação | Tier/ranks, requisito | Custo e números | Efeito, reação e apresentação | Estado/decisão |
|---|---|---|---|---|
| Remendo de Campo | T2, R1–3; Cuidado no Reparo | 1 kit simples, CD 20 s proposto | U1. Repara 10/15/20% da durabilidade atual até 60% do máximo; não repara item quebrado/artefato | Dormente/feedback-only |
| Reparo Rápido | T3, R1–3; Remendo | materiais reais da receita, sem cooldown fora de combate | U1 canal 1,2/1,0/0,8 s; repara até 85% em campo. Em bancada usa fluxo normal, não duplica bônus | Dormente e sem mapping; definir diferença do Remendo antes de implementar |
| Irrigador Portátil | T3, R1–3; Reparo Rápido | 18 STA + 1 carga, CD 8 s proposto | O1. Rega linha 3/4/5 plots válidos; não fertiliza, não cresce crop e não consome em alvo sem solo seco | Parcial: watering real existe; faltam carga, linha e recusa completa |
| Bomba Improvisada | T3, R1–3; Reparo Rápido | consome 1 bomba; `18 + 3 × (rank−1)`, CD 12 s | O1 trajetória em arco, explosão raio 1,8, `MaxTargets=4`, um hit/alvo, posture alto. Tipo depende da receita; base físico, não Toxic gratuito | Parcial: hoje projétil Toxic perfurante e cobra 20 STA, não item |
| Marca de Eficiência | T4, R1–3; Bomba | 6 STA, CD 45 s, duração 12/16/20 s | O1/U1 em estação ou área 2,5: -20/25/30% STA de ações agrícolas/ofício; uma marca ativa; não reduz materiais/tempo. Quatro ações de 8 STA deixam saldo líquido +0,4/+2,0/+3,6 | Dormente/feedback-only; custo anterior de 20 STA era dominado |

## Contrato das 35 passivas e capstones

Passivas não têm custo de mana/stamina nem animação de uso. Cada rank custa 1 ponto. O feedback é um
ícone/tooltip de fonte no painel de atributos ou no resultado afetado; efeitos condicionais só aparecem
quando a condição é verdadeira. Valores abaixo preservam o payload atual quando ele existe e limitam
ganhos percentuais para evitar cap invisível.

### Melee (6)

| Nó | Requisito | Efeito por rank e condição | Estado/refino |
|---|---|---|---|
| Pegada de Ferro | raiz T1 | +1 Attack melee/R, apenas arma melee | Real; mostrar contribuição |
| Postura Guardada | Pegada | +1 Defense/R enquanto arma melee ou escudo equipado | Hoje incondicional; condicionar |
| Fluxo de Duas Lâminas | Pegada | +10% attack speed/R somente dual wield; cap +30% | Mod existe; consumer/condição precisam prova |
| Ímpeto de Duas Mãos | Postura | +1 dano base/R e +5% posture/R somente two-handed | Mod existe; falta consumidor específico |
| Treino de Esquiva | Arrancada | -10% custo STA de dodge/R; cap -30%, sem reduzir i-frames/cooldown | Parcial; descrição não deve prometer cooldown |
| Ritmo de Batalha — Kanthor/Kaand | T5; `melee_dodge_training` + `melee.investida_quebra_guarda`; +26 pts | R1 escolhe variante; R2 consistência; R3 ápice. Valores e gatilhos exatos estão na escada abaixo | O catálogo aceita apenas um requisito; spec de dados deve suportar os dois IDs. Cap 3 recomendado |

### Ranged (6)

| Nó | Requisito | Efeito por rank e condição | Estado/refino |
|---|---|---|---|
| Mão Firme | raiz T1 | +1 BowDamage/R | Real; fonte visível |
| Mira Longa | Mão Firme | +0,5 tile range/R, cap +1,5; não aumenta auto-target além da tela | Real; validar câmera/alvo |
| Encaixe Rápido | Mão Firme | -8% recovery de tiro/R, cap -24%; não reduz cooldown de skills | Hoje usa AttackSpeed; reconciliar tooltip |
| Passos de Kiting | Tiro Triplo | após disparar, +5% move/R por 1,5 s enquanto se afasta; atacar renova, não empilha | Hoje MoveSpeed incondicional; corrigir condição |
| Afinação de Projétil | Passos | +1 tile/s/R e -5% queda balística/R, somente bow projectiles | Mod morto; exige consumer |
| Foco da Águia / Três Luas | T5; `ranged_bleeding_arrow` + `ranged_projectile_tuning`; +26 pts | Primeiro alvo marcado no encontro recebe marca lunar. R1 escolhe uma lua; R2 melhora consistência; R3 efeito pleno. Uma lua ativa, sem somar três bônus | Baseline dá range/speed genéricos; requer dois IDs e variante salva. Cap 3 recomendado |

### Magic (5)

| Nó | Requisito | Efeito por rank e condição | Estado/refino |
|---|---|---|---|
| Poço de Mana | raiz T1 | +10 MaxMana/R | Real; até cap dinâmico |
| Canalização Rápida | Poço | substituir +1 MP/s flat por +8% da regen base/R, cap +24%; não escala bônus externos | Flat atual ameaça sustain; decisão D04 |
| Fio Arcano | Poço | +1 Arcane/Magic Attack/R, não dano físico | Hoje AttackFlat genérico; separar consumer |
| Domínio do Raio Arcano | Fio | preserva ID e proposta de +2 dano/R e +5% velocidade/R para um spell arcano futuro | **Passiva pendente e não comprável** até existir `actionId` arcano separado; não aplicar a Fagulha só para preencher o hook |
| Confluência — Anya/Senya | T5; `magic_elemental_ward` + `magic_slowing_sigils`; +26 pts | Gasto ≥35% MaxMP em janela deslizante de 6 s prepara a próxima magia espiritual/ofensiva; resultado por rank está na escada proposta abaixo | Requer dois IDs e variante salva; runtime ausente. Cap 3 recomendado |

### Survival (9)

| Nó | Requisito | Efeito por rank e condição | Estado/refino |
|---|---|---|---|
| Pulmões da Caverna | raiz T1 | +10 MaxStamina/R | Real |
| Pele Dura | Pulmões | +5 MaxHP/R | Real |
| Rações Curtas | Pulmões | -10% hunger drain/R, cap -30%; não reduz custo de receita | Real se consumer vigente; provar |
| Senso Tóxico | Pele | +1 resistência Toxic/R; tooltip traduz em redução efetiva | Real; mostrar fórmula |
| Hábito do Frio | Pele | +1 resistência Cold/R | Real; mostrar fórmula |
| Têmpera do Calor | Pele | +1 resistência Heat/R | Real; mostrar fórmula |
| Recuperação Instintiva | Senso Tóxico | -10% duração Poison/Burn/Slow/R, cap -30%; não reduz dano instantâneo | Real se applier consome; provar status a status |
| Passo Seguro | Rações | -15/25/35% penalidade de terreno, sem bônus em chão normal | Hoje +MoveSpeed incondicional; corrigir |
| Nascido da Caverna / Telisandra | T5; `survival_last_breath` + `survival.campo_seguro`; +26 pts | 1× por cave run quando HP<25%, STA<15% ou Exausto: sobrevivência temporária, resistência, dodge ampliado e recuperação após perigo. Nunca rearma em checkpoint ou load | Requer dois IDs e estado da run no save. Cap 3 recomendado |

### Crafting (9)

| Nó | Requisito | Efeito por rank e condição | Estado/refino |
|---|---|---|---|
| Mãos Ágeis | raiz T1 | -10% tempo de craft/R, cap combinado de redução 60% | Real; hoje cap global pode chegar 75%, refinar |
| Cuidado no Reparo | Mãos | +10% durabilidade restaurada/R, cap +30%, sem reduzir materiais | Real se repair consome; provar |
| Olho de Material | Mãos | +5% chance/R de +1 recurso em coleta elegível; um roll seeded por node, não duplica loot raro/quest | Hook pendente |
| Foco de Bancada | Olho | -5% materiais comuns/R apenas em workstation adequada; arredondar após soma e mínimo 1; não afeta ingrediente raro | Hook atual diz custo, mod atual diz tempo; reconciliar |
| Mochila Ordenada | Mãos; R1 | libera preset de abastecimento de estação e reserva de slots; não cobra ponto por sort/search/transfer básicos | Manter dormente até preset/reserva existirem na UI |
| Método de Salvage | Foco | +5% chance/R de recuperar 1 material comum adicional; nunca restitui item inteiro/ingrediente raro | Hook atual errado (tool efficiency); consumer pendente |
| Acabamento Durável | Salvage | +8% DurabilityMax/R em equipamento criado pelo jogador; snapshot no item instance, cap +24% | Consumer pendente/save por tipo simples |
| Senso de Mercado | Mochila | escolher ao comprar **ou** vender: -5% preço de compra/R ou +5% venda/R, cap 15%; escolha muda só em respec | Hook pendente; evita arbitragem simétrica |
| Mestre Artesão / Forja Viva de Thoren | T5; `crafting_durable_finish` + `crafting.marca_eficiencia`; +26 pts | 1×/dia ao concluir craft/lote elegível de ferramenta, arma, armadura ou lote de consumíveis com qualidade: progressão exata abaixo. O uso é consumido após criar o output, persiste por day index e não reseta em load/respec | Requer dois IDs, evento de conclusão e estado diário no save. Cap 3 recomendado |

## Requisitos de progressão e legibilidade

Tier e pré-requisito são cumulativos. A UI deve mostrar: `faltam N pontos na árvore`, `exige X`, rank
atual/cap, ganho exato do próximo rank, condição de equipamento/estado, custo da ação e motivo de dormência.
O primeiro rank nunca esconde todo o poder de uma passiva de cinco ranks; cada rank deve alterar ao menos
um número perceptível. Se o ganho final for menor que 15% e não mudar nenhuma capacidade, fundir ranks ou
reduzir cap em vez de criar progressão ornamental.

Passivas flat precisam de uma prova em três marcos antes de seus números serem aceitos: nível 1 sem item,
nível 50 com equipamento mediano e nível 100 com equipamento-alvo. R1 deve representar aproximadamente
5–15% do atributo nu no momento em que abre; o rank máximo deve continuar valendo ao menos 5% do total
equipado no fim do jogo sem superar 20%. Se `+1 Attack/Defense/resist`, `+5 HP` ou `+10 Mana` ficar fora
desse envelope, a spec de dados troca o valor ou usa percentual. Resistências também devem publicar a
redução efetiva resultante, porque `+1` sem fórmula não é uma promessa verificável.

Capstones em R1 já devem definir a build, mas R2/R3 precisam justificar especialização. A recomendação de
três ranks mantém híbridos possíveis no teto de 50 pontos base e aproximadamente 55 efetivos, e impede
gastar dois pontos invisíveis em R4/R5. Esta recomendação depende da confirmação humana D02 e não altera
o runtime atual.

### Escada baseline dos capstones, aprovada em D02

O gatilho e sua frequência não melhoram com rank. O rank aumenta o resultado, evitando que especialização
também gere mais ativações. Os números abaixo substituem as descrições vagas das tabelas anteriores quando
baseline v1 for implementada.

| Capstone | R1 — identidade | R2 — consistência | R3 — especialização |
|---|---|---|---|
| Ritmo de Batalha — Kanthor | perfect block/quebra: 8 s, +10% dano, +15% estabilidade e +12% resist a stagger; hit na janela cura 2% HP | +12% dano, +20% estabilidade, +16% resist; cura 3% | +15% dano, +25% estabilidade, +20% resist; cura 3% ou concede +10 STA se HP cheio |
| Ritmo de Batalha — Kaand | ao causar posture break: 8 s, +18% dano e +15% crit damage, sem cura | +24% dano e +20% crit damage | +30% dano e +25% crit damage |
| Foco da Águia — Alihana | primeiro alvo marcado no encontro: 6 s de reveal e +8% alcance efetivo contra ele | 8 s e +12% alcance | 10 s, +16% alcance e projétil +15% mais rápido contra o marcado |
| Foco da Águia — Senya | primeiro alvo marcado: 6 s; hits ranged adicionam 8% do dano base no elemento da última magia ofensiva | 8 s e 12% | 10 s e 16%; o bônus não aplica status nem reage consigo mesmo |
| Foco da Águia — Nyx | primeiro alvo marcado sem outro hostil em 2,5 tiles: 6 s, +8% crit chance | 8 s e +12% | 10 s, +15% crit chance e +15% crit damage; perde o bônus enquanto deixar de estar isolado |
| Confluência — Anya | ao gastar 35% MaxMP em 6 s: próxima espiritual custa -30% MP e ganha +20% output | -40% MP, +28% output e eco de regen por 2 s | -50% MP, +35% output e eco de regen por 4 s |
| Confluência — Senya | mesmo gatilho: próxima magia ganha +20% dano e +8% crit/overload por +5% MP | +28%, +12%, custo +8% | +35%, +15%, custo +10% |
| Nascido da Caverna — Telisandra | 1×/runId: 8 s, -30% custo de corrida/dodge, +20% resist ambiental/mental e +0,4 tile no dodge sem i-frame extra | 9 s, -38% custo, +25% nas mesmas famílias | 10 s, -45% custo, +30% e, após sair de combate, regen natural de HP ×2 pelo restante do buff |
| Mestre Artesão — Thoren | 1×/dia: melhora um estágio de qualidade de um output sem ultrapassar o cap | pode escolher qualidade ou economizar 1 material comum, mínimo 1 consumido | mantém a escolha e adiciona +8% DurabilityMax ao equipamento criado; lote de até 5 consumíveis recebe +8% de duração, sem valor de venda adicional |

Confluência precisa de um limite de reativação que preserve a rotação de mana. A proposta para playtest é
lockout interno de 25 s após consumir a magia preparada; o gasto durante o lockout não acumula para o
próximo gatilho. Esse valor ainda é uma decisão de balanceamento, não regra canônica.

O nome `Último Fôlego` já pertence a uma ação ativa diferente do capstone de Telisandra. O ID e o nome são
preservados para não produzir migração cosmética sem necessidade; uma eventual troca de display name é uma
decisão editorial separada e não muda a mecânica.

Ao cruzar HP<25%, Nascido da Caverna resolve automaticamente primeiro se ainda estiver armado na cave run.
Depois, Último Fôlego fica disponível por 5 s e exige input do jogador. Os efeitos podem coexistir porque
um ocupa capstone e o outro ocupa slot ativo, mas cada um consome sua própria chave persistente: `runId`
para Telisandra e `encounterId` para Último Fôlego. STA<15% ou Exausto ativa apenas Telisandra. Load não
rearma nenhum dos dois e a cura de Último Fôlego não reinicia o gatilho no mesmo encontro.

`encounterId` nasce quando um grupo hostil entra em aggro pela primeira vez. Inimigos que se juntam recebem
o mesmo ID. Distância, perda temporária de alvo, fuga do jogador e load não encerram nem recriam o encontro.
Ele só resolve quando todos os membros morrerem, fugirem de forma permanente ou forem removidos por uma
transição canônica de cena, KO/derrota ou encerramento da cave run. Reaggro do grupo não rearma Último
Fôlego. Em Cave, ID e estado resolvido integram o snapshot estável do nível.

### Orçamento material de utilidades

Os nomes abaixo são IDs de contrato propostos; a spec econômica deve mapeá-los aos itens estáveis do
catálogo sem inventar duplicatas.

| Consumível/carga | Receita-base proposta | Consumo e limite | Proteção econômica |
|---|---|---|---|
| `field_dressing` | 2 fibras comuns + 1 reagente medicinal | 1 por Kit de Emergência no commit | cura não vende nem gera subproduto; custo médio ≥ 8% de uma provisão de cave inicial |
| `camp_supply` | 2 madeiras comuns + 1 fibra + 1 ração | 1 por Campo Seguro e no máximo 1 uso/run | não pode ser criado dentro da Cave sem bancada/kit já levado |
| `improvised_lure` | 1 fibra + 1 alimento de baixo valor | 1 por Isca no commit | inimigo não derruba a isca de volta |
| `portable_irrigator_charge` | água + peça comum reutilizável | uma carga por linha realmente regada | recusa em solo inválido não consome; não cria água |
| `improvised_bomb` | invólucro + reagente + componente do tipo | 1 por arremesso no commit | output não pode ser vendido acima dos inputs e não retorna material em hit |

Forja Viva só é aceita se o valor médio adicional diário ficar abaixo de 20% da renda mediana de um dia
de Town no mesmo estágio. O uso diário é consumido após output criado com sucesso; cancelamento/falha não
consome. `dayIndex` e variante ficam no save, e respec não restaura a carga do dia.

## Equilíbrio: fórmulas e limites verificáveis

- Dano por rank: `base + incremento × (rank−1)` antes de mitigação.
- Pierce/chain/fan aplicam a queda declarada por alvo/projétil antes de crit e vulnerabilidade.
- Modificadores percentuais da mesma categoria combinam aditivamente até o cap; categorias diferentes
  multiplicam. UI e runtime mostram a mesma decomposição.
- Dano sustentado comparável: `(dano esperado × alvos efetivos) / (windup + recovery + cooldown)`; burst
  também mede recurso por dano e exposição durante cast.
- Controle mede `duração efetiva / cooldown`, resistência e, se D09 for aprovada, diminishing returns; não apenas chance.
- Recuperação gratuita mede HP/STA/MP líquido por minuto e por cave run. Esperar em segurança não pode
  substituir comida, poção, descanso e retorno à cidade.
- Economia mede custo de inputs, tempo, output, preço de compra/venda e arredondamento. Nenhuma combinação
  pode comprar, transformar e vender com lucro garantido infinito.

Playtest mínimo por ação ofensiva: alvo único, três alinhados, três dispersos, elite, boss em janela,
obstáculo e whiff. Mesma cena, seed, equipamento e atributos; variar somente a habilidade/rank. Registrar
TTK, dano recebido, recurso líquido, tempo de controle, acertos por alvo e decisões úteis. Para Survival,
usar uma cave run curta e longa sem regenerar níveis visitados. Para Crafting, usar a mesma receita,
qualidade, estoque e dia.

Critérios iniciais de comparação, como envelopes e não aprovação final:

- habilidade de área não supera a melhor opção single-target em alvo único;
- controle comum não mantém elite controlada por mais de 40% do tempo e não controla boss fora de janela;
- rank adicional entrega 12–25% de ganho na dimensão principal ou capacidade observável;
- uma ação básica sustentável é aceitável, mas uma rotação inteira de dano+controle+cura não pode ser
  infinita sem provisão/risco;
- quatro slots permanecem custo de oportunidade real; passivas não ocupam slot.

### Checagem analítica do envelope ofensivo

Esta comparação usa o maior rank autorado, dano base antes de mitigação e o cooldown base. Área/chain
usa o máximo teórico contratado, portanto é teto, não DPS garantido. DoT/status e windup não foram
convertidos artificialmente em dano.

| Caso | Dano máximo por execução | Dano/CD | Dano/recurso | Leitura |
|---|---:|---:|---:|---|
| Corte secundário R3 | 12 | 4,8 | 1,20 | bom single curto; gate de offhand necessário |
| Giro R3, um alvo / seis alvos | 14 / 71,4 com queda | 2,3 / 11,9 | 0,64 / 3,25 | forte só quando cercado; recovery longo sustenta risco |
| Quebra-Guarda R3 | 24 | 2,7 | 0,92 | dano contido porque o valor principal é posture×3 |
| Linha R3, um / cinco alvos | 18 / 60,5 com queda | 2,6 / 8,6 | 1,00 / 3,36 | teto exige alinhamento perfeito |
| Tiro Triplo R5, um / três alvos | 24 / 48 | 3,0 / 6,0 | 1,00 / 2,00 | regra de segundo projétil a 50% evita 48 single-target |
| Fagulha R5 | 20 | 6,7 | 2,00 | baseline sustentável; sem status/área |
| Chama R3, um / quatro alvos | 12 / 37,2 com queda | 4,8 / 14,9 | 1,50 / 4,65 | teto alto exige proximidade; validar windup e Burn |
| Rajada R3, um / três alvos | 12 / 24 | 2,0 / 4,0 | 0,75 / 1,50 | controle/cobertura, não burst principal |
| Nuvem R5, alvo fica 4 s | 48 | 6,0 | 2,67 | iguala orçamento dos três projéteis atuais, mas exige permanência |
| Corrente R5, um / quatro alvos | 20 / 54 | 2,5 / 6,75 | 1,00 / 2,70 | teto depende de pack dentro da distância de salto |
| Bomba R3, um / quatro alvos | 24 / 96 | 2,0 / 8,0 | usa item, não STA | teto de área exige custo material e um hit/alvo |

O envelope não revelou dominância single-target inevitável depois das regras de sobreposição. Chama e
Bomba têm os maiores tetos de grupo e exigem, respectivamente, proximidade/cast e item craftado. Nuvem
preserva o dano total teórico atual em vez de quadruplicá-lo ao virar quatro ticks. Esses cálculos não
incluem arma, atributo, crit, vulnerabilidade, resistências ou passivas; balanceamento final precisa medir
as builds completas e os inimigos reais.

## Decisões aprovadas para execução faseada

| ID | Escolha baseline v1 | Risco a validar em jogo |
|---|---|---|
| D02 | Capstones com máximo de 3 ranks e a escada deste documento | Custo total de builds e migração/validação de compra |
| D03 | Disparo Carregado com custo interpolado de 12–26 STA | Runtime usa 20 e addendum usa 38; charge real muda risco e eficiência |
| D04 | Canalização Rápida como percentual da regen base, máximo +24% | Evita regen flat superar o custo médio da Fagulha sem equipamento |
| D05 | Nuvem Tóxica como área persistente de quatro pulsos e orçamento total preservado | Troca três projéteis por controle de espaço sem multiplicar o dano |
| D06 | Senso de Mercado escolhe compra ou venda até o próximo respec | Aplicar os dois lados cria arbitragem e uma passiva dominante |
| D07 | Confluência com lockout interno inicial de 25 s | O addendum define o gatilho, mas não limita reativação em rotação de mana |
| D08 | Preservar `Último Fôlego` como nome da ação ativa | Renomear apenas por preferência editorial, sem alterar ID ou save |
| D09 | DR 100/60/30% + imunidade 4 s somente para controle forte | Exige migração explícita da game rule atual |
| D10 | Kit consome `item_consumable_field_dressing`; Campo consome `item_consumable_camp_supply` e limita 1/runId; Instinto não restaura STA | Preserva o ciclo Farm/Town → preparação → Cave |
| D11 | Marca de Eficiência custa 6 STA; mantém duração 12/16/20 e redução 20/25/30% | Break-even passa a exigir uso real, sem tornar uma ativação isolada vantajosa |
| D12 | Chama Breve aplica `status_burn_minor`; Nuvem aplica `status_poison` na entrada | Reusa perfis canônicos e evita uma segunda definição global de Burn/Poison |
| D13 | `runId` identifica uso único; seed serve apenas ao RNG; encounter persiste por reaggro/load | Impede rearmar cura/capstone por revisita, seed repetida ou perda temporária de alvo |
| D14 | Ranks sem degrau funcional são limitados ao último efeito autorado; Mochila Ordenada fica R1 dormente | Nenhum ponto pode ser gasto em rank ornamental ou consumer ausente |
| D15 | Telisandra resiste apenas ambiental/mental e amplia dodge em 0,4 tile sem i-frame; Thoren limita lote a 5 | Mantém capstones fortes sem virar mitigação universal nem multiplicador econômico sem teto |
| D16 | Survival usa os contratos fechados da spec 15: Último Fôlego 1/encontro+CD90; Sinal direcional 15 STA/CD18; Isca máx. 5; Kit canal 0,8 s; Instinto reveal materializado; Campo raio 2,5 e 1/runId | Fecha exploits de reaggro/load/seed, preserva provisões e impede recuperação gratuita ou reveal de conteúdo não gerado |

Há ainda uma correção de governança que não muda o design: `skill_tree_rules.md` registra roster aproximado
de 69 e cooldown por slot. O catálogo vivo possui 66 e D01 já aprovou/implementou cooldown por `actionId`.
O closeout da próxima spec de dados deve reconciliar a game rule e sua ADR de origem; até lá, o runtime e
a decisão humana recente são evidência vigente, e o conflito permanece explicitamente registrado.

A revisão independente recomendou um projétil arcano neutro. A ideia combina com o domínio de magia, mas
não corresponde a um dos 66 nós vivos; por RF00/RF10, fica candidata ao catálogo separado de spells/tomos.
A mesma revisão demonstrou que cooldown não limita recuperação fora de combate. D10 incorpora a solução
como proposta econômica explícita: curas relevantes exigem provisão, e reveal não carrega uma restauração
de STA sem relação com sua identidade.

## Arte, UI e animação por habilidade

Cada ação precisa de `Icon`, `AnimationProfile`, `VFXProfile`, `FeedbackProfile` e, quando a fundação existir,
`SFXProfile`. Ícone comunica forma e função, não apenas cor da árvore. Silhuetas recomendadas: lâmina/arco
para melee, trajetória/alvo para ranged, glifo elemental para magic, ferramenta/sinal para survival/crafting.
Estados Locked, Purchasable, Owned, Equipped, Cooldown, Blocked e Dormant reutilizam o mesmo ícone com
moldura/overlay; não criar sete sprites desconectados.

As capturas atuais provam que o projétil nasce no avatar, porém ele ainda parece um disco laranja. A spec
de pixel art deve substituir o placeholder por formas distintas: fagulha assimétrica com núcleo, gelo com
estilhaço angular, toxic com bolha/fumaça e lightning com segmento quebrado. O impacto precisa ter 3–5
frames e leitura no tamanho real do jogo. A tela Canvas deve exibir preview de forma/alcance e o ganho do
próximo rank; `Camera.Render` do mundo não valida UI overlay.

## Escopo recomendado de specs executáveis

Este refinamento substitui decisões implícitas da spec guarda-chuva 05, mas não autoriza runtime. Gerar
fatias pequenas e seriadas:

1. **Dados/rank/readiness:** fonte única dos números e estados Operacional/Parcial/Dormente.
2. **Melee:** gates de arma, geometria, posture e três movimentos distintos.
3. **Ranged:** charge, pierce/falloff, fan overlap e marca.
4. **Magic projectiles/shapes:** cone, chain e identidades Fire/Ice/Lightning.
5. **Magic zones/support:** Toxic cloud, sigils, ward e, se aprovado, Purificar.
6. **Survival:** thresholds, reveal, zones, encounter/run state e provisões.
7. **Crafting/Farm:** repair, watering, bomba consumível e eficiência sem arbitragem.
8. **Passivas/capstones:** consumers por domínio e save dos triggers.
9. **Canvas/pixel art/feedback:** somente após readiness e profiles estarem estáveis.
10. **Balance acceptance:** matriz de builds híbridas, inimigos reais, economia e cave run.

Cada spec filha deve nomear arquivos/consumidores reais, testes determinísticos e cenário PlayMode. Nenhuma
deve marcar ação feedback-only como sucesso. As decisões D02–D10 foram aceitas pelo pedido humano de
execução faseada em 2026-09-10. Cada spec filha registra apenas as decisões que afetam seu escopo e mantém
os números como tuning v1 até o playtest final.

## Fontes

- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- `Assets/_Game/Scripts/Gameplay/SkillActionEffectCatalog.cs`
- `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
- `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md`
- `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
- `docs/refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md`
- `docs/validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md`
