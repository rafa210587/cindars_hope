# SPEC — Animação de Ataque por Arquétipo de Arma (Player)

> **Spec ID:** `fable_84_spec_player_attack_anim_archetype`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Animação & Combat Feel
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Player / Combat
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs que não tocam PlayerWalkAnimator, PlayerMeleeSwingEvent, PlayerAttackController.Attacks.cs ou pasta Resources/PlayerSprites/
> **Must not run with:** qualquer outra spec que altere PlayerMeleeSwingEvent, PlayerWalkAnimator ou Resources/PlayerSprites/
> **Repo lock scope:** `Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs`, `Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs`, `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs`, `Assets/_Game/Resources/PlayerSprites/attack*/`
> **Depends on:**
> - WeaponType enum estável (Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs) — já existe e tem os valores pinados
> **Blocks:**
> - Specs futuras de arte de ataque do player (precisam da pasta attack_{archetype}/)
> - Paper-doll de equipamentos (fora de escopo desta spec, mas depende do contrato de diretório)
> **Scope:** Trocar o filtro `if(!IsSword)` de animação por despacho por arquétipo, adicionando enum `PlayerAttackAnimArchetype` no Core, reescrevendo o carregamento de frames de ataque no PlayerWalkAnimator e o publisher em PlayerAttackController.
> **Out of scope:** Paper-doll de equipamentos visíveis no sprite, animações de inimigos, animação de cast de magia (PlayerBowShootEvent não muda), arte final dos arquétipos novos (Heavy/Thrust/Dagger/Cast), qualquer mudança de save, qualquer mudança de cena/prefab.

---

## 5. Contexto

### Por que esta spec existe

Hoje o player tem animação de ataque somente para espada e arco. O filtro em `PlayerWalkAnimator.OnMeleeSwing` (linha 132) faz `if (!evt.IsSword) return;`: qualquer arma que não seja espada não aciona nenhuma animação de ataque, o que é aceitável como MVP mas precisa ser superado antes de adicionar arquétipos de arma ao catálogo (Axe/Hammer/Spear/Dagger/Staff/Wand chegaram via fable_32).

### Estado atual do repo

- `PlayerMeleeSwingEvent` (Core/Events) carrega apenas primitivos — `bool IsSword` — para não acoplar Core a Combat. O comentário do arquivo documenta isso explicitamente.
- `PlayerWalkAnimator` carrega frames de `Resources/PlayerSprites/attack/{dir}/` (8 direções) na inicialização e aciona o one-shot apenas se `IsSword == true`. Frames de arco vivem em `bow/{dir}/`.
- `PlayerAttackController.Attacks.cs` linha 117 publica `new PlayerMeleeSwingEvent(swingDirection, cooldown, weapon.Type == WeaponType.Sword)`.
- `WeaponType` (WeaponDataSO.cs, linhas 66–85) tem os valores: None=0, Sword=1, Spear=2, Axe=3, Bow=4, Staff=5, Dagger=6, Hammer=100, Wand=101, Tool=102. Valores fixados explicitamente para não driftar.
- Não existe enum `PlayerAttackAnimArchetype` no repositório.

### O que destrava

Permite que armas novas (Axe, Hammer, Spear, Dagger, Staff, Wand) tenham animação de ataque quando a arte de cada arquétipo for entregue, sem nova mudança de código. Fallback automático em Sword mantém jogabilidade para arquétipos sem arte.

### Trilha futura (fora de escopo)

Paper-doll (corpo + item separados por camada) para equipamentos visíveis no sprite. Esta spec usa "arquétipo fundido" (sprite de corpo + arma no mesmo frame), que é o caminho escolhido para o player principal neste momento. Paper-doll exigiria múltiplos SpriteRenderers sobrepostos e uma pipeline de arte completamente distinta — reservado para personagens futuros.

---

## 6. Problema

Sem despacho por arquétipo, cada novo tipo de arma adicionado ao catálogo exige edição manual no `PlayerWalkAnimator` para acionar a animação correspondente. Com o bool `IsSword`, qualquer arma que não é espada fica muda: o player ataca mas o sprite não reage, quebrando o game feel de combate.

---

## 7. Objetivo

