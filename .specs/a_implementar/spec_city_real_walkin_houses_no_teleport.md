# SPEC — Cidade Real: Casas Percorríveis, Portas Funcionais, Marcos Cívicos e Sem Teleporte

> **Spec ID:** `spec_city_real_walkin_houses_no_teleport`
> **Status:** A implementar
> **Wave:** WAVE CITY — Coerência da TownScene (pós fable_40)
> **Priority:** P1
> **Type:** Runtime + Tooling (editor scene generator)
> **Domain:** City
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs que não tocam TownScene nem o gerador da cidade
> **Must not run with:** qualquer spec que edite `CreateMvpTownScene.cs`, `TownDistrictLayout.cs`, a TownScene, ou os validators da cidade
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`
> - `Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs`
> - `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs`
> - `Assets/_Game/Scripts/World/RoofRevealController.cs`, `HouseDoorInteractable.cs`
> **Depende de (Depends on):**
> - fable_40 (relayout 48×42 → ampliado) — já implementado, este spec o estende
> - GameEventBus, InteractionSystem, NpcScheduleAnchor (já existem)
> **Bloqueia (Blocks):**
> - Nenhuma spec mapeada (melhoria de coerência da cidade)
> **Scope:** Toda residência/marco da cidade é um prédio FÍSICO percorrível com porta funcional (E abre, sem teleporte), no MESMO plano da cidade; a cidade cresce o suficiente para acomodar todas as residências e marcos cívicos maiores, sem sobreposição.
> **Out of scope:** Arte final, NPC schedule rework, novos sistemas de gameplay, save schema, balance.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

A TownScene da fable_40 usava interiores numa **faixa off-playfield (y>+44)** com **portas de teleporte**: ao apertar E numa casa o jogador era teletransportado para uma grade de interiores distante e via "[E] Sair" para voltar. O usuário rejeitou esse modelo: quer **casas reais, físicas, no plano da cidade**, em que a porta **abre** (com E) e o jogador **entra a pé**, sem teleporte e sem cena separada.

Trabalho já iniciado (a completar/validar nesta spec):
- `RoofRevealController.cs` — telhado some quando o jogador entra (interior in-place).
- `HouseDoorInteractable.cs` — porta fechada bloqueia o vão; E desliza/abre; sem teleporte.
- `CreateWalkInHouse` no gerador — chão andável + paredes + porta + telhado + roof-reveal.
- Remoção da faixa off-field + portas de teleporte; anchor "home" passou a cair dentro da casa física.

Esta spec **fecha** esse modelo e adiciona o que o usuário pediu ao ver a cidade regenerada: a cidade parece esparsa e os **marcos cívicos não se destacam**. Precisamos **ampliar o footprint** e dar **destaque** a igreja, cemitério, mercado, uma **praça de eventos** e a **câmara**.

## 6. Problema

Sem esta spec:
- O jogador continua sem uma cidade coerente: marcos cívicos do tamanho de casas, sem distrito de mercado nem área de eventos, e (se a cena não for regenerada com o novo modelo) ainda com teleporte off-field — exatamente o que o usuário recusou.
- Não há contabilização explícita de "uma residência por morador indoor", arriscando NPCs sem lar físico ou casas sobrepostas quando a cidade cresce.

## 7. Objetivo

Ao final desta spec, a TownScene deve ter **todas as residências e marcos como prédios físicos percorríveis com porta funcional (E abre, sem teleporte)**, num footprint ampliado que acomoda cada residência e os marcos cívicos maiores (igreja, cemitério, distrito de mercado, praça de eventos, câmara) **sem sobreposição**, permitindo que o jogador entre em qualquer prédio a pé — sem alterar save schema, sistemas de combate/farm, nem exigir cena separada.

## 8. Fontes obrigatórias lidas

```text
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
.claude/rules/unity-assets.md
.claude/skills/unity-validation/SKILL.md
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (auditar)
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs (auditar)
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs (auditar)
Assets/_Game/Scripts/NPC/NpcTownRosterRegistry.cs (inventário de NPCs/residências)
```

## 9. Estado atual do repo

- `CreateMvpTownScene.cs` — EXISTE; já gera casas percorríveis (`CreateWalkInHouse`), portas (`CreateHouseDoor` + `HouseDoorInteractable`), roof-reveal e removeu a faixa off-field. **Não recriar** esses helpers; ampliar/ajustar layout e marcos.
- `TownDistrictLayout.cs` — EXISTE; `HalfWidth=28 / HalfHeight=24` (56×48). Esta spec **amplia** esses valores e os retângulos de distrito.
- `RoofRevealController.cs`, `HouseDoorInteractable.cs` — EXISTEM (runtime, `CindarsHope.World`). Completos. **Não recriar.**
- `ValidateFableCitySchedule.cs` — EXISTE; já checa casas percorríveis/roof-reveal (não mais 24 portas off-field). Ajustar contagens conforme o novo inventário.
- Faixa off-field / `DoorInteractable` de teleporte na cidade — REMOVIDA. `DoorInteractable.cs` permanece (usado por cave/farm) — **não apagar**.
- TownScene `.unity` — regenerada pelo menu `CindarsHope/Inicializar Projeto`. **Não editar YAML manualmente.**

Estado real deve ser confirmado na Phase 0 antes de implementar. Não recriar sistema existente.

## 10. Engineering stories

```text
Como jogador, quero entrar em qualquer casa apertando E na porta (a porta abre e eu ando dentro), sem ser teleportado para longe.
Como jogador, quero que a igreja, o cemitério, o mercado, a praça de eventos e a câmara sejam claramente maiores/distintos das casas comuns.
Como morador (NPC indoor), quero uma residência física na cidade onde o jogador me encontra à noite.
Como maintainer, quero que a geração avise (log) qualquer sobreposição de prédio ou prédio fora do playfield.
```

## 11. Escopo

Inclui:
- Ampliar o footprint da cidade (`TownDistrictLayout`) e os retângulos de distrito para acomodar todas as residências + marcos maiores com ruas e sem sobreposição.
- Garantir que **todo NPC indoor** tenha uma residência física percorrível (modelo compartilhado: 14 casas para 20 moradores indoor; 4 dormem ao relento).
- **Igreja** (Temple): maior prédio, multi-cômodo (nave + cômodo lateral), porta funcional.
- **Cemitério**: terreno maior — cripta destacada, ≥8 túmulos, cerca com portão.
- **Distrito de mercado**: um salão de mercado coberto (percorrível) + fileira de bancas numa praça de mercado.
- **Praça de eventos (festivais)**: área aberta dedicada com o `FestivalStallAnchor` (eventos do calendário).
- **Câmara** (casa das decisões): prédio cívico percorrível e destacado perto da praça.
- Cada prédio percorrível: chão andável + paredes sólidas + **porta funcional** (`HouseDoorInteractable`: fechada bloqueia, E abre deslizando, sem teleporte) + telhado com `RoofRevealController`.
- Auditoria de geração (`AuditHouseOverlaps`) cobrindo todos os prédios (residências + marcos).
- Atualizar `ValidateFableCitySchedule` para as novas contagens (residências percorríveis, ≥3 anchors por NPC).
- Atualizar a doc de direção da cidade (`docs/design/gameplay/city/`) com o inventário de residências e marcos.

## 12. Fora de escopo

Não inclui:
- Arte/sprites finais (placeholders coloridos seguem).
- Refactor de NPC schedule/AI, combate, farm, economia.
- Save schema / migrations.
- Cena separada de interior (decisão: interiores são in-place).
- Validação humana imediata (DEFERRED_TO_FINAL_VALIDATION).

## 13. Regras de não duplicação

```text
Não recriar RoofRevealController nem HouseDoorInteractable (já existem).
Não recriar CreateWalkInHouse / CreateHouseDoor (já existem; ajustar/parametrizar).
Não criar segundo gerador de cena; estender CreateMvpTownScene.
Não reintroduzir a faixa off-field nem DoorInteractable de teleporte na cidade.
Não criar novo sistema de interação; usar InteractionSystem + IInteractable existentes.
Não usar GameObject.Find/FindObjectOfType em runtime.
```

## 14. Critérios de aceite

### 14.1 Sem teleporte / sem off-field
- Nenhum GameObject de interior em `y > +40`.
- Nenhum `DoorInteractable` (porta de teleporte) na TownScene.
- Evidência: contagem por `ValidateFableCitySchedule` (0 interiores off-field; 0 DoorInteractable na cidade) + inspeção do gerador.

### 14.2 Toda residência é física e percorrível com porta
- Cada prédio em `TownHouseSpecs` tem: `Floor`, paredes (`Wall_*`), `Door` com `HouseDoorInteractable` (collider sólido + trigger de interação) e `RoofReveal` com `RoofRevealController`.
- Porta fechada bloqueia o vão; ao interagir (E) o collider sólido desliga e a folha desliza (sem mudar a posição do jogador).
- Evidência: `ValidateFableCitySchedule` PASS (N residências com Floor + RoofReveal + Door); inspeção do `CreateWalkInHouse`/`CreateHouseDoor`.

### 14.3 Todos os moradores indoor têm residência
- Para cada NPC com `HomeAssignment.Indoor`, existe a casa nomeada e o anchor `home` cai dentro dela (coords finais).
- Evidência: tabela de inventário (seção 15) batendo com `TownNpcHomes`; ≥3 anchors por NPC no validator.

### 14.4 Marcos cívicos ampliados e distintos
- Igreja com footprint ≥ 8×7 e ≥2 cômodos internos (divisória).
- Cemitério com cripta + ≥8 túmulos + cerca com vão de portão.
- Distrito de mercado com salão coberto percorrível + ≥6 bancas agrupadas.
- Praça de eventos: área aberta dedicada com `FestivalStallAnchor` (não sobreposta a casas).
- Câmara: prédio percorrível com footprint ≥ 6.5×5 perto da praça.
- Evidência: `TownHouseSpecs`/geradores de marco + log de auditoria sem sobreposição.

### 14.5 Cidade cresce e nada sobrepõe
- `TownDistrictLayout.HalfWidth/HalfHeight` ampliados; todos os prédios dentro do playfield (1 tile da borda) e sem sobreposição.
- Evidência: `AuditHouseOverlaps` sem `LogError`; `LogRelayoutElementCountAudit` consistente (House_* == TownHouseSpecs.Length, TownTree_* == TownTreePositions.Length).

### 14.6 Build limpo
- `Assembly-CSharp` e `Assembly-CSharp-Editor` exit 0.
- `.\tools\docs\run_strict_validation.ps1` exit 0.

---

# /speckit.plan

## 15. Arquitetura alvo

### 15.1 Footprint (ampliação)
`TownDistrictLayout`: `HalfWidth 28 → 38`, `HalfHeight 24 → 32` (≈76×64). 21 prédios percorríveis +
praça + mercado + praça de eventos + arredores cabem com ruas confortáveis (cobertura ~13%). Atualizar
`WidthTiles/HeightTiles` e os retângulos `Districts`. A borda de floresta (`BuildBorderTreeRing`) e os
Bounds acompanham automaticamente (já usam as constantes).

### 15.2 Inventário de residências (modelo MISTO — compartilhar só com relação real)

Regra: residência compartilhada **apenas** quando há relação que justifique (família/parceria de ofício);
caso contrário, cada NPC indoor tem **casa própria**. 20 NPCs indoor; 4 dormem ao relento.

Compartilhadas (relação explícita):
- **House_CarvalhoTorto** — Gurd + Hund Carvalho-Torto (mesmo sobrenome → família).
- **House_Inn** (Estalagem) — Orlan + Gruta (tocam a estalagem juntos).

| # | Prédio | Tipo | Morador(es) indoor | Tamanho alvo |
|---|--------|------|--------------------|--------------|
| 1 | House_Temple (Igreja) | Marco cívico/residência | Corvus | 8.0×7.0 (multi-cômodo) |
| 2 | House_Chamber (Câmara) | Marco cívico | — (decisões) | 6.6×5.2 |
| 3 | House_Manor (Mansão) | Residência (líder) | Velorin | 7.0×5.6 |
| 4 | House_Inn (Estalagem) | Residência/serviço **compartilhada** | Orlan, Gruta | 6.4×5.2 |
| 5 | House_CarvalhoTorto | Residência **compartilhada** (família) | Gurd, Hund | 6.2×5.0 |
| 6 | House_Registry | Residência/ofício | Mara | 5.8×4.6 |
| 7 | House_Tovin | Residência | Tovin | 5.4×4.4 |
| 8 | House_Blacksmith | Residência/ofício | Brumdar | 5.8×4.6 |
| 9 | House_Dagna | Residência | Dagna | 5.4×4.4 |
| 10 | House_Archive | Residência/ofício | Thalindra | 5.8×4.6 |
| 11 | House_Workshop | Residência/ofício | Nimble | 5.8×4.6 |
| 12 | House_AlchemyLab | Residência/ofício | Ozzra | 5.8×4.6 |
| 13 | House_AnimalYard | Residência/ofício | Eiran | 5.8×4.6 |
| 14 | House_GateKeeper | Residência/posto | Alaric | 5.6×4.6 |
| 15 | House_Pip | Residência | Pip | 5.4×4.4 |
| 16 | House_Residential_1 | Residência | Sylveth | 5.8×4.6 |
| 17 | House_Residential_2 | Residência | Renko | 5.6×4.4 |
| 18 | House_Residential_3 | Residência | Mirela | 5.6×4.4 |
| 19 | House_Residential_4 | Residência | Savra | 5.4×4.4 |
| 20 | House_Prison | Marco cívico | — | 5.6×4.6 |
| 21 | House_MarketHall | Mercado coberto (percorrível) | — | 6.4×5.2 |

Cobertura indoor (20): Corvus, Velorin, Orlan+Gruta, Gurd+Hund, Mara, Tovin, Brumdar, Dagna,
Thalindra, Nimble, Ozzra, Eiran, Alaric, Pip, Sylveth, Renko, Mirela, Savra. Novos nomes
(House_Tovin, House_Dagna, House_Pip, House_CarvalhoTorto) são prédios novos — não quebram saves
(IDs antigos House_* preservados; Gurd/Hund saem de Workshop/GateKeeper para a casa de família).

Marcos NÃO-prédio (sem porta, áreas):
- **Praça de mercado**: ≥6 bancas (`Stall_*`, já geradas por vendedor) agrupadas + toldos, ao redor do MarketHall.
- **Praça de eventos**: área aberta dedicada (slab de chão + `FestivalStallAnchor`), sem casas em cima.
- **Cemitério** (NW): cripta + ≥8 túmulos + cerca com portão (decoração, só a cripta/rochas colidem se fizer sentido). Maelor dorme aqui.
- **Boca de gruta** (E): rochas que BLOQUEIAM (CreateBlocker). Zrix dorme aqui.
- **Tenda noturna** (S): Yael dorme aqui.
- **Jardim da estátua** (praça central): Liora dorme aqui.

Ao relento (4): Maelor, Zrix, Yael, Liora — sem casa (já em `TownNpcHomes` Outdoor).

> Observação: MarketRow_A/B/C (3 "casas" antigas sem morador) são **substituídas** pelo MarketHall + praça de bancas; isso reduz de 19 para 17 prédios com porta, mais coerente (não há casa vazia).

### 15.3 Distritos (alvo, footprint ~72×60)
```
Praça central (estátua)            centro (0,0), ~12×12
Distrito de mercado                oeste-centro: MarketHall + bancas
Praça de eventos (festivais)       sul-centro: área aberta + FestivalStallAnchor
Quarteirão cívico                  norte: Igreja (grande) + Câmara + Mansão
Residencial Oeste/Leste            colunas de casas com ruas
Residencial Sul                    casas + Prisão
Arredores                          Cemitério (NW), Gruta (E), Tenda (S)
Entrada da fazenda                 portal no meio-oeste (vão na floresta)
```

### 15.4 Arquivos
```
Assets/_Game/Scripts/Editor/SceneCreation/
  TownDistrictLayout.cs        (ampliar footprint + distritos)
  CreateMvpTownScene.cs        (reposicionar/dimensionar prédios; igreja multi-cômodo;
                                MarketHall; praça de eventos; cemitério maior; auditoria)
