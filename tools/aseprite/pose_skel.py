#!/usr/bin/env python
# pose_skel.py - gera esqueletos OpenPose (BODY-18 COCO) no formato que o ControlNet
# OpenPose entende: fundo preto, limbs+pontos coloridos no padrao OpenPose.
# Coordenadas normalizadas (0..1), eu controlo cada junta -> pose precisa e consistente
# entre frames (mesmo personagem, so muda a geometria).
import sys, math, argparse
from PIL import Image, ImageDraw

# 18 keypoints COCO: 0 nose,1 neck,2 Rsho,3 Relb,4 Rwri,5 Lsho,6 Lelb,7 Lwri,
# 8 Rhip,9 Rkne,10 Rank,11 Lhip,12 Lkne,13 Lank,14 Reye,15 Leye,16 Rear,17 Lear
LIMBS = [(1,2),(1,5),(2,3),(3,4),(5,6),(6,7),(1,8),(8,9),(9,10),
         (1,11),(11,12),(12,13),(1,0),(0,14),(14,16),(0,15),(15,17)]
COLORS = [(255,0,0),(255,85,0),(255,170,0),(255,255,0),(170,255,0),(85,255,0),
          (0,255,0),(0,255,85),(0,255,170),(0,255,255),(0,170,255),(0,85,255),
          (0,0,255),(85,0,255),(170,0,255),(255,0,255),(255,0,170),(255,0,85)]

