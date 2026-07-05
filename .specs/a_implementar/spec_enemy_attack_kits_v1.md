# SPEC — Kits de Ataque de Inimigo (Universo Completo) + 4 Primitivas P2

> **Spec ID:** `spec_enemy_attack_kits_v1`
> **Status:** A implementar
> **Wave:** Lote ENEMY_ATTACK_KITS (design-first, gerado 2026-07-03)
> **Priority:** P2
> **Type:** Runtime + Data + Editor
> **Domain:** Combat / Cave / Enemy AI / Bestiary
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** qualquer spec que edite `EnemyBrain.cs`, `EnemyActionSO.cs`, `EnemyActionExecution.cs`, `CindarsHopeMenu.cs`, ou que gere/edite assets em `Assets/_Game/Data/Enemies/Actions/**` ou `Assets/_Game/Data/Enemies/ActionSets/**` (ex.: fable_83, fable_24, fable_05, CX13, CV01)
> **Repo lock scope:** `Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs`, `Assets/_Game/Scripts/Combat/EnemyHealth.cs`, `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`, `Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs`, `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs`, `Assets/_Game/Data/Enemies/Actions/**`, `Assets/_Game/Data/Enemies/ActionSets/**`
> **Depends on:**
> - `docs/design/gameplay/enemies/ENEMY_ATTACK_CATALOG_DIRECTION_v1.0.md` (v1.1 — fonte de design canônica)
> - `docs/design/gameplay/enemies/ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md` (fonte de implementação)
> - fable_83 (EnemyActionType base + EnemyActionExecution + os 5 tipos signature já implementados)
> - fable_24 (EliteAffix / Moves canônicos), fable_33/fable_80 (bestiário 60+117), CX13 (ContactFilter2D — não tocar)
> **Blocks:**
> - spec futura de `EnemyAttackAnimator` (folhas de sprite; gap de apresentação §6 do catálogo — fora de escopo aqui)
> - spec futura das primitivas P3/P4 (silence, riposte, reflect, execute, twin-link, decoy, ambiente)
> **Scope:** autoria data-driven do kit de 2-3 ações (Melee/Ranged + Especial) para os ~121 donos de kit do universo (113 fichas canônicas das 7 bands + 7 novas do Roster + `enemy_meteor_ooze_king`), wiring das 53 variâncias do Roster ao actionset da criatura-mãe, e as 4 primitivas mecânicas novas de prioridade P2 do catálogo (Rise-once, AllyHeal/AllyBuff, HazardZone, Pull) com generator idempotente + validator read-only.
> **Out of scope:** `EnemyAttackAnimator` / folhas de sprite de ataque; primitivas P3/P4 (morte volátil como especial de espécie, decoy summon, alerta de pack como ação, quebra-guarda, silence, riposte, reflect, execute condicional, twin-link, interações de ambiente); mecânicas de encontro do The Four (`boss_*`, DORMANTE); qualquer crosswalk de arte/skin binding.

required_adrs: [ADR-0005]
required_game_rules: [combat_rules.md, cave_rules.md]

---

# /speckit.specify

## Contexto

O `ENEMY_ATTACK_CATALOG_DIRECTION_v1.0.md` (v1.1, 2026-07-03) e seu par
`ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md` fecham o design de ataque para o universo
completo de 178 IDs de inimigo (117 fichas canônicas do `CanonicalBestiaryCatalog` + 60 IDs do
Roster em jogo + `enemy_meteor_ooze_king`), incluindo qual arquétipo mecânico (`atk_slash`,
`atk_bite`, `atk_cast`, etc.) cada criatura usa como Normal e Especial, o crosswalk de quais dos
60 IDs do Roster são variâncias (53, herdam kit da mãe) versus novas (7, kit próprio), e as
primitivas mecânicas que faltam no runtime atual (`ENEMY_ATTACK_CATALOG_DIRECTION_v1.0.md` §5).
fable_83 já implementou a base de 5 `EnemyActionType` signature (ComboStrike, TelegraphedAoE,
SummonAdds, MultiHitCharge, DebuffStrike) com `EnemyActionExecution` puro e testável — esta
spec **não recria** essa base; ela a consome. Esta spec destrava a wave de conteúdo do
catálogo: sem os action sets do universo completo, apenas os 60 IDs do Roster (não os 117
canônicos) têm ataque configurado, e faltam 4 primitivas de dados (Rise-once, AllyHeal/AllyBuff,
HazardZone, Pull) citadas repetidamente na tabela §5 do catálogo como bloqueio de ~15 fichas.

O escopo é pequeno e fatiado em 3 slices para reduzir risco: Slice 1 (código runtime das 4
primitivas, EditMode-testável), Slice 2 (generator data-driven que autora os `EnemyActionSO`/
`EnemyActionSetSO` do universo + wiring de variância), Slice 3 (validator read-only). Ficam para
specs futuras: o `EnemyAttackAnimator` (sprites — gap §6 do catálogo, ainda não existe pipeline)
e as primitivas P3/P4 (menos criaturas bloqueadas, mecânica mais arriscada — morte volátil,
decoy, silence, riposte, reflect, execute, twin-link, ambiente).

## Problema

Sem os action sets do universo completo, as 113 fichas canônicas do `CanonicalBestiaryCatalog`
(que a cave NÃO spawna hoje, mas que são a fonte de verdade do bestiário e alimentam specs
futuras de encontro/boss) não têm nenhum ataque configurado — `EnemyDataSO.ActionSetId` fica
vazio ou aponta para nada, e `EnemyBrain.HasResolvedActionSet` retorna `false`. Das 60 fichas do
Roster que a cave spawna hoje, todas já têm `EnemyActionSO`/`EnemyActionSetSO` (gerados por
`CreateEnemyActionsAndSets.CreateAll()`), mas com naming livre (ex.: `action_grashnaar_knife_jab`)
em vez do padrão `action_{enemyId}_{melee|ranged|special}` e sem refletir o kit final do catálogo
v1.1 (arquétipos corretos, `MinRange` no ranged, Especiais nomeados). Sem `Rise-once`,
`AllyHeal/AllyBuff`, `HazardZone` e `Pull`, ~15 fichas do catálogo ficam com Especial "fake"
(reduzido a um segundo Melee genérico) porque a mecânica descrita na ficha (ex.: `cracked_bone`
reerguer 1×, `goblin_shaman` curar aliado) não existe no runtime.

## Objetivo

