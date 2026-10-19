// Generates a self-contained, evidence-labelled browser overview from Unity's canonical skill assets.
// Run from the repository root: node docs/validation/skills_sdd_v1/build_skill_showcase.js
const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '../../..');
const dataDir = path.join(root, 'Assets/_Game/Data/Skills');
const output = path.join(__dirname, 'skill_showcase.html');

function field(source, name) {
  const match = source.match(new RegExp('^  ' + name + ': (.*)$', 'm'));
  if (!match) return '';
  let value = match[1].trim();
  if (value.startsWith('"') && !value.endsWith('"')) {
    const rest = source.slice(match.index + match[0].length).match(/^(?:\r?\n    .*)+/)?.[0] || '';
    value += rest.replace(/\r?\n    /g, ' ');
  }
  if (!value.startsWith('"')) return value;
  try {
    return JSON.parse(value.replace(/\\x([0-9A-Fa-f]{2})/g, (_, hex) => {
      const n = parseInt(hex, 16);
      return n < 128 ? String.fromCharCode(n) : '\\u00' + hex.toLowerCase();
    }));
  } catch { return value.slice(1, -1); }
}
function numeric(value) { return Number(value || 0); }
function assets(kind, idField) {
  const folder = path.join(dataDir, kind);
  const byGuid = new Map();
  for (const name of fs.readdirSync(folder).filter(n => n.endsWith('.asset'))) {
    const file = path.join(folder, name);
    const meta = fs.readFileSync(file + '.meta', 'utf8');
    const guid = meta.match(/^guid: (\w+)/m)?.[1];
    if (!guid) throw Error('Missing GUID: ' + name);
    const source = fs.readFileSync(file, 'utf8');
    byGuid.set(guid, { id: field(source, idField), name: field(source, 'DisplayName'), source, file });
  }
  const db = fs.readFileSync(path.join(dataDir, kind === 'Actions' ? 'SkillActionDatabase.asset' : 'SkillNodeDatabase.asset'), 'utf8');
  return [...db.matchAll(/\{fileID: 11400000, guid: (\w+), type: 2\}/g)].map(m => {
    const asset = byGuid.get(m[1]);
    if (!asset) throw Error('Database references missing asset: ' + m[1]);
    return asset;
  });
}

