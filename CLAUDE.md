# CLAUDE.md — Cindar's Hope

## Contexto do projeto

Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, região Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resolução 1280x720.
IA de arte: ChatGPT/DALL-E para conceito e ícones simples; PixelLab/Scenario opcionais para sprites/tilesets; Aseprite obrigatório para acabamento final.
Geração de código: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify → Plan → Tasks → Implement.

---

## Regra operacional de continuidade

Antes de qualquer tarefa, ler obrigatoriamente:

1. `PROJECT_LOG.md` — log operacional, decisões recentes, pendências e próximo passo.
2. `docs/IMPLEMENTATION_STATUS.md` — tracking curto de capacidades/specs implementadas e pendentes.
3. `AGENTS.md` e/ou `CLAUDE.md` — regras permanentes de agente.
4. Documentos de referência do PR/tarefa.
5. Se o trabalho tocar specs, ler `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

Ao final de qualquer tarefa relevante, atualizar obrigatoriamente:

- `PROJECT_LOG.md` com branch usada, escopo executado, arquivos alterados, testes executados/não executados, pendências/riscos e próximo passo recomendado.
- `docs/IMPLEMENTATION_STATUS.md` com status curto, verificável e comparável de capacidades/specs.

`PROJECT_LOG.md` é append-only por padrão: não apagar histórico anterior salvo correção factual explícita.

`docs/IMPLEMENTATION_STATUS.md` deve ser curto: marcar `Implementado`, `Implementado parcial`, `Especificado` ou `Pendente`; se houver dúvida, marcar `Parcial` e registrar pendência.

---

## Regra operacional de Git para agentes

Agentes podem preparar commits locais, mas não devem executar operações remotas ou destrutivas, salvo pedido humano explícito nesta conversa.

Permitido ao agente:

- criar ou usar branch local indicada pelo humano;
- alterar somente arquivos explicitamente permitidos no escopo da tarefa;
- criar commits locais em português;
- atualizar `PROJECT_LOG.md` ao final de tarefa relevante;
- atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante;
- entregar ao humano a lista de commits, arquivos alterados, testes executados e testes pendentes.

Proibido ao agente sem autorização explícita:

- executar `git push`;
- abrir PR/MR;
- fazer merge;
- deletar branches locais ou remotas;
- executar `git stash`;
- executar `git clean`;
- executar `git reset --hard`;
- commitar arquivos fora do escopo permitido.

Push, PR/MR, merge e limpeza de branches são responsabilidade humana por padrão, salvo autorização explícita na sessão.

---

## Documentos de referência

Ler antes de qualquer tarefa:

- `PROJECT_LOG.md` — log operacional e continuidade entre agentes.
- `docs/IMPLEMENTATION_STATUS.md` — tracking de capacidades/specs.
- `docs/SPEC_EVOLUTION_POLICY_v1.0.md` — política para specs aprovadas, amendments e correções.
- `docs/GDD_v2.6.md` — design do jogo completo.
- `docs/ARCH_fase4_v2.2.md` — arquitetura técnica, padrões, eventos.
- `docs/FASE5_ambiente_v1.2.md` — setup micro do ambiente.
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` — próxima fase recomendada: Cave procedural/resources.
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md` — SpecKit funcional da próxima fase.
- `specs/` — specs por sistema.

---

## Modelo de LLM padrão

claude-sonnet-4-6

---

## Regras INVIOLÁVEIS de código

1. NUNCA usar `GameObject.Find()` ou `FindObjectOfType()`.
   - Usar injeção via `[SerializeField]` no Inspector, installers de cena ou eventos.
2. NUNCA criar comunicação direta entre sistemas quando a comunicação for de gameplay.
   - Usar `GameEventBus.Publish()` e `Subscribe()`.
3. NUNCA hardcodar dados de jogo em `MonoBehaviour` quando forem dados de balanceamento/conteúdo.
   - Usar ScriptableObject em `Assets/_Game/Data/`.
4. SEMPRE fazer unsubscribe em `OnDisable` ou `OnDestroy`.
5. NUNCA escrever lógica de negócio pesada em `MonoBehaviour`.
   - `MonoBehaviour` deve ser ponte Unity/runtime; lógica deve ser isolável quando possível.
6. SEMPRE prefixar ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, `WeaponDataSO`, etc.
7. SEMPRE prefixar eventos: `DayStartedEvent`, `ItemCraftedEvent`, `ToolEquippedEvent`, etc.
8. SEMPRE commits em português.
9. NUNCA implementar feature sem spec aprovada.
10. Sprites: SEMPRE importar com Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`.
11. Direção visual: cozy farm pixel art inspirado por Harvest Moon/Stardew Valley, mas com identidade própria; não copiar assets, personagens, UI ou paleta proprietária.
12. SEMPRE atualizar `PROJECT_LOG.md` ao final de tarefa relevante.
13. SEMPRE atualizar `docs/IMPLEMENTATION_STATUS.md` ao final de tarefa relevante.
14. Save deve persistir IDs e tipos simples, nunca referências Unity.
15. Não usar `StreamingAssets` para save editável; usar `Application.persistentDataPath`.
16. Não serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody` em DTOs de save.

---

## Regras de manutenção do tracking

Ao implementar uma spec:

- mover o item correspondente de pendente/especificado para implementado ou implementado parcial;
- registrar evidência curta no repo: arquivo principal, manager, asset, cena ou validator;
- registrar pendências reais de validação ou polish;
- não marcar como implementado sem evidência.

Ao criar nova spec aprovada:

- adicionar no bloco de pendências de `docs/IMPLEMENTATION_STATUS.md`;
- não apagar specs antigas aprovadas;
- se uma spec antiga mudar, criar amendment/correction/errata conforme `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

