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

- suíte EditMode: 2.591/2.670 PASS; 79 falhas conhecidas, zero novas contra o baseline de 81;
- build/smoke Windows: PASS;
- scanner abriu Farm, Town e Cave e encontrou 0 missing scripts;
- PlayMode batch: BLOCKED. Com `-testPlatform PlayMode`, filtro e assembly explícita, o Unity cria
  `InitTestScene`, entra em `FarmScene`, mas não inicia o teste e não escreve XML. O processo de
  validação foi encerrado e a cena temporária removida. Isso impede remoções de legado e a afirmação
  de smoke PlayMode das três cenas, mas não invalida os gates compilados/EditMode/standalone.
