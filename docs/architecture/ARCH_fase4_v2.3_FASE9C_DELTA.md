# ARCH fase4 v2.3 — Delta FASE 9C Tools/Farm/Combat

> Complementa `docs/architecture/ARCH_fase4_v2.2.md`.
> Não substitui a arquitetura anterior; adiciona contratos para FASE 9C.

## 1. Novos módulos planejados

```text
Assets/_Game/Scripts/
├── Tools/
│   ├── Data/
│   │   ├── ToolType.cs
│   │   ├── ToolTier.cs
│   │   ├── ToolDataSO.cs
│   │   └── ToolDatabaseSO.cs
│   ├── ToolRequirement.cs
│   └── ToolActionResolver.cs
├── Equipment/
│   └── EquipmentManager.cs
└── Combat/
    ├── WeaponType.cs
    ├── WeaponDataSO.cs
    ├── WeaponDatabaseSO.cs
    ├── PlayerCombatController.cs
    ├── PlayerDodgeController.cs
    └── ProjectileController.cs
```

## 2. Tool System

### 2.1 Objetivo

Centralizar regras de uso de ferramenta para farm/world actions.

A ferramenta deve determinar:

- permissão da ação;
- eficiência;
- yield;
- action power;
- cooldown;
- custo de fome/stamina;
- fallback sem ferramenta correta.

### 2.2 ToolDataSO

`ToolDataSO` deve ser `ScriptableObject` identificável por ID estável e resolvido via `ToolDatabaseSO`.

Campos mínimos:

- `Id`
- `Item`
- `ToolType`
- `ToolTier`
- `ActionPower`
- `ActionCooldownSeconds`
- `EfficiencyMultiplier`
- `YieldMultiplier`
- `FlatYieldBonus`
- `HungerCostMultiplier`

### 2.3 ToolRequirement

`ToolRequirement` deve ser serializável em componentes como `FarmPlot`, `TreeNode`, `FishingSpot` e futuro `ResourceNode`.

Campos mínimos:

- `RequiredToolType`
- `MinimumTier`
- `AllowFallbackWithoutTool`
- `FallbackItemId`
- `FallbackAmount`
- `FallbackProgressesTarget`
- `FailureMessage`

### 2.4 ToolActionResolver

Regra: componentes de gameplay não devem duplicar lógica de comparação de tool/tier.

Criar `ToolActionResolver` puro para receber requirement + tool equipada e retornar:

- sucesso/falha;
- uso de fallback;
- action power;
- yield multiplier;
- flat yield bonus;
- mensagem de feedback.

## 3. EquipmentManager

### 3.1 Responsabilidade

Novo manager persistente no `GameBootstrap`.

Responsável por:

- `EquippedToolId`;
- `EquippedWeaponId`;
- equipar ferramenta;
- equipar arma;
- resolver `ToolDataSO` e `WeaponDataSO`;
- capturar/restaurar `EquipmentSaveData`;
- publicar eventos de troca.

### 3.2 Integração com GameBootstrap

Adicionar referência serializada opcional no gerador da Farm/Cave/Town e expor propriedade:

```csharp
public EquipmentManager EquipmentManager { get; private set; }
```

Duplicatas de bootstrap não devem resetar equipment.

## 4. Farm Action Architecture

### 4.1 Plantio

`FarmPlot` deve parar de escolher seed automaticamente.

Novo fluxo:

1. receber seed ativa ou seleção de seed;
2. consultar `EquipmentManager`;
3. resolver `ToolRequirement` de plantio;
4. aplicar penalidade/fallback se permitido;
5. publicar eventos.

### 4.2 Colheita

Colheita deve aplicar:

- yield mínimo sem Sickle;
- yield normal com Sickle Basic;
- bônus por tier.

### 4.3 TreeNode

`TreeNode` deve exigir Axe para progresso real.

Sem Axe:

- fallback mínimo opcional;
- não incrementar `HitsTaken`;
- não cortar árvore.

Com Axe:

- aplica `ActionPower`;
- aplica `YieldMultiplier`/`FlatYieldBonus`.

### 4.4 FishingSpot

Migrar `_requiredToolId` para `ToolRequirement`.

## 5. Weapon Architecture

### 5.1 WeaponDataSO

Armas devem ser data-driven.

Campos mínimos:

- `Id`
- `Item`
- `WeaponType`
- `Damage`
- `Range`
- `CooldownSeconds`
- `KnockbackForce`
- `ProjectileSpeed`
- `ProjectileLifetimeSeconds`
- `AmmoItemId`
- `AmmoCost`
- `MpCost`
- `ElementId`

### 5.2 WeaponType

Tipos obrigatórios:

- `Melee`
- `RangedPhysical`
- `RangedMagic`

### 5.3 PlayerCombatController

Substitui gradualmente `PlayerAttackController`.

Responsabilidades:

- ler input de ataque;
- obter arma equipada;
- usar fallback `Unarmed` se necessário;
- executar ataque por tipo;
- respeitar cooldown;
- publicar eventos.

## 6. Dodge Architecture

Adicionar `PlayerDodgeController`.

Regras:

- Space: esquiva;
- A/D + Space: lateral;
- S + Space: para trás;
- curta invulnerabilidade;
- cooldown;
- bloquear ataque/interação durante dodge;
- mover via `Rigidbody2D.MovePosition`.

## 7. Projectile Architecture

`ProjectileController` deve:

- mover em direção fixa;
- aplicar `DamageRequest` em `EnemyHealth`;
- destruir/desativar no impacto ou fim de vida;
- não depender de tags;
- usar componente para detecção.

## 8. Save Architecture

Adicionar opcionalmente:

```csharp
public EquipmentSaveData Equipment = new EquipmentSaveData();
```

Compatibilidade:

- saves antigos sem `Equipment` devem carregar com equipment vazio;
- nunca serializar `ToolDataSO`, `WeaponDataSO` ou referências Unity;
- apenas IDs simples.

## 9. Eventos arquiteturais novos

- `ToolEquippedEvent`
- `ToolActionResolvedEvent`
- `WeaponEquippedEvent`
- `PlayerAttackStartedEvent`
- `PlayerAttackResolvedEvent`
- `ProjectileSpawnedEvent`
- `ProjectileHitEvent`
- `SpellCastEvent`
- `PlayerDodgeStartedEvent`
- `PlayerDodgeEndedEvent`

## 10. Regra de implementação

Cada PR deve ser pequeno:

1. contratos;
2. assets;
3. manager;
4. integração por sistema;
5. validação;
6. cena/gerador;
7. handoff.

Não misturar Tool System, Weapon System, Dodge e UI no mesmo PR.