Ao final desta spec, o projeto deve ter: (1) as 4 primitivas mecânicas P2 do catálogo
(Rise-once, AllyHeal/AllyBuff, HazardZone, Pull) implementadas em C# puro e testável, seguindo o
padrão `EnemyActionExecution` de fable_83; (2) `EnemyActionSO`/`EnemyActionSetSO` gerados para
os ~121 donos de kit do universo (113 canônicas + 7 novas do Roster + `enemy_meteor_ooze_king`),
com o kit de 2-3 ações descrito nas tabelas §4/§4B do catálogo; (3) as 53 variâncias do Roster
com `EnemyDataSO.ActionSetId` apontando para o actionset da criatura-mãe (sem duplicar ações);
(4) um validator read-only registrado em `Validar Projeto`; tudo isso **sem** alterar
`EnemyAttackAnimator` (não existe ainda), sem tocar nas 4 fichas `boss_*` do The Four (DORMANTE),
e sem quebrar o stable-run da cave (nenhuma mudança de seed/spawn/layout).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/design/gameplay/enemies/ENEMY_ATTACK_CATALOG_DIRECTION_v1.0.md (v1.1 — kits por criatura §4/§4A/§4B, arquétipos §2, primitivas novas §5)
docs/design/gameplay/enemies/ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md (mecânica+visual por arquétipo §3, primitivas §4, validação §6)
Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs (EnemyActionType, campos existentes incl. ComboHits/AoeDelay/AoeRadius/SummonCount/SummonEnemyId/DebuffStatusId)
Assets/_Game/Scripts/Combat/Data/EnemyActionSetSO.cs
Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs (padrão de lógica pura a seguir para as 4 primitivas novas)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (SelectBestAction, BeginAction/ResolveAction, EnemyBrainState)
Assets/_Game/Scripts/Combat/EnemyHealth.cs (Die() — único ponto de morte; ponto de extensão do Rise-once)
Assets/_Game/Scripts/Combat/EnemyDataSO.cs (ActionSetId, PrimaryDamageTypeId, CaveBand, BestiarySizeClass)
Assets/_Game/Scripts/Combat/Bestiary/CanonicalBestiaryCatalog*.cs (117 fichas — fonte dos IDs canônicos)
Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemyActionsAndSets.cs (generator existente dos 60 IDs — NÃO recriar; referência de padrão e o que já existe)
Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyActions.cs (padrão de validator existente — NÃO recriar; estender/substituir escopo)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs (FASE A do InicializarProjeto; RunStep pattern)
Assets/_Game/Scripts/Core/Data/StatusEffectDatabaseSO.cs (DataRegistrySO<StatusEffectSO> — StatusApplicationIds devem resolver aqui)
.specs/implementados/spec_fable_83_enemy_signature_attacks_light_pathing_runtime.md (calibração de densidade + precedente EnemyActionExecution)
.claude/rules/cave-stable-run.md ; .claude/rules/id-stability.md ; .claude/rules/no-magic-balance-values.md
.claude/rules/editor-generation-orchestration.md (3 comandos canônicos; RunStep; sem [MenuItem] avulso)
.claude/rules/testing-quality-gate.md ; .claude/skills/spec-execution/SKILL.md ; .claude/skills/unity-validation/SKILL.md
.claude/skills/data-catalog-authoring/SKILL.md ; .claude/skills/enemy-ai-authoring/SKILL.md
.claude/skills/editor-validator-authoring/SKILL.md ; .claude/skills/rng-and-determinism/SKILL.md
.claude/skills/editmode-test-authoring/SKILL.md ; .claude/skills/no-magic-balance-values (stub)
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyActionSO (13 EnemyActionType incl. os 5 signature de fable_83) + EnemyActionSetSO +
  EnemyActionDatabaseSO/EnemyActionSetDatabaseSO (DataRegistrySO<T> — auto-scan por tipo,
  sem lista manual a registrar);
- EnemyActionExecution (lógica pura de ComboStrike/TelegraphedAoE/SummonAdds/MultiHitCharge/
  DebuffStrike) — padrão a seguir para as 4 primitivas novas desta spec;
- EnemyBrain (SelectBestAction por range+cooldown já lê MinRange; BeginAction/ResolveAction;
  EnemyBrainState) e EnemyTelegraphController (piscar de cor);
- EnemyHealth.Die() — único ponto de morte de inimigo (spawna evento EnemyKilledEvent/
  EnemyKilledByEnemyEvent); ponto de extensão único para Rise-once;
- StatusEffectDatabaseSO (DataRegistrySO<StatusEffectSO>) com os status canônicos (fable_01);
- CreateEnemyActionsAndSets.CreateAll() — JÁ gera EnemyActionSO/EnemyActionSetSO para os 60 IDs
  do Roster (pasta Assets/_Game/Data/Enemies/Actions/ e .../ActionSets/), mas com IDs de ação
  livres (ex. action_grashnaar_knife_jab, não action_{enemyId}_{melee|ranged|special}) e sem
  refletir o kit final do catálogo v1.1 nem MinRange consistente. Chamado por
  GenerateAndWireSpec13GAssets (verificar se este por sua vez está no RunStep do
  InicializarProjeto — auditar na Fase 0);
- ValidateSpec13EnemyActions — valida APENAS os 40 IDs de uma lista hardcoded antiga
  (RequiredActionSetIds), não os 60 do Roster nem as 113 fichas canônicas;
- CanonicalBestiaryCatalog (117 fichas, split por band BandStone/Fungal/Ice/Fire/Ruins/Deep/Void
  + BandFinalFour) — nenhuma tem ActionSetId configurado até esta spec.

Não existe:
- Rise-once, AllyHeal/AllyBuff, HazardZone, Pull (as 4 primitivas P2 do catálogo §5);
- EnemyActionSO/EnemyActionSetSO para as 113 fichas canônicas nem para as 7 NOVAS do Roster
  (ash_crawler, blackroot_sprout, corrupted_bone_knight, hollow_stagling, stone_rat,
  frost_gnawer, thorn_archer) nem para enemy_meteor_ooze_king;
- wiring de variância (53 IDs do Roster) apontando ActionSetId para a criatura-mãe;
- validator cobrindo o universo completo (117+60+1) com as checagens do catálogo §4B/IMPLEMENTATION §6.

