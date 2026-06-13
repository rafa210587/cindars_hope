using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Single source of truth for the expanded town NPC dialogue content (WAVE25+ expansion).
    /// Each NPC gets a standard 13-node tree: greeting hub (with random line pool), role,
    /// town context, service, advice, rumor branches and a goodbye, all with return-to-hub
    /// choices. The editor menu "CindarsHope/NPCs/Rebuild Town NPC Dialogues" writes this
    /// content into the DialogueTreeSO assets; NpcDialogueSetRegistry reports node counts
    /// from here so registry and content never drift apart.
    /// </summary>
    public static class TownNpcDialogueLibrary
    {
        public sealed class NpcDialogueContent
        {
            public string NpcId;
            public string DialogueSetId;
            public string[] Greetings;
            public string Role1;
            public string Role2;
            public string Town1;
            public string Town2;
            public string Service1;
            public string Service2;
            public string[] AdviceLines;
            public string Rumor1;
            public string Rumor2;
            public string[] Goodbyes;
            public bool HasShop;
        }

        public const int NodesPerNpc = 13;

        private static readonly List<NpcDialogueContent> s_content = new List<NpcDialogueContent>
        {
            new NpcDialogueContent
            {
                NpcId = "npc_corvus", DialogueSetId = "dialogue_corvus", HasShop = true,
                Greetings = new[]
                {
                    "Que a Fonte ilumine seus passos, viajante.",
                    "Bem-vindo ao templo. Aqui o silencio fala mais alto que sinos.",
                    "Voce chega num bom momento. As velas acabaram de ser acesas."
                },
                Role1 = "Sou Corvus, guardiao do templo de Cindar's Hope. Cuido das oferendas, das velas e das memorias que ninguem mais quer carregar.",
                Role2 = "Antes de vestir este manto, eu desci a caverna como qualquer outro. O que vi la embaixo me trouxe para perto da Fonte, nao para longe dela.",
                Town1 = "A cidade resiste. Enquanto a estatua do guerreiro estiver de pe na praca, as pessoas lembram por que vieram para ca.",
                Town2 = "Dornecia inteira ja foi mais brilhante. Mas brilho nao se herda; se acende todo dia, vela por vela.",
                Service1 = "Vendo incensos, aguas benzidas e amuletos simples. Nada que substitua coragem, mas tudo que acalma o coracao antes da descida.",
                Service2 = "Se trouxer fragmentos estranhos da caverna, posso olhar. O templo guarda registros antigos sobre a pedra negra.",
                AdviceLines = new[]
                {
                    "Desca a caverna descansado. Cansaco mata mais que monstro.",
                    "Quando duvidar do caminho, volte ao ultimo ponto de luz.",
                    "Nao carregue tudo. Saber largar tambem e sabedoria."
                },
                Rumor1 = "Dizem que nas noites de lua cheia a agua da Fonte fica mais quente. Eu mesmo ja senti o vapor subir diferente.",
                Rumor2 = "Um mineiro jurou ter ouvido cantos no nivel trinta. Cantos, nao gritos. Isso me preocupa mais.",
                Goodbyes = new[] { "Que a Fonte o acompanhe.", "Volte quando o peso for grande demais.", "Va em paz, e volte inteiro." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_mara", DialogueSetId = "dialogue_mara", HasShop = true,
                Greetings = new[]
                {
                    "Bom dia. Documentos, registros ou reclamacoes? Atendo os tres, mas so dois com sorriso.",
                    "Chegou cedo. A fila de papelada ainda esta do tamanho de ontem.",
                    "Se veio registrar terra, sente. Se veio registrar queixa, respire primeiro."
                },
                Role1 = "Mara, escriva do registro civico. Cada lote, cada licenca e cada promessa desta cidade passa pela minha mesa.",
                Role2 = "Pode parecer so papel, mas e memoria. Quando alguem some na caverna, e meu registro que diz que essa pessoa existiu.",
                Town1 = "A cidade cresce devagar, mas cresce certo. Prefiro dez casas bem registradas do que cinquenta no improviso.",
                Town2 = "O conselho discute expandir a rua do mercado. Se acontecer, voce ouviu aqui primeiro.",
                Service1 = "Emito segundas vias, autentico contratos de compra e venda e registro melhorias de fazenda.",
                Service2 = "Tambem guardo mapas antigos do vale. Copias custam pouco; os originais nao saem daqui nem com ordem real.",
                AdviceLines = new[]
                {
                    "Registre suas colheitas grandes. Papel hoje evita briga amanha.",
                    "Leia tudo antes de assinar. Ate de mim.",
                    "Guarde recibos. A memoria falha; a tinta nao."
                },
                Rumor1 = "Tem um lote abandonado perto do portao sul cujo dono nunca apareceu. Faz vinte anos. O registro continua aberto.",
                Rumor2 = "Yael pediu licenca para o mercado noturno tres vezes. Foi negada duas. Na terceira, alguem de cima assinou. Curioso, nao?",
                Goodbyes = new[] { "Proximo!", "Leve seus papeis, nao os meus.", "Ate logo. E nao perca o protocolo." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_tovin", DialogueSetId = "dialogue_tovin", HasShop = true,
                Greetings = new[]
                {
                    "Licencas e alvaras. Se nao for isso, e no balcao da Mara.",
                    "Ah, um rosto novo. Espero que com a papelada em dia.",
                    "Bem-vindo ao balcao de permissoes. A burocracia agradece sua paciencia."
                },
                Role1 = "Tovin, oficial de permissoes. Construcao, comercio ambulante, exploracao da caverna em grupo... tudo precisa do meu carimbo.",
                Role2 = "Ja neguei alvara a gente importante. O carimbo nao reconhece sobrenome, so requisito.",
                Town1 = "Cidade organizada e cidade viva. O caos parece charmoso ate o primeiro incendio.",
                Town2 = "O fluxo de aventureiros dobrou desde a primavera. Bom para o comercio, pessimo para minha pilha de pedidos.",
                Service1 = "Vendo selos de autorizacao e formularios prontos. Com eles, voce resolve em um dia o que levaria uma semana.",
                Service2 = "Quer montar banca no mercado? Traga o formulario M-3 preenchido e o ouro da taxa. Eu cuido do resto.",
                AdviceLines = new[]
                {
                    "Peca a licenca antes de construir, nao depois.",
                    "Taxa atrasada dobra. Sempre. Nao teste.",
                    "Se um formulario parecer inutil, preencha mesmo assim. Ele protege voce."
                },
                Rumor1 = "Alguem pediu licenca para 'pesquisa de fauna subterranea'. Nivel cinquenta para baixo. O conselho nem sabia que isso existia.",
                Rumor2 = "Ouvi que Gurd quer erguer um muro novo no lado leste. O orcamento... bem, o orcamento e otimista.",
                Goodbyes = new[] { "Carimbado. Proximo.", "Boa sorte com a papelada.", "Volte com o formulario certo." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_sylveth", DialogueSetId = "dialogue_sylveth", HasShop = true,
                Greetings = new[]
                {
                    "Sementes frescas, chegadas hoje! Sinta o cheiro de terra boa.",
                    "Ola, fazendeiro! Suas plantas mandaram lembrancas.",
                    "Bem-vindo! Se esta com as maos sujas de terra, ja gosto de voce."
                },
                Role1 = "Sylveth, vendedora de sementes e apaixonada por tudo que brota. Minha banca tem desde trigo comum ate cenoura premiada.",
                Role2 = "Cresci numa fazenda a tres vales daqui. Quando a colheita falhou, aprendi que semente boa vale mais que ouro guardado.",
                Town1 = "Esta cidade come do que voce planta. Cada canteiro seu alimenta alguem da praca.",
                Town2 = "O solo do vale e generoso, mas tem humor. Chuva demais na primavera, sede no verao. Plante sabendo disso.",
                Service1 = "Vendo sementes de estacao, mudas e adubo basico. Se a semente nao germinar, troco sem discussao.",
                Service2 = "De vez em quando aparece semente rara vinda da caverna. Brilham no escuro. Plantei uma... melhor nao contar o resultado.",
                AdviceLines = new[]
                {
                    "Regue de manha. A tarde a agua evapora antes de afundar.",
                    "Roda de culturas! Nao repita a mesma planta no mesmo canteiro.",
                    "Cenoura gosta de solo fofo. Capriche na enxada."
                },
                Rumor1 = "O Eiran anda criando um filhote estranho no quintal. Ele jura que e cabra. Cabra nao tem escama, Eiran.",
                Rumor2 = "Dizem que ha cogumelos gigantes no nivel quinze da caverna. Se alguem me trouxer esporos, pago bem.",
                Goodbyes = new[] { "Boa colheita!", "Va com as maos cheias e volte com elas vazias!", "Que a chuva venha na hora certa." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_renko", DialogueSetId = "dialogue_renko", HasShop = true,
                Greetings = new[]
                {
                    "Renko tem de tudo! E o que nao tem, consegue ate amanha.",
                    "Cliente bom e cliente que volta. E voce voltou!",
                    "Entre, olhe, toque! So nao derrube a pilha de panelas."
                },
                Role1 = "Renko, comerciante geral. Ferramentas, corda, lampiao, panela, botao... se cabe numa prateleira, eu vendo.",
                Role2 = "Comecei com uma mochila e duas rotas de comercio. Hoje tenho a maior banca da rua do mercado. Amanha? Quem sabe uma filial na capital.",
                Town1 = "O mercado e o coracao da cidade. Quando a praca enche, ate a estatua parece sorrir.",
                Town2 = "Concorrencia? Eu chamo de vizinhanca. A Mirela costura, o Brumdar forja, eu vendo o resto.",
                Service1 = "Compro excedente de colheita e materiais da caverna a preco justo. Justo de verdade, pode pesquisar.",
                Service2 = "Se procura algo raro, me diga. Tenho contatos nas caravanas que passam pelo vale.",
                AdviceLines = new[]
                {
                    "Compre corda. Ninguem nunca se arrependeu de ter corda.",
                    "Venda na alta, plante na baixa. Funciona para nabo e para minerio.",
                    "Cliente apressado paga caro. Respire antes de negociar."
                },
                Rumor1 = "Uma caravana sumiu na estrada norte semana passada. Acharam as carrocas... vazias e arrumadas. Arrumadas!",
                Rumor2 = "Dizem que existe um mercador que aparece DENTRO da caverna. Se for verdade, e meu concorrente mais corajoso.",
                Goodbyes = new[] { "Volte sempre! E traga amigos!", "Negocio fechado e amizade mantida.", "Se faltar algo, ja sabe onde achar." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_mirela", DialogueSetId = "dialogue_mirela", HasShop = true,
                Greetings = new[]
                {
                    "Essa costura do seu casaco... senta. Eu arrumo em dez minutos.",
                    "Bem-vindo ao atelie! Cuidado com os alfinetes no chao.",
                    "Ola, querido. Tecido novo chegou de Dornecia, quer ver?"
                },
                Role1 = "Mirela, alfaiate da cidade. Visto noivas, mineiros e todo mundo entre os dois.",
                Role2 = "Aprendi o oficio com minha avo. Ela dizia: roupa boa nao esconde quem voce e, ela apresenta.",
                Town1 = "Pela roupa eu sei como vai a cidade. Muita roupa rasgada de caverna? Tempos dificeis. Muita roupa de festa? Colheita boa.",
                Town2 = "A praca renovada deu vida nova a rua. Ate o Brumdar passou a limpar a fuligem da barba.",
                Service1 = "Faco reparos, ajustes e roupas sob medida. Tecido resistente para a caverna e meu carro-chefe.",
                Service2 = "Couro de criatura da caverna rende casacos incriveis. Se trouxer material, desconto o trabalho.",
                AdviceLines = new[]
                {
                    "Costure os bolsos por dentro. Batedores de carteira odeiam isso.",
                    "Leve sempre uma agulha e linha na mochila. Salva expedicoes.",
                    "Roupa molhada na caverna e convite para febre. Troque-se."
                },
                Rumor1 = "A Liora encomendou um vestido de palco cor de meia-noite. Disse que e para 'a noite em que a estatua cantar'. Poetas...",
                Rumor2 = "O Maelor passa aqui toda lua nova para remendar o mesmo casaco. Sempre o mesmo rasgo, no mesmo lugar. Nunca explica.",
                Goodbyes = new[] { "Volte para a prova final!", "Cuide das suas bainhas, querido.", "Ate mais. E nada de rasgar isso de novo!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_orlan", DialogueSetId = "dialogue_orlan", HasShop = true,
                Greetings = new[]
                {
                    "Bem-vindo a Estalagem do Vale! Cama seca e sopa quente.",
                    "Entre, entre! O vento la fora nao paga aluguel.",
                    "Um viajante! As melhores historias chegam com botas sujas."
                },
                Role1 = "Orlan, dono da estalagem. Vinte quartos, uma lareira eterna e a melhor sopa de raiz do vale.",
                Role2 = "Ja fui guarda de caravana. Decidi que prefiro receber viajantes a protege-los. Paga melhor e doi menos.",
                Town1 = "Quem dorme bem explora melhor. Metade dos herois desta cidade acordou nas minhas camas.",
                Town2 = "A estalagem ouve tudo. Se quiser saber da cidade, sente no salao por uma noite e apenas escute.",
                Service1 = "Alugo quartos por noite e vendo refeicoes quentes. Hospede tem direito a banho e fofoca gratis.",
                Service2 = "Guardo cartas e encomendas para quem vive descendo a caverna. Ja salvou muito combinado.",
                AdviceLines = new[]
                {
                    "Durma antes de descer. A caverna cobra juros do sono atrasado.",
                    "Sopa quente resolve oitenta por cento dos problemas. O resto e com o Brumdar.",
                    "Nunca conte seu ouro na frente dos outros hospedes."
                },
                Rumor1 = "Um hospede pagou adiantado um mes e nunca subiu do nivel quarenta. O quarto dele continua trancado, como prometido.",
                Rumor2 = "As cartas que chegam para o Maelor nao tem remetente. E cheiram a sal. Nos nao temos mar por perto.",
                Goodbyes = new[] { "Boa estrada! E volte para a sopa.", "A porta fecha tarde, ate logo.", "Que seus sonhos sejam de colheita cheia." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_gruta", DialogueSetId = "dialogue_gruta", HasShop = true,
                Greetings = new[]
                {
                    "HAH! Mais um sedento! Senta que o barril ta novo.",
                    "Bem-vindo a Taverna da Gruta! Aqui dentro ninguem e estranho.",
                    "Chegou na hora! A cerveja de mel acabou de descansar."
                },
                Role1 = "Gruta, taverneira e juiza nao oficial de braco de ferro. Minha taverna e o lugar mais barulhento e mais honesto da cidade.",
                Role2 = "Herdei o balcao do meu pai. Ele dizia: sirva bem o primeiro copo e escute o terceiro. Sabedoria de taverna.",
                Town1 = "A cidade trabalha de dia e desabafa aqui de noite. Eu so mantenho os copos cheios e as brigas curtas.",
                Town2 = "Mineiro, fazendeiro, guarda, poeta... no meu balcao todo mundo senta no mesmo banco.",
                Service1 = "Sirvo bebida, comida farta e musica quando a Liora aparece. Tambem alugo o salao para festas de colheita.",
                Service2 = "Aventureiro que volta da caverna ganha o primeiro gole por minha conta. Tradicao da casa.",
                AdviceLines = new[]
                {
                    "Nunca desca a caverna de ressaca. NUNCA.",
                    "Quem paga a rodada faz amigos. Quem paga duas faz testemunhas.",
                    "Briga aqui dentro? O perdedor limpa o salao."
                },
                Rumor1 = "Um cliente bebado jurou que viu a estatua da praca mudar a posicao do escudo. Bebado, claro. Claro...",
                Rumor2 = "O Zrix anda recusando cerveja. Zrix! Ou esta doente, ou viu algo na estrada da caverna que o deixou serio.",
                Goodbyes = new[] { "Vai com Deus e volta com sede!", "Cuidado no caminho, a rua gira as vezes!", "Proxima rodada tem teu nome!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_brumdar", DialogueSetId = "dialogue_brumdar", HasShop = true,
                Greetings = new[]
                {
                    "Fala logo, o ferro nao espera.",
                    "Hm. Voce de novo. A lamina aguentou?",
                    "Entra. Cuidado com a fagulha, ela nao escolhe rosto."
                },
                Role1 = "Brumdar. Ferreiro. Faco lamina, enxada, prego e machado. Tudo que corta ou aguenta porrada sai desta forja.",
                Role2 = "Meu avo forjou a espada da estatua da praca. Espada bastarda, tempera dupla. Ainda hoje nao faco igual. Ainda.",
                Town1 = "Cidade que tem forja acesa nao morre. Pode anotar.",
                Town2 = "O minerio do vale e honesto, mas o da caverna... o da caverna canta quando bate no fogo. Metal estranho.",
                Service1 = "Vendo ferramentas e armas. Conserto o que voce quebrar, sem julgar como quebrou. Quase sem julgar.",
                Service2 = "Traga minerio da caverna e eu pago na hora. Cobre, ferro... e se achar do escuro, traga embrulhado.",
                AdviceLines = new[]
                {
                    "Afie antes de precisar, nao depois.",
                    "Ferramenta cara e barata se durar. Ferramenta barata custa um dedo.",
                    "Lamina lascada se conserta. Orgulho lascado, nao."
                },
                Rumor1 = "A Dagna trouxe uma pedra da pedreira que nao esquenta na forja. Fica fria. FRIA. Guardei num balde, longe do resto.",
                Rumor2 = "Pediram-me uma replica da espada da estatua. Paguei para ver quem pediu... e o sujeito sumiu da cidade no dia seguinte.",
                Goodbyes = new[] { "Vai. E nao quebra isso de novo.", "Hm. Ate.", "Volta quando o fio cansar." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_dagna", DialogueSetId = "dialogue_dagna", HasShop = true,
                Greetings = new[]
                {
                    "Opa! Cuidado com o carrinho, ele desce sozinho as vezes.",
                    "Dia de poeira boa! O que procura?",
                    "Se veio pela pedra, chegou ao lugar certo. Se veio pela vista, tambem."
                },
                Role1 = "Dagna, mestra da pedreira. Corto pedra para casa, muro, praca e ate para a base da estatua, que foi a minha avo que assentou.",
                Role2 = "Pedra e como gente: tem veio, tem humor e racha onde voce menos espera. Trinta anos lendo pedra e ainda me surpreendo.",
                Town1 = "Toda casa nova da cidade tem um pedaco da minha pedreira. Isso me enche o peito, sabia?",
                Town2 = "O conselho quer calcar a rua do mercado. Otima ideia. Pessima estimativa de quantas carrocas de pedra isso leva.",
                Service1 = "Vendo pedra bruta, pedra aparelhada e cal. Para fundacao de fazenda, tenho preco de vizinho.",
                Service2 = "Compro pedra especial da caverna. As com veia azul pagam o dobro. As que sussurram... essas eu nao compro. Serio.",
                AdviceLines = new[]
                {
                    "Na caverna, bata na parede antes de confiar nela. Som oco e aviso.",
                    "Picareta cega cansa o dobro. Visita o Brumdar.",
                    "Pedra molhada engana o pe. Pisa devagar."
                },
                Rumor1 = "Achei um fossil na camada nova. Bicho grande, asa comprida. O vale ja foi fundo do mar? Ou ceu de outra coisa?",
                Rumor2 = "O pessoal do nivel vinte diz que as paredes de la 'respiram'. Pedra nao respira. Mas confesso que fui ver... e voltei calada.",
                Goodbyes = new[] { "Vai pela sombra, a poeira agradece!", "Pedra no caminho? Me chama!", "Ate! E olha o carrinho!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_hund", DialogueSetId = "dialogue_hund", HasShop = true,
                Greetings = new[]
                {
                    "Tudo em ordem por aqui. Voce que chegou desarruma?",
                    "Patrulha tranquila hoje. Bom sinal. Ou pessimo, nunca sei.",
                    "Cidadao. Algum problema a relatar?"
                },
                Role1 = "Hund, guarda da ronda urbana. Da praca ao portao, passando pelo mercado, meu turno cobre tudo.",
                Role2 = "Servi na fronteira antes de vir para ca. La aprendi que vigiar e noventa por cento andar e dez por cento estar no lugar certo.",
                Town1 = "Cidade segura nao e a que nao tem problema. E a que resolve rapido. Estamos resolvendo cada vez mais rapido.",
                Town2 = "A entrada da caverna e meu pesadelo logistico. Todo dia entra gente demais e sai gente de menos.",
                Service1 = "Mantenho o quadro de avisos e registro ocorrencias. Tambem vendo equipamento basico de seguranca aprovado pela guarda.",
                Service2 = "Se vai descer fundo na caverna, deixe aviso comigo. Se nao voltar em tres dias, organizamos busca.",
                AdviceLines = new[]
                {
                    "Ande pelo centro da rua a noite. Sombra e esconderijo.",
                    "Aviso dado a guarda nunca e tempo perdido.",
                    "Se ouvir briga na taverna, deixa. A Gruta resolve mais rapido que eu."
                },
                Rumor1 = "Pegadas estranhas no portao leste, tres noites seguidas. Grandes, descalcas, e somem no meio da rua. SOMEM.",
                Rumor2 = "O Alaric anda dobrando o turno no portao sul sem mandato. Quando guarda veterano faz isso, e porque farejou algo.",
                Goodbyes = new[] { "Siga em seguranca.", "Qualquer coisa, grite. Eu ouco.", "Ordem e prosperidade, cidadao." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_thalindra", DialogueSetId = "dialogue_thalindra", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado com a pilha da esquerda, ela morde. Brincadeira. Ela so desaba.",
                    "Bem-vindo ao arquivo. Faca silencio... ou pelo menos fofoque baixo.",
                    "Ah, voce. Tenho separado uns registros que talvez lhe interessem."
                },
                Role1 = "Thalindra, arquivista de Cindar's Hope. Guardo a historia da cidade: mapas, diarios, atas e tudo que o tempo tentou apagar.",
                Role2 = "Estudei na grande biblioteca de Dornecia. Vim para ca porque os documentos daqui faziam perguntas que os de la nao sabiam responder.",
                Town1 = "Esta cidade foi fundada ao redor da Fonte por gente que fugia de algo. Os diarios nunca dizem do que. Isso me tira o sono.",
                Town2 = "A estatua da praca representa o Guerreiro de Cindar. A espada e o escudo sao reais, fundidos na base. Poucos sabem disso.",
                Service1 = "Consulto registros, copio mapas e avalio relicarios. Se achou algo escrito na caverna, traga. Eu leio ate o ilegivel.",
                Service2 = "De tempos em tempos preciso de maos para tarefas praticas: materiais, entregas, verificacoes. Pago do fundo do arquivo.",
                AdviceLines = new[]
                {
                    "Anote o nivel onde achar qualquer inscricao. Contexto vale mais que o achado.",
                    "Historia se repete primeiro como aviso. Leia os avisos.",
                    "Mapas antigos erram, mas erram com padrao. Aprenda o padrao."
                },
                Rumor1 = "Ha um diario de 90 anos atras que descreve a caverna com 101 niveis. O mesmo numero de hoje. Cavernas crescem. Essa nao.",
                Rumor2 = "O fundador da cidade assinava 'C.' em tudo. Cindar? Talvez. Mas ha uma ata assinada 'C.' datada de antes do nascimento dele.",
                Goodbyes = new[] { "Volte com perguntas melhores. As suas ja sao boas.", "O arquivo nao fecha; eu que durmo.", "Leve conhecimento, deixe poeira." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_alaric", DialogueSetId = "dialogue_alaric", HasShop = false,
                Greetings = new[]
                {
                    "Alto. ...Ah, e voce. Pode passar.",
                    "Portao sul, tudo calmo. Por enquanto.",
                    "Identifique-se. Costume antigo, nao leve a mal."
                },
                Role1 = "Alaric, sentinela do portao sul. Primeiro rosto que o viajante ve, ultimo que o problema encontra.",
                Role2 = "Vinte anos de muralha. Ja vi caravana, tempestade, lobo e coisa que prefiro chamar de lobo para dormir melhor.",
                Town1 = "O portao sul e o pulso da cidade. Pela manha sai suor para a fazenda; a noite volta cansaco e historias.",
                Town2 = "A estrada para a fazenda anda segura. Mantenho assim a base de bota gasta e olho aberto.",
                Service1 = "Nao vendo nada. Vigio. Mas se precisar de orientacao sobre as estradas, pergunta certa no posto certo.",
                Service2 = "Registro entrada e saida de grupos grandes. Se sua familia vier visitar, me avise que eu agilizo.",
                AdviceLines = new[]
                {
                    "Saia cedo, volte antes do sol sumir. A estrada muda de cara no escuro.",
                    "Avise alguem do seu destino. Sempre.",
                    "Se vir fumaca na estrada, nao va investigar. Venha me chamar."
                },
                Rumor1 = "Tres noites atras, algo grande passou rente a muralha. Nao deixou pegada no barro. O barro estava fresco.",
                Rumor2 = "O velho posto de vigia do morro leste acendeu luz semana passada. Esta abandonado ha dez anos. Mandei verificar... nada.",
                Goodbyes = new[] { "Siga. E mantenha-se na estrada.", "Portao fecha ao ultimo sino.", "Va. Eu fico. E assim que funciona." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_pip", DialogueSetId = "dialogue_pip", HasShop = true,
                Greetings = new[]
                {
                    "OI! Voce e o da fazenda, ne? NE?!",
                    "Visitante! Visitante! Eu vi primeiro!",
                    "Bem-vindo, bem-vindo! Quer um mapa? Eu desenhei! Ta quase certo!"
                },
                Role1 = "Pip! Recepcionista oficial nao oficial da cidade! Eu mostro onde fica tudo e ainda carrego pacote pequeno!",
                Role2 = "Um dia vou ser explorador da caverna. Ja desci ate o nivel UM! Sozinho! Quase. O Zrix foi junto. Atras de mim. Segurando minha mao.",
                Town1 = "A cidade e DEMAIS! Tem a estatua do guerreiro, a sopa do Orlan, e a Gruta deixa eu ficar na taverna ate o segundo sino!",
                Town2 = "Todo mundo aqui se conhece. Se voce espirrar na praca, no portao ja falam que voce ta gripado.",
                Service1 = "Vendo mapinhas da cidade, recados entregues e lembrancinhas! Barato! Quase de graca! Mas pago, ta?",
                Service2 = "Sei TODOS os atalhos. Tipo o do beco da Mirela que corta direto pro mercado. Custa uma moeda. A informacao, nao o beco.",
                AdviceLines = new[]
                {
                    "A sopa do Orlan as quintas tem o dobro de raiz! E o mesmo preco!",
                    "Nao mexe na pilha de pedra da Dagna. Confia em mim. CONFIA.",
                    "Se a Gruta gritar teu nome, corre pra la. Ou pra longe. Depende do tom."
                },
                Rumor1 = "Eu VI o mercador errante! Na entrada da caverna! Ele tem uma mochila MAIOR QUE EU e sumiu quando pisquei!",
                Rumor2 = "A estatua da praca... eu deixei uma flor no escudo dela ontem. Hoje a flor tava na MAO dela. Ninguem acredita em mim!",
                Goodbyes = new[] { "Tchau tchau! Me chama se se perder!", "Vou contar pra todo mundo que voce passou aqui!", "ATE MAIS! Cuidado com o degrau! Esse ai! Esse!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_nimble", DialogueSetId = "dialogue_nimble", HasShop = true,
                Greetings = new[]
                {
                    "Nao encosta nessa alavanca... tarde demais. Bom, agora voce e parte do teste.",
                    "Entra rapido! A engenhoca nova esta quase estavel. Quase.",
                    "Ah, perfeito! Preciso de alguem com dois bracos. Voce tem dois, certo?"
                },
                Role1 = "Nimble, inventora-chefe da oficina. Crio mecanismos para a fazenda, para a mina e, de vez em quando, para o caos.",
                Role2 = "Minha primeira invencao foi um espantalho giratorio. Espantou os corvos, duas cabras e o telhado do vizinho. Evolui desde entao.",
                Town1 = "Esta cidade tem potencial mecanico ABSURDO. Imagine: esteiras da pedreira ate a forja! Roda d'agua dupla! Ninguem me ouve.",
                Town2 = "O Gurd construiu metade da cidade. Eu mantenho a metade que se move.",
                Service1 = "Vendo dispositivos, pecas e ferramentas de precisao. Conserto engenhocas, inclusive as que nao fui eu que estraguei.",
                Service2 = "Engrenagens antigas da caverna sao um TESOURO. Traga e eu pago bem. Ou troco por invencao. A escolha (arriscada) e sua.",
                AdviceLines = new[]
                {
                    "Oleo na engrenagem toda lua. TODA lua.",
                    "Se a maquina fizer um som novo, ela esta tentando te contar algo. Escute.",
                    "Nunca teste invencao minha em ambiente fechado. Aprendi por voce."
                },
                Rumor1 = "Achei um mecanismo na caverna que GIRA SOZINHO. Sem corda, sem peso, sem mola. Esta na minha bancada. Girando. Ha tres semanas.",
                Rumor2 = "A Ozzra pediu um agitador automatico de poções. Combinamos que se explodir, a culpa e dividida: 60% dela, 40% minha.",
                Goodbyes = new[] { "Volta amanha! A versao 2 estara pronta!", "Sai pela esquerda! A direita esta... em manutencao.", "Leva esse parafuso. Confia, leva." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_gurd", DialogueSetId = "dialogue_gurd", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado com a vida ai embaixo! Ah, oi. Pensei que era a viga.",
                    "Dia bom pra construir! Todo dia e dia bom pra construir.",
                    "Se veio ajudar, pega um capacete. Se veio olhar, pega um capacete tambem."
                },
                Role1 = "Gurd, mestre de obras. Casa, celeiro, muro, ponte... se fica em pe e era pra ficar em pe, fui eu.",
                Role2 = "Construi meu primeiro celeiro aos quatorze. Caiu aos quinze. O segundo esta de pe ate hoje. Falha e fundacao do oficio.",
                Town1 = "A cidade cresce mais rapido que minha equipe. Bom problema. Cansativo, mas bom.",
                Town2 = "A praca nova ficou bonita, ne? A base da estatua fui eu que reforcei. Aquilo NAO cai nem com terremoto.",
                Service1 = "Construo e reformo. Para fazenda tenho pacote: fundacao, estrutura e telhado em dez dias, se a pedra da Dagna chegar.",
                Service2 = "Compro madeira de qualidade e pedra aparelhada. Material da caverna tambem, se nao estiver... estranho.",
                AdviceLines = new[]
                {
                    "Fundacao primeiro, pressa depois.",
                    "Madeira verde entorta. Deixa secar uma estacao.",
                    "Telhado bom se faz no verao pra agradecer no inverno."
                },
                Rumor1 = "Tem uma rachadura no muro leste que conserto toda semana. Toda semana ela volta. No mesmo desenho. Vou chamar o Corvus.",
                Rumor2 = "Escavando pra fundacao nova, achamos tijolo ANTIGO. Mais antigo que a cidade. Quem construiu aqui antes de nos?",
                Goodbyes = new[] { "Vai com cuidado e longe do andaime!", "Obra te chama, eu atendo!", "Devolve o capacete na saida!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_yael", DialogueSetId = "dialogue_yael", HasShop = true,
                Greetings = new[]
                {
                    "Shhh. Fale baixo. Os melhores negocios sussurram.",
                    "Voce achou o mercado noturno. Ou ele achou voce.",
                    "Bem-vindo. Aqui vendemos o que o dia nao explica."
                },
                Role1 = "Yael. Mercadora do anoitecer. Minha banca abre quando as outras fecham, e vende o que as outras nao tem coragem.",
                Role2 = "Vim de longe, de uma cidade portuaria que nao existe mais. Nao pergunte. Ou pergunte; a historia custa uma compra.",
                Town1 = "Toda cidade tem duas faces. Eu atendo a que aparece depois do ultimo sino.",
                Town2 = "A guarda me tolera porque sou util. Sei o que se move na noite, e as vezes... compartilho.",
                Service1 = "Itens raros, curiosidades da caverna, essencias e segredos engarrafados. Tudo com procedencia. Procedencia noturna, mas procedencia.",
                Service2 = "Compro achados incomuns sem perguntas. E pago em ouro ou em informacao. A segunda moeda vale mais.",
                AdviceLines = new[]
                {
                    "O que brilha demais na caverna geralmente e isca.",
                    "Compre na luz, venda na sombra. Ou o contrario. Depende do item.",
                    "Nunca conte tudo que voce tem. Nem pra mim. PRINCIPALMENTE pra mim."
                },
                Rumor1 = "Alguem anda comprando TODA pedra negra que aparece. Pagando triplo. Por tras de intermediario. Eu odeio nao saber quem.",
                Rumor2 = "O mercador errante da caverna? Real. Nos cruzamos uma vez. Ele me vendeu um mapa... do mercado noturno. O MEU mercado. Antes de eu monta-lo.",
                Goodbyes = new[] { "A noite te acompanha.", "Nao me viu, nao falamos. Mas volte.", "Leve o embrulho. Nao abra na frente da guarda." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_maelor", DialogueSetId = "dialogue_maelor", HasShop = false,
                Greetings = new[]
                {
                    "...Voce tambem nao consegue dormir?",
                    "A noite esta falando hoje. Escute.",
                    "Ah. Um rosto desperto. Raro a esta hora."
                },
                Role1 = "Maelor. Eu caminho. As pessoas chamam de ronda, vigilia, mania... eu chamo de escuta. A cidade conta coisas a quem anda devagar.",
                Role2 = "Ja fui marinheiro, num mar que voce nao encontra nos mapas daqui. A agua me trouxe terra adentro. Ainda estou entendendo por que.",
                Town1 = "De noite a cidade tira a mascara. As pedras esfriam, os sonhos vazam pelas janelas. E meu horario favorito.",
                Town2 = "A estatua do guerreiro... eu converso com ela. Ela nunca responde. Mas escuta melhor que muita gente viva.",
                Service1 = "Nao vendo nada. Mas se algo se perder na noite — um objeto, um animal, uma pessoa — eu costumo saber onde a noite guarda as coisas.",
                Service2 = "Ouvi dizer que voce desce a caverna. Eu sinto a caverna daqui de cima, sabia? Ela tem... mares. Desca na mare baixa.",
                AdviceLines = new[]
                {
                    "Hoje a caverna esta inquieta. Va amanha.",
                    "Se ouvir seu nome no escuro, nao responda na primeira. Nem na segunda.",
                    "O sono e uma porta. Tranque-a por dentro."
                },
                Rumor1 = "Ha uma lua que so aparece refletida na Fonte. Olhe na agua numa noite limpa. Conte as luas. Depois me conte voce.",
                Rumor2 = "As cartas que recebo cheiram a sal porque o remetente ainda navega. Em qual mar, eu nao sei. Talvez embaixo de nos.",
                Goodbyes = new[] { "Durma, se conseguir.", "A noite e longa. Eu cuido dela.", "Va. Os sonhos nao gostam de esperar." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_zrix", DialogueSetId = "dialogue_zrix", HasShop = true,
                Greetings = new[]
                {
                    "Voltando da caverna ou indo? A resposta muda meu conselho.",
                    "Estrada limpa hoje. La embaixo... bem, la embaixo nunca esta limpa.",
                    "Ei. Verifique as botas antes de descer. Sempre."
                },
                Role1 = "Zrix, batedor da estrada da caverna. Patrulho o caminho entre a cidade e a boca do abismo. Alguem precisa.",
                Role2 = "Ja desci ate o nivel trinta e cinco. Voltei com este cicatriz e esta regra: a pressa escolhe o tumulo.",
                Town1 = "A cidade vive da caverna mais do que admite. Metade do ouro da praca subiu de la nas costas de alguem.",
                Town2 = "Vejo os novatos descendo com brilho nos olhos. Meu trabalho e garantir que subam com o brilho ainda aceso.",
                Service1 = "Vendo suprimentos de descida: tochas, cordas, racoes secas e antidoto basico. O kit que separa susto de tragedia.",
                Service2 = "Tambem marco seu nome no quadro de descidas. Se nao voltar no prazo, eu mesmo desco para buscar. Ja trouxe sete de volta.",
                AdviceLines = new[]
                {
                    "Nivel novo, regra nova. Nao assuma nada do andar anterior.",
                    "Inimigo que recua nao desistiu. Esta te levando pra algum lugar.",
                    "Marque seu caminho. A caverna gosta de embaralhar quem confia na memoria."
                },
                Rumor1 = "Os bichos do nivel dez estao descendo pro doze. Algo la em cima dos niveis esta... empurrando eles pra baixo. O que empurra monstro?",
                Rumor2 = "Vi o mercador errante duas vezes no mesmo dia. Em niveis diferentes. DISTANTES. Ou ele tem um irmao, ou as regras dele sao outras.",
                Goodbyes = new[] { "Desca devagar, suba inteiro.", "Te vejo no quadro de retorno.", "Boa sorte. E conta as tochas DUAS vezes." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_savra", DialogueSetId = "dialogue_savra", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado onde pisa. Essa florzinha ai leva dois anos pra crescer.",
                    "O bosque esta generoso hoje. Colhi tres cestas antes do sol alto.",
                    "Bem-vindo a borda do verde. A cidade termina aqui; a floresta so comeca."
                },
                Role1 = "Savra, herborista do portao da floresta. Colho, seco, misturo e curo. As plantas falam; eu so traduzo.",
                Role2 = "Minha mae me ensinou as ervas. A floresta me ensinou o resto, geralmente do jeito dificil.",
                Town1 = "A cidade e a floresta fazem um acordo silencioso: ela nos da remedio e madeira, nos damos respeito. Quando alguem quebra o acordo, eu ouco primeiro.",
                Town2 = "O chá que a Gruta serve no inverno? Mistura minha. O perfume da Mirela? Tambem. A cidade cheira a meu jardim e nem sabe.",
                Service1 = "Vendo ervas medicinais, antidotos, chas e unguentos. Para quem desce a caverna, o pacote anti-veneno e obrigatorio.",
                Service2 = "Compro plantas raras, especialmente as da caverna. Musgo que brilha, flor que cresce no escuro... pago muito bem.",
                AdviceLines = new[]
                {
                    "Folha em par, pode tocar. Folha trincada, deixa quieta.",
                    "Antidoto vence. Confira a data antes de descer.",
                    "Mel resolve tosse, briga e negociacao. Leve mel."
                },
                Rumor1 = "Tem um cogumelo novo crescendo na boca da caverna. Nao esta em nenhum dos meus livros. Plantei um em vaso... ele virou na direcao da caverna.",
                Rumor2 = "Os passaros pararam de fazer ninho na arvore alta do portao leste. Passaro sabe das coisas antes da gente.",
                Goodbyes = new[] { "Va pelo caminho marcado!", "Leve agua. E respeito.", "Que o verde te acompanhe." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_ozzra", DialogueSetId = "dialogue_ozzra", HasShop = true,
                Greetings = new[]
                {
                    "NAO respire fundo ainda! ...Pronto. Agora pode. Bem-vindo!",
                    "Ah, otimo, um voluntario! Brincadeira. A menos que...",
                    "Entre! O cheiro e normal. O normal daqui, claro."
                },
                Role1 = "Ozzra, alquimista. Transformo planta, mineral e coragem alheia em pocao, elixir e ocasionalmente fumaca roxa.",
                Role2 = "Estudei em Dornecia ate me convidarem a 'pesquisar em outro lugar'. O laboratorio deles era pequeno demais para minhas ideias. Literalmente. Explodiu a parede leste.",
                Town1 = "Esta cidade e perfeita para alquimia: ervas da Savra, minerios da caverna e vizinhos compreensivos. Ou surdos. Nunca perguntei.",
                Town2 = "A agua da Fonte tem propriedades que desafiam meus instrumentos. E meus instrumentos foram feitos pela Nimble, entao ja desafiavam bastante coisa.",
                Service1 = "Vendo pocoes de vida, frascos de energia e reagentes. Tudo testado! Em mim, geralmente. Por isso o desconto de quinta-feira.",
                Service2 = "Traga ingredientes exoticos da caverna e fazemos negocio. Olho de criatura, cristal ressonante, qualquer coisa que pulse.",
                AdviceLines = new[]
                {
                    "Pocao vermelha cura. Pocao roxa... depende. Pergunte antes.",
                    "Nunca misture pocoes no estomago. Misture no frasco, como gente civilizada.",
                    "Frasco vazio tambem vale ouro. Devolva e ganhe desconto."
                },
                Rumor1 = "Destilei a agua de uma poca do nivel oito. O residuo... se move. Guardei no armario triplo. O armario anda arranhado por DENTRO.",
                Rumor2 = "A pedra negra reage a agua da Fonte. Reage MUITO. O conselho me proibiu de repetir o teste. Foi um muro so, gente.",
                Goodbyes = new[] { "Saia antes que algo borbulhe!", "Volte com frascos vazios e curiosidade cheia!", "Se sentir gosto de metal, volta aqui CORRENDO." }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_eiran", DialogueSetId = "dialogue_eiran", HasShop = true,
                Greetings = new[]
                {
                    "Ei! Voce assustou as galinhas. Brincadeira, elas assustam sozinhas.",
                    "Bem-vindo ao quintal mais barulhento da cidade!",
                    "Chegou na hora da ordenha! Quer aprender? E mais zen do que parece."
                },
                Role1 = "Eiran, criador de animais. Galinha, cabra, vaca e o que mais aparecer berrando no meu portao.",
                Role2 = "Os bichos me entendem melhor que as pessoas. E mais sincero o relacionamento: eu trato bem, eles produzem. Sem fofoca.",
                Town1 = "O leite da praca, os ovos da estalagem e a la da Mirela saem daqui. A cidade toma cafe da manha no meu quintal.",
                Town2 = "Animal sente a cidade. Quando algo vai mal, eles avisam primeiro. Ultimamente... andam avisando.",
                Service1 = "Vendo ovos, leite, la e racao. Filhotes na primavera, para quem tiver espaco e paciencia.",
                Service2 = "Cuido de animal de fazenda enquanto voce desce a caverna. Taxa justa, bicho volta gordo.",
                AdviceLines = new[]
                {
                    "Galinha feliz bota mais. Musica ajuda. Nao pergunte como descobri.",
                    "Cabra que olha muito pra cerca ja decidiu pular. Reforce antes.",
                    "Animal novo, quarentena. Sempre."
                },
                Rumor1 = "As cabras se recusam a pastar perto da entrada da caverna desde a lua passada. CABRAS. Que comem ate avental.",
                Rumor2 = "O galo cantou meia-noite em ponto, tres noites seguidas. Meu avo dizia que isso anuncia visita... de longe. MUITO longe.",
                Goodbyes = new[] { "Vai la! E fecha o portao, pelas cabras!", "Leva ovo fresco, ta na cesta!", "Volta pra ordenha de domingo!" }
            },
            new NpcDialogueContent
            {
                NpcId = "npc_liora", DialogueSetId = "dialogue_liora", HasShop = false,
                Greetings = new[]
                {
                    "Shh... estou compondo. Pronto, perdi. Era linda. Culpa sua. Brincadeira: era mediana.",
                    "Voce chega como um acorde inesperado. Fica para o refrao?",
                    "O jardim da estatua tem a melhor acustica da cidade. E o melhor publico: ele nunca vaia."
                },
                Role1 = "Liora, musica e poeta. Canto nas tavernas, nos festivais e para a estatua, que e meu critico mais honesto.",
                Role2 = "Aprendi musica ouvindo o vento nos canaviais de Vaalara. Vim para ca atras de uma melodia que sonhei. Ainda nao a encontrei. Mas ouco pedacos dela... vindos do chao.",
                Town1 = "Cada cidade tem uma cancao escondida. A desta e em tom menor, mas com esperanca no refrao. Como o nome dela, alias.",
                Town2 = "O guerreiro da estatua... componho sobre ele ha anos. Quem ergue a espada para o ceu e segura o escudo para o povo? Um protetor. Ou um pedido de socorro em bronze.",
                Service1 = "Nao vendo nada; ofereco. Musica na praca ao entardecer, versos por um sorriso. Se quiser uma cancao sua, traga-me uma historia verdadeira.",
                Service2 = "Para festivais e festas, toco mediante convite e janta. A Gruta sabe meu repertorio de taverna; o Corvus, o de templo.",
                AdviceLines = new[]
                {
                    "Cante na caverna. Os ecos respondem... e os que respondem errado, evite.",
                    "Toda colheita merece uma cancao. Ate a ruim. PRINCIPALMENTE a ruim.",
                    "Ouca mais o silencio entre as palavras do que as palavras."
                },
                Rumor1 = "Quando toco certa sequencia de notas perto da estatua, o vento muda. Tres notas. Sempre as mesmas. Nao toco mais a quarta.",
                Rumor2 = "A melodia que sonhei? Um mineiro a assobiou semana passada. Disse que ouviu 'la embaixo, no fundo'. Ele nunca tinha me visto cantar.",
                Goodbyes = new[] { "Que sua estrada rime.", "Volte ao entardecer; a luz ajuda a musica.", "Vou colocar voce numa cancao. A parte boa, prometo." }
            },
        };

        public static IReadOnlyList<NpcDialogueContent> AllContent => s_content;

        public static bool TryGetContent(string npcId, out NpcDialogueContent content)
        {
            content = null;
            foreach (var entry in s_content)
            {
                if (entry.NpcId == npcId)
                {
                    content = entry;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds the standard 13-node tree from one NPC's content block.
        /// Node graph: greeting → (role → role_2), (town → town_2), (service → service_2),
        /// advice, (rumor → rumor_2), hub, goodbye. Every branch returns to node_hub.
        /// </summary>
        public static List<DialogueNode> BuildNodes(NpcDialogueContent content)
        {
            var hubChoices = BuildHubChoices(content);

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    NodeId = "node_greeting",
                    Text = content.Greetings[0],
                    RandomLinePool = new List<string>(content.Greetings),
                    Choices = CloneChoices(hubChoices)
                },
                new DialogueNode
                {
                    NodeId = "node_role",
                    Text = content.Role1,
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { Label = "Conte mais.", NextNodeId = "node_role_2" },
                        BackToHubChoice()
                    }
                },
                new DialogueNode
                {
                    NodeId = "node_role_2",
                    Text = content.Role2,
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                },
                new DialogueNode
                {
                    NodeId = "node_town",
                    Text = content.Town1,
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { Label = "E o que mais?", NextNodeId = "node_town_2" },
                        BackToHubChoice()
                    }
                },
                new DialogueNode
                {
                    NodeId = "node_town_2",
                    Text = content.Town2,
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                },
                new DialogueNode
                {
                    NodeId = "node_service",
                    Text = content.Service1,
                    Choices = BuildServiceChoices(content)
                },
                new DialogueNode
                {
                    NodeId = "node_service_2",
                    Text = content.Service2,
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                },
                new DialogueNode
                {
                    NodeId = "node_advice",
                    Text = content.AdviceLines[0],
                    RandomLinePool = new List<string>(content.AdviceLines),
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                },
                new DialogueNode
                {
                    NodeId = "node_rumor",
                    Text = content.Rumor1,
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { Label = "Tem mais alguma historia?", NextNodeId = "node_rumor_2" },
                        BackToHubChoice()
                    }
                },
                new DialogueNode
                {
                    NodeId = "node_rumor_2",
                    Text = content.Rumor2,
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                },
                new DialogueNode
                {
                    NodeId = "node_hub",
                    Text = "Mais alguma coisa?",
                    Choices = CloneChoices(hubChoices)
                },
                new DialogueNode
                {
                    NodeId = "node_goodbye",
                    Text = content.Goodbyes[0],
                    RandomLinePool = new List<string>(content.Goodbyes),
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { Label = "Ate logo.", NextNodeId = string.Empty, ActionType = DialogueActionType.CloseDialogue }
                    }
                },
                new DialogueNode
                {
                    NodeId = "node_smalltalk",
                    Text = "Hm. O dia segue, o trabalho tambem.",
                    RandomLinePool = new List<string>
                    {
                        "Hm. O dia segue, o trabalho tambem.",
                        "Esses ceus de Vaalara... nunca se repetem.",
                        "Dizem que a Fonte esta mais clara esta semana."
                    },
                    Choices = new List<DialogueChoice> { BackToHubChoice() }
                }
            };

            return nodes;
        }

        private static List<DialogueChoice> BuildHubChoices(NpcDialogueContent content)
        {
            var choices = new List<DialogueChoice>
            {
                new DialogueChoice { Label = "Quem e voce?", NextNodeId = "node_role" },
                new DialogueChoice { Label = "Como vao as coisas na cidade?", NextNodeId = "node_town" },
                new DialogueChoice { Label = "Em que voce trabalha?", NextNodeId = "node_service" },
                new DialogueChoice { Label = "Algum conselho?", NextNodeId = "node_advice" },
                new DialogueChoice { Label = "Ouviu algo interessante?", NextNodeId = "node_rumor" },
                new DialogueChoice { Label = "Adeus.", NextNodeId = "node_goodbye" }
            };
            return choices;
        }

        private static List<DialogueChoice> BuildServiceChoices(NpcDialogueContent content)
        {
            var choices = new List<DialogueChoice>();
            if (content.HasShop)
            {
                choices.Add(new DialogueChoice
                {
                    Label = "Mostre o que voce vende.",
                    NextNodeId = string.Empty,
                    ActionType = DialogueActionType.OpenShop
                });
            }

            choices.Add(new DialogueChoice { Label = "Interessante, continue.", NextNodeId = "node_service_2" });
            choices.Add(BackToHubChoice());
            return choices;
        }

        private static DialogueChoice BackToHubChoice()
        {
            return new DialogueChoice { Label = "Voltar.", NextNodeId = "node_hub" };
        }

        private static List<DialogueChoice> CloneChoices(List<DialogueChoice> source)
        {
            var clones = new List<DialogueChoice>(source.Count);
            foreach (var choice in source)
            {
                clones.Add(new DialogueChoice
                {
                    Label = choice.Label,
                    NextNodeId = choice.NextNodeId,
                    ActionType = choice.ActionType,
                    ActionPayload = choice.ActionPayload
                });
            }

            return clones;
        }
    }
}
