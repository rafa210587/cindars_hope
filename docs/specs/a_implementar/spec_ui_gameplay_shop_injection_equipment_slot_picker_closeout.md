# SPEC 17D - Fix Shop Injection + Equipment Slot Picker UX

**Status:** Implementado em codigo - Unity e Play Mode humano pendentes
**Ordem de execucao:** 17D
**Depende de:** SPEC 17C implementada em codigo
**Bloqueia:** fechamento humano final da UI gameplay

## /speckit.specify

Corrigir a inicializacao verificavel de `ShopManager`, `BuyPanel` e `SellPanel` em `TownScene` e transformar o painel de equipamento aberto por `L` em um seletor por slot que usa o inventario para escolher somente itens compativeis.

## /speckit.plan

- Endurecer o contrato runtime de shop sem busca global ou fallback silencioso.
- Adicionar validator Editor-time para wiring e dados dos shops em `TownScene`.
- Manter `K` para atributos, `I` para inventario normal, `U` para skill trees e `L` para equipamento.
- Permitir `Equipar`/`Trocar`/`Desequipar` por `Chest`, `RightHand`, `LeftHand` e `Accessory`.
- Abrir inventario em modo selecao para um slot alvo e retornar ao painel `L` ao selecionar ou cancelar.
- Identificar binding de inventario por slot de equipamento, evitando limpar multiplas copias por `itemId`.

## /speckit.tasks

- [x] Registrar evidencia anterior em `docs/validation/SPEC17D_REPRO_BEFORE_20260526.md`.
- [x] Validar e endurecer injecao shop buy/sell.
- [x] Implementar selector de equipamento por slot e filtro de compatibilidade.
- [ ] Validar missing scripts, compile Unity e shops em batchmode quando a instancia Unity liberar o projeto.
- [x] Registrar evidencias automaticas e pendencias de Play Mode.
