# Execution Report — fable_66 (Limpeza de Débitos de Código Declarados)

> Spec: `.specs/a_implementar/fable/fable_66_spec_code_debt_cleanup_slice_mode.md`
> Wave: FABLE Batch 11 | Priority: P3 | Type: Runtime / Refactor / Debt
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (slice mode RE-REGISTRADO honestamente; Play Mode DIFERIDO)
> Data: 2026-06-21

---

## Tabela débito → decisão → evidência

| # | Débito | Decisão | Evidência |
|---|--------|---------|-----------|
| 1 | `FarmPlot._temporarySequentialSliceMode` + seed + bypass stamina | **RE-REGISTRADO** (SEM gate) — não removido | Marcador atualizado c/ owner/condição; ver CA-4 |
| 2 | `FarmResourceInteractable` adapter smoke | **RE-REGISTRADO** (SEM gate) — não removido | Marcador atualizado c/ owner/condição |
| 3 | `DebugLoadoutProvisioner` TODO | **ANOTADO permanente** (editor tooling) | Marcador → comentário de propósito |
| 4 | `CaveDeathResolver` 4 TODOs | **FECHADO** | dia/hora reais; replace via manager; layout hash real; ReplaceActiveCorpse morto removido |
| 5 | `PlayerHitEvent` evento órfão | **FECHADO** (aposentado) | Evento + assinatura removidos; pipeline real documentado |
| 6 | `STAMINA_BLOCK_DEBT` guard silencioso | **FECHADO** | Guard → erro de wiring logado alto (one-shot) |

---

## Acceptance criteria extracted

| CA | Critério | Status | Evidência |
|----|----------|--------|-----------|
| CA-1 | Resolver com dados reais (dia/hora/layout; replace ligado; ReplaceActiveCorpse removido) | **OK** | `CaveDeathResolver.cs` sem os 4 TODOs; `CaveCorpseStamp` + testes |
| CA-2 | PlayerHitEvent aposentado (zero refs de tipo; assinatura removida; comentário do pipeline) | **OK** | `PlayerHitEvent.cs` deletado; `PlayerCombatController` sem subscribe/handler; grep limpo |
| CA-3 | Block sem guard silencioso (erro de wiring logado; wiring garantido; comportamento pinado) | **OK** | `PlayerBlockController.LogStaminaWiringError`; teste de mitigação |
| CA-4 | Slice mode resolvido honestamente | **OK (re-registrado)** | SEM gate: kit em inventário não verificável + regen de cena = ação Unity DIFERIDA → re-registro com owner/condição |
| CA-5 | Caracterização ANTES de cada mudança | **OK** | Commit `test(fable_66)` precede commits de produção |

---

## Existing systems audit

| Sistema | Encontrado | Reutilizado / Criado |
|---------|-----------|----------------------|
| `CorpseRecoveryManager.SetActiveCorpse` | SIM (já faz replace + `CorpseReplacedEvent`) | **REUSADO** — resolver não reimplementa replace; chamado por `DeathSystemBootstrap`/`CaveDeathEventHandler` |
| `TimeManager.CurrentDay` | SIM (owner canônico do dia; publica `DayStartedEvent`) | **REUSADO** — injetado no resolver (param opcional) |
| Relógio intra-dia (hora) | **NÃO EXISTE** (TimeManager só tem `CurrentDay`) | Documentado; `GetCurrentGameTime` saneia 0 via helper, ponto de troca único |
| `CaveLayoutStableHash.Compute` (FNV-1a, ADR-0005) | SIM | **REUSADO** — hash de layout determinístico do corpse |
| `PlayerDamageReceiver.ApplyDamage` | SIM (caminho real de dano: EnemyBrain/contato/projétil/armadilha) | **REUSADO** — prova que dano flui sem PlayerHitEvent |
| `StaminaManager` | SIM | **CONSUMIDO** — zero mudança; só wiring garantido no block |
| Criados (puros, testáveis) | — | `CaveCorpseStamp`, `CorpseReplaceDecision` (helpers de caracterização; NÃO sistemas paralelos) |

Nenhum sistema paralelo criado (skill `system-reuse-audit` aplicada na Fase 0).

---

## Spec Compliance Matrix

| Requisito | Implementação | OK |
|-----------|---------------|----|
| Resolver: dia real | `GetCurrentGameDay()` → `_timeManager.CurrentDay` via `CaveCorpseStamp.ResolveGameDay` (floor 1) | OK |
| Resolver: hora real | `GetCurrentGameTime()` → `CaveCorpseStamp.ResolveGameTime` (owner ausente ⇒ 0 saneado, documentado) | OK |
| Resolver: layout hash real | `GetCurrentLayoutHash()` → `CaveCorpseStamp.ResolveLayoutHash(worldSeed, runSeed, level)` determinístico | OK |
| Resolver: `ReplaceActiveCorpse` morto removido | método deletado; comentário aponta `CorpseRecoveryManager.SetActiveCorpse` | OK |
| PlayerHitEvent removido | `PlayerHitEvent.cs` + `PlayerHitEvent.cs.meta` deletados; csproj atualizado | OK |
| Assinatura órfã removida | `OnEnable/OnDisable` subscribe + `OnPlayerHit` removidos de `PlayerCombatController` | OK |
| Comentário do pipeline real | `PlayerDamageReceiver` doc-comment atualizado (chamada DIRETA, sem evento) | OK |
| Block: guard → wiring error | `LogStaminaWiringError` (one-shot) em `Start()` e `DrainStamina()` | OK |
| Block: comportamento inalterado | `MitigateNormalBlock` + drain idênticos (pinado por teste) | OK |
| Slice mode honesto | re-registrado com owner/condição (SEM gate) | OK |
| Provisioner decidido | anotado permanente (editor tooling) | OK |
| Caracterização antes | commit de testes precede produção | OK |

