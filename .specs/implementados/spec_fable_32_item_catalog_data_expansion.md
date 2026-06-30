# SPEC — Dados: Expansão do ItemDatabase para o Catálogo Canônico (~118 itens)

> **Spec ID:** `fable_32_spec_item_catalog_data_expansion`
> **Status:** A implementar
> **Wave:** FABLE Batch 5
> **Priority:** P1
> **Type:** Data / Editor
> **Domain:** Items / Economy
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_5
> **Can run with:** specs que não tocam geradores de item, ItemDatabase ou ItemDataSO
> **Must not run with:** F06, F22, F23, F31 (consomem itens — rodar F32 ANTES deles quando possível; locks de gerador de itens)
> **Repo lock scope:** ItemDataInitializer/geradores de item, ItemDatabase
> **Depends on:**
> - F30 (validador pronto p/ closeout)
> - F08 (efeitos de consumível p/ poções/comidas)
> **Blocks:**
> - F22 (essências)
> - F23 (acessórios)
> - F31 (mágicos)
> - F06 (drops)
> - F12 (produtos)
> **Scope:** materializar TODOS os itens do ITEM_CATALOG como dados com BV/efeitos.
> **Out of scope:** sistemas novos (só dados + efeitos já roteáveis); arte/ícones.

required_adrs: []
required_game_rules: [economy_rules.md]

---

# /speckit.specify

## Contexto

O `ITEM_CATALOG_DIRECTION` v1.0 é a lista nominal e canônica de ~118 itens do jogo, cada
um com ID (`item_<categoria>_<slug>`), BaseValue, fonte e uso: sementes/cultivos em 12
pares (com variantes de qualidade `_silver`/`_gold`), 20 comidas com receitas, 8 poções,
4 óleos de arma, 6 tipos de flecha, materiais de caverna por banda, essências, partes de
monstro (a lista de drops que F06/F33 referenciam), presentes, ferramentas, munição,
armas/armaduras/escudos nominais, wands/scrolls e chaves/documentos. A direction de
economia (`ECONOMY_PRICING_STOCK_REFRESH_DIRECTION`) governa preço final/restock; o
catálogo governa LISTA, BV e fonte.

O `ItemDatabase` atual tem uma fração disso, materializada pelo `ItemDataInitializer`
(padrão gerador do projeto). Esta spec NÃO cria sistemas: materializa DADOS — um gerador
idempotente, data-driven, que atualiza existentes por ID, cria os faltantes e nunca
deleta, com efeitos de consumo mapeados aos efeitos F08 já existentes e itens dormantes
documentados (cujos sistemas consumidores chegam em F22/F23/F31).

Esta spec destrava a cadeia de dados inteira: F06 (drops), F12 (produtos animais),
F22 (essências), F23 (acessórios) e F31 (mágicos) referenciam IDs que nascem aqui.
O closeout DEVE rodar o validador F30 e anexar o log de contagens.

## Problema

Enquanto o ItemDatabase tiver fração do catálogo, toda spec consumidora (drops, shops,
receitas, presentes, têmpera, identificação) referencia IDs inexistentes — exatamente a
classe de erro que o F30 acusa como ERROR. Sem gerador idempotente, re-execuções
duplicariam ou apagariam itens, quebrando saves que persistem IDs de stack. E sem
evidência de geração (regra `generated-asset-evidence`), não há como auditar se o
catálogo realmente entrou no jogo.

## Objetivo

