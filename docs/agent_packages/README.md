# Agent Packages - Cindar's Hope

Este diretório controla execuções pequenas e fechadas para Claude/Codex.

O objetivo é evitar prompts grandes demais, specs parcialmente executadas e registries contraditórios.

## Regra central

```text
1 pacote = 1 objetivo testável = 1 commit validado
```

O agente deve executar apenas o pacote solicitado e parar. Não deve avançar para o próximo pacote sem comando explícito.

## Arquivos principais

- `PACKAGE_EXECUTION_ORDER.md`: ordem, status e dependências dos pacotes.
- `PACKAGE_TEMPLATE.md`: modelo obrigatório para novos pacotes.
- `P00_registry_gate.md`: corrige/valida registries e status antes de código.
- `P12A_input_hands_attack.md`: SPEC 12 parte A - input Q/E, mãos e ataque.
- `P12B_mana_spells_arcane_bolt.md`: SPEC 12 parte B - mana, spell e ArcaneBolt.
- `P12C_bow_dodge_active_slots.md`: SPEC 12 parte C - bow, dodge e active slots.
- `P12D_spec12_closure.md`: fechamento documental/validação da SPEC 12.

## Runbook rápido

1. Atualizar branch:

```powershell
git checkout dev
git pull origin dev
git status
```

2. Ver pacote atual:

```powershell
Get-Content .\docs\agent_packages\PACKAGE_EXECUTION_ORDER.md
```

3. Rodar helper do pacote:

```powershell
.\tools\agent_orchestrator\RunAgentPackage.ps1 -PackageId P00
```

4. Copiar o conteúdo do pacote para Claude/Codex.

5. O agente executa somente esse pacote.

6. Validar resultado:

```powershell
.\tools\agent_orchestrator\ValidateAgentPackage.ps1 -PackageId P00
```

7. Só então atualizar o status do pacote no `PACKAGE_EXECUTION_ORDER.md` e passar ao próximo.

## Estados permitidos

- `Ready`: pode executar agora.
- `Blocked`: depende de outro pacote.
- `In Progress`: execução começou, ainda não fechada.
- `Done`: entregue, validado e documentado.
- `Partial`: houve entrega parcial com pendência objetiva.

## Regras invioláveis

- Não executar pacote bloqueado.
- Não alterar arquivos fora de `Arquivos permitidos` do pacote.
- Não atualizar registry como completo se runtime ficou parcial.
- Não remover spec de `a_implementar` se ela ainda bloqueia próximos pacotes.
- Não avançar para SPEC 13 enquanto P12D não estiver `Done`.
- Não avançar para SPEC 17 enquanto 12-16 não estiverem completas ou explicitamente aceitas como baseline funcional.