Ao final desta spec, o projeto deve ter um sistema de animação de ataque extensível por arquétipo (Sword, Bow, Heavy, Thrust, Dagger, Cast), publicado via evento no Core com tipo neutro, carregando frames de pastas nomeadas por arquétipo, permitindo que qualquer novo tipo de arma seja mapeado a um arquétipo sem edição no código de animação, sem alterar save/cenas/prefabs e sem quebrar a animação existente de espada e arco.

---

## 8. Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs
Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs
Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
.claude/rules/unity-architecture.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

---

## 9. Estado atual do repo

| Artefato | Estado |
|---|---|
| `PlayerMeleeSwingEvent.cs` | Existe. Campo `IsSword bool` a ser substituído por `Archetype PlayerAttackAnimArchetype`. |
| `PlayerWalkAnimator.cs` | Existe. `OnMeleeSwing` faz `if (!evt.IsSword) return`. Pasta carregada: `attack/{dir}`. |
| `PlayerAttackController.Attacks.cs` | Existe. Linha 117 publica `PlayerMeleeSwingEvent` com `weapon.Type == WeaponType.Sword`. |
| `PlayerAttackAnimArchetype` (enum) | NÃO existe. A ser criado em Core. |
| `WeaponAttackArchetypeMapper` (classe de mapeamento) | NÃO existe. A ser criado em Combat. |
| `Resources/PlayerSprites/attack/` | Existe (sprites de espada). A ser renomeada para `attack_sword/`. |
| `Resources/PlayerSprites/attack_heavy/` etc. | NÃO existem. Arte é dependência externa (ver §Fora de escopo). |
| `Resources/PlayerSprites/bow/` | Existe. Não muda nesta spec. |

---

## 10. User stories / engineering stories

- Como agente executor, quero um enum `PlayerAttackAnimArchetype` no namespace Core para que o evento `PlayerMeleeSwingEvent` carregue o arquétipo sem importar tipos de Combat.
- Como PlayerWalkAnimator, quero carregar frames de `attack_{archetype}/{dir}/` para despachar a animação certa por tipo de arma sem if-chains.
- Como PlayerAttackController, quero computar o arquétipo a partir do `WeaponType` e publicá-lo no evento, mantendo o Core ignorante de Combat.
- Como QA, quero que uma espada continue animando e que uma arma sem arte caia em fallback Sword sem crashar.
- Como artista, quero que a convenção de pasta `attack_{archetype}/` esteja documentada para poder entregar frames de Heavy/Thrust/Dagger/Cast sem mudar código.

---

## 11. Escopo

Inclui:

- Enum `PlayerAttackAnimArchetype { Sword, Bow, Heavy, Thrust, Dagger, Cast }` no namespace `CindarsHope.Core.Events` (mesmo arquivo ou arquivo adjacente ao `PlayerMeleeSwingEvent`).
- Substituição de `bool IsSword` por `PlayerAttackAnimArchetype Archetype` em `PlayerMeleeSwingEvent`.
- Classe `WeaponAttackArchetypeMapper` em `CindarsHope.Combat` com método `public static PlayerAttackAnimArchetype FromWeaponType(WeaponType type)` implementando o mapa completo (ver §Contratos).
- Atualização do publisher em `PlayerAttackController.Attacks.cs` linha 117: computar arquétipo via `WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type)` e passar ao construtor do evento.
- Atualização de `PlayerWalkAnimator`: carregar `Sprite[][]` por arquétipo (dict ou array indexado por enum), despachar por `evt.Archetype` em `OnMeleeSwing`, com fallback para Sword se a coleção do arquétipo estiver vazia.
- Renomear estrutura de diretório de assets: `Resources/PlayerSprites/attack/` → `Resources/PlayerSprites/attack_sword/`. (Os arquivos `.png` e `.meta` já existem; a operação é mover a pasta. Instruir o humano a fazer via Unity Editor para preservar GUIDs, OU fazê-lo via script utilitário que chame `AssetDatabase.MoveAsset`.)
- EditMode test para `WeaponAttackArchetypeMapper.FromWeaponType` cobrindo todos os 10 valores de `WeaponType`.
- Execution report em `docs/validation/fable_84_execution_report.md`.

---

