local root='D:/Projetos/Cindars_Hope/cindars_hope/'
local out=root..'dev/art/aseprite/keyart-v4/contact-v16/art/'
local pc=app.pixelColor
local source=assert(app.open(root..'Assets/_Game/Art/Generated/World/building/barn_keyart_v4.png'))
assert(source.width==125 and source.height==166)
local base=Image(125,166,ColorMode.RGB); base:drawSprite(source,1); source:close()
local patch=Image(125,166,ColorMode.RGB); patch:clear()
local function paintWood(x0,x1,y0,y1)
 for y=y0,y1 do for x=x0,x1 do
  local p=base:getPixel(x,y); local r,g,b=pc.rgbaR(p),pc.rgbaG(p),pc.rgbaB(p)
  -- Only red timber interior: pale bracing, black joints and copper highlights stay exact.
  if pc.rgbaA(p)==255 and r>65 and r<160 and g<r*.67 and b<g*.72 then
   local c
   if r<91 then c=pc.rgba(80,33,12,255)
   elseif r<120 then c=pc.rgba(106,43,14,255)
   else c=pc.rgba(137,61,18,255) end
   patch:drawPixel(x,y,c)
  end
 end end
end
-- Six existing gable plank interiors. Window, silhouette, crossbeams and lower doors are untouched.
paintWood(29,32,70,81); paintWood(35,38,67,81); paintWood(42,45,65,81)
paintWood(77,80,67,81); paintWood(84,87,70,81); paintWood(91,94,73,81)
local s=Sprite(125,166,ColorMode.RGB); s.layers[1].name='Original barn - preserved'
s:newCel(s.layers[1],1,base,Point(0,0))
local l=s:newLayer(); l.name='Six red plank interiors - reduce embedded gradients'; s:newCel(l,1,patch,Point(0,0))
s:saveAs(out..'barn_contact_v16.aseprite'); s:saveCopyAs(out..'barn_contact_v16.png'); s:close()
local reopened=assert(app.open(out..'barn_contact_v16.aseprite')); assert(#reopened.layers==2)
local result=Image(125,166,ColorMode.RGB); result:drawSprite(reopened,1); reopened:close()
local changed=0
for y=0,165 do for x=0,124 do
 assert(pc.rgbaA(result:getPixel(x,y))==pc.rgbaA(base:getPixel(x,y)))
 if base:getPixel(x,y)~=result:getPixel(x,y) then changed=changed+1; assert(x>=29 and x<=94 and y>=65 and y<=81) end
end end
local board=Sprite(420,280,ColorMode.RGB); local img=Image(420,280,ColorMode.RGB); img:clear(pc.rgba(29,34,30,255))
local function draw(sample,ox)
 for y=0,259 do for x=0,195 do
  local sx=math.min(124,math.floor((x+.5)*125/196))
  local sy=math.min(165,math.floor((y+.5)*166/260))
  img:drawPixel(ox+x,10+y,sample:getPixel(sx,sy))
 end end
end
draw(base,8); draw(result,216); board:newCel(board.layers[1],1,img,Point(0,0)); board:saveCopyAs(out..'barn-offline-1x-board.png'); board:close()
print('PASS barn125x1662layers alpha preserved; changed '..changed..' pixels in local red gable only; nearest offline1.569screenpx/sourcepx')