# poses em coord normalizada {idx:(x,y)}; x->direita, y->baixo.
# Estimadas das referencias realistas (ai_stab/poseref/*) -> anatomia natural.
POSES = {
 # ATTACK 3 frames (swing): bracos de alto(windup) -> meio(swing) -> baixo-frente(impacto)
 "atk1": {
   0:(0.48,0.22),1:(0.49,0.30),2:(0.44,0.31),3:(0.50,0.23),4:(0.58,0.15),
   5:(0.54,0.31),6:(0.59,0.23),7:(0.63,0.16),8:(0.46,0.54),9:(0.44,0.66),10:(0.43,0.82),
   11:(0.54,0.54),12:(0.56,0.66),13:(0.57,0.82),
   14:(0.465,0.21),15:(0.50,0.21),16:(0.45,0.22),17:(0.51,0.22),
 },
 "atk2": {
   0:(0.52,0.24),1:(0.50,0.31),2:(0.45,0.32),3:(0.54,0.34),4:(0.64,0.40),
   5:(0.55,0.32),6:(0.62,0.36),7:(0.70,0.42),8:(0.46,0.55),9:(0.44,0.67),10:(0.43,0.83),
   11:(0.54,0.55),12:(0.56,0.67),13:(0.57,0.83),
   14:(0.50,0.23),15:(0.535,0.23),16:(0.485,0.24),17:(0.545,0.24),
 },
 "atk3": {
   0:(0.55,0.27),1:(0.51,0.33),2:(0.46,0.34),3:(0.56,0.42),4:(0.66,0.50),
   5:(0.56,0.34),6:(0.64,0.44),7:(0.72,0.52),8:(0.46,0.56),9:(0.44,0.68),10:(0.43,0.84),
   11:(0.54,0.56),12:(0.56,0.68),13:(0.57,0.84),
   14:(0.53,0.26),15:(0.565,0.26),16:(0.515,0.27),17:(0.575,0.27),
 },
 # idle de PERFIL (em pe parado, visto de lado, virado p/ direita)
 "idle_side": {
   0:(0.53,0.10),1:(0.50,0.19),2:(0.49,0.20),3:(0.50,0.31),4:(0.51,0.42),
   5:(0.49,0.20),6:(0.47,0.31),7:(0.46,0.42),8:(0.49,0.45),9:(0.50,0.63),10:(0.51,0.82),
   11:(0.49,0.45),12:(0.47,0.63),13:(0.46,0.82),
   14:(0.55,0.09),15:(0.515,0.09),16:(0.535,0.10),17:(0.49,0.10),
 },
 # em pe, frontal, levemente combat-ready
 "idle": {
   0:(0.50,0.10),1:(0.50,0.19),2:(0.41,0.20),3:(0.37,0.31),4:(0.36,0.42),
   5:(0.59,0.20),6:(0.63,0.31),7:(0.64,0.42),8:(0.45,0.45),9:(0.43,0.63),10:(0.42,0.82),
   11:(0.55,0.45),12:(0.57,0.63),13:(0.58,0.82),
   14:(0.475,0.09),15:(0.525,0.09),16:(0.44,0.10),17:(0.56,0.10),
 },
 # ataque: base larga, espada erguida nas DUAS maos atras/acima da cabeca (windup)
 "attack": {
   0:(0.46,0.22),1:(0.47,0.30),2:(0.42,0.31),3:(0.50,0.24),4:(0.60,0.16),
   5:(0.52,0.30),6:(0.58,0.24),7:(0.62,0.17),8:(0.43,0.54),9:(0.33,0.63),10:(0.24,0.73),
   11:(0.51,0.54),12:(0.60,0.64),13:(0.66,0.78),
   14:(0.44,0.21),15:(0.485,0.21),16:(0.43,0.22),17:(0.50,0.22),
 },
 # corrida: perfil p/ direita, joelho da frente erguido, perna de tras estendida
 "run": {
   0:(0.56,0.20),1:(0.50,0.27),2:(0.47,0.28),3:(0.41,0.30),4:(0.44,0.22),
   5:(0.50,0.28),6:(0.56,0.34),7:(0.62,0.40),8:(0.46,0.52),9:(0.56,0.54),10:(0.53,0.66),
   11:(0.45,0.52),12:(0.37,0.60),13:(0.29,0.55),
   14:(0.57,0.19),15:(0.54,0.19),16:(0.53,0.20),17:(0.51,0.205),
 },
 # defesa: perfil p/ esquerda, agachado, escudo (mao esquerda) estendido a frente-baixo
 "defend": {
   0:(0.50,0.21),1:(0.50,0.28),2:(0.54,0.29),3:(0.56,0.38),4:(0.52,0.47),
   5:(0.47,0.29),6:(0.41,0.38),7:(0.37,0.47),8:(0.55,0.55),9:(0.60,0.70),10:(0.62,0.85),
   11:(0.49,0.55),12:(0.43,0.70),13:(0.40,0.85),
   14:(0.49,0.20),15:(0.52,0.20),16:(0.47,0.21),17:(0.53,0.21),
 },
 # andar lateral: perfil p/ direita, passo natural, perna direita a frente
 "walk_side": {
   0:(0.53,0.19),1:(0.50,0.26),2:(0.49,0.27),3:(0.53,0.34),4:(0.55,0.42),
   5:(0.49,0.27),6:(0.45,0.34),7:(0.43,0.42),8:(0.49,0.52),9:(0.54,0.66),10:(0.57,0.82),
   11:(0.49,0.52),12:(0.45,0.66),13:(0.40,0.80),
   14:(0.54,0.18),15:(0.51,0.18),16:(0.52,0.19),17:(0.49,0.19),
 },
 # walk cycle lateral 4 frames COM LIFT de pe. perfil p/ direita.
 # f1 contato: perna D (R) frente esticada, perna E (L) tras esticada
 "walk1": {
   0:(0.53,0.19),1:(0.50,0.26),2:(0.49,0.27),3:(0.45,0.34),4:(0.43,0.42),
   5:(0.49,0.27),6:(0.53,0.34),7:(0.55,0.42),8:(0.49,0.52),9:(0.53,0.67),10:(0.56,0.83),
   11:(0.49,0.52),12:(0.45,0.67),13:(0.41,0.83),
   14:(0.54,0.18),15:(0.51,0.18),16:(0.52,0.19),17:(0.49,0.19),
 },
 # f2 passagem: perna E LEVANTA (joelho dobra a frente, pe recolhido no ar), D apoio esticada atras, corpo ALTO
 "walk2": {
   0:(0.53,0.175),1:(0.50,0.245),2:(0.49,0.255),3:(0.47,0.33),4:(0.46,0.41),
   5:(0.49,0.255),6:(0.51,0.33),7:(0.52,0.41),8:(0.49,0.50),9:(0.48,0.67),10:(0.46,0.84),
   11:(0.49,0.50),12:(0.54,0.62),13:(0.51,0.73),
   14:(0.54,0.165),15:(0.51,0.165),16:(0.52,0.175),17:(0.49,0.175),
 },
 # f3 contato oposto: perna E (L) frente esticada, D (R) tras esticada, bracos trocados
 "walk3": {
   0:(0.53,0.19),1:(0.50,0.26),2:(0.49,0.27),3:(0.53,0.34),4:(0.55,0.42),
   5:(0.49,0.27),6:(0.45,0.34),7:(0.43,0.42),8:(0.49,0.52),9:(0.45,0.67),10:(0.41,0.83),
   11:(0.49,0.52),12:(0.53,0.67),13:(0.56,0.83),
   14:(0.54,0.18),15:(0.51,0.18),16:(0.52,0.19),17:(0.49,0.19),
 },
 # f4 passagem: perna D LEVANTA (joelho a frente, pe no ar), E apoio esticada atras, corpo ALTO
 "walk4": {
   0:(0.53,0.175),1:(0.50,0.245),2:(0.49,0.255),3:(0.47,0.33),4:(0.46,0.41),
   5:(0.49,0.255),6:(0.51,0.33),7:(0.52,0.41),8:(0.49,0.50),9:(0.54,0.62),10:(0.51,0.73),
   11:(0.49,0.50),12:(0.48,0.67),13:(0.46,0.84),
   14:(0.54,0.165),15:(0.51,0.165),16:(0.52,0.175),17:(0.49,0.175),
 },
 # walk frontal frame 1: perna direita ERGUE (joelho alto, pe no ar), corpo baixo, bracos balancam
 "walk_down1": {
   0:(0.50,0.11),1:(0.50,0.20),2:(0.41,0.21),3:(0.39,0.32),4:(0.38,0.43),
   5:(0.59,0.21),6:(0.62,0.30),7:(0.63,0.40),8:(0.45,0.46),9:(0.44,0.58),10:(0.45,0.72),
   11:(0.55,0.46),12:(0.56,0.64),13:(0.57,0.84),
   14:(0.475,0.10),15:(0.525,0.10),16:(0.44,0.11),17:(0.56,0.11),
 },
 # frame 2: passagem/contato, corpo no ponto MAIS ALTO (bob), bracos neutros
 "walk_down2": {
   0:(0.50,0.08),1:(0.50,0.17),2:(0.41,0.18),3:(0.40,0.29),4:(0.40,0.40),
   5:(0.59,0.18),6:(0.60,0.29),7:(0.60,0.40),8:(0.46,0.43),9:(0.45,0.62),10:(0.45,0.82),
   11:(0.54,0.43),12:(0.55,0.62),13:(0.55,0.82),
   14:(0.475,0.07),15:(0.525,0.07),16:(0.44,0.08),17:(0.56,0.08),
 },
 # frame 3: perna esquerda ERGUE (joelho alto), corpo baixo, bracos trocados
 "walk_down3": {
   0:(0.50,0.11),1:(0.50,0.20),2:(0.41,0.21),3:(0.38,0.30),4:(0.37,0.40),
   5:(0.59,0.21),6:(0.61,0.32),7:(0.62,0.43),8:(0.45,0.46),9:(0.44,0.64),10:(0.43,0.84),
   11:(0.55,0.46),12:(0.56,0.58),13:(0.55,0.72),
   14:(0.475,0.10),15:(0.525,0.10),16:(0.44,0.11),17:(0.56,0.11),
 },
}

