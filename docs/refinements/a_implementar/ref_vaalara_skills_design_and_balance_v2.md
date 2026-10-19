---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Vaalara: revisão de sentido das habilidades e equilíbrio — v2

Derivação em planejamento: [lote SDD com specs, planos e tasks](../../../.specs/features_futuras/skills_sdd_v1/README.md).
Os rascunhos não tornam estas propostas automaticamente aprovadas ou implementadas.

> 2026-09-09, data local da sessão. Proposta de design, não implementação autorizada.
> Complementa e qualifica o [refinamento técnico v1](ref_skills_capabilities_gameplay_ui_pixelart_v1.md).
> Evidência numérica: [análise de equilíbrio](../../validation/skills_balance_v1/BALANCE_REVIEW.md).
> Fontes do mundo e decisões humanas prevalecem sobre sugestões deste documento.

## 1. O jogo que estas habilidades precisam servir

Cindar's Hope é um RPG de ação e vida rural em **Dornécia, no mundo de Vaalara**. A superfície oferece
rotina pastoral; o subsolo revela horror, ruínas e consequências antigas. A fazenda prepara expedições,
a cidade oferece relações, conhecimento e serviços, e a caverna fornece risco e materiais raros.
Uma build interessante modifica a maneira de participar desses ciclos, sem eliminar a necessidade deles.

A inspiração em D&D está explicitada na direção de magia: projétil arcano, cone de fogo, raio de gelo,
marca, barreira, cura e purificação como fantasias reconhecíveis. O próprio projeto manda **não copiar
nomes, texto, progressão, valores ou funcionamento exato**. Não há classe fixa: atributos, equipamento
e investimento nas cinco árvores formam o personagem. Não introduzir turnos, d20, escolas divinas ou
spell slots de descanso apenas para aproximar o jogo de D&D.

O que vale transportar dessa inspiração: preparação antes de entrar no perigo, ferramentas com usos
distintos, oportunidades de resolver um problema de várias formas e escolhas com custo. No combate de
ação, isso se traduz em posição, direção, tempo de preparação, interrupção, postura e recursos reais.

### O mundo precisa alterar o design, não apenas os nomes

| Elemento canônico | Consequência para habilidades |
|---|---|
| Thandra | Cultivo e rotina rural podem ter identidade própria, sem depender de milagre de Anya |
| Anya e Cindar Nymiriana | Cura, águas, memória e esperança; progressão narrativa gradual, sem resolver cedo o mistério de Anya |
| Fonte/Água Viva | Recurso/serviço raro e não spammável; habilidade comum não substitui Fonte, respec ou ressurreição |
| Fruto Mana | Raríssimo, árvore consciente, não crop comercial; **não é o MP gasto em cada magia** |
| Kanthor/Kaand | Proteção/ordem contra conflito/fúria; variantes exigem condutas e contrapartidas diferentes |
| Alihana/Senya/Nyx | Três luas com identidade de descoberta, magia/caos e noite/segredo; não multiplicadores universais acumuláveis |
| Telisandra | Resiliência e retirada na natureza; a capstone favorece sair vivo, não vencer automaticamente |
| Thoren | Produção/ofício e equipamento; qualidade e utilidade, não ouro infinito |
| Bromécia | Engenharia, irrigação e armazenamento avançados fazem sentido; exigem custo ou ambiguidade |
| Elyndor | Portais são progressão perigosa; não justificar teleporte livre atravessando a caverna desde cedo |
| Finan/Merithus | Achados/sorte e contratos/comércio são papéis diferentes; não resumir ambos a +ouro de monstro |

Deus no nome é identidade, não obrigação de culto ou classe. Magia comum de cura pode evocar Anya:
a raridade pertence aos milagres/recursos e ao arco narrativo, não a uma proibição de toda cura cotidiana.

## 2. Correções à revisão anterior após leitura do cânone

O primeiro refinamento identificou corretamente falhas de execução, mas algumas propostas de design
foram mais fortes do que a leitura das fontes permitia. Corrigir explicitamente:

1. **Instinto de Sobrevivência:** o addendum prevê *revelação + restauração*. A falta de revelação é
   uma lacuna; restaurar stamina não é, por si só, erro. Retirar a restauração é opção de rebalanceamento.
2. **Kit de Emergência:** o addendum permite cura flat sem custo de item, fora de combate. Exigir
   consumível é uma proposta nova, não a recuperação automática de uma regra já aprovada.
