# SPEC — Sistema de Recolor por Shader (Palette Swap) do Player

> **Spec ID:** `fable_87_spec_player_recolor_palette_system`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Animação & Combat Feel
> **Priority:** P1 (infraestrutura de alavancagem — habilita cores/tinta sem arte nova)
> **Type:** Runtime (shader + runtime controller) + Data (color sets)
> **Domain:** Player / Rendering
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não toquem `PlayerWalkAnimator`, o `SpriteRenderer` do player ou o material do player
> **Must not run with:** specs que troquem o material/shader do player ou reescrevam `PlayerWalkAnimator`
> **Repo lock scope:** `Assets/_Game/Art/Shaders/PlayerRecolor.shader`, `Assets/_Game/Scripts/Player/PlayerRecolorController.cs`, `Assets/_Game/Scripts/Player/PlayerColorSetSO.cs`
> **Depends on:**
> - Player com `SpriteRenderer` único (PlayerWalkAnimator.cs:59 — já existe).
> **Blocks:**
> - Moldes de armadura (skins) e variações de NPC/inimigo reusarão este mesmo shader/controller.
> **Scope:** Criar um sistema de recolor por **shader keyed em cor-fonte** (sem máscara por frame) que remapeia regiões do sprite do player (cabelo, camisa/creme, macacão, metal) para cores-alvo escolhidas em um `PlayerColorSetSO`, preservando o sombreado (luminância), aplicado via `MaterialPropertyBlock` no `SpriteRenderer` do player — dando infinitas variações de cor (cabelo, tinta de roupa) com **zero arte nova**.
> **Out of scope:** Paper-doll/rig esquelético; moldes de armadura (arte de skin nova); recolor de NPCs/inimigos (mesmo sistema, spec futura); máscaras por-frame; mudança de save; mudança de animação.

---

## 5. Contexto

### Por que esta spec existe

A pipeline de arte do player é frame a frame (custo alto por movimento). Cor é o multiplicador mais barato de eliminar: em vez de gerar arte por cor de cabelo / cor de roupa / tinta de armadura, recolore-se **em runtime por shader**, reutilizando a MESMA arte. Isso remove um eixo inteiro da explosão combinatória (`arquétipos × direções × frames × moldes × COR`) — e é puramente código/dados, dirigível sem ferramenta de arte.

A discussão de arquitetura desta wave definiu três eixos ortogonais de variação do player: **arquétipo** (animação), **molde** (silhueta/skin) e **cor** (palette swap). Esta spec entrega o eixo **cor**.

### Abordagem escolhida (e por quê)

Os sprites do fazendeiro são gerados por IA com **sombreado suave** (muitos tons por região). Duas opções de recolor:

1. **Máscara por região (por frame):** robusta, mas exige um asset de máscara por sprite (multiplica assets) e um gerador.
2. **Recolor por cor-fonte no shader (sem máscara):** o shader classifica cada pixel pela proximidade a uma **cor-fonte de referência** da região e remapeia para a cor-alvo, preservando a luminância. **Zero máscara, funciona em todos os frames automaticamente.**

Esta spec adota a **opção 2** (sem máscara) por ser mais simples, sem assets extras e cobrir todos os frames de uma vez. Risco conhecido: regiões de cor parecida (cabelo laranja vs macacão marrom) podem ter leve sangramento — mitigado por tolerância apertada em RGB e gate de luminância; refinável por região.

### Cores-fonte canônicas (medidas no walk do player)

| Região | Cor-fonte (referência) |
|---|---|
| Cabelo | `#994d12` (153, 78, 18) |
| Camisa/creme (pele clara) | `#e9bb6d` (233, 188, 109) |
| Macacão (calça marrom) | `#643a11` (101, 58, 17) |
| Metal (fivelas/acessório) | `#7a736b` (122, 116, 108) |

### Estado atual do repo

- Player: `SpriteRenderer` único em `PlayerWalkAnimator.cs:59`, `GetComponent` em Awake (linha 86). Usa material default de sprite.
- Não existe shader de recolor, `PlayerColorSetSO` nem `PlayerRecolorController`.
- Sistema de palette/tint: NÃO existe — esta é a primeira implementação (não duplicar nada).

---