def compact_pose(pose, leg=0.70, head=1.18):
    # proporcao heroica/compacta (Children of Morta): pernas mais curtas + cabeca um pouco maior
    hip_y = (pose[8][1] + pose[11][1]) / 2.0
    neck = pose[1]
    out = {}
    for i, (x, y) in pose.items():
        if i in (9, 10, 12, 13):           # joelhos e tornozelos -> sobem (pernas curtas)
            y = hip_y + (y - hip_y) * leg
        if i in (0, 14, 15, 16, 17):       # cabeca -> afasta do pescoco (cabeca maior)
            x = neck[0] + (x - neck[0]) * head
            y = neck[1] + (y - neck[1]) * head
        out[i] = (x, y)
    return out

def draw(pose, W, H):
    img = Image.new("RGB",(W,H),(0,0,0))
    d = ImageDraw.Draw(img)
    sw = max(4, int(W/85))
    pts = {i:(x*W,y*H) for i,(x,y) in pose.items()}
    for i,(a,b) in enumerate(LIMBS):
        if a in pts and b in pts:
            x1,y1 = pts[a]; x2,y2 = pts[b]
            c = COLORS[i % len(COLORS)]
            d.line([(x1,y1),(x2,y2)], fill=c, width=sw)
            r = sw/2
            for (x,y) in ((x1,y1),(x2,y2)):
                d.ellipse([x-r,y-r,x+r,y+r], fill=c)
    rp = max(4, int(W/95))
    for i,(x,y) in pts.items():
        c = COLORS[i % len(COLORS)]
        d.ellipse([x-rp,y-rp,x+rp,y+rp], fill=c)
    return img

if __name__ == "__main__":
    p = argparse.ArgumentParser()
    p.add_argument("pose", choices=list(POSES.keys()))
    p.add_argument("--out", required=True)
    p.add_argument("--w", type=int, default=704)
    p.add_argument("--h", type=int, default=1024)
    p.add_argument("--compact", action="store_true")
    p.add_argument("--leg", type=float, default=0.70)
    p.add_argument("--head", type=float, default=1.18)
    a = p.parse_args()
    pose = compact_pose(POSES[a.pose], a.leg, a.head) if a.compact else POSES[a.pose]
    img = draw(pose, a.w, a.h)
    img.save(a.out)
    print(f"esqueleto '{a.pose}' -> {a.out} ({a.w}x{a.h})")
