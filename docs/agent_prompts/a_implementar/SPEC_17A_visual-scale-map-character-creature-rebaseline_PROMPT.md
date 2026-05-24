# Prompt para Claude Code/Codex — Cindar's Hope

Use este prompt dentro do repositório local `rafa210587/cindars_hope`, preferencialmente partindo da branch `dev` atualizada.

## Regra central desta rodada

O humano só fará validação manual no final de tudo. Portanto:

- Não pare no meio pedindo validação humana.
- Não use "validar manualmente depois" como critério de pronto intermediário.
- Rode as validações automatizadas/documentais disponíveis.
- Quando Play Mode exigir interação humana e você não conseguir executar, registre formalmente como `NOT RUN`, com motivo, comando tentado e risco residual.
- Prepare evidências e checklist para a validação humana final, mas siga para fechar a spec dentro do possível.
- Execute somente esta spec. Não avance para a próxima spec na mesma resposta/sessão, salvo se o humano colar explicitamente o próximo prompt.

## Leitura mínima obrigatória

Leia antes de alterar qualquer arquivo:

```text
AGENTS.md
CLAUDE.md
PROJECT_LOG.md                         # topo/entradas recentes
docs/IMPLEMENTATION_STATUS.md
docs/operations/AGENT_EXECUTION_PROTOCOL.md
docs/specs/SPEC_SOURCE_OF_TRUTH.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
memory/MEMORY.md
memory/project_skills_available.md
memory/feedback_working_method.md
```

## Regras invioláveis

- Fonte oficial de specs: `docs/specs/`. Não recriar `specs/` nem `spec/` na raiz.
- Não usar `GameObject.Find()`, `FindObjectOfType()` ou busca global em runtime para wiring de sistema.
- Comunicação de gameplay deve passar por `GameEventBus` quando cruzar sistemas.
- Dados de jogo/balanceamento devem ficar em `ScriptableObject`, não hardcoded em `MonoBehaviour`.
- Save deve persistir IDs e tipos simples. Nunca serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider`, `Rigidbody` ou refs Unity.
- Save editável deve usar `Application.persistentDataPath`; não usar `StreamingAssets`.
- Criar assets Unity via Editor script/menu idempotente, não YAML manual complexo.
- Não ampliar escopo para V2/FULL além do necessário para fechar esta spec.
- Não fazer `git push`, abrir PR, merge, reset hard, stash ou clean sem autorização explícita.
- Commits locais em português.

## Skills obrigatórias a usar

Use explicitamente os padrões abaixo de `memory/project_skills_available.md` quando aplicável:

1. `SPEC Validation Pattern` — rodar e interpretar validação Unity.
2. `Scene Wiring Validation Pattern` — conferir GameBootstrap, scene installers e scripts de criação de cena.
3. `Unity Asset Creation Pattern` — criar SOs/assets/configs via Editor script idempotente.
4. `Save/Load Data Pattern` — persistir apenas DTOs simples e IDs.
5. `Event Publishing Pattern` — eventos desacoplados via `GameEventBus`.
6. `Spec Closure / Registry Reconciliation Pattern` — atualizar status, registries, docs e log sem contradição.
7. `Play Mode Manual Validation Checklist` — registrar o checklist final mesmo se o Play Mode não puder ser executado por você.

## Comandos de validação obrigatórios

Se alterar docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Se alterar runtime Unity/C# ou assets/cenas:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

Se o caminho do Unity local for diferente, descubra o caminho instalado ou registre `NOT RUN` formalmente.

## Encerramento obrigatório

Ao terminar, entregue e registre em `PROJECT_LOG.md`:

```text
Arquivos alterados
Resumo técnico
Specs/refinements lidos
Skills usadas
Validações executadas
Validações NOT RUN, com motivo
Pendências reais
Risco residual
Commit local criado
Checklist Play Mode para validação humana final
```

---

# SPEC 17A — Visual Scale, Map Size, Character & Creature Size Rebaseline

## Branch sugerida

```bash
git checkout dev
git pull
git checkout -b feature/spec-17a-visual-scale-rebaseline
```

Se a branch já existir, reutilize-a. Antes de alterar, rode `git status` e registre se o working tree não estiver limpo.

## Por que esta spec existe

Houve uma decisão de design discutida para aumentar a legibilidade e a sensação de exploração do jogo:

```text
Mapas: aumentar em 4x.
Personagem: aumentar em 2x.
Criaturas: formalizar categorias de tamanho.
Exemplo:
- Tiny = 1x
- Average = 2x
- Large/Huge/Boss maiores conforme categoria.
```

Essa decisão não está consolidada como spec ativa no repo `dev`. Hoje o projeto ainda está documentado com base em:

```text
Tile: 32x32
Player: 32x48
NPC humanoide: 32x48
Inimigo placeholder: 32x32
Farm MVP: 20x16 tiles
```

Esta spec deve ser criada/executada antes da SPEC 17 UI/UX final, porque HUD, context hints, barras de vida, damage popup, câmera, checkpoint UI e offsets de UI dependem da escala real de gameplay.

## Spec alvo a criar ou consolidar

Se ainda não existir, crie a spec oficial em:

```text
docs/specs/a_implementar/spec_visual_scale_map_character_creature_rebaseline.md
```

Depois registre essa spec como `17A` nos documentos operacionais:

```text
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