Ao final desta spec, o ItemDatabase deve conter o catálogo canônico completo (~118 itens
+ variantes de qualidade), gerado por `GenerateCanonicalItemCatalog` idempotente
(2ª execução = zero mudanças), com BV/stack/categoria por grupo, efeitos de consumo
mapeados a F08, receitas e entradas de loja por NPC, validado pelo F30 com log anexado —
sem criar nenhum sistema novo e sem alterar itens existentes além da atualização por ID.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (INTEIRO)
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§3)
.claude/rules/generated-asset-evidence.md
.claude/skills/unity-asset-generation/SKILL.md
.claude/skills/combat-data-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- ItemDataInitializer (padrão gerador de itens do projeto) — ESTENDER este padrão;
- ItemDatabase (registry canônico de itens);
- ItemDataSO (auditar campos: categoria/BV/stack/efeito);
- efeitos de consumível (F08) — destino do mapeamento heal/stamina/buff/status-cure;
- sistema de receitas (crafting) — recebe as receitas novas;
- geradores de shop (entradas por NPC).
Não existe:
- a maioria dos itens do catálogo;
- campos p/ óleo/essência/presente (auditar se ItemDataSO precisa de aditivos);
- variantes de qualidade _silver/_gold como itens;
- tabela data-driven única do catálogo.
Auditar Fase 0:
- diff itens existentes × catálogo (tabela no report: manter/atualizar/criar);
- shape do ItemDataSO (quais campos aditivos são necessários);
- assinatura dos efeitos F08 disponíveis para o mapeamento.
```

## Engineering stories

```text
Como spec consumidora (F06/F22/F23/F31/F12), quero todos os IDs canônicos existindo no database para referenciar sem ERROR.
Como gerador, quero idempotência por ID (atualiza/cria/nunca deleta) para rodar quantas vezes for preciso sem quebrar saves.
Como economia, quero BV correto por grupo e variantes ×1.5/×2.2 para o pricing existente funcionar.
Como auditor, quero o log do F30 anexado ao closeout provando contagem = catálogo.
```

## Escopo

```text
Inclui:
- gerador GenerateCanonicalItemCatalog (idempotente — atualiza existentes por ID, cria
  faltantes, NUNCA deleta): grupos do catálogo §3-19 com BV/stack/categoria;
- variantes de qualidade como itens separados (crop_X, crop_X_silver ×1.5BV, crop_X_gold
  ×2.2BV — regra do catálogo §5, arredondamento consistente);
- consumíveis mapeados aos efeitos F08 (heal/stamina/buff/status-cure); óleos aplicam
  StatusTag temporária (weapon oil — consome F03 tags; dormante documentado se F22 não
  rodou);
- essências (8 — uma por elemento+void), partes de monstro (IDs que F06/F33 referenciam),
  presentes (gift_* p/ F26), munição (arrow_* tipos elementais);
- receitas novas de comida/poção (catálogo §7-8) no sistema de crafting existente;
- entradas de loja por NPC (catálogo §20 — quem vende o quê) via geradores de shop;
- closeout OBRIGATÓRIO: rodar ValidateCatalogConsistency (F30) e anexar log (contagens
  esperadas × reais por grupo);
- EditMode tests: BVs de amostra por grupo, variantes ×1.5/×2.2, IDs únicos, receitas
  com ingredientes válidos.
```

## Fora de escopo

```text
Não inclui:
- sistemas novos (só dados + efeitos já roteáveis hoje);
- arte/ícones;
- têmpera (F22), identificação (F31), amizade/presentes (F26), acessórios equipáveis (F23)
  — os SISTEMAS consumidores ficam nas specs deles; aqui só DADOS prontos;
