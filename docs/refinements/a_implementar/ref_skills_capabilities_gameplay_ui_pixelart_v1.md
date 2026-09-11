---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Habilidades que mudam a forma de jogar — mecânica, progressão e UI pixel art

> Versão 1 — 2026-09-09. Proposta para revisão humana, não spec aprovada nem autorização de implementação.
> Base: [auditoria do código, dados e wiring](../../validation/SKILLS_CAPABILITIES_AUDIT_2026_09_09.md).
> Redação inicial delegada ao Codex Spark; revisão, correções e consolidação pelo agente principal.
> Revisão posterior de mundo/design: [Vaalara e equilíbrio v2](ref_vaalara_skills_design_and_balance_v2.md).
> A v2 qualifica as propostas abaixo após leitura do cânone e do addendum; nenhuma delas altera regra aceita.

## 1. Diagnóstico e objetivo

O jogo já possui a maior parte das peças de um sistema de habilidades, mas elas ainda não formam uma
experiência coerente. Há 66 nós no catálogo vivo: 35 passivos/capstones e 31 ativas. Pelo menos nove
passivos não chegam a um consumidor de gameplay. Seis ativas retornam sucesso sem efeito; uma não tem
mapeamento. Parte das demais altera estado, porém faz algo diferente do que seu nome e descrição prometem.

A interface amplia essa distância: o menu instalado é uma lista IMGUI, não permite aumentar ranks nem
escolher variantes de capstone, e o HUD de slots Canvas não está ligado a controles/ícones nessa rota.
Problemas de origem do caster, chance de status e saldo após respec podem fazer até capacidades com
implementação parecerem inúteis ou impedir que sejam usadas corretamente.

O objetivo é que cada ponto gasto produza uma consequência compreensível e verificável. O jogador deve
saber **o que aprendeu, quando usar, por que falhou, o que mudou no mundo e o que ganhará no próximo rank**.
A arte deve tornar essas diferenças legíveis. Uma moldura bonita não resolve uma ação sem efeito;
um efeito interno correto também não basta se não houver sinal perceptível ao jogador.

## 2. Decisões preservadas e propostas que exigem reconciliação

Preservar as cinco árvores Melee/Ranged/Magic/Survival/Crafting, quatro slots de ativas, capacidades base
de movimento/defesa separadas dos slots, nível máximo 100, um ponto por dois níveis e bônus por ato.
Manter tiers por investimento 0/5/11/18/26, rank cap dinâmico e respec na Fonte de Anya com confirmação,
primeiro gratuito, seguintes conforme política canônica, revalidação de equipamento e proteção dos saves.
Fontes: ADR-0010 e `docs/game_rules/skill_tree_rules.md`. Rótulos de input devem vir da rota vigente;
não copiar atalhos de documentos antigos conflitantes.

| Tema | Proposta | Situação |
|---|---|---|
| Recarga | Cooldown por action ID, compartilhado por quaisquer slots que a mostrem | Altera a regra aceita por slot; requer decisão registrada |
| Duplicação | Uma action ocupa no máximo um slot; atribuir em outro move a mesma ação | Proposta de UX e integridade a aprovar junto da recarga |
| Capstone | Teto específico de três ranks, limitado também pela elegibilidade da árvore | Reconciliar menção canônica a três ranks com runtime que permite cinco |
| Nó incompleto | Não permitir comprar/aumentar rank/equipar quando efeito não estiver pronto | Proposta de disponibilidade com migração e análise de descendentes |
| Tempo de cooldown | Seguir o relógio de gameplay: congela somente quando o jogo estiver realmente pausado | Explicitar política; hoje modal interrompe o decremento implicitamente |
| Novas capacidades | Fechar identidades existentes antes de adicionar novos nós | Direção proposta para esta revisão |

Essas propostas não modificam a regra aceita neste documento. A futura spec deve registrar a decisão
aplicável e incorporar seus contratos; não deve interpretar este refinamento como override silencioso.

## 3. O que vale como habilidade pronta

Cada nó terá uma ficha de contrato com estes campos obrigatórios:

