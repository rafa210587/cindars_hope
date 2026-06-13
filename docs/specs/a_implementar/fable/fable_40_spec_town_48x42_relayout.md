# SPEC — Cena: TownScene 48×42 Relayout Canônico (distritos do HUD_LAYOUT)

> **Spec ID:** `fable_40_spec_town_48x42_relayout`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P2
> **Type:** Editor / Scene
> **Domain:** City / Scene
> **Parallelizable:** NO (gerador de cena lock)
> **Parallel group:** N/A (lock dos geradores de cena)
> **Can run with:** N/A
> **Must not run with:** F11, F09, F41 (geradores de cena — ordem F11 → F40 → F41)
> **Repo lock scope:** `Editor/SceneCreation/CreateMvpTownScene.cs`, TownScene gerada
> **Depends on:**
> - F11 (interiores/portas se executada antes — auditar), F19 (Fonte física)
> **Blocks:** N/A
> **Scope:** regenerar a cidade no tamanho canônico 48×42 com os distritos do layout doc.
> **Out of scope:** arte/tiles reais, NPCs novos, interiores novos além dos existentes.

required_adrs: []
required_game_rules: [city_rules.md]

---

# /speckit.specify

## Contexto

Decisão Q12.1: a Town passa ao tamanho canônico de 48×42 tiles. HUD_LAYOUT_SCENES §3
desenha o mapa de distritos da cidade: praça central (com a estátua do guerreiro),
mercado (oeste), residencial (leste), templo/Fonte (norte), curral/entrada sul (conexão
com a fazenda), prefeitura+mural (nordeste) e lago/parque (sudoeste) — com anel viário
interno e distritos com respiro.

A cidade atual é gerada pelo CreateMvpTownScene com bounds ±16×±12 (32×24) — menor que o
canônico e sem os distritos novos (lago/parque e prefeitura distinta não existem).
Objetivo: atualizar o GERADOR (nunca YAML manual — regra unity-yaml-editing-policy) para
o footprint canônico, reposicionando TODOS os elementos existentes pelos distritos sem
perder nenhum (23 NPCs, casas, lojas, board, estátua, árvores) e mantendo os IDs dos
pontos nomeados de schedule — apenas as posições mudam.

## Problema

Sem o relayout, a cidade fica fora do canônico (Q12.1) e sem espaço para os elementos
que outras specs precisam: a prefeitura (ponto de venda das escrituras de lote — F41), o
mural na parede da prefeitura (F34) e o lago/parque. Se o relayout for feito editando a
cena YAML manualmente, viola a política de edição de YAML e perde reprodutibilidade. Se
os IDs dos pontos nomeados de schedule mudarem, os 23 NPCs quebram suas rotinas
silenciosamente.

## Objetivo

Ao final desta spec, o projeto deve ter a TownScene regenerada em 48×42 (bounds
−24..24 / −21..21) com os 7 distritos do HUD_LAYOUT §3, contagem de elementos preservada
(antes=depois no log do gerador), IDs de schedule e spawn points intactos, lago/parque e
prefeitura+mural novos, e colliders no novo perímetro — tudo via gerador com evidência
de geração.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md (§3 — mapa de distritos)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§12)
docs/design/gameplay/npcs_city/CITY_NPC_ROSTER_DIRECTION (locais de trabalho/casa)
.claude/rules/unity-yaml-editing-policy.md (gerador, nunca YAML manual)
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CreateMvpTownScene (gerador atual: distritos ±16×±12, praça/estátua/12 casas/mercados/
  wander radius por NPC) — ATUALIZAR, nunca criar segundo gerador;
- NpcWanderer bounds (recalcular pelo distrito de trabalho);
- schedules com pontos nomeados (IDs consumidos pelos NPCs — preservar);
- board/quadro de avisos, estátua, árvores, casas, lojas (reposicionar, não recriar);
- validador de cena existente (critério de PASS pós-geração).
Não existe:
- footprint 48×42 (bounds −24..24 / −21..21);
- distritos canônicos data-driven;
- lago/parque (SW) e prefeitura distinta (NE) com mural.
Auditar Fase 0:
- todos os pontos nomeados consumidos por schedules (não quebrar IDs);
- todos os spawn points por ID (entrada da fazenda S etc.);
- estado da F11 (interiores/portas) — se executada antes, preservar offsets de interior.
```

## Engineering stories

```text
Como gerador de cena, quero o mapa de distritos data-driven (retângulos nomeados), para
  posicionar e validar elementos por distrito em vez de coordenadas soltas.
Como NPC com schedule, quero meus pontos nomeados com os MESMOS IDs, para minha rotina
  continuar resolvendo após o relayout.
