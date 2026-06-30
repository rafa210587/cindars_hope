# SPEC — Integração da Arte do Arquétipo Heavy (Machado de Dois Gumes) no Player

> **Spec ID:** `fable_85_spec_player_heavy_archetype_art_integration`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Animação & Combat Feel
> **Priority:** P1
> **Type:** Integration (asset wiring)
> **Domain:** Player / Combat
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs que não toquem `Resources/PlayerSprites/`, `PlayerWalkAnimator` ou o catálogo de armas (Axe/Hammer)
> **Must not run with:** `fable_84` (enquanto não concluída), qualquer spec que altere `PlayerWalkAnimator`, o loader de frames de ataque ou a pasta `Resources/PlayerSprites/attack*`
> **Repo lock scope:** `Assets/_Game/Resources/PlayerSprites/attack_heavy/**`
> **Depends on:**
> - `fable_84_spec_player_attack_anim_archetype` — estabelece o enum `PlayerAttackAnimArchetype`, o `WeaponAttackArchetypeMapper` (Axe→Heavy, Hammer→Heavy), o despacho por arquétipo no `PlayerWalkAnimator` e a convenção de pasta `Resources/PlayerSprites/attack_{archetype}/{dir}/`. **Esta spec não anima nada sem o código da fable_84 já em vigor.**
> **Blocks:**
> - Specs futuras de integração de arte dos outros arquétipos (`thrust`, `dagger`, `cast`) — todas seguem exatamente este mesmo contrato de diretório, naming e validação.
> **Scope:** Materializar a arte já produzida do arquétipo Heavy (8 direções × 4 frames, fazendeiro empunhando um machado de dois gumes de duas mãos) em `Resources/PlayerSprites/attack_heavy/{dir}/`, com naming canônico `atk_{dir}_NN.png` e import correto (128 PPU, Point, BottomCenter), para que armas mapeadas em Heavy (Axe, Hammer) animem o golpe de machado em jogo no lugar do fallback Sword.
> **Out of scope:** Qualquer código (é dependência da fable_84); arte dos arquétipos `thrust`/`dagger`/`cast`; moldes de armadura (skins de corpo); sistema de cor/tinta (palette swap); paper-doll; qualquer mudança de save, cena, prefab, evento ou balance.

---

## 5. Contexto

### Por que esta spec existe

A `fable_84` transforma a animação de ataque do player num despacho por **arquétipo** (`Sword`, `Bow`, `Heavy`, `Thrust`, `Dagger`, `Cast`), com a regra de carregar frames de `Resources/PlayerSprites/attack_{archetype}/{dir}/` e cair em **fallback Sword** quando o arquétipo não tem arte. A `fable_84` deixa explícito que a arte dos arquétipos novos é **dependência externa**.

Esta spec **fecha essa dependência para o arquétipo Heavy**: a arte do golpe de machado já foi produzida e validada (ver §Procedência da arte) e precisa ser colocada na estrutura de pastas que o loader da `fable_84` espera, com o naming e o import corretos, para que o golpe de machado realmente apareça em jogo.

### Procedência da arte (já concluída, fora do escopo desta spec gerar)

A arte do arquétipo Heavy foi gerada pelo pipeline híbrido documentado em `tools/aseprite/WALK_PIPELINE.md` (master sheet gerado no ChatGPT + pós-produção local: `rembg` → `slice_sheet.py` → ordenação/flip → export), com os seguintes invariantes de qualidade aplicados e validados visualmente direção a direção:

- **Personagem ancorado:** mesmo fazendeiro jovem (sem barba, cabelo castanho-avermelhado, camisa creme, macacão marrom com suspensórios) das sheets de espada/walk. As vistas de frente foram ancoradas anexando a referência frontal do fazendeiro ao projeto do ChatGPT para evitar drift de identidade (o modelo tende a derivar para um "guerreiro barbudo" no front view sem âncora de imagem).
- **Arma travada:** um **machado de guerra de dois gumes** (cabeça de aço simétrica com duas lâminas em meia-lua, cabo de madeira longo), mantido idêntico nos 4 frames de cada direção.
- **Tamanho de corpo consistente** entre os 4 frames (sem encolher o personagem para caber o machado erguido).
- **Golpe legível:** o frame de impacto crava o machado para baixo, segurado com as duas mãos (não largado no chão).
- **Direções:** geradas `right`, `up`, `down`, `downright`, `upright`; espelhadas `left`, `downleft`, `upleft`.