| Campo | Pergunta que precisa responder |
|---|---|
| Identidade estável | Qual node ID, action ID e effect ID? Qual save existente os utiliza? |
| Categoria e função | Ability base, ativa, passiva ou capstone? Que decisão nova oferece? |
| Gatilho | Pressionar, segurar/soltar, acertar, bloquear, colher, terminar job? |
| Pré-condições | Arma/mão, alvo, distância, linha de visão, contexto, recurso, item/carga, tier? |
| Transformação | Qual HP/status/posição/solo/item/custo muda, em qual consumidor real? |
| Números | Valor por rank, unidade, fonte SO, duração, intervalo, limite e política de acúmulo |
| Falhas | O que bloqueia? Quando debita? Quando reembolsa? Quando inicia recarga? |
| Apresentação | Pose/telegraph, efeito de impacto, indicador de duração, som e motivo de falha |
| Persistência | O que permanece após troca de cena/save/load/respec e o que é transitório? |
| Prova | Qual teste observa estado real e qual cenário demonstra efeito/arte no jogo? |

Passiva não precisa disparar um VFX a cada frame. Sua contribuição deve aparecer na ficha e na ação
afetada: custo menor, tempo menor, colheita adicional, defesa ou mobilidade contextual. “Hook registrado”,
“executor existe” e “toast apareceu” não são critérios suficientes de conclusão.

## 4. Prioridade zero: conectar a execução ao jogador e conservar estado

### 4.1. Avatar e contexto

O caster deve ser o corpo controlado pelo jogador, vinculado explicitamente pela composição da cena.
Manager de vitais/progressão pode viver no bootstrap; isso não o torna origem espacial da ação.
Reutilizar as referências existentes de avatar/PlayerController e interação. Proibir fallback silencioso
para `(0,0)` ou direção direita quando a habilidade depende de um avatar válido.

No boot e após transição, provar: caster é o avatar ativo; posição de emissão corresponde ao socket/origem
definida; direção acompanha mira; irrigação usa a interação do avatar; há uma autoridade de PlayerManager.
Se dependência faltar, a habilidade fica indisponível com diagnóstico, sem consumir custo ou fingir sucesso.

Avanços ofensivos usam `PlayerMovementDisplacementResolver` e a autoridade física já existente. Não fazer
teleporte concorrente em Update. Definir quando o hit ocorre: durante o percurso ou na chegada, com um
acerto por alvo por janela. Dois colliders do mesmo EnemyHealth não duplicam o dano.

### 4.2. Pontos, compra, respec e migração

Uma única autoridade deve conservar a relação **obtidos = gastos + disponíveis**, incluindo nível e atos.
Reutilizar PlayerProgressionManager/SkillTreeState e seus providers, corrigindo responsabilidade e transação;
não criar outro saldo independente. A compra valida e confirma ambas as projeções antes de publicar sucesso.
Falha intermediária não pode deixar nó comprado sem débito, nem saldo debitado sem nó.

Respec reembolsa investimento efetivo, preserva pontos de atos e contadores/custos canônicos. A próxima
compra, a transição e o load não podem sobrescrever o reembolso. Revalidar itens equipados e utilizáveis
contra tier relock, resolvendo diferença entre ID de conteúdo e ID da instância. Manter itens no inventário.

Migração de nó removido usa o mesmo saldo, com reembolso único por rank realmente comprado. Validar
idempotência: carregar duas vezes não gera dois reembolsos. Não remover IDs do catálogo nem reescrever
assets em massa sem inventário dos saves/aliases e uma spec própria de compatibilidade.

Ordem proposta no load: restaurar dados simples → aplicar migrações uma vez → validar conservação de
pontos/referências → recomputar efeitos/tiers → revalidar equipamento → projetar UI. Load não executa
respec. Respec é outra transação explícita: validar contexto/custo → confirmar reset e reembolso →
recomputar/revalidar → publicar resultado. Reutilizar versionamento/migrações existentes; só introduzir
marcador persistido adicional se necessário e justificado pela spec de save.

Cada nó alterado terá linha de compatibilidade: ID antigo, ID preservado/destino, ranks convertidos,
reembolso, tratamento dos slots e descendentes. Aliases não podem manter duas compras independentes do
mesmo poder. Critério adicional: load migrado → respec → save → dois loads preserva exatamente o saldo.

