# SPEC 13D — EnemyBrain Runtime MVP
## Validation Record — 2026-05-27

---

## 1. Resumo do implementado

SPEC 13D implementa o runtime MVP do `EnemyBrain` como state machine data-driven completa. Extende o `EnemyBrain` existente (esqueleto de SPEC 13A) com resolução de action sets, execução data-driven de `EnemyActionSO`, integração com `DamageCalculator`, telegraph via `EnemyTelegraphController`, e janelas de vulnerabilidade via `EnemyVulnerabilityState`. Cria três databases ScriptableObject para lookup em runtime e dois eventos de vulnerabilidade.

---

## 2. Arquivos alterados

### Scripts (runtime)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` | **REESCRITO** — state machine data-driven completa; substitui skeleton sem ação data-driven por implementação MVP completa |
| `Assets/_Game/Scripts/Enemy/EnemyActionRuntime.cs` | **CRIADO** — rastreamento de cooldown por ação (plain class, não MonoBehaviour) |
| `Assets/_Game/Scripts/Enemy/EnemyVulnerabilityState.cs` | **CRIADO** — MonoBehaviour para gerenciar janela de vulnerabilidade; publica `EnemyVulnerabilityStartedEvent`/`EnemyVulnerabilityEndedEvent` |
| `Assets/_Game/Scripts/Combat/Data/EnemyActionDatabaseSO.cs` | **CRIADO** — `DataRegistrySO<EnemyActionSO>` para lookup de ações por ID em runtime |
| `Assets/_Game/Scripts/Combat/Data/EnemyActionSetDatabaseSO.cs` | **CRIADO** — `DataRegistrySO<EnemyActionSetSO>` para lookup de action sets por ID |
| `Assets/_Game/Scripts/Combat/Data/EnemyTelegraphProfileDatabaseSO.cs` | **CRIADO** — `DataRegistrySO<EnemyTelegraphProfileSO>` para lookup de telegraph profiles |
| `Assets/_Game/Scripts/Core/Events/EnemyEvents.cs` | **MODIFICADO** — adicionados `EnemyVulnerabilityStartedEvent` e `EnemyVulnerabilityEndedEvent` |

### Scripts (editor)

| Arquivo | Alteração |
|---|---|
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyBrainRuntime.cs` | **CRIADO** — menu `CindarsHope > Validation > Validate SPEC 13D - EnemyBrain Runtime` |

### Csproj

| Arquivo | Alteração |
|---|---|
| `Assembly-CSharp.csproj` | Adicionadas 5 entradas `Compile Include` (3 databases + EnemyActionRuntime + EnemyVulnerabilityState) |
| `Assembly-CSharp-Editor.csproj` | Adicionada 1 entrada `Compile Include` (ValidateSpec13EnemyBrainRuntime) |

---

## 3. EnemyBrain — State Machine

### Estados suportados

| Estado | Comportamento |
|---|---|
| `Idle` | Transição imediata para Alert/Patrol no primeiro tick |
| `Patrol` | Movimento errado periódico; transiciona para Chase ao detectar player |
| `Alert` | Sem movimento; transiciona para Chase/Patrol baseado em detecção |
| `Chase` | Perseguição ativa; seleciona melhor ação quando em range |
| `Kite` | Mantém distância preferida; usado por KiteRanged/CasterKeepAway |
| `GuardHold` | Estático; usado por GuardStationary |
| `AttackWindup` | Executa contagem regressiva de windup; exibe telegraph |
| `AttackRecover` | Executa contagem regressiva de recover; abre janela de vulnerabilidade |
| `Stunned` | Estado bloqueado externo (sem transição automática) |
| `Dead` | Terminal; sem processamento de Update |

### Tipos de movimento suportados (via EnemyMovementType)

| MovementType | Chase State | Comportamento |
|---|---|---|
| GroundChase | Chase | Movimento direto ao player |
| TankSlowPush | Chase | Igual GroundChase mas velocidade limitada a 1.5 |
| SwarmErratic | Chase | Direção ao player + offset randômico; atualiza a cada 0.15–0.5s |
| KiteRanged | Kite | Mantém distância preferida; recua se muito próximo |
| CasterKeepAway | Kite | Mesma lógica de Kite |
| GuardStationary | GuardHold | Para completamente; ataca quando em range |
| BurrowAmbush / PhaseShortBlink / Leaper | Chase | Fallback para GroundChase (mecanismos específicos são SPEC 13E/F) |

---

## 4. Action Resolution

Lógica de seleção (`SelectBestAction`):
1. Itera `EnemyActionSetSO.ActionIds` em ordem
2. Verifica lookup em `EnemyActionDatabaseSO` (por string ID)
3. Verifica cooldown via `EnemyActionRuntime.IsReady(Time.time)`
4. Para `SelfBuff`: sempre válido quando cooldown pronto
5. Para demais tipos: `dist >= action.MinRange && dist <= action.Range`
6. Retorna primeira ação válida encontrada

Ciclo de execução:
- `BeginAction` → publica `EnemyActionStartedEvent` + `EnemyTelegraphStartedEvent` + inicia telegraph
- `WindupSeconds` → contagem regressiva em `Update`
- Resolve: calcula dano via `DamageCalculator.Calculate()` → publica `PlayerHitEvent`
- `RecoverSeconds` → contagem regressiva
- Pós-recover: marca cooldown, publica `EnemyActionResolvedEvent`

---

## 5. Integração com DamageCalculator

```
DamageRequest(
    targetId: "player",
    baseDamage: action.BaseDamage,
    damageType: [parse DamageType enum de action.DamageType string],
    sourceId: enemyData.enemyId
)
SourcePosition = transform.position
KnockbackForce = enemyData.contactKnockbackForce
CanTriggerVulnerability = false  // enemy attacks não abrem vulnerabilidade do player

