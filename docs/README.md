# Cindar's Hope - Documentacao ativa

> Status: documentacao ativa reconciliada.
> Historico integral preservado em `docs_old/`.

## Camadas

- `docs/design/` - design ativo do jogo.
- `docs/architecture/` - arquitetura ativa.
- `docs/operations/` - operacao, agentes, SpecKit, ambiente e pipeline.
- `docs/roadmap/` - roadmap ativo.
- `docs/amendments/` - amendments ativos.
- `docs/validation/` - smoke tests e validacoes ativas.
- `docs/backlog/` - backlog ativo e ideias futuras preservadas.
- `docs/specs/` - fonte unica oficial de specs.
- `docs/specs/implementados/` - specs consolidadas do que ja existe no repo.
- `docs/specs/a_implementar/` - specs futuras no padrao SpecKit.
- `docs/specs/SPEC_EXECUTION_ORDER.md` - ordem oficial de execucao.
- `docs/refinements/implementados/` - refinamentos, audits, handoffs e waves implementadas/parciais.
- `docs/refinements/a_implementar/pre_refinamentos/` - pre-refinamentos vivos.
- `docs_old/` - historico preservado; nao editar como fonte ativa.

## Arquivos ativos na raiz de docs/

A raiz de `docs/` deve conter apenas:

- `README.md` — este arquivo
- `IMPLEMENTATION_STATUS.md` — status de implementacao ativa
- `DOCS_OLD_TO_ACTIVE_CROSSWALK.md` — mapa de migracao de docs_old

Auditorias antigas, handoffs concluidos, planos pontuais e templates historicos ficam em `docs/archive/`.

## Regra

A pasta raiz `specs/` foi removida e nao deve ser recriada. A pasta raiz `spec/` tambem nao deve ser recriada.

Specs antigas nao sao apagadas de `docs_old/`. Mudancas futuras entram como nova spec, amendment, correction ou errata.