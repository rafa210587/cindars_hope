# Town — continuação de interação e entradas, 2026-09-12

> Continuação de conteúdo: [interiores cívicos](town_civic_interiors_20260912.md), com nova cena, 23/23 entradas e comparação visual. Este relatório permanece como evidência do bugfix de interação.

## Escopo e critérios

Bugfix C-3/C-4 da `spec_town_components_doors_and_collision_v1`: preservar interação de ambos os lados quando a porta desativa seu blocker; completar entrada/reveal/saída com física real. Censo de suporte visual é verificação local dos móveis, não aprovação global C-5 ou artística. Sem promoção da spec.

## Causa e correções

`InteractionSystem` recebia callbacks por collider mas deduplicava/removia por interactable. A saída do blocker podia remover também o sensor da mesma porta. Registro e remoção agora usam collider; candidatos desativados são descartados. Sem mudar distância, input, eventos, IDs ou save. Classificação: bug preexistente exposto pelo probe, sem atribuição de autoria/regressão histórica.

O probe corrigiu dois problemas próprios: suporte-filho pode usar o renderer de seu proprietário local; uma porta aberta pela atividade NPC precisa de preparo fechado com espera limitada. Falhas por lote são preservadas e não impedem tentar os seguintes. Nenhum collider de gameplay foi removido ou redimensionado.

## Validação

- Unity 6000.5.7f1, owner único nesta tarefa. EditMode **PASS 32/32**, exit 0, log scan sem erros críticos. Filtro `InteractionSystemCandidateTests;HouseDoorInteractableTests;RoofRevealControllerTests;TownShopDoorHoursPolicyTests`, por `tools/unity/RunUnityEditModeTests.ps1`. XML/log: `Logs/town-interaction-edit-20260912.xml` e `.log`. Há warnings baseline; não é compile sem warnings.
- Revisão independente read-only: sem regressão bloqueante identificada. Pedido de reforçar teste com desativação do blocker foi incorporado antes dessa suíte.
- Auditoria independente dos artefatos finais confirmou 32/32 e 23/23, incluindo Bakery/Archive e restauração, sem falso PASS relevante para o escopo automatizado. Limites humano/visual/serviço/C-5 mantidos.
- Validação física das 23 entradas: **PASS 23/23** no r7, entre 22:59:30 e 23:01:46 UTC. [JSON real](town_all_entries_20260912_225930/town-all-entries.json); log `Logs/town-all-entries-20260912-r7.log`. Comando Unity: `-batchmode -force-d3d11 -projectPath D:/Projetos/Cindars_Hope/cindars_hope -executeMethod CindarsHope.Editor.Validation.TownAllEntriesPlayModeProbe.RunBatch -logFile Logs/town-all-entries-20260912-r7.log` (sem `-quit`; o probe encerra após finalizar evidência).
- Os 23 lotes passaram blocker fechado/sweep, animação/abertura, sweep livre, interação dos dois lados, entrada/reveal, segurança de ocupação e saída/recobrimento. Bakery e Archive passaram. **161 registros** de colliders de móveis possuem renderer visível em owner local. Registros não equivalem a 161 móveis únicos: um móvel pode ter mais de um suporte.
- `sceneUnchanged`, `savesUnchanged` e `setupRestored` verdadeiros. [Hashes dos inputs executados](town_interaction_inputs_20260912.txt). O estado da cena foi reutilizado, sem regeneração.
- Horários em PlayMode e 162 rotas da rodada anterior são evidências reutilizadas em seus escopos: nenhum código de horário/grafo/cena mudou nesta continuação. Não representam input humano nem simulação nova de 24h.

## Organização e limites

Dois executores Terra para correção isolada e probe; revisor Sol para risco do componente compartilhado. Root revisou o diff, corrigiu a lacuna do teste e executou Unity. Não há medida de custo/economia percentual.

Mudanças desta continuação: `InteractionSystem.cs`, `Tests/EditMode/Interaction/InteractionSystemCandidateTests.cs`/metas, `TownAllEntriesPlayModeProbe.cs` e este relatório. Demais arquivos dirty preservados. Sem commit/push e sem regenerar Town/Farm/Cave nesta continuação.

Human input, fidelidade artística, áudio e interiores funcionais permanecem pendentes. Nenhuma porcentagem de semelhança visual é atribuída.

## Conclusão e próximo recorte

Bugfix de interação e cobertura automatizada das entradas concluídos. O probe aciona a interação escolhida pelo seletor real e move o Rigidbody2D real; não injeta tecla E. NPCs permanecem ativos, mas isso não comprova circulação de todos durante 24h. AnimalYard continua a exceção exterior, sem interior fictício.

O PASS é **SCOPED** para C-3/C-4 e suporte local observado; não certifica integralmente C-5 (censo global), C-9 (áudio), C-10 (serviço completo), arte ou aceitação humana. As specs permanecem sem promoção. Próximo recorte: interiores de prisão, arquivo, registro e guarita coerentes com a função, preservando os acessos agora comprovados.