A arte materializada vive em `art/animations/Fazendeiro/attack_heavy_{dir}/` (uma pasta por direção, frames `atk_{dir}_NN.png` + o `_source.png` master), no mesmo padrão de organização das sheets de espada do projeto.

### Estado atual do repo

- `art/animations/Fazendeiro/attack_heavy_{dir}/` — **EXISTE** para as 8 direções (`right, up, down, downright, upright, left, downleft, upleft`), cada uma com `atk_{dir}_01.png … atk_{dir}_04.png` e `_source.png`. É a fonte de verdade da arte (cópia de trabalho/arquivo).
- `Resources/PlayerSprites/attack_heavy/{dir}/` — **NÃO EXISTE** ainda. É o destino runtime que o loader da `fable_84` lê.
- `Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs` — **EXISTE**: aplica automaticamente import settings (128 PPU, FilterMode Point, SpriteMode Single, Pivot BottomCenter, sem compressão, sem mipmap) a qualquer asset sob `Resources/PlayerSprites/`.
- Loader de frames de ataque por arquétipo (`PlayerWalkAnimator`) — **depende da fable_84** estar concluída; sem ela, nem o caminho `attack_heavy/` é lido.

### O que destrava

Com a arte no lugar, **Axe e Hammer** (ambos mapeados para `Heavy` no `WeaponAttackArchetypeMapper`) param de usar o fallback de espada e passam a tocar o golpe de machado dedicado nas 8 direções. Fecha o game feel de "atacar com machado" e prova o pipeline arte→jogo de ponta a ponta antes de investir nos outros 3 arquétipos.

### O que fica para specs futuras

`thrust` (Spear), `dagger` (Dagger) e `cast` (Staff/Wand) — cada um repetindo este mesmo contrato: gerar arte com o pipeline, materializar em `Resources/PlayerSprites/attack_{archetype}/{dir}/`, validar em Play Mode. Esta spec serve de **template de integração** para essas.

---

## 6. Problema

Sem materializar a arte do Heavy em `Resources/PlayerSprites/attack_heavy/`, qualquer arma mapeada em Heavy (Axe, Hammer) continua caindo no **fallback Sword**: o player empunha um machado mas executa a animação de golpe de espada. O resultado é uma incoerência visual evidente (arma e animação não combinam) num dos arquétipos centrais de combate, e o trabalho de arte já entregue fica órfão (existe no repo de arte mas nunca chega ao jogo).

---

## 7. Objetivo

Ao final desta spec, equipar uma arma do arquétipo Heavy (Axe ou Hammer) e atacar deve reproduzir a animação dedicada do golpe de machado de dois gumes nas 8 direções, com os frames carregados de `Resources/PlayerSprites/attack_heavy/{dir}/atk_{dir}_NN.png`, importados a 128 PPU / Point / BottomCenter pelo `GeneratedSpriteImporter`, sem alterar nenhum código, save, cena, prefab ou outro arquétipo, e sem regredir as animações de espada e arco.

---

## 8. Fontes obrigatórias lidas

```text
.specs/a_implementar/fable/fable_84_spec_player_attack_anim_archetype.md
tools/aseprite/WALK_PIPELINE.md
Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs
Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs   (apenas o loader de frames por arquétipo, conforme fable_84)
.claude/rules/unity-assets.md
.claude/rules/testing-quality-gate.md
.claude/skills/unity-asset-generation/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md   (referência de naming/import de sprites do player)
```

---

## 9. Estado atual do repo

| Artefato | Estado |
|---|---|
| `art/animations/Fazendeiro/attack_heavy_{dir}/` (8 dirs) | EXISTE. `atk_{dir}_01..04.png` + `_source.png`. Fonte de verdade da arte. |
| `Resources/PlayerSprites/attack_heavy/{dir}/` | NÃO EXISTE. A ser criada (nested, uma subpasta por direção). |
| `Resources/PlayerSprites/attack_sword/` | Renomeada pela `fable_84` (a partir de `attack/`). Não muda aqui. |
| `Resources/PlayerSprites/bow/` | EXISTE. Não muda aqui. |
| `GeneratedSpriteImporter.cs` | EXISTE. Aplica import a `Resources/PlayerSprites/**`. Não muda aqui. |
| Loader por arquétipo no `PlayerWalkAnimator` | Provido pela `fable_84` (dependência). Não muda aqui. |
| `WeaponAttackArchetypeMapper` (Axe→Heavy, Hammer→Heavy) | Provido pela `fable_84` (dependência). Não muda aqui. |

