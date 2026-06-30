#!/usr/bin/env python
# cn_pilot.py - piloto ControlNet/pose para sprites. Fala com a API do ComfyUI (stdlib only).
# Modos:
#   ref      - txt2img realista (gera referencia de pose), sem LoRA pixel
#   extract  - SDPose: extrai esqueleto de uma imagem e desenha (testa o preprocessor)
#   cn       - txt2img pixel art FORCADO por imagem de controle (ControlNet OpenPose)
#   img2img  - re-estiliza uma imagem base para pixel art (denoise medio, preserva pose)
import json, urllib.request, urllib.parse, time, sys, os, argparse

SERVER = "http://127.0.0.1:8188"
CKPT   = "sd_xl_base_1.0.safetensors"
LORA   = "pixel-art-xl.safetensors"
CN     = "controlnet-openpose-sdxl-xinsir.safetensors"

def post(wf):
    data = json.dumps({"prompt": wf}).encode()
    req = urllib.request.Request(SERVER + "/prompt", data=data, headers={"Content-Type": "application/json"})
    return json.load(urllib.request.urlopen(req))["prompt_id"]

def wait_img(pid, timeout=360):
    for _ in range(timeout // 2):
        time.sleep(2)
        try:
            h = json.load(urllib.request.urlopen(SERVER + f"/history/{pid}"))
        except Exception:
            continue
        if pid in h:
            entry = h[pid]
            if entry.get("status", {}).get("status_str") == "error":
                print("  ERRO no workflow:", json.dumps(entry.get("status", {}))[:500]); return None
            if entry.get("outputs"):
                for node in entry["outputs"].values():
                    if node.get("images"):
                        return node["images"][0]
    return None

def fetch(img, outpath):
    q = urllib.parse.urlencode({"filename": img["filename"], "subfolder": img.get("subfolder", ""), "type": img["type"]})
    urllib.request.urlretrieve(SERVER + "/view?" + q, outpath)

def upload(path):
    # envia imagem pro input do ComfyUI (necessario p/ LoadImage)
    import mimetypes, uuid
    boundary = "----cnpilot" + uuid.uuid4().hex
    fname = os.path.basename(path)
    with open(path, "rb") as f:
        content = f.read()
    body = b""
    body += ("--" + boundary + "\r\n").encode()
    body += (f'Content-Disposition: form-data; name="image"; filename="{fname}"\r\n').encode()
    body += b"Content-Type: image/png\r\n\r\n"
    body += content + b"\r\n"
    body += ("--" + boundary + "\r\n").encode()
    body += b'Content-Disposition: form-data; name="overwrite"\r\n\r\ntrue\r\n'
    body += ("--" + boundary + "--\r\n").encode()
    req = urllib.request.Request(SERVER + "/upload/image", data=body,
                                 headers={"Content-Type": "multipart/form-data; boundary=" + boundary})
    r = json.load(urllib.request.urlopen(req))
    return r["name"]

def run(wf, out):
    pid = post(wf)
    print("  prompt_id:", pid)
    img = wait_img(pid)
    if not img:
        print("  SEM IMAGEM (timeout/erro)"); return False
    fetch(img, out)
    ok = os.path.exists(out) and os.path.getsize(out) > 0
    print(f"  -> {out} ({os.path.getsize(out) if ok else 0} bytes)")
    return ok

def mode_ref(a):
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "5": {"class_type": "EmptyLatentImage", "inputs": {"width": a.w, "height": a.h, "batch_size": 1}},
      "6": {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": ["4", 1]}},
      "7": {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": ["4", 1]}},
      "3": {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
            "sampler_name": "dpmpp_2m", "scheduler": "karras", "denoise": 1.0,
            "model": ["4", 0], "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["5", 0]}},
      "8": {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}},
      "9": {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "ref"}},
    }
    return run(wf, a.out)

def mode_extract(a):
    name = upload(a.image)
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "1": {"class_type": "LoadImage", "inputs": {"image": name}},
      "2": {"class_type": "SDPoseKeypointExtractor", "inputs": {"model": ["4", 0], "vae": ["4", 2], "image": ["1", 0], "batch_size": 1}},
      "3": {"class_type": "SDPoseDrawKeypoints", "inputs": {"keypoints": ["2", 0], "draw_body": True,
            "draw_hands": True, "draw_face": False, "draw_feet": True, "stick_width": 4,
            "face_point_size": 2, "score_threshold": 0.3, "draw_head": True}},
      "9": {"class_type": "SaveImage", "inputs": {"images": ["3", 0], "filename_prefix": "skel"}},
    }
    return run(wf, a.out)

