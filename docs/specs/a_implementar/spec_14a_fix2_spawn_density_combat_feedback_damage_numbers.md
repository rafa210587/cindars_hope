---
required_adrs: [ADR-0005-cave-stable-run-and-replay]
required_game_rules: [cave_rules.md, combat_rules.md]
---

# SPEC 14A-FIX2 - Spawn Density, Combat Logs and Floating Damage Numbers

> Spec ID: spec_14a_fix2_spawn_density_combat_feedback_damage_numbers
> Status: A implementar
> Ordem de execucao: 14A-FIX2
> Tipo: Cave Spawn Density / Combat Feedback / Floating Damage Numbers
> Fonte de refinamento: `docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14a_fix2_spawn_density_combat_feedback_damage_numbers.md`
> Depende de: SPEC 14A/14B com inimigos materializando na cave.
> Bloqueia: Nenhuma spec futura diretamente.
> Fora de escopo: balance final, UI final de combate, critical system, healing numbers completo, boss UI, SPEC 18 movement/projectiles/melee visuals.

---

# /speckit.specify

## O QUE

Implementar três ajustes de feedback e gameplay curto:

1. Aumentar em aproximadamente 20% a quantidade de criaturas por cave level.
2. Padronizar logs de combate para incluir o nome da criatura em todo log envolvendo inimigos.
3. Implementar números de dano flutuantes acima da entidade que tomou dano.

## POR QUE

Os inimigos já aparecem na cave. Agora precisamos melhorar sensação de risco, debugabilidade e feedback visual de combate.

O log atual de contato inimigo -> player não mostra qual criatura causou dano:

```text
EnemyContactDamage: dealt 3 damage to player. HP should update through PlayerManager.
```

O esperado é que todo log de combate envolvendo inimigo carregue nome e EnemyId.

Além disso, o jogador precisa ver feedback visual do dano recebido/causado.

---

# /speckit.clarify

## Decisões fechadas

| Tema | Decisão |
|---|---|
| Aumento de inimigos | Aproximadamente +20% |
| Range antigo esperado | 12-20 |
| Novo range alvo | 14-24 |
| Log de criatura | Sempre incluir DisplayName e EnemyId |
| Dano flutuante em inimigo | Sim |
| Dano flutuante no player | Sim |
| Package novo | Não |
| Critical/heal final | Fora de escopo |
| UI final | Fora de escopo |

---

# /speckit.plan

## Escopo técnico

### Spawn density

Ajustar o limite/configuração usada pelo `CaveEnemySpawnPlanner`/`CaveRuntimeMaterializer`.

Preferência:

```text
_maxEnemiesPerLevel: 20 -> 24
min desejado, se existir: 12 -> 14
```

Preservar:

- determinismo por seed;
- faction locks;
- room size constraints;
- packs;
- warnings quando não houver espaço.

### Combat logs

Ajustar logs em:

```text
EnemyContactDamage
EnemyHealth
outros pontos de dano encontrados
```

Logs envolvendo inimigo devem incluir:

```text
DisplayName
EnemyId
EnemyInstanceId se disponível
Damage
DamageType se disponível
Target
HP before/after quando disponível
```

### Floating damage numbers

Criar sistema MVP:

```text
FloatingDamageNumberManager
FloatingDamageNumberView
```

Ou nomes equivalentes alinhados ao projeto.

Requisitos:

- mostrar número acima do target que recebeu dano;
- funcionar para inimigo;
- funcionar para player;
- não depender de arte final;
- não adicionar package;
- usar evento ou chamada central;
- não usar busca global runtime proibida.

Eventos preferidos:

```text
DamageAppliedEvent para inimigos
HPChangedEvent ou PlayerDamagedEvent para player
```

Se `HPChangedEvent` não carrega posição, emitir número diretamente no ponto de dano do player ou criar evento de player damage com position simples.

---

# /speckit.tasks

## C01 — Aumentar densidade

- [ ] Localizar `_maxEnemiesPerLevel` e configurações equivalentes.
- [ ] Ajustar max para aproximadamente 24.
- [ ] Ajustar mínimo para aproximadamente 14, se houver mínimo configurável.
- [ ] Garantir que a mudança preserva seed determinística.
- [ ] Atualizar validator/doc para registrar densidade esperada.

## C02 — Logs de contato inimigo -> player

- [ ] Atualizar `EnemyContactDamage`.
- [ ] Logar `SourceName` e `SourceEnemyId`.
- [ ] Logar `Target=Player`.
- [ ] Logar `Damage`.
- [ ] Se possível, logar `EnemyInstanceId`.

Exemplo:

```text
CombatLog: EnemyContactDamage. SourceName=Rato de Basalto, SourceEnemyId=enemy_stone_rat, Target=Player, Damage=3.
```

## C03 — Logs de dano em inimigo

- [ ] Revisar `EnemyHealth`.
- [ ] Garantir que hit, knockback, status e death logs já têm nome/EnemyId.
- [ ] Ajustar qualquer log restante sem DisplayName/EnemyId.

## C04 — Floating damage numbers

