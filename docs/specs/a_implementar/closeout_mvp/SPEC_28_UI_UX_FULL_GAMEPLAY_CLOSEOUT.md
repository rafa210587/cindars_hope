# SPEC_28 — UI/UX full gameplay closeout

> Spec ID: `spec_mvp_closeout_28_ui_ux_full_gameplay_closeout`  
> Ordem: 28  
> Status: A implementar  
> Depende de: SPEC_19-27  
> Bloqueia: SPEC_29  
> Tipo: UI/UX/Runtime/Input/Validation

## /speckit.specify

Fechar a spec ativa `docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` sem criar gameplay novo.

## /speckit.plan

Ler:

```text
docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Input/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Craft/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Player/Death/**
```

Implementação esperada:

1. Auditar UI real e painéis OnGUI/debug remanescentes.
2. Fechar HUD gameplay:
   - HP;
   - hunger;
   - stamina;
   - mana;
   - gold;
   - day/time;
   - status effects;
   - mãos/equipment;
   - active skills.
3. Fechar inventory/equipment/crafting/shop/skill tree com modal stack consistente.
4. Fechar UI mínima de cave checkpoint/death/corpse/Anya se SPEC_24-26 estiverem completos.
5. Fechar pause/options básico.
6. Fechar toasts/context hints/confirmation dialogs.
7. Garantir bloqueio de input enquanto modal está ativo.
8. Não criar novas regras de gameplay.

## /speckit.tasks

- [ ] UI audit real.
- [ ] Debug/OnGUI classificados: manter apenas dev mode ou substituir.
- [ ] HUD final MVP PASS.
- [ ] Inventory/equipment/crafting/shop/skills PASS.
- [ ] Cave/death/Anya UI PASS se dependências completas.
- [ ] Pause/options PASS.
- [ ] Modal stack/input blocking PASS.
- [ ] Relatório `docs/validation/spec_mvp_closeout_28_ui_ux_full_gameplay_closeout_execution_report.md`.

## Regressão a evitar

- Não quebrar UI validada em 17C-17F.
- Não quebrar buy/sell.
- Não quebrar slot picker L.
- Não quebrar skill tree U/K.
- Não quebrar save/load.
- Não criar gameplay novo.

## Critérios de aceite

- UI/UX full gameplay MVP validado em Play Mode.
- Spec 17 original pode sair de `a_implementar` depois de promoção documental.