def mode_cn(a):
    ctrl = upload(a.control)
    model, clip = ["4", 0], ["4", 1]
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "1": {"class_type": "LoadImage", "inputs": {"image": ctrl}},
      "11": {"class_type": "ControlNetLoader", "inputs": {"control_net_name": CN}},
      "5": {"class_type": "EmptyLatentImage", "inputs": {"width": a.w, "height": a.h, "batch_size": 1}},
    }
    if a.lora:
        wf["10"] = {"class_type": "LoraLoader", "inputs": {"model": ["4", 0], "clip": ["4", 1],
                    "lora_name": LORA, "strength_model": a.lora, "strength_clip": a.lora}}
        model, clip = ["10", 0], ["10", 1]
    wf["6"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": clip}}
    wf["7"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": clip}}
    wf["12"] = {"class_type": "ControlNetApplyAdvanced", "inputs": {"positive": ["6", 0], "negative": ["7", 0],
                "control_net": ["11", 0], "image": ["1", 0], "strength": a.cn_str,
                "start_percent": 0.0, "end_percent": a.cn_end, "vae": ["4", 2]}}
    wf["3"] = {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
               "sampler_name": "euler", "scheduler": "normal", "denoise": 1.0,
               "model": model, "positive": ["12", 0], "negative": ["12", 1], "latent_image": ["5", 0]}}
    wf["8"] = {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}}
    wf["9"] = {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "cn"}}
    return run(wf, a.out)

def mode_cn_i2i(a):
    # img2img (preserva identidade do frame-mestre PIXEL) + ControlNet (impoe a pose).
    base = upload(a.image)      # frame-ancora (mestre de identidade)
    ctrl = upload(a.control)    # esqueleto da pose-alvo
    model, clip = ["4", 0], ["4", 1]
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "1": {"class_type": "LoadImage", "inputs": {"image": base}},
      "20": {"class_type": "LoadImage", "inputs": {"image": ctrl}},
      "2": {"class_type": "VAEEncode", "inputs": {"pixels": ["1", 0], "vae": ["4", 2]}},
      "11": {"class_type": "ControlNetLoader", "inputs": {"control_net_name": CN}},
    }
    if a.lora:
        wf["10"] = {"class_type": "LoraLoader", "inputs": {"model": ["4", 0], "clip": ["4", 1],
                    "lora_name": LORA, "strength_model": a.lora, "strength_clip": a.lora}}
        model, clip = ["10", 0], ["10", 1]
    wf["6"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": clip}}
    wf["7"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": clip}}
    wf["12"] = {"class_type": "ControlNetApplyAdvanced", "inputs": {"positive": ["6", 0], "negative": ["7", 0],
                "control_net": ["11", 0], "image": ["20", 0], "strength": a.cn_str,
                "start_percent": 0.0, "end_percent": a.cn_end, "vae": ["4", 2]}}
    wf["3"] = {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
               "sampler_name": "euler", "scheduler": "normal", "denoise": a.denoise,
               "model": model, "positive": ["12", 0], "negative": ["12", 1], "latent_image": ["2", 0]}}
    wf["8"] = {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}}
    wf["9"] = {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "cni2i"}}
    return run(wf, a.out)