---

## Regras FASE9F — Cave Procedural/Resources

1. Implementar PRs pequenos: contratos → generator → run regeneration → checkpoints → resource contracts → runtime nodes → save/load → spawn → loot → XP → refresh → boss → validator.
2. Não misturar generator procedural, ResourceNode runtime, loot tables, XP e boss no mesmo PR.
3. Cave usa `CaveWorldSeed` persistente e `CaveRunSeed` por run.
4. KO/derrota gera nova `CaveRunSeed` e preserva checkpoints.
5. Checkpoints oficiais: `1, 15, 30, 45, 60, 75, 90`.
6. ResourceNode exige ferramenta, tier e stamina.
7. Minério sem Pickaxe/tier suficiente gera apenas fallback `1x item_material_stone`, não entrega minério principal e não depleta o node.
8. Nodes com `RespawnsDaily = true` renovam no novo dia; os demais permanecem depleted.
9. Save de cave deve conter somente DTOs e tipos simples.
10. Slime especial deve ter visual/cor diferente, não só stats maiores.
11. Boss de mudança de bioma bloqueia avanço.

---

## Regra FASE9F — Cave Stable Run

Antes de qualquer alteração em Cave procedural, agentes devem ler:

- `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/amendments/stable_run_replay.md`
- `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md`
- `docs/audits/PR170_192_CAVE_STABLE_RUN_PRE_IMPLEMENTATION_AUDIT.md`

Regra central:

- `CaveLevel` já visitado dentro da mesma `CaveRunSeed` deve ser carregado por snapshot.
- `ForwardExit` e `BackExit` não podem regenerar layout, composição de inimigos ou composição de resource nodes.
- Procedural só muda em novo jogo, KO/morte/derrota do personagem ou comando debug explícito.
- Enemy count por snapshot novo de level deve ficar entre `12` e `20`.
- Resource node count por snapshot novo de level deve ficar entre `4` e `10`.
- Revisitar level não pode rerollar inimigos, resource nodes, layout, entrada ou saída.
- `SaveData` de cave deve persistir snapshots com tipos simples e sem Unity refs.
- `UnityEngine.Camera` deve ser usado explicitamente quando o tipo for a câmera da Unity, para evitar colisão com `CindarsHope.Camera`.

