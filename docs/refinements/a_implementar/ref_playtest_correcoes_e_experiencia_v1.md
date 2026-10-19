# Refinamento — correções e experiência do playtest v1

**Status:** PROPOSTO — refinamento solicitado por Rafa em 2026-09-27; sem aprovação automática para implementação.  
**Origem:** playtests exploratórios de Farm, Town e Cave, incluindo continuação de menus.  
**Escopo:** integridade, recuperação, acesso a funcionalidades, apresentação, circulação e combate.  
**Não autoriza:** nova feature sem spec aprovada, alteração de balanceamento, IDs/schema, regeneração de Cave ou edição manual de YAML.

## Objetivo e evidência

O jogador deve conseguir cultivar na Farm, negociar na Town e enfrentar riscos na Cave com informação legível, ações previsíveis e progresso recuperável. O polimento não pode esconder funcionalidades incompletas nem reduzir as consequências de derrota sem decisão de design.

Fontes de evidência:
- [E1 — playtest Farm/Town/Cave](../../validation/EXPLORATORY_PLAYTEST_2026_09_27.md).
- [E2 — menus, crafting, magia, cultivo e save](../../validation/EXPLORATORY_MENUS_2026_09_27.md).
- [E3 — wiring e gates pendentes](../../validation/FARM_WIRING_RECONCILIATION.md).
- [E4 — estado de execução](../../project/CURRENT_STATE.md).
- [E5 — auditoria estática complementar de Cave, mapas e movimento](../../validation/WORLD_PHYSICS_MOVEMENT_AUDIT_2026_09_27.md).
- Reprodução local: `TestResults/exploratory-playtest/crafting-unloadable-slot.json`; log `Logs/exploratory-playtest.log` (linhas nos relatórios E1/E2).

Fatos observados não equivalem a cobertura integral. Aproximações controladas não provam circulação; pausas entre inputs não medem dificuldade; telas abertas não provam progressão funcional. As decisões D01–D25 abaixo são recomendações de escopo, ainda propostas. Prioridade P1 bloqueia confiança no fluxo principal; P2 prejudica compreensão/uso; P3 organiza cobertura e acabamento.

## Integridade e recuperação

### D01 — P1 — roundtrip de crafting e save
**Fato:** F5 grava e F9 rejeita o próprio arquivo após três crafts. A estação ociosa contém Job com saída vazia/quantidade zero; o validador rejeita esse objeto (E2).
**Proposta:** distinguir ausência de trabalho de trabalho inválido no ciclo captura→serialização→leitura→validação→restore. Reusar `CraftingRuntime`, `CraftingSectionProvider` e `SaveManager.Migration`; investigar o comportamento real do serializador antes de escolher representação. Não aceitar indiscriminadamente outputs vazios nem remover validação de itens.
**Aceite candidato AC01:** salvar/carregar estação ausente, ociosa, em andamento e concluída usando o serializador real; reproduzir o arquivo preservado; restaurar inventário, estação, posição, horário e equipamento sem duplicar output. Save genuinamente inválido é rejeitado antes de mutar a sessão e apresenta mensagem útil. Compatibilidade de arquivos já produzidos exige estratégia explícita, sem apagar saves.

### D02 — P1 — lágrima disponível na decisão de morte
**Fato:** duas lágrimas antes da morte; tela mostra zero após transferência do inventário (E1). `CaveDeathResolver` e `UI/Death/DeathScreenCanvasController` participam do fluxo.
**Proposta:** uma única operação de domínio resolve elegibilidade, consumo e destino dos itens; a tela projeta o resultado. Não corrigir apenas o contador visual. Preservar penalidades vigentes e evitar inventário/corpo contendo as mesmas unidades.
**AC02:** cenários com 0/1/2 lágrimas, clique repetido, duas mortes consecutivas e save/load em estado suportado. Revive consome exatamente uma unidade elegível, restaura controle/vida conforme contrato e mantém conservação dos demais itens. A política de propriedade da lágrima durante a decisão deve ser reconciliada com a spec de morte antes de implementar.