**Auditoria obrigatória na Fase 0:** confirmar que as 8 pastas `art/animations/Fazendeiro/attack_heavy_{dir}/` existem e contêm 4 frames `atk_{dir}_NN.png` cada (32 frames no total), e que `Resources/PlayerSprites/attack_heavy/` ainda não existe. Confirmar também que a `fable_84` já foi implementada (enum + loader + mapper presentes); se não, esta spec fica **BLOCKED_BY_DEPENDENCY** até a fable_84 fechar.

---

## 10. User stories / engineering stories

- Como `PlayerWalkAnimator` (loader da fable_84), quero encontrar frames em `Resources/PlayerSprites/attack_heavy/{dir}/` para tocar o golpe de machado em vez do fallback Sword.
- Como agente executor, quero mapear as 8 pastas flat de arte (`attack_heavy_{dir}/`) para a estrutura nested de runtime (`attack_heavy/{dir}/`) preservando o naming `atk_{dir}_NN.png`.
- Como `GeneratedSpriteImporter`, quero que os novos PNGs entrem sob `Resources/PlayerSprites/` para aplicar 128 PPU / Point / BottomCenter automaticamente.
- Como QA, quero equipar Axe e Hammer e ver o golpe de machado nas 8 direções, com espada e arco intactos.
- Como artista, quero que este contrato de integração sirva de template para `thrust`/`dagger`/`cast`.

---

## 11. Escopo

Inclui:

- Criar a estrutura `Resources/PlayerSprites/attack_heavy/{dir}/` para as 8 direções (`right, upright, up, upleft, left, downleft, down, downright`).
- Copiar os 32 frames de `art/animations/Fazendeiro/attack_heavy_{dir}/atk_{dir}_NN.png` para `Resources/PlayerSprites/attack_heavy/{dir}/atk_{dir}_NN.png` (mapeamento flat→nested, naming preservado).
- Garantir o import correto via `GeneratedSpriteImporter` (reimport no Unity Editor) — 128 PPU, Point, Single, BottomCenter, sem compressão/mipmap — confirmado por inspeção dos `.meta`/import settings.
- Cenário de validação em Play Mode documentado (golpe de machado em 8 direções com Axe e Hammer; espada e arco intactos).
- Execution report em `docs/validation/fable_85_execution_report.md` com a contagem esperada vs. real de frames e evidência de import.

---

## 12. Fora de escopo

Não inclui:

- Qualquer mudança de código (enum, mapper, animator, evento) — pertence à `fable_84`.
- Renomear `attack/` → `attack_sword/` — pertence à `fable_84`.
- Arte/integração dos arquétipos `thrust`, `dagger`, `cast`.
- Moldes de armadura (skins de corpo: sem armadura / armadura / mago).
- Sistema de cor/tinta (palette swap de cabelo, roupa, armadura).
- Paper-doll de equipamentos.
- Qualquer mudança de save, DTO, seção de save, evento, HUD, cena ou prefab.
- Balance de dano/stamina/cooldown de Axe/Hammer.

---

## 13. Regras de não duplicação

- Não recriar o loader de frames nem o despacho por arquétipo — é da `fable_84`; esta spec só entrega os assets que ele consome.
- Não criar um segundo `GeneratedSpriteImporter` nem alterar o existente; reusar o import automático sob `Resources/PlayerSprites/`.
- Não inventar um naming alternativo de frame — manter `atk_{dir}_NN.png` (mesmo padrão da espada).
- Não duplicar a arte em outro caminho de Resources; o único destino runtime é `Resources/PlayerSprites/attack_heavy/{dir}/`.
- A fonte de verdade da arte permanece em `art/animations/Fazendeiro/attack_heavy_{dir}/`; o Resources é cópia derivada.

---

## 14. Critérios de aceite

### 14.1 Estrutura de pastas criada

