# Execution Report — fable_87 (Sistema de Recolor por Shader do Player)

> **Spec:** `.specs/a_implementar/fable/fable_87_spec_player_recolor_palette_system.md`
> **Data:** 2026-06-29
> **Status:** BUILD_VALIDATED (C#) — shader compile + wiring na cena + Play Mode DEFERRED ao Unity

---

## Fases concluídas (verificadas pelo orquestrador)

### Shader
- `Assets/_Game/Art/Shaders/PlayerRecolor.shader` criado — base sprite transparente (premultiplied), 4 slots de região (Src/Dst/Tol/Ena), recolor por proximidade à cor-fonte **preservando a luminância** (sombreado intacto). Slot off / pixel fora de região passa adiante.
- ShaderLab **não compila no `dotnet build`** (só no Unity) → compile do shader marcado DEFERRED.

### Código (C#) — build verificado
- `Assets/_Game/Scripts/Player/PlayerColorSetSO.cs` — ScriptableObject: 4 cores-alvo (cabelo/camisa/calça/metal) + 4 flags. Só tipos simples.
- `Assets/_Game/Scripts/Player/PlayerRecolorController.cs` — aplica via `MaterialPropertyBlock` (NÃO instancia material → preserva batching), sem trabalho em Update. Cores-FONTE canônicas como `static readonly` em um único lugar: cabelo `#994d12`, camisa `#e9bb6d`, calça `#643a11`, metal `#7a736b`. Parte determinística extraída em `BuildSlots(set)` (testável sem GPU).
- Includes adicionados ao `Assembly-CSharp.csproj`.

```text
dotnet build Assembly-CSharp.csproj --no-restore → EXIT 0 (0 erros; 1 warning preexistente alheio)
```

### EditMode test
- `Assets/_Game/Tests/EditMode/Player/PlayerColorSetMappingTests.cs` — 5 casos cobrindo `BuildSlots` (null = tudo off; ordem cabelo/camisa/calça/metal; mapeamento de cor-alvo/flag por slot).
- O csproj de testes só é gerado com o Unity aberto → o teste **roda no Unity** (Test Runner EditMode). Compile/execução DEFERRED.

---

## DEFERRED ao Unity (wiring + validação visual)

```text
Status: PENDENTE (próximo passo, via comando canônico — sem o humano tocar ferramenta)
1. Material asset usando o shader CindarsHope/PlayerRecolor.
2. CreatePlayer (3 scene creators) atribui esse material ao SpriteRenderer do player + AddComponent<PlayerRecolorController> + ColorSet default.
3. ColorSet(s) de exemplo (.asset) para validar (ex.: cabelo loiro/preto, calça azul).
4. Unity: shader compila; EditMode test roda; Play Mode confirma recolor com sombreado intacto e regiões com flag off preservadas.
```

---

## Resultado

```text
Shader: ESCRITO (compile DEFERRED ao Unity)
C# (SO + controller): BUILD PASS (exit 0)
EditMode test: ESCRITO (execução DEFERRED ao Unity)
Wiring na cena (material + controller no player + ColorSet): PRÓXIMO PASSO (editor, via comando)
Play Mode: DEFERRED_TO_FINAL_VALIDATION
Status: BUILD_VALIDATED
```
