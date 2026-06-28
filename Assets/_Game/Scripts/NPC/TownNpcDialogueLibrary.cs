using System.Collections.Generic;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Single source of truth for the expanded town NPC dialogue content (WAVE25+ expansion).
    /// Each NPC gets a standard 13-node tree: greeting hub (with random line pool), role,
    /// town context, service, advice, rumor branches and a goodbye, all with return-to-hub
    /// choices. The editor menu "CindarsHope/NPCs/Rebuild Town NPC Dialogues" writes this
    /// content into the DialogueTreeSO assets; NpcDialogueSetRegistry reports node counts
    /// from here so registry and content never drift apart.
    ///
    /// fable_28 — the greeting node also carries a per-NPC <see cref="DialogueNode.ConditionalLines"/>
    /// pool (season/rain/festival/friendship-band/main-quest milestone) authored in the roster v1.1
    /// voice. The base <c>Greetings</c> stay as the guaranteed fallback (selection never empty).
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

            // fable_10 — optional main-quest offer. When set, the service node exposes an
            // OfferQuest choice (same DialogueActionType.OfferQuest pattern as Thalindra's quest
            // tree). The QuestGiverInteractable on the NPC is the canonical accept path; this
            // dialogue choice is the in-conversation entry point. Node count stays at 13.
            public string OfferedMainQuestId;
            public string OfferQuestLabel;

            // fable_28 — conditional greeting pool (all optional; null entries simply contribute
            // nothing to the pool). Authored in the NPC's voice.
            public string SpringLine;   // estação Primavera
            public string SummerLine;   // estação Verao
            public string AutumnLine;   // estação Outono
            public string WinterLine;   // estação Inverno
            public string RainLine;     // clima chuvoso (Rainy/Stormy)
            public string FestivalLine; // dia de festival genérico
            public string FriendStrangerLine; // amizade 0-1
            public string FriendWarmLine;      // amizade 2-3
            public string FriendCloseLine;     // amizade 4-5
            public string MilestoneArrivalLine;   // marco: chegada (flag_main_arrival)
            public string MilestonePostAct1Line;  // marco: pós-Ato-1 (flag_main_post_act1)
            public string MilestonePostAct3Line;  // marco: pós-Ato-3 (flag_main_post_act3)
        }

        public const int NodesPerNpc = 13;

        // fable_28 — friendship band thresholds (FriendshipLevel 0-5): stranger 0-1, warm 2-3, close 4-5.
        public const int FriendshipWarmMin = 2;
        public const int FriendshipCloseMin = 4;

        // fable_28 — synthetic main-quest milestone flag ids. The main-quest system sets these; this
        // library only authors the lines gated on them (RequiredFlag). Stable ids, never renamed.
        public const string FlagMainArrival = "flag_main_arrival";
        public const string FlagMainPostAct1 = "flag_main_post_act1";
        public const string FlagMainPostAct3 = "flag_main_post_act3";

        private static readonly List<NpcDialogueContent> s_content = new List<NpcDialogueContent>
        {
            new NpcDialogueContent
            {
                NpcId = "npc_corvus", DialogueSetId = "dialogue_corvus", HasShop = true,
                Greetings = new[]
                {
                    "Que a balança de Kanthor pese leve sobre você. Aqui, até o silêncio tem peso.",
                    "Bem-vindo ao templo. Foi erguido sobre pedra mais antiga que a cidade — e pedra antiga não esquece.",
                    "Chegou na hora da vela. Acendo uma por cada alma que desceu ao escuro e não voltou inteira."
                },
                Role1 = "Sou Corvus, guardião do templo de Kanthor em Cindar's Hope. Cuido das oferendas, das velas, e das memórias que ninguém mais tem coragem de carregar.",
                Role2 = "Antes deste manto, desci a caverna como qualquer outro. O que vi no escuro não me afastou da Fonte de Anya — me empurrou para perto dela, de joelhos.",
                Town1 = "A cidade resiste porque ainda tem o que defender. Enquanto o Guerreiro de pedra vigiar a praça, o povo lembra por que veio parar neste vale.",
                Town2 = "Dizem que Cindar's Hope foi erguida sobre uma esperança antiga. Poucos percebem que o nome também é um aviso. Acendo vela pelos dois sentidos.",
                Service1 = "Ofereço incenso, água benta e amuletos simples. Nenhum substitui coragem — mas todos acalmam o coração de quem vai descer ao escuro.",
                Service2 = "Traga fragmentos da caverna e eu os examino. O templo guarda registros sobre a pedra negra que a cidade preferiu enterrar e esquecer.",
                AdviceLines = new[]
                {
                    "Desça descansado. O cansaço mata mais fundo que qualquer monstro.",
                    "Na dúvida, volte ao último ponto de luz. Kanthor honra quem sabe recuar.",
                    "Não carregue tudo. Largar no tempo certo também é uma forma de fé."
                },
                Rumor1 = "Nas noites de Alihana, a lua branca, juram que a água da Fonte aquece sozinha. Já senti o vapor subir diferente — e ainda não sei se rezo ou se temo.",
                Rumor2 = "Um mineiro voltou do nível trinta falando de cantos no escuro. Cantos, não gritos. Canto que chama assusta mais que grito que avisa.",
                Goodbyes = new[] { "Que a Fonte o acompanhe.", "Volte quando o peso for grande demais para carregar sozinho.", "Vá em paz. E, por Kanthor, volte inteiro." },
                SpringLine = "A primavera reacende as velas com mais facilidade. Até a cera parece perdoar mais rápido nesta estação.",
                SummerLine = "Verão. O templo guarda sombra enquanto lá fora o sol castiga. Entre, descanse a alma junto com o suor.",
                AutumnLine = "No outono, trazem folha seca e gratidão no lugar de moeda. Confesso que prefiro a oferenda que não se conta.",
                WinterLine = "Inverno em Cindar's Hope. O frio empurra o povo para perto da Fonte. A dor sempre soube o caminho do templo.",
                RainLine = "A chuva lava a praça e traz fiéis encharcados. Seque o manto perto das velas — Kanthor não exige sofrimento, só honestidade.",
                FestivalLine = "Dia de festa. O templo não disputa com a praça; apenas guarda um canto de silêncio para quem cansar do barulho.",
                FriendStrangerLine = "Que a balança pese leve sobre você, viajante. Ainda não sei seu nome, mas já reconheço o seu cansaço.",
                FriendWarmLine = "Você de novo. Já distingo seus passos no corredor. Para um guardião, reconhecer um passo já é quase confiar.",
                FriendCloseLine = "Ah, é você. Sente-se. Guardei uma vela acesa apostando que viria. Há presenças que a gente aprende a esperar como quem reza.",
                MilestoneArrivalLine = "Você chegou há pouco a Cindar's Hope. A Fonte já o notou — ela nota todos que descem o vale carregando perguntas.",
                MilestonePostAct1Line = "Depois do que houve, a cidade respira diferente. Passei a acender uma vela a mais por noite. Por precaução, ou por esperança.",
                MilestonePostAct3Line = "Você mexeu no rumo de coisas mais velhas que esta cidade. O templo guardará seu nome ao lado do Guerreiro da praça.",
                // fable_10 — Corvus abre o Ato 1 (mq_act1_01) e recebe a entrega (mq_act1_05). O
                // QuestGiverInteractable resolve qual etapa ofertar/entregar; o dialogo so abre o canal.
                OfferedMainQuestId = "mq_act1_01_fonte_adormecida",
                OfferQuestLabel = "Fale-me da Fonte adormecida."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_mara", DialogueSetId = "dialogue_mara", HasShop = true,
                Greetings = new[]
                {
                    "Bom dia. Documentos, registros ou queixas? Atendo os três — mas só dois com sorriso.",
                    "Chegou cedo. A fila de papelada continua do tamanho de ontem, e ontem foi generoso.",
                    "Se veio registrar terra, sente-se. Se veio registrar queixa, respire primeiro."
                },
                Role1 = "Mara, escrivã do registro cívico, sob o selo de Merithus. Cada lote, cada licença e cada promessa desta cidade passa pela minha mesa antes de existir de verdade.",
                Role2 = "Pode parecer só papel. É memória. Quando alguém não volta da caverna, é o meu registro que jura ao mundo que aquela pessoa existiu.",
                Town1 = "A cidade cresce devagar, mas cresce com nome e data. Prefiro dez casas bem registradas a cinquenta erguidas no improviso e no esquecimento.",
                Town2 = "O conselho discute esticar a rua do mercado. Se sair do papel, você ouviu aqui primeiro — e por escrito, que é como as coisas duram.",
                Service1 = "Emito segundas vias, autentico contratos de compra e venda, e registro melhorias de fazenda sob fé pública.",
                Service2 = "Também guardo mapas antigos do vale. Cópias custam pouco; os originais não saem daqui nem por ordem da coroa de Dornécia.",
                AdviceLines = new[]
                {
                    "Registre suas colheitas grandes. Papel hoje evita briga amanhã.",
                    "Leia tudo antes de assinar. Até o que eu lhe entrego.",
                    "Guarde recibos. A memória falha; a tinta de Merithus, não."
                },
                Rumor1 = "Há um lote abandonado perto do portão sul cujo dono nunca apareceu. Faz vinte anos. O registro continua aberto, como uma boca que não fecha.",
                Rumor2 = "Yael pediu licença para o mercado noturno três vezes. Negada duas. Na terceira, alguém de muito acima assinou — sob a lua de Nyx, dizem. Curioso, não?",
                Goodbyes = new[] { "Próximo!", "Leve seus papéis, não os meus.", "Até logo. E não perca o protocolo — ele é a sua existência." },
                SpringLine = "Primavera: época de registrar lote novo. A fila de licença de plantio dobra. Pegue uma senha e tenha paciência de semente.",
                SummerLine = "O verão seca a tinta rápido demais. Assine com calma, ou seu nome fica borrado para sempre — e nome borrado é nome perdido.",
                AutumnLine = "Outono é mês de prestação de contas. Traga seus recibos de colheita antes que eu vá atrás deles. E eu vou.",
                WinterLine = "Inverno. Menos gente no balcão, mais papelada atrasada para pôr em ordem. Aproveito o frio para arquivar o ano inteiro.",
                RainLine = "Chuva? Sacuda o casaco antes de entrar. Documento molhado é documento perdido — e eu não reescrevo o que o tempo apagou.",
                FestivalLine = "Dia de festival até o registro fecha cedo. Mas se for urgente, eu carimbo. Só hoje. Só por você.",
                FriendStrangerLine = "Bom dia. Você ainda não consta no meu registro. Vamos resolver isso: nome, origem e motivo. Memória oficial começa por aí.",
                FriendWarmLine = "Ah, já conheço o seu protocolo. Sente-se — sua papelada hoje anda mais rápido. Eficiência também se constrói com confiança.",
                FriendCloseLine = "Você de novo, e bem-vindo. Confesso que separo seus documentos com um cuidado que não dou aos outros. Não conte a ninguém.",
                MilestoneArrivalLine = "Você é novo na cidade. Vou abrir um registro com seu nome agora. A partir desta linha, você existe oficialmente em Cindar's Hope.",
                MilestonePostAct1Line = "Depois daqueles dias, tive que abrir uma pasta nova só para os fatos que não cabem na lei. Você está nela. No topo.",
                MilestonePostAct3Line = "Seu nome agora aparece em atas que vão durar mais que nós dois. Cuide bem dessa tinta — é assim que um mortal vira história."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_tovin", DialogueSetId = "dialogue_tovin", HasShop = true,
                Greetings = new[]
                {
                    "Licenças e alvarás. Se não for isso, é no balcão da Mara.",
                    "Ah, um rosto novo. Espero que com a papelada em dia — o carimbo é justo, mas não é paciente.",
                    "Bem-vindo ao balcão de permissões. A burocracia de Merithus agradece sua paciência."
                },
                Role1 = "Tovin, oficial de permissões sob o selo de Merithus. Construção, comércio ambulante, expedição em grupo à caverna — tudo precisa do meu carimbo antes de ser legal.",
                Role2 = "Já neguei alvará a gente importante. O carimbo não reconhece sobrenome, só requisito. É a única justiça que esta cidade consegue garantir todo dia.",
                Town1 = "Cidade organizada é cidade viva. O caos parece charmoso até o primeiro incêndio sem registro de quem ergueu o quê.",
                Town2 = "O fluxo de aventureiros dobrou desde a primavera. Bom para o comércio, péssimo para a minha pilha de pedidos — e para o meu sono.",
                Service1 = "Vendo selos de autorização e formulários prontos. Com eles, você resolve em um dia o que levaria uma semana de fila.",
                Service2 = "Quer montar banca no mercado? Traga o formulário M-3 preenchido e o ouro da taxa. Do resto, cuido eu.",
                AdviceLines = new[]
                {
                    "Peça a licença antes de construir, nunca depois. Depois custa o dobro e a vergonha.",
                    "Taxa atrasada dobra. Sempre. Não teste a paciência de Merithus.",
                    "Se um formulário parecer inútil, preencha mesmo assim. Ele protege você de coisas que ainda não imagina."
                },
                Rumor1 = "Alguém pediu licença para 'pesquisa de fauna subterrânea'. Nível cinquenta para baixo. O conselho nem sabia que havia algo lá para pesquisar.",
                Rumor2 = "Ouvi que Gurd quer erguer um muro novo no lado leste. O orçamento... bem, é otimista como quem nunca cavou a própria fundação.",
                Goodbyes = new[] { "Carimbado. Próximo.", "Boa sorte com a papelada.", "Volte com o formulário certo — e só com ele." },
                SpringLine = "A primavera traz pedido de banca de mercado em pilha. Plante o seu alvará antes da colheita de gente que vem atrás dele.",
                SummerLine = "Verão é temporada alta de aventureiro. Mais alvará de grupo, mais carimbo, menos mate quente para mim.",
                AutumnLine = "Outono: prazo de renovação. Quem não renovar o alvará antes da primeira geada paga dobrado. A lei não esfria.",
                WinterLine = "Inverno acalma o balcão. Bom para revisar formulários; ruim para a minha pilha de mate frio esquecido.",
                RainLine = "Com essa chuva o carimbo borra. Espere secar ou leve o documento manchado. Sua escolha, sua taxa.",
                FestivalLine = "Festival exige licença especial para barraca. Tenho o formulário F-9 aqui. Preenchido, é claro — eu não improviso nem em festa.",
                FriendStrangerLine = "Licenças e alvarás. Você ainda não tem ficha aqui? Então começamos pelo básico, sem atalho. Atalho vira processo.",
                FriendWarmLine = "Já reconheço o seu pedido de longe. Vou adiantar: traga o formulário certo e a gente termina antes do meu mate esfriar.",
                FriendCloseLine = "Você de novo. Para você, eu já deixo o carimbo na mão. Confiança também é um tipo de alvará — e o mais difícil de emitir.",
                MilestoneArrivalLine = "Recém-chegado. Seu primeiro carimbo nesta cidade sai daqui. Guarde-o; é a porta que abre todas as outras.",
                MilestonePostAct1Line = "Depois da confusão, o conselho apertou o controle de entrada. Mais formulário para todos. Inclusive para você. Lei nervosa é lei cega.",
                MilestonePostAct3Line = "Com o que você fez, liberaram alvarás que eu jurava que morreriam na gaveta. Até a burocracia abriu uma exceção. Impressionante."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_sylveth", DialogueSetId = "dialogue_sylveth", HasShop = true,
                Greetings = new[]
                {
                    "Sementes frescas, chegadas hoje! Sinta o cheiro de terra boa acordando.",
                    "Olá, fazendeiro! Suas plantas mandaram lembranças — e pedem água.",
                    "Bem-vindo! Se está com as mãos sujas de terra, já gosto de você. Thandra também."
                },
                Role1 = "Sylveth, vendedora de sementes e devota de Thandra. Minha banca tem desde trigo comum até cenoura premiada, e cada uma carrega uma estação inteira adormecida dentro.",
                Role2 = "Cresci numa fazenda a três vales daqui. Quando a colheita falhou e a fome bateu, aprendi que semente boa vale mais que ouro trancado em cofre.",
                Town1 = "Esta cidade come do que você planta. Cada canteiro seu alimenta alguém da praça. É o milagre mais discreto de Thandra.",
                Town2 = "O solo do vale é generoso, mas tem humor. Chuva demais na primavera, sede no verão. Plante sabendo com quem você negocia.",
                Service1 = "Vendo sementes de estação, mudas e adubo. Se a semente não germinar, troco sem discussão — Thandra não promete o que não cumpre.",
                Service2 = "De vez em quando aparece semente rara, vinda da caverna. Brilham no escuro. Plantei uma... e melhor eu não contar o que brotou.",
                AdviceLines = new[]
                {
                    "Regue de manhã. À tarde a água evapora antes de afundar na raiz.",
                    "Roda de culturas! Não repita a mesma planta no mesmo canteiro, ou a terra cansa e cobra.",
                    "Cenoura gosta de solo fofo. Capriche na enxada antes de caprichar na esperança."
                },
                Rumor1 = "O Eiran anda criando um filhote estranho no quintal. Jura que é cabra. Cabra não tem escama, Eiran. E cabra não encara a lua de Nyx daquele jeito.",
                Rumor2 = "Dizem que há cogumelos gigantes no nível quinze da caverna. Se alguém me trouxer esporos, pago bem — e planto longe de casa, por garantia.",
                Goodbyes = new[] { "Boa colheita!", "Vá com as mãos cheias e volte com elas vazias!", "Que a chuva venha na hora certa, e não na véspera." },
                SpringLine = "PRIMAVERA! Minha estação favorita! Sementes novas, terra acordando, tudo querendo brotar de uma vez. Sente esse cheiro? É Thandra respirando.",
                SummerLine = "Verão puxado. Regue de manhã, ouviu? À tarde o sol bebe a água antes da raiz. Tenho semente de sol aqui, dessas que gostam de castigo.",
                AutumnLine = "Outono é colheita farta e plantio de raiz. A terra dá o último empurrão antes de dormir. Aproveite a generosidade antes do sono dela.",
                WinterLine = "Inverno gela o solo, mas não a vontade de plantar. Tenho sementes resistentes e mudas de estufa — a vida não para, só se agasalha.",
                RainLine = "Chuva boa! O solo agradece e eu também. Dia de chuva é dia de planejar canteiro, não de regar. Thandra cuida da rega hoje. Folga!",
                FestivalLine = "Festival! Trouxe sementes especiais só para hoje. Plante uma lembrança do dia de festa no seu canteiro e colha a memória depois!",
                FriendStrangerLine = "Olá! Rosto novo na banca! Se as suas mãos ainda estão limpas de terra, a gente resolve isso rapidinho. Terra suja é terra amiga.",
                FriendWarmLine = "Ei, você! Já sei do que as suas plantas gostam. Separei uma semente pensando no seu canteiro — dá uma olhada antes que eu me arrependa.",
                FriendCloseLine = "Meu fazendeiro favorito chegou! Separei a melhor muda da safra para você. Amizade rende boa colheita; minha avó dizia, e Thandra concorda.",
                MilestoneArrivalLine = "Você é o da fazenda nova, né? Bem-vindo ao vale! Comece com semente fácil — a terra daqui tem humor e testa quem chega.",
                MilestonePostAct1Line = "Depois do susto, o solo parece mais firme. Ou é impressão de quem só quer ver tudo brotar de novo e esquecer o que tremeu lá embaixo.",
                MilestonePostAct3Line = "Dizem que até as sementes da caverna respiram diferente agora, graças a você. Plantei uma. Brilhou bonito — e dessa vez não tive medo."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_renko", DialogueSetId = "dialogue_renko", HasShop = true,
                Greetings = new[]
                {
                    "Renko tem de tudo! E o que não tem, consegue até amanhã — que Finan me ajude.",
                    "Cliente bom é cliente que volta. E você voltou! Finan sorriu para nós dois.",
                    "Entre, olhe, toque! Só não derrube a pilha de panelas — ela tem opinião própria."
                },
                Role1 = "Renko, comerciante geral e apostador honesto de Finan. Ferramentas, corda, lampião, panela, botão... se cabe numa prateleira eu vendo; se não cabe, eu negocio.",
                Role2 = "Comecei com uma mochila e duas rotas de caravana. Hoje tenho a maior banca da rua do mercado. Amanhã? Finan é generoso com quem ousa — talvez uma filial na capital.",
                Town1 = "O mercado é o coração da cidade. Quando a praça enche, até o Guerreiro de pedra parece sorrir para o movimento.",
                Town2 = "Concorrência? Eu chamo de vizinhança. A Mirela costura, o Brumdar forja, eu vendo o resto. Maré que enche um barco enche todos.",
                Service1 = "Compro excedente de colheita e materiais da caverna a preço justo. Justo de verdade — pode pesquisar de banca em banca.",
                Service2 = "Se procura algo raro, me diga. Tenho contatos nas caravanas que cruzam o vale, e Finan tem o hábito de pôr coisas estranhas no meu caminho.",
                AdviceLines = new[]
                {
                    "Compre corda. Ninguém nunca se arrependeu de ter corda.",
                    "Venda na alta, plante na baixa. Funciona para nabo e para minério.",
                    "Cliente apressado paga caro. Respire antes de negociar — a pressa é imposto que você cobra de si mesmo."
                },
                Rumor1 = "Uma caravana sumiu na estrada norte semana passada. Acharam as carroças... vazias e arrumadas. Arrumadas! Quem rouba e ainda organiza?",
                Rumor2 = "Dizem que existe um mercador que aparece DENTRO da caverna, sob a lua de Nyx. Se for verdade, é o meu concorrente mais corajoso — ou o menos vivo.",
                Goodbyes = new[] { "Volte sempre! E traga amigos!", "Negócio fechado e amizade mantida.", "Se faltar algo, já sabe onde achar." },
                SpringLine = "A primavera move o estoque! Todo mundo precisa de ferramenta nova para a terra acordada. Finan adora começo de safra!",
                SummerLine = "Verão quente vende corda, cantil e chapéu. Eu? Vendo de tudo, mas hoje o cantil sai voando da prateleira.",
                AutumnLine = "Outono é mês de estocar para o frio. Compre agora; no inverno o preço sobe e a culpa não é minha, é da estação.",
                WinterLine = "Inverno. Lampião, óleo e cobertor saem bem. E corda, sempre corda. Ninguém se arrepende de ter corda.",
                RainLine = "Chuva enche minha banca de gente abrigada e de poça. Pise com cuidado e leve um lampião à prova d'água!",
                FestivalLine = "Festival é dia de ouro! Lembrancinha, bandeira, fita... tudo no precinho de festa. Finan está solto hoje, aproveite!",
                FriendStrangerLine = "Cliente novo! Renko tem de tudo, e o que não tem, consegue até amanhã. Diga o que procura, sem vergonha — vergonha não paga conta.",
                FriendWarmLine = "Você voltou! Cliente bom é cliente que volta. Já vou separando aquilo que costuma levar, dá uma olhada.",
                FriendCloseLine = "Meu melhor cliente! Para você tem o preço de amigo e o café da casa. Negócio fechado e amizade mantida.",
                MilestoneArrivalLine = "Rosto novo na cidade! Bem-vindo. Comece a equipar essa mochila comigo; o resto da estrada agradece, e Finan também.",
                MilestonePostAct1Line = "Depois daquilo, as caravanas ficaram nervosas. Mercadoria sobe de preço. Mas para você eu seguro o que der — amizade tem desconto.",
                MilestonePostAct3Line = "Com o que você fez, até as rotas do norte reabriram! Vou ter mercadoria que esta cidade nunca viu. Que Finan abençoe a sua coragem. Obrigado!"
            },
            new NpcDialogueContent
            {
                NpcId = "npc_mirela", DialogueSetId = "dialogue_mirela", HasShop = true,
                Greetings = new[]
                {
                    "Essa costura do seu casaco... senta. Eu arrumo em dez minutos e ainda fico sabendo a sua história.",
                    "Bem-vindo ao ateliê! Cuidado com os alfinetes no chão — eles têm fome de pé descalço.",
                    "Olá, querido. Tecido novo chegou de Dornécia. Quer ver antes que o mercado todo queira?"
                },
                Role1 = "Mirela, alfaiate da cidade. Visto noivas, mineiros e todo mundo entre os dois. Pano é a segunda pele; eu cuido das duas.",
                Role2 = "Aprendi o ofício com minha avó. Ela dizia: roupa boa não esconde quem você é — ela apresenta. Costuro essa frase em cada bainha.",
                Town1 = "Pela roupa eu sei como vai a cidade. Muito casaco rasgado de caverna? Tempos difíceis. Muita roupa de festa? Colheita boa e coração leve.",
                Town2 = "A praça renovada deu vida nova à rua. Até o Brumdar passou a limpar a fuligem da barba. Milagres existem, e usam avental.",
                Service1 = "Faço reparos, ajustes e roupas sob medida. Tecido resistente para a caverna é o meu carro-chefe — costuro pensando em quem desce.",
                Service2 = "Couro de criatura da caverna rende casacos incríveis. Traga o material e eu desconto o trabalho. Só não me diga o que era o bicho.",
                AdviceLines = new[]
                {
                    "Costure os bolsos por dentro. Batedores de carteira odeiam isso.",
                    "Leve sempre agulha e linha na mochila. Já salvou mais expedição que espada.",
                    "Roupa molhada na caverna é convite para febre. Troque-se antes de tremer."
                },
                Rumor1 = "A Liora encomendou um vestido de palco cor de meia-noite. Disse que é para 'a noite em que a estátua cantar'. Poetas... mas guardei o molde.",
                Rumor2 = "O Maelor passa aqui toda lua nova de Nyx para remendar o mesmo casaco. Sempre o mesmo rasgo, no mesmo lugar. Nunca explica. Eu nunca pergunto.",
                Goodbyes = new[] { "Volte para a prova final!", "Cuide das suas bainhas, querido.", "Até mais. E nada de rasgar isso de novo!" },
                SpringLine = "A primavera pede tecido leve e cor viva. Chegou um linho de Dornécia que parece feito de pétala. Quer ver, querido?",
                SummerLine = "Verão é linho fino e chapéu de aba larga. Roupa pesada agora é castigo; deixe que eu alivio o seu corte.",
                AutumnLine = "Outono. Hora de forrar casaco e remendar o que o verão gastou. Traga suas peças antes do frio bater à porta.",
                WinterLine = "Inverno, querido! Lã, feltro e forro duplo. Roupa molhada na caverna é febre certa. Vista-se como gente que pretende voltar.",
                RainLine = "Com essa chuva, troque essa roupa encharcada antes que pegue um resfriado. Tenho uma peça seca aqui do seu tamanho, confie.",
                FestivalLine = "Festival! Todo mundo quer estar bonito. Tenho fita, gola e ajuste rápido. Senta que em dez minutos você brilha mais que a praça.",
                FriendStrangerLine = "Essa costura do seu casaco... senta, querido. Eu arrumo num instante. E, de quebra, fico sabendo o seu nome e a sua história.",
                FriendWarmLine = "Ah, você! Já sei suas medidas de cor. Separei um tecido que combina com o seu jeito — dá uma olhada antes que eu o reserve.",
                FriendCloseLine = "Meu cliente querido! Guardei o melhor retalho da estação pensando em você. Roupa boa apresenta quem você é; quero que se apresente bem.",
                MilestoneArrivalLine = "Rosto novo! Bem-vindo. Pela sua roupa de estrada, você veio de longe e dormiu pouco. Deixa eu dar uns pontos de boas-vindas.",
                MilestonePostAct1Line = "Depois daquilo, vi muito casaco rasgado de caverna passar por aqui. Tempos difíceis se leem no tecido antes de chegarem à boca.",
                MilestonePostAct3Line = "Sabe o que mudou desde o que você fez? Voltei a costurar roupa de festa. Isso, para uma alfaiate, é a forma mais honesta de esperança."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_orlan", DialogueSetId = "dialogue_orlan", HasShop = true,
                Greetings = new[]
                {
                    "Bem-vindo à Estalagem do Vale! Cama seca e sopa quente, nessa ordem de urgência.",
                    "Entre, entre! O vento lá fora não paga aluguel, e aqui dentro ele não manda.",
                    "Um viajante! As melhores histórias chegam com botas sujas. Sente-se e descalce as suas."
                },
                Role1 = "Orlan, dono da estalagem. Vinte quartos, uma lareira que nunca apaga e a melhor sopa de raiz do vale. O resto é conversa, e conversa é de graça.",
                Role2 = "Já fui guarda de caravana. Decidi que prefiro receber viajantes a protegê-los na estrada. Paga melhor, dói menos, e ninguém morre na minha porta.",
                Town1 = "Quem dorme bem explora melhor. Metade dos heróis desta cidade acordou nas minhas camas — e a outra metade devia ter dormido mais.",
                Town2 = "A estalagem ouve tudo. Se quiser saber da cidade, sente no salão por uma noite e apenas escute. A verdade vem com o terceiro copo.",
                Service1 = "Alugo quartos por noite e sirvo refeição quente. Hóspede tem direito a banho, lareira e fofoca grátis. A fofoca é a melhor parte.",
                Service2 = "Guardo cartas e encomendas para quem vive descendo a caverna. Já salvou muito combinado — e muita despedida que não precisou ser final.",
                AdviceLines = new[]
                {
                    "Durma antes de descer. A caverna cobra juros do sono atrasado, e cobra caro.",
                    "Sopa quente resolve oitenta por cento dos problemas. O resto é com o Brumdar.",
                    "Nunca conte seu ouro na frente dos outros hóspedes. Nem dos amigos, por via das dúvidas."
                },
                Rumor1 = "Um hóspede pagou um mês adiantado e nunca subiu do nível quarenta. O quarto dele continua trancado, como prometido. Eu troco as velas toda semana.",
                Rumor2 = "As cartas que chegam para o Maelor não têm remetente. E cheiram a sal. Não temos mar por perto — não há mar em Dornécia inteira deste lado.",
                Goodbyes = new[] { "Boa estrada! E volte para a sopa.", "A porta fecha tarde, até logo.", "Que seus sonhos sejam de colheita cheia." },
                SpringLine = "A primavera enche meu salão de viajante novo. A sopa de raiz nova está uma delícia, e cama seca nunca falta. Entre.",
                SummerLine = "Verão é movimento! O salão ferve de história de aventureiro. Quarto fresco no andar de cima, se o calor apertar.",
                AutumnLine = "Outono pede sopa mais grossa e lareira acesa. Bom para hóspede cansado de uma colheita puxada. Entre e desabe num banco.",
                WinterLine = "Inverno! A lareira eterna nunca foi tão bem-vinda. Sopa quente resolve oitenta por cento dos problemas; o frio é só um deles.",
                RainLine = "Chuva lá fora? O vento não paga aluguel, mas a cama seca e a sopa quente, sim. Entre e sacuda esse casaco antes que pingue na lareira.",
                FestivalLine = "Dia de festa enche meu salão até a porta. Tem música, tem caldo e tem cama para quem exagerar na comemoração. E sempre exageram.",
                FriendStrangerLine = "Bem-vindo à Estalagem do Vale! Rosto novo sempre traz história nova. Cama seca, sopa quente, e o seu nome — esse fica para depois do caldo.",
                FriendWarmLine = "Você de novo! Já sei seu quarto favorito e o ponto da sua sopa. Hóspede que volta vira quase família aqui — e família às vezes come de graça.",
                FriendCloseLine = "Meu hóspede de confiança! Deixei a lareira acesa do seu lado e a sopa no ponto que você gosta. Sente-se, conte como foi lá embaixo.",
                MilestoneArrivalLine = "Recém-chegado ao vale! Metade dos heróis desta cidade acordou nas minhas camas. Você será o próximo nome no livro de hóspedes.",
                MilestonePostAct1Line = "Depois daqueles dias, o salão virou ponto de conversa baixa. Sente-se e escute; a estalagem ouve tudo, e ultimamente ouve coisas pesadas.",
                MilestonePostAct3Line = "Desde o que você fez, as histórias contadas aqui ganharam final feliz. Por sua conta, a primeira sopa. Herói não paga o primeiro caldo."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_gruta", DialogueSetId = "dialogue_gruta", HasShop = true,
                Greetings = new[]
                {
                    "HÁ! Mais um sedento! Senta que o barril tá novo e a noite é longa.",
                    "Bem-vindo à Taverna da Gruta! Aqui dentro ninguém é estranho por mais de um copo.",
                    "Chegou na hora! A cerveja de mel acabou de descansar e tá implorando por um gole."
                },
                Role1 = "Gruta, taverneira e juíza não oficial de braço de ferro. Minha taverna é o lugar mais barulhento e mais honesto da cidade — bêbado não sabe mentir.",
                Role2 = "Herdei o balcão do meu pai. Ele dizia: sirva bem o primeiro copo e escute o terceiro. No terceiro copo, sob a lua de Senya, é que a verdade desaba.",
                Town1 = "A cidade trabalha de dia e desabafa aqui de noite. Eu só mantenho os copos cheios e as brigas curtas. Senya cuida do resto da bagunça.",
                Town2 = "Mineiro, fazendeiro, guarda, poeta... no meu balcão todo mundo senta no mesmo banco. Aqui não tem sobrenome, só sede.",
                Service1 = "Sirvo bebida, comida farta e música quando a Liora aparece. Também alugo o salão pra festa de colheita — Senya abençoa quem comemora direito.",
                Service2 = "Aventureiro que volta da caverna ganha o primeiro gole por minha conta. Tradição da casa. Quem encara o escuro merece molhar a garganta.",
                AdviceLines = new[]
                {
                    "Nunca desça a caverna de ressaca. NUNCA.",
                    "Quem paga a rodada faz amigos. Quem paga duas faz testemunhas.",
                    "Briga aqui dentro? O perdedor limpa o salão. Regra de Senya: o caos se arruma sozinho."
                },
                Rumor1 = "Um cliente bêbado jurou que viu a estátua da praça mudar a posição do escudo. Bêbado, claro. Claro... mas ele não bebeu mais depois daquilo.",
                Rumor2 = "O Zrix anda recusando cerveja. ZRIX! Ou está doente, ou viu algo na estrada da caverna que o deixou sério demais para beber.",
                Goodbyes = new[] { "Vai com Deus e volta com sede!", "Cuidado no caminho, a rua gira às vezes!", "Próxima rodada tem o teu nome!" },
                SpringLine = "Primavera! O barril novo tá fresco e a sidra de flor acabou de descansar. Senta que a primeira espuma é tua!",
                SummerLine = "Verão dá sede, e sede dá movimento! Cerveja gelada no balcão, e o salão só esquenta quando a Liora aparece pra cantar.",
                AutumnLine = "Outono é mês de festa de colheita. Alugo o salão, sirvo o melhor caldo, e a cerveja de mel fica perfeita. Senya adora outono!",
                WinterLine = "Inverno! Vinho quente, lareira e história comprida. NUNCA desça a caverna de ressaca no frio. NUNCA, ouviu?",
                RainLine = "Chuvarada lá fora? Ótimo! Chuva enche minha taverna de gente sedenta e seca. Senta perto do fogo, paga depois!",
                FestivalLine = "DIA DE FESTA! Sob a lua escarlate de Senya, a Liora canta e a cerveja corre solta! Hoje até o Zrix bebe. Senta, vamos comemorar!",
                FriendStrangerLine = "HÁ! Mais um sedento! Rosto novo no balcão. Senta, no primeiro copo a gente conversa e eu decoro a tua cara.",
                FriendWarmLine = "Ei, você! Já sei do que tu gosta. Senta no teu banco de sempre que eu já sirvo. Aqui dentro ninguém é estranho!",
                FriendCloseLine = "AH, chegou! Meu freguês de confiança! Esse copo é por minha conta. Quem paga a rodada faz amigos, e tu já és um dos meus!",
                MilestoneArrivalLine = "Cara nova na cidade! Bem-vindo! No meu balcão todo mundo senta no mesmo banco. Primeiro gole pra quebrar o gelo, vai!",
                MilestonePostAct1Line = "Depois daquilo, o salão ficou mais cheio e mais calado. As pessoas vêm beber a tensão. Eu mantenho os copos cheios e a boca fechada.",
                MilestonePostAct3Line = "Desde o que tu fez, voltaram a rir alto no meu salão! Isso vale mais que ouro. Rodada da casa pra comemorar, e que Senya pague a conta!"
            },
            new NpcDialogueContent
            {
                NpcId = "npc_brumdar", DialogueSetId = "dialogue_brumdar", HasShop = true,
                Greetings = new[]
                {
                    "Fala logo, o ferro não espera.",
                    "Hm. Você de novo. A lâmina aguentou?",
                    "Entra. Cuidado com a fagulha — ela não escolhe rosto."
                },
                Role1 = "Brumdar. Ferreiro, sob a bigorna de Thoren. Faço lâmina, enxada, prego e machado. Tudo que corta ou aguenta porrada sai desta forja.",
                Role2 = "Meu avô forjou a espada da estátua da praça. Bastarda, têmpera dupla. Até hoje não faço igual. Thoren ainda não me achou digno. Ainda.",
                Town1 = "Cidade que tem forja acesa não morre. Pode anotar. O fogo é o último a apagar.",
                Town2 = "O minério do vale é honesto. O da caverna... o da caverna CANTA quando bate no fogo. Metal estranho. Metal que lembra de onde veio.",
                Service1 = "Vendo ferramenta e arma. Conserto o que você quebrar, sem julgar como quebrou. Quase sem julgar.",
                Service2 = "Traga minério da caverna e eu pago na hora. Cobre, ferro... e se achar do escuro, do que canta, traz embrulhado. Embrulhado, ouviu?",
                AdviceLines = new[]
                {
                    "Afie antes de precisar, não depois.",
                    "Ferramenta cara é barata se durar. Ferramenta barata custa um dedo.",
                    "Lâmina lascada se conserta. Orgulho lascado, não."
                },
                Rumor1 = "A Dagna trouxe uma pedra da pedreira que não esquenta na forja. Fica fria. FRIA. Guardei num balde, longe do resto. Longe de tudo.",
                Rumor2 = "Pediram-me uma réplica da espada da estátua. Paguei pra ver quem pedia... e o sujeito sumiu da cidade no dia seguinte. Não forjei. Não vou.",
                Goodbyes = new[] { "Vai. E não quebra isso de novo.", "Hm. Até.", "Volta quando o fio cansar." },
                SpringLine = "Primavera. O fogo pega mais fácil com o ar morno. Bom pra forjar enxada. Fala logo o que precisa.",
                SummerLine = "Verão. A forja já é quente; agora é forno duplo. Bebo água e bato ferro. Se vai me pedir lâmina, é agora.",
                AutumnLine = "Outono. Época de afiar tudo antes que o frio enrijeça o aço. Traz tua lâmina, eu devolvo o fio.",
                WinterLine = "Inverno. Forja acesa é o melhor lugar da cidade no frio. Cidade que tem forja acesa não morre. Pode anotar.",
                RainLine = "Chuva. A fagulha briga com a umidade, mas o ferro não espera. Cuidado com o chão molhado perto da bigorna.",
                FestivalLine = "Festival. Hm. Eu forjo igual, festa ou não. Mas reconheço: até eu limpo a fuligem da barba num dia desses.",
                FriendStrangerLine = "Fala logo, o ferro não espera. Cara nova. Diz o que quer, não tenho o dia todo. ...Bem-vindo, suponho.",
                FriendWarmLine = "Hm. Você de novo. A lâmina aguentou? Já sei o teu peso de mão. Vou ajustar o próximo corte pensando nisso.",
                FriendCloseLine = "Ah, é você. Pode entrar sem cerimônia. Guardei um aço bom esperando alguém que saiba usar. Acho que é teu.",
                MilestoneArrivalLine = "Cara nova na cidade. Se vai descer a caverna, vai precisar de ferro honesto. O meu é. Começa pelo básico.",
                MilestonePostAct1Line = "Depois daquilo, recebi minério estranho da caverna que CANTA no fogo. Guardei. Metal que canta conta história ruim.",
                MilestonePostAct3Line = "O que você fez... meu avô forjou a espada da estátua e nunca fiz igual. Por você, vou tentar de novo. Talvez Thoren enfim me olhe. Ainda."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_dagna", DialogueSetId = "dialogue_dagna", HasShop = true,
                Greetings = new[]
                {
                    "Opa! Cuidado com o carrinho, ele desce sozinho às vezes.",
                    "Dia de poeira boa! O que você procura?",
                    "Se veio pela pedra, chegou ao lugar certo. Se veio pela vista, também."
                },
                Role1 = "Dagna, mestra da pedreira, mãos calejadas por Thoren. Corto pedra pra casa, muro, praça e até pra base da estátua — que foi a minha avó quem assentou.",
                Role2 = "Pedra é como gente: tem veio, tem humor e racha onde você menos espera. Trinta anos lendo pedra e ela ainda me surpreende. Thoren ri de mim.",
                Town1 = "Toda casa nova da cidade tem um pedaço da minha pedreira. Isso me enche o peito, sabia? A cidade é feita de mim, tijolo por tijolo.",
                Town2 = "O conselho quer calçar a rua do mercado. Ótima ideia. Péssima estimativa de quantas carroças de pedra isso engole.",
                Service1 = "Vendo pedra bruta, pedra aparelhada e cal. Pra fundação de fazenda, tenho preço de vizinho. Fundação boa honra Thoren.",
                Service2 = "Compro pedra especial da caverna. As de veia azul pagam o dobro. As que sussurram... essas eu não compro. Sério. Nunca.",
                AdviceLines = new[]
                {
                    "Na caverna, bata na parede antes de confiar nela. Som oco é aviso.",
                    "Picareta cega cansa o dobro. Visita o Brumdar.",
                    "Pedra molhada engana o pé. Pisa devagar — Thoren não perdoa pressa em terreno fofo."
                },
                Rumor1 = "Achei um fóssil na camada nova. Bicho grande, asa comprida. O vale já foi fundo de mar? Ou céu de outra coisa que caiu aqui?",
                Rumor2 = "O pessoal do nível vinte diz que as paredes de lá 'respiram'. Pedra não respira. Mas confesso que fui ver... e voltei calada. Bem calada.",
                Goodbyes = new[] { "Vai pela sombra, a poeira agradece!", "Pedra no caminho? Me chama!", "Até! E olha o carrinho!" },
                SpringLine = "A primavera amolece a terra em volta da pedra. Corte mais fácil, veio mais limpo. Boa época pra fundação!",
                SummerLine = "Verão racha pedra mal cortada. O calor encontra cada falha. Corto devagar e bebo muita água. Sente a poeira no ar?",
                AutumnLine = "Outono é bom pra entregar pedra antes da chuva forte. Calce sua fundação agora, ou o inverno cobra o atraso com juros.",
                WinterLine = "Inverno gela a pedra e a torna traiçoeira. Pedra fria racha onde você menos espera. Trabalho com respeito redobrado a Thoren.",
                RainLine = "Chuva! Pedra molhada engana o pé, na pedreira e na caverna. Pisa devagar e bata na parede antes de confiar nela.",
                FestivalLine = "Festival? A pedreira para, mas eu não. Brincadeira: hoje descanso. Até pedra precisa de um dia sem picareta.",
                FriendStrangerLine = "Opa! Cuidado com o carrinho, ele desce sozinho. Cara nova! Se veio pela pedra, chegou ao lugar certo.",
                FriendWarmLine = "Você de novo! Já sei o tipo de pedra que te serve. Separei umas aparelhadas, dá uma olhada antes de pedir.",
                FriendCloseLine = "Ah, minha companhia favorita! Guardei a pedra de veia azul pensando em você. Só não conta pros outros fregueses.",
                MilestoneArrivalLine = "Cara nova na cidade! Toda casa daqui tem um pedaço da minha pedreira. A sua também vai ter. Bem-vindo ao alicerce!",
                MilestonePostAct1Line = "Depois daquilo, achei tijolo ANTIGO escavando fundação. Mais antigo que a cidade. Quem cavou aqui antes de nós? E por quê?",
                MilestonePostAct3Line = "Sabe a base da estátua, que minha avó assentou? Desde o que você fez, juro que ela parece mais firme. Thoren e eu agradecemos. Orgulho."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_hund", DialogueSetId = "dialogue_hund", HasShop = true,
                Greetings = new[]
                {
                    "Tudo em ordem por aqui. Você que chegou desarruma?",
                    "Patrulha tranquila hoje. Bom sinal. Ou péssimo, nunca sei qual dos dois.",
                    "Cidadão. Algum problema a relatar?"
                },
                Role1 = "Hund, guarda da ronda urbana, sob a lei de Kanthor. Da praça ao portão, passando pelo mercado, meu turno cobre tudo que respira na cidade.",
                Role2 = "Servi na fronteira antes de vir pra cá. Lá aprendi que vigiar é noventa por cento andar e dez por cento estar no lugar certo na hora errada.",
                Town1 = "Cidade segura não é a que não tem problema. É a que resolve rápido. E estamos resolvendo cada vez mais rápido — eu faço questão.",
                Town2 = "A entrada da caverna é o meu pesadelo logístico. Todo dia entra gente demais e sai gente de menos. Kanthor que me perdoe a conta que não fecha.",
                Service1 = "Mantenho o quadro de avisos e registro ocorrências. Também vendo equipamento básico de segurança, aprovado pela guarda.",
                Service2 = "Se vai descer fundo na caverna, deixa aviso comigo. Se não voltar em três dias, organizamos busca. É a promessa que a lei consegue cumprir.",
                AdviceLines = new[]
                {
                    "Ande pelo centro da rua à noite. Sombra é esconderijo.",
                    "Aviso dado à guarda nunca é tempo perdido.",
                    "Se ouvir briga na taverna, deixa. A Gruta resolve mais rápido que eu, e com menos papelada."
                },
                Rumor1 = "Pegadas estranhas no portão leste, três noites seguidas. Grandes, descalças, e somem no meio da rua. SOMEM. Pé não evapora, cidadão.",
                Rumor2 = "O Alaric anda dobrando o turno no portão sul sem mandato. Quando um guarda veterano faz isso, é porque farejou algo que ainda não cabe num relatório.",
                Goodbyes = new[] { "Siga em segurança.", "Qualquer coisa, grite. Eu ouço.", "Ordem e prosperidade, cidadão." },
                SpringLine = "A primavera traz movimento ao portão. Mais fazendeiro saindo, mais aventureiro chegando. Ronda dobrada, olho aberto.",
                SummerLine = "Verão é temporada cheia. A entrada da caverna é o meu pesadelo logístico: entra gente demais, sai gente de menos. Saia cedo.",
                AutumnLine = "Outono acalma o fluxo. Bom pra revisar o quadro de ocorrências. Cidade segura é a que resolve rápido — e o outono me dá fôlego.",
                WinterLine = "Inverno esvazia a rua cedo. Ande pelo centro à noite; sombra é esconderijo. Aviso dado à guarda nunca é tempo perdido.",
                RainLine = "Com essa chuva, a visibilidade na ronda cai. Se for sair, avise o destino. Sempre. Chuva esconde mais que a noite.",
                FestivalLine = "Festival enche a praça, e praça cheia é o meu trabalho dobrado. Aproveite a festa; eu fico de olho no que você não vê.",
                FriendStrangerLine = "Tudo em ordem por aqui. Você que chegou desarruma? Cara nova. Algum problema a relatar, cidadão?",
                FriendWarmLine = "Você de novo. Já sei que não dá trabalho. Bom. Guarda gosta de rosto previsível. Siga, e mantenha-se na rua.",
                FriendCloseLine = "Ah, é você. Pode passar sem o ritual. Confio na sua palavra, e isso, vindo de um guarda, é o maior elogio que tenho a dar.",
                MilestoneArrivalLine = "Identifique-se. ...Recém-chegado, então. Primeiro rosto que o viajante vê, último que o problema encontra. Bem-vindo à ordem de Kanthor.",
                MilestonePostAct1Line = "Depois daquilo, dobrei a ronda por conta própria. Algo grande passou rente à muralha sem deixar pegada. Fico atento. Kanthor não dorme, eu também não.",
                MilestonePostAct3Line = "Desde o que você fez, durmo um pouco melhor. Um pouco. Guarda veterano nunca relaxa de todo, mas reconhece quem segura a ordem ao seu lado."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_thalindra", DialogueSetId = "dialogue_thalindra", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado com a pilha da esquerda — ela morde. Brincadeira. Ela só desaba sobre quem tem pressa.",
                    "Bem-vindo ao arquivo. Faça silêncio... ou pelo menos fofoque baixo, que o pó tem ouvido.",
                    "Ah, você. Separei uns registros que talvez lhe interessem. Ou que talvez seja melhor você nunca ter lido."
                },
                Role1 = "Thalindra, arquivista de Cindar's Hope. Guardo a história da cidade: mapas, diários, atas e tudo que o tempo tentou apagar e quase conseguiu.",
                Role2 = "Estudei na grande biblioteca de Dornécia. Vim pra cá porque os documentos daqui faziam perguntas que os de lá não ousavam responder. Sobre Anya. Sobre Cindar.",
                Town1 = "Esta cidade foi fundada ao redor da Fonte por gente que fugia de algo. Os diários nunca dizem do quê. É esse silêncio que me tira o sono, não o barulho.",
                Town2 = "A estátua da praça representa o Guerreiro de Cindar. A espada e o escudo são reais, fundidos na base. Poucos sabem. Menos ainda perguntam de quem ele guardava o quê.",
                Service1 = "Consulto registros, copio mapas e avalio relicários. Se achou algo escrito na caverna, traga. Eu leio até o ilegível — e o ilegível costuma ser o importante.",
                Service2 = "De tempos em tempos preciso de mãos para tarefas práticas: materiais, entregas, verificações. Pago do fundo do arquivo, onde guardo o que não tem preço.",
                AdviceLines = new[]
                {
                    "Anote o nível onde achar qualquer inscrição. O contexto vale mais que o achado.",
                    "A história se repete primeiro como aviso. Leia os avisos antes de virar um.",
                    "Mapas antigos erram, mas erram com padrão. Aprenda o padrão e o erro vira mapa."
                },
                Rumor1 = "Há um diário de noventa anos que descreve a caverna com cento e um níveis. O mesmo número de hoje. Cavernas crescem. Essa não. Essa esperou.",
                Rumor2 = "O fundador da cidade assinava 'C.' em tudo. Cindar? Talvez. Mas há uma ata assinada 'C.' datada de antes do nascimento dele. Os Nymirianos assinavam assim, dizem.",
                Goodbyes = new[] { "Volte com perguntas melhores. As suas já são boas.", "O arquivo não fecha; eu é que durmo.", "Leve conhecimento, deixe a poeira." },
                SpringLine = "Primavera. A umidade nova faz mal aos pergaminhos antigos. Passo o dia conservando o que o tempo quer levar — e o tempo é teimoso.",
                SummerLine = "O verão resseca a tinta velha até o pergaminho rachar. Mantenho o arquivo na sombra; conhecimento teme o sol tanto quanto teme o esquecimento.",
                AutumnLine = "Outono é a melhor estação para ler. Luz suave, ar seco. Separei uns registros que talvez lhe interessem — e um que talvez mude o que você pensa da Fonte.",
                WinterLine = "Inverno. O frio preserva os documentos, mas gela meus dedos ao copiar mapa. Pequeno preço pela memória intacta de Cindar's Hope.",
                RainLine = "Chuva! Faça silêncio e não goteje sobre as atas. Documento molhado é história apagada, e eu não reescrevo o tempo — só o transcrevo.",
                FestivalLine = "Festival? O arquivo continua aberto. Festa passa; registro fica. Mas confesso que anoto até as datas das festas. Tudo é fonte, um dia.",
                FriendStrangerLine = "Cuidado com a pilha da esquerda, ela desaba. Ah, um rosto novo. O arquivo recebe quem traz perguntas. Quais são as suas?",
                FriendWarmLine = "Você de novo. Suas perguntas estão ficando melhores. Separei um diário que só mostro a quem sabe o que procura. Você está quase lá.",
                FriendCloseLine = "Ah, é você. Sente-se. Confio em pouquíssimos o bastante para mostrar os documentos do fundo. Você é um deles. Veja — mas guarde silêncio.",
                MilestoneArrivalLine = "Rosto novo. Esta cidade foi fundada ao redor da Fonte por gente que fugia de algo. Você chega no meio dessa história — talvez para terminá-la.",
                MilestonePostAct1Line = "Depois daquilo, reabri o diário de noventa anos que descreve cento e um níveis. O mesmo número de hoje. A caverna não cresceu. Ela aguardava você.",
                MilestonePostAct3Line = "O que você fez vai entrar nos registros. Cuidarei para que a sua versão sobreviva ao tempo. É assim que um nome se torna história — e, às vezes, lenda."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_alaric", DialogueSetId = "dialogue_alaric", HasShop = false,
                Greetings = new[]
                {
                    "Alto. ...Ah, é você. Pode passar.",
                    "Portão sul, tudo calmo. Por enquanto.",
                    "Identifique-se. Costume antigo, não leve a mal."
                },
                Role1 = "Alaric, sentinela do portão sul. Primeiro rosto que o viajante vê, último que o problema encontra. Sirvo à ordem de Kanthor com a bota gasta.",
                Role2 = "Vinte anos de muralha. Já vi caravana, tempestade, lobo, e coisa que prefiro chamar de lobo só pra conseguir dormir. Vinte anos ensinam a mentir pra si mesmo.",
                Town1 = "O portão sul é o pulso da cidade. De manhã sai suor rumo à fazenda; de noite volta cansaço e história. Conto cada um que sai e cada um que volta.",
                Town2 = "A estrada pra fazenda anda segura. Mantenho assim à base de bota gasta e olho aberto. Segurança não se herda; se vigia, turno por turno.",
                Service1 = "Não vendo nada. Vigio. Mas se precisar de orientação sobre as estradas, pergunta certa no posto certo tem resposta honesta.",
                Service2 = "Registro entrada e saída de grupos grandes. Se a sua família vier visitar, me avise que eu agilizo. Pra quem confio, o portão é mais leve.",
                AdviceLines = new[]
                {
                    "Saia cedo, volte antes do sol sumir. A estrada muda de cara no escuro.",
                    "Avise alguém do seu destino. Sempre.",
                    "Se vir fumaça na estrada, não vá investigar. Venha me chamar. Curiosidade enterra gente boa."
                },
                Rumor1 = "Três noites atrás, algo grande passou rente à muralha. Não deixou pegada no barro. O barro estava fresco. Eu olhei três vezes. Três.",
                Rumor2 = "O velho posto de vigia do morro leste acendeu luz semana passada. Está abandonado há dez anos. Mandei verificar... nada. Luz não acende sozinha. Não devia.",
                Goodbyes = new[] { "Siga. E mantenha-se na estrada.", "Portão fecha ao último sino.", "Vá. Eu fico. É assim que funciona." },
                SpringLine = "Primavera. De manhã sai suor rumo à fazenda; de noite volta cansaço e história. O portão sul é o pulso da cidade, e eu sou o dedo nele.",
                SummerLine = "Verão. O fluxo na estrada não para. Mantenho o caminho seguro à base de bota gasta e olho aberto. Saia cedo, viajante.",
                AutumnLine = "Outono encurta o dia. Volte antes do sol sumir; a estrada muda de cara no escuro. Vinte anos de muralha me ensinaram a temer o crepúsculo.",
                WinterLine = "Inverno. A estrada gela e some sob a neve. Se vir fumaça lá fora, não vá investigar. Venha me chamar. Sempre.",
                RainLine = "Chuva apaga pegada e abafa som. Péssima noite pra vigia, ótima pra quem não quer ser visto. Fique na estrada, à vista.",
                FestivalLine = "Festival? Eu vigio igual. Festa atrai gente boa e gente que se aproveita da gente boa. Aproveite; eu fico de olho no resto.",
                FriendStrangerLine = "Alto. Identifique-se. ...Costume antigo, não leve a mal. Cara nova. Pode passar, mas a estrada exige respeito.",
                FriendWarmLine = "Ah, é você. Pode passar. Já reconheço o seu passo no barro. Pra um sentinela, isso é quase um aperto de mão.",
                FriendCloseLine = "Você. Bem-vindo. Se a sua família vier visitar, me avise que eu agilizo. Pra quem confio, o portão pesa menos.",
                MilestoneArrivalLine = "Recém-chegado pelo portão sul. Primeiro rosto que o viajante vê, último que o problema encontra. Esse sou eu. Siga, e respeite a estrada.",
                MilestonePostAct1Line = "Depois daquilo, algo grande passou rente à muralha três noites. Sem pegada no barro fresco. Dobrei a vigília. Kanthor que me dê olhos para o que não deixa rastro.",
                MilestonePostAct3Line = "Desde o que você fez, a estrada anda mais segura. Mantenho assim, como sempre. Mas reconheço a sua parte nisso. Vá — e volte quando quiser."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_pip", DialogueSetId = "dialogue_pip", HasShop = true,
                Greetings = new[]
                {
                    "OI! Você é o da fazenda, né? NÉ?!",
                    "Visitante! Visitante! Eu vi primeiro!",
                    "Bem-vindo, bem-vindo! Quer um mapa? Eu desenhei! Tá quase certo!"
                },
                Role1 = "Pip! Recepcionista oficial NÃO oficial da cidade! Eu mostro onde fica tudo e ainda carrego pacote pequeno! E médio! Grande não, ainda.",
                Role2 = "Um dia vou ser explorador da caverna! Já desci até o nível UM! Sozinho! Quase. O Zrix foi junto. Atrás de mim. Segurando minha mão. Mas EU fui na frente!",
                Town1 = "A cidade é DEMAIS! Tem a estátua do Guerreiro, a sopa do Orlan, e a Gruta deixa eu ficar na taverna até o segundo sino!",
                Town2 = "Todo mundo aqui se conhece. Se você espirrar na praça, no portão já falam que você tá gripado. Eu sei porque EU que conto!",
                Service1 = "Vendo mapinhas da cidade, recados entregues e lembrancinhas! Barato! Quase de graça! Mas pago, tá? Pago!",
                Service2 = "Sei TODOS os atalhos! Tipo o do beco da Mirela que corta direto pro mercado! Custa uma moeda. A informação, não o beco. O beco é de graça.",
                AdviceLines = new[]
                {
                    "A sopa do Orlan às quintas tem o dobro de raiz! E o mesmo preço!",
                    "Não mexe na pilha de pedra da Dagna. Confia em mim. CONFIA.",
                    "Se a Gruta gritar teu nome, corre pra lá. Ou pra longe. Depende do tom."
                },
                Rumor1 = "Eu VI o mercador errante! Na entrada da caverna! Ele tem uma mochila MAIOR QUE EU e sumiu quando eu pisquei! Eu pisquei rápido! Não adiantou!",
                Rumor2 = "A estátua da praça... eu deixei uma flor no escudo dela ontem. Hoje a flor tava na MÃO dela! Na MÃO! Ninguém acredita em mim! Mas eu vi! EU VI!",
                Goodbyes = new[] { "Tchau tchau! Me chama se se perder!", "Vou contar pra todo mundo que você passou aqui!", "ATÉ MAIS! Cuidado com o degrau! Esse aí! ESSE!" },
                SpringLine = "PRIMAVERA! Tudo florindo! Eu fiz um mapa novo com as flores marcadas! Tá quase certo! Quer? Custa uma moedinha!",
                SummerLine = "Calor! Calor! A sopa do Orlan no verão vem com água de fruta! Eu sei porque eu provo TODO dia! NÃO conta!",
                AutumnLine = "Outono! As folhas caem e eu junto as mais bonitas pra vender! Marcador de livro de folha! Ninguém compra, mas eu TENHO!",
                WinterLine = "Brrr! Inverno! Eu mostro o atalho quentinho que passa perto da forja do Brumdar! O calor é de graça, o atalho NÃO!",
                RainLine = "CHUVA! Eu sei onde NÃO molha indo pro mercado! Sei TODOS os beirais! Te levo seco por uma moeda! Confia! CONFIA!",
                FestivalLine = "FESTIVAL!!! O melhor dia do ANO! Eu decoro a praça, entrego convite e ainda ganho doce! Vem, eu te mostro TUDO!",
                FriendStrangerLine = "OI! Você é novo aqui, né? NÉ?! Eu vi você primeiro! Eu mostro onde fica tudo! Como você chama? Como? COMO?!",
                FriendWarmLine = "VOCÊ voltou! Eu falei pra todo mundo que a gente é amigo! Te guardei um mapa especial! O melhor! Só pra você!",
                FriendCloseLine = "MEU MELHOR AMIGO CHEGOU! Eu fiz um desenho de nós dois! Tá na parede! Vou te mostrar TODOS os atalhos de graça! Quase!",
                MilestoneArrivalLine = "VOCÊ chegou faz pouco tempo, né?! Eu sei TUDO da cidade! Te mostro a estátua, a sopa e onde a Gruta deixa eu ficar!",
                MilestonePostAct1Line = "Depois daquele dia assustador, todo mundo ficou sério. Eu não gosto. Mas eu SEI que VOCÊ ajudou! Eu vi! Quase vi!",
                MilestonePostAct3Line = "VOCÊ é o HERÓI da cidade agora! Eu falei que conhecia você ANTES de todo mundo! Vou colocar você no meu mapa! No MEIO!"
            },
            new NpcDialogueContent
            {
                NpcId = "npc_nimble", DialogueSetId = "dialogue_nimble", HasShop = true,
                Greetings = new[]
                {
                    "Não encosta nessa alavanca... tarde demais. Bom, agora você é parte do teste.",
                    "Entra rápido! A engenhoca nova está quase estável. Quase.",
                    "Ah, perfeito! Preciso de alguém com dois braços. Você tem dois, certo?"
                },
                Role1 = "Nimble, inventora-chefe da oficina. Crio mecanismos para a fazenda, para a mina e, de vez em quando, para o caos puro.",
                Role2 = "Minha primeira invenção foi um espantalho giratório. Espantou os corvos, duas cabras e o telhado do vizinho. Evoluí desde então. Um pouco.",
                Town1 = "Esta cidade tem potencial mecânico ABSURDO. Imagine: esteiras da pedreira até a forja! Roda d'água dupla! Ninguém me ouve. Ainda.",
                Town2 = "O Gurd construiu metade da cidade. Eu mantenho a metade que se move. Juntos, faríamos a cidade andar — literalmente, se ele me deixasse.",
                Service1 = "Vendo dispositivos, peças e ferramentas de precisão. Conserto engenhocas, inclusive as que não fui eu que estraguei. Inclusive essas.",
                Service2 = "Engrenagens antigas da caverna são um TESOURO. Traga e eu pago bem. Ou troco por invenção. A escolha (arriscada) é sua.",
                AdviceLines = new[]
                {
                    "Óleo na engrenagem toda lua. TODA lua.",
                    "Se a máquina fizer um som novo, ela está tentando te contar algo. Escute.",
                    "Nunca teste invenção minha em ambiente fechado. Aprendi isso por você."
                },
                Rumor1 = "Achei um mecanismo na caverna que GIRA SOZINHO. Sem corda, sem peso, sem mola. Está na minha bancada há três semanas. Girando. Como se lembrasse de um trabalho que ninguém mais pediu.",
                Rumor2 = "A Ozzra pediu um agitador automático de poções. Combinamos que se explodir, a culpa é dividida: sessenta por cento dela, quarenta minha. Ela aceitou rápido demais.",
                Goodbyes = new[] { "Volta amanhã! A versão 2 estará pronta!", "Sai pela esquerda! A direita está... em manutenção.", "Leva esse parafuso. Confia, leva." },
                SpringLine = "Primavera! Umidade ideal para calibrar engrenagem. Tudo expande na medida certa. Cuidado com a alavanca! ...Tarde demais.",
                SummerLine = "O verão dilata o metal! Minhas máquinas ficam birrentas no calor. Óleo na engrenagem toda lua, ouviu? TODA lua!",
                AutumnLine = "Outono é estação de manutenção geral. Reviso esteira, roda e mola antes que o frio enrijeça tudo. Pega um parafuso, vai precisar.",
                WinterLine = "Inverno trava engrenagem fria! Aqueço a oficina e o óleo junto. Máquina que faz som novo no frio está te avisando algo. Escute.",
                RainLine = "CHUVA! Perfeito para testar o telhado giratório! ...Que ainda não impermeabilizei. Fica longe daquela goteira ali!",
                FestivalLine = "Festival! Eu queria montar fogos mecânicos, mas o conselho VETOU. De novo. Foi UMA explosão, gente. Uma! Pequena!",
                FriendStrangerLine = "Não encosta nessa alava... tarde demais. Ótimo, agora você é parte do teste! Cara nova! Você tem dois braços, certo?",
                FriendWarmLine = "Ah, você! Já sei que você não quebra minhas coisas de propósito. Confiança rara por aqui! Dá uma olhada na bancada.",
                FriendCloseLine = "Meu cobaia... digo, parceiro favorito! Pra você eu mostro o projeto secreto. Se explodir, a culpa é cinquenta por cento de cada, combinado?",
                MilestoneArrivalLine = "Cara nova na cidade! Esta cidade tem potencial mecânico ABSURDO e ninguém me ouve! Mas você vai ouvir, né? NÉ?",
                MilestonePostAct1Line = "Depois daquilo, o mecanismo da caverna que GIRA SOZINHO acelerou. Sem corda, sem mola. Tá na bancada. Girando mais rápido. Como se tivesse acordado.",
                MilestonePostAct3Line = "Sabe o que você fez? Me deu coragem de ligar a engenhoca que gira sozinha. Agora eu desconfio que ela não foi feita aqui. Nem por mãos. Vai dar certo! Acho!"
            },
            new NpcDialogueContent
            {
                NpcId = "npc_gurd", DialogueSetId = "dialogue_gurd", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado com a vida aí embaixo! Ah, oi. Pensei que era a viga.",
                    "Dia bom pra construir! Todo dia é dia bom pra construir.",
                    "Se veio ajudar, pega um capacete. Se veio olhar, pega um capacete também."
                },
                Role1 = "Gurd, mestre de obras. Casa, celeiro, muro, ponte... se fica em pé e era pra ficar em pé, fui eu que ergui.",
                Role2 = "Construí meu primeiro celeiro aos quatorze. Caiu aos quinze. O segundo está de pé até hoje. A falha é a fundação do ofício; só os tolos constroem sem cair antes.",
                Town1 = "A cidade cresce mais rápido que minha equipe. Bom problema. Cansativo, mas bom — cada parede minha é uma família a mais que fica.",
                Town2 = "A praça nova ficou bonita, né? A base da estátua fui eu que reforcei, sobre o que a Dagna assentou. Aquilo NÃO cai nem com terremoto.",
                Service1 = "Construo e reformo. Para fazenda tenho pacote: fundação, estrutura e telhado em dez dias, se a pedra da Dagna chegar no prazo.",
                Service2 = "Compro madeira de qualidade e pedra aparelhada. Material da caverna também, se não estiver... estranho. Eu sei reconhecer pedra que não devia existir.",
                AdviceLines = new[]
                {
                    "Fundação primeiro, pressa depois. Sempre nessa ordem.",
                    "Madeira verde entorta. Deixa secar uma estação inteira.",
                    "Telhado bom se faz no verão pra agradecer no inverno."
                },
                Rumor1 = "Tem uma rachadura no muro leste que conserto toda semana. Toda semana ela volta. No mesmo desenho exato. Já não é desgaste. Vou chamar o Corvus.",
                Rumor2 = "Escavando pra fundação nova, achamos tijolo ANTIGO. Mais antigo que a cidade, talvez mais antigo que Dornécia. Quem construiu aqui antes de nós? E o que enterrou?",
                Goodbyes = new[] { "Vai com cuidado e longe do andaime!", "Obra te chama, eu atendo!", "Devolve o capacete na saída!" },
                SpringLine = "Primavera é temporada de obra! Todo mundo quer celeiro novo antes do plantio. Minha equipe não para. Pega um capacete!",
                SummerLine = "Verão é o melhor pra erguer telhado. Madeira seca rápido, argamassa cura firme. Telhado bom no verão agradece no inverno.",
                AutumnLine = "Outono: termine a obra antes da chuva pesada. Fundação na lama não pega. Pressa agora evita prejuízo depois.",
                WinterLine = "Inverno desacelera a obra, mas não para. Aproveito pra planejar fundação. Aquilo NÃO cai nem com terremoto, pode crer.",
                RainLine = "Chuva! Obra parada, andaime escorregadio. Fica longe da estrutura molhada. Madeira verde com chuva entorta feio!",
                FestivalLine = "Festival! Até a equipe folga. Mas a base da estátua, que eu reforcei, aguenta a multidão toda pulando. Pode comemorar em cima dela!",
                FriendStrangerLine = "Cuidado com a viga! Ah, oi, pensei que era queda de material. Cara nova! Se veio olhar, pega um capacete também.",
                FriendWarmLine = "Você de novo! Já confio em te deixar perto da obra sem capacete... quase. Pega um mesmo assim. Segurança primeiro!",
                FriendCloseLine = "Ah, meu parceiro de obra! Pra você eu faço o pacote completo com preço de amigo: fundação, estrutura e telhado. Fechado?",
                MilestoneArrivalLine = "Cara nova na cidade! A cidade cresce mais rápido que minha equipe. Bom problema. Sua fazenda vai precisar de mim, já aviso.",
                MilestonePostAct1Line = "Depois daquilo, a rachadura do muro leste voltou três vezes na mesma semana. No mesmo desenho. Como se algo do outro lado quisesse passar.",
                MilestonePostAct3Line = "Sabe a base da estátua que eu reforcei? Desde o que você fez, ela virou símbolo. Construir pra você é construir pra história. Eu assino embaixo."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_yael", DialogueSetId = "dialogue_yael", HasShop = true,
                Greetings = new[]
                {
                    "Shhh. Fale baixo. Os melhores negócios sussurram.",
                    "Você achou o mercado noturno. Ou ele achou você.",
                    "Bem-vindo. Aqui, sob a lua oculta de Nyx, vendemos o que o dia não explica."
                },
                Role1 = "Yael. Mercadora do anoitecer, devota discreta de Nyx. Minha banca abre quando as outras fecham, e vende o que as outras não têm coragem de estocar.",
                Role2 = "Vim de longe, de uma cidade portuária que não existe mais. Não pergunte. Ou pergunte; a história custa uma compra, e não é barata.",
                Town1 = "Toda cidade tem duas faces. Eu atendo a que aparece depois do último sino — a face que Nyx ilumina sem clarear.",
                Town2 = "A guarda me tolera porque sou útil. Sei o que se move na noite, e às vezes... compartilho. O resto eu guardo, e o resto é o que importa.",
                Service1 = "Itens raros, curiosidades da caverna, essências e segredos engarrafados. Tudo com procedência. Procedência noturna, mas procedência.",
                Service2 = "Compro achados incomuns sem perguntas. E pago em ouro ou em informação. A segunda moeda vale mais, e some mais rápido.",
                AdviceLines = new[]
                {
                    "O que brilha demais na caverna geralmente é isca.",
                    "Compre na luz, venda na sombra. Ou o contrário. Depende do item.",
                    "Nunca conte tudo que você tem. Nem pra mim. PRINCIPALMENTE pra mim."
                },
                Rumor1 = "Alguém anda comprando TODA pedra negra que aparece. Pagando triplo, por trás de intermediário. Pedra negra cultista não se compra por capricho. Odeio não saber quem.",
                Rumor2 = "O mercador errante da caverna? Real. Nos cruzamos uma vez. Ele me vendeu um mapa... do mercado noturno. O MEU mercado. Antes de eu montá-lo. Pense nisso.",
                Goodbyes = new[] { "A noite te acompanha.", "Não me viu, não falamos. Mas volte.", "Leve o embrulho. Não abra na frente da guarda." },
                SpringLine = "Primavera. As noites encurtam e meu horário também. Compre cedo; o que o dia não explica, a primavera teima em revelar rápido demais.",
                SummerLine = "Verão. Noites mornas, mercado movimentado. Mais gente acorda tarde, mais gente acha minha banca. Bom para os negócios, ruim para os segredos.",
                AutumnLine = "Outono. A escuridão se estende e meus melhores itens aparecem. Outono é quando Nyx fica generosa, sussurrando ofertas no escuro.",
                WinterLine = "Inverno. Noite longa, freguesia seleta. Quem enfrenta o frio para me achar quer mesmo o que vendo. Aproxime-se, e fale baixo.",
                RainLine = "Chuva abafa passos e olhos curiosos. Ótima noite para um negócio discreto. Fale baixo; a chuva guarda segredo melhor que eu.",
                FestivalLine = "Festival? O dia é da praça e de Senya. Mas quando o último fogo de festa apaga, a noite volta a ser de Nyx — e dela, minha. Volte então.",
                FriendStrangerLine = "Shhh. Fale baixo. Você achou o mercado noturno, ou ele achou você. Cara nova. Aqui vendemos o que o dia não explica.",
                FriendWarmLine = "Ah, você de novo. Já sei o tipo de curiosidade que move você. Tenho algo guardado que talvez sirva. Talvez. Aproxime-se.",
                FriendCloseLine = "Você. Para os de confiança, eu abro a gaveta de baixo. O que há nela não tem procedência que eu conte em voz alta. Mas é seu.",
                MilestoneArrivalLine = "Rosto novo na noite. Toda cidade tem duas faces; eu atendo a que aparece depois do último sino. Bem-vindo à face de Nyx.",
                MilestonePostAct1Line = "Depois daquilo, a compra de pedra negra triplicou. Pagam o que for, por trás de intermediário. Quem junta tanta pedra cultista está construindo algo. Odeio não saber o quê.",
                MilestonePostAct3Line = "Desde o que você fez, até meus contatos noturnos falam seu nome. Raro. Para você, a primeira informação sai de graça. Só a primeira — Nyx não dá duas."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_maelor", DialogueSetId = "dialogue_maelor", HasShop = false,
                Greetings = new[]
                {
                    "...Você também não consegue dormir?",
                    "A noite está falando hoje. Escute.",
                    "Ah. Um rosto desperto. Raro a esta hora."
                },
                Role1 = "Maelor. Eu caminho. As pessoas chamam de ronda, vigília, mania... eu chamo de escuta. A cidade conta coisas a quem anda devagar sob a lua de Nyx.",
                Role2 = "Já fui marinheiro, num mar que você não encontra em nenhum mapa de Dornécia. A água me trouxe terra adentro e levou meu nome no caminho. Ainda procuro os dois.",
                Town1 = "De noite a cidade tira a máscara. As pedras esfriam, os sonhos vazam pelas janelas. É o meu horário favorito, e o mais honesto.",
                Town2 = "A estátua do Guerreiro... eu converso com ela. Nunca responde. Mas escuta melhor que muita gente viva — e às vezes, juro, ela parece prestes a falar.",
                Service1 = "Não vendo nada. Mas se algo se perder na noite — um objeto, um animal, uma pessoa — eu costumo saber onde a noite guarda as coisas que somem.",
                Service2 = "Ouvi dizer que você desce a caverna. Eu sinto a caverna daqui de cima, sabia? Ela tem... marés. Como o mar que me esqueceu. Desça na maré baixa.",
                AdviceLines = new[]
                {
                    "Hoje a caverna está inquieta. Vá amanhã.",
                    "Se ouvir seu nome no escuro, não responda na primeira. Nem na segunda.",
                    "O sono é uma porta. Tranque-a por dentro."
                },
                Rumor1 = "Há uma lua que só aparece refletida na Fonte. Não está no céu. Olhe na água numa noite limpa e conte as luas. Conte de novo. Depois me diga quantas viu.",
                Rumor2 = "As cartas que recebo cheiram a sal porque quem as envia ainda navega. Em qual mar, eu não sei. Talvez num que corre embaixo de nós, no escuro que a Anya esqueceu.",
                Goodbyes = new[] { "Durma, se conseguir.", "A noite é longa. Eu cuido dela.", "Vá. Os sonhos não gostam de esperar." },
                SpringLine = "Primavera. A noite cheira a terra molhada e à coisa que vai brotar. Até o escuro acorda diferente nesta estação.",
                SummerLine = "Verão. As noites são curtas e mornas, mas a cidade ainda sussurra. Escute o que o calor não deixa o dia ouvir.",
                AutumnLine = "Outono. As folhas caem como dias contados. A noite fica mais longa, e eu, mais ouvinte. A cidade confessa mais nesta estação.",
                WinterLine = "Inverno. O frio silencia até os sonhos. Ando devagar pela neve para não acordar o que dorme sob a cidade. Você ouve isso? Eu ouço.",
                RainLine = "A chuva conversa, sabia? Cada cidade tem uma voz na chuva. A desta fala baixo esta noite. Pare. Escute comigo um instante, só um.",
                FestivalLine = "Festa? O dia comemora com Senya. Eu prefiro a hora depois, quando os fogos calam e a cidade tira a máscara para Nyx. Esse é o meu horário.",
                FriendStrangerLine = "...Você também não consegue dormir? A noite está falando hoje. Ainda não sei o seu nome — mas a noite já o anotou.",
                FriendWarmLine = "Ah. Você de novo, a esta hora. Começo a reconhecer o seu passo na rua escura. Poucos andam devagar o bastante para eu notar.",
                FriendCloseLine = "Você. Sente-se comigo no jardim da estátua. A esta altura, confio a pouquíssimos o que a noite me conta. Você é um deles. Ouça.",
                MilestoneArrivalLine = "Um rosto novo e desperto. Raro. A cidade conta coisas a quem chega ouvindo. Ande devagar; você vai entender por que a maré o trouxe.",
                MilestonePostAct1Line = "Depois daquilo, a caverna ficou inquieta. Eu a sinto daqui de cima, sabia? Ela tem marés. Naquela noite, a maré subiu — e algo subiu junto.",
                MilestonePostAct3Line = "O que você fez acalmou algo que eu ouvia há anos sob a cidade. Pela primeira vez, a noite dorme tranquila. E, por um instante, lembrei do meu nome. Obrigado.",
                // fable_10 — Maelor da a pista do eco (mq_act1_03). O QuestGiverInteractable so
                // ofertara quando os prerequisitos (mq_act1_02) estiverem completos.
                OfferedMainQuestId = "mq_act1_03_eco_da_agua",
                OfferQuestLabel = "O que a noite diz sobre a caverna?"
            },
            new NpcDialogueContent
            {
                NpcId = "npc_zrix", DialogueSetId = "dialogue_zrix", HasShop = true,
                Greetings = new[]
                {
                    "Voltando da caverna ou indo? A resposta muda o meu conselho.",
                    "Estrada limpa hoje. Lá embaixo... bem, lá embaixo nunca está limpa.",
                    "Ei. Verifique as botas antes de descer. Sempre."
                },
                Role1 = "Zrix, batedor da estrada da caverna. Patrulho o caminho entre a cidade e a boca do abismo. Alguém precisa, e ninguém mais quis.",
                Role2 = "Já desci até o nível trinta e cinco. Voltei com esta cicatriz e esta regra: a pressa escolhe o túmulo. Trinta e cinco me ensinaram a respeitar o quê não vejo.",
                Town1 = "A cidade vive da caverna mais do que admite. Metade do ouro da praça subiu de lá nas costas de alguém — e nem todo mundo que desce volta para gastar a sua parte.",
                Town2 = "Vejo os novatos descendo com brilho nos olhos. Meu trabalho é garantir que subam com o brilho ainda aceso, e não apagado lá embaixo.",
                Service1 = "Vendo suprimento de descida: tochas, cordas, rações secas e antídoto básico. O kit que separa susto de tragédia.",
                Service2 = "Também marco o seu nome no quadro de descidas. Se não voltar no prazo, eu mesmo desço para buscar. Já trouxe sete de volta. E dois eu não trouxe.",
                AdviceLines = new[]
                {
                    "Nível novo, regra nova. Não assuma nada do andar anterior.",
                    "Inimigo que recua não desistiu. Está te levando pra algum lugar.",
                    "Marque o seu caminho. A caverna gosta de embaralhar quem confia na memória."
                },
                Rumor1 = "Os bichos do nível dez estão descendo pro doze. Algo lá em cima dos níveis está... empurrando eles pra baixo. Monstro não foge de monstro. Então o que empurra monstro?",
                Rumor2 = "Vi o mercador errante duas vezes no mesmo dia. Em níveis diferentes. DISTANTES. Ou ele tem um irmão, ou as regras dele são outras. As regras da caverna, talvez.",
                Goodbyes = new[] { "Desça devagar, suba inteiro.", "Te vejo no quadro de retorno.", "Boa sorte. E conta as tochas DUAS vezes." },
                SpringLine = "Primavera. A boca da caverna fica mais movimentada com o degelo. Mais novato descendo. Verifique as botas antes de ir.",
                SummerLine = "Verão. O calor lá fora não chega lá embaixo; a caverna ignora estação. Leve agasalho mesmo no verão. Confie em mim.",
                AutumnLine = "Outono. Os dias encurtam e a descida fica mais arriscada no escuro. Marque o seu nome no quadro antes de entrar.",
                WinterLine = "Inverno. A estrada até a caverna congela e escorrega. Pressa no gelo escolhe o túmulo. Desça com o dobro de cuidado.",
                RainLine = "Chuva torna a trilha até a boca da caverna um lamaçal. Pise firme. Inimigo que recua na chuva não desistiu; te leva pra algum lugar.",
                FestivalLine = "Festival? Eu patrulho a estrada igual. Aventureiro animado com festa desce afoito. Justamente nesses dias eu trago mais gente de volta.",
                FriendStrangerLine = "Voltando da caverna ou indo? A resposta muda o meu conselho. Cara nova. Verifique as botas antes de descer. Sempre.",
                FriendWarmLine = "Ah, você de novo. Já sei que você escuta o meu conselho, e isso te mantém vivo. Marquei o seu nome no quadro, como sempre.",
                FriendCloseLine = "Você. Bom te ver inteiro. Para quem confio, deixo o melhor kit separado e desço eu mesmo se você não voltar no prazo. Já trouxe sete.",
                MilestoneArrivalLine = "Cara nova na estrada da caverna. Alguém precisa patrulhar entre a cidade e o abismo; sou eu. Meu trabalho é te ver subir inteiro.",
                MilestonePostAct1Line = "Depois daquilo, os bichos do nível dez estão descendo pro doze. Algo lá no fundo está empurrando eles pra cima de nós. O que assusta um monstro, viajante?",
                MilestonePostAct3Line = "Desde o que você fez, a estrada da caverna anda mais calma. Trinta e cinco níveis eu desci e voltei. Você foi mais fundo, e voltou. Respeito de verdade."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_savra", DialogueSetId = "dialogue_savra", HasShop = true,
                Greetings = new[]
                {
                    "Cuidado onde pisa. Essa florzinha aí leva dois anos pra crescer.",
                    "O bosque está generoso hoje. Colhi três cestas antes do sol alto.",
                    "Bem-vindo à borda do verde. A cidade termina aqui; a floresta só começa."
                },
                Role1 = "Savra, herborista do portão da floresta, ouvido atento de Telisandra. Colho, seco, misturo e curo. As plantas falam; eu só traduzo o que a mata dita.",
                Role2 = "Minha mãe me ensinou as ervas. A floresta me ensinou o resto, geralmente do jeito difícil — toda cicatriz minha tem nome de planta.",
                Town1 = "A cidade e a floresta fazem um acordo silencioso: ela nos dá remédio e madeira, nós damos respeito. Quando alguém quebra o acordo, eu sou a primeira a ouvir.",
                Town2 = "O chá que a Gruta serve no inverno? Mistura minha. O perfume da Mirela? Também. A cidade inteira cheira ao meu jardim e nem desconfia.",
                Service1 = "Vendo ervas medicinais, antídotos, chás e unguentos. Para quem desce a caverna, o pacote anti-veneno não é luxo, é obrigação.",
                Service2 = "Compro plantas raras, especialmente as da caverna. Musgo que brilha, flor que cresce no escuro sem sol... pago muito bem, e pergunto pouco.",
                AdviceLines = new[]
                {
                    "Folha em par, pode tocar. Folha trincada, deixa quieta.",
                    "Antídoto vence. Confira a data antes de descer.",
                    "Mel resolve tosse, briga e negociação. Leve mel."
                },
                Rumor1 = "Tem um cogumelo novo crescendo na boca da caverna. Não está em nenhum dos meus livros. Plantei um em vaso... e ele virou na direção da caverna. Plantas não fazem isso.",
                Rumor2 = "Os pássaros pararam de fazer ninho na árvore alta do portão leste. Pássaro sabe das coisas antes da gente. Telisandra avisa pelos bichos; eu só escuto.",
                Goodbyes = new[] { "Vá pelo caminho marcado!", "Leve água. E respeito.", "Que o verde te acompanhe." },
                SpringLine = "Primavera! O bosque transborda. Colhi três cestas antes do sol alto. Folha nova cura melhor; leve enquanto há.",
                SummerLine = "Verão seca as ervas no pé. Colho de madrugada, antes do calor roubar o óleo das folhas. Antídoto fresco aqui, viajante.",
                AutumnLine = "Outono é raiz e casca, a força que a planta guarda para o frio. O pacote anti-veneno fica mais potente nesta estação.",
                WinterLine = "Inverno. A floresta dorme, mas eu não. Mistura minha aquece o chá da Gruta. Roupa molhada na caverna é febre; leve chá.",
                RainLine = "Chuva alimenta o verde e a mim. Dia de chuva é dia de secar erva na varanda e moer raiz. O bosque agradece cada gota.",
                FestivalLine = "Festival! Levo guirlanda e chá de festa para a praça. A floresta e a cidade têm um acordo; em dia de festa, ele floresce à vista de todos.",
                FriendStrangerLine = "Cuidado onde pisa. Essa florzinha leva dois anos pra crescer. Cara nova. Bem-vindo à borda do verde; a cidade termina aqui.",
                FriendWarmLine = "Ah, você de novo. Já sei as ervas que te servem. Separei um antídoto fresco pensando na sua próxima descida. Leve.",
                FriendCloseLine = "Minha companhia querida! Para você, a erva mais rara da colheita e o chá que só ofereço a quem o verde já aprendeu a amar.",
                MilestoneArrivalLine = "Rosto novo na borda da floresta. A cidade e a mata fazem um acordo silencioso. Respeite o verde, e ele te recebe. Bem-vindo.",
                MilestonePostAct1Line = "Depois daquilo, um cogumelo novo cresceu na boca da caverna. Não está em nenhum livro meu. Plantei um; ele virou na direção da caverna, e não para de virar.",
                MilestonePostAct3Line = "Desde o que você fez, os pássaros voltaram a fazer ninho na árvore alta do portão leste. Pássaro sabe das coisas. Eles confiam de novo, e eu também."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_ozzra", DialogueSetId = "dialogue_ozzra", HasShop = true,
                Greetings = new[]
                {
                    "NÃO respire fundo ainda! ...Pronto. Agora pode. Bem-vindo!",
                    "Ah, ótimo, um voluntário! Brincadeira. A menos que...",
                    "Entre! O cheiro é normal. O normal daqui, claro."
                },
                Role1 = "Ozzra, alquimista e filha bastarda do caos de Senya. Transformo planta, mineral e coragem alheia em poção, elixir e, ocasionalmente, fumaça roxa.",
                Role2 = "Estudei em Dornécia até me convidarem a 'pesquisar em outro lugar'. O laboratório deles era pequeno demais para as minhas ideias. Literalmente. Explodiu a parede leste.",
                Town1 = "Esta cidade é perfeita para alquimia: ervas da Savra, minérios da caverna e vizinhos compreensivos. Ou surdos. Nunca perguntei qual dos dois.",
                Town2 = "A água da Fonte tem propriedades que desafiam os meus instrumentos. E os meus instrumentos foram feitos pela Nimble, então já desafiavam bastante coisa de fábrica.",
                Service1 = "Vendo poções de vida, frascos de energia e reagentes. Tudo testado! Em mim, geralmente. Por isso o desconto de quinta-feira.",
                Service2 = "Traga ingredientes exóticos da caverna e fazemos negócio. Olho de criatura, cristal ressonante, qualquer coisa que pulse quando ninguém olha.",
                AdviceLines = new[]
                {
                    "Poção vermelha cura. Poção roxa... depende. Pergunte antes.",
                    "Nunca misture poções no estômago. Misture no frasco, como gente civilizada.",
                    "Frasco vazio também vale ouro. Devolva e ganhe desconto."
                },
                Rumor1 = "Destilei a água de uma poça do nível oito. O resíduo... se move. Guardei no armário triplo. E o armário anda arranhado por DENTRO. Por dentro, viajante.",
                Rumor2 = "A pedra negra reage à água da Fonte. Reage MUITO. Como se uma quisesse devorar a outra. O conselho me proibiu de repetir o teste. Foi um muro só, gente.",
                Goodbyes = new[] { "Saia antes que algo borbulhe!", "Volte com frascos vazios e curiosidade cheia!", "Se sentir gosto de metal, volta aqui CORRENDO." },
                SpringLine = "Primavera! Reagentes vegetais no auge. A Savra me traz erva fresca e as minhas poções saem mais limpas. Sente o cheiro? É normal! Quase.",
                SummerLine = "O verão acelera toda reação! As minhas poções fermentam rápido demais. NÃO respire fundo perto do alambique hoje. Sério. NÃO.",
                AutumnLine = "Outono é estação de destilar raiz e essência concentrada. Elixir mais forte sai agora. Traga frasco vazio; ganha desconto.",
                WinterLine = "Inverno desacelera as misturas, ótimo para experimento delicado. Poção de vida quente para o frio! Testada! Em mim, claro.",
                RainLine = "A chuva muda a pressão e as minhas reações ficam... imprevisíveis. Hoje a fumaça saiu roxa. Roxa depende. Pergunte antes de comprar!",
                FestivalLine = "Festival! Sob a lua de Senya até a minha química fica mais ousada. Eu queria soltar fumaça festiva, mas o conselho... bem, foi UM muro. Compre um elixir e comemore!",
                FriendStrangerLine = "NÃO respire fundo ainda! ...Pronto, agora pode. Bem-vindo! Cara nova, ótimo, um voluntár... digo, cliente! O cheiro é normal.",
                FriendWarmLine = "Ah, você! Já sei que você não foge da minha fumaça. Coragem rara! Dá uma olhada nos reagentes novos, sem encostar naquele ali.",
                FriendCloseLine = "Meu cliente de confiança! Pra você eu mostro o elixir experimental. Se der gosto de metal na língua, volta CORRENDO, combinado?",
                MilestoneArrivalLine = "Cara nova na cidade! Esta cidade é perfeita para alquimia: erva da Savra, minério da caverna e vizinhos compreensivos. Ou surdos.",
                MilestonePostAct1Line = "Depois daquilo, destilei água de uma poça do nível oito. O resíduo se MOVE. Guardei no armário triplo. Ele anda arranhado por dentro. E eu não o arranhei.",
                MilestonePostAct3Line = "Sabe o que você fez? Me deu vontade de repetir o teste da pedra negra com a água da Fonte. Só que dessa vez... longe das paredes. E com você por perto. Talvez."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_eiran", DialogueSetId = "dialogue_eiran", HasShop = true,
                Greetings = new[]
                {
                    "Ei! Você assustou as galinhas. Brincadeira, elas se assustam sozinhas.",
                    "Bem-vindo ao quintal mais barulhento da cidade!",
                    "Chegou na hora da ordenha! Quer aprender? É mais zen do que parece."
                },
                Role1 = "Eiran, criador de animais sob a bênção de Thandra. Galinha, cabra, vaca e o que mais aparecer berrando no meu portão — eu acolho e Thandra fecunda.",
                Role2 = "Os bichos me entendem melhor que as pessoas. É mais sincero o relacionamento: eu trato bem, eles produzem. Sem fofoca, sem mágoa guardada.",
                Town1 = "O leite da praça, os ovos da estalagem e a lã da Mirela saem daqui. A cidade inteira toma café da manhã no meu quintal e nem percebe.",
                Town2 = "Animal sente a cidade. Quando algo vai mal, eles avisam primeiro, muito antes da guarda. E ultimamente... andam avisando demais.",
                Service1 = "Vendo ovos, leite, lã e ração. Filhotes na primavera, para quem tiver espaço e paciência — Thandra não dá vida a quem tem pressa.",
                Service2 = "Cuido de animal de fazenda enquanto você desce a caverna. Taxa justa, bicho volta gordo e com saudade. Não respondo por saudade.",
                AdviceLines = new[]
                {
                    "Galinha feliz bota mais. Música ajuda. Não pergunte como descobri.",
                    "Cabra que olha muito pra cerca já decidiu pular. Reforce antes.",
                    "Animal novo, quarentena. Sempre."
                },
                Rumor1 = "As cabras se recusam a pastar perto da entrada da caverna desde a lua passada. CABRAS. Que comem até avental. Se elas têm medo, eu tenho mais.",
                Rumor2 = "O galo cantou meia-noite em ponto, três noites seguidas. Meu avô dizia que isso anuncia visita... de longe. MUITO longe. De baixo, talvez.",
                Goodbyes = new[] { "Vai lá! E fecha o portão, pelas cabras!", "Leva ovo fresco, tá na cesta!", "Volta pra ordenha de domingo!" },
                SpringLine = "Primavera! Época de filhote! Tem pintinho, cabrito e bezerro novo no quintal. Quer levar um? Só se tiver espaço e paciência!",
                SummerLine = "Verão dá muito leite e muito calor. Galinha feliz bota mais com sombra boa. Ovo fresco na cesta toda manhã, viu?",
                AutumnLine = "Outono é mês de engordar o rebanho antes do frio. Vendo ração reforçada. Animal bem tratado atravessa o inverno inteiro.",
                WinterLine = "Inverno. Recolho os bichos cedo e reforço o curral. Cabra que olha pra cerca já decidiu pular; no frio, reforce antes!",
                RainLine = "Chuva! As galinhas se abrigam e ficam mais quietas, graças a Thandra. Cuidado com a lama no curral; o portão escorrega.",
                FestivalLine = "Festival! Levo ovo, leite e queijo fresco para a praça. Em dia de festa, até as cabras parecem mais comportadas. Quase.",
                FriendStrangerLine = "Ei! Você assustou as galinhas. Brincadeira, elas se assustam sozinhas. Cara nova! Bem-vindo ao quintal mais barulhento da cidade!",
                FriendWarmLine = "Você de novo! Os bichos já te reconhecem, sabia? Eles confiam em pouca gente. Pega um ovo fresco da cesta, é por minha conta.",
                FriendCloseLine = "Ah, meu amigo! Até a cabra mais ranzinza gosta de você, e isso é raríssimo. Separei o melhor queijo da semana só pra você. Leva!",
                MilestoneArrivalLine = "Cara nova na cidade! A cidade toma café da manhã no meu quintal: leite da praça, ovo da estalagem. Bem-vindo ao barulho!",
                MilestonePostAct1Line = "Depois daquilo, as cabras se recusam a pastar perto da caverna. CABRAS. Que comem até avental. Animal sente o que a gente ainda não vê.",
                MilestonePostAct3Line = "Sabe o que mudou desde o que você fez? As cabras voltaram a pastar tranquilas perto da caverna. Bicho só relaxa quando o perigo passa de verdade. Obrigado."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_liora", DialogueSetId = "dialogue_liora", HasShop = false,
                Greetings = new[]
                {
                    "Shh... estou compondo. Pronto, perdi. Era linda. Culpa sua. Brincadeira: era mediana.",
                    "Você chega como um acorde inesperado. Fica para o refrão?",
                    "O jardim da estátua tem a melhor acústica da cidade. E o melhor público: ele nunca vaia."
                },
                Role1 = "Liora, música e poeta sob a lua branca de Alihana. Canto nas tavernas, nos festivais e para a estátua, que é o meu crítico mais honesto.",
                Role2 = "Aprendi música ouvindo o vento nos canaviais de Vaalara. Vim pra cá atrás de uma melodia que sonhei sob Alihana. Ainda não a encontrei inteira — mas ouço pedaços dela vindos do chão.",
                Town1 = "Cada cidade tem uma canção escondida. A desta é em tom menor, mas com esperança no refrão. Como o nome dela, aliás. Cindar deixou a melodia; cabe a alguém terminá-la.",
                Town2 = "O Guerreiro da estátua... componho sobre ele há anos. Quem ergue a espada para o céu e segura o escudo para o povo? Um protetor. Ou um pedido de socorro fundido em bronze.",
                Service1 = "Não vendo nada; ofereço. Música na praça ao entardecer, versos por um sorriso. Se quiser uma canção sua, traga-me uma história verdadeira — só as verdadeiras cantam.",
                Service2 = "Para festivais e festas, toco mediante convite e janta. A Gruta sabe o meu repertório de taverna; o Corvus, o de templo. Guardo um terceiro que ainda não cantei para ninguém.",
                AdviceLines = new[]
                {
                    "Cante na caverna. Os ecos respondem... e os que respondem errado, evite.",
                    "Toda colheita merece uma canção. Até a ruim. PRINCIPALMENTE a ruim.",
                    "Ouça mais o silêncio entre as palavras do que as palavras."
                },
                Rumor1 = "Quando toco certa sequência de notas perto da estátua, o vento muda. Três notas. Sempre as mesmas. Não toco mais a quarta — da última vez, algo respondeu.",
                Rumor2 = "A melodia que sonhei? Um mineiro a assobiou semana passada. Disse que a ouviu 'lá embaixo, no fundo'. Ele nunca me viu cantar. Como se canta uma canção que ninguém ensinou?",
                Goodbyes = new[] { "Que a sua estrada rime.", "Volte ao entardecer; a luz ajuda a música.", "Vou colocar você numa canção. A parte boa, prometo." },
                SpringLine = "A primavera afina o mundo. Os pássaros voltam e roubam as minhas melhores notas. Componho sobre flores que nem desabrocharam ainda.",
                SummerLine = "Verão. As noites mornas pedem canção longa na praça. Fico até tarde; o calor estica a música como estica o dia.",
                AutumnLine = "Outono é a estação mais musical: tudo cai em tom menor, mas com esperança no refrão. Como o nome desta cidade, aliás.",
                WinterLine = "Inverno silencia a praça, então levo a música para a taverna da Gruta. Ouça mais o silêncio entre as notas do que as notas.",
                RainLine = "A chuva tem ritmo próprio, sabia? Sento sob o beiral e deixo ela marcar o compasso. As melhores canções nascem molhadas.",
                FestivalLine = "Festival! Sob a lua de Alihana, hoje eu canto para a cidade inteira, não só para a estátua. Traga uma história verdadeira e eu a transformo em verso.",
                FriendStrangerLine = "Shh... estou compondo. Pronto, perdi. Era linda. Culpa sua. Brincadeira: era mediana. Você chega como um acorde inesperado.",
                FriendWarmLine = "Ah, você de novo. Você vira refrão na minha cabeça, sabia? Sente-se; o jardim da estátua tem a melhor acústica e o melhor público.",
                FriendCloseLine = "Meu ouvinte favorito! Compus uma canção pensando em você, e juro que te dei a parte boa. Fique para o refrão, sempre fique.",
                MilestoneArrivalLine = "Um rosto novo na cidade. Cada cidade tem uma canção escondida; a desta é em tom menor com esperança. Você chega bem no começo dela.",
                MilestonePostAct1Line = "Depois daquilo, quando toco certa sequência perto da estátua, o vento muda. Três notas. Sempre as mesmas. E agora juro que ouço uma quarta me respondendo do chão.",
                MilestonePostAct3Line = "A melodia que sonhei a vida toda? Desde o que você fez, finalmente ouvi o final dela, subindo do chão como água limpa. Você a completou. Obrigada — de verdade."
            },
            new NpcDialogueContent
            {
                NpcId = "npc_velorin", DialogueSetId = "dialogue_velorin", HasShop = false,
                Greetings = new[]
                {
                    "Aproxime-se, jovem. Os velhos enxergam pouco de perto, mas longe demais para o próprio bem.",
                    "Bem-vindo à Câmara. Aqui se decide o pouco que uma aldeia pode decidir, e se carrega o resto.",
                    "Ah. Um rosto que ainda não conheço. Sente-se; a pressa é o único luxo que a idade me tirou."
                },
                Role1 = "Sou Velorin, ancião e líder de Cindar's Hope. Guardo as atas, as decisões e a memória viva desta aldeia — que é mais pesada do que parece.",
                Role2 = "Sirvo à ordem de Kanthor, não à minha vontade. Um líder que confunde as duas afunda a aldeia junto com o próprio nome. Já vi acontecer.",
                Town1 = "Já vi esta aldeia nascer, cair e se reerguer. Cada pedra que pisa carrega o nome de alguém que partiu para que você chegasse.",
                Town2 = "Cindar's Hope foi erguida sobre uma esperança — e sobre um aviso. Eu guardo os dois com igual cuidado. Um dia entenderá por quê.",
                Service1 = "Não vendo nada; ofereço conselho e ouço petições. Quem tem queixa contra a guarda, disputa de terra ou medo da caverna, traz a mim.",
                Service2 = "As decisões da aldeia passam por esta Câmara. Se quiser mudar algo grande, traga razão e paciência — eu tenho de sobra da segunda.",
                AdviceLines = new[]
                {
                    "Não confunda pressa com coragem. A caverna devora os afoitos primeiro.",
                    "Ouça os velhos e os animais: ambos sentem a tempestade antes do trovão.",
                    "A justiça de Kanthor pesa o sobrevivente tanto quanto o caído. Aja como quem será pesado."
                },
                Rumor1 = "Há atas na Câmara assinadas 'C.' — anteriores à fundação oficial. Cindar? Talvez. Guardo-as fechadas. Alguns nomes pesam demais para o ar.",
                Rumor2 = "A Fonte de Anya escolheu este vale por um motivo que os fundadores juraram esquecer. Eu não esqueci. Mas ainda não é hora de você saber.",
                Goodbyes = new[] { "Vá com a ordem de Kanthor.", "A aldeia conta com cada um de nós. Não a decepcione.", "Volte quando a dúvida for grande demais para carregar sozinho." },
                SpringLine = "Primavera. Mais um ciclo que eu não esperava ver, e vejo. A aldeia rejuvenesce enquanto eu não; é justo assim.",
                SummerLine = "Verão. O calor cansa estes ossos antigos. Fico na sombra da Câmara, mas a porta segue aberta a quem precisa de conselho.",
                AutumnLine = "Outono. A estação que mais entendo: tudo amadurece para então partir. Há dignidade nisso, se a gente para de temer.",
                WinterLine = "Inverno. O frio me lembra de cada inverno que esta aldeia quase não atravessou. Atravessamos. Sempre atravessamos.",
                RainLine = "A chuva é boa para os arquivos e ruim para os meus joelhos. Sente-se; deixe o temporal passar conversando com um velho.",
                FestivalLine = "Dia de festa. Eu abençoo a praça e me recolho cedo — a alegria dos jovens é melhor sem um ancião vigiando. Aproveitem por mim.",
                FriendStrangerLine = "Aproxime-se, jovem. Ainda não sei seu nome, mas a aldeia já o registrou em mim. Eu lembro de todos que chegam.",
                FriendWarmLine = "Você de novo. Começo a confiar no seu juízo, e a confiança de um velho líder não se dá barato. Sente-se, conversemos.",
                FriendCloseLine = "Ah, é você. Sente-se aqui, perto. Confio a poucos o que sei desta aldeia — e você se tornou um deles. Ouça com cuidado.",
                MilestoneArrivalLine = "Você chegou há pouco. Bem-vindo a Cindar's Hope. Lidero esta aldeia há mais tempo do que a maioria está viva; deixe-me guiá-lo no começo.",
                MilestonePostAct1Line = "Depois do que houve, reuni a Câmara três vezes numa semana. A aldeia está assustada, e o medo é mau conselheiro. Sua firmeza ajudou.",
                MilestonePostAct3Line = "O que você fez muda o rumo de coisas mais antigas que eu. Registrarei seu nome ao lado dos fundadores. Poucos mereceram essa tinta."
            },

            // ── Sael Maré-Quieta — Pescador (Tiefling) ──
            new NpcDialogueContent
            {
                NpcId = "npc_sael",
                DialogueSetId = "dialogue_sael",
                HasShop = true,
                Greetings = new[]
                {
                    "Fica quieto um instante. A água está dizendo algo, e eu prefiro ouvir antes de falar.",
                    "Bem-vindo ao cais. Aqui o tempo passa no ritmo do anzol — devagar, e quando você menos espera.",
                    "Ah. Um rosto novo na beira do lago. Senta. Quem tem pressa espanta o peixe e a conversa."
                },
                Role1 = "Sou Sael. Pesco para a vila — peixe fresco, defumado, e a paciência que ninguém quer comprar mas todos precisam.",
                Role2 = "Vim dos pântanos do sul, subindo o rio. Não pergunte do lugar; eu mesmo evito lembrar. O lago me deu um recomeço silencioso.",
                Town1 = "Cindar's Hope vive de costas para a água, e isso é um erro. O lago alimenta, cura e — se você o ouvir — avisa.",
                Town2 = "Dizem que este lago é só um lago. Eu já vi a superfície ficar lisa demais antes de uma desgraça. Águas paradas guardam memória.",
                Service1 = "Vendo peixe, isca e o que precisar para pescar. Compro o que você tirar das águas — inclusive o que vem das poças da caverna.",
                Service2 = "Se quiser aprender a ler a água, eu ensino de graça. Vender é o ofício; ensinar é o que me mantém humano.",
                AdviceLines = new[]
                {
                    "Não brigue com a correnteza. Quem rema contra a água cansa antes de chegar.",
                    "O peixe morde no silêncio. A pressa é o ruído que afasta tudo que vale a pena.",
                    "Olhe a superfície antes de entrar. A água mansa esconde mais que a brava."
                },
                Rumor1 = "Todo crepúsculo eu deixo um peixe sobre a pedra lisa, ali na ponta da doca. Não pergunte para quem. Há coisas no rio que merecem respeito, não nome.",
                Rumor2 = "Uma vez puxei do fundo um anel que não era meu nem de ninguém da vila. Devolvi à água. Há heranças que é melhor não herdar.",
                Goodbyes = new[]
                {
                    "Vá com a maré calma.",
                    "Volte quando o cesto estiver vazio — ou a cabeça cheia demais.",
                    "A água estará aqui. Eu também."
                },
                SpringLine = "Primavera. A água esquenta e o peixe sobe. Boa época para quem tem mãos firmes e a consciência leve.",
                SummerLine = "Verão. O lago fica preguiçoso ao meio-dia; eu também. Pesco no frescor da manhã e da noite — venha cedo ou venha tarde.",
                AutumnLine = "Outono. A água escurece e o peixe engorda para o frio. É quando a pesca dá mais — e fala mais, se você ouvir.",
                WinterLine = "Inverno. A beira congela nas bordas e o silêncio fica espesso. Pesco no buraco do gelo, sozinho. A solidão e eu nos damos bem.",
                RainLine = "Chuva é boa para mim — o peixe sobe e a vila some de medo de se molhar. Mais água para nós dois conversarmos.",
                FestivalLine = "Dia de festa. Levo peixe defumado para a praça e volto para a doca antes do barulho. A multidão me cansa mais que uma rede cheia.",
                FriendStrangerLine = "Você de novo na beira. Ainda não sei se confio, mas a água gosta de quem volta. Fique, se souber ficar quieto.",
                FriendWarmLine = "Ah, é você. Trouxe a calma de sempre. Já reservei a melhor isca — quem tem paciência merece o melhor anzol.",
                FriendCloseLine = "Senta aqui, na ponta da doca. Vou te contar o que a água me sussurra. Confio isso a quem aprendeu a ouvir — e você aprendeu.",
                MilestoneArrivalLine = "Você chegou há pouco. O lago já reparou em você antes de mim. Se precisar de comida fácil, a água não cobra caro de quem a respeita.",
                MilestonePostAct1Line = "Depois do que houve, o lago ficou estranho por dias — peixe fundo, superfície tensa. As águas sentem o que a vila sofre.",
                MilestonePostAct3Line = "O que você mexeu lá embaixo, eu senti aqui em cima. A pedra da minha oferenda amanheceu seca três dias. Algo antigo prestou atenção em você."
            },

            // ── Mella Forno-Quente — Padeira / Moleira (Humana) ──
            new NpcDialogueContent
            {
                NpcId = "npc_mella",
                DialogueSetId = "dialogue_mella",
                HasShop = true,
                Greetings = new[]
                {
                    "Entra, entra! Acabou de sair do forno — sente o cheiro? Pão quente conserta quase tudo.",
                    "Bem-vindo à padaria. Aqui ninguém sai de mãos vazias se eu puder evitar.",
                    "Ah, um rosto faminto. Não negue — eu reconheço fome a três passos. Pega um pão, conversa depois."
                },
                Role1 = "Sou Mella. Moo o grão da vila e asso o pão dela. Trigo entra de um lado, pão sai do outro — e a barriga de Cindar's Hope agradece.",
                Role2 = "Vim para cá recomeçar. Não falo do antes. Mas trouxe a mó do meu marido nas costas, por três províncias. Ela ainda gira; ele, não.",
                Town1 = "Pão é a cola silenciosa de uma vila. Brigam de dia, mas à mesa, partindo o mesmo pão, lembram que são gente.",
                Town2 = "Esta vila tem fome de mais que comida — tem fome de aconchego. Eu não conserto isso, mas um pão quente engana bem a falta.",
                Service1 = "Vendo pão, farinha, doces e bolo de festa. Compro o seu trigo — quanto mais colheita você traz, mais a vila come.",
                Service2 = "Traga grão e eu moo na hora. Farinha fresca rende pão melhor; pão melhor rende gente melhor. É a minha teologia.",
                AdviceLines = new[]
                {
                    "Massa boa não tem pressa. Sove, descanse, sove de novo. A vida é igual.",
                    "Nunca durma de barriga vazia nem de coração cheio de raiva. As duas coisas azedam.",
                    "Quem alimenta os outros nunca passa fome de verdade — alguém sempre devolve o pão."
                },
                Rumor1 = "Os pães que sobram no fim do dia? Eu... dou. Para quem precisa. Não conte a ninguém — caridade contada vira vaidade, e eu não asso vaidade.",
                Rumor2 = "Toda data certa, asso o dobro e choro um pouco quando ninguém vê. É aniversário de uma despedida. O forno guarda meus segredos melhor que a gente.",
                Goodbyes = new[]
                {
                    "Vai com Deus e com o pão quente.",
                    "Não esquece de comer. Promete pra essa velha aqui.",
                    "Volta amanhã — tem fornada nova ao amanhecer."
                },
                SpringLine = "Primavera! O trigo verdeja e meu forno mal descansa. Época de massa leve e janela aberta — entra esse cheiro de vida.",
                SummerLine = "Verão. O forno transforma a padaria num inferno gostoso. Asso de madrugada, antes do sol competir comigo no calor.",
                AutumnLine = "Outono é a minha estação. A colheita chega, o moinho gira sem parar, e a vila estoca pão para o frio. Trabalho que enche a alma.",
                WinterLine = "Inverno. O forno é o lugar mais quente da vila, e a porta fica encostada de propósito. Quem tiver frio, que entre e amasse comigo.",
                RainLine = "Chuva lá fora, brasa aqui dentro. Dia perfeito para um pão demorado e uma conversa demorada. Senta perto do forno.",
                FestivalLine = "Dia de festa! Asso bolo até as mãos doerem. Ninguém celebra de barriga vazia — essa é a minha contribuição para a alegria de todos.",
                FriendStrangerLine = "Você de novo, faminto. Já estou guardando o melhor pedaço para você sem nem saber seu nome direito. Coisa de padeira.",
                FriendWarmLine = "Ah, é você! Senta, senta. Tem pão saindo e uma cadeira quentinha. Você virou parte da minha fornada de gente querida.",
                FriendCloseLine = "Meu bem, chega aqui. Para você eu asso o que não vendo: o pão da minha terra, a receita que sobrou dele. É o que eu tenho de mais meu.",
                MilestoneArrivalLine = "Você chegou faz pouco, magrelo. Come direito? Pega esse pão — em Cindar's Hope, ninguém recomeça de estômago vazio. Eu sei como é recomeçar.",
                MilestonePostAct1Line = "Depois do susto, assei o triplo. Gente assustada come mais e fala menos; eu prefiro alimentar do que ficar repetindo a desgraça.",
                MilestonePostAct3Line = "O que você fez, a vila inteira vai lembrar. Assei um bolo só seu — sem motivo de venda. Só porque você merece um doce e poucos sabem disso."
            },

            // ── Hess Couro-Fundo — Curtidor (Draconato) ──
            new NpcDialogueContent
            {
                NpcId = "npc_hess",
                DialogueSetId = "dialogue_hess",
                HasShop = true,
                Greetings = new[]
                {
                    "Devagar. O couro ensina pressa nenhuma, e eu sou feito do que ensino.",
                    "Você chegou. O cheiro espanta os apressados; quem fica, costuma valer a conversa.",
                    "Hmm. Mãos de quem trabalha. Senta. Velho gosta de companhia que não fala demais."
                },
                Role1 = "Sou Hess. Curto as peles que vêm da caverna e do campo — viro morte em abrigo, ferida em armadura. Ofício honesto, cheiro forte.",
                Role2 = "Sou o mais velho desta vila, por minha conta. Perdi o costume de medir os anos; meço o couro, que mente menos que o calendário.",
                Town1 = "Moro na beira, e está bom assim. O cheiro do tanino afasta a fofoca, e o silêncio é o único vizinho que nunca me decepcionou.",
                Town2 = "Cindar's Hope é jovem e afobada. Eu sou a parte lenta dela — a que lembra que tudo que dura foi, primeiro, curtido com paciência.",
                Service1 = "Vendo couro, correias e armadura leve. Compro hides — do gado da vila ou das feras que você abater lá embaixo. Tudo vira algo útil.",
                Service2 = "Eu curto, a Mirela costura. Couro meu, linha dela — a melhor armadura leve da vila sai dessas duas mãos teimosas trabalhando juntas.",
                AdviceLines = new[]
                {
                    "Pressa estraga o couro e o homem. O que se faz devagar, dura.",
                    "Respeite o que morreu para te vestir. Quem despreza a pele, despreza a própria.",
                    "Mão calejada não tem vergonha. Vergonha é mão limpa que nunca fez nada."
                },
                Rumor1 = "Eu agradeço cada pele antes de curtir. Toda pele guarda a memória do bicho — o medo, a corrida, o último fôlego. Curtir sem agradecer é roubo.",
                Rumor2 = "Tem uma pele que eu nunca vou curtir. Está pendurada no fundo, intacta. Era de algo que me olhou nos olhos antes de cair. Esse couro não é mercadoria.",
                Goodbyes = new[]
                {
                    "Vá devagar. O mundo não foge.",
                    "O que tiver de durar, durará. Volte quando precisar.",
                    "Leve o couro e a paciência. O segundo é de graça."
                },
                SpringLine = "Primavera. As peles secam rápido neste sol novo. Boa estação para curtir — e para um velho fingir que ainda tem primaveras pela frente.",
                SummerLine = "Verão. O tanino fede mais no calor, e eu trabalho na sombra. Mas o couro cura bem; o sol é um curtidor mais velho que eu.",
                AutumnLine = "Outono. A estação que entendo na pele: tudo se prepara para partir. Eu curto o que o verão matou. Há respeito nisso, não tristeza.",
                WinterLine = "Inverno. O frio engrossa o couro e os meus ossos. Trabalho perto da brasa, devagar. O inverno e eu nos respeitamos de longe.",
                RainLine = "Chuva atrapalha a secagem, mas apura a conversa. Entra, foge do temporal. Couro espera; gente boa, nem sempre.",
                FestivalLine = "Dia de festa lá na praça. Eu fico aqui, com as peles. Barulho de mais para um velho — mas mandei correias novas para os músicos. Festejo do meu jeito.",
                FriendStrangerLine = "Você voltou ao cheiro forte. Resistiu — já é mais do que a maioria. O couro gosta de teimoso, e eu também.",
                FriendWarmLine = "Ah, você. Mãos cada vez mais firmes. Separei um couro melhor — quem honra o ofício merece o material que honra de volta.",
                FriendCloseLine = "Chega perto, jovem. Vou te mostrar a pele do fundo — a que não curto. Mostro a poucos. Você entende de respeito; por isso pode ver.",
                MilestoneArrivalLine = "Você é novo aqui. Traga-me hides e leve couro — começo honesto. Em Cindar's Hope, quem trabalha com as mãos nunca passa necessidade.",
                MilestonePostAct1Line = "Depois do que veio da caverna, recebi peles que eu não reconhecia. Curti com mais reza que de costume. Algo lá embaixo está acordando.",
                MilestonePostAct3Line = "O que você enfrentou deixou marca até no couro que me trazem. Guardei a melhor pele para a sua armadura. Poucos couros merecem carregar um nome."
            },

            // ── Tibbet Vela-Torta — Coveiro e Coroinha (Gnomo; segredo: adora Nyx) ──
            new NpcDialogueContent
            {
                NpcId = "npc_tibbet",
                DialogueSetId = "dialogue_tibbet",
                HasShop = false,
                Greetings = new[]
                {
                    "Oh! Olá. Desculpe a terra nas mãos — estava... arrumando uma cova. Alguém precisa cuidar de quem partiu, não é?",
                    "Shhh, fale baixinho. Os que descansam aqui merecem sossego. Mas pode ficar; companhia dos vivos também faz bem.",
                    "Bem-vindo. Não tenha medo do cemitério — é o lugar mais honesto da vila. Aqui ninguém finge mais nada."
                },
                Role1 = "Sou Tibbet, auxiliar do Padre Corvus. De dia sirvo no altar de Kanthor; de noite, cuido das covas. Acendo velas para os dois lados do dia.",
                Role2 = "Cavo, rezo, acendo, vigio. O Padre cuida das almas; eu cuido da terra que as cobre. Alguém tem que velar quando o templo dorme.",
                Town1 = "Cindar's Hope é boa com os vivos e esquece os mortos rápido demais. Eu não esqueço. Conheço cada nome neste chão — alguns que a vila já apagou.",
                Town2 = "A vila reza para Kanthor, a Ordem. Mas a noite tem dona, e ela é mais antiga que qualquer ordem. Os mortos sabem disso melhor que nós.",
                Service1 = "Não vendo nada — cuido. Se precisar enterrar alguém com dignidade, ou de uma bênção que o Padre está ocupado demais para dar, me procure.",
                Service2 = "Acendo uma vela por quem você perdeu, se me der o nome. É de graça. A luz é pouca, mas no escuro até pouca luz é uma companhia.",
                AdviceLines = new[]
                {
                    "Não tema o escuro. O escuro só guarda o que a luz cansou de carregar.",
                    "Trate bem os mortos. Um dia você vai ser um, e vai querer alguém acendendo a sua vela.",
                    "A noite não é o fim do dia. É a parte dele que aprende a ficar quieta."
                },
                Rumor1 = "O Padre diz que os mortos vão para a luz de Kanthor. Eu... acho que vão para outro lugar. Mais macio. Mais escuro. Mas não conte isso a ele, por favor.",
                Rumor2 = "Às vezes, à meia-noite, deixo uma vela apagada sobre as covas — não acesa. É um costume meu. O Padre não entenderia, e eu não saberia explicar sem... assustá-lo.",
                Goodbyes = new[]
                {
                    "Vá com a luz — e com a falta dela, que também cuida.",
                    "Volte quando quiser. Os mortos não reclamam de visita, e eu também não.",
                    "Que a noite seja mansa com você."
                },
                SpringLine = "Primavera. Até no cemitério nasce flor — bem sobre as covas, das mais bonitas. Os mortos adubam a vida. Acho isso lindo, não triste.",
                SummerLine = "Verão. Cavo de manhã cedo, antes do calor. À noite o cemitério fica fresco e estrelado — meu momento preferido. Eu e o céu escuro.",
                AutumnLine = "Outono. A estação favorita de quem cuida dos mortos: as folhas caem como pequenas despedidas. Varro com cuidado; cada folha foi viva.",
                WinterLine = "Inverno. A terra endurece e cavar custa. Acendo mais velas — o frio dos vivos eu não aqueço, mas o dos que partiram, ao menos eu ilumino.",
                RainLine = "Chuva no cemitério é triste e bonita. Lava as lápides, deixa os nomes legíveis de novo. A chuva lembra o que a vila esquece.",
                FestivalLine = "Dia de festa. Eu apareço pouco — alegria demais perto das covas parece desrespeito. Mas acendo uma vela a mais, para os que não podem mais festejar.",
                FriendStrangerLine = "Você voltou ao cemitério por vontade própria. Pouca gente faz isso. Os mortos gostam de você; eu também começo a gostar.",
                FriendWarmLine = "Ah, é você de novo! Que bom. Aqui a companhia viva é rara. Senta comigo entre as velas — o silêncio fica menos pesado a dois.",
                FriendCloseLine = "Posso te confiar uma coisa? Eu rezo para Nyx, a Noite — não para Kanthor. Acho que os mortos são dela. Não conte ao Padre. Você é o único que sabe.",
                MilestoneArrivalLine = "Você chegou há pouco. Bem-vindo. Se um dia perder alguém aqui, me procure — eu cuido para que ninguém parta sem uma vela acesa.",
                MilestonePostAct1Line = "Depois do que houve, cavei mais covas do que queria. Rezei por todas — às escondidas, do meu jeito. A Noite recebeu cada uma com cuidado.",
                MilestonePostAct3Line = "O que você fez tocou até os que descansam aqui — senti as velas tremerem sem vento. Nyx reparou em você. Não sei se isso é bênção ou aviso."
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
                    ConditionalLines = BuildGreetingConditionalLines(content),
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

        /// <summary>
        /// fable_28 — materializes the per-NPC conditional greeting pool from the authored fields.
        /// Each non-null field becomes a <see cref="ConditionalDialogueLine"/> with the matching
        /// <see cref="DialogueLineCondition"/>. Order is fixed (season → rain → festival → friendship
        /// → milestones) so tie-breaking is deterministic. The base greeting stays the fallback, so
        /// this pool never needs to be exhaustive and selection is never empty.
        /// </summary>
        public static List<ConditionalDialogueLine> BuildGreetingConditionalLines(NpcDialogueContent content)
        {
            var lines = new List<ConditionalDialogueLine>();

            AddIf(lines, content.SpringLine, new DialogueLineCondition { Season = Season.Primavera });
            AddIf(lines, content.SummerLine, new DialogueLineCondition { Season = Season.Verao });
            AddIf(lines, content.AutumnLine, new DialogueLineCondition { Season = Season.Outono });
            AddIf(lines, content.WinterLine, new DialogueLineCondition { Season = Season.Inverno });

            // Wet weather = Rainy OR Stormy (parity with WorldWeatherService.IsWetWeather). The
            // condition matches a single WeatherType, so the same line is registered for both so it
            // fires on a stormy day too (still 1 authored line; deterministic tie-break unaffected).
            AddIf(lines, content.RainLine, new DialogueLineCondition { Weather = WeatherType.Rainy });
            AddIf(lines, content.RainLine, new DialogueLineCondition { Weather = WeatherType.Stormy });
            AddIf(lines, content.FestivalLine, new DialogueLineCondition { RequiresFestivalDay = true });

            AddIf(lines, content.FriendStrangerLine, new DialogueLineCondition { MinFriendship = 0 });
            AddIf(lines, content.FriendWarmLine, new DialogueLineCondition { MinFriendship = FriendshipWarmMin });
            AddIf(lines, content.FriendCloseLine, new DialogueLineCondition { MinFriendship = FriendshipCloseMin });

            AddIf(lines, content.MilestoneArrivalLine, new DialogueLineCondition { RequiredFlag = FlagMainArrival });
            AddIf(lines, content.MilestonePostAct1Line, new DialogueLineCondition { RequiredFlag = FlagMainPostAct1 });
            AddIf(lines, content.MilestonePostAct3Line, new DialogueLineCondition { RequiredFlag = FlagMainPostAct3 });

            return lines;
        }

        private static void AddIf(List<ConditionalDialogueLine> lines, string text, DialogueLineCondition condition)
        {
            if (!string.IsNullOrEmpty(text))
            {
                lines.Add(new ConditionalDialogueLine(text, condition));
            }
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

            // fable_10 — in-conversation main-quest offer (same OfferQuest action as Thalindra).
            if (!string.IsNullOrEmpty(content.OfferedMainQuestId))
            {
                choices.Add(new DialogueChoice
                {
                    Label = string.IsNullOrEmpty(content.OfferQuestLabel) ? "Sobre a Fonte..." : content.OfferQuestLabel,
                    NextNodeId = string.Empty,
                    ActionType = DialogueActionType.OfferQuest,
                    ActionPayload = content.OfferedMainQuestId
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