## 12. Fora de escopo

Não inclui:

- Arte de ataque para os 4 arquétipos novos (Heavy, Thrust, Dagger, Cast) — dependência externa, entregue pelo pipeline GPT/WALK_PIPELINE.md pelo artista.
- Paper-doll de equipamentos visíveis no sprite do player.
- Animação de cast de magia vinculada a `PlayerBowShootEvent` ou a um evento novo de spell — escopo futuro.
- Qualquer alteração em `PlayerBowShootEvent.cs`.
- Qualquer mudança em cenas (`.unity`), prefabs (`.prefab`) ou ScriptableObjects (`.asset`) além da renomeação de pasta de sprites.
- Mudança de save schema, DTOs ou seções de save.
- Balance de dano ou stats de arma.
- Animações de inimigos.

---

## 13. Regras de não duplicação

- Não criar segundo sistema de animação de ataque paralelo ao existente em `PlayerWalkAnimator`.
- Não mover o enum `PlayerAttackAnimArchetype` para `Combat` — ele deve ficar em `Core` para não acoplar Core a Combat na direção proibida.
- Não criar `WeaponType`-to-archetype mapping em múltiplos lugares; o mapper canônico é `WeaponAttackArchetypeMapper` em Combat.
- Não recriar `PlayerMeleeSwingEvent` — apenas alterar o campo existente.
- Não usar `GameObject.Find` ou `FindObjectOfType` em nenhum código novo (rule unity-architecture).

---

## 14. Critérios de aceite

### 14.1 Enum de arquétipo no Core

- Arquivo `Assets/_Game/Scripts/Core/Events/PlayerAttackAnimArchetype.cs` (ou inline em `PlayerMeleeSwingEvent.cs`) existe.
- Namespace: `CindarsHope.Core.Events`.
- Valores: `Sword = 0, Bow = 1, Heavy = 2, Thrust = 3, Dagger = 4, Cast = 5` (valores explícitos recomendados para estabilidade).
- Nenhum `using CindarsHope.Combat` no arquivo que define o enum.

### 14.2 PlayerMeleeSwingEvent atualizado

- Campo `bool IsSword` removido; campo `PlayerAttackAnimArchetype Archetype` presente.
- Construtor atualizado: `PlayerMeleeSwingEvent(Vector2 direction, float duration, PlayerAttackAnimArchetype archetype)`.
- Evidência: `grep -r "IsSword" Assets/_Game/Scripts/` retorna zero resultados.

### 14.3 WeaponAttackArchetypeMapper implementa mapa completo

- Classe existe em `Assets/_Game/Scripts/Combat/` (namespace `CindarsHope.Combat`).
- Mapa exato:

| WeaponType | Arquétipo |
|---|---|
| Sword | Sword |
| Bow | Bow |
| Axe | Heavy |
| Hammer | Heavy |
| Spear | Thrust |
| Dagger | Dagger |
| Staff | Cast |
| Wand | Cast |
| Tool | Sword (fallback) |
| None | Sword (fallback) |

- Valores não listados (improvável mas possível via enum extensão futura) → fallback Sword com `Debug.LogWarning`.

### 14.4 Publisher atualizado

- `PlayerAttackController.Attacks.cs` linha ~117: `new PlayerMeleeSwingEvent(swingDirection, cooldown, weapon.Type == WeaponType.Sword)` substituído por `new PlayerMeleeSwingEvent(swingDirection, cooldown, WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type))`.
- Nenhuma referência a `IsSword` permanece em `PlayerAttackController.Attacks.cs`.

### 14.5 PlayerWalkAnimator despacha por arquétipo

- Carregamento de frames em `LoadAllFrames` usa pastas `attack_{archetype}/{dir}/` derivadas do enum (sem literais soltos — o path é construído por `"attack_" + archetype.ToString().ToLowerInvariant()`).
- `OnMeleeSwing` despacha `BeginOneShot` com o set correto para `evt.Archetype`.
- Fallback: se o set do arquétipo recebido estiver vazio ou nulo, usa `_attackFramesByArchetype[PlayerAttackAnimArchetype.Sword]`.
- Sem hardcoded `if (!evt.IsSword) return;`.

### 14.6 Pasta renomeada

