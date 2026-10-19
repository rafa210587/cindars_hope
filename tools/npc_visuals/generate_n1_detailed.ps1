param([string]$Repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
Add-Type -AssemblyName System.Drawing
$walk = Join-Path $Repo 'Assets/_Game/Resources/NpcWalkSprites'
$body = Join-Path $Repo 'Assets/_Game/Resources/NpcSprites'
function Transform([string]$src,[string]$dst,[string]$kind) {
  $in = [Drawing.Bitmap]::new($src); $out=[Drawing.Bitmap]::new($in.Width,$in.Height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  for($y=0;$y -lt $in.Height;$y++){for($x=0;$x -lt $in.Width;$x++){
    $c=$in.GetPixel($x,$y); if($c.A -eq 0){continue}; $r=$c.R;$g=$c.G;$b=$c.B
    if($kind -eq 'corvus') {
      # Velorin's approved priest sheet supplies the detailed anatomy; map civic-blue mantle to Corvus ivory/gold.
      if($b -gt $r*1.08 -and $b -gt $g*0.98 -and $r -lt 150){$r=155+$b/5;$g=138+$b/7;$b=108+$b/10}
      elseif($r -gt $g*1.22 -and $g -gt $b*1.15 -and $r -lt 170){$r=195;$g=155;$b=52}
    } else {
      # Zrix's approved traveler silhouette supplies the hood, pack, boots and grounded footprint.
      # Repaint draconite green body and brown cloak into a neutral Vaalara wanderer palette.
      if($g -gt $r*1.05 -and $g -gt $b*1.05 -and $g -lt 205){$r=145;$g=105;$b=98}
      elseif($r -gt $g*1.15 -and $r -gt $b*1.35 -and $r -lt 150){$r=58;$g=72;$b=86}
    }
    $out.SetPixel($x,$y,[Drawing.Color]::FromArgb($c.A,[Math]::Min(255,[int]$r),[Math]::Min(255,[int]$g),[Math]::Min(255,[int]$b)))
  }}
  $out.Save($dst,[Drawing.Imaging.ImageFormat]::Png);$out.Dispose();$in.Dispose()
}
Transform (Join-Path $walk 'npc_velorin_walk.png') (Join-Path $walk 'npc_corvus_walk.png') 'corvus'
Transform (Join-Path $walk 'npc_zrix_walk.png') (Join-Path $walk 'npc_vaalara_wanderer_01_walk.png') 'wanderer'
Transform (Join-Path $body 'anciao_velorin.png') (Join-Path $body 'npc_corvus.png') 'corvus'
Transform (Join-Path $body 'zrix_das_estradas.png') (Join-Path $body 'npc_vaalara_wanderer_01.png') 'wanderer'
