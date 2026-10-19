# SPEC — Player: Classe Inferida por Investimento (títulos + micro-bônus)

> **Spec ID:** `fable_39_spec_inferred_player_class_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 7
> **Priority:** P3
> **Type:** Runtime
> **Domain:** Player / Skills
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A (leitura da skill tree — rodar DEPOIS de F29)
> **Can run with:** specs que não toquem SkillTreeManager nem DerivedStats provider
> **Must not run with:** F29 (lê skill tree — rodar DEPOIS)
> **Repo lock scope:** leitura do SkillTreeManager, DerivedStats provider (1 entrada)
> **Depends on:**
> - F29 (catálogo canônico — investimento por árvore confiável)
> **Blocks:** N/A
> **Scope:** título dinâmico por dominância de árvore + micro-bônus de identidade.
> **Out of scope:** classes rígidas, respec de classe (deriva sozinho), UI além da ficha.

required_adrs: []
required_game_rules: [player_rules.md]

---

# /speckit.specify

## Contexto

Decisão Q9.1 aprovou o modelo de classe inferida: o jogador NÃO escolhe classe — a
"classe" é INFERIDA do investimento nas 5 árvores de skill, com um título exibido e um
micro-bônus temático de identidade (nunca poder real). O doc de decisões fixa o pacote:
título dinâmico + bônus pequeno + reconhecimento em diálogo. Com o catálogo canônico de
skills (F29) entregue, os pontos gastos por árvore no SkillTreeManager são confiáveis
como fonte da inferência.

Objetivo: um serviço de LEITURA PURA — função estática que recebe os pontos por árvore e
devolve {título, árvore dominante, secundária?, bônus} — recomputado a cada compra/respec
de skill. Zero estado novo: nada é salvo, tudo deriva do que o SkillTreeManager já
persiste.

## Problema

Sem a inferência, a decisão Q9.1 fica sem materialização: investir 20 pontos em uma
árvore não muda nada na identidade do personagem — sem título na ficha, sem saudação de
NPC, sem bônus temático. Se for implementada com estado salvo próprio (classe gravada),
cria-se duplicação do que o SkillTreeManager já persiste e abre-se espaço para
dessincronização após respec.

## Objetivo

Ao final desta spec, o projeto deve ter PlayerClassInference (função pura) que deriva
título e micro-bônus (≤5%) da distribuição de pontos por árvore, com recompute por evento
de compra/respec, exibição na ficha (F14) e condição de diálogo (F28) — sem nenhum estado
salvo novo e sem gate de conteúdo por classe.

## Fontes obrigatórias lidas

```text
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§9 — modelo aprovado)
docs/design/gameplay/player_character/PLAYER_SKILL_TREES_DIRECTION.md (árvores)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- SkillTreeManager (pontos gastos por árvore — F29, fonte única do investimento);
- DerivedStats provider (F02 — ponto de entrada para o micro-bônus);
- ficha do player (tela equipment F14 — onde o título aparece);
- eventos de skill (SkillPurchasedEvent/respec — gatilhos de recompute).
Não existe:
- inferência de classe, tabela de títulos, micro-bônus de identidade.
Auditar Fase 0:
- nomes/IDs reais das 5 árvores no SkillTreeManager (F29) — mapear os rótulos da tabela
  de títulos aos IDs reais;
