# FINAL_HUMAN_VALIDATION_CHECKLIST — Cindar's Hope

Este arquivo é um template para a validação humana final após a SPEC 17.

## Pré-condições

- Branch local com todas as specs finalizadas.
- Unity abre sem erros de compilação.
- `tools/docs/validate_docs.ps1` passou.
- `tools/unity/RunUnityCompileValidation.ps1` passou ou `NOT RUN` está formalmente registrado.

## Fluxo humano end-to-end

1. Abrir BootScene.
2. Iniciar/continuar jogo.
3. Validar Farm: mover, plantar, irrigar, colher, fome/stamina, inventory.
4. Validar Town: entrar, falar com Pip, comprar, vender, diálogo, NPC ambulante.
5. Validar Craft: abrir workstation, iniciar craft, cancelar, coletar output.
6. Validar Equipment: equipar/desequipar, repair, drop, save/load.
7. Validar Combat: melee, ranged, spell, mana, dodge, active slots.
8. Validar Enemy AI: tipos diferentes, telegraph, bestiary, XP/drop.
9. Validar Cave: geração, stable run, checkpoints, boss gate, resource nodes.
10. Validar Death/Anya/Corpse: morrer, recuperar corpse, respec, save/load.
11. Validar UI: HUD, hotbar, modals, pause/options, confirmations, toasts.
12. Salvar, fechar Unity, reabrir e confirmar estado restaurado.

## Resultado

```text
Passed: YES/NO
Bugs encontrados:
Evidências:
Decisão final:
```


---

## SPEC 17A — Visual Scale Rebaseline

Validar manualmente:

- [ ] FarmScene está em mapa 4x área, sem scale global de tilemap.
- [ ] Player/NPCs estão visualmente maiores conforme baseline decidido.
- [ ] Criaturas Tiny/Average/Boss aparecem com categorias de tamanho coerentes.
- [ ] Colliders usam footprint/pé e não o sprite inteiro.
- [ ] Câmera e bounds funcionam nos mapas maiores.
- [ ] Context hints, health bars e damage popups têm offsets aceitáveis.
- [ ] Cave spawn não coloca criatura grande em corredor/sala inválida.
