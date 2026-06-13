# SPEC — Romance: Fundação Runtime (candidatos canônicos, bi, poliamor 2, gates)

> **Spec ID:** `fable_46_spec_romance_foundation_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 9
> **Priority:** P3
> **Type:** Runtime / Save
> **Domain:** NPC / Social
> **Parallelizable:** NO
> **Parallel group:** N/A (NPC/diálogo/save de amizade lock)
> **Can run with:** specs sem NPC, diálogo ou save
> **Must not run with:** F25, F26, F28, F35 (cadeia NPC/amizade), F07/F12/F21/F42 (seções de save)
> **Repo lock scope:** `NPC/Friendship/**` (extensão), pools de diálogo (F28), seção de save de amizade
> **Depends on:**
> - `fable_26_spec_friendship_state_contract` (níveis 0-5 + FriendshipSaveData)
> - `fable_35_spec_npc_side_quest_chains` (cadeia pessoal completa como gate)
> - `fable_28_spec_dialogue_conditions_pools` (pools condicionais p/ falas de parceiro)
> - `fable_36_spec_main_quest_acts_2_4` (flag do Ato 3 — gate do NPC Nymiriano)
> **Blocks:** casamento/cerimônia (spec futura), parceiro-companion (futuro)
> **Scope:** RomanceState por NPC (Interesse→Namoro→Compromisso) com gates canônicos, todos bi, máximo 2 parceiros simultâneos, falas e bônus de parceiro, save aditivo.
> **Out of scope:** casamento/cerimônia, visitas/ajuda na fazenda, ciúme, Nymiriano antes do Ato 3, companion unlock por romance.

required_adrs: []
required_game_rules: [save_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

O CITY_NPC_ROSTER v1.1 já marca os candidatos por NPC (§4 estados: RomanceEligibleAnyPlayerGender,
UnavailableForRomance, LateRomanceEligible, MarriedToNpc; §6 lista os 11 candidatos — Sylveth,
Ozzra, Zrix, Yael, Thalindra, Dagna, Ser Alaric, Eiran, Liora, Savra e Maelor [tardio]) e o
NPC Nymiriano (FABLE_DECISOES §1) é do sexo oposto ao do jogador, romanceável e TARDIO
(chega no Ato 3 via F36). As decisões §13 fecharam: usar os candidatos do roster, todos
bissexuais (RomanceEligibleAnyPlayerGender), e poliamor com MÁXIMO 2 parceiros simultâneos
— decisão NOVA que SOBREPÕE o canon antigo do SOCIAL_RELATIONSHIP_ROMANCE (que dizia até 3).
A F26 entrega a fundação de amizade (níveis 0-5, save) com romance explicitamente fora de
escopo. Esta spec é a fundação de romance por cima dela.

## Problema

O roster define romance como parte da identidade dos NPCs e as quests de vínculo já estão
nomeadas nas fichas, mas não existe NENHUM estado de romance em runtime: amizade para no
nível 5 sem destino, as decisões aprovadas (bi/poliamor 2/Nymiriano tardio) não têm onde
viver, e qualquer spec futura de casamento não tem fundação. Sem limite implementado, um
terceiro pedido de namoro não teria resposta canônica — inconsistência narrativa direta.

## Objetivo

Ao final desta spec, cada NPC candidato deve ter um RomanceState progressível
(None→Interesse→Namoro→Compromisso) gated por amizade nível 5 + cadeia pessoal F35
completa + confissão via diálogo; o limite de 2 parceiros simultâneos deve ser aplicado
com recusa falada no 3º pedido; parceiros devem ter falas próprias (pools F28 com condição
IsPartner) e presentes com +50% de amizade; tudo persistido como campos aditivos na seção
de amizade — sem casamento e sem Nymiriano antes do Ato 3.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md (§4-6 estados/candidatos; fichas: quests de vínculo)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§13 romance/poliamor 2; §1 Nymiriano)
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md (estágios/consentimento — ONDE NÃO CONFLITAR com as decisões; limite 3→2 SOBREPOSTO)
.claude/rules/testing-quality-gate.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/npc-dialogue-authoring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar (estado esperado pós F26/F28/F35 — confirmar na Fase 0):
- FriendshipService (níveis 0-5, IsAtLeast, FriendshipSaveData padrão WI-18, eventos);
- pools de diálogo condicionais (F28) com sistema de condições extensível;
- cadeias pessoais por NPC (F35) com flag de conclusão por cadeia;
- NpcDefinition/NpcRegistry (IDs canônicos dos 23 NPCs); flag act_3_done (F36);
- GameEventBus; SaveManager WI-18.
Não existe:
- RomanceState, eligibility por NPC em dados, limite de parceiros, confissão, falas de
  parceiro, bônus de presente de parceiro.
Auditar Fase 0: onde o roster está refletido em dados (NpcDefinition tem campo de
relacionamento? Se não: tabela canônica nesta spec); IDs reais dos 11+1 candidatos;
shape das condições F28 e da flag de cadeia F35.
```

## Engineering stories

```text
Como jogador, quero declarar interesse a um NPC de amizade máxima e vê-lo virar parceiro.
Como jogador, quero que qualquer candidato aceite meu personagem independente de gênero (bi).
Como jogador com 2 parceiros, quero que o 3º pedido seja recusado com uma fala digna, não um erro.
Como NPC parceiro, quero falas diferentes de um conhecido (pools IsPartner).
Como sistema de save, quero romance como campos aditivos simples na seção de amizade.
Como main quest, quero o Nymiriano romanceável apenas após o Ato 3 (gate por flag).
```

## Escopo

```text
Inclui:
- enum RomanceStage {None, Interesse, Namoro, Compromisso} + RomanceService hospedado no
  FriendshipService/bootstrap (API: GetStage(npcId), CanConfess(npcId), Confess(npcId),
  Advance(npcId), Partners() — lista de npcIds com stage >= Namoro);
- eligibility canônica em dados: tabela RomanceEligibility {npcId → Eligible | Unavailable |
  LateActGated} derivada do roster v1.1 (11 candidatos Eligible — Sylveth, Ozzra, Zrix,
  Yael, Thalindra, Dagna, Alaric, Eiran, Liora, Savra; Maelor LateRomanceEligible — gate
  adicional: amizade 5 + flag de cadeia dele; Nymiriano gated por act_3_done F36; demais
  Unavailable — ex.: Mara casada, Corvus, Brumdar); todos os elegíveis aceitam qualquer
  gênero do jogador (decisão bi — SEM checagem de gênero no código);
- gates de confissão: amizade nível 5 (F26 IsAtLeast) + cadeia pessoal F35 completa (flag)
  + eligibility OK → opção "Confessar" no diálogo (F28 entry condicional); sucesso →
  Interesse; progressão Interesse→Namoro→Compromisso por marcos simples v1 (N interações
  de parceiro + 1 presente em cada estágio — constantes nomeadas testadas);
- limite de 2 parceiros SIMULTÂNEOS (decisão §13 — sobrepõe o canon de 3): Partners().Count
  >= 2 → Confess recusa com diálogo dedicado de recusa (pool F28, texto neutro digno);
  ciúme NÃO entra (decisão de simplicidade v1);
- falas de parceiro: condição nova IsPartner(npcId) registrada no sistema de condições F28;
  pools de parceiro por estágio (mínimo: 3 falas/estágio/NPC genérico-personalizável);
- presentes: parceiro (Namoro+) recebe +50% pontos de amizade por presente (multiplicador
  no ponto único de ganho por presente do F26 — anti-exploit do cap diário mantido);
- eventos: RomanceStageChangedEvent(npcId, stage) e RomanceConfessionRejectedEvent(npcId,
  motivo) → toasts;
- save: campos ADITIVOS na FriendshipSaveData (por NPC: romanceStage int, stageProgress
  int, confessedDay int) — load legado = tudo None;
- EditMode tests: gates de confissão (cada condição isolada), limite 2 (3º recusado),
  recusa determinística, late gates (Maelor/Nymiriano), progressão de estágio, +50%
  presente (com cap diário), round-trip de save, load legado.
```

## Fora de escopo

```text
Não inclui: casamento/cerimônia/anel (spec futura); visitas/ajuda na fazenda; quests de
vínculo autorais nomeadas nas fichas (entram como conteúdo na spec de casamento);
parceiro-companion; ciúme/breakup; polycule UI; aniversários.
```

## Regras de não duplicação

```text
Não criar segundo tracker social — romance ESTENDE FriendshipService/save (F26).
Não criar segundo sistema de condição de diálogo — registrar IsPartner no F28.
Não criar nova seção de save — campos aditivos na seção de amizade.
Não duplicar a tabela do roster — fonte única RomanceEligibility consumida por diálogo/serviço.
```

## Critérios de aceite

### CA-1 Confissão gated
- Confessar exige amizade 5 + cadeia F35 + eligibility; faltando qualquer um, a opção não
  aparece (ou recusa com motivo); sucesso → Interesse + evento.
- Evidência: testes por condição isolada.

### CA-2 Limite de 2 parceiros
- Com 2 parceiros (Namoro+), 3ª confissão é recusada com diálogo dedicado e estado
  inalterado; ao terminar não existir (sem breakup v1), limite é permanente por save.
- Evidência: teste do limite + texto de recusa presente no pool.

### CA-3 Late gates
- Maelor exige cadeia própria além do nível 5; Nymiriano não é confessável sem act_3_done.
- Evidência: testes de gate.

### CA-4 Parceiro vivo no jogo
- IsPartner muda pools de fala; presente de parceiro rende +50% (cap diário respeitado).
- Evidência: testes + cenário humano.

### CA-5 Persistência
- Round-trip preserva estágios/progresso; save legado (sem campos) carrega tudo None.
- Evidência: testes de save.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/NPC/Friendship/
  RomanceStage.cs / RomanceEligibility.cs (enum + tabela canônica)
  RomanceService.cs (regras puras + host no FriendshipService)
Assets/_Game/Scripts/NPC/Dialogue/<condição IsPartner registrada no sistema F28>
Assets/_Game/Tests/EditMode/NPC/RomanceFoundationTests.cs
docs/validation/fable_46_spec_romance_foundation_runtime_execution_report.md
```

## Contratos

### Data contracts
RomanceEligibility: tabela estática npcId→status (derivada do roster; validável pelo F30
contra o NpcRegistry). Constantes de progressão nomeadas.
### Runtime contracts
RomanceService: GetStage/CanConfess (retorna motivo de recusa enum)/Confess/Advance/Partners.
Puro onde possível (regras testáveis sem Unity).
### Event contracts
RomanceStageChangedEvent(npcId, stage); RomanceConfessionRejectedEvent(npcId, motivo).
### Save contracts
Campos aditivos por entrada da FriendshipSaveData: romanceStage(int), stageProgress(int),
confessedDay(int). Sem refs Unity; defaults = None/0.
### UI contracts
Entrada "Confessar" condicional no diálogo (padrão F28); toasts pelos eventos. Sem tela nova.

## Sistemas afetados

```text
NPC friendship (extensão), diálogo/pools (condição+entradas), save de amizade (aditivo),
event bus, presentes (multiplicador no ponto único F26).
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/NPC/Friendship/** (novos + extensão mínima do FriendshipService)
Assets/_Game/Scripts/NPC/Dialogue/** (condição IsPartner + entradas de pool)
Assets/_Game/Tests/EditMode/NPC/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manuais ; Packages/** ; ProjectSettings/**
QuestRegistry/cadeias F35 (consumir flag apenas) ; SaveManager core
NpcShopController e serviços F25 (sem perks de loja por romance no v1)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar F26/F28/F35 reais; mapear IDs dos candidatos no NpcRegistry; confirmar ausência de campo de relacionamento em NpcDefinition.
### Fase 1 — RomanceStage/Eligibility/RomanceService (regras puras) + testes de gates/limite.
### Fase 2 — Save aditivo na seção de amizade + round-trip/legado.
### Fase 3 — Diálogo: IsPartner + entrada Confessar + pools (confissão/recusa/parceiro por estágio).
### Fase 4 — Presente +50% no ponto único F26 + eventos/toasts + testes finais.
### Fase 5 — run_strict_validation + execution report + cenário humano.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs sem NPC/diálogo/save
- Must not run with: F25, F26, F28, F35, F07/F12/F21/F42
- Shared files/systems that require lock: FriendshipService/save, sistema de diálogo
- Reason: estende a seção de save de amizade e os pools de diálogo compartilhados.

## Impacto em save/load

```text
Does this change save schema? YES (campos aditivos na seção de amizade F26)
Does this add a save section? NO
Does this require migration? NO (defaults None; load legado testado)
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: YES — RomanceStageChangedEvent, RomanceConfessionRejectedEvent
Changes existing events: NO | Requires unsubscribe pattern: YES (toasts/UI)
```

## Impacto em UI/Unity

```text
Changes UI: mínimo (entrada de diálogo condicional + toasts em canais existentes)
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
(pools como dados/código conforme padrão F28; se F28 usar assets, regenerar via gerador)
Requires Play Mode final validation: YES (fluxo de confissão no diálogo real)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: divergência roster×registry (IDs). Mitigação: tabela validada por teste contra
NpcRegistry + validador F30.
Risco: exploit de presente (+50%) furar caps diários. Mitigação: multiplicador APÓS o cap
do F26, no ponto único, com teste.
Risco: decisão 2-parceiros vs canon 3 confundir specs futuras. Mitigação: constante
MaxSimultaneousPartners=2 com comentário citando FABLE_DECISOES §13.
Risco: F26/F28/F35 não executadas/divergentes na Fase 0. Mitigação: stop-and-report
(dependência pendente — BLOCKED_BY_DEPENDENCY_PENDING, não pivotar).
```

## Rollback

```text
Campos aditivos inertes sem o serviço; remover entradas de pool desliga confissão;
multiplicador de presente removível isoladamente. Amizade F26 intacta. Nenhum save apagado.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar F26/F28/F35 + IDs canônicos dos 12 candidatos (11 + Nymiriano).
- [ ] T002 — RomanceStage/Eligibility/Service (puro) + testes de gates e limite 2.
- [ ] T003 — Save aditivo (romanceStage/stageProgress/confessedDay) + round-trip/legado.
- [ ] T004 — IsPartner no F28 + entrada Confessar + pools (confissão/recusa/parceiro).
- [ ] T005 — Presente +50% pós-cap + eventos/toasts + testes.
- [ ] T006 — csproj; run_strict_validation; execution report + cenário humano.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (gates, limite, progressão, multiplicador)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (confissão via diálogo real)
- Requires regression test: YES (amizade F26: caps/níveis/round-trip intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano confessando a
  1 NPC e recebendo recusa no 3º com 2 parceiros ativos

## Definition of Done

```text
12 candidatos com eligibility canônica; confissão gated funcionando; limite 2 com recusa
falada; falas/bônus de parceiro; save aditivo com legado OK; builds 0E; report; sem
casamento; sem claim ACCEPTED.
```

## Anti-regressão

```text
F26 intacta (níveis/caps/eventos); pools F28 existentes não alterados (apenas adições);
flag F35/F36 só lida; NPCs Unavailable (Mara/Corvus/Brumdar etc.) jamais confessáveis;
Nymiriano invisível ao romance pré-Ato 3; sem checagem de gênero em código (bi por decisão).
```

## Notas para execução posterior

```text
Casamento/cerimônia, visitas à fazenda e parceiro-companion = specs futuras sobre esta
fundação. Quests de vínculo nomeadas nas fichas do roster entram na spec de casamento.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. Jogador tem apresentação M/F/NEUTRO (decisão 1.2-B, criada na fable_56). Para jogador
   neutro, o NPC nymiriano usa apresentação ambígua/etérea (proposta registrada — confirmar
   na Fase 0 antes de autorar diálogos).
```
