# Cenário Humano — Cidade Coerente Preservation-First

> Spec: `spec_city_preservation_first_coherent_relayout`
> Plataforma: Windows / Unity Editor 6000.4.7f1
> Duração estimada: 15–20 minutos
> Status: NOT RUN — validação humana final

## Objetivo

Confirmar visualmente que a TownScene preservou todo o conteúdo, possui circulação clara, prédios
distintos, portas acessíveis e NPCs coerentes com seus papéis e horários.

## Preparação

1. Abrir o projeto no Unity.
2. Executar `CindarsHope/Inicializar Projeto` se a cena precisar ser regenerada.
3. Executar `CindarsHope/Validate/Fable City Schedule (fable_11)` e exigir `PASS: 19 | FAIL: 0`.
4. Abrir `Assets/_Game/Scenes/TownScene.unity` e entrar em Play Mode.
5. Manter o Console aberto e limpar mensagens anteriores.

## Cenário 1 — Navegação e marcos

1. Entrar pelo portão sul vindo da fazenda.
2. Caminhar pela avenida principal e visitar praça central, mercado, templo de Kanthor, Câmara,
   Town Hall, cemitério, lago/cais, distrito de ofícios, estalagem e estrada da caverna.
3. Abrir e atravessar portas voltadas ao norte, sul, leste e oeste.
4. Entrar em diferentes casas e confirmar que o telhado desaparece e reaparece corretamente.
5. Caminhar junto às árvores internas: a colisão deve ficar no tronco, não na copa.
6. Tentar atravessar o lago: a água deve bloquear; o cais deve continuar acessível.

Resultado esperado: nenhuma casa, barraca, árvore ou prop bloqueia rota obrigatória; os prédios não
ficam sobre as vias; os marcos possuem leitura distinta; acesso sul, portais e spawns funcionam.

## Cenário 2 — Papéis e horários

### 10:00 — expediente

- Velorin permanece no `House_Chamber`/conselho e Tibbet no cemitério.
- Corvus permanece no templo; Mara/Tovin ficam nos serviços cívicos.
- Brumdar, Nimble, Ozzra, Mella, Hess, Sael e Eiran ficam junto aos respectivos serviços.
- Alaric/Hund/Dagna executam suas rotas de guarda/trabalho.

### 19:00 — período social

- lojistas deixam o trabalho e usam os pontos sociais definidos;
- Velorin usa o núcleo cívico/social, sem vagar por bairros aleatórios;
- taverna, praça, jardim e mercado recebem NPCs compatíveis com esses locais.

### 21:00–01:00 — cidade noturna

- Yael trabalha no mercado noturno entre 20:00 e 02:00;
- Maelor percorre sua rota noturna/cemitério;
- guardas continuam em serviço até 22:00;
- moradores seguem para suas casas pela aproximação da porta, sem atravessar paredes.

### 03:00 — madrugada

- moradores estão em casa/no anchor home;
- Yael e Maelor encerraram o trabalho noturno após 02:00;
- nenhum NPC atravessa prédio, água ou collider de árvore.

## Cenário 3 — Preservação e interação

1. Confirmar visualmente 24 prédios percorríveis e as 29 barracas.
2. Interagir com lojas/serviços em horário válido e fora do horário.
3. Abrir/fechar portas com E.
4. Confirmar que Q/E, HUD, inventário e save não sofreram alteração.

## Console

- Erros e exceções esperados: nenhum.
- Qualquer `ERROR`, `NullReferenceException`, anchor ausente ou path bloqueado reprova o cenário.

## Checklist

- [ ] Acesso sul e spawns funcionam.
- [ ] Todos os marcos são alcançáveis.
- [ ] Portas N/S/E/W abrem e têm aproximação livre.
- [ ] Roof reveal funciona nas casas testadas.
- [ ] Árvores colidem somente pelo tronco.
- [ ] Lago bloqueia e cais é acessível.
- [ ] Velorin e Tibbet trabalham nos locais corretos às 10:00.
- [ ] Lojistas respeitam 09–18 e social 18–22.
- [ ] Guardas respeitam 06–22.
- [ ] Yael/Maelor respeitam 20–02.
- [ ] Moradores usam home/porta sem atravessar paredes.
- [ ] Nenhuma regressão em interação, HUD, inventário ou save.
- [ ] Console sem erros.

**Resultado:** NOT RUN
**Testador:** pendente
**Data:** pendente
