# Cindar's Hope — Direção de Arte & Descrições Visuais

> **Local:** `docs/design/art/`
> **Função:** lar único das **descrições visuais** (texto) usadas para produzir sprites e arte do jogo — personagens/NPCs, fazenda, cidade, props e monstros. Toda spec que for gerar sprite, prefab visual, atlas, animação, VFX ou prompt de arte deve ler os documentos desta pasta.

## Documentos

| Documento | Cobre |
|---|---|
| [ART_DIRECTION_ILLUSTRATOR_GUIDE.md](./ART_DIRECTION_ILLUSTRATOR_GUIDE.md) | Guia completo para o ilustrador: referências visuais, escala/unidade, **PLAYER + todos os NPCs nomeados** (28) + arquétipos de raça + pets, **fazenda** (terreno, crops, árvores, animais, construções, props), **cidade** (construções, ruas) e a seção de **monstros/inimigos**. |
| [CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md](./CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md) | Direção visual canônica por **família/criatura da caverna** para sprites: silhueta, cor, tamanho, comportamento visual, bioma, variantes, animações mínimas e telegraphs. Inclui a expansão fable_80 (seção 10b). |

## Regra importante — onde vive a arte de fato

Esta pasta guarda **descrições/direção** (Markdown) e pode hospedar **concept/reference art**. Os **sprites finais (PNG) e assets importados NÃO ficam aqui** — eles vivem em `Assets/` para o Unity importar (texturas, atlas, AnimatorControllers, prefabs). O fluxo é: descrição aqui → produção do sprite → import em `Assets/_Game/...`.

## Cobertura (status)

- **NPCs nomeados:** 28/28 descritos (inclui Ancião Velorin e os 4 novos: Sael, Mella, Hess, Tibbet).
- **Monstros do bestiário:** 60/60 com descrição visual (os 14 da expansão fable_80 entraram na seção 10b do doc de caverna).
- **Pendência conhecida:** os docs descrevem ~30+ criaturas e ~12 bosses que ainda **não têm `bestiary_*.asset`** — decidir se viram criaturas reais ou se as descrições são podadas antes de gerar sprites em massa.