## 6. Problema

Sem recolor por shader, cada variação de cor (cabelo do player, tinta de roupa/armadura, variantes de NPC) exigiria **arte nova** — multiplicando o já alto custo frame a frame. O eixo "cor" fica preso à geração de sprites, insustentável quando somado a arquétipos × direções × frames × moldes.

---

## 7. Objetivo

Ao final desta spec, o `SpriteRenderer` do player usa um shader de recolor que, dado um `PlayerColorSetSO` (cores-alvo por região: cabelo, camisa, calça, metal), remapeia as regiões correspondentes preservando o sombreado, aplicado uma vez via `MaterialPropertyBlock` (sem alocação por frame, sem quebrar batching), produzindo variações de cor sem arte nova e sem alterar animação, save ou cena.

---

## 8. Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs   (SpriteRenderer único; ponto de aplicação do MPB)
.claude/rules/unity-architecture.md
.claude/rules/csharp-style.md   (sem alocação em hot path)
.claude/skills/ui-projection-pattern/SKILL.md   (ScriptableObject + dados; referência de estilo)
.claude/rules/testing-quality-gate.md
```

---

## 9. User stories / engineering stories

- Como jogador (futuro), quero escolher a cor do cabelo do personagem sem que isso exija arte nova.
- Como sistema de equipamento (futuro), quero tingir a roupa/armadura aplicando uma cor-alvo na região de pano.
- Como `PlayerRecolorController`, quero aplicar as cores-alvo uma única vez via `MaterialPropertyBlock` (sem custo por frame, sem instanciar material → preserva batching).
- Como shader, quero remapear cada região por proximidade à cor-fonte preservando a luminância do pixel (sombreado intacto).
- Como mantenedor, quero um `PlayerColorSetSO` de dados para definir paletas sem tocar código.

---

## 10. Escopo

Inclui:

- **Shader** `Assets/_Game/Art/Shaders/PlayerRecolor.shader` (base sprite, transparente), com **4 slots de região**, cada um: `_SrcN` (cor-fonte), `_TolN` (tolerância), `_DstN` (cor-alvo), `_EnaN` (0/1). Para cada pixel: se a distância de cor à `_SrcN` < `_TolN`, substitui por `_DstN` escalado pela razão de luminância (pixel/fonte), preservando sombra; senão passa adiante. Slots desabilitados (`_EnaN=0`) não alteram o pixel.
- **ScriptableObject** `PlayerColorSetSO` (namespace `CindarsHope.Player`): campos `HairColor`, `ShirtColor`, `PantsColor`, `MetalColor` (Color) + flags `RecolorHair/Shirt/Pants/Metal` (bool). Cores-fonte canônicas ficam como `const`/`static readonly` no controller (não no SO).
- **MonoBehaviour** `PlayerRecolorController` (namespace `CindarsHope.Player`), no mesmo GameObject do `SpriteRenderer`: serialized `PlayerColorSetSO _colorSet`; em `Awake`/`OnEnable` e em `Apply()` constrói um `MaterialPropertyBlock` com as cores-fonte (consts) + alvos do set + flags e chama `_spriteRenderer.SetPropertyBlock(mpb)`. Material default-recolor referenciado/serializado. Sem trabalho em Update.
- Cores-fonte canônicas como consts: Hair `#994d12`, Shirt `#e9bb6d`, Pants `#643a11`, Metal `#7a736b`.
- Um `PlayerColorSetSO` de exemplo (ex.: cabelo loiro/preto, calça azul) em `Assets/_Game/Data/Player/` para validar visualmente.
- EditMode test do mapeamento de `PlayerColorSetSO` → propriedades (parte determinística que não precisa de GPU).
- Execution report.

---

## 11. Fora de escopo

Não inclui: rig esquelético; moldes de armadura (skins de corpo novas); recolor de NPCs/inimigos; máscaras por frame; UI de seleção de cor; persistência da cor escolhida em save; mudança de animação/arquétipo; balance.

---

## 12. Regras de não duplicação