3. **Mochila Ordenada:** a organização básica deve ser boa para todos; entretanto, o cânone admite
   depósito auto-organizável bromeciano. Não aposentar o nó só por parecer genérico ou estar vazio.
   Verificar primeiro se existe papel de logística avançada que não cobre pontos por conforto básico.
4. **Nuvem Tóxica:** a direção promete área por tick, mas o addendum numérico descreve três projéteis.
   Há conflito de design entre fontes. Recomendo área persistente por diferenciação, mas é alteração
   explícita do contrato numérico, não correção inequívoca de um único texto.
5. **Dormentes:** foram autorizadas em decisão FABLE v3. Sua implementação incompleta não prova que
   o conceito deve ser removido. A política de venda/aviso ainda precisa ser reconciliada.
6. **Raridade de Anya:** não tornar Água Viva consumo obrigatório de toda magia de cura/purificação.
   A direção já permite magias com esse sabor usando MP comum.

O contexto confirma a utilidade da maioria das famílias existentes. Recomendo **conservar o repertório
enquanto se avaliam suas funções**, sem aprovar cortes, novos IDs ou aumento de contagem nesta revisão.

## 3. Critério para manter, refinar ou propor uma habilidade

Uma proposta passa por cinco perguntas: oferece decisão diferente? cabe no mundo? tem oportunidade de
uso suficiente? deixa uma alternativa razoável para outra build? seu ganho compensa ponto/slot/tempo sem
dominar as alternativas em todos os contextos?

Resultado possível: **manter**, **refinar**, **restaurar algo já previsto**, **experimentar uma capacidade
nova** ou **adiar por falta de cenário**. “Sem executor” não é categoria de avaliação de design.
Não forçar todo nó a ser espetáculo: uma passiva modesta pode sustentar uma build, desde que explique
seu efeito e não seja gasto obrigatório em algo inerte.

## 4. Revisão dirigida das habilidades

| Habilidade/família | Parecer | Razão e mecânica recomendada | Contrapeso/alternativa |
|---|---|---|---|
| Corte Amplo/Giratório | Manter | Controle de proximidade contra grupos; arco/círculo claramente visível | Não superar golpe dedicado contra alvo único; dano secundário/limite autorados |
| Arrancada de Combate, Avanço de Aço, Ataque Saltante | Refinar, sem fundir agora | Arrancada reposiciona; Avanço pune recuperação em linha; Saltante compromete a chegada | Diferenciar geometria e recuperação antes de aumentar dano; paredes e áreas perigosas continuam relevantes |
| Grito de Desafio | Refinar | Provocação abre janela de aproximação/retirada ou interrompe comportamento elegível | Contra chefe não sequestra IA; não somar taunt, dano alto e knockback alto sem custo |
| Contra-Ataque | Restaurar direção existente | Após bloqueio perfeito, um próximo ataque dentro de 1,5 s recebe benefício condicionado | Janela expira; um consumo; crítico não retriggera o próprio benefício |
| Marcador de Presa | Manter e fechar | Um alvo, duração limitada, bônus do jogador e aliados invocados quando existirem | Custo de slot/tempo; não deve ser obrigatório para todo tiro contra inimigo fraco |
| Disparo Carregado | Manter | Preparar um tiro forte expõe o arqueiro; resolve abertura, não spam | Interrupção/deslocamento, custo e recuperação; tiro rápido continua útil |
| Linha Perfurante e Leque | Manter ambos | Linha premia alinhamento; leque cobertura angular e distância curta | Definir se múltiplas flechas podem acertar o mesmo alvo; não presumir que três flechas são sempre três alvos |
| Faísca de Fogo e Chama Breve | Refinar formas | Faísca é tiro a distância; proposta para Chama é cone curto com risco de aproximação | Evitar dois projéteis equivalentes diferenciados só por dano/custo; cone não deve somar toda a área em alvo único |
| Laço/Rajada de Gelo | Manter | Controle leve e previsível; diferença entre alvo preciso e região angular | Resistência e refresh limitado; sem stun permanente por aplicação repetida |
| Nuvem Tóxica | Recomendar área por ticks | Permite controlar chão e antecipar movimento; dá função diferente a raio/gelo/fogo | Dano total depende de permanência; não aplicar dano integral ao tocar a borda |
| Corrente Relâmpago | Manter cadeia real | Premia proximidade entre inimigos e escolhas de primeiro alvo | Alvos distintos e limite de saltos; não substituir automaticamente Linha Perfurante |
| Selo/Guarda Elemental | Manter | Antecipação de dano, alternativa a dodge e cura posterior | Proteção curta/parcial, sem retirar o risco de posicionamento |
| Instinto de Sobrevivência | Manter conceito; rebalancear pacote | Revelação temporária sobre objetos já gerados; stamina só se deliberadamente parte do pacote | Não revelar baú secreto garantido, rerrolar loot ou virar bateria ilimitada universal |
| Campo Seguro/Descanso | Refinar | Pausa vulnerável fora de combate, em zona/condição válida | Sem cura e MP gratuitos ilimitados apenas esperando; comida, tempo e retorno à cidade continuam úteis |
| Kit de Emergência/Último Fôlego | Manter, diferenciar | Kit recupera após perigo; Último Fôlego atende urgência; Telisandra é capstone por run separada | Não confundir nomes ou autorizar três curas gratuitas equivalentes |
| Olho de Material/Salvage/Senso de Mercado | Manter papéis, corrigir rotas | Coleta, reaproveitamento e negociação são decisões econômicas diferentes | Sem duplicação de output/reembolso e sem arbitragem de compra/venda |
| Mochila Ordenada | Manter em revisão, sem vender efeito vazio | Investigar preparo/abastecimento de estação ou logística bromeciana | Ordenar, buscar e transferir itens básicos deve funcionar sem comprar talento |
| Irrigador/Engenharia Bromeciana | Manter | Preparação agrícola libera tempo de expedição; infraestrutura é recompensa de exploração/ofício | Custo de construção/manutenção e limite de área; não tornar regador inicial inútil |
| Bomba Improvisada | Manter | Consumível explosivo de preparo, útil contra grupos/postura | Não competir como magia mana-only; consumo real e explosão por alvo única |
| Capstones das cinco árvores | Manter identidade canônica | Recompensar jogar como protetor, agressor, caçador, arcano, sobrevivente ou artesão | Um trigger não realimenta a si mesmo; once/run/day preservado no save; híbrido não exige culto |

