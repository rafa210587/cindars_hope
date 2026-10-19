# FarmScene Scale and Physics Correction — Human Test Scenario

Status: `NOT RUN`.

1. Regenerate FarmScene with `CindarsHope/Inicializar Projeto`.
2. Run `CindarsHope/Validar Navegacao FarmScene`; require `Bridge corridor physics: PASS`.
3. Cross Bridge_01 north-to-south and south-to-north. The water on both sides must block movement; its centre must not.
4. Walk into Workbench, Forge and CookingStation. Each must block player movement; press the normal interaction key from its free south approach and confirm the crafting modal opens.
5. Compare the regenerated homestead closeup to the reference keyart: no farmhouse/greenhouse sprite overlap; the crafts must not be more than twice the player height.
6. Confirm flowers and clutter do not occupy the homestead, greenhouse or craft-yard two-unit clearance.

Pass only when there are no Console errors and every check passes.
