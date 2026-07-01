---
name: monobehaviour-decomposition
description: Decompõe com segurança um MonoBehaviour grande/god que mistura input, rules, UI, audio, save e animation em um adapter fino + core em C# puro, de forma incremental e sem quebrar scenes/prefabs. Use quando um MonoBehaviour cresceu a ponto de misturar várias responsabilidades ou está difícil de testar.
---

# Skill: Decomposição de MonoBehaviour

O house style do projeto é um adapter `MonoBehaviour` fino sobre um core em C# puro (skill `ui-projection-pattern`, rule `gameplay-design-patterns`). Esta skill leva um god-`MonoBehaviour` existente até esse formato **sem** quebrar a serialization de scene/prefab nem mudar comportamento por acidente.

## Restrições rígidas (ler antes de extrair)

- **Não reordene nem renomeie campos `[SerializeField]` despreocupadamente** — isso quebra o wiring do Inspector em scenes/prefabs, e edições de YAML `.unity`/`.prefab` são gated (rule `unity-assets`, `permissions.ask`). Mantenha os serialized fields onde estão; mova *logic*, não declarações de campo, nas primeiras passadas.
- **Rode a skill `system-reuse-audit` primeiro.** O core extraído pode já existir (ex.: um service/projection). Extrair para um duplicado dispara o duplicate-class check do `runtime-code-guard` e cria um sistema paralelo.

## Procedimento

1. **Leia o arquivo inteiro primeiro.** Liste cada responsabilidade presente: input, domain rule, presentation/UI, audio, animation, persistence, networking, engine lifecycle.
2. **Desenhe a costura (seam).** Marque o que é *pure rule* (sem `UnityEngine`) vs *engine adapter* (input, components, prefabs, physics). A parte pura é o alvo da extração.
3. **Extraia C# puro primeiro, a menor peça coesa.** Mova uma rule para uma classe plana que o `MonoBehaviour` possui e delega. Sem mudança de scene, API pública estável, comportamento idêntico.
4. **Roteie chamadas cross-system pelo bus.** Se o god object chamava outros objetos de gameplay diretamente, substitua por publish/subscribe via `GameEventBus` (rule `event-bus-only-gameplay-communication`) — geralmente esse é o maior ganho de desacoplamento.
5. **Um passo, uma validação.** Após cada extração: compile (skill `unity-validation`) e rode o EditMode test que você acabou de adicionar para o core extraído. Não acumule cinco extrações antes de validar.
6. **Adicione testes conforme avança.** Cada classe pura extraída ganha cobertura EditMode (skill `editmode-test-authoring`) — essa cobertura é a prova de que o refactor preservou comportamento.
7. **Pare na costura.** Deixe o `MonoBehaviour` como um adapter fino: ler input → chamar core → aplicar resultados / publicar events / dirigir visuals. Não faça gold-plating nas partes que você não precisou tocar (rule `00-operational-discipline`).

## Saída esperada

- Inventário de responsabilidades (antes).
- Plano de extração em ordem segura (pure rule → bus wiring → emagrecer o adapter).
- Estrutura de nova classe/projection (formato alvo).
- Validação + EditMode test após cada passo.
- Residual risk: qualquer movimentação de serialized-field adiada, qualquer comportamento mudado intencionalmente (deve ser explícito), re-check de Play Mode se o wiring de scene foi tocado (skill `gameplay-test-scenario`).

## Não fazer

- Misturar este refactor com uma mudança de feature no mesmo commit (rule `00-operational-discipline`).
- Reescrever a classe inteira de uma vez — só passos incrementais que preservam comportamento.
- Introduzir `FindObjectOfType`/`GameObject.Find` para "simplificar" o wiring (rule `unity-architecture`).
