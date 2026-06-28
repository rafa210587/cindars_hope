-- gen_creature.lua — criaturas com ARQUETIPO + proporcao humana + sombreamento cilindrico
-- Params: id,w,h,body(pele),accent(roupa/marca),eye,hair,steel,outline,archetype,outdir
--   archetype = warrior|mage|rogue|brute|skeleton|commoner|elder|child | beast|dragon|flyer|blob
local p = app.params
local outdir = (p["outdir"] or "."):gsub("\\","/")
local function logTo(n,t) local f=io.open(outdir.."/"..n,"w"); if f then f:write(t); f:close() end end

local ok,err = pcall(function()
  local id=p["id"] or "creature"
  local w=tonumber(p["w"]) or 32
  local h=tonumber(p["h"]) or 48
  local arche=p["archetype"] or "commoner"
  local PC=app.pixelColor
  local function clamp(v) return math.max(0,math.min(255,math.floor(v))) end
  local function hx(s,d) s=(s or d):gsub("#",""); return PC.rgba(tonumber(s:sub(1,2),16),tonumber(s:sub(3,4),16),tonumber(s:sub(5,6),16),255) end
  local function shift(c,d) return PC.rgba(clamp(PC.rgbaR(c)+d),clamp(PC.rgbaG(c)+d),clamp(PC.rgbaB(c)+d),255) end
  local skin=hx(p["body"],"C0C4C8")
  local cloth=hx(p["accent"],"6A2AA0")
  local pants=shift(cloth,-34)
  local eye=hx(p["eye"],"2A2A2A")
  local hair=hx(p["hair"],"3A2A1A")
  local steel=hx(p["steel"],"9098A8")
  local outline=hx(p["outline"],"161018")
  local clear=PC.rgba(0,0,0,0)
  local cx=math.floor(w/2)
  local sx=w/32.0; local sy=h/48.0
  local function X(v) return cx+math.floor(v*sx) end   -- offset horizontal a partir do centro
  local function Y(v) return math.floor(v*sy) end       -- linha absoluta (px de 0..h em ref 48)

  local function setp(img,x,y,c) if x>=0 and y>=0 and x<w and y<h then img:drawPixel(x,y,c) end end
  local function isEmpty(img,x,y) if x<0 or y<0 or x>=w or y>=h then return true end return PC.rgbaA(img:getPixel(x,y))==0 end
  local function rect(img,x0,y0,x1,y1,c) for y=y0,y1 do for x=x0,x1 do setp(img,x,y,c) end end end
  -- linha horizontal com luz na esq + sombra na dir (cilindro)
  local function row(img,y,x0,x1,base)
    if x1<x0 then return end
    local d=x0+math.floor((x1-x0)*0.62)
    for x=x0,x1 do local c=base
      if x>=d then c=shift(base,-30) end
      if x==x0 then c=shift(base,22) end
      setp(img,x,y,c) end
  end
  -- forma vertical: rx por linha via fn(y)->halfwidth(px), centro deslocado off
  local function shape(img,y0,y1,off,fn,base)
    for y=y0,y1 do local hw=fn(y); if hw>=0.5 then row(img,y,cx+off-math.floor(hw),cx+off+math.floor(hw),base) end end
  end
  local function ellipseRow(cyv,ry,rx) return function(y) local t=(y-cyv)/ry; if t*t>=1 then return 0 end return rx*math.sqrt(1-t*t) end end
  local function lerpfn(yA,hwA,yB,hwB) return function(y) if yB==yA then return hwA end local t=(y-yA)/(yB-yA); return hwA+(hwB-hwA)*t end end
  local function outlineImg(img)
    local t={} for y=0,h-1 do for x=0,w-1 do
      if isEmpty(img,x,y) and ((not isEmpty(img,x-1,y)) or (not isEmpty(img,x+1,y)) or (not isEmpty(img,x,y-1)) or (not isEmpty(img,x,y+1))) then t[#t+1]={x,y} end
    end end for _,q in ipairs(t) do setp(img,q[1],q[2],outline) end
  end
  local function newImg() return Image(w,h,ColorMode.RGB) end
  local function line(img,x0,y0,x1,y1,c,th) local s=math.max(math.abs(x1-x0),math.abs(y1-y0),1)
    for i=0,s do local x=math.floor(x0+(x1-x0)*i/s+0.5); local y=math.floor(y0+(y1-y0)*i/s+0.5); for t=0,(th or 1)-1 do setp(img,x+t,y,c) end end end
  local function ball(img,cxv,cyv,rx,ry,base) for y=math.floor(cyv-ry),math.ceil(cyv+ry) do local t=(y-cyv)/ry; if t*t<1 then local hw=rx*math.sqrt(1-t*t); row(img,y,math.floor(cxv-hw),math.ceil(cxv+hw),base) end end end

  -- referencias verticais (px ref 48)
  local headCy=Y(9); local headRy=Y(6.2); local headRx=math.floor(6.2*sx)
  local neckY0=Y(14); local neckY1=Y(16)
  local shY=Y(16); local waistY=Y(29); local hipY=Y(31); local legBot=Y(45)
  local shHW=7.6; local waistHW=5.2; local hipHW=6.0

  local A={ warrior={head="helmet",weapon="sword",cape=true},
    mage={head="hood",weapon="staff",robe=true}, rogue={head="hood",weapon="dagger",slim=true},
    brute={bulky=true,tusks=true,weapon="club"}, skeleton={bony=true,weapon="sword"},
    commoner={}, elder={head="hood",weapon="staff",robe=true,beard=true}, child={small=true} }
  local o=A[arche] or {}

  local function humanoid(variant)
    local img=newImg()
    local bob=(variant=="idle2") and 1 or 0
    local atk=(variant=="attack")
    local sw = o.bulky and 1.25 or (o.slim and 0.85 or (o.small and 0.8 or 1.0))
    local shp=shHW*sw; local wsp=waistHW*sw; local hpp=hipHW*sw

    -- capa atras
    if o.cape then shape(img,shY-1,legBot,0,lerpfn(shY,shp*0.9,legBot,hpp*1.4),shift(cloth,-66)) end

    -- pernas ou robe
    if o.robe then
      shape(img,waistY+bob,legBot,0, lerpfn(waistY,wsp,legBot,hpp*1.7), cloth)
    else
      local st=(variant=="walk") and math.floor(2*sx) or 0
      shape(img,hipY+bob,legBot, -math.floor(3*sx)-st, function() return 2.0*sx end, pants)
      shape(img,hipY+bob,legBot,  math.floor(3*sx)+st, function() return 2.0*sx end, pants)
      rect(img,X(-5),legBot-1,X(-1),legBot,shift(outline,30)) -- botas
      rect(img,X(1),legBot-1,X(5),legBot,shift(outline,30))
    end

    -- torso
    shape(img,shY+bob,hipY+bob,0, lerpfn(shY,shp,waistY,wsp), o.bony and shift(skin,8) or cloth)
    if o.bony then for r=0,2 do local yy=Y(19)+bob+r*Y(2.2); rect(img,X(-3),yy,X(2),yy,shift(skin,-60)) end end

    -- bracos (mangas) + maos
    local armHW=2.0*sx
    local laX=-math.floor((shp+2)) ; local raX=math.floor((shp+2))
    if atk then
      shape(img,shY+1+bob,waistY+bob,raX-1,function() return armHW end, cloth)         -- braco dir adiantado
      ball(img,X(raX/sx*0+0)+raX, waistY+bob, armHW, armHW, skin)
    else
      shape(img,shY+1+bob,Y(28)+bob,raX,function() return armHW end, cloth)
      setp(img,cx+raX,Y(28)+bob,skin); setp(img,cx+raX,Y(29)+bob,skin)
    end
    shape(img,shY+1+bob,Y(28)+bob,laX,function() return armHW end, cloth)
    setp(img,cx+laX,Y(28)+bob,skin); setp(img,cx+laX,Y(29)+bob,skin)

    -- pescoco + cabeca
    rect(img,X(-1.5),neckY0+bob,X(1.5),neckY1+bob,shift(skin,-10))
    shape(img,headCy-headRy+bob,headCy+headRy+bob,0, ellipseRow(headCy+bob,headRy,headRx), skin)

    -- cabelo (se sem elmo/capuz)
    if not o.head then
      for y=headCy-headRy+bob, headCy-Y(0.5)+bob do local t=(y-(headCy+bob))/headRy; if t*t<1 then local hw=headRx*math.sqrt(1-t*t); row(img,y,math.floor(cx-hw),math.ceil(cx+hw),hair) end end
      rect(img,X(-headRx/sx),headCy-Y(1)+bob,X(-headRx/sx+1),headCy+Y(2)+bob,hair) -- costeleta esq
      rect(img,X(headRx/sx-1),headCy-Y(1)+bob,X(headRx/sx),headCy+Y(2)+bob,hair)
    end
    -- rosto
    setp(img,X(-2),headCy+bob,eye); setp(img,X(1),headCy+bob,eye)
    if o.beard then for y=headCy+Y(2)+bob,headCy+Y(5)+bob do local t=(y-(headCy+bob))/headRy; local hw=headRx*math.sqrt(math.max(0,1-t*t)); row(img,y,math.floor(cx-hw),math.ceil(cx+hw),shift(steel,50)) end end
    if o.tusks then setp(img,X(-2),headCy+Y(3)+bob,shift(skin,90)); setp(img,X(1),headCy+Y(3)+bob,shift(skin,90)) end

    -- headgear
    if o.head=="helmet" then
      shape(img,headCy-headRy-1+bob,headCy+Y(1)+bob,0, ellipseRow(headCy+bob,headRy,headRx+1), steel)
      rect(img,X(-headRx/sx),headCy+bob,X(headRx/sx-1),headCy+1+bob,shift(steel,-55))
      setp(img,X(-2),headCy+bob,eye); setp(img,X(1),headCy+bob,eye)
    elseif o.head=="hood" then
      shape(img,headCy-headRy-1+bob,headCy+Y(2)+bob,0, ellipseRow(headCy+bob,headRy+1,headRx+1), cloth)
      ball(img,cx,headCy+Y(1)+bob,headRx-1,headRy-1,shift(skin,-75)) -- sombra do rosto
      setp(img,X(-2),headCy+Y(1)+bob,eye); setp(img,X(1),headCy+Y(1)+bob,eye)
    end

    -- arma
    local wp=o.weapon
    if wp=="sword" or wp=="dagger" then
      local ln=(wp=="dagger") and Y(8) or Y(16)
      local bx=cx+raX+math.floor(1*sx); local by=atk and waistY+bob or Y(34)
      line(img,bx,by,bx+math.floor(5*sx),by-ln,steel,2); rect(img,bx-1,by,bx+math.floor(4*sx),by+1,shift(steel,-45))
    elseif wp=="staff" then
      local sxp=cx+raX+math.floor(1*sx); line(img,sxp,headCy+bob,sxp,legBot,shift(hair,-10),2); ball(img,sxp,headCy-Y(2)+bob,math.floor(2.4*sx),math.floor(2.4*sy),eye)
    elseif wp=="club" then
      local bx=cx+raX+math.floor(1*sx); local by=atk and Y(26)+bob or Y(36)
      line(img,bx,by,bx+math.floor(3*sx),by-Y(11),shift(hair,-10),3); ball(img,bx+math.floor(3*sx),by-Y(12),math.floor(3.2*sx),math.floor(3*sy),shift(steel,-25))
    end
    if (arche=="warrior" or arche=="skeleton") then rect(img,X(-1),shY+Y(3)+bob,X(0),waistY+bob,cloth) end

    outlineImg(img)
    if variant=="hit" then for y=0,h-1 do for x=0,w-1 do if not isEmpty(img,x,y) and img:getPixel(x,y)~=outline then setp(img,x,y,shift(img:getPixel(x,y),55)) end end end end
    if variant=="death" then local d=newImg(); ball(d,cx,h-Y(4),math.floor(13*sx),math.floor(4*sy),o.robe and cloth or skin); outlineImg(d); return d end
    return img
  end

  -- ===== templates nao-humanoides (mantidos, com ball cilindrico) =====
  local function dragon(variant)
    local img=newImg(); local fl=(variant~="idle2")
    if variant=="death" then ball(img,cx,h-Y(6),math.floor(20*sx),math.floor(5*sy),skin); outlineImg(img); return img end
    local wy=Y(16)-(fl and Y(3) or 0)
    for i=0,math.floor(20*sx) do local sp=math.floor(Y(10)*(1-i/(20*sx)))
      rect(img,X(-5)-i,wy+i,X(-5)-i,wy+i+sp,shift(cloth,-26)); rect(img,X(5)+i,wy+i,X(5)+i,wy+i+sp,shift(cloth,-26)) end
    ball(img,cx,Y(30),math.floor(13*sx),math.floor(8*sy),skin)
    line(img,cx,Y(26),X(11),Y(14),skin,math.max(2,math.floor(4*sx)))
    ball(img,X(12),Y(13),math.floor(5*sx),math.floor(4*sy),skin); rect(img,X(14),Y(13),X(19),Y(15),skin)
    line(img,X(-9),Y(32),X(-22),Y(40),skin,3)
    rect(img,X(-7),Y(36),X(-4),h-2,shift(skin,-30)); rect(img,X(3),Y(36),X(6),h-2,shift(skin,-30))
    setp(img,X(14),Y(12),eye)
    if variant=="attack" then ball(img,X(20),Y(14),math.floor(3*sx),math.floor(3*sy),eye) end
    outlineImg(img); return img
  end
  local function beast(variant)
    local img=newImg(); local bob=(variant=="idle2") and 1 or 0
    if variant=="death" then ball(img,cx,h-Y(5),math.floor(20*sx),math.floor(4*sy),skin); outlineImg(img); return img end
    ball(img,cx,Y(27)+bob,math.floor(17*sx),math.floor(8*sy),skin)
    ball(img,X(14),Y(22)+bob,math.floor(6*sx),math.floor(5*sy),skin)
    rect(img,X(-9),Y(20)+bob,X(5),Y(21)+bob,cloth)
    local s=(variant=="walk") and 2 or 0
    rect(img,X(-13),Y(33),X(-10),h-2-s,shift(skin,-28)); rect(img,X(-5),Y(33),X(-2),h-2+s,shift(skin,-28))
    rect(img,X(3),Y(33),X(6),h-2-s,shift(skin,-28)); rect(img,X(10),Y(33),X(13),h-2+s,shift(skin,-28))
    setp(img,X(16),Y(21)+bob,eye); outlineImg(img); return img
  end
  local function flyer(variant)
    local img=newImg(); local up=(variant~="idle2" and variant~="attack")
    if variant=="death" then ball(img,cx,h-Y(8),math.floor(8*sx),math.floor(4*sy),skin); outlineImg(img); return img end
    ball(img,cx,Y(24),math.floor(5*sx),math.floor(8*sy),skin)
    local wy=Y(20)-(up and Y(5) or -Y(3))
    for i=0,math.floor(13*sx) do local sp=math.floor(Y(11)*(1-i/(13*sx)))
      rect(img,X(-4)-i,wy+i,X(-4)-i,wy+i+sp,shift(cloth,-8)); rect(img,X(4)+i,wy+i,X(4)+i,wy+i+sp,shift(cloth,-8)) end
    setp(img,X(-2),Y(22),eye); setp(img,X(1),Y(22),eye); outlineImg(img); return img
  end
  local function blob(variant)
    local img=newImg(); local sq=(variant=="idle2" or variant=="walk") and 1 or 0
    if variant=="death" then ball(img,cx,h-Y(5),math.floor(20*sx),math.floor(3*sy),skin); outlineImg(img); return img end
    local ry=math.floor(14*sy)-sq; local rx=math.floor(13*sx)+sq
    ball(img,cx,h-3-ry,rx,ry,skin); ball(img,cx,h-4-ry,math.max(2,rx-6),math.max(2,ry-5),cloth)
    setp(img,X(-3),h-5-ry,eye); setp(img,X(3),h-5-ry,eye); outlineImg(img); return img
  end

  local draw=(arche=="dragon" and dragon) or (arche=="beast" and beast) or (arche=="flyer" and flyer) or (arche=="blob" and blob) or humanoid
  local spr=Sprite(w,h,ColorMode.RGB); spr.filename=outdir.."/"..id..".aseprite"; spr.layers[1].name="art"
  local kinds={"idle1","idle2","walk","attack","hit","death"}
  spr:newCel(spr.layers[1],1,draw(kinds[1]),Point(0,0))
  for i=2,#kinds do spr:newEmptyFrame(i); spr:newCel(spr.layers[1],i,draw(kinds[i]),Point(0,0)) end
  local function tg(n,a,b) local t=spr:newTag(a,b); t.name=n end
  tg("idle",1,2); tg("walk",3,3); tg("attack",4,4); tg("hit",5,5); tg("death",6,6)
  spr:saveAs(spr.filename)
  app.command.ExportSpriteSheet{ ui=false, askOverwrite=false, type=SpriteSheetType.ROWS,
    textureFilename=outdir.."/"..id.."_sheet.png", dataFilename=outdir.."/"..id.."_sheet.json",
    dataFormat=SpriteSheetDataFormat.JSON_HASH, listTags=true }
  logTo(id.."_log.txt","OK "..id.." arche="..arche.." "..w.."x"..h.."\n")
end)
if not ok then logTo("_ERROR.txt","FALHOU: "..tostring(err)) end