- balance fino de preço pós-playtest (ECONOMY_PRICING governa; pendência declarada lá);
- loot tables por família (autoria na execução da F06 usando os IDs daqui).
```

## Regras de não duplicação

```text
Não criar segundo initializer — estender o padrão ItemDataInitializer existente.
Não criar segundo database/registry de itens.
Sistemas consumidores (têmpera, identificação, amizade) ficam nas specs deles; itens cujos
sistemas ainda não existem entram como dormantes DOCUMENTADOS, nunca como sistema paralelo.
Não duplicar regra de preço — BV aqui; preço final/restock na economia existente.
```

## Critérios de aceite

### CA-1 Catálogo completo validado

- O validador F30 reporta a categoria itens COMPLETE (contagem = catálogo, por grupo).
- Evidência: log do ValidateCatalogConsistency anexado ao execution report.

### CA-2 Variantes de qualidade corretas

- Toda variante tem BV correto (×1.5 silver / ×2.2 gold, arredondado de forma
  consistente) e ID com sufixo canônico.
- Evidência: EditMode test parametrizado nas variantes.

### CA-3 Idempotência

- 2ª execução do gerador = zero mudanças (nenhum asset alterado/criado/deletado).
- Evidência: log de duas execuções consecutivas (contadores zerados na 2ª).

### CA-4 Integridade

- Nenhum item órfão sem categoria/BV; IDs únicos em todo o database; receitas apontam
  apenas para ingredientes existentes.
- Evidência: EditMode tests de unicidade/completude + checagens (d)/(e) do F30 sem ERROR.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/Items/
  GenerateCanonicalItemCatalog.cs  (NOVO — tabela data-driven única, ordenada por seção
                                    do catálogo, idempotente por ID)
Assets/_Game/Scripts/Items/ItemDataSO.cs
  (campos aditivos SE a auditoria exigir: óleo/essência/presente)
receitas novas no sistema de crafting existente (autoria data-driven)
entradas de loja por NPC nos geradores de shop existentes
Assets/_Game/Tests/EditMode/Items/ItemCatalogDataTests.cs (NOVO)
docs/validation/fable_32_spec_item_catalog_data_expansion_execution_report.md
```

## Contratos

### Data contracts

- Tabela data-driven única: uma linha por item {id, categoria, BV, stack, fonte, efeito/
  payload, dormante?}, ordenada pelas seções §3-19 do catálogo para revisão lado a lado.
- IDs: `item_<categoria>_<slug>` (regra §1 do catálogo — ID sem categoria é erro F30).
- Variantes: `<id>_silver` (BV ×1.5) / `<id>_gold` (BV ×2.2).

### Runtime contracts

- N/A novos — consumíveis apontam para efeitos F08 existentes; óleos para StatusTags F03;
  itens sem sistema consumidor carregam flag/nota de dormante documentado.

### Event contracts

- N/A — geração de dados não publica eventos.

### Save contracts

- N/A — nenhum schema novo; idempotência por ID protege stacks salvos (IDs estáveis,
  nunca deletados).

### UI contracts

- N/A — tooltips/telas existentes leem os campos atuais do ItemDataSO.

## Sistemas afetados

```text
ItemDatabase / geradores de item (editor)
Crafting (receitas novas — dados)
Shops por NPC (entradas — dados)
Consumo de itens (mapeamento a F08 — sem mudança de código de runtime)
Validação de catálogo (F30 no closeout)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Items/** (gerador novo, estendendo o padrão existente)
Assets/_Game/Scripts/Items/ItemDataSO.cs (campos aditivos SE auditoria exigir)
autoria data-driven de receitas/shop entries (arquivos auditados na Fase 0)
Assets/_Game/Tests/EditMode/Items/**
docs/validation/**
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets SÓ via gerador editor)
Packages/**
ProjectSettings/**
Sistemas de runtime (inventário/economia/crafting — código intacto, só dados)
SaveManager / seções de save
ECONOMY pricing/restock (regras existentes intactas)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Diff itens existentes × catálogo (tabela manter/atualizar/criar no report); auditar
campos do ItemDataSO e assinatura dos efeitos F08.

### Fase 1 — Tabela e gerador
Tabela data-driven única (todas as seções §3-19, ordenada pelo doc) + gerador
idempotente (atualiza por ID / cria / nunca deleta) + testes de unicidade/idempotência.

### Fase 2 — Efeitos e grupos especiais
Mapeamento de consumo (F08), óleos (StatusTag/F03 — dormante documentado se F22 ausente),
essências, partes de monstro, presentes, munição + testes de BV por grupo e variantes.

### Fase 3 — Receitas e lojas
Receitas de comida/poção (catálogo §7-8) + entradas de loja por NPC (catálogo §20).

### Fase 4 — Geração, validação e fechamento
Rodar gerador (evidência: menu/método, log, contagens) + F30 com log anexado; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch_5.
- Can run with: specs que não tocam geradores de item/ItemDatabase/ItemDataSO.
- Must not run with: F06, F22, F23, F31 (consomem itens — rodar F32 ANTES deles quando
  possível).
- Shared files/systems that require lock: ItemDataInitializer/geradores de item,
  ItemDatabase.
- Reason: as specs consumidoras referenciam IDs nascidos aqui; execução concorrente nos
  geradores causa conflito direto.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO (itens existentes atualizados por ID, nunca recriados —
IDs de stacks salvos permanecem válidos)
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: YES — somente via gerador editor
Geração de assets: EVIDÊNCIA obrigatória (menu/método, log, exit code, contagens
esperadas × reais — regra generated-asset-evidence)
Requires Play Mode final validation: NOT REQUIRED (data-only; uso real validado nas
specs consumidoras)
Human validation timing: NOT REQUIRED (consumidoras validam em seus lotes)
```

