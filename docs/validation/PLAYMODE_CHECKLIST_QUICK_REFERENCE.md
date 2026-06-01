# Quick Reference — Play Mode Validation Checklist

**Imprima ou deixe visível enquanto testa.**

---

## Step-by-Step Rápido

### ANTES DE INICIAR
- [ ] Unity Editor aberto
- [ ] Branch dev (verificado)
- [ ] Project carregado
- [ ] Console limpo
- [ ] Nenhuma mudança local não-commitada

### PARTE 1: VALIDATORS (do Menu)

Rodar e anotar PASS/FAIL/NOT RUN:

- [ ] CindarsHope/Validate/Combat/Validate Projectile Prefabs
- [ ] CindarsHope/Validate/Combat/Validate Combat Databases  
- [ ] CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP (em FarmScene)
- [ ] CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP (em TownScene)
- [ ] CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP (em CaveScene)

### PARTE 2: PLAY MODE (FarmScene)

Abrir FarmScene → Play Mode → Executar cada item:

**Movimento:**
- [ ] WASD mover player
- [ ] E interagir com árvore
- [ ] E interagir com plot  
- [ ] E interagir com lago

**UI:**
- [ ] I abrir inventory
- [ ] C abrir equipment
- [ ] 1-6 hotbar numbers

**Bow & Arrow:**
- [ ] Equipar bow + arrow
- [ ] Q dispara arrow (mão da arrow)
- [ ] Confirmar bow BLOQUEADO (não dispara pela mão do bow)

**Fireball:**
- [ ] Equipar fireball
- [ ] Q dispara fireball

**Damage (se enemy disponível):**
- [ ] Fireball causa dano
- [ ] Burn/DOT funciona (se aplicável)

**Save/Load:**
- [ ] Ctrl+S salvar
- [ ] Ctrl+L carregar
- [ ] Hotbar/equipment/inventory preservados

**Transitions:**
- [ ] Farm → Town (portal)
- [ ] Town → Farm (portal)
- [ ] Town → Cave (se disponível)

**Final:**
- [ ] Nenhum erro crítico novo no Console

---

## Se Encontrar Bug

PARE. Não corrija. Registre:

1. **Qual item:** (ex: "2.9 Disparar arrow")
2. **Scene ativa:** (ex: "FarmScene")
3. **Steps para reproduzir:** (ex: "Equipar bow+arrow, apertar Q")
4. **Erro no Console:** (copiar mensagem vermelha)
5. **Comportamento esperado:** (ex: "Arrow sai pela mão da arrow")
6. **Comportamento obtido:** (ex: "Arrow não sai, ou sai pelo bow")

---

## Template Rápido para Anotações

```
VALIDATOR: [Nome]
Scene: [FarmScene/TownScene/CaveScene]
Result: [ ] PASS [ ] FAIL [ ] NOT RUN
Error: [copiar]

PLAY MODE ITEM: [Número e descrição, ex: "2.9 Disparar arrow"]
Result: [ ] PASS [ ] FAIL [ ] NOT TESTED
Notes: [o que aconteceu]

BUG (se houver):
- Item: 
- Steps: 
- Error: 
```

---

## Ao Finalizar

1. Preencher `docs/validation/reorg_human_playmode_validation_report.md` com detalhes
2. Registrar summary executivo no PROJECT_LOG.md
3. Listar bugs encontrados (se houver) com steps de reprodução

---

**Tempo estimado:** ~1-1.5 horas (validators ~15min, Play Mode ~45min)  
**Não é teste QA completo** — apenas validação de regressão pós-reorg.