### 4.3. Custos, recarga e prontidão

Validar recursos, dependências e contexto antes de mutar. Custos devem ser declarados explicitamente;
uma bomba não pode consumir mana apenas porque seu DamageType é Toxic. Definir custo físico, mana,
consumível ou combinação por ação. Se a execução falhar antes de ser aceita, nenhum débito/recarga.

Um disparo aceito que erra o inimigo continua sendo um disparo: consome custo e cooldown. “Sem efeito
implementado” e “dependência ausente” são falhas de prontidão, não erros de mira. Para ações contextuais
como reparar e irrigar, validar objeto elegível antes de consumir. Essa distinção evita reembolso explorável.

Proposta de fluxo: avaliar prontidão → reservar custo quando necessário → confirmar execução real →
debitar uma vez → iniciar recarga e publicar apresentação. Cancelamento de carga antes da soltura usa
política própria, explícita. Após o commit, o mundo aplica impactos mesmo se o jogador abrir o menu,
conforme a política de pausa aprovada.

| Resultado | Débito | Recarga | Apresentação |
|---|---|---|---|
| Pré-condição/dependência ausente | Nenhum; liberar reserva | Não inicia | Motivo de bloqueio |
| Carga cancelada antes da soltura | Nenhum na proposta inicial; liberar reserva | Não inicia | Cancelamento sem impacto |
| Ação aceita, alvo errou/desviou | Custo autorado uma vez | Inicia | Cast/disparo real; sem falso hit |
| Ação contextual aceita | Custo autorado uma vez | Inicia | Reparo/rega/buff realmente aplicado |
| Falha técnica antes do commit | Nenhum; liberar reserva | Não inicia | Falha e diagnóstico |

Na proposta inicial, carga não possui custo de sustentação; introduzi-lo exige contrato adicional.
Depois de aceita a proposta de cooldown por action, a UI deriva a recarga do ID da ação e apenas a exibe
em cada slot. Saves legados com duplicatas são normalizados preservando o primeiro slot; não resetar
recarga viva ao mover a ação. A spec deve definir dados transitórios e comportamento ao carregar:
proposta é preservar recarga restante em save/load para impedir cura infinita por reload, com duração
simples por ID e migração compatível, sem expiração por tempo offline.

## 5. Revisão das cinco árvores e capacidades

### 5.1. Melee — posição, compromisso e defesa ativa

Identidade: escolher alcance, direção, janela e postura; cada golpe especial deve alterar como se aproxima
ou controla o inimigo. Bônus de duas armas/duas mãos só atuam no equipamento correspondente. Guarded Stance
deve comunicar exatamente o que protege; não prometer condição que o cálculo ignora.

| Capacidade existente | Refinamento proposto |
|---|---|
| Corte da Mão Secundária | Ataque curto condicionado à mão/arma compatível, arco visível e custo explícito |
| Corte Giratório | Limpeza ao redor com compromisso de recuperação; uma aplicação por alvo |
| Ataque Saltante | Aproximação com telegraph e chegada legível; não atravessa paredes nem implica invulnerabilidade sem contrato |
| Arrancada de Combate / Avanço de Aço | Diferenciar mobilidade ofensiva e golpe de entrada por janela/alcance/compromisso; evitar dois aliases cosméticos |
| Grito de Desafio | Definir provocação limitada ou assumir controle radial; se mantiver nome de desafio, IA deve responder de modo observável |
| Investida Quebra-Guarda | Pressão de postura, com sinal de stagger/ruptura e limites para chefes |
| Treino de Esquiva | Reduzir efetivamente custo/recarga na ability base; não ocupar slot |
| Capstone Kanthor/Kaand | Ler variante e aplicar somente seu contrato canônico; apresentar comparação antes de confirmar |

### 5.2. Ranged — preparação, linhas de tiro e reposicionamento