def mode_ip_i2i(a):
    # img2img do frame LPC (POSE/movimento da imagem) + IP-Adapter (IDENTIDADE travada do fazendeiro)
    base = upload(a.image)      # frame LPC (da o movimento das pernas)
    ref = upload(a.ref)         # imagem de identidade (fazendeiro)
    model, clip = ["4", 0], ["4", 1]
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "30": {"class_type": "IPAdapterModelLoader", "inputs": {"ipadapter_file": "ip-adapter-plus_sdxl_vit-h.safetensors"}},
      "31": {"class_type": "CLIPVisionLoader", "inputs": {"clip_name": "CLIP-ViT-H-14-laion2B-s32B-b79K.safetensors"}},
      "32": {"class_type": "LoadImage", "inputs": {"image": ref}},
      "1": {"class_type": "LoadImage", "inputs": {"image": base}},
      "2": {"class_type": "VAEEncode", "inputs": {"pixels": ["1", 0], "vae": ["4", 2]}},
    }
    if a.lora:
        wf["10"] = {"class_type": "LoraLoader", "inputs": {"model": ["4", 0], "clip": ["4", 1],
                    "lora_name": LORA, "strength_model": a.lora, "strength_clip": a.lora}}
        model, clip = ["10", 0], ["10", 1]
    wf["33"] = {"class_type": "IPAdapterAdvanced", "inputs": {
        "model": model, "ipadapter": ["30", 0], "image": ["32", 0],
        "weight": a.ip_weight, "weight_type": "linear", "combine_embeds": "concat",
        "start_at": 0.0, "end_at": 1.0, "embeds_scaling": "V only", "clip_vision": ["31", 0]}}
    wf["6"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": clip}}
    wf["7"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": clip}}
    wf["3"] = {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
               "sampler_name": "dpmpp_2m", "scheduler": "karras", "denoise": a.denoise,
               "model": ["33", 0], "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["2", 0]}}
    wf["8"] = {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}}
    wf["9"] = {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "ipi2i"}}
    return run(wf, a.out)

def mode_pose(a):
    # extrai esqueleto OpenPose (DWPose) de uma imagem de referencia (ex: frame LPC)
    name = upload(a.image)
    wf = {
      "1": {"class_type": "LoadImage", "inputs": {"image": name}},
      "2": {"class_type": "DWPreprocessor", "inputs": {"image": ["1", 0],
            "detect_hand": "disable", "detect_body": "enable", "detect_face": "disable",
            "resolution": a.w, "bbox_detector": a.bbox, "pose_estimator": "dw-ll_ucoco_384.onnx",
            "scale_stick_for_xinsr_cn": "enable"}},
      "9": {"class_type": "SaveImage", "inputs": {"images": ["2", 0], "filename_prefix": "pose"}},
    }
    return run(wf, a.out)

def mode_ip_cn(a):
    # IP-Adapter (trava IDENTIDADE do frame-ancora) + ControlNet (POSE 100% do esqueleto), txt2img.
    ref = upload(a.image)       # imagem de identidade (frame-ancora)
    ctrl = upload(a.control)    # esqueleto da pose-alvo
    model, clip = ["4", 0], ["4", 1]
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "30": {"class_type": "IPAdapterModelLoader", "inputs": {"ipadapter_file": "ip-adapter-plus_sdxl_vit-h.safetensors"}},
      "31": {"class_type": "CLIPVisionLoader", "inputs": {"clip_name": "CLIP-ViT-H-14-laion2B-s32B-b79K.safetensors"}},
      "32": {"class_type": "LoadImage", "inputs": {"image": ref}},
      "1": {"class_type": "LoadImage", "inputs": {"image": ctrl}},
      "11": {"class_type": "ControlNetLoader", "inputs": {"control_net_name": CN}},
      "5": {"class_type": "EmptyLatentImage", "inputs": {"width": a.w, "height": a.h, "batch_size": 1}},
    }
    if a.lora:
        wf["10"] = {"class_type": "LoraLoader", "inputs": {"model": ["4", 0], "clip": ["4", 1],
                    "lora_name": LORA, "strength_model": a.lora, "strength_clip": a.lora}}
        model, clip = ["10", 0], ["10", 1]
    wf["33"] = {"class_type": "IPAdapterAdvanced", "inputs": {
        "model": model, "ipadapter": ["30", 0], "image": ["32", 0],
        "weight": a.ip_weight, "weight_type": "linear", "combine_embeds": "concat",
        "start_at": 0.0, "end_at": 1.0, "embeds_scaling": "V only", "clip_vision": ["31", 0]}}
    wf["6"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": clip}}
    wf["7"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": clip}}
    wf["12"] = {"class_type": "ControlNetApplyAdvanced", "inputs": {"positive": ["6", 0], "negative": ["7", 0],
                "control_net": ["11", 0], "image": ["1", 0], "strength": a.cn_str,
                "start_percent": 0.0, "end_percent": a.cn_end, "vae": ["4", 2]}}
    wf["3"] = {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
               "sampler_name": "dpmpp_2m", "scheduler": "karras", "denoise": 1.0,
               "model": ["33", 0], "positive": ["12", 0], "negative": ["12", 1], "latent_image": ["5", 0]}}
    wf["8"] = {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}}
    wf["9"] = {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "ipcn"}}
    return run(wf, a.out)

