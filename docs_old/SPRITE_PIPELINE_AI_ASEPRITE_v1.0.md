# Cindar's Hope — Pipeline de Sprites com IA + Aseprite v1.0

> **Status:** guia operacional para produção de arte  
> **Objetivo:** usar IA para acelerar conceito/rascunho sem perder consistência visual nem controle autoral  
> **Fonte final:** Aseprite + PNG exportado + import correto no Unity

---

## 1. Regra central

```text
IA ajuda a chegar perto. Aseprite transforma em asset de jogo.
```

A imagem gerada por IA não deve entrar direto no jogo sem revisão.

---

## 2. Ferramentas

| Ferramenta | Papel | Quando usar | Observação |
|---|---|---|---|
| ChatGPT/DALL-E | Conceito, ícones simples, variações rápidas | Itens 32x32, mockups de UI, ideias de NPC | Pode gerar antialiasing; precisa retoque |
| PixelLab | Pixel art focado em game asset | Sprites, itens, tilesets, personagens, variações | Bom candidato para produção indie se disponível |
| Scenario | Consistência em lote/modelo próprio | Quando houver art bible e muitos assets | Útil para escala e custom models |
| Aseprite | Acabamento final | Sempre | Paleta, outline, frames, spritesheet, export |
| Pixelorama/LibreSprite | Fallback | Se não usar Aseprite | Deve exportar o mesmo padrão |

---

## 3. Restrições autorais e de identidade

Permitido:

- Usar Stardew Valley/Harvest Moon como referência de gênero, câmera, legibilidade e sensação cozy.
- Pedir “cozy top-down farm pixel art”.
- Usar paleta própria de Cindar's Hope.

Proibido:

- Copiar sprite, personagem, tileset, UI ou paleta proprietária.
- Gerar “igual Stardew Valley”.
- Usar assets comerciais sem licença.
- Misturar estilos incompatíveis sem retoque.

Prompt deve dizer:

```text
inspired by cozy farming RPG readability, original visual identity, not a copy of any existing game
```

---

## 4. Paleta do projeto

### Fazenda

| Uso | Hex |
|---|---|
| Laranja colheita | `#D4832A` |
| Bronze | `#8B6914` |
| Terra | `#6B3A2A` |
| Verde musgo | `#4A6741` |
| Chão claro | `#C8A464` |

### Cidade

| Uso | Hex |
|---|---|
| Azul ardósia | `#4A5E7A` |
| Pedra | `#7A8A9A` |
| Âmbar lanterna | `#D4A850` |

### Caverna

| Uso | Hex |
|---|---|
| Cinza carvão | `#2A2A2A` |
| Roxo escuro | `#3A1F4A` |
| Cinza úmido | `#4A4A5A` |

### Global

| Uso | Hex |
|---|---|
| Outline | `#0A0A0A` |
| Texto claro | `#FFFFFF` |
| Ouro UI | `#F0C040` |

---

## 5. Tamanhos padrão

| Asset | Tamanho |
|---|---:|
| Tile | 32x32 |
| Ícone de item | 32x32 |
| Ícone UI pequeno | 16x16 ou 24x24 |
| Jogador | 32x48 |
| NPC humanoide | 32x48 |
| Árvore pequena | 32x64 |
| Portrait simples | 64x64 |
| VFX particle | 8x8 ou 8x4 |

---

## 6. Fluxo para ícones 32x32

### 6.1 Prompt base

```text
Create an original 32x32 pixel art game item icon of [ITEM].
Top-down cozy fantasy farming RPG readability.
Warm orange, bronze, moss green and earthy palette.
Transparent background.
Clean silhouette.
1px dark outline where appropriate.
No antialiasing, no blur, no text, no watermark.
Original asset, not copied from any existing game.
```

### 6.2 Exemplo — Semente de Trigo

```text
Create an original 32x32 pixel art item icon of a small wheat seed pouch.
Cozy fantasy farming RPG readability, warm harvest palette.
The pouch is tan cloth with a small golden wheat symbol, tied with a brown string.
Transparent background, clean silhouette, 1px dark outline, no text, no watermark, no antialiasing.
```

### 6.3 Retoque no Aseprite

- Reduzir canvas para 32x32.
- Remover pixels semitransparentes.
- Ajustar para paleta do projeto.
- Adicionar outline 1px quando necessário.
- Testar legibilidade em zoom 100%.
- Exportar PNG sem escala.

---

## 7. Fluxo para personagens 32x48

### 7.1 Prompt base

```text
Create an original 32x48 pixel art top-down RPG character sprite.
Character: [DESCRIÇÃO].
Cozy fantasy farming RPG, readable silhouette, simple proportions.
Front-facing idle pose.
Transparent background.
Use dark 1px outline, limited palette, no antialiasing.
Original character, not copied from any existing game.
```

### 7.2 Direções

Gerar ou desenhar separadamente:

- Down/front
- Up/back
- Left
- Right

Para MVP, pode usar placeholder. Para arte final, manter proporção consistente.

### 7.3 Animação mínima

