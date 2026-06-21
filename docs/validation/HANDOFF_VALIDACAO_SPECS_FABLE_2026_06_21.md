# Handoff — Validação das specs FABLE (estado em 2026-06-21)

> Prompt/contexto para uma nova sessão **continuar a validação** das ~73 specs FABLE já
> implementadas. Leia este arquivo inteiro antes de tocar em qualquer coisa. Branch: `dev`.

---

## 0. Como prosseguir (resumo de 1 parágrafo)

Todas as ~73 specs FABLE já foram **implementadas e commitadas**. A fase atual é
**validação humana em Play Mode** pelo dono (Rafa), que testa no Unity e reporta bugs; a
sessão Claude corrige um bug de cada vez (ler arquivo → editar C# → build gate → commit por
path explícito em PT-BR sem acentos → **nunca push**). Não reimplemente specs, não promova
specs para `implementados/`, não rode geradores em batch. O trabalho é **polish + bugfix +
validação** dirigido pelo teste do dono. Use o checklist humano (seção 4) como roteiro.

---

## 1. Identidade do projeto e ambiente

- **Cindar's Hope** — RPG 2D pixel art + farm sim em Unity LTS/C#. Mundos: Vaalara / Cindar's
  Hope / Dornecia.
- **OS:** Windows 11, **PowerShell** (sintaxe PS, checar `$LASTEXITCODE`). Bash tool existe mas
  o projeto é Windows-first.
- **Sem licença Unity no ambiente do agente** — não há como rodar batchmode/Play Mode aqui.
  A materialização de `.asset` e o teste de gameplay acontecem **na máquina do dono**, quando
  ele roda os menus do Unity. Validação automatizada aqui = `dotnet build` + `validate_docs.ps1`.

### Regras de processo NÃO-NEGOCIÁVEIS (estão em CLAUDE.md / .claude/rules)

- **Commits em PT-BR sem acentos** + trailer:
  `Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>`
- **Commit por path EXPLÍCITO** (`git add -- "caminho/arquivo.cs"`). Nunca `git add .`/`-A`.
- **NUNCA** `git push` (e nunca git destrutivo sem autorização por instância).
- **Nunca commitar:** `.unity` / `.asset` / `.meta` / `.prefab` alheios, `*.csproj`
  (gitignored — edite local pra buildar, nunca commite), `tools/`, `.claude/`.
- **Nunca editar YAML manualmente** de `.unity`/`.prefab`/`.asset` (use geradores/SerializedObject).
  Exceção pontual já em uso: `GameTimeBalance.asset` (dado numérico simples — ver seção 3).
- **Arquitetura runtime:** sem `GameObject.Find`/`FindObjectOfType` em gameplay (o idiom
  `FindAnyObjectByType` nos `*RuntimeBootstrap` de self-wiring é tolerado, não copie pra código
  novo); comunicação de gameplay só via `GameEventBus`; save DTOs só com tipos simples + IDs;
  falhas via `bool`/`FailureReason` (sem no-op silencioso).

