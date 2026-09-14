# Town — prioridades executadas em 2026-09-12

Status: entrega parcial; sem promoção de C1/N1 ou aceite visual humano.

> Continuação vigente: [correção de interação e entradas r7](town_interaction_followup_20260912.md) — 23/23 entradas PASS e 32/32 EditMode. As falhas r1–r6 abaixo ficam preservadas como histórico de diagnóstico.

Este relatório consolida apenas as mudanças e evidências desta rodada. Relatórios antigos permanecem históricos e não comprovam o estado integrado atual.

## Mudanças

- Grafo: quatro ligações físicas locais para as rotinas de Hund/Gurd. A seleção do destino de uma rota normal aceita nó residencial não-safe; recovery/snap continua restrito a nós safe. Sem alteração de IDs ou schedules.
- Nove lojas recebem política física de horário pelo resolver de disponibilidade existente. A porta compartilhada com Farm recebe política opcional; portas sem política mantêm o comportamento existente. Saída interna permitida fora do horário, fechamento protegido por ocupação e passagem automática de NPC preservada.
- TownScene regenerada pelo gerador, preservando o modelo de 23 interiores na mesma cena e AnimalYard externo. Sem edição manual de YAML.
- Novo probe de entradas usa corpo real do Player, seletor real de interação e movimento físico; NPCs continuam ativos. O acionamento é por chamada da interação selecionada, não por teclado.

## Evidência verificada

Unity 6000.5.7f1. Raiz foi o único executor Unity neste projeto.

| Verificação | Resultado | Artefato em Logs/ |
|---|---|---|
| Gerar Town integrada | PASS, processo exit 0 | town-priorities-regen-20260912.log |
| Rotas finais, 162 pares | PASS, 0 erros/avisos; pior 2,9x | town-priorities-detour-final-20260912.log |
| Primeira suíte EditMode | 28/29; falha de rota posteriormente corrigida | town-priorities-edit-20260912.xml |
| Suíte rota após fix | 12/13; teste de compaction teve assert corrigido, rerun pendente | town-priorities-route-fix-20260912.xml |
| Horários em PlayMode | PASS, 1/1 cenário | town-shop-hours-play-20260912.xml |
| Suíte EditMode final integrada | PASS, 40/40; exit 0 e log scan PASS | town-priorities-final-edit-20260912.xml |
| Entradas r6 | FAIL; sete ciclos físicos completos, interrompido no oitavo lote | town-all-entries-20260912-r6.log |

O cenário de horários verifica nove políticas materializadas e exercita a padaria às 08h/12h, feedback, abertura pelo sensor NPC, saída interna e fechamento seguro após desocupação. Usa posicionamento controlado dos atores; não prova input nem caminhada da rotina NPC. Suprime diagnósticos baseline via LogAssert durante o cenário: não representa ausência global de erros. Snapshot de arquivos persistentes permaneceu igual.

Comandos: `CreateMvpTownScene.CreateScene`; `ValidateTownRouteDetourRatio.Validate`; runner EditMode com filtros registrados nos logs; PlayMode `-testFilter TownShopDoorHoursPlayModeTests`. Probes de entrada usam `TownAllEntriesPlayModeProbe.RunBatch`, sem `-quit`.

## Pendências e limites

- Entradas: r1 falhou no preparo de boot; r2 na seleção; r3/r5 na aproximação excessiva; r4 teve erro de cast Vector3/Vector2 no probe, corrigido. Medida da cena: blocker Temple começa em y=18,49 e topo do collider Player fica 1,125u acima dos pés, portanto pés param corretamente em ~17,35; o alvo 18,29 era inalcançável. R6 usa o seletor real para encerrar a aproximação. Nenhum resultado anterior prova 23/23 entradas. Resultado final será registrado abaixo.
- NPCs: o validador final mede caminhos no grafo e física da cena; não é uma simulação nova de 24h nem prova ausência de congestionamento dinâmico.
- Interiores: vários prédios especiais ainda usam cama/mesa genéricas. Priorizar prisão, arquivo, registro e guarita com composição funcional usando assets existentes, mantendo corredor e colliders. Não houve nova autoria de arte nesta rodada.
- Horários: serviço comercial não foi alterado; a disponibilidade da porta reutiliza seu resolver. Compra/venda não foi exercitada neste cenário. Um jogador pode acompanhar uma porta aberta por NPC: controle de permissão de travessia não foi introduzido.
- FarmScene/CaveScene e saves não foram modificados intencionalmente. Testes compartilhados de porta/reveal estão na suíte EditMode, mas não houve PlayMode novo da Farm.
- Aceite visual, áudio e a cobertura global das specs continuam pendentes. Não atribuir percentual de fidelidade novo sem revisão de imagens.

## Preservação

Cena original deste turno copiada para `C:/Users/Rafa/AppData/Local/Temp/town-review-baseline-65fec38b44384248985568a6336a9bc2/TownScene.unity`, SHA256 `EF32C759CA8616379C17B15A2A867328A43EED5EF8D4F681122106F7D784A5DA`.
Cena integrada: SHA256 `A596653704F27296E2085F0C0CCE4D2594C1E30D271478DE756D8FF2EA87A8E7`.
Outras alterações simultâneas de documentação/harness permanecem fora do escopo. Nenhum commit ou push.

## Resultado final do probe r6

Evidência: [JSON real](town_all_entries_20260912_224529/town-all-entries.json). **FAIL, não 23/23 PASS.**

Temple, Chamber, Prison, Manor, Registry, Archive e MarketHall tiveram abertura, sweep livre, entrada, reveal, interação interna e recobrimento após saída medidos como verdadeiros. Bakery abriu e revelou, mas o seletor interno retornou null, candidatos vazios, embora a distância ao sensor fosse zero. Os quinze lotes seguintes não foram executados. Não interpretar campos default desses casos como falhas físicas medidas.

Os sete lotes concluídos também não receberam PASS global: o censo de móveis exige renderer descendente de cada collider e reprova suportes-filhos cuja arte está no pai imediato. É necessário distinguir esse falso negativo de um sólido realmente invisível sem aceitar qualquer renderer da casa como justificativa. Archive foi encontrado aberto por atividade NPC, portanto sua precondição de bloqueio fechado não foi comprovada nessa execução.

Cena, saves e setup permaneceram iguais após o probe (`sceneUnchanged`, `savesUnchanged`, `setupRestored` true). A captura inicial contou seis corpos sólidos NPC já ativos; não equivale a comprovar todos os NPCs spawnados ou uma simulação de 24h.

Próxima prioridade concreta: investigar o registro/desregistro da interação da padaria quando solid blocker e sensor compartilham o mesmo interactable; corrigir o censo de suportes para o vínculo local real; depois completar a cobertura restante. Evitar novas execuções cegas ou alterar colliders para fabricar aprovação.

Identificação dos fontes/testes: [SHA256 dos inputs](town_priorities_inputs_20260912.txt). Os testes de runtime não foram invalidados por alterações posteriores somente no probe Editor. A revisão independente do ajuste Safe não encontrou finding de código; não substitui os limites PlayMode acima.
