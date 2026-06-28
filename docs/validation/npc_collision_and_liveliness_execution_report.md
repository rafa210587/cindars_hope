# Execution Report — NPCs respeitam barreiras + movimentação viva

**Data:** 2026-06-23
**Branch:** dev
**Status:** `BUILD_VALIDATED_WITH_WARNINGS` (runtime + editor compilam exit 0; comportamento de
física/movimento/porta depende de Play Mode — DEFERRED, com cenário humano escrito)

---

## Pedido

> "os npcs n respeitam as barreiras das casas, eles precisam respeitar, abrir portas se forem entrar
> etc. Os npcs q n se movimentam, tem motivo pra n se movimentar? todo npc deveria ter uma
> movimentação (mesmo q pequena, 2 tiles pra um lado 2 pra outro aleatoriamente a cada X tempo)...
> os q podem se movimentar mais poderiam andar mais, se mover pela cidade."

## Causa raiz (antes)

- **Colisão:** o único collider do NPC era um **trigger** (interação) → mesmo os que andavam
  **atravessavam paredes**; ao ir dormir, o NPC encostava na parede e **teleportava atravessando**
  após 5 s (stuck fallback). Não usavam a porta.
- **Movimento:** lojistas (`CreateShopNpc`) **não recebiam Rigidbody nem NpcWanderer** → 100%
  estáticos. Só NPCs de diálogo com `canWander=true` se moviam.

## Decisão de abordagem

A separação limpa "NPC sólido vs. parede, mas jogador atravessa NPC" normalmente exige uma layer de
física + matriz de colisão (**ProjectSettings**, que exige autorização). Em vez disso, usei
`Physics2D.IgnoreCollision(colliderDoNpc, colliderDoPlayer)` em runtime, com a ref do collider do
jogador **injetada na geração** (sem busca global) — entrega o mesmo resultado **sem tocar em
ProjectSettings**. (A pergunta ao usuário foi dispensada; segui este caminho que não exige a decisão.)

## Mudanças

### Runtime (`Assembly-CSharp`)
- **`NpcWanderer.cs` (reescrito):** config de movimento própria (não depende mais de
  `NpcDataSO.MovementMode/WanderData`). Idle-wander universal (vaivém curto aleatório em torno do
  ponto, raio/velocidade/pausa configuráveis + clamp do playfield). Override de destino de agenda com
  **waypoint opcional (porta)** → alvo final. Auto-pausa na conversa via `GameEventBus`
  (`NpcInteractionStarted/Ended`). `ConfigureMovement(...)` para o gerador setar o tier.
- **`NpcPhysicsBody.cs` (novo):** em `OnEnable`, `Physics2D.IgnoreCollision(solid, player)` → NPC
  sólido contra cenário, jogador atravessa NPC. Refs injetadas (sem busca global).
- **`NpcDweller.cs` (novo):** marcador de morador; a porta usa para distinguir NPC do jogador.
- **`NpcScheduleAnchor.cs`:** ponto de **aproximação** opcional (o vão da porta).
- **`NpcScheduleService.cs`:** ao rotear para um anchor com aproximação, passa o waypoint da porta ao
  wanderer (`SetDestination(target, waypoint, radius)`).
- **`HouseDoorInteractable.cs`:** **auto-abre** quando um `NpcDweller` entra no trigger e **fecha** um
  instante após ele sair (`OnTriggerEnter/Exit2D` + timer). Controle manual do jogador (E) intacto.

### Geração de cena (`Assembly-CSharp-Editor`)
- **`CreateMvpTownScene.cs`:**
  - `ConfigureNpcMovement(...)` aplicado a **TODOS** os NPCs (shop + diálogo): Rigidbody2D dinâmico +
    collider sólido pequeno nos pés + `NpcPhysicsBody` (ignora player) + `NpcDweller` + `NpcWanderer`
    por tier. Substitui o antigo `AddNpcSolidBody` (no-op) e o bloco gated por `canWander`.
  - `MovementTierFor(profile)`: **3 tiers** — Idle/micro (lojista/estático ~2 tiles), Local
    (Patrol/WanderWithinZone/NightOnly), Roam (Peregrino, raio ~8, cobre a cidade).
  - `TryResolveHomeDoorApproach(spec)`: ponto fora da porta do morador indoor; gravado no anchor
    "home" via `CreateScheduleAnchor(..., hasApproach, approachPoint)`.
  - `CreateNpcs` injeta o collider do jogador (`playerTransform.GetComponent<Collider2D>()`).
- **`Assembly-CSharp.csproj`:** +`NpcDweller.cs`, +`NpcPhysicsBody.cs`.

## Validação

```text
Validation method: dotnet build por assembly (headless; Unity Editor indisponível)
Assembly-CSharp:        PASS (exit 0, 1 warning pré-existente)
Assembly-CSharp-Editor: PASS (exit 0, 3 warnings pré-existentes)
```

### Testing Quality Gate
```text
Changed runtime code:           YES (wanderer, schedule, door, 2 componentes novos)
Changed deterministic logic:    NO (movimento/física são frame/scene-based)
Changed Unity scene/prefab:     NO (gerador programático; cena materializa no Editor)
Automated tests added/updated:  NO
Automated tests command:        N/A
Manual Play Mode scenario:      docs/validation/playmode/npc_collision_and_liveliness_human_test_scenario.md
Justification if no tests:      comportamento depende de Rigidbody2D/colisão/trigger/lifecycle — fora de EditMode; coberto por cenário humano
Residual risk:                  (1) NPCs sólidos podem acotovelar em aglomerados; (2) entrada pela porta depende de alinhamento com o vão — há fallback de teleporte anti-travamento; (3) separação player↔NPC via IgnoreCollision (não via layer) — confirmar no Play Mode. Cena só materializa após `CindarsHope/Inicializar Projeto`.
```

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: o código compila exit 0 e implementa os três pedidos (respeitar
paredes, usar a porta ao entrar, movimentação viva por tier para todos). Não promovo além disso
porque é um sistema de física/movimento/cena que só valida de fato em Play Mode (indisponível aqui) —
reportado como DEFERRED com cenário humano, nunca convertido em PASS.

## Trabalho restante / futuro
- Rodar `CindarsHope/Inicializar Projeto` + o cenário humano acima.
- Possível refino: tier "Roam" circulando entre hubs nomeados (hoje é um raio grande) e suavizar o
  re-âncora horário do schedule para não puxar o NPC de volta ao posto a cada hora.