Auditar Fase 0 (obrigatório antes de codar):
- confirmar se GenerateAndWireSpec13GAssets (que chama CreateEnemyActionsAndSets.CreateAll())
  já está registrado como RunStep em CindarsHopeMenu.InicializarProjeto, e em que ordem relativo
  ao bestiário canônico — o novo generator desta spec deve rodar DEPOIS de ambos;
- decidir se os 60 EnemyActionSO/EnemyActionSetSO já existentes são realinhados in-place
  (mesmos ActionId antigos, campos atualizados) ou se o generator novo cria os ~121 assets do
  universo com o naming padrão `action_{enemyId sem prefixo}_{melee|ranged|special[_n]}` /
  `actionset_{enemyId}` e os 60 antigos ficam órfãos — Fase 0 deve registrar a decisão e o
  motivo (recomendação: realinhar in-place por AssetDatabase.LoadAssetAtPath, evitando
  duplicar 60 assets e quebrar refs existentes de EnemyDataSO.ActionSetId já wired em cena);
- checar quais EnemyDataSO das 113 fichas canônicas existem hoje como asset em
  Assets/_Game/Data/Enemies/Canonical/ (o catálogo cita esse path como fonte) e se já têm
  campo ActionSetId presente mas vazio, versus fichas que não têm asset ainda (fora do escopo
  criar EnemyDataSO novo — só popular ActionSetId nos que já existem; ausência de asset para
  uma ficha do catálogo é log de aviso do validator, não erro bloqueante desta spec).
```

## Engineering stories

```text
Como o sistema de combate, quero que toda ficha dona de kit (universo completo) tenha um
  EnemyActionSetSO válido com ≥1 Melee/Ranged + ≥1 Especial, para que EnemyBrain nunca rode
  com HasResolvedActionSet=false.
Como designer, quero os kits derivados dos dados do catálogo (arquétipo, dano base, elemento),
  não hardcoded linha a linha, para que futuras correções no catálogo só peçam re-rodar o
  generator.
Como stable-run, quero que Rise-once/HazardZone/Pull não usem GUID/timestamp e que HazardZone
  vire hazard determinístico por instância, para não quebrar o replay da FASE9F.
Como QA, quero um validator read-only que apanhe kit incompleto, ranged sem fallback melee,
  variância desalinhada da mãe, StatusApplicationId inexistente ou TelegraphProfileId ausente,
  antes que isso vire bug em Play Mode.