### Gate de build (rodar após QUALQUER mudança de C#)

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore -clp:ErrorsOnly
# Se NETSDK1004 (assets ausentes) -> restaurar primeiro:
dotnet build .\Assembly-CSharp.csproj -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
# Editor assembly tambem deve passar quando mexer em Assets/_Game/Scripts/Editor:
dotnet build .\Assembly-CSharp-Editor.csproj -clp:ErrorsOnly
# Docs:
.\tools\docs\validate_docs.ps1   # exit 0
```

`run_strict_validation.ps1` faz tudo e escreve `docs/validation/LAST_STRICT_VALIDATION_RESULT.json`.

---

## 2. Os 4 menus do Unity (o dono usa estes)

Consolidados em `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`, root-level `CindarsHope/`:

| Menu | Para quê |
|------|----------|
| **Inicializar Projeto** | Roda todos os geradores idempotentes + repara starting items (Lágrima da Deusa, flechas). É o "preparar tudo". |
| **Validar Projeto** | Roda os ~60 validators de conteúdo (IDs duplicados, refs pendentes, ranges). |
| **Reparar e Reconstruir** | Geradores + remoção durável de assets órfãos/legados + reparo de starting items. |
| **Build Standalone Windows** | Build do jogo. |

Geradores são **idempotentes** (load-or-create por id). Os `.asset` só materializam quando o
dono roda os menus no Unity.

---

## 3. PENDÊNCIAS TEMPORÁRIAS a reverter quando o dono confirmar (IMPORTANTE)

Duas coisas estão em estado "temporário para validação". **Quando o dono confirmar que está OK,
reverter:**

1. **Logs `[Music]` de diagnóstico** em
   `Assets/_Game/Scripts/Audio/SfxEventBridge.cs` (commit `ad31573d`). São `Debug.Log("[Music]...")`
   em Bootstrap / TryApplySceneAmbient / PushMusicStateIfChanged. **Remover** quando o dono
   confirmar que a música troca por cena (Caverna/Fazenda/Cidade distintas). Eles já provaram a
   causa raiz (combate não zerava ao sair da cena — corrigido em `6b686cff`).

2. **`Assets/_Game/Data/Config/GameTimeBalance.asset`** está com
   `_dayDurationMinutes: 3 / _nightDurationMinutes: 3` (TEMP, pra validar rápido). O **padrão do
   código é 20/10**. Reverter pro valor que o dono escolher quando a validação de tempo acabar.
   (Esse `.asset` é dado numérico simples — edição autorizada; mas confirme com o dono antes de
   commitar mudança nele, e lembre que `.asset` normalmente não vai pro commit.)

---

## 4. Roteiro de validação humana

Já existe um checklist passo-a-passo gerado:
**`docs/validation/HUMAN_VALIDATION_CHECKLIST_2026_06_21.md`** (commit `d7f6d1c7`). Use-o como
roteiro. O fluxo por bug é sempre: dono testa no Unity → reporta → você reproduz pelo código →
corrige a menor mudança possível → gate de build → commit por path → dono re-testa.

---

## 5. Sistemas que foram mexidos nesta run (mapa rápido p/ achar bugs)

### Morte / respawn (mais recente, acabou de ser corrigido)
- `Assets/_Game/Scripts/Player/Death/PlayerDeathController.cs` — self-bootstrap; escuta
  `HPChangedEvent`; dispara `PlayerDiedEvent` com a cena capturada no momento da morte; rearma
  `_isDead` quando HP volta > 0. Lógica pura testável em `PlayerDeathDecision.cs` (+ EditMode tests).
- `Assets/_Game/Scripts/Cave/Death/DeathSystemBootstrap.cs` — self-bootstrap; cria o corpo na
  caverna; **NÃO** respawna mais sozinho (a escolha é do jogador via tela de morte).
- `Assets/_Game/Scripts/Cave/Death/CaveDeathResolver.cs` — **acabou de ser corrigido** (`4de9fbf7`):
  tolera `CaveRunManager` null fora da caverna; `SetCaveRunManager` pega a referência viva na
  morte; `ResolveCaveDeath` guarda contra run ausente. (Antes lançava `ArgumentNullException` no
  boot e derrubava todo o sistema de morte.)
- `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` + `DeathScreenViewModel.cs` —
  overlay "Voce Morreu" (sortingOrder 900, bg escuro), 2 botões: **Reviver com Lágrima da Deusa**
  (consome `item_goddess_tear`, SetHP(MaxHP)) e **Respawnar na Fonte da Anya** (cross-cena).
  Pausa via `Time.timeScale=0` + `ModalManager.PushModal(ModalType.Death)`.
- `Assets/_Game/Scripts/Player/Death/AnyaFountainRespawnFlow.cs` — respawn cross-cena: se há
  `AnyaFountain` na cena ativa, teleporta; senão carrega FarmScene via `SceneTransitionRouter`
  (âncora `spawn_farm_default`) e re-resolve a fonte no sceneLoaded; anti-softlock revive in place.
- Item **Lágrima da Deusa** = `item_goddess_tear`; 2 no starter (pra testar). Reparado por
  `Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs`.

### Arco / flecha (`7b6a18d1`, fable_48)
- Flecha (`item_ammo_arrow_basic`) é equipável em LeftHand/RightHand; usar o arco consome flecha
  do inventário e lança o projétil (`ProjectileSpawnService`); 30 flechas no starter.
- `Assets/_Game/Scripts/Combat/BowArrowAttackService.cs` (TryFire), `ArrowBallisticsResolver.cs`
  (alias arrow_basic → wood), `RepairPlayerStartingItems.cs` (EnsureStartingArrows).

### Áudio / música (vários commits cc85cff0 → 6b686cff)
- `Assets/_Game/Scripts/Audio/SfxEventBridge.cs` — driver de música por cena (Update faz poll de
  `GetActiveScene().name` porque LoadSceneInPlayMode não dispara sceneLoaded confiável); zera
  combate em `SceneTransitionStartedEvent`. **Contém os logs [Music] temporários (seção 3).**
- `ProceduralSfxSpec.cs` (MusicPhrase, frases estilo-FF, MusicState Fazenda/Cidade/Caverna),
  `ProceduralSfxFactory.cs` (FillMusicSamples + guard one-shot de log), `MusicStateResolver.cs`
  (sceneAmbient floor; prioridade Boss > Combate > Festival > sceneAmbient).

### HUD (`78296f5b`)
- `Assets/_Game/Scripts/UI/HUD/GameplayHudTextOverlay.cs` — HUD de texto code-built: relógio
  ("Dia N — HH:MM (Dia/Noite)"), HP/Stamina/Fome, Ouro, prompt de interação. As specs de UI
  estavam `DEFERRED_UI_VISUAL` (entregaram ViewModel mas não o Canvas visível); este overlay
  fecha o gap visível.

### Validators / editor
- `Assets/_Game/Scripts/Editor/Validation/ValidateSceneTransitions.cs` — corrigido (`cda453e5`)
  para restaurar a cena aberta do dono (antes deixava o Editor numa cena vazia → "No cameras
  rendering" → "o jogo não abre depois de validar").
- `Assets/_Game/Scripts/Editor/Validation/RepairPlayerStartingItems.cs` — Lágrima + flechas,
  append-idempotente via SerializedObject, remove StartingItems null.

---

## 6. Dívidas/observações conhecidas

- **DEFERRED_UI_VISUAL:** várias specs de UI entregaram só o ViewModel/projection em C#, sem o
  Canvas visível. O `GameplayHudTextOverlay` cobriu o HUD principal; outras telas podem aparecer
  "headless" no teste. Se o dono reclamar de uma UI invisível, é provável que seja isto.
- **Self-bootstrap idiom:** ~25 sistemas usam `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` +
  DontDestroyOnLoad + singleton guard + coroutine `BindWhenReady` que espera `GameBootstrap.Instance`
  e managers. Cuidado: o que é esperado no `BindWhenReady` precisa existir no boot (foi exatamente
  a causa do bug de morte — esperava PlayerManager/CorpseRecoveryManager mas usava CaveRunManager,
  que é null fora da caverna).
- **Scene loading:** transições usam `EditorSceneManager.LoadSceneInPlayMode(Single)` no editor /
  `SceneManager.LoadScene` no build — **não** dispara `sceneLoaded`/`activeSceneChanged` de forma
  confiável. Quem precisa reagir a troca de cena deve fazer **poll** de `GetActiveScene().name`.

---

## 7. Primeiro passo da nova sessão

1. `git log --oneline -10` e `git status --short` — confirmar branch `dev` e que não há `.cs`
   pendente inesperado.
2. Ler `docs/validation/HUMAN_VALIDATION_CHECKLIST_2026_06_21.md`.
3. Perguntar ao dono qual item do checklist ele está testando / qual bug apareceu, e seguir o
   fluxo da seção 0.
4. Lembrar das pendências da seção 3 (logs [Music] + GameTimeBalance) — **só reverter quando o
   dono confirmar**.