Disparo Carregado deve aceitar segurar/soltar, mostrar carga e aplicar a curva declarada. Cancelar antes da
soltura não deve disparar nem consumir a flecha; eventual custo de sustentação precisa ser explícito.
Perfurante preserva uma trajetória reta; Leque ocupa uma região angular. Arte deve mostrar flechas, não
o mesmo orbe usado por magia. Flecha Sangrante deve aplicar bleed segundo chance autorada e sinalizar o status.

Presa Marcada marca um alvo válido por duração definida e beneficia ataques especificados contra ele.
Chefes podem receber bônus controlado, sem imobilização implícita. Passos de Kiting ativa após disparo e
expira; não é velocidade permanente. Afinação de Projétil precisa chegar à velocidade real do projétil.

### 5.3. Magic — formas espaciais e combinações legíveis

| Capacidade | Contrato de identidade proposto |
|---|---|
| Fagulha/Chama Breve | Definir diferença real de alcance/forma/cadência; Burn apenas se declarado e implementado |
| Laço de Gelo / Rajada Gélida | Aplicar chill/slow conforme contrato; informar duração e resistência do alvo |
| Nuvem Tóxica | Área persistente, duração e intervalo de tick autorados; limite de acúmulo; saída encerra exposição conforme regra |
| Corrente Relâmpago | Buscar próximo alvo elegível dentro do raio; limite de saltos; não repetir o mesmo alvo na mesma cadeia |
| Guarda Elemental | Buff temporário, resistências delimitadas, indicador e remoção ao expirar |
| Sigilos Lentificantes | Status válido antes do débito; contar apenas aplicações aceitas |
| Domínio do Raio Arcano | Bônus na magia/forma declarada, sem aumentar todo ataque genérico |
| Anya/Senya | Efeitos de suporte/ofensiva mutuamente exclusivos conforme decisão existente |

Para cadeia, desempatar candidatos por critério estável, limitar consulta e registrar alvos visitados.
Para nuvem, não criar dano por frame: agendar ticks e limpar a área ao término/transição. Não definir aqui
um novo sistema genérico de magia: reutilizar formas/pipeline de projéteis, status e alvos existentes.

### 5.4. Survival — informação, retirada e preparação de expedição

Instinto de Sobrevivência deve revelar temporariamente recursos/perigos/interagíveis próximos, sem
gerar conteúdo, revelar mapa inteiro ou alterar snapshots da cave. Usar marcadores de apresentação sobre
objetos existentes e removê-los ao expirar. O addendum admite revelação + restauração de stamina;
o corte da restauração é alternativa de rebalanceamento a decidir, não correção obrigatória do cânone.

Campo Seguro deve ser uma zona identificável, fora de combate, com benefício delimitado e regra de
interrupção. Proposta: efeito cessa ao entrar em combate ou sair da zona; limite de um campo por jogador.
Sinal de Retirada altera custos das abilities/ações de movimento realmente utilizadas. Isca Improvisada
atrai criaturas elegíveis por tempo limitado; chefes não abandonam toda a lógica de combate.

Kit de Emergência pode ganhar custo de consumível como proposta; o addendum atual permite cura sem item
fora de combate. Diferenciá-lo de Último Fôlego: kit é preparo
logístico fora de combate; Último Fôlego é recuperação emergencial de cooldown alto. Os números finais
dependem do balanceamento aprovado, sem regeneração gratuita escondida em habilidades de exploração.

Passo Seguro reduz penalidade de terreno, preservando velocidade em terreno normal. Recuperação Instintiva
altera a duração real de status recebidos. Resistências já conectadas são preservadas e verificadas.

### 5.5. Crafting — ferramenta, oficina e rotina de fazenda

Esta árvore tem a maior concentração de passivos vazios e não deve cobrar pontos por “hooks futuros”.
Separar benefício de oficina, reparo, coleta e eficiência agrícola, indicando o consumidor real de cada um.