```

## Escopo

```text
Inclui:
- SLICE 1 (runtime, C# puro + EnemyHealth/EnemyBrain hooks):
  - RiseOnce: flag configurável em EnemyDataSO (ou EnemyActionSO do especial) + lógica pura em
    EnemyActionExecution (ex.: EnemyActionExecution.ShouldRiseOnce(...), ResolveRiseHp(...)) +
    hook em EnemyHealth.Die() que intercepta a morte, aplica colapso 2s + reergue 1x com X% HP,
    salvo se o último DamageType aplicado casar com um elemento bloqueador configurável
    (fire/radiant por padrão, valor em dado). Usuários iniciais: cracked_bone, undead_shambler,
    frostbound_revenant (ver ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0.md §4 "Rise-once").
  - AllyHeal/AllyBuff: variação de SelfBuff com alvo-aliado num raio configurável (cura % HP OU
    buff dano/velocidade OU escudo temporário) — lógica pura de seleção de alvo-aliado mais
    ferido/mais próximo em raio (EnemyActionExecution.ResolveAllyHealTarget(...) ou similar,
    recebendo posições+HP como parâmetros, sem MonoBehaviour). Usuários: goblin_shaman (Mend),
    cold_cult_acolyte, orc_drummer, grimfang_packleader (Howl), cultist_zealot, crystal_hound,
    mycobulwark, duergar_frostdelver (VisualScale), gravedelver_warder (GuardHold), etc.
  - HazardZone: componente/estrutura de dados genérica de zona no chão (dano/status por tick +
    duração + elemento), reusável por (a) TelegraphedAoE com flag LeaveHazard (a zona persiste
    após o dano inicial) e (b) rastro contínuo por movimento (magma_slug, lava_bulwark). Lógica
    pura de tick/expiração testável sem MonoBehaviour; runtime component fino se necessário para
    detectar overlap com o player (reusar padrão de trigger/overlap já existente no projeto,
    não inventar sistema de física novo).
  - Pull: variação de DebuffStrike com deslocamento do player N tiles na direção configurada
    (do atacante ou do hazard) — cálculo puro de posição-alvo (clamp contra paredes é
    responsabilidade do consumidor/EnemyBrain, não desta primitiva). Usuários: cinder_shade,
    ruin_warden, abyssal_lurker, starfall_remnant, heralds_hand, lake_lurker.
  - Cada primitiva: parâmetros em EnemyActionSO (campos aditivos, sem magic values inline),
    EditMode tests cobrindo a lógica pura (seguindo o padrão de EnemySignatureActionsTests.cs),
    sem GameObject.Find/FindObjectOfType, comunicação de efeito colateral (se necessária) via
    GameEventBus.

- SLICE 2 (editor, generator data-driven, idempotente):
  - GenerateEnemyAttackKits: método public static idempotente, SEM [MenuItem] próprio — registrado
    como RunStep em CindarsHopeMenu.InicializarProjeto, na FASE A, DEPOIS do bestiário canônico e
    dos movement profiles (ordem de dependência: precisa ler EnemyDataSO.CaveBand/PrimaryRole/
    PrimaryDamageTypeId já populados).
  - Gera/realinha EnemyActionSO (`action_{enemyId sem prefixo}_{melee|ranged|special[_n]}`) e
    EnemyActionSetSO (`actionset_{enemyId}`) para os ~121 donos de kit: as 113 fichas canônicas
    das 7 bands (Stone/Fungal/Ice/Fire/Ruins/Deep/Void) que já têm EnemyDataSO asset + as 7 NOVAS
    do Roster (ash_crawler, blackroot_sprout, corrupted_bone_knight, hollow_stagling, stone_rat,
    frost_gnawer, thorn_archer) + enemy_meteor_ooze_king. Os `boss_*` do The Four NÃO ganham kit
    (permanecem DORMANTE, sem ActionSetId).
  - Kits de 2 ações (Melee+Especial) ou 3 (Melee fallback + Ranged + Especial, com MinRange no
    ranged) conforme as tabelas §4/§4B do catálogo; melee de fallback por plano de corpo (§1b
    do catálogo: humanoide caster/atirador→atk_slash fraco; besta/dracônico→atk_bite;
    construct→atk_slam; planta/fungo→atk_whip; espectro/elemental→atk_claw; dano ~60% do
    ranged, sem status); parâmetros (dano/windup/cooldown/range) derivados dos stats da ficha
    (BaseDamage a partir de maxHp/enemyLevel/baseDifficulty já existentes) com multiplicadores
    NOMEADOS por arquétipo (consts, não literais soltos) — replicando a tabela de ranges/windups
    do §3 do ENEMY_ATTACK_IMPLEMENTATION_DIRECTION por arquétipo.
  - Bosses/minibosses: 1 EnemyActionSetSO por fase conforme catálogo §4B (Normal + S1/S2 para
    minibosses; Normal + especial por fase F1/F2/F3 para gate bosses) — reusa o mecanismo
    `BossPhaseShift` já existente (trocar ActionSet por fase); esta spec autora os DADOS
    (actionsets por fase), não o mecanismo de troca.
  - As 53 VARIÂNCIAS (§4A do catálogo): para cada ID do Roster classificado como variância, o
    generator seta `EnemyDataSO.ActionSetId` (via AssetDatabase, carregando o asset existente)
    para apontar ao ActionSetId da criatura-mãe do catálogo — SEM duplicar EnemyActionSO/
    EnemyActionSetSO. Update de asset existente SÓ via editor API (SerializedObject/API pública
    do SO + AssetDatabase.SaveAssets), nunca edição manual de YAML.
  - Idempotente: re-rodar não duplica assets (checa por ActionId/ActionSetId existente antes de
    criar, no padrão de CreateEnemyActionsAndSets); realinha campos de assets já existentes dos
    60 IDs do Roster para o kit final do catálogo v1.1 em vez de deixar dois conjuntos
    divergentes (decisão de Fase 0).

- SLICE 3 (editor, validator read-only):
  - ValidateEnemyAttackKits: método public static read-only, registrado como RunStep em
    CindarsHopeMenu.ValidarProjeto (não muta nada). Checa:
    - todo dono de kit (dos ~121, exceto The Four) tem EnemyActionSetSO com ≥1 ação
      Melee/RangedProjectile/CastProjectile/... "normal" + ≥1 ação classificada como Especial;
    - toda ação classificada como Ranged (RangedProjectile/CastProjectile com Range acima de um
      threshold) tem, no mesmo actionset, uma ação de melee fallback E MinRange > 0 configurado
      na ação ranged;
    - toda variância (53 IDs) tem ActionSetId igual ao da criatura-mãe (crosswalk §4A embutido
      como tabela de dados no validator, ou lido de um asset/JSON gerado pelo Slice 2 —
      decisão de implementação, mas sem duplicar a tabela em 2 lugares no código-fonte);
    - todo StatusApplicationId referenciado por uma EnemyActionSO existe no
      StatusEffectDatabaseSO;
    - todo TelegraphProfileId referenciado existe no EnemyTelegraphProfileDatabaseSO;
    - ação Ranged só existe em criatura com Role Ranged/Caster ou explicitamente listada no
      catálogo com Dist R/C (não travar em heurística frágil — usar a lista letigimada pelo
      catálogo como fonte, não inferir de PrimaryRole sozinho).
    - Log de aviso (não erro bloqueante) para fichas canônicas sem EnemyDataSO asset ainda
      materializado (fora do escopo desta spec criar o asset).
```

## Fora de escopo

```text
Não inclui:
- EnemyAttackAnimator ou qualquer folha de sprite de ataque/movimento (gap §6 do catálogo —
  pipeline de arte não existe ainda; specs de conteúdo visual ficam para depois);
- primitivas P3/P4 do catálogo §5: morte volátil como especial de espécie (reusa
  EnemyVolatileExplosionRunner do elite affix — só popular flag, sem código novo, então TAMBÉM
  fora daqui pois não é uma das 4 primitivas P2 pedidas), decoy summon, alerta de pack como ação,
  quebra-guarda, silence, riposte/counter, reflect de projétil, execute condicional, twin-link,
  interações de ambiente (apagar tochas / isca de loot);
- mecânicas de encontro do The Four (`boss_*`, nível 101) — permanecem DORMANTE por design;
- criação de EnemyDataSO novo para fichas canônicas que ainda não têm asset materializado
  (populam apenas ActionSetId nos assets já existentes; ausência vira aviso do validator);
- correção dos bindings de arte marcados com † no §4A do catálogo (duergar_*, orc_kaand_berserker,
  frost_wailer) — é um follow-up de crosswalk de arte, não desta spec de mecânica;
- criação de fichas novas no CanonicalBestiaryCatalog para as 7 NOVAS do Roster (follow-up citado
  no catálogo §7.4 — fora de escopo aqui, que só autora o kit de ataque das 7 já existentes como
  EnemyDataSO do Roster);
- pooling de projéteis/adds/hazards (object-pooling é spec própria — usar spawn/instantiate
  existente, sem introduzir sistema de pool novo);
- navmesh/pathing — não tocado;
- balance final (números de dano/windup são derivação inicial a partir dos stats existentes,
  não tuning definitivo);
- validação humana imediata (Play Mode fica DEFERRED_TO_FINAL_VALIDATION, conforme template).
```

## Regras de não duplicação

```text
NÃO criar segundo EnemyActionType enum, segundo EnemyActionSO/EnemyActionSetSO, segundo
  EnemyActionExecution ou segundo EnemyBrain — estender os existentes (aditivo).
NÃO recriar CreateEnemyActionsAndSets — realinhar/estender seu output existente (Fase 0 decide
  a estratégia exata: idempotência via AssetDatabase.LoadAssetAtPath por ActionId/ActionSetId).
NÃO recriar ValidateSpec13EnemyActions do zero se puder ser estendido; se o escopo mudar tanto
  (universo completo vs. 40 IDs hardcoded) que um novo método for mais claro, documentar a
  decisão e considerar deprecar/redirecionar o validator antigo em vez de manter dois validators
  parcialmente sobrepostos rodando em paralelo no menu.
NÃO criar segundo StatusEffectDatabaseSO/EnemyTelegraphProfileDatabaseSO — apenas referenciar.
NÃO criar sistema de hazard-zone paralelo a triggers/overlap já usados no projeto para detectar
  o player em área (auditar Fase 0 o que EnemyProjectileBehaviour/TelegraphedAoE já usam antes
  de escrever detecção nova).
Parâmetros de balance (multiplicadores por arquétipo, %HP de Rise-once, raio de HazardZone,
  distância de Pull) em consts nomeadas ou campos de EnemyActionSO/SO de balance — nunca
  literais soltos no meio de um método (no-magic-balance-values).
```

## Critérios de aceite

### CA-1 Primitivas P2 executáveis e testadas
- RiseOnce, AllyHeal/AllyBuff, HazardZone e Pull existem como lógica pura (métodos estáticos em
  `EnemyActionExecution` ou classe auxiliar no mesmo padrão) e têm EditMode tests cobrindo: (a)
  RiseOnce reerguendo com o %HP configurado, exceto quando o último dano é do elemento bloqueador;
  (b) AllyHeal/AllyBuff selecionando o alvo-aliado correto dentro do raio (mais ferido, ou mais
  próximo se não houver critério de HP aplicável); (c) HazardZone tick/expiração determinístico
  por duração; (d) Pull calculando a posição-alvo deslocada N tiles na direção configurada.
- Evidência: EditMode tests em `Assets/_Game/Tests/EditMode/Cave/` (ou pasta correspondente já
  usada por fable_83) + build 0E.

### CA-2 EnemyHealth.Die() intercepta Rise-once sem quebrar o fluxo de morte existente
- Quando um EnemyHealth com RiseOnce habilitado e ainda não consumido morre por dano de um
  elemento diferente do bloqueador, a morte é interceptada: o inimigo não publica
  `EnemyKilledEvent`/`EnemyKilledByEnemyEvent` imediatamente, entra em estado de colapso por
  2s (parâmetro configurável) e reergue com o %HP configurado, consumindo o Rise-once (não
  reergue uma segunda vez). Quando morre pelo elemento bloqueador, ou já consumiu o Rise-once,
  o fluxo de `Die()` é inalterado (publica o evento normalmente).
- Evidência: EditMode test cobrindo os 3 ramos (reerguer, bloqueado por elemento, já consumido).

### CA-3 Kits gerados para os ~121 donos de kit do universo
- Após rodar `GenerateEnemyAttackKits` (via `Inicializar Projeto`), todo EnemyDataSO das 113
  fichas canônicas materializadas + as 7 novas do Roster + `enemy_meteor_ooze_king` tem
  `ActionSetId` não vazio apontando para um `EnemyActionSetSO` existente com ≥1 ação
  Melee/Ranged/Cast "normal" + ≥1 ação classificada como Especial (conforme tabela §4/§4B do
  catálogo); kits de 3 ações têm `MinRange > 0` na ação ranged e uma ação de melee fallback no
  mesmo set.
- Evidência: log do generator (contagem criado/realinhado/pulado) + validator (CA-5) PASS.

### CA-4 Variâncias apontam para a mãe sem duplicar dados
- Os 53 IDs do Roster classificados como variância (§4A do catálogo) têm
  `EnemyDataSO.ActionSetId` idêntico ao `ActionSetId` da criatura-mãe correspondente; nenhum
  `EnemyActionSO`/`EnemyActionSetSO` novo foi criado para eles.
- Evidência: validator (CA-5) checando o crosswalk completo das 53 linhas da tabela §4A.

### CA-5 Validator read-only PASS
- `ValidateEnemyAttackKits` roda via `Validar Projeto`, não muta nenhum asset, e reporta 0 erros
  para: kit completo (CA-3), fallback+MinRange em kits de 3 (CA-3), variância alinhada (CA-4),
  `StatusApplicationIds` resolvendo no `StatusEffectDatabaseSO`, `TelegraphProfileId` resolvendo
  no `EnemyTelegraphProfileDatabaseSO`, e ação Ranged restrita a criaturas autorizadas pelo
  catálogo. Avisos (não erros) para fichas canônicas sem EnemyDataSO asset materializado.
- Evidência: log do Console + `docs/validation/` execution report citando contagem de
  erros/avisos.

### CA-6 Stable-run e regressão intactos
- Nenhuma mudança de `CaveRunSeed`/spawn/layout; `EnemyContactDamage` (dano de contato) e o
  fluxo de save (HP/estado de inimigo) permanecem inalterados fora do ramo novo de Rise-once
  (que é estado runtime transitório, não persistido — mesmo idioma do `_stunUntil`/"Ferido"
  documentado em `EnemyHealth`). Os 8 EnemyActionType base + os 5 signature de fable_83
  continuam funcionando sem alteração de comportamento.
- Evidência: build 0E + testes de regressão existentes (fable_83/fable_24) continuam passando.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Data/
  EnemyActionSO.cs                       (campos aditivos: RiseOnce*, AllyHeal/Buff*, Hazard*, Pull*)
Assets/_Game/Scripts/Combat/
  EnemyHealth.cs                         (hook em Die() para Rise-once)
Assets/_Game/Scripts/Enemy/
  EnemyActionExecution.cs                (+ métodos puros: RiseOnce, AllyHeal/Buff, HazardZone, Pull)
  EnemyHazardZone.cs                     (NOVO, se necessário — componente fino de overlap; ou
                                           reuso de estrutura existente conforme Fase 0)
  EnemyBrain.cs                          (orquestra timing dos novos usos; mínimo necessário)
Assets/_Game/Scripts/Editor/EnemyTaxonomy/
  GenerateEnemyAttackKits.cs              (NOVO — generator idempotente, SEM [MenuItem])
Assets/_Game/Scripts/Editor/Validation/
  ValidateEnemyAttackKits.cs              (NOVO — validator read-only, SEM [MenuItem])
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs
  (RunStep novo em InicializarProjeto FASE A; RunStep novo em ValidarProjeto)
Assets/_Game/Data/Enemies/Actions/**      (assets gerados/realinhados)
Assets/_Game/Data/Enemies/ActionSets/**   (assets gerados/realinhados)
Assets/_Game/Tests/EditMode/Cave/
  EnemyAttackKitPrimitivesTests.cs        (NOVO — RiseOnce/AllyHeal-Buff/HazardZone/Pull)
docs/validation/
  spec_enemy_attack_kits_v1_execution_report.md
```

## Contratos, dados e eventos

### 16.1 Data contracts
`EnemyActionSO` ganha campos aditivos (save-safe, no fim da classe, seguindo o precedente do
bloco "Signature Attacks (fable_83)"): `RiseOnceEnabled` (bool), `RiseOnceHpPercent` (float 0-1),
`RiseOnceBlockedByDamageTypes` (string[], ex. `{"fire","radiant"}`), `RiseOnceCollapseSeconds`
(float); `AllyTargetRadius` (float), `AllyHealPercent` (float 0-1, 0 = não cura),
`AllyBuffStatusId` (string, reusa StatusEffect ou buff runtime existente); `HazardRadius`,
`HazardDurationSeconds`, `HazardTickSeconds`, `HazardDamagePerTick`, `HazardStatusId`,
`LeavesHazard` (bool); `PullDistanceTiles` (float), `PullFromAttackerOrigin` (bool). Todos com
defaults neutros para não afetar ações existentes. `EnemyActionSetSO` inalterado (kit continua
sendo lista de `ActionIds`).

### 16.2 Runtime contracts
`EnemyActionExecution.ResolveRiseOnce(...)`, `.ShouldBlockRise(lastDamageType, blockedTypes)`,
`.ResolveAllyHealTarget(...)`, `.ResolveHazardTick(...)`, `.ResolvePullTargetPosition(...)` —
todos puros (sem `UnityEngine.Time`, sem `MonoBehaviour`), seguindo o padrão já estabelecido no
arquivo. `EnemyHealth.Die()` ganha um branch condicional mínimo que checa RiseOnce antes de
publicar o evento de morte. Se HazardZone precisar de detecção de overlap em runtime, usar um
componente fino dedicado (`EnemyHazardZone` ou nome equivalente) que só delega a
`EnemyActionExecution` para a lógica de tick — sem duplicar física.

### 16.3 Event contracts
Nenhum evento novo obrigatório. Se o Rise-once precisar sinalizar UI/telemetria de "inimigo se
reergueu", avaliar reuso de eventos de combate existentes antes de criar um novo (aditivo, se
inevitável — ex. `EnemyRoseAgainEvent`), documentando no execution report.