- assinatura real dos eventos de compra/respec e do DerivedStats provider.
```

## Engineering stories

```text
Como jogador, quero que meu investimento em skills gere um título visível ("Lâmina de
  Cindar"), para sentir identidade sem escolher classe rígida.
Como sistema de stats, quero o micro-bônus como 1 entrada no DerivedStats provider,
  para nunca somar bônus em pontos espalhados.
Como sistema de save, quero zero estado novo (tudo derivado dos pontos por árvore),
  para respec nunca dessincronizar a classe.
Como NPC (F28), quero uma condição de título, para saudações ocasionais usarem a classe.
```

## Escopo

```text
Inclui:
- PlayerClassInference (puro, estático): GetProfile(pontosPorÁrvore) →
  {título, árvoreDominante, secundária?, bônus};
- regras: dominante = árvore com mais pontos (≥6 e ≥40% do total); híbrido se 2ª ≥70% da
  1ª; sem dominante = "Colono" (sem bônus);
- títulos: Guerreiro→"Lâmina de Cindar", Caçador→"Olho da Mata", Místico→"Tecelão",
  Lavrador→"Mão da Terra", Vínculo→"Voz do Vale"; híbridos compostos ("Lâmina Tecelã" —
  tabela 5×4 do doc de decisões);
- micro-bônus (NUNCA acima de 5% — identidade, não poder): Guerreiro +3% postura causada,
  Caçador +3% crit chance, Místico −3% custo de mana, Lavrador +5% stamina fora da caverna,
  Vínculo +3% amizade ganha; híbrido = metade de cada;
- bônus entra como 1 entrada no DerivedStats provider (recompute em SkillPurchasedEvent);
- exibição: título na ficha (F14) + saudação ocasional de NPC usa título (F28 condição);
- EditMode tests: dominância/híbrido/empate/colono, magnitudes, recompute por evento.
```

## Fora de escopo

```text
Não inclui:
- classes rígidas ou escolha de classe;
- respec de classe (a classe deriva sozinha do investimento);
- UI além da linha na ficha (F14);
- gate de conteúdo por classe (decisão: identidade leve);
- estado salvo de classe (derivado — proibido persistir).
```

## Regras de não duplicação

```text
Não criar segundo registro de pontos — SkillTreeManager é a fonte única do investimento.
Não criar estado salvo de classe — tudo deriva dos pontos por árvore já persistidos.
Não somar bônus em pontos espalhados — 1 entrada única no DerivedStats provider (F02).
Não criar sistema novo de saudação — condição aditiva no diálogo condicional (F28).
Não criar tela nova — linha aditiva na ficha existente (F14).
```

## Critérios de aceite

### CA-1 — Dominância simples

- Com 10 pontos em Guerreiro e 2 em Caçador, o perfil é "Lâmina de Cindar" com +3% de
  postura causada (dominante: ≥6 pontos e ≥40% do total; 2ª <70% da 1ª).
- Evidência: EditMode test do caso exato.

### CA-2 — Híbrido

- Com 8/6 (2ª ≥70% da 1ª), o perfil é híbrido com título composto da tabela 5×4 e METADE
  de cada micro-bônus.
- Evidência: EditMode test de híbrido (título composto + magnitudes pela metade).

### CA-3 — Colono

- Com 2/2/2 (nenhuma árvore com ≥6 pontos e ≥40%), o perfil é "Colono" sem bônus.
- Evidência: EditMode test de ausência de dominante (incluindo empates).

### CA-4 — Recompute por evento

- Compra de skill e respec recomputam o perfil imediatamente (evento), refletindo na
  entrada do DerivedStats provider e no título da ficha.
- Evidência: EditMode test de recompute disparado por SkillPurchasedEvent/respec.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Player/
  PlayerClassInference.cs           (NOVO — função pura + tabela de títulos/bônus)
Assets/_Game/Scripts/Player/ (entrada no DerivedStats provider + recompute por evento)
Assets/_Game/Scripts/UI/ (linha do título na ficha F14 — aditivo)
Assets/_Game/Tests/EditMode/Player/
  ClassInferenceTests.cs            (NOVO)
docs/validation/
  fable_39_spec_inferred_player_class_runtime_execution_report.md
```

## Contratos

### Data contracts
Tabela de títulos por árvore (5 puros + tabela 5×4 de híbridos compostos do doc de
decisões) e tabela de micro-bônus (magnitudes fixas ≤5%; híbrido = metade de cada) —
constantes em PlayerClassInference, sem asset novo.

### Runtime contracts
PlayerClassInference.GetProfile(pontosPorÁrvore) é puro e determinístico:
dominante = árvore com mais pontos se (pontos ≥6 E ≥40% do total); híbrido se a 2ª
colocada ≥70% da 1ª; caso contrário "Colono" sem bônus. O resultado alimenta 1 entrada
no DerivedStats provider (F02); recompute assinado em SkillPurchasedEvent/respec.
Nenhum estado próprio: o perfil é função dos pontos atuais.

### Event contracts
Nenhum evento novo. Consome SkillPurchasedEvent e evento de respec existentes (auditar
nomes reais na Fase 0) para recompute.

### Save contracts
N/A — zero estado salvo. O perfil deriva dos pontos por árvore que o SkillTreeManager
já persiste; após load, o primeiro recompute restaura o perfil idêntico.

### UI contracts
Linha de título na ficha do player (tela equipment F14 — campo aditivo); saudação
ocasional de NPC via condição de título no diálogo condicional (F28).

## Sistemas afetados

```text
Skills (leitura do SkillTreeManager — sem alteração de contrato)
DerivedStats provider (F02 — 1 entrada nova)
Ficha do player (F14 — linha aditiva)
Diálogo condicional (F28 — condição de título)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Player/** (novo: PlayerClassInference + provider entry aditiva)
Assets/_Game/Scripts/UI/** (linha do título na ficha — aditivo)
Assets/_Game/Tests/EditMode/Player/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual
Packages/**
ProjectSettings/**
SkillTreeManager core (ler, não alterar contrato)
SaveManager / seções de save (zero estado novo)
catálogo de skills F29 (consumir IDs, não alterar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
IDs reais das 5 árvores no SkillTreeManager (mapear rótulos da tabela de títulos);
assinatura dos eventos de compra/respec; ponto de entrada do DerivedStats provider;
ponto da linha na ficha (F14).
### Fase 1 — Inferência pura
PlayerClassInference (regras de dominância/híbrido/colono + tabela de títulos 5 + 5×4 +
magnitudes) + ClassInferenceTests (dominância, híbrido, empate, colono, magnitudes).
### Fase 2 — Provider e recompute
1 entrada no DerivedStats provider + recompute em SkillPurchasedEvent/respec + teste de
recompute.
### Fase 3 — Exibição e closeout
Linha do título na ficha (F14) + condição de título no F28; csproj;
run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: N/A
- Can run with: specs que não toquem SkillTreeManager nem DerivedStats provider
- Must not run with: F29 (lê skill tree — rodar DEPOIS de F29 fechada)
- Shared files/systems that require lock: leitura do SkillTreeManager, DerivedStats
  provider (1 entrada)
- Reason: a inferência depende do catálogo canônico (F29) para os pontos por árvore
  serem confiáveis; a entrada no provider compartilha arquivo com specs de stats.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Perfil 100% derivado: após load, recompute reproduz título/bônus sem estado próprio.
```

## Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (consumidor de SkillPurchasedEvent/respec)
```

## Impacto em UI/Unity

```text
Changes UI: linha do título na ficha (F14 — aditivo)
Changes scenes: NO | Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (lote)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: nomes de árvore do doc não baterem com os IDs reais do SkillTreeManager.
Mitigação: auditoria Fase 0 mapeia rótulo→ID real; tabela usa IDs auditados.
Risco: bônus virar poder real (acima da régua de identidade).
Mitigação: magnitudes fixas ≤5% na tabela + teste de magnitude por caso.
Risco: dessincronização após respec.
Mitigação: zero estado próprio + recompute por evento + teste CA-4.
Risco: dupla contagem do bônus no DerivedStats.
Mitigação: 1 entrada única nomeada no provider (substituída a cada recompute).
```

## Rollback

```text
Remover a entrada do DerivedStats provider = stats atuais intactos.
Linha da ficha e condição F28 aditivas removíveis.
Nenhum estado salvo para limpar (derivado).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar IDs das árvores/eventos/provider; inferência pura + tabela de
        títulos (5 + híbridos 5×4) + magnitudes + testes.
- [ ] T002 — Provider entry (1 entrada) + recompute por SkillPurchasedEvent/respec +
        teste de recompute.
- [ ] T003 — Linha do título na ficha (F14) + condição de título (F28); csproj;
        run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (inferência pura)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (sem skill investida = sem mudança de stats/título)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + título visível na ficha

## Definition of Done

```text
Título + micro-bônus derivados do investimento (dominante ≥6 pts e ≥40%; híbrido 2ª
≥70% da 1ª; Colono sem dominante); bônus ≤5% como 1 entrada no provider; recompute por
evento; zero estado novo; título na ficha e condição F28.
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
SkillTreeManager sem mudança de contrato (leitura pura).
DerivedStats existentes inalterados (apenas 1 entrada nova, removível).
Jogador sem pontos investidos = comportamento idêntico ao atual (Colono, sem bônus).
Nenhum estado salvo novo; save schema intacto.
Nenhum GameObject.Find em runtime; comunicação via GameEventBus.
```
