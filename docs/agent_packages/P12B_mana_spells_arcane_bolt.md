# P12B - SPEC 12 parte B: mana, spells e ArcaneBolt

> Package ID: P12B
> Status: Blocked
> Spec alvo: SPEC 12 - Player combat, weapons, spells, skill actions
> Depende de: P12A
> Bloqueia: P12C
> Tipo: runtime

## Objetivo

Completar o fluxo de mana e spell inicial da SPEC 12 com `ManaManager`, `SpellDataSO`, `PlayerSpellCaster` e `ArcaneBolt` real.

## Escopo

Este pacote deve implementar:

- `ManaManager` inicializado em bootstrap/scene quando aplicável.
- Mana max inicial 100.
- Mana regen 5/s.
- Save/load de mana.
- `SpellDataSO` real.
- `ArcaneBolt` real como spell inicial de teste.
- `PlayerSpellCaster` deixa de ser `Debug.Log` e executa efeito/projétil/dano real.
- Spell usa `DamageCalculator` e `DamageType.Arcane` por padrão.

## Fora de escopo

Este pacote não deve implementar:

- active slots R/T/Y/G;
- skill tree;
- bow/dodge;
- SPEC 13+;
- UI final.

## Arquivos obrigatórios para ler

```text
AGENTS.md
CLAUDE.md
memory/MEMORY.md
memory/project_skills_available.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
docs/agent_packages/P12B_mana_spells_arcane_bolt.md
docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md
docs/refinements/a_implementar/pre_refinamentos/refinamento_init_player_combat_weapons_spells_skill_actions.md
docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
```

## Arquivos obrigatórios para abrir no código

```text
Assets/_Game/Scripts/Player/ManaManager.cs
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Combat/PlayerSpellCaster.cs
Assets/_Game/Scripts/Combat/SpellDataSO.cs
Assets/_Game/Scripts/Combat/ArcaneBolt.cs
Assets/_Game/Scripts/Combat/DamageCalculator.cs
Assets/_Game/Scripts/Combat/DamageRequest.cs
Assets/_Game/Scripts/Core/Events/ManaChangedEvent.cs
```

Se algum arquivo não existir, criar somente se for necessário para este pacote.

## Arquivos permitidos para alterar

```text
Assets/_Game/Scripts/Player/ManaManager.cs
Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Combat/PlayerSpellCaster.cs
Assets/_Game/Scripts/Combat/SpellDataSO.cs
Assets/_Game/Scripts/Combat/ArcaneBolt.cs
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Core/Events/**
docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
docs/agent_packages/PACKAGE_EXECUTION_ORDER.md
docs/agent_packages/P12B_mana_spells_arcane_bolt.md
```

## Passos obrigatórios

1. Verificar que P12A está `Done` e P12B está `Ready`.
2. Confirmar branch e status local.
3. Ler arquivos obrigatórios.
4. Garantir lifecycle do ManaManager.
5. Garantir save/load de mana.
6. Criar/ajustar SpellDataSO.
7. Criar/ajustar ArcaneBolt.
8. Refatorar PlayerSpellCaster para executar spell real.
9. Usar DamageCalculator no impacto.
10. Rodar validações.
11. Atualizar audit/log/status e package order.
12. Commitar em português.
13. Parar.

## Critérios de aceite

- ManaManager existe, inicializa, regenera e salva/carrega.
- SpellDataSO existe como ScriptableObject de spell.
- ArcaneBolt existe e causa efeito real/dano real.
- PlayerSpellCaster não fica só em Debug.Log.
- Spell consome mana.
- Spell respeita cooldown.
- Unity compile passa ou NOT RUN é registrado.

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
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "ManaManager|SpellDataSO|ArcaneBolt|PlayerSpellCaster|ManaChangedEvent" -Recurse
Select-String -Path "Assets/_Game/Scripts/Combat/PlayerSpellCaster.cs" -Pattern "Debug.Log"
```

## Entrega esperada

Responder com:

- branch usada;
- arquivos alterados;
- como mana/spell/ArcaneBolt funcionam;
- validações;
- commit criado;
- se P12C está liberado ou bloqueado.