Não recrie `specs/` ou `spec/` na raiz.

## Dependências diretas

```text
Depende de: 01-16 reconciliadas sem bloqueador crítico.
Executar antes de: 17 UI/UX final.
```

Se alguma spec anterior ainda estiver parcial, avalie se ela bloqueia escala visual. Exemplos:

- Player combat range e enemy AI precisam considerar novo tamanho.
- Cave generation precisa considerar footprints maiores.
- Death/corpse/checkpoints precisam considerar offsets/portais.
- Skill/action visuals precisam considerar player 2x.

## Objetivo desta execução

Criar e implementar o rebaseline de escala visual e espacial do jogo sem quebrar arquitetura, save, geração procedural, colisores, sorting ou validações Unity.

Esta spec deve deixar o projeto pronto para construir a UI final sobre escala estável.

## Decisões obrigatórias

### 1. Manter tile base e PPU

Manter:

```text
Tile = 32x32
Pixels Per Unit = 32
```

Não mudar PPU global.

### 2. Não usar Transform scale como solução final

Proibido usar como implementação estrutural:

```csharp
transform.localScale = new Vector3(2, 2, 1);
```

Pode existir apenas como debug temporário, nunca como solução final da spec.

Motivo:

- distorce pixel art;
- mascara tamanho real do sprite;
- quebra consistência de colliders;
- dificulta sorting;
- dificulta animação;
- dificulta hitboxes;
- complica prefab variants;
- cria bugs em VFX/projectiles.

### 3. Aumentar por asset, prefab, collider e profile

A escala correta deve vir de:

- sprites com canvas maior;
- colliders reconfigurados;
- pivots bottom-center;
- SortingPoint correto;
- size profiles em ScriptableObject;
- mapas maiores por quantidade de tiles;
- camera bounds e orthographic size ajustados;
- ranges e footprints recalibrados.

## Decisão sobre mapa 4x

Interpretar `mapa 4x` como:

```text
4x em área total, não 4x em cada eixo.
```

Portanto:

```text
largura x2
altura x2
área x4
```

Farm MVP atual:

```text
20x16 tiles = 320 tiles
```

Farm rebaseline:

```text
40x32 tiles = 1280 tiles
```

Não usar como padrão agora:

```text
80x64 tiles = 5120 tiles
```

Motivo: `80x64` seria 16x em área, não 4x. Pode virar expansão futura, não baseline desta spec.

## Decisão sobre player 2x

Atual:

```text
Player sprite: 32x48
```

Novo baseline:

```text
Player sprite: 64x96
```

Regra crítica:

```text
Sprite 2x, collider não 2x completo.
```

O collider do player deve representar o footprint/pé, não o corpo inteiro. Sugestão inicial:

