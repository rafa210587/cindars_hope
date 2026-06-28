# Human Test Scenario — Autossuficiência da Vila: 4 NPCs

**Slice:** village_economy — slice 3
**Pré-requisito:** rodar `CindarsHope/Inicializar Projeto` (regenera TownScene com os assets, casas,
bancas e anchors novos) antes de testar.

---

## Setup

1. No Editor, menu `CindarsHope/Inicializar Projeto` → aguardar o log
   `[CreateMvpTownScene] Npc_Sael.asset ... criado` (e Mella/Hess/Tibbet + 3 Shop_* + 28 NPCs).
2. Abrir `Assets/_Game/Scenes/TownScene.unity` e entrar em Play Mode.

## Checks

### 1. Os 4 NPCs existem e falam
- [ ] Encontrar **Sael** no cais (SO, junto ao lago), **Mella** na praça/mercado (N), **Hess** na
      borda leste, **Tibbet** no cemitério (NO).
- [ ] Cada um abre uma árvore de diálogo completa (saudação → papel/vila/serviço/conselho/rumor →
      despedida), sem nó quebrado.

### 2. Lojas (Sael / Mella / Hess)
- [ ] Sael: vende perch/sun bass/amber trout + peixe grelhado.
- [ ] Mella: vende pão/fornada thandra/bolo de festival/sopa de abóbora.
- [ ] Hess: vende couro/pele/armadura leve de couro.
- [ ] Tibbet **não** abre loja (só diálogo).

### 3. Schedule / moradias
- [ ] À noite, entrar na `House_Fishery`/`House_Bakery`/`House_Tannery` e encontrar Sael/Mella/Hess
      dormindo dentro (telhado some ao entrar — `RoofRevealController`).
- [ ] Tibbet dorme ao relento, entre as covas do cemitério.
- [ ] Nenhuma casa sobreposta a outra nem a zona reservada (sem erro `[walk-in-houses]` no Console).

### 4. Segredo de Tibbet (Nyx)
- [ ] Subir a afinidade com Tibbet até "próximo": a fala revela que ele reza para Nyx, a Noite, e
      pede segredo ao Padre Corvus.

### 5. Coerência de população
- [ ] Console sem `FAIL` em `ValidateFableCitySchedule` (28 NPCs com ≥3 anchors).
- [ ] Spawn fazenda→cidade continua no portão oeste (nenhuma casa nova caiu sobre o corredor de spawn).

## Resultado
- PASS se todos os checks marcados; registrar divergências com screenshot do Console.
