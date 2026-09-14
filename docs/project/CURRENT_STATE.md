# Current State — Cindar's Hope

> Contexto primário de execução. Snapshot auditado em 2026-09-08; branch `dev`.
> O estado anterior integral está em [arquivo de 2026-09-12](../archive/CURRENT_STATE_before_efficiency_20260912.md)
> e o snapshot anterior em [2026-09-08](../archive/CURRENT_STATE_before_SOLID_AI_2026_09_08.md).

## Onde estamos

- RPG + farm sim 2D em Unity 6000.5.7f1 / C#; specs em `.specs/`.
- Foundation/Gameplay puros, Runtime/Editor/testes em assemblies explícitas; composition root,
  save providers e projections existentes. Runtime ainda é compartilhado entre domínios;
  scanner sem pares mútuos não prova ausência de ciclos ou conformidade SOLID.
- MVP extenso, mas aceitação humana final de gameplay pendente. Preservar edits concorrentes de farm,
  keyart, arte e cenas.

## Validação vigente (não reexecutada nesta edição)

| Gate | Resultado registrado |
|---|---|
| .NET | 7/7 PASS, exit 0, 22 warnings |
| Unity compile | Test Runner/batch; compile-only redundante não executado |
| EditMode | 2911/2915; quatro falhas de farm do baseline 2900/2904 |
| Fixtures selecionadas | 126/126 PASS |
| PlayMode composição | 2/2 PASS; não certifica aceitação visual/humana |
| Ratchet arquitetural | PASS; baseline permite dívida, sem aumento medido |
| Docs/strict global | Sem GLOBAL_PASS; 47 diagnósticos, 26 falsos positivos removidos |
| Menu real | FAIL: wiring de 2 picaretas, Fireball e 4 gates/4 anchors |
| PlayMode humano completo | NOT RUN |

Falhas de farm permanecem em ranges WaterEdge, escalas ponte/árvore e bridge center arável.
[Relatório de melhorias](../validation/QUALITY_IMPROVEMENTS_EXECUTION.md) delimita escopo e riscos;
SCOPED_PASS vale apenas para gates selecionados. Quality global também sinaliza três cenas já dirty.

## Trabalho SOLID_AI e fundação

InventorySlotOperations cobre add/remove/capacidade e split/move/merge/swap; fachada preserva
catálogo, aggregate/eventos, restore e IDs após consumo. SaveBackupService concentra escrita segura;
schema/recuperação intactos. Três consumidores usam `QuestRuntimeIds.SupplyQuestId`.
Contexto por classe usa `Get-ClassContext.ps1`; matriz/runners sem falso PASS; 17 testes redundantes
removidos, 3 scans no owner PowerShell, saves inválidos com estado real; Tooling95+gerador30 contratos PASS.
Specs `spec_solid_inventory_save_refactor_v1`, `spec_solid_class_context_v1` e
`spec_solid_ai_harness_hardening_v1` seguem em `a_implementar` enquanto gates estiverem pendentes.

Fundação de habilidades (2026-09-10): avatar/efeitos, transações/save v3 e cooldown por actionId
corrigidos; 131/131 EditMode, cinco cenários PlayMode e revisão sem bloqueadores. Arte provisória,
Canvas e equilíbrio global pendentes. [Evidência](../validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md).

## Farm/keyart — histórico consolidado e limites

Os incrementos v15–v20 estão integrados, com evidência específica em [v4 REPORT](../validation/farm_keyart_v4/REPORT.md),
[ambient runtime](../validation/farm_keyart_v4/ambient_runtime15/index.html), [contact v16](../validation/farm_keyart_v4/contact_v16/REPORT.md),
[boat v17](../validation/farm_keyart_v4/boat_v17/index.html), [motion v18](../validation/farm_keyart_v4/motion_v18/REPORT.md),
[herd v19](../validation/farm_keyart_v4/herd_v19/REPORT.md) e [groundcover v20](../validation/farm_keyart_v4/groundcover_v20/REPORT.md).
Os reports registram os passes selecionados: v15 33/33 + 18/18; v16 53/53 e follow-up 10/10 + 19;
v18 62/62 + Play C 60s; v19 65/65 + quatro animais/60s; v20 15/15; ambient 41s real PlayMode.

Limites não resolvidos: aceitação visual/humana e input/circulação/respawn; caminho/composição, contorno
de água, acabamento interior, emenda de grama no gate leste e full herd. Stage13 motion probe falha
direção dos frames; animator candidato não aprovado. Evidência controlada não certifica transições completas.
Material visual continua em `spec_farm_enclosed_valley_keyart_v2`; preservar trabalho de Town.

## Próximas ações

1. Reconciliar falhas farm/keyart e wiring de assets/gates sem alterar cena concorrente.
2. Resolver docs/strict sem afrouxar gates.
3. Executar `spec_validation_human_playmode_smoke_v1` em Town/Farm/Cave e registrar evidência humana.
4. Prosseguir refactors de UI, ports tipados e bootstraps legados com testes.

## Regras de continuidade

- Não reexecutar specs encerradas/fases concluídas; spec vence roadmap/refinement; conflito com este arquivo exige reconciliação.
- `features_futuras/` fica fora de automações salvo decisão humana.
- Cave stable run preserva snapshots por nível/seed; ForwardExit/BackExit não regeneram conteúdo; ler referências FASE9F antes de tocar a área.
- Preservar APIs, GUIDs, campos serializados e IDs/schema de save; wiring no composition root/installers; gameplay entre sistemas via GameEventBus; UI nova via Canvas/projection.
- Ler ADRs/game rules somente quando listados pela spec: [decisões](DECISION_LOG.md) e [game rules](../game_rules/GAME_RULES_INDEX.md).
- Decisões humanas FABLE v1/v2/v3 permanecem vinculantes nos originais referenciados pelo snapshot histórico.
- Build/teste automatizado não permite declarar aceitação humana.

## Referências

[Arquitetura implementada](../architecture/MODULARIZATION_IMPLEMENTED_ARCHITECTURE_GUIDE.md) ·
[Navegação AI](../architecture/AI_CODE_CONTEXT.md) · [Índice](DOCUMENT_INDEX.md).
`PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md` são histórico/auditoria, não leitura padrão.
