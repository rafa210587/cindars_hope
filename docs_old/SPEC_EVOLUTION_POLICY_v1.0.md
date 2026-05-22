# Política de Evolução de Specs — Cindar's Hope v1.0

> **Status:** regra operacional aprovada.  
> **Escopo:** governança de specs, amendments, corrections e implementação paralela.  
> **Aplicação:** ChatGPT, Codex, Claude e qualquer agente humano/IA trabalhando no projeto.

---

## 1. Princípio

Specs aprovadas são baseline de implementação e não devem ser reescritas destrutivamente.

Implementação pode continuar em paralelo enquanto novas specs são refinadas em outros chats, desde que cada PR use a spec aprovada vigente no início do trabalho.

---

## 2. Regra principal

```text
Specs aprovadas são imutáveis como baseline.
Alterações futuras não reescrevem a spec antiga.
Correções, decisões novas ou mudanças de direção entram como nova spec, amendment, errata ou correction spec.
```

---

## 3. Fluxo permitido

### 3.1 Implementação em paralelo

É permitido implementar specs já aprovadas enquanto novas specs são refinadas em outros chats.

Exemplo:

```text
Chat A: implementação da FASE9F Cave procedural/resources.
Chat B: refinamento da próxima spec, como UI, bosses, magic, quests ou companions.
```

### 3.2 PR segue a spec vigente

Um PR iniciado deve seguir a spec aprovada no início do PR.

Mudanças descobertas durante o PR devem ser registradas para PR futuro, salvo bug crítico ou decisão humana explícita.

### 3.3 Specs antigas não são apagadas

Não apagar nem sobrescrever histórico de decisão.

Versões antigas podem ficar como referência/auditoria.

---

## 4. Como registrar mudanças em specs antigas

Quando uma spec aprovada precisar de correção, usar uma das opções:

```text
docs/amendments/FASE9F_AMENDMENT_v1.1.md
docs/amendments/FASE9F_CORRECTION_RESOURCE_NODE_TOOLS_v1.0.md
docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.1.md
```

Toda correção deve declarar:

- qual spec original altera;
- qual decisão antiga substitui ou complementa;
- motivo da correção;
- impacto em PRs futuros;
- se afeta implementação já feita ou apenas implementação futura.

---

## 5. Estrutura recomendada

```text
docs/
  FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md
  FASE9G_...
  amendments/
    FASE9F_AMENDMENT_v1.1.md
    FASE9E_UI_CORRECTIONS_v1.1.md
  logs/
    PROJECT_LOG_ARCHIVE_...
specs/
  FASE9F_CAVE_RESOURCES_ENCOUNTERS/
    spec.md
  FASE9G_.../
    spec.md
```

---

## 6. Regra para backlog futuro

Mudança descoberta durante discussão ou implementação deve virar backlog futuro quando não for bloqueio crítico.

Exemplo:

```text
Durante PR-174, surgiu uma nova flag ResourceNode.RequiresLight.
Não alterar PR-174 se ele já está em execução.
Registrar amendment/correction para PR futuro de cave lighting/resource hardening.
```

---

## 7. Regra para PRs em andamento

```text
PRs já iniciados seguem a spec vigente no início do PR.
Mudanças novas entram em spec/amendment futuro.
Só alterar o PR em andamento se for bug crítico, quebra de compilação ou decisão humana explícita.
```

---

## 8. Definições

| Termo | Uso |
|---|---|
| Spec baseline | Documento aprovado que guia implementação inicial |
| Amendment | Complemento ou ajuste sobre spec aprovada |
| Correction spec | Correção específica de uma decisão anterior |
| Errata | Lista de correções menores sem reescrever spec original |
| Future implementation note | Nota de implementação futura sem impacto imediato |

---

## 9. Aplicação imediata

A partir desta política:

- FASE9E e FASE9F continuam como specs baseline.
- Qualquer ajuste em FASE9E/FASE9F deve virar amendment/correction, não edição destrutiva.
- Novas specs podem ser escritas em paralelo neste chat.
- Implementação pode continuar em outro chat usando as specs aprovadas.
- `PROJECT_LOG.md` deve apontar a spec/amendment vigente antes de cada novo PR/pacote.

---

## 10. Resumo operacional curto

```text
Implementar specs aprovadas pode acontecer em paralelo ao refinamento de novas specs.
Specs antigas aprovadas não devem ser reescritas destrutivamente.
Mudanças futuras entram como nova spec, amendment, correction ou errata.
PR iniciado segue a spec vigente no início, salvo bug crítico ou decisão humana explícita.
```
