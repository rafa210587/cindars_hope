# SPEC — Town Layout v9 (orgânico, guiado pela imagem de referência)

**Status:** A_IMPLEMENTAR
**Tipo:** WAVE_INTEGRATION / scene layout (editor-only)
**Dependências:** Fundação de rendering v2 já aplicada (sorting layers Ground/World/Roof + Y-sort custom axis Y + pivots BottomCenter — concluída em 2026-07-03).
**Origem:** Pedido direto do usuário (2026-07-03): aproximar a TownScene da imagem de referência gerada (cidade murada, portão sul, praça central com estátua/fonte, distritos), podendo ser MAIS ORGÂNICO que a grade rígida da referência.

---

## Objetivo

Reorganizar o layout da cidade em `TownCityLayout.cs` / `TownDistrictLayout.cs` / `CreateMvpTownScene.cs` para refletir a composição da imagem de referência, mantendo TODOS os contratos congelados (nomes de lote, arquétipos, roster de 28 NPCs, IDs).

## Invariantes (NÃO PODEM MUDAR)

1. Os **24 nomes de lote** (`House_*`) e seus **arquétipos** — apenas posição, tamanho e lado da porta mudam.
2. Os **28 NPC ids** e a semântica das âncoras (work/social/home) — as posições das âncoras são RECALCULADAS junto com os prédios (work dentro/adjacente ao prédio do NPC; social na praça/taverna/mercado; home = interior do prédio).
3. Footprint total **120×90** (x −60..60, y −45..45), muralha no perímetro, **portão único ao SUL em x=0** (gate half 4).
4. Portais Town↔Farm e spawn points: manter IDs; se posição conflitar com o novo layout, mover o mínimo necessário (portão/borda).
5. Padrão walk-in das casas (piso interior + shell + porta + telhado RoofReveal) intocado — só as coordenadas dos lotes mudam.
6. Sorting: taxonomia Ground/World/Roof do contrato de rendering v2 — nenhuma regressão.
7. Auditoria de contagem (`LogRelayoutElementCountAudit`) e auditoria de overlap continuam passando — atualizar baselines se contagem mudar legitimamente.

## Composição-alvo (traduzida da referência, versão orgânica)

Coordenadas são ALVO com tolerância ±2un para resolver overlap/clearance. Regras duras: nenhum lote a menos de 3un da muralha; nenhuma sobreposição lote-lote nem lote-rua; **toda porta abre para uma rua adjacente**.

