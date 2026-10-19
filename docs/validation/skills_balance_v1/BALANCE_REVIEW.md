# Equilíbrio das habilidades — revisão analítica de 2026-09-09

> Resultado: **riscos de equilíbrio identificados; aprovação global pendente**.
> O modelo passou suas checagens. Isso não significa que o jogo esteja equilibrado.
> Proposta de design: [revisão Vaalara v2](../../refinements/a_implementar/ref_vaalara_skills_design_and_balance_v2.md).

## 1. Evidência e reprodução

Executado: `node docs/validation/skills_balance_v1/balance-model.mjs`.
Exit code: **0**; resultado `ANALYTICAL_MODEL_CHECKS_PASS`.
Saída: [balance-results.json](balance-results.json). Modelo: [balance-model.mjs](balance-model.mjs).
O JSON guarda hashes SHA-256 das fontes, valores e sequências completas de compra.

O modelo lê catálogo/tiers/perfil de preços, verifica contagens e calcula cenários determinísticos.
Valores de ataques/ranks vêm do addendum; parâmetros de recuperação, regeneração e caps do código.
Ele não executa as classes C#, física, IA, defesa/crit/status, animação, RNG de acerto, quests ou Unity.
Comparações de dano são **potência bruta por uso/recurso/recarga**, não DPS final de personagem ou TTK.

A auditoria anterior mostrou que parte das skills nem recebe rank ou avatar correto. Para balancear o
design pretendido, separam-se aqui **baseline de código**, **curvas previstas** e **hipóteses propostas**.
Não chamar resultado de modelo de captura do jogo nem concluir sobre todos os canais econômicos.

## 2. Pontos e capstones: híbridos são possíveis

Contagem extraída: 66 nós, 31 ativas e 35 passivas/capstones. O modelo construiu, para cada árvore,
uma sequência legal de 27 compras/ranks que chega ao primeiro rank de capstone: respeita tiers,
pré-requisitos e teto dinâmico. Não otimiza build nem exige que todo nó comprado esteja funcional.
Escolha de variante, quando necessária, é pressuposta válida; o menu real ainda tem sua lacuna.

| Investimento | Custo estrutural |
|---|---:|
| Abrir T5 de uma árvore | 26 |
| Comprar capstone R1 | 27 total |
| Duas capstones R1 | 54 total, cabe em 55 |
| Duas capstones R3 | Pelo menos 58, não cabe em 55 |

Veredito: não subir gates automaticamente. “Duas capstones iniciais” não é “duas árvores masterizadas”.
O risco é concentrar todo o efeito no primeiro rank, fazendo os outros dois compras inferiores. Precisam
de delta real, especialmente se variantes tiverem muitos bônus simultâneos.

| Nível | Pontos-base por nível | XP seguinte da fórmula analítica |
|---:|---:|---:|
| 1 | 0 | 60 |
| 5 | 2 | 671 |
| 10 | 5 | 1.897 |
| 25 | 12 | 7.500 |
| 50 | 25 | 21.213 |
| 75 | 37 | 38.971 |
| 100 | 50 | Cap; fórmula retorna 60.000, sem avanço normal |

XP calculado por `round(60*N^1.5)` em dupla precisão; não teste bit a bit de Mathf. Pontos de atos são
adicionais, conforme marcos realmente completados. Não há conclusão sobre minutos para subir nível.

## 3. MP: sustain muda muito com bônus flat

Default ManaManager: 2 MP por tick; GameTimeManager usa intervalo 1 s, condicionado ao relógio ativo.
`magic_quick_channel` adiciona 1 MP por rank. Faísca custa 10 MP/3 s: demanda média 3,333 MP/s.

| Rank quick_channel | Regen default + bônus | Saldo usando só Faísca na recarga |
|---:|---:|---:|
| 0 | 2 MP/s | -1,333 MP/s |
| 1 | 3 MP/s | -0,333 MP/s |
| 2 | 4 MP/s | +0,667 MP/s |
| 3 | 5 MP/s | +1,667 MP/s |
| 5 | 7 MP/s | +3,667 MP/s |

Cinco pontos permitem, pelas dependências do catálogo, mana_well R1 + quick_channel R2 + arcane_edge R1
+ fire_spark R1. A regen pode sustentar a magia básica cedo. Limite: usa default, ticks regulares,
um único spell e acerto/cast não altera o custo modelado; não significa mana infinita para toda rotação.

