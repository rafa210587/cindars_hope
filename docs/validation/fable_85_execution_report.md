# Execution Report — fable_85 (Integração da Arte do Arquétipo Heavy)

> **Spec:** `.specs/a_implementar/fable/fable_85_spec_player_heavy_archetype_art_integration.md`
> **Data:** 2026-06-29
> **Status:** ASSET_COPIED — pendente import do Unity + Play Mode final (DEFERRED_TO_FINAL_VALIDATION)
> **Depende de:** `fable_84` (loader por arquétipo) — implementação em paralelo neste mesmo lote.

---

## Fase 0 — Auditoria

- Fonte de arte confirmada: `art/animations/Fazendeiro/attack_heavy_{dir}/` para as 8 direções, 4 frames cada (`atk_{dir}_01..04.png`) — 32 frames. ✅
- `Resources/PlayerSprites/attack_heavy/` não existia antes desta execução. ✅
- Dependência `fable_84` (enum `PlayerAttackAnimArchetype`, `WeaponAttackArchetypeMapper`, loader por arquétipo no `PlayerWalkAnimator`): em implementação no mesmo lote. A animação em jogo só ocorre com a fable_84 em vigor; a cópia de assets (esta fase) é independente e não quebra nada sem ela (sem a fable_84, o caminho `attack_heavy/` simplesmente não é lido).

## Fase 1 — Cópia flat → nested

Copiados 32 frames de `art/animations/Fazendeiro/attack_heavy_{dir}/atk_{dir}_NN.png` para `Assets/_Game/Resources/PlayerSprites/attack_heavy/{dir}/atk_{dir}_NN.png`.

| Direção | Frames |
|---|---|
| right | 4 |
| upright | 4 |
| up | 4 |
| upleft | 4 |
| left | 4 |
| downleft | 4 |
| down | 4 |
| downright | 4 |

- **Total no destino:** 32/32 PNGs ✅
- **8 direções presentes:** right, upright, up, upleft, left, downleft, down, downright ✅
- Cópia fiel (sem reprocessar). Espelhos (left/downleft/upleft) já materializados na fonte pelo export.

Comando de verificação:
```powershell
(Get-ChildItem Assets/_Game/Resources/PlayerSprites/attack_heavy -Recurse -Filter atk_*.png | Measure-Object).Count   # = 32
Get-ChildItem Assets/_Game/Resources/PlayerSprites/attack_heavy -Directory   # = 8 pastas
```

## Fase 2 — Import no Unity

```text
Status: NOT RUN (DEFERRED)
Motivo: Unity não está aberto/headless nesta execução.
Ação ao abrir o Unity: AssetDatabase.Refresh() aciona o GeneratedSpriteImporter,
que aplica 128 PPU / FilterMode Point / SpriteMode Single / Pivot BottomCenter / sem compressão / sem mipmap
a qualquer asset sob Resources/PlayerSprites/. Confirmar nos .meta gerados.
Risco residual: até abrir o Unity, os 32 PNGs estão no disco mas sem .meta/import settings confirmados.
```

## Fase 3 — Play Mode (final)

```text
Status: DEFERRED_TO_FINAL_VALIDATION
Cenário (ver §25 da spec): com a fable_84 em vigor, equipar Axe e Hammer e atacar nas 4 cardinais + 1 diagonal
→ deve tocar o golpe de machado de dois gumes (não o fallback de espada). Espada e arco intactos.
```

---

## Validação de tamanho e cor vs walk (+ ajuste)

Antes do Play Mode, os frames do Heavy foram validados contra o **walk** (referência canônica do player em jogo):

- **Tamanho:** os frames assentados do Heavy medem corpo 66–69px (pés→topo do cabelo) vs walk 67–70px; o slice já normalizou ambos a `char_h=72` (mesma altura de conteúdo, mesmo PPU 128) → **mesma escala em jogo**. Frames de windup (53–57px) são menores por pose (antecipação), não por escala. **Nenhum ajuste de tamanho necessário.**
- **Cor (ajuste aplicado):** medição da cor média do personagem (pixels warm, excluindo o machado cinza) acusou o Heavy mais escuro e alaranjado que o walk: RGB `134,74,19` / lum `85.3` vs walk `134,80,31` / lum `90.6` (azul baixo = cast laranja). Aplicado **histogram matching por canal** nos pixels do personagem (preservando machado cinza, contornos e transparência), casando a faixa tonal com a do walk. **Pós-ajuste:** Heavy `133,80,31` / lum `90.3` ≈ walk `134,80,31` / lum `90.6`. Os 32 frames corrigidos foram redeployados em `Resources/PlayerSprites/attack_heavy/` **e** na fonte `art/animations/Fazendeiro/attack_heavy_*/` (mantidos em sync).
- **Residual conhecido:** o *estilo de shading* do Heavy é levemente mais suave que o cel-shading chapado do walk (inerente à geração por IA; não é diferença de cor/tamanho). Aceitável; polimento futuro opcional.

## Resultado

```text
Size vs walk: MATCH (sem ajuste)
Color vs walk: AJUSTADO (histogram match por canal; char mean e luminância casados com o walk)
Asset copy: PASS (32/32 frames, 8 direções)
Unity import: NOT RUN (deferido — abrir Unity)
Play Mode: DEFERRED_TO_FINAL_VALIDATION
Código alterado: NENHUM (spec asset-only)
Arquivo proibido alterado: NENHUM
Status: ASSET_COPIED — pendente import + Play Mode
```

## Próximos passos (1 sessão de Unity, junto com a fable_84)

1. Concluir/validar o build da `fable_84` (código).
2. Abrir Unity → rodar o MenuItem de rename `attack/`→`attack_sword/` da fable_84 → `AssetDatabase.Refresh` aplica import nos 32 frames do Heavy.
3. `CindarsHope → Inicializar Projeto` (se necessário) → Play → executar o cenário §25.