### 16.4 Save contracts
`Does this change save schema? NO` — Rise-once é estado runtime transitório (mesmo idioma do
`_woundedUntil`/`_stunUntil` já documentado em `EnemyHealth`/`EnemyBrain`), não persiste entre
sessões; HazardZone instances são recomputáveis (não sobrevivem a save/load, mesmo espírito de
summons determinísticos de fable_83). `Does this add a save section? NO`. `Does this require
migration? NO`. `Does this persist Unity references? NO` (garantido pelas rules do projeto).

### 16.5 UI contracts
Nenhuma tela nova. Telegraph reusa `EnemyTelegraphController` existente (F04).

## Sistemas afetados

```text
Enemy actions (núcleo, aditivo) | EnemyBrain (execução dos 4 tipos novos de efeito)
EnemyHealth (hook de Rise-once em Die()) | Status effects (consumido por AllyBuff/HazardZone)
Bestiary (CanonicalBestiaryCatalog — leitura, sem alterar as 117 fichas) | Editor tooling
  (generator + validator registrados nos 3 comandos canônicos) | Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs (campos aditivos)
Assets/_Game/Scripts/Combat/EnemyHealth.cs (hook Rise-once em Die(), mínimo)
Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs (+métodos puros das 4 primitivas)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (orquestração mínima dos novos usos)
Assets/_Game/Scripts/Enemy/EnemyHazardZone.cs (novo, se necessário — componente fino)
Assets/_Game/Scripts/Editor/EnemyTaxonomy/GenerateEnemyAttackKits.cs (novo)
Assets/_Game/Scripts/Editor/Validation/ValidateEnemyAttackKits.cs (novo)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs (2 RunStep novos: Inicializar + Validar)
Assets/_Game/Data/Enemies/Actions/** (assets gerados/realinhados via AssetDatabase)
Assets/_Game/Data/Enemies/ActionSets/** (assets gerados/realinhados via AssetDatabase)
Assets/_Game/Tests/EditMode/Cave/** (novo test file)
docs/validation/** ; csproj includes (Assembly-CSharp.csproj / Assembly-CSharp-Editor.csproj)
```