| Nó/capacidade | Direção proposta e limite |
|---|---|
| Olho de Material | Bônus de rendimento no evento de coleta/colheita definido, com regra determinística e cap; decidir quais fontes |
| Mochila Ordenada | Não vender organização básica como poder. Propor utilidade concreta ou aposentar nó com reembolso e religação de dependências |
| Método de Salvage | Melhorar resultado do salvage real; não publicar apenas ToolEfficiency |
| Acabamento Durável | Alterar durabilidade máxima de equipamento produzido no ponto de criação; persistir valor/identidade simples |
| Senso de Mercado | Preço de compra/venda no sistema de shop; não confundir com gold drop de combate |
| Foco de Bancada | Tempo/custo no job elegível, com preview; não aplicar desconto duplicado entre UI e execução |
| Remendo de Campo / Reparo Rápido | Distinguir reparo portátil limitado e operação mais eficiente; custo/material/alvo autorados |
| Irrigador Portátil | Alvos elegíveis em área definida, limite e custo claros; solo já molhado não conta como novo efeito |
| Bomba Improvisada | Consumir carga craftada; explosão ao impacto/fim do voo, raio e postura delimitados; sem perfuração disfarçada |
| Marca de Eficiência | Buff temporário consumido por ações agrícolas/crafting elegíveis; sem acúmulo e com expiração |

Descontos e yields não podem criar ciclos de lucro infinito: nunca custo negativo, nenhuma duplicação de
output ao carregar job, bônus calculado uma vez na fronteira definida. A futura spec deve escolher se o
job guarda a condição do início ou consulta a condição na conclusão; preferência proposta: snapshot no
início, para impedir respec/equipamento antes da entrega de reescrever o resultado.

## 6. Progressão e expansão de capacidades

Cada rank comprado precisa mostrar e aplicar um delta: magnitude, duração, custo ou alcance. Não obrigar
todas as ativas a escalar da mesma maneira. Se ranks extras não oferecem benefício, bloqueá-los até existir
curva real; o ponto não compra apenas um numeral. Bônus condicionais devem informar sua condição atual.

A spec deve declarar o teto autorado de cada nó. Teto efetivo = menor entre teto autorado e limite
dinâmico da árvore; capstone segue o teto reconciliado na decisão. Rank sem benefício não é vendido.
Isso é regra de disponibilidade do conteúdo, não autorização para reduzir silenciosamente ranks já
comprados: ranks antigos afetados entram na tabela de compatibilidade/reembolso.

Antes de aumentar o catálogo, entregar três experiências completas: **combatente de postura**, **arqueiro
de preparação** e **explorador/artesão de expedição**. São combinações das árvores existentes, sem classes
fixas novas. Validar distribuição de pontos em marcos 5/11/18/26, pré-requisitos e teto total vigente.

Capacidades futuras candidatas, após fechamento das atuais: interações controladas de elementos com
ambiente, preparação de expedição na fazenda e sinergias de marca/postura. Não adicionar uma sexta árvore,
árvore social ou poderes de geração de cave neste refinamento. Primeiro provar que informação, terreno,
preparo e combate atuais já mudam decisões sem tornar outra árvore obrigatória para ações básicas.

## 7. UI: árvore compreensível e operável

Migrar a rota instalada para um modal Canvas único, reutilizando `SkillTreeMenuViewModel`, manager e
ModalManager. IMGUI pode continuar como diagnóstico durante transição; não deve ser a entrega de produto.

```text
┌ HABILIDADES                  Pontos disponíveis: N          Fechar ┐
│ [Melee] [Ranged] [Magic] [Survival] [Crafting]                       │
│ Árvore: pontos X / próximo tier Y   │ Ícone  Nome    Ativa/Passiva  │
│                                    │ Rank atual → próximo          │
│ T1  nós e conexões                  │ Efeito, alvo e condição       │
│ T2  nós e conexões                  │ Custo / duração / recarga     │
│ T3  nós e conexões                  │ Ganho real do próximo rank    │
│ T4  nós e conexões                  │ Motivo de bloqueio            │
│ T5  capstone / variantes            │ [Aprender/Melhorar] [Equipar]  │
│ [1 habilidade] [2 habilidade] [3 habilidade] [4 habilidade]         │
└ Ajuda de controles conforme dispositivo / acesso contextual respec ┘
```

Diagrama de fluxo, não composição pixel art aprovada. A árvore ocupa a área principal; a ficha permanece
fixa enquanto seleção muda. Mostrar tier e pré-requisitos por conexões; scroll/foco leva ao nó bloqueador.
Uma avaliação de domínio alimenta botão, texto de bloqueio e execução: não duplicar `RequirementsMet` na view.

