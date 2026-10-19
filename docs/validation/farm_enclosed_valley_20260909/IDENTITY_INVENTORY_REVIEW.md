# Inventário e IDs — vale fechado v2

Comparação estática somente leitura: backup `FarmScene.before.unity.backup` SHA256 `9f7b92e72d144728f8b0abae8fffd32f3a3778cdedb205e3dc7027818347a540` contra cena salva SHA256 `c6e2af6e04b449047ad47e7c3f745fc1ad0531cd660c53a0c293c0444f30ff77`.

Resultado: IDs serializados comparados sem adições, perdas ou alterações; referências dos registries preservadas como conjuntos. Os 71 TreeNodes permanecem: o TreeRegistry contém 68 membros e o instalador SceneRuntimeReferences contém 71 (inclui as três árvores históricas do PerimeterTreeline). Não declarar que o TreeRegistry tem 71. Os quatro plots do instalador, estações e referências de dados do NPC/Zrix permanecem iguais.

| Componente | Antes | Depois |
|---|---:|---:|
| MonoBehaviour | 263 | 263 |
| Rigidbody2D | 2 | 2 |
| CircleCollider2D | 1 | 1 |
| BoxCollider2D | 164 | 165 |
| PolygonCollider2D | 4 | 31 |
| Total físico | 171 | 199 |
| SpriteRenderer | 1470 | 1902 |

As diferenças físicas correspondem à remoção de quatro paredes retangulares, inclusão de cinco bases estruturais (galinheiro, celeiro e três lados da estufa), remoção da colisão independente da montanha e inclusão das 28 faixas da fronteira natural. A periferia contém 936 SpriteRenderers e 28 PolygonCollider2D; nenhum MonoBehaviour ou Rigidbody adicional. Reposicionamentos e alterações de escala de 303 Transforms existentes incluem marcos e decoração; estão discriminados no JSON, não classificados automaticamente como preservação física.

A comparação completa dos corpos MonoBehaviour, normalizando referências locais por hierarquia/tipo, difere apenas em SceneRuntimeReferences: ordem de arrays e alias `_craftingPoint`, antes Forge e depois CookingStation. O hook `CraftingPoint.RebindCraftingManager` permanece compatibilidade sem efeito; não confundir a troca do alias com alteração dos IDs ou componentes das estações. Os demais 262 corpos MonoBehaviour normalizados são iguais.

## Reprodução e limites

[Método executável](identity-inventory-method.json) contém código Python3 sem dependências externas. Na raiz do projeto: carregar JSON e executar seu campo `source` com `exec(json.load(open(..., encoding='utf-8'))['source'])`. O método somente lê as duas cenas e escreve [resultado](identity-inventory-comparison.json) nesta pasta. A execução retornou código 0.

O parser textual segmenta documentos Unity por classID/fileID; resolve referências locais para hierarquia/tipo e compara valores de IDs, corpos MonoBehaviour e membros dos registries. Componentes do mesmo tipo duplicados no mesmo objeto podem colidir no mapa; conteúdos internos de assets externos não foram comparados. Esta revisão não executou Unity nem prova travessia física, oclusão, gameplay ou aceite humano. Capturas e probes do root constituem evidência separada.