### D03 — P1 — retorno à Fonte da Anya e corpo
**Fato:** UI promete fonte; destino `spawn_farm_default` não é encontrado e jogador aparece na plantação (E1).
**Proposta:** resolver âncora válida da fonte pelo fluxo existente `AnyaFountainRespawnFlow`; fallback seguro verificável, sem inventar coordenadas em MonoBehaviour. Fonte e tela de morte devem descrever o mesmo destino.
**AC03:** derrota→retorno termina na fonte, fora de colisões, com câmera, vida, foco e controle coerentes. Recuperar corpo uma vez; repetir interação não duplica itens. Nova derrota respeita política vigente de substituição de corpo. Ao tocar Cave, aplicar FASE9F; retorno não autoriza regeneração fora das condições canônicas.

## Acesso, foco e funcionalidade real

### D04 — P1 — pausa e entrada nos menus de sistema
**Fato:** Escape no mundo não abriu pausa. `GameplayInputRouter` publica `PauseOpenedEvent`; busca atual encontra declaração/publicação sem subscriber. Title/settings não foram alcançados no boot Farm (E2).
**Proposta:** rastrear o boot real e integrar os owners existentes de UI/modal. Fechar modal superior antes de abrir pausa; retomar deve devolver foco. Separar reparo da pausa da eventual criação de título/configurações inexistentes.
**AC04:** Escape mundo→pausa→retomar; Escape em inventário/diálogo fecha somente o topo; sem movimento, ataque ou avanço do relógio durante pausa. Repetir após transição e load sem listeners duplicados. Verificar entrada normal do jogo, Novo/Continuar/configurações conforme funcionalidades existentes; opções sem suporte não aparentam funcionar.

### D05 — P2 — arbitragem de input e campos de texto
**Fato:** colagem no campo de quantidade abriu Debug Info (E1). E é usado para interação e combate; colisão funcional ainda precisa de cenário específico.
**Proposta:** contexto com foco recebe o input; edição de texto e modal bloqueiam atalhos de gameplay/debug. Uma ativação não executa duas ações. Preservar bindings atuais até verificar conflitos; não escolher remapeamento global incidentalmente.
**AC05:** digitar/colar quantidade, confirmar, cancelar e fechar sem debug/ataque involuntário; E junto a recurso e inimigo executa somente a ação anunciada. Verificar J de quests versus uso de pergaminhos no caminho real, sem afirmar conflito já reproduzido.

### D06 — P2 — política de tempo em diálogos
**Fato:** inventário/atributos/quests pausam; fonte continua relógio (E2).
**Proposta:** inventariar política canônica por tipo de UI; se fonte for modal de decisão, recomendar pausa consistente. Não aplicar pausa a todo prompt de proximidade nem mudar o tempo da pesca/crafting por consequência.
**AC06:** tabela explícita de telas com pausa/foco; abertura/fechamento/nesting restaura estado anterior, sem relógio preso nem múltiplas retomadas. Decisão sobre fonte permanece aberta até conferir contrato vigente.

### D07 — P2 — baú e funcionalidades indisponíveis
**Fato:** Abrir baú só escreve log de feature futura (E2).
**Proposta:** apresentar indisponibilidade de forma legível ou retirar affordance acionável, conforme direção aprovada. Criar armazenamento completo é expansão separada, não correção de prompt.
**AC07:** jogador não recebe promessa de armazenamento funcional nem perde itens ao interagir. Caso armazenamento entre no escopo, exigir spec própria de capacidade, transferências, persistência e acesso; não presumir essas regras neste refinement.

