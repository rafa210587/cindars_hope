# Cenário humano — expansão espacial e acessibilidade da Town v1

**Status:** NOT RUN — validação final agrupada
**Tempo estimado:** 12 minutos
**Plataforma:** Windows / Unity Play Mode

## Resumo

Validar que a Town 160×112 preserva seus habitantes e serviços, oferece entradas confortáveis e
permite que Player e NPCs se cruzem sem bloqueio, mantendo a escala visual dos prédios em 1,20.

## Estado inicial

- Abrir `Assets/_Game/Scenes/TownScene.unity` gerada pela spec.
- Iniciar pelo spawn `town_from_farm`, sem alterar velocidade, colliders ou schedules.
- Console limpo; nenhum save especial é necessário.

## Cenário 1 — eixo sul, praça e mercado

1. Caminhar do portão sul até a fonte central pela avenida principal.
2. Cruzar por pelo menos um NPC no sentido oposto sem sair da via.
3. Contornar as seis bancas e entrar em Bakery e MarketHall.
4. Sair das duas construções e retornar à praça.

Esperado: nenhum engasgo entre collider de NPC/prop; portas, degraus e aprons permanecem visíveis e
acessíveis; telhados revelam o interior sem bloquear a saída.

## Cenário 2 — serviços e bordas

1. Ir da praça a Blacksmith, Alchemy, Workshop e Tannery.
2. Entrar em três serviços e girar no interior antes de sair.
3. Seguir até Temple, Town Hall/mural, lago/cais, AnimalYard e saída leste.
4. Confirmar que nenhuma rota exige atravessar água, prédio, cerca sólida ou floresta de borda.

Esperado: Player e NPCs alcançam seus pontos de trabalho; há espaço perceptível entre construções;
entrada do AnimalYard e os três acessos externos continuam funcionais.

## Cenário 3 — preservação e regressão

1. Observar NPCs na praça, comércio, ofícios e residências durante um ciclo de schedule.
2. Verificar visualmente que não há prédios ausentes, fachadas ocultas ou chão/câmera cortados.
3. Fazer Town→Farm→Town pelos portais existentes.
4. Confirmar que a posição de retorno é válida e que nenhuma cena/save foi alterado pelo probe.

## Console

- Esperado: `[TownAccessPlayMode] PASS:8 FAIL:0` na execução automatizada correspondente.
- Proibido: qualquer `ERROR`, `Exception`, ator preso ou portal sem destino.

## Checklist

- [ ] Spawn sul→praça e cruzamento bidirecional confortáveis.
- [ ] Bakery/MarketHall e três interiores de serviço acessíveis.
- [ ] Temple, Town Hall, lago, AnimalYard e saída leste alcançáveis.
- [ ] 29 NPCs e 24 prédios preservados visualmente.
- [ ] Nenhuma fachada/porta/escada gravemente ocultada.
- [ ] Ground, muralha, água e câmera cobrem os bounds 160×112.
- [ ] Sem erros no Console e sem regressão nos portais.

**Resultado final:** PASS / FAIL — preencher após execução humana.
