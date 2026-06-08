# Cindar's Hope — Final Human Validation by Wave

> **Status:** checklist operacional para validação humana final.  
> **Local:** `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`  
> **Tipo:** validação manual final / Unity Play Mode / regressão por wave.  
> **Função:** orientar o humano a validar o jogo no Unity depois que specs/lotes estiverem implementados e as validações automatizadas disponíveis tiverem rodado.  
> **Não é spec implementável.** Nenhum agente deve executar código a partir deste arquivo.  
> **Regra:** este arquivo consolida cenários finais por wave. Ele não exige validação humana intermediária spec a spec.

---

## 0. Regra central

A validação humana deve acontecer no final de um bloco/lote combinado ou no final da execução completa, conforme decisão do usuário.

Durante implementação de specs:

```text
Não pedir ao usuário para abrir Unity a cada spec.
Não bloquear micro-spec por ausência de Play Mode humano.
Não declarar ACCEPTED por validação humana se este checklist ainda não foi executado.
Specs runtime que dependem de Play Mode humano devem marcar validação como DEFERRED_TO_FINAL_VALIDATION.
```

Este documento deve ser usado quando:

```text
as specs do lote/wave foram implementadas;
docs validation foi rodada ou reportada;
build/compile/Unity validation foram rodadas ou reportadas quando aplicável;
execution reports existem;
o usuário decidiu iniciar validação humana final.
```

---

## 1. Como registrar resultado

Para cada wave, registrar:

```text
Wave:
Data:
Branch/commit:
Unity version:
Cena(s) testadas:
Resultado: PASS / PASS_WITH_NOTES / BLOCKED / FAIL
Bugs encontrados:
Screenshots/logs anexados:
Observações:
```

Status:

```text
PASS:
  Fluxo validado sem bug bloqueador.

PASS_WITH_NOTES:
  Fluxo principal validado, mas há ajuste visual/balance/UX não bloqueador.

BLOCKED:
  Não foi possível testar por erro de cena, compile, crash, missing script, ausência de prefab/asset ou fluxo inacessível.

FAIL:
  Fluxo acessível, mas comportamento esperado não funciona.
```

---

## 2. Preparação antes de validar no Unity

### 2.1 Preparação do repo

1. Abrir o repositório local na branch alvo.
2. Confirmar que o estado local corresponde ao commit/lote a validar.
3. Confirmar que não há arquivos locais modificados fora do esperado.
4. Ler os reports do lote em `docs/validation/`.
5. Ler pendências declaradas nos reports antes de iniciar Play Mode.

### 2.2 Preparação do Unity

1. Abrir o projeto pelo Unity Hub.
2. Aguardar importação completa.
3. Abrir Console.
4. Limpar Console.
5. Confirmar que não há erros vermelhos após abrir o projeto.
6. Abrir a primeira cena recomendada do lote.
7. Entrar em Play Mode.
8. Confirmar que o Console não gera erro novo imediatamente.

Se houver erro vermelho antes de qualquer interação:

```text
Resultado da wave: BLOCKED.
Registrar erro e stack trace.
Não continuar testando fluxos dependentes desse erro.
```

---

## 3. Regressão base obrigatória para qualquer validação final

Executar antes de validar waves específicas.

### 3.1 Boot e cena inicial

1. Abrir a cena inicial do jogo.
2. Entrar em Play Mode.
3. Confirmar que o jogo inicia sem erro vermelho no Console.
4. Confirmar que o jogador aparece em posição válida.
5. Confirmar que câmera acompanha o jogador.
6. Confirmar que é possível mover o jogador.
7. Confirmar que o jogador não atravessa colisores óbvios.
8. Sair do Play Mode.

### 3.2 Save básico

1. Entrar em Play Mode.
2. Fazer uma ação que altere estado persistido simples.
3. Salvar.
4. Sair do Play Mode.
5. Entrar novamente.
6. Carregar save.
7. Confirmar que o estado salvo foi restaurado.
8. Confirmar que o Console não mostra erro de load.

### 3.3 Modal/input básico

1. Abrir uma UI modal disponível.
2. Tentar mover o personagem enquanto modal está aberta.
3. Confirmar que gameplay input fica bloqueado quando esperado.
4. Fechar a UI com tecla/botão previsto.
5. Confirmar que gameplay input volta.

---

# WAVE 00 — Roadmap / governança

## Objetivo da validação humana