### D08 — P2 — pergaminhos e magia acessível
**Fato:** varinha dispara e gasta MP; acerto não validado. Pergaminho mostra Usar indisponível e equipar não comprova efeito; projétil usa fallback procedural (E2).
**Proposta:** conferir contrato de cada tipo de pergaminho e sua ação válida, requisito e consumo. Ocultar/desabilitar ações incompatíveis com motivo; manter varinha no pipeline existente. Avaliar prefab/sprite no runtime, mesmo com validator de referência PASS.
**AC08:** uso válido produz efeito previsto e consome uma vez; falha não consome; magia acerta alvo, desconta recurso/recarga e respeita obstáculos conforme contrato. Sem disparo por trás de modal. Arte de projétil aprovada tem direção, contraste e escala legíveis; fallback é diagnóstico, não prova de acabamento.

## Layout, comunicação e feedback

### D09 — P2 — loja consistente e legível
**Fato:** estoque da lista atualiza 5→4, detalhe fica 5; mensagem de saldo insuficiente cortada (E1).
**Proposta:** lista, detalhe e confirmação derivam do mesmo estado atualizado. Mostrar nome localizado, quantidade, custo total e motivo da recusa, com quebra de linha e espaço suficiente.
**AC09:** comprar última unidade, saldo exato/insuficiente, quantidade inválida e inventário cheio; sucesso altera ouro/estoque/itens uma vez e falha não altera. Detalhe acompanha seleção/transação e mensagens ficam integralmente visíveis.

### D10 — P2 — crafting comunica resultado
**Fato:** três crafts entregaram seis madeiras, mas Station idle não sinalizou sucesso (E2).
**Proposta:** craft imediato mostra resultado e quantidade; trabalho temporizado mostra progresso e estado. Ingredientes/disponibilidade atualizam após transação. Cancelamento explica indisponibilidade sem mensagem técnica em inglês.
**AC10:** uma confirmação corresponde a um craft; feedback permite distinguir sucesso, falta de material, trabalho ativo e cancelamento. Não introduzir espera artificial em receita instantânea para tornar o resultado visível; não alterar custos/tempo de receita neste escopo.

### D11 — P2 — linguagem e hierarquia dos painéis
**Fato:** fundos transparentes, mistura de idiomas, enum/IDs internos e stats provisórios (E1/E2).
**Proposta:** base visual comum com painel de contraste suficiente, título, conteúdo e ações distintos; textos via localização existente, IDs somente no diagnóstico. Atributos, equipamento, skills, quests, fonte e cultivo explicam estados/bloqueios em linguagem de jogador. Stats exibem valores derivados reais ou ausência explícita de informação, sem número inventado.
**AC11:** revisar sobre fundos claros/escuros em 1280×720 e 1920×1080 como alvos propostos, também com texto longo. Nenhum controle essencial cortado, sobreposto ou dependente apenas de cor; scroll e foco alcançam conteúdo completo. Labels localizados não alteram IDs persistidos.

### D12 — P2 — inventário e equipamentos
**Proposta:** distinguir seleção de equipado, mão/slot por rótulo e ícone; destacar compatibilidade, quantidade e ação disponível. Filtros/categorias só se reutilizarem catálogo e mantiverem seleção estável. Confirmar operações irreversíveis conforme contrato existente, sem adicionar confirmação a toda ação.
**AC12:** equipar/trocar/remover; split/mover/mesclar/swap nos limites de stack e capacidade; drop/destruir/salvage quando suportados. UI acompanha resultado autoritativo sem perder seleção/foco ou duplicar item. Categorias e novos ícones são extensão de apresentação sujeita a spec/arte, não requisito para corrigir transações.

### D13 — P2 — HUD e prompts de ação
**Proposta:** mostrar arma/ferramenta selecionada, ação contextual, custo/recarga quando relevantes, e motivo de falha próximo da ação. Hierarquia mantém HP/risco visível sem cobrir alvo ou área de cultivo. Debug sai da apresentação normal por configuração existente; logs de diagnóstico permanecem disponíveis.
**AC13:** jogador identifica ferramenta/mão ativa e ação de E/F antes de executar; troca de equipamento atualiza HUD; mensagens não se sobrepõem indefinidamente. Revisar em cultivo, loja e combate, com barras cheias/vazias e recarga ativa. Layout exato exige mockup/revisão visual.

