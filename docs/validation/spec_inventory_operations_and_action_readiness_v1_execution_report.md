# Execution report — inventory operations and action readiness

Data: 2026-09-08. Spec: `spec_inventory_operations_and_action_readiness_v1`.
Status: CODE_COMPLETE; evidência integrada e limitações no relatório
[QUALITY_IMPROVEMENTS_EXECUTION](QUALITY_IMPROVEMENTS_EXECUTION.md).

validated_adrs: []
validated_game_rules: []

## Acceptance criteria extracted

Distribuição de Add preenche pilhas existentes antes de vazios; Remove retira do fim;
recusa não produz mutação parcial; bindings/equipados e índices preservados.
Fachada mantém catálogo, aggregate e um evento após sucesso.
Slot adulterado com aggregate desatualizado não permite retirada parcial.
Botão e execução compartilham prontidão; feedback conserva ID após consumir última unidade.

## Existing systems audit

Reusados InventorySlotOperations, InventoryAddResult e owners ItemUseManager/InventoryManager.
Sem inventário paralelo, fachada de repasse, novo bus ou interfaces sem consumidor.
Core recebe slots/valores, não manager. Policy de Use decide precedência; UI traduz mensagens.
Preservados schema, GUIDs existentes, serialized fields, lifecycle e input.
Clear/restore e consumo por ItemId continuam com seus contratos legados.

## Spec Compliance Matrix

| Critério | Implementação | Proteção |
|---|---|---|
| Add/capacidade | InventorySlotOperations; delegação no manager | Distribuição, limites, metadata, recusa e fachada |
| Remove | Pré-checagem real, retirada reversa | Insuficiência sem mutação; binding parcial/esgotado |
| Integração | Aggregate/eventos no manager | Subscriber observa slots e aggregate consistentes |
| Corrupção exposta | Recusa quando slots têm menos que aggregate | Teste de fachada altera Slots e verifica false/zero eventos |
| Use | InventoryItemActionPolicy + ID capturado | Última unidade, handler recusa, seleção/serviço/item indisponível |
| Drop | Owner existente, seleção por valores | Sem spawner: quantidade conservada e feedback de falha |

## Validation

Caracterização antes da extração: `TestResults/quality-improvements/20260908-002412/`.
42 casos, 37 PASS; um reproduziu `Used .` em vez de `Used fixture_consumable.`.
Quatro falharam no SetUp de cenas por cena TestRunner untitled; fixtures corrigidas
para preview scenes. Duas tentativas anteriores de compile detectaram API SceneHandle
obsoleta e atributo NUnit indisponível; corrigidas, logs preservados.

Testes novos verificam regras/efeitos observáveis, sem duplicar toda a matriz pura na fachada.
Revisão independente do runtime não encontrou regressão nos fluxos válidos; recomendação
de teste do aggregate desatualizado foi implementada.
Execuções finais, comandos, exits, fingerprints e XML estão no relatório integrado:
40/40 casos de InventorySlotOperations, InventorySlotMoveMerge e InventoryItemAction PASS
dentro do full 2911/2915; quatro falhas de farm são idênticas à baseline.
`run_strict_validation.ps1` usa gates selecionados pela matriz; SCOPED_PASS não é global.
Existing tests executed: YES
Test evidence: TestResults/quality-improvements/20260908-003712/final-editmode.xml
Test result: PASS nos40 casos de inventário; FAIL na suíte completa pelas4 falhas preexistentes.

## Honest status rationale

Código concluído; não declarar aceitação humana ou aprovação global.
Endurecimento de Remove é intencional e limitado à insuficiência real. Não corrige
silenciosamente aggregate adulterado nem promete recuperar qualquer corrupção externa.
Chamadas diretas legadas da UI e demais responsabilidades do controller seguem dívida;
esta entrega não transforma toda a classe em adapter fino.
Roteiro humano complementar no cenário integrado; cenário escrito não é execução.
