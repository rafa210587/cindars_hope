# comfy_sdxl_test.ps1 — gera sprites de teste com SDXL base + LoRA pixel-art-xl (estilo menos anime)
#
# !!! DEPRECATED (2026-06-28) — NAO USE PARA TESTAR POSE !!!
# Este script RE-INJETA "full body, standing, centered" no positivo (linha ~25), o que
# REINTRODUZ a pose de mostruario que removemos do build_prompts.py. Use comfy_gen.ps1:
#   .\comfy_gen.ps1 -Ckpt "sd_xl_base_1.0.safetensors" -Out "<dir>" -Id <id> -Steps 12
# Ele consome it.positive do prompts.json AS-IS (pose combat-ready ja embutida). Ver skill
# sprite-generation-pipeline.
param(
  [string]$Ckpt = "sd_xl_base_1.0.safetensors",
  [string]$Lora = "pixel-art-xl.safetensors",
  [Parameter(Mandatory=$true)][string]$Out,
  [string[]]$Ids,
  [double]$LoraStr = 1.1,
  [int]$Steps = 25,
  [double]$Cfg = 7.0,
  [string]$Prompts = "D:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\prompts.json",
  [string]$Server = "http://127.0.0.1:8188"
)
New-Item -ItemType Directory -Force $Out | Out-Null
$data = Get-Content $Prompts -Raw | ConvertFrom-Json
# criaturas teimosas levam negative booru anti-garota; humanoides (orc/npc) negative normal
$stubborn = @('cave_bat','crystal_leaper','frost_wailer','corrupted_draconic_spawn','stone_rat')
$negBooru = "1girl, 1woman, female, girl, woman, breasts, cleavage, humanoid, person, human, waifu, anime girl, cute girl, dress, skirt, " + $data.negative
$idx=0
foreach($id in $Ids){
  $idx++
  $it = $data.items | Where-Object { $_.id -eq $id }
  if(-not $it){ "[$idx] ${id}: NAO achei no prompts.json"; continue }
  $neg = $data.negative
  if($stubborn -contains $id){ $neg = $negBooru }
  $pos = "pixel art, " + $it.positive + ", full body, standing, centered, fills the frame, bold black outline, flat cel-shaded colors, consistent 16-bit jrpg game sprite style, simple solid white background"
  $wf = [ordered]@{
    "4"  = @{ class_type="CheckpointLoaderSimple"; inputs=@{ ckpt_name=$Ckpt } }
    "10" = @{ class_type="LoraLoader"; inputs=@{ model=@("4",0); clip=@("4",1); lora_name=$Lora; strength_model=$LoraStr; strength_clip=$LoraStr } }
    "5"  = @{ class_type="EmptyLatentImage"; inputs=@{ width=1024; height=1024; batch_size=1 } }
    "6"  = @{ class_type="CLIPTextEncode"; inputs=@{ text=$pos; clip=@("10",1) } }
    "7"  = @{ class_type="CLIPTextEncode"; inputs=@{ text=$neg; clip=@("10",1) } }
    "3"  = @{ class_type="KSampler"; inputs=@{ seed=(7000+$idx*13); steps=$Steps; cfg=$Cfg; sampler_name="euler"; scheduler="normal"; denoise=1.0; model=@("10",0); positive=@("6",0); negative=@("7",0); latent_image=@("5",0) } }
    "8"  = @{ class_type="VAEDecode"; inputs=@{ samples=@("3",0); vae=@("4",2) } }
    "9"  = @{ class_type="SaveImage"; inputs=@{ images=@("8",0); filename_prefix=("sdxl_"+$id) } }
  }
  $body = @{ prompt=$wf } | ConvertTo-Json -Depth 12
  try { $resp = Invoke-RestMethod -Uri "$Server/prompt" -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop }
  catch { "[$idx] ${id}: ERRO POST -> $($_.Exception.Message)"; continue }
  $pid2 = $resp.prompt_id; $img=$null; $cerr=0
  for($t=0;$t -lt 180;$t++){
    Start-Sleep -Seconds 2
    try{ $h=Invoke-RestMethod -Uri "$Server/history/$pid2" -ErrorAction Stop; $cerr=0 }catch{ $cerr++; if($cerr -gt 6){break}; continue }
    $e=$h.$pid2; if($e -and $e.outputs){ foreach($n in $e.outputs.PSObject.Properties){ if($n.Value.images){ $img=$n.Value.images[0]; break } }; if($img){break} }
  }
  if(-not $img){ "[$idx] ${id}: TIMEOUT"; continue }
  Invoke-WebRequest -Uri "$Server/view?filename=$($img.filename)&subfolder=$($img.subfolder)&type=$($img.type)" -OutFile (Join-Path $Out "$($id)_raw.png") -EA SilentlyContinue
  "[$idx/$($Ids.Count)] ${id}: OK"
}
"--- fim ---"
