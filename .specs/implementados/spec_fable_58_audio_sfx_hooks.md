# SPEC — Áudio: AudioManager Mínimo + Hooks de SFX por Evento (placeholders procedurais)

> **Spec ID:** `fable_58_spec_audio_sfx_hooks`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P3
> **Type:** Runtime / Integration
> **Domain:** Audio
> **Parallelizable:** YES (lock próprio Audio/** — só consome eventos existentes)
> **Parallel group:** fable_batch10_audio (grupo próprio)
> **Can run with:** F54, F55, F56, F57, F59, F60 (binding de volume com F56 é aditivo — coordenar)
> **Must not run with:** N/A (nenhuma spec toca Audio/**)
> **Repo lock scope:** `Assets/_Game/Scripts/Audio/**` (novo), GameBootstrap (wiring aditivo)
> **Depends on:**
> - F02 (executada — eventos de combate publicados)
> - F27 (executada — CombatPostureEvents: charged/posture/perfect block)
> **Blocks:** N/A
> **Scope:** AudioManager (canais SFX/música, volume) + hooks consumindo eventos JÁ publicados + clipes placeholder procedurais por categoria.
> **Out of scope:** música composta/assets de áudio reais (fase de arte), áudio posicional/spatial, mixer profissional, SFX que exijam eventos inexistentes.

required_adrs: []
required_game_rules: [event_rules.md]

---

# /speckit.specify

## Contexto

O projeto tem ZERO código de áudio: nenhum AudioManager, nenhum AudioSource, nenhum
AudioClip em `Assets/_Game/Scripts/**` (auditoria de completude desta wave). Enquanto
isso, as directions assumem som como parte do contrato de legibilidade: COMBAT_CORE
trata telegraph como sinal visual/sonoro do golpe ("som/partícula antes de ativar" é
counterplay canônico de armadilha, telegraphs legíveis são critério de validação humana)
e UI_UX_FULL_GAMEPLAY pede feedback consistente de ações (vender, colher, comprar,
notificações).

O lado bom do atraso: o GameEventBus já publica TODOS os ganchos de que o áudio precisa
— `PlayerChargedAttackEvent`, `EnemyPostureBrokenEvent`, `PlayerPerfectBlockEvent`
(CombatPostureEvents, F27), `StatusEffectAppliedEvent`, `DamageAppliedEvent`/
`DamageBlockedEvent`/`PlayerDamagedEvent` (StatusAndDamageEvents), `EnemyKilledEvent`,
`NotificationToastRequestedEvent`, `PlayerActionFeedbackEvent`, `ItemPickedUpEvent`,
`DayStartedEvent`, `GameSavedEvent`, `EnemyTelegraphStartedEvent`... Um AudioManager que
SÓ escuta eventos entra sem tocar nenhum sistema de gameplay — paralelizável por
construção.

Restrição vinculante: não existem assets de áudio e a fase de arte não chegou. Os clipes
do v1 são PLACEHOLDERS gerados proceduralmente em runtime (`AudioClip.Create` com formas
de onda por categoria — beep/blip/noise burst com pitch/duração distintos), e a ausência
de qualquer clipe NUNCA bloqueia ou quebra gameplay (fallback silencioso + log de
wiring, jamais exceção).

## Problema

Um jogo de ação sem NENHUM feedback sonoro falha o contrato de telegraph/counterplay das
directions (o jogador lê menos, o combate parece pior do que é) e empurra o problema
para a fase de arte SEM a infraestrutura pronta — quando os assets chegarem, não haverá
onde pluga-los. E se o áudio nascer acoplado (chamadas diretas de
MonoBehaviour-para-MonoBehaviour), viola a regra de comunicação exclusiva por
GameEventBus e cria dívida em dezenas de call sites.

## Objetivo

Ao final desta spec, deve existir um `AudioManager` (host bootstrap, wiring aditivo no
GameBootstrap) com canais SFX e música, volume master/por canal (consumindo
`GameAudioSettings` da F56 se existir, senão default local), um `SfxEventBridge` que
assina os eventos publicados e dispara SFX por categoria via tabela
evento→categoria→clipe, e um `ProceduralSfxFactory` que gera os clipes placeholder por
categoria em runtime — com pool de AudioSources (sem Instantiate/Destroy por som),
fallback silencioso e zero chamadas diretas de gameplay.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md (telegraphs/feedback/validação humana)
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md (feedbacks de ação)
.claude/rules/testing-quality-gate.md
.claude/skills/event-bus-pattern/SKILL.md
.claude/skills/bootstrap-wiring/SKILL.md
.claude/skills/object-pooling-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- GameEventBus + eventos publicados (confirmados na auditoria):
  CombatPostureEvents {PlayerChargedAttackEvent, EnemyPostureBrokenEvent,
  PlayerPerfectBlockEvent}, StatusEffectAppliedEvent, StatusAndDamageEvents
  {DamageAppliedEvent, DamageBlockedEvent, PlayerDamagedEvent}, EnemyKilledEvent,
  EnemyTelegraphStartedEvent, NotificationToastRequestedEvent,
  PlayerActionFeedbackEvent, ItemPickedUpEvent, ItemCraftedEvent, CropHarvestedEvent,
  FishCaughtEvent, GoldChangedEvent, DayStartedEvent, GameSavedEvent,
  PlayerLevelChangedEvent, SceneTransition*Event;
- GameBootstrap (padrão de wiring por referência serializada/injeção);
- GameAudioSettings (F56 — volume persistido; consumir SE existir).
Não existe:
- QUALQUER código de áudio (AudioManager/AudioSource/AudioClip ausentes do projeto).
Auditar Fase 0:
- ordem F56×F58 no batch: se F56 ainda não rodou, AudioManager usa default local e
  expõe SetMasterVolume p/ binding posterior (contrato documentado);
- AudioListener: onde vive a câmera principal por cena (1 listener por cena —
  conferir nas 3 cenas geradas);
- lista final de eventos audíveis v1 (tabela abaixo é o mínimo).
```

## Engineering stories

```text
Como jogador, quero ouvir feedback de golpes, blocks perfeitos e quebra de postura,
  para ler o combate sem olhar números.
Como jogador, quero sons distintos por categoria (combate/UI/farm/dia), para o mundo
  ter presença mesmo com placeholders.
Como fase de arte futura, quero uma tabela evento→categoria→clipe pronta, para trocar
  beeps por assets reais sem tocar em gameplay.
Como arquitetura, quero o áudio 100% por eventos, para nenhum sistema de gameplay
  conhecer o AudioManager.
```

## Escopo

```text
Inclui:
- AudioManager (host bootstrap; DontDestroyOnLoad auditado vs padrão do projeto):
  canais {Sfx, Music}; volume master + por canal (0-100); consumo do GameAudioSettings
  (F56) se presente, senão default 80; API PlaySfx(categoryId) e
  PlayMusic(trackId)/StopMusic (música v1: silêncio ou drone placeholder — só a API);
- pool de AudioSources (8-16 vozes SFX, reuso round-robin — sem Instantiate por som;
  skill object-pooling-pattern);
- ProceduralSfxFactory: AudioClip.Create por categoria (formas de onda distintas:
  hit = burst curto grave, perfect_block = ping agudo, posture_break = queda de pitch,
  charged = sweep ascendente, status = blip duplo, ui_toast = blip suave,
  pickup = blip curto, levelup = arpejo 3 notas, day_start = acorde suave,
  save = confirmação 2 notas) — clipes cacheados, gerados 1× no boot;
- SfxEventBridge: assina os eventos do GameEventBus e mapeia para categorias
  (tabela estática evento→categoria; cooldown anti-spam por categoria ~50ms para
  rajadas de DamageApplied);
- eventos audíveis v1 (mínimo): PlayerChargedAttackEvent, EnemyPostureBrokenEvent,
  PlayerPerfectBlockEvent, StatusEffectAppliedEvent, DamageAppliedEvent,
  DamageBlockedEvent, PlayerDamagedEvent, EnemyKilledEvent,
  NotificationToastRequestedEvent, PlayerActionFeedbackEvent, ItemPickedUpEvent,
  ItemCraftedEvent, CropHarvestedEvent, FishCaughtEvent, PlayerLevelChangedEvent,
  DayStartedEvent, GameSavedEvent;
- fallback: categoria sem clipe = silêncio + log de wiring 1× (nunca exceção, nunca
  bloquear gameplay);
- EditMode tests: mapeamento evento→categoria completo, factory gera clipes válidos
  (samples > 0, duração esperada), cooldown anti-spam, volume aplicado por canal,
  fallback silencioso sem exceção.
```

## Fora de escopo

```text
Não inclui:
- assets de áudio reais/música composta (fase de arte — a tabela é o contrato);
- áudio posicional/3D/spatial blend (2D flat no v1);
- AudioMixer/ducking/snapshots;
- sons de footstep por terreno (PlayerStepEvent existe — registrar como categoria
  futura, OFF no v1 para não virar metralhadora de beep);
- opções de áudio além de volume (aba Sistema F56 é a UI);
- SFX para eventos que ainda não existem (traps F60 etc. — consumidores futuros
  adicionam linhas na tabela).
```

## Regras de não duplicação

```text
Não criar segundo caminho de disparo de som — TODO SFX nasce de evento no
SfxEventBridge (zero PlaySfx chamado por sistemas de gameplay).
Não criar segundo storage de volume — GameAudioSettings (F56) é a fonte; F58 só
consome/expõe.
Não criar AudioSource avulso por sistema — pool único do AudioManager.
Namespace CindarsHope.Audio (CindarsHope.Debug/Temp proibidos por regra).
```

## Critérios de aceite

### CA-1 AudioManager vivo e configurável

- AudioManager sobe via bootstrap com canais SFX/música e volume master/por canal;
  mudar volume altera o ganho aplicado (testável na projection do ganho).
- Evidência: EditMode tests de ganho/canal + cenário humano do lote.

### CA-2 Hooks por evento funcionando

- Os eventos audíveis v1 disparam SFX da categoria correta via SfxEventBridge; nenhum
  sistema de gameplay referencia o AudioManager diretamente.
- Evidência: EditMode test da tabela (todo evento v1 tem categoria) + grep no diff
  (zero referência a AudioManager fora de Audio/** e GameBootstrap).

### CA-3 Placeholders procedurais por categoria

- ProceduralSfxFactory gera clipes distintos e válidos para todas as categorias da
  tabela; categorias soam diferentes (parâmetros de onda distintos por categoria).
- Evidência: EditMode tests (clipe válido por categoria; parâmetros únicos).

### CA-4 Nunca bloquear gameplay

- Categoria/clipe ausente = silêncio + 1 log de wiring; rajada de eventos não estoura
  o pool (reuso) nem spamma (cooldown); nenhuma exceção propagada.
- Evidência: EditMode tests de fallback/cooldown/pool.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Audio/
  AudioManager.cs           (NOVO — host bootstrap; canais; volume; pool de vozes)
  AudioChannel.cs           (NOVO — enum {Sfx, Music} + ganho)
  SfxCategory.cs            (NOVO — enum/ids estáveis das categorias v1)
  SfxEventBridge.cs         (NOVO — assina GameEventBus; tabela evento→categoria;
                             cooldown anti-spam)
  ProceduralSfxFactory.cs   (NOVO — AudioClip.Create por categoria; cache)
GameBootstrap               (wiring aditivo — referência serializada/injeção)
Assets/_Game/Tests/EditMode/Audio/AudioSfxHooksTests.cs (NOVO)
docs/validation/fable_58_spec_audio_sfx_hooks_execution_report.md
```

## Contratos

### Data contracts

- `SfxCategory`: ids estáveis (sfx_hit, sfx_perfect_block, sfx_posture_break,
  sfx_charged, sfx_status, sfx_ui_toast, sfx_pickup, sfx_levelup, sfx_day_start,
  sfx_save, sfx_block, sfx_enemy_killed, sfx_craft, sfx_harvest, sfx_fish).
- Tabela estática evento→categoria (uma linha por evento audível v1).

### Runtime contracts

- `AudioManager.PlaySfx(SfxCategory)`: voz do pool + clipe do cache + ganho
  (master × canal); nunca lança (fallback silencioso + log 1×).
- `SfxEventBridge`: único call site de PlaySfx; Subscribe no OnEnable, Unsubscribe no
  OnDisable (padrão event_rules); cooldown por categoria (~50ms) anti-spam.
- Volume: lê GameAudioSettings (F56) quando presente; expõe SetMasterVolume p/ binding.
- Lógica de mapeamento/ganho/cooldown em classes puras testáveis (sem depender de
  AudioSource em EditMode).

### Event contracts

- Adds: NENHUM evento novo. Consome somente eventos existentes via GameEventBus.

### Save contracts

- NENHUM campo de save (volume é UI state em PlayerPrefs via F56/GameAudioSettings).

### UI contracts

- Sem UI nesta spec (slider de volume vive na aba Sistema F56; binding aditivo).

## Sistemas afetados

```text
Audio (domínio novo — lock próprio)
GameBootstrap (wiring aditivo)
Nenhum sistema de gameplay modificado (consumo passivo de eventos)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Audio/** (tudo novo)
GameBootstrap (wiring aditivo — arquivo auditado na Fase 0)
Assets/_Game/Tests/EditMode/Audio/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML
Packages/** ; ProjectSettings/** (inclusive AudioManager do projeto — settings nativos)
QUALQUER arquivo de gameplay (Combat/Farm/UI/...) — zero call site fora de Audio/**
assets de áudio importados (.wav/.ogg/.mp3 — placeholders são 100% procedurais)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
GameBootstrap (ponto de wiring); AudioListener por cena; presença do
GameAudioSettings (F56); lista final de eventos audíveis v1.

### Fase 1 — Núcleo
AudioManager + canais + pool de vozes + ProceduralSfxFactory + testes
(ganho/clipes/pool).

### Fase 2 — Bridge
SfxEventBridge + tabela evento→categoria + cooldown + testes (mapeamento completo/
anti-spam/fallback).

### Fase 3 — Fechamento
Wiring no GameBootstrap; binding de volume (se F56 presente); csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: YES.
- Parallel group: fable_batch10_audio (grupo próprio).
- Can run with: F54, F55, F56, F57, F59, F60.
- Must not run with: N/A (nenhuma outra spec toca Audio/**; GameBootstrap é wiring
  aditivo de 1 linha — coordenar merge).
- Shared files/systems that require lock: Assets/_Game/Scripts/Audio/** (novo),
  GameBootstrap (aditivo).
- Reason: domínio novo e isolado; só consome eventos publicados.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (SfxEventBridge assina ~17 eventos — unsubscribe
obrigatório no OnDisable)
```

## Impacto em UI/Unity

```text
Changes UI: NO (slider vive na F56)
Changes scenes: NO (AudioManager via bootstrap; AudioListener já existe nas câmeras)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (clipes procedurais em memória)
Requires Play Mode final validation: YES (lote final — som é inerentemente Play Mode)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: spam de SFX em rajadas (DamageApplied em multi-hit) virar ruído/clipping.
Mitigação: cooldown por categoria + pool com reuso; teste de rajada.

Risco: ordem F56×F58 (volume sem dono).
Mitigação: contrato — F58 consome GameAudioSettings SE existir, senão default local
com SetMasterVolume exposto; documentado no report.

Risco: AudioClip.Create em EditMode tests (API Unity em teste).
Mitigação: lógica de parâmetros de onda/mapeamento/cooldown em classes puras;
testes de clipe usam a API só onde EditMode permite (sem Play Mode).

Risco: vazamento de subscriptions (17 eventos).
Mitigação: padrão Subscribe/Unsubscribe simétrico + revisão non-regression.

Risco: beeps irritantes degradarem a percepção do jogo.
Mitigação: volumes default conservadores (SFX 60%), categorias curtas (<300ms),
footsteps OFF no v1.
```

## Rollback

```text
Remover o wiring do GameBootstrap e a pasta Audio/** = jogo volta ao silêncio atual.
Nenhum sistema de gameplay é tocado; nada persiste. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar GameBootstrap/AudioListener/GameAudioSettings/lista de eventos v1.
- [ ] T002 — AudioManager + canais + pool + ProceduralSfxFactory + testes.
- [ ] T003 — SfxEventBridge + tabela evento→categoria + cooldown + fallback + testes.
- [ ] T004 — Wiring bootstrap + binding de volume (se F56) + csproj;
        run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (mapeamento, cooldown, ganho, parâmetros de onda)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — audição real)
- Requires regression test: YES (nenhum sistema de gameplay alterado — diff prova)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano ouvindo SFX
  distintos em combate (hit/perfect block/quebra de postura) e UI (toast/save)

## Definition of Done

```text
AudioManager com canais e volume vivo via bootstrap; ~17 eventos audíveis mapeados no
SfxEventBridge (único call site); clipes placeholder procedurais distintos por
categoria com cache; pool de vozes + cooldown anti-spam; fallback silencioso (gameplay
jamais bloqueado por asset ausente); zero call site de áudio fora de Audio/**;
builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Nenhum arquivo de gameplay modificado (Combat/Farm/UI/... intactos — diff é a prova).
Nenhum evento novo/alterado; unsubscribe simétrico em todas as assinaturas.
Nenhum asset de áudio importado; nenhum .asset/.prefab/.unity tocado.
Nenhum AudioSource criado fora do pool do AudioManager.
Namespace CindarsHope.Audio (Debug/Temp proibidos); zero GameObject.Find em runtime.
```


---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v3.0.md, decisão 4.9)

```text
MUSIC STATE + TRANSIÇÕES COM CROSSFADE PLACEHOLDER (decisão 4.9). A F58 passa a incluir, além dos
hooks de SFX já especificados, uma máquina de ESTADO DE MÚSICA mínima — faixas reais ficam para a
fase de áudio; aqui só a infraestrutura + placeholders procedurais.

1. ENUM MusicState (ids estáveis, save-safe — embora não seja persistido):
   Calmo · Combate · Boss · Festival.
   - Calmo = default (mundo/fazenda/cidade fora de combate);
   - Combate = ao menos um inimigo hostil engajado com o jogador;
   - Boss = luta de boss/miniboss ativa (vence Combate enquanto durar);
   - Festival = durante evento de festival (vence Calmo; Boss/Combate ainda podem sobrepor —
     resolver prioridade na tabela abaixo).

2. PRIORIDADE DE ESTADO (resolução determinística quando mais de um se aplica):
   Boss > Combate > Festival > Calmo.
   - A transição de volta ocorre quando a condição de maior prioridade cessa (ex.: boss morto →
     se ainda há combate, Combate; senão Festival se ativo; senão Calmo).

3. EVENTOS DE TRANSIÇÃO (via GameEventBus — coerente com a regra de comunicação por eventos):
   - O AudioManager/SfxEventBridge DERIVA o MusicState a partir de eventos JÁ publicados sempre que
     possível (ex.: EnemyKilledEvent, eventos de combate/boss/festival existentes). Auditar na
     Fase 0 quais eventos já sinalizam entrada/saída de combate, boss e festival.
   - SE a derivação não for possível a partir dos eventos existentes, esta spec PODE publicar UM
     evento próprio de transição de música — MusicStateChangedEvent(MusicState previous,
     MusicState next) — documentado no report (exceção explícita à regra "adds events: NO" desta
     spec, autorizada por esta emenda; payload simples, sem refs Unity). Nenhum sistema de gameplay
     assina esse evento — ele é interno ao áudio.
   - NÃO criar segundo caminho de transição: a troca de faixa nasce SOMENTE da mudança de
     MusicState (derivada de evento ou do evento próprio acima), nunca de chamada direta de
     gameplay.

4. CROSSFADE PLACEHOLDER: a troca entre estados faz CROSSFADE (fade-out da faixa atual + fade-in da
   nova) numa janela curta (~0,5–1,0s). No v1 as "faixas" são PLACEHOLDERS procedurais por estado
   (drones/acordes distintos gerados pelo ProceduralSfxFactory/equivalente — Calmo suave, Combate
   tenso, Boss mais grave/denso, Festival alegre), ou silêncio se uma categoria não tiver
   placeholder. O crossfade NUNCA lança exceção nem bloqueia gameplay (fallback silencioso, padrão
   da spec).

5. ESCOPO/CONTRATOS:
   - PlayMusic(MusicState)/StopMusic já previstos passam a operar por MusicState (não por trackId
     livre); o canal Music do AudioManager hospeda o crossfade (dois AudioSources de música em
     reuso para o cruzamento — sem Instantiate por troca).
   - Volume da música usa o canal Music (Master × Música) — agora alimentado pelos TRÊS volumes da
     aba Sistema F56 (Master/SFX/Música — ver emenda V3 da F56); se F56 ausente, default local.
   - Faixas reais e composição: fase de áudio (a máquina de estado + a tabela estado→placeholder
     são o contrato de entrega).

6. FORA DE ESCOPO (mantido): faixas/música composta reais, stingers por evento, ducking/mixer,
   layering adaptativo (stems). Só estado + crossfade simples + placeholders.

7. TESTING (EditMode): resolução de prioridade da máquina de estado (Boss>Combate>Festival>Calmo,
   incluindo o retorno ao cessar a condição dominante), mapeamento estado→placeholder presente para
   os 4 estados, e a lógica de crossfade (curva/duração) em classe pura sem depender de AudioSource;
   fallback silencioso sem exceção quando falta placeholder. Audição real (faixas distintas, sem
   estouro na transição) fica no cenário humano do lote.

NOVO CA:

### CA-5 Estado de música com transições por crossfade
- O MusicState (Calmo/Combate/Boss/Festival) é derivado de eventos (ou do evento próprio
  documentado) com prioridade determinística Boss>Combate>Festival>Calmo; entrar/sair de combate,
  boss e festival troca a faixa-placeholder com crossfade curto, sem exceção e sem bloquear
  gameplay; faltando placeholder, silêncio + log 1×.
- Evidência: EditMode tests (resolução de prioridade, mapeamento estado→placeholder, lógica de
  crossfade em classe pura, fallback silencioso) + cenário humano (passar por calmo→combate→boss→
  retorno e ouvir transições suaves).
```
