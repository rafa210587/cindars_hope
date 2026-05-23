# Tasks — FASE9E Player Level Up, XP e Progressão

> **Feature:** FASE9E_PLAYER_LEVEL_UP_PROGRESSION  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-162 — Player progression contracts

### Escopo

Criar contratos de progressão, atributos, XP source e dificuldade.

### Critérios

- [ ] PlayerAttributeType existe.
- [ ] XpSourceType existe.
- [ ] EnemyDifficulty existe.
- [ ] PlayerStats contém Strength, Dexterity, Intelligence, Willpower, Constitution e Breath.
- [ ] Defaults começam em 1.

---

## PR-163 — PlayerProgressionManager

### Escopo

Criar manager de XP, level up e pontos.

### Critérios

- [ ] Level inicial 1.
- [ ] MaxLevel 100.
- [ ] CurrentXp inicial 0.
- [ ] XpToNextLevel inicial 100.
- [ ] Curva por bloco de 10 níveis implementada.
- [ ] +1 AttributePoint por level.
- [ ] +1 SkillPoint a cada 3 níveis.
- [ ] Multi-level up funciona.

---

## PR-164 — Derived stats integration

### Escopo

Integrar Willpower, Constitution e Breath aos recursos derivados.

### Critérios

- [ ] MaxMana = 10 + Willpower * 4.
- [ ] MaxHP = 12 + Constitution * 5.
- [ ] MaxStamina = 10 + Breath * 2.
- [ ] Atualização acontece ao gastar ponto.
- [ ] Valores não ficam negativos.

---

## PR-165 — Attribute spending MVP

### Escopo

Criar forma debug/OnGUI de gastar pontos.

### Critérios

- [ ] Pode gastar ponto em Strength.
- [ ] Pode gastar ponto em Dexterity.
- [ ] Pode gastar ponto em Intelligence.
- [ ] Pode gastar ponto em Willpower.
- [ ] Pode gastar ponto em Constitution.
- [ ] Pode gastar ponto em Breath.
- [ ] Não permite pontos negativos.
- [ ] AttributeMax 100 respeitado.

---

## PR-166 — Progression save/load integration

### Escopo

Garantir persistência de progressão.

### Critérios

- [ ] Level salva/carrega.
- [ ] CurrentXp salva/carrega.
- [ ] XpToNextLevel salva/carrega.
- [ ] UnspentAttributePoints salva/carrega.
- [ ] UnspentSkillPoints salva/carrega.
- [ ] 6 atributos salvam/carregam.
- [ ] Save antigo recebe defaults seguros.

---

## PR-167 — Progression events/debug HUD

### Escopo

Adicionar eventos e HUD/debug.

### Critérios

- [ ] XpGainedEvent publicado.
- [ ] LevelUpEvent publicado.
- [ ] AttributePointSpentEvent publicado.
- [ ] DebugHud mostra level/XP.
- [ ] DebugHud mostra pontos.
- [ ] DebugHud mostra 6 atributos.

---

## PR-168 — Enemy XP difficulty integration

### Escopo

Integrar XP com inimigos.

### Critérios

- [ ] EnemyDataSO tem EnemyLevel.
- [ ] EnemyDataSO tem BaseDifficulty.
- [ ] EnemyDataSO tem XpRewardOverride.
- [ ] XP usa override quando > 0.
- [ ] XP calcula EnemyLevel × DifficultyMultiplier quando não houver override.
- [ ] Dificuldade relativa considera LevelDelta.
- [ ] XP só concede na morte do inimigo.

---

## PR-169 — CaveLevel spawn progression rules

### Escopo

Preparar contratos para CaveLevel e spawn por nível.

### Critérios

- [ ] CaveLevel definido como conceito/contrato.
- [ ] Spawn base EnemyLevel = CaveLevel.
- [ ] 10–20% podem ser CaveLevel + 1.
- [ ] 10% chance de especial CaveLevel + 2.
- [ ] Especial tem drop melhor documentado/previsto.

---

## PR-170 — Progression validator/handoff

### Escopo

Validar dados e documentar smoke test.

### Critérios

- [ ] Validator detecta Level < 1.
- [ ] Validator detecta atributo < 1.
- [ ] Validator detecta atributo > 100.
- [ ] Validator detecta XpToNextLevel <= 0.
- [ ] Validator detecta EnemyLevel inválido.
- [ ] Handoff contém smoke test.

---

## Smoke test final

- [ ] Ganhar XP debug.
- [ ] Subir 1 nível.
- [ ] Subir múltiplos níveis.
- [ ] Confirmar +1 AttributePoint por nível.
- [ ] Confirmar +1 SkillPoint no level 3/6/9.
- [ ] Gastar ponto em Strength e ver melee subir.
- [ ] Gastar ponto em Dexterity e ver ranged subir.
- [ ] Gastar ponto em Intelligence e ver magic subir.
- [ ] Gastar ponto em Willpower e ver mana subir.
- [ ] Gastar ponto em Constitution e ver HP subir.
- [ ] Gastar ponto em Breath e ver stamina subir.
- [ ] Derrotar inimigo e ganhar XP calculado.
- [ ] Salvar/carregar progressão.
- [ ] Console sem erro vermelho.

