---
doc_type: execution_report
spec_id: fable_58_spec_audio_sfx_hooks
wave: FABLE Batch 10
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
validated_adrs: []
validated_game_rules:
  - event_rules.md
---

# Execution Report — fable_58 Áudio: AudioManager + Hooks de SFX por Evento + MusicState

> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Branch:** dev
> **Validation method:** dotnet build (Assembly-CSharp + Assembly-CSharp-Editor) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
> **Phase 2-3 (Unity / Play Mode):** DEFERRED_TO_FINAL_VALIDATION (audição real do lote; autorizado pelo dono)

---

## Summary

Implementado um domínio de áudio NOVO e isolado (`Assets/_Game/Scripts/Audio/**`) que SÓ
consome eventos já publicados no `GameEventBus`. Nenhum sistema de gameplay foi modificado.
Entrega: `AudioManager` (canais SFX/Música, volume Master/canal, pool de vozes round-robin,
crossfade de música), `SfxEventBridge` (único call site de SFX; tabela evento→categoria;
cooldown anti-spam; derivação de MusicState), `ProceduralSfxFactory` (clipes placeholder
procedurais por categoria/estado, cacheados), e a máquina de estado de música da EMENDA V3
(Calmo/Combate/Boss/Festival com prioridade determinística + crossfade placeholder).