- `Resources/PlayerSprites/attack_heavy/{dir}/` existe para as 8 direções: `right, upright, up, upleft, left, downleft, down, downright`.
- Evidência: `Get-ChildItem Assets/_Game/Resources/PlayerSprites/attack_heavy -Directory` lista 8 pastas.

### 14.2 Frames copiados com naming canônico

- Cada direção contém exatamente 4 frames: `atk_{dir}_01.png … atk_{dir}_04.png`.
- Total: 32 PNGs sob `attack_heavy/`.
- O conteúdo bate byte-a-byte com a fonte em `art/animations/Fazendeiro/attack_heavy_{dir}/` (cópia fiel, sem reprocessar).
- Evidência: contagem de 32 arquivos + comparação de tamanho/hash com a fonte registrada no report.

### 14.3 Import settings corretos

- Cada PNG novo importa como: PPU **128**, FilterMode **Point**, SpriteMode **Single**, Pivot **BottomCenter**, sem compressão, sem mipmap (aplicado por `GeneratedSpriteImporter`).
- Evidência: inspeção dos `.meta` gerados (ou log do `GeneratedSpriteImporter`) confirmando os valores; registrar no report.

### 14.4 Heavy anima em jogo (Play Mode — final)

- Com a `fable_84` em vigor, equipar **Axe** e atacar → animação de golpe de machado de dois gumes toca, na direção correta (testar as 4 cardinais + ao menos 1 diagonal).
- Equipar **Hammer** e atacar → também toca a animação Heavy (Hammer mapeia para Heavy).
- Nenhum fallback Sword visível para Axe/Hammer.
- Evidência: cenário Play Mode documentado (§25), executado na validação final humana do lote.

### 14.5 Sem regressão

- Espada continua animando (8 direções).
- Arco continua animando.
- Walk/idle inalterados.
- Nenhum código, cena, prefab ou save alterado por esta spec.

---

## 15. Arquitetura alvo

```text
art/animations/Fazendeiro/                      (FONTE — não muda)
  attack_heavy_right/   atk_right_01..04.png + _source.png
  attack_heavy_up/      ...
  attack_heavy_down/    ...
  attack_heavy_downright/, attack_heavy_upright/
  attack_heavy_left/, attack_heavy_downleft/, attack_heavy_upleft/   (espelhos)

            │  cópia flat → nested (esta spec)
            ▼

Assets/_Game/Resources/PlayerSprites/attack_heavy/   (DESTINO RUNTIME — criar)
  right/     atk_right_01..04.png
  upright/   atk_upright_01..04.png
  up/        atk_up_01..04.png
  upleft/    atk_upleft_01..04.png
  left/      atk_left_01..04.png
  downleft/  atk_downleft_01..04.png
  down/      atk_down_01..04.png
  downright/ atk_downright_01..04.png

GeneratedSpriteImporter.cs   → aplica 128 PPU / Point / Single / BottomCenter (existente)
PlayerWalkAnimator (fable_84) → carrega attack_heavy/{dir}/ e despacha por arquétipo (existente, dependência)

docs/validation/
  fable_85_execution_report.md
```

---

## 16. Contratos, dados e eventos

### 16.1 Data contracts

- **Naming de frame:** `atk_{dir}_NN.png`, `NN` zero-padded de dois dígitos (`01`–`04`), ordem = sequência da animação (1 preparação, 2 machado no alto, 3 golpe descendo, 4 recuperação). O `PlayerWalkAnimator` ordena por `string.CompareOrdinal` do nome, então o padding de 2 dígitos é obrigatório.
- **Direções canônicas (8):** `right, upright, up, upleft, left, downleft, down, downright` — exatamente as mesmas chaves usadas pelo walk/idle/attack_sword.
- **Contagem:** 4 frames por direção × 8 direções = 32 sprites.

### 16.2 Runtime contracts

N/A direto — esta spec não adiciona runtime. O contrato runtime que ela satisfaz é o do loader da `fable_84`: caminho `"PlayerSprites/attack_heavy/" + dir`. Esta spec apenas garante que o caminho exista e tenha os frames.

### 16.3 Event contracts

N/A — nenhum evento novo ou alterado.

### 16.4 Save contracts

N/A — nenhum DTO, seção de save ou migration. Sprites são Resources, não persistidos em save.

