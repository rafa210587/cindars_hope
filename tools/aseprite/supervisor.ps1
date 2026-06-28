# supervisor.ps1 — roda o lote de geracao com auto-restart do ComfyUI (resiliente a crash do DirectML)
param(
  [string]$Ckpt = "AziibPixelMix_Full.safetensors",
  [Parameter(Mandatory=$true)][string]$Out,
  [int]$Steps = 20,
  [int]$MaxRounds = 40,
  [int]$Total = 164,
  [string]$Cat = "",                              # filtra categoria (monster|npc|world|player)
  [double]$LoraStr = 1.1,                          # forca do LoRA (humanoides 1.0, monstros/world 0.8)
  [string]$Lora = "pixel-art-xl.safetensors"
)
$vpy   = "D:\AI\ComfyUI\venv\Scripts\python.exe"
$main  = "D:\AI\ComfyUI\main.py"
$drv   = "D:\Projetos\Jogos\Cindars_hope\cindars_hope\tools\aseprite\comfy_gen.ps1"
$server= "http://127.0.0.1:8188"
New-Item -ItemType Directory -Force $Out | Out-Null

function Test-Up { try { Invoke-RestMethod "$server/system_stats" -TimeoutSec 4 | Out-Null; return $true } catch { return $false } }

function Start-Comfy {
  Start-Process -FilePath $vpy `
    -ArgumentList @($main, "--directml", "--cpu-vae", "--disable-smart-memory", "--port", "8188") `
    -WorkingDirectory "D:\AI\ComfyUI" -WindowStyle Hidden `
    -RedirectStandardOutput "D:\AI\ComfyUI\_srv_out.log" -RedirectStandardError "D:\AI\ComfyUI\_srv_err.log" | Out-Null
  for ($i = 0; $i -lt 40; $i++) { if (Test-Up) { return $true }; Start-Sleep -Seconds 3 }
  return (Test-Up)
}

for ($r = 1; $r -le $MaxRounds; $r++) {
  if (-not (Test-Up)) {
    "[round $r] servidor fora do ar - iniciando ComfyUI..."
    if (-not (Start-Comfy)) { "[round $r] FALHOU ao subir o servidor"; Start-Sleep 5; continue }
    "[round $r] servidor no ar"
  }
  "[round $r] rodando driver (resumivel)..."
  & $drv -Ckpt $Ckpt -Out $Out -Steps $Steps -Cat $Cat -LoraStr $LoraStr -Lora $Lora 2>&1 | Tee-Object -FilePath "$Out\_batch.log" -Append
  $done = (Get-ChildItem "$Out\*_raw.png" -ErrorAction SilentlyContinue).Count
  "[round $r] progresso: $done/$Total"
  if ($done -ge $Total) { "=== LOTE COMPLETO: $done/$Total ==="; break }
  "[round $r] driver parou com $done/$Total (provavel crash do servidor) - vai reiniciar"
  Start-Sleep -Seconds 3
}
$final = (Get-ChildItem "$Out\*_raw.png" -ErrorAction SilentlyContinue).Count
"FIM. total gerado: $final/$Total"