- Não criar segundo material/shader de sprite paralelo ao default fora deste sistema.
- Não instanciar material por player (quebra batching) — usar `MaterialPropertyBlock` sobre um material compartilhado.
- Cores-fonte canônicas vivem em UM lugar (consts no controller) — não espalhar magic colors.
- Não tocar `PlayerWalkAnimator` além de, se necessário, garantir o material no `SpriteRenderer` (preferir setar o material no prefab/scene via wiring, não em código de animação).

---

## 13. Critérios de aceite

### 13.1 Shader
- `PlayerRecolor.shader` existe, base sprite transparente, 4 slots de região (Src/Tol/Dst/Ena).
- Preserva luminância (pixel recolorido = Dst × (lum_pixel / lum_src), clamp), sombreado intacto.
- Slot desabilitado não altera o pixel; pixel fora de toda região passa adiante.

### 13.2 Data + controller
- `PlayerColorSetSO` com 4 cores-alvo + 4 flags.
- `PlayerRecolorController` aplica via `MaterialPropertyBlock` (não instancia material), sem trabalho em Update.
- Cores-fonte como consts (`#994d12`, `#e9bb6d`, `#643a11`, `#7a736b`).

### 13.3 Build
- `dotnet build Assembly-CSharp.csproj --no-restore` exit 0.
- (Shader compila no Unity — ver §riscos; não há compile de ShaderLab no dotnet build.)

### 13.4 Visual (Play Mode — final)
- Com um `PlayerColorSetSO` de cabelo diferente aplicado, o cabelo do player muda de cor mantendo o sombreado, e camisa/calça/metal intactos (se as flags delas estiverem off).
- Tingir a calça (PantsColor + flag) muda só a calça.

---

## 14. Arquitetura alvo

```text
Assets/_Game/Art/Shaders/
  PlayerRecolor.shader            ← shader de recolor por cor-fonte (novo)

Assets/_Game/Scripts/Player/
  PlayerColorSetSO.cs             ← ScriptableObject (cores-alvo + flags) (novo)
  PlayerRecolorController.cs      ← aplica MPB no SpriteRenderer (novo)

Assets/_Game/Data/Player/
  ColorSet_Default.asset          ← paleta canônica (opcional, identidade)
  ColorSet_Example_Blonde.asset   ← exemplo para validar visualmente

Assets/_Game/Tests/EditMode/Player/
  PlayerColorSetMappingTests.cs   ← teste do mapeamento set->props (novo)

docs/validation/
  fable_87_execution_report.md
```

---

## 15. Contratos

### 15.1 Data contracts
`PlayerColorSetSO`: `Color HairColor, ShirtColor, PantsColor, MetalColor` + `bool RecolorHair, RecolorShirt, RecolorPants, RecolorMetal`. Apenas tipos simples (Color/bool) — sem refs de cena.

### 15.2 Runtime contracts
`PlayerRecolorController.Apply(PlayerColorSetSO set)` → constrói MPB com consts de fonte + alvos/flags do set e aplica no `SpriteRenderer`. Idempotente; chamável quando a cor mudar.

### 15.3 Save / Event / UI contracts
N/A nesta spec (a cor escolhida não é persistida nem exposta em UI aqui — specs futuras).

---

## 16. Arquivos permitidos
```text
Assets/_Game/Art/Shaders/PlayerRecolor.shader            ← criar
Assets/_Game/Scripts/Player/PlayerColorSetSO.cs          ← criar
Assets/_Game/Scripts/Player/PlayerRecolorController.cs   ← criar
Assets/_Game/Data/Player/ColorSet_*.asset                ← criar (via editor; .asset de dados explícito autorizado)
Assets/_Game/Tests/EditMode/Player/PlayerColorSetMappingTests.cs  ← criar
docs/validation/fable_87_execution_report.md             ← criar
```

## 17. Arquivos proibidos
```text
Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs   ← evitar (não reescrever animação; material via wiring de cena/prefab)
Assets/**/*.unity                                    ← proibido sem autorização
Assets/**/*.prefab                                   ← proibido sem autorização
docs_old/**, docs/archive/**, Packages/**, ProjectSettings/**
```

---