## Arquivos proibidos

```text
*.unity / *.prefab (edição manual)
Packages/** ; ProjectSettings/**
CanonicalBestiaryCatalog*.cs (ler os 117 IDs; NÃO editar a definição das fichas)
EnemyDataSO.cs (ler campos existentes; NÃO adicionar campo novo — ActionSetId já existe)
Assets/_Game/Data/Enemies/Roster/enemy_*.asset e Assets/_Game/Data/Enemies/Canonical/*.asset
  fora de setar ActionSetId via AssetDatabase API (nunca editar YAML manualmente)
boss_vel_karaum / boss_cindrathel / boss_archivist_of_silence / boss_ithryndor (The Four —
  qualquer asset ou script relacionado ao encontro DORMANTE)
StatusEffectDatabaseSO / EnemyTelegraphProfileDatabaseSO (consumir, não alterar a definição)
CX13 physics layers / ContactFilter2D (não tocar — spec paralela travada por lock)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Confirmar wiring atual de CreateEnemyActionsAndSets/GenerateAndWireSpec13GAssets no menu;
decidir estratégia de realinhamento in-place vs. novo naming para os 60 assets já existentes;
confirmar quais das 113 fichas canônicas têm EnemyDataSO materializado (Assets/_Game/Data/
Enemies/Canonical/); auditar padrão de overlap/trigger já usado por TelegraphedAoE para reusar
na detecção de HazardZone; ler EnemyHealth.Die() por inteiro para desenhar o ponto de
interceptação do Rise-once sem quebrar o caminho de EnemyKilledByEnemyEvent (kill inter-monstro).

### Fase 1 — Primitivas P2 (Slice 1)
EnemyActionSO += campos aditivos das 4 primitivas. EnemyActionExecution += métodos puros
(RiseOnce, AllyHeal/Buff, HazardZone tick, Pull). EnemyHealth.Die() += hook condicional de
Rise-once. EnemyBrain += orquestração mínima (chamar os métodos puros, aplicar resultado).
EditMode tests dos 4 (CA-1, CA-2).

### Fase 2 — Generator de kits (Slice 2)
GenerateEnemyAttackKits: monta o kit por criatura a partir das tabelas §4/§4B do catálogo
(dados embutidos no generator, no padrão de CreateEnemyActionsAndSets — não é preciso parsear
o Markdown em runtime); deriva BaseDamage/Windup/Cooldown por arquétipo com multiplicadores
nomeados; cria/realinha EnemyActionSO+EnemyActionSetSO idempotentemente; seta ActionSetId nas
113 fichas + 7 novas + meteor_ooze_king; wireia as 53 variâncias para a mãe. RunStep registrado
em CindarsHopeMenu.InicializarProjeto (FASE A, após bestiário/movement profiles).

### Fase 3 — Validator (Slice 3)
ValidateEnemyAttackKits: as checagens de CA-5. RunStep registrado em
CindarsHopeMenu.ValidarProjeto.

### Fase 4 — Validação e relatório
dotnet build (2 csproj) + EditMode tests + rodar o generator/validator no Unity Editor (humano,
com evidência de log conforme skill unity-asset-generation) + execution report.
```