### D14 — P2 — diálogo e escolhas
**Proposta:** reduzir vazios excessivos, separar nome/retrato/fala/opções, permitir quebra de linha e foco previsível. Opções debug não integram diálogo normal. Estado vazio de quests orienta o jogador sem inventar objetivos.
**AC14:** falas curtas/longas e escolhas indisponíveis legíveis; mouse/teclado chegam à mesma opção; fechar não aciona gameplay. Preservar conteúdo narrativo, preços e condições de quests.

## Mundo, circulação e combate

### D15 — P1 — legibilidade da Cave
**Fato:** piso com blocos pretos e grande retângulo claro persistente; alvos confundem-se com cenário (E1).
**Proposta:** isolar composição, tiles/alpha, sorting, luz, overlays e materiais antes de trocar arte. Preservar identidade da Cave e distinguir piso navegável, obstáculo, recurso, inimigo e saída.
**AC15:** captura antes/depois da mesma área/seed/câmera, entrada e luta; sem retângulos indevidos nem chão ilegível, alvo e limites reconhecíveis. Reentrada mantém snapshots conforme FASE9F; corrigir render não altera layout/loot/inimigos. Avaliação humana visual ainda necessária.

### D16 — P2 — estufa e estados do solo
**Fato:** arar/plantar/regar funcionam, mas exterior esconde ator e canteiro em aproximação controlada; solo arado externo pouco distinguível (E1/E2).
**Proposta:** conferir entrada normal, footprint e referências de reveal existente (`RoofRevealController`), sem remover fachada globalmente. Diferenciar decorativo, arável, arado, seco, molhado, plantado e pronto; marcar alvo real da ação e orientar E/F.
**AC16:** entrar/sair andando e cultivar sem perder leitura; cobertura volta ao sair e não pisca na borda. Seed consome uma vez, rega altera estado correto; estados distinguíveis sem depender só de cor. Cobrir plantio→dias necessários→colheita→inventário/save, sem acelerar produção como mudança permanente.

### D17 — P2 — coerência de escala entre cenas
**Fato:** diferença perceptual de escala/zoom Farm↔Town (E1); medição ainda ausente.
**Proposta:** comparar câmera, resolução, PPU, pivôs e proporções de ator/porta/árvore na escala de jogo. Preservar dimensões e direção de arte aprovadas; não redimensionar Town ou Farm por impressão de uma captura.
**AC17:** pares de capturas comparáveis e medidas documentadas; personagem/portas/props coerentes com referências aprovadas, pixels sem blur. Reconciliar specs de keyart antes de qualquer mudança de câmera/escala.

### D18 — P2 — ponte, colisões e rotas de NPC
**Fato:** dash parou na borda oeste; logs Zrix/Renko têm route_blocked/graph_missing. Bloqueio ao caminhar não comprovado (E1).
**Proposta:** reproduzir caminhada nos dois sentidos e medir espaço útil versus colisores; verificar grafo, âncoras e horários dos NPCs. Separar problema de dash, passagem física e navegação; não usar teleporte recorrente ou remover colisores como solução geral.
**AC18:** atravessar ponte e acessos relevantes andando; dash respeita obstáculos sem deixar ator preso. NPC chega às âncoras exercitadas ou informa diagnóstico específico sem spam contínuo; testar troca de período/cena. Decoração não invade percurso funcional.

### D19 — P2 — leitura e intenção do combate
**Fato:** dano, impacto, recuo e morte de inimigo funcionaram; testes não medem equilíbrio (E1).
**Proposta:** preservar esses efeitos; melhorar silhueta/contraste do alvo, direção e alcance comunicados do golpe e antecipação de inimigo. Feedback diferencia acerto, bloqueio/falha e indisponibilidade de recurso. Não aumentar dano/HP, ampliar hitbox ou dar invulnerabilidade adicional neste refinamento.
**AC19:** cenários contínuos 1×1 e grupo com equipamentos fixos; início do ataque inimigo e área perigosa perceptíveis, efeito visual acompanha hitbox/tempo reais. Medir respostas com input contínuo antes de propor tuning. Alteração de timing que mude janela de reação precisa decisão de balanceamento e dados, não apenas animação.

