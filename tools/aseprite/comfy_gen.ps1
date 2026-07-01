# comfy_gen.ps1 — dirige a API do ComfyUI pra gerar sprites a partir de prompts.json
# Uso:
#   .\comfy_gen.ps1 -Ckpt "v1-5-pruned-emaonly-fp16.safetensors" -Out "C:\saida" -Id npc_corvus
#   .\comfy_gen.ps1 -Ckpt "aziib.safetensors" -Out "C:\saida" -Cat npc -Limit 5
param(
  [Parameter(Mandatory=$true)][string]$Ckpt,
  [Parameter(Mandatory=$true)][string]$Out,
  [string]$Prompts = "D:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\prompts.json",
  [string]$Server = "http://127.0.0.1:8188",
  [string]$Id = "",        # gerar so esse id
  [string]$Cat = "",       # filtrar por categoria (monster|npc|world)
  [int]$Limit = 0,         # max de itens
  [int]$Steps = 22,
  [double]$Cfg = 7.0,
  [int]$Seed = 0,          # 0 = aleatorio por item (variado pelo indice)
  [string]$Positive = "",  # override do prompt positivo (use com -Id pra testar)
  [string]$Negative = "",  # override do prompt negativo
  [string]$Suffix = "",    # sufixo no nome do arquivo de saida (p/ variacoes: _v1, _v2...)
  [string]$Lora = "pixel-art-xl.safetensors",  # LoRA de estilo (SDXL); "" desliga
  [double]$LoraStr = 1.1,   # forca da LoRA
  [int]$MaxSide = 1024      # lado maior da geracao (SDXL=1024; SD1.5/Aziib use 768)
)

New-Item -ItemType Directory -Force $Out | Out-Null
$data = Get-Content $Prompts -Raw | ConvertFrom-Json
$items = $data.items
if($Id){ $items = $items | Where-Object { $_.id -eq $Id } }
if($Cat){ $items = $items | Where-Object { $_.cat -eq $Cat } }
if($Limit -gt 0){ $items = $items | Select-Object -First $Limit }
"itens a gerar: $($items.Count)"

function Get-GenSize($w,$h){
  # lado maior = $MaxSide, multiplo de 64, mantendo aspecto (SDXL=1024; SD1.5/Aziib=768)
  $ms = $MaxSide
  $ar = $w / $h
  if($ar -ge 1){ $W=$ms; $H=[math]::Round($ms/$ar/64)*64 } else { $H=$ms; $W=[math]::Round($ms*$ar/64)*64 }
  $min = [math]::Min(512, $ms)
  if($W -lt $min){$W=$min}; if($H -lt $min){$H=$min}
  return @($W,$H)
}

$idx = 0
foreach($it in $items){
  $idx++
  $done = Join-Path $Out "$($it.id)$($Suffix)_raw.png"
  if(Test-Path $done){ "[$idx/$($items.Count)] $($it.id): ja existe, pulando"; continue }
  $sz = Get-GenSize $it.w $it.h
  $pos = if($Positive){$Positive}else{$it.positive}
  $neg = if($Negative){$Negative}else{$it.negative}
  $seed = if($Seed -gt 0){$Seed}else{ 100000 + $idx * 7919 }
  $modelRef = @("4",0); $clipRef = @("4",1)
  $wf = [ordered]@{
    "4" = @{ class_type="CheckpointLoaderSimple"; inputs=@{ ckpt_name=$Ckpt } }
    "5" = @{ class_type="EmptyLatentImage"; inputs=@{ width=$sz[0]; height=$sz[1]; batch_size=1 } }
  }
  if($Lora){
    $wf["10"] = @{ class_type="LoraLoader"; inputs=@{ model=@("4",0); clip=@("4",1); lora_name=$Lora; strength_model=$LoraStr; strength_clip=$LoraStr } }
    $modelRef = @("10",0); $clipRef = @("10",1)
  }
  $wf["6"] = @{ class_type="CLIPTextEncode"; inputs=@{ text=$pos; clip=$clipRef } }
  $wf["7"] = @{ class_type="CLIPTextEncode"; inputs=@{ text=$neg; clip=$clipRef } }
  $wf["3"] = @{ class_type="KSampler"; inputs=@{ seed=$seed; steps=$Steps; cfg=$Cfg; sampler_name="euler"; scheduler="normal"; denoise=1.0; model=$modelRef; positive=@("6",0); negative=@("7",0); latent_image=@("5",0) } }
  $wf["8"] = @{ class_type="VAEDecode"; inputs=@{ samples=@("3",0); vae=@("4",2) } }
  $wf["9"] = @{ class_type="SaveImage"; inputs=@{ images=@("8",0); filename_prefix=("ch_"+$it.id) } }
  $body = @{ prompt=$wf } | ConvertTo-Json -Depth 12
  try {
    $resp = Invoke-RestMethod -Uri "$Server/prompt" -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
  } catch {
    "[$idx/$($items.Count)] $($it.id): ERRO no POST -> $($_.Exception.Message)"; continue
  }
  $promptId = $resp.prompt_id
  # poll history
  $img = $null; $cerr = 0
  for($t=0; $t -lt 120; $t++){
    Start-Sleep -Seconds 2
    try { $hist = Invoke-RestMethod -Uri "$Server/history/$promptId" -ErrorAction Stop; $cerr = 0 }
    catch { $cerr++; if($cerr -gt 5){ "  servidor caiu durante poll - abortando run"; return }; continue }
    $entry = $hist.$promptId
    if($entry -and $entry.outputs){
      foreach($node in $entry.outputs.PSObject.Properties){
        if($node.Value.images){ $img = $node.Value.images[0]; break }
      }
      if($img){ break }
    }
  }
  if(-not $img){ "[$idx/$($items.Count)] $($it.id): TIMEOUT/sem imagem"; continue }
  $vurl = "$Server/view?filename=$($img.filename)&subfolder=$($img.subfolder)&type=$($img.type)"
  Invoke-WebRequest -Uri $vurl -OutFile (Join-Path $Out "$($it.id)$($Suffix)_raw.png") -ErrorAction SilentlyContinue
  "[$idx/$($items.Count)] $($it.id): OK ($($sz[0])x$($sz[1]))"
}
"--- fim ---"
