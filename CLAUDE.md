# CLAUDE.md — Cindar's Hope

## Contexto do projeto
Jogo 2D pixel art RPG + farm sim desenvolvido em Unity LTS com C#.
Mundo: Vaalara, cidade Cindar's Hope, região Dornecia.
Arte: Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback, sprites 32x32px, resolução 1280x720.
IA de arte: ChatGPT/DALL-E para conceito e ícones simples; PixelLab/Scenario opcionais para sprites/tilesets; Aseprite obrigatório para acabamento final.
Geração de código: Codex (VS Code) + Claude.
Spec: GitHub SpecKit com fluxo Specify → Plan → Tasks → Implement.

## Regra operacional de continuidade

Antes de qualquer tarefa, ler obrigatoriamente:

1. `PROJECT_LOG.md` — estado operacional, decisões recentes, pendências e smoke tests.
2. `AGENTS.md` e/ou `CLAUDE.md` — regras permanentes de agente.
3. Documentos de referência do PR/tarefa.

Ao final de qualquer tarefa relevante, atualizar `PROJECT_LOG.md` com:

- branch usada;
- escopo executado;
- arquivos alterados;
- testes executados ou não executados;
- pendências/riscos;
- próximo passo recomendado.

`PROJECT_LOG.md` é append-only por padrão: não apagar histórico anterior salvo correção factual explícita.

## Documentos de referência (ler antes de qualquer tarefa)
- PROJECT_LOG.md — log operacional e continuidade entre agentes
- docs/GDD_v2.6.md — design completo do jogo
- docs/ARCH_fase4_v2.2.md — arquitetura técnica, padrões, eventos
- docs/FASE5_ambiente_v1.2.md — setup micro do ambiente
- docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md — spec da próxima fase
- specs/ — specs por sistema

## Modelo de LLM padrão
claude-sonnet-4-6

## Regras INVIOLÁVEIS de código

1. NUNCA usar GameObject.Find() ou FindObjectOfType()
   → Usar injeção via [SerializeField] no Inspector ou eventos
2. NUNCA criar comunicação direta entre sistemas
   → Sempre via GameEventBus.Publish() e Subscribe()
3. NUNCA hardcodar dados de jogo (HP, dano, preços, nomes)
   → Sempre em ScriptableObject no Assets/_Game/Data/
4. SEMPRE fazer Unsubscribe em OnDisable ou OnDestroy
   → void OnDisable() => GameEventBus.Unsubscribe<XEvent>(OnX);
5. NUNCA escrever lógica de negócio em MonoBehaviour
   → MonoBehaviour só faz ponte entre Unity e classes C# puras
6. SEMPRE prefixar ScriptableObjects: ItemDataSO, SeedDataSO, ToolDataSO, WeaponDataSO etc.
7. SEMPRE prefixar eventos: DayStartedEvent, ToolEquippedEvent, PlayerDodgeStartedEvent etc.
8. SEMPRE commits em português
9. NUNCA implementar feature sem spec aprovada
10. Sprites: SEMPRE importar com Filter Mode Point + Compression None
11. Direção visual: cozy farm pixel art inspirado por Harvest Moon/Stardew Valley, mas com identidade própria; não copiar assets, personagens, UI ou paleta proprietária
12. SEMPRE atualizar `PROJECT_LOG.md` ao final de tarefa relevante
13. Save deve persistir IDs e tipos simples, nunca referências Unity

## Regras FASE 9C — Tools/Farm/Combat

1. Implementar PRs pequenos: contratos → assets → manager → integração → validação.
2. Não misturar Tool System, Weapon System, Dodge, UI e arte no mesmo PR.
3. Ferramentas devem usar `ToolDataSO`, `ToolTier`, `ToolRequirement` e `ToolDatabaseSO`.
4. Armas devem usar `WeaponDataSO`, `WeaponType` e `WeaponDatabaseSO`.
5. `EquipmentManager` deve persistir `EquippedToolId` e `EquippedWeaponId` por save.
6. Plantio não deve escolher seed automaticamente de forma fixa; usar seed explícita/ativa.
7. Árvore só deve progredir corte real com Axe; fallback sem Axe não deve cortar a árvore.
8. FishingSpot deve migrar para ToolRequirement, não item ID hardcoded.
9. Dodge deve ficar em `PlayerDodgeController`, com cooldown e invulnerabilidade curta.
10. Ranged físico e magia devem usar projectile controller sem depender de tags.
11. Detecção de gameplay deve preferir componentes a tags.

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

## Paleta de cores (hex)

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

## Documentos operacionais adicionais

- `PROJECT_LOG.md` — log operacional e continuidade obrigatória entre agentes.
- `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md` — spec aprovada para próxima fase.
- `docs/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md` — delta de roadmap da FASE 9C.
- `docs/ARCH_fase4_v2.3_FASE9C_DELTA.md` — delta arquitetural.
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` — delta de contratos/eventos/save/IDs.
- `docs/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` — pipeline de arte com IA + Aseprite.

## Regras de arte com IA

1. Não copiar assets, paleta proprietária, UI, personagens ou sprites de Stardew Valley/Harvest Moon.
2. Usar referências apenas como direção de gênero: legibilidade, câmera, conforto visual.
3. IA pode gerar conceito, variações e rascunhos.
4. Aseprite é a etapa final obrigatória para limpar pixels, paleta, outline e export.
5. Todo sprite aprovado deve ter `.aseprite/.ase` fonte quando houver edição manual relevante.
6. PNG final entra em `Assets/_Game/Sprites/...` com import `Point`, `Compression None`, `Generate Mip Maps false`.

## Fluxo de agentes — Git e entrega

### Agentes preparam, humanos entregam por padrão
- Agentes criam commits locais em português.
- Agentes não executam push/PR/merge sem pedido humano explícito.
- Push, PR/MR, merge e deleção de branches são responsabilidade humana por padrão.

### Ao final de cada PR/pacote, agente entrega
- Commits criados.
- Arquivos alterados por PR.
- Testes executados e testes pendentes.
- Instruções reproduzíveis para validação local.

Veja [AGENTS.md](AGENTS.md) para detalhes completos do fluxo de trabalho.
