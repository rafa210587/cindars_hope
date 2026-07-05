# Fase 8 — UI e legado

Data: 2026-07-05
Branch: `dev`

## Estado verificado

- os `Update()` vazios das views HUD foram removidos na Fase 1;
- `GameplayHudTextOverlay` já exibe o prompt de interação em Canvas, alimentado por evento e dirty
  state, substituindo funcionalmente o antigo `ContextHintController.OnGUI`;
- shop e inventory já possuem ViewModels/projections e validação pura; a regra compartilhada proíbe
  novo `OnGUI`, polling vazio e pipelines paralelos;
- o código antigo não foi apagado: a busca não encontrou referência serializada ao
  `ContextHintController`, mas o gate do plano também exige PlayMode antes de remover legado.

## Dívida mantida de propósito

Existem outros `OnGUI` em telas antigas. Migrá-los em massa sem specs visuais, scene wiring e smoke
PlayMode seria um rework de UI observável, não uma limpeza segura. Eles ficam inventariados e novos
usos ficam proibidos pela rule `unity-architecture` sincronizada em `.codex` e `.claude`.

## Validação e bloqueio de ambiente

- suíte EditMode: 2.593/2.672 PASS; 79 falhas conhecidas, zero novas contra o baseline de 81;
- build/smoke Windows: PASS;
- scanner abriu Farm, Town e Cave e encontrou 0 missing scripts;
- PlayMode batch: 2/2 PASS. A causa do bloqueio era `PlayModeStartSceneSetter`, que substituía a
  `InitTestScene` do runner pela Farm durante `-runTests`. O setter agora ignora processos de teste.
  O smoke carregou Farm, Town e Cave, confirmou a cena ativa e varreu toda a hierarquia por missing
  scripts. Evidência: `TestResults/modularization-phase8-playmode.xml`.
