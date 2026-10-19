-- gen_placeholder.lua — gera um sprite placeholder (silhueta + paleta + esqueleto de animacao)
-- Uso: Aseprite.exe -b --script-param id=... --script-param w=32 --script-param h=48 \
--      --script-param body=C0C4C8 --script-param bodysh=9AA0A6 --script-param accent=6A2AA0 \
--      --script-param outline=1A1620 --script-param outdir=C:/.../out --script gen_placeholder.lua
-- Saida: <id>.aseprite, <id>_sheet.png, <id>_sheet.json, <id>_log.txt

local p = app.params
local outdir = (p["outdir"] or "."):gsub("\\", "/")

local function logTo(name, text)
  local f = io.open(outdir .. "/" .. name, "w")
  if f then f:write(text); f:close() end
end

local ok, err = pcall(function()
  local id = p["id"] or "creature"
  local w = tonumber(p["w"]) or 32
  local h = tonumber(p["h"]) or 48

  local function hexPix(s, defa)
    s = (s or defa):gsub("#", "")
    local r = tonumber(s:sub(1,2),16)
    local g = tonumber(s:sub(3,4),16)
    local b = tonumber(s:sub(5,6),16)
    return Color{ r=r, g=g, b=b, a=255 }.rgbaPixel
  end

  local body    = hexPix(p["body"],    "C0C4C8")
  local bodysh  = hexPix(p["bodysh"],  "9AA0A6")
  local accent  = hexPix(p["accent"],  "6A2AA0")
  local outline = hexPix(p["outline"], "1A1620")
  local clear   = app.pixelColor.rgba(0,0,0,0)

  local spr = Sprite(w, h, ColorMode.RGB)
  spr.filename = outdir .. "/" .. id .. ".aseprite"

  local pal = Palette(5)
  pal:setColor(0, Color{r=0,g=0,b=0,a=0})
  pal:setColor(1, Color(body))
  pal:setColor(2, Color(bodysh))
  pal:setColor(3, Color(accent))
  pal:setColor(4, Color(outline))
  spr:setPalette(pal)

  local layer = spr.layers[1]
  layer.name = "art"

  local function rect(img, x0,y0,x1,y1,col)
    for y=y0,y1 do for x=x0,x1 do
      if x>=0 and y>=0 and x<w and y<h then img:drawPixel(x,y,col) end
    end end
  end

  local function isEmpty(img,x,y)
    if x<0 or y<0 or x>=w or y>=h then return true end
    return app.pixelColor.rgbaA(img:getPixel(x,y)) == 0
  end

  local function addOutline(img)
    local targets = {}
    for y=0,h-1 do for x=0,w-1 do
      if isEmpty(img,x,y) then
        if (not isEmpty(img,x-1,y)) or (not isEmpty(img,x+1,y))
           or (not isEmpty(img,x,y-1)) or (not isEmpty(img,x,y+1)) then
          targets[#targets+1] = {x,y}
        end
      end
    end end
    for _,t in ipairs(targets) do img:drawPixel(t[1],t[2],outline) end
  end

  local function drawHumanoid(variant)
    local img = Image(w,h, ColorMode.RGB)
    local cy = (variant=="idle2") and 1 or 0
    local cx = math.floor(w/2)

    if variant=="death" then
      rect(img, 5, h-12, w-6, h-3, bodysh)
      rect(img, 7, h-10, w-8, h-5, body)
      rect(img, 12, h-9, 15, h-6, accent)
      addOutline(img)
      return img
    end

    -- head
    rect(img, cx-5, 3+cy, cx+4, 12+cy, body)
    img:drawPixel(cx-5, 3+cy, clear)
    img:drawPixel(cx+4, 3+cy, clear)
    -- torso
    rect(img, cx-7, 13+cy, cx+6, 33+cy, body)
    rect(img, cx+3, 13+cy, cx+6, 33+cy, bodysh) -- sombra lado direito
    -- arms
    if variant=="attack" then
      rect(img, cx-11, 9+cy, cx-8, 22+cy, bodysh)  -- braco esq erguido
      rect(img, cx+7, 14+cy, cx+9, 30+cy, bodysh)
    else
      rect(img, cx-9, 14+cy, cx-7, 30+cy, bodysh)
      rect(img, cx+7, 14+cy, cx+9, 30+cy, bodysh)
    end
    -- legs
    local split = (variant=="walk") and 2 or 0
    rect(img, cx-5-split, 34+cy, cx-2-split, 45, bodysh)
    rect(img, cx+1+split, 34+cy, cx+4+split, 45, bodysh)
    -- fenda do vazio no peito
    rect(img, cx-2, 18+cy, cx+1, 26+cy, accent)
    if variant=="hit" then
      -- flash: clareia o corpo trocando body por accent nas bordas do torso
      rect(img, cx-7, 13, cx-6, 33, accent)
    end
    addOutline(img)
    return img
  end

  local frames = {
    {kind="idle1"}, {kind="idle2"}, {kind="walk"},
    {kind="attack"}, {kind="hit"}, {kind="death"},
  }

  spr:newCel(layer, 1, drawHumanoid(frames[1].kind), Point(0,0))
  for i=2,#frames do
    spr:newEmptyFrame(i)
    spr:newCel(layer, i, drawHumanoid(frames[i].kind), Point(0,0))
  end

  local function tag(name, from, to)
    local t = spr:newTag(from, to); t.name = name
  end
  tag("idle", 1, 2)
  tag("walk", 3, 3)
  tag("attack", 4, 4)
  tag("hit", 5, 5)
  tag("death", 6, 6)

  spr:saveAs(spr.filename)

  local png  = outdir .. "/" .. id .. "_sheet.png"
  local json = outdir .. "/" .. id .. "_sheet.json"
  app.command.ExportSpriteSheet{
    ui=false,
    askOverwrite=false,
    type=SpriteSheetType.ROWS,
    textureFilename=png,
    dataFilename=json,
    dataFormat=SpriteSheetDataFormat.JSON_HASH,
    listLayers=false,
    listTags=true,
  }

  logTo(id .. "_log.txt", "OK id="..id.." "..w.."x"..h.." frames="..#frames.."\npng="..png.."\njson="..json.."\n")
end)

if not ok then
  logTo("_ERROR.txt", "FALHOU: " .. tostring(err))
end
