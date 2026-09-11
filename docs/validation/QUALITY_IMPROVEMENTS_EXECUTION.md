# Melhorias executadas — organização, SOLID e validação

Data: 2026-09-08. Continuação autorizada da auditoria do projeto e da revisão de testes.
Status: CODE_COMPLETE, com falhas globais e aceitação humana pendentes.

## Resultado e escopo

- InventorySlotOperations agora concentra capacidade, Add/Remove e as operações de
  split/move/merge já extraídas. InventoryManager conserva estado, catálogo, aggregate e eventos.
- InventoryPanelController compartilha prontidão de uso e captura o ID antes do consumo,
  corrigindo a mensagem vazia da última unidade. Preservados handlers e ownership de drop.
- ValidationReport/runner/menu distinguem erro, aviso e configuração ausente; exceptions
  não interrompem os demais validators. Resumo mantém as causas reais.
- Transições, farm e agenda da cidade preservam ownership das cenas carregadas; inspeção
  não salva assets. City Schedule deixou de usar OpenScene(Single).
- Cortados 17 testes tautológicos/repetidos e 3 scans duplicados do Unity. Scans continuam
  obrigatórios no ratchet PowerShell; 2 provas de assemblies compiladas permanecem.
  Quatro casos fracos de save agora verificam estado real preservado.
- Uma matriz canônica seleciona gates por risco. Removidos builds/docs repetidos por
  agente/fase, assemblies históricas fixas e instruções incompatíveis com os runners.
  Runners verificam versão, lock, timeout, exit, XML novo/positivo e filtro.
- Guard de scope recusa arrays nulos e reports inexistentes. Ratchet verifica baselines
  válidas, dívida, 24 GUIDs e referências a assemblies predefinidas.

O trabalho anterior desta sessão também entregou regra/skill SOLID, escrita segura
centralizada em SaveBackupService, IDs canônicos e contexto seletivo Roslyn por classe.
O objetivo é reduzir responsabilidades e dependências ocultas, sem quota de interfaces
ou obrigação de aplicar um pattern onde não existe variação.

## Contexto de classe para AI

Frontmatter manual em toda classe duplicaria nomes, dependências e métodos que mudam
com o código. Isso aumenta manutenção e pode fornecer contexto desatualizado.
A alternativa implementada é XML curto para intenção/invariantes não óbvias e
`tools/architecture/Get-ClassContext.ps1 -Type InventoryManager` para fatos estruturais.
Não há medição que permita prometer economia percentual de tokens.
Consultar [AI_CODE_CONTEXT](../architecture/AI_CODE_CONTEXT.md) e a regra
`solid-and-ai-context`; manter `CURRENT_STATE.md` como entrada curta.

## Evidência integrada

Pasta principal: `TestResults/quality-improvements/20260908-003712/`.
Root inspecionou XML, failures, resultados/processos e logs; revisão independente
inspecionou o diff. Narração delegada não foi usada como única prova.

| Gate | Resultado |
|---|---|
| EditMode completo | **FAIL: 2911/2915**, mesmas quatro falhas de farm do baseline 2900/2904 |
| Fixtures alteradas | **126/126 PASS**, extraídos do XML completo; sem segunda execução desnecessária |
| .NET | **PASS: 7 projetos**, uma invocação do grafo, exit 0, 22 warnings/0 erros |
| PlayMode composição | **PASS: 2/2**, Unity exit 0 e scanner Tests PASS |
| Corruption + architecture | **SCOPED_PASS**, exit 0; baseline de dívida não aumentada |
| Diff + quality da tarefa | **SCOPED_PASS**, manifesto explícito de 151 arquivos da sessão; trabalhos concorrentes excluídos |
| Tooling | **95 contratos PASS** (strict31, Unity25, scope22, ratchet17); fake executable/fixtures, não Unity real |
| Docs global | **FAIL: 47 diagnósticos**; detector deixou de marcar 26 variáveis PowerShell legítimas como placeholders |
| Menu batch completo | **FAIL**, exit 1; duas picaretas sem WeaponId, Fireball sem prefab e 4 gates/4 anchors canônicos ausentes |
| PlayMode humano / build Player | **NOT RUN**; cenário humano documentado, sem mudança de target/build/packaging |

O runner de EditMode retornou1 após Unity2 e não alcançou seu scanner embutido.
O XML/log foram inspecionados; isso comprova execução dos testes e compile Editor,
não scannerPASS para essa invocação. PlayMode teve scanner executado.
Os dois últimos ajustes de ownership/config em validators Editor ocorreram depois da
suíte ampla; o batch posterior comprovou compile desses ajustes. Runtime, testes,
assemblies e assets pertinentes ao full/.NET/PlayMode não mudaram depois.

As quatro falhas são WaterEdge, escala da ponte, range de árvore e bridge center arável.
Nenhuma expectativa foi relaxada; farm/keyart/arte/cenas de trabalho concorrente preservados.
PlayMode fez migração incidental de ProjectSettings; snapshot restaurado byte a byte:
`4c1a9ac0195aa73ab48eafd226cae67f71812a79ec61bef7d1ae6c347bc02c8c`.
Nenhuma mudança restante de settings foi aceita como parte do refactor.

