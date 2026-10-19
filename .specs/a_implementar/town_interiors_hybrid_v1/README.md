# Town — interiores habitáveis e transição híbrida

**Revisão comum:** 1.0 — 2026-09-12. **Status:** S01 UNITY_VALIDATED (dados); S02 PARTIAL (contratos/FSM6/6, integração pendente). [S01](../../../docs/validation/town_interiors_hybrid_v1/S01.md) · [S02](../../../docs/validation/town_interiors_hybrid_v1/S02.md). Sem aceite visual/físico do pacote.

Pacote aprovado pelo humano em 2026-09-12 para execução delegada e validação proporcional. S02 depende da revisão independente de arquitetura/save. Não é uma nova macro-wave nem muda a prioridade das waves existentes.

| Ordem | Spec | Entrega |
|---|---|---|
| 1 | [S01 — catálogo e programa das 24 construções](spec_town_interior_catalog_v1.md) | Owners/residentes, finalidade, cômodos, móveis, dimensões, comércio e amendment. |
| 2 | [S02 — transição e persistência](spec_town_interior_transition_save_v1.md) | Porta/fade/cena aditiva/retorno, rollback, save e carregamento seguro. |
| 3 | [S03 — moradores e comércio](spec_town_interior_npc_commerce_v1.md) | Agenda entre espaços, unicidade, atendimento, estoque e funcionamento das casas. |
| 4 | [S04 — produção e aceitação](spec_town_interior_materialization_v1.md) | Três pilotos, 24 interiores, arte, física, navegação e evidência final. |

[Refinamento e auditoria](../../../docs/refinements/a_implementar/ref_town_interiors_hybrid_v1.md). Fontes canônicas e contratos estão em cada spec; os números são baseline de código, não testes Unity executados nesta tarefa.

## Resposta de design

Comércio **não fica só na rua**: treze fornecedores têm posto principal interno, dez externo. O local principal não duplica o NPC, ShopId nem estoque. Serviços canônicos mantêm proprietário, custo e efeito. A pousada combina recepção, taverna, cozinha, banho e quartos; lojas-oficinas possuem espaço doméstico separado. Chamber, Prison e MarketHall são institucionais, não moradias aleatórias.

22 fachadas com portal; duas construções híbridas (guarita e curral) mantêm função exterior e recebem aposento residencial separado. São **24 definições de interior**, carregadas numa cena aditiva compartilhada, não 24 runtimes separados.

## Travas antes de executar

- [x] Aprovação humana das fichas de S01, transição e acesso: pedido de execução de 2026-09-12.
- [ ] Revisão independente de arquitetura/save e de design — retomada na execução; achados devem ser resolvidos antes da migração.
- [ ] Reconfirmar Phase 0, dependências e nomes propostos contra código atual.
- [x] Amendment aprovado ao baseline 23 walk-in, registrado em city_rules; critérios físicos anteriores preservados.
- [x] Pacote e referências promovidos para `a_implementar` após autorização; registry local atualizado, ordem global não alterada. Contratos críticos conservam gate de revisão.

Nenhuma revisão de arquivo equivale a validação Unity. Nenhum percentual de fidelidade foi inventado. Após implementação, usar `finish-spec` por slice; o pacote não fecha enquanto houver bloqueador de save, passagem ou identidade de moradores.

[Revisão documental, checks executados e pendências](../../../docs/validation/town_interiors_hybrid_v1/authoring_review.md).
