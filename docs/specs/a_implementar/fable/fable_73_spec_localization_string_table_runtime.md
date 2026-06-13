# SPEC — Localization String Table Runtime (tabela id→string para texto de quest/diálogo a partir da P4)

> **Spec ID:** `fable_73_spec_localization_string_table_runtime`
> **Status:** A implementar
> **Wave:** FABLE (infraestrutura de texto — habilitadora da P4; deve entrar antes/no início da P4)
> **Priority:** P2
> **Type:** Runtime (infra leve + disciplina de autoria)
> **Domain:** Localization / Text-authoring / Quest / Dialogue
> **Parallelizable:** YES
> **Parallel group:** janela LIVRE (cria um serviço novo, isolado, sem reescrever sistemas existentes)
> **Can run with:** qualquer spec que não recrie um serviço de texto/localização nem mexa no contrato dos campos `*Key` de quest
> **Must not run with:** specs que reescrevam `QuestDefinition`/`QuestStepDefinition`/`QuestObjective` (campos `*Key`) ou que introduzam um segundo resolver de texto
> **Repo lock scope:** `Localization/**` (novo), `Assets/_Game/Tests/EditMode/Localization/**` (novo), csproj includes
> **Depends on:**
> - GameEventBus (existente — não usado para lookup, mas o serviço respeita a invariante de comunicação)
> - F67 (`fable_67_spec_canonical_governance_input_map_adr` — governança canônica de input map / convenção de id; esta spec ALINHA a convenção de id de texto com F67, sem depender de execução dela para o lookup básico)
> **Blocks:**
> - `fable_35_spec_npc_side_quest_chains` (texto NOVO de quest da P4 deve referenciar id→string desta tabela)
> - `fable_36_spec_main_quest_acts_2_4` (idem — texto NOVO de quest principal)
> - `fable_70_spec_npc_side_quest_chains_wave2` (idem — segunda onda de side quests)
> - toda spec de texto de quest/diálogo da P4 em diante (referenciam chaves desta tabela)
> **Scope:** `LocalizationService` leve (lookup por id, idioma único PT-BR default, fallback ao próprio id quando a chave falta — SEM framework de terceiros, SEM troca de locale em runtime, SEM .po/.resx); a convenção de autoria id→string para texto NOVO de quest/diálogo a partir da P4; o registro de como specs futuras de texto declaram suas chaves; EditMode tests (lookup OK, fallback ao id, ausência de chave, chave vazia/nula).
> **Out of scope:** retrofit do texto JÁ hardcoded das waves anteriores (ADR-0012 — débito explícito, NÃO re-tocar); framework de localização; troca de idioma em runtime; UI de seleção de locale; fallback de fonte por locale; pluralização; persistência em save (a tabela é asset/recurso de dados, não estado de jogo); textos de UI chrome / logs / chrome de menu (a menos que uma spec futura opte por entrar).

required_adrs: [ADR-0012-localization-string-table-from-p4.md]
required_game_rules: [documentation_rules.md, event_rules.md]

---

# /speckit.specify

## Contexto

Decisão **4.7** do Refinamento v3 (`docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`, VINCULANTE, 2026-06-13,
marcada como **OVERRIDE** do default) e sua materialização canônica em **ADR-0012 —
Localization via String Table from Phase P4** (`docs/decisions/ADR-0012-localization-string-table-from-p4.md`):
a partir da fase **P4** (a onda F35 / F36 / F70 + trabalho de diálogo), texto de quest e de
diálogo passa a ser autorado via uma **tabela id → string** — uma disciplina leve, **sem
adotar framework de localização**. Apenas um idioma (o de autoria, PT-BR) é populado em v1; a
disciplina existe para que um segundo idioma possa ser adicionado depois **populando a tabela**,
não reescrevendo cada call site.

O ADR-0012 fixa explicitamente a fronteira de retrofit (seção "Registered Debt"): o texto JÁ
hardcoded nas waves anteriores à P4 **não** é migrado por esta decisão; essa migração é débito
registrado, agendável só se/quando um segundo idioma ou framework completo for adotado. Esta
spec entrega a **semente** desse caminho: o serviço de lookup mais a convenção de autoria — sem
tocar no texto legado.