## Riscos técnicos

```text
Risco: catálogo grande (118+ linhas) virar autoria inauditável.
Mitigação: tabela única ordenada por seção do doc, revisável lado a lado com o catálogo.

Risco: gerador não idempotente duplicar/apagar itens e quebrar stacks salvos.
Mitigação: chave por ID (atualiza/cria/NUNCA deleta) + teste de 2ª execução zero-change.

Risco: item dormante (óleo/essência/presente) parecer bug em runtime.
Mitigação: dormantes documentados (flag/nota + INFO no F30) até as specs consumidoras.

Risco: divergência de contagem silenciosa entre tabela e catálogo.
Mitigação: closeout obrigatório com F30 (contagens esperadas × reais por grupo).
```

## Rollback

```text
O gerador simplesmente não roda (ou não se commitam os assets gerados); itens novos são
inertes — nada em runtime depende deles até as specs consumidoras. Itens pré-existentes
permanecem com seus IDs; reverter o gerador/testes desfaz a spec sem tocar saves.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Diff existentes × catálogo (tabela manter/atualizar/criar no report).
- [ ] T002 — Tabela data-driven (todas as seções §3-19) + gerador idempotente + testes.
- [ ] T003 — Efeitos de consumo mapeados (F08) + óleos/essências/presentes/munição + testes de BV/variantes.
- [ ] T004 — Receitas (catálogo §7-8) + shop entries por NPC (catálogo §20).
- [ ] T005 — Rodar gerador + F30 validador + evidência (logs/contagens); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (tabela/BVs/variantes/idempotência)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: NOT REQUIRED (data-only; specs
  consumidoras validam uso em seus lotes)
- Requires regression test: YES (itens existentes atualizados por ID, não recriados —
  stacks salvos permanecem válidos)
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: testes + log do gerador + log do validador F30

## Definition of Done

```text
ItemDatabase = catálogo canônico completo (~118 itens + variantes), gerador idempotente
provado (2ª execução zero mudanças), efeitos de consumo mapeados, dormantes documentados,
receitas e shop entries gerados, evidência de geração registrada (menu/log/contagens) e
log do F30 anexado; nenhum arquivo proibido alterado; builds 0E; execution report criado.
```

## Anti-regressão

```text
Itens existentes nunca deletados/recriados — atualização por ID apenas (saves intactos).
IDs únicos com categoria (regra §1) — sem item órfão sem categoria/BV.
Regras de preço/restock da economia existente intactas (BV aqui; preço final lá).
Nenhum código de runtime alterado; nenhum sistema paralelo de efeito criado.
Variantes _silver ×1.5 / _gold ×2.2 consistentes em todo o database.
```

---

## EMENDA 2026-06-13-V3 (Refinamento v3 — VINCULANTE)

> Fonte: `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (decisões 2.1, 2.5, 2.7) +
> acréscimo #6 da re-auditoria de código (`reaudit-code-v3`, 2026-06-13).
> Em conflito com o corpo original desta spec, **esta emenda vence** (mais nova).
> O catálogo de dados real (receitas, gifts, gear tier alto, drops, peixes) é definido pelo
> **ITEM_CATALOG (artefato A4 do Refinamento v3)** — referenciar de lá, não reinventar aqui.

