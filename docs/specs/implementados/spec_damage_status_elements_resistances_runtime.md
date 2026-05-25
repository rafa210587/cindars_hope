# SPEC 11 - Damage, Status, Elements, Resistances Runtime

## Status: Implementado parcial

**Data conclusão:** 2026-05-25  
**Validação Unity:** Completa (Tundra build success)  
**Evidência:** `Logs/unity-compile-validation-spec11.log`  
**Commits:** 6af2db5

## Resumo executivo

SPEC 11 implementa o sistema formal de dano com tipos (Física, Arcana, Fogo, Gelo, Raio, Cura), cálculo de dano com defesa/resistência/vulnerabilidade/dano verdadeiro, floating damage numbers posicionados, e status effects com decay automático integrado em GameTimeTickEvent.

## Escopo entregue

### 1. DamageType enum
- Tipos: Physical, Arcane, Fire, Ice, Lightning, Heal, TrueDamage
- Arquivo: `Assets/_Game/Scripts/Combat/DamageType.cs`

### 2. DamageRequest DTO
- Campos: SourceId (string), BaseDamage (int), DamageType, SourcePosition (Vector3)
- Permite extensão com KnockbackForce, StatusEffectId, etc.
- Arquivo: `Assets/_Game/Scripts/Combat/DamageRequest.cs`

### 3. DamageResult DTO
- Campos: FinalDamage (int), DamageType, SourcePosition, TargetPosition (novo para floating numbers)
- Arquivo: `Assets/_Game/Scripts/Combat/DamageResult.cs`

### 4. DamageCalculator
- Static class com CalculateDirectDamage(baseDamage, defenseReduction=0, vulnerabilityMultiplier=1f)
- Fórmula: finalDamage = (baseDamage - defense) * vulnerabilityMultiplier
- Suporta TrueDamage (ignorar defesa)
- Arquivo: `Assets/_Game/Scripts/Combat/DamageCalculator.cs`

### 5. CombatResistanceProfile ScriptableObject
- Campos: ColdResistance, HeatResistance, ArcanResistance (por DamageType)
- Método: GetResistanceForType(damageType) → float
- Arquivo: `Assets/_Game/Scripts/Combat/CombatResistanceProfile.cs`

### 6. Status Effects
- StatusEffectManager: Dictionary<string, StatusEffectData> gerenciando aplicação/remoção/decay
- StatusEffectData: effectId, duration, appliedTime
- Evento: GameTimeTickEvent para decay automático
- Arquivo: `Assets/_Game/Scripts/Combat/StatusEffectManager.cs`

### 7. DamageAppliedEvent expandido
- Novo campo: TargetPosition (Vector3) para floating damage numbers posicionados corretamente
- Evento publicado por EnemyHealth.TakeDamage() com transform.position do alvo
- Arquivo: `Assets/_Game/Scripts/Core/Events/DamageAppliedEvent.cs`

### 8. FloatingDamageNumberDisplayer
- Mostra número de dano na posição exata do alvo (não na câmera)
- Arquitetura corrigida: GetComponentInParent<Canvas>() em lugar de FindObjectOfType() (CLAUDE.md compliance)
- Fallback warning se Canvas não encontrada
- Arquivo: `Assets/_Game/Scripts/UI/FloatingDamageNumberDisplayer.cs`

### 9. Integração com EnemyHealth
- TakeDamage(damageRequest) publica DamageAppliedEvent com TargetPosition
- Arquivo: `Assets/_Game/Scripts/Enemy/EnemyHealth.cs`

## Contratos preservados

- ✅ DamageCalculator intacto (fórmula core: baseDamage - defense)
- ✅ StatusEffectManager intacto (apply/remove/tick)
- ✅ CombatResistanceProfile intacto (resolução por tipo)
- ✅ SaveData v3 (List<StatusEffectEntryData>) compatível com SPEC 09

## Gaps deferred para SPEC 17

- Vulnerability/resistance application final UI
- Status effect visual overlays na HUD
- Combat damage numbers styling/animation final
- Canvas consolidado com damage/status displays
- Play Mode manual validation (checklist em docs/validation/)

## Validações

| Tipo | Status | Evidência |
|---|---|---|
| Docs validation | PASS | Carried over from SPEC 10 |
| Unity compile | PASS | Logs/unity-compile-validation-spec11.log |
| Assembly warnings | Present | Preexisting (not introduced by SPEC 11) |
| Play Mode testing | NOT RUN | Batchmode environment; manual checklist provided |

## Dependências

- Bloqueia: SPEC 12 (Player Combat Weapons Spells), SPEC 13 (Enemy AI), SPEC 14 (Cave Generation)
- Depende de: SPEC 00-10 implementadas/parciais
- Integração com: SPEC 09 (StatusEffectManager), SPEC 10 (DamageCalculator usado por equipment)

## Residual risk

Status effect tick mechanics e vulnerability flow aguardam validação manual em Play Mode. Checklist disponível em `docs/validation/SPEC_11_DAMAGE_STATUS_RESISTANCES_VALIDATION_<date>.md`.