A re-auditoria de código de 2026-06-13 confirmou um ponto decisivo: os modelos de quest já
expõem campos terminados em `Key` (`DisplayNameKey`, `DescriptionKey`, `HiddenDisplayNameKey`,
`HintTextKey`, `DescriptionKnownKey`, `DescriptionHiddenKey`), e o
`QuestLogProjectionService` hoje usa esses campos **diretamente como texto de exibição** com
fallback ao id da quest (`def.DisplayNameKey ?? def.QuestId`). Ou seja, hoje "a chave É o texto".
Esta spec converte esse acoplamento implícito numa **resolução real id→string com fallback ao id**,
exatamente o comportamento que o projeto já pratica de forma ad-hoc — formalizado, testado e
reutilizável por toda spec de texto da P4 em diante.

## Problema

1. **Sem ponto único de resolução de texto.** Texto de quest/diálogo NOVO continuaria sendo
   escrito como literal inline no call site (como em `TownNpcDialogueLibrary.cs`, ~600 linhas de
   PT-BR embutido). Sem uma tabela id→string, um futuro esforço de localização vira um retrofit
   que toca cada call site do projeto — exatamente o cenário que o ADR-0012 quer evitar.
2. **Fallback ad-hoc e não testado.** O único "lookup" existente é o
   `def.DisplayNameKey ?? def.QuestId` espalhado em `QuestLogProjectionService` — sem contrato,
   sem teste, sem garantia de que chave ausente/vazia degrade de forma previsível (mostra a
   chave/id em vez de quebrar ou exibir vazio inesperado).
3. **Sem convenção declarada de chaves.** Não há regra de como as specs F35/F36/F70 e
   posteriores nomeiam e declaram suas chaves de texto, então cada uma inventaria a sua —
   reintroduzindo a divergência que a tabela centralizada deve eliminar.

## Objetivo

Ao final desta spec o projeto deve ter: (1) um **`LocalizationService` leve** — lookup
`string Get(string id)` que resolve um id de texto contra uma **tabela id→string** PT-BR e,
quando o id não existe / é nulo / vazio / não tem entrada, **retorna o próprio id** (fallback
determinístico e visível, mesmo padrão do `?? QuestId` já praticado), sem exceção e sem string
vazia surpresa; (2) uma **fonte de tabela** simples (mapa id-keyed em dados, mirando o padrão de
catálogo estático já usado por `TownNpcDialogueLibrary`/`NpcDialogueSetRegistry` — sem framework,
sem locale-switch, sem .po/.resx); (3) a **convenção de autoria id→string** documentada para
texto NOVO de quest/diálogo a partir da P4, incluindo como uma spec futura declara suas chaves;
(4) **EditMode tests** cobrindo lookup OK, fallback ao id, chave ausente, chave vazia/nula, e
idempotência do lookup. NÃO migra o texto legado pré-P4 (ADR-0012). Sem dependência de save. Sem
GameObject.Find/FindObjectOfType. Sem comunicação fora do GameEventBus para gameplay.

## Fontes obrigatórias lidas

```text
docs/decisions/ADR-0012-localization-string-table-from-p4.md (decisão canônica; fronteira de retrofit; débito registrado)
docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md (decisão 4.7 — OVERRIDE, VINCULANTE; "Ambiguidades interpretadas" item 4)
docs/game_rules/documentation_rules.md (paths canônicos; onde vivem ADR/game_rules/specs)
docs/game_rules/event_rules.md (comunicação de gameplay só via GameEventBus; DTO de evento só tipos simples)
docs/specs/a_implementar/fable/fable_35_spec_npc_side_quest_chains.md (consumidora — texto novo de side quest P4)
docs/specs/a_implementar/fable/fable_36_spec_main_quest_acts_2_4.md (consumidora — texto novo de quest principal)
docs/specs/a_implementar/fable/fable_70_spec_npc_side_quest_chains_wave2.md (consumidora — segunda onda de side quests)
docs/specs/a_implementar/fable/fable_67_spec_canonical_governance_input_map_adr.md (convenção canônica de id/texto coordenada com F67)
.claude/rules/unity-architecture.md (no global search; event bus; save DTO simples)
.claude/rules/testing-quality-gate.md (testes obrigatórios para lógica determinística — parsers/adapters/serviços puros)
.claude/skills/system-reuse-audit/SKILL.md (Fase 0 — não criar serviço paralelo)
```

## Estado atual do repo

