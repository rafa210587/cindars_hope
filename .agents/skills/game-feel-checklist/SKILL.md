---
name: game-feel-checklist
description: Checklist de feedback mínimo ("juice") para interações voltadas ao player — combat hits, blocks, harvests, pickups, failures. Use para specs de combat/movement/ability (fable_27 perfect block, fable_02 weapon actions, fable_05 boss) e qualquer spec em que o player executa uma ação física.
---

# Skill: Checklist de Game Feel

Uma mecânica sem feedback parece quebrada mesmo quando a logic está correta. Este checklist define o feedback MÍNIMO por interação, usando os sistemas que já existem.

## Infraestrutura de feedback existente (reusar, não reinventar)

- Events: `PlayerActionFeedbackEvent`, `PlayerHitEvent`, `StatusAndDamageEvents`, `CombatPostureEvents` (`Core/Events/`), HUD notification events (wave_integration_08).
- Telegraphs: `EnemyTelegraphController` + `EnemyTelegraphProfileSO` — ataques de enemy se anunciam.
- Precedente de validator: `ValidateSpec14AFix2CombatFeedback`.

> **Gap de fundação conhecido: o projeto tem ZERO código de audio** (nenhum AudioSource/AudioClip em Scripts/). Os itens de audio abaixo estão DEFERRED até existir uma spec de audio foundation — mas toda spec ainda deve PUBLICAR o feedback event para que audio possa fazer subscribe depois sem tocar em gameplay code. Recomende uma spec de `audio foundation` antes das waves de polish.

## Checklist por tipo de interação

### Player attack / ability (fable_02, fable_27)

- [ ] Windup legível (animation/sprite swap ou scale pulse) antes dos active frames
- [ ] Impact: hit flash no target + knockback (path existente `KnockbackForce`) + damage number/HUD event
- [ ] Whiff feedback: errar ainda deve mostrar o swing (sem sensação de input comido)
- [ ] Recusa de cooldown/stamina expõe uma razão via HUD event — nunca um no-op silencioso (convenção do projeto: strings de `FailureReason`)
- [ ] Input buffering: presses durante recovery enfileiram a próxima action dentro de uma pequena janela (~0.15s) em vez de descartar
- [ ] **Perfect block (fable_27)**: a timing window deve ser data-tunable (campo de SO, não constante); o sucesso ganha feedback DISTINTO do block normal (flash maior + posture event); a duração do telegraph do ataque que chega define a fairness da janela

### Tomar damage

- [ ] Player hit: flash + knockback breve + reação de HUD health via `PlayerHitEvent` — nunca só um número mudando
- [ ] A invulnerability window após o hit deve ser visível (blink), não só estado interno

### Farm / interactables (crops, mining, chopping)

- [ ] Cada tool hit mostra progresso (sprite stage, shake ou particle) — o depletion guard já existe nos interactables (skill: scene-interactable-wiring)
- [ ] Yield feedback: item fly-to-inventory ou pickup pop + HUD notification event
- [ ] Target inválido/sem stamina mostra a razão (HUD event), não silêncio

### Confirmações de UI

- [ ] Purchases, crafts, quest turn-ins publicam um feedback event que o HUD pode dar toast (já é o pattern do wave_integration_08)

## Regras

1. **Feedback viaja via GameEventBus events** — gameplay publica, presentation faz subscribe (rule: unity-architecture). Nunca chame UI/VFX diretamente do combat code.
2. **Todos os valores de timing (windows, durações de flash, buffer sizes) vivem em SOs/profiles** — feel é tunado em data, recompilar mata a iteração.
3. **Visuals pooled**: damage numbers/hit particles spawnam por hit — faça pooling deles (skill: object-pooling-pattern).
4. **Fairness de telegraph**: a duração do telegraph do ataque de enemy ≥ budget de reação do player (~0.25s no mínimo); moves de one-shot de boss precisam de telegraphs mais longos e distintos (skill: enemy-ai-authoring).

## Fechamento

Itens de game-feel são Play Mode-visíveis por natureza: o human test scenario (skill: gameplay-test-scenario) deve incluir uma seção "feedback presence" listando cada item do checklist como um check. EditMode cobre as partes determinísticas (matemática da buffer window, avaliação da perfect-block window, event publicado na mudança de state).
