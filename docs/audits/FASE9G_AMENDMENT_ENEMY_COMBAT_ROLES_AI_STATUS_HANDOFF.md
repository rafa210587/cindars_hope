# Handoff — FASE9G Amendment Enemy Combat Roles, AI, Status & Movesets

> **Data:** 2026-05-20  
> **Branch:** dev  
> **Responsável:** ChatGPT  
> **Escopo:** registrar criação do amendment de combate/IA/status/movesets da FASE9G sem sobrescrever `PROJECT_LOG.md`, que recebeu várias entradas novas de implementação em paralelo.

---

## 1. Arquivos criados

- `docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md`
- `specs/FASE9G_ENEMY_COMBAT_ROLES_AI_STATUS/spec.md`

---

## 2. Motivo

A FASE9G v1.0 definiu bestiário, faction locks, ecologia procedural e boss/miniboss candidates, mas não detalhava:

- movimentação das criaturas;
- comportamento de combate;
- status aplicados;
- tipos de dano;
- scaling por CaveLevel;
- movesets de minibosses;
- fases de bosses;
- hardening de legibilidade e balance.

Este amendment cobre essas lacunas.

---

## 3. Hardening aplicado

- Telegraph obrigatório para ataques especiais.
- Cooldown mínimo para ataques fortes.
- Status budget por tier de inimigo.
- Spawn composition budget por sala.
- Proibição de chain control inevitável.
- Fallback para criaturas aéreas quando pathing/arena não suportar voo real.
- Obrigatoriedade de usar `DamageCalculator` e status data-driven.
- Armas elementais humanoides como affixes data-driven e drop raro.

---

## 4. Decisões registradas

- Combate da cave deve ser action RPG simples, com padrões legíveis.
- Inimigos podem ter mais de um status.
- Bosses têm 3 fases na spec.
- Minibosses têm 2 mecânicas especiais.
- Status como Fear, Curse, Blind e Chill entram como IDs de contrato.
- Humanoides podem usar armas elementais e dropar versões raras/danificadas.
- Wyverns/drakes podem ser aéreos, terrestres ou híbridos conforme tipo.
- Toda criatura precisa ter MovementPattern + CombatBehavior + StatusProfile.

---

## 5. Validação

- [x] Amendment criado no repo.
- [x] SpecKit criado no repo.
- [x] Handoff documental criado.
- [ ] Unity não executado; alteração documental/spec.
- [ ] `PROJECT_LOG.md` não foi editado para evitar sobrescrever entradas paralelas recentes.

---

## 6. Próximo passo recomendado

Quando a implementação de FASE9G começar, usar os dois arquivos como baseline para:

- `EnemyCombatProfileSO`;
- MovementPattern/CombatBehavior/Status IDs;
- AI data-driven;
- boss/miniboss phase templates;
- debug de perfil de combate;
- balance futuro de status/cooldown/telegraph.