### 16.5 UI contracts

N/A — sem HUD/canvas/cena.

---

## 17. Sistemas afetados

- Pipeline de carregamento de sprites de ataque do player (Resources) — passa a ter o set `attack_heavy`.
- Animação de ataque do player (consumo via `PlayerWalkAnimator`, sem alteração de código).
- Import pipeline do editor (`GeneratedSpriteImporter`) — processa os 32 PNGs novos.
- Validation reports.

---

## 18. Arquivos permitidos

```text
Assets/_Game/Resources/PlayerSprites/attack_heavy/**          ← criar (32 PNGs + .meta gerados pelo importer)
docs/validation/fable_85_execution_report.md                  ← criar
tools/**  (apenas se for usado um script utilitário de cópia flat→nested; opcional)
```

---

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/**            ← proibido (sem código; é da fable_84)
Assets/**/*.unity                  ← proibido
Assets/**/*.prefab                 ← proibido
Assets/**/*.asset                  ← proibido
Assets/_Game/Resources/PlayerSprites/attack_sword/**  ← proibido (espada não muda aqui)
Assets/_Game/Resources/PlayerSprites/bow/**           ← proibido
art/animations/Fazendeiro/**       ← proibido alterar a fonte (somente ler/copiar dela)
docs_old/**
docs/archive/**
Packages/**
ProjectSettings/**
```

---

## 20. Estratégia de implementação

### Fase 0 — Auditoria (obrigatória antes de qualquer cópia)

- Confirmar que `fable_84` está implementada (enum `PlayerAttackAnimArchetype`, `WeaponAttackArchetypeMapper`, loader por arquétipo no `PlayerWalkAnimator`). Se não estiver, parar com status **BLOCKED_BY_DEPENDENCY**.
- Confirmar 8 pastas `art/animations/Fazendeiro/attack_heavy_{dir}/`, cada uma com `atk_{dir}_01..04.png` (32 frames).
- Confirmar que `Resources/PlayerSprites/attack_heavy/` ainda não existe.

### Fase 1 — Materializar a estrutura nested e copiar os frames

- Para cada direção, criar `Assets/_Game/Resources/PlayerSprites/attack_heavy/{dir}/` e copiar os 4 `atk_{dir}_NN.png` da fonte flat correspondente.
- Cópia fiel (sem reprocessar/rembg/slice de novo — a arte já está finalizada). Mapear flat (`attack_heavy_{dir}/`) → nested (`attack_heavy/{dir}/`).

### Fase 2 — Import no Unity

- Acionar reimport (abrir Unity / `AssetDatabase.Refresh`) para o `GeneratedSpriteImporter` aplicar os import settings aos 32 PNGs.
- Confirmar import (PPU 128, Point, Single, BottomCenter) por inspeção de `.meta`/log.

### Fase 3 — Validação

- Contagem 32/32 e comparação com a fonte.
- Cenário Play Mode documentado (Axe/Hammer animam Heavy; espada/arco intactos) — execução na validação final do lote.

### Fase 4 — Relatório

- `docs/validation/fable_85_execution_report.md` com: contagem esperada vs. real, evidência de import, e o cenário de Play Mode marcado DEFERRED_TO_FINAL_VALIDATION.

---

## 21. Ordem segura de execução

```text
1. Fase 0: confirmar dependência fable_84 + presença das 8 pastas de arte (32 frames) + ausência de Resources/.../attack_heavy.
2. Fase 1: criar attack_heavy/{dir}/ e copiar os 32 frames (flat→nested).
3. Fase 2: reimport no Unity; confirmar import settings.
4. Fase 3: contagem + cenário Play Mode documentado.
5. Fase 4: execution report.
```

---

## 22. Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs que não toquem `Resources/PlayerSprites/`, `PlayerWalkAnimator` ou o catálogo de Axe/Hammer.
- Must not run with: `fable_84` (até concluída); qualquer spec que altere o loader de ataque ou a pasta `attack*` de Resources.
- Shared files/systems that require lock:
  - `Assets/_Game/Resources/PlayerSprites/attack_heavy/**`
- Reason: depende do contrato de diretório e do loader da `fable_84`; concorrência sobre `Resources/PlayerSprites/` causaria conflito de import/GUID.

---

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? MUST BE NO — não se aplica (sprites são Resources, não persistidos)
```

---

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

---

## 25. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (sprites de Resources, não .asset configurados)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

Cenário de validação em Play Mode (execução humana no final do lote, junto com a fable_84):

1. Abrir cena com player (`Inicializar Projeto` se necessário para regenerar wiring).
2. Equipar **Axe** → atacar nas direções `down`, `up`, `right`, `left` e ao menos uma diagonal (`downright`) → confirmar que toca a animação do **golpe de machado de dois gumes** (não a de espada) na direção correta.
3. Equipar **Hammer** → atacar → confirmar que também toca a animação Heavy (mapeia para Heavy).
4. Equipar **Sword** → atacar → confirmar que a animação de espada continua correta (sem regressão).
5. Equipar **Bow** → atirar → confirmar que a animação de arco continua intacta.
6. Mover em 4 direções → confirmar walk/idle inalterados.

---

## 26. Riscos técnicos

| Risco | Mitigação |
|---|---|
| Import settings não aplicados (PNG entra com PPU/filter default) | Confirmar que o caminho é exatamente sob `Resources/PlayerSprites/`; acionar `AssetDatabase.Refresh`/reimport; inspecionar `.meta`. |
| Mapeamento flat→nested errado (frame na direção trocada) | Copiar 1:1 `attack_heavy_{dir}/` → `attack_heavy/{dir}/`, conferindo a direção no nome do arquivo (`atk_{dir}_NN`). Registrar contagem por direção no report. |
| Ordem de frame trocada (golpe antes da preparação) | Naming `01..04` já reflete a sequência aprovada; ordenação por `CompareOrdinal` exige padding de 2 dígitos (já garantido). |
| Escala do machado/personagem destoando do walk em jogo | Risco residual documentado; a frame mais ereta foi normalizada ao gabarito da espada no export. Ajuste fino de escala é decisão de Play Mode (um multiplicador único, se necessário), registrado como follow-up — não bloqueia esta spec. |
| Executar antes da fable_84 | Fase 0 obrigatória: se o loader por arquétipo não existir, status BLOCKED_BY_DEPENDENCY. |
| Reimport não roda em modo headless/CI | Se Unity estiver bloqueado, registrar import como `NOT RUN` com motivo; a cópia dos arquivos não depende do Unity, só o import settings. |

---

## 27. Rollback

```text
1. Apagar a pasta Assets/_Game/Resources/PlayerSprites/attack_heavy/ (e os .meta).
2. Reimport — Axe/Hammer voltam ao fallback Sword (comportamento da fable_84 sem arte).
3. A fonte em art/animations/Fazendeiro/attack_heavy_*/ permanece intacta.
4. Não apaga save real do usuário (sem mudança de schema).
```

---

## 28. Tasks

- [ ] T001 — Fase 0: confirmar fable_84 implementada (enum + mapper + loader por arquétipo). Se ausente, marcar BLOCKED_BY_DEPENDENCY e parar.
- [ ] T002 — Fase 0: confirmar 8 pastas `art/animations/Fazendeiro/attack_heavy_{dir}/` com 4 frames cada (32 total) e ausência de `Resources/PlayerSprites/attack_heavy/`.
- [ ] T003 — Fase 1: criar `Resources/PlayerSprites/attack_heavy/{dir}/` para as 8 direções e copiar os 32 `atk_{dir}_NN.png` (flat→nested, cópia fiel).
- [ ] T004 — Fase 2: acionar reimport no Unity; confirmar import settings (128 PPU, Point, Single, BottomCenter) por `.meta`/log.
- [ ] T005 — Fase 3: validar contagem 32/32 e comparação byte/tamanho com a fonte; montar evidência.
- [ ] T006 — Fase 3: documentar o cenário Play Mode (§25) marcado DEFERRED_TO_FINAL_VALIDATION.
- [ ] T007 — Fase 4: criar `docs/validation/fable_85_execution_report.md` com evidências.

---

## 29. Validações obrigatórias

```powershell
# Contagem de frames no destino (esperado: 32)
Get-ChildItem Assets/_Game/Resources/PlayerSprites/attack_heavy -Recurse -Filter atk_*.png | Measure-Object