### D20 — P2 — pesca compreensível
**Fato:** início/custo confirmados, captura não; ausência de captura não prova defeito (E2).
**Proposta:** verificar indicação de espera, mordida, janela de resposta, sucesso e falha no fluxo existente. Se informação já estiver presente, validar antes de acrescentar painel. Manter custo e janela até medir teste humano contínuo.
**AC20:** início, captura, erro de timing, cancelamento e saída da área; inventário cheio tem comportamento definido; consumo de stamina e recompensa ocorrem exatamente conforme contrato. Tamanho da janela e ritmo continuam hipóteses, sem números novos aprovados.

### D23 — P1 — faixas e feixe da Cave
**Fato:** o PNG `light_shaft.png` é totalmente opaco, com fundo claro quadriculado, e o runtime o amplia para cerca de 29,4×49 unidades; é causa provável do retângulo claro, ainda sem confronto do renderer no frame observado (E5). Tints de borda, parede e vinheta são candidatos separados para os blocos/faixas escuros.
**Proposta:** instrumentar o mesmo nível/seed e separar contribuição de sprite, Tilemap e vinheta. Corrigir apenas o componente confirmado, preservando composição e stable run.
**AC23 candidato:** nenhuma forma retangular indevida em captura comparável; feixe recortado se for mantido; piso transitável, parede e inimigo distinguíveis. Hash, snapshots e conteúdo da Cave idênticos antes/depois da correção de render. Aceite visual humano pendente.

### D24 — P2 — suporte físico de props e corredores
**Fato:** postes e três props da Farm; poço, cerca e pontes visuais da Town não têm collider próprio (E5). Isso não prova base atravessável, pois outro collider pode cobri-la. O censo histórico da Town isentou essas famílias e foi feito sobre outra versão da cena.
**Proposta:** medir suporte no ponto dos pés e trajetória do Player na cena atual. Classificar cada elemento como cenário de fundo, sólido ou passagem, alinhando collider e arte apenas quando houver contrato físico.
**AC24 candidato:** postes/poço/caixas/feno/cercas bloqueiam somente onde a arte/contrato indica solidez; pontes e acessos conservam corredor caminhável; nenhum collider invisível bloqueia rota obrigatória. Probe headless com corpo real e travessia jogada quando autorizada.

### D25 — P2 — movimento intencional de NPCs e monstros
**Fato:** fallback direto de agenda sem grafo, wander sem teste de alcance e movimentos especiais de inimigos sem avoidance podem criar travamento/atravessamento; raios pontuais ignoram largura do corpo (E5). São riscos de código, não defeitos vistos em jogo nesta rodada.
**Proposta:** testar rota faltante, obstáculo temporário, corredor estreito e ataque especial contra parede com corpos reais. NPC deve aguardar/replanejar sem teleporte cotidiano ou spam; inimigo deve preservar identidade do movimento sem atravessar sólido.
**AC25 candidato:** progresso e recuperação dentro dos limites da spec de navegação aplicável; logs finitos e diagnósticos; zero interpenetração persistente em obstáculos sólidos. Qualquer mudança de timing, alcance ou dano do monstro exige avaliação de combate própria.

## Organização, cobertura e gates

### D21 — P3 — roteiro reproduzível e funcionalidades ainda abertas
**Proposta:** duas rotas: (A) boot→menus→equipamento→loja→craft→save→load; (B) cultivo→pesca→Cave→combate→derrota→fonte→corpo. Preparação controlada serve diagnóstico; aceite de acesso usa deslocamento normal.
**AC21:** checklist registra PASS/FAIL/PARTIAL/NOT RUN, build/commit, cena, estado inicial, input, esperado/observado e evidência por cenário. Complementar compra de skill com pontos, gasto de atributos, quest completa, outras estações, consumíveis com recurso faltante, bosses, áudio e gamepad. Não chamar tela vazia de validação do sistema inteiro.