---

## Convenções de nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Classes | PascalCase | PlayerController, EquipmentManager |
| Eventos | [Acao][Substantivo]Event | DayStartedEvent, ToolEquippedEvent |
| ScriptableObjects | [Tipo]DataSO | ItemDataSO, ToolDataSO, WeaponDataSO |
| Prefabs | [Categoria]_[Nome] | Creature_Slime, NPC_Brumdar |
| Sprites | [Cat]_[Nome]_[Tamanho].png | Item_SwordIron_32x32.png |
| Variáveis private | _camelCase | _currentHP, _isGrounded |
| Variáveis public/[SerializeField] | PascalCase | MaxHP, MoveSpeed |
| Constantes | UPPER_SNAKE | MAX_COMPANIONS, BASE_HUNGER_RATE |
| Cenas | PascalCase | FarmScene, TownScene, CaveScene |

---

## Estrutura de pastas

```text
Assets/_Game/
├── Data/
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Farm/
│   ├── Cave/
│   ├── Combat/
│   ├── Tools/
│   ├── Equipment/
│   ├── Craft/
│   ├── Companion/
│   ├── NPC/
│   ├── UI/
│   ├── Save/
│   └── Utils/
├── Scenes/
├── Prefabs/
├── Sprites/
├── Animations/
├── Tilemaps/
└── Audio/
```

---

## Padrão de MonoBehaviour

```csharp
public class NomeDoSistema : MonoBehaviour
{
    [SerializeField] private NomeDataSO _data;

    void OnEnable() => GameEventBus.Subscribe<XEvent>(OnX);
    void OnDisable() => GameEventBus.Unsubscribe<XEvent>(OnX);

    private void OnX(XEvent e)
    {
        // reagir ao evento
    }
}
```

---

## Padrão de ScriptableObject

```csharp
[CreateAssetMenu(fileName = "X_NomeDoItem", menuName = "CindarsHope/Categoria/Tipo")]
public class TipoDataSO : ScriptableObject
{
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;
}
```

---

## Paleta de cores

| Ambiente | Cor | Hex |
|---|---|---|
| Fazenda | Laranja | #D4832A |
| Fazenda | Bronze | #8B6914 |
| Fazenda | Terra | #6B3A2A |
| Fazenda | Verde musgo | #4A6741 |
| Cidade | Azul ardósia | #4A5E7A |
| Cidade | Pedra | #7A8A9A |
| Cidade | Âmbar lanterna | #D4A850 |
| Caverna | Cinza carvão | #2A2A2A |
| Caverna | Roxo escuro | #3A1F4A |
| Caverna | Cinza úmido | #4A4A5A |
| Global | Outline personagens | #0A0A0A |

---

## Documentos operacionais adicionais

- `PROJECT_LOG.md` — log operacional e continuidade obrigatória entre agentes.
- `docs/IMPLEMENTATION_STATUS.md` — status curto de capacidades/specs.
- `docs/SPEC_EVOLUTION_POLICY_v1.0.md` — política de evolução de specs.
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` — spec aprovada para Cave procedural/resources.
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md` — SpecKit da FASE9F.
- `docs/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` — pipeline de arte com IA + Aseprite.

---

## Regras de arte com IA

1. Não copiar assets, paleta proprietária, UI, personagens ou sprites de Stardew Valley/Harvest Moon.
2. Usar referências apenas como direção de gênero: legibilidade, câmera, conforto visual.
3. IA pode gerar conceito, variações e rascunhos.
4. Aseprite é a etapa final obrigatória para limpar pixels, paleta, outline e export.
5. Todo sprite aprovado deve ter `.aseprite/.ase` fonte quando houver edição manual relevante.
6. PNG final entra em `Assets/_Game/Sprites/...` com import `Point`, `Compression None`, `Generate Mip Maps false`.
