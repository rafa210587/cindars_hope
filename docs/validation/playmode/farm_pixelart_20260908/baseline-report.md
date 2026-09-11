# Farm pixel art — baseline da cena salva

Captura realizada em 2026-09-08 local (2026-09-09 01:48:42 UTC), escopo SCOPED. Referência de seleção: `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`, linha Scene/prefab/SO/import. Esta rodada coleta baseline; não fecha a spec de aceitação humana.

- Unity Editor 6000.5.7f1: PASS, processo exit 0, compilação Tundra bem-sucedida no log.
- Scan único de log: PASS, exit 0 (`baseline-log-scan.txt`).
- Capturas: PASS, três arquivos novos 1600×1200, imagem geral inspecionada e contendo a cena.
- PlayMode, controles, física, animações e aceitação humana: NOT RUN; captura foi em Edit Mode por câmeras de diagnóstico.
- Scene preservada: SHA256 antes/depois `DCD8BED38BE0A7A07BDBF958F00A2136563AE5DE1655819D03AF83D18073036B`.
- Nenhuma regeneração, alteração de código ou salvamento de cena realizado.

O método executado foi `CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmTopDown`, com `-batchmode -quit`, gráficos Direct3D11 ativos e sem `-nographics`. Comando completo, horário e exit estão em [baseline-process.json](baseline-process.json). Inputs e snapshot de cena em [baseline-inputs.json](baseline-inputs.json) e `before_history/FarmScene.unity.snapshot`.

Evidências: [log Unity](baseline-capture-unity.log), [scan](baseline-log-scan.txt), [resultado e hashes](baseline-result.json), [geral](before/farm_capture.png), [homestead](before/farm_capture_closeup.png), [animais](before/farm_capture_animals.png). Quatro capturas históricas foram copiadas integralmente para `before_history` antes da execução. `farm_capture_colliders.png` permanece histórico e não foi produzido nesta rodada.

O log registra `Licensing::Module Error: Access token is unavailable; failed to update` na inicialização. A licença permitiu executar e encerrar normalmente; esse diagnóstico de infraestrutura não foi ocultado, mas não constituiu falha de compile/captura.

Risco residual: estas imagens não provam resultado na câmera real, sorting durante movimento, colisões, prompts ou naturalidade da animação. Gates automáticos conhecidos de farm e aceitação humana permanecem conforme CURRENT_STATE; nenhum PASS global foi alegado.
