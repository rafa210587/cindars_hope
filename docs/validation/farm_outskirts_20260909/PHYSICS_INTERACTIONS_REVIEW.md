# Revisão independente — física e interações da periferia

Resultado: PASS no escopo estático de preservação física/IDs. Há diferenças de serialização em um instalador, sem efeito comportamental atual demonstrável, discriminadas abaixo. Revisão somente de leitura; nenhum código, cena ou asset foi modificado pelo revisor. Evidência de 2026-09-09.

## Entradas comparadas

- Antes: `FarmScene.before.unity.backup`, cópia do executor cujo hash confere com o snapshot independente capturado antes da geração.
- SHA256 antes: `751A9263324F2A322135E9900E969FC5B22F73955DB31D629A592296FFACF803`.
- Depois: `Assets/_Game/Scenes/FarmScene.unity`, geração final após correção da emenda visual da estrada leste.
- SHA256 depois: `9F7B92E72D144728F8B0ABAE8FFFD32F3A3778CDEDB205E3DC7027818347A540`.

## Resultado concreto

| Categoria | Antes | Depois | Diferença relevante |
|---|---:|---:|---|
| Rigidbody2D | 2 | 2 | 0 |
| CircleCollider2D | 1 | 1 | 0 |
| PolygonCollider2D | 4 | 4 | 0 |
| BoxCollider2D | 164 | 164 | 0 |
| MonoBehaviour | 263 | 263 | Um instalador, explicado abaixo |
| Componentes com campos de ID serializados | 54 | 54 | 0 |

Os 171 componentes físicos preservaram integralmente dados, referências, cadeia de posição/rotação/escala e estado ativo/layer/tag dos objetos e ancestrais. Não apenas suas contagens. NPC_Zrix_Farm e seu wiring estão entre os componentes preservados. Valores/listas e referências externas dos MonoBehaviours entraram na comparação integral.

Sob `FarmPerimeterVisuals`, a cena final contém 621 Transform (raiz + filhos) e 620 SpriteRenderer. Zero Collider, Rigidbody ou MonoBehaviour nesse ramo: a decoração exterior não introduz interações ou física. O helper `Place` cria somente SpriteRenderer; `FitsExterior` rejeita bounds que sobreponham o retângulo jogável expandido e restringe vegetação alta no corredor leste. Essa leitura do código sustenta a intenção de posicionamento, sem substituir avaliação visual das capturas.

## Diferenças de serialização, sem ocultar o diff bruto

A comparação integral retorna dois registros (antes/depois) de um único componente: `SceneRuntimeReferences`.

1. `_farmPlots`: mesmos 4 membros, ordem diferente; zero adição/remoção.
2. `_treeNodes`: mesmos 71 membros, ordem diferente; zero adição/remoção.
3. `_craftingPoint`: `CraftingStation_CookingStation` passou para `CraftingStation_Forge`.

O instalador percorre plots/trees somente para injetar as mesmas referências de inventory/stamina em cada elemento (`FarmSceneRuntimeReferenceInstaller.cs:92–111`); não seleciona um elemento com base na ordem. As listas dos registries e os dados dos elementos continuam iguais na comparação integral.

O gerador já seleciona `craftingPoints[0]` após uma busca de objetos (`CreateMvpFarmScene.cs:2205–2207`). O consumidor (`FarmSceneRuntimeReferenceInstaller.cs:132–134`) apenas chama `RebindCraftingManager`, que é um compatibility hook vazio (`Craft/CraftingPoint.cs:69–72`). A interação real (`CraftingPoint.cs:41–47`) usa runtime/modal próprios de cada estação, todos preservados.

Após ordenar somente esses dois arrays do instalador e excluir somente seu alias sem efeito, há zero diferenças nos 434 componentes examinados. Nenhuma ordenação global de arrays foi aplicada. Nenhum fix/refactor de crafting foi feito. Esses desvios devem ser reavaliados se os consumidores passarem a depender de ordem ou se o hook ganhar comportamento.

## Método e limites

Leitura de blocos YAML sem edição. Class IDs Unity identificam física/MonoBehaviours; referências locais fileID são normalizadas para caminho hierárquico + tipo + script GUID. Referências externas com GUID permanecem literais. Objetos são comparados por conteúdo ordenado, evitando tratar renumeração de fileIDs como regressão. Transform e estado de ancestrais também integram a assinatura.

A extração curta de IDs é auxiliar; a comparação integral inclui listas e campos que não terminam em Id. A normalização por caminho/tipo pode não distinguir referências entre componentes idênticos duplicados no mesmo objeto; não é um parser geral de todos os formatos Unity. Não compara conteúdo interno de assets externos, apenas referências. Não prova gameplay, aceitação visual, oclusão nem caminhada humana. Nenhum Unity foi iniciado pelo revisor. Rotas/capturas obtidas por outro owner precisam de sua própria evidência.

## Evidência

- [Comparação final](semantic-comparison.json): hashes, contagens, diff bruto, arrays/alias e inventário exterior.
- [Semântica antes](semantic-before.json) e [depois](semantic-after.json): dados antes da normalização excepcional dos três campos documentados.
- [IDs antes](interactive-ids-baseline.json) e [depois](interactive-ids-after.json).
- [Snapshot inicial independente](physical-interactive-baseline.json).