Confirmar documentalmente que a estrutura de geração/execução está compreensível antes de validar gameplay.

## Unity necessário

```text
Não.
```

## Passos

1. Abrir `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`.
2. Confirmar que as waves estão claras.
3. Confirmar que a 01Q aparece como fundacional antes de execução em massa das Waves 02+.
4. Abrir `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`.
5. Confirmar que o template exige paralelização.
6. Confirmar que o template não exige human test intermediário.
7. Abrir `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
8. Confirmar que specs futuras concretas aparecem apenas quando realmente criadas.

## Critério de PASS

```text
Documentos de governança claros, sem instrução para validar humanamente spec a spec, e sem conflito evidente entre roadmap/template/registry.
```

---

# WAVE 01 — Core IDs / Events / Save baseline / Test Gate

## Objetivo da validação humana

Confirmar que a base técnica não quebrou boot, save/load, eventos observáveis e fluxo mínimo do jogo.

## Cenas prováveis

```text
BootScene
FarmScene
TownScene, se existir no lote
CaveScene, se existir no lote
```

## Passos

### 01.1 Boot limpo

1. Abrir `BootScene` ou cena inicial equivalente.
2. Entrar em Play Mode.
3. Confirmar que não há erro vermelho no Console.
4. Confirmar que a cena inicial carrega a cena jogável esperada.
5. Confirmar que o jogador aparece em spawn válido.

### 01.2 Save/load round-trip mínimo

1. Entrar em Play Mode.
2. Alterar pelo menos 3 estados simples, quando disponíveis:
   - posição/cena;
   - inventário/ouro;
   - dia/hora;
   - estado de crop/pickup/quest.
3. Salvar.
4. Sair do Play Mode.
5. Entrar novamente.
6. Carregar.
7. Confirmar que os estados voltaram corretamente.
8. Confirmar que não há duplicação de item/reward/pickup.

### 01.3 Robustez de ID inválido, se exposta para teste

1. Fazer backup do save manualmente.
2. Alterar no JSON um ID de item/asset/quest para valor inválido controlado.
3. Reabrir o jogo.
4. Carregar save.
5. Confirmar que o jogo não quebra.
6. Confirmar fallback, warning ou estado ignorado conforme especificado.
7. Restaurar backup do save original.

### 01.4 Regressão de eventos base

1. Executar ações que publiquem eventos existentes:
   - mudar dia;
   - ganhar/gastar ouro;
   - alterar inventário;
   - plantar/colher;
   - entrar/sair de cena.
2. Confirmar que os sistemas dependentes reagem uma única vez.
3. Confirmar que não há duplicação após trocar de cena e voltar.

## Critério de PASS

```text
Boot, save/load e eventos base funcionam sem erros vermelhos, duplicação ou perda evidente de estado.
```

---

# WAVE 02 — Time / Calendar / Weather / Lunar

## Objetivo da validação humana

Confirmar que tempo, calendário, clima e luas mudam estado do mundo sem quebrar farm, UI ou save/load.

## Passos

### 02.1 Ciclo de tempo

1. Abrir cena jogável principal.
2. Entrar em Play Mode.
3. Observar relógio/dia, se houver HUD.
4. Avançar tempo por fluxo normal ou ferramenta de debug autorizada.
5. Confirmar transição de hora/dia.
6. Confirmar que sistemas inscritos reagem uma única vez.

### 02.2 Day transition

1. Plantar ou preparar algum estado dependente de dia, se disponível.
2. Avançar para o próximo dia.
3. Confirmar atualização de crops, stamina/hunger/status temporais, jobs ou schedules quando aplicável.
4. Confirmar que o dia não avança duas vezes.

### 02.3 Calendário/season

1. Avançar dias até mudança de estação ou usar ferramenta autorizada.
2. Confirmar que season atual muda.
3. Confirmar que crops/eventos/shops que dependem de season respeitam a nova estação, se implementado.
4. Confirmar que UI não revela informação secreta indevida.

### 02.4 Weather

1. Gerar ou avançar até um dia com clima diferente.
2. Confirmar feedback visual/HUD de clima, se implementado.
3. Confirmar que chuva irriga ou afeta sistemas conforme especificado.
4. Confirmar que clima persiste após save/load.

### 02.5 Lunar cycle

1. Avançar até mudança de lua ou fase lunar.
2. Confirmar exibição correta da lua, se houver UI.
3. Confirmar efeitos de Alihana/Senya/Nyx apenas quando implementados.
4. Confirmar que o estado lunar persiste após save/load.

## Critério de PASS

```text
Tempo, dia, calendário, clima e lua avançam de forma consistente, persistem corretamente e não disparam efeitos duplicados.
```

---

# WAVE 03 — Quest / Objective / Event System

## Objetivo da validação humana

Confirmar que quests avançam por triggers/conditions corretos, rewards são idempotentes e save/load preserva estado.

## Passos

### 03.1 Receber quest

1. Iniciar uma quest por NPC, board, item ou trigger autorizado.
2. Confirmar que a quest aparece no estado/log correto.
3. Confirmar que texto visível não revela spoiler indevido.

### 03.2 Avançar objective

1. Executar ação necessária para avançar objetivo.
2. Confirmar que a objective muda uma única vez.
3. Repetir a mesma ação.
4. Confirmar que não há avanço duplicado indevido.

### 03.3 Conditions vs triggers

1. Criar uma situação em que a condição está incompleta.
2. Executar trigger relacionado.
3. Confirmar que a quest não avança indevidamente.
4. Completar a condição.
5. Executar trigger.
6. Confirmar avanço correto.

### 03.4 Reward idempotente

1. Completar uma quest com reward.
2. Confirmar recebimento do reward.
3. Salvar e carregar.
4. Tentar acionar entrega novamente.
5. Confirmar que o reward não é duplicado.

### 03.5 Persistência

1. Avançar uma quest até estado intermediário.
2. Salvar.
3. Reabrir/carregar.
4. Confirmar que estado, objetivos, flags e rewards pendentes persistiram.

## Critério de PASS

```text
Quest system respeita condition/trigger, não duplica reward, persiste estado e não revela spoiler indevido.
```

---

# WAVE 04 — UI foundation

## Objetivo da validação humana

Confirmar que input, modal stack, foco, HUD, notificações e estados de erro/vazio funcionam sem quebrar gameplay.

## Passos

### 04.1 Modal bloqueia gameplay

1. Entrar em Play Mode.
2. Abrir menu/modal disponível.
3. Tentar mover/interagir com o personagem.
4. Confirmar que input de gameplay fica bloqueado.
5. Fechar modal.
6. Confirmar que input volta.

### 04.2 Stack de modais

1. Abrir um menu.
2. Abrir uma confirmação/submodal.
3. Pressionar Esc/back.
4. Confirmar que fecha apenas o topo da stack.
5. Pressionar Esc/back novamente.
6. Confirmar que fecha o menu anterior.

### 04.3 Empty/error states

1. Abrir tela/lista sem itens ou sem dados.
2. Confirmar que aparece empty state claro.
3. Tentar ação inválida.
4. Confirmar mensagem de erro ou feedback compreensível.
5. Confirmar que o Console não recebe erro vermelho.

### 04.4 HUD principal

1. Observar HUD em gameplay normal.
2. Alterar HP/fome/stamina/ouro/dia quando disponível.
3. Confirmar atualização visual correta.
4. Confirmar que HUD final não depende de DebugHud para informação essencial.

## Critério de PASS

```text
UI foundation controla input/foco corretamente, exibe feedback básico e não deixa gameplay mover durante modais.
```

---

# WAVE 05 — Inventory / ItemInstance / Equipment / Hotbar

## Objetivo da validação humana

Confirmar que itens, stacks, instâncias, equipamento, durabilidade e hotbar funcionam sem perda ou duplicação.

## Passos

### 05.1 Inventário e stack

1. Coletar dois itens iguais.
2. Confirmar que stack aumenta corretamente quando aplicável.
3. Coletar item diferente.
4. Confirmar que ocupa slot separado.
5. Testar limite de stack/slot se disponível.

### 05.2 Split/move

1. Abrir inventário.
2. Mover item entre slots.
3. Dividir stack, se implementado.
4. Reunir stack, se implementado.
5. Confirmar que quantidades não somem nem duplicam.

### 05.3 ItemInstance mutável

1. Obter item com estado mutável, se disponível.
2. Alterar durabilidade/qualidade/atributo.
3. Salvar e carregar.
4. Confirmar que o estado mutável persistiu.

### 05.4 Equipment

1. Equipar item em slot válido.
2. Confirmar mudança de stats/HUD quando aplicável.
3. Tentar equipar em slot inválido.
4. Confirmar bloqueio/feedback.
5. Remover equipamento.
6. Confirmar restauração dos stats.

### 05.5 Hotbar

1. Vincular item/ação à hotbar.
2. Usar pelo atalho.
3. Remover item do inventário.
4. Confirmar que hotbar invalida ou atualiza binding corretamente.
5. Salvar/carregar e confirmar persistência.

## Critério de PASS

```text
Inventory/equipment/hotbar não perdem estado, não duplicam itens e validam slots/bindings inválidos.
```

---

# WAVE 06 — UI screens essenciais

## Objetivo da validação humana

Confirmar que telas finais essenciais usam os contratos runtime corretos e respeitam input/modal/foco.

## Passos

### 06.1 Inventory screen

1. Abrir inventário.
2. Ver itens, quantidades, empty state e detalhes.
3. Mover/usar item quando aplicável.
4. Fechar e reabrir.
5. Confirmar estado consistente.

### 06.2 Storage/chest screen

1. Abrir storage/chest.
2. Transferir item do player para storage.
3. Transferir item de volta.
4. Salvar/carregar.
5. Confirmar persistência.

### 06.3 Equipment screen

1. Abrir tela de equipamento.
2. Selecionar item equipável.
3. Comparar stats.
4. Equipar/remover.
5. Confirmar feedback visual e stats corretos.

### 06.4 Shop screen

1. Abrir loja.
2. Comprar item permitido.
3. Confirmar ouro descontado e item recebido.
4. Vender item permitido.
5. Confirmar ouro recebido e item removido.
6. Tentar comprar sem ouro.
7. Confirmar erro controlado.

### 06.5 Crafting screen

1. Abrir crafting.
2. Ver receita disponível e indisponível.
3. Craftar receita com ingredientes suficientes.
4. Tentar craftar sem ingredientes.
5. Confirmar feedback correto.

### 06.6 Quest log / calendar / Fonte menu

1. Abrir quest log.
2. Confirmar visibilidade e spoiler control.
3. Abrir calendar.
4. Confirmar dia/season/eventos visíveis.
5. Abrir Fonte menu, se disponível.
6. Confirmar que só funções desbloqueadas aparecem.

## Critério de PASS

```text
Telas essenciais representam estado real, bloqueiam input corretamente e tratam erro/empty state sem Console error.
```

---

# WAVE 07 — Farm / Economy / Crafting

## Objetivo da validação humana

Confirmar o loop agrícola/econômico: plantar, crescer, colher, armazenar, vender, comprar, craftar/processar e persistir.

## Passos

### 07.1 Plantio e crescimento

1. Obter sementes.
2. Plantar em tile válido.
3. Tentar plantar em tile inválido.
4. Confirmar feedback.
5. Avançar dia.
6. Confirmar crescimento conforme regra de crop/season/weather.

### 07.2 Chuva/irrigação

1. Criar ou aguardar dia chuvoso.
2. Confirmar irrigação automática quando especificado.
3. Avançar dia.
4. Confirmar que crop irrigado evolui corretamente.

### 07.3 Colheita e qualidade

1. Colher crop pronto.
2. Confirmar item no inventário.
3. Confirmar qualidade/fertilizante se implementado.
4. Confirmar que plot volta ao estado esperado.

### 07.4 Shipping bin / venda

1. Colocar item vendável no shipping bin ou sell point.
2. Avançar dia ou confirmar venda conforme regra.
3. Confirmar ouro recebido.
4. Confirmar que item foi removido uma única vez.

### 07.5 Shop stock e pricing

1. Abrir loja.
2. Comprar item.
3. Confirmar stock atualizado.
4. Avançar dia/season quando aplicável.
5. Confirmar restock/preço conforme regra.
6. Tentar ciclo de compra/venda para anti-arbitragem simples.

### 07.6 Crafting/processing

1. Abrir crafting/station.
2. Iniciar receita/processamento.
3. Confirmar consumo de ingredientes.
4. Avançar tempo/dia.
5. Coletar resultado.
6. Salvar/carregar durante processamento e confirmar retomada correta.

## Critério de PASS

```text
Loop farm/economy/crafting funciona de ponta a ponta, persiste estado e não permite duplicação óbvia de item/ouro.
```

---

# WAVE 08 — City / NPC / Dialogue / Services

## Objetivo da validação humana

Confirmar cidade navegável, NPCs, schedules, diálogo, serviços, lojas e hooks de quest.

## Passos

### 08.1 Acesso à cidade

1. Sair da fazenda pela rota/portal de cidade.
2. Confirmar carregamento da cidade.
3. Confirmar spawn correto.
4. Voltar para fazenda.
5. Confirmar retorno ao spawn correto.

### 08.2 Navegação e colisão

1. Caminhar por vias principais.
2. Testar colisão em paredes, água, balcões e objetos sólidos.
3. Confirmar que entradas/saídas funcionam.
4. Confirmar que não há softlock em interiores.

### 08.3 NPC schedule

1. Localizar NPC em horário inicial.
2. Avançar tempo.
3. Confirmar deslocamento ou troca de estado/local.
4. Salvar/carregar e confirmar NPC em estado coerente.

### 08.4 Diálogo

1. Interagir com NPC.
2. Confirmar abertura de diálogo.
3. Confirmar bloqueio de movimento durante diálogo.
4. Avançar/fechar diálogo.
5. Confirmar retorno de input.

### 08.5 Serviços/shops

1. Abrir serviço de NPC.
2. Comprar/vender/usar serviço.
3. Confirmar efeitos e feedback.
4. Tentar serviço fora de horário ou bloqueado.
5. Confirmar mensagem correta.

### 08.6 Notice board / orders / contracts

1. Abrir board, se implementado.
2. Aceitar ordem/contrato.
3. Confirmar entrada no quest system.
4. Completar entrega/condição.
5. Confirmar reward idempotente.

## Critério de PASS

```text
Cidade, NPCs, diálogos e serviços são acessíveis, consistentes com tempo/quest/economia e não causam softlock.
```

---

# WAVE 09 — Player / Skills / Magic

## Objetivo da validação humana

Confirmar progressão do jogador, stats, recursos, skill tree, active slots, respec e magia.

## Passos

### 09.1 Stats e recursos

1. Abrir tela de personagem/stats.
2. Confirmar HP/stamina/mana/hunger conforme implementado.
3. Executar ação que consome recurso.
4. Confirmar consumo e recuperação conforme regra.
5. Salvar/carregar e confirmar persistência.

### 09.2 Level/skill points

1. Ganhar XP por ação disponível.
2. Subir de nível.
3. Confirmar ganho de skill point conforme regra.
4. Salvar/carregar e confirmar estado.

### 09.3 Skill tree

1. Abrir skill tree.
2. Comprar node com pré-requisitos válidos.
3. Tentar comprar node sem pré-requisito/ponto.
4. Confirmar bloqueio.
5. Confirmar aplicação de passivo/ativo.

### 09.4 Active slots

1. Aprender skill ativa.
2. Equipar em active slot.
3. Usar skill.
4. Preencher slots e tentar equipar outra.
5. Confirmar regra de slot cheio.
6. Salvar/carregar e confirmar bindings.

### 09.5 Respec na Fonte

1. Acessar Fonte quando desbloqueada.
2. Executar respec.
3. Confirmar devolução/limpeza conforme regra.
4. Confirmar que active slots ficam válidos.
5. Salvar/carregar.

### 09.6 Magia/spells

1. Aprender spell por fonte autorizada.
2. Ver spell na UI.
3. Usar spell com mana suficiente.
4. Tentar usar sem requisito/mana.
5. Confirmar feedback correto.

## Critério de PASS

```text
Progressão, skills, active slots, respec e magia respeitam requisitos, persistem e não concedem poder duplicado.
```

---

# WAVE 10 — Cave / Combat / Enemies / Death

## Objetivo da validação humana

Confirmar loop de caverna: entrada, geração, navegação, inimigos, combate, loot, checkpoints, boss gates, morte e recuperação.

## Passos

### 10.1 Entrada e saída da caverna

1. Entrar na caverna pela rota correta.
2. Confirmar carregamento da CaveScene.
3. Confirmar spawn seguro.
4. Sair da caverna.
5. Confirmar retorno correto.

### 10.2 Run seed / geração

1. Entrar no andar inicial.
2. Observar layout básico.
3. Salvar/carregar ou sair/voltar conforme regra.
4. Confirmar que o layout não rerolla indevidamente quando deveria persistir.

### 10.3 Navegação/confinamento

1. Caminhar por corredores/salas.
2. Confirmar que jogador não atravessa parede.
3. Confirmar que não nasce preso.
4. Confirmar que exits/stairs/gates são acessíveis.

### 10.4 Recursos/loot

1. Coletar recurso da caverna.
2. Confirmar item no inventário.
3. Trocar de cena/salvar/carregar.
4. Confirmar persistência ou refresh conforme regra.

### 10.5 Enemies e IA

1. Encontrar inimigo melee.
2. Confirmar detecção/perseguição/ataque conforme esperado.
3. Encontrar inimigo ranged/status se implementado.
4. Confirmar comportamento sem erro.
5. Confirmar que enemy não atravessa barreiras indevidas.

### 10.6 Combate do jogador

1. Atacar inimigo com ação física.
2. Usar skill/spell se disponível.
3. Confirmar dano/feedback/status.
4. Derrotar inimigo.
5. Confirmar loot/XP/evento uma única vez.

### 10.7 Status effects

1. Aplicar ou sofrer status.
2. Confirmar duração/tick/efeito visual quando implementado.
3. Confirmar remoção ao final.
4. Salvar/carregar se status persistente for esperado.

### 10.8 Checkpoint/boss gate

1. Avançar até checkpoint.
2. Ativar checkpoint.
3. Sair/morrer/carregar conforme regra.
4. Confirmar retorno correto.
5. Tentar boss gate sem requisito.
6. Confirmar bloqueio.
7. Cumprir requisito e confirmar acesso.

### 10.9 Death/corpse recovery

1. Morrer em cenário controlado.
2. Confirmar respawn correto.
3. Confirmar perda/penalidade conforme regra.
4. Retornar ao local/corpo, se sistema existir.
5. Recuperar itens conforme regra.
6. Confirmar que não duplica perda/recuperação.

## Critério de PASS

```text
Caverna é navegável, combate funciona, death/recovery não duplica estado e run/checkpoints persistem conforme contrato.
```

---

# WAVE 11 — Bestiary / Knowledge Discovery

## Objetivo da validação humana

Confirmar que descoberta de conhecimento ocorre sem revelar tudo antes da hora e sem quebrar combat/equipment/spell UI.

## Passos

### 11.1 Estado inicial desconhecido

1. Abrir UI/log onde conhecimento aparece, se implementado.
2. Confirmar que inimigos/fragilidades não descobertos aparecem ocultos ou parciais.

### 11.2 Descoberta por encontro

1. Encontrar inimigo novo.
2. Confirmar criação de entrada parcial, se aplicável.
3. Derrotar, observar ou sofrer ataque conforme regra.
4. Confirmar desbloqueio incremental correto.

### 11.3 Vulnerability/known interactions

1. Usar arma/material/spell contra inimigo.
2. Confirmar que efetividade só aparece após descoberta.
3. Confirmar que tooltip não revela fraqueza desconhecida cedo.

### 11.4 Persistência

1. Descobrir informação.
2. Salvar/carregar.
3. Confirmar que conhecimento persistiu.
4. Confirmar que conhecimento não desbloqueado continua oculto.

### 11.5 Quest/bestiary hooks

1. Iniciar quest de pesquisa, se implementada.
2. Cumprir condição de descoberta.
3. Confirmar avanço de objective.
4. Confirmar reward idempotente.

## Critério de PASS

```text
Bestiary/knowledge descobre informações incrementalmente, persiste corretamente e respeita spoiler control.
```

---

# WAVE 12 — Future systems

## Objetivo da validação humana

Validar somente os sistemas futuros que forem realmente implementados. Não validar sistemas que continuarem marcados como future.

## Sistemas possíveis

```text
pets;
companions;
social/relationship;
romance/casamento/poliamor;
partner helper;
festival minigames;
bestiary research service;
endgame level 100/101;
final choice cinematic;
full accessibility settings.
```

## Passos genéricos

Para cada sistema futuro implementado:

1. Confirmar que o sistema está desbloqueado apenas no momento correto.
2. Confirmar que não vira power path obrigatório se a direction disser que é opcional.
3. Confirmar que não substitui sistemas principais.
4. Confirmar que persiste save/load.
5. Confirmar que respeita UI/input/modal stack.
6. Confirmar que não causa softlock.
7. Confirmar que não revela spoiler/endgame cedo.

## Critério de PASS

```text
Somente sistemas futuros implementados são validados; sistemas ainda future não contam como falha.
```

---

## 13. Checklist final de regressão full-game

Executar após passar pelas waves relevantes.

### 13.1 Loop completo mínimo

1. Iniciar jogo novo.
2. Mover na fazenda.
3. Abrir inventário.
4. Plantar crop.
5. Avançar dia.
6. Colher.
7. Vender.
8. Comprar algo na cidade.
9. Falar com NPC.
10. Aceitar/avançar uma quest simples.
11. Entrar na caverna.
12. Derrotar inimigo.
13. Coletar loot.
14. Sair da caverna.
15. Salvar.
16. Fechar Play Mode.
17. Reabrir/carregar.
18. Confirmar estado restaurado.

### 13.2 Console final

1. Limpar Console antes do teste full-game.
2. Executar loop completo.
3. Confirmar que não há erro vermelho novo.
4. Registrar warnings relevantes.

### 13.3 Save final

1. Criar save novo.
2. Jogar pelo menos um ciclo com fazenda/cidade/caverna.
3. Salvar.
4. Carregar.
5. Confirmar:
   - inventário;
   - ouro;
   - dia/hora/season/weather/lua;
   - quests;
   - skills;
   - cave state;
   - bestiary/knowledge;
   - NPC/schedules quando aplicável.

### 13.4 Critério final de aceite humano

```text
O jogo precisa completar o loop principal sem erro bloqueador:
Farm -> City -> Quest/Service -> Cave/Combat -> Loot/Economy -> Save/Load.
```

Se qualquer etapa bloquear progressão principal:

```text
Resultado final: FAIL ou BLOCKED.
Abrir bugfix spec antes de considerar o lote aceito.
```

---

## 14. O que não fazer durante validação humana final

```text
Não alterar código para contornar bug.
Não alterar prefab/scene manualmente sem registrar.
Não aceitar feature porque “parece compilar”.
Não marcar ACCEPTED se o comportamento principal não foi testado.
Não testar sistemas future que não foram implementados.
Não misturar bug visual menor com blocker de gameplay.
Não apagar save de teste sem registrar evidência quando o bug for de persistência.
```

---

## 15. Evidências recomendadas

Para cada bug ou aprovação relevante, guardar:

```text
screenshot da cena;
print do Console;
arquivo de save usado;
commit/branch;
passos para reproduzir;
resultado esperado;
resultado observado.
```

Local recomendado para reports finais:

```text
docs/validation/final/<wave>_human_validation_report.md
```

Exemplo:

```text
docs/validation/final/wave_07_human_validation_report.md
docs/validation/final/full_game_human_validation_report.md
```

---

## Nota de Correção de Numeração — SPEC 12 (2026-06-08)

As seções "WAVE 10 — Cave/Combat/Enemies/Death" e "WAVE 11 — Bestiary/Knowledge Discovery" acima foram geradas antes da execução real e referem-se ao CONTEÚDO correto mas com NUMERAÇÃO desatualizada. No plano de execução real:

- WAVE 10 executada = **Main Progression / Fonte / Endgame** (level 100/101, FinalChoice, MemoryArc, BlackStone)
- WAVE 11 executada = **UI Projections / HUD / Input Focus / Inventory / Shop menus**

As seções corretas para validação humana dessas waves executadas estão abaixo.

---

# WAVE 10 (Executada) — Main Progression / Fonte / Endgame

## Objetivo da validação humana

Confirmar que level 100 gate, level 101 unlock, final choice, memory arc / BlackStone threats, e Fonte functions avançam e persistem sem spoilers indevidos.

## Unity necessário

```text
SIM — requer FlowScene, cena principal, Fonte scene ou equivalente.
```

## Passos

### W10.1 Level 100 gate

1. Criar estado de save com nível próximo a 100 ou usar ferramenta de debug autorizada.
2. Confirmar que level 100 gate aparece apenas quando requisito do fragmento de Life está presente.
3. Tentar passar o gate sem Life fragment.
4. Confirmar que bloqueio é visível e explícito.

### W10.2 Level 101 unlock

1. Completar requisito de Life fragment + Level 100 gate.
2. Confirmar que opção de nível 101 fica disponível.
3. Confirmar que nível 101 não fica disponível sem Hope fragment ou requisito final.
4. Confirmar que o estado persiste após save/load.

### W10.3 Final Choice

1. Cumprir requisitos para FinalChoice (Hope fragment + Level 101 access + confirmação forte).
2. Escolher uma das 3 opções (Protect / Seal / Use).
3. Confirmar que o resultado é idempotente.
4. Tentar escolher novamente após escolha feita.
5. Confirmar bloqueio idempotente.
6. Confirmar que save/load preserva o estado correto.

### W10.4 Memory Arc / BlackStone spoiler gate

1. Confirmar que MemoryArcState começa em Unknown.
2. Avançar pelo ato 1 sem trigger de MemoryArc.
3. Confirmar que termos/personagens do Memory Arc (Sethra, Vaelrion) não aparecem.
4. Avançar até ato que desbloqueia MemoryArc.
5. Confirmar desbloqueio incremental conforme ato.

### W10.5 Fonte functions gate

1. Acessar Fonte UI.
2. Confirmar que Respec está oculto (antes do Memory fragment).
3. Confirmar que Purification está oculta (antes do Life fragment).
4. Confirmar que FinalChoice está oculta (antes de Hope/final route).
5. Desbloquear fragmento e confirmar que a função correspondente aparece.

## Critério de PASS

```text
Level 100/101 gates respeitam requisitos, FinalChoice é idempotente, Memory Arc respeita spoiler por ato, Fonte mostra apenas funções desbloqueadas.
```

---

# WAVE 11 (Executada) — UI Projections / HUD / Input Focus / Inventory / Menus

## Objetivo da validação humana

Confirmar que HUD projections, input focus modal routing, inventário com proteções, equipment comparison e menus de shop/crafting/skill tree/Fonte funcionam sem permitir gameplay input atrás de modais e sem revelar itens proibidos como vendáveis.

## Unity necessário

```text
SIM — requer cenas com HUD ativo, inventory UI, shop, crafting, Fonte menu, skill tree.
```

## Passos

### W11.1 HUD projection

1. Entrar em Play Mode.
2. Observar HUD principal.
3. Confirmar que hotbar mostra até 4 active skill slots.
4. Confirmar que nenhum slot de Dash/Dodge/Block aparece no hotbar.
5. Confirmar que debug info não aparece na HUD final (apenas em builds debug).
6. Confirmar que notificações aparecem com prioridade correta e não excedem cap.

### W11.2 Modal input blocking

1. Abrir inventário.
2. Tentar mover WASD enquanto inventário está aberto.
3. Confirmar que jogador não se move.
4. Fechar inventário.
5. Confirmar que WASD volta a funcionar.
6. Repetir para shop, crafting, skill tree, Fonte menu.

### W11.3 Inventory proteções

1. Tentar vender item de quest no shop.
2. Confirmar que item não aparece como vendável ou mostra proteção explícita.
3. Tentar descartar item de chave (key item).
4. Confirmar bloqueio com mensagem clara.
5. Tentar descartar item Unique.
6. Confirmar que aparece confirmação forte.

### W11.4 Equipment comparison

1. Abrir inventário com item equipável.
2. Selecionar item candidato.
3. Ver comparação de stats.
4. Confirmar que a comparação é preview only — item não é equipado automaticamente.
5. Confirmar que stats corretos aparecem (AttackDelta, DefenseDelta).

### W11.5 Shop Buy/Sell mode

1. Abrir shop.
2. Entrar no modo Buy.
3. Confirmar que o estoque da loja aparece.
4. Entrar no modo Sell.
5. Confirmar que o inventário vendável do jogador aparece.
6. Confirmar que itens de quest/chave não aparecem como vendáveis.
7. Testar empty state — loja sem estoque deve mostrar mensagem, não tela em branco.

### W11.6 Crafting menu

1. Abrir crafting.
2. Ver receita disponível vs. bloqueada.
3. Tentar craftar receita sem materiais.
4. Confirmar mensagem de ingrediente faltando com detalhes corretos.

### W11.7 Skill tree menu

1. Abrir skill tree.
2. Confirmar 5 árvores/abas disponíveis.
3. Ver active slots (máximo 4).
4. Confirmar que Respec não aparece antes do Memory fragment.
5. Tentar comprar node sem pré-requisito.
6. Confirmar bloqueio com reason text.

### W11.8 Fonte menu

1. Abrir Fonte menu (se cena disponível).
2. Confirmar que funções não desbloqueadas estão ocultas.
3. Confirmar que Living Water básico aparece se desbloqueado.
4. Confirmar que Respec/Purification/FinalChoice não aparecem antes dos fragmentos corretos.

## Critério de PASS

```text
HUD não excede limites de slot nem mostra debug, input é bloqueado em todos modais, proteções de item funcionam, equipment comparison é preview-only, menus mostram estados corretos com spoiler gates respeitados.
```
