-- gen_prop.lua — gerador de props/itens (e um humanoide detalhado) com sombreamento por tons
-- Params: id, kind, w, h, c1, c2, outdir
--   kind = warrior | tree | ore_node | crystal | barrel | crate | potion | ingot
-- Saida: <id>.aseprite, <id>_sheet.png, <id>_sheet.json, <id>_log.txt
local p = app.params
local outdir = (p["outdir"] or "."):gsub("\\","/")
local function logTo(n,t) local f=io.open(outdir.."/"..n,"w"); if f then f:write(t); f:close() end end

local ok,err = pcall(function()
  local id = p["id"] or "prop"
  local kind = p["kind"] or "barrel"
  local w = tonumber(p["w"]) or 32
  local h = tonumber(p["h"]) or 32
  local PC = app.pixelColor
  local function clamp(v) return math.max(0,math.min(255,math.floor(v))) end
  local function hex(s,d) s=(s or d):gsub("#",""); return PC.rgba(tonumber(s:sub(1,2),16),tonumber(s:sub(3,4),16),tonumber(s:sub(5,6),16),255) end
  local function shift(c,d) return PC.rgba(clamp(PC.rgbaR(c)+d),clamp(PC.rgbaG(c)+d),clamp(PC.rgbaB(c)+d),255) end
  local c1 = hex(p["c1"], "8B5E3C")
  local c2 = hex(p["c2"], "8898A8")
  local outline = hex(p["outline"], "1A1620")
  local clear = PC.rgba(0,0,0,0)

  local spr = Sprite(w,h,ColorMode.RGB); spr.filename = outdir.."/"..id..".aseprite"
  spr.layers[1].name = "art"
  local img = Image(w,h,ColorMode.RGB)

  local function setp(x,y,c) if x>=0 and y>=0 and x<w and y<h then img:drawPixel(x,y,c) end end
  local function rect(x0,y0,x1,y1,c) for y=y0,y1 do for x=x0,x1 do setp(x,y,c) end end end
  local function ellipse(cx,cy,rx,ry,c)
    for y=math.floor(cy-ry),math.ceil(cy+ry) do for x=math.floor(cx-rx),math.ceil(cx+rx) do
      local dx=(x-cx)/rx; local dy=(y-cy)/ry; if dx*dx+dy*dy<=1.0 then setp(x,y,c) end
    end end
  end
  local function ball(cx,cy,rx,ry,base) -- volume: dk cheio, mid e lt deslocados p/ cima-esq
    ellipse(cx,cy,rx,ry,shift(base,-36))
    ellipse(cx-rx*0.16,cy-ry*0.16,rx*0.82,ry*0.82,base)
    ellipse(cx-rx*0.30,cy-ry*0.30,rx*0.5,ry*0.5,shift(base,28))
  end
  local function box(x0,y0,x1,y1,base)
    rect(x0,y0,x1,y1,base)
    rect(x0,y0,x0+math.floor((x1-x0)*0.22),y1,shift(base,22))
    rect(x1-math.floor((x1-x0)*0.22),y0,x1,y1,shift(base,-30))
  end
  local function gem(cx,cy,r,col)
    for dy=-r,r do local rw=r-math.abs(dy); rect(cx-rw,cy+dy,cx+rw,cy+dy,col) end
    setp(cx-1,cy-1,shift(col,46))
  end
  local function line(x0,y0,x1,y1,col,thick)
    local steps=math.max(math.abs(x1-x0),math.abs(y1-y0),1)
    for s=0,steps do local x=math.floor(x0+(x1-x0)*s/steps+0.5); local y=math.floor(y0+(y1-y0)*s/steps+0.5)
      for t=0,(thick or 1)-1 do setp(x+t,y,col) end end
  end
  local function isEmpty(x,y) if x<0 or y<0 or x>=w or y>=h then return true end return PC.rgbaA(img:getPixel(x,y))==0 end
  local function addOutline()
    local t={} for y=0,h-1 do for x=0,w-1 do
      if isEmpty(x,y) and ((not isEmpty(x-1,y)) or (not isEmpty(x+1,y)) or (not isEmpty(x,y-1)) or (not isEmpty(x,y+1))) then t[#t+1]={x,y} end
    end end
    for _,q in ipairs(t) do setp(q[1],q[2],outline) end
  end
  local cx = math.floor(w/2)

  local K = {}
  K.warrior = function()
    -- cape escura atras
    rect(cx-7,15,cx+6,38,shift(c2,-60))
    -- pernas
    box(cx-5,34,cx-2,46,c1); box(cx+1,34,cx+4,46,c1)
    -- torso (armadura)
    ball(cx,25,8,11,c1)
    -- ombreiras
    ball(cx-8,19,4,4,c1); ball(cx+8,19,4,4,c1)
    -- cinto
    rect(cx-7,29,cx+6,30,shift(c1,-40))
    -- cabeca/elmo
    ball(cx,9,5,6,c1)
    rect(cx-4,9,cx+3,11,shift(c1,-46)) -- visor
    setp(cx-2,10,c2); setp(cx+1,10,c2) -- olhos brilho
    -- espada na mao direita (diagonal)
    line(cx+8,32,cx+13,14,c2,2)           -- lamina
    rect(cx+7,31,cx+12,32,shift(c1,-30))  -- guarda
    rect(cx+8,33,cx+9,36,shift(c1,-30))   -- punho
  end
  K.tree = function()
    box(cx-3,42,cx+2,62,c2)                -- tronco
    line(cx-1,52,cx-5,46,shift(c2,-20),1)  -- raiz/galho
    ball(cx-6,30,9,10,c1); ball(cx+6,28,9,10,c1); ball(cx,20,11,12,c1) -- copa
    -- frutos
    setp(cx-4,26,shift(c2,80)); setp(cx+5,24,shift(c2,80)); setp(cx,30,shift(c2,80))
  end
  K.ore_node = function()
    ball(cx,20,13,10,c1)                   -- rocha
    gem(cx-5,18,3,c2); gem(cx+4,22,2,c2); gem(cx+2,14,2,c2) -- veios/gemas
  end
  K.crystal = function()
    local function shard(bx,by,hgt,wdt,col)
      for k=0,hgt do local ww=math.max(0,math.floor(wdt*(1-k/hgt))); rect(bx-ww,by-k,bx+ww,by-k,col) end
      for k=0,hgt do local ww=math.max(0,math.floor(wdt*(1-k/hgt))); setp(bx-ww,by-k,shift(col,40)) end -- facet esq
    end
    shard(cx-5,28,16,4,c1); shard(cx+4,28,20,5,c1); shard(cx,30,12,3,c1)
  end
  K.barrel = function()
    ellipse(cx,16,11,15,c1); box(cx-9,3,cx+8,28,c1) -- corpo arredondado
    -- recorta laterais com transparente p/ dar curvatura
    for y=3,28 do for x=0,w-1 do local dx=(x-cx)/12; local dy=(y-16)/15; if dx*dx+dy*dy>1.0 then setp(x,y,clear) end end end
    rect(cx-9,9,cx+8,11,c2); rect(cx-9,21,cx+8,23,c2)  -- arcos de metal
    line(cx-2,3,cx-2,28,shift(c1,-30),1)               -- juntas das ripas
    line(cx+3,3,cx+3,28,shift(c1,-30),1)
  end
  K.crate = function()
    box(cx-12,4,cx+11,27,c1)
    rect(cx-12,4,cx+11,5,shift(c1,-40)); rect(cx-12,26,cx+11,27,shift(c1,-40))
    line(cx-12,5,cx+11,26,shift(c1,-46),1); line(cx+11,5,cx-12,26,shift(c1,-46),1) -- X
    rect(cx-12,14,cx+11,15,shift(c1,-30))
  end
  K.potion = function()
    rect(cx-2,3,cx+1,7,shift(c2,-30))     -- cortica
    ball(cx,15,6,7,shift(c2,40))          -- vidro (claro)
    rect(cx-2,7,cx+1,10,shift(c2,40))     -- gargalo
    -- liquido na metade de baixo
    for y=14,21 do for x=cx-6,cx+6 do local dx=(x-cx)/6; local dy=(y-15)/7; if dx*dx+dy*dy<=1.0 then setp(x,y,c1) end end end
    ellipse(cx-2,17,2,2,shift(c1,50))     -- brilho do liquido
  end
  K.ingot = function()
    -- barra trapezoidal
    rect(cx-9,h-9,cx+8,h-4,c1); rect(cx-7,h-13,cx+6,h-9,c1)
    rect(cx-7,h-13,cx+6,h-12,shift(c1,40)) -- topo brilho
    rect(cx-9,h-5,cx+8,h-4,shift(c1,-40))  -- base sombra
  end

  (K[kind] or K.barrel)()
  addOutline()

  spr:newCel(spr.layers[1],1,img,Point(0,0))
  local t=spr:newTag(1,1); t.name="idle"
  spr:saveAs(spr.filename)
  app.command.ExportSpriteSheet{ ui=false, askOverwrite=false, type=SpriteSheetType.ROWS,
    textureFilename=outdir.."/"..id.."_sheet.png", dataFilename=outdir.."/"..id.."_sheet.json",
    dataFormat=SpriteSheetDataFormat.JSON_HASH, listTags=true }
  logTo(id.."_log.txt","OK "..id.." kind="..kind.." "..w.."x"..h.."\n")
end)
if not ok then logTo("_ERROR.txt","FALHOU: "..tostring(err)) end