```text
EXISTE e NÃO RECRIAR / NÃO RETROFITAR (confirmado pela auditoria 2026-06-13):

- Ganchos por chave JÁ EXISTEM nos modelos de quest (ESTE é o ponto de injeção do serviço):
  - Assets/_Game/Scripts/Quests/QuestDefinition.cs:9-12 — DisplayNameKey, HiddenDisplayNameKey,
    DescriptionKey (campos string já nomeados "*Key");
  - Assets/_Game/Scripts/Quests/QuestStepDefinition.cs:17 (QuestObjective.HintTextKey),
    :30-32 (QuestStepDefinition.DisplayNameKey, DescriptionKnownKey, DescriptionHiddenKey).
  - Resolução AD-HOC já em uso: Assets/_Game/Scripts/Quests/Log/QuestLogProjectionService.cs:39
    (def.DisplayNameKey ?? def.QuestId), :40 (HiddenDisplayNameKey ?? "???"),
    :47/:82 (DescriptionKey ?? ""), :81 (DisplayNameKey ?? def.QuestId).
    => "a chave hoje É o texto, com fallback ao id". Esta spec FORMALIZA esse fallback num
       serviço testado; NÃO altera a assinatura dos campos *Key.

- Texto de DIÁLOGO hardcoded inline (PRÉ-P4 — débito ADR-0012, NÃO migrar nesta spec):
  - Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs (linhas ~37-611: ~26 NPCs com Greetings/
    Role/Town/Service/Advice/Rumor/Goodbye em PT-BR literal embutido);
  - Assets/_Game/Scripts/NPC/DialogueNode.cs:11 (campo string Text — texto literal por nó);
  - Assets/_Game/Scripts/NPC/DialogueTreeSO.cs:12 (List<DialogueNode> Nodes — Text por nó nos assets).
  Esse texto permanece inline. A disciplina id→string vale apenas para texto NOVO a partir da P4.

- Padrão de tabela/catálogo estático a ESPELHAR (não criar um padrão novo):
  - Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs (catálogo estático, init único, AllContent
    + TryGetContent(id, out content)) e Assets/_Game/Scripts/NPC/NpcDialogueSetRegistry.cs
    (static, lazy-init de s_entries, TryGet(id, out entry)) — copiar o ESTILO (estático,
    lazy, TryGet), não o conteúdo.
  - Assets/_Game/Scripts/Core/Data/IIdentifiedData.cs (interface { string Id { get; } }) — usar
    se a entrada da tabela precisar de id estável.

- GameEventBus (Assets/_Game/Scripts/Core/GameEventBus.cs): canal canônico de comunicação de
  gameplay. Lookup de texto NÃO é gameplay communication (é leitura síncrona de dado), então NÃO
  precisa de evento; mas nenhum canal paralelo de comunicação é criado.

NÃO EXISTE (confirmado ausente — escopo desta spec):
- LocalizationService / serviço de resolução de texto id→string (NENHUM arquivo em
  Assets/_Game/Scripts/Localization/**; o termo "Localization" não aparece como serviço em runtime);
- tabela id→string formal (a "tabela" hoje é o acoplamento implícito chave==texto em quest);
- convenção declarada de nomenclatura/declaração de chaves de texto para specs da P4.

AUDITAR na Fase 0 (antes de criar qualquer arquivo):
- system-reuse-audit: confirmar que NÃO existe nenhum LocalizationService/TextService/StringTable
  em runtime nem em Editor que já faça lookup id→string (grep por "Localization", "StringTable",
  "TextTable", "I18n", "L10n"); se existir algo equivalente, REUSAR e marcar a divergência.
- convenção de id de texto coordenada com F67 (fable_67 — governança de input map / convenção de id): confirmar o formato canônico de id
  (ex.: prefixo de domínio "quest.", "dialogue.", separador, caixa). Se F67 ainda não fixou o
  formato, esta spec PROPÕE um formato mínimo e o marca como "proposta a calibrar" com F67,
  sem bloquear o lookup básico (o lookup é agnóstico ao formato do id).
- onde a tabela-fonte vive: catálogo estático em C# (espelhando TownNpcDialogueLibrary) é o
  caminho de menor risco e sem dependência de asset .asset YAML (que é arquivo proibido). Um
  TextAsset/JSON em Resources é alternativa; decidir na Fase 0 priorizando "sem edição de YAML"
  e "testável em EditMode sem cena".
```

## Engineering stories

