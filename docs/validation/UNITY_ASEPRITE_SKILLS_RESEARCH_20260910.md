# Pesquisa de skills e integrações Unity/Aseprite — 2026-09-10

## Escopo e método

Revisão dos quatro links enviados, buscas adicionais na web e leitura de fontes primárias.
Comparação com o harness local e CURRENT_STATE. Avaliação documental, sem instalar servidores,
executar scripts remotos ou alterar runtime/assets. Fontes `main` são móveis; fixar revisão e
licença antes de qualquer incorporação futura. Decisão: [ADR-0031](../decisions/ADR-0031-evidence-driven-unity-aseprite-harness.md).

## Recursos e decisão

| Fonte consultada | Valor para o projeto | Decisão |
|---|---|---|
| [Unity-Technologies/skills](https://github.com/Unity-Technologies/skills) | Catálogo oficial com uGUI, packages, CLI e UI de Editor | Referência seletiva; não instalar o catálogo |
| [ui-ugui](https://github.com/Unity-Technologies/skills/blob/main/skills/ui-ugui/SKILL.md) | Inspeção da hierarquia, layout e interação | Adaptar critérios ao HUD existente; não importar defaults ou edição de YAML |
| [unity-package-management](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-package-management/SKILL.md) | Operações UPM e verificação de versões | Referência para tarefa futura de packages; não adicionar dependências por gênero |
| [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) | Editor, testes, profiling e builds; dependência Python/uv documentada | Candidato de piloto ao lado de IvanMurzak; sem vencedor operacional ainda |
| [IvanMurzak/Unity-MCP](https://github.com/ivanmurzak/unity-mcp) | CLI, skills geradas e ferramentas extensíveis | Candidato; controlar geração de skills e distinguir conexão hospedada/local |
| [Servidor IvanMurzak](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/mcp-server.md) | Documenta stdio/HTTP e autenticação configurável | Verificar bind e autenticação; defaults não são garantia de isolamento |
| [Unity MCP oficial](https://unity.com/pt/blog/unity-ai-mcp-how-to-get-started) | Integração pelo AI Assistant, com pré-requisitos Unity AI/Cloud no artigo | Candidato se requisitos da versão forem atendidos |
| [Arquitetura oficial](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html) | Relay local, IPC e aprovação de cliente externo | Boa referência; verificar comportamento na versão instalada |
| [Besty0728/Unity-Skills](https://github.com/Besty0728/Unity-Skills) | Catálogo amplo, incluindo scene contracts e módulos de arquitetura | Referência, sem importar outra camada de servidor e regras concorrentes |
| [tea-x-random/unity-game-skills](https://github.com/tea-x-random/unity-game-skills) | Integra geração de assets, QA e profiling | Aproveitar seleção por evidência; rejeitar pressupostos iOS/serviços |
| [unity-debug-profiler](https://github.com/tea-x-random/unity-game-skills/blob/main/skills/unity-debug-profiler/SKILL.md) | Repro, baseline e recuperação após reload | Escrever orientação própria; não copiar soluções genéricas de NRE, limpar lockfile ou compressão móvel |
| [Profiler oficial](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-profiling-applications.html) | Medição no Player/ambiente alvo | Base técnica da nova skill de profiling |
| [uGUI Auto Layout](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UIAutoLayout.html) | Quem controla tamanho e posição | Referência de layout, conferindo versão do pacote local |

## Links originais de orientação geral

[MindStudio](https://www.mindstudio.ai/blog/gpt-6-astra-video-game-development): aproveitar o
ciclo referência/produção/integração/playtest. O relato não fornece um pacote reproduzível
que comprove autonomia ou qualidade no nosso jogo 2D.

[Medium](https://medium.com/@unicodeveloper/9-must-have-skills-for-codex-in-2026-b5124b375eec):
WarpGrep e Valyu ficam adiados até haver lacuna medida; create-plan sobrepõe nosso SDD;
frontend fica para web; stop-slop já tem cobertura editorial; CI/comments são condicionais à rotina
de PR. Codex Security é avaliação separada por superfície de risco.

[Superpowers](https://github.com/obra/superpowers) adicionaria um segundo ciclo completo de
execução. Manter nosso fluxo proporcional. As fontes originais de
[gh-fix-ci](https://github.com/openai/skills/blob/main/skills/.curated/gh-fix-ci/SKILL.md) e
[gh-address-comments](https://github.com/openai/skills/blob/main/skills/.curated/gh-address-comments/SKILL.md)
incluem interação/seleção; o resumo do blog exagera a automação. Não há `.github/workflows`
neste checkout, portanto não criar uma skill de CI sem uso demonstrado.

## Lacunas locais confirmadas

- `performance-auditor` continha afirmações temporais sem evidência sobre ausência de pooling
  e contagens de métodos; remover, exigindo inspeção atual e separação hipótese/medição.
- `hud-canvas-binding` carregava histórico e checklist com respostas SIM predefinidas;
  substituir por critérios observáveis, ownership de layout e interação em runtime.
- `asset-wiring-specialist` ainda centrava instruções em GameBootstrap e ação humana no
  Inspector; alinhar à composição atual e execução autorizada pelo Editor API.
- `unity-asset-generation` usava caminho com versão Unity antiga; derivar da instalação
  correspondente ao ProjectVersion e respeitar a instância aberta.
- Falta contrato comum para MCP: identidade da sessão, estado dirty, reload, resultado incerto
  e limites das ferramentas de inspeção versus mutação.

## Aseprite e animação

| Fonte primária | Avaliação e aplicação |
|---|---|
| [CLI oficial](https://www.aseprite.org/docs/cli/) | Preservar ordem de flags e export; CLI permanece base |
| [Image API](https://www.aseprite.org/api/image) | Documenta limitações de undo em escrita direta; corrigida orientação de cel existente |
| [Frame API](https://www.aseprite.org/api/frame) | Duração Lua em segundos; converter unidades e índices na fronteira do adaptador |
| [willibrandon/pixel-plugin](https://github.com/willibrandon/pixel-plugin) | Já adaptado localmente com licença MIT e revisão fixada; não duplicar instalação |
| [willibrandon/pixel-mcp](https://github.com/willibrandon/pixel-mcp) | Candidato; persistência de seleção/clipboard em user data e layer auxiliar exige auditoria |
| [diivi/aseprite-mcp](https://github.com/diivi/aseprite-mcp) | Diagnósticos de frames/onion skin; comparação de pixels não aprova movimento |
| [giangdvdotdev/aseprite-mcp](https://github.com/giangdvdotdev/aseprite-mcp) | Bridge live sobre documento ativo; adiar até necessidade que justifique sessão/socket e escrita de exports |

Todos os adaptadores Aseprite acima: integração local NOT RUN nesta entrega. Pesquisa independente
confirmou que a skill local de animação já preserva provenance da adaptação, cels ligados e
unidades de timing. Foram acrescentados ownership de documento, metadata auxiliar e contrato
de adaptadores, em vez de uma nova camada de autoria.

## Limites da entrega

Nenhum MCP recebeu PASS de integração. Não inferir performance, custo ou economia de tokens
pela quantidade de tools, tamanho de arquivos ou recomendações dos autores. Nenhuma nova
dependência de Unity Localization, Addressables, URP, DOTS, serviços ou geração paga foi adotada.