Como spec F41, quero a prefeitura existindo no NE, para vender as escrituras de lote.
Como auditor, quero contagem de elementos antes=depois no log do gerador, para provar
  que nada se perdeu no relayout.
```

## Escopo

```text
Inclui:
- constantes do gerador: bounds (−24..24, −21..21); mapa de distritos data-driven
  (retângulos nomeados conforme §3);
- reposicionamento: cada elemento existente movido para seu distrito (tabela
  elemento→distrito no gerador); pontos nomeados de schedule MANTÊM IDs (posições mudam);
- novos: lago/parque SW (água + bancos placeholder), prefeitura NE (prédio + mural F34
  na parede), caminhos de terra ligando distritos (visual placeholder);
- wander bounds por NPC recalculados pelo distrito de trabalho;
- colliders de borda no novo perímetro; spawn points (entrada da fazenda S) preservados
  por ID;
- validação: gerador roda → validador de cena existente PASS; CA: nenhum NPC/interactable
  perdido (contagem antes=depois no log do gerador);
- EditMode tests: mapa de distritos (pontos dentro dos bounds), tabela completa
  (todo elemento tem distrito), IDs de schedule inalterados.
```

## Fora de escopo

```text
Não inclui:
- arte/tiles reais (placeholder visual);
- NPCs novos;
- interiores novos além dos existentes (F11 é a dona dos interiores);
- lotes da fazenda (F41 — roda DEPOIS desta);
- ajuste de velocidade de NPC (se as distâncias maiores deixarem rotinas lentas,
  ANOTAR no report — não tocar).
```

## Regras de não duplicação

```text
Não criar segundo gerador — atualizar o CreateMvpTownScene existente.
Não editar a TownScene YAML manualmente — geração via gerador/AssetDatabase
  (evidência de geração obrigatória).
Não recriar NPCs/schedules — reposicionar pontos mantendo IDs.
Não recriar o board/mural — mover para a parede da prefeitura (mural F34) e praça.
Não duplicar o validador de cena — usar o existente como critério de PASS.
```

## Critérios de aceite

### CA-1 — Footprint e contagem preservada

- Cena regenerada em 48×42 (bounds −24..24 / −21..21) com os 7 distritos do §3 e
  contagem de elementos preservada (23 NPCs, casas, lojas, board, estátua, árvores).
- Evidência: log do gerador com contagem antes=depois + EditMode tests das tabelas.

### CA-2 — Schedules intactos

- Todos os pontos nomeados de schedule resolvem (mesmos IDs, posições novas) e o
  validador de cena existente passa.
- Evidência: EditMode test de IDs inalterados + validador de cena PASS.

### CA-3 — Distritos novos

- Lago/parque (SW) e prefeitura (NE) com mural na parede existem; o board permanece na
  praça.
- Evidência: log do gerador + inspeção no cenário humano de navegação.

### CA-4 — Perímetro e spawns

- Colliders de borda cobrem o novo perímetro; spawn points (entrada da fazenda S)
  preservados por ID.
- Evidência: EditMode test de spawn IDs + cenário humano (bordas intransponíveis).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateMvpTownScene.cs             (ATUALIZADO — bounds 48×42 + mapa de distritos
                                     data-driven + tabela elemento→distrito)
Assets/_Game/Tests/EditMode/City/
  TownLayoutTests.cs                (NOVO — tabelas puras: distritos/elementos/IDs)
docs/validation/
  fable_40_spec_town_48x42_relayout_execution_report.md
```

## Contratos

### Data contracts
Mapa de distritos data-driven: retângulos nomeados conforme §3 (praça central, mercado W,
residencial E, templo/Fonte N, curral/entrada S, prefeitura+mural NE, lago/parque SW)
dentro dos bounds −24..24 / −21..21; tabela elemento→distrito cobrindo TODO elemento
existente (nenhum órfão).

### Runtime contracts
N/A novo em runtime — o relayout é de geração. NpcWanderer recebe bounds recalculados
pelo distrito de trabalho (mesmo contrato). Pontos nomeados de schedule mantêm IDs.

### Event contracts
N/A — nenhum evento novo ou alterado.

### Save contracts
N/A — nenhuma mudança de save. Spawn points preservados por ID garantem compatibilidade
de transição de cena com saves existentes.

### UI contracts
N/A — nenhuma UI nova (o mural na prefeitura é o interactable F34 reposicionado).

## Sistemas afetados

