---
name: boot-integration-smoke
description: Verificação estática/headless do boot-wiring antes de depender de Play Mode — provar que GameBootstrap injeta managers, que os *RuntimeBootstrap se auto-conectam e que subscribers de eventos assinam, sem abrir o Editor. Use quando uma spec runtime fecha em "Play Mode deferred" e precisa de algum sinal de integração automatizável.
---

# Skill: Smoke de Integração de Boot

Quase toda spec runtime da run fechou em *"Play Mode deferred"* — o build compila e os testes EditMode de lógica pura passam, mas ninguém prova que o **wiring de boot** acontece (managers injetados, `*RuntimeBootstrap` conectados, subscribers assinando). Esta skill cobre o máximo de integração verificável **sem** Play Mode, reduzindo o que sobra para validação humana. Precedente de intenção: `fable_59` (combat telemetry).

## Quando usar

- Spec runtime que fecharia só com "Play Mode deferred" e quer um sinal de integração automatizável.
- Sistema que se auto-conecta via `RuntimeInitializeOnLoadMethod` (`*RuntimeBootstrap`) e precisa provar que conecta.
- Ao adicionar um manager ao `GameBootstrap` (provar a injeção sem abrir cena).
- Ao adicionar um subscriber de `GameEventBus` (provar que assina e reage).

## Por que existe

Compile PASS prova que builda, não que integra (rule `testing-quality-gate`). Boot-wiring é exatamente o que compile não pega: ref obrigatória null, subscribe ausente, ordem de bootstrap. Um smoke estático/EditMode dá um sinal barato e repetível antes do humano gastar tempo em Play Mode.

## Sistemas existentes (reusar, não duplicar)

| Referência | Papel |
|---|---|
| `GameBootstrap` | Injeta managers/databases via serialized refs — alvo da verificação de wiring |
| `*RuntimeBootstrap` (~14) | Self-wiring via `RuntimeInitializeOnLoadMethod`; expõem `Instance` para checar conexão |
| `GameEventBus` | Publish/Subscribe — verificar contrato (publicar evento → subscriber reage) |
| `(skill: editmode-test-authoring)` | Onde os testes de smoke vivem (`Tests/EditMode/**`) |
| `(skill: observability-and-logging)` | Wiring-error com contexto quando o smoke encontra ref ausente |

## Procedimento

### 1. O que dá para verificar SEM Play Mode (EditMode)

- **Contrato de evento:** publicar um evento e assertar que um subscriber-puro reagiu (precedente: testes de `SfxEventMap`/`MusicStateResolver`).
- **Lógica de wiring extraída:** se o `*RuntimeBootstrap` tem regra de "monta o quê a partir de quê", extraia para método puro e teste-o.
- **Catálogo/registry:** assertar que ids referenciados pelo bootstrap resolvem (sem null).
- **FSM/projection:** Enter/Tick/Exit e estados — já cobertos por `(skill: state-machine-design)`/`(skill: ui-projection-pattern)`.

### 2. O que precisa de Editor/batchmode (não EditMode puro)

- Injeção real de serialized refs na cena → validator de cena (`(skill: editor-validator-authoring)`): abre a cena via `AssetDatabase`, confere que os campos obrigatórios do `GameBootstrap` estão ligados, conta error/warning.
- Self-wiring `RuntimeInitializeOnLoadMethod` real → só em Play Mode; documente como residual e cubra a **lógica** em EditMode.

### 3. O que sobra para humano (Play Mode)

O que depende de lifecycle real, input, física, render. Liste no cenário humano (`(skill: gameplay-test-scenario)`) e no ledger de dívida diferida (`(skill: implementation-closeout)`).

## Regras

- Smoke é **sinal**, não prova de gameplay — nunca eleve "smoke PASS" a `PLAYMODE_VALIDATED` (rule `validation-truth`).
- Extraia lógica de wiring para C# puro testável em vez de testar o MonoBehaviour inteiro.
- Ref obrigatória ausente encontrada pelo smoke = wiring-error com contexto (`(skill: observability-and-logging)`), não falha silenciosa.
- Não usar `FindObjectOfType` no smoke nem no runtime (rule `unity-architecture`); use `Instance`/serialized refs.
- Validador de cena é editor-only; nunca vira dependência de runtime.

## Saída esperada (checklist)

```text
Contrato de evento testado em EditMode (publish -> subscriber reage): SIM/NAO APLICAVEL
Logica de wiring extraida e testada (pura): SIM/NAO APLICAVEL
Ids do bootstrap resolvem (sem null) — testado: SIM/NAO APLICAVEL
Validador de cena para serialized refs do GameBootstrap: SIM/NAO APLICAVEL
Residual real (Play Mode) listado no cenario humano + ledger: SIM
Smoke NAO declarado como PLAYMODE_VALIDATED: SIM
```

## Relacionados

- `(skill: runtime-bootstrap-pattern)` — o idiom de self-wiring que o smoke verifica
- `(skill: editmode-test-authoring)` — onde os testes de smoke vivem
- `(skill: editor-validator-authoring)` — validador de cena para serialized refs
- `(skill: observability-and-logging)` — wiring-error com contexto
- `(rule: testing-quality-gate)` — níveis de teste; smoke complementa, não substitui Play Mode
- `(rule: validation-truth)` — não inflar smoke para Play Mode
