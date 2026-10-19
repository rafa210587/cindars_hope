-- gen_sprite.lua — placeholder refinado por TIPO DE CORPO (silhueta + paleta + esqueleto de animacao)
-- Params: id, w, h, body(hex mid), accent(hex), outline(hex), eye(hex), bodytype, outdir
--   bodytype = humanoid | flying | blob | quadruped   (insect->blob, dragon->quadruped por enquanto)
-- Saida: <id>.aseprite, <id>_sheet.png, <id>_sheet.json, <id>_log.txt

local p = app.params
local outdir = (p["outdir"] or "."):gsub("\\", "/")
local function logTo(name, text)
  local f = io.open(outdir .. "/" .. name, "w"); if f then f:write(text); f:close() end
end

local ok, err = pcall(function()
  local id = p["id"] or "creature"
  local w = tonumber(p["w"]) or 32
  local h = tonumber(p["h"]) or 48
  local bodytype = p["bodytype"] or "humanoid"
  if bodytype == "insect" then bodytype = "blob" end
  if bodytype == "dragon" then bodytype = "quadruped" end

  local PC = app.pixelColor
  local function clamp(v) return math.max(0, math.min(255, math.floor(v))) end
  local function hex(s, defa)
    s = (s or defa):gsub("#","")
    return PC.rgba(tonumber(s:sub(1,2),16), tonumber(s:sub(3,4),16), tonumber(s:sub(5,6),16), 255)
  end
  local function shift(c, d)
    return PC.rgba(clamp(PC.rgbaR(c)+d), clamp(PC.rgbaG(c)+d), clamp(PC.rgbaB(c)+d), 255)
  end

  local body    = hex(p["body"],    "C0C4C8")
  local bodyLt  = shift(body,  30)
  local bodyDk  = shift(body, -38)
  local accent  = hex(p["accent"],  "6A2AA0")
  local outline = hex(p["outline"], "1A1620")
  local eye     = hex(p["eye"],     "E8E8F0")
  local clear   = PC.rgba(0,0,0,0)

  local spr = Sprite(w, h, ColorMode.RGB)
  spr.filename = outdir .. "/" .. id .. ".aseprite"
  local pal = Palette(7)
  pal:setColor(0, Color{r=0,g=0,b=0,a=0})
  pal:setColor(1, Color(bodyLt)); pal:setColor(2, Color(body)); pal:setColor(3, Color(bodyDk))
  pal:setColor(4, Color(accent)); pal:setColor(5, Color(outline)); pal:setColor(6, Color(eye))
  spr:setPalette(pal)
  spr.layers[1].name = "art"

  -- ---- primitivas ----
  local function setp(img,x,y,c)
    if x>=0 and y>=0 and x<w and y<h then img:drawPixel(x,y,c) end
  end
  local function rect(img,x0,y0,x1,y1,c) for y=y0,y1 do for x=x0,x1 do setp(img,x,y,c) end end end
  local function ellipse(img,cx,cy,rx,ry,c)
    for y=math.floor(cy-ry),math.ceil(cy+ry) do
      for x=math.floor(cx-rx),math.ceil(cx+rx) do
        local dx=(x-cx)/rx; local dy=(y-cy)/ry
        if dx*dx+dy*dy<=1.0 then setp(img,x,y,c) end
      end
    end
  end
  local function isEmpty(img,x,y)
    if x<0 or y<0 or x>=w or y>=h then return true end
    return PC.rgbaA(img:getPixel(x,y))==0
  end
  local function isMid(img,x,y)
    if x<0 or y<0 or x>=w or y>=h then return false end
    return img:getPixel(x,y)==body
  end
  -- rim light no topo/esq, rim shadow embaixo/dir (so nos pixels mid)
  local function rimShade(img)
    local lt,dk = {},{}
    for y=0,h-1 do for x=0,w-1 do
      if isMid(img,x,y) then
        if isEmpty(img,x,y-1) or isEmpty(img,x-1,y) then lt[#lt+1]={x,y}
        elseif isEmpty(img,x,y+1) or isEmpty(img,x+1,y) then dk[#dk+1]={x,y} end
      end
    end end
    for _,t in ipairs(lt) do setp(img,t[1],t[2],bodyLt) end
    for _,t in ipairs(dk) do setp(img,t[1],t[2],bodyDk) end
  end
  local function addOutline(img)
    local t={}
    for y=0,h-1 do for x=0,w-1 do
      if isEmpty(img,x,y) and ((not isEmpty(img,x-1,y)) or (not isEmpty(img,x+1,y))
         or (not isEmpty(img,x,y-1)) or (not isEmpty(img,x,y+1))) then t[#t+1]={x,y} end
    end end
    for _,q in ipairs(t) do setp(img,q[1],q[2],outline) end
  end
  local function finish(img) rimShade(img); addOutline(img); return img end
  local function newImg() return Image(w,h,ColorMode.RGB) end

  -- ---- templates por tipo de corpo ----
  local cx = math.floor(w/2)
  local templates = {}

  templates.humanoid = function(v)
    local img=newImg(); local cy=(v=="idle2") and 1 or 0
    if v=="death" then
      ellipse(img, cx, h-7, w*0.40, 5, body); rect(img,cx-2,h-9,cx+1,h-6,accent)
      return finish(img)
    end
    ellipse(img, cx, 8+cy, 6, 6, body)              -- cabeca
    rect(img, cx-7,13+cy, cx+6,33+cy, body)         -- torso
    if v=="attack" then rect(img,cx-11,9+cy,cx-8,22+cy,body) else rect(img,cx-9,14+cy,cx-7,29+cy,body) end
    rect(img, cx+7,14+cy, cx+9,29+cy, body)         -- bracos
    local s=(v=="walk") and 2 or 0
    rect(img, cx-5-s,34+cy, cx-2-s,45, body); rect(img, cx+1+s,34+cy, cx+4+s,45, body) -- pernas
    finish(img)
    rect(img, cx-2,18+cy, cx+1,26+cy, accent)       -- fenda do vazio (por cima da sombra)
    setp(img, cx-3,7+cy, eye); setp(img, cx+2,7+cy, eye)
    if v=="hit" then rect(img,cx-7,13,cx-6,33,accent) end
    return img
  end

  templates.flying = function(v)
    local img=newImg(); local up=(v=="idle1" or v=="walk" or v=="hit")
    if v=="death" then ellipse(img,cx,h-6,5,3,body); return finish(img) end
    ellipse(img, cx, math.floor(h/2), 4, 6, body)   -- corpo central
    local wy = math.floor(h/2) - (up and 4 or -2)
    -- asas (triangulos) variando com o flap
    for i=0,11 do
      local span = math.floor(11 - i*0.4)
      setp2 = nil
      rect(img, cx-4-i, wy+i-(up and 0 or 2), cx-4-i, wy+i+span, body)
      rect(img, cx+4+i, wy+i-(up and 0 or 2), cx+4+i, wy+i+span, body)
    end
    if v=="attack" then ellipse(img,cx,math.floor(h/2)+6,3,3,accent) end
    finish(img)
    setp(img, cx-2, math.floor(h/2)-2, eye); setp(img, cx+1, math.floor(h/2)-2, eye)
    return img
  end

  templates.blob = function(v)
    local img=newImg(); local sq=(v=="idle2" or v=="walk") and 1 or 0
    if v=="death" then ellipse(img,cx,h-5,w*0.42,3,body); return finish(img) end
    local ry = math.floor(h*0.32)-sq; local rx=math.floor(w*0.40)+sq
    ellipse(img, cx, h-4-ry, rx, ry, body)          -- massa gelatinosa
    finish(img)
    ellipse(img, cx, h-6-ry, math.max(2,rx-6), math.max(2,ry-5), accent) -- nucleo interno
    setp(img, cx-3, h-6-ry, eye); setp(img, cx+3, h-6-ry, eye)
    if v=="attack" then ellipse(img,cx,h-4-ry,rx,ry,accent) end
    return img
  end

  templates.quadruped = function(v)
    local img=newImg(); local cy=(v=="idle2") and 1 or 0
    if v=="death" then ellipse(img,cx,h-6,w*0.42,4,body); return finish(img) end
    ellipse(img, cx, math.floor(h*0.55)+cy, math.floor(w*0.40), math.floor(h*0.18), body) -- tronco
    ellipse(img, cx+math.floor(w*0.30), math.floor(h*0.45)+cy, 5, 4, body)                -- cabeca
    local s=(v=="walk") and 2 or 0
    local ly = math.floor(h*0.70)
    rect(img, cx-9, ly+cy, cx-7, h-3-s, body); rect(img, cx-3, ly+cy, cx-1, h-3+s, body)
    rect(img, cx+2, ly+cy, cx+4, h-3-s, body);  rect(img, cx+7, ly+cy, cx+9, h-3+s, body)
    if v=="attack" then rect(img, cx+math.floor(w*0.36),math.floor(h*0.40),cx+math.floor(w*0.46),math.floor(h*0.50),accent) end
    finish(img)
    rect(img, cx-6, math.floor(h*0.45)+cy, cx+4, math.floor(h*0.47)+cy, accent) -- faixa nas costas
    setp(img, cx+math.floor(w*0.33), math.floor(h*0.44)+cy, eye)
    return img
  end

  local tmpl = templates[bodytype] or templates.humanoid

  local kinds = {"idle1","idle2","walk","attack","hit","death"}
  spr:newCel(spr.layers[1], 1, tmpl(kinds[1]), Point(0,0))
  for i=2,#kinds do
    spr:newEmptyFrame(i)
    spr:newCel(spr.layers[1], i, tmpl(kinds[i]), Point(0,0))
  end
  local function tag(n,a,b) local t=spr:newTag(a,b); t.name=n end
  tag("idle",1,2); tag("walk",3,3); tag("attack",4,4); tag("hit",5,5); tag("death",6,6)

  spr:saveAs(spr.filename)
  local png=outdir.."/"..id.."_sheet.png"; local json=outdir.."/"..id.."_sheet.json"
  app.command.ExportSpriteSheet{ ui=false, askOverwrite=false, type=SpriteSheetType.ROWS,
    textureFilename=png, dataFilename=json, dataFormat=SpriteSheetDataFormat.JSON_HASH, listTags=true }
  logTo(id.."_log.txt", "OK id="..id.." type="..bodytype.." "..w.."x"..h.." frames="..#kinds.."\n")
end)

if not ok then logTo("_ERROR.txt", "FALHOU: "..tostring(err)) end