```text
Gerador da cidade (CreateMvpTownScene — bounds/distritos/tabela)
TownScene gerada (regeneração com evidência)
NPC schedules/wander (pontos nomeados reposicionados, IDs intactos)
Spawn points/transições de cena (preservados por ID)
Validador de cena existente (critério de PASS)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (atualizar)
Assets/_Game/Tests/EditMode/City/** ; docs/validation/** ; csproj includes
TownScene regenerada APENAS via gerador (evidência obrigatória)
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset manual (geração SOMENTE via gerador — NUNCA YAML manual)
Packages/**
ProjectSettings/**
NpcDataSO/schedules data (IDs de pontos nomeados — reposicionar via gerador, não editar)
CreateMvpFarmScene (F41 é a dona — roda depois)
interiores F11 (não tocar além de preservar offsets se já existirem)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Inventário completo: pontos nomeados consumidos por schedules, spawn points por ID,
contagem de elementos atual (NPCs/casas/lojas/board/estátua/árvores), estado da F11.
### Fase 1 — Tabelas data-driven
Bounds (−24..24, −21..21) + mapa de distritos (retângulos nomeados §3) + tabela
elemento→distrito completa + TownLayoutTests (bounds, cobertura, IDs).
### Fase 2 — Relayout no gerador
Reposicionamento por distrito; novos elementos (lago/parque SW, prefeitura NE + mural,
caminhos de terra); wander bounds recalculados; colliders de borda; spawns por ID.
### Fase 3 — Regeneração e evidência
Rodar o gerador (menu/executeMethod), validador de cena PASS, log com contagem
antes=depois — evidência completa (regra generated-asset-evidence).
### Fase 4 — Closeout
csproj; run_strict_validation; execution report com evidência de geração.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with: N/A
- Must not run with: F11, F09, F41 (geradores de cena — ordem obrigatória F11 → F40 → F41)
- Shared files/systems that require lock: `Editor/SceneCreation/CreateMvpTownScene.cs`,
  TownScene gerada
- Reason: edita o gerador e regenera a cena compartilhada; F41 depende do padrão de
  distritos data-driven e da prefeitura criados aqui.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
Spawn points por ID preservados = saves existentes transicionam de cena sem erro.
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
Changes scenes: YES — TownScene regenerada VIA GERADOR (evidência obrigatória)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (posições via gerador)
Requires Play Mode final validation: YES (lote — navegação pelos distritos)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: quebrar pontos nomeados de schedule (NPCs com rotina morta).
Mitigação: IDs preservados (só posições mudam) + EditMode test de IDs + validador.
Risco: perder elementos no relayout (NPC/interactable sumido).
Mitigação: tabela elemento→distrito completa + contagem antes=depois no log (CA-1).
Risco: distâncias maiores deixarem rotinas de NPC lentas.
Mitigação: não tocar velocidade — ANOTAR no report se ficar lento (fora de escopo).
Risco: conflito com interiores da F11 (offsets y>+40).
Mitigação: auditoria Fase 0 detecta o estado da F11 e preserva offsets de interior.
Risco: edição YAML manual por atalho.
Mitigação: proibição explícita — geração somente via gerador com evidência.
```

## Rollback

```text
git revert do gerador + regenerar a cena = cidade anterior restaurada.
Nenhuma mudança de save/eventos para desfazer.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar pontos nomeados/spawns consumidos + contagem de elementos + F11.
- [ ] T002 — Bounds (−24..24, −21..21) + mapa de distritos data-driven + testes.
- [ ] T003 — Tabela elemento→distrito + reposicionamento + novos (lago/prefeitura+mural/
        caminhos) + wander bounds + colliders de borda.
- [ ] T004 — Regenerar + validadores + evidência (log antes=depois); csproj;
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

- Changed deterministic logic: YES (tabelas de distritos/elementos)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (navegação pelos distritos)
- Requires regression test: YES (schedules/spawns)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + log do gerador (contagens) +
  cenário humano andando pelos distritos

## Definition of Done

```text
Cidade canônica 48×42 com 7 distritos; zero elementos perdidos (contagem antes=depois);
schedules/spawns intactos (IDs preservados); lago/parque + prefeitura/mural existem;
colliders no novo perímetro; geração via gerador com evidência completa.
Nenhum arquivo proibido alterado; Builds 0E; run_strict_validation exit 0; report.
```

## Anti-regressão

```text
IDs de pontos nomeados de schedule e spawn points INALTERADOS.
Contagem de NPCs/interactables preservada (23 NPCs etc. — antes=depois).
Nenhuma edição manual de YAML (.unity/.prefab/.asset).
Interiores F11 (se existirem) preservados com seus offsets.
Validador de cena existente PASS após regeneração.
```