---

## Validation

```
Validation method: run_strict_validation.ps1 + validate_docs.ps1 + check_spec_diff_completeness.ps1
Assembly-CSharp: PASS (exit 0, 0 erros, 1 warning pré-existente CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros, 3 warnings pré-existentes)
Docs validation: PASS (exit 0)
Diff completeness: PASS (exit 0; WARN não-bloqueante: tests em commit anterior — ver nota)
run_strict_validation.ps1: exit 1 AMBIENTAL — único FAIL é "forbidden files altered" das 3 cenas
  Assets/_Game/Scenes/{Cave,Farm,Town}Scene.unity, JÁ modificadas no working tree ANTES desta spec
  (não tocadas aqui; zero edição de YAML). Os WARNs de "missing sections" são de reports históricos
  (01_*, 08_*, 09_*, etc.), não deste report. Os gates da fable_66 (docs, ambos builds, diff) = PASS.
PlayerHitEvent grep: apenas comentários/nomes de teste (zero referências de tipo compilável)
```

Automated tests added/updated: YES (CaveDebtCleanupCharacterizationTests — 8 testes puros). Committados
ANTES da produção (CA-5), por isso não aparecem no diff do commit de produção (a checagem de diff é
por-commit). Não executados via Unity Test Runner (Play Mode/Unity DIFERIDO por autorização do dono);
compilam em `Assembly-CSharp` (PASS). Automated tests not added: N/A (foram adicionados).

---

## Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS.** 4 dos 6 débitos foram FECHADOS no código com caracterização
(resolver, PlayerHitEvent, block, provisioner). Os 2 débitos de slice mode (FarmPlot + adapter) foram
**RE-REGISTRADOS honestamente, não fechados** — a remoção exige (a) o kit inicial canônico GRANTAR
hoe + watering-can + sementes ao **inventário** do player no new game, o que vive em
`PlayerDataSO.StartingItems` (asset YAML não verificável nesta sessão sem Unity) e não é provado pela
fable_63 (intro), e (b) regeneração de `CreateMvpFarmScene` no Unity Editor com o flag OFF — ação Unity
DIFERIDA. Remover o slice mode sem (a)+(b) quebraria o ÚNICO caminho jogável do loop de crop. A própria
spec manda re-registrar nesse caso (CA-4 SEM gate; risco/mitigação) — nunca fingir fechamento. Não há
claim de Play Mode / aceitação: validação humana DIFERIDA.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (carimbos de morte, layout hash, regra de replace, guard de block)
Changed Unity scene/prefab/asset wiring: NO (zero edição de .unity/.prefab/.asset)
Automated tests added/updated: YES (CaveDebtCleanupCharacterizationTests — 8 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila); Unity Test Runner DIFERIDO
Manual Play Mode scenario: docs/validation/playmode/fable_66_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: hora intra-dia ainda é 0 (sem relógio canônico — ponto de troca único documentado);
  slice mode permanece ativo até kit em inventário + regen de cena; Play Mode dos 3 fluxos
  (crop loop, block drena stamina, morte dupla na caverna substitui corpo) não executado (DIFERIDO).
```

validated_game_rules: `cave_rules.md` (corpse stamp dia/hora/layout; ADR-0005 layout hash determinístico
preservado — zero mudança de seed/IDs), `combat_rules.md` (pipeline central de dano inalterado; block
mitiga 50% floor 1 — pinado), `death_anya_corpse_rules.md` (replace via `CorpseRecoveryManager` —
contrato preservado), `farm_rules.md` (loop de crop preservado — slice mode intacto, re-registrado).

required_adrs validados: ADR-0005 (cave stable run — hash determinístico, sem GUID/timestamp),
ADR-0007 (event bus — PlayerHitEvent morto removido; comunicação real é chamada direta documentada,
não um novo acoplamento MB→MB de gameplay).

---

## Dependency Chain

```
Original target: fable_66
Dependency chain: nenhuma dependência same-wave pendente (F32/E18 já BUILD_VALIDATED; kit inicial
  resolvido em design por EMENDA-D mas NÃO provado em inventário runtime).
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES
```

---

## Remaining work

1. **Kit inicial em inventário (owner do slice mode):** verificar/garantir que `PlayerDataSO.StartingItems`
   grant hoe + watering-can + sementes ao inventário no new game; então regenerar `CreateMvpFarmScene`
   com `_temporarySequentialSliceMode = false` e remover `FarmResourceInteractable` (ação Unity).
2. **Relógio intra-dia:** quando existir owner canônico de hora, ligar `GetCurrentGameTime` a ele
   (ponto de troca único já isolado em `CaveCorpseStamp.ResolveGameTime`).
3. **Play Mode (DIFERIDO):** executar o cenário humano dos 3 fluxos.
4. **Stamina por skill water (onda de skills):** débito re-registrado, fora do escopo.