### V3.1 — Multiplicador Gold corrigido para ×2.0 e qualidade limitada a crops (decisão 2.1)

```text
CORREÇÃO VINCULANTE: o catálogo vence sobre a economia.
- São 3 NÍVEIS de qualidade: base / Silver ×1.5 / Gold ×2.0.
- O multiplicador Gold passa de ×2.2 (valor antigo, ERRADO) para ×2.0.
- A tabela Q0-Q4 da ECONOMY_PRICING NÃO se aplica a crops; fica para sistemas futuros.
- As variantes de qualidade (_silver/_gold) aplicam-se a CROPS; o gerador arredonda de
  forma consistente (mesma regra de arredondamento para silver e gold).

SUBSTITUIÇÕES no corpo original (passam a valer ×2.0, não ×2.2):
- linha "Como economia, quero BV correto ... variantes ×1.5/×2.2" → ×1.5/×2.0.
- Escopo "crop_X_gold ×2.2BV ... §5" → crop_X_gold ×2.0BV.
- CA-2 "×1.5 silver / ×2.2 gold" → ×1.5 silver / ×2.0 gold.
- Anti-regressão "Variantes _silver ×1.5 / _gold ×2.2" → _gold ×2.0.
- Testing Quality Gate "variantes ×1.5/×2.2" → ×1.5/×2.0.
- T002/T003 e qualquer outra menção a ×2.2 → ×2.0.

O EditMode test parametrizado de variantes (CA-2) DEVE assertar Gold = round(base ×2.0).
```

### V3.2 — Seis essências, não oito (decisão 2.5)

```text
CORREÇÃO VINCULANTE: são 6 essências (uma por banda/elemento+void conforme catálogo),
NÃO 8. O Escopo original cita "essências (8 — uma por elemento+void)" — ler **6 essências**.
O gerador e o EditMode test de contagem por grupo DEVEM esperar 6 essências; o validador
F30 (CA-1) reporta a categoria essências COMPLETE com contagem = 6.
```

### V3.3 — Enums save-safe: WeaponType e ItemCategory (decisão 2.7 + re-audit)

```text
A re-auditoria de código (2026-06-13) confirmou o estado real dos enums no working tree:

WeaponType (Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs) — valores IMPLÍCITOS hoje:
  None=0, Sword=1, Spear=2, Axe=3, Bow=4, Staff=5, Dagger=6.
  FALTAM: Hammer, Wand, Tool.

ItemCategory (Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs) — já save-safe:
  legados Seed=0..Misc=6 (inclui Tool=4); novos None=100..Furniture=110.
  FALTAM: Armor, Shield, Accessory, Relic, Essence, AnimalProduct.

DIREÇÃO SAVE-SAFE (obrigatória):
- WeaponType: adicionar Hammer / Wand / Tool com VALORES ALTOS EXPLÍCITOS, SEM renumerar
  os existentes. Como o enum hoje é ordinal (None=0..Dagger=6), os novos membros entram
  com valores explícitos altos (ex.: Hammer=100, Wand=101, Tool=102) para nunca colidir
  nem deslocar Sword..Dagger em saves/assets já serializados por inteiro.
- ItemCategory: adicionar Armor / Shield / Accessory / Relic / Essence / AnimalProduct
  com VALORES ALTOS EXPLÍCITOS APENDIDOS (próximos livres a partir de 111, ex.:
  Armor=111, Shield=112, Accessory=113, Relic=114, Essence=115, AnimalProduct=116),
  SEM renumerar nenhum valor existente.

NOTAS confirmadas pela re-auditoria (NÃO duplicar / NÃO confundir):
- `Tool` JÁ EXISTE em ItemCategory (valor 4). A adição de "Tool" é APENAS em WeaponType
  (martelo/varinha/ferramenta como TIPO de arma). NÃO criar um segundo `Tool` em
  ItemCategory.
- "armadura" como SLOT já existe via EquipmentSlot. A emenda 2.7 adiciona a CATEGORIA de
  item `Armor` (e Shield/Accessory/Relic/Essence/AnimalProduct), NÃO um novo sistema de
  armadura. É taxonomia de item, não mecânica de equipamento.
- O lado de COMBATE da expansão de WeaponType (Hammer/Wand/Tool) é refletido na
  fable_03 (equipment baselines/weapon data) — ver emenda V3 daquela spec.

ESCOPO desta emenda: estender os dois enums de forma save-safe e materializar os itens do
catálogo nas categorias novas. NÃO alterar o significado nem o valor de membros existentes.
EditMode test recomendado: assertar que os valores inteiros de Sword..Dagger e Seed..Furniture
permanecem inalterados (guarda de regressão save-safe).
```