- `Resources/PlayerSprites/attack_sword/` existe com os sprites originais de espada.
- `Resources/PlayerSprites/attack/` não existe mais (verificável por ausência de assets com esse path).
- Sprites de espada continuam funcionando após a renomeação (animação de espada visível em Play Mode).

### 14.7 EditMode tests

- Arquivo de teste em `Assets/_Game/Tests/EditMode/Combat/WeaponAttackArchetypeMapperTests.cs`.
- Cobre todos os 10 valores de `WeaponType` com `Assert.AreEqual` para o arquétipo esperado.
- Roda sem Play Mode.

### 14.8 Build compila sem erros

- `dotnet build Assembly-CSharp.csproj --no-restore` exit code 0.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` exit code 0.

---

## 15. Arquitetura alvo

```
Assets/_Game/Scripts/Core/Events/
  PlayerAttackAnimArchetype.cs      ← enum novo (Core — sem dep de Combat)
  PlayerMeleeSwingEvent.cs          ← campo IsSword → Archetype (alterado)

Assets/_Game/Scripts/Combat/
  WeaponAttackArchetypeMapper.cs    ← mapeamento WeaponType → PlayerAttackAnimArchetype (novo)
  PlayerAttackController.Attacks.cs ← publisher atualizado (linha ~117)

Assets/_Game/Scripts/Player/
  PlayerWalkAnimator.cs             ← LoadAllFrames + OnMeleeSwing reescritos (alterado)

Assets/_Game/Resources/PlayerSprites/
  attack_sword/{right,upright,up,upleft,left,downleft,down,downright}/  ← renomeado de attack/
  attack_heavy/    ← pasta vazia criada ou documentada como dependência externa
  attack_thrust/   ← idem
  attack_dagger/   ← idem
  attack_cast/     ← idem

Assets/_Game/Tests/EditMode/Combat/
  WeaponAttackArchetypeMapperTests.cs  ← novo

docs/validation/
  fable_84_execution_report.md
```

---

## 16. Contratos, dados e eventos

### 16.1 Data contracts

Enum `PlayerAttackAnimArchetype`:

```csharp
namespace CindarsHope.Core.Events
{
    public enum PlayerAttackAnimArchetype
    {
        Sword  = 0,
        Bow    = 1,
        Heavy  = 2,
        Thrust = 3,
        Dagger = 4,
        Cast   = 5,
    }
}
```

Justificativa de localização: o enum deve viver em `Core.Events` porque é consumido por `PlayerMeleeSwingEvent` (Core) e por `PlayerWalkAnimator` (Player, que referencia Core). Se vivesse em Combat, Player precisaria referenciar Combat, violando a regra de não acoplar Core/Player a Combat (rule unity-architecture: comunicação por GameEventBus com tipos neutros no Core).

### 16.2 Runtime contracts

`WeaponAttackArchetypeMapper` (Combat) — método público estático:

```csharp
namespace CindarsHope.Combat
{
    public static class WeaponAttackArchetypeMapper
    {
        public static CindarsHope.Core.Events.PlayerAttackAnimArchetype
            FromWeaponType(WeaponType type)
        {
            // mapa completo conforme §14.3
        }
    }
}
```

`PlayerWalkAnimator` — campo de frames indexado por arquétipo:

```csharp
// Sugestão de implementação (o executor pode usar Dictionary ou array indexado)
private readonly Dictionary<PlayerAttackAnimArchetype, Sprite[][]>
    _attackFramesByArchetype = new();
```

Caminho de pasta derivado sem literal solto:

```csharp
string folder = "PlayerSprites/attack_" + archetype.ToString().ToLowerInvariant() + "/" + DirKeys[i];
```

### 16.3 Event contracts

`PlayerMeleeSwingEvent` — contrato novo:

```csharp
public sealed class PlayerMeleeSwingEvent
{
    public Vector2 Direction { get; }
    public float Duration { get; }
    public PlayerAttackAnimArchetype Archetype { get; }

