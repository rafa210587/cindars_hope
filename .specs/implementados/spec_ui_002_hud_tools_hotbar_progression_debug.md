# SPEC UI-002 — HUD tools hotbar progression debug

> Status: Implementado parcial
> Camada: UI
> Fonte histórica: `docs_old/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md`
> Refinamento relacionado: `docs/refinements/implementados/ref_pr131_validacao_hud_tools_progression.md`
> Evidência principal: `Assets/_Game/Scripts/UI/DebugHud.cs`

---

## 1. /speckit.specify

### O que existe
HUD debug mostra painéis, feedback temporário, hotbar, ferramenta, progressão e status da cave conforme MVP/debug.

### Por que existe
Dá visibilidade operacional para validar sistemas antes da UI final.

### Fora de escopo
Não é HUD final, não substitui UI/UX full gameplay.

---

## 2. /speckit.plan

### Arquitetura real
DebugHud, HotbarState, PlayerActionFeedbackEvent e PlayerProgressionManager.

### Fluxo
Input debug e eventos atualizam estado exibido; feedback temporário aparece por evento.

### Persistência
HUD não persiste; lê managers e DTOs persistidos por outros sistemas.

---

## 3. /speckit.tasks

### Implementado
- [x] Evidência principal registrada.
- [x] Fonte histórica preservada em docs_old/.
- [x] Refinamento ativo linkado em docs/refinements/implementados/ quando aplicável.

### Implementado parcial
- [ ] Validação Unity Play Mode pode estar pendente conforme status.

### Pendente/futuro
- [ ] Substituir por HUD real em FASE9L.

---

## 4. Evidência no repo

| Tipo | Caminho | Observação |
|---|---|---|
| Código | `Assets/_Game/Scripts/UI/DebugHud.cs` | Evidência principal. |
| Histórico | `docs_old/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md` | Fonte histórica preservada. |
| Refinamento | `docs/refinements/implementados/ref_pr131_validacao_hud_tools_progression.md` | Refinamento ativo relacionado. |

---

## 5. Pendências e riscos

- Substituir por HUD real em FASE9L.




