# Handoff — FASE9H Cave Loot, Crafting, Equipment & Gear Progression

> **Data:** 2026-05-20  
> **Branch:** dev  
> **Responsável:** ChatGPT  
> **Escopo:** registrar criação da FASE9H como baseline de loot/crafting/equipment progression sem sobrescrever `PROJECT_LOG.md`, que pode estar recebendo entradas paralelas de implementação.

---

## 1. Arquivos criados

- `docs/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md`
- `specs/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION/spec.md`

---

## 2. Decisões registradas

- Equipamento será mais RPG, com múltiplas armas/builds por tier.
- Armor existe no MVP, mas como peça única.
- MVP possui 1 slot de accessory.
- Armaduras têm defesa, atributos, peso, resistências elementais e resistências ambientais.
- `HeatResistance` e `ColdResistance` são separadas de `FireResistance` e `IceResistance`.
- Ambientes quentes/frios podem causar dano ou bloquear progressão se resistência for insuficiente.
- Cloth favorece Intelligence/Willpower.
- Leather favorece Dexterity/crit/mobilidade.
- Metal favorece defesa/resistência, mas pesa mais.
- Durabilidade existe no MVP: equipáveis têm `DurabilityMax = 100` e perdem 1 a cada 3 usos relevantes.
- Boss drops podem ser key items, materiais raros vendáveis ou ambos.
- Armas elementais dropadas por humanoides vêm quebradas e exigem reparo.
- Crafting exige segurar botão por tempo.
- Level e Dexterity reduzem tempo de crafting.
- Crafting em área hostil cancela se o jogador tomar dano.
- Refinamento/crafting usa múltiplas estações.
- Tool tier bloqueia nodes e pode bloquear caminho principal se ferramenta/material/receita estiverem disponíveis antes.

---

## 3. Hardening aplicado

- Sem hard lock injusto: progressão principal só pode exigir ferramenta/equipamento se o jogador tiver acesso prévio à receita e materiais.
- Aviso antes de dano ambiental pesado.
- Armadura pesada não pode ser melhor em tudo.
- Cloth/Leather precisam permanecer viáveis como builds.
- Boss key items não são vendáveis por padrão.
- Armas elementais quebradas não quebram balance porque precisam de repair flow.
- Durability deve evitar microgerenciamento excessivo.

---

## 4. Validação

- [x] Documento FASE9H criado no repo.
- [x] SpecKit FASE9H criado no repo.
- [x] Handoff documental criado.
- [ ] Unity não executado; alteração documental/spec.
- [ ] `PROJECT_LOG.md` não foi editado para evitar sobrescrever entradas paralelas recentes.

---

## 5. Próximo passo recomendado

Quando a implementação chegar nesta área, usar FASE9H como baseline para:

- equipment contracts;
- durability;
- crafting time;
- repair bench;
- environmental resistance;
- armor/accessory slots;
- broken elemental weapons;
- loot tables por família/boss;
- tool/environment gates.

O MVP recomendado da spec começa por contracts e exemplos mínimos, não por todos os tiers completos.
