# Skills 21F — execução e decisão de validação

**Data:** 2026-09-13  
**Spec:** `.specs/a_implementar/spec_skills_21f_purchased_origin_v1.md`  
**Decisão /finish-spec:** **PARCIAL; não promover**. T1–T6 têm runtime e testes focais; T7 não satisfaz AC04/AC07 completos. T8 teve revisão independente sem P1/P2 no recorte implementado, mas não pode encerrar a spec.

## Comportamento entregue

- Compra registra custo efetivamente pago por unidade. Venda em Shop/SellAll/SellPoint/Shipping limita cada unidade comprada a menos que o custo, após os bônus; produto próprio conserva preço normal.
- Inventário, drop/pickup, corpo, shipping, crafting, processamento, serviços, têmpera e refunds transportam lotes. Save v6 migra v5 para `LegacyUntracked` e trata origem v6 ausente como `Invalid`; preflight rejeita IDs impossíveis antes de aplicar o estado.
- UI de venda projeta quantidade elegível, retenção e payout do mesmo quote usado no commit. A Farm tinha um pickup estático `seed_carrot` fora do ItemDatabase; cena e gerador agora usam `item_seed_carrot`, corrigindo o roundtrip v6 sem alterar sua posição.
- Morte com flechas esgotadas ou arco inicial equipado por binding genérico não cria item fantasma no corpo. Recuperação equipada publica eventos após inventário, equipamento e corpo ficarem coerentes.

## Evidência Unity vigente

| Gate | Resultado | Artefato |
|---|---|---|
| EditMode focal T1–T6, save v5/v6, morte, shipping, crafting, UI | **PASS 200/200**, scanner PASS, exit 0 | `Logs/skills21f-t1-t6-editmode-r10.xml/.log` |
| PlayMode de arbitragem Farm anterior | **PASS 1/1**, scanner PASS | `Logs/skills21f-existing-arbitrage-playmode-r2.xml/.log` |
| PlayMode de SellPanel real | **PASS 1/1**, scanner PASS | `Logs/skills21f-sell-panel-playmode-r2.xml/.log` |
| PlayMode Renko→SellPoint→Shipping→save/load→overnight/retry | **PASS 1/1**, scanner PASS, exit 0 | `Logs/skills21f-economy-playmode-r5.xml/.log` |
| PlayMode receita registrada de madeira e forja com job de 4s | **PASS 1/1**, scanner PASS, exit 0 | `Logs/skills21f-craft-chains-playmode-r3.xml/.log` |
| PlayMode agrícola L20 base, L50 base/R1, L54 base/R3 | **PASS 5/5** em processos separados, scanners PASS | `Logs/skills21f-income-l20-r1.xml/.log`; `Logs/skills21f-income-l50_*r1.xml/.log`; `Logs/skills21f-income-l54_*r1.xml/.log` |
| Revisão independente T1–T6 | **Sem P1/P2 restante** após correção do binding do arco e pickup da Farm | Revisão do diff r9/r5 e recheck dos três preços T5 após EditMode r10/PlayMode craft r3 pelo agente `skills21f_t1_nonregression` |

O editor original estava aberto e bloqueava batch. Os testes rodaram numa cópia isolada em `D:\Projetos\Cindars_Hope\cindars_hope_validation_21f`, sincronizada com Assets/Packages/ProjectSettings e fixtures; não se fechou o editor do usuário. EditMode incluiu compilação Unity; o PlayMode de crafting r2 compilou a correção posterior do gerador da Farm. A cópia não demonstra uma sessão humana no editor original.

Os runners falhos intermediários permanecem como histórico: EditMode r5/r6 por arquivo PlayMode fora da assembly/import faltante; PlayMode economia r1–r3 por fixture de boot/recompensa diária, r4 expôs o pickup inválido; craft r1 esperava um log de boot que não ocorreu. Só os XMLs acima contam como PASS. A recompensa de meta diária foi separada do payout por `EconomyTransactionCompletedEvent`, impedindo falso ganho de arbitragem.

O ratchet arquitetural global foi executado após a integração e **FAIL** no worktree compartilhado. Três novas ocorrências de `Resources.Load` em Craft/Processing foram removidas pela 21F; o contador `ResourcesLoad` voltou a **22/22** do baseline. As falhas remanescentes estão em NPC, input, UI/Crafting, Skills e SceneManagement de trabalhos concorrentes, fora do recorte 21F. Não declarar o gate global PASS. O gate documental `quality` com manifesto 21F foi **SCOPED_PASS**; o gate global de documentação não foi repetido.

No checkout atual não existe asset `EconomyBalanceConfig`, então os três caminhos de craft usam o multiplicador default 1,0 de `ItemPriceResolver`, igual ao `Resources.Load` anterior que retornava null e ao SellPoint real. Se um asset de balanceamento com multiplicador de Shipping diferente for materializado no futuro, a mesma configuração deverá ser injetada no cálculo de orçamento de origem e no canal de venda. A revisão independente confirmou ausência de mudança de preço no estado atual.

## Equilíbrio e trabalho restante

Na rotina agrícola controlada D3–D5, as medianas compostas pós-21F ficaram em **90g L20 base, 90g L50 base/R1, 90g L54 base e 91g L54 R3**; o incremento R3 nessa cesta foi 1,11%. A venda de madeira de receita registrada retornou **8g para 10g de base comprada**. A forja com minério comprado/próprio conservou orçamento de **23g** no job e na coleta (8g restrito + 15g próprio). [Matriz T7](../balance/PHASE_21F_T7_INCOME_AND_TRANSFORMATION_MATRIX.md) explica método e limites.

Para AC04/AC07, faltam as receitas `recipe_grilled_fish`, `recipe_leather_from_hide` e `recipe_leather_armor` no `RecipeDatabase` executável; seus assets existem, mas registrar GUIDs/estações exige trabalho de catálogo e wiring. O asset legado `recipe_processed_wood` foi intencionalmente excluído e não deve ser reativado. A segunda transformação nomeada, a rota natural Town→Farm com NPC/SellAll, a cesta global L20/L50/L54 (comida, Cave, combate, quests e todos os sinks/sources), o limite `<20%`, hash pré-batch e aceitação gráfica/humana **não foram comprovados**. Manter 21F em `.specs/a_implementar/` e AC05 global **PARCIAL**, sem promover a skill tree visual por este relatório.

O [draft 21G](../../../../.specs/features_futuras/skills_sdd_v1/spec_skills_21g_recipe_activation_v1.md) contém spec→plan→tasks para as três receitas, com decisões de balanceamento D1–D3 ainda abertas e implementação não autorizada. A dependência é de T1–T6 da 21F, sem ciclo com o T7 que ela desbloqueia.
