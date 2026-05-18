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
- PROJECT_LOG.md             — log operacional e continuidade entre agentes
- docs/GDD_v2.6.md           — design completo do jogo
- docs/ARCH_fase4_v2.2.md    — arquitetura técnica, padrões, eventos
- docs/FASE5_ambiente_v1.2.md — setup micro do ambiente
- specs/                      — specs por sistema (geradas na Fase 7)

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
6. SEMPRE prefixar ScriptableObjects: ItemDataSO, SeedDataSO, etc.
7. SEMPRE prefixar eventos: DayStartedEvent, PlayerDiedEvent, etc.
8. SEMPRE commits em português
9. NUNCA implementar feature sem spec aprovada (Fase 7+)
10. Sprites: SEMPRE importar com Filter Mode Point + Compression None
11. Direção visual: cozy farm pixel art inspirado por Harvest Moon/Stardew Valley, mas com identidade própria; não copiar assets, personagens, UI ou paleta proprietária
12. SEMPRE atualizar `PROJECT_LOG.md` ao final de tarefa relevante

## Convenções de nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Classes | PascalCase | PlayerController, FarmSystem |
| Eventos | [Acao][Substantivo]Event | DayStartedEvent, ItemCraftedEvent |
| ScriptableObjects | [Tipo]DataSO | ItemDataSO, SeedDataSO |
| Prefabs | [Categoria]_[Nome] | Creature_Slime, NPC_Brumdar |
| Sprites | [Cat]_[Nome]_[Tamanho].png | Item_SwordIron_32x32.png |
| Variáveis private | _camelCase | _currentHP, _isGrounded |
| Variáveis public/[SerializeField] | PascalCase | MaxHP, MoveSpeed |
| Constantes | UPPER_SNAKE | MAX_COMPANIONS, BASE_HUNGER_RATE |
| Cenas | PascalCase | FarmScene, TownScene, CaveScene |

## Estrutura de pastas

```
Assets/_Game/
├── Data/           ← ScriptableObjects (nunca .cs aqui)
├── Scripts/
│   ├── Core/       ← GameEventBus, TimeManager, SaveManager, Events/
│   ├── Player/
│   ├── Farm/
│   ├── Cave/
│   ├── Combat/
│   ├── Craft/
│   ├── Companion/
│   ├── NPC/
│   ├── UI/
│   ├── Save/
│   └── Utils/
├── Scenes/
├── Prefabs/
├── Sprites/
│   └── Placeholders/  ← retângulos coloridos para MVP
├── Animations/
├── Tilemaps/
└── Audio/             ← vazio até Fase 10 (polish)
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

## Placeholders do MVP (Fase 8)

Retângulos coloridos em Assets/_Game/Sprites/Placeholders/:
- Jogador: cubo azul 32x48
- Inimigo: cubo vermelho 32x32
- Planta estágio 0: cubo verde escuro 32x32
- Planta estágio final: cubo verde claro 32x32
- Tile chão fazenda: #C8A464 (areia laranja)
- Tile chão caverna: #555555
- Tile parede caverna: #333333
- Item dropado: cubo amarelo 16x16
- NPC: cubo branco 32x48
- Workshop: retângulo cinza 64x64


## Regras adicionais para Codex — Fase 8

1. Implementar em PRs pequenos, seguindo `FASE8_EXECUTION_PLAN_CODEX_v1.0.md`.
2. Nunca implementar mais de um PR por tarefa, salvo pedido explícito.
3. Todo PR deve declarar arquivos alterados e teste manual.
4. Nunca trocar arquitetura por conveniência.
5. Nunca usar `StreamingAssets` para save editável; usar `Application.persistentDataPath`.
6. Nunca serializar `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour` ou referência Unity em JSON de save.
7. Persistência sempre por IDs estáveis.
8. Não usar `FindObjectsByType` em runtime para montar sistemas; preferir `[SerializeField]`, installer de cena ou Editor script.
9. Se a tarefa exigir configuração manual pesada no Unity, criar Editor script reproduzível.
10. Se houver dúvida entre MVP e V2/FULL, escolher MVP e registrar pendência.
11. Ao terminar o PR/tarefa, atualizar `PROJECT_LOG.md` com resultado, testes e pendências.

## Documentos operacionais adicionais

- `PROJECT_LOG.md` — log operacional e continuidade obrigatória entre agentes.
- `docs/FASE8_EXECUTION_PLAN_CODEX_v1.0.md` — plano de execução por PR.
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` — contratos de eventos, IDs, registries e save.
- `docs/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` — pipeline de arte com IA + Aseprite.

## Regras de arte com IA

1. Não copiar assets, paleta proprietária, UI, personagens ou sprites de Stardew Valley/Harvest Moon.
2. Usar referências apenas como direção de gênero: legibilidade, câmera, conforto visual.
3. IA pode gerar conceito, variações e rascunhos.
4. Aseprite é a etapa final obrigatória para limpar pixels, paleta, outline e export.
5. Todo sprite aprovado deve ter `.aseprite/.ase` fonte quando houver edição manual relevante.
6. PNG final entra em `Assets/_Game/Sprites/...` com import `Point`, `Compression None`, `Generate Mip Maps false`.

## Fluxo de agentes — Git e entrega

### Agentes preparam, humanos entregam
- Agentes (Claude) criam commits locais em português.
- Agentes **NÃO** executam `git push`.
- Agentes **NÃO** abrem PR/MR.
- Agentes **NÃO** deletam branches locais/remotas.
- Push, PR/MR, merge e deleção de branches são responsabilidade **exclusiva** do humano.

### Ao final de cada PR/pacote, agente entrega
- Commits locais criados (listados por SHA e mensagem).
- Arquivos alterados por PR.
- Testes executados (✓) e testes pendentes (✗).
- Instruções reproduzíveis para validação local.
- Sugestões de comandos para o humano (mas sem executá-los).

Veja [AGENTS.md](AGENTS.md) para detalhes completos do fluxo de trabalho.