## 5. Capacidades propostas: poucas e com origem declarada

### A. Contra-Ataque — restauração, não habilidade inédita

Já existe em PLAYER_SKILL_TREES_DIRECTION: Melee T3, três ranks, próximo ataque em 1,5 s após block
perfeito ganha +20/35/50 pontos percentuais de chance crítica. É uma resposta ofensiva que depende do
domínio do jogador, apropriada ao guerreiro sem classe fixa.

Não ocupa novo slot: modifica o próximo ataque. Consumir uma vez, impedir cadeia de triggers no mesmo hit
e aplicar o cap crítico vigente. Comparável numérico: a própria direção, não número inventado. A análise
de burst deve incluir arma, CritDamage e Kanthor/Kaand antes de declarar os três ranks equilibrados.

### B. Purificar Mácula — priorizar capacidade já desenhada

`purify_taint` já está na direção de magia: 24 MP, 22 s, cast 0,60 s, Magic T3, pré-requisito de
Água/Natureza ou cura. Recomendo avaliar sua disponibilidade real antes de criar outro poder semelhante.
Funciona como ferramenta de preparação/reação a Poison/Corruption leve, sem remover maldição de chefe
ou resolver ritual narrativo.

Justificativa: amplia solução de problemas além de dano/cura, conecta águas/esperança ao mundo e deixa
antídoto/equipamento resistentes como alternativas. Não exige Fruto Mana nem Água Viva por padrão.
O slot de purificação compete com dano/controle; não torná-la compulsória para atravessar um andar.

### C. Contenção de Circuito — capacidade ambiental nova, preferencialmente variante de selo

Proposta nova neste recorte, não novo nó automaticamente: permitir ao Selo de Silêncio Rúnico atuar
sobre **um emissor rúnico ambiental elegível**, suspendendo seu próximo pulso durante a duração do selo.
A mesma execução escolhe emissor ou zona anti-caster; não recebe os dois benefícios ao mesmo tempo.
Usa o mesmo slot/recarga, sem atalho para duplicar uso entre variantes.

