# Human Test Scenario — Cidade Real: Casas Percorríveis e Portas (sem teleporte)

> **Spec:** `spec_city_real_walkin_houses_no_teleport`
> **Tipo:** Play Mode manual (DEFERRED_TO_FINAL_VALIDATION)
> **Pré-requisito:** rodar `CindarsHope/Inicializar Projeto` (regenera a TownScene + recompila) e
> depois `CindarsHope/NPCs/Rebuild Town NPC Dialogues`.

## Setup
1. Abrir o Unity; menu `CindarsHope/Inicializar Projeto`. Confirmar no Console:
   - `[walk-in-houses] 21 prédios percorríveis posicionados (auditoria concluída).`
   - **Nenhum** `LogError` `[walk-in-houses] sobreposição` / `sai do playfield` / `invade uma zona reservada`.
2. Abrir `Assets/_Game/Scenes/TownScene.unity` e entrar em Play Mode.

## Casos de teste

### CT1 — Sem teleporte / sem faixa off-field
- [ ] Apertar F8 (super zoom-out). NÃO existe grade de casas "lá em cima" (y>+40). A cidade inteira cabe num footprint 76×64 com borda de floresta fechada.
- [ ] Não aparece prompt "[E] Sair" ao entrar numa casa (não há porta de teleporte).

### CT2 — Porta funcional (E abre, entra a pé)
- [ ] Andar até a porta (vão central de baixo) de qualquer casa. Surge o prompt **"Abrir porta"**.
- [ ] Com a porta FECHADA, o corpo do jogador é BLOQUEADO no vão (não atravessa).
- [ ] Apertar **E**: a folha desliza para o lado, o prompt vira **"Fechar porta"**, e o jogador ENTRA a pé pelo vão. A posição do jogador NÃO salta (sem teleporte).
- [ ] Dentro: o telhado some (roof-reveal) e dá pra andar pelo cômodo (chão andável, paredes bloqueiam).
- [ ] Sair pelo vão e apertar E de novo fecha a porta (volta a bloquear).

### CT3 — Marcos cívicos ampliados
- [ ] **Igreja** (House_Temple): é o maior prédio, com uma divisória interna (dois cômodos: nave + lateral).
- [ ] **Cemitério** (NW): terreno de terra, cripta destacada, ~9 lápides, cerca com vão de portão.
- [ ] **Distrito de mercado** (norte): Salão de Mercado percorrível + ~6 bancas com toldos coloridos agrupadas.
- [ ] **Praça de eventos** (sul): área aberta com um tablado; é onde as barracas de festival aparecem.
- [ ] **Câmara** e **Mansão** são prédios percorríveis distintos.

### CT4 — Residências e moradores
- [ ] À noite, cada NPC indoor está dentro da sua casa (entrar e encontrar). Compartilhadas: Orlan+Gruta (Estalagem), Gurd+Hund (Casa Carvalho-Torto). Demais têm casa própria.
- [ ] Maelor (cemitério), Zrix (boca de gruta), Yael (tenda sul), Liora (jardim da praça) dormem ao relento, à vista.

### CT5 — Blocos justificados (F7)
- [ ] Apertar F7 (overlay de colisão). Os vermelhos (sólidos) são: borda do mapa, árvores, paredes de casa, portas FECHADAS, balcões de banca, pedestal da estátua, rochas da gruta. Não há mais caixotes/barris soltos sem motivo.

## Resultado esperado
Cidade coerente e maior; toda casa é física e se entra a pé pela porta (E), sem teleporte; marcos cívicos
se destacam; cada residência acomodada sem sobreposição.
