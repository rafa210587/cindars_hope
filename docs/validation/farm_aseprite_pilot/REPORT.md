# Piloto Aseprite — estrada e cercas

Status: SCOPED_PASS técnico; aceitação visual humana pendente. Não encerra a reconstrução da fazenda.

## Preparação concluída antes da edição

- Skill canônica `aseprite-authoring`, registrada no HARNESS_INDEX e sincronizada para Codex.
- Entrada curta, referências condicionais CLI/Lua e aceitação, exemplos PowerShell/Lua e template de ordem de trabalho.
- Gerador: zero problemas de frontmatter e zero falhas de cópia. Links locais e hashes fonte/cópia PASS.
- `quick_validate.py` não executou sua validação: dependência Python `yaml` ausente. Não foi tratado como PASS.
- Aseprite instalado em `E:/SteamLibrary/steamapps/common/Aseprite/Aseprite.exe`, versão 1.3.18.5-x64.
- CLI/Lua suficientes para o piloto; nenhum MCP instalado. Aplicativo aberto com os arquivos editáveis.

## Arte e integração

Fonte original `ground_path_dirt.png` preservada. Versão editável final:
[farm-road-v3.aseprite](../../../dev/art/aseprite/farm-road-pilot/farm-road-v3.aseprite), com original oculto,
terra em paleta controlada e pedras em camada própria. Grama:
[grass-edge-v1.aseprite](../../../dev/art/aseprite/farm-road-pilot/grass-edge-v1.aseprite), com transparência real
e cores amostradas da textura existente. Scripts Lua e ordem de trabalho estão na mesma pasta.

Aseprite gravou, reabriu e verificou a estrada (64x64, três camadas, um frame) antes de exportar. O Unity usa
um atlas alinhado ao mundo: células de oito pixels a 32 PPU correspondem à geometria existente de 0,25 unidade.
Os sprites de células são subassets persistidos de Tiles. O PNG fonte importado mantém o PPU padrão do importer;
a densidade efetiva é definida nos sprites derivados. Point, sem mipmap e compressão base desativada foram conferidos.

`FarmLandscapeVisualComposer` aplica bordas irregulares de grama nos trechos expostos dos caminhos. As cercas
passaram a derivar do contrato físico: postes nos pés, trilhos contínuos divididos em trechos com ordenação local,
preservando portões. Partes recortadas da cerca são assets persistidos; não houve edição manual de YAML.

## Iterações e limites

Primeira versão: automação e integração funcionaram, mas revisão independente observou estrada plana e problema
de ordenação das cercas. Segunda versão de terra rejeitada por faixas repetidas evidentes na captura. Terceira
removeu esse padrão e manteve desgaste discreto. A borda agora quebra a linha rígida, mas a referência ainda tem
agrupamentos de pedras e vegetação mais ricos. Cercas verticais continuam estreitas por sua orientação top-down.

Não foram alterados campo/cultivo, distribuição norte, ponte, lago ou tamanho da casa neste piloto. Essas diferenças
de composição continuam pendentes. Não há medição comparativa de custo/tokens contra geração no ChatGPT.

## Evidência

- Regeneração final: [regen_v3.log](regen_v3.log), Unity exit 0. Tentativa inicial `regen.log` terminou com exit 1
  antes da compilação, sem causa conclusiva; as tentativas posteriores executaram normalmente.
- [Captura final da região](editor_v3/farm_capture_region_cultivation.png).
- [Visão geral final](editor_v3/farm_capture_keyart_composition.png).
- [Play Mode final](gameplay_final/capture-metadata.json): PASS, 18 capturas, 19/19 rotas,
  480/480 pontos da fronteira, 27/27 verificações físicas, 7/7 seleções de interação e zero erros de runtime.
  Hash da cena antes/depois igual: `9045ec2f17b12654967322a84de05c0c426606d46b225a5475ec79fc3036f697`.
  [Pomar na câmera de jogo](gameplay_final/orchard.png) inspecionado por root. Capturas e consultas físicas
  não provam movimento humano, acionamento das interações ou todos os casos de sobreposição visual.
- Revisão independente de código confirmou a correção P2 dos pivôs/trechos de cerca. Reload e renderização
  executados no Play Mode final. Walkthrough de sobreposição player/cerca continua pendente.

Sem nova suíte EditMode: mudanças restritas à arte/composição Editor; geração, reload, física e interação reais
foram selecionados para verificar esta integração. A suíte anterior não é apresentada como executada novamente.
Sem commits ou promoção da spec; walkthrough humano e aceitação visual permanecem pendentes.