```text
Como autor de quest da P4 (F35/F36/F70), quero referenciar texto por um id estável e resolver via
  uma tabela id→string, para que um segundo idioma possa ser adicionado depois sem reescrever
  meu call site.
Como QuestLogProjectionService, quero um fallback determinístico (mostrar o id) quando a chave de
  texto não existe, em vez do "?? QuestId" ad-hoc atual — mesmo comportamento, agora testado.
Como mantenedor, quero NÃO migrar o texto legado pré-P4 (TownNpcDialogueLibrary, DialogueNode.Text):
  ele continua inline como débito registrado (ADR-0012), e nenhuma spec deve re-tocá-lo "de brinde".
Como sistema, quero que o lookup de texto seja uma leitura de dado síncrona e pura (sem evento,
  sem busca global, sem Unity ref), testável em EditMode sem cena.
Como spec futura de texto, quero uma convenção declarada (como nomear e declarar minhas chaves)
  para não inventar um esquema próprio.
```

## Escopo

```text
Inclui:
- LocalizationService (Localization/, NOVO): API leve e pura de resolução de texto:
    string Get(string id);            // retorna a string PT-BR; se faltar/nulo/vazio => retorna o próprio id
    bool TryGet(string id, out string value); // false quando não há entrada (value = id por conveniência)
    bool Has(string id);              // existência de chave
  Idioma único PT-BR (default). SEM troca de locale em runtime. SEM framework. SEM .po/.resx.
  Implementação determinística e idempotente; reentrante; thread-agnóstica (leitura).
- Fonte de tabela id→string (Localization/, NOVO): mapa id-keyed em DADOS, espelhando o estilo de
  TownNpcDialogueLibrary/NpcDialogueSetRegistry (catálogo estático C#, init único, lookup por id).
  Em v1 a tabela pode iniciar VAZIA ou com um punhado de chaves-semente de exemplo da convenção;
  o ponto é o CONTRATO e a disciplina, não popular texto (o texto vem das specs consumidoras).
- Convenção de autoria id→string (DOCUMENTADA no report + na seção desta spec): formato de id
  (proposta a calibrar com F67), como uma spec futura declara suas chaves, e a regra de
  que texto NOVO de quest/diálogo a partir da P4 usa id, não literal no call site.
- Integração MÍNIMA e segura no consumo já-ad-hoc: documentar que os campos *Key de quest passam
  a ser resolvíveis via LocalizationService.Get(key). A troca real do "?? QuestId" por
  "LocalizationService.Get(...)" em QuestLogProjectionService é OPCIONAL nesta spec e, se feita,
  deve preservar EXATAMENTE o comportamento de fallback (id visível) — coberta por teste de
  paridade. Se a Fase 0 julgar arriscado tocar o projection agora, a integração fica DEFERRED
  para a primeira spec consumidora (F35), com o serviço já pronto e testado — documentar honesto.
- EditMode tests (Localization/, NOVO): Get(id existente) => valor; Get(id ausente) => id;
  Get(null)/Get("") => entrada de borda determinística; TryGet false quando ausente; Has;
  idempotência (mesma entrada, mesmo resultado); paridade com o fallback "?? id" legado.
```

## Fora de escopo

```text
- Retrofit do texto hardcoded pré-P4 (TownNpcDialogueLibrary.cs, DialogueNode.Text,
  DialogueTreeSO Nodes[].Text) — ADR-0012 registra como débito; NÃO migrar, NÃO re-tocar.
- Framework de localização; troca de idioma em runtime; UI de seleção de locale; fallback de
  fonte por locale; regras de pluralização/gênero (ADR-0012 "What it explicitly does NOT do").
- Persistência em save: a tabela é dado/recurso, não estado de jogo (sem DTO, sem section, sem migração).
- Texto de UI chrome, logs, chrome de menu, tooltips de sistema — fora a menos que uma spec
  futura explicitamente opte por entrar (ADR-0012 Scope "Out of scope").
- Reescrita dos modelos de quest (QuestDefinition/QuestStepDefinition/QuestObjective): os campos
  *Key permanecem; esta spec apenas os RESOLVE.
- Definição/edição de conteúdo de texto das quests (isso é das specs F35/F36/F70).
```

## Regras de não duplicação

```text
- Um único serviço de resolução de texto. Fase 0 audita (system-reuse-audit) antes de criar;
  se já existir um TextService/StringTable/Localization equivalente, REUSAR e marcar divergência.
- Não criar um segundo padrão de catálogo: espelhar o estilo estático/lazy/TryGet de
  TownNpcDialogueLibrary + NpcDialogueSetRegistry.
- Não duplicar o fallback: o "?? id" passa a ser responsabilidade única do LocalizationService;
  o "?? QuestId" legado é o comportamento de referência (paridade testada), não um segundo fallback.
- Não migrar texto legado para a tabela (ADR-0012). Texto legado permanece inline.
- Não introduzir comunicação de texto via canal paralelo ao GameEventBus; lookup é leitura síncrona.
- Sem GameObject.Find/FindObjectOfType: o serviço é puro/estático ou injetado; nenhuma busca global.
```

