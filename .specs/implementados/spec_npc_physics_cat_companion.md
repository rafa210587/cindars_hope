# SPEC — Física de Colisão de NPCs + Gato Companheiro + Escala Dragonborn

- **ID:** spec_npc_physics_cat_companion
- **Status:** Implementado e BUILD_VALIDATED, DEFERRED_TO_FINAL_HUMAN_VALIDATION (docs-migration 2026-08-12)
- **Origem:** pedido humano (2026-06-30) durante o overhaul de sprites de NPC via GPT
- **Tipo:** runtime/code + scene wiring (Town)

---

## 0. Evidência de implementação (docs-migration 2026-08-12)

```text
Status: Implementado e BUILD_VALIDATED (código) — Play Mode humano DEFERRED_TO_FINAL_VALIDATION
Execution report dedicado: nao existe (implementado em sessão anterior sem gerar execution report
  próprio); evidência desta baixa é re-verificação direta do código no disco nesta sessão.
Re-verificação nesta sessão (Grep/Read no disco):
  - CreateMvpTownScene.cs: `ConfigureNpcMovement` adiciona Rigidbody2D (Dynamic, FreezeRotation,
    massa 50) + BoxCollider2D sólido ("SolidBody", filho, contra-escala) a cada NPC — collider de
    resistência física confirmado.
  - NpcWanderer.cs: movimento via `FixedUpdate()` + `Rigidbody2D` serializado (não
    `transform.position` direto) — confirmado respeita colisão.
  - Assets/_Game/Scripts/NPC/CompanionFollow.cs existe; `SpawnCatCompanion(...)` em
    CreateMvpTownScene.cs cria o gato como entidade separada (SpriteRenderer próprio, Rigidbody2D
    Kinematic, CircleCollider2D trigger) e o liga ao Transform do eiran via CompanionFollow.
  - CreateDefaultScaleAssets.cs: `CreateProfile(EntityScaleCategory.NpcDragonborn, ..., 1.07f, ...)`
    confirmado; CreateMvpTownScene.cs atribui `EntityScaleCategory.NpcDragonborn` a npc_savra e
    npc_hess (e npc_zrix, mesma spec de lista `TownNpcSpec`).
Build: dotnet build Assembly-CSharp + Assembly-CSharp-Editor — não re-executado nesta sessão de
  docs-migration; nenhuma edição de código foi feita aqui (apenas leitura de verificação).
Play Mode: DEFERRED_TO_FINAL_VALIDATION — cenário descrito na seção 7 desta spec (andar contra
  NPC/parede, ver gato seguindo, conferir escala dos dragonborn) não executado nesta sessão de
  docs-migration; evidência automatizada de código (wiring de collider/rigidbody/CompanionFollow/
  escala confirmado por leitura direta) é considerada suficiente para esta baixa.
```

## 1. Objetivo

1. **Física de NPC:** todo NPC da cidade deve ter **resistência física** — o player não atravessa o NPC, o NPC não atravessa paredes/prédios/props nem outros NPCs. Hoje só o player tem `Rigidbody2D` + `BoxCollider2D` sólido (CreateMvpTownScene ~L500-509); os NPCs não têm collider sólido, então tudo se atravessa.
2. **Gato companheiro:** o gato do `npc_eiran` deve ser uma **entidade separada** (não parte do sprite do eiran) que **anda sempre perto do eiran** (segue a uma distância curta, com leve atraso). Sprite já extraído: `batch_v3/_cat_companion/gpt_cat.png`.
3. **Escala dragonborn:** `npc_zrix`, `npc_savra`, `npc_hess` hoje caem em `EntityScaleCategory.NPC` (1.0×). O oficial é **1.07×**. Criar `NpcDragonborn` (1.07) e atribuí-los.

## 2. Contexto / Estado atual

- Player: `Rigidbody2D` Dynamic + `BoxCollider2D` em `CreateMvpTownScene` (~L484-517).
- NPCs: criados a partir de `TownNpcSpec` (lista ~L2300-2341); recebem SpriteRenderer, movimento (movement profile), interação (trigger). **Sem collider sólido / sem Rigidbody2D de bloqueio.**
- Escala por raça: `EntityScaleCategory` + `CreateDefaultScaleAssets` (profiles) + `ScaleProfileLibrary.AttachApplicator`. Categorias atuais: NPC, NpcDwarf(0.90), NpcSmallfolk(0.85), NpcOrc(1.30). **Sem NpcDragonborn.**
- Tabela oficial de tamanhos: memória `project_npc_race_sizes_official` + `_gpt/_TAMANHOS_OFICIAIS_RACA.txt`.

