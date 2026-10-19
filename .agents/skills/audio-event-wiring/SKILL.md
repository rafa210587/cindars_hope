---
name: audio-event-wiring
description: Wiring de SFX e música por contexto via GameEventBus — nunca AudioSource direto no código de gameplay. Usar quando uma spec de combat/farm/UI precisar de feedback sonoro ou quando fable_58 (audio system) for o sistema de apoio.
---

# Skill: Wiring de Áudio por Evento

O sistema de áudio do projeto (`fable_58`) foi construído sobre o princípio de que **nenhum código de gameplay chama `AudioSource.Play()` diretamente** — a mesma filosofia da rule `event-bus-only-gameplay-communication`. O `SfxEventBridge` (singleton DontDestroyOnLoad, bootstrap `RuntimeInitializeOnLoadMethod`) assina os eventos audíveis e é o **único call site** de SFX do jogo.

## Quando usar

- Qualquer spec de combat, farm, UI ou NPC que precise de som de feedback.
- Quando a spec mencionar "tocar SFX", "feedback sonoro", "música de combate", "crossfade de música".
- Waves de polish (UI canvas, game feel, fable_71).
- Ao adicionar um novo evento de gameplay que deve ter som associado.

## Por que existe

Gameplay chamando `AudioSource` diretamente quebraria o contrato de event-bus, criaria dependências de Unity em código de lógica pura e tornaria o audio untestable. O `SfxEventBridge` isola todo o wiring em um único lugar.

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `AudioManager` (`CindarsHope.Audio`) | Host de áudio (singleton); pool de vozes SFX round-robin; dois `AudioSource` para crossfade de música |
| `SfxEventBridge` | ÚNICO call site de SFX; assina eventos via `GameEventBus`; aplica `SfxCooldownGate`; deriva `MusicState` |
| `SfxEventMap` | Tabela estática `Type → SfxCategory` — fonte única de verdade do mapeamento evento→som |
| `SfxCooldownGate` | Anti-spam por categoria: garante intervalo mínimo entre toques da mesma categoria |
| `SfxCategory` | Enum de categorias de SFX (`Hit`, `Harvest`, `Craft`, `Pickup`, `LevelUp`, `UiToast`, `PerfectBlock`, …) |
| `MusicStateResolver` | Máquina de estado pura (sem Unity); prioridade Boss > Combate > Festival > Calmo |
| `MusicState` | Enum: `Calmo`, `Combate`, `Boss`, `Festival` |
| `MusicStateChangedEvent` | Único evento próprio do bridge; dispara o crossfade |
| `MusicCrossfade` | Constantes de duração (default `MusicCrossfade.DefaultDurationSeconds`) |
| `AudioGainCalculator` | Conversão de volume 0–100 para volume linear Unity |
| `ProceduralSfxFactory` | Clipes placeholder procedurais (sin wave / noise) para sons ainda sem asset real |

## Procedimento

### Adicionar SFX para um evento de gameplay existente

1. Confirme que o evento já existe no `GameEventBus` (não crie evento de gameplay aqui).
2. Em `SfxEventMap`, adicione a entrada no dicionário estático `Map`:
   ```csharp
   { typeof(MeuNovoEvento), SfxCategory.SuaCategoria },
   ```
3. Em `SfxEventBridge.SubscribeAll()`, adicione a assinatura:
   ```csharp
   Add(GameEventBus.Subscribe<MeuNovoEvento>(_ => PlayMapped<MeuNovoEvento>()));
   ```
4. Se a categoria for nova, adicione-a ao enum `SfxCategory` (valor único).

### Dirigir música por contexto (MusicState)

O `SfxEventBridge` já deriva `MusicState` automaticamente de `EnemySpawnedEvent`/`EnemySeenEvent` (→ Combate) e `EnemyKilledEvent` (desengajamento) e `CaveBossDefeatedEvent` (Boss off). Para novos contextos (festival, área especial):

```csharp
// Em SfxEventBridge.SubscribeAll():
Add(GameEventBus.Subscribe<FestivalStartedEvent>(_ => { _musicResolver.SetFestivalActive(true); PushMusicStateIfChanged(); }));
Add(GameEventBus.Subscribe<FestivalEndedEvent>(_ => { _musicResolver.SetFestivalActive(false); PushMusicStateIfChanged(); }));
```

`PushMusicStateIfChanged()` publica `MusicStateChangedEvent` e chama `AudioManager.Instance?.PlayMusic(next)` — é o único caminho de troca de faixa.

### Testar sem Play Mode

`SfxEventMap` é classe pura → testável em EditMode:
```csharp
Assert.AreNotEqual(SfxCategory.None, SfxEventMap.CategoryFor<MeuNovoEvento>());
```

`MusicStateResolver` também é pura:
```csharp
var r = new MusicStateResolver();
r.EnemyEngaged();
Assert.AreEqual(MusicState.Combate, r.CurrentState);
```

## Regras

- **NUNCA** chamar `AudioManager.Instance.PlaySfx()` ou `AudioSource.Play()` de código de gameplay.
- Gameplay **publica evento** no `GameEventBus`; `SfxEventBridge` reage.
- **NÃO criar** um segundo bridge, um `AudioController` paralelo ou qualquer component que leia input e toque som diretamente.
- `SfxCooldownGate` é obrigatório (já aplicado no bridge) — **não** contornar com um `AudioSource` separado para "não ter o cooldown".
- Clipes sem asset real → usar `ProceduralSfxFactory` (fallback silencioso + log 1×, nunca exceção).
- `AudioManager` não é referenciado por código de gameplay — apenas pelo bridge e pelo bootstrap de áudio.

## Saída esperada (checklist de fechamento)

```text
SFX adicionado via SfxEventMap + SfxEventBridge: SIM/NÃO
Categoria de SFX: <nome>
Evento que dispara: <EventType>
Teste EditMode de SfxEventMap.CategoryFor<T>: SIM/NÃO APLICÁVEL
MusicState alterado: SIM (via MusicStateResolver) / NÃO APLICÁVEL
AudioSource direto em código de gameplay: NENHUM
```

## Relacionados

- `(rule: unity-architecture)` — gameplay só via GameEventBus (mesma invariante)
- `(skill: game-feel-checklist)` — checklist de polish que inclui SFX; esta skill fecha o débito de som
- `(skill: object-pooling-pattern)` — pool de vozes SFX já é interno ao `AudioManager`
- `(skill: data-catalog-authoring)` — mapear SfxCategory → asset via SO de catálogo
- `(skill: editmode-test-authoring)` — testar `SfxEventMap` e `MusicStateResolver` (classes puras)