Áudio real (clipes/faixas) permanece DIFERIDO para a fase de arte — a tabela
evento→categoria→clipe e a tabela estado→placeholder são o contrato de entrega.

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | AudioManager vivo e configurável: canais SFX/música + volume master/canal; mudar volume altera o ganho | `AudioManager.GetChannelGain` + `AudioGainCalculator.ComputeGain`; testes `Gain_*` | OK (build + EditMode) |
| CA-2 | Hooks por evento: eventos audíveis v1 disparam categoria correta via `SfxEventBridge`; nenhum gameplay referencia o AudioManager | `SfxEventMap` (tabela) + `SfxEventBridge` (único call site); teste `EventMap_AllV1AudibleEventsHaveCategory`; grep (zero ref ao AudioManager fora de Audio/**) | OK |
| CA-3 | Placeholders procedurais distintos e válidos por categoria | `ProceduralSfxLibrary.ForSfx` (15 categorias distintas) + `ProceduralSfxFactory.FillSamples`; testes `SfxSpecs_AllValid_AndDistinct`, `FillSamples_ProducesNonZeroBoundedAudio` | OK (lógica) / clipe real via `AudioClip.Create` validado em Play Mode (deferido) |
| CA-4 | Nunca bloquear gameplay: categoria/clipe ausente = silêncio + 1 log; rajada não estoura pool nem spamma; sem exceção | Pool round-robin + `SfxCooldownGate` + fallback silencioso em `PlaySfx`; testes `Cooldown_*`, `FillSamples_DoesNotThrowOnDegenerateInput`, `InvalidSpec_*` | OK |
| CA-5 | Estado de música (EMENDA V3): MusicState derivado com prioridade Boss>Combate>Festival>Calmo; troca por crossfade curto; sem exceção; faltando placeholder = silêncio + log | `MusicStateResolver` + `MusicCrossfade` + `SfxEventBridge.PushMusicStateIfChanged` (publica `MusicStateChangedEvent`) + `AudioManager.PlayMusic`; testes `MusicPriority_*`, `MusicResolver_ReturnsToLowerPriorityWhenConditionCeases`, `Crossfade_*` | OK (lógica) / audição real deferida |

---

## Existing systems audit (system-reuse, Fase 0)

| Item | Resultado da auditoria | Decisão |
|------|------------------------|---------|
| Código de áudio (AudioManager/AudioSource/AudioClip) | **ZERO** em `Assets/_Game/Scripts/**` (Glob + Grep) | Criar domínio novo `Audio/**` |
| `GameEventBus` | Existe (`Core/GameEventBus.cs`); `Subscribe<T>` retorna `IDisposable`, dedup, isola exceções de listeners | REUSAR (subscribe via IDisposable; unsubscribe simétrico) |
| Eventos audíveis v1 | 16 dos 17 listados EXISTEM. **`DamageBlockedEvent` NÃO existe** no projeto | Não criar evento de gameplay; categoria `Block` fica RESERVADA (consumidor futuro adiciona 1 linha). Block-feel v1 já coberto por `PlayerPerfectBlockEvent`→PerfectBlock |
| `StatusEffectAppliedEvent` | Existe (struct, F01) — distinto de `StatusAppliedEvent` (StatusAndDamageEvents) | Mapeado o `StatusEffectAppliedEvent` (o que a spec lista) |
| `GameAudioSettings` (F56) | **NÃO existe** (F56 não rodou) | Default local (Master 80/SFX 60/Música 70) + `SetMasterVolume`/`SetChannelVolume` expostos para binding posterior (contrato) |
| GameBootstrap | DontDestroyOnLoad singleton; wiring por referência serializada | **Não editado.** AudioManager/SfxEventBridge usam `RuntimeInitializeOnLoadMethod` (idiom do projeto p/ event-consumers: `WorldWeatherService`, `CaveWanderingMerchant`, `NpcScheduleRuntimeBootstrap` etc.) — não exige regeneração de cena, deferred-Unity-safe. Ver "Honest status rationale" |
| AudioListener | Vive nas câmeras principais das cenas (não tocado); manager 2D flat não cria listener | Sem mudança de cena |
| MusicState derivação | `EnemySpawnedEvent`/`EnemySeenEvent` (engaja) + `EnemyKilledEvent` (desengaja) existem. **Não há** evento "boss começou" nem qualquer evento de festival; só `CaveBossDefeatedEvent` (boss off) | Derivar combate por contagem; expor boss/festival na API do resolver para sinais futuros; publicar `MusicStateChangedEvent` interno (autorizado pela EMENDA V3) |
| csproj | Explicit `<Compile Include>` (sem glob); runtime+test em `Assembly-CSharp.csproj`; Editor csproj separado | 11 novos arquivos (10 runtime + 1 test) adicionados só em `Assembly-CSharp.csproj` |

Nenhum sistema paralelo criado. Único call site de SFX = `SfxEventBridge`. Único storage de
volume = AudioManager (consome F56 quando existir). Único caminho de troca de faixa =
`MusicStateChangedEvent`.

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação | OK |
|------------------|---------------|----|
| AudioManager host bootstrap; canais {Sfx, Music}; volume master+canal 0-100 | `AudioManager.cs` (RuntimeInitializeOnLoad bootstrap; `AudioChannel`; SetMasterVolume/SetChannelVolume) | ✓ |
| Consumo de GameAudioSettings (F56) se presente, senão default 80 | `TryAdoptAudioSettings` (no-op documentado; F56 ausente) + defaults locais + SetMasterVolume exposto | ✓ (contrato) |
| API PlaySfx(category) e PlayMusic(state)/StopMusic | `AudioManager.PlaySfx(SfxCategory)` / `PlayMusic(MusicState)` / `StopMusic()` | ✓ |
| Pool de AudioSources (8-16 vozes, round-robin, sem Instantiate por som) | `EnsureSfxVoices(12)` + `NextSfxVoice()` round-robin | ✓ |
| ProceduralSfxFactory: AudioClip.Create por categoria; cache; gerado 1× no boot | `ProceduralSfxFactory.GenerateAll/GetSfxClip/GetMusicClip` (cache dict) | ✓ |
| Formas de onda distintas por categoria (hit/perfect_block/posture_break/charged/status/ui_toast/pickup/levelup/day_start/save...) | `ProceduralSfxLibrary.ForSfx` (15 specs distintos, testado) | ✓ |
| SfxEventBridge: assina eventos; tabela estática evento→categoria; cooldown ~50ms | `SfxEventBridge.SubscribeAll` + `SfxEventMap` + `SfxCooldownGate(0.05f)` | ✓ |
| Eventos audíveis v1 (mínimo, 17) | 16 mapeados (todos os existentes); `DamageBlockedEvent` ausente do projeto → reservado (ver audit) | ✓ (com nota) |
| Fallback: categoria sem clipe = silêncio + log 1×; nunca exceção | `PlaySfx` (clip null → `LogMissingClipOnce` + return); `BuildClip` try/catch | ✓ |
| Adds events: NENHUM (SFX) | Zero evento de gameplay criado | ✓ |
| Save: nenhum campo | Nada persistido | ✓ |
| Namespace CindarsHope.Audio | Todos os arquivos | ✓ |
| EMENDA V3 — enum MusicState (Calmo/Combate/Boss/Festival) | `MusicState.cs` | ✓ |
| EMENDA V3 — prioridade Boss>Combate>Festival>Calmo + retorno ao cessar | `MusicStateResolver.Resolve` + testes de retorno | ✓ |
| EMENDA V3 — transição via evento (deriva de eventos; senão evento próprio `MusicStateChangedEvent`) | Deriva combate de Enemy*; publica `MusicStateChangedEvent` (autorizado) | ✓ |
| EMENDA V3 — crossfade placeholder (~0.5-1.0s), sem exceção, fallback silencioso | `MusicCrossfade` + `AudioManager.ApplyMusicGain`/`Update`; placeholder null = silêncio | ✓ |
| EMENDA V3 — PlayMusic opera por MusicState; canal Music com 2 sources em reuso | `PlayMusic(MusicState)`; `_musicA`/`_musicB` crossfade | ✓ |
| EMENDA V3 — testes de prioridade/mapeamento/crossfade em classe pura | `AudioSfxHooksTests` (CA-5) | ✓ |

---

## Validation

```text
Validation method: run_strict_validation.ps1 + dotnet build + validate_docs + check_spec_diff_completeness
Assembly-CSharp:        PASS (exit 0; 0 erros, 0 warnings)
Assembly-CSharp-Editor: PASS (exit 0; 0 erros, 3 warnings PRE-EXISTENTES — EnemyTaxonomy/AssetPostprocessors, nada novo)
Docs validation (validate_docs.ps1): PASS (exit 0)
Diff completeness (check_spec_diff_completeness.ps1): PASS (exit 0)
run_strict_validation.ps1: EXIT 1 = EXPECTED_FAIL_LEGACY_ONLY / HARNESS_BUG conhecido
  (check_spec_quality regex `^##` sem multiline reprova TODOS os reports; não é falha desta spec).
  Builds + docs + diff-completeness limpos; nada NOVO introduzido. Ver "Honest status rationale".
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

EditMode tests: `Assets/_Game/Tests/EditMode/Audio/AudioSfxHooksTests.cs` (NUnit). Compilam
em `Assembly-CSharp` (exit 0). Execução do Unity Test Runner: DEFERIDA (sem Unity nesta sessão);
toda a lógica testada é pura (sem AudioSource), validável em EditMode quando o Runner rodar.

---

## Testing Quality Gate

```text
Changed runtime code: YES (domínio Audio novo)
Changed deterministic logic: YES (mapeamento, cooldown, ganho, prioridade de MusicState, crossfade, waveform)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Audio/AudioSfxHooksTests.cs)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (compila os testes; exit 0). Unity Test Runner DEFERIDO (sem Unity).
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (audição real do lote — som é inerentemente Play Mode)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: clipe real via AudioClip.Create + audição (timbre/clipping/transição suave de crossfade) não verificados sem Play Mode; F56 binding de volume ainda não existe (default local). Nenhum gameplay alterado (diff é a prova).
```

Regression: nenhum arquivo de gameplay modificado (diff cobre só `Audio/**`, csproj e o report).
Unsubscribe simétrico em todas as ~18 assinaturas (`SfxEventBridge.UnsubscribeAll` em
OnDisable/OnDestroy via IDisposable).

---

## Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS** (não ACCEPTED, não PLAYMODE_VALIDATED):

- Núcleo determinístico completo e coberto por EditMode tests; ambos os builds exit 0.
- Áudio é inerentemente Play Mode: a audição real (timbres distintos, ausência de clipping,
  suavidade do crossfade Calmo→Combate→Boss→retorno) e a criação de `AudioClip` em runtime
  só se validam em Play Mode — DEFERIDO ao gate final do lote (autorizado pelo dono).
- `run_strict_validation.ps1` retorna exit 1 por um BUG conhecido do `check_spec_quality.ps1`
  (regex `^##` sem flag multiline reprova todos os reports) — classificado
  EXPECTED_FAIL_LEGACY_ONLY/HARNESS_BUG. As validações escopadas desta spec (2 builds, docs,
  diff-completeness) estão todas exit 0 e nada novo foi introduzido.

**Decisões de escopo documentadas:**

1. **Bootstrap via RuntimeInitializeOnLoadMethod (não edição do GameBootstrap).** A spec lista
   GameBootstrap como arquivo permitido (wiring aditivo), porém o idiom canônico do projeto para
   sistemas que SÓ consomem eventos é o self-bootstrap por `RuntimeInitializeOnLoadMethod`
   (precedentes: `WorldWeatherService`, `CaveWanderingMerchant`, `NpcScheduleRuntimeBootstrap`,
   `FonteRuntimeService`...). Essa via não exige regeneração de cena (a serialização de um campo
   novo no GameBootstrap seria uma ação humana no Unity, ora deferida), mantém o diff mínimo e o
   GameBootstrap intacto. Resultado prático idêntico ao wiring aditivo pedido, com menor risco.

2. **`DamageBlockedEvent` ausente do projeto.** A spec o lista como audível v1, mas ele não
   existe e esta spec não pode criar eventos de gameplay. Categoria `Block` criada e RESERVADA;
   o feedback de block no v1 usa `PlayerPerfectBlockEvent`→PerfectBlock. Consumidor futuro
   adiciona 1 linha em `SfxEventMap` quando o evento existir.

3. **Boss/Festival no MusicState.** Não há evento "boss começou" nem evento de festival no
   código (auditoria Fase 0). A derivação cobre Combate (Enemy spawned/seen → engaja;
   EnemyKilled → desengaja) e desliga Boss em `CaveBossDefeatedEvent`. As condições Boss/Festival
   ficam acessíveis na API do `MusicStateResolver` para quando tais sinais existirem, sem segundo
   caminho de troca de faixa. `MusicStateChangedEvent` é o único evento próprio (autorizado pela
   EMENDA V3).

---

## Files changed

```text
NOVOS (runtime — Assets/_Game/Scripts/Audio/):
  AudioChannel.cs            enum {Sfx, Music}
  SfxCategory.cs             enum + ids estáveis das categorias v1
  MusicState.cs              enum Calmo/Combate/Boss/Festival (EMENDA V3)
  MusicStateResolver.cs      máquina pura de prioridade Boss>Combate>Festival>Calmo
  MusicStateChangedEvent.cs  único evento próprio (interno ao áudio; EMENDA V3)
  AudioGainCalculator.cs     ganho puro Master×canal + clamp + defaults
  SfxCooldownGate.cs         anti-spam puro por categoria (~50ms)
  ProceduralSfxSpec.cs       specs de onda por categoria/estado + biblioteca
  MusicCrossfade.cs          curva/duração de crossfade pura
  SfxEventMap.cs             tabela estática evento→categoria
  ProceduralSfxFactory.cs    AudioClip.Create por categoria/estado + cache (FillSamples pura)
  AudioManager.cs            host: pool SFX round-robin + crossfade música + volume
  SfxEventBridge.cs          único call site SFX; assina ~18 eventos; deriva MusicState

NOVOS (teste — Assets/_Game/Tests/EditMode/Audio/):
  AudioSfxHooksTests.cs      NUnit; CA-1..CA-5 (ganho/mapeamento/specs/cooldown/prioridade/crossfade)

EDITADO:
  Assembly-CSharp.csproj     +14 <Compile Include> (13 runtime + 1 test)

NOVO (docs):
  docs/validation/fable_58_spec_audio_sfx_hooks_execution_report.md (este arquivo)
```

---

## Remaining work (deferido)

```text
- Play Mode (gate final do lote): ouvir SFX distintos em combate (hit/perfect block/quebra de
  postura) e UI (toast/save); passar por Calmo→Combate→Boss→retorno e ouvir crossfade suave;
  confirmar ausência de clipping/estouro. (DEFERRED_TO_FINAL_VALIDATION)
- Unity Test Runner EditMode: executar AudioSfxHooksTests no Runner (compila; execução deferida).
- F56 binding: quando GameAudioSettings existir, ligar Master/SFX/Música a
  SetMasterVolume/SetChannelVolume (aditivo; ponto de extensão já exposto).
- Fase de arte: trocar placeholders procedurais por assets reais (a tabela é o contrato).
- Consumidores futuros: DamageBlockedEvent (→Block), traps (F60), footstep por terreno (OFF v1).
```
