# Relatório da assembly pura de gameplay

Data: 2026-07-05

## Decisão

`CindarsHope.Gameplay` é a primeira fronteira de domínio puro além de Foundation. Ela contém apenas
decisões e estados sem dependência de Unity:

- `GameplayShortcutDecision`;
- `NpcShopInteractionSession`;
- `ThalindraQuestDialoguePolicy`;
- `QuestGiverInteractionMode`.
- `SkillActionEffectCatalog` (30 mapeamentos read-only).

O Runtime referencia Gameplay. Gameplay não referencia Runtime, Foundation, Unity ou pacotes.
Os três scripts movidos preservaram seus `.meta` e GUIDs; o enum manteve namespace e valores públicos.

## Gates

- `CindarsHope.Gameplay.asmdef`: `noEngineReferences: true`;
- teste arquitetural confirma a assembly carregada, fontes permitidas e ausência de `UnityEngine`;
- seis projetos explícitos: exit 0, 0 erros, 0 warnings;
- EditMode completa: 2.702/2.702;
- architecture ratchet: PASS;
- dependency snapshot: 1.569 arquivos, 215 edges por pasta, 49 pares mútuos, 29 tipos internal e
  3 crossings de teste.

O aumento de edges do scanner de pasta vem da introdução do módulo `Gameplay`; não surgiu novo par
mútuo. Qualquer crescimento dessa assembly exige decisão explícita e atualização do teste de escopo.
