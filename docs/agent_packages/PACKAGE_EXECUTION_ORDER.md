# PACKAGE EXECUTION ORDER

Regra: executar um pacote por vez. Não avançar para o próximo pacote sem validação e comando explícito.

| Ordem | Package | Status | Depende de | Bloqueia | Objetivo |
|---|---|---|---|---|---|
| 00 | [P00_registry_gate](P00_registry_gate.md) | Ready | Nenhuma | P12A | Corrigir o gate documental: registry, execution order, audit e log devem concordar que 12-16 ainda não estão completas. |
| 12A | [P12A_input_hands_attack](P12A_input_hands_attack.md) | Blocked | P00 | P12B | Implementar SPEC 12 parte A: input Q/E, prioridade de interação, mãos e ataque básico por equipment. |
| 12B | [P12B_mana_spells_arcane_bolt](P12B_mana_spells_arcane_bolt.md) | Blocked | P12A | P12C | Implementar SPEC 12 parte B: ManaManager em runtime/bootstrap/save, SpellDataSO e ArcaneBolt real. |
| 12C | [P12C_bow_dodge_active_slots](P12C_bow_dodge_active_slots.md) | Blocked | P12B | P12D | Implementar SPEC 12 parte C: bow range 6.0, dodge e active slots R/T/Y/G. |
| 12D | [P12D_spec12_closure](P12D_spec12_closure.md) | Blocked | P12C | P13A | Fechar SPEC 12: validação, audit, registries, status, log e checklist. |
| 13A | P13A_enemy_ai_roster_bestiary | Blocked | P12D | P14A | Criar pacote futuro para SPEC 13 somente após SPEC 12 fechada. |
| 17 | SPEC 17 UI/UX final | Blocked | 12-16 completas ou aceitas como baseline | Nenhuma | Não executar agora. |

## Estado atual recomendado

- Execute agora: `P00_registry_gate`.
- Não executar: P12A+ antes de P00 ficar Done.
- Não executar: SPEC 13/14/15/16/17 antes da SPEC 12 estar fechada.

## Como atualizar status

Após executar um pacote:

1. Se entregou tudo e validou: mudar `Status` para `Done`.
2. Se entregou parte: mudar para `Partial` e registrar pendência no próprio pacote ou PROJECT_LOG.
3. Se falhou validação: manter `In Progress` ou `Partial`.
4. Liberar próximo pacote alterando `Blocked` para `Ready` apenas se o pacote anterior estiver `Done`.