## Ordem segura de execução

```text
1. Auditar (Fase 0) sem alterar nada.
2. Implementar as 4 primitivas + EditMode tests (Slice 1), build 0E.
3. Implementar o generator (Slice 2), registrar RunStep, build 0E.
4. Implementar o validator (Slice 3), registrar RunStep, build 0E.
5. Rodar o generator no Unity Editor (humano) e capturar log/evidência.
6. Rodar o validator no Unity Editor (humano) e capturar log/evidência (0 erros).
7. Rodar EditMode tests via Unity Test Runner.
8. Registrar execution report com todas as evidências.
```

## Paralelização

- Parallelizable: NO — lock central em `EnemyBrain.cs`, `EnemyActionSO.cs`,
  `EnemyActionExecution.cs`, `CindarsHopeMenu.cs`, e nas pastas de asset
  `Assets/_Game/Data/Enemies/Actions/**` / `ActionSets/**`.
- Must not run with: qualquer spec que edite os mesmos arquivos (fable_83 follow-ups, fable_24
  follow-ups, fable_05, CX13, CV01) ou que gere assets na mesma pasta.
- Reason: edição concorrente do action database/brain e do menu de geração causaria conflito
  direto e risco de assets duplicados/corrompidos.

## Impacto em save/load

```text
Changes save schema? NO
Adds save section? NO
Requires migration? NO
Persists Unity references? NO (garantido; Rise-once/HazardZone são estado runtime transitório)
```

## Impacto em eventos

```text
Adds events: CONDITIONAL (avaliar na Fase 1 se um EnemyRoseAgainEvent aditivo é necessário;
  documentar decisão no execution report)
Changes existing events: NO
Requires unsubscribe pattern: YES (se evento novo for adicionado)
```

## Impacto em UI/Unity

```text
Changes UI: NO (telegraph reusa F04)
Changes scenes: NO
Changes prefabs: NO (a menos que HazardZone exija um prefab fino de overlap — se sim, documentar
  e tratar como asset explícito, com evidência de geração)
Changes ScriptableObjects/assets: YES — EnemyActionSO/EnemyActionSetSO (~121 kits) + ActionSetId
  em EnemyDataSO existentes (via editor API, evidência obrigatória)
Requires Play Mode final validation: YES (sentir Rise-once/AllyHeal/HazardZone/Pull em pelo
  menos 1 criatura por primitiva)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: Rise-once interferir no caminho de EnemyKilledByEnemyEvent (kill inter-monstro) e quebrar
  o loot reduzido do ecossistema (fable_78).
Mitigação: Rise-once só intercepta quando _pendingEnemyKillerInstanceId é null (kill do player);
  kill inter-monstro sempre segue o fluxo normal. Teste de regressão explícito.

Risco: gerar ~121 EnemyActionSO/EnemyActionSetSO duplicando os 60 já existentes se a Fase 0 não
  decidir corretamente a estratégia de idempotência.
Mitigação: Fase 0 obrigatória decide e documenta realinhamento in-place por ActionId/ActionSetId
  existente antes de qualquer criação; generator sempre checa AssetDatabase.LoadAssetAtPath
  primeiro (padrão já usado por CreateEnemyActionsAndSets).

Risco: HazardZone introduzir alocação/overlap caro em hot path (Update) se implementado sem
  cuidado.
Mitigação: reusar padrão de overlap/trigger já existente no projeto (auditar Fase 0); tick de
  hazard não por-frame arbitrário, e sim por intervalo configurado (HazardTickSeconds).

Risco: derivar dano/windup por arquétipo com multiplicadores mal calibrados gera kits
  desbalanceados no universo inteiro de uma vez.
Mitigação: escopo explícito diz que balance final é fora de escopo; generator usa
  multiplicadores nomeados documentados no próprio arquivo, revisáveis/ajustáveis depois sem
  re-desenhar a arquitetura.

Risco: validator com crosswalk das 53 variâncias duplicado (uma cópia no generator, outra no
  validator) diverge com o tempo.
Mitigação: crosswalk mantido em uma única fonte de dados (ex. lista compartilhada/const no
  generator, referenciada pelo validator, ou um asset intermediário) — decisão de Fase 2,
  documentada.
```

## Rollback

