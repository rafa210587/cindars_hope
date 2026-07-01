---
name: event-catalog-and-tracing
description: Governança e observabilidade da superfície do GameEventBus — manter um catálogo vivo de eventos (publishers/subscribers), checar duplicação antes de criar evento novo, e um tracer dev-only que loga publish→N subscribers. Use ao criar/alterar um evento em Core/Events, ou ao investigar "evento não chega"/"quem assina isso".
---

# Skill: Catálogo e Tracing de Eventos

O `GameEventBus` é a única via de comunicação de gameplay (rule `unity-architecture`), e depois de ~70 specs a superfície de eventos em `Assets/_Game/Scripts/Core/Events/` é enorme. O `event-bus-pattern` ensina a **usar** o bus; esta skill cuida da **governança** (não duplicar contrato) e da **observabilidade** (ver o que está sendo publicado e por quem é consumido). Precedente concreto do risco: `HudSuppressionChangedEvent` quase virou dois eventos paralelos entre `fable_43` (dono temporário) e `fable_71` (dono canônico).

## Quando usar

- Antes de **criar um evento novo** em `Core/Events/` — checar se já existe equivalente.
- Ao **alterar a assinatura** de um evento existente (quem mais publica/assina pode quebrar).
- Quando um sistema "não reage" a algo — descobrir se o evento é publicado e quem o assina.
- Ao herdar/entregar um contrato de evento entre specs (temporary ownership → canonical owner).
- Em wave de polish/integração que adiciona muitos eventos.

## Por que existe

Sem catálogo, dois autores criam eventos com o mesmo significado (duplicação de contrato), ou alteram um evento sem saber quem depende dele. Sem tracer, "o evento não chega" vira debugging às cegas. O catálogo é a fonte de verdade da superfície; o tracer mostra o tráfego real.

## Sistemas existentes (reusar, não duplicar)

| Referência | Papel |
|---|---|
| `GameEventBus` (`CindarsHope.Core`) | `Publish<T>()` / `Subscribe<T>()` / `Unsubscribe` (via `IDisposable`); isola exceções de listeners |
| `Assets/_Game/Scripts/Core/Events/**` | Onde TODO evento de gameplay vive (DTOs imutáveis) |
| `(skill: event-bus-pattern)` | Como definir/publicar/assinar corretamente |
| `SfxEventBridge` | Exemplo de subscriber-hub que assina dezenas de eventos num só lugar |

## Procedimento

### 1. Checar duplicação ANTES de criar evento (obrigatório)

```powershell
# o evento (ou um sinônimo) já existe?
Select-String -Path Assets\_Game\Scripts\Core\Events\*.cs -Pattern "class .*Event"
Select-String -Path Assets\_Game\Scripts -Pattern "MeuConceitoEvent" -Recurse
```

Se já existe um evento com o mesmo **significado de domínio** (mesmo fato do mundo), reuse-o — não crie um segundo (rode `(skill: system-reuse-audit)`). Se a assinatura precisa mudar, faça-o no evento existente e ajuste todos os call sites.

### 2. Manter o catálogo vivo

Fonte de verdade: `docs/architecture/EVENT_CATALOG.md` (criar se não existir). Uma linha por evento:

```
| Evento | Payload | Publicado por | Assinado por | Spec dona |
|---|---|---|---|---|
| HudSuppressionChangedEvent | int Phase, IReadOnlyList<string> HiddenWidgets | EndgameSequenceController, HudSuppressionBroadcaster | HudSuppressionConsumer | fable_71 (canonical) |
```

Atualize a linha ao criar/alterar o evento. "Spec dona" resolve o caso de ownership temporário (quem é a fonte canônica do contrato).

### 3. Tracer dev-only (ver o tráfego)

Para investigar "o evento chega?", exponha um tracer opt-in que envolve o publish e loga `evento → N subscribers` (1 linha por publish, gateado por flag dev):

```csharp
// dev-only, #if UNITY_EDITOR || DEVELOPMENT_BUILD; liga via flag estatica.
// Loga: "[EventTrace] PlayerDamagedEvent -> 3 subscribers". Respeita (skill: observability-and-logging)
// (prefixo [EventTrace], sem spam: so quando a flag esta ligada).
```

Nunca deixe o tracer ligado em build shipped; ele é diagnóstico, não gameplay.

## Regras

- Evento novo só depois de checar o catálogo + grep (sem duplicar contrato).
- Todo evento em `Core/Events/` é DTO imutável de tipos simples (sem refs Unity) — mesma disciplina de save DTO (rule `unity-architecture`).
- Ao alterar assinatura: atualizar TODOS os publishers/subscribers no mesmo commit + a linha do catálogo.
- Ownership temporário (uma spec cria o contrato que outra vai assumir): marcar "dona" no catálogo e conferir não-duplicação quando a dona canônica landar (precedente F43→F71).
- Tracer é dev-only/read-only; não vira dependência de gameplay nem entra em shipped.
- Não criar um segundo bus, um `EventManager` paralelo ou um sistema de mensagens concorrente.

## Saída esperada (checklist)

```text
Evento novo checado contra catalogo + grep (sem duplicata): SIM/NAO APLICAVEL
EVENT_CATALOG.md atualizado (linha do evento): SIM/NAO APLICAVEL
Assinatura alterada -> todos publishers/subscribers ajustados: SIM/NAO APLICAVEL
Payload e DTO de tipos simples (sem refs Unity): SIM
Tracer (se criado) e dev-only e opt-in: SIM/NAO APLICAVEL
```

## Relacionados

- `(skill: event-bus-pattern)` — como definir/publicar/assinar (esta skill é a governança em cima)
- `(skill: system-reuse-audit)` — checagem pré-criação para não duplicar contrato
- `(skill: observability-and-logging)` — convenções do tracer (prefixo, sem spam)
- `(rule: unity-architecture)` — comunicação de gameplay só via GameEventBus
