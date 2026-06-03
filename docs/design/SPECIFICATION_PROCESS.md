# Cindar's Hope — Specification Process

> **Status:** processo canônico para criação/refinamento de specs  
> **Local:** `docs/design/SPECIFICATION_PROCESS.md`  
> **Função:** definir como transformar design directions em specs implementáveis para Codex/Claude Code/Unity.  
> **Regra:** toda spec nova deve seguir este processo e declarar suas fontes.

---

## 1. Objetivo

Este documento define como iremos especificar o projeto.

Ele existe para evitar que specs sejam criadas a partir de memória solta, conversa incompleta ou leitura excessiva do repo inteiro.

Toda spec deve nascer de:

```text
1. Mapa de fontes
2. Canon de Vaalara
3. Design direction do domínio
4. Estado real do repo
5. Escopo implementável pequeno
6. Critérios de aceite claros
7. Validação Unity explícita
```

---

## 2. Regra principal

Antes de criar qualquer spec, ler:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Depois, ler os documentos do domínio indicado pelo `SPEC_SOURCE_MAP.md`.

Exemplo para cidade:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Exemplo para fazenda:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
```

---

## 3. Diferença entre design e spec

Design direction responde:

```text
Como o sistema deve funcionar no jogo?
Qual é a intenção de produto?
Qual lore sustenta isso?
Quais regras de gameplay existem?
Como isso se conecta ao resto do jogo?
```

Spec implementável responde:

```text
O que será implementado agora?
Quais arquivos podem mudar?
Quais dados/eventos serão criados ou alterados?
O que está fora do escopo?
Como validar no Unity?
Quais critérios provam que a entrega está correta?
```

---

## 4. Pipeline de especificação

```text
Design direction canônico
  ↓
Escolha de uma fatia implementável pequena
  ↓
Leitura do SPEC_SOURCE_MAP.md
  ↓
Leitura das fontes obrigatórias
  ↓
Revalidação do estado real do repo
  ↓
Criação da spec em docs/specs/a_implementar/
  ↓
Implementação por Codex/Claude Code
  ↓
Validação automatizada quando existir
  ↓
Validação Unity
  ↓
Validação humana final
  ↓
Registro em docs/specs/implementados/ e/ou refinements/implementados/
```

---

## 5. Template obrigatório de spec

Toda spec deve usar esta estrutura mínima:

```md
# Cindar's Hope — <Nome da Spec>

> Status: a implementar
> Tipo: spec implementável
> Domínio: <farm/city/cave/ui/combat/etc>

## Fontes obrigatórias lidas

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/design/...

## Objetivo

## Escopo

## Fora de escopo

## Estado atual do repo

## Dependências

## Contratos, dados e eventos

## Arquivos permitidos

## Arquivos proibidos

## Passos de implementação

## Critérios de aceite

## Validação automatizada

## Validação Unity

## Validação humana final

## Riscos

## Rollback
```

---

## 6. Estado atual do repo

A seção `Estado atual do repo` deve ser obrigatória.

Ela deve responder:

```text
Quais arquivos/sistemas já existem?
O que já está implementado?
O que é parcial?
O que a spec não deve recriar?
```

Regra:

```text
Não pedir para o agente recriar sistema existente sem antes confirmar estado real no repo.
```

---

## 7. Arquivos permitidos e proibidos

Toda spec precisa declarar limites.

Exemplo:

```md
## Arquivos permitidos

- Assets/_Game/Scripts/City/...
- Assets/_Game/Scripts/NPC/...
- Assets/_Game/Data/City/...

## Arquivos proibidos

- Assets/_Game/Scripts/Core/GameEventBus.cs
- Assets/_Game/Scripts/Save/... salvo se explicitamente necessário
- docs/design/... durante implementação
```

Motivo:

- reduzir blast radius;
- evitar mudanças em core contracts sem necessidade;
- facilitar review;
- proteger save/event bus.

---

## 8. Critérios de aceite

Critérios devem ser verificáveis.

Ruim:

```text
A cidade deve ficar boa.
```

Bom:

```text
Ao entrar na CityScene pelo spawn_from_farm, o player aparece no caminho sul da cidade.
O player não atravessa paredes, água, balcões, estátuas ou poço lacrado.
A porta da taverna abre o interior correto durante horário permitido.
A porta de loja fechada mostra mensagem com horário.
```

---

## 9. Validação Unity

Toda spec precisa ter seção de validação Unity.

Formato recomendado:

```md
## Validação Unity

