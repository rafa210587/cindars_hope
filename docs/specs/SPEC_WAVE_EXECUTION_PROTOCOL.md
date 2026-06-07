# Cindar's Hope — Wave Execution Protocol

> **Status:** documento canônico de governança de execução por waves.  
> **Local:** `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> **Tipo:** protocolo operacional; não é spec implementável.  
> **Função:** definir como specs devem ser executadas por agentes, em ordem, com paralelização segura, locks, reports e validação final.  
> **Regra:** este documento orienta execução; nenhum agente deve implementar código a partir dele isoladamente.

---

## 1. Fontes relacionadas

Ler junto quando estiver planejando execução de specs:

```text
docs/project/CURRENT_STATE.md
docs/specs/SPEC_SOURCE_OF_TRUTH.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_EXECUTION_ORDER.md
.claude/rules/testing-quality-gate.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
```

Regra de leitura:

```text
Para execução, seguir o active spec e CURRENT_STATE.md.
Para geração de specs, seguir roadmap, source map e template.
Para validação, seguir validation matrix, testing gate e reports.
```

---

## 2. Diferença entre gerar spec e executar spec

### Gerar spec

```text
Criar contrato implementável em docs/specs/a_implementar/.
Não alterar runtime.
Não alterar Assets/, Packages/ ou ProjectSettings/.
Não mover spec para implementados.
Não atualizar SPEC_EXECUTION_ORDER.md sem autorização explícita.
```

### Executar spec

```text
Implementar apenas o escopo da spec.
Respeitar arquivos permitidos/proibidos.
Rodar validações obrigatórias ou registrar NOT RUN com motivo.
Criar execution report.
Não pedir validação humana intermediária.
Não promover spec automaticamente sem evidência.
```

---

## 3. Estados de spec/wave

```text
NOT_STARTED
  Existe no roadmap, mas ainda não virou spec concreta.

READY_FOR_SPEC_GENERATION
  Pode ser transformada em spec implementável.

SPEC_CREATED
  Arquivo existe em docs/specs/a_implementar/.

READY_FOR_IMPLEMENTATION
  Dependências documentais existem e não há bloqueador em CURRENT_STATE.md.

IMPLEMENTING
  Agente está executando a spec.

BUILD_VALIDATED
  Docs/build básicos passaram; não implica comportamento validado.

UNITY_VALIDATED
  Unity compile/validators passaram quando aplicável.

PLAYMODE_VALIDATED
  Play Mode automatizado ou cenário humano final executado com evidência.

DEFERRED_TO_FINAL_HUMAN_VALIDATION
  Há cenário humano documentado, mas validação real foi adiada para o fim do lote/wave.

ACCEPTED
  Todas as validações obrigatórias foram satisfeitas ou exceção humana explícita foi registrada.

PARTIAL
  Parte do escopo foi concluída, mas há validação, código ou risco residual pendente.

BLOCKED
  Não pode prosseguir sem decisão humana, dependência ou correção externa.
```

---

## 4. Pré-condições para iniciar uma wave

Antes de executar uma wave:

```text
1. Confirmar branch dev ou branch autorizada.
2. Ler docs/project/CURRENT_STATE.md.
3. Confirmar que não há blocker explícito para a wave.
4. Confirmar que specs da wave existem em docs/specs/a_implementar/.
5. Confirmar dependências anteriores.
6. Confirmar locks de arquivos/sistemas.
7. Confirmar matriz de validação aplicável.
8. Confirmar se a wave pode rodar em paralelo com outra.
```

Se CURRENT_STATE.md conflitar com uma spec:

```text
STOP.
Reportar conflito.
Não implementar até reconciliar.
```

---

## 5. Política de lote

Tamanho recomendado por execução:

```text
1 spec por vez:
  runtime core;
  save/load;
  event bus;
  bootstrap;
  scene flow;
  UI global;
  contratos compartilhados.

2 specs por vez:
  governança;
  documentação;
  specs médias de mesmo domínio sem lock compartilhado.

3 specs por vez:
  apenas docs-only sem arquivos compartilhados e com baixo risco.

4+ specs por vez:
  não recomendado.