    public PlayerMeleeSwingEvent(Vector2 direction, float duration,
                                  PlayerAttackAnimArchetype archetype)
    {
        Direction  = direction;
        Duration   = duration;
        Archetype  = archetype;
    }
}
```

Subscribers afetados:

| Arquivo | Ação |
|---|---|
| `PlayerWalkAnimator.cs` | Consumidor — `OnMeleeSwing`: remover check `IsSword`, usar `evt.Archetype`. |
| `PlayerAttackController.Attacks.cs` | Producer — linha ~117: computar arquétipo via mapper. |

Buscar por outros consumidores antes de implementar (Fase 0):

```powershell
Select-String -Path "Assets\_Game\Scripts\**\*.cs" -Pattern "PlayerMeleeSwingEvent" -Recurse
```

### 16.4 Save contracts

N/A — esta spec não toca nenhum DTO de save, seção de save ou `SaveMigrationService`.

### 16.5 UI contracts

N/A — sem mudanças de HUD, canvas ou cenas.

---

## 17. Sistemas afetados

- Sistema de animação do player (`PlayerWalkAnimator`)
- Sistema de publicação de eventos de combate (`PlayerAttackController.Attacks.cs`)
- Contrato de evento de melee (`PlayerMeleeSwingEvent`)
- Pipeline de carregamento de sprites de ataque do player (Resources)
- EditMode test suite (novo arquivo de testes)

---

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/Events/PlayerAttackAnimArchetype.cs   ← criar
Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs       ← alterar
Assets/_Game/Scripts/Combat/WeaponAttackArchetypeMapper.cs      ← criar
Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs   ← alterar
Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs               ← alterar
Assets/_Game/Tests/EditMode/Combat/WeaponAttackArchetypeMapperTests.cs ← criar
Assets/_Game/Resources/PlayerSprites/attack*/                   ← renomear pasta (via Editor API)
docs/validation/fable_84_execution_report.md                    ← criar
```

---

## 19. Arquivos proibidos

```text
Assets/**/*.unity          ← proibido (não há mudança de cena)
Assets/**/*.prefab         ← proibido
Assets/**/*.asset          ← proibido
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs ← proibido (WeaponType não muda)
Assets/_Game/Scripts/Core/Events/PlayerBowShootEvent.cs ← proibido (arco não muda)
docs_old/**
docs/archive/**
Packages/**
ProjectSettings/**
```

---

## 20. Estratégia de implementação

### Fase 0 — Auditoria (obrigatória antes de qualquer edição)

- Buscar TODOS os consumidores de `PlayerMeleeSwingEvent` e referências a `IsSword`:

```powershell
Select-String -Path "Assets\_Game\Scripts\**\*.cs" -Pattern "PlayerMeleeSwingEvent|IsSword" -Recurse
```

- Confirmar existência de `Resources/PlayerSprites/attack/` e ausência de `attack_sword/`.
- Confirmar ausência de `PlayerAttackAnimArchetype` no repo.
- Se forem encontrados consumidores adicionais de `IsSword`, listá-los e atualizá-los no mesmo PR.

### Fase 1 — Enum no Core

- Criar `PlayerAttackAnimArchetype.cs` em `Core/Events/`.
- Sem dependências de Combat.

### Fase 2 — Atualizar contrato do evento

- Editar `PlayerMeleeSwingEvent.cs`: remover `IsSword`, adicionar `Archetype`.

### Fase 3 — Mapper em Combat

- Criar `WeaponAttackArchetypeMapper.cs` com o mapa completo de `WeaponType` → `PlayerAttackAnimArchetype`.
- Incluir fallback com `Debug.LogWarning` para valores não mapeados.

### Fase 4 — Atualizar publisher

- Editar `PlayerAttackController.Attacks.cs` linha ~117: computar arquétipo, publicar evento atualizado.

### Fase 5 — Atualizar animador

- Editar `PlayerWalkAnimator.cs`:
  - `LoadAllFrames`: para cada arquétipo do enum, carregar as 8 direções de `attack_{archetype}/{dir}/`.
  - `OnMeleeSwing`: remover `if (!evt.IsSword) return;`, despachar `BeginOneShot` com frames do arquétipo + fallback Sword.
  - Remover campo `_attackFrames` (array fixo de espada) — substituir pelo dict/array indexado.

### Fase 6 — Renomear pasta de sprites