const category = id => id.split('_')[1];
const labels = { melee: 'Corpo a corpo', ranged: 'Distância', magic: 'Magia', survival: 'Sobrevivência', crafting: 'Ofício' };
const colors = { melee: '#e9a36d', ranged: '#a5d293', magic: '#bda1ed', survival: '#e4d68a', crafting: '#9dcee0' };
// Curated from the approved family specs and their PlayMode reports; the SOs hold numeric tuning.
const effect = {
  skill_melee_offhand_cut: 'Golpe frontal curto com adaga íntegra equipada na mão secundária.',
  skill_melee_battle_dash: 'Avança e resolve um golpe no arco frontal ao término do deslocamento.',
  skill_melee_leap_attack: 'Salta até um destino válido e acerta alvos na chegada.',
  skill_melee_whirl_cut: 'Giro de 360° em até seis alvos; dano cai para 70% após os três primeiros.',
  skill_melee_avanco_aco: 'Avança até o alcance do golpe, respeitando obstáculos sólidos.',
  skill_melee_grito_desafio: 'Provoca inimigos em área; elites e chefes têm resistência e janelas próprias.',
  skill_melee_investida_quebra_guarda: 'Investida frontal que amplifica em 3× o dano de postura.',
  skill_ranged_charged_shot: 'Segurar a carga aumenta dano, alcance e pressão de postura, com custo crescente.',
  skill_ranged_line_piercer: 'Projétil atravessa até cinco alvos, com dano decrescente; parede interrompe.',
  skill_ranged_multishot_fan: 'Três flechas em leque; um alvo recebe no máximo dois impactos.',
  skill_ranged_bleeding_arrow: 'Flecha que aplica sangramento e renova o status sem criar pilhas.',
  skill_ranged_marked_prey: 'Marca o inimigo válido mais próximo e amplia o dano ranged do próprio jogador.',
  skill_magic_fire_spark: 'Projétil de fogo de alvo único.',
  skill_magic_ice_bind: 'Projétil gelado que aplica Chill; repetição pode impor Slow mais forte.',
  skill_magic_toxic_cloud: 'Nuvem fixa com quatro pulsos globais e veneno, sem dano retroativo ao reentrar.',
  skill_magic_lightning_chain: 'Raio salta entre alvos elegíveis em ordem estável, respeitando obstáculos.',
  skill_magic_elemental_ward: 'Guarda absorve parte de dois impactos elementais diretos.',
  skill_magic_slowing_sigils: 'Zona fixa de lentidão, sujeita a limites especiais para elite e chefe.',
  skill_magic_chama_breve: 'Cone de fogo de 70°, até quatro alvos, com Burn renovável.',
  skill_magic_rajada_gelida: 'Três projéteis gelados em leque, até dois impactos por alvo.',
  skill_survival_last_breath: 'Após cruzar HP abaixo de 25%, cura uma vez por encontro dentro da janela.',
  skill_survival_sinal_retirada: 'Ao se afastar da ameaça, reduz custo de corrida/esquiva e aumenta velocidade.',
  skill_survival_isca_improvisada: 'Consome isca ao colocar; atrai ou desvia até cinco inimigos. Chefes ignoram.',
  skill_survival_kit_emergencia: 'Canaliza fora de combate, consome curativo e recupera HP; movimento/dano cancela.',
  skill_survival_instinto_sobrevivencia: 'Revela temporariamente recursos, perigos e interações já materializados.',
  skill_survival_campo_seguro: 'Cria zona fixa que reduz fome/fadiga e acelera regeneração natural.',
  skill_crafting_field_patch: 'Consome kit básico e recupera durabilidade de equipamento elegível até 60%.',
  skill_crafting_quick_repair: 'Canaliza fora de combate, consome kit padrão e repara até 85%.',
  skill_crafting_irrigador_portatil: 'Com regador e carga, irriga em linha três a cinco solos secos.',
  skill_crafting_bomba_improvisada: 'Consome uma bomba do inventário, lança em arco e atinge até quatro alvos.',
  skill_crafting_marca_eficiencia: 'Em uma estação, reduz custo de stamina de trabalho agrícola e ofício por janela.'
};
const actionFields = ['SkillActionType', 'BaseDamage', 'DamagePerRank', 'StaminaCost', 'ManaCost', 'CooldownSeconds', 'Range', 'ProjectileCount', 'LinePierceCount', 'EffectRadius', 'PulseCount', 'ArcDegrees', 'DashDistance', 'LeapDistance', 'StatusEffectId', 'NotYetExecutable', 'ChargeTimeSeconds', 'ChargeMaximumDamage', 'ChargeMaximumRange', 'ChargeMaximumStaminaCost', 'EffectDurationSeconds', 'TimingProfileId'];
const actions = assets('Actions', 'SkillActionId').map(a => ({
  id: a.id, name: a.name, category: category(a.id),
  effect: effect[a.id] || '',
  ...Object.fromEntries(actionFields.map(k => [k, field(a.source, k)])),
  source: path.relative(__dirname, a.file).replace(/\\/g, '/')
}));
const nodes = assets('Nodes', 'SkillNodeId').map(n => ({
  id: n.id, name: n.name, category: field(n.source, 'TreeId'),
  description: field(n.source, 'Description'),
  actionId: field(n.source, 'UnlockedSkillActionId'),
  tier: numeric(field(n.source, 'Tier')),
  maxRank: numeric(field(n.source, 'AuthoredMaxRank')),
  pointCost: numeric(field(n.source, 'SkillPointCost')),
  capstone: field(n.source, 'IsCapstone') === '1',
  dormant: field(n.source, 'NotYetExecutable') === '1',
  effectPending: field(n.source, 'EffectPending') === '1',
  iconMissing: field(n.source, 'Icon').includes('fileID: 0'),
  source: path.relative(__dirname, n.file).replace(/\\/g, '/')
}));
if (actions.length !== 31 || nodes.length !== 66) throw Error(`Catalog count mismatch: ${actions.length} actions, ${nodes.length} nodes`);
if (actions.some(a => !a.effect)) throw Error('An active action lacks a reviewed effect summary');
if (new Set(actions.map(a => a.id)).size !== 31 || new Set(nodes.map(n => n.id)).size !== 66) throw Error('Duplicate catalog ID');
const ids = new Set(actions.map(a => a.id));
for (const n of nodes) if (n.actionId && !ids.has(n.actionId)) throw Error('Missing action for ' + n.id);