## 18. Estratégia de implementação
```text
Fase 0 — Confirmar SpriteRenderer único do player; confirmar ausência de shader/SO/controller de recolor.
Fase 1 — Shader PlayerRecolor (4 slots, luminance-preserving).
Fase 2 — PlayerColorSetSO (dados) + consts de cor-fonte.
Fase 3 — PlayerRecolorController (MPB; sem Update).
Fase 4 — ColorSet de exemplo (editor) para validação visual.
Fase 5 — EditMode test do mapeamento set->propriedades.
Fase 6 — dotnet build (C#) exit 0; shader compile + visual = DEFERRED ao Unity. Report.
```

---

## 19. Impacto save/eventos/UI
```text
Save schema: NO | Save section: NO | Migration: NO | Persiste Unity ref: NO
Eventos: NO | UI: NO | Cenas: NO | Prefabs: idealmente NO (material via wiring; se precisar, autorizar)
Requires Play Mode final validation: YES | Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

---

## 20. Riscos técnicos
| Risco | Mitigação |
|---|---|
| ShaderLab não compila no `dotnet build` (só no Unity) | Verificar C# por build; shader compile + visual marcados DEFERRED ao Unity, validados em Play Mode final. |
| Sangramento entre regiões de cor parecida (cabelo vs macacão) | Tolerância apertada em RGB + gate de luminância; flags por região permitem recolorir só o que se quer; refinável por slot. |
| Instanciar material quebraria batching | Usar `MaterialPropertyBlock` sobre material compartilhado — nunca `renderer.material`. |
| Custo por frame | Aplicar MPB só na mudança de cor (Awake/Apply), nunca em Update. |
| Sprite default sem suporte a props custom | Material do player deve usar o shader PlayerRecolor (wiring de cena/prefab ou material asset). |

---

## 21. Rollback
```text
Remover shader, PlayerColorSetSO, PlayerRecolorController, ColorSets e teste; voltar o SpriteRenderer ao material default. Sem mudança de save.
```

---

## 22. Tasks
- [ ] T001 — Fase 0: confirmar SpriteRenderer único + ausência do sistema.
- [ ] T002 — Shader PlayerRecolor (4 slots, luminance-preserving).
- [ ] T003 — PlayerColorSetSO (4 cores + 4 flags).
- [ ] T004 — PlayerRecolorController (MPB; consts de fonte; sem Update).
- [ ] T005 — ColorSet de exemplo (editor) + material do player com o shader.
- [ ] T006 — EditMode test do mapeamento set->props.
- [ ] T007 — dotnet build C# exit 0; report (shader/visual DEFERRED).

---

## 23. Validações obrigatórias
```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
.\tools\docs\validate_docs.ps1
```
Shader compile + Play Mode: DEFERRED_TO_FINAL_VALIDATION (abrir Unity; aplicar ColorSet de exemplo; conferir cabelo recolorido com sombreado intacto e demais regiões preservadas).

---

## 24. Testing Quality Gate
```text
Changed deterministic logic: YES (mapeamento PlayerColorSetSO -> propriedades de material)
Requires EditMode tests: YES (mapeamento set->props; sem GPU)
Requires PlayMode automated or final human scenario: YES (recolor visível)
Requires regression test: NO
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Minimum validation evidence for ACCEPTED:
  - dotnet build C# exit 0
  - EditMode test do mapeamento passando
  - Play Mode humano: ColorSet muda cabelo (e/ou calça) preservando sombreado; regiões com flag off intactas
Status máximo sem Play Mode/shader compile: BUILD_VALIDATED.
```

---

## 25. Anti-regressão
```text
- Não instanciar material por player (preservar batching) — só MaterialPropertyBlock.
- Sem trabalho de recolor em Update (aplicar só na mudança).
- Não alterar animação/save/cena.
- Cores-fonte em um único lugar (consts) — sem magic colors espalhadas.
- Slots desabilitados e pixels fora de região não podem ser alterados pelo shader.
```

---

## 26. Notas para execução posterior
- Mesmo shader/controller servirá para **moldes de armadura** (skins) e **NPCs/inimigos** — basta novos `PlayerColorSetSO`/cores-fonte por personagem.
- Persistir a cor escolhida em save e expor seleção em UI são specs futuras (fora daqui).
- Se o sangramento entre cabelo e macacão incomodar, evoluir o slot afetado para usar uma **máscara opcional** por região (upgrade aditivo, sem quebrar o caminho sem-máscara).