- Renomear `Resources/PlayerSprites/attack/` → `Resources/PlayerSprites/attack_sword/` via `AssetDatabase.MoveAsset` (Editor script one-shot OU instrução ao humano para fazer via Unity Project window para preservar GUIDs).
- **IMPORTANTE:** Fazer via Unity Editor (não via File System direto) para que os `.meta` GUIDs sejam preservados.

### Fase 7 — EditMode tests

- Criar `WeaponAttackArchetypeMapperTests.cs` cobrindo todos os valores de `WeaponType`.

### Fase 8 — Build + relatório

- Rodar `dotnet build` em ambos os assemblies.
- Criar `fable_84_execution_report.md`.

---

## 21. Ordem segura de execução

```text
1. Fase 0: auditar consumers de PlayerMeleeSwingEvent + estado de pastas.
2. Fase 1: criar enum PlayerAttackAnimArchetype (sem deps de Combat).
3. Fase 2: atualizar PlayerMeleeSwingEvent (substitui IsSword por Archetype).
4. Fase 3: criar WeaponAttackArchetypeMapper em Combat.
5. Fase 4: atualizar publisher em PlayerAttackController.Attacks.cs.
6. Fase 5: atualizar PlayerWalkAnimator (carregamento + dispatch).
7. Fase 6: renomear pasta attack/ → attack_sword/ (via Unity Editor / AssetDatabase).
8. Fase 7: criar EditMode tests do mapper.
9. Fase 8: dotnet build + relatório.
```

---

## 22. Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: nenhuma spec que toque PlayerWalkAnimator, PlayerMeleeSwingEvent ou Resources/PlayerSprites/
- Must not run with: qualquer spec que altere evento de melee, animador do player ou pasta de sprites de ataque
- Shared files/systems that require lock:
  - `Assets/_Game/Scripts/Core/Events/PlayerMeleeSwingEvent.cs`
  - `Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs`
  - `Assets/_Game/Scripts/Combat/PlayerAttackController.Attacks.cs`
  - `Assets/_Game/Resources/PlayerSprites/attack*/`
- Reason: altera contrato de evento publicado/consumido por dois sistemas; renomear pasta de sprites durante edição paralela causaria conflito de GUIDs.

---

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? MUST BE NO — não se aplica
```

---

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: YES — PlayerMeleeSwingEvent (substitui IsSword bool por Archetype enum)
Requires unsubscribe pattern: NO (já implementado em PlayerWalkAnimator OnDisable)
```

Evento alterado: `PlayerMeleeSwingEvent`

- Campo removido: `bool IsSword`
- Campo adicionado: `PlayerAttackAnimArchetype Archetype`
- Construtor antigo: `(Vector2 direction, float duration, bool isSword)`
- Construtor novo: `(Vector2 direction, float duration, PlayerAttackAnimArchetype archetype)`
- Todos os consumers e producers DEVEM ser atualizados no mesmo PR (Fase 0 mapeia todos).

---

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (sprites são Resources, não assets configurados)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

Cenário de validação em Play Mode (para execução humana no final do lote):

1. Abrir cena com player.
2. Equipar espada → atacar → confirmar animação de espada toca (8 direções).
3. Equipar machado (Axe) → atacar → confirmar fallback Sword toca (sem crash, sem sprite congelado).
4. Equipar cajado (Staff) → atacar → confirmar fallback Sword toca (sem crash).
5. Equipar arco → atirar → confirmar animação de arco intacta.
6. Deslocar em 4 direções principais e confirmar walk/idle inalterados.

---

## 26. Riscos técnicos