const payload = JSON.stringify({ actions, nodes, labels, colors }).replace(/</g, '\\u003c');
const html = `<!doctype html>
<html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Habilidades de Vaalara — estado e demonstração</title>
<style>
:root{color-scheme:dark;--bg:#101721;--panel:#1b2631;--rim:#4e5763;--ink:#f2e9d6;--muted:#b8bfbd;--gold:#efcf80}*{box-sizing:border-box}body{margin:0;background:radial-gradient(circle at top,#243749,#101721 55%);color:var(--ink);font:15px/1.45 system-ui,Segoe UI,sans-serif}a{color:#a9d7ed}button,input{font:inherit}button{cursor:pointer}header,main,footer{max-width:1180px;margin:auto;padding:22px}h1,h2,h3,p{margin-top:0}h1{font:700 clamp(28px,4vw,48px)/1.12 Georgia,serif;letter-spacing:.02em}h2{font:700 25px Georgia,serif}header{padding-top:46px}header p{max-width:900px;color:var(--muted)}.eyebrow{letter-spacing:.22em;text-transform:uppercase;color:var(--gold);font-size:12px;font-weight:800}.status{display:grid;grid-template-columns:repeat(4,1fr);gap:10px;margin:22px 0}.status div,.panel,.card{background:var(--panel);border:2px solid var(--rim);box-shadow:5px 5px 0 #0a1017}.status div{padding:15px}.status b{display:block;font-size:25px;color:var(--gold)}.status span{font-size:12px;color:var(--muted)}.warning{border-left:4px solid #e7a46e;background:#322a25;padding:15px 18px;margin:18px 0}.panel{padding:20px;margin:25px 0}.layout{display:grid;grid-template-columns:1.4fr 1fr;gap:20px}.stage{position:relative;background:#111d23;border:3px solid #707d73;overflow:hidden;aspect-ratio:16/9}.stage canvas{display:block;width:100%;height:100%;image-rendering:pixelated}.stage-label{position:absolute;left:10px;top:10px;background:#0e1924d9;padding:5px 9px;font-size:11px;letter-spacing:.1em;text-transform:uppercase}.details h3{font:700 28px Georgia,serif;margin-bottom:4px}.tag{display:inline-block;border:1px solid currentColor;padding:2px 7px;margin:3px 4px 3px 0;font-size:12px}.metrics{display:grid;grid-template-columns:repeat(2,1fr);gap:8px;margin:16px 0}.metrics div{border-bottom:1px solid #4e5763;padding:6px 0}.metrics b{display:block;font-size:19px;color:#f1e1b4}.metrics small{color:var(--muted)}.controls{display:flex;flex-wrap:wrap;gap:8px;align-items:center;margin-top:16px}.controls button,.filters button{background:#263e4b;color:var(--ink);border:2px solid #829389;padding:8px 12px}.controls button:hover,.filters button:hover,.filters button.active{background:#496457}.controls button:focus-visible,.filters button:focus-visible,input:focus-visible,.card:focus-visible{outline:3px solid #f6d584;outline-offset:2px}.filters{display:flex;flex-wrap:wrap;gap:7px;margin:10px 0 18px}.search{width:100%;max-width:390px;background:#121e27;color:var(--ink);border:2px solid #809087;padding:10px}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(215px,1fr));gap:12px}.card{text-align:left;color:var(--ink);padding:13px;min-height:118px}.card strong{display:block;font-size:16px}.card small{color:var(--muted)}.card .glyph{width:26px;height:26px;display:inline-grid;place-items:center;vertical-align:middle;margin-right:8px;border:2px solid currentColor}.card.selected{border-color:var(--gold)}.tree{columns:2;column-gap:25px}.tree article{break-inside:avoid;border-bottom:1px solid #4c5962;padding:8px 0}.tree strong{display:block}.tree small{color:var(--muted)}.captures{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}.captures img{display:block;width:100%;border:2px solid #718474;image-rendering:pixelated}.captures figure{margin:0}.captures figcaption{margin:5px 0;color:var(--muted);font-size:12px}.note{color:var(--muted);font-size:13px}.evidence li{margin:6px 0}@media(max-width:780px){.layout{grid-template-columns:1fr}.status{grid-template-columns:repeat(2,1fr)}.captures{grid-template-columns:1fr}.tree{columns:1}}@media(prefers-reduced-motion:reduce){*{scroll-behavior:auto!important}}
</style></head><body><header><div class="eyebrow">Cindar's Hope · Vaalara · revisão de skills</div><h1>O que as habilidades já fazem no jogo</h1><p>Catálogo canônico lido dos assets Unity. Selecione uma ação e use <b>Executar</b> para ver uma representação de alcance, forma e impacto. A cena animada abaixo é <b>esquemática</b>: não é gravação do Unity nem a arte final.</p>
<div class="status"><div><b>66</b><span>nós canônicos</span></div><div><b>31</b><span>ações ativas</span></div><div><b>257/257</b><span>EditMode integrado da fase mecânica</span></div><div><b>não aceita</b><span>matriz de equilíbrio da fase 21</span></div></div>
<div class="warning"><b>Resposta direta:</b> há mecânicas implementadas e testes por família, mas <em>não</em> há comprovação de que todas as builds estejam equilibradas e visualmente prontas. A matriz revisada comprou 8/8 ações pré-capstone e 18/18 capstones legais, incluindo Telisandra/Thoren, com 22/22 armas nas builds de combate. Marca de Eficiência foi usada numa estação real da Farm. O probe classificou 52 instâncias Common e 10 Elite pelo asset e afixo; a distinção visual da Cave ainda precisa de correção. Um acerto real de Corte Giratório foi confirmado, mas nenhum TTK válido foi medido. Tiro Carregado agora tem bônus provisório de +2 de dano por rank; a comparação real de TTK ainda falta. Esta animação HTML é ilustrativa. A árvore, a tela de equipar, a pixel art e as animações finais ainda aguardam suas fases.</div></header>
<main><section class="panel"><div class="eyebrow">Demonstração interativa · arte esquemática</div><h2>Experimente as ações</h2><div class="layout"><div><div class="stage"><canvas id="stage" width="640" height="360" aria-label="Demonstração esquemática da habilidade selecionada"></canvas><span class="stage-label">simulação ilustrativa — dados reais do catálogo</span></div><div class="controls"><button id="cast">▶ Executar</button><button id="rank">Rank 1</button><span id="feedback" role="status"></span></div></div><div class="details"><div id="details"></div></div></div></section>
<section class="panel"><div class="eyebrow">31 ações · dados dos ScriptableObjects</div><h2>Catálogo ativo</h2><input class="search" id="search" placeholder="Buscar habilidade ou ID" aria-label="Buscar ação"><div class="filters" id="filters"></div><div class="grid" id="cards"></div></section>
<section class="panel"><div class="eyebrow">Passivas · capstones · desbloqueios</div><h2>Todos os 66 nós</h2><p class="note">Dados autorados dos nós: árvore, tier, rank máximo e vínculo de ação. O texto de cada nó descreve intenção de conteúdo; números de balanceamento comparativo ainda não foram aceitos.</p><div id="nodes"></div></section>
<section class="panel"><div class="eyebrow">Capturas reais do Unity · Fagulha Ígnea</div><h2>Único registro gráfico desta rodada</h2><div class="captures"><figure><img src="execution/captures/FarmScene-fire-spark.png" alt="Fagulha Ígnea na FarmScene capturada após o cast"><figcaption>FarmScene · câmera de gameplay após o cast</figcaption></figure><figure><img src="execution/captures/TownScene-fire-spark.png" alt="Fagulha Ígnea na TownScene capturada após o cast"><figcaption>TownScene · câmera de gameplay após o cast</figcaption></figure><figure><img src="execution/captures/CaveScene-fire-spark.png" alt="Fagulha Ígnea na CaveScene capturada após o cast"><figcaption>CaveScene · câmera de gameplay após o cast</figcaption></figure></div><p class="note">As capturas mostram avatar e projétil, mas o projétil é arte provisória. Camera.Render não demonstra UI, ciclo completo da animação nem aceite humano.</p></section>
<section class="panel"><div class="eyebrow">Observação real de PlayMode · Cave L20</div><h2>Corte Giratório acertou; TTK segue pendente</h2><p>Com R1 comprado e espada de ferro equipada, a ação atingiu uma instância Common natural de Spore Imp: <b>262 → 252 HP</b>, 10 de dano atribuído à habilidade e nenhum dano externo. O teste isolou o alvo por <code>EnemyInstanceId</code>. <a href="../../../Logs/phase21_combat_probe/phase21_common_whirl_hit_20260913_014846_393.json">Abrir observação bruta</a>.</p><p class="note">O novo driver de input passou 2/2 PlayMode. Numa seed aberta, a espada deu 4 hits e a skill 1 hit no alvo real, mas outros inimigos derrotaram o jogador; <a href="../../../Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_032306_533.json">ver r9</a>. O seletor conservador rejeitou os Common dos nove seeds testados; a candidata r11 tinha decor bloqueante ou inimigos a 2,28–5,12 unidades do ataque. <a href="../../../Logs/phase21_combat_probe/phase21_common_whirl_r1_ttk_20260913_035258_638.json">Ver rejeições r11</a>. Ainda não há TTK nem aceite de equilíbrio; a animação acima é ilustrativa.</p></section>
<section class="panel evidence"><div class="eyebrow">Como testar de verdade</div><h2>Evidências e limites</h2><ul><li><a href="mechanics/PHASE_10V_SURVIVAL_CRAFTING_INTEGRATED_REPORT.md">Fase mecânica final</a>: 257/257 EditMode, 5/5 Living Forge e 4/4 Caveborn PlayMode; relatórios anteriores cobrem melee, ranged, magic, survival e crafting.</li><li><a href="execution/FOUNDATION_REPORT.md">Fundação e capturas</a>: controller real nas três cenas, cooldown por actionId e status; cinco cenários PlayMode.</li><li><a href="balance/PHASE_09_BALANCE_ACCEPTANCE_REPORT.md">Equilíbrio comparativo</a>: fase 21 não aceita. O PlayMode comprou 18/18 capstones, executou Marca de Eficiência R1 em estação real e confirmou um acerto de Corte Giratório. O probe r8 classificou 52 Common/10 Elite por instância; Disparo Carregado passou 7/7 EditMode e 2/2 PlayMode focado com dano bruto R1–R3, mas combate completo ainda não produziu kill/TTK válido. Faltam Boss por estágio, comparação de efeitos por rank e aceitação gráfica. O smoke Farm/Town/Cave passou, mas a Cave mantém uma faixa escura na captura estabilizada.</li><li><a href="playmode/spec_skills_20_survival_crafting_capstones_v1_human_test_scenario.md">Roteiro humano</a>: permite testar no editor, mas o roteiro escrito não conta como execução.</li></ul><p class="note">Gerado dos bancos SkillActionDatabase.asset e SkillNodeDatabase.asset. Valores exibidos são v1; o dano da ação é a base antes de defesa, resistências e modificadores.</p></section></main><footer class="note">Cindar's Hope · visualização de revisão, não interface integrada ao jogo.</footer>
<script>const DATA=${payload};
const $=id=>document.getElementById(id);let selected=DATA.actions.find(a=>a.id==='skill_magic_fire_spark'),filter='all',query='',rank=1,start=-1;
const esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
function fmt(v){const n=Number(v);return Number.isInteger(n)?String(n):n.toFixed(1).replace('.',',')}
function select(a){selected=a;start=-1;$('details').innerHTML='<span class="tag" style="color:'+DATA.colors[a.category]+'">'+esc(DATA.labels[a.category])+'</span><h3>'+esc(a.name)+'</h3><small>'+esc(a.id)+'</small><p>'+esc(a.effect)+'</p><div class="metrics">'+[
['Dano base R'+rank,Number(a.ChargeTimeSeconds)>0?fmt(Number(a.BaseDamage)+Number(a.DamagePerRank)*(rank-1))+' → '+fmt(Number(a.ChargeMaximumDamage)+Number(a.DamagePerRank)*(rank-1)):fmt(Number(a.BaseDamage)+Number(a.DamagePerRank)*(rank-1))],['STA',Number(a.ChargeTimeSeconds)>0?fmt(a.StaminaCost)+' → '+fmt(a.ChargeMaximumStaminaCost):fmt(a.StaminaCost)],['MP',fmt(a.ManaCost)],['Recarga',fmt(a.CooldownSeconds)+' s'],['Alcance',Number(a.ChargeTimeSeconds)>0?fmt(a.Range)+' → '+fmt(a.ChargeMaximumRange)+' tiles':fmt(a.Range)+' tiles'],['Forma',shape(a)]].map(([k,v])=>'<div><small>'+k+'</small><b>'+esc(v)+'</b></div>').join('')+'</div>'+extras(a)+'<p class="note">Valores de asset; o desenho é uma analogia visual, não o efeito VFX final. <a href="'+esc(a.source)+'">Abrir asset</a>.</p>';renderCards();draw(0)}
function shape(a){if(Number(a.DashDistance)>0)return 'avanço';if(Number(a.LeapDistance)>0)return 'salto';if(Number(a.ProjectileCount)>1)return 'leque ×'+a.ProjectileCount;if(Number(a.SkillActionType)===6)return 'projétil';if(Number(a.SkillActionType)===7)return 'área';if(Number(a.ArcDegrees)>0)return 'arco '+fmt(a.ArcDegrees)+'°';return 'utilidade / gatilho'}
function extras(a){const x=[];if(a.StatusEffectId)x.push('Status: '+a.StatusEffectId.replace('status_',''));if(Number(a.LinePierceCount)>1)x.push('Perfura até '+a.LinePierceCount+' alvos');if(Number(a.PulseCount)>0)x.push(a.PulseCount+' pulsos');if(Number(a.ChargeTimeSeconds)>0)x.push('Carga até '+fmt(a.ChargeTimeSeconds)+' s');if(Number(a.NotYetExecutable))x.push('Marcada como não executável no asset');return x.map(t=>'<span class="tag">'+esc(t)+'</span>').join('')||'<span class="tag">Efeito condicionado pelo alvo/contexto</span>'}
function renderCards(){const rows=DATA.actions.filter(a=>(filter==='all'||a.category===filter)&&(a.name+' '+a.id).toLowerCase().includes(query));$('cards').innerHTML=rows.map(a=>'<button class="card '+(selected.id===a.id?'selected':'')+'" data-id="'+esc(a.id)+'"><span class="glyph" style="color:'+DATA.colors[a.category]+'">'+({melee:'✦',ranged:'➶',magic:'✧',survival:'✚',crafting:'⚒'}[a.category])+'</span><strong>'+esc(a.name)+'</strong><small>'+esc(DATA.labels[a.category])+' · '+esc(shape(a))+'</small></button>').join('');$('cards').querySelectorAll('button').forEach(b=>b.onclick=()=>select(DATA.actions.find(a=>a.id===b.dataset.id)))}
function renderNodes(){const groups=Object.keys(DATA.labels);$('nodes').innerHTML=groups.map(g=>'<h3 style="color:'+DATA.colors[g]+'">'+DATA.labels[g]+'</h3><div class="tree">'+DATA.nodes.filter(n=>n.category===g).sort((a,b)=>a.tier-b.tier||a.name.localeCompare(b.name)).map(n=>'<article><strong>'+esc(n.name)+(n.capstone?' ★':'')+'</strong><small>Tier '+n.tier+' · até rank '+n.maxRank+' · '+n.pointCost+' ponto(s)'+(n.actionId?' · ação ativa':' · passiva/efeito de sistema')+(n.dormant||n.effectPending?' · pendência no asset':'')+'</small>'+(n.description?'<div>'+esc(n.description)+'</div>':'')+'<a href="'+esc(n.source)+'">'+esc(n.id)+'</a></article>').join('')+'</div>').join('')}
const ctx=$('stage').getContext('2d');ctx.imageSmoothingEnabled=false;function rect(x,y,w,h,c){ctx.fillStyle=c;ctx.fillRect(Math.round(x),Math.round(y),Math.round(w),Math.round(h))}function circle(x,y,r,c){ctx.fillStyle=c;ctx.beginPath();ctx.arc(x,y,r,0,Math.PI*2);ctx.fill()}
function draw(now){const t=start<0?-1:Math.min(1,(now-start)/1000),a=selected,col=DATA.colors[a.category];rect(0,0,640,360,'#17272a');for(let y=0;y<360;y+=32)for(let x=0;x<640;x+=32){rect(x,y,31,31,(x/32+y/32)%2?'#253d38':'#2c453e');if((x+y)%96===0)rect(x+6,y+8,3,3,'#53735a')}rect(0,268,640,7,'#586754');rect(45,175,20,90,'#4a5350');rect(510,170,20,96,'#4a5350');let px=160,py=204;if(t>=0&&Number(a.DashDistance)>0)px+=Math.min(1,t*2)*170;if(t>=0&&Number(a.LeapDistance)>0){px+=Math.min(1,t*1.7)*155;py-=Math.sin(Math.PI*Math.min(1,t*1.7))*65}rect(px-10,py-10,20,8,'#ebc28a');rect(px-13,py-2,26,28,'#536f9a');rect(px-11,py+26,9,10,'#263747');rect(px+3,py+26,9,10,'#263747');rect(px+12,py+6,9,6,'#e6c89c');rect(444,190,25,18,'#d4b57c');rect(440,208,33,41,'#744b55');rect(442,249,9,14,'#3d3438');rect(461,249,9,14,'#3d3438');if(t>=0){let p=Math.max(0,Math.min(1,(t-.12)/.60));const typ=Number(a.SkillActionType);if(typ===6){for(let i=0;i<Math.max(1,Number(a.ProjectileCount));i++){let spread=(i-(Math.max(1,Number(a.ProjectileCount))-1)/2)*25;circle(px+35+p*(430-px-35),py+12+p*spread,7,col)}}else if(typ===7||Number(a.EffectRadius)>0){circle(453,221,20+70*p,col+'44');ctx.strokeStyle=col;ctx.lineWidth=4;ctx.beginPath();ctx.arc(453,221,20+70*p,0,Math.PI*2);ctx.stroke()}else if(Number(a.ArcDegrees)>0){ctx.strokeStyle=col;ctx.lineWidth=13;ctx.beginPath();ctx.arc(px+22,py+12,45+110*p,-.7,.7);ctx.stroke()}else{circle(px,py+10,28+45*p,col+'55');ctx.strokeStyle=col;ctx.lineWidth=4;ctx.strokeRect(px-25-20*p,py-26-20*p,50+40*p,75+40*p)}if(t>.65&&Number(a.BaseDamage)>0){rect(430,165,50,18,'#192b2b');ctx.fillStyle=col;ctx.font='bold 20px monospace';ctx.fillText('−'+(Number(a.BaseDamage)+Number(a.DamagePerRank)*(rank-1)),433,179)}if(t>=1){start=-1;$('feedback').textContent='Execução ilustrada; o resultado real depende de alvo, recursos e contexto.'}}ctx.fillStyle='#e9e2cd';ctx.font='14px monospace';ctx.fillText('JOGADOR',112,295);ctx.fillText('ALVO',432,295);if(start>=0)requestAnimationFrame(draw)}
$('cast').onclick=()=>{start=performance.now();$('feedback').textContent='Executando representação...';requestAnimationFrame(draw)};$('rank').onclick=()=>{rank=rank===1?3:1;$('rank').textContent='Rank '+rank;select(selected)};$('search').oninput=e=>{query=e.target.value.toLowerCase();renderCards()};$('filters').innerHTML=['all',...Object.keys(DATA.labels)].map(g=>'<button data-filter="'+g+'" class="'+(g==='all'?'active':'')+'">'+(g==='all'?'Todas':DATA.labels[g])+'</button>').join('');$('filters').querySelectorAll('button').forEach(b=>b.onclick=()=>{filter=b.dataset.filter;$('filters').querySelectorAll('button').forEach(x=>x.classList.toggle('active',x===b));renderCards()});renderNodes();select(selected);
</script></body></html>`;
fs.writeFileSync(output, html, 'utf8');
console.log(`${path.relative(root, output)}: ${actions.length} actions, ${nodes.length} nodes, ${nodes.filter(n => n.iconMissing).length} null node icons`);
