# Playtest exploratório — 2026-09-27

Resultado: fluxos básicos operantes, com defeitos de recuperação e problemas de legibilidade. Não constitui aceite humano nem validação completa do MVP.

Continuação: [menus, crafting, magia, plantio/rega e falha de save/load](EXPLORATORY_MENUS_2026_09_27.md). Os limites abaixo descrevem apenas as duas sessões iniciais.

## Ambiente e método
- Unity 6000.5.7f1, projeto Cindar's Hope, duas sessões reais de Play Mode iniciadas pela FarmScene; teclado/mouse na Game View.
- Exercitados Farm, Town e Cave; inventário, equipamento, diálogo, comércio, arado, dash, ataque, dano, morte e respawn.
- Aproximação a NPC/portais/plantação usou reposicionamento temporário do personagem. As interações/transições seguintes usaram input real; não certifica circulação completa.
- Saves direcionados ao diretório temporário `town-interior-save-tests/0d723e24882a4aeebcf95bdfbe925b27`. Nenhuma pasta `saves` encontrada no persistentDataPath ao encerrar.
- Evidência técnica: `Logs/exploratory-playtest.log`; imagens observadas durante a sessão de ferramentas. Não foi produzido vídeo contínuo.
- Probe temporário removido; Play Mode encerrado, FarmScene sem dirty e nenhuma cena salva. Alterações anteriores preservadas.

## Cenários executados
| Cenário | Resultado observado |
|---|---|
| Inventário/equipamento | Enxada na esquerda e espada na direita; contexto e navegação funcionaram; relógio pausado no inventário. |
| Farm ↔ Town | Ida e volta concluídas por E, mantendo ouro/inventário. Aproximações controladas. |
| Pip/loja | Diálogo e compra de semente: 60→55g, estoque 5→4; três pães de 20g recusados com saldo 55g. |
| Arado | F gerou `Tile (32,22) arado com sucesso` (log:2954); mudança visual pouco distinguível. |
| Farm → Cave | Entrada por E concluída nas duas sessões (log:6445,9546). |
| Combate | Espada equipada, dash e dois ataques E; dano 17 e arco de golpe visíveis, hit flash/recuo registrados. Um inimigo morreu e concedeu madeira (log:10531–10720). |
| Derrota/respawn | Duas mortes, tela com corpo/ouro/XP; Return confirmou respawn, HP restaurado a 100, personagem surgiu na plantação. |

## Correções prioritárias
| Prioridade | Evidência e impacto | Resultado esperado da correção |
|---|---|---|
| P1 — Lágrima indisponível na morte | Inventário tinha duas; ambas as telas mostraram zero e revive desabilitado. `CaveDeathResolver` esvazia inventário; `DeathScreenCanvasController:249` consulta esse inventário após a transferência. | Revive poder consumir exatamente uma lágrima elegível, sem duplicar corpo/itens; cobrir a ordem real dos eventos. |
| P1 — Destino de respawn incorreto | Botão promete Fonte da Anya; retorno usa `spawn_farm_default`, inexistente (log:7897,7993,8119; repetido após segunda morte). Aparece na plantação. | Spawn válido na fonte e retorno com posição, vida e controle coerentes. |
| P1 — Leitura da caverna | Blocos pretos intercalados no piso e grande retângulo claro persistiram durante entrada, movimento e luta; inimigos se confundem com rochas/chão. Causa gráfica ainda não isolada. | Piso, obstáculos e personagens distinguíveis; investigar tiles, transparência, luz e sorting antes de trocar arte. |
| P2 — Loja contradiz estoque | Após comprar, lista exibe 4 e detalhe continua 5. Mensagem de sucesso expõe `item_seed_wheat`; insuficiência de ouro aparece cortada. | Uma projeção atualizada, nomes localizados e mensagem inteira com custo/saldo. |
| P2 — Atalho durante edição | Preencher quantidade por colagem abriu Debug Info. Confirmado visualmente; filtro de foco ainda não investigado. | Campos de texto absorverem atalhos de gameplay/debug. |
| P2 — Circulação/NPCs | Dash parou em x≈24,62,y=3 na borda oeste da ponte; Zrix/Renko registraram `route_blocked`/`graph_missing`. Não prova que caminhar seja impossível. | Reproduzir caminhando e conferir colisão/rotas frente à passagem desenhada. |

## Melhorias visuais, de layout e combate
- **Coerência entre cenas:** reduzir a mudança aparente de escala/zoom de Farm para Town; revisar densidade de pixels entre personagem, construções, portão e chão. Comparar na mesma resolução/câmera.
- **Inventário:** fundo mais opaco, ícones, categorias e identificação legível de equipamento; remover `[equipment-slot:RightHand]` e mistura de idiomas. Confirmar ações destrutivas no fluxo apropriado.
- **HUD:** tornar arma/ferramenta, slot ativo e custo/recarga consultáveis durante a ação; aproximar feedback do personagem. O HUD observado prioriza quatro barras e deixa a seleção pouco clara.
- **Diálogo/loja:** diminuir área vazia, melhorar hierarquia entre retrato, fala e escolhas, ajustar quebra de linha e retirar opções de debug do fluxo normal de teste do jogador.
- **Farm:** diferenciar solo decorativo, arável, arado e plantado; destacar célula alvo e explicar F no guia de controles. Plantio/rega não foram concluídos.
- **Combate:** preservar impacto, recuo e números que já funcionam; melhorar contraste do alvo, direção/alcance do golpe e antecipação do ataque inimigo. Avaliar E compartilhado entre interação/coleta/ataque perto de recursos.
- **Organização do teste:** preparar rota curta reproduzível Farm→Pip→Cave→derrota→fonte→corpo, com checklist e condições iniciais explícitas; ocultar overlays debug durante avaliação visual final.

## Limites e continuação
- As pausas entre comandos deixam o personagem exposto; mortes observadas não demonstram dificuldade injusta. Não há avaliação confiável de balanceamento, precisão de esquiva ou timing fino.
- Áudio, gamepad, bosses, magia/projétil, crafting, pesca, ciclo completo agrícola, recuperação efetiva do corpo e save/load não validados nesta sessão.
- Dois erros do probe (`IOException`/`FileNotFoundException` no arquivo de comando) pertencem à instrumentação; não foram classificados como defeitos do jogo. Sem `error CS` ou `NullReferenceException` encontrados nesse log.
- Ordem recomendada: corrigir revive/respawn; resolver leitura da Cave; corrigir loja/foco; validar circulação; então polir HUD/inventário e medir combate com input contínuo.
- Não promove specs nem substitui `spec_validation_human_playmode_smoke_v1`. Os testes automatizados anteriores continuam com seu escopo original.