Por que existe: interagir com engenharia perigosa de Bromécia oferece solução tática diferente de
receber menos dano ou curar depois. Preserva rotas alternativas por timing/dodge; não abre portal de
Elyndor, não purifica Pedra Negra Cultista e não desativa mecânica central de chefe.

Envelope inicial **derivado do selo existente**: T4 e pré-requisitos atuais, 36 MP, 28 s de cooldown,
0,75 s de cast e 3–5 s de duração. Não reduzir preço por parecer utilidade. O emissor volta a operar;
posição, loot, run seed e snapshot do nível não são rerrolados. Apresentação mostra circuito contido e
tempo restante, sem apagar permanentemente o perigo.

Decisão: **candidata condicional**, não aprovada para implementação. Primeiro provar em cenário existente
que há emissores recorrentes e que o selo acrescenta uma decisão. Se só servir a um puzzle isolado,
implementar interação daquele cenário em vez de cobrar skill/slot. Busca realizada nas direções de magia,
skills e catálogo não encontrou esse modo ambiental; não é prova de ausência em todo projeto.

Não proponho agora teleporte livre, roubo de vida passivo universal, multiplicador global de loot,
respec portátil ou uma nova árvore. Todos eliminariam limites importantes ou duplicariam sistemas.

## 6. Regras de equilíbrio recomendadas

### Recursos e risco

MP mede capacidade arcana, stamina mede esforço imediato e fome/cansaço desgaste de expedição. Skill que
restaura um não deveria limpar os outros por conveniência. Bônus de regeneração devem preservar a
distinção entre uma magia básica sustentável e uma sequência inteira gratuita de controle/cura/dano.

A sustentabilidade de Faísca isolada pode ser uma escolha válida; não considero obrigatório nerfá-la.
O problema é ocorrer cedo por +MP flat sem decisão, enquanto a direção prevê regeneração lenta percentual.
Recomendo testar primeiro a escala percentual canônica e comparar a mesma build/gear, mantendo números
de dano. Se o objetivo humano for magia básica sustentável, registrar isso e limitar o restante do pacote.

Recuperação gratuita por cooldown equivale a recurso infinito quando esperar é seguro. Não resolver
apenas aumentando cooldown. Preferência de experimento: benefício limitado por encontro/run ou custo
de provisão, mantendo a recuperação cotidiana útil; alterar o kit sem item e o restore do Instinto exige
decisão explícita. Nenhuma opção está implementada ou escolhida como cânone por este documento.

### Acúmulo e valor de rank

Agregados precisam mostrar ganho efetivo depois do cap. Hoje +50% tempo de craft e +25% já atingem o
teto de 75%; capstone que adiciona mais redução não melhora essa dimensão. Preferir reduzir redundância
entre fontes e reservar à Forja Viva o proc de ofício já previsto, em vez de simplesmente elevar o cap.

Descontos de fontes diferentes exigem uma política única. Exemplo analítico: -25% e -45% somados deixam
30% do custo; multiplicados deixam 41,25%. Ambos são finitos, mas têm impacto bem diferente. Não misturar
as duas convenções entre UI e execução nem escolher a mais forte sem avaliar kit completo.

Efeitos de status têm limites por alvo e resistência. Duração/cooldown fornece teto de disponibilidade,
não prova ausência de stunlock: várias skills/aliados podem alternar controle. Testar a composição inteira.

### Pontos, slots e híbridos

Com 26 pontos para abrir T5 e +1 para primeiro rank, duas capstones cabem em 54 dos ~55 pontos.
Isso não viola automaticamente “não masterizar duas árvores”: dois ranks iniciais não são duas árvores
completas. Recomendo **manter possibilidade de híbrido** e autorar ganhos por rank; se todo o poder estiver
no rank 1, o híbrido pode ficar superior sem pagar especialização. Duas capstones rank 3 custam pelo menos
58, fora do teto de 55. Não subir gates por impulso para eliminar builds criativas.

Quatro slots são custo de oportunidade real, mas duplicação/reatribuição não pode multiplicar recarga.
Passive de qualidade de vida, técnica de arma e magia aprendida não precisam virar três compras para
o mesmo efeito. Reconciliar node IDs/action IDs/spells antes de materializar novas entradas.

### Economia e rotina

Lucro legítimo de produção remunera material, trabalho, tempo e infraestrutura. Bônus de compra e venda
simétricos se realimentam; preço final deve ser verificado em todos os canais/reputações/eventos, incluindo
rounding e transformações. Skill de mercado não recebe automaticamente 5% por rank nos dois lados.

