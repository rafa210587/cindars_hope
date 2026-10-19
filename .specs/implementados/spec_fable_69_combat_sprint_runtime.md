# SPEC — Player: Sprint em Combate (velocidade mantida ao custo de stamina)

> **Spec ID:** `fable_69_spec_combat_sprint_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 11
> **Priority:** P3
> **Type:** Runtime
> **Domain:** Player / Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem PlayerController/movement controllers
> **Must not run with:** F47 (PlayerSpeedComposer — DEVE rodar DEPOIS dela e usar o composer)
> **Repo lock scope:** `Player/Movement/**`, PlayerController.SpeedMultiplier (via composer F47)
> **Depends on:**
> - F47/E11 (PlayerSpeedComposer — fator nomeado, sem escrita direta)
> - F16 (executada — fadiga por stamina gasta já integra)
> **Blocks:** N/A
> **Scope:** sprint segurável que mantém a velocidade de fora-de-combate DENTRO de combate, drenando stamina.
> **Out of scope:** sprint fora de combate (velocidade já é plena), dash (existe), animação de arte.

required_adrs: []
required_game_rules: [combat_rules.md]

---

# /speckit.specify

## Contexto

Decisão 6.5-A do Refinamento v2 (`FABLE_DECISOES_RESPOSTAS_v2.0.md`): aprovada a pendência
C9 do v1 que ficou órfã quando a F27 virou Perfect Block. Canon de movimento
(COMBAT_CORE): fora de combate o player anda a 3.8-4.2 tiles/s; AO ENTRAR em combate a
velocidade cai para 3.4-3.8. O sprint permite MANTER 3.8-4.2 dentro de combate ao custo
de **8 stamina/s** — uma troca tática (mobilidade × recurso de ataque/block/dodge).

## Problema

Sem o sprint, a única resposta de mobilidade em combate é o dodge (custo alto, i-frame) —
kiting e reposicionamento contínuo não têm ferramenta. A decisão está aprovada e sem spec.

## Objetivo

Tecla de sprint segurável (definir na Fase 0 consultando o input map F67 — candidata:
LeftCtrl ou dobro-toque de direção) que, ENQUANTO em estado de combate, aplica o fator
de velocidade "sprint" (restaura para a faixa de fora-de-combate) via PlayerSpeedComposer
(F47), drenando 8 stamina/s (mesmo padrão de acumulador do block); solta a tecla ou
stamina insuficiente → fator removido.

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md (6.5-A)
docs/design/gameplay/combat/COMBAT_CORE_MECHANICS_DIRECTION.md (velocidades em tiles/s)
docs/game_rules/input_map.md (F67 — quando existir)
.claude/skills/player-ability-runtime/SKILL.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- PlayerController.SpeedMultiplier — após F47, escrito SOMENTE via PlayerSpeedComposer
  (fator nomeado por sistema);
- PlayerBlockController (padrão de drenagem por acumulador 18 STA/s — copiar o padrão);
- StaminaManager.TrySpendStamina; PlayerStatusReceiver (Stun bloqueia ação F01);
- detecção de "em combate": auditar Fase 0 (existe flag? threat dos inimigos? fallback:
  dano dado/recebido nos últimos N segundos define InCombat — implementar leve se não houver).
Não existe:
- sprint, estado InCombat formal (auditar), tecla reservada.
Auditar Fase 0:
- como saber "em combate" (CombatStateTracker leve se necessário);
- tecla livre no input map (F67); conflito com dash/block.
```

## Engineering stories

```text
Como jogador, quero segurar sprint para manter mobilidade plena em combate, pagando stamina.
Como sistema de stamina, quero a drenagem de 8/s no mesmo padrão do block (acumulador).
Como F47, quero o sprint como FATOR NOMEADO no composer — nunca escrita direta no multiplier.
```

## Escopo

```text
Inclui:
- PlayerSprintController (Player/Movement/): hold da tecla → se InCombat e stamina > 0,
  registra fator "sprint" no PlayerSpeedComposer que neutraliza a penalidade de combate
  (resultado: 3.8-4.2 tiles/s); drena 8 stamina/s (acumulador); solta/sem stamina → remove;
- CombatStateTracker leve SE não existir (dano dado/recebido nos últimos 4s = em combate);
- fora de combate: sprint é no-op silencioso (velocidade já plena);
- bloqueios: Stun (F01) e modal cancelam sprint;
- EditMode tests: drenagem por segundo, fator aplicado/removido, no-op fora de combate,
  cancelamento por stun/stamina.
```

## Fora de escopo

```text
Sprint fora de combate; mudanças no dash/dodge/block; animação; rebind (F67 documenta a tecla).
```

## Regras de não duplicação

```text
Velocidade SÓ via PlayerSpeedComposer (F47). Drenagem segue o padrão do block. Não criar
segundo tracker de combate se algum existir (Fase 0 audita).
```

## Critérios de aceite

### CA-1 — Sprint em combate funciona e drena
- Em combate, segurar sprint mantém a velocidade plena e consome 8 stamina/s; soltar
  restaura a penalidade de combate.
- Evidência: EditMode tests do fator + drenagem; cenário humano.

### CA-2 — Limites respeitados
- Sem stamina → sprint cai sozinho; Stun/modal cancelam; fora de combate é no-op.
- Evidência: EditMode tests dos 3 casos.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Player/Movement/PlayerSprintController.cs (NOVO)
Assets/_Game/Scripts/Combat/CombatStateTracker.cs              (NOVO, se Fase 0 confirmar ausência)
Assets/_Game/Tests/EditMode/Player/SprintTests.cs              (NOVO)
```

## Contratos
- Runtime: fator "sprint" no PlayerSpeedComposer; `CombatStateTracker.IsInCombat` (janela 4s).
- Event: consome DamageAppliedEvent/PlayerDamagedEvent (existentes) p/ janela de combate.
- Save/UI: N/A (transiente).

## Sistemas afetados
```text
Player movement, Stamina, Combat state
```
## Arquivos permitidos
```text
Arquitetura acima; Assets/_Game/Tests/EditMode/Player/**; docs/validation/**; csproj includes
```
## Arquivos proibidos
```text
*.unity/*.prefab/*.asset manual; Packages/**; ProjectSettings/**; PlayerController (direto)
```
## Estratégia de implementação
```md
### Fase 0 — Auditar InCombat existente + tecla livre (input map F67) + composer F47.
### Fase 1 — CombatStateTracker (se preciso) + testes.
### Fase 2 — SprintController (fator + drenagem) + testes.
### Fase 3 — csproj; run_strict_validation; report.
```
## Paralelização
- CONDITIONAL — após F47; não com specs de movement.
## Impacto em save/load
```text
NO
```
## Impacto em eventos
```text
Consome existentes; nenhum novo.
```
## Impacto em UI/Unity
```text
UI: NO | Play Mode final: YES (feel) | DEFERRED_TO_FINAL_VALIDATION
```
## Riscos técnicos
```text
Risco: empilhar com dash/block → composer resolve por fatores; block cancela sprint
(estados mutuamente exclusivos — teste).
```
## Rollback
```text
Remover controller/fator.
```

# /speckit.tasks

## Tasks
```md
- [ ] T001 — Fase 0 (InCombat/tecla/composer).
- [ ] T002 — Tracker leve + testes.
- [ ] T003 — Sprint (fator+drenagem+cancelamentos) + testes.
- [ ] T004 — csproj; run_strict_validation; report.
```
## Validações obrigatórias
```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```
## Testing Quality Gate
- Changed deterministic logic: YES | EditMode: YES | PlayMode/human: YES (feel) | Regression: YES (dash/block intactos)
- Minimum evidence: testes + cenário humano com sprint em combate

## Definition of Done
```text
Sprint canônico (3.8-4.2 em combate, 8 STA/s) via composer; cancelamentos corretos; builds 0E; report.
```
## Anti-regressão
```text
Nenhuma escrita direta em SpeedMultiplier. Block/dash/dodge inalterados. Sem novo i-frame.
```