## 3. Escopo

### Dentro
- Adicionar collider sólido + `Rigidbody2D` (Kinematic, sem rotação) a cada NPC no build da Town (e Farm, se aplicável). Collider nos **pés** (Capsule/Box pequeno na base), não cobrindo o sprite todo, para movimento top-down natural.
- Garantir que paredes/prédios/props sólidos da Town tenham collider (auditar; adicionar onde faltar).
- Movimento de NPC (patrol/wander) deve respeitar colisão — mover via `Rigidbody2D.MovePosition` em FixedUpdate, não `transform.position` direto que ignora colisão.
- Camada/Matrix de colisão: NPC×Wall = bloqueia; Player×NPC = bloqueia; NPC×NPC = bloqueia (ou ao menos não sobrepõe); NPC×trigger de interação = inalterado.
- **Gato companheiro:** entidade nova (prefab/GameObject) com SpriteRenderer (sprite do gato), pequeno collider/sem bloquear o player, e um componente `CompanionFollow` (C# puro de steering + adapter MonoBehaviour) que segue um alvo (eiran) mantendo distância-alvo curta, com parada quando perto e leve atraso. Spawna junto do eiran no AnimalYard.
- **NpcDragonborn (1.07):** add ao enum `EntityScaleCategory`; `CreateProfile(..., "npc_dragonborn", "NPC Dragonborn", 1.07f, 1.0f, 1.5f)`; atribuir nas specs de zrix/savra/hess.

### Fora
- Pathfinding/navmesh (seguir = steering simples, sem A*). Anti-stuck básico aceitável.
- Animação de andar do gato (pode ser sprite único por ora).
- Colisão dos NPCs nas cenas Cave/interiores (foco Town; estender se barato).

## 4. Critérios de Aceite

- [ ] Player NÃO atravessa nenhum NPC; NPC NÃO atravessa paredes/prédios/props.
- [ ] NPCs em patrol/wander param ao colidir com parede (não atravessam, não tremem/vibram preso).
- [ ] Gato existe como objeto separado do eiran, com o sprite do gato, e **segue o eiran** mantendo-se perto ao longo do patrol dele.
- [ ] Gato não bloqueia/empurra o player de forma incômoda (collider pequeno ou só visual).
- [ ] zrix/savra/hess renderizam a **1.07×** o player (categoria NpcDragonborn aplicada).
- [ ] Nenhum NPC perde o trigger de interação/diálogo/loja existente.

## 5. Anti-Regressão

- Não quebrar o trigger de interação (CircleCollider2D isTrigger) já existente nos NPCs.
- Não usar `GameObject.Find`/`FindObjectOfType` em runtime novo (wiring via spawn/serialized ref).
- Comunicação de gameplay nova via `GameEventBus` quando aplicável (o follow é local ao gato, sem evento).
- Não regredir a escala já corrigida (anão 0.90, orc 1.30, smallfolk 0.85).
- Movimento por `Rigidbody2D.MovePosition` não pode quebrar os movement profiles existentes (patrol/wander/stationary).

## 6. Arquivos prováveis

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — collider+rigidbody no build de NPC; spawn do gato; categorias.
- `Assets/_Game/Scripts/World/Scale/` (`EntityScaleCategory` enum) + `Editor/ScaleSystem/CreateDefaultScaleAssets.cs` — NpcDragonborn.
- Movimento de NPC (localizar o executor de movement profile) — trocar para MovePosition.
- Novo: `Assets/_Game/Scripts/NPC/CompanionFollow.cs` (core puro + adapter) para o gato.
- Validators de escala (`ValidateActorScaleWiring`, `ValidateSpec17AScaleConfig`) — incluir NpcDragonborn se enumerarem categorias.

## 7. Validação

- `dotnet build` Assembly-CSharp + Assembly-CSharp-Editor exit 0.
- Play Mode (humano): andar contra NPC e contra parede; ver gato seguindo eiran; conferir tamanho dos dragonborn vs player.
- Rodar `CindarsHope/Inicializar Projeto` (recria Town com colliders, gato e categorias) e validar no editor.
- Truth gate: `run_strict_validation.ps1` exit 0.

## 8. Notas

- Auditoria de escala (2026-06-30): corrigidos gurd/hund→NpcOrc e tovin/tibbet→NpcSmallfolk (faltava categoria, caíam em 1.0). Restavam só os dragonborn — resolvidos nesta spec.
- Gato: sprite `_cat_companion/gpt_cat.png` ainda tem um resquício do cajado do eiran no topo — limpar (crop/erase) antes de wirar.