result = DamageCalculator.Calculate(request, enemyData.defense)
→ GameEventBus.Publish(new PlayerHitEvent(result.FinalDamage))
```

---

## 6. Janelas de Vulnerabilidade

Trigger mapping em `TickActionTimers`:

| Fase | Triggers verificados |
|---|---|
| Windup resolve | `DuringChargeWindup`, `AfterCast`, `AfterProjectileVolley` |
| Recover complete | `AfterAttackRecover` |

`AlwaysForTest` → abre janela sempre ao fim de `AfterAttackRecover`.

`EnemyVulnerabilityState.OpenWindow` guarda cooldown para evitar abertura dupla.

---

## 7. Eventos publicados (SPEC 13D)

| Evento | Quando |
|---|---|
| `EnemyActionStartedEvent` | Ao iniciar windup |
| `EnemyTelegraphStartedEvent` | Ao iniciar windup |
| `EnemyActionResolvedEvent` | Ao finalizar recover |
| `PlayerHitEvent` | Na resolução do ataque (dano ao player) |
| `EnemyVulnerabilityStartedEvent` | Ao abrir janela de vulnerabilidade |
| `EnemyVulnerabilityEndedEvent` | Ao fechar janela de vulnerabilidade |

---

## 8. Risco residual — Roster 13B vs 13C

Os 8 inimigos testáveis referenciados pelo SPEC 13D usam IDs canônicos:
- `enemy_stone_rat`, `enemy_blackroot_sprout`, `enemy_kobold_scout`, etc.

Estes IDs existem nos `EnemyActionSetSO` (gerados por SPEC 13C), mas os `EnemyDataSO` canônicos ainda não existem como assets Unity (exceto `enemy_cave_mite.asset` e `enemy_meteor_ooze_king.asset`). O validator de SPEC 13D emite WARN (não ERROR) para os 8 testáveis ausentes.

**Reconciliação necessária antes de testes in-Editor**: criar `EnemyDataSO` canônicos e setar `ActionSetId` correspondente.

---

## 9. Validações executadas

| Validação | Resultado |
|---|---|
| `dotnet build Assembly-CSharp.csproj` | 0 erros, 0 avisos |
| `dotnet build Assembly-CSharp-Editor.csproj` | 0 erros, 3 avisos pré-existentes (SPEC 13C CS0649/CS0219) |
| `tools/docs/validate_docs.ps1` | PASSED |

---

## 10. Validações não executadas

| Validação | Motivo |
|---|---|
| `CindarsHope > Validation > Validate SPEC 13D - EnemyBrain Runtime` | Requer Unity Editor |
| `CindarsHope > SPEC 13 > Create Enemy Actions and Sets` | Assets ainda não gerados — requer Unity Editor |
| Play Mode — 8 inimigos testáveis | Requer prefabs configurados + databases wired no Inspector |
| `tools/unity/RunUnityCompileValidation.ps1` | Requer Unity Editor aberto |

---

## 11. Confirmação de escopo

| Item | Status |
|---|---|
| SPEC 13A — taxonomy, profiles, contracts | **FECHADO** |
| SPEC 13B — Roster 40 EnemyDataSO | **FECHADO em código** (roster alternativo — reconciliação pendente) |
| SPEC 13C — EnemyActionSO/action sets | **FECHADO em código** — assets gerados no Unity Editor |
| SPEC 13D — EnemyBrain runtime MVP | **FECHADO em código** — wiring no Unity Editor pendente |
| SPEC 13E — Bestiary runtime/save | **NÃO implementado** |
| SPEC 13F — SpawnResolver ecologia/faction locks | **NÃO implementado** |

---

## 12. Próximo recorte recomendado

**Reconciliação de roster (pré-requisito para testes):**
1. Criar `EnemyDataSO` para os 40 IDs canônicos (ou converter o roster 13B)
2. Setar `ActionSetId` em cada `EnemyDataSO` canônico
3. Criar asset databases (EnemyActionDatabaseSO, EnemyActionSetDatabaseSO, EnemyTelegraphProfileDatabaseSO)
4. Configurar prefabs de inimigo com EnemyBrain + databases + EnemyVulnerabilityState

**SPEC 13E — Bestiary runtime/save**
**SPEC 13F — SpawnResolver ecologia/faction locks**