| Risco | Mitigação |
|---|---|
| Renomear pasta `attack/` fora do Unity Editor quebra GUIDs dos `.meta` | Fazer via `AssetDatabase.MoveAsset` (script one-shot) OU instruir humano a mover via Unity Project window; jamais via `File.Move` direto no sistema de arquivos. |
| Consumer adicional de `IsSword` não encontrado na Fase 0 → build quebra | Fase 0 obrigatória com busca por `IsSword` antes de qualquer edição de contrato. |
| `PlayerAttackAnimArchetype` enum string usado em path (`ToLowerInvariant`) diverge de nome real de pasta | Fixar mapeamento de pasta com constantes ou método auxiliar para não depender de `ToString()` de enum. Alternativa mais segura: array estático `private static readonly string[] ArchetypeFolders = { "attack_sword", "attack_bow", "attack_heavy", "attack_thrust", "attack_dagger", "attack_cast" }` indexado pelo valor int do enum. |
| Arquétipos sem arte (Heavy/Thrust/Dagger/Cast) carregam array vazio → crash no fallback | Fallback obrigatório: antes de `BeginOneShot`, checar se set está vazio → cair em Sword. Cobrir no EditMode test de integração mínimo do fallback. |
| Dict/array indexado por enum alocado em Awake → allocation desnecessária em Update | Carregar uma vez em `Awake`; `OnMeleeSwing` apenas lê a coleção (sem allocation). |

---

## 27. Rollback

Caso a spec precise ser revertida:

1. Reverter `PlayerMeleeSwingEvent.cs` para `bool IsSword`.
2. Reverter `PlayerAttackController.Attacks.cs` linha ~117 para `weapon.Type == WeaponType.Sword`.
3. Reverter `PlayerWalkAnimator.cs` para o comportamento de `_attackFrames` original.
4. Remover `PlayerAttackAnimArchetype.cs` e `WeaponAttackArchetypeMapper.cs`.
5. Renomear `attack_sword/` de volta para `attack/` via Unity Editor (para preservar GUIDs).
6. Remover `WeaponAttackArchetypeMapperTests.cs`.

Não apaga saves reais do usuário (sem mudança de schema).

---

## 28. Tasks

- [ ] T001 — Fase 0: buscar todos os consumidores de `PlayerMeleeSwingEvent` e referências a `IsSword` no repo; documentar na Fase 0 do relatório.
- [ ] T002 — Fase 0: confirmar existência de `Resources/PlayerSprites/attack/` e ausência de `attack_sword/` e `attack_{heavy/thrust/dagger/cast}/`.
- [ ] T003 — Fase 1: criar `Assets/_Game/Scripts/Core/Events/PlayerAttackAnimArchetype.cs` com enum de 6 valores.
- [ ] T004 — Fase 2: editar `PlayerMeleeSwingEvent.cs` — remover `IsSword`, adicionar `Archetype PlayerAttackAnimArchetype`, atualizar construtor e XML doc.
- [ ] T005 — Fase 3: criar `WeaponAttackArchetypeMapper.cs` em `CindarsHope.Combat` com mapa completo dos 10 valores de `WeaponType`.
- [ ] T006 — Fase 4: editar `PlayerAttackController.Attacks.cs` linha ~117 — usar `WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type)` no construtor do evento.
- [ ] T007 — Fase 5: editar `PlayerWalkAnimator.cs` — substituir `_attackFrames` por coleção indexada por arquétipo; reescrever `LoadAllFrames` e `OnMeleeSwing`; adicionar fallback Sword.
- [ ] T008 — Fase 6: renomear pasta `Resources/PlayerSprites/attack/` → `attack_sword/` via script `AssetDatabase.MoveAsset` (ou instrução ao humano via Unity Editor); confirmar que `attack/` não existe mais e que `attack_sword/` contém os sprites originais.
- [ ] T009 — Fase 7: criar `WeaponAttackArchetypeMapperTests.cs` com testes para todos os 10 valores de `WeaponType`.
- [ ] T010 — Fase 8: rodar `dotnet build Assembly-CSharp.csproj --no-restore` e `dotnet build Assembly-CSharp-Editor.csproj --no-restore`; confirmar exit code 0 em ambos.
- [ ] T011 — Fase 8: criar `docs/validation/fable_84_execution_report.md` com evidências de cada fase.

---

## 29. Validações obrigatórias

```powershell
# Confirmar ausência de IsSword após as edições
Select-String -Path "Assets\_Game\Scripts\**\*.cs" -Pattern "IsSword" -Recurse
# Deve retornar zero resultados.

# Build C# runtime
dotnet build .\Assembly-CSharp.csproj --no-restore

# Build C# editor
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore

# Docs validation
.\tools\docs\validate_docs.ps1
```

Unity compile (se Unity não estiver bloqueado):

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Play Mode: DEFERRED_TO_FINAL_VALIDATION (ver §25 para o cenário documentado).