Compra inicial, rank-up e escolha de variante são ações distintas. Mostrar comparação das variantes e
confirmar exclusividade; não tentar comprar sem chosenVariant. Equipar abre escolha entre quatro slots;
indicar substituição e tecla real de **uso**. Separar atalhos de atribuição dos rótulos da HUD.

Estados necessários: bloqueado por pré-requisito, tier, pontos, conteúdo indisponível, comprado, rank
melhorável, máximo, equipado e selecionado. Durante gameplay: vazio, pronto, recarga, recurso insuficiente,
contexto inválido e ação em andamento. Um ícone cinza sozinho não explica esses estados.

| Estado | Sinal visual + texto obrigatório |
|---|---|
| Pré-requisito/tier/pontos insuficientes | Cadeado/badge e requisito específico na ficha |
| Conteúdo indisponível | Badge distinto e explicação; sem botão de compra ativo |
| Comprado/rank melhorável/máximo | Pips de rank e comando correspondente ou indicação de teto |
| Equipado/foco | Número do slot e contorno de foco independente da cor da árvore |
| Recarga | Sobreposição de progresso + tempo restante; origem no estado da action |
| Recurso/contexto insuficiente | Badge de recurso/alvo e motivo textual acessível por foco |
| Em andamento | Indicador de carga/cast, sem permitir uma segunda execução incompatível |

Mouse e teclado devem completar o fluxo inteiro. Gamepad requer navegação explícita por nós/abas/slots,
foco inicial e retorno consistente; não presumir suporte por existir uma interface abstrata de input.
Modal bloqueia ações de gameplay; fechar restaura foco sem disparar habilidade pela mesma tecla.

## 8. Brief de pixel art para interface e efeitos

### Referência observada e decisões provisórias

Referência aberta: `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`, aprovada
para o mundo. Madeira escura, pedra, verdes/ocres e ciano da Fonte são referências de materiais/cor.
O guia visual exige pixel art sem bordas suavizadas. Nenhuma composição de UI de skills foi aprovada ainda.

Proposta: painel de madeira discreto com interior escuro de alta legibilidade e acentos por árvore.
Evitar ornamento ocupar o espaço dos efeitos/custos. Interface frontal e plana; não transportar a
perspectiva 3/4 do mundo para texto/botões. Ciano fica para foco/afinidade apropriada, sem cobrir tudo.

### Kit modular a produzir

| Peça | Proposta inicial | Critério |
|---|---|---|
| Ícones | Base 32×32; um por nó exposto | Silhueta distinguível em 1x, 2x e no HUD real |
| Emblemas | Cinco árvores, mesma família | Identificação por forma e cor |
| Moldura/painéis | 9-slice, borda nativa proposta 4 px | Redimensionar sem esticar cantos nem borrar |
| Nós/conectores | Estados normal/foco/bloqueado/comprado/equipado | Estado não depende só de vermelho/verde |
| Badges | Ativa/passiva/capstone, rank, indisponível | Leitura sem encobrir ícone |
| Slots | Quatro molduras com tecla, ícone, recarga e custo | Mesma action tem mesmo ícone no menu e HUD |
| Feedback | Marcador de alvo, cast/impacto, buff e expiração | Reflete evento aceito; nunca simula sucesso vazio |

Texto localizado fora do bitmap, com fonte que suporte português, acentos e números legíveis. Testar nomes
longos sem cortar custo/pré-requisitos. O tamanho lógico da UI será escolhido pela escala efetiva do Canvas;
32 px é proposta de ícone, não pressuposto sobre PPU ou resolução atual do jogo.

Import: Point, Compression None, mipmaps false; transparência real nos sprites; escala inteira sempre que
compatível com o layout. Preservar arquivos-fonte em camadas e exportações por peça. Uma imagem achatada
de toda a tela serve apenas como conceito, não como UI interativa pronta.

