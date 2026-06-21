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
                Goodbyes = new[] { "Que a Fonte o acompanhe.", "Volte quando o peso for grande demais.", "Va em paz, e volte inteiro." },
                SpringLine = "A primavera reacende as velas com mais facilidade. Ate a cera parece mais leve nesta estacao.",
                SummerLine = "Verao. O templo fica fresco enquanto la fora o sol castiga. Entre, descanse a alma e o suor.",
                AutumnLine = "No outono as oferendas mudam: trazem folha seca e gratidao. Prefiro essa a moeda, confesso.",
                WinterLine = "Inverno em Cindar's Hope. O frio aproxima as pessoas da Fonte. A dor sempre soube o caminho do templo.",
                RainLine = "A chuva lava a praca e traz fieis encharcados. Deixe o manto secar perto das velas, viajante.",
                FestivalLine = "Dia de festa. O templo nao compete com a praca; apenas guarda um canto de silencio para quem cansar do barulho.",
                FriendStrangerLine = "Que a Fonte ilumine seus passos, viajante. Ainda nao conheco seu nome, mas conheco seu cansaco.",
                FriendWarmLine = "Voce de novo. Ja reconheco seus passos no corredor. Isso, para um guardiao, ja e quase amizade.",
                FriendCloseLine = "Ah, e voce. Sente-se. Guardei uma vela acesa pensando que viria. Algumas presencas a gente aprende a esperar.",
                MilestoneArrivalLine = "Voce chegou ha pouco a Cindar's Hope. A Fonte ja o notou; ela nota todos que descem com perguntas.",
                MilestonePostAct1Line = "Depois do que houve, a cidade respira diferente. Eu acendo uma vela a mais por noite agora.",
                MilestonePostAct3Line = "Voce mudou o rumo de coisas antigas. O templo guardara seu nome ao lado do guerreiro da praca.",
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
                Goodbyes = new[] { "Proximo!", "Leve seus papeis, nao os meus.", "Ate logo. E nao perca o protocolo." },
                SpringLine = "Primavera: epoca de registrar lote novo. A fila de licenca de plantio dobra. Pegue uma senha.",
                SummerLine = "Verao seca a tinta rapido demais. Assine com calma, ou seu nome fica borrado para sempre.",
                AutumnLine = "Outono e mes de prestacao de contas. Traga seus recibos de colheita antes que eu va atras deles.",
                WinterLine = "Inverno. Menos gente, mais papelada atrasada para por em ordem. Aproveito o frio para arquivar.",
                RainLine = "Chuva? Sacuda o casaco antes de entrar. Documento molhado e documento perdido, e eu nao reescrevo.",
                FestivalLine = "Dia de festival ate o registro fecha mais cedo. Mas se for urgente, eu carimbo. So hoje. So por voce.",
                FriendStrangerLine = "Bom dia. Voce ainda nao consta no meu registro. Vamos resolver isso: nome, origem e motivo.",
                FriendWarmLine = "Ah, ja conheco seu protocolo. Sente-se, sua papelada hoje vai mais rapido. Eficiencia se constroi.",
                FriendCloseLine = "Voce de novo, e bem-vindo. Confesso que separo seus documentos com um cuidado que nao dou aos outros.",
                MilestoneArrivalLine = "Voce e novo na cidade. Vou abrir um registro com seu nome. Memoria oficial comeca hoje.",
                MilestonePostAct1Line = "Depois daqueles dias, tive que abrir uma pasta nova so para os fatos estranhos. Voce esta nela.",
                MilestonePostAct3Line = "Seu nome agora aparece em atas que vao durar mais que nos dois. Cuide bem dessa tinta."
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
                Goodbyes = new[] { "Carimbado. Proximo.", "Boa sorte com a papelada.", "Volte com o formulario certo." },
                SpringLine = "Primavera traz pedido de banca de mercado em pilha. Plante seu alvara antes da colheita de gente.",
                SummerLine = "Verao e temporada alta de aventureiro. Mais alvara de grupo, mais carimbo, menos cafe para mim.",
                AutumnLine = "Outono: prazo de renovacao. Quem nao renovar o alvara antes da primeira geada, paga dobrado.",
                WinterLine = "Inverno acalma o balcao. Bom para revisar formularios; ruim para a minha pilha de mate frio.",
                RainLine = "Com essa chuva, o carimbo borra. Espere secar ou leve o documento manchado. Sua escolha, sua taxa.",
                FestivalLine = "Festival exige licenca especial para barraca. Tenho o formulario F-9 aqui. Preenchido, e claro.",
                FriendStrangerLine = "Licencas e alvaras. Voce ainda nao tem ficha aqui? Entao comecamos pelo basico, sem atalho.",
                FriendWarmLine = "Ja reconheco seu pedido de longe. Vou adiantar: traga o formulario certo e a gente termina rapido.",
                FriendCloseLine = "Voce de novo. Para voce, eu ja deixo o carimbo na mao. Confianca tambem e um tipo de alvara.",
                MilestoneArrivalLine = "Recem-chegado. Seu primeiro carimbo nesta cidade sai daqui. Guarde-o; o resto vem mais facil.",
                MilestonePostAct1Line = "Depois da confusao, o conselho aumentou o controle de entrada. Mais formulario para todos. Inclusive voce.",
                MilestonePostAct3Line = "Com o que voce fez, ate liberaram alvaras que eu jurava que morreriam na gaveta. Impressionante."
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
                Goodbyes = new[] { "Boa colheita!", "Va com as maos cheias e volte com elas vazias!", "Que a chuva venha na hora certa." },
                SpringLine = "PRIMAVERA! Minha estacao favorita! Sementes novas, terra acordando, tudo querendo brotar. Sente o cheiro?",
                SummerLine = "Verao puxado. Regue de manha, ouviu? A tarde o sol bebe a agua antes da raiz. Tenho semente de sol aqui.",
                AutumnLine = "Outono e colheita farta e plantio de raiz. A terra da o ultimo empurrao antes de dormir. Aproveite.",
                WinterLine = "Inverno gela o solo, mas nao a vontade de plantar. Tenho sementes resistentes e mudas de estufa, viu?",
                RainLine = "Chuva boa! O solo agradece e eu tambem. Dia de chuva e dia de planejar canteiro, nao de regar. Folga!",
                FestivalLine = "Festival! Trouxe sementes especiais so para hoje. Plante uma lembranca do dia de festa no seu canteiro!",
                FriendStrangerLine = "Ola! Rosto novo na banca! Se as suas maos ainda estao limpas de terra, a gente resolve isso rapidinho.",
                FriendWarmLine = "Ei, voce! Ja sei do que suas plantas gostam. Trouxe uma semente pensando no seu canteiro, da uma olhada.",
                FriendCloseLine = "Meu fazendeiro favorito chegou! Separei a melhor muda da safra para voce. Amizade rende boa colheita.",
                MilestoneArrivalLine = "Voce e o da fazenda nova, ne? Bem-vindo ao vale! Comece com semente facil; a terra daqui tem humor.",
                MilestonePostAct1Line = "Depois do susto, o solo parece mais firme. Ou e impressao minha de quem so quer ver tudo brotar de novo.",
                MilestonePostAct3Line = "Dizem que ate as sementes da caverna respiram diferente agora, gracas a voce. Plantei uma. Brilhou bonito."
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
                Goodbyes = new[] { "Volte sempre! E traga amigos!", "Negocio fechado e amizade mantida.", "Se faltar algo, ja sabe onde achar." },
                SpringLine = "Primavera move o estoque! Todo mundo precisa de ferramenta nova para a terra acordada. Bom para o negocio!",
                SummerLine = "Verao quente vende corda, cantil e chapeu. Eu? Vendo de tudo, mas hoje o cantil sai voando da prateleira.",
                AutumnLine = "Outono e mes de estocar para o frio. Compre agora; no inverno o preco sobe e a culpa nao e minha.",
                WinterLine = "Inverno. Lampiao, oleo e cobertor saem bem. E corda, sempre corda. Ninguem se arrepende de ter corda.",
                RainLine = "Chuva enche minha banca de gente abrigada e de poca. Pise com cuidado e leve um lampiao a prova d'agua!",
                FestivalLine = "Festival e dia de ouro! Lembrancinha, bandeira, fita... tudo barato, tudo no precinho de festa. Aproveite!",
                FriendStrangerLine = "Cliente novo! Renko tem de tudo, e o que nao tem, consegue ate amanha. Diga o que procura, sem vergonha.",
                FriendWarmLine = "Voce voltou! Cliente bom e cliente que volta. Ja vou separando aquilo que costuma levar, da uma olhada.",
                FriendCloseLine = "Meu melhor cliente! Para voce tem o preco de amigo e o cafe da casa. Negocio fechado e amizade mantida.",
                MilestoneArrivalLine = "Rosto novo na cidade! Bem-vindo. Comece a equipar essa mochila comigo; o resto da estrada agradece.",
                MilestonePostAct1Line = "Depois daquilo, as caravanas ficaram nervosas. Mercadoria sobe de preco. Mas para voce eu seguro o que da.",
                MilestonePostAct3Line = "Com o que voce fez, ate as rotas do norte reabriram! Vou ter mercadoria que essa cidade nunca viu. Obrigado!"
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
                Goodbyes = new[] { "Volte para a prova final!", "Cuide das suas bainhas, querido.", "Ate mais. E nada de rasgar isso de novo!" },
                SpringLine = "Primavera pede tecido leve e cor viva. Chegou um linho de Dornecia que parece feito de petala. Quer ver?",
                SummerLine = "Verao e linho fino e chapeu de aba larga. Roupa pesada agora e castigo; deixe que eu alivio o seu corte.",
                AutumnLine = "Outono. Hora de forrar casaco e remendar o que o verao gastou. Traga suas pecas antes do frio bater.",
                WinterLine = "Inverno, querido! La, feltro e forro duplo. Roupa molhada na caverna e febre certa. Vista-se como gente.",
                RainLine = "Com essa chuva, troque essa roupa encharcada antes que pegue um resfriado. Tenho um seco aqui do seu tamanho.",
                FestivalLine = "Festival! Todo mundo quer estar bonito. Tenho fita, gola e ajuste rapido. Senta que em dez minutos voce brilha.",
                FriendStrangerLine = "Essa costura do seu casaco... senta, querido. Eu arrumo num instante. E de quebra fico sabendo seu nome.",
                FriendWarmLine = "Ah, voce! Ja sei suas medidas de cor. Separei um tecido que combina com o seu jeito. Da uma olhada.",
                FriendCloseLine = "Meu cliente querido! Guardei o melhor retalho da estacao pensando em voce. Roupa boa apresenta quem voce e.",
                MilestoneArrivalLine = "Rosto novo! Bem-vindo. Pela sua roupa de estrada, voce veio de longe. Deixa eu dar uns pontos de boas-vindas.",
                MilestonePostAct1Line = "Depois daquilo, vi muita roupa rasgada de caverna passar por aqui. Tempos dificeis se leem no tecido.",
                MilestonePostAct3Line = "Sabe o que mudou desde o que voce fez? Voltei a costurar roupa de festa. Isso, para uma alfaiate, e esperanca."
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
                Goodbyes = new[] { "Boa estrada! E volte para a sopa.", "A porta fecha tarde, ate logo.", "Que seus sonhos sejam de colheita cheia." },
                SpringLine = "Primavera enche meu salao de viajante novo. A sopa de raiz nova esta uma delicia. Cama seca tambem tem.",
                SummerLine = "Verao e movimento! O salao ferve de historia de aventureiro. Quarto fresco no andar de cima, se quiser.",
                AutumnLine = "Outono pede sopa mais grossa e lareira acesa. Bom para hospede cansado de uma colheita puxada. Entre.",
                WinterLine = "Inverno! A lareira eterna nunca foi tao bem-vinda. Sopa quente resolve oitenta por cento dos problemas.",
                RainLine = "Chuva la fora? O vento nao paga aluguel, mas a cama seca e a sopa quente, sim. Entre e sacuda esse casaco.",
                FestivalLine = "Dia de festa enche meu salao ate a porta. Tem musica, tem caldo e tem cama para quem exagerar na comemoracao.",
                FriendStrangerLine = "Bem-vindo a Estalagem do Vale! Rosto novo sempre traz historia nova. Cama seca, sopa quente e seu nome, depois.",
                FriendWarmLine = "Voce de novo! Ja sei seu quarto favorito e o ponto da sua sopa. Hospede que volta vira quase familia aqui.",
                FriendCloseLine = "Meu hospede de confianca! Deixei a lareira acesa do seu lado e a sopa no ponto que voce gosta. Sente-se.",
                MilestoneArrivalLine = "Recem-chegado ao vale! Metade dos herois desta cidade acordou nas minhas camas. Voce sera o proximo nome.",
                MilestonePostAct1Line = "Depois daqueles dias, o salao virou ponto de conversa baixa. Sente-se e escute; a estalagem ouve tudo.",
                MilestonePostAct3Line = "Desde o que voce fez, as historias contadas aqui ganharam final feliz. Por sua conta, a primeira sopa."
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
                Goodbyes = new[] { "Vai com Deus e volta com sede!", "Cuidado no caminho, a rua gira as vezes!", "Proxima rodada tem teu nome!" },
                SpringLine = "Primavera! O barril novo ta fresco e a sidra de flor acabou de descansar. Senta que a primeira espuma e tua!",
                SummerLine = "Verao da sede, e sede da movimento! Cerveja gelada no balcao, e o salao so esquenta quando a Liora aparece.",
                AutumnLine = "Outono e mes de festa de colheita. Alugo o salao, sirvo o melhor caldo e a cerveja de mel fica perfeita!",
                WinterLine = "Inverno! Vinho quente, lareira e historia comprida. NUNCA desca a caverna de ressaca no frio. NUNCA, ouviu?",
                RainLine = "Chuvarada la fora? Otimo! Chuva enche minha taverna de gente sedenta e seca. Senta perto do fogo, paga depois!",
                FestivalLine = "DIA DE FESTA! Alugo o salao, a Liora canta, e a cerveja corre solta! Hoje ate o Zrix bebe. Senta, comemoramos!",
                FriendStrangerLine = "HAH! Mais um sedento! Rosto novo no balcao. Senta, primeiro copo a gente conversa e eu decoro tua cara.",
                FriendWarmLine = "Ei, voce! Ja sei do que tu gosta. Senta no teu banco de sempre que eu ja sirvo. Aqui dentro ninguem e estranho!",
                FriendCloseLine = "AH, chegou! Meu fregues de confianca! Esse copo e por minha conta. Quem paga a rodada faz amigos, e tu ja es um!",
                MilestoneArrivalLine = "Cara nova na cidade! Bem-vindo! No meu balcao todo mundo senta no mesmo banco. Primeiro gole pra quebrar o gelo!",
                MilestonePostAct1Line = "Depois daquilo, o salao ficou mais cheio e mais calado. As pessoas vem beber a tensao. Eu mantenho os copos cheios.",
                MilestonePostAct3Line = "Desde o que tu fez, voltaram a rir alto no meu salao! Isso vale mais que ouro. Rodada da casa pra comemorar!"
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
                Goodbyes = new[] { "Vai. E nao quebra isso de novo.", "Hm. Ate.", "Volta quando o fio cansar." },
                SpringLine = "Primavera. O fogo pega mais facil com o ar morno. Bom para forjar enxada. Fala logo o que precisa.",
                SummerLine = "Verao. A forja ja e quente; agora e forno duplo. Bebo agua e bato ferro. Se vai me pedir lamina, e agora.",
                AutumnLine = "Outono. Epoca de afiar tudo antes que o frio enrijeça o aco. Traz tua lamina, eu dou o fio de volta.",
                WinterLine = "Inverno. Forja acesa e o melhor lugar da cidade no frio. Cidade que tem forja acesa nao morre. Pode anotar.",
                RainLine = "Chuva. A fagulha briga com a umidade, mas o ferro nao espera. Cuidado com o chao molhado perto da bigorna.",
                FestivalLine = "Festival. Hm. Eu forjo igual, festa ou nao. Mas reconheco: ate eu limpo a fuligem da barba num dia desses.",
                FriendStrangerLine = "Fala logo, o ferro nao espera. Cara nova. Diz o que quer que eu nao tenho o dia todo. ...Bem-vindo, suponho.",
                FriendWarmLine = "Hm. Voce de novo. A lamina aguentou? Ja sei o teu peso de mao. Vou ajustar o proximo corte pensando nisso.",
                FriendCloseLine = "Ah, e voce. Pode entrar sem cerimonia. Guardei um aco bom esperando alguem que saiba usar. Acho que e teu.",
                MilestoneArrivalLine = "Cara nova na cidade. Se vai descer a caverna, vai precisar de ferro honesto. O meu e. Comeca pelo basico.",
                MilestonePostAct1Line = "Depois daquilo, recebi minerio estranho da caverna que CANTA no fogo. Guardei. Metal assim conta historia ruim.",
                MilestonePostAct3Line = "O que voce fez... meu avo forjou a espada da estatua e nunca fiz igual. Por voce, vou tentar de novo. Ainda."
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
                Goodbyes = new[] { "Vai pela sombra, a poeira agradece!", "Pedra no caminho? Me chama!", "Ate! E olha o carrinho!" },
                SpringLine = "Primavera amolece a terra em volta da pedra. Corte mais facil, veio mais limpo. Boa epoca para fundacao!",
                SummerLine = "Verao racha pedra mal cortada. O calor encontra cada falha. Corto devagar e bebo muita agua. Sente a poeira?",
                AutumnLine = "Outono e bom para entregar pedra antes da chuva forte. Calce sua fundacao agora, ou o inverno cobra o atraso.",
                WinterLine = "Inverno gela a pedra e a torna traicoeira. Pedra fria racha onde voce menos espera. Trabalho com respeito redobrado.",
                RainLine = "Chuva! Pedra molhada engana o pe, na pedreira e na caverna. Pisa devagar e bata na parede antes de confiar nela.",
                FestivalLine = "Festival? A pedreira para, mas eu nao. Brincadeira: hoje descanso. Ate pedra precisa de um dia sem picareta.",
                FriendStrangerLine = "Opa! Cuidado com o carrinho, ele desce sozinho. Cara nova! Se veio pela pedra, chegou ao lugar certo.",
                FriendWarmLine = "Voce de novo! Ja sei o tipo de pedra que serve pra voce. Separei umas aparelhadas, da uma olhada antes de pedir.",
                FriendCloseLine = "Ah, minha companhia favorita! Guardei a pedra de veio azul pensando em voce. So nao conta pros outros fregueses.",
                MilestoneArrivalLine = "Cara nova na cidade! Toda casa daqui tem um pedaco da minha pedreira. A sua tambem vai ter. Bem-vindo!",
                MilestonePostAct1Line = "Depois daquilo, achei tijolo ANTIGO escavando fundacao. Mais antigo que a cidade. Quem cavou aqui antes de nos?",
                MilestonePostAct3Line = "Sabe a base da estatua, que minha avo assentou? Desde o que voce fez, eu juro que ela parece mais firme. Orgulho."
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
                Goodbyes = new[] { "Siga em seguranca.", "Qualquer coisa, grite. Eu ouco.", "Ordem e prosperidade, cidadao." },
                SpringLine = "Primavera traz movimento ao portao. Mais fazendeiro saindo, mais aventureiro chegando. Ronda dobrada, olho aberto.",
                SummerLine = "Verao e temporada cheia. A entrada da caverna e meu pesadelo logistico: entra gente demais, sai gente de menos.",
                AutumnLine = "Outono acalma o fluxo. Bom para revisar o quadro de ocorrencias. Cidade segura e a que resolve rapido.",
                WinterLine = "Inverno esvazia a rua cedo. Ande pelo centro a noite; sombra e esconderijo. Aviso dado a guarda nunca e tempo perdido.",
                RainLine = "Com essa chuva, a visibilidade na ronda cai. Se for sair, avise o destino. Sempre. Chuva esconde mais que a noite.",
                FestivalLine = "Festival enche a praca, e praca cheia e meu trabalho dobrado. Aproveite a festa; eu fico de olho no que voce nao ve.",
                FriendStrangerLine = "Tudo em ordem por aqui. Voce que chegou desarruma? Cara nova. Algum problema a relatar, cidadao?",
                FriendWarmLine = "Voce de novo. Ja sei que nao da trabalho. Bom. Guarda gosta de rosto previsivel. Siga, e mantenha-se na rua.",
                FriendCloseLine = "Ah, e voce. Pode passar sem o ritual. Confio na sua palavra, e isso, vindo de um guarda, e o maior elogio que tenho.",
                MilestoneArrivalLine = "Identifique-se. ...Recem-chegado, entao. Primeiro rosto que o viajante ve, ultimo que o problema encontra. Bem-vindo.",
                MilestonePostAct1Line = "Depois daquilo, dobrei a ronda por conta propria. Algo grande passou rente a muralha sem deixar pegada. Fico atento.",
                MilestonePostAct3Line = "Desde o que voce fez, durmo um pouco melhor. Um pouco. Guarda veterano nunca relaxa de todo, mas reconhece quem ajuda."
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
                Goodbyes = new[] { "Volte com perguntas melhores. As suas ja sao boas.", "O arquivo nao fecha; eu que durmo.", "Leve conhecimento, deixe poeira." },
                SpringLine = "Primavera. A umidade nova faz mal aos pergaminhos antigos. Passo o dia conservando o que o tempo quer levar.",
                SummerLine = "Verao resseca a tinta velha ate o pergaminho rachar. Mantenho o arquivo na sombra; conhecimento teme o sol.",
                AutumnLine = "Outono e a melhor estacao para ler. Luz suave, ar seco. Separei uns registros que talvez lhe interessem.",
                WinterLine = "Inverno. O frio preserva os documentos, mas gela meus dedos ao copiar mapa. Pequeno preco pela memoria intacta.",
                RainLine = "Chuva! Faca silencio e nao goteje sobre as atas. Documento molhado e historia apagada, e eu nao reescrevo o tempo.",
                FestivalLine = "Festival? O arquivo continua aberto. Festa passa; registro fica. Mas confesso que anoto ate as datas das festas.",
                FriendStrangerLine = "Cuidado com a pilha da esquerda, ela desaba. Ah, um rosto novo. O arquivo recebe quem traz perguntas. As suas?",
                FriendWarmLine = "Voce de novo. Suas perguntas estao ficando melhores. Separei um diario que so mostro a quem sabe o que procura.",
                FriendCloseLine = "Ah, e voce. Sente-se. Confio em poucos o suficiente para mostrar os documentos do fundo. Voce e um deles.",
                MilestoneArrivalLine = "Rosto novo. Esta cidade foi fundada ao redor da Fonte por gente que fugia de algo. Voce chega no meio dessa historia.",
                MilestonePostAct1Line = "Depois daquilo, reabri um diario de 90 anos que descreve a caverna com 101 niveis. O mesmo numero de hoje. Curioso, nao?",
                MilestonePostAct3Line = "O que voce fez vai entrar nos registros. Cuidarei para que sua versao sobreviva ao tempo. Historia se escreve assim."
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
                Goodbyes = new[] { "Siga. E mantenha-se na estrada.", "Portao fecha ao ultimo sino.", "Va. Eu fico. E assim que funciona." },
                SpringLine = "Primavera. Pela manha sai suor para a fazenda; a noite volta cansaco e historia. O portao sul e o pulso da cidade.",
                SummerLine = "Verao. O fluxo na estrada nao para. Mantenho o caminho seguro a base de bota gasta e olho aberto. Saia cedo.",
                AutumnLine = "Outono encurta o dia. Volte antes do sol sumir; a estrada muda de cara no escuro. Vinte anos de muralha me ensinaram.",
                WinterLine = "Inverno. A estrada gela e some sob a neve. Se vir fumaca la fora, nao va investigar. Venha me chamar. Sempre.",
                RainLine = "Chuva apaga pegada e abafa som. Pessima noite para vigia, otima para quem nao quer ser visto. Fique na estrada.",
                FestivalLine = "Festival? Eu vigio igual. Festa atrai gente boa e gente que se aproveita da gente boa. Aproveite; eu fico de olho.",
                FriendStrangerLine = "Alto. Identifique-se. ...Costume antigo, nao leve a mal. Cara nova. Pode passar, mas a estrada exige respeito.",
                FriendWarmLine = "Ah, e voce. Pode passar. Ja reconheco seu passo no barro. Para um sentinela, isso e quase um aperto de mao.",
                FriendCloseLine = "Voce. Bem-vindo. Se sua familia vier visitar, me avise que eu agilizo. Para quem confio, o portao e mais leve.",
                MilestoneArrivalLine = "Recem-chegado pelo portao sul. Primeiro rosto que o viajante ve, ultimo que o problema encontra. Esse sou eu. Siga.",
                MilestonePostAct1Line = "Depois daquilo, algo grande passou rente a muralha tres noites. Sem pegada no barro fresco. Dobrei a vigilia.",
                MilestonePostAct3Line = "Desde o que voce fez, a estrada anda mais segura. Mantenho assim, como sempre. Mas reconheco a sua parte nisso. Va."
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
                Goodbyes = new[] { "Tchau tchau! Me chama se se perder!", "Vou contar pra todo mundo que voce passou aqui!", "ATE MAIS! Cuidado com o degrau! Esse ai! Esse!" },
                SpringLine = "PRIMAVERA! Tudo florindo! Eu fiz um mapa novo com as flores marcadas! Ta quase certo! Quer? Custa uma moedinha!",
                SummerLine = "Calor! Calor! A sopa do Orlan no verao vem com agua de fruta! Eu sei porque eu provo TODO dia! NAO conta!",
                AutumnLine = "Outono! As folhas caem e eu junto as mais bonitas pra vender! Marcador de livro de folha! Ninguem compra, mas eu tenho!",
                WinterLine = "Brrr! Inverno! Eu mostro o atalho quentinho que passa perto da forja do Brumdar! O calor e de graca, o atalho NAO!",
                RainLine = "CHUVA! Eu sei onde NAO molha indo pro mercado! Sei TODOS os beirais! Te levo seco por uma moeda! Confia! CONFIA!",
                FestivalLine = "FESTIVAL!!! O melhor dia do ANO! Eu decoro a praca, entrego convite e ainda ganho doce! Vem, eu te mostro tudo!",
                FriendStrangerLine = "OI! Voce e novo aqui, ne? NE?! Eu vi voce primeiro! Eu mostro onde fica tudo! Como voce chama? Como? COMO?!",
                FriendWarmLine = "VOCE voltou! Eu falei pra todo mundo que a gente e amigo! Te guardei um mapa especial! O melhor! So pra voce!",
                FriendCloseLine = "MEU MELHOR AMIGO CHEGOU! Eu fiz um desenho de nos dois! Ta na parede! Vou te mostrar TODOS os atalhos de graca! Quase!",
                MilestoneArrivalLine = "VOCE chegou faz pouco tempo, ne?! Eu sei TUDO da cidade! Te mostro a estatua, a sopa e onde a Gruta deixa eu ficar!",
                MilestonePostAct1Line = "Depois daquele dia assustador, todo mundo ficou serio. Eu nao gosto. Mas eu sei que VOCE ajudou! Eu vi! Quase vi!",
                MilestonePostAct3Line = "VOCE e o HEROI da cidade agora! Eu falei que conhecia voce ANTES de todo mundo! Vou colocar voce no meu mapa! No meio!"
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
                Goodbyes = new[] { "Volta amanha! A versao 2 estara pronta!", "Sai pela esquerda! A direita esta... em manutencao.", "Leva esse parafuso. Confia, leva." },
                SpringLine = "Primavera! Umidade ideal para calibrar engrenagem. Tudo expande na medida certa. Cuidado com a alavanca! Tarde demais.",
                SummerLine = "Verao dilata o metal! Minhas maquinas ficam birrentas no calor. Oleo na engrenagem toda lua, ouviu? TODA lua!",
                AutumnLine = "Outono e estacao de manutencao geral. Reviso esteira, roda e mola antes que o frio enrijeca tudo. Pega um parafuso.",
                WinterLine = "Inverno trava engrenagem fria! Aqueco a oficina e o oleo junto. Maquina que faz som novo no frio esta te avisando algo.",
                RainLine = "CHUVA! Perfeito para testar o telhado giratorio! ...Que ainda nao impermeabilizei. Fica longe daquela goteira ali!",
                FestivalLine = "Festival! Eu queria montar fogos mecanicos, mas o conselho VETOU. De novo. Foi UMA explosao, gente. Uma! Pequena!",
                FriendStrangerLine = "Nao encosta nessa alava... tarde demais. Otimo, agora voce e parte do teste! Cara nova! Voce tem dois bracos, certo?",
                FriendWarmLine = "Ah, voce! Ja sei que voce nao quebra minhas coisas de proposito. Confianca rara por aqui! Da uma olhada na bancada.",
                FriendCloseLine = "Meu cobaia... digo, parceiro favorito! Pra voce eu mostro o projeto secreto. Se explodir, a culpa e 50/50, combinado?",
                MilestoneArrivalLine = "Cara nova na cidade! Esta cidade tem potencial mecanico ABSURDO e ninguem me ouve! Mas voce vai ouvir, ne? NE?",
                MilestonePostAct1Line = "Depois daquilo, achei um mecanismo na caverna que GIRA SOZINHO. Sem corda, sem mola. Ta na bancada. Girando. Ha semanas.",
                MilestonePostAct3Line = "Sabe o que voce fez? Me deu coragem de ligar a tal engenhoca que gira sozinha no resto da oficina. Vai dar certo! Acho!"
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
                Goodbyes = new[] { "Vai com cuidado e longe do andaime!", "Obra te chama, eu atendo!", "Devolve o capacete na saida!" },
                SpringLine = "Primavera e temporada de obra! Todo mundo quer celeiro novo antes do plantio. Minha equipe nao para. Pega um capacete!",
                SummerLine = "Verao e o melhor pra erguer telhado. Madeira seca rapido, argamassa cura firme. Telhado bom no verao agradece no inverno.",
                AutumnLine = "Outono: termine a obra antes da chuva pesada. Fundacao na lama nao pega. Pressa agora evita prejuizo depois.",
                WinterLine = "Inverno desacelera a obra, mas nao para. Aproveito pra planejar fundacao. Aquilo NAO cai nem com terremoto, pode crer.",
                RainLine = "Chuva! Obra parada, andaime escorregadio. Fica longe da estrutura molhada. Madeira verde com chuva entorta feio!",
                FestivalLine = "Festival! Ate a equipe folga. Mas a base da estatua, que eu reforcei, aguenta a multidao toda pulando. Pode comemorar!",
                FriendStrangerLine = "Cuidado com a viga! Ah, oi, pensei que era a queda de material. Cara nova! Se veio olhar, pega um capacete tambem.",
                FriendWarmLine = "Voce de novo! Ja confio em te deixar perto da obra sem capacete... quase. Pega um mesmo assim. Seguranca primeiro!",
                FriendCloseLine = "Ah, meu parceiro de obra! Pra voce eu faco o pacote completo com preco de amigo: fundacao, estrutura e telhado. Fechado?",
                MilestoneArrivalLine = "Cara nova na cidade! A cidade cresce mais rapido que minha equipe. Bom problema. Sua fazenda vai precisar de mim, ja aviso.",
                MilestonePostAct1Line = "Depois daquilo, tem uma rachadura no muro leste que conserto toda semana. Toda semana ela volta. No mesmo desenho. Esquisito.",
                MilestonePostAct3Line = "Sabe a base da estatua que eu reforcei? Desde o que voce fez, ela virou simbolo. Construir pra voce e construir pra historia."
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
                Goodbyes = new[] { "A noite te acompanha.", "Nao me viu, nao falamos. Mas volte.", "Leve o embrulho. Nao abra na frente da guarda." },
                SpringLine = "Primavera. As noites encurtam e meu horario tambem. Compre cedo; o que o dia nao explica, a primavera revela rapido.",
                SummerLine = "Verao. Noites mornas, mercado movimentado. Mais gente acorda tarde, mais gente acha minha banca. Bom para os negocios.",
                AutumnLine = "Outono. A escuridao se estende e meus melhores itens aparecem. Outono e quando a noite fica generosa, sussurrando.",
                WinterLine = "Inverno. Noite longa, freguesia seleta. Quem enfrenta o frio para me achar quer mesmo o que vendo. Aproxime-se.",
                RainLine = "Chuva abafa passos e olhos curiosos. Otima noite para um negocio discreto. Fale baixo; a chuva guarda segredo melhor que eu.",
                FestivalLine = "Festival? O dia e da praca. Mas quando o ultimo fogo de festa apaga, a noite e minha, e ela tem ofertas que a festa nao tem.",
                FriendStrangerLine = "Shhh. Fale baixo. Voce achou o mercado noturno, ou ele achou voce. Cara nova. Aqui vendemos o que o dia nao explica.",
                FriendWarmLine = "Ah, voce de novo. Ja sei o tipo de curiosidade que move voce. Tenho algo guardado que talvez sirva. Talvez. Aproxime-se.",
                FriendCloseLine = "Voce. Para os de confianca, eu abro a gaveta de baixo. O que ha nela nao tem procedencia que eu conte em voz alta. Mas e seu.",
                MilestoneArrivalLine = "Rosto novo na noite. Toda cidade tem duas faces; eu atendo a que aparece depois do ultimo sino. Bem-vindo a essa.",
                MilestonePostAct1Line = "Depois daquilo, alguem anda comprando TODA pedra negra que aparece. Pagando triplo, por tras de intermediario. Eu odeio nao saber quem.",
                MilestonePostAct3Line = "Desde o que voce fez, ate meus contatos noturnos falam seu nome. Raro. Para voce, a primeira informacao sai de graca. So a primeira."
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
                Goodbyes = new[] { "Durma, se conseguir.", "A noite e longa. Eu cuido dela.", "Va. Os sonhos nao gostam de esperar." },
                SpringLine = "Primavera. A noite cheira a terra molhada e a coisa que vai brotar. Ate o escuro acorda diferente nesta estacao.",
                SummerLine = "Verao. As noites sao curtas e mornas, mas a cidade ainda sussurra. Escute o que o calor nao deixa o dia ouvir.",
                AutumnLine = "Outono. As folhas caem como dias contados. A noite fica mais longa, e eu, mais ouvinte. A cidade conta mais agora.",
                WinterLine = "Inverno. O frio silencia ate os sonhos. Ando devagar pela neve para nao acordar o que dorme sob a cidade. Voce ouve?",
                RainLine = "A chuva conversa, sabia? Cada cidade tem uma voz na chuva. A desta fala baixo esta noite. Pare. Escute comigo um instante.",
                FestivalLine = "Festa? O dia comemora. Eu prefiro a hora depois, quando os fogos calam e a cidade tira a mascara. Esse e meu horario.",
                FriendStrangerLine = "...Voce tambem nao consegue dormir? A noite esta falando hoje. Ainda nao sei seu nome, mas a noite ja o anotou.",
                FriendWarmLine = "Ah. Voce de novo, a esta hora. Comeco a reconhecer seu passo na rua escura. Poucos andam devagar o bastante para eu notar.",
                FriendCloseLine = "Voce. Sente-se comigo no jardim da estatua. A esta altura, confio a poucos o que a noite me conta. Voce e um deles.",
                MilestoneArrivalLine = "Um rosto novo e desperto. Raro. A cidade conta coisas a quem chega ouvindo. Ande devagar; voce vai entender por que veio.",
                MilestonePostAct1Line = "Depois daquilo, a caverna ficou inquieta. Eu a sinto daqui de cima, sabia? Ela tem mares. Naquela noite, a mare subiu.",
                MilestonePostAct3Line = "O que voce fez acalmou algo que eu ouvia ha anos sob a cidade. Pela primeira vez, a noite dorme tranquila. Obrigado por isso.",
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
                Goodbyes = new[] { "Desca devagar, suba inteiro.", "Te vejo no quadro de retorno.", "Boa sorte. E conta as tochas DUAS vezes." },
                SpringLine = "Primavera. A boca da caverna fica mais movimentada com o degelo. Mais novato descendo. Verifique as botas antes de ir.",
                SummerLine = "Verao. O calor la fora nao chega la embaixo; a caverna ignora estacao. Leve agasalho mesmo no verao. Confie em mim.",
                AutumnLine = "Outono. Os dias encurtam e a descida fica mais arriscada no escuro. Marque seu nome no quadro antes de entrar.",
                WinterLine = "Inverno. A estrada ate a caverna congela e escorrega. Pressa no gelo escolhe o tumulo. Desca com o dobro de cuidado.",
                RainLine = "Chuva torna a trilha ate a boca da caverna um lamacal. Pise firme. Inimigo que recua na chuva nao desistiu; te leva pra algum lugar.",
                FestivalLine = "Festival? Eu patrulho a estrada igual. Aventureiro animado com festa desce afoito. Justamente nesses dias eu trago gente de volta.",
                FriendStrangerLine = "Voltando da caverna ou indo? A resposta muda meu conselho. Cara nova. Verifique as botas antes de descer. Sempre.",
                FriendWarmLine = "Ah, voce de novo. Ja sei que voce escuta meu conselho, e isso te mantem vivo. Marquei seu nome no quadro, como sempre.",
                FriendCloseLine = "Voce. Bom te ver inteiro. Para quem confio, deixo o melhor kit separado e desco eu mesmo se voce nao voltar no prazo. Ja trouxe sete.",
                MilestoneArrivalLine = "Cara nova na estrada da caverna. Alguem precisa patrulhar entre a cidade e o abismo; sou eu. Meu trabalho e te ver subir inteiro.",
                MilestonePostAct1Line = "Depois daquilo, os bichos do nivel dez estao descendo pro doze. Algo la em cima esta empurrando eles pra baixo. O que empurra monstro?",
                MilestonePostAct3Line = "Desde o que voce fez, a estrada da caverna anda mais calma. Trinta e cinco niveis eu desci e voltei. Voce foi mais fundo. Respeito."
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
                Goodbyes = new[] { "Va pelo caminho marcado!", "Leve agua. E respeito.", "Que o verde te acompanhe." },
                SpringLine = "Primavera! O bosque transborda. Colhi tres cestas antes do sol alto. Folha nova cura melhor; leve enquanto ha.",
                SummerLine = "Verao seca as ervas no pe. Colho de madrugada, antes do calor roubar o oleo das folhas. Antidoto fresco aqui, viajante.",
                AutumnLine = "Outono e raiz e casca, a forca que a planta guarda para o frio. O pacote anti-veneno fica mais potente nesta estacao.",
                WinterLine = "Inverno. A floresta dorme, mas eu nao. Mistura minha aquece o cha da Gruta. Roupa molhada na caverna e febre; leve cha.",
                RainLine = "Chuva alimenta o verde e a mim. Dia de chuva e dia de secar erva na varanda e moer raiz. O bosque agradece cada gota.",
                FestivalLine = "Festival! Levo guirlanda e cha de festa para a praca. A floresta e a cidade tem um acordo; em dia de festa, ele floresce.",
                FriendStrangerLine = "Cuidado onde pisa. Essa florzinha leva dois anos pra crescer. Cara nova. Bem-vindo a borda do verde; a cidade termina aqui.",
                FriendWarmLine = "Ah, voce de novo. Ja sei as ervas que te servem. Separei um antidoto fresco pensando na sua proxima descida. Leve.",
                FriendCloseLine = "Minha companhia querida! Para voce, a erva mais rara da colheita e o cha que so ofereco a quem o verde ja aprendeu a amar.",
                MilestoneArrivalLine = "Rosto novo na borda da floresta. A cidade e a mata fazem um acordo silencioso. Respeite o verde, e ele te recebe. Bem-vindo.",
                MilestonePostAct1Line = "Depois daquilo, um cogumelo novo cresceu na boca da caverna. Nao esta em nenhum livro meu. Plantei um; ele virou na direcao da caverna.",
                MilestonePostAct3Line = "Desde o que voce fez, os passaros voltaram a fazer ninho na arvore alta do portao leste. Passaro sabe das coisas. Eles confiam de novo."
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
                Goodbyes = new[] { "Saia antes que algo borbulhe!", "Volte com frascos vazios e curiosidade cheia!", "Se sentir gosto de metal, volta aqui CORRENDO." },
                SpringLine = "Primavera! Reagentes vegetais no auge. A Savra me traz erva fresca e minhas pocoes saem mais limpas. Sente o cheiro? E normal!",
                SummerLine = "Verao acelera toda reacao! Minhas pocoes fermentam rapido demais. NAO respire fundo perto do alambique hoje. Serio. NAO.",
                AutumnLine = "Outono e estacao de destilar raiz e essencia concentrada. Elixir mais forte sai agora. Traga frasco vazio; ganha desconto.",
                WinterLine = "Inverno desacelera as misturas, otimo para experimento delicado. Pocao de vida quente para o frio! Testada! Em mim, claro.",
                RainLine = "Chuva muda a pressao e minhas reacoes ficam... imprevisiveis. Hoje a fumaca saiu roxa. Roxa depende. Pergunte antes de comprar!",
                FestivalLine = "Festival! Eu queria soltar fumaca colorida festiva, mas o conselho... bem, voce sabe. Foi UM muro. Compre um elixir e comemore!",
                FriendStrangerLine = "NAO respire fundo ainda! ...Pronto, agora pode. Bem-vindo! Cara nova, otimo, um voluntar... digo, cliente! O cheiro e normal.",
                FriendWarmLine = "Ah, voce! Ja sei que voce nao foge da minha fumaca. Coragem rara! Da uma olhada nos reagentes novos, sem encostar naquele ali.",
                FriendCloseLine = "Meu cliente de confianca! Pra voce eu mostro o elixir experimental. Se der gosto de metal na lingua, volta CORRENDO, combinado?",
                MilestoneArrivalLine = "Cara nova na cidade! Esta cidade e perfeita para alquimia: erva da Savra, minerio da caverna e vizinhos compreensivos. Ou surdos.",
                MilestonePostAct1Line = "Depois daquilo, destilei agua de uma poca do nivel oito. O residuo se MOVE. Guardei no armario triplo. Ele anda arranhado por dentro.",
                MilestonePostAct3Line = "Sabe o que voce fez? Me deu vontade de repetir o teste da pedra negra com a agua da Fonte. So que dessa vez... longe das paredes. Talvez."
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
                Goodbyes = new[] { "Vai la! E fecha o portao, pelas cabras!", "Leva ovo fresco, ta na cesta!", "Volta pra ordenha de domingo!" },
                SpringLine = "Primavera! Epoca de filhote! Tem pintinho, cabrito e bezerro novo no quintal. Quer levar um? So se tiver espaco e paciencia!",
                SummerLine = "Verao da muito leite e muito calor. Galinha feliz bota mais com sombra boa. Ovo fresco na cesta toda manha, viu?",
                AutumnLine = "Outono e mes de engordar o rebanho antes do frio. Vendo racao reforcada. Animal bem tratado atravessa o inverno inteiro.",
                WinterLine = "Inverno. Recolho os bichos cedo e reforco o curral. Cabra que olha pra cerca ja decidiu pular; no frio, reforce antes!",
                RainLine = "Chuva! As galinhas se abrigam e ficam mais quietas, gracas a Deus. Cuidado com a lama no curral; o portao escorrega.",
                FestivalLine = "Festival! Levo ovo, leite e queijo fresco para a praca. Em dia de festa, ate as cabras parecem mais comportadas. Quase.",
                FriendStrangerLine = "Ei! Voce assustou as galinhas. Brincadeira, elas se assustam sozinhas. Cara nova! Bem-vindo ao quintal mais barulhento da cidade!",
                FriendWarmLine = "Voce de novo! Os bichos ja te reconhecem, sabia? Eles confiam em pouca gente. Pega um ovo fresco da cesta, e por minha conta.",
                FriendCloseLine = "Ah, meu amigo! Ate a cabra mais ranzinza gosta de voce, e isso e raro. Separei o melhor queijo da semana so pra voce. Leva!",
                MilestoneArrivalLine = "Cara nova na cidade! A cidade toma cafe da manha no meu quintal: leite da praca, ovo da estalagem. Bem-vindo ao barulho!",
                MilestonePostAct1Line = "Depois daquilo, as cabras se recusam a pastar perto da entrada da caverna. CABRAS. Que comem ate avental. Animal sente as coisas.",
                MilestonePostAct3Line = "Sabe o que mudou desde o que voce fez? As cabras voltaram a pastar tranquilas perto da caverna. Bicho so relaxa quando o perigo passa."
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
                Goodbyes = new[] { "Que sua estrada rime.", "Volte ao entardecer; a luz ajuda a musica.", "Vou colocar voce numa cancao. A parte boa, prometo." },
                SpringLine = "Primavera afina o mundo. Os passaros voltam e roubam minhas melhores notas. Componho sobre flores que nem desabrocharam ainda.",
                SummerLine = "Verao. As noites mornas pedem cancao longa na praca. Fico ate tarde; o calor estica a musica como estica o dia.",
                AutumnLine = "Outono e a estacao mais musical: tudo cai em tom menor, mas com esperanca no refrao. Como o nome desta cidade, alias.",
                WinterLine = "Inverno silencia a praca, entao levo a musica para a taverna da Gruta. Ouca mais o silencio entre as notas do que as notas.",
                RainLine = "A chuva tem ritmo proprio, sabia? Sento sob o beiral e deixo ela marcar o compasso. As melhores cancoes nascem molhadas.",
                FestivalLine = "Festival! Hoje eu canto para a cidade inteira, nao so para a estatua. Traga uma historia verdadeira e eu transformo em verso.",
                FriendStrangerLine = "Shh... estou compondo. Pronto, perdi. Era linda. Culpa sua. Brincadeira: era mediana. Voce chega como um acorde inesperado.",
                FriendWarmLine = "Ah, voce de novo. Voce vira refrao na minha cabeca, sabia? Sente-se; o jardim da estatua tem a melhor acustica e o melhor publico.",
                FriendCloseLine = "Meu ouvinte favorito! Compus uma cancao pensando em voce, e juro que dei a voce a parte boa. Fique para o refrao, sempre fique.",
                MilestoneArrivalLine = "Um rosto novo na cidade. Cada cidade tem uma cancao escondida; a desta e em tom menor com esperanca. Voce chega no comeco dela.",
                MilestonePostAct1Line = "Depois daquilo, quando toco certa sequencia perto da estatua, o vento muda. Tres notas. Sempre as mesmas. Nao toco mais a quarta.",
                MilestonePostAct3Line = "A melodia que sonhei a vida toda? Desde o que voce fez, finalmente ouvi o final dela, vindo do chao. Voce a completou. Obrigada."
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
