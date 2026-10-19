---
doc_type: refinement
status: inbox
source_of_truth: false
implemented_by: ""
superseded_by: ""
must_be_self_contained_in_spec: true
read_by_executor_by_default: false
---

# Skills 21G — utilidade e equilíbrio das receitas de peixe e couro

> 2026-09-13. Revisão de design anterior à ativação. Propostas aqui não alteram dados nem autorizam implementação. Draft dependente: [spec 21G](../../../.specs/features_futuras/skills_sdd_v1/spec_skills_21g_recipe_activation_v1.md). A procedência comprada da [21F](../../../.specs/a_implementar/spec_skills_21f_purchased_origin_v1.md) já protege a revenda; não prova utilidade, ritmo ou acesso às receitas.

## Resultado desejado em Vaalara

Pescar em Dornecia deve oferecer provisão para incursões na Cave ou renda justa na Town; cozinhar deve aumentar a utilidade da captura sem superar trivialmente pão e plantio. O couro obtido de criaturas deve alimentar um ofício identificável; a armadura deve reduzir dano de fato quando equipada. Sael e Hess devem oferecer alternativas convenientes sem tornar pesca, loot e costura irrelevantes. O jogador precisa encontrar a estação, ver custo/resultado, concluir o trabalho e receber feedback de falha.

## Evidência observada e limites

| Cadeia atual | Valor dos insumos → saída | Efeito jogável observado/inspecionado | Implicação |
|---|---:|---|---|
| Perca → grelhado, Cooking 0s/0 STA | 14g → 40g; 2,86× | `HungerRestore=35`; `ItemUseContractResolver` e `FoodConsumer` permitem consumo. Sael vende perca e grelhado. | Fora da faixa Food 1,10–1,80× dos [economy rules](../../game_rules/economy_rules.md); +26g por perca Earned cozida e vendida. |
| 2 hides → leather, Sewing 0s/0 STA | 16g → 15g | Hess vende hide e leather; nenhum drop/produção Earned de hide encontrado; beast oferece leather direto. | Conversão perde 1g em valor base e não fecha o loop de loot. |
| 4 leather → armadura, Sewing 0s/0 STA | 60g → 100g | Item equipa no Chest pelo fallback de ID, mas `PlayerAttackController` constrói stats sem `equipmentSource`; o `BaseDefense=5` do equipamento não chega à mitigação por esse caminho. | Desconto nominal de craft de 40g contra Hess, porém equipamento sem defesa demonstrada é armadilha de valor. |

As três receitas existem como assets, mas seus GUIDs não estão no `RecipeDatabase`. A Farm tem runtime/modal Cooking; a Town não materializa `CraftingRuntime`/`CraftingModal` para a estação Sewing. `GameBootstrap` conserva o primeiro `_Bootstrap` entre cenas e destrói o segundo: referências da segunda cena exigem rebind explícito. Os três outputs têm ícone vazio e descrição provisória. Há ainda um SKU legado distinto, `item_shop_armor_leather` de 35g, que partilha o nome da armadura de 100g; sua disponibilidade real em cena ainda não foi comprovada. A leitura estática da defesa exige teste de dano antes/depois de equipar, não declaração de falha PlayMode já medida.

Perca comprada a valor nominal: três unidades custam 42g e viram 105 pontos de fome, contra dois pães a 40g/60 fome. A cotação real, inclusive com Senso R3 legal em L50/L54, ainda não foi capturada. Uma perca Earned extra por dia num cenário controlado de 3 canteiros elevaria 104→130g/dia (+25%); isso **não é** a mediana global nem prova falha do gate de 20%. Fishing exige 20 STA por tentativa e minigame; um craft instantâneo não pode ser tratado como o trabalho que justifica sozinho o prêmio de 26g.

## Alternativas e decisão recomendada

| Alternativa | Benefício | Custo/risco | Veredito |
|---|---|---|---|
| Manter as três dormentes | Preserva economia e itens atuais | Não fecha as cadeias nomeadas pela 21F | Estado seguro enquanto faltam provas; não chamar de conclusão. |
| Registrar as três como estão | Menor diff de catálogo | Peixe acima da regra, hide sem fonte, armor sem defesa e Sewing inacessível | Rejeitada: habilitar asset não produz mecânica funcional. |
| Reparar fluxo e ativar seletivamente | Cada receita passa por acesso físico, utilidade, balance e renda | Exige specs encadeadas de runtime, dados e teste | Recomendada; peixe primeiro apenas após ajuste econômico aceito e medido. |

