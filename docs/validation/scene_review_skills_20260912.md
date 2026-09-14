# Skills de avaliação visual e física — 2026-09-12

## Entrega

Pedido: criar skills para impedir aprovação indevida de cenas como o piloto rejeitado da guarita.
Fonte canônica `.claude/`; cópias Codex em `.agents/skills/`, publicadas pelo gerador existente.

- `scene-composition-review`: limites arquitetônicos, perspectiva, proporção contextual, densidade de pixels, função do ambiente e oclusão. Reprova defeito visível mesmo com testes físicos verdes; referência ausente não impede reconhecer incoerência, mas impede atestar fidelidade.
- `scene-physics-review`: todos os sólidos efetivos, incluindo suportes filhos, folgas medidas, portas, movimento real e tráfego Player/NPC. Cast não equivale a travessia; sucesso posterior não resolve travamento anterior sem causa sustentada.
- Roteamento atualizado em `sprite-scene-integration`, `visual-asset-review`, `unity-validation` e `HARNESS_INDEX`. Sem criar nova matriz global ou prometer fiscalização automática.
- Um link transitivo legado de tilemap, quebrado na cópia Codex, foi corrigido na fonte. Nenhuma regra de gameplay alterada.

## Teste comportamental independente

Executor `review_skill_forward_test`, modelo Terra, contexto sem histórico. Recebeu apenas as duas skills,
a captura `town_gatekeeper_pilot_20260912_run3/interior_gatekeeper.png` e JSONs run2/run3, com pedido de decidir
se o interior estava pronto para replicação e informação de 6/6 EditMode. Sem resposta esperada fornecida.
Root validou estrutura/roteamento em paralelo; nenhum executor rodou Unity ou editou Assets.

Resultado observado: NÃO replicar. Separou composição FAIL, fidelidade à referência NOT RUN e evidência
física parcial; apontou ambiguidade de paredes/entrada, ausência de prova de circulação junto aos móveis,
travamento da guarita na run2 não resolvido pela run3 e resultado agregado 22/23, não PASS global.
Não aceitou os 6/6 informados como prova artística nem como XML verificado.

Sugestões do teste incorporadas: vínculo verificável captura/revisão; registro explícito de artefato
falho, causa, correção, reteste e resolução. Esse exercício avalia decisões sobre artefatos existentes,
não valida novamente a física do jogo nem garante comportamento perfeito em tarefas futuras.

## Verificações

- Gerador existente: zero problemas de frontmatter/cópia; paridade das cinco skills novas/roteadas.
- Idempotência: duas gerações com mesmos inputs produziram os mesmos hashes nos 193 outputs inspecionados.
- Links Markdown transitivos: primeira verificação encontrou um link legado quebrado em tilemap;
  corrigido. Verificação final: 72 documentos alcançados, zero links ausentes; paridade 5/5 e diff whitespace PASS.
- `quick_validate.py`: BLOCKED pelo ambiente. O alias Python do sistema não é um runtime e o Python
  bundled não possui PyYAML (`ModuleNotFoundError: yaml`). Nenhuma dependência foi instalada; usado
  o preflight de frontmatter/catálogo do gerador, paridade, links e teste comportamental como fallback.
- Unity: NOT APPLICABLE, mudança exclusiva de instruções. Sem cena, sprite, collider ou runtime editado.

Limite: as skills criam critérios e roteamento instrucionais; não são detector automático de qualidade
visual. Descoberta automática das novas entradas pode exigir recarregar a sessão; os arquivos já estão
disponíveis e os consumidores existentes apontam para eles.