## Critérios de aceite

### CA-1 — Lookup id→string PT-BR funciona
- `LocalizationService.Get(id)` para um id presente na tabela retorna a string PT-BR
  correspondente; `TryGet(id, out value)` retorna `true` e a string; `Has(id)` retorna `true`.
- Evidência: EditMode tests `LocalizationServiceTests` (Get/TryGet/Has para id presente).

### CA-2 — Fallback ao id é determinístico e visível
- `Get(id)` para um id AUSENTE retorna **o próprio id** (string não vazia, nunca exceção, nunca
  string vazia surpresa); `TryGet` retorna `false`. Comportamento idêntico ao `?? QuestId`
  legado (paridade), agora único e testado.
- Evidência: EditMode tests (id ausente => id; paridade com o fallback legado documentada).

### CA-3 — Bordas (nulo/vazio) são determinísticas
- `Get(null)` e `Get("")` têm resultado determinístico e documentado (sem `NullReferenceException`):
  retornam string vazia ou o próprio argumento de forma consistente, conforme decidido na Fase 0 e
  fixado no teste.
- Evidência: EditMode tests de borda (null, "").

### CA-4 — Sem framework, sem locale-switch, sem save (conformidade ADR-0012)
- Nenhum framework de terceiros adicionado (sem nova dependência em Packages/); nenhum mecanismo
  de troca de locale em runtime; nenhuma persistência em save (sem DTO/section/migração); o
  serviço é puro/estático sem Unity ref, sem busca global, sem evento de gameplay.
- Evidência: diff só em `Localization/**` + testes; report declara conformidade ADR-0012 item a item.

### CA-5 — Convenção de autoria declarada + texto legado intocado
- A convenção id→string para texto NOVO de quest/diálogo a partir da P4 está documentada
  (formato de id como proposta a calibrar com F67; como specs futuras declaram chaves);
  o texto legado pré-P4 (`TownNpcDialogueLibrary.cs`, `DialogueNode.Text`) permanece inline e
  não é tocado por esta spec.
- Evidência: seção "Convenção" no report; `git diff` mostra ZERO alteração em arquivos de NPC/diálogo legados.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Localization/LocalizationService.cs        (NOVO — API pura: Get/TryGet/Has; fallback ao id)
Assets/_Game/Scripts/Localization/LocalizationStringTable.cs    (NOVO — fonte id→string PT-BR, catálogo estático/lazy, estilo TownNpcDialogueLibrary)
Assets/_Game/Scripts/Localization/LocalizationKey.cs            (NOVO, OPCIONAL — constantes/convenção de id; só se a Fase 0 confirmar valor; senão omitir)
Assets/_Game/Tests/EditMode/Localization/LocalizationServiceTests.cs (NOVO — lookup, fallback, bordas, idempotência, paridade)
docs/validation/fable_73_spec_localization_string_table_runtime_execution_report.md (report individual)
docs/validation/playmode/fable_73_human_test_scenario.md        (cenário humano leve — opcional; ver Testing Quality Gate)

