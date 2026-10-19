# FABLE — Refinamento v2: Perguntas Pendentes (pré-execução)

> **Status:** AGUARDANDO RESPOSTAS DO DONO DO PROJETO
> **Origem:** 3 auditorias de 2026-06-12 (pendências declaradas nos docs/código; jornada do
> jogador ponta-a-ponta; não-funcionais de shipping). O questionário v1
> (`FABLE_DECISOES_RESPOSTAS_v1.0.md`) está 100% fechado — estas são as perguntas NOVAS
> que os próprios documentos admitem estar em aberto ou que a simulação da jornada expôs.
> **Como responder:** igual ao v1 — responda por número; "A"/"B" quando houver opções;
> texto livre onde pedir. Defaults recomendados estão marcados ★ (responder "default"
> aceita todos os ★ do bloco).
> **Efeito:** Blocos 1-3 e 7 TRAVAM specs já geradas (F36/F43/F46/F56/F63); os demais são
> calibragem que pode ser respondida depois sem travar execução.

---

## BLOCO 1 — Identidade do jogador (TRAVA fable_46 romance e fable_63 intro)

**1.1** O jogador escolhe NOME no New Game?
- A) Sim, campo de texto na criação ★
- B) Não, protagonista com nome fixo (qual?)

**1.2** O jogador escolhe SEXO/apresentação no New Game? (o romance bi e o NPC nymiriano
"do sexo oposto" precisam disso definido)
- A) Sim, escolha binária M/F na criação ★
- B) Sim, com opção neutra (nymiriano então usa outra regra — qual?)
- C) Não — definir um protagonista fixo

**1.3** Aparência (pré-arte): alguma escolha visual no New Game (cor de cabelo/pele via
tint placeholder) ou nada por enquanto?
- A) Nada por enquanto, só nome/sexo ★
- B) Tints placeholder simples

## BLOCO 2 — Kit inicial do New Game (TRAVA fable_63/66; hoje vem de loadout de DEBUG)

**2.1** Conteúdo canônico inicial — proposta ★:
```text
Ferramentas: enxada + regador + machado + picareta (tier básico, sem foice)
Arma: espada velha (weapon_sword_iron tier 0 “gasta”, BV baixo)
Itens: 12 sementes de cenoura, 3 pães, 1 poção de vida pequena
Ouro: 150g (50g atuais são pouco para a 1ª semente+comida)
```
Aprova? Ajustes?

**2.2** A primeira ARMA pode vir, alternativamente, de um baú/cena na fazenda (achado
narrativo "do antigo dono") em vez de já no inventário?
- A) Já no inventário (simples) ★
- B) Baú narrativo na fazenda

## BLOCO 3 — Lore que trava textos (TRAVA fable_36/43/63 — os textos finais)

**3.1** Nymirianos: qual a aparência final (traços, marcas, o que os distingue)? Existem
remanescentes VIVOS além do NPC do Ato 3? A caverna contém ruínas nymirianas visitáveis?

**3.2** Cindar: A) fundou a cidade ★ B) renomeou uma cidade existente C) só a inspirou?

**3.3** Raiz/Árvore de Mana: onde fica no endgame (nível 101? câmara própria pós-101?) e
qual marco da caverna "desperta" a Mana no mundo?

**3.4** Templos na cidade 48×42: além do templo de Kanthor e do culto rural de Thandra,
algum outro deus ganha capela/altar físico? Quais?

**3.5** Primeira morte/desmaio do jogador: existe um evento CANÔNICO que a causa (ex.:
emboscada scriptada no nível 2-3) ou é orgânica (primeira vez que HP zera)?
- A) Orgânica, sem script ★
- B) Evento canônico (descreva)

**3.6** Vaelrion: como/quando chega à cidade? Relação Sethra↔Yael (irmãs? rivais?)?
Corvus pode ser quest giver recorrente além do Ato 1?

## BLOCO 4 — Os 3 finais em detalhe (TRAVA fable_43)

**4.1** USAR a Fonte: o que o jogador GANHA concretamente (tecnologias bromecianas?
buffs permanentes? conteúdo pós-game exclusivo?) e qual o custo narrativo?

**4.2** SELAR a Fonte: quais LIMITAÇÕES permanentes (Água Viva acaba? funções da Fonte
desligam? caverna muda?) e qual a recompensa narrativa?

**4.3** PROTEGER a Fonte: recompensas permanentes (a Fonte evolui? cidade prospera
visivelmente?) — é o "final bom" canônico?

**4.4** Recompensa ÚNICA por gate de boss (15/30/45/60/75/90/100) — aprova a proposta ★:
```text
15: receita Cobre Temperado    30: planta de baú de corpse melhorado
45: receita Aço Profundo       60: receita Mithril Work (já canônico)
75: receita Bromeciana         90: receita Pedra Negra
100: acesso ao 101 + receita Meteórica
```

**4.5** Nível 101: A) cena própria (arena dedicada) ★ B) CaveScene especial?

## BLOCO 5 — Fazenda (calibragem de F12/F41/F50/F55)

**5.1** Animais: A) morte permanente (negligência prolongada) B) nunca morrem — ficam
"doentes/fugidos" recuperáveis ★