### D22 — P2 — wiring e validadores restantes
**Fato:** E3 registra duas picaretas sem WeaponId e checks legados de transição/Bounds; Fireball validator PASS coexistiu com fallback visual em runtime.
**Proposta:** separar defeito de asset de validator incompatível; reconciliar critérios com componentes/contratos atuais sem afrouxar gates para obter PASS. Preservar resultado anterior 56/56 no seu escopo.
**AC22:** referências de picaretas resolvem IDs válidos; checks exercitam gates/anchors usados de fato; falhas restantes têm diagnóstico reproduzível. PASS estático não substitui disparo/impacto ou circulação em jogo.

## Alternativas e limites

Manter comportamento atual é aceitável somente para funcionalidades explicitamente indisponíveis e cobertura ainda não concluída; não resolve save, revive, respawn ou pausa. Recomenda-se reparar owners e projeções existentes antes de redesenhar sistemas. Reescrever todos os menus, gerar arte nova para toda cena, adicionar armazenamento ou rebalancear combate ampliaria risco sem evidência suficiente. Novos componentes precisam necessidade demonstrada, wiring no composition root e comunicação de gameplay pelo EventBus.

## Fatias candidatas e dependências

| Fatia | Decisões | Dependência e encaminhamento |
|---|---|---|
| A — persistência | D01 | Primeiro bloqueador; análise de contrato/serialização e revisão independente antes de execução de risco. |
| B — derrota e recuperação | D02–D03 | Política canônica de morte; validar save após A; preservar stable run. |
| C — acesso/foco | D04–D07 | Auditar boot/UI atual; correções locais podem seguir bugfix, UI inexistente exige spec. |
| D — transações e feedback | D08–D10, D12 | Reusar domínio; A para persistência, C para input; separar operação de apresentação. |
| E — apresentação comum | D11, D13–D14 | Mockup sobre cenas reais e regras de UI vigentes; consumir estados corrigidos em C/D. |
| F — leitura e circulação | D15–D18, D23–D24 | Specs de keyart/colisão e FASE9F; D23 pede prova do renderer e D24 pede física da cena atual antes de editar. |
| G — combate/pesca/IA | D19–D20, D25 | C, D08 e legibilidade F antes de medir UX/timing; probes físicos de NPC/inimigo antes de corrigir IA, sem tuning implícito. |
| H — cobertura/gates | D21–D22 | Checklist desde o início; fechamento por cenário após respectivas correções. |

Não constitui nova wave aprovada nem gera automaticamente oito specs. Ao especificar, procurar cobertura existente e anexar aceites faltantes; bugfix de contrato local não precisa spec artificial. Mudanças estruturais, de save ou novas capacidades seguem o workflow proporcional do projeto.

## Decisões ainda abertas e próximo estágio

- Confirmar regra canônica de propriedade/consumo da lágrima e corpo durante a decisão de morte (bloqueia desenho de D02, não D01).
- Confirmar política de pausa na fonte e disponibilidade real de título/settings (D04/D06).
- Definir apresentação do baú indisponível; armazenamento novo fica fora do pacote até decisão própria (D07).
- Escolher layout/arte por mockup mantendo direção aprovada; resoluções acima são alvos propostos, não suporte já demonstrado (D11–D17).
- Confirmar bindings de pergaminho/quests e prioridade interação/ataque pelo fluxo real (D05/D08).
- Balanceamento, duração de feedback e janelas de reação só serão propostos após medição adequada (D19/D20).

Próximo estágio: resolver contratos materiais por fatia, localizar specs existentes e usar `spec-authoring` para requisitos/aceites aprováveis. Nenhuma pendência deste documento autoriza inventar regra durante implementação. Este turno produziu apenas refinamento documental; não rodou novo Play Mode nem promoveu aceites.