```

Motivo:

```text
O gargalo é consistência, não volume de texto.
Specs demais no mesmo lote aumentam risco de conflitos, duplicação e validação fraca.
```

---

## 6. Política de paralelização

Toda spec deve declarar:

```text
Parallelizable: YES / NO / CONDITIONAL
Parallel group
Can run with
Must not run with
Repo lock scope
```

### Pode paralelizar quando

```text
Não altera os mesmos arquivos.
Não altera o mesmo contrato central.
Não altera a mesma save section.
Não altera o mesmo event contract.
Não altera a mesma cena/prefab/asset compartilhado.
Não depende de resultado ainda não criado por outra spec.
```

### Não pode paralelizar quando

```text
Ambas alteram SaveManager/GameSaveData/save schema.
Ambas alteram GameEventBus ou contratos de evento.
Ambas alteram bootstrap/global managers.
Ambas alteram a mesma UI global/input/modal stack.
Ambas alteram a mesma scene/prefab/ScriptableObject.
Uma depende diretamente do output da outra.
```

### Conditional

Usar `CONDITIONAL` quando:

```text
pode rodar em paralelo apenas se locks forem respeitados;
precisa de integração por checkpoint;
usa contrato já publicado, mas não deve editá-lo;
trabalha em domínio diferente, mas toca registry ou README comum.
```

---

## 7. Locks de arquivos/sistemas

Locks devem ser tratados como exclusividade operacional.

Exemplos de lock forte:

```text
Assets/_Game/Scripts/Core/GameEventBus.cs
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Bootstrap/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset compartilhado
ProjectSettings/**
Packages/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
```

Regra:

```text
Se duas specs precisam editar o mesmo lock scope, elas não rodam em paralelo.
Se a spec precisar sair do lock declarado, STOP e reportar.
```

---

## 8. Atualização de registries

`SPEC_REGISTRY_TO_IMPLEMENT.md` deve ser atualizado quando:

```text
uma spec concreta nova é criada em docs/specs/a_implementar/;
um status futuro muda de forma documentada;
uma dependência ou observação precisa ser refletida para evitar execução errada.
```

Não atualizar registry para:

```text
documento guia que não é spec;
ideia ainda não aprovada;
item future não consolidado;
arquivo arquivado;
execução parcial sem evidência.
```

---

## 9. Política para SPEC_EXECUTION_ORDER.md

Não atualizar `SPEC_EXECUTION_ORDER.md` apenas porque uma spec foi imaginada ou criada.

Atualizar somente quando:

```text
as specs concretas existem;
as dependências estão reconciliadas;
CURRENT_STATE.md não bloqueia;
o lote de execução foi autorizado;
o usuário autorizou explicitamente atualizar a ordem de execução.
```

---

## 10. Validação humana final

Não pedir human test spec a spec.

Quando uma spec runtime exigir Play Mode humano:

```text
marcar Human validation timing: DEFERRED_TO_FINAL_VALIDATION;
documentar cenário final ou apontar para docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md;
não declarar ACCEPTED por Play Mode humano antes da execução real.
```

A validação humana final deve ocorrer:

```text
no fim do lote;
no fim da wave;
ou no fim completo, conforme decisão do usuário.
```

---

## 11. Reports mínimos

Toda execução deve reportar:

```text
Spec executada;
Branch/commit;
Arquivos alterados;
Arquivos proibidos não alterados;
Validações rodadas;
Validações NOT RUN com motivo;
Testing Quality Gate;
Status final;
Risco residual;
Pendências;
Specs que podem rodar em paralelo depois.
```

Para runtime/code, criar:

```text
docs/validation/<spec_id>_execution_report.md
```

---

## 12. Stop conditions

Parar e reportar se:

```text
Spec conflita com CURRENT_STATE.md.
Spec exige arquivo fora do escopo permitido.
Dependência anterior está bloqueada.
Arquivos proibidos seriam alterados.
Validação obrigatória falha sem caminho claro.
Runtime/code não tem Testing Quality Gate.
Agente precisaria pedir validação humana intermediária.
Spec tenta promover para implementados sem evidência.
Spec tenta atualizar SPEC_EXECUTION_ORDER.md sem autorização.
```

---

## 13. Handoff para Codex/Claude Code

Prompt mínimo para execução:

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

Leia:
- CLAUDE.md
- docs/project/CURRENT_STATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- spec alvo
- arquivos explicitamente listados na spec

Implemente apenas a spec alvo.
Respeite arquivos permitidos/proibidos.
Não altere SPEC_EXECUTION_ORDER.md sem autorização explícita.
Não peça validação humana intermediária.
Se Play Mode humano for necessário, marque DEFERRED_TO_FINAL_VALIDATION.
Registre execution report e risco residual.
```

---

## 14. Checklist antes de iniciar execução paralela

```text
[ ] Specs concretas existem.
[ ] Cada spec declara Parallelizable.
[ ] Cada spec declara Repo lock scope.
[ ] Locks não se sobrepõem.
[ ] Dependências estão prontas.
[ ] CURRENT_STATE.md não bloqueia.
[ ] Validation matrix aplicável.
[ ] Testing Quality Gate aplicável.
[ ] SPEC_EXECUTION_ORDER.md não será editado sem autorização.
```