def mode_img2img(a):
    name = upload(a.image)
    model, clip = ["4", 0], ["4", 1]
    wf = {
      "4": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
      "1": {"class_type": "LoadImage", "inputs": {"image": name}},
      "2": {"class_type": "VAEEncode", "inputs": {"pixels": ["1", 0], "vae": ["4", 2]}},
    }
    if a.lora:
        wf["10"] = {"class_type": "LoraLoader", "inputs": {"model": ["4", 0], "clip": ["4", 1],
                    "lora_name": LORA, "strength_model": a.lora, "strength_clip": a.lora}}
        model, clip = ["10", 0], ["10", 1]
    wf["6"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.pos, "clip": clip}}
    wf["7"] = {"class_type": "CLIPTextEncode", "inputs": {"text": a.neg, "clip": clip}}
    wf["3"] = {"class_type": "KSampler", "inputs": {"seed": a.seed, "steps": a.steps, "cfg": a.cfg,
               "sampler_name": "euler", "scheduler": "normal", "denoise": a.denoise,
               "model": model, "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["2", 0]}}
    wf["8"] = {"class_type": "VAEDecode", "inputs": {"samples": ["3", 0], "vae": ["4", 2]}}
    wf["9"] = {"class_type": "SaveImage", "inputs": {"images": ["8", 0], "filename_prefix": "i2i"}}
    return run(wf, a.out)

if __name__ == "__main__":
    p = argparse.ArgumentParser()
    p.add_argument("mode", choices=["ref", "extract", "cn", "img2img", "cn_i2i", "ip_cn", "pose", "ip_i2i"])
    p.add_argument("--out", required=True)
    p.add_argument("--pos", default="")
    p.add_argument("--neg", default="")
    p.add_argument("--image", default="")       # extract/img2img: imagem base
    p.add_argument("--control", default="")      # cn: imagem de controle (esqueleto)
    p.add_argument("--w", type=int, default=832)
    p.add_argument("--h", type=int, default=1024)
    p.add_argument("--steps", type=int, default=22)
    p.add_argument("--cfg", type=float, default=7.0)
    p.add_argument("--seed", type=int, default=12345)
    p.add_argument("--lora", type=float, default=0.0)     # 0 = sem LoRA pixel; >0 = forca
    p.add_argument("--cn_str", type=float, default=1.0)
    p.add_argument("--cn_end", type=float, default=1.0)
    p.add_argument("--denoise", type=float, default=0.55)
    p.add_argument("--ip_weight", type=float, default=0.8)
    p.add_argument("--bbox", default="yolox_l.onnx")
    p.add_argument("--ref", default="")
    a = p.parse_args()
    ok = {"ref": mode_ref, "extract": mode_extract, "cn": mode_cn, "img2img": mode_img2img, "cn_i2i": mode_cn_i2i, "ip_cn": mode_ip_cn, "pose": mode_pose, "ip_i2i": mode_ip_i2i}[a.mode](a)
    sys.exit(0 if ok else 1)
