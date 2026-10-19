#!/usr/bin/env python
# slice_sheet.py - pipeline de producao: sheet GPT -> remove fundo branco ->
# detecta linhas (bands) -> fatia frames por gaps -> downscale -> GIF por linha.
import sys, os, argparse
from PIL import Image, ImageDraw
import numpy as np

def load_rgba(path):
    return Image.open(path).convert('RGBA')

def remove_bg(img, thr=232):
    # flood fill do branco conectado as bordas -> transparente (preserva branco interno)
    im = img.convert('RGB')
    W,H = im.size
    sentinel = (255,0,255)
    seeds = []
    step = 8
    for x in range(0,W,step): seeds += [(x,0),(x,H-1)]
    for y in range(0,H,step): seeds += [(0,y),(W-1,y)]
    for s in seeds:
        px = im.getpixel(s)
        if px[0]>=thr and px[1]>=thr and px[2]>=thr:
            ImageDraw.floodfill(im, s, sentinel, thresh=40)
    arr = np.array(im)
    out = np.array(img)
    mask = (arr[:,:,0]==255)&(arr[:,:,1]==0)&(arr[:,:,2]==255)
    out[mask,3] = 0
    return Image.fromarray(out,'RGBA')

def alpha_rows(arr):
    return (arr[:,:,3]>10).sum(axis=1)  # pixels opacos por linha

def find_bands(mask1d, min_run, gap_merge=4):
    # acha intervalos contiguos com conteudo
    bands=[]; start=None; gap=0
    for i,v in enumerate(mask1d):
        if v:
            if start is None: start=i
            gap=0
        else:
            if start is not None:
                gap+=1
                if gap>gap_merge:
                    bands.append((start,i-gap+1)); start=None
    if start is not None: bands.append((start,len(mask1d)))
    return [(a,b) for (a,b) in bands if b-a>=min_run]

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('sheet')
    ap.add_argument('--out', required=True)
    ap.add_argument('--char_h', type=int, default=64)   # altura alvo do personagem (px jogo)
    ap.add_argument('--row_min', type=int, default=40)  # altura minima p/ ser linha de personagem
    ap.add_argument('--col_min', type=int, default=10)  # largura minima de um frame
    ap.add_argument('--fps', type=int, default=8)
    ap.add_argument('--skip_bg', action='store_true')  # sheet ja tem alpha (ex: rembg)
    a=ap.parse_args()
    os.makedirs(a.out, exist_ok=True)
    img=load_rgba(a.sheet) if a.skip_bg else remove_bg(load_rgba(a.sheet))
    img.save(os.path.join(a.out,'_nobg.png'))
    arr=np.array(img)
    rows=alpha_rows(arr)
    bands=find_bands(rows>3, a.row_min, gap_merge=6)
    print(f'{len(bands)} bandas de conteudo (altura>={a.row_min})')
    report=[]
    for ri,(y0,y1) in enumerate(bands):
        strip=arr[y0:y1,:,:]
        cols=(strip[:,:,3]>10).sum(axis=0)
        fbands=find_bands(cols>2, a.col_min, gap_merge=3)
        if not fbands: continue
        frames=[]
        for (x0,x1) in fbands:
            sub=img.crop((x0,y0,x1,y1))
            bb=sub.getbbox()
            if not bb: continue
            sub=sub.crop(bb)
            scale=a.char_h/sub.height
            nw=max(1,round(sub.width*scale)); nh=max(1,round(sub.height*scale))
            sub=sub.resize((nw,nh), Image.LANCZOS)
            frames.append(sub)
        if not frames: continue
        # canvas uniforme = max dims, personagem centrado embaixo
        cw=max(f.width for f in frames); ch=max(f.height for f in frames)
        cw+=4; ch+=4
        norm=[]
        for fi,f in enumerate(frames):
            cv=Image.new('RGBA',(cw,ch),(0,0,0,0))
            cv.paste(f,((cw-f.width)//2, ch-f.height-2), f)
            cv.save(os.path.join(a.out,f'row{ri:02d}_f{fi:02d}.png'))
            norm.append(cv)
        # GIF (fundo cinza p/ ver alpha)
        dur=int(1000/a.fps)
        gif=[Image.alpha_composite(Image.new('RGBA',(cw,ch),(60,60,70,255)),f).convert('P',palette=Image.ADAPTIVE) for f in norm]
        gif[0].save(os.path.join(a.out,f'row{ri:02d}.gif'),save_all=True,append_images=gif[1:],duration=dur,loop=0,disposal=2)
        report.append((ri,len(frames),cw,ch,y0,y1))
        print(f'row{ri:02d}: {len(frames)} frames, cell {cw}x{ch}, y {y0}-{y1}')
    print('OK', len(report),'linhas processadas ->', a.out)

if __name__=='__main__':
    main()
