# Human Play Mode & Editor Validator Validation Report

**Date:** [PREENCHER]  
**Tester:** [PREENCHER - seu nome]  
**Branch:** dev  
**Unity Version:** [PREENCHER]  
**Mode:** Human validation checkpoint pós SPEC_12  

---

## Pré-condições ✓

- [x] Branch dev ativa (verificado em CLI: git branch --show-current = dev)
- [x] Nenhuma mudança local não-commitada (verificado em CLI: git status --short = vazio)
- [ ] Unity Editor aberto
- [ ] Console limpo
- [ ] Project carregado sem erro de import

---

## Parte 1: Editor Validators

Rodar cada validator do menu. Registrar resultado com scene ativa.

### 1.1 CindarsHope/Validate/Combat/Validate Projectile Prefabs

**Scene ativa:** [PREENCHER]  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens de erro:** [PREENCHER - copiar do Console]  
**Warnings relevantes:** [PREENCHER]  
**Exceção no Console:** [ ] Sim / [ ] Não  

**Notas:**
```
[PREENCHER - qualquer informação adicional]
```

---

### 1.2 CindarsHope/Validate/Combat/Validate Combat Databases

**Scene ativa:** [PREENCHER]  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens de erro:** [PREENCHER]  
**Warnings relevantes:** [PREENCHER]  
**Exceção no Console:** [ ] Sim / [ ] Não  

**Notas:**
```
[PREENCHER]
```

---

### 1.3 CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP

**Scene ativa:** FarmScene  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens de erro:** [PREENCHER]  
**Warnings relevantes:** [PREENCHER]  
**Exceção no Console:** [ ] Sim / [ ] Não  

**Notas:**
```
[PREENCHER]
```

---

### 1.4 CindarsHope/Advanced/Legacy/Validate/Validate Farm Town MVP (TownScene)

**Scene ativa:** TownScene  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens de erro:** [PREENCHER]  
**Warnings relevantes:** [PREENCHER]  
**Exceção no Console:** [ ] Sim / [ ] Não  

**Notas:**
```
[PREENCHER]
```

---

### 1.5 CindarsHope/Advanced/Legacy/Validate/Validate Cave MVP

**Scene ativa:** CaveScene  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens de erro:** [PREENCHER]  
**Warnings relevantes:** [PREENCHER]  
**Exceção no Console:** [ ] Sim / [ ] Não  

**Notas:**
```
[PREENCHER]
```

---

### 1.6 Outros Validators (se encontrados)

**Nome do validator:** [PREENCHER]  
**Scene ativa:** [PREENCHER]  
**Resultado:** [ ] PASS / [ ] FAIL / [ ] NOT RUN  
**Motivo (se NOT RUN):** [PREENCHER]  
**Mensagens:** [PREENCHER]  

---

## Parte 2: Play Mode Checklist

Abrir FarmScene. Entrar em Play Mode. Executar cada item em ordem.

**Scene:** FarmScene  
**Play Mode Duration:** [PREENCHER - tempo total]  
**Console Errors Durante Play Mode:** [ ] Sim / [ ] Não  

### Movimento & Interação

- [ ] 2.1 Mover player com WASD — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER se falhou ou comportamento inesperado]

- [ ] 2.2 Interagir com árvore (E) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.3 Interagir com plot (E) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.4 Interagir com lago/fishing (E) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

### UI & Inventory

- [ ] 2.5 Abrir hotbar (numbers 1-6) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.6 Abrir inventory (I) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.7 Abrir equipment (C) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

### Combat — Bow & Arrow

- [ ] 2.8 Equipar bow + arrow do inventory — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.9 Disparar arrow (Q) pela mão da arrow — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER - descrever se arrow saiu corretamente]

- [ ] 2.10 Confirmar que bow NÃO dispara pela mão do bow (Q) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER - confirmar se bow está bloqueado quando arrow é mão primária]

### Combat — Fireball

- [ ] 2.11 Equipar fireball do inventory — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.12 Disparar fireball (Q) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

### Combat — Enemy/Damage (se disponível)

**Sub-scene ou transição usada:** [PREENCHER - FarmScene com enemy spawned, ou Cave, ou outra]

- [ ] 2.13 Confirmar dano da fireball — [ ] PASS / [ ] FAIL / [ ] NOT TESTED / [ ] NO ENEMY AVAILABLE
  - Notas: [PREENCHER]