A direção prevê Fluxo Lento como +5–25% da regen, não +1–5 MP/s. Aplicada à mesma base 2, daria
2,1–2,5 MP/s, diferença grande. Recomendação: decidir se magia básica sustentável é intencional;
testar escala percentual primeiro, sem tratar sustain isolado como necessariamente errado.

## 4. Recuperação: esperar produz recurso

Limites em regime contínuo, com HP faltante, sem interrupção/overheal e usando na recarga:

| Capacidade atual | Capacidade média |
|---|---:|
| Kit: 30 HP/45 s | 40 HP/min |
| Último Fôlego: 40 HP/90 s | 26,67 HP/min |
| Campo: 15 HP/60 s | 15 HP/min |
| Três anteriores, três slots | 81,67 HP/min |
| Quatro cópias de Último Fôlego, cooldown independente | 106,67 HP/min |
| Instinto: 50 stamina/30 s | 100 stamina/min |
| Campo: 15 MP/60 s | 15 MP/min |

Não são números exatos do primeiro minuto: começar com todas prontas produz burst diferente.
Recarga única remove multiplicação por slots, mas não limita a recuperação infinita por espera segura.
Slots são contrapeso, não solução completa: pode haver troca fora de combate. Não aumentar cooldown
indefinidamente como único tratamento. Toda mudança de custo de Kit/Instinto precisa de aprovação,
pois o addendum permite essas restaurações sem item.

## 5. Ofensivas: eficiência e contexto

Potência bruta de R1 e R3 previsto pelo addendum, sem multiplicadores/passivas/defesa/status:

| Ação | Dano R1 → R3 | Custo | Dano/recurso R1 → R3 | Dano/segundo de recarga R1 → R3 |
|---|---:|---:|---:|---:|
| Corte secundário | 8 → 12 | 10 STA | 0,80 → 1,20 | 3,20 → 4,80 |
| Giratório, por alvo | 10 → 14 | 22 STA | 0,45 → 0,64 | 1,67 → 2,33 |
| Perfurante, primeiro alvo | 12 → 18 | 18 STA | 0,67 → 1,00 | 1,71 → 2,57 |
| Leque, uma flecha | 8 → 12 | 24 STA pelo leque | 0,33 → 0,50 | 1,00 → 1,50 |
| Faísca | 12 → 16 | 10 MP | 1,20 → 1,60 | 4,00 → 5,33 |
| Chama Breve | 8 → 12 | 8 MP | 1,00 → 1,50 | 3,20 → 4,80 |

Faísca tem mais dano por MP, maior alcance e maior potência por recarga que Chama no baseline;
Chama custa menos por disparo e retorna antes. Não é dominância estrita em todos os casos, mas falta
diferenciação suficiente quando ambas são bolts sem Burn funcional. Cone curto é proposta por função,
não motivo para aumentar dano sem calcular área/risco.

Leque pode causar 8 ou 24 de dano a um alvo no R1, dependendo de trajetória/hitbox e política de colisão.
Não equilibrar supondo três alvos se todos os projéteis puderem atingir um chefe grande. Perfurante com
quatro alvos a 12 causa 48 sem queda, mas 35,424 com redução multiplicativa de 20% por alvo; diferença
de 35,5% sobre a versão reduzida. A definição da geometria precede o ajuste de números.

### Marcador de Presa precisa pagar seu tempo de uso

Modelo: alvo disponível por horizonte total T, incluindo cast t; jogador causaria D dano/s; bônus b.
Marcar é favorável em dano quando `(1+b)*D*(T-t) > D*T`, isto é, `t < b*T/(1+b)`.

| Rank | Bônus e horizonte comparável | Tempo máximo perdido antes de ficar pior |
|---:|---|---:|
| 1 | 5%, 8 s | 0,381 s |
| 2 | 8%, 10 s | 0,741 s |
| 3 | 12%, 12 s | 1,286 s |

É cenário conservador sem aliados: se o alvo viver a duração inteira **após** o cast, o limite vira b*T
(0,4/0,8/1,44 s). Atributos/status/aliados mudam a conta. Recomendação: marca de baixo compromisso
temporal ou integrada ao primeiro tiro; não adicionar um cast lento e depois exigir marca em toda luta.

## 6. Craft: ganho saturado, não só cap seguro

Código: Mãos Ágeis 10%/rank + Foco de Bancada 5%/rank + Mestre Artesão 15%/rank; cap de redução 75%.
Multiplicador de produção limitada por tempo = `1/(1-redução)`.

