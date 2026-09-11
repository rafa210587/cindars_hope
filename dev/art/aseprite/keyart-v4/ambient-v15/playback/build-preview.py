"""Create timestamp-driven playback from real Unity captures, without synthesizing frames."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[6]
output = root / 'docs/validation/farm_keyart_v4/ambient_runtime15'
metadata = json.loads((output / 'capture-metadata.json').read_text(encoding='utf-8-sig'))
evidence = metadata['ambient']
samples = evidence['samples']
for sample in samples:
    sample['url'] = 'ambient-playback/' + Path(sample['path'].replace('\\', '/')).name
    assert (output / sample['url']).is_file()
payload = json.dumps(evidence, ensure_ascii=False).replace('</', '<\\/')
html = '''<!doctype html><html lang="pt-BR"><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>Fazenda · animações no Unity</title>
<style>body{background:#14221e;color:#e8ecd8;font:16px/1.6 system-ui;max-width:1150px;margin:32px auto;padding:0 20px}a{color:#edca8c}.grid{display:flex;flex-wrap:wrap;gap:22px}article{background:#203229;padding:20px;border-radius:12px;flex:1;min-width:260px}img{image-rendering:pixelated;max-width:100%;display:block}button,select{font:inherit;background:#38533d;color:white;padding:8px;border:1px solid #6b8266;border-radius:6px}small{display:block;color:#b6c6b8}.stage{min-height:280px;display:flex;align-items:center;justify-content:center;background:#102018}h2{margin-top:0}</style>
<h1>Movimento real na fazenda</h1><p>Recortes da câmera do Unity, no zoom de jogo. Reprodução usa os horários registrados; não cria quadros intermediários. A câmera foi reposicionada para observar cada objeto. Não é gravação de input ou da interface.</p>
<p id="result"></p><div class="grid" id="cards"></div><p><a href="../progress-review.html">Voltar ao comparativo</a> · <a href="capture-metadata.json">Evidência completa</a></p>
<script>const evidence=PAYLOAD;
document.querySelector('#result').textContent='Resultado automatizado: '+evidence.status+' · distância observada entre saltos: '+evidence.observedJumpSpacing.toFixed(2)+' s.';
for(const [source,title] of [['fountain','Fonte'],['cascade','Cascata'],['fish','Peixe']]){
 const frames=evidence.samples.filter(s=>s.source===source);const card=document.createElement('article');
 card.innerHTML='<h2>'+title+'</h2><div class="stage"><img alt="Captura real '+title+'"></div><small></small><button>Pausar</button> <select><option value="1">1× · ritmo real</option><option value="0.25">0,25× · inspeção</option></select>';
 document.querySelector('#cards').append(card);const img=card.querySelector('img'),label=card.querySelector('small');let playing=true,speed=1,elapsed=0,last=performance.now();
 const start=frames[0].time,end=frames[frames.length-1].time+.1,duration=end-start;
 if(source==='fish'){const jump=document.createElement('button');jump.textContent='Ir ao salto';jump.onclick=()=>{elapsed=evidence.jumps[0].observedStart-start-.2};card.append(jump)}
 card.querySelector('button').onclick=e=>{playing=!playing;e.target.textContent=playing?'Pausar':'Continuar'};card.querySelector('select').onchange=e=>speed=+e.target.value;
 function tick(now){if(playing)elapsed+=(now-last)/1000*speed;last=now;let t=start+elapsed%duration;let selected=frames[0];for(const f of frames){if(f.time>t)break;selected=f}if(img.dataset.path!==selected.url){img.src=selected.url;img.dataset.path=selected.url}label.textContent='Tempo de jogo '+selected.time.toFixed(2)+' s · '+(selected.sprite==='null'?'invisível':selected.sprite);requestAnimationFrame(tick)}requestAnimationFrame(tick);
}
</script></html>'''.replace('PAYLOAD', payload)
(output / 'index.html').write_text(html, encoding='utf-8')
print(output / 'index.html')