Thoren 1×/dia e Telisandra 1×/run precisam conservar consumo ao carregar, mudar de cena e fazer respec.
Não colocar cura de campo em nível equivalente a Água Viva rara nem transformar crop rara em Fruto Mana
comum. Buffs de luas devem alterar contexto, não acumular todas as vantagens na mesma build o tempo todo.

## 7. Validação de equilíbrio executada e seus limites

Foi executado um modelo analítico reproduzível, com hashes das fontes e sequências legais de compra.
Ele verifica contagem do catálogo, alcançabilidade estrutural de capstones, limites de regeneração,
eficiência por custo/recarga, recuperação, saturação de craft e risco de arbitragem. Resultados no relatório
associado. A checagem do modelo passou; **o equilíbrio geral do jogo não está aprovado**.

| Caso | Veredito desta revisão |
|---|---|
| Papéis das cinco árvores e relação com Vaalara | Coerentes; preservar |
| Duplicações de forma/descrição | Refino necessário, não corte automático |
| Pontos para dois capstones R1 | Estruturalmente possível; avaliar front-loading, não proibir por princípio |
| Regeneração MP flat e curas gratuitas | Risco alto de esvaziar preparação; decisão/teste necessários |
| Craft time sobreposto | Saturação demonstrada; ranks precisam de ganho real |
| Mercado com bônus simétrico alto | Reprovado no cenário econômico modelado |
| Contenção ambiental | Envelope derivado, disponibilidade limitada; utilidade e integração ainda não demonstradas |
| Balanceamento de combate completo | Pendente de confrontos/telemetria após correção do wiring |

### Protocolo de aceitação das propostas

Comparar a mesma cena, seed, equipamento, inimigos e habilidade do jogador, variando apenas a proposta.
Usar alvo único, três alvos alinhados, três dispersos, elite e chefe com janela; evitar medir tiro que
sai do bootstrap em vez do avatar. Registrar tempo de eliminação, dano recebido, recursos líquidos,
controle efetivo, ações canceladas e número de decisões úteis. Variação de acerto/dodge precisa aparecer.

Para produção: mesma estação, receita, qualidade, estoque e tempo disponível; medir custo/output por
job e ganho por dia com gargalo real. Para exploração: mesma rota, sem regenerar a cave, medir provisões
gastas e se a alternativa sem a skill continua viável. Para novas capacidades, provar uso recorrente
antes de cobrar ponto permanente; não transformar uma hipótese de lore em nó obrigatório.

Níveis 1/5/10/25/50/75/100 e marcos 5/11/18/26 pontos são amostras de progressão. Nível não equivale a
andar de cave nem a tempo real garantido. Tempo para subir depende de XP de combate/quests/farm e de
comportamento observado; não fabricar “nível 10 na primeira semana” sem medir essas fontes.

## 8. Fontes lidas e próximos artefatos

- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`: recorte, deuses, Mana, Fonte, Bromécia/Elyndor.
- `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`: sem classe fixa, atributos/recursos.
- `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`: identidades, habilidades e capstones.
- `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`: inspiração D&D, utilidades e formas.
- `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md`: custos, ranks, triggers e divergências.
- `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` e ADR-0010: decisões humanas vinculantes.
- `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`: contextos/janelas de combate.
- Código de catálogo/executores, ManaManager, GameTimeManager, SkillTierRules, DerivedFollowupFormulas,
  PricingProfile/PricingService e providers examinados na auditoria técnica.

Não usei regras externas de uma edição de D&D para sobrepor o mundo do usuário. A inspiração foi lida
na fonte do próprio projeto. Há documentação antiga de combate por turnos que conflita com as direções
de ação; não foi adotada como contrato de execução nem corrigida silenciosamente.

A próxima materialização deve escolher propostas aprovadas e gerar specs com contratos completos,
mantendo esta revisão como proposta. Nenhum nó foi adicionado, retirado ou alterado no jogo nesta tarefa.

Orquestração: Codex Spark recebeu as fontes de lore, árvores e addendum integralmente para uma revisão
delimitada de sentido/duplicação. O principal confrontou suas sugestões com a direção de magia, recusou
exigir recurso raro para utilidade comum sem justificativa e executou/revisou as contas separadamente.