| Rank em cada uma das três fontes | Redução bruta | Redução aplicada | Produção por tempo |
|---:|---:|---:|---:|
| 1 | 30% | 30% | 1,43× |
| 2 | 60% | 60% | 2,50× |
| 3 | 90% | 75% | 4× |
| 5 | 150% | 75% | 4× |

Linhas são comparações de agregados, não builds de três pontos com capstone disponível. O JSON inclui
sequência de 29 pontos que chega ao capstone R3 com fast_hands R5 e station_focus R5: essas duas já
somam 75% antes do capstone. Logo a dimensão tempo pode não melhorar mesmo no primeiro rank de Mestre
Artesão; reparo é outro efeito e não foi confundido com perda total de benefício.

Se materiais/estoque forem gargalo, 4× velocidade não é 4× lucro diário. Não aumentar cap para resolver
rank desperdiçado: distribuir benefícios e recuperar identidade do proc Thoren 1×/dia exige menos inflação.

## 7. Mercado: cenário de arbitragem que deve ser rejeitado

PricingProfile default: compra 1,30 BV, venda direta 0,90 BV. Hipótese: desconto b na compra e bônus b
na venda, mesma qualidade/raridade comum, sem rounding/reputação/eventos. Não é hook já ligado no jogo.

| Bônus simétrico | Comprar | Vender | Lucro da volta completa por BV |
|---:|---:|---:|---:|
| 0% | 1,300 | 0,900 | -0,400 |
| 10% | 1,170 | 0,990 | -0,180 |
| 15% | 1,105 | 1,035 | -0,070 |
| 20% | 1,040 | 1,080 | +0,040 |
| 25% | 0,975 | 1,125 | +0,150 |

Break-even b = `(1,30-0,90)/(1,30+0,90)` = **18,18%**. Portanto ligar 5%/rank nos dois lados até R5
seria inseguro neste cenário. 15% não é aprovação global: reputação, preço de estoque, rounding e
eventos podem fechar os 0,07 BV restantes. Validar preço final, ciclos de craft e canais reais.

## 8. Capstones e utilidade: riscos adicionais

Anya/Senya exigem gastar 35% do MP máximo em seis segundos. Pools comparáveis de 30/60/100/200 exigem
10,5/21/35/70 MP; casts de 10 MP exigiriam 2/3/4/7 usos. O pool maior torna o trigger mais difícil se
o repertório mantiver o mesmo gasto. Não concluir que aumentar MaxMP sempre melhora a capstone.
O timing precisa caber na janela; sete casts de uma magia com 3 s de recarga não cabem em seis segundos.

O addendum mistura “janela deslizante” com “reset após seis segundos sem gasto”: não são equivalentes.
Exemplo de gasto 25 nos instantes 0/5/10, threshold 70: janela real nunca soma 75; acumulador por
inatividade soma. Futura spec deve implementar a janela deslizante de seis segundos declarada, ou
registrar mudança; evitar autoalimentação por gasto descontado e re-trigger da própria Semente.

Contenção de Circuito proposta reutiliza envelope de Selo Rúnico: duração 3–5/recarga 28 implica
disponibilidade máxima de **10,7–17,9%**; gasto médio 36/28 = **1,286 MP/s**. Com regen default 2,
o uso isolado pode ser sustentável. Custo de MP não é limite de tentativas; cast, exposição, duração,
recarga compartilhada e alvos elegíveis são os contrapesos. Não declarar balanceada sem cenário de hazard.

## 9. Veredito e pendências

**Demonstrado analiticamente:** dois capstones R1 cabem; flat MP sustenta Faísca no cenário padrão;
craft satura; bônus simétrico alto permite arbitragem no perfil modelado; recuperação livre produz
recursos com espera; dano em área depende fortemente de geometria. Estes resultados justificam decisões,
não uma aplicação automática de nerfs.

**Não demonstrado:** TTK/risco final, distribuição de uso, valor por minuto real em cada andar, todos
os ciclos econômicos, força relativa das variantes de capstone com equipamento, UI e aceitação humana.
Não há base para declarar uma skill “15% melhor” ou toda build equilibrada sem considerar contexto.

Unity validation: **NOT RUN** — nenhuma alteração de runtime/assets; nenhum comando Unity tentado.
Residual risk: Unity compile not validated locally. Play Mode/telemetria: **NOT RUN**.
Evidência é estática/analítica. Nenhum valor de balanceamento do jogo foi alterado.