Assets/_Game/Scripts/Editor/Validation/
  ValidateFableCitySchedule.cs (contagens novas)
docs/design/gameplay/city/
  CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md  (inventário de residências/marcos)
docs/validation/
  spec_city_real_walkin_houses_no_teleport_execution_report.md
docs/validation/playmode/
  spec_city_real_walkin_houses_no_teleport_human_test_scenario.md
```

## 16. Contratos, dados e eventos

- **16.1 Data contracts:** `TownHouseSpecs` (nome, posição final, cor, tamanho) — ampliado/reordenado. IDs/nomes `House_*` preservados onde houver save/anchor dependente.
- **16.2 Runtime contracts:** `HouseDoorInteractable : IInteractable` (E abre/fecha; collider sólido toggled; sem mover o jogador). `RoofRevealController` (alpha do telhado por presença do player).
- **16.3 Event contracts:** Usa `PlayerActionFeedbackEvent` (já existe) para feedback "Porta aberta/fechada". **Nenhum evento novo.**
- **16.4 Save contracts:** N/A — sem mudança de save schema. Portas começam fechadas a cada carregamento (estado não persistido; aceitável).
- **16.5 UI contracts:** Usa prompt de interação existente ("Abrir porta"/"Fechar porta").

## 17. Sistemas afetados

```text
Editor scene generation (TownScene)
City layout (TownDistrictLayout)
Interaction (IInteractable / InteractionSystem) — consumo, sem alterar
NPC schedule anchors (home in-place)
City validators
Docs de direção da cidade
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs
Assets/_Game/Scripts/World/RoofRevealController.cs        (ajuste fino se necessário)
Assets/_Game/Scripts/World/HouseDoorInteractable.cs        (ajuste fino se necessário)
docs/design/gameplay/city/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity (edição manual de YAML — regenerar via menu)
Assets/**/*.prefab
Assets/**/*.asset (YAML manual)
DoorInteractable.cs (não apagar — usado por cave/farm)
Packages/**
ProjectSettings/**
docs_old/**, docs/archive/**
```

## 20. Estratégia de implementação

```
Fase 0 — Auditoria: confirmar helpers existentes (walk-in, door, roof-reveal), inventário de NpcTownHomes, validators. Não recriar.
Fase 1 — Footprint: ampliar TownDistrictLayout (HalfWidth/Height + distritos).
Fase 2 — Prédios: reposicionar/dimensionar TownHouseSpecs (igreja grande multi-cômodo; substituir MarketRow por MarketHall; câmara/mansão/prisão). Igreja com divisória (2 cômodos).
Fase 3 — Marcos-área: distrito de mercado (hall + bancas), praça de eventos (slab + FestivalStallAnchor reposicionado), cemitério maior.
Fase 4 — Auditoria/validator: AuditHouseOverlaps cobre tudo; ValidateFableCitySchedule contagens novas.
Fase 5 — Build + docs + report + cenário humano.
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar gerador/validators/inventário (Phase 0).
2. Ampliar TownDistrictLayout.
3. Reposicionar/dimensionar prédios; igreja multi-cômodo; MarketHall.
4. Praça de eventos + distrito de mercado + cemitério maior.
5. Atualizar AuditHouseOverlaps + ValidateFableCitySchedule.
6. dotnet build (runtime + editor) exit 0.
7. run_strict_validation.ps1 exit 0.
8. Atualizar doc de direção + execution report + cenário humano.
9. (Humano) CindarsHope/Inicializar Projeto + Rebuild Town NPC Dialogues; inspecionar com F7/F8.
```

## 22. Paralelização

```md
- Parallelizable: NO
- Reason: edita o gerador da cidade, o layout e os validators — arquivos de lock central da TownScene.
- Shared files/systems that require lock: CreateMvpTownScene.cs, TownDistrictLayout.cs, ValidateFableCitySchedule.cs
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: YES (RoofRevealController/HouseDoorInteractable já tratam OnEnable/OnDisable conforme aplicável)
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (usa prompt de interação existente)
Changes scenes: YES (TownScene — via gerador/menu, não YAML manual)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: sobreposição de prédios ao ampliar/reposicionar. Mitigação: AuditHouseOverlaps loga erro na geração; ajustar coords até zero erro.
Risco: marco maior cair fora do playfield. Mitigação: auditoria de bounds + clamp; igreja/mercado dentro de ±(Half-1).
Risco: porta detectada pela interação mas inalcançável (telhado/parede). Mitigação: trigger de interação maior que o vão; porta desenhada acima do telhado (visível).
Risco: instabilidade do csproj/Library headless (TMP). Mitigação: validar no Unity (Inicializar Projeto) que recompila tudo; reportar build headless honestamente.
Risco: renomear House_* quebra anchors/save. Mitigação: preservar nomes existentes; novos prédios (MarketHall) com nome novo; documentar.
```

## 27. Rollback

```text
Reverter TownDistrictLayout (HalfWidth/Height) e TownHouseSpecs para os valores anteriores.
Manter RoofRevealController/HouseDoorInteractable (já fechados).
Regenerar a cena via menu para voltar ao layout anterior.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Phase 0: auditar CreateWalkInHouse/CreateHouseDoor/RoofRevealController/HouseDoorInteractable + TownNpcHomes + validators (não recriar).
- [ ] T002 — Ampliar TownDistrictLayout (HalfWidth 36 / HalfHeight 30) + retângulos de distrito.
- [ ] T003 — Reposicionar/dimensionar TownHouseSpecs (igreja 8×7 multi-cômodo; câmara/mansão/prisão; substituir MarketRow_A/B/C por House_MarketHall).
- [ ] T004 — Igreja multi-cômodo (divisória interna) reutilizando CreateInteriorWall/Prop.
- [ ] T005 — Distrito de mercado: posicionar bancas (Stall_*) agrupadas ao redor do MarketHall.
- [ ] T006 — Praça de eventos: slab de chão dedicado + reposicionar FestivalStallAnchor para a praça (sem casa em cima).
- [ ] T007 — Cemitério maior: cripta + ≥8 túmulos + cerca com portão.
- [ ] T008 — AuditHouseOverlaps cobre todos os prédios; zero LogError.
- [ ] T009 — ValidateFableCitySchedule: contagens novas (residências percorríveis, ≥3 anchors/NPC, 0 off-field, 0 DoorInteractable).
- [ ] T010 — dotnet build runtime+editor exit 0; run_strict_validation.ps1 exit 0.
- [ ] T011 — Atualizar doc de direção da cidade com inventário de residências/marcos.
- [ ] T012 — Execution report + cenário humano de Play Mode.
```

## 29. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
.\tools\docs\run_strict_validation.ps1
```
Unity compile/asset gen: via `CindarsHope/Inicializar Projeto` (regenera a cena). Se algum comando não rodar, registrar `NOT RUN` com motivo e risco residual.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: NO (geometria de cena/editor; sem fórmula de gameplay nova)
- Requires EditMode tests: NO (lógica determinística não alterada; HouseDoorInteractable é toggle simples — coberto por cenário humano)
- Requires PlayMode automated or final human scenario: YES (porta abre/fecha, entrada física, roof-reveal, sem teleporte)
- Requires regression test: NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: build exit 0; AuditHouseOverlaps sem erro; ValidateFableCitySchedule PASS; cenário humano confirmando porta E + entrada in-place + marcos maiores.
```

## 31. Definition of Done

```text
Footprint ampliado; todos os prédios dentro do playfield e sem sobreposição (auditoria limpa).
Toda residência indoor física e percorrível com porta funcional (E abre, sem teleporte).
Marcos ampliados: igreja grande multi-cômodo, cemitério maior, distrito de mercado, praça de eventos, câmara.
Sem faixa off-field; sem DoorInteractable de teleporte na cidade.
Build runtime+editor exit 0; run_strict_validation exit 0.
Doc de direção + execution report + cenário humano criados.
Sem claim de ACCEPTED sem validação humana final.
```

## 32. Anti-regressão

```text
Não reintroduzir faixa off-field nem teleporte na cidade.
Não renomear House_* já referenciados por anchors/save sem justificativa.
Não usar GameObject.Find/FindObjectOfType em runtime.
Não editar YAML de .unity/.prefab/.asset manualmente.
Não apagar DoorInteractable.cs (cave/farm dependem).
Não quebrar LogRelayoutElementCountAudit (House_*/TownTree_* counts).
```

## 33. Notas para execução posterior

```text
Esta spec mantém placeholders coloridos (arte final é outra spec).
Estado de porta (aberta/fechada) não é persistido — reabre fechada ao carregar (aceitável; persistência futura se necessário).
Validação humana final é deferida para o lote, conforme protocolo.
Se o usuário quiser 1 casa por NPC (em vez de compartilhada), é uma variação do footprint (≈84×68) — registrar como follow-up.
```