```text
Player collider: 32x28 até 40x32 px equivalentes
Pivot: bottom-center
Sorting: por pé/base do sprite
```

Não transformar o player em bloco colidível 64x96.

## NPCs

NPC humanoide atual:

```text
32x48
```

Novo baseline:

```text
64x96
```

Regras:

- NPC fixo usa footprint similar ao player.
- NPC ambulante precisa de path com largura suficiente.
- Pip, lojistas e Fonte/Anya se aplicável devem usar offsets coerentes.
- Dialogue/camera/context hints devem considerar altura visual maior.

## Criaturas — categorias de tamanho

Criar enum ou estrutura equivalente:

```csharp
public enum CreatureSizeCategory
{
    Tiny,
    Small,
    Average,
    AverageHumanoid,
    Large,
    Huge,
    Boss
}
```

Tabela baseline:

| Categoria | Multiplicador | Sprite sugerido | Footprint sugerido | Uso |
|---|---:|---:|---:|---|
| Tiny | 1x | 32x32 | 1x1 tile ou menor | morcego, rato, slime pequeno |
| Small | 1.5x | 48x48 | 1x1 tile | aranha, goblin pequeno |
| Average | 2x | 64x64 | 1x1 a 2x1 tiles | inimigo comum médio |
| AverageHumanoid | 2x | 64x96 | footprint tipo player | cultista, guarda, humanoide |
| Large | 3x | 96x96 | 2x2 tiles | fera grande, elite |
| Huge | 4x | 128x128 | 3x3 tiles | golem, mini-boss grande |
| Boss | 5x-6x | 160x160 ou 192x192 | 4x4+ tiles | boss de gate/bioma |

Implementar via dados, não via ifs espalhados.

Criar ou adaptar:

```text
CreatureSizeProfileSO
ActorSizeProfileSO
SizeCategoryDataSO
```

Campos mínimos sugeridos:

```text
SizeCategory
SpriteCanvasWidth
SpriteCanvasHeight
FootprintTilesX
FootprintTilesY
ColliderSize
ColliderOffset
InteractionRadius
AggroRadiusMultiplier
AttackRangeMultiplier
ProjectileSpawnOffset
HealthBarOffset
DamagePopupOffset
ShadowScale
CanPassOneTileCorridor
RequiresLargeSpawnAnchor
```

## Mapas e geração procedural

### FarmScene

Atualizar scripts/editor installers para gerar layout 40x32.

Regras:

- Não escalar Tilemap por transform.
- Aumentar quantidade de tiles.
- Reposicionar zonas: plots, lago, árvores, SellPoint/deprecated sell point, portal town/cave, workstations.
- Aumentar espaços entre objetos para player/NPC 2x.
- Câmera deve ter bounds corretos.
- Spawn inicial deve não colidir com nada.

### TownScene

Rebaseline para área 4x do layout MVP atual.

Regras:

- Ruas largas o suficiente para NPCs 2x.
- Pip consegue se aproximar do player sem bloquear portal.
- Lojistas têm área de interação clara.
- NPC ambulante tem path seguro.

### CaveScene

Não mudar stable run semantics.

Regras:

- Aumentar dimensões por config/profiles de geração.
- Aumentar largura mínima de corredor.
- Aumentar padding de salas.
- Spawn anchors devem validar footprint da criatura.
- Boss rooms precisam suportar Boss/Huge sem overlap.
- ForwardExit e BackExit não podem regenerar nível já visitado na mesma CaveRunSeed.
- Snapshots devem continuar válidos e persistentes.

## Sistemas afetados que devem ser auditados