## Custos observados

Baseline: XML 9,468 s/processo 45,098 s. Final: XML 6,661 s/runner 26,170 s.
Build de grafo 13,288 s; PlayMode 21,010 s. Cache/import/configuração influenciam duração:
esses runs não são benchmark controlado nem provam aceleração percentual causada pelos cortes.
A suíte cresceu de2904 para2915 casos porque as novas proteções úteis superam os cortes;
reduzir o número total de testes não é o objetivo.

## Limites e melhorias seguintes

1. Resolver contratos de farm com o trabalho ativo de layout/keyart; não apagar testes.
   Reconciliar também wiring de picaretas/Fireball e gates/anchors canônicos com os
   componentes legados ainda presentes. Ausência dos componentes canônicos não prova,
   isoladamente, que toda transição de gameplay esteja quebrada.
2. Reconciliar47 diagnósticos documentais de nomes/headers/markers/ADRs nas specs legadas
   e concorrentes. Nenhuma baseline foi elevada para ocultá-los.
3. Executar o [cenário humano agrupado](playmode/quality_improvements_human_test_scenario.md)
   e o smoke MVP já previsto. Automação de composição não certifica UX/visual.
4. A UI ainda usa owners concretos/service locators legados. Próximos cortes exigem
   consumidores e contratos reais, especialmente ports object/casts e bootstraps.
5. Substituir testes estruturais de HUD/reputação por comportamento antes de cortar.
   Save/recovery, economia, EventBus e cave snapshot/replay mantêm suas proteções.

Preview fixtures provam isolamento da inspeção, inactive/duplicates e manutenção do
estado observado. Não exercitam toda abertura/fechamento por caminho existente nem
forçam a cena original a dirty. Ownership da aquisição teve revisão estática e
execução batch separada; inspeção visual do Editor ainda consta do cenário.
A revisão não certifica todo o código como aderente a SOLID.

## Relatórios e comandos

- [Inventário e ações](spec_inventory_operations_and_action_readiness_v1_execution_report.md).
- [Validação e limpeza de testes](spec_validation_outcomes_and_test_cleanup_v1_execution_report.md).
- [Harness/tooling](spec_validation_efficiency_harness_v1_execution_report.md).
- [Auditoria original](SOLID_AI_PROJECT_AUDIT.md) e [revisão de testes](TEST_VALIDATION_EFFICIENCY_REVIEW.md).
- XML/comandos/fingerprints em `TestResults/quality-improvements/20260908-003712/`.
- `TestResults/quality-improvements/final/static-gates.log`:
  `powershell.exe -File tools/docs/run_strict_validation.ps1 -Gates corruption,architecture`.
- `TestResults/quality-improvements/final/docs-global.log`:
  `powershell.exe -File tools/docs/run_strict_validation.ps1 -Gates docs`.
- Contratos e receipts verificáveis em `TestResults/quality-improvements/tooling/`.
- Manifesto `TestResults/quality-improvements/final/task-scope.json`; resultado em
  `scoped-final.log`: `run_strict_validation.ps1 -Gates corruption,architecture,diff,quality -ScopePath TestResults/quality-improvements/final/task-scope.json`.
  Inclui entregas anteriores SOLID desta sessão e continuação; 151 paths não significam
  151 classes refatoradas, pois há instruções canônicas e paridade gerada.

Batch adicional: `TestResults/quality-improvements/20260908-004245/validators21.log`,
Entrypoint: `CindarsHope.Editor.CindarsHopeMenu.ValidateProjectBatch`; argumentos
exatos e versão do executável constam no JSON de comando junto ao log.
21 passos executados, City Schedule 21/21 checks PASS; fingerprints inalterados,
Unityexit1/wall 10,729 s. O resumo tem3 mensagens de erro agregadas, não3 defeitos:
Combat3 erros +Projectile1 repetindoFireball +Transitions8. Três mensagens warning
incluem um bloco com59 avisos de art profiles e o aviso de escala de boss com seu summary.
Não gerar/reparar assets só para tornar esse resultado verde.

O passo de projéteis permanece: embora repita o diagnóstico de Fireball, também
verifica ProjectileBehaviour, Rigidbody2D, Collider2D e isTrigger nos prefabs.
O validator de databases não cobre essas verificações. Remover o passo inteiro
perderia proteção; a deduplicação futura deve tratar somente checks equivalentes.

Paridade Codex regenerada: config SHA256 antes/depois
`6F369E1652B349C0B19AD365C038ADE22D65FA2BBD293C3C53FC4A39152A39C4`;
bloco manual de AGENTS preservado. Contratos do gerador30/30 PASS.
Receipts de tooling recuperam stdout/exit dos eventos originais; não foram rerodados
apenas para produzir logs. A primeira tentativa do contrato do gerador não encontrou
Get-FileHash por PSModulePath herdado; corrigido ambiente do filho e contrato passou.

Gates selecionados têm nome/escopo explícitos e não produzem GLOBAL_PASS.
Specs permanecem em `a_implementar`; nenhum commit, push, merge ou promoção nesta rodada.