| Animação | Frames | Prioridade |
|---|---:|---|
| Idle Down/Up/Left/Right | 1–4 | MVP/V2 |
| Walk Down/Up/Left/Right | 6 | V2 |
| Farming/Harvest | 4–6 | V2/FULL |
| Fishing | 4–6 | V2/FULL |
| Attack | 6 | Caverna/Combate |

---

## 8. Fluxo para tilesets 32x32

### 8.1 Prompt base

```text
Create an original 32x32 pixel art tile for a cozy fantasy farm RPG.
Tile type: [ground/water/soil/wall].
Seamless/repeating tile, top-down view.
Limited warm earthy palette.
No objects that break repetition.
No antialiasing, no blur, no watermark.
```

### 8.2 Regras de tileset

- Tile de chão precisa repetir sem emenda feia.
- Tile de água pode ter 2 frames de animação.
- Borda de lago precisa de variações N/S/E/W.
- Parede/borda precisa ter colisão clara no Unity.
- Evitar detalhe demais em 32x32.

### 8.3 Checklist de tile aprovado

- [ ] Repete bem em 5x5.
- [ ] Não tem ruído excessivo.
- [ ] Não parece objeto único quando pintado em massa.
- [ ] Contrasta com personagem.
- [ ] PPU 32 no Unity.

---

## 9. Fluxo para crops

### 9.1 Trigo MVP

Arquivos:

```text
Crop_Trigo_Stage0_32x32.png
Crop_Trigo_Stage1_32x32.png
Item_SementeTrigo_32x32.png
Item_Trigo_32x32.png
```

Stage0:

- terra + broto pequeno;
- ocupar menos de 40% do tile;
- verde musgo.

Stage1:

- hastes douradas;
- ocupar 70–85% da altura;
- silhueta clara.

### 9.2 Cenoura MVP

Arquivos:

```text
Crop_Cenoura_Stage0_32x32.png
Crop_Cenoura_Stage1_32x32.png
Item_SementeCenoura_32x32.png
Item_Cenoura_32x32.png
```

Stage1 deve mostrar folhas verdes e topo laranja visível, sem precisar ver a cenoura inteira fora do solo.

---

## 10. Nomenclatura

```text
[Categoria]_[Nome]_[EstadoOpcional]_[Tamanho].png
```

Exemplos:

```text
Item_SementeTrigo_32x32.png
Item_Trigo_32x32.png
Crop_Trigo_Stage0_32x32.png
Crop_Trigo_Stage1_32x32.png
Tile_Ground_Farm_32x32.png
Tile_Water_A_32x32.png
Player_Placeholder_Down_32x48.png
Tree_Carvalho_Level3_32x64.png
UI_GoldCoin_16x16.png
VFX_Leaf_8x8.png
```

---

## 11. Import no Unity

Configuração obrigatória:

| Campo | Valor |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single ou Multiple |
| Pixels Per Unit | 32 |
| Filter Mode | Point |
| Compression | None |
| Generate Mip Maps | false |
| Max Size | 2048 |

Criar preset:

```text
PixelArt_Sprite
```

Todo sprite importado deve usar esse preset.

---

## 12. QA visual

### 12.1 Teste de legibilidade

Ver sprite em:

- zoom 100%;
- fundo claro;
- fundo escuro;
- dentro da FarmScene;
- ao lado do player.

### 12.2 Critérios de reprovação

Reprovar sprite se:

- parecer borrado;
- tiver antialiasing evidente;
- tiver fundo não transparente;
- não for legível em 32x32;
- tiver detalhe demais;
- destoar totalmente da paleta;
- lembrar asset específico de outro jogo.

---

## 13. Ordem de produção de arte para MVP

1. Placeholders programáticos ou Aseprite simples.
2. Ícones: sementes, trigo, cenoura, madeira, peixe, cana, moeda.
3. Tiles: chão, borda, canteiro, água.
4. Objetos: árvore, sell point, fishing spot.
5. UI: painel, moeda, tecla E, sol.
6. VFX: folha, terra, lasca, gota.
7. Personagem final só depois do loop funcional.

---

## 14. Prompts prontos MVP

### Madeira

```text
Create an original 32x32 pixel art item icon of a small stack of cut wood logs.
Cozy fantasy farming RPG readability, warm brown and bronze palette.
Transparent background, clean silhouette, 1px dark outline, no text, no watermark, no antialiasing.
```

### Peixe comum

```text
Create an original 32x32 pixel art item icon of a common small lake fish.
Cozy fantasy farming RPG readability, blue-gray fish with simple fins.
Transparent background, clean silhouette, 1px dark outline, no text, no watermark, no antialiasing.
```

### Moeda UI

```text
Create an original 16x16 pixel art gold coin UI icon.
Simple circular coin, warm gold, dark bronze edge, readable at tiny size.
Transparent background, no text, no watermark, no antialiasing.
```

### Canteiro vazio

```text
Create an original 32x32 top-down pixel art farm soil tile.
Dark earthy soil, subtle 2-3 color texture, seamless/repeating, no plants.
No outline, no antialiasing, no blur, no watermark.
```

### Água do lago

```text
Create an original 32x32 top-down pixel art water tile for a small farm pond.
Blue water, subtle light reflection, seamless/repeating, cozy fantasy farm RPG.
No antialiasing, no blur, no watermark.
```