# 8 direções presentes
Get-ChildItem Assets/_Game/Resources/PlayerSprites/attack_heavy -Directory | Select-Object Name

# Docs validation
.\tools\docs\validate_docs.ps1
```

Unity import/compile (se Unity não estiver bloqueado):

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Play Mode: DEFERRED_TO_FINAL_VALIDATION (ver §25). Se o import não puder rodar (Unity bloqueado), registrar `NOT RUN` com motivo e risco residual: "frames copiados mas import settings não confirmados até abrir o Unity".

---

## 30. Testing Quality Gate

```text
Changed deterministic logic: NO (integração de asset; nenhuma lógica C# nova)
Requires EditMode tests: NO (sem lógica determinística; cobertura do mapper Axe/Hammer→Heavy é da fable_84)
Requires PlayMode automated or final human scenario: YES (animação visível por arma)
Requires regression test: YES (espada e arco devem continuar; via cenário Play Mode)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Minimum validation evidence for ACCEPTED:
  - 32 frames presentes em Resources/PlayerSprites/attack_heavy/ (8 dir × 4), conferidos contra a fonte
  - import settings confirmados (128 PPU / Point / Single / BottomCenter)
  - Play Mode humano confirmando: Axe e Hammer tocam o golpe de machado nas direções testadas; espada e arco intactos
Status máximo sem Play Mode: BUILD_VALIDATED / UNITY_VALIDATED conforme evidência de import.
```

---

## 31. Definition of Done

```text
[ ] fable_84 confirmada implementada (dependência) — senão BLOCKED_BY_DEPENDENCY.
[ ] Resources/PlayerSprites/attack_heavy/{dir}/ existe para as 8 direções.
[ ] 32 frames atk_{dir}_NN.png presentes, cópia fiel da fonte.
[ ] Import settings confirmados (128 PPU / Point / Single / BottomCenter).
[ ] Cenário Play Mode documentado (Axe/Hammer animam Heavy; espada/arco intactos), DEFERRED_TO_FINAL_VALIDATION.
[ ] Nenhum arquivo proibido alterado (zero mudança de código/cena/prefab/save).
[ ] fable_85_execution_report.md criado com contagem e evidência de import.
[ ] Sem claim de ACCEPTED sem evidência Play Mode.
```

---

## 32. Anti-regressão

```text
- Animação de espada DEVE continuar funcionando (attack_sword intacto).
- Animação de arco DEVE permanecer intacta.
- Walk e idle do player NÃO podem ser afetados.
- Nenhum código alterado por esta spec (é asset-only).
- A fonte de arte em art/animations/Fazendeiro/attack_heavy_*/ NÃO pode ser modificada (somente lida/copiada).
- Naming de frame DEVE ser atk_{dir}_NN.png com padding de 2 dígitos (ordenação por CompareOrdinal).
- O único destino runtime é Resources/PlayerSprites/attack_heavy/{dir}/ — não duplicar a arte em outro caminho.
- Não alterar attack_sword/ nem bow/.
```

---

## 33. Notas para execução posterior

- Esta spec é o **template de integração de arte de arquétipo**. Os arquétipos `thrust` (Spear), `dagger` (Dagger) e `cast` (Staff/Wand) seguirão specs irmãs idênticas em estrutura, trocando apenas o nome do arquétipo e a arte de origem.
- Lições do pipeline a aplicar nos próximos arquétipos (registradas para não repetir o custo): (1) **ancorar a identidade** anexando a referência frontal do fazendeiro ao projeto do ChatGPT, sobretudo para vistas de frente; (2) **travar a arma** com descrição específica e "mesmo objeto idêntico nos 4 frames"; (3) exigir **mesmo tamanho de corpo** entre frames; (4) **validar cada direção na imagem** antes de fatiar; (5) distinguir `right` (semi-frontal, consistente com o walk) de `downright` apenas sutilmente — não forçar perfil estrito, que destoa do walk (decisão registrada nesta wave).
- Eventual ajuste fino de **escala** do player Heavy vs. espada é decisão de Play Mode (um multiplicador único por arquétipo, se necessário), e fica como follow-up — não bloqueia esta spec.
- Os **moldes de armadura** (skins de corpo: sem armadura / armadura / mago) e o **sistema de cor/tinta** (palette swap) são trilhas futuras separadas, com specs próprias, e reusarão estes mesmos arquétipos de animação.
