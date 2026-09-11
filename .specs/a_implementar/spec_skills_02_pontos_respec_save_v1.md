---
required_adrs: []
required_game_rules: []
---

# Pontos, respec e save de habilidades com estado consistente

> Spec ID: spec_skills_02_pontos_respec_save_v1
> Status: UNITY_VALIDATED — 131 testes integrados; aceitação final da wave pendente
> Wave: SKILLS_SDD_V1 | Prioridade: P0 | Domínio: Skills / Save
> Revisão: 2 — 2026-09-10
> Execução: autorizada pelo usuário; agente primário após limite do Spark
> Evidência: [relatório](../../docs/validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md)

# /speckit.specify

## Spec

O saldo deve representar todos os pontos recebidos, inclusive bônus de atos, sem recalcular o respec somente pelo nível. Compra, rank e respec só anunciam sucesso depois do commit de pontos e ouro. O snapshot de entrada do save não deve ser alterado.

- AC01: compra/rank debita o custo real uma vez; falha ou callback reentrante não altera saldo/nós nem publica sucesso indevido.
- AC02: 12 pontos gastos + 3 livres tornam-se 15 no respec; a próxima compra de custo 1 deixa 14.
- AC03: primeiro respec gratuito; seguintes custam 250. Ouro insuficiente preserva todo o estado; evento de conclusão observa carteira e pontos finais.
- AC04: duas restaurações completas do mesmo snapshot produzem o mesmo resultado, sem mutar o DTO; save normalizado não duplica restituição.
- AC05: migração segue a tabela abaixo e recusa incompatibilidade antes de restaurar qualquer seção ou trocar a cena.

Preservar a Fonte/Anya como local e gate de respec, incluindo os fragmentos existentes. Não criar botão de respec remoto gratuito. Não alterar capstones ou curvas de nível.

### Política versionada

| Entrada | Resultado |
|---|---|
| v0/v1 sem ranks | um rank e um ponto gasto por ID único |
| v2 com ranks | rank limitado a 1..AbsoluteRankCap; custo histórico inferido em um ponto/rank |
| v3 | SpentPoints preserva custo de compra + upgrades; valor ausente/inferior ao rank usa inferência legada |
| Duplicata | última entrada vence; nunca somar duplicatas |
| ID desconhecido | remover e restituir uma vez o custo normalizado |
| Coleções/entradas nulas | tratar como ausentes |
| RespecCount negativo | normalizar a zero |
| Versão maior que 3 ou ledger acima de int.MaxValue | rejeitar antes de qualquer apply ou troca de cena |

Não inventar custo histórico não persistido em saves antigos.

# /speckit.plan

## Plan e ownership

Modificar:

- Assets/_Game/Scripts/Skills/SkillTreeManager.cs
- Assets/_Game/Scripts/Skills/SkillPurchaseService.cs
- Assets/_Game/Scripts/Skills/SkillRespecService.cs
- Assets/_Game/Scripts/Skills/SkillTreeState.cs
- Assets/_Game/Scripts/Skills/SkillTreeSaveData.cs
- Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs
- Assets/_Game/Scripts/Fonte/FonteInteractable.cs
- Assets/_Game/Scripts/UI/Locations/AnyaFountainMenu.cs
- Assets/_Game/Scripts/Save/SaveManager.cs

Criar Assets/_Game/Tests/EditMode/Skills/SkillPointTransactionTests.cs.

Reusar services e providers existentes. Validar em candidato copiado, debitar ledger real, substituir estado e recomputar antes de eventos de sucesso. Guard de reentrância permanece ativo durante callbacks. Respec coordena callback da carteira nos dois callers existentes antes de SkillTreeRespecCompletedEvent.

Progressão clona os campos e listas antes de normalizar o input. Save de skills v3 adiciona somente SpentPoints, tipo simples. SkillTreeState.ValidateSaveLedger executa a mesma normalização em candidato para preflight; SaveManager chama antes de iniciar transição e antes do primeiro restore. A ordem válida dos providers permanece.

# /speckit.tasks

## Tasks

| Estado | Task | Critérios |
|---|---|---|
| [x] | T01: consultas puras de compra/rank, sem eventos nem mutação | AC01 |
| [x] | T02: commit coordenado e guard de reentrância | AC01 |
| [x] | T03: respec por saldo + gasto; carteira antes do evento | AC02, AC03 |
| [x] | T04: clone do DTO, v3 e migração explícita | AC04, AC05 |
| [x] | T05: preflight antes de qualquer restore/transição | AC05 |
| [x] | T06: testes determinísticos com snapshot e observação de eventos | AC01–05 |
| [x] | T07: fechamento técnico integrado e revisão final, sem promoção da wave | AC01–05 |

## Validação

Rodada foundation-editmode-r4.xml: 46/46 testes de Skills PASS, incluindo custos não unitários, respec, callback de ouro real, reentrância, restore repetido, duplicatas conflitantes e rejeição de versão/overflow antes de alterar até a primeira seção (dia).

A recusa do load pela API pública de arquivo e troca de cena tem revisão estática, sem teste de gravação no save do usuário. Não alegar atomicidade global para falhas de outros providers: o preflight desta fatia protege a seção de skills. Gates finais e riscos no relatório; sem promoção automática.



## Dependências de execução

Ordem de execucao: fundação serial antes de dados/readiness e UI.
Depende de: contratos atuais de avatar, progressão e save, já existentes; nenhuma decisão de capstone.
Bloqueia: fechamento integrado de UI e equilíbrio do lote SKILLS_SDD_V1.

