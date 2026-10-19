# Continuação do playtest — menus e sistemas — 2026-09-27

Resultado: cobertura ampliada; save/load FAIL após crafting. Não é validação completa nem aceite humano.

## Método e evidência
- Unity 6000.5.7f1, exclusivamente janela `cindars_hope`, FarmScene em Play Mode, teclado/mouse reais.
- Aproximações por reposicionamento temporário; não certifica circulação normal até fonte, bancada, pesca, baú ou estufa.
- Save isolado em temporaryCachePath, pasta `town-interior-save-tests/a8e4b7ed44fe4b06b23a8a1f676f9181`.
- Log: `Logs/exploratory-playtest.log`; capturas observadas na sessão de ferramentas, sem vídeo contínuo.
- Reprodutor preservado: `TestResults/exploratory-playtest/crafting-unloadable-slot.json`.
- Encerramento confirmado: Play Mode desligado, FarmScene sem dirty, probe temporário removido; nenhuma pasta saves no persistentDataPath.

## Cobertura executada
| Fluxo | Resultado e limite |
|---|---|
| I — inventário | Abriu/fechou; equipou varinha, cana, enxada e regador. Relógio pausado. Split/mover/destruir/salvage não exercitados. |
| K — atributos | Abriu; tentativa sem pontos recusada com mensagem correta. Incremento com pontos não testado. |
| L — equipamento | Troca da mão direita por Fire Wand funcionou. IDs técnicos aparecem; stats derivados exibem texto provisório. |
| U — habilidades | Seletor e ramo Magic abriram; bloqueios sem pontos/pré-requisitos visíveis; Escape volta/fecha. Compra não testada. |
| J — quests | Lista vazia abriu/fechou, pausando relógio. Aceite/progresso/conclusão não testados. |
| Escape — pausa | FAIL: não abriu menu nem pausou no mundo, com Game View focada; outros atalhos funcionaram. |
| Fonte da Anya | Abriu/fechou; exibe `AwakenedReturnOnly`. Relógio continuou durante diálogo, diferente dos menus acima. |
| Bancada | Três crafts imediatos: Wood 15→9 e Madeira Processada 0→6. Cancelar sem trabalho recusado. |
| Varinha | Disparo e gasto de MP confirmados; log 14799/14819 registra projétil Fireball e sprite procedural de fallback. Acerto não validado. |
| Pergaminho | Contexto exibe Usar indisponível; equipar não comprovou consumo/desbloqueio. |
| Pesca | Cana equipada, E iniciou e gastou stamina 100→80. Nenhuma captura confirmada; timing fino não validado. |
| Baú da casa | NÃO IMPLEMENTADO: E registra placeholder, sem armazenamento (log 16544/16776). |
| Plantio | Plot 202: Raw→TilledDry→Growing; trigo 5→4; sem regador mostra requisito; equipado, rega conclui e prompt muda para Irrigado. Colheita não testada. |
| Save/load | FAIL: F5 gravou; F9 recusou o próprio arquivo e não restaurou posição/equipamento/horário. |

## Achados prioritários
- **P1 — save incompatível com o carregador:** após crafting, bancada ociosa serializa `Job` com `OutputItemId=""` e `OutputAmount=0` (JSON:1660). `SaveManager.Migration.cs:243` rejeita; log:16020: `Crafting job has an unknown output.` Reproduzir: nova sessão Farm → três crafts Workbench Timber → F5 → mudar posição/equipamento → F9. Investigar representação de job ausente e roundtrip, antes de atribuir erro ao catálogo de itens.
- **P1 — acesso aos menus de sistema:** pausa não apareceu. Busca estática encontra publicação de PauseOpenedEvent, sem subscriber em Assets. Title/settings não alcançados no boot exercitado; existência de controllers não comprova UI integrada.
- **P2 — feedback de crafting:** resultado chega ao inventário, mas painel continua Station idle e não sinaliza sucesso; repetir parece necessário mesmo quando craft funcionou.
- **P2 — estufa oculta interação:** no posicionamento controlado (22.3,13), exterior cobre personagem/canteiro durante arado, plantio e rega. Revisar sorting/ocultação de teto e testar entrada normal; não há prova de bloqueio de acesso.
- **P2 — consistência visual/linguagem:** fundos transparentes prejudicam leitura sobre cenário; estados técnicos, IDs e idiomas misturados em equipamento, habilidades, fonte e cultivo.
- **P2 — promessa de interação:** baú mostra Abrir baú, mas apenas escreve log de implementação futura. Alinhar disponibilidade/apresentação ao estado real.

## Continuação e limites
- Prioridade sugerida: roundtrip de save; revive/respawn e legibilidade da Cave já relatados; acesso à pausa; feedback/localização/layout; depois ampliar cobertura de progressão e combate.
- Título/settings, todas as operações de inventário, demais estações, quests completas, compra de skills, colheita, captura de peixe, bosses, áudio/gamepad e recuperação do corpo seguem sem validação completa.
- Não houve alteração de produto ou cena para corrigir os achados; validação automatizada anterior não foi repetida por esta auditoria exploratória.