Produzir primeiro amostra de cinco ícones (um por árvore), um slot e uma ficha com todos os estados.
Revisar no jogo em 1280×720 e 1920×1080 como resoluções de teste propostas, sem afirmar suporte atual.
Somente após aceitação dessa amostra expandir para todos os nós efetivamente expostos. Não encomendar
68 ícones só porque há 68 assets antigos: o inventário aprovado de conteúdo deve conduzir a produção.

Aceitação visual mínima: em ambas as resoluções de teste, nenhum nome/custo/requisito em português fica
truncado ou sobreposto; foco e bloqueio continuam distinguíveis em escala de cinza; tempo de recarga não
encobre o número do slot; texto não é esticado junto da moldura. Comparar capturas dos mesmos estados e
registrar aprovação da amostra antes do lote completo, sem alegar conformidade de acessibilidade por inferência.

Para efeitos, começar por golpe/arco, flecha, magia de gelo, revelação e irrigação. Comparar origem,
direção, duração e leitura sobre fundo de farm e cave. Áudio usa a rota de eventos viva; não ligar apenas
ao evento legado que o controller atual não publica. Reutilizar eventos adequados ou definir payload
de apresentação com action ID, fase, origem e resultado na spec, evitando bus de gameplay paralelo.

## 9. Plano para materializar specs residuais

Os nomes abaixo são fatias propostas, **não arquivos/specs já criados**. Cada spec futura deverá conter
assinaturas reais, edits por arquivo, testes nomeados e critérios copiados integralmente deste contrato.

| Fatia | Escopo/ownership principal | Depende de | Saída verificável |
|---|---|---|---|
| S1 Integridade de execução | ActiveSkillExecutionController, SkillEffectContext, binding de avatar em composição; executores melee/projétil/slow | Auditoria A01/A06/A11 | Caster correto, status aplica, falhas sem custo, hits únicos, física consistente |
| S2 Pontos e respec | SkillTreeManager/State/RespecService, PlayerProgressionManager, providers SkillTree/Progression, SkillTierEquipGate/EquipmentManager | S1 não obrigatório; coordenar ownership de manager | Conservação de pontos, reembolso de atos/IDs, equipamentos revalidados |
| S3 Contrato por nó e prontidão | DefaultSkillCatalog, SkillActionEffectCatalog, registry, SkillPurchaseService, projeções HUD | Decisões de cooldown/capstone/disponibilidade; S1/S2 | 31 ativas classificadas; zero sucesso vazio exposto; rank com delta e grafo alcançável |
| S4 Identidade de combate | Executores e pipelines existentes de movimento/projéteis/magia/status, dados SO | S1/S3 | Charge, chain, cloud, marca/ward e postura com comportamento e apresentação corretos |
| S5 Utilidade e produção | Hooks/aggregator e consumidores Farm/Craft/Equipment/Shop, ações de Survival/Crafting | S2/S3 | Nove passivos inertes resolvidos/retirados com migração; utilidades cumprem identidade |
| S6 Canvas + amostra pixel art | UI/Skills, HUD views/projections, DomainRuntimeInstallers, criador de Canvas e catálogo visual | Modelo de prontidão S3; pode prototipar arte antes | Comprar/rank/variante/equipar, foco e amostra visual funcionando |
| S7 Catálogo visual e aceitação | Assets aprovados, binding Canvas, apresentação de execução/impacto e cenários | S4/S5/S6 | Cada nó exposto demonstrado no jogo, save/load e transições |

Não compartilhar edição de SkillTreeManager entre S2/S3/S6 simultaneamente. Revisão de arquitetura/save
independente é necessária nas integrações de risco; um único owner roda validação Unity integrada.
Usar skills existentes: skill-tree-authoring, save-load-pattern, player-ability-runtime,
ability-effect-composition, hud-canvas-binding, ui-projection-pattern, ui-modal-stack, localization-authoring,
pixel-art-direction e visual-asset-review, somente quando a fatia exigir. Não recriar managers paralelos.

## 10. Critérios de aceitação e testes propostos

Nomes abaixo são testes a criar/adaptar na spec, não resultados executados.