### V3.4 — Acréscimo re-audit #6: unificar os dois modelos de item paralelos

```text
A re-auditoria (#6) confirmou DOIS modelos de item paralelos no código, com risco de drift:
- `ItemDataSO` — ScriptableObject (registry/asset, padrão materializado pelo
  ItemDataInitializer / GenerateCanonicalItemCatalog).
- `ItemDefinition` — POCO com Rarity / Quality / EconomicFlags.

DIREÇÃO REGISTRADA (decidir e seguir; evitar drift silencioso):
- DECISÃO: o `ItemDataSO` (ScriptableObject) é a SUPERFÍCIE CANÔNICA de dados de item
  materializada por esta spec. O POCO `ItemDefinition` (Rarity/Quality/EconomicFlags) NÃO
  é uma segunda fonte de verdade paralela.
- DUAS direções permitidas (escolher na Fase 0 desta spec, registrar no execution report):
  (a) UNIFICAR a superfície de dados — `ItemDataSO` absorve os campos de
      Rarity/Quality/EconomicFlags do POCO, e `ItemDefinition` vira projeção/leitura
      (ou é aposentado) para que exista UMA superfície de dados de item; OU
  (b) DEMARCAR responsabilidades — `ItemDataSO` = dados persistidos/registry (BV, categoria,
      stack, efeito, IDs); `ItemDefinition` = view/projeção runtime derivada do SO, sem
      campos canônicos próprios duplicados. Nesse caso, documentar a fronteira explícita.
- PROIBIDO: manter dois conjuntos de campos canônicos divergentes (Rarity/Quality/
  EconomicFlags vivendo em ambos como fonte de verdade independente).
- Esta emenda NÃO autoriza alterar runtime nesta tarefa de docs; ela REGISTRA a direção
  para a execução da spec. O `ItemDataSO` permanece o destino do gerador idempotente.
- Auditar na Fase 0: onde `ItemDefinition` é lido/escrito hoje; quais campos divergem de
  `ItemDataSO`; qual das direções (a)/(b) minimiza retrabalho. Registrar a escolha no report.
```

### V3.5 — Fonte de dados do catálogo (artefato A4)

```text
O catálogo de dados REAL — receitas de poção/óleo/comida, matriz de gifts (gift_*), gear
tier alto craftável, drops órfãos do bestiário, roster de peixes — é definido no
ITEM_CATALOG (artefato A4 do Refinamento v3). Esta spec MATERIALIZA esses dados; não os
inventa. Onde o corpo original cita "ITEM_CATALOG_DIRECTION_v1.0", ler como o ITEM_CATALOG
canônico atualizado pelo artefato A4. As contagens por grupo do validador F30 (CA-1)
seguem o A4 (incluindo: 6 essências; 3 níveis de qualidade Silver ×1.5/Gold ×2.0 só em crops).
```
