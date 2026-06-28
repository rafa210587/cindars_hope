# Human Test Scenario — Estações de Ofício nas Casas

> **Spec:** `spec_city_artisan_stations` · Play Mode manual (DEFERRED_TO_FINAL_VALIDATION)
> **Pré-requisito:** `CindarsHope/Inicializar Projeto` (regenera a TownScene com as estações).

## Setup
1. Unity → `CindarsHope/Inicializar Projeto`. Entrar em Play Mode na TownScene.

## Casos de teste

### CT1 — Estação abre o craft do tipo certo
- [ ] Entrar na **Ferraria** (House_Blacksmith). Há um prop de estação (forja, alaranjado) com prompt **"Usar a forja [E]"**.
- [ ] Apertar **E** → abre o craft mostrando **só receitas de Forge** (armas/armadura de metal). Esc fecha.
- [ ] Repetir na **Alquimia** (alambique → só receitas Alchemy: poções/óleos).
- [ ] **Estalagem** (fogão → CookingStation: comidas). **Oficina** (bancada → Carpentry). **Casa da Mirela** (tear → Sewing). **Salão de Mercado** (bancada → Workbench).

### CT2 — Desacoplado / sem regressão
- [ ] A tecla **C** (pocket crafting) continua abrindo o craft de bolso normalmente.
- [ ] Abrir uma estação não trava o jogador nem deixa modal preso (Esc fecha; movimento volta).

### CT3 — Filtro coerente
- [ ] Cada estação lista **apenas** receitas do seu WorkshopType (forja não mostra poções; alquimia não mostra armas).
- [ ] Estação cujo tipo ainda não tem receita (ex.: Sewing, antes do slice 2) abre **vazia** ("No compatible recipes") — esperado.

## Resultado esperado
Cada casa-ofício tem sua estação física; apertar E abre o craft filtrado pelo ofício daquela casa, sem
quebrar o craft de bolso (C). As casas passam a ter a "mesma lógica" funcional.