| Teste/cenário | Assert verificável |
|---|---|
| SkillCasterBindingUsesAvatarAcrossScenes | Em Farm/Town/Cave e transição, caster/body pertencem ao avatar ativo; bootstrap não se desloca |
| SkillCatalogCoversEveryExposedActive | Enumerar catálogo exposto, não registry; nenhuma action sem mapping ou feedback-only habilitada |
| SkillStatusAppliesAtAuthoredChance | Chance 1 aplica status e chance 0 não; RNG seed controlada quando probabilístico |
| SkillMissingDependencyDoesNotSpend | Status/avatar/consumível ausente não altera saldo nem cooldown |
| MeleeMultiColliderAppliesOnce | Dois colliders do mesmo inimigo causam uma aplicação por janela |
| SkillRespecConservesEarnedPoints | Nível 10: cinco pontos de nível + um de ato, todos gastos → respec seis disponíveis → compra deixa cinco |
| RemovedNodeRefundIsIdempotent | Dois loads do estado migrado não repetem reembolso |
| SkillRankChangesEffectiveOutcome | Rank 1 e 2 produzem os valores distintos autorados, não apenas label diferente |
| CapstoneVariantChangesConsumer | Alternativas produzem efeitos distintos e somente uma está ativa após load/respec |
| SkillCooldownFollowsAction | Se proposta aprovada, mover action não elimina recarga e outra action não herda a recarga dela |
| SkillUiReadinessMatchesDomain | Cada motivo de bloqueio impede comando e mostra o mesmo motivo localizado |
| SkillModalCompletesProgressionFlow | Aprender → aumentar rank → escolher variante → equipar → fechar → executar sem input vazando |
| CraftSkillConsumerChangesTransaction | Bônus altera custo/tempo/yield na transação real e não duplica ao completar/load |
| RevealSkillDoesNotMutateCaveSnapshot | Revelação muda somente apresentação e desaparece ao expirar |
| SkillVisualBindingHasCompleteReferences | Todos os nós expostos resolvem ícone; quatro slots possuem bindings; nenhum placeholder silencioso |

Play Mode obrigatório: novo jogo normal, sem pontos artificiais, até primeira compra; save preparado
separado para ranks/capstones; mana/stamina insuficiente; sem alvo; parede; inimigo multi-collider;
recarga com troca de slot/modal/cena; respec e compra seguinte; save/load de build; irrigação no canteiro;
craft/reparo em consumidor real. Capturar antes/depois com efeito, recurso e feedback visíveis.

A amostra de arte requer captura real e revisão humana. Build e testes de projeção não comprovam que
Canvas está desenhando, que o ícone é legível ou que a habilidade parece distinta. A validação deve separar
código PASS, wiring PASS, Play Mode automatizado e aceitação visual/humana; registrar pendências com motivo.

## 11. Falhas, limites e alternativas

Não ocultar um pré-requisito quebrado deixando descendentes inalcançáveis. Resolver o efeito primeiro ou
aprovar nova ligação com migração/reembolso. Não trocar identidade só para justificar o executor existente:
renomear uma skill é alternativa válida apenas com aprovação de design e ajuste de texto/dados/save compatível.

Não equilibrar números antes de corrigir origem/status/rank: medir DPS do projétil errado dá diagnóstico
enganoso. Não aumentar catálogo para mascarar nós inertes. Não criar um framework universal de efeitos
antes de provar a fatia vertical no pipeline atual. Novos tipos só após auditoria de reutilização.

Alternativas consideradas: polir apenas UI é insuficiente; reescrever tudo perde infraestrutura útil;
adicionar novas árvores aumenta dívida. A proposta escolhida é corrigir integridade, restaurar identidade
e integrar arte por fatias completas. Mudanças de cave devem obedecer stable run; revele objetos existentes,
não regenere layout/recursos. Alterações procedurais exigem leitura dos amendments obrigatórios.

## 12. Estado desta entrega

Auditoria e refinamento concluídos documentalmente. Proposta de arte preparada; pixel art final ainda não
gerada nem aprovada. Nenhuma spec executável foi promovida e nenhuma mecânica foi implementada nesta tarefa.
Unity/build/Play Mode: NOT RUN para esta edição documental. A revisão final verificou coerência com os
achados, preservação das regras aceitas, distinção entre proposta e implementação e cobertura de aceitação.
