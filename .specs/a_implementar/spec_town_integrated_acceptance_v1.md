# SPEC — Town Integrated Acceptance v1

**Spec ID:** `town_integrated_acceptance_v1`  
**Status:** A_IMPLEMENTAR — gate final especificado por pedido humano em 2026-09-10  
**Wave:** TOWN.G1 — aceite integrado  
**Domain:** TownScene / validation / performance / visual review  
**Priority:** P0  
**Ordem de execucao:** TOWN.G1; último gate, sem correções de produção dentro desta spec  
**Depende de:** TOWN.V1, TOWN.A1, TOWN.C1 e TOWN.N1 concluídas com evidência vigente  
**Bloqueia:** promoção da wave Town e declaração de proximidade >=80%  
**Repo lock:** validadores/capturas/evidência; cena somente regeneração canônica final  
**Must not run with:** geradores Unity concorrentes ou mudança ativa de Town/Farm

required_adrs: []
required_game_rules: [city_rules, npc_rules, event_rules]

# /speckit.specify

## Objetivo

Validar a cidade como cena jogável completa e comparar sua apresentação real à keyart. Esta spec é um
gate: não implementa arte ou gameplay ocultamente. Falha retorna evidência e owner exato à spec produtora.

## Baseline de entrada

- Town 160×112, escala de construções +20%, 24 lotes, 29 NPCs, 28 agendas e 84 anchors.
- Expansão: physics 187/187, TownAccess 8/8, schedule 21/21, 107/107 rotas e House_Prison 4/4.
- Fidelidade vigente antes desta wave: aproximadamente 68/100; >=80 ainda não comprovado.
- Sem perfil de performance específico da Town; 754 árvores na cena materializada conhecida.

## Escopo permitido

Validadores/capturadores Town existentes, testes, relatórios e imagens versionadas em
`docs/validation/town_keyart_final_v1/`. Correções pertencem às specs produtoras e exigem nova execução
delas; o gate pode corrigir somente defeito reproduzido do próprio harness, sem afrouxar asserts.

# /speckit.plan

1. Congelar hash da referência, cena, fontes/assets relevantes e saves antes do teste.
2. Executar regeneração canônica uma vez e todos os gates estáticos/EditMode pertinentes.
3. Em PlayMode, percorrer por input real os sete distritos, entrar/sair de três interiores de famílias
   distintas, usar portas/estações e fazer Farm→Town→serviço/NPC→Farm.
4. Rodar agenda acelerada com NPCs ativos, incluindo entrada por porta e conflito de circulação.
5. Observar dia/noite, clima, iluminação, água/roda, curral e feedback audiovisual de porta.
6. Medir três execuções de60s da mesma rota após10s de warmup; registrar hardware, resolução, build/
   editor e comparar CPU main-thread mediana/p95, allocations, batches e memória à baseline pré-wave.
7. Capturar mesma câmera/crop da baseline e detalhes críticos. Dois revisores pontuam sem compartilhar
   score inicial; consolidar pela regra v2.
8. Comparar hashes de cena/fontes/saves e registrar todo NOT RUN como pendência, nunca como PASS.

## Matriz de aceite binário

- [ ] G-1 Visual: cada um dos dois revisores atribui >=80/100; cada categoria >=60%; composição e
  arquitetura >=75%; zero blocker. Divergência usa o menor score, nunca média escolhida após o resultado.
- [ ] G-2 Conteúdo: 24/24 lotes, 29/29 NPCs finais, 28 agendas, 84 anchors, IDs, portais, lojas,
  interações e estações preservados.
- [ ] G-3 Física: `eligibleIds == censusIds`, zero desconhecido/duplicado/não classificado, zero sólido
  atravessável, zero blocker invisível e todas as rotas/acessos passam com colliders materializados.
- [ ] G-4 Portas/interiores: três famílias distintas completam fechar→abrir→atravessar→revelar→sair→
  recobrir; uma por input e uma por NPC; nenhuma porta pintada duplicada.
- [ ] G-5 Navegação: agenda acelerada completa sem teleporte onscreen, interpenetração acima da tolerância
  ou bloqueio >5s após liberação; snap load/offscreen é contabilizado separadamente.
- [ ] G-6 Loop: Farm→Town→serviço/NPC→Farm preserva spawn, NPC state, cena e saves. Em horário aberto o
  serviço é acessível; fechado bloqueia com aviso; Yael/Maelor demonstram disponibilidade invertida.
- [ ] G-7 Ambiente: água/roda/luzes/clima/curral observados no runtime, sorting estável e porta com feedback
  físico distinto de toast genérico.
- [ ] G-8 Performance: nas três execuções, mediana e p95 de CPU main-thread não regridem >10%, batches
  não sobem >15%, memória não sobe >10% e não há nova alocação recorrente por frame após warmup. Se a
  baseline pré-wave estiver ausente, resultado obrigatório é NOT RUN e requer decisão humana.
- [ ] G-9 Testes: suites Town/door/NPC e validadores terminam 100% PASS; falhas globais preexistentes são
  separadas por baseline e não ocultadas.
- [ ] G-10 Preservação: hashes antes/depois confirmam que teste não gravou saves nem alterou fontes/cena
  fora da regeneração autorizada.

## Roteiro humano mínimo

1. Entrar pelo portão sul, alcançar praça e visitar os sete distritos.
2. Entrar em templo/edifício cívico, residência e ofício; observar porta e interior completos.
3. Colidir deliberadamente com água, árvore, muralha, banca, banco, cerca, caixa e móvel.
4. Esperar um NPC usar uma porta e observar dois NPCs cruzando uma passagem estreita.
5. Usar loja/estação, atravessar Town→Farm→Town e validar spawn/estado.
6. Observar uma mudança de luz/clima e ouvir abertura/fechamento de porta.

# /speckit.tasks

- [ ] Auditar evidências dependentes e rejeitar qualquer gate desatualizado.
- [ ] Rodar geração, static checks, EditMode e PlayMode integrado.
- [ ] Executar roteiro humano e captura de performance reproduzível.
- [ ] Produzir overview/detalhes comparáveis e duas revisões independentes.
- [ ] Publicar score, matriz PASS/FAIL/NOT RUN, hashes e riscos residuais.
- [ ] Promover somente se G-1..G-10 passarem; caso contrário devolver cada falha ao owner correto.