**R21G-01 — ordem proposta.** Primeiro estabilizar o serviço Craft entre Farm/Town, modal local, SaveManager e validadores contraditórios. Em seguida corrigir a economia do peixe e medir a jornada real. A cadeia hide/couro e armadura só entra após fonte Earned de hide, efeito de equipamento em dano e identidade visual/nominal dos itens. O sistema de estação `Craft/` é o executor; `Crafting/` permanece para unlocks existentes. Não criar terceiro manager.

**R21G-02 — alvo numérico proposto para peixe, pendente.** `BaseValue` do grelhado em torno de **23g**: 35/30 × pão20 ≈23, dentro de 1,10–1,80× perca14 (máximo 25g). Com isso a perca Earned cozida acrescentaria 9g em vez de 26g; no cenário controlado 104→113g/dia (+8,65%), ainda sem prova global. Reduzir só o grelhado não corrige fome comprada barata. Cotação de Sael para perca **≥19g em L20** é hipótese de teste (19/35 ≈0,543g por fome vs pão 20/30≈0,667), sujeita a recotação com Senso R3 e cestas reais L50/L54. Não gravar esses valores antes da decisão de balanceamento e da revisão da spec 21G; preços/BV são propostas condicionadas, não autorização vigente.

**R21G-03 — cadeia de couro proposta, pendente.** Uma fonte Earned de hide ligada ao bestiário deve ter frequência e custo de combate medidos; não acrescentar drop arbitrário. Para 2 hides de 8g, leather com BV **≥17g** deixa de perder valor; **20g** daria prêmio de 25% por analogia ao ingot, mas altera a base de 4 leather para 80g e exige rever a armadura de 100g. Costura 4s/0 STA pode usar a forja como comparador de job, sujeito a save/cancel/rollback. Primeiro provar que a armadura do item da receita transmite `BaseDefense=5` e causa delta determinístico de mitigação; verificar o SKU legado de 35g e a progressão de equipamento. Sem isso, não ativar receita de armadura.

**R21G-04 — feedback mínimo.** Receita no ponto físico deve exibir item/ícone/descrição úteis, ingredientes, tempo/STA, output e razão de indisponibilidade; erro de ingrediente, estação ou inventário cheio deve ser percebido sem gasto parcial. Ativos visuais precisam seguir Point/None/no mip maps. Ícones provisórios não autorizam dizer que a apresentação visual foi concluída.

## Aceite mensurável e slices de spec

1. **Fundação Craft entre cenas:** [spec 21G0](../../../.specs/features_futuras/skills_sdd_v1/spec_skills_21g0_craft_cross_scene_foundation_v1.md) independente. Inícios diretos Farm/Town, Farm→Town→Farm e Town→Farm→Town com exatamente um runtime persistente; modal/estação local correta, provider de save ligado ao runtime sobrevivente, sem duplicar outputs. Reconciliar os dois validadores sobre a receita de madeira legada sem a reintroduzir.
2. **Peixe:** compra e cotação real de Sael, pesca Earned com seed controlada, craft físico, consumo e venda por NPC/SellPoint/Shipping; custo por fome e renda mediana pré/pós em builds legais L20/L50/L54. Exigir razão de renda 0,80–1,20 e preservar anti-arbitragem da 21F. Checar feedback e ícone.
3. **Couro/armadura:** fonte de hide em encontro real, duas transformações, rollback/save entre elas, comparação de defesa/dano antes/depois de equipar e cotações Hess/SKU legado. Sem mitigação observada ou fonte Earned, a cadeia permanece dormente.

**Perguntas materiais ainda abertas:** aprovar ou não o ajuste 23g/≥19g do peixe; qual encontro de Vaalara deve gerar hide Earned e em que frequência; que peça deve ser a armadura canônica se o SKU legado estiver acessível; custo/gate de Sewing após medir acesso e renda. O executor não deve escolher essas respostas incidentalmente. A fundação foi separada no draft 21G0; a ativação 21G depende de sua evidência e das decisões D1–D3.