Antes de alterar, pesquisar e auditar:

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Town/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/**/Editor/**
Assets/_Game/Data/**
```

## Áreas permitidas para alteração

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Town/**
Assets/_Game/Scripts/UI/**                 # somente offsets/hooks necessários, não UI final
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Save/**               # somente se size profile precisa persistir por ID
Assets/_Game/Scripts/**/Editor/**
Assets/_Game/Data/**
Assets/_Game/Prefabs/**
Assets/_Game/Scenes/**
docs/specs/a_implementar/spec_visual_scale_map_character_creature_rebaseline.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Fora de escopo

Não fazer nesta spec:

- UI final completa da SPEC 17.
- Arte final de sprites.
- Roster completo novo de monstros.
- Balanceamento final de dano/HP.
- Minimap.
- Controller/gamepad completo.
- Novos biomas.
- Novas quests.
- Mudar PPU global.
- Escalar tudo com transform.
- Reescrever toda a geração procedural do zero.

## Implementação mínima esperada

### Código/dados

- Criar categoria formal de tamanho para criaturas/atores.
- Adicionar size category/profile em EnemyDataSO/CreatureDataSO ou modelo oficial equivalente.
- Criar size profiles oficiais via Editor script idempotente.
- Atualizar player/NPC size baseline para 64x96 em dados/prefabs/placeholders.
- Atualizar colliders e interaction radius.
- Atualizar combat ranges e projectile spawn offsets quando dependem do tamanho.
- Atualizar health bar/damage popup offsets.
- Atualizar cave spawn validation para footprints maiores.
- Atualizar scene creation scripts para mapas 4x área.
- Atualizar camera bounds/orthographic size se necessário.

### Assets placeholder

Pode criar placeholders maiores programaticamente ou via Editor script:

```text
Player_Placeholder_64x96.png
NPC_Placeholder_64x96.png
Enemy_Tiny_Placeholder_32x32.png
Enemy_Small_Placeholder_48x48.png
Enemy_Average_Placeholder_64x64.png
Enemy_AverageHumanoid_Placeholder_64x96.png
Enemy_Large_Placeholder_96x96.png
Enemy_Huge_Placeholder_128x128.png
Enemy_Boss_Placeholder_160x160.png
```

Se não for viável gerar PNGs reais no ambiente, criar Editor script que gera Texture2D/PNG ou registrar `NOT RUN` com risco residual.

## Critérios de aceite

- A spec 17A existe em `docs/specs/a_implementar/` ou foi consolidada em local oficial equivalente dentro de `docs/specs/`.
- `SPEC_EXECUTION_ORDER.md` inclui 17A antes da SPEC 17.
- Mapas MVP principais são rebaseline para 4x área, não transform scale.
- Player/NPC baseline está em 64x96 ou decisão alternativa justificada por evidência.
- Criaturas têm categoria formal de tamanho.
- Pelo menos Tiny, Average e Boss têm size profiles funcionais.
- Cave spawn/rooms validam footprint ou registram bloqueio real se o sistema ainda não suportar.
- Colliders usam footprint, não sprite inteiro.
- Camera/bounds não deixam o player sair da área visível/jogável indevidamente.
- Damage popup/healthbar/context hint têm offsets coerentes com escala maior ou estão preparados para SPEC 17.
- Unity compile validation passa ou registra `NOT RUN` formalmente.
- Play Mode checklist é criado para validação humana final.

## Validação Unity obrigatória

Rodar:

```powershell
.\tools\docs\validate_docs.ps1

.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

## Play Mode checklist obrigatório

Registrar no resumo e em `docs/validation/`:

```text
PLAY MODE TEST: SPEC 17A — Visual Scale Rebaseline
Scene used: FarmScene / TownScene / CaveScene
Steps executed:
  1. Abrir FarmScene rebaseline 40x32.
  2. Mover player 64x96 em todas as direções.
  3. Confirmar que o collider não prende em bordas/objetos com espaço adequado.
  4. Confirmar câmera/bounds.
  5. Entrar na TownScene e validar NPC 64x96 + interação.
  6. Entrar na CaveScene e validar spawn Tiny/Average/Boss se disponíveis.
  7. Validar healthbar/damage popup/context hint offsets.
Expected result: escala maior funcional, sem colisão quebrada, sem câmera quebrada, sem spawn inválido.
Observed result:
Bugs found:
Passed: YES/NO/NOT RUN
Evidence:
```

## Commit

Crie commit local em português:

```bash
git add <arquivos>
git commit -m "feat: adicionar rebaseline visual de escala e mapas"
```

Não faça push.