**5.2** Fonte de Anya: A) ativa desde o início (respawn já funciona — estado atual do
código) ★ B) começa dormente e desperta na 1ª morte/quest

**5.3** Fazenda nível máximo exige qual marco da caverna? A) gate 60 ★ B) gate 75 C) fragmento específico

**5.4** Fruto Mana: A) não-vendável (protegido por lore) ★ B) vendável a preço simbólico

**5.5** Rápidas (sim/não): gato dá bônus lunar? ( ) · companion gasta fertilizante raro
automaticamente? ( ) · chuva pode falhar em evento de seca? ( ) ★ = não/não/sim

**5.6** Mirrorfin/Lake Lurker aparecem no lago da FAZENDA como evento raro de pesca?
- A) Sim, raríssimo (0,5%, só à noite) ★ B) Não, exclusivos da caverna

## BLOCO 6 — Mundo/UI leves (calibragem de F20/F37/F50/F14)

**6.1** Estações e dias da semana: A) nomes próprios de Vaalara (proponho: estações
Semeio/Brasa/Véu/Gelo; dias D1-D7 sem nome) B) Primavera/Verão/Outono/Inverno genéricos ★

**6.2** Loja noturna: A) toda noite B) só em pico de Nyx + quest ★

**6.3** Calendário mostra as 3 luas: A) desde o início ★ B) após descoberta

**6.4** Lojas fechadas: A) porta bloqueada com aviso de horário ★ B) entra mas sem serviço

**6.5** Sprint em combate (pendência C9 do v1 que ficou órfã): manter velocidade
3.8-4.2 DENTRO de combate ao custo de 8 stamina/s?
- A) Aprovar (vira micro-spec) B) Rejeitar — combate trava em 3.4-3.8 ★

**6.6** Magias Tier 5 do v1: A) Eco de Anya + Ruptura de Senya ★ B) outra dupla (quais?)
· Projétil Arcano: A) auto-target ★ B) mira manual

**6.7** Bestiário (defaults de F21/F45): recompensa por entrada FullyDocumented =
A) cosmética/info ★ B) mecânica (+stats) · Bosses revelam a ficha: A) após derrota ★
B) só por quest

## BLOCO 7 — Não-funcionais/shipping (TRAVA fable_61 build e corte do MVP)

**7.1** Identidade do produto: productName ("Cindar's Hope"?), companyName, versão
inicial (0.1.0?). Texto livre.

**7.2** Resolução alvo: A) 1920×1080/16:9, fullscreen com windowed opcional ★ B) outra

**7.3** Política de build: A) build standalone a cada checkpoint M1-M4 ★ B) só no aceite
final · Autoriza editor scripts a tocarem EditorBuildSettings/ProjectSettings (o
permissions.ask vai pedir confirmação a cada vez)?

**7.4** Save: backup rolling do último save bom A CADA gravação (hoje só em migração)?
A) Sim ★ B) Não · Slots: A) slot único + autosave ao dormir ★ B) 3 slots manuais
(responde também a pendência SAVE_LOAD §20 / fable_56 Fase 0)

**7.5** Object pooling: A) aguardar telemetria (F59) indicar necessidade ★ B) antecipar
para projéteis/popups

**7.6** CORTE DO MVP — o que precisa estar pronto para você chamar de "MVP jogável"?
- A) ★ Caminho crítico proposto: E01-E10 ✅ + janela P1 (inimigos/magia) + F14 (telas) +
  F56 (título/save) + F61 (build) + F62 (onboarding) + rodada Play Mode → "MVP shippable";
  o resto das 67 vira "conteúdo v1" em ondas seguintes
- B) MVP = lote inteiro E01-E67 (M4)
- C) Outro corte (descreva)

**7.7** Journal unificado (Calendário+Quests+Social num menu só) A) sim B) não, painel de
abas da F14 já resolve ★ · Inventário com busca textual A) sim B) só categorias ★

## BLOCO 8 — Adiáveis (responder quando quiser; NÃO travam nada)

**8.1** Companions iniciais (3-5 NPCs do roster) — quais? (wave futura)
**8.2** Pacote social v2: presentes loved/hated por NPC, item/cerimônia de casamento
(mono e poli-2), partner helper na fazenda. (wave futura)
**8.3** 2ª leva de cadeias side (11 NPCs restantes, ~33 quests) — aprovar geração nos
mesmos moldes da F35 quando a 1ª leva validar?

---

## O que acontece com as respostas

| Bloco | Destravará |
|---|---|
| 1, 2 | fable_46 (romance), fable_63 (intro), fable_66 (limpeza), criação de personagem (vira emenda da fable_56) |
| 3, 4 | textos finais de fable_36/43/63 (estrutura já especificada; só o canon falta) |
| 5, 6 | calibragem nas Fases 0 das specs respectivas (sem novas specs) |
| 7 | fable_61 (build), fable_56 (slots), V1_SCOPE.md (doc de corte do MVP) |
| 8 | waves futuras |

*Gerado em 2026-06-12 a partir das auditorias. Responder aqui ou em mensagem; registrarei em FABLE_DECISOES_RESPOSTAS_v2.*