```text
Remover GenerateEnemyAttackKits.cs e ValidateEnemyAttackKits.cs e os 2 RunStep em
  CindarsHopeMenu.cs reverte a geração/validação sem afetar os 60 assets pré-existentes gerados
  por CreateEnemyActionsAndSets (se a Fase 0 decidir por realinhamento in-place, documentar
  também como reverter os campos alterados — idealmente via git diff dos .asset).
Campos aditivos em EnemyActionSO são save-safe (defaults neutros) — reverter o código não quebra
  assets já gerados, apenas o generator para de rodar.
Hook de Rise-once em EnemyHealth.Die() é condicional (RiseOnceEnabled default false) — reverter
  o código volta ao comportamento de morte padrão em 100% dos casos.
Não apagar EnemyDataSO/EnemyActionSO/EnemyActionSetSO existentes sem necessidade — preferir
  reverter só o código novo.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar wiring do menu (CreateEnemyActionsAndSets/GenerateAndWireSpec13GAssets),
      decidir estratégia de idempotência dos 60 assets existentes, mapear EnemyDataSO canônicos
      materializados, auditar padrão de overlap/trigger reusável para HazardZone, ler EnemyHealth.Die()
      por inteiro.
- [ ] T002 — Slice 1: EnemyActionSO += campos das 4 primitivas; EnemyActionExecution += métodos
      puros (RiseOnce/AllyHeal-Buff/HazardZone/Pull); EnemyHealth.Die() += hook Rise-once;
      EnemyBrain += orquestração mínima. EditMode tests dos 4. Build 0E.
- [ ] T003 — Slice 2: GenerateEnemyAttackKits (dados do kit por criatura embutidos no generator a
      partir do catálogo §4/§4B; derivação de dano/windup/cooldown por arquétipo com
      multiplicadores nomeados; idempotência via AssetDatabase; wiring das 53 variâncias; kit dos
      ~121 donos incl. bosses por fase). RunStep em CindarsHopeMenu.InicializarProjeto. Build 0E.
- [ ] T004 — Slice 3: ValidateEnemyAttackKits (checagens CA-5). RunStep em
      CindarsHopeMenu.ValidarProjeto. Build 0E.
- [ ] T005 — Rodar generator + validator no Unity Editor (humano), capturar evidência de log;
      rodar EditMode tests via Unity Test Runner; strict validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

Unity compile + geração de asset (execução humana, evidência obrigatória conforme skill
`unity-asset-generation` e rule `unity-assets`):

```text
Unity Editor > CindarsHope/Inicializar Projeto (log do RunStep "Gerar kits de ataque de inimigo")
Unity Editor > CindarsHope/Validar Projeto (log do RunStep "Validar kits de ataque de inimigo" — 0 erros)
Unity Test Runner — EditMode (EnemyAttackKitPrimitivesTests + regressão fable_83/fable_24)
```

Se algum comando não puder rodar (Unity ocupado, sandbox, licença), o report registra `NOT RUN`
com motivo e risco residual — nunca infere PASS.

## Testing Quality Gate

- Changed deterministic logic: YES (RiseOnce/AllyHeal-Buff/HazardZone/Pull são lógica pura
  determinística; derivação de kit por arquétipo também é determinística a partir dos dados)
- Requires EditMode tests: YES (as 4 primitivas + pelo menos 1 caso de derivação de kit por
  arquétipo, se praticável sem exigir todo o universo coberto por teste)
- Requires PlayMode automated or final human scenario: YES (sentir Rise-once/AllyHeal/HazardZone/
  Pull em pelo menos 1 criatura cada; ver 1 variância herdando corretamente o kit da mãe)
- Requires regression test: YES (fable_83 signature actions e fable_24 moves/elite affixes
  continuam funcionando sem alteração de comportamento; replay/stable-run da cave intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: EditMode tests das 4 primitivas + build 0E + log do
  generator (contagem criado/realinhado/pulado) + log do validator (0 erros) + cenário humano
  final documentado (combate contra 1 criatura por primitiva + 1 variância)

## Definition of Done

```text
4 primitivas P2 (RiseOnce, AllyHeal/AllyBuff, HazardZone, Pull) implementadas em C# puro,
testadas em EditMode, sem magic values inline, sem GameObject.Find, comunicação via
GameEventBus quando aplicável.
Kits de ataque gerados/realinhados para os ~121 donos de kit do universo (113 canônicas + 7
novas do Roster + enemy_meteor_ooze_king), com o padrão de kit de 2-3 ações do catálogo v1.1.
53 variâncias do Roster apontando ActionSetId para a criatura-mãe, sem duplicar dados.
Validator read-only cobrindo o universo completo, registrado em Validar Projeto, 0 erros.
Generator registrado em Inicializar Projeto (FASE A, após bestiário/movement profiles).
The Four (boss_*) permanecem sem ActionSetId (DORMANTE) — nenhuma mudança nas 4 fichas.
Builds 0E (Assembly-CSharp e Assembly-CSharp-Editor); strict validation exit 0.
Execution report com todas as evidências (build, testes, logs de generator/validator, cenário
humano documentado como DEFERRED_TO_FINAL_VALIDATION se não executado nesta sessão).
Sem claim de ACCEPTED sem evidência.
```

## Anti-regressão

```text
Não alterar EnemyContactDamage nem o cálculo de dano existente (DamageCalculator).
Não alterar comportamento dos 8 EnemyActionType base nem dos 5 signature de fable_83.
Não mudar CaveRunSeed, spawn plan, layout ou qualquer conteúdo determinístico da cave (rule
  cave-stable-run) — as 4 primitivas não introduzem GUID/timestamp em nenhum ponto.
Não persistir referência Unity em nenhum campo/estado das 4 primitivas (rule unity-architecture).
Não usar GameObject.Find/FindObjectOfType em nenhum código novo.
Não editar YAML de .asset manualmente — toda mudança de EnemyActionSO/EnemyActionSetSO/
  EnemyDataSO.ActionSetId via AssetDatabase/SerializedObject.
Não tocar nas 4 fichas boss_* do The Four nem introduzir ActionSetId nelas.
Não duplicar os 60 EnemyActionSO/EnemyActionSetSO já existentes (idempotência obrigatória).
Fluxo de EnemyKilledByEnemyEvent (kill inter-monstro, fable_78) permanece 100% inalterado —
  Rise-once só se aplica ao caminho de kill normal (pelo player).
```

## Notas para execução posterior

```text
Esta spec não implementa EnemyAttackAnimator nem qualquer sprite/folha de ataque — é puramente
mecânica/dados. A skill nova citada no catálogo (§7.2, "enemy-attack-animation") e a spec de
apresentação visual ficam para depois desta.
Esta spec não implementa as primitivas P3/P4 do catálogo §5 (morte volátil como especial de
espécie, decoy summon, alerta de pack como ação, quebra-guarda, silence, riposte, reflect,
execute condicional, twin-link, interações de ambiente) — follow-up natural depois que as P2
estiverem validadas em Play Mode.
Esta spec não corrige os bindings de arte marcados com † no catálogo §4A (duergar_*,
orc_kaand_berserker, frost_wailer) nem cria fichas novas no CanonicalBestiaryCatalog para as 7
NOVAS do Roster — follow-ups documentados separadamente no catálogo §7.
Esta spec não executa validação humana imediata — cria o cenário e marca
DEFERRED_TO_FINAL_VALIDATION, a ser consolidado no lote/batch conforme decisão do usuário.
Esta spec deve ser revisada antes de execução em paralelo com qualquer spec que edite
EnemyBrain.cs, EnemyActionSO.cs, EnemyActionExecution.cs, CindarsHopeMenu.cs, ou que gere
assets em Assets/_Game/Data/Enemies/Actions|ActionSets/**.
```
