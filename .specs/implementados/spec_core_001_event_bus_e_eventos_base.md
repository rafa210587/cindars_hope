# SPEC CORE-001 - Event bus e eventos base

> Status: Implementado
> Camada: Core
> Fonte historica: `docs_old/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md; docs_old/ARCH_fase4_v2.2.md`
> Evidencia principal: `Assets/_Game/Scripts/Core/GameEventBus.cs`

---

## 1. /speckit.specify

### O que existe
GameEventBus centraliza publicacao e assinatura de eventos simples para comunicacao desacoplada.

### Por que existe
Esta capacidade sustenta o loop jogavel atual de Cindar's Hope e normaliza, em uma spec ativa, o que ja esta implementado ou parcialmente implementado no repositorio.

### Fora de escopo
Nao tratar como implementado final qualquer item listado como pendente, qualquer UX/arte/balanceamento final nao validado ou qualquer sistema futuro ainda sem spec propria.

---

## 2. /speckit.plan

### Arquitetura real
A arquitetura real e composta pelos arquivos listados na evidencia, pelos dados preservados em `docs_old/` e pelo status operacional registrado em `PROJECT_LOG.md`.

### Fluxo
O fluxo operacional segue o MVP atual: sistemas runtime consultam managers/dados por IDs, publicam eventos simples quando aplicavel e expõem estado para HUD, save ou validadores conforme o sistema.

### Persistencia
Quando ha persistencia, ela deve usar DTOs simples e IDs estaveis. Referencias Unity permanecem fora dos DTOs. Quando nao ha persistencia propria, o estado e derivado de managers ou dados ScriptableObject.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidencia principal existe no repo.
- [x] Estado foi registrado ou reconciliado em `PROJECT_LOG.md` e/ou `docs_old/IMPLEMENTATION_STATUS.md`.
- [x] Conteudo historico antigo foi preservado em `docs_old/`.

### Implementado parcial
- [ ] Validacao Unity Play Mode completa pode estar pendente conforme a area.
- [ ] UX final, arte final, balanceamento final ou cobertura completa so contam quando houver spec e validacao propria.

### Pendente/futuro
- [ ] Auditar payloads de eventos futuros e manter unsubscribe no ciclo de vida.

---

## 4. Evidencia no repo

| Tipo | Caminho | Observacao |
|---|---|---|
| Codigo | `Assets/_Game/Scripts/Core/GameEventBus.cs` | Evidencia principal da capacidade. |
| Codigo | `Assets/_Game/Scripts/Core/Events/**` | Evidencia principal da capacidade. |
| Historico | `PROJECT_LOG.md` | Log operacional e decisoes recentes. |
| Status antigo | `docs_old/IMPLEMENTATION_STATUS.md` | Tracking anterior preservado. |

---

## 5. Refinamentos relacionados

- `docs/refinements/implementados/ref_implementados_map.md`
- `docs/refinements/a_implementar/ref_futuro_map.md`
- `docs_old/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md; docs_old/ARCH_fase4_v2.2.md`

---

## 6. Pendencias e riscos

- Auditar payloads de eventos futuros e manter unsubscribe no ciclo de vida.
- Se a implementacao for parcial, nao promover para final sem evidencia de Unity e sem atualizar esta spec ou criar amendment/correction.