---

## 30. Testing Quality Gate

```text
Changed deterministic logic: YES (mapeamento WeaponType → arquétipo; carregamento de frames por arquétipo)
Requires EditMode tests: YES (WeaponAttackArchetypeMapper — lógica pura, sem Play Mode)
Requires PlayMode automated or final human scenario: YES (animação visível em Play Mode)
Requires regression test: YES (espada e arco devem continuar funcionando)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Minimum validation evidence for ACCEPTED:
  - dotnet build ambos assemblies exit 0
  - EditMode tests do mapper passando (10 casos)
  - grep por IsSword retorna zero
  - Play Mode humano confirmando: espada anima, armas sem arte fazem fallback sem crash, arco intacto
```

---

## 31. Definition of Done

```text
[ ] Enum PlayerAttackAnimArchetype criado em Core/Events sem deps de Combat.
[ ] PlayerMeleeSwingEvent sem IsSword; campo Archetype presente.
[ ] WeaponAttackArchetypeMapper em Combat cobre todos os 10 WeaponType values.
[ ] PlayerAttackController.Attacks.cs usa mapper ao publicar evento de melee.
[ ] PlayerWalkAnimator despacha por arquétipo; fallback Sword sem crash.
[ ] Pasta attack_sword/ existe; attack/ não existe.
[ ] EditMode tests do mapper: 10 casos, todos passando.
[ ] dotnet build Assembly-CSharp exit 0.
[ ] dotnet build Assembly-CSharp-Editor exit 0.
[ ] grep por IsSword retorna zero resultados.
[ ] fable_84_execution_report.md criado com evidências.
[ ] Nenhum arquivo proibido alterado.
[ ] Spec não promovida sem evidência Play Mode (DEFERRED_TO_FINAL_VALIDATION).
```

---

## 32. Anti-regressão

```text
- Animação de espada DEVE continuar funcionando após a renomeação de attack/ → attack_sword/.
- Animação de arco (PlayerBowShootEvent) DEVE permanecer intacta (não toca nenhum código de bow).
- Walk e idle do player NÃO podem ser afetados.
- Armas sem arte (Heavy/Thrust/Dagger/Cast sem pasta) DEVEM fazer fallback em Sword sem crash e sem log de erro (log de warning é aceitável).
- Nenhum IsSword deve permanecer no codebase após a spec.
- Nenhum literal de path de pasta de ataque deve existir fora da lógica de carregamento do PlayerWalkAnimator (sem magic strings espalhadas).
- PlayerMeleeSwingEvent não pode carregar tipos de Combat (WeaponType, WeaponDataSO).
- Não pode haver FindObjectOfType ou GameObject.Find em código novo.
- WeaponType enum NÃO pode ser alterado (valores pinados são contrato de save/assets).
```

---

## 33. Notas para execução posterior

- A arte dos 4 arquétipos novos (Heavy, Thrust, Dagger, Cast) é uma dependência externa do artista. O pipeline esperado está documentado em `WALK_PIPELINE.md`. A spec não bloqueia a falta de arte — o fallback Sword cobre o gap.
- Convenção de arte para arquétipos novos: 8 direções × 4 frames por arquétipo, nomeados `atk_{dir}_NN.png` (ex.: `atk_right_01.png`), em `Resources/PlayerSprites/attack_{archetype}/{dir}/`. Os frames devem herdar o esqueleto/proporção da animação de espada existente (corpo fundido com arma no mesmo sprite).
- Paper-doll (corpo + item visíveis em camadas separadas) está reservado para personagens futuros e exige pipeline de arte e código completamente distintos — não implementar nesta spec.
- Se forem descobertos consumers adicionais de `PlayerMeleeSwingEvent.IsSword` na Fase 0 (ex.: sistemas de feedback, HUD, audio bridge), atualizá-los no mesmo PR antes de remover o campo.
- A renomeação de pasta via Unity Editor é preferível ao script `AssetDatabase.MoveAsset` por ser menos propensa a deixar GUIDs órfãos. Se o executor optar pelo script, deve confirmar via `AssetDatabase.GUIDFromAssetPath` que os GUIDs foram preservados.