- [ ] 2.14 Confirmar burn/DOT se possível — [ ] PASS / [ ] FAIL / [ ] NOT TESTED / [ ] NO BURN MECHANIC / [ ] NO ENEMY
  - Notas: [PREENCHER]

### Save & Load

- [ ] 2.15 Salvar game (Ctrl+S ou menu) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.16 Sair do Play Mode — [ ] PASS / [ ] FAIL / [ ] NOT TESTED

- [ ] 2.17 Carregar game (Ctrl+L ou menu) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

- [ ] 2.18 Confirmar hotbar/equipment/inventory preservados após load — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER - descrever o que mudou, se algo]

### Scene Transitions

**Transição 1:**
- [ ] 2.19 Transicionar Farm → Town (portal) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

**Transição 2:**
- [ ] 2.20 Transicionar Town → Farm (portal) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Notas: [PREENCHER]

**Transição 3 (opcional):**
- [ ] 2.21 Transicionar para Cave (se fluxo disponível) — [ ] PASS / [ ] FAIL / [ ] NOT TESTED / [ ] FLUXO NAO DISPONIVEL
  - Notas: [PREENCHER]

### Final Check

- [ ] 2.22 Confirmar que não há erro crítico novo no Console — [ ] PASS / [ ] FAIL / [ ] NOT TESTED
  - Critical errors encontrados: [PREENCHER - listar qualquer erro que apareça como vermelho no Console]

---

## Bugs Encontrados

Se algum item acima falhou ou teve comportamento inesperado, registrar aqui. NÃO corrigir durante teste.

### Bug #1 (se houver)

**Item falho:** [PREENCHER - qual checklist item]  
**Scene:** [PREENCHER]  
**Steps para reproduzir:** [PREENCHER]  
**Resultado esperado:** [PREENCHER]  
**Resultado obtido:** [PREENCHER]  
**Mensagem de erro no Console:** [PREENCHER]  
**Severidade:** [ ] Critical / [ ] High / [ ] Medium / [ ] Low  

```
[PREENCHER - stack trace ou mais detalhes]
```

---

### Bug #2 (se houver)

[REPETIR TEMPLATE]

---

## Resumo Executivo

### Validators Status
| Validator | Status | Scene |
|-----------|--------|-------|
| Projectile Prefabs | [ ] PASS / [ ] FAIL / [ ] NOT RUN | [PREENCHER] |
| Combat Databases | [ ] PASS / [ ] FAIL / [ ] NOT RUN | [PREENCHER] |
| Farm Town MVP (Farm) | [ ] PASS / [ ] FAIL / [ ] NOT RUN | FarmScene |
| Farm Town MVP (Town) | [ ] PASS / [ ] FAIL / [ ] NOT RUN | TownScene |
| Cave MVP | [ ] PASS / [ ] FAIL / [ ] NOT RUN | CaveScene |

### Play Mode Summary
- **Total items testados:** 22
- **Items PASS:** [PREENCHER]
- **Items FAIL:** [PREENCHER]
- **Items NOT TESTED:** [PREENCHER]
- **Bugs encontrados:** [PREENCHER - número]
- **Critical bugs:** [ ] Sim / [ ] Não

### Regressão Detectada?
[ ] Sim — [PREENCHER detalhes]  
[ ] Não  

---

## Conclusão

**Status geral:** [ ] PASS / [ ] FAIL / [ ] PARTIAL PASS  

**Observações:**
```
[PREENCHER - resumo do que funcionou, o que não funcionou, 
recomendações, próximos passos se necessário]
```

**Data/hora de conclusão:** [PREENCHER]  
**Próximas ações recomendadas:** [PREENCHER]

---

## Checklist de Preenchimento Final

Antes de finalizar este relatório:

- [ ] Todos os validators foram testados ou marcados como NOT RUN com motivo
- [ ] Play Mode checklist foi preenchido item-a-item
- [ ] Bugs encontrados foram registrados com steps de reprodução
- [ ] Nenhum resultado foi mascarado (PASS quando na verdade foi FAIL/NOT TESTED)
- [ ] Console errors foram copiados quando relevantes
- [ ] Scenes ativas foram registradas
- [ ] Data e tester estão preenchidos
- [ ] Resumo executivo está coerente com os detalhes acima