### Praça central (coração da cidade)
- Pavimento cobble aproximadamente circular, centro **(0, 4)**, raio ~13.
- Centro: **estátua sobre a fonte** (estátua existente + fountain basin como pedestal d'água).
- 2 anéis ao redor do centro: anel interno com 8 canteiros de flores (foliage `flower_patch`), anel externo com 6 bancos + 4 postes de luz.
- Placa de avisos (board) na borda sul da praça.

### Avenidas
- **Avenida N-S**: x=0, largura 6, do portão sul (y=−45) até a fachada cívica (y≈26).
- **Avenida E-W**: y=4, largura 5, de x=−50 a x=50, tangenciando a praça.
- Rua de fachada cívica: y≈26, de x=−52 a x=52.
- Ruas secundárias: ligar CADA porta de lote à malha (segmentos curtos, orgânicos — não precisa grade perfeita).
- Postes de luz ao longo das avenidas a cada ~12un.

### Distrito NORTE (cívico/religioso) — fachadas com porta SUL, ao longo de y≈30–36
| Lote | Centro alvo | Tamanho | Porta |
|---|---|---|---|
| House_Temple (igreja) | (−38, 32) | 16×13 | S |
| **Cemitério** (mover de outskirts p/ cá) | região (−52..−44, 26..38) | — | — |
| House_Chamber (town hall) | (−12, 33) | 15×12 | S |
| House_Prison (guarnição) | (8, 33) | 12×11 | S |
| House_Manor (mansão) | (34, 33) | 16×12 | S |
| House_Registry | (22, 20) | 11×9 | S |
| House_Archive | (48, 20) | 11×9 | W |

### Distrito OESTE (comércio + parque)
| Lote | Centro alvo | Tamanho | Porta |
|---|---|---|---|
| House_MarketHall | (−34, 12) | 14×10 | E |
| Tenda de mercado (prop) | ao lado S do MarketHall | — | — |
| House_Bakery | (−20, 14) | 10×8 | S |
| House_Inn (taverna) | (−22, −4) | 14×11 | E |
| **Lago/parque**: água orgânica ~14×10 centrada em (−38, −18), deque de pesca na borda E, cercado por árvores + cerca com portão | | | |
| House_Fishery (moinho d'água) | (−46, −8) | 11×9 | E |

### Distrito LESTE (ofícios)
| Lote | Centro alvo | Tamanho | Porta |
|---|---|---|---|
| House_Blacksmith | (30, 12) | 12×9 | W |
| House_AlchemyLab | (44, 12) | 11×9 | S |
| House_Workshop | (30, −6) | 12×9 | W |
| House_Tannery | (46, −6) | 11×9 | S |

### Distrito SUL (residencial) — casinhas com jardim cercado
Duas colunas ladeando a avenida N-S, portas voltadas para a avenida ou para vielas N-S locais:
| Lote | Centro alvo | Tamanho | Porta |
|---|---|---|---|
| House_Residential_4 | (−28, −22) | 8×7 | E |
| House_CarvalhoTorto | (−28, −34) | 8×7 | E |
| House_Residential_1 | (−16, −22) | 8×7 | E |
| House_Dagna | (−16, −34) | 8×7 | E |
| House_Residential_2 | (14, −22) | 8×7 | W |
| House_Pip | (14, −34) | 8×7 | W |
| House_Residential_3 | (26, −22) | 8×7 | W |
| House_Tovin | (26, −34) | 8×7 | W |
| House_GateKeeper | (8, −41) | 9×7 | W (junto ao portão) |
| House_AnimalYard (armazém/celeiro) | (−42, −32) | 16×12 | E |

Cada lote residencial: cerca perimetral com vão na frente da porta + 1–2 flower_patch/arbusto no jardim.

### Verde
- Manter anel de floresta EXTERNO à muralha (BuildBorderTreeRing).
- ADICIONAR fileira orgânica de árvores INTERNA junto à muralha (como na referência), sem bloquear ruas/portas.
- Parque do lago: agrupamento denso de árvores + bancos.

### Distritos (`TownDistrictLayout.cs`)
Atualizar os bounds dos 7 distritos para as novas regiões (mesmos IDs/nome de distrito).

## Fora de escopo
- Trocar sprites/visual das casas (spec de visuals, separada).
- Qualquer código runtime, save, NPC schedule logic (só as POSIÇÕES das âncoras mudam nos dados).
- Edição manual de .unity/.prefab/.asset (regeneração é via `CindarsHope/Inicializar Projeto` pelo humano).

## Arquivos permitidos
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` (ruas, praça, props, cemitério, lago, árvores internas)

## Validação obrigatória
1. `dotnet build .\Assembly-CSharp.csproj --no-restore` e `.\Assembly-CSharp-Editor.csproj` — exit 0.
2. Auditoria estática de overlap: nenhum par lote-lote/lote-rua sobreposto; toda porta com rua adjacente; clearance ≥3un da muralha (implementar como verificação no próprio gerador se ainda não existir, logando erro claro).
3. Humano: regenerar via `CindarsHope/Inicializar Projeto` e validar em Play Mode (andar do portão à praça, entrar em 3 casas, visitar cada distrito).

## Critérios de aceite
- [ ] Portão sul + avenida axial + praça circular com estátua/fonte e anéis de canteiros/bancos.
- [ ] 7 distritos organizados conforme tabelas (tolerância ±2un).
- [ ] Cemitério interno junto ao Temple; lago/parque com deque e moinho ao lado.
- [ ] 24 lotes, 28 NPCs, portais e spawns preservados (nomes/IDs idênticos).
- [ ] Cada residência com cerca+jardim; postes nas avenidas; tenda ao lado do mercado.
- [ ] Builds exit 0 + auditorias de overlap/contagem passando.