1. Abrir a cena `<SceneName>`.
2. Entrar em Play Mode.
3. Executar os passos abaixo.
4. Confirmar resultados esperados.
```

Exemplo:

```md
1. Abrir `CityScene`.
2. Entrar pelo spawn `spawn_from_farm`.
3. Caminhar até Praça Central.
4. Confirmar colisão em bancos, fonte pública e paredes.
5. Entrar na Taverna durante horário aberto.
6. Alterar hora para noite e confirmar loja noturna habilitada apenas sob condição correta.
```

A validação humana final deve ser deixada para o usuário no final do ciclo, não a cada microetapa.

---

## 10. Como quebrar specs grandes

Uma spec deve ser pequena o bastante para um agente implementar e validar sem alterar o jogo inteiro.

Exemplo de quebra correta para cidade:

```text
1. spec_city_scene_tilemap_collision_spawns.md
2. spec_city_buildings_exteriors_and_doors.md
3. spec_city_core_interiors_shops_services.md
4. spec_city_npc_residences_beds_schedule_markers.md
5. spec_city_npc_pathfinding_waypoints.md
```

Não criar uma spec única chamada:

```text
spec_implementar_cidade_inteira.md
```

---

## 11. Roadmap dentro dos design docs

Todo design direction importante deve ter um roadmap conceitual.

O roadmap deve seguir este padrão:

```text
Roadmap 0 — Reconciliar base existente
Roadmap 1 — Base navegável/jogável essencial
Roadmap 2 — Serviços/sistemas principais
Roadmap 3 — Simulação/estado/save
Roadmap 4 — Integrações sociais/econômicas
Roadmap 5 — Eventos/variações/lore
Roadmap 6 — Endgame/segredos/sistemas avançados
```

Não precisa usar exatamente os mesmos nomes, mas deve mostrar:

- ordem lógica;
- dependências;
- o que vem antes;
- o que não deve ser implementado cedo;
- como quebrar specs futuras.

---

## 12. Prompt padrão para Codex/Claude Code

Quando uma spec for passada para Codex/Claude Code, usar prompt semelhante:

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

Antes de implementar, leia obrigatoriamente:
- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- <fontes listadas na spec>
- <arquivos do estado atual indicados na spec>

Implemente apenas o escopo desta spec.
Não altere arquivos fora de "Arquivos permitidos".
Não altere arquivos listados como proibidos.
Não reimplemente sistemas existentes.
Mantenha compatibilidade com save/event bus/contracts existentes.

Depois de implementar:
- rode os testes/validações automatizadas disponíveis;
- gere um resumo técnico;
- liste arquivos alterados;
- liste pendências;
- explique como validar no Unity.

A validação humana será feita apenas no final pelo usuário.
```

---

## 13. Checklist antes de considerar uma spec pronta

```text
[ ] A spec declara fontes obrigatórias lidas.
[ ] A spec cita o design direction correto.
[ ] A spec tem escopo pequeno.
[ ] A spec tem fora de escopo claro.
[ ] A spec revalida estado real do repo.
[ ] A spec lista arquivos permitidos.
[ ] A spec lista arquivos proibidos.
[ ] A spec define dados/eventos/contratos.
[ ] A spec tem critérios de aceite verificáveis.
[ ] A spec tem validação Unity.
[ ] A spec indica riscos e rollback.
[ ] A spec não contradiz Vaalara.
[ ] A spec não contradiz documentos canônicos do domínio.
```

---

## 14. Regra de atualização documental

Ao refinar uma área já documentada:

```text
Atualizar o documento canônico existente.
Não criar versões paralelas.
Usar histórico do Git como versionamento.
```

Ao criar uma spec:

```text
Criar um arquivo novo, pois cada spec é uma unidade implementável separada.
```

---

## 15. Resultado esperado

O projeto deve ficar com três níveis claros:

```text
docs/design/
  visão e direção do jogo

 docs/design/SPEC_SOURCE_MAP.md
  quais fontes ler para cada spec

 docs/design/SPECIFICATION_PROCESS.md
  como criar specs corretamente

 docs/specs/a_implementar/
  specs pequenas e implementáveis

 docs/specs/implementados/
  registro do que foi implementado
```