- [ ] Criar `FloatingDamageNumberManager`.
- [ ] Criar `FloatingDamageNumberView`.
- [ ] Criar prefab ou fallback runtime seguro.
- [ ] Mostrar dano em world position + offset.
- [ ] Fazer número subir/desaparecer em tempo curto.
- [ ] Garantir que múltiplos danos simultâneos funcionam.
- [ ] Integrar com dano em inimigo.
- [ ] Integrar com dano no player.

## C05 — Eventos/payloads

- [ ] Reutilizar `DamageAppliedEvent` para inimigos se possível.
- [ ] Criar `PlayerDamagedEvent` somente se necessário.
- [ ] Payloads devem ter tipos simples: amount, world position, target id/name opcional.
- [ ] Não carregar GameObject/Transform/MonoBehaviour.

## C06 — Validator

Criar:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AFix2CombatFeedback.cs
```

Verificar:

- max enemies configurado para novo alvo;
- `EnemyContactDamage` possui acesso a `EnemyDataSO` para logs;
- existe manager/view de floating damage number;
- eventos de dano usados não carregam Unity refs;
- não há busca global runtime proibida nos arquivos novos.

## C07 — Documento de validação

Criar:

```text
docs/validation/SPEC14A_FIX2_SPAWN_DENSITY_COMBAT_FEEDBACK_VALIDATION_<YYYYMMDD>.md
```

Conteúdo:

- resumo;
- arquivos alterados;
- densidade antes/depois;
- exemplos de logs;
- como damage numbers são exibidos;
- validações executadas;
- validações pendentes;
- riscos residuais.

---

# /speckit.implement

## Prompt completo para Claude/Codex

```md
Estamos no projeto Cindar's Hope / repositório rafa210587/cindars_hope.

Branch alvo: dev.

Implemente somente:
SPEC 14A-FIX2 - Spawn Density, Combat Logs and Floating Damage Numbers

Leia primeiro:
- AGENTS.md
- CLAUDE.md
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/a_implementar/spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14a_fix2_spawn_density_combat_feedback_damage_numbers.md
- docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md
- docs/specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md
- docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md

Contexto:
As criaturas já aparecem na cave. Agora queremos aumentar cerca de 20% a quantidade de criaturas, melhorar logs de combate para sempre mostrar nome/EnemyId da criatura e adicionar números de dano flutuantes acima de quem tomou dano.

Escopo:
1. Aumentar densidade de inimigos em aproximadamente 20%.
2. Atualizar logs de combate envolvendo inimigos para incluir DisplayName e EnemyId.
3. Criar sistema MVP de floating damage numbers para player e inimigos.
4. Criar validator e documentação.

Proibido:
- Não implementar SPEC 18.
- Não implementar boss fights.
- Não implementar respawn 2 dias.
- Não implementar redistribuição pós-morte.
- Não adicionar package.
- Não criar sistema paralelo de dano/status/save/event bus.
- Não usar GameObject.Find/FindObjectOfType/FindObjectsByType em runtime.
- Não serializar Unity refs em save DTO.

Requisitos de densidade:
- Se o max atual for 20, ajustar para 24.
- Se existir min 12, ajustar para 14.
- Preservar determinismo por seed.
- Preservar room constraints/faction locks/packs.

Requisitos de logs:
- EnemyContactDamage deve logar SourceName e SourceEnemyId.
- EnemyHealth deve manter/garantir logs com TargetName e TargetEnemyId.
- Todo novo log de combate deve começar com CombatLog.

Exemplo esperado:
CombatLog: EnemyContactDamage. SourceName=Rato de Basalto, SourceEnemyId=enemy_stone_rat, Target=Player, Damage=3.

Requisitos de damage numbers:
- Aparecer acima do inimigo quando inimigo toma dano.
- Aparecer acima do player quando player toma dano.
- Pode ser texto simples/fallback, sem arte final.
- Deve subir e desaparecer em curto período.
- Deve suportar múltiplos hits.
- Deve usar world position + offset.

Validações obrigatórias:
```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
git diff --check
```

Checklist Play Mode:
1. Entrar na cave nível 1.
2. Confirmar que há aproximadamente 20% mais inimigos que antes, respeitando espaço/seed.
3. Encostar em inimigo e confirmar log com nome/EnemyId.
4. Confirmar número de dano acima do player.
5. Bater em inimigo e confirmar log com nome/EnemyId.
6. Confirmar número de dano acima do inimigo.
7. Confirmar console sem erros novos.

Entrega final:
Responder com:
1. resumo técnico;
2. arquivos alterados;
3. densidade antes/depois;
4. exemplos de logs;
5. como damage numbers foram implementados;
6. validações executadas;
7. validações pendentes;
8. riscos residuais.
```

---

# Definition of Done

- Projeto compila.
- Inimigos na cave aumentam aproximadamente 20%.
- `EnemyContactDamage` loga nome e EnemyId.
- Logs de dano em inimigo têm nome e EnemyId.
- Damage number aparece acima do player ao tomar dano.
- Damage number aparece acima do inimigo ao tomar dano.
- Validator criado.
- Documento de validação criado.
