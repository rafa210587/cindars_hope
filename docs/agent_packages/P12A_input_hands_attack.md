# P12A - SPEC 12 parte A: input, mãos e ataque

> Package ID: P12A
> Status: Blocked
> Spec alvo: SPEC 12 - Player combat, weapons, spells, skill actions
> Depende de: P00
> Bloqueia: P12B
> Tipo: runtime

## Objetivo

Implementar a base real da SPEC 12 para input Q/E, prioridade de interação, uso de LeftHand/RightHand e ataque básico via EquipmentManager + DamageCalculator.

## Escopo

Este pacote deve implementar:

- Q usa `EquipmentSlot.LeftHand`.
- E interage se houver `IInteractable` válido no alcance.
- E usa `EquipmentSlot.RightHand` se não houver interagível.
- Ataque desarmado quando a mão não tiver arma/tool usável para ataque.
- Ataque com arma equipada quando houver weapon/equipment compatível.
- PlayerCombatController usa `EquipmentManager` como fonte de mãos.
- PlayerCombatController usa `DamageCalculator` como pipeline de dano.
- Stamina cost respeita `StaminaManager`.
- Uso normal de arma/tool não depende de `SkillActionSO`.

## Fora de escopo

Este pacote não deve implementar:

- spells;
- ArcaneBolt;
- SkillActionSO;
- active slots R/T/Y/G;
- bow avançado;
- skill tree;
- SPEC 13+;
- UI final.

## Arquivos obrigatórios para ler

```text
AGENTS.md
CLAUDE.md
memory/MEMORY.md
memory/project_skills_available.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
docs/agent_packages/P12A_input_hands_attack.md
docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md
docs/refinements/a_implementar/pre_refinamentos/refinamento_init_player_combat_weapons_spells_skill_actions.md
docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md
docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
docs/specs/implementados/spec_hunger_stamina_status_balance.md
```

## Arquivos obrigatórios para abrir no código

```text
Assets/_Game/Scripts/Player/PlayerCombatController.cs
Assets/_Game/Scripts/Player/StaminaManager.cs
Assets/_Game/Scripts/Player/PlayerManager.cs
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Equipment/EquipmentSlot.cs
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Combat/DamageCalculator.cs
Assets/_Game/Scripts/Combat/DamageRequest.cs
Assets/_Game/Scripts/Combat/DamageResult.cs
Assets/_Game/Scripts/Interaction/IInteractable.cs
Assets/_Game/Scripts/Core/Events/**
```

## Arquivos permitidos para alterar

```text
Assets/_Game/Scripts/Player/PlayerCombatController.cs
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Interaction/**
docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
docs/agent_packages/P12A_input_hands_attack.md
```

## Passos obrigatórios

1. Verificar que P00 está `Done` e P12A está `Ready`.
2. Confirmar branch `dev` e `git status`.
3. Ler spec/refinement da SPEC 12.
4. Ler specs implementadas de 09, 10, 11.
5. Ler arquivos de código obrigatórios.
6. Implementar roteamento Q/E.
7. Implementar prioridade E para interação.
8. Implementar fallback E para RightHand.
9. Implementar Q para LeftHand.
10. Refatorar ataque para usar mão/equipment quando aplicável.
11. Garantir DamageCalculator no pipeline.
12. Garantir stamina cost.
13. Rodar validações.
14. Atualizar audit/log/status e package order.
15. Commitar em português.
16. Parar.

## Critérios de aceite

- Q chama ação/ataque da LeftHand.
- E chama interação quando há interagível.
- E chama RightHand quando não há interagível.
- Ataque com mão vazia funciona como unarmed.
- Ataque com arma/equipment usa dados do equipamento quando disponíveis.
- DamageCalculator é usado.
- StaminaManager é respeitado.
- Não há spell/skill action acoplada indevidamente ao uso normal de arma/tool.
- Unity compile passa ou NOT RUN é registrado com motivo real.

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

## Checks finais

```powershell
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "KeyCode.Q|KeyCode.E|LeftHand|RightHand|IInteractable|DamageCalculator" -Recurse
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "GameObject.Find|FindObjectOfType" -Recurse
```

## Entrega esperada

Responder com:

- branch usada;
- arquivos alterados;
- como Q/E foram implementados;
- validações;
- commit criado;
- se P12B está liberado ou bloqueado.