Namespace alvo: CindarsHope.Localization (coerente com CindarsHope.NPC / CindarsHope.Core).
NÃO usar CindarsHope.Debug / CindarsHope.Temp (namespaces proibidos).
```

## Contratos

### Data contracts
- **Entrada da tabela:** par `id` (string estável) → `value` (string PT-BR). Tipos simples apenas.
  Se modelada como tipo, pode implementar `CindarsHope.Core.Data.IIdentifiedData` (`Id => id`).
- **Sem refs Unity** em nenhum ponto (sem ScriptableObject/GameObject/Transform/Sprite). A fonte
  da tabela é dado puro (catálogo estático C#); se a Fase 0 optar por TextAsset/JSON em Resources,
  o carregamento é leitura de dado, não ref persistida.
- **Sem persistência:** a tabela é asset/recurso de dados, NÃO entra em save. Nenhum DTO/section/migração.

### Runtime contracts
```csharp
namespace CindarsHope.Localization
{
    public static class LocalizationService   // pura/estática; sem MonoBehaviour, sem Unity ref
    {
        public static string Get(string id);                       // id presente => valor; ausente/nulo/vazio => fallback determinístico (id)
        public static bool   TryGet(string id, out string value);  // true + valor; false => value = id (conveniência)
        public static bool   Has(string id);                       // existência de chave
        // (Fase 0) método de teste para injetar/limpar a tabela em EditMode sem cena, se necessário.
    }
}
```
- Resolução determinística e idempotente; reentrante; sem efeitos colaterais; sem `Debug`/`Temp`.
- O serviço NÃO faz GameObject.Find/FindObjectOfType. A tabela é estática/lazy (estilo
  `NpcDialogueSetRegistry.Entries`) ou injetada explicitamente.

### Event contracts
- **Nenhum evento novo.** Lookup de texto é leitura síncrona de dado, não comunicação de gameplay
  (event_rules.md aplica-se a comunicação entre sistemas, não a leitura de tabela). Nenhum canal
  paralelo ao GameEventBus é criado; nenhuma assinatura/Publish.

### Save/UI contracts
- **Save:** N/A (tabela = dado; sem DTO, sem section, sem migração; ADR-0012: não é estado).
- **UI:** esta spec NÃO altera nenhuma tela. Os campos `*Key` de quest passam a ser RESOLVÍVEIS
  via `LocalizationService.Get(key)`; a troca do `?? QuestId` em `QuestLogProjectionService` é
  opcional e, se feita, mantém paridade exata (teste).

## Sistemas afetados

```text
Localization (camada nova, isolada). Quest text-authoring (ponto de consumo via campos *Key —
contrato preservado). NPC/diálogo legado: NÃO tocado (débito ADR-0012). GameEventBus: não usado
(lookup é leitura). Save: não tocado. UI: não tocada (integração no projection é opcional/paridade).
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Localization/**            (serviço + tabela + convenção opcional)
Assets/_Game/Tests/EditMode/Localization/**     (testes EditMode)
docs/validation/**                              (report + cenário humano opcional)
csproj includes (Assembly-CSharp / Assembly-CSharp-Editor) — somente os <Compile Include> dos arquivos novos
[OPCIONAL, só se a Fase 0 decidir integrar a paridade agora]
  Assets/_Game/Scripts/Quests/Log/QuestLogProjectionService.cs — APENAS para trocar "?? QuestId/??\"\"" por
  LocalizationService.Get(...), preservando paridade exata (teste). Se em dúvida, NÃO tocar (DEFERRED p/ F35).
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML; sem autorização explícita de spec)
Packages/** ; ProjectSettings/** (nenhuma dependência/framework novo)
Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs   (texto legado pré-P4 — débito ADR-0012)
Assets/_Game/Scripts/NPC/DialogueNode.cs / DialogueTreeSO.cs (texto legado de diálogo — não retrofitar)
Assets/_Game/Scripts/Quests/QuestDefinition.cs / QuestStepDefinition.cs (campos *Key: NÃO alterar assinatura)
Qualquer save DTO / SaveManager / *SectionProvider (tabela não é estado)
*Tests.cs fora de Assets/_Game/Tests/EditMode/** (hook protected-path-guard bloqueia)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar (system-reuse-audit): confirmar ausência de qualquer Localization/TextService/StringTable
  (grep "Localization", "StringTable", "TextTable", "I18n", "L10n" em Scripts e Editor). Confirmar a
  convenção de id de texto com F67 (fable_67) (se ausente, propor formato mínimo e marcar "proposta a
  calibrar"). Decidir a fonte da tabela (catálogo estático C# preferido — sem YAML/asset proibido;
  TextAsset/JSON em Resources só se justificado). Decidir o comportamento de borda de Get(null)/Get("").
  Decidir SE integra a paridade no QuestLogProjectionService agora ou DEFERRED para F35.
### Fase 1 — LocalizationStringTable (fonte id→string PT-BR, estática/lazy, estilo TownNpcDialogueLibrary;
  pode iniciar com chaves-semente da convenção) + LocalizationKey opcional (constantes da convenção).
### Fase 2 — LocalizationService (Get/TryGet/Has com fallback ao id determinístico) + EditMode tests
  (lookup OK, fallback ao id, bordas null/"", TryGet false, Has, idempotência, paridade com "?? id").
### Fase 3 — [OPCIONAL] paridade no QuestLogProjectionService (trocar "?? QuestId"/"?? \"\"" por
  LocalizationService.Get(...)) preservando comportamento, com teste de paridade; senão DEFERRED p/ F35.
### Fase 4 — csproj includes; run_strict_validation (exit 0); execution report (Spec Compliance Matrix,
  conformidade ADR-0012 item a item, convenção declarada, débito legado reafirmado) + cenário humano opcional.
```

## Paralelização

- **Parallelizable:** YES — cria um serviço novo isolado em `Localization/**`; não reescreve
  nenhum sistema existente nem altera contratos públicos (campos `*Key` preservados).
- **Parallel group:** janela LIVRE.
- **Can run with:** qualquer spec que não recrie um serviço de texto nem mexa nos campos `*Key`.
- **Must not run with:** specs que reescrevam `QuestDefinition`/`QuestStepDefinition`/`QuestObjective`
  (campos `*Key`) ou que introduzam um segundo resolver de texto; e (se a Fase 3 opcional for feita)
  specs que toquem `QuestLogProjectionService` simultaneamente.
- **Shared files/systems that require lock:** csproj includes; (condicional) `QuestLogProjectionService.cs`.
- **Reason:** serviço novo e autocontido; a única superfície compartilhada é o csproj e, se a
  integração de paridade for feita, o projection service.

## Impacto em save/load

```text
Does this change save schema? NO.
Does this add a save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO (a tabela é dado/recurso, não estado; ADR-0012).
```

## Impacto em eventos

```text
Adds events: NO (lookup é leitura síncrona de dado; não é comunicação de gameplay).
Changes existing events: NO.
Requires unsubscribe pattern: N/A (serviço puro/estático, sem assinatura).
```

## Impacto em UI/Unity

```text
Changes UI: NO (telas inalteradas; campos *Key apenas tornam-se resolvíveis via serviço).
Changes scenes: NO. Changes prefabs: NO. Changes ScriptableObjects/assets: NO (catálogo estático C# preferido).
Requires Play Mode final validation: NO obrigatório — lógica é determinística e coberta por EditMode.
  Cenário humano leve é OPCIONAL (verificar que o quest log exibe texto/ id de fallback corretamente
  se a Fase 3 opcional for feita). Human validation timing: DEFERRED_TO_FINAL_VALIDATION (se aplicável).
```

## Riscos técnicos

```text
Risco: criar um segundo sistema de texto sem ver um existente.
  Mitigação: Fase 0 system-reuse-audit (grep Localization/StringTable/TextTable/I18n/L10n) antes de criar.
Risco: por engano migrar/retrofitar o texto legado (TownNpcDialogueLibrary, DialogueNode.Text).
  Mitigação: ADR-0012 fixa que NÃO se migra; anti-regressão e arquivos proibidos bloqueiam; diff = 0 nesses arquivos.
Risco: fallback inconsistente (chave ausente => string vazia/exceção em um lugar e id em outro).
  Mitigação: fallback ÚNICO no LocalizationService (sempre retorna id quando falta); teste de paridade
  com o "?? QuestId" legado; bordas null/"" testadas.
Risco: introduzir framework/dependência (viola ADR-0012 "no framework").
  Mitigação: implementação 100% C# puro; nenhuma alteração em Packages/; CA-4 verifica.
Risco: formato de id divergir do que F67 vai canonizar.
  Mitigação: lookup é agnóstico ao formato do id; a convenção de formato é "proposta a calibrar" com F67,
  não bloqueante; nenhuma chave de produção é populada por esta spec (texto vem das consumidoras).
Risco: tabela como .asset YAML viraria arquivo proibido / não testável sem cena.
  Mitigação: fonte preferida é catálogo estático C# (estilo TownNpcDialogueLibrary), testável em EditMode.
```

## Rollback

```text
Remover Assets/_Game/Scripts/Localization/** e os testes desliga a camada de localização. Os
campos *Key de quest continuam funcionando exatamente como hoje (resolução ad-hoc "?? QuestId" no
QuestLogProjectionService permanece, pois a Fase 3 de paridade é opcional/reversível). As specs
consumidoras (F35/F36/F70) voltam a depender desta infra (ficam bloqueadas até reintroduzi-la).
Nenhum save afetado; nenhum texto legado tocado; nenhum evento removido.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: system-reuse-audit (sem Localization/TextService/StringTable existente); fixar
       convenção de id (proposta a calibrar c/ F67); escolher fonte (catálogo estático C#);
       definir borda Get(null)/Get(""); decidir paridade-agora vs DEFERRED p/ F35. Registrar no report.
- [ ] T002 — LocalizationStringTable (fonte id→string PT-BR, estática/lazy, estilo TownNpcDialogueLibrary/
       NpcDialogueSetRegistry; chaves-semente da convenção, sem popular texto de produção).
- [ ] T003 — LocalizationService (Get/TryGet/Has; fallback determinístico ao id; sem Unity ref, sem
       busca global, sem evento) + (opcional) LocalizationKey de constantes da convenção.
- [ ] T004 — EditMode tests LocalizationServiceTests: Get(id presente)=>valor; Get(ausente)=>id;
       Get(null)/Get("")=>borda determinística; TryGet false; Has; idempotência; paridade com "?? id".
- [ ] T005 — [OPCIONAL/condicional à Fase 0] paridade no QuestLogProjectionService (Get(...) no lugar
       de "?? QuestId"/"?? \"\"") com teste de paridade; senão DEFERRED p/ F35 (documentar honesto).
- [ ] T006 — csproj includes (Assembly-CSharp + Editor p/ os testes); run_strict_validation (exit 0);
       execution report (Spec Compliance Matrix, conformidade ADR-0012 item a item, convenção declarada,
       débito legado reafirmado) + cenário humano opcional.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp FAIL"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "Assembly-CSharp-Editor FAIL"; exit 1 }
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { Write-Host "Strict validation FAIL"; exit 1 }
```

## Testing Quality Gate

```text
Changed runtime code: YES (novo serviço puro de resolução de texto).
Changed deterministic logic: YES (lookup + fallback ao id — lógica determinística, EditMode obrigatório
  pela testing-quality-gate: "parsers, adapters, pure services, validators and rule engines").
Changed Unity scene/prefab/asset wiring: NO (fonte é catálogo estático C#; nenhuma cena/prefab/asset).
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Localization/LocalizationServiceTests.cs).
Automated tests command: dotnet build .\Assembly-CSharp-Editor.csproj --no-restore + (Unity EditMode runner quando disponível).
Manual Play Mode scenario: NOT REQUIRED (lógica determinística coberta por EditMode). Se a Fase 3
  opcional integrar o projection, cenário humano leve em docs/validation/playmode/fable_73_human_test_scenario.md
  (quest log mostra texto resolvido OU id de fallback).
Justification if no automated tests: N/A (testes são obrigatórios e estão no escopo).
Residual risk: formato de id é "proposta a calibrar" com F67 — não bloqueia o lookup
  (agnóstico ao formato); integração de paridade no projection pode ficar DEFERRED p/ F35 sem perda
  de comportamento (fallback legado preservado).
```

## Definition of Done

```text
LocalizationService leve (Get/TryGet/Has) com fallback determinístico ao id; tabela id→string PT-BR
estática/lazy (estilo TownNpcDialogueLibrary); convenção de autoria id→string declarada (formato a
calibrar com F67); EditMode tests (lookup, fallback, bordas, idempotência, paridade); SEM framework,
SEM locale-switch, SEM save, SEM busca global, SEM evento novo, SEM tocar texto legado pré-P4;
builds Assembly-CSharp e Assembly-CSharp-Editor 0E; run_strict_validation exit 0; execution report
com Spec Compliance Matrix + conformidade ADR-0012 item a item + débito legado reafirmado; sem claim ACCEPTED.
```

## Anti-regressão

```text
- NÃO recriar/duplicar serviço de texto: um único LocalizationService (Fase 0 audita).
- NÃO migrar nem re-tocar o texto legado pré-P4: TownNpcDialogueLibrary.cs, DialogueNode.Text,
  DialogueTreeSO Nodes[].Text permanecem inline (ADR-0012 — débito registrado). git diff = 0 nesses arquivos.
- NÃO alterar a assinatura dos campos *Key de QuestDefinition/QuestStepDefinition/QuestObjective.
- Fallback ÚNICO no serviço (sempre retorna o id quando a chave falta) — paridade testada com o
  "?? QuestId"/"?? \"\"" legado de QuestLogProjectionService; nenhum segundo fallback divergente.
- Nenhuma dependência/framework novo (Packages/ intocado); nenhum mecanismo de locale-switch.
- Sem GameObject.Find/FindObjectOfType; sem comunicação de gameplay fora do GameEventBus; sem
  namespaces proibidos (CindarsHope.Debug/Temp).
- Nenhum save DTO/section/migração (a tabela é dado, não estado).
```
